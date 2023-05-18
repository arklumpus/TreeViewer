
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

namespace Branches
{
    /// <summary>
    /// This module is used to draw the branches of the tree. It can draw branches for the different Coordinates modules.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Branches";
        public const string HelpText = "Plots tree branches as lines.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.2.0");
        public const string Id = "7c767b07-71be-48b2-8753-b27f3e973570";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABmSURBVDhPtZIBCsAgCEVt7MQexCsvAoM/meaMHoRB8fxGjQow86PbGihwE6y6iEicPhLg2aX1N0My1iuG7bqMaUGBlXmUR5jcWj/JpGjbcyPZuZFwhEla7F2MBOE/QI69TcrqC4g6l/szDtdX0MEAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACySURBVEhLxZQBDoAgCEWtdWIvUF3AK5eWOjWsL1K9jTGbgnzIQQmitV6sm8/Vyei9FFlwcWwFmzO/PIAkokq/wxgT46ISfV86RY9Ex6G0dArqEpP3EbupSe+C1Vp29nIj6haBpwooqk12wYL5Tyykf7QLryeo9iCV5q4vTwy1w0UC9mRBCURxCXskKYF6UNIiGXeK+I8fIhGyJ1CVCAEZBEoi92D9S5dECC0ycqcIlFGpHcxKWZgwW1sLAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC8SURBVFhH7ZaBDkAwDETHJ+8H8AP7ZYYh2G3aVI14iSyTJVuvd6MySlhrWz8082yjDqMGp81V8Qr04xOmK5oKRCF7APXyKs653Z4cBcrsZY5PeWCq4tjLHEg1TQW6MO6AVfgTJ91OVQCRUqBMt6P3XB5PwX+AVAqieZfs/whHgWieuVS5iqTyjnjcAxDpvCPelwKEX8/6U5JUQPbbQfUAdf1C+R6gQr03UgqI3ni38z0PUOF6RlIBhmeMGQCR0VXP7KODKgAAAABJRU5ErkJggg==";

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
                /// <param name="Root branch">
                /// If this check box is checked, a branch is drawn starting from the root node of the tree. Otherwise, the
                /// root node has no branch going into it. This can be useful to highlight whether a tree is rooted or
                /// unrooted.
                /// </param>
                ( "Root branch", "CheckBox:true" ),
               
                /// <param name="Rounding:">
                /// This parameter determines the amount of rounding to apply to the angles of the branches. A value of `0` produces
                /// sharp angles, while a value of `1` produces completely rounded angles.
                /// </param>
                ( "Rounding:", "Slider:0[\"0\",\"1\"]" ),

                ( "Appearance", "Group:5" ),
                
                /// <param name="Auto colour by node">
                /// If this check box is checked, the colour of each branch is determined algorithmically in a pseudo-random way
                /// designed to achieve an aestethically pleasing distribution of colours, while being reproducible if the same
                /// tree is rendered multiple times.
                /// </param>
                ( "Auto colour by node", "CheckBox:false" ),
                
                /// <param name="Opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto colour by node](#auto-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to draw each branch (if the [Auto colour by node](#auto-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
                
                /// <param name="Line weight:">
                /// This parameter determines the thickness of the lines used to draw the branches. This can be based on the value of
                /// an attribute of the nodes in the tree. For nodes that do not possess the specified attribute (or that have the
                /// attribute with an invalid value), a default value is used. The default attribute used to determine the line
                /// weight is `Thickness`.
                /// </param>
                ( "Line weight:", "NumericUpDownByNode:1[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"Thickness\",\"Number\",\"true\"]" ),
                
                /// <param name="Line cap:">
                /// The line cap to use when drawing the branches.
                /// </param>
                ( "Line cap:", "ComboBox:1[\"Butt\",\"Round\",\"Square\"]" ),

            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>()
            {

            };

            parametersToChange = new Dictionary<string, object>() { };

            if ((bool)currentParameterValues["Auto colour by node"])
            {
                controlStatus.Add("Opacity:", ControlStatus.Enabled);
                controlStatus.Add("Colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Opacity:", ControlStatus.Hidden);
                controlStatus.Add("Colour:", ControlStatus.Enabled);
            }


            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            CheckCoordinates(tree, parameterValues, coordinates);

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            double straightness;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out _))
            {
                // Rectangular coordinates
                straightness = 0;
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out _))
            {
                // Circular coordinates
                straightness = 2;
            }
            else
            {
                // Radial coordinates
                straightness = 1;
            }

            double circleness = straightness - 1;
            bool autoElbow = true;
            double elbow = 0.5;
            double smoothing = (double)parameterValues["Rounding:"];

            ColourFormatterOptions ColourFO = (ColourFormatterOptions)parameterValues["Colour:"];
            Colour defaultColour = ColourFO.DefaultColour;
            Func<object, Colour?> colourFormatter = ColourFO.Formatter;


            bool autoColour = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];


            LineCaps cap = (LineCaps)(int)parameterValues["Line cap:"];
            LineJoins join = (LineJoins)(2 - (int)parameterValues["Line cap:"]);

            NumberFormatterOptions WeightFO = (NumberFormatterOptions)parameterValues["Line weight:"];
            double defaultWeight = WeightFO.DefaultValue;
            Func<object, double?> weightFormatter = WeightFO.Formatter;

            Point rootPoint;

            if (!coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out rootPoint))
            {
                rootPoint = coordinates[Modules.RootNodeId];
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

            static Point rotatePoint(Point pt, double angle)
            {
                return new Point(pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle), pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle));
            }

            static Point normalizePoint(Point pt)
            {
                double modulus = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                return new Point(pt.X / modulus, pt.Y / modulus);
            }

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                bool isCartooned = node.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe");
                bool isCartoonedParent = node.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe") && (node.Parent == null || !node.Parent.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe"));

                Colour colour = defaultColour;

                if (!autoColour)
                {
                    if (node.Attributes.TryGetValue(ColourFO.AttributeName, out object colourAttributeObject) && colourAttributeObject != null)
                    {
                        colour = colourFormatter(colourAttributeObject) ?? defaultColour;
                    }
                }
                else
                {
                    colour = Modules.DefaultColours[Math.Abs(string.Join(",", node.GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                }


                double weight = defaultWeight;

                if (node.Attributes.TryGetValue(WeightFO.AttributeName, out object weightAttributeObject) && weightAttributeObject != null)
                {
                    weight = weightFormatter(weightAttributeObject) ?? defaultWeight;
                }

                if (node.Parent != null && (!isCartooned || isCartoonedParent) && weight > 0 && colour.A > 0)
                {
                    Point childPoint = coordinates[node.Id];
                    Point parentPoint = coordinates[node.Parent.Id];

                    if (straightness == 1)
                    {
                        graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).LineTo(childPoint), colour, weight, cap, join, tag: node.Id);
                    }
                    else if (straightness < 1)
                    {
                        Point anglePoint;

                        Point pA = coordinates[node.Parent.Children[0].Id];
                        Point pB = coordinates[node.Parent.Children[^1].Id];

                        double numerator = pA.Y + pB.Y - 2 * parentPoint.Y;
                        double denominator = pA.X + pB.X - 2 * parentPoint.X;

                        if (Math.Abs(numerator) > 1e-5 && Math.Abs(denominator) > 1e-5)
                        {
                            double m = numerator / denominator;

                            double x = (m * (parentPoint.Y - childPoint.Y + m * childPoint.X) + parentPoint.X) / (m * m + 1);
                            double y = parentPoint.Y - (x - parentPoint.X) / m;

                            anglePoint = new Point(x, y);
                        }
                        else if (Math.Abs(numerator) > 1e-5)
                        {
                            anglePoint = new Point(childPoint.X, parentPoint.Y);
                        }
                        else if (Math.Abs(denominator) > 1e-5)
                        {
                            anglePoint = new Point(parentPoint.X, childPoint.Y);
                        }
                        else
                        {
                            anglePoint = childPoint;
                        }

                        double prop = elbow;
                        if (autoElbow)
                        {
                            double distAP2 = (anglePoint.X - parentPoint.X) * (anglePoint.X - parentPoint.X) + (anglePoint.Y - parentPoint.Y) * (anglePoint.Y - parentPoint.Y);
                            double distAC2 = (anglePoint.X - childPoint.X) * (anglePoint.X - childPoint.X) + (anglePoint.Y - childPoint.Y) * (anglePoint.Y - childPoint.Y);

                            prop = Math.Sqrt(distAP2 / (distAP2 + distAC2));
                        }

                        if (double.IsNaN(prop))
                        {
                            prop = 0;
                        }

                        Point internalPoint = new Point(parentPoint.X * (1 - prop) + childPoint.X * prop, parentPoint.Y * (1 - prop) + childPoint.Y * prop);

                        Point intermediatePoint = new Point(anglePoint.X * (1 - straightness) + internalPoint.X * straightness, anglePoint.Y * (1 - straightness) + internalPoint.Y * straightness);

                        if (smoothing == 0)
                        {
                            graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).LineTo(intermediatePoint).LineTo(childPoint), colour, weight, cap, join, tag: node.Id);
                        }
                        else
                        {
                            Point ctrlPoint1 = new Point(parentPoint.X * smoothing + intermediatePoint.X * (1 - smoothing), parentPoint.Y * smoothing + intermediatePoint.Y * (1 - smoothing));
                            Point ctrlPoint2 = new Point(childPoint.X * smoothing + intermediatePoint.X * (1 - smoothing), childPoint.Y * smoothing + intermediatePoint.Y * (1 - smoothing));

                            graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).LineTo(ctrlPoint1).CubicBezierTo(intermediatePoint, intermediatePoint, ctrlPoint2).LineTo(childPoint), colour, weight, cap, join, tag: node.Id);
                        }

                        minX = Math.Min(minX, anglePoint.X);
                        maxX = Math.Max(maxX, anglePoint.X);
                        minY = Math.Min(minY, anglePoint.Y);
                        maxY = Math.Max(maxY, anglePoint.Y);
                    }
                    else if (straightness > 1)
                    {
                        double myRadius = distance(childPoint, rootPoint);
                        double parentRadius = distance(parentPoint, rootPoint);

                        Point realElbowPoint = sumPoint(childPoint, multiplyPoint(subtractPoint(rootPoint, childPoint), (myRadius - parentRadius) / myRadius));

                        if (smoothing == 0)
                        {
                            Point elbowPoint = sumPoint(multiplyPoint(realElbowPoint, circleness), multiplyPoint(parentPoint, 1 - circleness));

                            double startAngle = Math.Atan2(parentPoint.Y - rootPoint.Y, parentPoint.X - rootPoint.X);
                            double endAngle = Math.Atan2(elbowPoint.Y - rootPoint.Y, elbowPoint.X - rootPoint.X);

                            if (Math.Abs(startAngle - endAngle) > Math.PI)
                            {
                                endAngle += 2 * Math.PI * Math.Sign(startAngle - endAngle);
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).Arc(rootPoint, parentRadius, startAngle, endAngle).LineTo(childPoint), colour, weight, cap, join, tag: node.Id);
                        }
                        else
                        {
                            Point currentElbowPoint = sumPoint(multiplyPoint(realElbowPoint, circleness), multiplyPoint(parentPoint, 1 - circleness));
                            double endAngle = Math.Atan2(currentElbowPoint.Y - rootPoint.Y, currentElbowPoint.X - rootPoint.X);
                            currentElbowPoint = new Point(rootPoint.X + parentRadius * Math.Cos(endAngle), rootPoint.Y + parentRadius * Math.Sin(endAngle));


                            Point ctrlPoint2 = sumPoint(multiplyPoint(childPoint, smoothing), multiplyPoint(currentElbowPoint, 1 - smoothing));
                            Point ctrlPoint1 = sumPoint(multiplyPoint(realElbowPoint, circleness * (1 - smoothing)), multiplyPoint(parentPoint, 1 - circleness * (1 - smoothing)));

                            double startAngle = Math.Atan2(parentPoint.Y - rootPoint.Y, parentPoint.X - rootPoint.X);
                            endAngle = Math.Atan2(ctrlPoint1.Y - rootPoint.Y, ctrlPoint1.X - rootPoint.X);

                            if (Math.Abs(startAngle - endAngle) > Math.PI)
                            {
                                endAngle += 2 * Math.PI * Math.Sign(startAngle - endAngle);
                            }

                            ctrlPoint1 = new Point(rootPoint.X + parentRadius * Math.Cos(endAngle), rootPoint.Y + parentRadius * Math.Sin(endAngle));

                            Point perpRadius = normalizePoint(rotatePoint(subtractPoint(ctrlPoint1, rootPoint), Math.PI / 2));

                            Point otherCtrlPoint = currentElbowPoint;

                            if (!double.IsNaN(perpRadius.X) && !double.IsNaN(perpRadius.Y))
                            {
                                Point diff = subtractPoint(currentElbowPoint, ctrlPoint1);
                                double dotProd = diff.X * perpRadius.X + diff.Y * perpRadius.Y;
                                otherCtrlPoint = sumPoint(ctrlPoint1, multiplyPoint(perpRadius, dotProd));
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).Arc(rootPoint, parentRadius, startAngle, endAngle).CubicBezierTo(otherCtrlPoint, currentElbowPoint, ctrlPoint2).LineTo(childPoint), colour, weight, cap, join, tag: node.Id);
                        }


                    }

                    minX = Math.Min(minX, childPoint.X);
                    maxX = Math.Max(maxX, childPoint.X);
                    minY = Math.Min(minY, childPoint.Y);
                    maxY = Math.Max(maxY, childPoint.Y);

                    minX = Math.Min(minX, parentPoint.X);
                    maxX = Math.Max(maxX, parentPoint.X);
                    minY = Math.Min(minY, parentPoint.Y);
                    maxY = Math.Max(maxY, parentPoint.Y);


                }
                else if (node.Parent == null)
                {
                    Point point = coordinates[node.Id];
                    minX = Math.Min(minX, point.X);
                    maxX = Math.Max(maxX, point.X);
                    minY = Math.Min(minY, point.Y);
                    maxY = Math.Max(maxY, point.Y);

                    if ((bool)parameterValues["Root branch"])
                    {
                        Point parentPoint = coordinates[Modules.RootNodeId];
                        minX = Math.Min(minX, parentPoint.X);
                        maxX = Math.Max(maxX, parentPoint.X);
                        minY = Math.Min(minY, parentPoint.Y);
                        maxY = Math.Max(maxY, parentPoint.Y);

                        graphics.StrokePath(new GraphicsPath().MoveTo(parentPoint).LineTo(point), colour, weight, cap, join, tag: node.Id);
                    }
                }

                if (isCartoonedParent)
                {
                    Colour fillColour = Colour.FromRgb(255, 255, 255);

                    if (node.Attributes.TryGetValue("0c3400fd-8872-4395-83bc-a5dc5f4967fe", out object fillAttributeObject) && fillAttributeObject != null)
                    {
                        fillColour = Modules.DefaultAttributeConvertersToColourCompiled[0].Formatter(fillAttributeObject) ?? Colour.FromRgb(255, 255, 255);
                    }

                    List<TreeNode> cartoonLeaves = node.GetLeaves();

                    GraphicsPath pth = new GraphicsPath().MoveTo(coordinates[node.Id]);

                    for (int i = 0; i < cartoonLeaves.Count; i++)
                    {
                        Point point = coordinates[cartoonLeaves[i].Id];
                        pth.LineTo(point);

                        minX = Math.Min(minX, point.X);
                        maxX = Math.Max(maxX, point.X);
                        minY = Math.Min(minY, point.Y);
                        maxY = Math.Max(maxY, point.Y);
                    }

                    pth.Close();

                    graphics.FillPath(pth, fillColour, tag: node.Id);
                    graphics.StrokePath(pth, colour, weight, cap, join, tag: node.Id);
                }
            }

            return new Point[] { new Point(minX - defaultWeight * 2, minY - defaultWeight * 2), new Point(maxX + defaultWeight * 2, maxY + defaultWeight * 2) };
        }

        private const string RootBranchMessageId = "b05de536-158d-4159-8996-d3207e2c6802";

        private static void CheckCoordinates(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates)
        {
            string message = "";
            string messageId = Id;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out _))
            {
                // Rectangular coordinates

                if (tree.Children.Count > 2 && (bool)parameterValues["Root branch"])
                {
                    messageId = RootBranchMessageId;
                    message += "The tree appears to be unrooted; thus, drawing the `Root branch` may be inappropriate.";
                }
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out _))
            {
                // Circular coordinates

                if (tree.Children.Count > 2 && (bool)parameterValues["Root branch"])
                {
                    messageId = RootBranchMessageId;
                    message += "The tree appears to be unrooted; thus, drawing the `Root branch` may be inappropriate.";
                }
            }

            else
            {
                // Radial coordinates
                if ((bool)parameterValues["Root branch"])
                {
                    message = "With the current Coordinates module, it is not recommended to draw the `Root branch`.";
                    messageId = RootBranchMessageId;
                }
            }

            if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
            {
                setWarning(message, messageId);
            }
        }
    }
}
