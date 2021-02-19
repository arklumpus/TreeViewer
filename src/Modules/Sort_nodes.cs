using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using System;

namespace SortNodes
{
    /// <summary>
    /// This module sort the nodes of the tree based on the number of levels of descendants. If two nodes have the same number
    /// of levels of descendants, they are sorted alphabetically based on the taxon names.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Sort nodes";
        public const string HelpText = "Sorts the nodes of the tree in ascending or descending order.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8a3e4e83-6c4d-45a8-8737-bff99accd176";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Order:">
                /// This parameter determines the order according to which the nodes are sorted.
                /// </param>
                ( "Order:", "ComboBox:0[\"Ascending\",\"Descending\"]" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (int)currentParameterValues["Order:"] != (int)previousParameterValues["Order:"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            tree.SortNodes((int)parameterValues["Order:"] == 1);
        }

    }
}
