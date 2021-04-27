using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;

namespace ad46f971574534d3aa498caf69e7808bc
{
    /// <summary>
    /// This module assigns to each leaf a number going from 1 to $n$ or from 0 to $n - 1$ (where $n$ is the number of leaves in the tree)
    /// and stores it on the tree as an attribute with the specified name.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Add index";
        public const string HelpText = "Adds the index of each leaf as an attribute on the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "d46f9715-7453-4d3a-a498-caf69e7808bc";

        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Attribute name:">
                /// This parameter determines the name of the attribute where the index value is stored. If the attribute already exists,
                /// it is overwritten.
                /// </param>
                ("Attribute name:", "TextBox:Index"),
                
                /// <param name="Order:">
                /// This parameter determines the order in which the leaves of the tree are numbered.
                /// </param>
                ("Order:","ComboBox:0[\"Ascending\",\"Descending\"]"),
                
                /// <param name="Start at:">
                /// This parameter determines whether the leaf indices go from 1 to $n$ or from 0 to $n - 1$. 
                /// </param>
                ("Start at:","ComboBox:1[\"0\",\"1\"]"),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameters and triggers a redraw of the tree.
                /// </param>
                ("Apply","Button:")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"] || ((int)previousParameterValues["Order:"] != (int)currentParameterValues["Order:"]) || ((int)previousParameterValues["Start at:"] != (int)currentParameterValues["Start at:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            int order = (int)parameterValues["Order:"];
            string attributeName = (string)parameterValues["Attribute name:"];
            int startAt = (int)parameterValues["Start at:"];

            List<TreeNode> leaves = tree.GetLeaves();

            for (int i = 0; i < leaves.Count; i++)
            {
                int index = -1;

                if (order == 0)
                {
                    index = i + startAt;
                }
                else if (order == 1)
                {
                    index = leaves.Count - 1 - i + startAt;
                }

                leaves[i].Attributes[attributeName] = (double)index;
            }
        }
    }
}
