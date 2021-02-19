using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Text.Json;

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

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
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