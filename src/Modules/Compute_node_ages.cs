using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace NodeAges
{
    /// <summary>
    /// This module computes the ages of nodes, based on the branch lengths in the tree. The ages can be computed
    /// either as a distance from the root of the tree, or as the distance in time from the most recent tip of
    /// the tree.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Compute node ages";
        public const string HelpText = "Computes node ages.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "70ea5221-9faf-4792-b428-5fee9aa1a001";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Age type:">
                /// This parameter determines the kind of age that is computed.
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
                
                /// <param name="Attribute:">
                /// The name of the attribute in which the age of the nodes is stored. If an attribute with the same
                /// name already exists, its value will be replaced by this module. The type of the attribute will be
                /// `Number`.
                /// </param>
                ( "Attribute:", "TextBox:Age" ),

                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw
                /// of the tree.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();

            parametersToChange = new Dictionary<string, object>()
            {
                { "Apply", false }
            };


            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            string attributeName = (string)parameterValues["Attribute:"];

            bool fromLeft = (int)parameterValues["Age type:"] == 1;

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double treeHeight = tree.LongestDownstreamLength();

            for (int i = 0; i < nodes.Count; i++)
            {
                double age = nodes[i].UpstreamLength();

                if (!fromLeft)
                {
                    age = treeHeight - age;
                }

                nodes[i].Attributes[attributeName] = age;
            }
        }
    }
}
