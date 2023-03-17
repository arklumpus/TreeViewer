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
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.IO;
using PhyloTree.Formats;
using System.Text;

namespace SetUpAgeDistributions
{
    /// <summary>
    /// This module is used to set up the age distributions for the nodes, that can then be plotted using the _Plot age distributions_ (id `5dbe1f3c-dbea-49b3-8f04-f319aefca534`) Plot
    /// Action module.
    /// 
    /// To use this module, you should add as an attachment a tree file containing e.g. a sample from the posterior distribution of dated trees. This
    /// module will use all the trees in the file to compute the age distributions. The tree file can be in NEXUS, Newick, Newick-with-attributes,
    /// NCBI ASN.1 or Binary format.
    /// </summary>


    public static class MyModule
    {
        public const string Name = "Set up age distributions (attachment)";
        public const string HelpText = "Computes node age distributions from trees contained in an attachment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.2.0");
        public const string Id = "5d721496-f2fa-48de-ad0d-90ef5d8086aa";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAC/SURBVDhPnZHBEoIwDESDXwZXL/5L7wx2vPdfvHDFP9NsTJwYaEHfDBMK3aW7dOQ4j/crj+m9ojzfLlg3Oek0TAwmNWwSDSK7JhIhHL3GZqSOxU+9/4ufDPgEX6WDwxEgTinFfVlKRLYtdwXZvXgopWDvgPXeX5DigviBFzajQdYJqmLAz3vM2rEFL+YLAsmPCPwO5eeqQUWM+0U7EFod+GObeEWzRMvsvrjw5XtqG1hRiojZDNE+HOnAWImJiF5HAlwXqgmmugAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAElSURBVEhLtZRBEoIwDEVBz+I5hK0b116DvQMd95zDrQvdwrj2Bh4G82vqQGkhYeTNZNK0w0+bhqZJhMP5VpEryczjcsR4EVv2A3riINvtT+n7eW05VrFh7+PEHSUnVTNKMCG0KMmgRF5pQqjLZS9ZIBzjTvaaaoKUxDser0Lskv/G+iVibxEmUv14oz95JklUvCgK/ztT13UVfCoiFy8Rz0m0pTijcUNmYpds2DvE4phjn2M+eALQK5VKvA+td9EEcwjEbZkWJZCKkxl1Al+8FwPbOTSHJrHj77QQiOFjMuwwFI+6T/xUQIxcaOcutkl8NG+RX3Nf3NYcC4sIHR9AHGtkwXqrXlPe6Q+O7c5jF6pJgOM3LolEHKjalMvgWhLMtGKSfAACabeWbAz4GwAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAH/SURBVFhHrZc9TgMxEIUTuECuQIFElyzdViQtBdRUhCtsjyCiX45A0tGCBG0imjSIQE1BS8kNwnvOeOVkf/yz/qRhbK/lNx6PHdHtWDi9frqFu4FNXu/O2Y7KnvgmKK68BBOVxgAqBKMHYcuA3r1J1CBqA7CIRAtiX/wWsnjV7k2GhycX3e+3x4X0gygF4CiuaR1EcQ09het4gb37XFcVQCTxLRCE9Y0hXYivpR0NLZ5l2RAuhyXsCytYlue5OjaXh8gHvpZa/AFuzjaYwEbiyRzf1THFPILiqRbxMWyKnV5xTINvPbh7GL+PinNqGYSTuAnmfdAXR8AFJH06Ta54iwvPsKT0DvBO826jyQKyUSuO/iBN0yNYb7lc/nKOCcbpxpVFKIvaMtEkzj6rnUW4kv4uaoO1t0AvXoNNXPeZySn7GB/AK9Cm+BlsZbuGVVlwFVc1oD1g9WsuYXwbssYAKrLgJU5knPzxz868hS0DRGfBFGcKXcX1+GfVvOId8IVBcAeO4rVBBgdA2oqTNhloLU6CAoglTrwDaBBnYfLhcRb3hiKwtSxeQoJQQTbNM/HKgCzYx46ONyNlZI7zzl3eAZMD2NemWcZXnPgG8APrb5rbhIgT3wBmsGT3bEPFSZtbwJ9bHgczwh+WoGoPfQdY7fxFY03wWGYQD/jnpNP5Bww3Xkwlyy0eAAAAAElFTkSuQmCC";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "StateData", "InstanceStateData:" ),

                ( "Data source", "Group:5" ),

                /// <param name="Attachment:">
                /// This parameter specifies the attachment from which the age distributions will be read, either as a tree file, or as a table.
                /// </param>
                ( "Attachment:", "Attachment:" ),

                /// <param name="Format:">
                /// This parameter specifies whether the attachment contains a list of trees (e.g., posterior samples) that should be used to
                /// determine the age distributions, or a table containing the age estimates for the nodes (e.g., the output file from a MCMCTree analysis).
                /// 
                /// If the file contains a list of trees, it should be a tree file in NEXUS, Newick, Newick-with-attributes, NCBI ASN.1 or Binary format.
                /// 
                /// If the file contains a table, this should be a tab-separated table containing a header row with parameter names, followed
                /// by data rows. The parameter names will determine the nodes to which the age estimates will be applied. Following the syntax
                /// of MCMCTree output files, parameter names for age estimates should be in the form `t_nX`, where `X` is a number identifying the node
                /// corresponding to each column. Columns with headers that do not follow this convention (e.g. `Gen`, `lnL`, ...) will be ignored.
                /// </param>
                ( "Format:", "ComboBox:0[\"List of trees\",\"Table\"]" ),

                /// <param name="Node indices:">
                /// If the attachment contains a table, this parameter determines how the node indices that correspond to column headers are determined.
                /// 
                /// If the selected value is `Infer automatically`, the node indices are determined automatically based on the tree topology. Note that
                /// if you have altered the tree topology using another _Further transformation_ module (e.g., by sorting or pruning nodes), the node
                /// indices will change, while the other options are more robust to topology changes.
                /// 
                /// If the selected value is `Use attribute`, the node indices are determined using the value of the specified attribute.
                /// 
                /// If the selected value is `Load from MCMCTree output file`, an attachment containing the MCMCTree output file (`out.txt`) should be
                /// provided, and the module will use the tree contained in there to determine the node indices.
                /// </param>
                ( "Node indices:", "ComboBox:0[\"Infer automatically\",\"Use attribute\",\"Load from MCMCTree output file\"]"),

                /// <param name="Index attribute:">
                /// This parameter determines the attribute used to determine node indices.
                /// </param>
                ( "Index attribute:", "AttributeSelector:Name" ),
                
                /// <param name="MCMCTree output file:">
                /// This parameter specifies the attachment containing the MCMCTree output file (`out.txt`) that should be used to determine the
                /// node indices.
                /// </param>
                ( "MCMCTree output file:", "Attachment:" ),
                
                /// <param name="Age type:">
                /// This parameter determines the kind of age that is computed if the attachment contains a list of trees.
                /// 
                /// If the value is `Since root`, the age of each node corresponds to the distance $d$ (as in, the
                /// sum of branch lengths) from the node to the root of the tree; in this case, the root node would
                /// have an age of `0`.
                /// 
                /// If the value is `Until tips`, first the total length $l$ of the tree from the root node to the
                /// most distant tip is computed; then, the age of each node is $d - l$. In this case, if all the
                /// tips of the tree are contemporaneous, they will have an age of `0`.
                /// </param>
                ( "Age type:", "ComboBox:0[\"Until tips\", \"Since root\"]"),

                ( "Summary statistics", "Group:3" ),
                
                /// <param name="Compute mean">
                /// If this check box is checked, in addition to the age distribution, the mean age for each node.
                /// </param>
                ( "Compute mean", "CheckBox:true" ),
                
                /// <param name="Credible interval:">
                /// This parameter determines what kind of credible interval for the age is computed. If the value is
                /// `None`, no credible interval is computed. If the value is `Highest-density`, the interval that contains
                /// the proportion of samples specified by the [Threshold](#parameter) with the highest probability
                /// density is computed. If the value is `Equal-tailed`, the interval corresponds to the symmetrical
                /// interval around the average that contains the specified proportion of samples.
                /// 
                /// The functions for computing credible intervals are based on code from the R package `bayestestR`,
                /// available under a GPLv3 licence [here](https://github.com/easystats/bayestestR/blob/e26dc16f3df711afbc2e79851442cfb5ea3b6f26/R/hdi.R).
                /// </param>
                ( "Credible interval:", "ComboBox:1[\"None\",\"Highest-density\",\"Equal-tailed\"]" ),
                
                /// <param name="Threshold">
                /// This parameter determines the threshold for the credible interval.
                /// </param>
                ( "Threshold:", "Slider:0.89[\"0\",\"1\",\"0.00\"]"),

                ( "Scaling", "Group:2" ),

                /// <param name="Scaling factor:">
                /// This parameter is used to scale the age distributions (and the tree, if the [Apply scaling to transformed tree](#apply-scaling-to-transformed-tree)
                /// check box is checked).
                /// </param>
                ( "Scaling factor:", "NumericUpDown:1[\"0\",\"Infinity\",\"1\",\"0.####\"]" ),
                
                /// <param name="Apply scaling to transformed tree">
                /// If this check box is checked, the [scaling factor](#scaling-factor) is applied to the transformed tree, as well as to the age distributions.
                /// </param>
                ( "Apply scaling to transformed tree", "CheckBox:true" ),

                /// <param name="Name:">
                /// This parameter specifies a name that can be used to identify the age distributions in cases where multiple age distributions have been computed
                /// for the same tree.
                /// </param>
                ( "Name:", "TextBox:" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();

            if ((int)currentParameterValues["Credible interval:"] != 0)
            {
                controlStatus.Add("Threshold:", ControlStatus.Enabled);
            }
            else
            {
                controlStatus.Add("Threshold:", ControlStatus.Hidden);
            }

            if ((int)currentParameterValues["Format:"] == 0)
            {
                controlStatus["Node indices:"] = ControlStatus.Hidden;
                controlStatus["Index attribute:"] = ControlStatus.Hidden;
                controlStatus["MCMCTree output file:"] = ControlStatus.Hidden;

                controlStatus["Age type:"] = ControlStatus.Enabled;
            }
            else if ((int)currentParameterValues["Format:"] == 1)
            {
                controlStatus["Node indices:"] = ControlStatus.Enabled;

                switch ((int)currentParameterValues["Node indices:"])
                {
                    case 0:
                        controlStatus["Index attribute:"] = ControlStatus.Hidden;
                        controlStatus["MCMCTree output file:"] = ControlStatus.Hidden;
                        break;
                    case 1:
                        controlStatus["Index attribute:"] = ControlStatus.Enabled;
                        controlStatus["MCMCTree output file:"] = ControlStatus.Hidden;
                        break;
                    case 2:
                        controlStatus["Index attribute:"] = ControlStatus.Hidden;
                        controlStatus["MCMCTree output file:"] = ControlStatus.Enabled;
                        break;
                }

                controlStatus["Age type:"] = ControlStatus.Hidden;
            }

            parametersToChange = new Dictionary<string, object>()
            {
                { "Apply", false }
            };

            if (string.IsNullOrEmpty((string)currentParameterValues["Name:"]) && (Attachment)currentParameterValues["Attachment:"] != (Attachment)previousParameterValues["Attachment:"])
            {
                parametersToChange["Name:"] = ((Attachment)currentParameterValues["Attachment:"]).Name;
            }

            return (bool)currentParameterValues["Apply"];
        }

        private static bool Compare(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            return !list1.Except(list2).Any();
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            bool fromLeft = (int)parameterValues["Age type:"] == 1;
            double threshold = (double)parameterValues["Threshold:"];
            int ciType = (int)parameterValues["Credible interval:"];
            bool computeMean = (bool)parameterValues["Compute mean"];
            string name = (string)parameterValues["Name:"];

            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            Attachment attachment = (Attachment)parameterValues["Attachment:"];

            int format = (int)parameterValues["Format:"];
            int nodeIndexType = (int)parameterValues["Node indices:"];

            if (attachment != null)
            {
                List<TreeNode> nodes = tree.GetChildrenRecursive();

                Dictionary<string, List<double>> ageSamples = new Dictionary<string, List<double>>();
                for (int i = 0; i < nodes.Count; i++)
                {
                    ageSamples.Add(nodes[i].Id, new List<double>());
                }

                attachment.Stream.Seek(attachment.StreamStart, System.IO.SeekOrigin.Begin);

                if (format == 0)
                {
                    foreach (TreeNode sampledTree in ReadTree.Read(attachment.Stream, progressAction))
                    {
                        double treeHeight = -1;

                        if (!fromLeft)
                        {
                            treeHeight = sampledTree.LongestDownstreamLength();
                        }

                        foreach (TreeNode node in sampledTree.GetChildrenRecursiveLazy())
                        {
                            TreeNode LCA = tree.GetLastCommonAncestor(node.GetLeafNames());

                            if (Compare(LCA.GetLeafNames(), node.GetLeafNames()))
                            {
                                double age = node.UpstreamLength();

                                if (!fromLeft)
                                {
                                    age = treeHeight - age;
                                }

                                ageSamples[LCA.Id].Add(age);
                            }
                        }
                    }
                }
                else if (format == 1)
                {
                    Dictionary<int, string> nodeIndices = new Dictionary<int, string>();

                    if (nodeIndexType == 0)
                    {
                        int nodeIndex = tree.GetLeaves().Count + 1;

                        for (int i = 0; i < nodes.Count; i++)
                        {
                            if (nodes[i].Children.Count > 0)
                            {
                                nodeIndices[nodeIndex] = nodes[i].Id;
                                nodeIndex++;
                            }
                        }
                    }
                    else if (nodeIndexType == 1)
                    {
                        string attributeName = (string)parameterValues["Index attribute:"];

                        foreach (TreeNode node in nodes)
                        {
                            if (node.Attributes.TryGetValue(attributeName, out object indexObject))
                            {
                                if (indexObject is int i)
                                {
                                    nodeIndices[i] = node.Id;
                                }
                                else if (indexObject is double d)
                                {
                                    nodeIndices[(int)d] = node.Id;
                                }
                                else if (indexObject is string sr && int.TryParse(sr, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int k))
                                {
                                    nodeIndices[k] = node.Id;
                                }
                            }
                        }
                    }
                    else if (nodeIndexType == 2)
                    {
                        Attachment mcmcTreeAttachment = (Attachment)parameterValues["MCMCTree output file:"];

                        if (mcmcTreeAttachment != null)
                        {
                            mcmcTreeAttachment.Stream.Seek(mcmcTreeAttachment.StreamStart, SeekOrigin.Begin);

                            using (StreamReader sr = new StreamReader(mcmcTreeAttachment.Stream, leaveOpen: true))
                            {
                                string line = sr.ReadLine();

                                if (line.StartsWith("MCMCTREE"))
                                {
                                    while (line != null && !line.StartsWith("Species tree"))
                                    {
                                        if (mcmcTreeAttachment.Stream.Position - mcmcTreeAttachment.StreamStart < mcmcTreeAttachment.StreamLength)
                                        {
                                            line = sr.ReadLine();
                                        }
                                        else
                                        {
                                            line = null;
                                        }
                                    }

                                    if (line != null && line.StartsWith("Species tree"))
                                    {
                                        line = sr.ReadLine();

                                        TreeNode speciesTree = NWKA.ParseTree(line);

                                        foreach (TreeNode node in speciesTree.GetLeaves())
                                        {
                                            int index = int.Parse(node.Name.Substring(0, node.Name.IndexOf("_")), System.Globalization.CultureInfo.InvariantCulture);
                                            node.Name = node.Name.Substring(node.Name.IndexOf("_") + 1);

                                            string id = tree.GetNodeFromName(node.Name)?.Id;

                                            if (id != null)
                                            {
                                                nodeIndices[index] = id;
                                            }
                                        }

                                        foreach (TreeNode node in speciesTree.GetChildrenRecursiveLazy())
                                        {
                                            if (!double.IsNaN(node.Support))
                                            {
                                                int index = (int)node.Support;

                                                string id = tree.GetLastCommonAncestor(node.GetLeafNames())?.Id;

                                                if (id != null)
                                                {
                                                    nodeIndices[index] = id;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    throw new ArgumentException("The selected attachment does not seem to be an MCMCTree output file! Please make sure you have provided the `out.txt` file.");
                                }
                            }
                        }
                        else
                        {
                            throw new NullReferenceException("No MCMCTree output file specified!");
                        }
                    }

                    using (StreamReader sr = new StreamReader(attachment.Stream, leaveOpen: true))
                    {
                        string headerLine = sr.ReadLine();
                        string[] headers = (from el in headerLine.Split('\t')
                                            let isNode = el.StartsWith("t_n")
                                            let index = isNode ? int.Parse(el.Replace("t_n", ""), System.Globalization.CultureInfo.InvariantCulture) : -1
                                            let isPresent = isNode && nodeIndices.ContainsKey(index)
                                            select isPresent ? nodeIndices[index] : null).ToArray();

                        string line = sr.ReadLine();

                        while (line != null)
                        {
                            string[] splitLine = line.Split('\t');

                            for (int i = 0; i < headers.Length; i++)
                            {
                                if (!string.IsNullOrEmpty(headers[i]))
                                {
                                    ageSamples[headers[i]].Add(double.Parse(splitLine[i], System.Globalization.CultureInfo.InvariantCulture));
                                }
                            }

                            if (attachment.Stream.Position - attachment.StreamStart <= attachment.StreamLength)
                            {
                                line = sr.ReadLine();
                            }
                            else
                            {
                                line = null;
                            }
                        }
                    }
                }

                if (ciType > 0 || computeMean)
                {
                    for (int i = 0; i < nodes.Count; i++)
                    {
                        if (ageSamples[nodes[i].Id].Count > 0)
                        {
                            if (computeMean)
                            {
                                nodes[i].Attributes["Mean age"] = ageSamples[nodes[i].Id].Average();
                            }

                            if (ciType == 1)
                            {
                                double[] hdi = BayesStats.HighestDensityInterval(ageSamples[nodes[i].Id], threshold);
                                nodes[i].Attributes[threshold.ToString("0%") + "_HDI"] = "[ " + hdi[0].ToString() + ", " + hdi[1].ToString() + "]";
                            }
                            else if (ciType == 2)
                            {
                                double[] eti = BayesStats.EqualTailedInterval(ageSamples[nodes[i].Id], threshold);
                                nodes[i].Attributes[threshold.ToString("0%") + "_ETI"] = "[ " + eti[0].ToString() + ", " + eti[1].ToString() + "]";
                            }
                        }
                    }
                }

                double scalingFactor = (double)parameterValues["Scaling factor:"];
                bool applyScalingToTree = (bool)parameterValues["Apply scaling to transformed tree"];

                if (scalingFactor != 1)
                {
                    foreach (KeyValuePair<string, List<double>> kvp in ageSamples)
                    {
                        for (int i = 0; i < kvp.Value.Count; i++)
                        {
                            kvp.Value[i] *= scalingFactor;
                        }
                    }

                    if (applyScalingToTree)
                    {
                        foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                        {
                            node.Length *= scalingFactor;
                        }
                    }
                }

                stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = ageSamples;
                tree.Attributes["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = "Age distributions";

                Dictionary<string, (string, object)> distributionCollection;

                if (stateData.Tags.TryGetValue("4e5d3934-44e6-4fe3-b11c-bd78e5b577d0", out object distribCollect))
                {
                    distributionCollection = distribCollect as Dictionary<string, (string, object)>;
                }
                else
                {
                    distributionCollection = new Dictionary<string, (string, object)>();
                    stateData.Tags["4e5d3934-44e6-4fe3-b11c-bd78e5b577d0"] = distributionCollection;
                }

                distributionCollection[(string)parameterValues[Modules.ModuleIDKey]] = (name, ageSamples);

            }
        }
    }

    internal static class ReadTree
    {
        public static IEnumerable<TreeNode> Read(Stream treeStream, Action<double> progressAction)
        {
            if (IsASNBer(treeStream))
            {
                return NcbiAsnBer.ParseTrees(treeStream, keepOpen: true);
            }
            else if (IsBinary(treeStream))
            {
                return BinaryTree.ParseTrees(treeStream, progressAction: progressAction, keepOpen: true);
            }
            else if (IsASNTxt(treeStream))
            {
                return NcbiAsnText.ParseTrees(treeStream, keepOpen: true);
            }
            else if (IsNEXUS(treeStream))
            {
                return NEXUS.ParseTrees(treeStream, progressAction: progressAction, keepOpen: true);
            }
            else if (IsNewick(treeStream))
            {
                return NWKA.ParseTrees(treeStream, progressAction: progressAction, keepOpen: true);
            }
            else
            {
                return null;
            }
        }

        private static bool IsBinary(Stream fs)
        {
            long pos = fs.Position;
            try
            {
                return BinaryTree.IsValidStream(fs, true);
            }
            finally
            {
                fs.Position = pos;
            }
        }

        private static bool IsASNBer(Stream fs)
        {
            long pos = fs.Position;

            try
            {
                using BinaryReader reader = new BinaryReader(fs, Encoding.Default, true);

                byte[] header = reader.ReadBytes(4);

                if (header[0] == 0x30 && header[1] == 0x80 && header[3] == 0x80)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                fs.Position = pos;
            }
        }

        private static bool IsASNTxt(Stream fs)
        {
            long pos = fs.Position;

            try
            {
                using (StreamReader sr = new StreamReader(fs, leaveOpen: true))
                {
                    StringBuilder sb = new StringBuilder();

                    int c = sr.Read();

                    while (c >= 0 && char.IsWhiteSpace((char)c))
                    {
                        c = sr.Read();
                    }

                    for (int i = 0; i < 16; i++)
                    {
                        sb.Append((char)c);
                        c = sr.Read();
                    }

                    return sb.ToString() == "BioTreeContainer";
                }
            }
            finally
            {
                fs.Position = pos;
            }
        }

        public static bool IsNewick(Stream fs)
        {
            long pos = fs.Position;

            try
            {
                using (StreamReader sr = new StreamReader(fs, leaveOpen: true))
                {
                    int c = sr.Read();

                    while (c >= 0 && char.IsWhiteSpace((char)c))
                    {
                        c = sr.Read();
                    }

                    return (char)c == '(';
                }
            }
            finally
            {
                fs.Position = pos;
            }
        }

        public static bool IsNEXUS(Stream fs)
        {
            long pos = fs.Position;

            try
            {
                using (StreamReader sr = new StreamReader(fs, leaveOpen: true))
                {
                    StringBuilder sb = new StringBuilder();

                    int c = sr.Read();

                    while (c >= 0 && char.IsWhiteSpace((char)c))
                    {
                        c = sr.Read();
                    }

                    sb.Append((char)c);
                    for (int i = 0; i < 5; i++)
                    {
                        c = sr.Read();
                        if (c >= 0)
                        {
                            sb.Append((char)c);
                        }
                    }

                    return sb.ToString().Equals("#NEXUS", StringComparison.OrdinalIgnoreCase);
                }
            }
            finally
            {
                fs.Position = pos;
            }
        }
    }
}

