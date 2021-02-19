using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace NodeShapes
{
    /// <summary>
    /// This module is used to draw shapes at each node in the tree. The size of the shape and its colour can vary from node
    /// to node based on the value of attributes of the nodes.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Node shapes";
        public const string HelpText = "Draws shapes on nodes, tips or branches.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "7434420a-1afd-46ee-aeea-75ed8a5eeada";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Show on:">
                /// This parameter determines on which nodes the shapes are shown. If the value is `Leaves`, the
                /// shapes are only shown for terminal nodes (nodes with no child nodes). If the value is `Internal
                /// nodes` the shapes are shown only for internal nodes (nodes which have at least one child).
                /// If the value is `All nodes`, shapes are shown for both leaves and internal nodes.
                /// </param>
                ( "Show on:", "ComboBox:0[\"Leaves\",\"Internal nodes\",\"All nodes\"]" ),

                ( "Position", "Group:4"),
                
                /// <param name="Anchor:">
                /// This parameter determines the anchor for the centre of the shape. If the value is `Node`, the centre of the shape is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the centre of the shape is aligned with the midpoint of the branch connecting the node to its parent.
                /// If the value is `Origin`, the alignment depends on the value of the [Branch reference](#branch-reference):
                /// 
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | Branch reference                   | Origin                                                                 |
                /// +====================================+========================================================================+
                /// | Rectangular                        | A point corresponding to the projection of the node on a line          |
                /// |                                    | perpedicular to the direction in which the tree expands and passing    |
                /// |                                    | through the root node. Usually (i.e. if the tree is horizontal), this  |
                /// |                                    | means a point with the same horizontal coordinate as the root node and |
                /// |                                    | the same vertical coordinate as the current node.                      |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | Radial                             | The root node.                                                         |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | Circular                           | The root node.                                                         |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// </param>
                ( "Anchor:", "ComboBox:0[\"Node\",\"Mid-branch\",\"Origin\"]" ),
                
                /// <param name="Orientation reference:">
                /// This parameter determines the direction along which the offset of the centre of the shape from the anchor is computed. If the value is `Horizontal`,
                /// the offset `X` coordinate of the offset corresponds to an horizontal displacement and the `Y` coordinate to a vertical displacement; if the value is
                /// `Branch`, the `X` coordinate corresponds to a shift in the direction of the branch, while the `Y` coordinate corresponds to a shift in a direction
                /// perpendicular to the branch.
                /// </param>
                ( "Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),
                
                /// <param name="Branch reference:">
                /// This parameter determines the algorithm used to compute branch orientations. For best results, the value of this parameter should correspond to the coordinates module actually used.
                /// </param>
                ( "Branch reference:", "ComboBox:0[\"Rectangular\",\"Radial\",\"Circular\"]" ),
                
                /// <param name="Position:">
                /// This parameter determines how shifted from the anchor point the shape is. The `X` coordinate corresponds to the line determined by the [Orientation reference](#orientation-reference);
                /// the `Y` coordinate corresponds to the line perpendicular to this.
                /// </param>
                ( "Position:", "Point:[0,0]" ),

                ( "Appearance", "Group:8"),
                
                /// <param name="Size:">
                /// This parameter determines the size (diameter) of the shapes.
                /// </param>
                ( "Size:", "NumericUpDownByNode:8[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"ShapeSize\",\"Number\",\"true\"]" ),
                
                /// <param name="Shape:">
                /// This parameter determines whether the shape is a circle or a polygon.
                /// </param>
                ( "Shape:", "ComboBox:1[\"Circle\",\"Polygon\"]" ),
                
                /// <param name="Sides:">
                /// If the [Shape](#shape) is a `Polygon`, this parameter determines the number of sides of the polygon.
                /// </param>
                ( "Sides:", "NumericUpDown:5[\"3\",\"Infinity\",\"1\",\"0\"]" ),
                
                /// <param name="Star">
                /// If this check box is checked, the polygon will have a star-like appearance.
                /// </param>
                ( "Star", "CheckBox:true" ),
                
                /// <param name="Angle:">
                /// This parameter determines the orientation of the shape with respect to the [Orientation reference](#orientation-reference). Changing
                /// this value rotates the shape around its centre.
                /// </param>
                ( "Angle:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
                
                /// <param name="Auto fill colour by node">
                /// If this check box is checked, the fill colour of each shape is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible
                /// if the same tree is rendered multiple times.
                /// </param>
                ( "Auto fill colour by node", "CheckBox:true"),
                
                /// <param name="Fill opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto fill colour by node](#auto-fill-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Fill opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to fill each shape (if the [Auto fill colour by node](#auto-fill-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Fill colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"162\",\"232\",\"255\",\"true\"]" ),
                
                /// <param name="Stroke thickness:">
                /// This parameter determines the thickness of the stroke used to draw the shape.
                /// </param>
                ( "Stroke thickness:", "NumericUpDown:0[\"0\",\"Infinity\"]" ),
                
                /// <param name="Auto stroke colour by node">
                /// If this check box is checked, the stroke colour of each shape is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible
                /// if the same tree is rendered multiple times.
                /// </param>
                ( "Auto stroke colour by node", "CheckBox:false"),
                
                /// <param name="Stroke opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto stroke colour by node](#auto-stroke-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Stroke opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to stroke each shape (if the [Auto stroke colour by node](#auto-stroke-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Stroke colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            if ((int)currentParameterValues["Shape:"] == 0)
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Sides:", ControlStatus.Hidden },
                    { "Star", ControlStatus.Hidden },
                    { "Angle:", ControlStatus.Hidden }
                };
            }
            else
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Sides:", ControlStatus.Enabled },
                    { "Star", ControlStatus.Enabled },
                    { "Angle:", ControlStatus.Enabled }
                };
            }

            if ((bool)currentParameterValues["Auto fill colour by node"])
            {
                controlStatus.Add("Fill opacity:", ControlStatus.Enabled);
                controlStatus.Add("Fill colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Fill opacity:", ControlStatus.Hidden);
                controlStatus.Add("Fill colour:", ControlStatus.Enabled);
            }


            if ((bool)currentParameterValues["Auto stroke colour by node"])
            {
                controlStatus.Add("Stroke opacity:", ControlStatus.Enabled);
                controlStatus.Add("Stroke colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Stroke opacity:", ControlStatus.Hidden);
                controlStatus.Add("Stroke colour:", ControlStatus.Enabled);
            }

            parametersToChange = new Dictionary<string, object>() { };

            if ((int)previousParameterValues["Anchor:"] != (int)currentParameterValues["Anchor:"])
            {
                parametersToChange.Add("Position:", new Point(0, 0));
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            bool anyPoint = false;

            void updateMaxMin(Point pt)
            {
                anyPoint = true;

                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static double distance(Point p1, Point p2)
            {
                return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            };

            static Point sumPoint(Point p1, Point p2)
            {
                return new Point(p1.X + p2.X, p1.Y + p2.Y);
            }

            static Point subtractPoint(Point p1, Point p2)
            {
                return new Point(p1.X - p2.X, p1.Y - p2.Y);
            }

            static Point multiplyPoint(Point p1, double scale)
            {
                return new Point(p1.X * scale, p1.Y * scale);
            }

            int showOn = (int)parameterValues["Show on:"];
            int anchor = (int)parameterValues["Anchor:"];
            int reference = (int)parameterValues["Orientation reference:"];
            int branchReference = (int)parameterValues["Branch reference:"];

            Point delta = (Point)parameterValues["Position:"];

            bool autoFill = (bool)parameterValues["Auto fill colour by node"];
            double fillOpacity = (double)parameterValues["Fill opacity:"];

            ColourFormatterOptions Fill = (ColourFormatterOptions)parameterValues["Fill colour:"];
            Colour defaultFill = Fill.DefaultColour;
            Func<object, Colour?> fillFormatter = Fill.Formatter;

            bool autoStroke = (bool)parameterValues["Auto stroke colour by node"];
            double strokeOpacity = (double)parameterValues["Stroke opacity:"];

            ColourFormatterOptions Stroke = (ColourFormatterOptions)parameterValues["Stroke colour:"];
            Colour defaultStroke = Stroke.DefaultColour;
            Func<object, Colour?> strokeFormatter = Stroke.Formatter;

            double strokeThickness = (double)parameterValues["Stroke thickness:"];

            double angle = (double)parameterValues["Angle:"] * Math.PI / 180;

            NumberFormatterOptions Size = (NumberFormatterOptions)parameterValues["Size:"];
            double defaultSize = Size.DefaultValue;
            Func<object, double?> sizeFormatter = Size.Formatter;

            bool circle = (int)parameterValues["Shape:"] == 0;

            int sides = (int)(double)parameterValues["Sides:"];
            bool star = (bool)parameterValues["Star"];

            Point rootPoint = coordinates[Modules.RootNodeId];
            coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out Point circularCenter);

            foreach (TreeNode node in nodes)
            {
                if (showOn == 2 || (showOn == 0 && node.Children.Count == 0) || (showOn == 1 && node.Children.Count > 0))
                {
                    Colour fillColour = defaultFill;

                    if (!autoFill)
                    {
                        if (node.Attributes.TryGetValue(Fill.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                        {
                            fillColour = fillFormatter(fillAttributeObject) ?? defaultFill;
                        }
                    }
                    else
                    {
                        List<string> leafNames = node.GetLeafNames();
                        if (leafNames.Count > 0)
                        {
                            fillColour = Modules.DefaultColours[Math.Abs(leafNames.Aggregate((a, b) => a + "," + b).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(fillOpacity);
                        }
                        else
                        {
                            fillColour = Colour.FromRgb(0, 0, 0);
                        }
                    }

                    Colour strokeColour = defaultStroke;

                    if (!autoStroke)
                    {
                        if (node.Attributes.TryGetValue(Stroke.AttributeName, out object strokeAttributeObject) && strokeAttributeObject != null)
                        {
                            strokeColour = strokeFormatter(strokeAttributeObject) ?? defaultStroke;
                        }
                    }
                    else
                    {
                        strokeColour = Modules.DefaultColours[Math.Abs(node.GetLeafNames().Aggregate((a, b) => a + "," + b).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(strokeOpacity);
                    }

                    double size = defaultSize;

                    if (node.Attributes.TryGetValue(Size.AttributeName, out object sizeAttributeObject) && sizeAttributeObject != null)
                    {
                        size = sizeFormatter(sizeAttributeObject) ?? defaultSize;
                    }

                    Point point = coordinates[node.Id];
                    Point anglePoint = point;
                    double referenceAngle = 0;

                    if (reference == 0 && anchor == 0)
                    {

                    }
                    else
                    {
                        //Rectangular
                        if (branchReference == 0)
                        {
                            if (node.Parent != null)
                            {
                                Point parentPoint = coordinates[node.Parent.Id];

                                Point pA = coordinates[node.Parent.Children[0].Id];
                                Point pB = coordinates[node.Parent.Children[^1].Id];

                                double numerator = pA.Y + pB.Y - 2 * parentPoint.Y;
                                double denominator = pA.X + pB.X - 2 * parentPoint.X;

                                Point rectAnglePoint;

                                if (Math.Abs(numerator) > 1e-5 && Math.Abs(denominator) > 1e-5)
                                {
                                    double m = numerator / denominator;

                                    double x = (m * (parentPoint.Y - point.Y + m * point.X) + parentPoint.X) / (m * m + 1);
                                    double y = parentPoint.Y - (x - parentPoint.X) / m;

                                    rectAnglePoint = new Point(x, y);
                                }
                                else if (Math.Abs(numerator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(point.X, parentPoint.Y);
                                }
                                else if (Math.Abs(denominator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(parentPoint.X, point.Y);
                                }
                                else
                                {
                                    rectAnglePoint = point;
                                }

                                if (reference == 1)
                                {
                                    referenceAngle = Math.Atan2(point.Y - rectAnglePoint.Y, point.X - rectAnglePoint.X);
                                }

                                if (anchor == 1)
                                {
                                    anglePoint = rectAnglePoint;
                                }
                                else if (anchor == 2)
                                {
                                    Point branchVector = new Point(Math.Cos(referenceAngle), Math.Sin(referenceAngle));

                                    double d = (rootPoint.X - point.X) * branchVector.X + (rootPoint.Y - point.Y) * branchVector.Y;

                                    Point proj = new Point(point.X + d * branchVector.X, point.Y + d * branchVector.Y);

                                    anglePoint = new Point(-point.X + proj.X * 2, -point.Y + proj.Y * 2);
                                }
                            }
                            else
                            {
                                Point parentPoint = coordinates[Modules.RootNodeId];

                                if (anchor == 1)
                                {
                                    anglePoint = parentPoint;
                                }
                                else if (anchor == 2)
                                {
                                    anglePoint = new Point(-point.X + parentPoint.X * 2, -point.Y + parentPoint.Y * 2);
                                }

                                if (reference == 1)
                                {
                                    referenceAngle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                                }
                            }
                        }
                        //Radial
                        else if (branchReference == 1)
                        {
                            Point parentPoint;

                            if (node.Parent != null)
                            {
                                parentPoint = coordinates[node.Parent.Id];
                            }
                            else
                            {
                                parentPoint = coordinates[Modules.RootNodeId];
                            }

                            if (anchor == 1)
                            {
                                anglePoint = parentPoint;
                            }

                            if (reference == 1)
                            {
                                referenceAngle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                            }

                        }
                        //Circular
                        else if (branchReference == 2)
                        {
                            Point parentPoint;

                            if (node.Parent != null)
                            {
                                parentPoint = coordinates[node.Parent.Id];
                            }
                            else
                            {
                                parentPoint = coordinates[Modules.RootNodeId];
                            }

                            double myRadius = distance(point, circularCenter);
                            double parentRadius = distance(parentPoint, circularCenter);

                            Point realElbowPoint = sumPoint(point, multiplyPoint(subtractPoint(circularCenter, point), (myRadius - parentRadius) / myRadius));

                            if (anchor == 1)
                            {
                                anglePoint = realElbowPoint;
                            }
                            else if (anchor == 2)
                            {
                                anglePoint = new Point(-point.X + circularCenter.X * 2, -point.Y + circularCenter.Y * 2);
                            }

                            if (reference == 1)
                            {
                                referenceAngle = Math.Atan2(point.Y - realElbowPoint.Y, point.X - realElbowPoint.X);
                            }
                        }
                    }

                    if (double.IsNaN(referenceAngle))
                    {
                        referenceAngle = 0;
                    }

                    point = new Point((point.X + anglePoint.X) * 0.5, (point.Y + anglePoint.Y) * 0.5);

                    if (size > 0)
                    {
                        graphics.Save();
                        graphics.Translate(point);
                        graphics.Rotate(referenceAngle);

                        Point originalPoint = new Point(point.X + delta.X * Math.Cos(referenceAngle) - delta.Y * Math.Sin(referenceAngle), point.Y + delta.X * Math.Sin(referenceAngle) + delta.Y * Math.Cos(referenceAngle));

                        point = delta;

                        GraphicsPath path = new GraphicsPath();
                        if (circle)
                        {
                            path.Arc(point, size * 0.5, 0, 2 * Math.PI);
                        }
                        else
                        {
                            path.MoveTo(point.X + Math.Cos(angle) * size * 0.5, point.Y + Math.Sin(angle) * size * 0.5);

                            if (!star)
                            {
                                double deltaAngle = Math.PI * 2 / sides;
                                for (int i = 1; i < sides; i++)
                                {
                                    path.LineTo(point.X + Math.Cos(angle + deltaAngle * i) * size * 0.5, point.Y + Math.Sin(angle + deltaAngle * i) * size * 0.5);
                                }
                                path.Close();
                            }
                            else
                            {
                                double deltaAngle = Math.PI / sides;
                                for (int i = 1; i < sides * 2; i++)
                                {
                                    if (i % 2 == 0)
                                    {
                                        path.LineTo(point.X + Math.Cos(angle + deltaAngle * i) * size * 0.5, point.Y + Math.Sin(angle + deltaAngle * i) * size * 0.5);
                                    }
                                    else
                                    {
                                        path.LineTo(point.X + Math.Cos(angle + deltaAngle * i) * size * 0.25, point.Y + Math.Sin(angle + deltaAngle * i) * size * 0.25);
                                    }
                                }
                                path.Close();
                            }
                        }

                        if (fillColour.A > 0)
                        {
                            graphics.FillPath(path, fillColour, tag: node.Id);
                        }

                        if (strokeColour.A > 0 && strokeThickness > 0)
                        {
                            graphics.StrokePath(path, strokeColour, strokeThickness, LineCaps.Round, LineJoins.Round, tag: node.Id);
                        }

                        graphics.Restore();

                        updateMaxMin(new Point(originalPoint.X - size * 1.5, originalPoint.Y - size * 1.5));
                        updateMaxMin(new Point(originalPoint.X - size * 1.5, originalPoint.Y + size * 1.5));
                        updateMaxMin(new Point(originalPoint.X + size * 1.5, originalPoint.Y + size * 1.5));
                        updateMaxMin(new Point(originalPoint.X + size * 1.5, originalPoint.Y - size * 1.5));
                    }
                }
            }

            if (!anyPoint)
            {
                minX = minY = maxX = maxY = 0;
            }

            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }
    }
}
