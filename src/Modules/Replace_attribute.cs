using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;

namespace ReplaceAttribute
{
    /// <summary>
    /// This module is used to add or change an attribute of the tree depending on the value of another attribute.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The two attributes can be the same, which causes the value of the attribute to be changed in-place (e.g. transform
    /// all taxon names to lowercase); if the two attributes are different, a new attribute can be added based on the
    /// value of an existing attribute (e.g. add an attribute to all nodes with a support value lower than a certain
    /// threshold).
    /// 
    /// This module is also used by the "Replace" function of the _Search_ module.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Replace attribute";
        public const string HelpText = "Changes the value of an attribute of a node that matches a criterion.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f17160ad-0462-449a-8a57-e1af775c92ba";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Search attribute", "Group:6" ),
                
                /// <param name="Attribute:" display="Attribute (search attribute)">
                /// This parameter determines the attribute that needs to match the search criterion. If the attribute name
                /// entered here does not exist in the tree, the module does nothing.
                /// </param>
                ( "Attribute:", "TextBox:" ),
                
                /// <param name="Attribute type:" display="Attribute type (search attribute)">
                /// This parameter should correspond to the correct attribute type for the attribute that needs to match the
                /// search criterion. If the attribute type is incorrect, the module does nothing.
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="Value:" display="Value (search attribute)">
                /// This text box is used to enter the value that needs to be matched.
                /// </param>
                ( "Value:", "TextBox:"),
                
                /// <param name="Comparison type: " display="Comparison type (`Number`)">
                /// If the [Attribute type](#attribute-type-search-attribute) of the attribute that is being matched is `Number`,
                /// the module can match attributes that are equal, smaller than or greather than the specified [Value](#value).
                /// </param>
                ( "Comparison type: ", "ComboBox:0[\"Equal\", \"Smaller than\", \"Greater than\"]"),
                
                /// <param name="Comparison type:" display="Comparison type (`String`)">
                /// If the [Attribute type](#attribute-type-search-attribute) of the attribute that is being matched is `String`,
                /// this parameter determines how the strings are compared. If the value is `Normal`, the strings need to match
                /// exactly. If the value is `Case insensitive`, the case of the strings does not matter (e.g. `AaBbCc` matches
                /// both `aabbcc` and `AABBCC`). If the value is `Culture-aware`, the comparison takes into account culture-specific
                /// rules of the current display language of the OS (for example, in Hungarian `ddzs` would match `dzsdzs`).
                /// </param>
                ( "Comparison type:", "ComboBox:0[\"Normal\", \"Case-insensitive\", \"Culture-aware\", \"Culture-aware, case-insensitive\"]"),
                
                /// <param name="Regex">
                /// If this check box is checked, string matches are performed using a regular expression. This makes it possible to
                /// search for complicated strings.
                /// </param>
                ( "Regex", "CheckBox:false"),
                
                /// <param name="Apply recursively to all children">
                /// If this check box is checked, the replacement attribute is applied to all descendants of matching nodes, regardless
                /// of whether the descendant actually matches the search criterion or not.
                /// </param>
                ( "Apply recursively to all children", "CheckBox:false"),

                ( "Replace attribute", "Group:3" ),
                
                /// <param name="Attribute: " display="Attribute (replace attribute)">
                /// This parameter determines the name of the attribute whose value will be changed by this module. If the attribute already
                /// exists, its value will be replaced. If the attribute does not currently exist, it will be added to matching nodes. This
                /// attribute can be the same as the [Attribute](#attribute-search-attribute) that is being matched.
                /// </param>
                ( "Attribute: ", "TextBox:" ),
                
                /// <param name="Attribute type: " display="Attribute type (replace attribute)">
                /// This parameter determines the type of the new attribute.
                /// </param>
                ( "Attribute type: ", "AttributeType:String"),
                
                /// <param name="Value: " display="Value (replace attribute)">
                /// This text box determines the value for the new attribute. The value should be coherent with the [Attribute type](#attribute-type-replace-attribute),
                /// i.e. a (decimal) number if the attribute type is `Number` or a text string if the attribute type is `String`.
                /// 
                /// If the [Regex](#regex) check box is checked, it is possible to recall the value of capture groups here. Note that capture
                /// groups are specified by prepending the group index with a `$` sign (e.g. `$2` represents the second capture group in
                /// the regex).
                /// </param>
                ( "Value: ", "TextBox:"),
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((string)currentParameterValues["Attribute type:"] == "String")
            {
                controlStatus.Add("Comparison type:", ControlStatus.Enabled);
                controlStatus.Add("Comparison type: ", ControlStatus.Hidden);
                controlStatus.Add("Regex", ControlStatus.Enabled);
            }
            else if ((string)currentParameterValues["Attribute type:"] == "Number")
            {
                controlStatus.Add("Comparison type:", ControlStatus.Hidden);
                controlStatus.Add("Comparison type: ", ControlStatus.Enabled);
                controlStatus.Add("Regex", ControlStatus.Hidden);
            }

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type:", attrType);

                    if (attrType == "String")
                    {
                        controlStatus["Comparison type:"] = ControlStatus.Enabled;
                        controlStatus["Comparison type: "] = ControlStatus.Hidden;
                        controlStatus["Regex"] = ControlStatus.Enabled;
                    }
                    else if (attrType == "Number")
                    {
                        controlStatus["Comparison type:"] = ControlStatus.Hidden;
                        controlStatus["Comparison type: "] = ControlStatus.Enabled;
                        controlStatus["Regex"] = ControlStatus.Hidden;
                    }
                }
            }

            if ((string)previousParameterValues["Attribute: "] != (string)currentParameterValues["Attribute: "])
            {
                string attributeName = (string)currentParameterValues["Attribute: "];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type: ", attrType);
                }
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();
            bool applyToChildren = (bool)parameterValues["Apply recursively to all children"];

            string attributeName = (string)parameterValues["Attribute:"];

            string attrType = (string)parameterValues["Attribute type:"];

            string attrValue = (string)parameterValues["Value:"];

            double numberNeedle = attrType == "Number" ? double.Parse(attrValue) : -1;

            string replacementName = (string)parameterValues["Attribute: "];

            string replacementType = (string)parameterValues["Attribute type: "];

            string replacementValue = (string)parameterValues["Value: "];

            int comparisonType = attrType == "String" ? (int)parameterValues["Comparison type:"] : (int)parameterValues["Comparison type: "];

            bool regex = (bool)parameterValues["Regex"];

            StringComparison comparison = StringComparison.InvariantCulture;
            RegexOptions options = RegexOptions.CultureInvariant;
            switch (comparisonType)
            {
                case 0:
                    comparison = StringComparison.InvariantCulture;
                    options = RegexOptions.CultureInvariant;
                    break;
                case 1:
                    comparison = StringComparison.InvariantCultureIgnoreCase;
                    options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                    break;
                case 2:
                    comparison = StringComparison.CurrentCulture;
                    options = RegexOptions.None;
                    break;
                case 3:
                    comparison = StringComparison.CurrentCultureIgnoreCase;
                    options = RegexOptions.IgnoreCase;
                    break;
            }


            Regex reg = regex ? new Regex(attrValue, options) : null;

            foreach (TreeNode node in nodes)
            {
                bool matched = false;

                if (node.Attributes.TryGetValue(attributeName, out object attributeValue))
                {
                    if (attrType == "String" && attributeValue is string actualValue)
                    {
                        if (regex)
                        {
                            if (reg.IsMatch(actualValue))
                            {
                                matched = true;
                            }
                        }
                        else
                        {
                            if (actualValue.Contains(attrValue, comparison))
                            {
                                matched = true;
                            }
                        }
                    }
                    else if (attrType == "Number" && attributeValue is double actualNumber)
                    {
                        switch (comparisonType)
                        {
                            case 0:
                                if (actualNumber == numberNeedle)
                                {
                                    matched = true;
                                }
                                break;
                            case 1:
                                if (actualNumber < numberNeedle)
                                {
                                    matched = true;
                                }
                                break;
                            case 2:
                                if (actualNumber > numberNeedle)
                                {
                                    matched = true;
                                }
                                break;
                        }
                    }
                }

                if (matched)
                {
                    string currentReplacementValue = replacementValue;

                    if (attributeName.Equals(replacementName, StringComparison.OrdinalIgnoreCase) && attrType == "String" && replacementType == "String")
                    {
                        if (!regex)
                        {
                            currentReplacementValue = ((string)attributeValue).Replace(attrValue, replacementValue, comparison);
                        }
                        else
                        {
                            currentReplacementValue = reg.Replace(((string)attributeValue), replacementValue);
                        }
                    }

                    if (!applyToChildren)
                    {
                        if (replacementName == "Name" && replacementType == "String")
                        {
                            node.Name = currentReplacementValue;
                        }
                        else if (replacementName == "Support" && replacementType == "Number")
                        {
                            node.Support = double.Parse(currentReplacementValue);
                        }
                        else if (replacementName == "Length" && replacementType == "Number")
                        {
                            node.Length = double.Parse(currentReplacementValue);
                        }
                        else if (!string.IsNullOrEmpty(currentReplacementValue))
                        {
                            if (replacementType == "String")
                            {
                                node.Attributes[replacementName] = currentReplacementValue;
                            }
                            else if (replacementType == "Number")
                            {
                                node.Attributes[replacementName] = double.Parse(currentReplacementValue);
                            }
                        }
                    }
                    else
                    {
                        foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                        {
                            if (replacementName == "Name" && replacementType == "String")
                            {
                                child.Name = currentReplacementValue;
                            }
                            else if (replacementName == "Support" && replacementType == "Number")
                            {
                                child.Support = double.Parse(currentReplacementValue);
                            }
                            else if (replacementName == "Length" && replacementType == "Number")
                            {
                                child.Length = double.Parse(currentReplacementValue);
                            }
                            else if (!string.IsNullOrEmpty(replacementType))
                            {
                                if (replacementType == "String")
                                {
                                    child.Attributes[replacementName] = currentReplacementValue;
                                }
                                else if (replacementType == "Number")
                                {
                                    child.Attributes[replacementName] = double.Parse(currentReplacementValue);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
