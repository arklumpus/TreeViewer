using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace ChangeAttribute
{
    /// <summary>
    /// This module can be used to change the value of an attribute of a node of the tree.
    ///
    /// The module can be added manually, or by selecting a node from the tree plot and using the "pencil" button
    /// to change the value of an attribute.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the _Add attribute_ module is that this module only lets you select
    /// an attribute that already exists in the tree (and, thus, only lets you change the value of existing
    /// attributes); the _Add attribute_ module, instead, lets you choose an arbitrary name for the new attribute
    /// (and, thus, lets you create a new attribute).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Change attribute";
        public const string HelpText = "Changes the value of an already existing attribute.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8de06406-68e4-4bd8-97eb-2185a0dd1127";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Node:">
                /// This parameter is used to select the node on which to change the attribute. If only a
                /// single node is selected, the attribute is changed to that node. If more than one node is
                /// selected, the attribute change is applied to the last common ancestor (LCA) of all of them.
                /// Nodes are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),

                ( "Attribute", "Group:3" ),
                
                /// <param name="Attribute:">
                /// The attribute to change. If the selected node does not have the chosen attribute (which
                /// must exist elsewhere in the tree), the attribute is added to the node.
                /// </param>
                ( "Attribute:", "AttributeSelector:Name" ),
                
                /// <param name="Attribute type:">
                /// The type of the attribute (this can be different than the current type of the attribute).
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="New value:">
                /// The value for the new attribute. This should be coherent with the [Attribute type](#attribute-type),
                /// i.e. a text string if the attribute type is `String` or a (decimal) number if the attribute
                /// type is `Number`.
                /// </param>
                ( "New value:", "TextBox:"),
                
                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw
                /// of the tree.
                /// </param>
                ( "Apply", "Button:" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type:", attrType);
                }
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            if (node == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            string attributeName = (string)parameterValues["Attribute:"];

            string attrType = (string)parameterValues["Attribute type:"];

            string attrValue = (string)parameterValues["New value:"];

            if (attributeName == "Name" && attrType == "String")
            {
                node.Name = attrValue;
            }
            else if (attributeName == "Support" && attrType == "Number")
            {
                node.Support = double.Parse(attrValue);
            }
            else if (attributeName == "Length" && attrType == "Number")
            {
                node.Length = double.Parse(attrValue);
            }
            else if (!string.IsNullOrEmpty(attrType))
            {
                if (attrType == "String")
                {
                    node.Attributes[attributeName] = attrValue;
                }
                else if (attrType == "Number")
                {
                    node.Attributes[attributeName] = double.Parse(attrValue);
                }
            }
        }
    }
}
