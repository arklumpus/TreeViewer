using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;

namespace RerootTree
{
    /// <summary>
    /// This module is used to re-root the tree using a specific node as the outgroup.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Reroot tree";
        public const string HelpText = "Re-roots the tree using the specified outgroup.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const string Id = "c6f96861-11c0-4853-9738-6a90cc81d660";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Rooting mode:">
                /// This parameter determines the algorithm used to re-root the tree. If the selected value is
                /// `Mid-point`, the root of the tree is determined automatically as the point halfway between
                /// the two most distant tips. If the selected value is `Outgroup`, the root of the tree is
                /// determined by the user-selected outgroup.
                /// </param>
                ("Rooting mode:", "ComboBox:1[\"Mid-point\",\"Outgroup\"]"),
                
                /// <param name="Outgroup:">
                /// This parameter determines the node used as the outgroup. If only a
                /// single node is selected, the outgroup corresponds to that node. If more than one node is
                /// selected, the outgroup corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Outgroup:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Position:">
                /// This parameter determines which proportion of the branch on which the root is placed is
                /// ascribed to the outgroup and which proportion to the ingroup. If the value is `0%`, the
                /// outgroup has branch length 0 and the ingroup's branch length corresponds to the original
                /// length of the branch on which the root is placed; if the value is `100%`, the ingroup has
                /// branch length 0 and the outgroup's branch length corresponds to the original branch length.
                /// </param>
                ( "Position:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            if ((int)currentParameterValues["Rooting mode:"] == 0)
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Outgroup:", ControlStatus.Hidden },
                    { "Position:", ControlStatus.Hidden }
                };
            }
            else
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Outgroup:", ControlStatus.Enabled },
                    { "Position:", ControlStatus.Enabled }
                };
            }

            parametersToChange = new Dictionary<string, object>();
            return true;
        }

        private static (double, TreeNode) GetDeepestTip(TreeNode node)
        {
            if (node.Children.Count == 0)
            {
                return (0, node);
            }
            else
            {
                double maxLen = 0;
                TreeNode maxDeepestTip = null;
                for (int i = 0; i < node.Children.Count; i++)
                {
                    (double chLen, TreeNode deepestTip) = GetDeepestTip(node.Children[i]);
                    chLen += node.Children[i].Length;

                    if (chLen > maxLen)
                    {
                        maxLen = chLen;
                        maxDeepestTip = deepestTip;
                    }
                }

                return (maxLen, maxDeepestTip);
            }
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            int rootingMode = (int)parameterValues["Rooting mode:"];

            if (rootingMode == 0)
            {
                double maxDistance = double.MinValue;
                bool found = false;

                TreeNode subject = tree;

                if (subject.Children.Count < 3)
                {
                    subject = subject.GetUnrootedTree();
                }

                (double _, TreeNode leaf1) = GetDeepestTip(subject);

                TreeNode rerooted = subject.GetRootedTree(leaf1);
                rerooted.Children.Remove(leaf1);

                (double _, TreeNode leaf2) = GetDeepestTip(rerooted);

                if (leaf1 != null && leaf2 != null)
                {
                    leaf2 = subject.GetNodeFromId(leaf2.Id);

                    found = true;
                    maxDistance = leaf1.PathLengthTo(leaf2);
                }

                if (found)
                {
                    TreeNode lca = TreeNode.GetLastCommonAncestor(new TreeNode[] { leaf1, leaf2 });

                    List<TreeNode> path = new List<TreeNode>();
                    List<TreeNode> path2 = new List<TreeNode>();

                    TreeNode anc1 = leaf1;
                    TreeNode anc2 = leaf2;

                    while (anc1 != lca)
                    {
                        path.Add(anc1);
                        anc1 = anc1.Parent;
                    }

                    while (anc2 != lca)
                    {
                        path2.Add(anc2);
                        anc2 = anc2.Parent;
                    }

                    for (int i = path2.Count - 1; i >= 0; i--)
                    {
                        path.Add(path2[i]);
                    }

                    int index = 0;
                    double currDistance = 0;

                    while (currDistance + (!double.IsNaN(path[index].Length) ? path[index].Length : 0) <= maxDistance * 0.5)
                    {
                        currDistance += (!double.IsNaN(path[index].Length) ? path[index].Length : 0);
                        index++;
                    }

                    double proportion = (maxDistance * 0.5 - currDistance) / path[index].Length;

                    if (index >= path.Count - path2.Count)
                    {
                        proportion = 1 - proportion;
                    }

                    tree = subject.GetRootedTree(path[index], proportion);
                }
                else
                {
                    throw new Exception("Could not find a suitable branch for rooting. Does the tree have branch lengths?");
                }
            }
            else if (rootingMode == 1)
            {
                string[] outgroupElements = (string[])parameterValues["Outgroup:"];

                TreeNode outgroup = tree.GetLastCommonAncestor(outgroupElements);

                if (outgroup == null)
                {
                    throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
                }

                double position = (double)parameterValues["Position:"];

                tree = tree.GetRootedTree(outgroup, position);
            }
        }
    }
}
