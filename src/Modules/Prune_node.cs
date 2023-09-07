/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using System.Text.RegularExpressions;
using VectSharp;
using System.Runtime.InteropServices;

namespace PruneNode
{
    /// <summary>
    /// This module is used to prune a subtree from the tree.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Prune node";
        public const string HelpText = "Prunes nodes off the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.3.0");
        public const string Id = "ffc97742-4cf5-44ef-81aa-d5b51708a003";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEtSURBVDhPrVGhUsRADM2WBQECc2j4jSro4DiB5jeKQgC3g0C134BDwwzYO1R/AVkU5gwCBDBT8tKks73juBngzWyzyUtesin9K/I839PrHBZxTq0ljPlkZVlOJKiY5Q5ObxohGB6fphEfRSJYFAU512pHxUCtlu4vDiXBQ214divBGBy3JBs9sHD9uLHbdQe8KekUPRy/Svfz1qMrncrFT2jnjDAzsqG3lx8FABYZsbHOgYvhf4tE7a+xorZD1D3wecA9TVNXVVXv1xrkCUeXd6OXt08ZefBe0+DjiXhs4Viwma5u03RtBy5trvtwfTLsnpRYMRMZDhKft/bRXYA7YsYjFzVKU2LFrDrBsSTlaRn/5yV6VgysOOaxMgRwR0xYxjJ+bokg4yUBi3miL6eLpTCC5hLNAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGXSURBVEhLtZSxTsQwDIZbrsCKxAPAgHiHbtcVkHgFFnYoiA2Vig3dFR6BV0DiWI+tr8EM4gUQ6LDT31GaJrlWHJ+Uc+y6ju06F/03MeRKOLh+WmCrWYPU5Hl+g+1S+vh2DiAKevEZey/wKRqtzcvtcSyrc0BVVTGtI6hOkPlho4VJXH3zwRlR8DFtJfMZ/4RiuFrkxAh+2VgUE0gvg6YIfZfWlNTK1keWSjgZZSASyA5WsAxS9Jkd3EeoRWb5cywh2BquRJb3AMrwlYRkbpLhWS+WfgOrVdya4AjbjCCdUHDu82mjKfbTNI3ruv57BRhJs+8mvdvkPMAOTsGUH9nNC9XrkM5H5uAf6ztm5iUko/fsg0SC6ApopLjf+s9r++stGi2+zx/vLh5gUpxcTc9+4uT+c2MXFkVJl8t5L1QFRvAMtzDjAO+be1v83IRtCK59aRWI0UFaJMFVTyHVi6xbDPFV2Y/5xkFtwXZ+DnWQrxD6q1gdrtNZZztUzRBfRirg8ZvLi5A8quaICkN8/WNKeEevv28U/QJdKMMEMy5iTgAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAH6SURBVFhH5ZY7TgMxEIadBzQpoyA6qKihIxVJC0jcJT0KES2KuAFnQCK0hIZcIRJN0tAgCqADKTC/M2t5d/3cTRr4JOPXjv/xeOwg/j0VrtfK8fntDzdzVLn+2yACtigYI9Dr9awh8xFrm3OAFrjT6xiK2K4sCVn0ZNkTg+FweMFtlYT3l2c5vborQ0PAohlxMMafkLVL3QKLeJd2Pw7dWKkjMIinQp/gPAKujbgEaA61PjcyifvwRsDkBNd9rgHET7mdwxUBbw7wwqNlTwLhYHEfwTlgiAQIEnclZMwtuOJaxzQWRUwEkGB66IEx62Ooce3EIg467Xa7MplM5MNTBK8DBnEk5DOVPdkr6YTTAYs4zv2JSotKaSesDpB4h6qbZU8ixenMxyQ0I8EX6s+p4DsAJx4xx/0gjA5A/LPWvP6ob203Fu8YUuLoAHYCTenE68aOWFTqB53D/WmME6lbQPc1l2zNr5lofc/lDwwPpYCzJP7wtrnLI4oBvXzeG6LeAU0cT22XywALTxtHSZhzYI7FU3ZU+rymExkBXTzr9TrmdJIIWD/kMbkjOZCmqJ2iSp4m4XVdITmnfau3o+yylPqPaBVUKVTJDqxeEnJO+1ZvR9llSSJgzVoek2ctB9IUtVOodyBjoO/OmmigqF2C9yEivIsUtRNCiF+43xNi/2MuCQAAAABJRU5ErkJggg==";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Action:">
                /// This parameter determines whether node(s) that have been selected (either by directly specifying a
                /// node, or through an attribute match) are pruned or if everything else is pruned. This is useful,
                /// e.g., if you wish to keep just a small part of the tree while pruning everything else.
                /// </param>
                ( "Action:", "ComboBox:0[\"Prune selection\",\"Keep only selection\"]" ),
                
                /// <param name="Mode:">
                /// This parameter determines whether a single node is pruned, or whether all nodes matching
                /// a search criterion are pruned.
                /// </param>
                ( "Mode:", "ComboBox:0[\"Single node\",\"Attribute match\"]" ),
                
                /// <param name="Node:">
                /// This parameter determines the node to prune off the tree. If only a
                /// single node is selected, the that node is pruned. If more than one node is
                /// selected, the last common ancestor (LCA) of all of them is pruned. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),

                ( "Attribute match", "Group:7" ),
                
                /// <param name="Attribute:">
                /// This parameter determines the attribute that needs to match the search criterion. If the attribute name
                /// entered here does not exist in the tree, the module does nothing.
                /// </param>
                ( "Attribute:", "TextBox:" ),
                
                /// <param name="Attribute type:">
                /// This parameter should correspond to the correct attribute type for the attribute that needs to match the
                /// search criterion. If the attribute type is incorrect, the module does nothing.
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="Value:">
                /// This text box is used to enter the value that needs to be matched.
                /// </param>
                ( "Value:", "TextBox:"),
                
                /// <param name="Comparison type: " display="Comparison type (`Number`)">
                /// If the [Attribute type](#attribute-type) of the attribute that is being matched is `Number`,
                /// the module can match attributes that are equal, smaller than or greather than the specified [Value](#value).
                /// </param>
                ( "Comparison type: ", "ComboBox:0[\"Equal\", \"Smaller than\", \"Greater than\"]"),
                
                /// <param name="Comparison type:" display="Comparison type (`String`)">
                /// If the [Attribute type](#attribute-type) of the attribute that is being matched is `String`,
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

                /// <param name="Match leaves only">
                /// If this check box is checked, only leaves (tips of the tree) are matched, and not internal nodes. Internal nodes will still
                /// be pruned appropriately, if the [Leave one-child parent](#leave-one-child-parent) check box is unchecked.
                /// </param>
                ( "Match leaves only", "CheckBox:false" ),
                
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
                
                /// <param name="Keep pruned node names">
                /// If this check box is checked, the Names of nodes that have been pruned are kept as the Names of the stumps
                /// (where the stump does not already have a name) and stored in an attribute on the tree. The stump is the
                /// surviving ancestor of the node that has been pruned.
                /// </param>
                ( "Keep pruned node names", "CheckBox:false"),
                
                /// <param name="Attribute name:">
                /// If the [Keep pruned node names](#keep-pruned-node-names) checkbox is checked, the names of the nodes that
                /// descend from a pruned node are stored on the stump in this attribute.
                /// </param>
                ( "Attribute name:", "TextBox:UnderlyingNodes" ),
                
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

            if ((bool)currentParameterValues["Keep pruned node names"])
            {
                controlStatus.Add("Attribute name:", ControlStatus.Enabled);
            }
            else
            {
                controlStatus.Add("Attribute name:", ControlStatus.Hidden);
            }

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if ((string)previousParameterValues["Attribute type:"] == (string)currentParameterValues["Attribute type:"])
                {
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
            }

            if ((int)currentParameterValues["Mode:"] == 0)
            {
                controlStatus["Attribute match"] = ControlStatus.Hidden;
                controlStatus["Attribute:"] = ControlStatus.Hidden;
                controlStatus["Attribute type:"] = ControlStatus.Hidden;
                controlStatus["Value:"] = ControlStatus.Hidden;
                controlStatus["Comparison type: "] = ControlStatus.Hidden;
                controlStatus["Comparison type:"] = ControlStatus.Hidden;
                controlStatus["Regex"] = ControlStatus.Hidden;
                controlStatus["Match leaves only"] = ControlStatus.Hidden;
                controlStatus["Node:"] = ControlStatus.Enabled;
            }
            else if ((int)currentParameterValues["Mode:"] == 1)
            {
                controlStatus["Attribute match"] = ControlStatus.Enabled;
                controlStatus["Attribute:"] = ControlStatus.Enabled;
                controlStatus["Attribute type:"] = ControlStatus.Enabled;
                controlStatus["Value:"] = ControlStatus.Enabled;
                controlStatus["Match leaves only"] = ControlStatus.Enabled;
                controlStatus["Node:"] = ControlStatus.Hidden;
            }

            return (bool)currentParameterValues["Apply"] || !((string[])previousParameterValues["Node:"]).SequenceEqual((string[])currentParameterValues["Node:"]) || (int)previousParameterValues["Action:"] != (int)currentParameterValues["Action:"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            double position = (double)parameterValues["Position:"];

            bool leaveParent = (bool)parameterValues["Leave one-child parent"];

            int mode = (int)parameterValues["Mode:"];

            bool keepNodeNames = (bool)parameterValues["Keep pruned node names"];

            string storeAttributeName = (string)parameterValues["Attribute name:"];

            int action = (int)parameterValues["Action:"];
            bool matchLeavesOnly = (bool)parameterValues["Match leaves only"];

            if (mode == 0)
            {
                string[] nodeElements = (string[])parameterValues["Node:"];

                TreeNode node = tree.GetLastCommonAncestor(nodeElements);

                if (action == 0)
                {
                    PruneNode(node, ref tree, position, leaveParent, keepNodeNames, storeAttributeName, matchLeavesOnly, mode);
                }
                else
                {
                    node.Parent = null;
                    node.Length = (1 - position) * node.Length;
                    tree = node;
                }
            }
            else if (mode == 1)
            {
                List<TreeNode> nodes = tree.GetChildrenRecursive();

                string attributeName = (string)parameterValues["Attribute:"];

                string attrType = (string)parameterValues["Attribute type:"];

                string attrValue = (string)parameterValues["Value:"];

                double numberNeedle = attrType == "Number" ? double.Parse(attrValue) : -1;

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

                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    bool matched = false;

                    if (nodes[i].Children.Count == 0 || !matchLeavesOnly)
                    {
                        if (nodes[i].Attributes.TryGetValue(attributeName, out object attributeValue))
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

                        if ((action == 0 && matched) || (action == 1 && !matched))
                        {
                            PruneNode(nodes[i], ref tree, position, leaveParent, keepNodeNames, storeAttributeName, matchLeavesOnly, mode);
                        }
                    }
                }
            }

        }

        private static void PruneNode(TreeNode node, ref TreeNode tree, double position, bool leaveParent, bool keepNodeNames, string storeAttributeName, bool matchLeavesOnly, int mode)
        {
            if (node == tree || (node.Parent == tree && tree.Children.Count < 3 && (from el in tree.Children where el != node select el).First().Children.Count == 0 && position == 0))
            {
                if (matchLeavesOnly || mode == 0)
                {
                    if (node == tree)
                    {
                        throw new Exception("Cannot prune the root node!");
                    }
                    else
                    {
                        throw new Exception("Cannot remove all nodes from the tree!");
                    }
                }
                else
                {
                    throw new Exception("Cannot remove all nodes from the tree!\nNote that the attribute match also matches internal nodes. If you only wish to prune some leaves, select the \"Match leaves only\" checkbox!");
                }
            }

            string underlyingNodes = "";

            if (keepNodeNames)
            {
                List<TreeNode> descendants = node.GetChildrenRecursive();

                for (int i = descendants.Count - 1; i >= 0; i--)
                {
                    string name = descendants[i].Name;
                    string underlying = null;

                    if (descendants[i].Attributes.TryGetValue(storeAttributeName, out object storedObject) && storedObject != null)
                    {
                        underlying = storedObject as string;
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        if (underlyingNodes != "")
                        {
                            underlyingNodes += ",";
                        }

                        underlyingNodes += name;
                    }

                    if (!string.IsNullOrEmpty(underlying) && underlying != name)
                    {
                        if (underlyingNodes != "")
                        {
                            underlyingNodes += ",";
                        }

                        underlyingNodes += underlying;
                    }
                }
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

                            if (index >= 0)
                            {
                                parent.Parent.Children[index] = otherChild;
                                otherChild.Length += parent.Length;
                                otherChild.Parent = parent.Parent;

                                if (keepNodeNames)
                                {
                                    if (!string.IsNullOrEmpty(underlyingNodes))
                                    {
                                        if (parent.Parent.Attributes.TryGetValue(storeAttributeName, out object storedNames) && storedNames is string storedString)
                                        {
                                            parent.Parent.Attributes[storeAttributeName] = storedString + "," + underlyingNodes;
                                        }
                                        else
                                        {
                                            parent.Parent.Attributes[storeAttributeName] = underlyingNodes;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (parent.Length > 0)
                            {
                                otherChild.Length += parent.Length;
                            }

                            foreach (KeyValuePair<string, object> kvp in parent.Attributes)
                            {
                                if (Guid.TryParse(kvp.Key, out _) && !otherChild.Attributes.ContainsKey(kvp.Key))
                                {
                                    otherChild.Attributes[kvp.Key] = kvp.Value;
                                }
                            }

                            otherChild.Parent = null;
                            tree = otherChild;
                        }
                    }
                }
                else
                {
                    if (keepNodeNames)
                    {
                        if (!string.IsNullOrEmpty(underlyingNodes))
                        {
                            if (node.Parent.Attributes.TryGetValue(storeAttributeName, out object storedNames) && storedNames is string storedString)
                            {
                                node.Parent.Attributes[storeAttributeName] = storedString + "," + underlyingNodes;
                            }
                            else
                            {
                                node.Parent.Attributes[storeAttributeName] = underlyingNodes;
                            }
                        }
                    }
                }
            }
            else
            {
                if (keepNodeNames)
                {
                    if (!string.IsNullOrEmpty(underlyingNodes))
                    {
                        node.Attributes[storeAttributeName] = underlyingNodes;

                        if (string.IsNullOrEmpty(node.Name))
                        {
                            node.Name = underlyingNodes;
                        }
                    }
                }


                node.Children.Clear();
                node.Length *= position;
            }
        }
    }
}
