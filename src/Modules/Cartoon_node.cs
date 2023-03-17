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
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Text.Json;
using VectSharp;
using System.Runtime.InteropServices;

namespace CartoonNode
{
    /// <summary>
    /// This module can be used to "cartoon" a node. This means that the topology below the selected node is hidden, and the node and its
    /// descendants are displayed as a triangle.
    /// 
    /// This module can be enabled manually, or by using the _Cartoon selection_ Selection action module.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the _Collapse node_ module is that with this module, the size of triangle is proportional
    /// to the number of tips that descend from the cartooned node, while with the _Collapse node_ module the size of the triangle is always
    /// the same (and corresponds to the size that would be obtained with this module if there were only two descendants.
    /// 
    /// This module works by adding to the selected node (and its descendants) an attribute whose name corresponds to the Id of this module
    /// (i.e. `0c3400fd-8872-4395-83bc-a5dc5f4967fe`), and whose value is a representation of the [Fill colour](#fill-colour). This signals
    /// to compliant Coordinates and Plot action modules that the node and its descendants are "cartooned" and should be drawn accordingly.
    /// 
    /// The same result could be obtained by applying the same attribute using a different module (e.g. _Add attribute_ or _Custom script_).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Cartoon node";
        public const string HelpText = "Marks a node to be displayed as a \"cartoon\".";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "0c3400fd-8872-4395-83bc-a5dc5f4967fe";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACDSURBVDhPY2SAgqKiov9QJnmAXAOYoDTZgGIDWKA00cCrdmMDkKqH8BgaWYj1O5rGg1C6HiUW+vr64HwYwKLxAIQJBvU4vUBAIxxgGECsRhjASEg3uO1BFEGNUIAIAxgg0QWYBsAAkQbhNgAGCMUCQQNgAM2gRihNvAEwgMMgcgEDAwAFHzd9VMnTCAAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADISURBVEhL7ZTBDcIwDEVTJFgDNuHOOLmj0AWySzeg81TMUOw0LlbVKInd3HhSZOfyn6y47UzEWvuC4pZbA0Awx/ZQTrE24y/Iwrdo9t6vdymP58C3se/49mgEm+Ax1rt6gp3g99IGnPgNMBgOTo/hGNzD4eGBakFpMFEsqA0msgJpMJEUaIMJvkXrNkznq/lcbthicHUowyXXEieAEoSAVJQWEEpRXkAIReUColJULyAKRXIBkRHpBcSOCPn9TY9iI8KPsyXGfAEQemYQ4Nk4pQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAFVSURBVFhHvZTNccMgEEZFrikqPsdFuAfp7B8VoCJcglNNjulE2Q/p86wYg5BZ9GZ2AFv2vl1ArlG0bXuT4Tqt9uFjHsmuyUHYgRHjMAyLz2viOzCOo48cnHOm4QW4eIV+OPbMVlgwIjwDVXnV6dUzYFk1OF5+9E3rq3aAFSOQWAIW+qZdqwgwKYgkvs9j49h2TckW6MQyhO8VJP6bptN3Jh1gxYhExb0Ekz9xNAZd1/lFbgf0bzMqDpk6wPtd0uotFYcktyAlVZqYJAV0iwHW3+fHTaI4MVk9hEjKxBYVh6wKWLU6xlMgbLckPSC5TKskJotTpl9Kv59f88yDxGZJZ3xh4Ragul2J3jM5dAcZEOEWWHXC/+/q2wenXwYtASxE8gRIBZFtAsRQ5D0BYiBSJkAKRGwEyBsitgJkg0gdAZIhUleAJEROmFQXIBGR/QTIUqTp/wGoudwIQ454rgAAAABJRU5ErkJggg==";

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

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Default cartoon colour:">
                /// This global settings determines the default colour used when this module is enabled. It can be changed from the global
                /// settings window accessible from Edit > Preferences... (the change will have no effect on instances of this module that
                /// have already been added to the plot, it will only affect new instances). Note that this setting affects both the
                /// _Cartoon node_ and the _Collapse node_ modules.
                /// </param>
                ("Default cartoon colour:", "Colour:[240,240,240,255]")
            };
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            Colour defaultColour = Colour.FromRgb(240, 240, 240);

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Default cartoon colour:", out object defaultColourObject))
            {
				if (defaultColourObject is Colour col)
				{
					defaultColour = col;
				}
				else if (defaultColourObject is JsonElement element)
				{
					defaultColour = JsonSerializer.Deserialize<Colour>(element.GetRawText(), GlobalSettings.SerializationOptions);
				}
            }

            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Node:">
                /// This parameter selects the node that should be "cartooned". If only a single node is selected, that node is cartooned.
                /// If mode than one node is selected, the last common ancestor (LCA) of all of them is cartooned. Nodes are selected based
                /// on their `Name`. Note that this module will have no effect if the selected node is a tip of the tree.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Equalise lengths">
                /// If this check box is checked, the branch lengths of the descendants of the selected node are adjusted so that the
                /// cartooned node looks like a triangle. Otherwise, the cartoon will have a "spiky" appearance, as the far edge of the
                /// shape passes through the points that correspond to the position of each tip in the tree that descends from the
                /// selected node.
                /// </param>
                ( "Equalise lengths", "CheckBox:true" ),
                
                /// <param name="Fill colour:" default="see [Default cartoon colour](#default-cartoon-colour)">
                /// The colour to use to fill the triangle when drawing the tree. The default value is determined by the [Default cartoon colour](#default-cartoon-colour)
                /// global setting. If you have the _Color picker_ Menu Action module installed, you can press `CTRL+SHIFT+C` (`CMD+SHIFT+C` on macOS) to open a colour
                /// picker dialog to choose this colour.
                /// </param>
                ( "Fill colour:", "Colour:[" + (defaultColour.R * 255).ToString("0") + "," + (defaultColour.G * 255).ToString("0") + "," + (defaultColour.B * 255).ToString("0") + "," + (defaultColour.A * 255).ToString("0") + "]" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { };

            return !((string[])previousParameterValues["Node:"]).SequenceEqual((string[])currentParameterValues["Node:"]) || !((VectSharp.Colour)currentParameterValues["Fill colour:"]).Equals(((VectSharp.Colour)previousParameterValues["Fill colour:"])) || ((bool)currentParameterValues["Equalise lengths"] != (bool)previousParameterValues["Equalise lengths"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            List<TreeNode> nodes = node.GetChildrenRecursive();

            bool equaliseLengths = (bool)parameterValues["Equalise lengths"];

            List<TreeNode> leaves = node.GetLeaves();

            if (leaves.Count > 1)
            {
                if (equaliseLengths)
                {
                    double[] leafLengths = new double[leaves.Count];
                    for (int i = 0; i < leaves.Count; i++)
                    {
                        leafLengths[i] = node.PathLengthTo(leaves[i], TreeNode.NodeRelationship.Ancestor);
                    }
                    double meanLength = leafLengths.Average();

                    for (int i = 1; i < nodes.Count; i++)
                    {
                        double upstreamLength = node.PathLengthTo(nodes[i], TreeNode.NodeRelationship.Ancestor);
                        double downstreamLength = nodes[i].LongestDownstreamLength();

                        if (Math.Abs((upstreamLength + downstreamLength - meanLength) / meanLength) > 0.001)
                        {
                            double factor = (meanLength - upstreamLength + nodes[i].Length) / (downstreamLength + nodes[i].Length);
                            foreach (TreeNode nd in nodes[i].GetChildrenRecursiveLazy())
                            {
                                nd.Length *= factor;
                            }
                        }
                    }
                }

                string fillColourString = ((VectSharp.Colour)parameterValues["Fill colour:"]).ToHexString();

                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].Attributes[Id] = fillColourString;
                }
            }
        }
    }
}