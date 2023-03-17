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
using System.Security.Cryptography;
using System.Text;
using PhyloTree;
using PhyloTree.Extensions;
using PhyloTree.Formats;
using TreeViewer;

namespace OpenNewick
{
    /// <summary>
    /// This module is used to read files in the Newick (-with-attributes) format (see [this link](https://github.com/arklumpus/TreeNode/blob/master/NWKA.md)
    /// for further information on this format).
    /// 
    /// To avoid memory overflow issues, the module does not read all the trees from the file into memory at once; instead, it only reads them
    /// one at a time when requested by the chosen Load file module.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Newick";
        public const string HelpText = "Opens a file containing one or more trees in the Newick (-with-attributes) format.\nSafe even when opening huge files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "79dfb9b2-ff10-4ed9-aa74-f7b3ae93c3d2";
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public static string[] Extensions { get; } = { "Newick files", "tree", "tre", "nwk", "nwka", "treefile" };
        public static double IsSupported(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                StringBuilder sb = new StringBuilder();

                int c = sr.Read();

                while (c >= 0 && char.IsWhiteSpace((char)c))
                {
                    c = sr.Read();
                }

                return ((char)c == '(') ? 0.01 : 0;
            }
        }

        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            bool escaping = false;
            bool escaped;
            bool openQuotes = false;
            bool openApostrophe = false;
            bool eof = false;

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
                        yield return NWKA.ParseTree(sb.ToString());
                        double progress = Math.Max(0, Math.Min(1, sr.BaseStream.Position / length));
                        progressAction?.Invoke(progress);
                    }
                }
            }
        }
    }
}
