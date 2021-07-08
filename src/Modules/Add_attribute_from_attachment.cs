using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using PhyloTree.Extensions;

namespace AddAttributeFromAttachment
{
    /// <summary>
    /// This module can be used to add an attribute to a list of taxa that is loaded as an attachment. This module is best suited for
    /// a "presence-absence" kind of attribute (i.e. if you have a text file with a list of taxa that have a certain feature, and you
    /// want to convert this to attributes on the tree). For more complex attributes, use the _Parse node states_ module.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// To use this module, first of all you will have to load a text file containing one taxon name per line as an attachment.
    /// 
    /// This module parses the selected attachment and reads each line of the file. The requested attribute is added to
    /// all the nodes in the tree whose [match attribute](#match-attribute) corresponds to a line in the file (and/or to their ancestors,
    /// based on the value selected for the [Apply to](#apply-to) parameter).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Add attribute";
        public const string HelpText = "Adds an attribute based on an attachment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.1");
        public const string Id = "f71a5e60-5e40-4a5e-9795-e5259fb283ab";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Taxa", "Group:2" ),
                
                /// <param name="Taxon list:">
                /// This parameter is used to select the attachment that represents the text file containing the list of taxa (one taxon
                /// name per line).
                /// </param>
                ( "Taxon list:", "Attachment:" ),
                ( "The taxon list should be an attachment with one taxon per line", "Label:[\"Left\",\"Italic\",\"#808080\"]" ),

                /// <param name="Match attribute:">
                /// This parameter determines the attribute that is matched agains the list of taxa in the attachment.
                /// </param>
                ( "Match attribute:", "AttributeSelector:Name" ),
                
                /// <param name="Match attribute type:">
                /// This parameter is used to select the type of the [match attribute](#match-attribute).
                /// </param>
                ( "Match attribute type:", "AttributeType:String" ),
                
                /// <param name="Apply to:">
                /// The value selected for this parameter determines to which taxa the attribute is applied.
                /// 
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// | Selected value    | Nodes to which the attribute is applied                                                     |
                /// +===================+=============================================================================================+
                /// | Specified taxa    | Only to the taxa whose names appear in the taxon list.                                      |
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// | Specified taxa    | To the taxa whose names appear in the taxon list and to all their ancestors, up to the root |
                /// | and all ancestors | node of the tree.                                                                           |
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// | Specified taxa    | To the taxa whose names appear in the taxon list and to their ancestors up to the last      |
                /// | and ancestors up  | common ancestor of all these taxa.                                                          |
                /// | to LCA            |                                                                                             |
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// | LCA               | To the last common ancestor of the taxa whose names appear in the taxon list.               |
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// | LCA and all       | To the last common ancestor of the taxa whose names appear in the taxon list and to all its |
                /// | children          | descendants (even those that do not appear in the taxon list).                              |
                /// +-------------------+---------------------------------------------------------------------------------------------+
                /// </param>
                ( "Apply to:", "ComboBox:0[\"Specified taxa\",\"Specified taxa and all ancestors\",\"Specified taxa and ancestors up to LCA\",\"LCA\",\"LCA and all children\"]" ),

                ( "Attribute", "Group:3" ),
                
                /// <param name="Attribute:">
                /// The name of the new attribute to add to the specified taxa.
                /// </param>
                ( "Attribute:", "TextBox:" ),
                
                /// <param name="Attribute type:">
                /// The type of the new attribute.
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="New value:">
                /// The value of the attribute to add. Note that the same value will be used for all the nodes to which the attribute
                /// is added. This should be coherent with the [Attribute type](#attribute-type), i.e. a text string if the attribute
                /// type is `String` or a (decimal) number if the attribute type is `Number`.
                /// </param>
                ( "New value:", "TextBox:"),

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

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type:", attrType);
                }
            }

            if ((string)previousParameterValues["Match attribute:"] != (string)currentParameterValues["Match attribute:"])
            {
                string attributeName = (string)currentParameterValues["Match attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Match attribute type:"] == (string)currentParameterValues["Match attribute type:"])
                {
                    parametersToChange.Add("Match attribute type:", attrType);
                }
            }

            Attachment previousAttachment = (Attachment)previousParameterValues["Taxon list:"];
            Attachment newAttachment = (Attachment)currentParameterValues["Taxon list:"];

            return (bool)currentParameterValues["Apply"] || (previousAttachment != newAttachment);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            Attachment attachment = (Attachment)parameterValues["Taxon list:"];

            if (attachment != null)
            {
                string[] taxonListString = attachment.GetLines();

                string attributeName = (string)parameterValues["Attribute:"];

                string attrType = (string)parameterValues["Attribute type:"];

                string attrValue = (string)parameterValues["New value:"];

                int applyTo = (int)parameterValues["Apply to:"];

                string matchAttribute = (string)parameterValues["Match attribute:"];
                string matchAttributeType = (string)parameterValues["Match attribute type:"];

                double[] taxonListDouble = null;

                if (matchAttributeType == "Number")
                {
                    double elDouble = double.NaN;
                    taxonListDouble = (from el in taxonListString where double.TryParse(el, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out elDouble) && !double.IsNaN(elDouble) select elDouble).ToArray();
                }

                if (applyTo == 0 || applyTo == 1 || applyTo == 2)
                {
                    TreeNode lca = null;

                    if (applyTo == 2)
                    {
                        if (matchAttributeType == "String")
                        {
                            lca = GetLCA(tree, taxonListString, matchAttribute);
                        }
                        else if (matchAttributeType == "Number")
                        {
                            lca = GetLCA(tree, taxonListDouble, matchAttribute);
                        }
                    }

                    foreach (TreeNode leaf in tree.GetLeaves())
                    {
                        TreeNode node = leaf;

                        bool matches = false;

                        if (matchAttributeType == "String")
                        {
                            if (node.Attributes.TryGetValue(matchAttribute, out object attrObject) && attrObject is string matchAttrValue && !string.IsNullOrEmpty(matchAttrValue))
                            {
                                matches = taxonListString.Contains(matchAttrValue);
                            }
                        }
                        else if (matchAttributeType == "Number")
                        {
                            if (node.Attributes.TryGetValue(matchAttribute, out object attrObject) && attrObject is double matchAttrValue && !double.IsNaN(matchAttrValue))
                            {
                                matches = taxonListDouble.Contains(matchAttrValue);
                            }
                        }

                        if (matches)
                        {
                            TreeNode targetNode = applyTo == 0 ? node.Parent : applyTo == 1 ? null : lca?.Parent;

                            while (node != targetNode)
                            {
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
                                node = node.Parent;
                            }
                        }
                    }
                }
                else if (applyTo == 3 || applyTo == 4)
                {
                    TreeNode node = null;

                    if (matchAttributeType == "String")
                    {
                        node = GetLCA(tree, taxonListString, matchAttribute);
                    }
                    else if (matchAttributeType == "Number")
                    {
                        node = GetLCA(tree, taxonListDouble, matchAttribute);
                    }

                    if (node == null)
                    {
                        throw new Exception("Could not find the requested ancestor!");
                    }

                    if (applyTo == 3)
                    {
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
                    else
                    {
                        foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                        {
                            if (attributeName == "Name" && attrType == "String")
                            {
                                child.Name = attrValue;
                            }
                            else if (attributeName == "Support" && attrType == "Number")
                            {
                                child.Support = double.Parse(attrValue);
                            }
                            else if (attributeName == "Length" && attrType == "Number")
                            {
                                child.Length = double.Parse(attrValue);
                            }
                            else if (!string.IsNullOrEmpty(attrType))
                            {
                                if (attrType == "String")
                                {
                                    child.Attributes[attributeName] = attrValue;
                                }
                                else if (attrType == "Number")
                                {
                                    child.Attributes[attributeName] = double.Parse(attrValue);
                                }
                            }
                        }
                    }
                }
            }
        }

        public static TreeNode GetLCA(TreeNode tree, string[] taxonList, string attributeName)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            if (taxonList.Length > 0)
            {
                TreeNode seed = null;

                foreach (TreeNode node in nodes)
                {
                    if (node.Attributes.TryGetValue(attributeName, out object attrValue) && attrValue is string attrString && !string.IsNullOrEmpty(attrString) && taxonList.Contains(attrString))
                    {
                        seed = node;
                        break;
                    }
                }

                while (seed != null && !GetAllAttributesString(seed, attributeName).ContainsAll(taxonList))
                {
                    seed = seed.Parent;
                }

                return seed;
            }
            else
            {
                return null;
            }
        }

        public static TreeNode GetLCA(TreeNode tree, double[] taxonList, string attributeName)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            if (taxonList.Length > 0)
            {
                TreeNode seed = null;

                foreach (TreeNode node in nodes)
                {
                    if (node.Attributes.TryGetValue(attributeName, out object attrValue) && attrValue is double attrDouble && !double.IsNaN(attrDouble) && taxonList.Contains(attrDouble))
                    {
                        seed = node;
                        break;
                    }
                }

                while (seed != null && !GetAllAttributesDouble(seed, attributeName).ContainsAll(taxonList))
                {
                    seed = seed.Parent;
                }

                return seed;
            }
            else
            {
                return null;
            }
        }

        public static IEnumerable<string> GetAllAttributesString(TreeNode node, string attributeName)
        {
            foreach (TreeNode child in node.GetChildrenRecursiveLazy())
            {
                if (child.Attributes.TryGetValue(attributeName, out object attrValue) && attrValue is string attrString && !string.IsNullOrEmpty(attrString))
                {
                    yield return attrString;
                }
            }
        }

        public static IEnumerable<double> GetAllAttributesDouble(TreeNode node, string attributeName)
        {
            foreach (TreeNode child in node.GetChildrenRecursiveLazy())
            {
                if (child.Attributes.TryGetValue(attributeName, out object attrValue) && attrValue is double attrDouble && !double.IsNaN(attrDouble))
                {
                    yield return attrDouble;
                }
            }
        }
    }
}