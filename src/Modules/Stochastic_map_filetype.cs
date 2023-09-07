/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PhyloTree;
using PhyloTree.Extensions;
using PhyloTree.Formats;
using TreeViewer;

namespace OpenSimmap
{
    /// <summary>
    /// This module adds support for tree files in the format produced by the `write.simmap` function of the R package `phytools`.
    /// 
    /// This module adds the proper branch lengths to trees in this format and stores the branch states in an attribute called
    /// `States` that is added to each node in the tree. A list of all the characters whose states are encoded in the tree and
    /// their possible states is saved in the `Characters` attribute of the root node of the tree.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Stochastic map";
        public const string HelpText = "Opens a file containing one or more trees in the format produced by the write.simmap function of phytools (Revell 2012).\nSafe even when opening huge files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.4");
        public const string Id = "e760952f-56c1-4192-8dfb-b5d6ec2692d2";
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public static string[] Extensions { get; } = { "Newick files", "tree", "tre", "nwk", "nwka", "smap" };
        public static double IsSupported(string fileName)
        {
            try
            {
                bool escaping = false;
                bool escaped;
                bool openQuotes = false;
                bool openApostrophe = false;
                bool eof = false;

                using (StreamReader sr = new StreamReader(fileName))
                {
                    StringBuilder sb = new StringBuilder();

                    char c = sr.NextToken(ref escaping, out escaped, ref openQuotes, ref openApostrophe, out eof);

                    while (!eof && !(c == ';' && !escaped && !openQuotes && !openApostrophe))
                    {
                        sb.Append((char)c);
                        c = sr.NextToken(ref escaping, out escaping, ref openQuotes, ref openApostrophe, out eof);
                    }

                    if (sb.Length > 0)
                    {
                        TreeNode tree = NWKA.ParseTree(sb.ToString());

                        if (tree.Children.Count > 0)
                        {
                            foreach (KeyValuePair<string, object> attribute in tree.Children[0].Attributes)
                            {
                                if (attribute.Key.StartsWith("Unknown") && attribute.Value is string attributeValue && attributeValue.StartsWith("{") && attributeValue.EndsWith("}"))
                                {
                                    SimmapBranchState[] states = SimmapBranchState.Parse(attributeValue).ToArray();

                                    if (states.Length > 0)
                                    {
                                        return 0.85;
                                    }
                                }
                            }
                        }

                        return 0;
                    }

                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }



        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            bool escaping = false;
            bool escaped;
            bool openQuotes = false;
            bool openApostrophe = false;
            bool eof = false;

            if (GlobalSettings.Settings.DrawTreeWhenOpened)
            {
                moduleSuggestions.Add(("32858c9d-0247-497f-aeee-03f7bfe24158", new Dictionary<string, object>()));

                moduleSuggestions.Add(("7c767b07-71be-48b2-8753-b27f3e973570", new Dictionary<string, object>() { }));
                moduleSuggestions.Add(("f7a20f2f-94b2-4331-8bbf-4e0087da6fba", new Dictionary<string, object>() { }));
                moduleSuggestions.Add(("ac496677-2650-4d92-8646-0812918bab03", new Dictionary<string, object>() { { "Position:", new VectSharp.Point(10, 0) } }));

                NumberFormatterOptions widthFO = new NumberFormatterOptions(Modules.DefaultAttributeConvertersToDouble[1]) { AttributeName = "StateWidth", AttributeType = "Number", DefaultValue = 10.0, Parameters = new object[] { Modules.DefaultAttributeConvertersToDouble[1], 0, double.PositiveInfinity, true } };
                NumberFormatterOptions heightFO = new NumberFormatterOptions(Modules.DefaultAttributeConvertersToDouble[1]) { AttributeName = "StateHeight", AttributeType = "Number", DefaultValue = 10.0, Parameters = new object[] { Modules.DefaultAttributeConvertersToDouble[1], 0, double.PositiveInfinity, true } };

                moduleSuggestions.Add(("0512b822-044d-4c13-b3bb-bca494c51daa", new Dictionary<string, object>()
                {
                    { "Show on:",  2 },
                    { "Stroke thickness:", 1.0 },
                    { "Width:", widthFO },
                    { "Height:", heightFO },
                    { "Attribute:", "ConditionedPosteriors" }
                }));
            }

            using (StreamReader sr = new StreamReader(fileName))
            {
                double length = sr.BaseStream.Length;

                while (!eof)
                {
                    StringBuilder sb = new StringBuilder();

                    char c = sr.NextToken(ref escaping, out escaped, ref openQuotes, ref openApostrophe, out eof);

                    while (!eof && !(c == ';' && !escaped && !openQuotes && !openApostrophe))
                    {
                        sb.Append((char)c);
                        c = sr.NextToken(ref escaping, out escaping, ref openQuotes, ref openApostrophe, out eof);
                    }

                    if (sb.Length > 0)
                    {
                        string treeText = sb.ToString();

                        if (!string.IsNullOrWhiteSpace(treeText))
                        {
                            TreeNode tree = NWKA.ParseTree(treeText);

                            HashSet<string>[] characters = null;

                            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                            {
                                string attributeToRemove = null;

                                foreach (KeyValuePair<string, object> attribute in node.Attributes)
                                {
                                    if (attribute.Key.StartsWith("Unknown") && attribute.Value is string attributeValue && attributeValue.StartsWith("{") && attributeValue.EndsWith("}"))
                                    {
                                        SimmapBranchState[] states = SimmapBranchState.Parse(attributeValue).ToArray();

                                        if (states.Length > 0)
                                        {
                                            if (characters == null)
                                            {
                                                characters = new HashSet<string>[states[0].States.Length];

                                                for (int i = 0; i < characters.Length; i++)
                                                {
                                                    characters[i] = new HashSet<string>();
                                                }
                                            }

                                            node.Attributes.Add("States", System.Text.Json.JsonSerializer.Serialize(states, Modules.DefaultSerializationOptions));
                                            node.Length = (from el in states select el.Length).Sum();

                                            foreach (SimmapBranchState state in states)
                                            {
                                                for (int i = 0; i < state.States.Length; i++)
                                                {
                                                    characters[i].Add(state.States[i]);
                                                }
                                            }

                                            attributeToRemove = attribute.Key;

                                            break;
                                        }
                                    }
                                }

                                if (!string.IsNullOrEmpty(attributeToRemove))
                                {
                                    node.Attributes.Remove(attributeToRemove);
                                }
                            }

                            tree.Attributes.Add("Characters", System.Text.Json.JsonSerializer.Serialize(characters, Modules.DefaultSerializationOptions));

                            yield return tree;
                            double progress = Math.Max(0, Math.Min(1, sr.BaseStream.Position / length));
                            progressAction?.Invoke(progress);
                        }
                    }
                }
            }
        }
    }

    internal class SimmapBranchState
    {
        public string[] States { get; set; }
        public double Length { get; set; }

        public SimmapBranchState()
        {

        }

        public static IEnumerable<SimmapBranchState> Parse(string value)
        {
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                value = value.Substring(1, value.Length - 2);

                string[] splitValue = value.Split(':');

                foreach (string sr in splitValue)
                {
                    string[] splitSr = sr.Split(',');
                    string[] states = splitSr[0].Split('|');
                    double length = double.Parse(splitSr[1], System.Globalization.CultureInfo.InvariantCulture);

                    yield return new SimmapBranchState() { Length = length, States = states };
                }
            }
        }
    }
}