using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using System.Linq;

namespace a85eee26d29b644708184f42ebe9a2567
{
    /// <summary>
    /// This module propagates the value of an attribute from the tips of the tree towards the root or vice versa.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Propagate attribute";
        public const string HelpText = "Propagates an attribute on the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "85eee26d-29b6-4470-8184-f42ebe9a2567";

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            string defaultAttribute = "Support";

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                foreach (KeyValuePair<string, object> attribute in node.Attributes)
                {
                    if (attribute.Key != "Length" && attribute.Key != "Support" && attribute.Key != "Name" && attribute.Value is double doubleValue && !double.IsNaN(doubleValue))
                    {
                        defaultAttribute = attribute.Key;
                        break;
                    }
                }
            }

            return new List<(string, string)>()
            {
                ("Attribute", "Group:2"),
                
                /// <param name="Attribute:">
                /// This parameter determines the attribute that is propagated."
                /// </param>
                ("Attribute:", "AttributeSelector:" + defaultAttribute),
                
                /// <param name="Attribute type:">
                /// This parameter determines the type of attribute that is propagated.
                /// </param>
                ("Attribute type:", "AttributeType:Number"),

                ("Propagation", "Group:6"),
                
                /// <param name="Direction:">
                /// This parameter determines whether the attribute values are propagated from the tips towards the root
                /// of the tree or vice versa.
                /// </param>
                ("Direction:", "ComboBox:0[\"From tips to root\",\"From root to tips\"]"),
                
                /// <param name="Propagation mode:" display="Propagation mode (from tips to root)">
                /// If the attribute is a Number and the propagation direction is `From tips to root`, this parameter
                /// determines the way the attribute values of the children of a node are coalesced to produce the attribute
                /// value for the parent node. If the selected value is `Average`, the average of the attribute values of the
                /// children is used; if the value is `Minimum`, the minimum value is selected; if the value is `Maximum`, the
                /// maximum value is selected. In all cases, not-a-number (NaN) values are excluded.
                /// </param>
                ("Propagation mode:", "ComboBox:0[\"Average\",\"Minimum\",\"Maximum\"]"),
                
                /// <param name="Propagation mode: " display="Propagation mode (from root to tips)">
                /// If the attribute is a Number and the propagation direction is `From root to tips`, this parameter
                /// determines the way the attribute values for the children of a node are computed starting from the value
                /// of the attribute for the parent node. If the value is `Preserve`, the child nodes inherit the same value
                /// as their parent. If the value is `Divide equally`, the value of the attribute for the parent is divided
                /// equally between all children (note that in this way the attribute values decreases exponentially). If the
                /// value is `Subtract`, the value of the attribute for the children is determined by subtracting a fixed
                /// amount (determined by the [Subtract value](#subtract-value) parameter) from the parent value.
                /// </param>
                ("Propagation mode: ", "ComboBox:0[\"Preserve\",\"Divide equally\",\"Subtract\"]"),
                
                /// <param name="Default value:">
                /// This parameter determines the default value of the attribute that is used for nodes where the attribute
                /// is not present.
                /// </param>
                ("Default value:", "TextBox:NaN"),
                
                /// <param name="Subtract value:">
                /// This parameter determines the value that is subtracted from the parent node value to compute the child
                /// node value when the [Propagation mode](#propagation-mode-from-root-to-tips) is `Subtract`.
                /// </param>
                ("Subtract value:", "NumericUpDown:0[\"-Infinity\",\"Infinity\"]"),
                
                /// <param name="Overwrite existing values">
                /// This check box determines whether the attribute value is overwritten for nodes which already have a value
                /// that has been specified for the attribute.
                /// </param>
                ("Overwrite existing values", "CheckBox:false"),

                /// <param name="Apply">
                /// This button applies the changes to the other parameters and triggers a redraw of the tree.
                /// </param>
                ("Apply", "Button:")
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

            if ((string)currentParameterValues["Attribute type:"] == "Number")
            {
                if ((int)currentParameterValues["Direction:"] == 0)
                {
                    controlStatus.Add("Propagation mode:", ControlStatus.Enabled);
                    controlStatus.Add("Propagation mode: ", ControlStatus.Hidden);
                    controlStatus.Add("Subtract value:", ControlStatus.Hidden);
                }
                else
                {
                    controlStatus.Add("Propagation mode:", ControlStatus.Hidden);
                    controlStatus.Add("Propagation mode: ", ControlStatus.Enabled);
                    if ((int)currentParameterValues["Propagation mode: "] == 2)
                    {
                        controlStatus.Add("Subtract value:", ControlStatus.Enabled);
                    }
                    else
                    {
                        controlStatus.Add("Subtract value:", ControlStatus.Hidden);
                    }
                }
            }
            else
            {
                controlStatus.Add("Propagation mode:", ControlStatus.Hidden);
                controlStatus.Add("Propagation mode: ", ControlStatus.Hidden);
                controlStatus.Add("Subtract value:", ControlStatus.Hidden);
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            string attribute = (string)parameterValues["Attribute:"];
            string attributeType = (string)parameterValues["Attribute type:"];

            int direction = (int)parameterValues["Direction:"];

            int propagationMode = (int)parameterValues["Propagation mode:"];
            int propagationMode2 = (int)parameterValues["Propagation mode: "];

            string defaultValue = (string)parameterValues["Default value:"];

            double defaultDouble = double.NaN;

            if (attributeType == "Number")
            {
                if (!double.TryParse(defaultValue, out defaultDouble))
                {
                    defaultDouble = double.NaN;
                }
            }

            double subtractValue = (double)parameterValues["Subtract value:"];

            bool overwrite = (bool)parameterValues["Overwrite existing values"];

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            if (direction == 0)
            {
                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    if (nodes[i].Children.Count == 0)
                    {
                        if (nodes[i].Attributes.TryGetValue(attribute, out object attributeValue))
                        {
                            if (overwrite)
                            {
                                if (attributeType == "String" && !(attributeValue is string))
                                {
                                    nodes[i].Attributes[attribute] = defaultValue;
                                }
                                else if (attributeType == "Number" && !(attributeValue is double))
                                {
                                    nodes[i].Attributes[attribute] = defaultDouble;
                                }
                            }
                        }
                        else
                        {
                            if (attributeType == "String")
                            {
                                nodes[i].Attributes[attribute] = defaultValue;
                            }
                            else if (attributeType == "Number")
                            {
                                nodes[i].Attributes[attribute] = defaultDouble;
                            }
                        }
                    }
                    else
                    {
                        if (overwrite || !nodes[i].Attributes.ContainsKey(attribute))
                        {
                            if (attributeType == "String")
                            {
                                bool found = false;

                                for (int j = 0; j < nodes[i].Children.Count; j++)
                                {
                                    if (nodes[i].Children[j].Attributes.TryGetValue(attribute, out object attributeValue) && attributeValue is string attributeStringValue && !string.IsNullOrEmpty(attributeStringValue))
                                    {
                                        found = true;
                                        nodes[i].Attributes[attribute] = attributeStringValue;
                                        break;
                                    }
                                }

                                if (!found)
                                {
                                    nodes[i].Attributes[attribute] = defaultValue;
                                }
                            }
                            else if (attributeType == "Number")
                            {
                                List<double> values = new List<double>();

                                for (int j = 0; j < nodes[i].Children.Count; j++)
                                {
                                    if (nodes[i].Children[j].Attributes.TryGetValue(attribute, out object attributeValue) && attributeValue is double attributeDoubleValue)
                                    {
                                        if (!double.IsNaN(attributeDoubleValue))
                                        {
                                            values.Add(attributeDoubleValue);
                                        }
                                    }
                                    else
                                    {
                                        if (!double.IsNaN(defaultDouble))
                                        {
                                            values.Add(defaultDouble);
                                        }
                                    }
                                }

                                if (values.Count > 0)
                                {
                                    switch (propagationMode)
                                    {
                                        case 0:
                                            nodes[i].Attributes[attribute] = values.Average();
                                            break;

                                        case 1:
                                            nodes[i].Attributes[attribute] = values.Min();
                                            break;

                                        case 2:
                                            nodes[i].Attributes[attribute] = values.Max();
                                            break;
                                    }
                                }
                                else
                                {
                                    nodes[i].Attributes[attribute] = defaultDouble;
                                }
                            }
                        }
                    }
                }
            }
            else if (direction == 1)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (nodes[i].Children.Count > 0)
                    {
                        if (attributeType == "String")
                        {
                            if (nodes[i].Parent == null)
                            {
                                if (!nodes[i].Attributes.ContainsKey(attribute))
                                {
                                    nodes[i].Attributes[attribute] = defaultValue;
                                }
                            }

                            string currValue = defaultValue;

                            if (nodes[i].Attributes.TryGetValue(attribute, out object nodeAttributeValue) && nodeAttributeValue is string nodeAttributeString && !string.IsNullOrEmpty(nodeAttributeString))
                            {
                                currValue = nodeAttributeString;
                            }

                            for (int j = 0; j < nodes[i].Children.Count; j++)
                            {
                                if (overwrite || !nodes[i].Children[j].Attributes.ContainsKey(attribute))
                                {
                                    nodes[i].Children[j].Attributes[attribute] = currValue;
                                }
                            }
                        }
                        else if (attributeType == "Number")
                        {
                            if (nodes[i].Parent == null)
                            {
                                if (!nodes[i].Attributes.ContainsKey(attribute))
                                {
                                    nodes[i].Attributes[attribute] = defaultDouble;
                                }
                            }

                            double currValue = defaultDouble;

                            if (nodes[i].Attributes.TryGetValue(attribute, out object nodeAttributeValue) && nodeAttributeValue is double nodeAttributeDouble && !double.IsNaN(nodeAttributeDouble))
                            {
                                currValue = nodeAttributeDouble;
                            }

                            for (int j = 0; j < nodes[i].Children.Count; j++)
                            {
                                if (overwrite || !nodes[i].Children[j].Attributes.ContainsKey(attribute))
                                {
                                    if (propagationMode2 == 0)
                                    {
                                        nodes[i].Children[j].Attributes[attribute] = currValue;
                                    }
                                    else if (propagationMode2 == 1)
                                    {
                                        nodes[i].Children[j].Attributes[attribute] = currValue / nodes[i].Children.Count;
                                    }
                                    else if (propagationMode2 == 2)
                                    {
                                        nodes[i].Children[j].Attributes[attribute] = currValue - subtractValue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
