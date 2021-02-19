using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;

namespace PruneNode
{
    /// <summary>
    /// This module is used to prune a subtree from the tree.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Prune node";
        public const string HelpText = "Prunes a node off the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "ffc97742-4cf5-44ef-81aa-d5b51708a003";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Node:">
                /// This parameter determines the node to prune off the tree. If only a
                /// single node is selected, the that node is pruned. If more than one node is
                /// selected, the last common ancestor (LCA) of all of them is pruned. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Position:">
                /// This parameter determines the relative position along the branch leading to the [Node](#node) at
                /// which the pruning is performed. If the value is `0`, the node is completely removed from the
                /// tree, including the branch that led to it. If the value is `1`, The children of the node are removed
                /// from the tree, but the branch leading to the node and the node itself are still kept in the same position.
                /// If the value is between `0` and `1`, the children of the selected node are removed from the tree
                /// and the length of the branch leading to the node is multiplied by this value.
                /// </param>
                ( "Position:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Leave one-child parent">
                /// If the [Position](#position) is 0 and the parent node of the selected node had two children (the selected
                /// node, which is being removed, and another node), after the pruning, the parent node should be left with a
                /// single children (the other node). If this check box is unchecked, the parent node is also removed from the
                /// tree, and the other child node is grafted onto the parent's parent, with its branch length incremented by
                /// the branch length of the parent.
                /// </param>
                ( "Leave one-child parent", "CheckBox:false" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw of the tree.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            controlStatus["Leave one-child parent"] = (double)currentParameterValues["Position:"] == 0 ? ControlStatus.Enabled : ControlStatus.Hidden;

            return (bool)currentParameterValues["Apply"] || !((string[])previousParameterValues["Node:"]).SequenceEqual((string[])currentParameterValues["Node:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            double position = (double)parameterValues["Position:"];

            bool leaveParent = (bool)parameterValues["Leave one-child parent"];

            if (node == tree || (node.Parent == tree && tree.Children.Count < 3 && (from el in tree.Children where el != node select el).First().Children.Count == 0 && position == 0))
            {
                throw new Exception("Cannot remove all nodes from the tree!");
            }

            if (position == 0)
            {
                node.Parent.Children.Remove(node);

                if (!leaveParent)
                {
                    if (node.Parent.Children.Count == 1)
                    {
                        TreeNode parent = node.Parent;
                        TreeNode otherChild = node.Parent.Children[0];
                        if (parent.Parent != null)
                        {
                            int index = parent.Parent.Children.IndexOf(parent);
                            parent.Parent.Children[index] = otherChild;
                            otherChild.Length += parent.Length;
                            otherChild.Parent = parent.Parent;
                        }
                        else
                        {
                            if (parent.Length > 0)
                            {
                                otherChild.Length += parent.Length;
                            }

                            otherChild.Parent = null;
                            tree = otherChild;
                        }
                    }
                }
            }
            else
            {
                node.Children.Clear();
                node.Length *= position;
            }
        }
    }
}