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
using VectSharp;
using System.Runtime.InteropServices;

namespace Labels
{
    /// <summary>
    /// This module is used to draw labels showing the value of an attribute. The labels can be anchored based on the position of nodes and branches.
    /// </summary>

    /// <description>
    /// ## Further information
    /// This module can be used to draw labels on the tree with a high degree of customisability.
    /// For example, labels can be used to show taxon names on the tips of the tree, branch lengths on the branches and support values at internal nodes.
    /// The labels can be anchored in multiple ways to obtain different effects.
    /// 
    /// A limitation is that all the labels drawn by an instance of this module must use the same font, anchor and shift from the anchor point;
    /// thus, if different nodes require different values for these properties, a different module needs to be added for each of them.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Labels";
        public const string HelpText = "Draws labels on nodes, tips or branches.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.3.0");
        public const string Id = "ac496677-2650-4d92-8646-0812918bab03";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACBSURBVDhP7ZBLDoAgDESL8XRuvAt7o8Q9d3Hj9bCtA+EX4wF4SSklYWZSGrRYawOuv5jQFf585P0PhQCze++N9HcsWbYrZKUmSaB27aW4z9XE4lFNZjlAdCfp2EUSgWOTTD/ArRfbsZiKSGw4K3GOCZJ7Tp2iR73ELxyWp4W3ARE9ib495aTh5ykAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAKRSURBVEhL7VRNaBNBFJ7ZpG1QCZT24B+iJwN6EMGe9KAHTWKRoJQWURQPAYWQJiEY2ybLqjchJv6heBH8wYPaGEqseFBUkIqIJ1FBvBQqUjwEFGrcGb83mSxNsgr1Ikg/2H3fe/v2ffPezixbxP8Prm0bEonEac55pxDidqFQeK3DC4ahbROGh4d3ovgoaBr2SD36d3AVQNFBTQnz+YLRJmCapheGin7F9QJivalUai89+xMimXs9/WZ5iXYdtAlUq9UhFF0qpbTgmhQDHyLbiqBZWR0aK50LZUuff3R4ZoWQ38JjpS/B0ftHdYrriNRI/H7/hXw+/0hFGBvIZDLdmjsw7FoOiwkzya8KLg9IxmLYNjOGwS6Fs6XtlEPjcJBOp1fatt2PFb+zLEtQDPwaihyu1WokfJliDQiPLExakah2FYLZ8pTB5EtQEnjc1AG2pFq9YRhnVQAAP0PWbUwo/lZTB8Lo/EgW3awj23QOksnkFEwfRtMUx5mYRRc96C5QLBbf67ACPuwa25aH8MJ6yeQqhJYjNwB+48GpyEFnRPF4fBNMH3EIYQHtQDfU4cm6x1hoZHwfPuwdxuUnyXgJbS5D1lM8ynHJ1SIdAa/XO4gxEB3H9YFIC47jahJgHm6h6LTHY2ycsPZ8pxA66oVorjEbR0DPeA7jcd3zGNMOtL4Fp3wbfh3PgrFKF2e1DSj0vFGc8JOxzfUPK5WE4vRrgFkLkevk/wa36AYRtREmz4fnkP8GdGs4Wz4WypZDOAMJQ8iH9LwBJYCXIsqrj8cVPp/vpqYD2mLxYj9EnmC1FzmTFQROSC5oEjP1jHmIRqNtx7wVsVisyy1v18jdFZh9QLtsd2aiO3rlVYd2F/FPwdgvT0DmZW4lR60AAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAOZSURBVFhH7ZVPSFRBHMdn3luDKIQMizAxOpR1iDSEtExKIl2tXUrKWx26VGi7igfJZRHNqFxXFIrqEIFUSqIruorRsfTQH6KoQx3KiLKiQIRydd/0/c3Oe+y2u3rZW37g+f3N7Mz8fu87b0a2zDL/PVxpQvr6+vSJiQk/53y1EGLG7/e71E8pQ1eakOzs7JNI3oYwD7obvJ6cnHwb+TU1aEqTcUKpBEXEtFNB0i2or6/Pge0fVPMxnj0UoG8ttuInxakgqQNIZL0t3vy8ClPugk1pIsxEPT6f71ldXZ1qyv7rkTA5lZ6hwrAwjqPgEibYGnj9C6/10TD4+NhFhzU/oQO1tbUFkHyK4cQVpTdJQUlDQ8NWFSek3DPYbzDxBMnp1OQh+SapjDs1jV0r9wRoSyUJC9A0zbIZ+/2KVNd1r+wA4XC4WoVxHPT2r+OMH0X4Ah+YW4RFgabxTDy74EQHjUF/kd0zdJbipQroUsra29u/QqYjrdjTEc3D5mPfIM5giyN/pMXROdrmfDrcfOQHnufBVke9EGxADhTiAElcAW632w7ZSDEsvEFqgm3wq3Cby+UqVnEcSB5QYTycvVG6mSSRA6a9YXx8kcEKbMdlFRJJt2ExOBOzpIKxDNKYU4C3XwmR9uJtLfujQf8EnClU23Qu0htPmTeYrhmhUwj3I1uWYDwLszNRQpocIIS8g2IuIhw1mnCbYiQSSEShSUyDwJAquNKvmhYVTYPVWP4upsg5gol3CD4hXMEEz8BK2zF3arTVmROzBei0bEVymhz9JCJuG+wXBopQ+T2ZXIhOpukbRlucW4ItzlI8xYyLUTVUYhVAVy9yHlLNqyhm5yLPMA3C+Cpsm9xLE8G1EqlMTAdbne5gcyWdnmhC8q+y1/oGsKh1tBYWFnxdXV3mkYsDW9UDqaQY69C86JsRFw76BZuSrX/AMdxLqU1LLQdQgLQT+mix5ERHR0cv5A/F5rwo5MWFygrKvIEdMlbYPYO3UHDM8ZUFwP4C/CArx4IPSJcC4+6TYt4+OJIrO4Gu8zuw/zPFmsFeljcFhnE190JnMPo0JkauYXUKZAFYrJQUzBmGEbmplgCJZQEKcz7DjTeFVZ2weoTasLsCVzP+KbFZFHbG4GG3HKiwvu6ampp0m802j2P1W3UtCZxbFQqF9O7ubrxdPGWNwUzdNpc7r2lfxpsd71U3O9wYWD/P0oyxS/bvqmuZZf5bGPsL0pFd1YnyZ4MAAAAASUVORK5CYII=";

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
            return new List<(string, string)>()
            {
                /// <param name="Show on:">
                /// This parameter determines on which nodes the label is shown. If the value is `Leaves`, the label is only shown for terminal nodes (nodes with no child nodes).
                /// If the value is `Internal nodes` the label is shown only for internal nodes (nodes which have at least one child). If the value is `All nodes`, labels are shown for both leaves and internal nodes.
                /// </param>
                ( "Show on:", "ComboBox:0[\"Leaves\",\"Internal nodes\",\"All nodes\"]" ),

                /// <param name="Exclude cartoon nodes">
                /// This parameter determines whether labels are shown for nodes which have been "cartooned" or collapsed. If the check box is checked, labels are not shown for nodes that have been "cartooned".
                /// </param>
                ( "Exclude cartoon nodes", "CheckBox:true" ),

                ( "Position", "Group:3"),

                //( "Anchor:", "ComboBox:0[\"Node\",\"Mid-branch (rectangular)\",\"Mid-branch (radial)\",\"Mid-branch (circular)\",\"Centre of leaves\"]" ),

                /// <param name="Anchor:">
                /// This parameter determines the anchor for the labels. If the value is `Node`, the mid-left of each label is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the mid-centre of the label is aligned with the midpoint of the branch connecting the node to its parent.
                /// If the value is `Centre of leaves` or `Origin`, the alignment depends on the value of the [Branch reference](#branch-reference):
                /// 
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | Branch reference                   | Centre of leaves                                                       | Origin                                                                 |
                /// +====================================+========================================================================+========================================================================+
                /// | Rectangular                        | The smallest rectangle containing all the leaves that descend from the | A point corresponding to the projection of the node on a line          |
                /// |                                    | current node is computed. The anchor corresponds to the centre of this | perpedicular to the direction in which the tree expands and passing    |
                /// |                                    | rectangle.                                                             | through the root node. Usually (i.e. if the tree is horizontal), this  |
                /// |                                    |                                                                        | means a point with the same horizontal coordinate as the root node and |
                /// |                                    |                                                                        | the same vertical coordinate as the current node.                      |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | Radial                             | The smallest rectangle containing all the leaves that descend from the | The root node.                                                         |
                /// |                                    | current node is computed. The anchor corresponds to the centre of this |                                                                        |
                /// |                                    | rectangle.                                                             |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | Circular                           | The centre of leaves is computed using polar coordinates: the minimum  | The root node.                                                         |
                /// |                                    | and maximum distance of the leaves that descend from the current node  |                                                                        |
                /// |                                    | are computed, as well as the minimum and maximum angle. The anchor has |                                                                        |
                /// |                                    | a distance corresponding to the average of the minimum and maximum     |                                                                        |
                /// |                                    | distance, and an angle corresponding to the average of the maximum and |                                                                        |
                /// |                                    | minimum angle.                                                         |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// </param>
                ( "Anchor:", "ComboBox:0[\"Node\",\"Mid-branch\",\"Centre of leaves\",\"Origin\"]" ),

                /// <param name="Position:">
                /// This parameter determines how shifted from the anchor point the label is. The `X` coordinate corresponds to the line determined by the [Orientation](#orientation) with respect to the [Reference](#reference);
                /// the `Y` coordinate corresponds to the line perpendicular to this.
                /// </param>
                ( "Position:", "Point:[5,0]" ),

                /// <param name="Alignment">
                /// This parameter determines the alignment of the labels with respect ot the anchor point. If the selected value is `Default`, the label extends outwards when the selected [Anchor](#anchor) is `Node`, `Centre of leaves`
                /// or `Origin`, and is centered on the anchor when it is `Mid-branch`.
                /// </param>
                ( "Alignment:", "ComboBox:0[\"Default\",\"Outwards\",\"Center\",\"Inwards\"]" ),

                ( "Orientation", "Group:3"),

                /// <param name="Orientation:">
                /// This parameter determines the orientation of the label with respect to the [Reference](#reference), in degrees. If this is `0°`, the label is parallel to the reference (e.g. the branch), if it is `90°` it is perpendicular to the branch and so on.
                /// </param>
                ( "Orientation:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
                
                //( "Reference:", "ComboBox:1[\"Horizontal\",\"Branch (rectangular)\",\"Branch (radial)\",\"Branch (circular)\"]" ),
                
                /// <param name="Reference:">
                /// This parameter (along with the [Orientation](#orientation)) determines the reference for the direction along which the text of the label flows.
                /// If this is `Horizontal`, the labels are all drawn in the same direction, regardless of the orientation of the branch to which they refer.
                /// If it is `Branch`, each label is drawn along the direction of the branch connecting the node to its parent, assuming that the branch is drawn in the style determined by the [Branch reference](#branch-reference).
                /// </param>
                ( "Reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),

                /// <param name="Branch reference:">
                /// This parameter determines the algorithm used to compute branch orientations. For best results, the value of this parameter should correspond to the coordinates module actually used.
                /// </param>
                ( "Branch reference:", "ComboBox:0[\"Rectangular\",\"Radial\",\"Circular\"]" ),

                ( "Appearance", "Group:4"),

                /// <param name="Font:">
                /// This parameter determines the font (font family and size) used to draw the labels.
                /// </param>
                ( "Font:", "Font:[\"Helvetica\",\"10\"]" ),

                /// <param name="Auto colour by node">
                /// If this check box is checked, the colour of each label is determined algorithmically in a pseudo-random way designed to achieve an aestethically pleasing distribution of colours, while being reproducible if the same tree is rendered multiple times.
                /// </param>
                ( "Auto colour by node", "CheckBox:false" ),

                /// <param name="Opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto colour by node](#auto-colour-by-node) option is enabled.
                /// </param>
                ( "Opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),

                /// <param name="Text colour:">
                /// This parameter determines the colour used to draw each label (if the [Auto colour by node](#auto-colour-by-node) option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Text colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),

                ( "Background", "Group:8"),
                
                /// <param name="Auto background by node">
                /// If this check box is checked, the background of each label is determined algorithmically in a pseudo-random way designed to achieve an aestethically pleasing distribution of colours, while being reproducible if the same tree is rendered multiple times.
                /// </param>
                ( "Auto background by node", "CheckBox:false" ),

                /// <param name="Background opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto background by node](#auto-background-by-node) option is enabled.
                /// </param>
                ( "Background opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Background colour:">
                /// This parameter determines the colour used to draw the background of the label (if the [Auto background by node](#auto-background-by-node) option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used. The default attribute used to determine the colour is `BackgroundColour`.
                /// </param>
                ( "Background colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"BackgroundColour\",\"String\",\"0\",\"0\",\"0\",\"0\",\"true\"]" ),

                /// <param name="Margin:">
                /// This parameter determines the margin between the label and the background (if the [Fixed size](#fixed-size) option is disabled).
                /// </param>
                ( "Margin:", "Point:[5,2]" ),

                /// <param name="Fixed width">
                /// If this check box is checked, the size of the background of the labels does not depend on the text of the label. Otherwise, the size of the background depends on the size of the text contained in the label.
                /// </param>
                ( "Fixed size", "CheckBox:false" ),
                
                /// <param name="Width:">
                /// This parameter determines the width of the label background (if the [Fixed size](#fixed-size) option is disabled).
                /// </param>
                ( "Width:", "NumericUpDownByNode:50[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"LabelWidth\",\"Number\",\"true\"]" ),
                
                /// <param name="Height:">
                /// This parameter determines the height of the label background (if the [Fixed size](#fixed-size) option is disabled).
                /// </param>
                ( "Height:", "NumericUpDownByNode:14[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"LabelHeight\",\"Number\",\"true\"]" ),

                ( "Border", "Group:6"),
                
                /// <param name="Border thickness:">
                /// This parameter determines the thickness of the border around the label.
                /// </param>
                ( "Border thickness:", "NumericUpDownByNode:0[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"BorderThickness\",\"Number\",\"true\"]" ),
                
                /// <param name="Auto border by node">
                /// If this check box is checked, the colour for the border of each label is determined algorithmically in a pseudo-random way designed to achieve an aestethically pleasing distribution of colours, while being reproducible if the same tree is rendered multiple times.
                /// </param>
                ( "Auto border by node", "CheckBox:true" ),

                /// <param name="Border opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto border by node](#auto-border-by-node) option is enabled.
                /// </param>
                ( "Border opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Border colour:">
                /// This parameter determines the colour used to draw the border of the label (if the [Auto border by node](#auto-border-by-node) option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used. The default attribute used to determine the colour is `BorderColour`.
                /// </param>
                ( "Border colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"BorderColour\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),

                /// <param name="Border style:">
                /// The line dash to use when drawing the borders.
                /// </param>
                ( "Border style:", "Dash:[0,0,0]"),
                
                /// <param name="Line join:">
                /// The line join to use at the corners of the border.
                /// </param>
                ( "Line join:", "ComboBox:1[\"Miter\",\"Round\",\"Bevel\"]" ),

                ( "Attribute", "Group:3" ),

                /// <param name="Attribute:">
                /// This parameter specifies the attribute used to determine the text of the labels. By default the `Name` of each node is drawn.
                /// </param>
                ( "Attribute:", "AttributeSelector:Name" ),

                /// <param name="Attribute type:">
                /// This parameter specifies the type of the attribute used to determine the text of the labels. By default this is `String`. If the type chosen here does not correspond to the actual type of the attribute
                /// (e.g. `Number` is chosen for the `Name` attribute, or `String` is chosen for the `Length` attribute), no label is drawn. If the attribute has values with different types for different nodes, the label is only shown on nodes
                /// whose attribute type corresponds to the one chosen here.
                /// </param>
                ( "Attribute type:", "AttributeType:String"),

                /// <param name="Attribute format...">
                /// This parameter determines how the value of the selected attribute is used to determine the text of the label. By default, if the [Attribute type](#attribute-type) is `String` the text of the label corresponds to the value of the attribute,
                /// while if the [Attribute type](#attribute-type) is `Number` the text of the label corresponds to the number rounded to 2 significant digits.
                /// </param>
                ( "Attribute format...", "Formatter:[\"Attribute type:\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConverters[0]) + ",\"true\"]"),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>()
            {

            };

            parametersToChange = new Dictionary<string, object>() { };

            if ((int)previousParameterValues["Anchor:"] != (int)currentParameterValues["Anchor:"] && currentParameterValues["Position:"] == previousParameterValues["Position:"])
            {
                parametersToChange.Add("Position:", new Point(0, 0));
            }

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Attribute type:"] == (string)currentParameterValues["Attribute type:"] && (string)currentParameterValues["Attribute type:"] != attrType)
                {
                    parametersToChange.Add("Attribute type:", attrType);

                    if (previousParameterValues["Attribute format..."] == currentParameterValues["Attribute format..."])
                    {
                        if (attrType == "String")
                        {
                            parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[0]) { Parameters = new object[] { Modules.DefaultAttributeConverters[0], true } });
                        }
                        else if (attrType == "Number")
                        {
                            parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } });
                        }
                    }
                }
            }

            if ((string)previousParameterValues["Attribute type:"] != (string)currentParameterValues["Attribute type:"])
            {
                string attrType = (string)currentParameterValues["Attribute type:"];
                
                if (previousParameterValues["Attribute format..."] == currentParameterValues["Attribute format..."])
                {
                    if (attrType == "String")
                    {
                        parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[0]) { Parameters = new object[] { Modules.DefaultAttributeConverters[0], true } });
                    }
                    else if (attrType == "Number")
                    {
                        parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } });
                    }
                }
            }

            if ((bool)currentParameterValues["Auto colour by node"])
            {
                controlStatus.Add("Opacity:", ControlStatus.Enabled);
                controlStatus.Add("Text colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Opacity:", ControlStatus.Hidden);
                controlStatus.Add("Text colour:", ControlStatus.Enabled);
            }


            if ((bool)currentParameterValues["Auto background by node"])
            {
                controlStatus.Add("Background opacity:", ControlStatus.Enabled);
                controlStatus.Add("Background colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Background opacity:", ControlStatus.Hidden);
                controlStatus.Add("Background colour:", ControlStatus.Enabled);
            }


            if ((bool)currentParameterValues["Fixed size"])
            {
                controlStatus.Add("Width:", ControlStatus.Enabled);
                controlStatus.Add("Height:", ControlStatus.Enabled);
            }
            else
            {
                controlStatus.Add("Width:", ControlStatus.Hidden);
                controlStatus.Add("Height:", ControlStatus.Hidden);
            }

            if ((bool)currentParameterValues["Auto border by node"])
            {
                controlStatus.Add("Border opacity:", ControlStatus.Enabled);
                controlStatus.Add("Border colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Border opacity:", ControlStatus.Hidden);
                controlStatus.Add("Border colour:", ControlStatus.Enabled);
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            CheckCoordinates(tree, parameterValues, coordinates);
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            string attribute = (string)parameterValues["Attribute:"];

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            bool anyMaxMin = false;

            void updateMaxMin(Point pt)
            {
                anyMaxMin = true;
                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static Point rotatePoint(Point pt, double angle)
            {
                return new Point(pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle), pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle));
            }

            static Point sumPoint(Point pt1, Point pt2)
            {
                return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
            }

            Font fnt = (Font)parameterValues["Font:"];

            int showOn = (int)parameterValues["Show on:"];
            int anchor = (int)parameterValues["Anchor:"];

            int alignment = (int)parameterValues["Alignment:"];

            Point delta = (Point)parameterValues["Position:"];

            ColourFormatterOptions Fill = (ColourFormatterOptions)parameterValues["Text colour:"];
            Colour defaultFill = Fill.DefaultColour;
            Func<object, Colour?> fillFormatter = Fill.Formatter;


            bool autoColour = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];

            bool backgroundAutoColour = (bool)parameterValues["Auto background by node"];
            double backgroundOpacity = (double)parameterValues["Background opacity:"];

            ColourFormatterOptions BackgroundColour = (ColourFormatterOptions)parameterValues["Background colour:"];
            Colour defaultBackgroundColour = BackgroundColour.DefaultColour;
            Func<object, Colour?> backgroundFormatter = BackgroundColour.Formatter;

            bool fixedSize = (bool)parameterValues["Fixed size"];
            Point margin = (Point)parameterValues["Margin:"];

            NumberFormatterOptions BackgroundWidth = (NumberFormatterOptions)parameterValues["Width:"];
            double defaultBackgroundWidth = BackgroundWidth.DefaultValue;
            Func<object, double?> backgroundWidthFormatter = BackgroundWidth.Formatter;

            NumberFormatterOptions BackgroundHeight = (NumberFormatterOptions)parameterValues["Height:"];
            double defaultBackgroundHeight = BackgroundHeight.DefaultValue;
            Func<object, double?> backgroundHeightFormatter = BackgroundHeight.Formatter;

            NumberFormatterOptions BorderThickness = (NumberFormatterOptions)parameterValues["Border thickness:"];
            double defaultBorderThickness = BorderThickness.DefaultValue;
            Func<object, double?> borderThicknessFormatter = BorderThickness.Formatter;

            bool borderAutoColour = (bool)parameterValues["Auto border by node"];
            double borderOpacity = (double)parameterValues["Border opacity:"];

            ColourFormatterOptions BorderColour = (ColourFormatterOptions)parameterValues["Border colour:"];
            Colour defaultBorderColour = BorderColour.DefaultColour;
            Func<object, Colour?> borderColourFormatter = BorderColour.Formatter;

            LineJoins join = (LineJoins)(int)parameterValues["Line join:"];
            LineDash dash = (LineDash)parameterValues["Border style:"];

            double orientation = (double)parameterValues["Orientation:"] * Math.PI / 180;
            int orientationReference = (int)parameterValues["Reference:"];

            int branchReference = (int)parameterValues["Branch reference:"];

            if (anchor == 2)
            {
                anchor = 4;
            }

            if (anchor == 3)
            {
                anchor = 5;
            }

            if (anchor == 1)
            {
                anchor += branchReference;
            }

            if (orientationReference == 1)
            {
                orientationReference += branchReference;
            }

            bool excludeCartoonNodes = (bool)parameterValues["Exclude cartoon nodes"];

            Func<object, string> formatter = ((FormatterOptions)parameterValues["Attribute format..."]).Formatter;

            Point rootPoint = coordinates[Modules.RootNodeId];
            coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out Point circularCenter);

            foreach (TreeNode node in nodes)
            {
                if ((showOn == 2 || (showOn == 0 && node.Children.Count == 0) || (showOn == 1 && node.Children.Count > 0)) && (!excludeCartoonNodes || !node.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe")))
                {
                    string attributeValue = "";

                    if (node.Attributes.TryGetValue(attribute, out object attributeObject) && attributeObject != null)
                    {
                        attributeValue = formatter(attributeObject);
                    }

                    if (!string.IsNullOrEmpty(attributeValue))
                    {
                        Colour fillColour = defaultFill;

                        if (!autoColour)
                        {
                            if (node.Attributes.TryGetValue(Fill.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                            {
                                fillColour = fillFormatter(fillAttributeObject) ?? defaultFill;
                            }
                        }
                        else
                        {
                            fillColour = Modules.DefaultColours[Math.Abs(string.Join(",", node.GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                        }

                        Colour backgroundColour = defaultBackgroundColour;

                        if (!backgroundAutoColour)
                        {
                            if (node.Attributes.TryGetValue(BackgroundColour.AttributeName, out object backgroundAttributeObject) && backgroundAttributeObject != null)
                            {
                                backgroundColour = backgroundFormatter(backgroundAttributeObject) ?? defaultBackgroundColour;
                            }
                        }
                        else
                        {
                            backgroundColour = Modules.DefaultColours[Math.Abs(string.Join(",", node.GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(backgroundOpacity);
                        }

                        Colour borderColour = defaultBorderColour;

                        if (!borderAutoColour)
                        {
                            if (node.Attributes.TryGetValue(BorderColour.AttributeName, out object borderColourAttributeObject) && borderColourAttributeObject != null)
                            {
                                borderColour = backgroundFormatter(borderColourAttributeObject) ?? defaultBorderColour;
                            }
                        }
                        else
                        {
                            borderColour = Modules.DefaultColours[Math.Abs(string.Join(",", node.GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(borderOpacity);
                        }

                        IEnumerable<FormattedText> formattedText;

                        if (fnt.FontFamily.IsStandardFamily)
                        {
                            formattedText = FormattedText.Format(attributeValue, (FontFamily.StandardFontFamilies)Array.IndexOf(FontFamily.StandardFamilies, fnt.FontFamily.FileName), fnt.FontSize);
                        }
                        else
                        {
                            formattedText = FormattedText.Format(attributeValue, fnt, fnt, fnt, fnt);
                        }

                        Font.DetailedFontMetrics textSize = formattedText.Measure();

                        double backgroundWidth = defaultBackgroundWidth;
                        double backgroundHeight = defaultBackgroundHeight;

                        if (fixedSize)
                        {
                            if (node.Attributes.TryGetValue(BackgroundWidth.AttributeName, out object backgroundWidthAttributeObject) && backgroundWidthAttributeObject != null)
                            {
                                backgroundWidth = backgroundWidthFormatter(backgroundWidthAttributeObject) ?? defaultBackgroundWidth;
                            }

                            if (node.Attributes.TryGetValue(BackgroundHeight.AttributeName, out object backgroundHeightAttributeObject) && backgroundHeightAttributeObject != null)
                            {
                                backgroundHeight = backgroundHeightFormatter(backgroundHeightAttributeObject) ?? defaultBackgroundHeight;
                            }
                        }
                        else
                        {
                            backgroundWidth = textSize.Width + margin.X * 2;
                            backgroundHeight = textSize.Height + margin.Y * 2;
                        }

                        double borderThickness = defaultBorderThickness;

                        if (node.Attributes.TryGetValue(BorderThickness.AttributeName, out object borderThicknessAttributeObject) && borderThicknessAttributeObject != null)
                        {
                            borderThickness = backgroundWidthFormatter(borderThicknessAttributeObject) ?? defaultBorderThickness;
                        }

                        Point point = coordinates[node.Id];

                        Point anglePoint = point;

                        if ((anchor == 1 || orientationReference == 1) && node.Parent != null)
                        {
                            Point parentPoint = coordinates[node.Parent.Id];

                            TreeNode parent = node.Parent;

                            while (point.Y - parentPoint.Y == 0 && point.X - parentPoint.X == 0 && parent.Parent != null)
                            {
                                parent = parent.Parent;
                                parentPoint = coordinates[parent.Id];
                            }

                            Point pA = coordinates[parent.Children[0].Id];
                            Point pB = coordinates[parent.Children[^1].Id];

                            double numerator = pA.Y + pB.Y - 2 * parentPoint.Y;
                            double denominator = pA.X + pB.X - 2 * parentPoint.X;

                            if (Math.Abs(numerator) > 1e-5 && Math.Abs(denominator) > 1e-5)
                            {
                                double m = numerator / denominator;

                                double x = (m * (parentPoint.Y - point.Y + m * point.X) + parentPoint.X) / (m * m + 1);
                                double y = parentPoint.Y - (x - parentPoint.X) / m;

                                anglePoint = new Point(x, y);
                            }
                            else if (Math.Abs(numerator) > 1e-5)
                            {
                                anglePoint = new Point(point.X, parentPoint.Y);
                            }
                            else if (Math.Abs(denominator) > 1e-5)
                            {
                                anglePoint = new Point(parentPoint.X, point.Y);
                            }
                            else
                            {
                                anglePoint = point;
                            }
                        }

                        if (anchor > 0 && node.Parent != null)
                        {
                            Point parentPoint = coordinates[node.Parent.Id];

                            if (anchor == 1)
                            {
                                point = new Point((point.X + anglePoint.X) * 0.5, (point.Y + anglePoint.Y) * 0.5);
                            }
                            else if (anchor == 2)
                            {
                                point = new Point((point.X + parentPoint.X) * 0.5, (point.Y + parentPoint.Y) * 0.5);
                            }
                            else if (anchor == 3)
                            {
                                double parentR = parentPoint.Modulus();
                                double myR = point.Modulus();
                                double myTheta = Math.Atan2(point.Y, point.X);

                                point = new Point((parentR + myR) * 0.5 * Math.Cos(myTheta), (parentR + myR) * 0.5 * Math.Sin(myTheta));
                            }
                        }
                        else if (anchor > 0 && node.Parent == null)
                        {
                            Point parentPoint = coordinates[Modules.RootNodeId];
                            point = new Point((point.X + parentPoint.X) * 0.5, (point.Y + parentPoint.Y) * 0.5);
                        }

                        if (anchor == 4)
                        {
                            if (branchReference == 2)
                            {
                                double minR = double.MaxValue;
                                double maxR = double.MinValue;
                                double minTheta = double.MaxValue;
                                double maxTheta = double.MinValue;


                                foreach (TreeNode leaf in node.GetLeaves())
                                {
                                    Point pt = coordinates[leaf.Id];

                                    double r = pt.Modulus();
                                    double theta = Math.Atan2(pt.Y, pt.X);

                                    minR = Math.Min(minR, r);
                                    maxR = Math.Max(maxR, r);
                                    minTheta = Math.Min(minTheta, theta);
                                    maxTheta = Math.Max(maxTheta, theta);
                                }

                                point = new Point((minR + maxR) * 0.5 * Math.Cos((minTheta + maxTheta) * 0.5), (minR + maxR) * 0.5 * Math.Sin((minTheta + maxTheta) * 0.5));
                            }
                            else
                            {
                                double minXChild = double.MaxValue;
                                double maxXChild = double.MinValue;
                                double minYChild = double.MaxValue;
                                double maxYChild = double.MinValue;


                                foreach (TreeNode leaf in node.GetLeaves())
                                {
                                    Point pt = coordinates[leaf.Id];

                                    minXChild = Math.Min(minXChild, pt.X);
                                    maxXChild = Math.Max(maxXChild, pt.X);
                                    minYChild = Math.Min(minYChild, pt.Y);
                                    maxYChild = Math.Max(maxYChild, pt.Y);
                                }

                                point = new Point((minXChild + maxXChild) * 0.5, (minYChild + maxYChild) * 0.5);
                            }
                        }



                        double rotationAngle = 0;

                        if (orientationReference > 0 && node.Parent != null)
                        {
                            if (orientationReference == 1)
                            {
                                double angle = Math.Atan2(point.Y - anglePoint.Y, point.X - anglePoint.X);
                                rotationAngle += angle;
                            }
                            else if (orientationReference == 2)
                            {
                                Point parentPoint = coordinates[node.Parent.Id];

                                TreeNode parent = node.Parent;

                                while (point.Y - parentPoint.Y == 0 && point.X - parentPoint.X == 0 && parent.Parent != null)
                                {
                                    parent = parent.Parent;
                                    parentPoint = coordinates[parent.Id];
                                }

                                double angle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                                rotationAngle += angle;
                            }
                            else if (orientationReference == 3)
                            {
                                Point pt = coordinates[node.Id];

                                rotationAngle += Math.Atan2(pt.Y, pt.X);
                            }
                        }
                        else if (orientationReference > 0 && node.Parent == null)
                        {
                            if (orientationReference != 3)
                            {
                                Point parentPoint = coordinates[Modules.RootNodeId];
                                double angle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                                rotationAngle += angle;
                            }
                            else
                            {
                                Point pt = coordinates[node.Id];

                                rotationAngle += Math.Atan2(pt.Y, pt.X);
                            }
                        }

                        while (Math.Abs(rotationAngle) > Math.PI)
                        {
                            rotationAngle -= 2 * Math.PI * Math.Sign(rotationAngle);
                        }

                        if (anchor == 5)
                        {
                            double referenceAngle = rotationAngle;

                            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out Point coordinateReference))
                            {
                                referenceAngle = Math.Atan2(coordinateReference.Y, coordinateReference.X);
                            }

                            if (branchReference == 0)
                            {
                                point = coordinates[node.Id];

                                Point branchVector = new Point(Math.Cos(referenceAngle), Math.Sin(referenceAngle));

                                double d = (rootPoint.X - point.X) * branchVector.X + (rootPoint.Y - point.Y) * branchVector.Y;

                                Point proj = new Point(point.X + d * branchVector.X, point.Y + d * branchVector.Y);

                                point = proj;
                            }
                            else if (branchReference == 1)
                            {
                                point = coordinates[Modules.RootNodeId];
                            }
                            else if (branchReference == 2)
                            {
                                point = circularCenter;
                            }
                        }

                        graphics.Save();

                        graphics.Translate(point.X, point.Y);
                        graphics.Rotate(rotationAngle);

                        graphics.Translate(delta.X, delta.Y);
                        graphics.Rotate(orientation);

                        double totalAngle = rotationAngle + orientation;

                        while (totalAngle > Math.PI)
                        {
                            totalAngle -= 2 * Math.PI;
                        }

                        while (totalAngle < -Math.PI)
                        {
                            totalAngle += 2 * Math.PI;
                        }

                        if (anchor == 0 || anchor == 4 || anchor == 5)
                        {
                            if (backgroundColour.A > 0)
                            {
                                if (alignment == 0 || alignment == 1)
                                {
                                    graphics.FillRectangle(-margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                                else if (alignment == 2)
                                {
                                    graphics.FillRectangle(-backgroundWidth * 0.5, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                                else if (alignment == 3)
                                {
                                    graphics.FillRectangle(-backgroundWidth + margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                            }

                            if (borderColour.A > 0 && borderThickness > 0)
                            {
                                if (alignment == 0 || alignment == 1)
                                {
                                    graphics.StrokeRectangle(-margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                                else if (alignment == 2)
                                {
                                    graphics.StrokeRectangle(-backgroundWidth * 0.5, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                                else if (alignment == 3)
                                {
                                    graphics.StrokeRectangle(-backgroundWidth + margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                            }

                            if (fillColour.A > 0)
                            {
                                if (Math.Abs(totalAngle) < Math.PI / 2)
                                {
                                    if (alignment == 0 || alignment == 1)
                                    {
                                        graphics.FillText(0, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 2)
                                    {
                                        graphics.FillText(-textSize.Width * 0.5, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 3)
                                    {
                                        graphics.FillText(-textSize.Width, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }

                                }
                                else
                                {
                                    graphics.Save();
                                    graphics.Rotate(Math.PI);

                                    if (alignment == 0 || alignment == 1)
                                    {
                                        graphics.FillText(-textSize.Width, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 2)
                                    {
                                        graphics.FillText(-textSize.Width * 0.5, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 3)
                                    {
                                        graphics.FillText(0, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }

                                    graphics.Restore();
                                }
                            }

                            if (alignment == 0 || alignment == 1)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-margin.X - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-margin.X - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + margin.X) + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + margin.X) + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                            else if (alignment == 2)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                            else if (alignment == 3)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + margin.X) - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + margin.X) - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(margin.X + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(margin.X + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                        }
                        else if (anchor == 1 || anchor == 2 || anchor == 3)
                        {
                            if (backgroundColour.A > 0)
                            {
                                if (alignment == 0 || alignment == 2)
                                {
                                    graphics.FillRectangle(-backgroundWidth * 0.5, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                                else if (alignment == 1)
                                {
                                    graphics.FillRectangle(-margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                                else if (alignment == 3)
                                {
                                    graphics.FillRectangle(-backgroundWidth + margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, backgroundColour, tag: node.Id);
                                }
                            }

                            if (borderColour.A > 0 && borderThickness > 0)
                            {
                                if (alignment == 0 || alignment == 2)
                                {
                                    graphics.StrokeRectangle(-backgroundWidth * 0.5, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                                else if (alignment == 1)
                                {
                                    graphics.StrokeRectangle(-margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                                else if (alignment == 3)
                                {
                                    graphics.StrokeRectangle(-backgroundWidth + margin.X, -backgroundHeight * 0.5, backgroundWidth, backgroundHeight, borderColour, borderThickness, lineJoin: join, lineDash: dash, tag: node.Id);
                                }
                            }

                            if (fillColour.A > 0)
                            {
                                if (Math.Abs(totalAngle) < Math.PI / 2)
                                {
                                    if (alignment == 0 || alignment == 2)
                                    {
                                        graphics.FillText(-textSize.Width * 0.5, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 1)
                                    {
                                        graphics.FillText(0, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 3)
                                    {
                                        graphics.FillText(-textSize.Width, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                }
                                else
                                {
                                    graphics.Save();
                                    graphics.Rotate(Math.PI);
                                    if (alignment == 0 || alignment == 2)
                                    {
                                        graphics.FillText(-textSize.Width * 0.5, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 1)
                                    {
                                        graphics.FillText(-textSize.Width, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }
                                    else if (alignment == 3)
                                    {
                                        graphics.FillText(0, 0, formattedText, fillColour, TextBaselines.Middle, tag: node.Id);
                                    }

                                    graphics.Restore();
                                }
                            }

                            if (alignment == 0 || alignment == 2)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + 2 * margin.X) * 0.5 + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                            else if (alignment == 1)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-margin.X - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-margin.X - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + margin.X) + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(Math.Max(backgroundWidth, textSize.Width + margin.X) + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                            else if (alignment == 3)
                            {
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + margin.X) - borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(-Math.Max(backgroundWidth, textSize.Width + margin.X) - borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(margin.X + borderThickness * 0.5, -Math.Max(textSize.Height, backgroundHeight) * 0.5 - borderThickness * 0.5), orientation)), rotationAngle)));
                                updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(margin.X + borderThickness * 0.5, Math.Max(textSize.Height, backgroundHeight) * 0.5 + borderThickness * 0.5), orientation)), rotationAngle)));
                            }
                        }

                        graphics.Restore();
                    }
                }
            }

            if (anyMaxMin)
            {
                return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
            }
            else
            {
                return new Point[] { new Point(), new Point() };
            }
        }

        private const string WrongReferenceMessageId = "";

        private static void CheckCoordinates(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates)
        {
            string message = "";
            string messageId = Id;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out _))
            {
                // Rectangular coordinates

                if ((int)parameterValues["Branch reference:"] != 0)
                {
                    message = "With the current Coordinates module, it is recommended that the _Branch reference_ parameter be set to `Rectangular`.";

                    messageId = WrongReferenceMessageId;
                }
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out _))
            {
                // Circular coordinates

                 if ((int)parameterValues["Branch reference:"] != 2)
                {
                    message = "With the current Coordinates module, it is recommended that the _Branch reference_ parameter be set to `Circular`.";

                    messageId = WrongReferenceMessageId;
                }
            }
            else
            {
                // Radial coordinates

                 if ((int)parameterValues["Branch reference:"] != 1)
                {
                    message = "With the current Coordinates module, it is recommended that the _Branch reference_ parameter be set to `Radial`.";

                    messageId = WrongReferenceMessageId;
                }
            }

            if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
            {
                setWarning(message, messageId);
            }
        }
    }
}
