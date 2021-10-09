using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Text.Json;
using System.Runtime.InteropServices;

namespace CollapseNode
{
    /// <summary>
    /// This module can be used to "collapse" a node. This means that the topology below the selected node is hidden, and the node and its
    /// descendants are displayed as a triangle.
    /// 
    /// This module can be enabled manually, or by using the _Collapse selection_ Selection action module.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the _Cartoon node_ module is that with the _Cartoon node_ module, the size of triangle is
    /// proportional to the number of tips that descend from the cartooned node, while with this module the size of the triangle is always
    /// the same (and corresponds to the size that would be obtained with the _Cartoon node_ module if there were only two descendants.
    /// 
    /// This module first of all removes all the children from the selected node except two; the branch lengths of these two remaining
    /// children are altered so that they correspond to the distance of the closest and fartest leaf that descended from the selected node.
    /// 
    /// Then, the module adds to the selected node (and the two children) an attribute whose name corresponds to the Id of the _Cartoon node_
    /// module (i.e. `0c3400fd-8872-4395-83bc-a5dc5f4967fe`) and whose value is a representation of the [Fill colour](#fill-colour), as well
    /// as another attribute whose name corresponds to the Id of this module (i.e. `3812314b-e821-4399-abfd-2a929a7a7d80`) and whose value is
    /// simply `Collapsed`. This signals to compliant Coordinates and Plot action modules that the node and its descendants are "collapsed"
    /// and should be drawn accordingly.
    /// 
    /// The same result could be obtained by applying the same attribute using a different module (e.g. _Add attribute_ or _Custom script_).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Collapse node";
        public const string HelpText = "Collapses a node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "3812314b-e821-4399-abfd-2a929a7a7d80";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACQSURBVDhPYxhwwAilCQKv2o0OQAqE68ECENCI1wAcmg4C8QMgVgBiewwDCGgCYWRQDzegqKjo/2tWeYa3bCCDwQCXJmRQzwRlgMHiznyQgY5A3AjE9kAcD8Qgl4BcBDcZGaC4oK+vD8VLJIUBNgOQAc5YAGmEchjwGUAQIBtECkAJRHLAwBuAEgtQJj0BAwMA/AEwYiIJ9gMAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADKSURBVEhLYxgFhAAjlCYbuNdu0mZm+C8KZDoAcT1YEAEaSbaAgIEHgfgBhMmgAMT2BC0gxsD/DAxfgQa9hgihgHqsFnjVbgQZRo6B6ADVgqKioobXrPL1b9lAvgMDUg1EB/VMUAYM1C/uzAdZ6gjEjUBsD8TxQIEskBwQg3ylALQQFGREAXQf/O/r60MRo2ocYLMAHVCUioixAB0QygdQGgJAFkCZVAOM6IaS6gOSAC18gJ5MqQ5GLSAIhr4F6Dm5AUih58YRDRgYAEtqUavuODR2AAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAGQSURBVFhH7ZQ9SwNBEIZn9q4QLKws/ChEu1T2UTwhKCqkFKzsLYOlYmws8wP8DYKI/pwQzoAgWAiiYJMd390787G54s7cJYX3wLK3M3uZd9/ZC5XMGo7nKSC8f/lY8UQHzBwI0Y6QtAsUMF4QwcUoNyBHAWkK8oeIhIopRL5uIzb+R46aT+vU0zURvUXMuwitRpkIWPyJqcvCHWLpkshblAGsruxkFymZqKBLGgG5FnRJEtBoNJqYbCKc26Rvb8E8DjHUQy0hir7HiezEApRdDLBBg8L5xhGPmXwt4mODP/56dhJ/odVq8d3NKfseraDgMaTcIhxizGNUcMsPcYQz+HeO1pwwqSr2LTF6Yd7PgtsCe2wjwAYc6s2H5Z6mqhaqYcMeQmtRps8XXHqBji6x7uD5VYSTrOy3IJMAl4kE5SHA5eDifkMpL8BFDbA0Y+SrARBEz+YiR23MWYBLCoeKFeAy6pBso0ibf4sOU5SAJCb/kPPEuJHkSJHM3IFSQCmgFJD4VzxNXAeu47nkv0D0A4bH0ZO2zTH8AAAAAElFTkSuQmCC";

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
                /// This parameter selects the node that should be "collapsed". If only a single node is selected, that node is collapsed.
                /// If mode than one node is selected, the last common ancestor (LCA) of all of them is collapsed. Nodes are selected based
                /// on their `Name`. Note that this module will have no effect if the selected node is a tip of the tree.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Equalise lengths">
                /// If this check box is checked, the branch lengths of the descendants of the selected node are adjusted so that the
                /// collapsed node looks like an isosceles triangle. Otherwise, the triangle will have a shorter side corresponding to
                /// the position of the descendant that is closest to the selected node, and a longer side corresponding to the position
                /// of the descendant node that is farthest from the selected node.
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

            bool equaliseLengths = (bool)parameterValues["Equalise lengths"];

            string fillColourString = ((VectSharp.Colour)parameterValues["Fill colour:"]).ToHexString();

            List<TreeNode> leaves = node.GetLeaves();

            if (leaves.Count > 1)
            {
                if (equaliseLengths)
                {
                    double meanLength = 0;
                    for (int i = 0; i < leaves.Count; i++)
                    {
                        meanLength += node.PathLengthTo(leaves[i], TreeNode.NodeRelationship.Ancestor);
                    }
                    meanLength /= leaves.Count;

                    node.Children.Clear();
                    node.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    node.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    TreeNode child1 = new TreeNode(node) { Length = meanLength, Name = leaves[0].Name };
                    child1.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    child1.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    TreeNode child2 = new TreeNode(node) { Length = meanLength, Name = leaves[^1].Name };
                    child2.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    child1.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    node.Children.Add(child1);
                    node.Children.Add(child2);
                }
                else
                {
                    double minLength = double.MaxValue;
                    double maxLength = double.MinValue;

                    string minName = null;
                    string maxName = null;

                    for (int i = 0; i < leaves.Count; i++)
                    {
                        double leafLength = node.PathLengthTo(leaves[i], TreeNode.NodeRelationship.Ancestor);
                        if (leafLength > maxLength)
                        {
                            maxLength = leafLength;
                            maxName = leaves[i].Name;
                        }

                        if (leafLength < minLength)
                        {
                            minLength = leafLength;
                            minName = leaves[i].Name;
                        }
                    }

                    node.Children.Clear();
                    node.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    node.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    TreeNode child1 = new TreeNode(node) { Length = minLength, Name = minName };
                    child1.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    child1.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    TreeNode child2 = new TreeNode(node) { Length = maxLength, Name = maxName };
                    child2.Attributes["0c3400fd-8872-4395-83bc-a5dc5f4967fe"] = fillColourString;
                    child2.Attributes["3812314b-e821-4399-abfd-2a929a7a7d80"] = "Collapsed";

                    node.Children.Add(child1);
                    node.Children.Add(child2);
                }
            }
        }
    }
}