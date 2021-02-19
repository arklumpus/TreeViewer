using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

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
        public static Version Version = new Version("1.0.0");
        public const string Id = "ac496677-2650-4d92-8646-0812918bab03";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

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

                ( "Position", "Group:2"),

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

                ( "Attribute", "Group:3" ),

                /// <param name="Attribute:">
                /// This parameter specifies the attribute used to determine the text of the labels. By default the `Name` of each node is drawn.
                /// </param>
                ( "Attribute:", "AttributeSelector:Name" ),

                /// <param name="Attribute type:">
                /// This parameter specifies the type of the attribute used to determine the text of the labels. By default this is `String` of each node is drawn. If the type chosen here does not correspond to the actual type of the attribute
                /// (e.g. is `Number` is chosen for the `Name` attribute, or `String` is chosen for the `Length` attribute), no label is drawn. If the attribute has values with different types for different nodes, the label is only shown on nodes
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

            if ((int)previousParameterValues["Anchor:"] != (int)currentParameterValues["Anchor:"])
            {
                parametersToChange.Add("Position:", new Point(0, 0));
            }

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Attribute type:"] == (string)currentParameterValues["Attribute type:"])
                {
                    parametersToChange.Add("Attribute type:", attrType);

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

            if ((string)previousParameterValues["Attribute type:"] != (string)currentParameterValues["Attribute type:"])
            {
                string attrType = (string)currentParameterValues["Attribute type:"];
                if (attrType == "String")
                {
                    parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[0]) { Parameters = new object[] { Modules.DefaultAttributeConverters[0], true } });
                }
                else if (attrType == "Number")
                {
                    parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } });
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

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
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

            Point delta = (Point)parameterValues["Position:"];

            ColourFormatterOptions Fill = (ColourFormatterOptions)parameterValues["Text colour:"];
            Colour defaultFill = Fill.DefaultColour;
            Func<object, Colour?> fillFormatter = Fill.Formatter;


            bool autoColour = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];


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
                            fillColour = Modules.DefaultColours[Math.Abs(node.GetLeafNames().Aggregate((a, b) => a + "," + b).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                        }

                        Point point = coordinates[node.Id];

                        Point anglePoint = point;

                        Size textSize = fnt.MeasureText(attributeValue);

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



                        double rotationAngle = orientation;

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

                        if (anchor == 0 || anchor == 4 || anchor == 5)
                        {
                            if (fillColour.A > 0)
                            {
                                if (Math.Abs(rotationAngle) < Math.PI / 2)
                                {
                                    graphics.FillText(delta.X, delta.Y, attributeValue, fnt, fillColour, TextBaselines.Middle, tag: node.Id);
                                }
                                else
                                {
                                    graphics.Save();
                                    graphics.Rotate(Math.PI);
                                    graphics.FillText(-delta.X - textSize.Width, delta.Y, attributeValue, fnt, fillColour, TextBaselines.Middle, tag: node.Id);
                                    graphics.Restore();
                                }
                            }

                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X, delta.Y - textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X, delta.Y + textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X + textSize.Width, delta.Y - textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X + textSize.Width, delta.Y + textSize.Height * 0.5), rotationAngle)));
                        }
                        else if (anchor == 1 || anchor == 2 || anchor == 3)
                        {
                            if (fillColour.A > 0)
                            {
                                if (Math.Abs(rotationAngle) < Math.PI / 2)
                                {
                                    graphics.FillText(delta.X - textSize.Width * 0.5, delta.Y, attributeValue, fnt, fillColour, TextBaselines.Middle, tag: node.Id);
                                }
                                else
                                {
                                    graphics.Save();
                                    graphics.Rotate(Math.PI);
                                    graphics.FillText(-delta.X - textSize.Width * 0.5, delta.Y, attributeValue, fnt, fillColour, TextBaselines.Middle, tag: node.Id);
                                    graphics.Restore();
                                }
                            }
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X - textSize.Width * 0.5, delta.Y - textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X - textSize.Width * 0.5, delta.Y + textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X + textSize.Width * 0.5, delta.Y - textSize.Height * 0.5), rotationAngle)));
                            updateMaxMin(sumPoint(point, rotatePoint(new Point(delta.X + textSize.Width * 0.5, delta.Y + textSize.Height * 0.5), rotationAngle)));
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
    }
}
