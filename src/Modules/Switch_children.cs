using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;

namespace SwitchChildren
{
    /// <summary>
    /// This module switches the children of a node, and, optionall, all their descendants.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Switch children";
        public const string HelpText = "Switches the order of the children of the node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "c4c71099-c7dc-44b3-93be-25a79afb1102";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Node:">
                /// This parameter determines the node whose children are switched. If only a
                /// single node is selected, that node's children are switched. If more than one node is
                /// selected, the node whose children are switched corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Recursive">
                /// If this check box is checked, the action is also applied recursively to all descendas of the
                /// selected node. Otherwise, only the direct children of the selected node are switched.
                /// </param>
                ( "Recursive", "CheckBox:false" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (bool)currentParameterValues["Recursive"] != (bool)previousParameterValues["Recursive"] || !((string[])currentParameterValues["Node:"]).SequenceEqual((string[])previousParameterValues["Node:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            bool recursive = (bool)parameterValues["Recursive"];

            if (!recursive)
            {
                List<TreeNode> newChildren = new List<TreeNode>(node.Children.Count);
                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    newChildren.Add(node.Children[i]);
                }
                node.Children.Clear();
                node.Children.AddRange(newChildren);
            }
            else
            {
                List<TreeNode> nodes = node.GetChildrenRecursive();
                for (int j = 0; j < nodes.Count; j++)
                {
                    List<TreeNode> newChildren = new List<TreeNode>(nodes[j].Children.Count);
                    for (int i = nodes[j].Children.Count - 1; i >= 0; i--)
                    {
                        newChildren.Add(nodes[j].Children[i]);
                    }
                    nodes[j].Children.Clear();
                    nodes[j].Children.AddRange(newChildren);
                }
            }
        }

    }
}
