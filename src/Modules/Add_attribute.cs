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
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace Add_attribute
{
    /// <summary>
    /// This module can be used to add new attributes to a node of the tree. If the attribute is already present,
    /// the effect of this module will be to change its value.
    ///
    /// The module can be added manually, or by selecting a node from the tree plot and using the `+` button to add
    /// a new attribute.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the _Change attribute_ module is that this module lets you choose an
    /// arbitrary name for the new attribute (and, thus, lets you create a new attribute); the _Change attribute_
    /// module, instead, only lets you select an attribute that already exists in the tree (and, thus, only lets you
    /// change the value of existing attributes).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Add attribute";
        public const string HelpText = "Adds or changes the value of an attribute of a node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "afb64d72-971d-4780-8dbb-a7d9248da30b";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;
        
        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEpSURBVDhPlZOxUoNAEIbvVB7BFxB6a1JZUdjkGVLSQmXhmIyFHS2deYY0zkCVShoabWH0AfQdzvv3lvMCh4zfDPNn73b/ZXeCFAtkWbbV8mCif4Ji/aiu65QP3J1x7oShc5qmIooiOquqSuR5TjrgNfAVg7quRVEUEjpwwWqZKwZJkkAU6xQUYy535tv7g3r7+OLoFOTaEeY6P21W4m7/Kt4/v/nE0Pc9/9IsdYZyqgX51NRXDFDkmlDVc67oMQZ4Y+P0V2fEgJIdgwHsYFeWpZ3Jnfn66lK+PK4loEsPdMGvYxeIYpigeNxxjHUemwDqvGBwAkzcnfDxL54dnLMSTdMc4ziWbdvehGEogiDYIsY5JaxXZvOHZkeqmXwL+r+OJFoslOMZhPgBVrklcs7G+GIAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGkSURBVEhLrZUtT8RAEIangMIRIFggDcEA/hRBVIDAg0LWFoXg44LANbg6MIAGBEmbQFCoCsAcoQ38APgPZWc7bbbdj24TnqSZ27m9952daXsO9CQIghMWjsvVP4Pi7CqyLCtswL1j9NtOqsp93wfXdcukBeMUjXSJx3EMURRBkiR8Xe2p1kZs2oLf03bcT1mLFqE4C73bIjJBUaKPuOd5GPgp6LMZFMfjqdqyeXhXXD2OaGUGNaQhd1W+Mj8N57evrN4CVhdmKKsGh9wwsGnL3NSktUnDwCS+dXTfELMxyfMc0jQF/qroqvz9+xcOLl9gZ30JdjeWKVvm1xZnG68b7D2K43PBGBoHKvL29aMcMFe92C/4xUAd1KOiAZ8DbVuunz5oBbwNZ3sDuHn+bOTbVJWHYVgbDDGJxxJRiYkmKtiBBizU4jW6NunaIsIFhBa14a8KcpRO0m6Lo4C2amlsoMFIM6nuoofTbUdXqQ6pAp0JwivuaaBENxP6WqZrBm10M0GT9n3ehfb/QGUiPqHSrahBa4C0TfqKI0YDRDTB2EccAOAP+5oIPGiJgooAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIOSURBVFhHtZa/TgJBEMb38BV8ARNCtLHSAiuiCRSa2KuVj8DZCw8Ana2FidqZWJgAhaG7xgcwJjyG9bnfsnu5m2N3Z/fwlyz754Dvu5m5gURsgeFwOJLT/XoXRmMDTcRBIwNl8X6/LwaDAZYs0jRVc0u9RtBEvExUBDji8/lcLBYLvau/z0RgR70GECj+IcerHL+r1aqDg3a7jakwF5QCbtiN+HQ6vZBjhBn7ckQMbAMROf/Ss4HuFSwDkQV3pGcD3Su8NeASf/78Fod7u3pXBTnvdrvHcmC+k0fn+DytAacBn/jL8keIPK+ZMCK68HpydOjnjQHrY8gJuzFx1euI69N9fcrD+Rhyc67uXEbAFgkX1hSE5jzGBPqETI9aVwzE5jzEBOmQ4+Ix9IUdOUauIQIzlPL1xAIVR5NSRcjNOfAVHq7fnB0k4jHN9dGa20kidcyZEsei5Qs7hRMJD4U4QAqcOY8x4aAiDqytmHOnoSaoOICBMRYoDhRJmf8wQWEVIb4cIhCDqAGVrZd1NhShXlUoDmNMKANUKJCiBnR+otMRS6UTZlm2lD+diErPtErzywZox3t7ehiLy5NaYYVQewq4kSjXQhOsRcTpjtsoQmsf8EUCzGazHO1Vj6hUWA0AlwmscaapdTguzr9kYFNhYvaK0+J8z9SNUJwRMNBIbOPODSwDoGxC01hcCCH+ADILl2HQ8UejAAAAAElFTkSuQmCC";

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
                /// <param name="Node:">
                /// This parameter is used to select the node on which to add/change the attribute. If only a
                /// single node is selected, the attribute is added to that node. If more than one node is
                /// selected, the attribute is applied to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Apply recursively to all children">
                /// If this check box is checked, the attribute is added to all the descendants of the selected
                /// node. Otherwise, it is only applied to the node itself.
                /// </param>
                ( "Apply recursively to all children", "CheckBox:false"),

                ( "Attribute", "Group:3" ),
                
                /// <param name="Attribute:">
                /// The name of the new attribute. If an attribute with the same name is present at the selected
                /// node, its value is changed by this module.
                /// </param>
                ( "Attribute:", "TextBox:" ),
                
                /// <param name="Attribute type:">
                /// The type of the new attribute.
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

                if (!string.IsNullOrEmpty(attrType) && previousParameterValues["Attribute type:"] == currentParameterValues["Attribute type:"])
                {
                    parametersToChange.Add("Attribute type:", attrType);
                }
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
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

            bool applyToChildren = (bool)parameterValues["Apply recursively to all children"];

            if (!applyToChildren)
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
