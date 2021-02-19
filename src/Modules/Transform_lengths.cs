using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;

namespace TransformLengths
{
    /// <summary>
    /// This module is used to transform the branch lengths of the tree, either by making them all equal, or by turning the tree into a cladogram.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Transform lengths";
        public const string HelpText = "Transforms all the branch lengths in the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f9241a42-c0cb-41a5-a1ee-a68fc339dee8";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Transform:">
                /// This parameter determines what kind of transformation is performed. If the value is `All equal`, all the branch lengths are set to a
                /// value of `1`. If the value is `Cladogram`, the branch lengths are adjusted so that the tips are all contemporaneous and the tree
                /// looks like a cladogram (the length of the shortest branch in this case will be `1`).
                /// </param>
                ( "Transform:", "ComboBox:0[\"All equal\",\"Cladogram\"]" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (int)currentParameterValues["Transform:"] != (int)previousParameterValues["Transform:"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            switch ((int)parameterValues["Transform:"])
            {
                case 0:
                    foreach (TreeNode node in nodes)
                    {
                        node.Length = 1;
                    }
                    break;
                case 1:
                    foreach (TreeNode node in nodes)
                    {
                        node.Length = 1;
                    }
                    double longestPath = tree.LongestDownstreamLength();

                    for (int i = 1; i < nodes.Count; i++)
                    {
                        double upstreamLength = nodes[i].UpstreamLength();
                        double downstreamLength = nodes[i].LongestDownstreamLength();

                        if (Math.Abs((upstreamLength + downstreamLength - longestPath) / longestPath) > 0.001)
                        {
                            double factor = (longestPath - upstreamLength + nodes[i].Length) / (downstreamLength + nodes[i].Length);
                            foreach (TreeNode node in nodes[i].GetChildrenRecursiveLazy())
                            {
                                node.Length *= factor;
                            }
                        }
                    }

                    break;
            }
        }

    }
}
