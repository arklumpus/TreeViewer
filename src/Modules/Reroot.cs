using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

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
        public static Version Version = new Version("1.0.0");
        public const string Id = "c6f96861-11c0-4853-9738-6a90cc81d660";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
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
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();
            return true;
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
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
