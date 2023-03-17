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

namespace ScaleAxis
{
    /// <summary>
    /// This module is used to draw a scale axis on the tree plot. The module can only be used when _Rectangular_ or _Circular_ coordinates are enabled.
    /// 
    /// For the default values of the parameters that follow, let $t$ be the total length of the tree from the root node to the farthest tip.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Scale axis";
        public const string HelpText = "Draws a scale axis.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const string Id = "aeacf625-90cf-41a5-8d10-c37c75aaa2b1";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABHSURBVDhPY4CBoqKiBhCGcgnyYYAJSpMNRg0YNQAEBsYAr9qN/4HYAcSmxAX7QYYwgkyDCpAFGKE0SQDJUkdKvOC4rdn/AAC+1Bl/V5f5TwAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABqSURBVEhLY2RAAkVFRQ1QJkNfXx+cTao4MmCC0jQDoxYQBKMWEASjFhAEoxYQBKMWYACv2o3/gdgByqWZD/bDLKFlEIEtYQR5CSpAE0DzSEap9KkB0ELEkZY+cNzW7H+AVhaADWdgYGAAAH34H4wiqvdHAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACBSURBVFhHY2RAA0VFRQ1QJhj09fWh8CmVRwdMUHrAwKgDRh0w6oBRB4w6YNQBow4YdcCoA0YdQFcHeNVu/A/EDlAuGAxECOxHdsRARQHcEQOZBsCOYATFC1RgQMCA5wKMviEtAZbQdhzIEHDc1ux/YKAcALYcxBgIB8AtZ2BgYAAAHz4knPTTCf0AAAAASUVORK5CYII=";

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
            double maxLength = tree.LongestDownstreamLength();
            double increment = maxLength * 0.01;
            double tickSpacing = maxLength * 0.05;

            return new List<(string, string)>()
            {
                ( "Axes", "Group:8"),
                
                /// <param name="Top axis">
                /// This check box determines whether the axis is shown above the tree.
                /// </param>
                ( "Top axis", "CheckBox:true"),
                
                /// <param name="Bottom axis">
                /// This check box determines whether the axis is shown below the tree.
                /// </param>
                ( "Bottom axis", "CheckBox:true"),
                
                /// <param name="Reverse axes">
                /// If this check box is checked, the units of the axes are reversed, i.e. the tips of the tree will start at an age of `0`. Otherwise,
                /// the age of `0` will correspond to the root node.
                /// </param>
                ( "Reverse axes", "CheckBox:true"),
                
                /// <param name="Spacing">
                /// This parameter determines the spacing between the tree and the axes.
                /// </param>
                ( "Spacing:", "NumericUpDown:10[\"-Infinity\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="Axis colour:">
                /// This parameter determines the colour used to draw the axis and the labels.
                /// </param>
                ( "Axis colour:", "Colour:[0,0,0,255]"),
                
                /// <param name="Line thickness:">
                /// This parameter determines the thickness of line used to draw the axis.
                /// </param>
                ( "Line thickness:", "NumericUpDown:1[\"0\",\"Infinity\"]"),
                
                /// <param name="Units:">
                /// This parameter is used to determine the measurement units of the scale axis (e.g. Mya or Gya).
                /// </param>
                ( "Units:", "TextBox:"),
                
                /// <param name="Unit position:">
                /// This parameter is used to determine the position of the measurement unit.
                /// </param>
                ( "Unit position:", "ComboBox:1[\"Root\",\"Both\",\"Tips\"]"),

                ( "Ticks", "Group:5"),
                
                /// <param name="Tick spacing:" default="0.05 $\cdot t$">
                /// This parameter determines the spacing between ticks on the axis (in tree age units).
                /// </param>
                ( "Tick spacing:", "NumericUpDown:" + tickSpacing.ToString() + "[\"0\",\"" + maxLength.ToString() + "\",\"" + increment.ToString() + "\",\"0.########\"]"),
                
                /// <param name="Start:">
                /// This parameter determines the start of the axis (in tree age units).
                /// </param>
                ( "Start:", "NumericUpDown:0[\"-Infinity\",\"Infinity\",\"" + increment.ToString() + "\",\"0.########\"]"),
                
                /// <param name="End:" default="$t$">
                /// This parameter determines the end of the axis (in tree age units).
                /// </param>
                ( "End:", "NumericUpDown:" + maxLength.ToString() + "[\"-Infinity\",\"Infinity\",\"" + increment.ToString() + "\",\"0.########\"]"),
                
                /// <param name="Tick size:">
                /// This parameter determines the size of the ticks on the axis.
                /// </param>
                ( "Tick size:", "NumericUpDown:5[\"0\",\"Infinity\"]"),
                
                /// <param name="Labels every:">
                /// This parameter determines how many ticks are labelled.
                /// </param>
                ( "Labels every:", "NumericUpDown:2[\"1\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="Digits:">
                /// This parameter determines the number of decimal digits to show for the age labels.
                /// </param>
                ( "Digits:", "NumericUpDown:2[\"0\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="Font:">
                /// This parameter determines the font used to draw the labels.
                /// </param>
                ( "Font:", "Font:[\"Helvetica\",\"10\"]" ),

                ( "Grid", "Group:3"),
                
                /// <param name="Grid type:">
                /// This parameter determines which kind of grid (if any) is drawn. If the value is `None`, no grid is shown. If the value is `Lines`, lines are drawn
                /// corresponding to every tick in the axis. If the value is `Shading`, alternating shaded and un-shaded areas are drawn.
                /// </param>
                ( "Grid type:", "ComboBox:2[\"None\",\"Lines\",\"Shading\"]"),
                
                /// <param name="Grid colour:">
                /// This parameter determines the colour of the grid lines or shading.
                /// </param>
                ( "Grid colour:", "Colour:[0,0,0,10]"),
                
                /// <param name="Grid dash:">
                /// If the [Grid type](#grid-type) is `Lines`, this parameter determines the dashing style used to draw the grid lines.
                /// </param>
                ( "Grid dash:", "Dash:[0,0,0]"),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { };

            if ((int)currentParameterValues["Grid type:"] == 0)
            {
                controlStatus.Add("Grid colour:", ControlStatus.Hidden);
                controlStatus.Add("Grid dash:", ControlStatus.Hidden);
            }
            else if ((int)currentParameterValues["Grid type:"] == 1)
            {
                controlStatus.Add("Grid colour:", ControlStatus.Enabled);
                controlStatus.Add("Grid dash:", ControlStatus.Enabled);
            }
            else if ((int)currentParameterValues["Grid type:"] == 2)
            {
                controlStatus.Add("Grid colour:", ControlStatus.Enabled);
                controlStatus.Add("Grid dash:", ControlStatus.Hidden);
            }

            parametersToChange = new Dictionary<string, object>() { };

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            Point scalePoint;

            bool circularCoordinates = false;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out scalePoint))
            {
                circularCoordinates = false;
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out scalePoint))
            {
                circularCoordinates = true;
            }
            else
            {
                throw new Exception("The coordinates module is not supported!");
            }

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            void updateMaxMin(Point pt)
            {
                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static Point sumPoint(Point pt1, Point pt2)
            {
                return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
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

            double maxLength = tree.LongestDownstreamLength();

            List<TreeNode> leaves = tree.GetLeaves();

            Font fnt = (Font)parameterValues["Font:"];

            bool showBottomAxis = (bool)parameterValues["Bottom axis"];
            bool showTopAxis = (bool)parameterValues["Top axis"];

            double tickSpacing = (double)parameterValues["Tick spacing:"];
            int labelsEvery = (int)(double)parameterValues["Labels every:"];
            int digits = (int)(double)parameterValues["Digits:"];

            double tickSize = (double)parameterValues["Tick size:"];

            double start = (double)parameterValues["Start:"];
            double end = (double)parameterValues["End:"];

            bool reverseAxis = (bool)parameterValues["Reverse axes"];

            Colour axisColour = (Colour)parameterValues["Axis colour:"];
            double lineWidth = (double)parameterValues["Line thickness:"];

            int gridType = (int)parameterValues["Grid type:"];
            Colour gridColour = (Colour)parameterValues["Grid colour:"];
            LineDash gridDash = (LineDash)parameterValues["Grid dash:"];

            int tickCount = (int)Math.Floor((end - start) / tickSpacing);
            double tickAmount = tickSpacing / (end - start);

            double spacing = (double)parameterValues["Spacing:"];

            string units = (string)parameterValues["Units:"];
            int unitPosition = (int)parameterValues["Unit position:"];

            if (!circularCoordinates)
            {
                Point scaleNorm = normalizePoint(scalePoint);

                Point[] topAxis = null;
                Point[] bottomAxis = null;

                Point p0 = coordinates[tree.Id];
                Point p1 = coordinates[leaves[0].Id];
                Point p2 = coordinates[leaves[^1].Id];
                Point p3 = new Point(scalePoint.X * maxLength + p0.X, scalePoint.Y * maxLength + p0.Y);

                Point perpScalePoint = rotatePoint(scalePoint, Math.PI / 2);
                Point perpScaleNorm = normalizePoint(perpScalePoint);

                if (Math.Abs(scaleNorm.X) > 1e-4 && Math.Abs(scaleNorm.Y) > 1e-4)
                {
                    double m = scalePoint.Y / scalePoint.X;
                    double mp = perpScalePoint.Y / perpScalePoint.X;

                    double xA = (p1.Y - m * p1.X - p0.Y + mp * p0.X) / (mp - m);
                    double yA = p0.Y + mp * (xA - p0.X);

                    double xB = (p2.Y - m * p2.X - p0.Y + mp * p0.X) / (mp - m);
                    double yB = p0.Y + mp * (xB - p0.X);

                    double xC = (p1.Y - m * p1.X - p3.Y + mp * p3.X) / (mp - m);
                    double yC = p3.Y + mp * (xC - p3.X);

                    double xD = (p2.Y - m * p2.X - p3.Y + mp * p3.X) / (mp - m);
                    double yD = p3.Y + mp * (xD - p3.X);

                    topAxis = new Point[] { new Point(xA, yA), new Point(xC, yC) };
                    bottomAxis = new Point[] { new Point(xB, yB), new Point(xD, yD) };
                }
                else if (Math.Abs(scaleNorm.X) > 1e-4)
                {
                    topAxis = new Point[] { new Point(p0.X, p1.Y), new Point(p3.X, p1.Y) };
                    bottomAxis = new Point[] { new Point(p0.X, p2.Y), new Point(p3.X, p2.Y) };
                }
                else if (Math.Abs(scaleNorm.Y) > 1e-4)
                {
                    topAxis = new Point[] { new Point(p1.X, p0.Y), new Point(p1.X, p3.Y) };
                    bottomAxis = new Point[] { new Point(p2.X, p0.Y), new Point(p2.X, p3.Y) };
                }

                if (reverseAxis)
                {
                    Point temp = topAxis[0];
                    topAxis[0] = topAxis[1];
                    topAxis[1] = temp;

                    temp = bottomAxis[0];
                    bottomAxis[0] = bottomAxis[1];
                    bottomAxis[1] = temp;
                }

                if (start != 0 || end != maxLength)
                {
                    Point newTop0 = new Point(topAxis[0].X + (topAxis[1].X - topAxis[0].X) * start / maxLength, topAxis[0].Y + (topAxis[1].Y - topAxis[0].Y) * start / maxLength);
                    Point newTop1 = new Point(topAxis[1].X + (topAxis[1].X - topAxis[0].X) * (end - maxLength) / maxLength, topAxis[1].Y + (topAxis[1].Y - topAxis[0].Y) * (end - maxLength) / maxLength);
                    topAxis = new Point[] { newTop0, newTop1 };

                    Point newBottom0 = new Point(bottomAxis[0].X + (bottomAxis[1].X - bottomAxis[0].X) * start / maxLength, bottomAxis[0].Y + (bottomAxis[1].Y - bottomAxis[0].Y) * start / maxLength);
                    Point newBottom1 = new Point(bottomAxis[1].X + (bottomAxis[1].X - bottomAxis[0].X) * (end - maxLength) / maxLength, bottomAxis[1].Y + (bottomAxis[1].Y - bottomAxis[0].Y) * (end - maxLength) / maxLength);

                    bottomAxis = new Point[] { newBottom0, newBottom1 };
                }

                topAxis[0] = new Point(topAxis[0].X - perpScaleNorm.X * spacing, topAxis[0].Y - perpScaleNorm.Y * spacing);
                topAxis[1] = new Point(topAxis[1].X - perpScaleNorm.X * spacing, topAxis[1].Y - perpScaleNorm.Y * spacing);
                bottomAxis[0] = new Point(bottomAxis[0].X + perpScaleNorm.X * spacing, bottomAxis[0].Y + perpScaleNorm.Y * spacing);
                bottomAxis[1] = new Point(bottomAxis[1].X + perpScaleNorm.X * spacing, bottomAxis[1].Y + perpScaleNorm.Y * spacing);

                updateMaxMin(topAxis[0]);
                updateMaxMin(topAxis[1]);
                updateMaxMin(bottomAxis[0]);
                updateMaxMin(bottomAxis[1]);

                double angle = Math.Atan2(scaleNorm.Y, scaleNorm.X);
                while (Math.Abs(angle) > Math.PI)
                {
                    angle -= 2 * Math.PI * Math.Sign(angle);
                }

                if (gridType == 1 && gridColour.A > 0)
                {
                    for (int i = 0; i <= tickCount; i++)
                    {
                        Point tickTop = new Point(topAxis[0].X * (1 - i * tickAmount) + topAxis[1].X * i * tickAmount, topAxis[0].Y * (1 - i * tickAmount) + topAxis[1].Y * i * tickAmount);
                        Point tickBottom = new Point(bottomAxis[0].X * (1 - i * tickAmount) + bottomAxis[1].X * i * tickAmount, bottomAxis[0].Y * (1 - i * tickAmount) + bottomAxis[1].Y * i * tickAmount);

                        graphics.StrokePath(new GraphicsPath().MoveTo(tickTop).LineTo(tickBottom), gridColour, lineWidth, lineDash: gridDash);
                    }
                }
                else if (gridType == 2 && gridColour.A > 0)
                {
                    for (int i = 0; i <= tickCount; i += 2)
                    {
                        Point tickTop = new Point(topAxis[0].X * (1 - i * tickAmount) + topAxis[1].X * i * tickAmount, topAxis[0].Y * (1 - i * tickAmount) + topAxis[1].Y * i * tickAmount);
                        Point tickBottom = new Point(bottomAxis[0].X * (1 - i * tickAmount) + bottomAxis[1].X * i * tickAmount, bottomAxis[0].Y * (1 - i * tickAmount) + bottomAxis[1].Y * i * tickAmount);

                        double endTick = Math.Min((i + 1) * tickAmount, 1);

                        Point nextTickTop = new Point(topAxis[0].X * (1 - endTick) + topAxis[1].X * endTick, topAxis[0].Y * (1 - endTick) + topAxis[1].Y * endTick);
                        Point nextTickBottom = new Point(bottomAxis[0].X * (1 - endTick) + bottomAxis[1].X * endTick, bottomAxis[0].Y * (1 - endTick) + bottomAxis[1].Y * endTick);

                        graphics.FillPath(new GraphicsPath().MoveTo(tickTop).LineTo(tickBottom).LineTo(nextTickBottom).LineTo(nextTickTop).Close(), gridColour);
                    }
                }

                if (showBottomAxis)
                {
                    graphics.StrokePath(new GraphicsPath().MoveTo(bottomAxis[0]).LineTo(bottomAxis[1]), axisColour, lineWidth);

                    string firstLabel = null;
                    string lastLabel = null;
                    Point firstLabelPoint = new Point(0, 0);
                    Point lastLabelPoint = new Point(0, 0);

                    for (int i = 0; i <= tickCount; i++)
                    {
                        Point tickStart = new Point(bottomAxis[0].X * (1 - i * tickAmount) + bottomAxis[1].X * i * tickAmount, bottomAxis[0].Y * (1 - i * tickAmount) + bottomAxis[1].Y * i * tickAmount);
                        Point tickEnd = new Point(tickStart.X + perpScaleNorm.X * tickSize * (i % labelsEvery == 0 ? 1 : 0.75), tickStart.Y + perpScaleNorm.Y * tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                        if (gridType == 0)
                        {
                            tickStart = new Point(tickStart.X - perpScaleNorm.X * tickSize * (i % labelsEvery == 0 ? 1 : 0.75), tickStart.Y - perpScaleNorm.Y * tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                        }

                        graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                        updateMaxMin(tickStart);
                        updateMaxMin(tickEnd);

                        if (i % labelsEvery == 0)
                        {
                            string text = (start + i * tickSpacing).ToString(digits);

                            if (i == 0)
                            {
                                firstLabel = text;
                                firstLabelPoint = tickEnd;
                            }

                            lastLabel = text;
                            lastLabelPoint = tickEnd;

                            Size textSize = fnt.MeasureText(text);
                            graphics.Save();
                            graphics.Translate(tickEnd);
                            graphics.Rotate(angle);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(-textSize.Width * 0.5, 10, text, fnt, axisColour);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(-textSize.Width * 0.5, -10 - textSize.Height, text, fnt, axisColour);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5 * scaleNorm.X + 10 * perpScaleNorm.X, -textSize.Width * 0.5 * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5 * scaleNorm.X + 10 * perpScaleNorm.X, textSize.Width * 0.5 * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5 * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, textSize.Width * 0.5 * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5 * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, -textSize.Width * 0.5 * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                        }
                    }

                    if (!string.IsNullOrEmpty(units))
                    {
                        Size textSize = fnt.MeasureText(units);

                        if (unitPosition == 0 || unitPosition == 1)
                        {
                            Point candidatePoint;
                            Point endPoint;

                            Size lastLabelSize = fnt.MeasureText(lastLabel);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * Math.Cos(angle) - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * Math.Sin(angle) - scaleNorm.Y * fnt.FontSize);
                                candidatePoint = new Point(bottomAxis[1].X - scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[1].Y - scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);
                            }
                            else
                            {
                                endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * scaleNorm.X - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * scaleNorm.Y - scaleNorm.Y * fnt.FontSize);
                                candidatePoint = new Point(bottomAxis[1].X - scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[1].Y - scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);
                            }

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff > 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);
                            graphics.Rotate(angle);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(-textSize.Width, 10, units, fnt, axisColour);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(0, -10 - fnt.MeasureTextAdvanced(units).Top, units, fnt, axisColour);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X + 10 * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(10 * perpScaleNorm.X, 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point((10 + textSize.Height) * perpScaleNorm.X, (10 + textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                        }

                        if (unitPosition == 2 || unitPosition == 1)
                        {
                            Point candidatePoint;
                            Point endPoint;

                            Size fistLabelSize = fnt.MeasureText(firstLabel);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * Math.Cos(angle) + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * Math.Sin(angle) + scaleNorm.Y * fnt.FontSize);
                                candidatePoint = new Point(bottomAxis[0].X + scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[0].Y + scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);
                            }
                            else
                            {
                                endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * scaleNorm.X + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * scaleNorm.Y + scaleNorm.Y * fnt.FontSize);
                                candidatePoint = new Point(bottomAxis[0].X + scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[0].Y + scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);
                            }


                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff < 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);
                            graphics.Rotate(angle);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(0, 10, units, fnt, axisColour);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(-textSize.Width, -10 - fnt.MeasureTextAdvanced(units).Top, units, fnt, axisColour);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X + 10 * perpScaleNorm.X, textSize.Width * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(10 * perpScaleNorm.X, 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point((10 + textSize.Height) * perpScaleNorm.X, (10 + textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, textSize.Width * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                        }
                    }
                }

                if (showTopAxis)
                {
                    graphics.StrokePath(new GraphicsPath().MoveTo(topAxis[0]).LineTo(topAxis[1]), axisColour, lineWidth);

                    string firstLabel = null;
                    string lastLabel = null;
                    Point firstLabelPoint = new Point(0, 0);
                    Point lastLabelPoint = new Point(0, 0);

                    for (int i = 0; i <= tickCount; i++)
                    {
                        Point tickStart = new Point(topAxis[0].X * (1 - i * tickAmount) + topAxis[1].X * i * tickAmount, topAxis[0].Y * (1 - i * tickAmount) + topAxis[1].Y * i * tickAmount);
                        Point tickEnd = new Point(tickStart.X - perpScaleNorm.X * tickSize * (i % labelsEvery == 0 ? 1 : 0.75), tickStart.Y - perpScaleNorm.Y * tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                        if (gridType == 0)
                        {
                            tickStart = new Point(tickStart.X + perpScaleNorm.X * tickSize * (i % labelsEvery == 0 ? 1 : 0.75), tickStart.Y + perpScaleNorm.Y * tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                        }

                        graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                        updateMaxMin(tickStart);
                        updateMaxMin(tickEnd);

                        if (i % labelsEvery == 0)
                        {
                            string text = (start + i * tickSpacing).ToString(digits);

                            if (i == 0)
                            {
                                firstLabel = text;
                                firstLabelPoint = tickEnd;
                            }

                            lastLabel = text;
                            lastLabelPoint = tickEnd;

                            Size textSize = fnt.MeasureText(text);
                            graphics.Save();
                            graphics.Translate(tickEnd);
                            graphics.Rotate(angle);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(-textSize.Width * 0.5, -10, text, fnt, axisColour, TextBaselines.Baseline);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(-textSize.Width * 0.5, 10 + textSize.Height, text, fnt, axisColour, TextBaselines.Baseline);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5 * scaleNorm.X - 10 * perpScaleNorm.X, -textSize.Width * 0.5 * scaleNorm.Y - 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5 * scaleNorm.X - 10 * perpScaleNorm.X, textSize.Width * 0.5 * scaleNorm.Y - 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5 * scaleNorm.X + (-10 - textSize.Height) * perpScaleNorm.X, textSize.Width * 0.5 * scaleNorm.Y + (-10 - textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5 * scaleNorm.X + (-10 - textSize.Height) * perpScaleNorm.X, -textSize.Width * 0.5 * scaleNorm.Y + (-10 - textSize.Height) * perpScaleNorm.Y)));
                        }
                    }

                    if (!string.IsNullOrEmpty(units))
                    {
                        Size textSize = fnt.MeasureText(units);

                        if (unitPosition == 0 || unitPosition == 1)
                        {
                            Point candidatePoint = new Point(topAxis[1].X - scaleNorm.X * fnt.FontSize - perpScaleNorm.X * tickSize, topAxis[1].Y - scaleNorm.Y * fnt.FontSize - perpScaleNorm.Y * tickSize);
                            Point endPoint;

                            Size lastLabelSize = fnt.MeasureText(lastLabel);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * Math.Cos(angle) - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * Math.Sin(angle) - scaleNorm.Y * fnt.FontSize);
                            }
                            else
                            {
                                endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * scaleNorm.X - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * scaleNorm.Y - scaleNorm.Y * fnt.FontSize);
                            }

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff > 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);
                            graphics.Rotate(angle);

                            Font.DetailedFontMetrics metrics = fnt.MeasureTextAdvanced(units);

                            double top = metrics.Top;

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(-textSize.Width, -10, units, fnt, axisColour, TextBaselines.Baseline);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(0, 10 + top, units, fnt, axisColour, TextBaselines.Baseline);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X - 10 * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + (-10) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-10 * perpScaleNorm.X, -10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-(10 + top) * perpScaleNorm.X, -(10 + top) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X - (10 + top) * perpScaleNorm.X, -textSize.Width * scaleNorm.Y - (10 + top) * perpScaleNorm.Y)));
                        }

                        if (unitPosition == 2 || unitPosition == 1)
                        {
                            Point candidatePoint = new Point(topAxis[0].X + scaleNorm.X * fnt.FontSize - perpScaleNorm.X * tickSize, topAxis[0].Y + scaleNorm.Y * fnt.FontSize - perpScaleNorm.Y * tickSize);
                            Point endPoint;

                            Size fistLabelSize = fnt.MeasureText(firstLabel);

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * Math.Cos(angle) + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * Math.Sin(angle) + scaleNorm.Y * fnt.FontSize);
                            }
                            else
                            {
                                endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * scaleNorm.X + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * scaleNorm.Y + scaleNorm.Y * fnt.FontSize);
                            }

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff < 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);
                            graphics.Rotate(angle);

                            double top = fnt.MeasureTextAdvanced(units).Top;

                            if (Math.Abs(angle) < Math.PI / 2)
                            {
                                graphics.FillText(0, -10, units, fnt, axisColour, TextBaselines.Baseline);
                            }
                            else
                            {
                                graphics.Rotate(Math.PI);
                                graphics.FillText(-textSize.Width, 10 + top, units, fnt, axisColour, TextBaselines.Baseline);
                            }

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X - 10 * perpScaleNorm.X, textSize.Width * scaleNorm.Y - 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-10 * perpScaleNorm.X, -10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-(10 + top) * perpScaleNorm.X, -(10 + top) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X - (10 + top) * perpScaleNorm.X, textSize.Width * scaleNorm.Y - (10 + top) * perpScaleNorm.Y)));
                        }
                    }
                }
            }
            else
            {
                Point center = coordinates["92aac276-3af7-4506-a263-7220e0df5797"];
                Point rootPoint = coordinates[Modules.RootNodeId];

                double radius0 = reverseAxis ? (maxLength * scalePoint.X + scalePoint.Y) : scalePoint.Y;
                double radiusScale = scalePoint.X * (reverseAxis ? -1 : 1);


                double maxRadius = (maxLength * scalePoint.X + scalePoint.Y);
                updateMaxMin(new Point(-maxRadius, -maxRadius));
                updateMaxMin(new Point(maxRadius, maxRadius));

                if (gridType == 1 && gridColour.A > 0)
                {
                    for (int i = 0; i <= tickCount; i++)
                    {
                        graphics.StrokePath(new GraphicsPath().Arc(center, radius0 + i * tickAmount * radiusScale * (end - start), 0, 2 * Math.PI), gridColour, lineWidth, lineDash: gridDash);
                    }
                }
                else if (gridType == 2 && gridColour.A > 0)
                {
                    for (int i = 0; i <= tickCount; i += 2)
                    {
                        double endTick = Math.Min((i + 1) * tickAmount, 1);

                        GraphicsPath pth = new GraphicsPath();

                        if (reverseAxis)
                        {
                            pth.Arc(center, radius0 + i * tickAmount * radiusScale * (end - start), 0, 2 * Math.PI).Close();
                            pth.MoveTo(new Point(center.X + radius0 + endTick * radiusScale * (end - start), center.Y));
                            pth.Arc(center, radius0 + endTick * radiusScale * (end - start), 0, 2 * Math.PI).Close();
                        }
                        else
                        {
                            pth.Arc(center, radius0 + endTick * radiusScale * (end - start), 0, 2 * Math.PI).Close();
                            pth.MoveTo(new Point(center.X + radius0 + i * tickAmount * radiusScale * (end - start), center.Y));
                            pth.Arc(center, radius0 + i * tickAmount * radiusScale * (end - start), 0, 2 * Math.PI).Close();
                        }

                        graphics.FillPath(pth, gridColour);
                    }
                }


                if (showBottomAxis)
                {
                    double axisY = center.Y + maxRadius + tickSize + 15 + spacing;

                    string firstLabel = null;
                    string lastLabel = null;
                    Point firstLabelPoint = new Point(0, 0);
                    Point lastLabelPoint = new Point(0, 0);

                    Point[] bottomAxis = new Point[]
                    {
                        new Point(center.X + scalePoint.Y, axisY),
                        new Point(center.X + maxRadius, axisY)
                    };

                    Point[] bottomAxis2 = new Point[]
                    {
                        new Point(center.X - scalePoint.Y, axisY),
                        new Point(center.X - maxRadius, axisY)
                    };

                    if (reverseAxis)
                    {
                        bottomAxis = new Point[] { bottomAxis[1], bottomAxis[0] };
                        bottomAxis2 = new Point[] { bottomAxis2[1], bottomAxis2[0] };
                    }

                    graphics.StrokePath(new GraphicsPath().MoveTo(bottomAxis[0]).LineTo(bottomAxis[1]), axisColour, lineWidth);
                    graphics.StrokePath(new GraphicsPath().MoveTo(bottomAxis2[0]).LineTo(bottomAxis2[1]), axisColour, lineWidth);

                    for (int i = 0; i <= tickCount; i++)
                    {
                        {
                            Point tickStart = new Point(bottomAxis[0].X * (1 - i * tickAmount) + bottomAxis[1].X * i * tickAmount, bottomAxis[0].Y * (1 - i * tickAmount) + bottomAxis[1].Y * i * tickAmount);
                            Point tickEnd = new Point(tickStart.X, tickStart.Y + tickSize * (i % labelsEvery == 0 ? 1 : 0.75));

                            if (gridType == 0)
                            {
                                tickStart = new Point(tickStart.X, tickStart.Y - tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                            updateMaxMin(tickStart);
                            updateMaxMin(tickEnd);

                            if (i % labelsEvery == 0)
                            {
                                string text = (start + i * tickSpacing).ToString(digits);

                                if (i == 0)
                                {
                                    firstLabel = text;
                                    firstLabelPoint = tickEnd;
                                }

                                Size textSize = fnt.MeasureText(text);
                                graphics.Save();
                                graphics.Translate(tickEnd);
                                graphics.FillText(-textSize.Width * 0.5, 10, text, fnt, axisColour);

                                graphics.Restore();

                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10 + textSize.Height)));
                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10 + textSize.Height)));
                            }
                        }

                        {
                            Point tickStart = new Point(bottomAxis2[0].X * (1 - i * tickAmount) + bottomAxis2[1].X * i * tickAmount, bottomAxis2[0].Y * (1 - i * tickAmount) + bottomAxis2[1].Y * i * tickAmount);
                            Point tickEnd = new Point(tickStart.X, tickStart.Y + tickSize * (i % labelsEvery == 0 ? 1 : 0.75));

                            if (gridType == 0)
                            {
                                tickStart = new Point(tickStart.X, tickStart.Y - tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                            updateMaxMin(tickStart);
                            updateMaxMin(tickEnd);

                            if (i % labelsEvery == 0)
                            {
                                string text = (start + i * tickSpacing).ToString(digits);

                                if (i == 0)
                                {
                                    lastLabel = text;
                                    lastLabelPoint = tickEnd;
                                }

                                Size textSize = fnt.MeasureText(text);
                                graphics.Save();
                                graphics.Translate(tickEnd);
                                graphics.FillText(-textSize.Width * 0.5, 10, text, fnt, axisColour);

                                graphics.Restore();

                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10 + textSize.Height)));
                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10 + textSize.Height)));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(units))
                    {
                        Size textSize = fnt.MeasureText(units);

                        Point scaleNorm = new Point(1, 0);
                        Point perpScaleNorm = new Point(0, 1);

                        if (unitPosition == 0 || unitPosition == 1)
                        {
                            Point candidatePoint;
                            Point endPoint;

                            Size lastLabelSize = fnt.MeasureText(lastLabel);

                            endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * scaleNorm.X - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * scaleNorm.Y - scaleNorm.Y * fnt.FontSize);
                            candidatePoint = new Point(bottomAxis[1].X - scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[1].Y - scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff > 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);

                            graphics.FillText(-textSize.Width, 10, units, fnt, axisColour);

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X + 10 * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(10 * perpScaleNorm.X, 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point((10 + textSize.Height) * perpScaleNorm.X, (10 + textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                        }

                        if (unitPosition == 2 || unitPosition == 1)
                        {
                            Point candidatePoint;
                            Point endPoint;

                            Size fistLabelSize = fnt.MeasureText(firstLabel);

                            endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * scaleNorm.X + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * scaleNorm.Y + scaleNorm.Y * fnt.FontSize);
                            candidatePoint = new Point(bottomAxis[0].X + scaleNorm.X * fnt.FontSize + perpScaleNorm.X * tickSize, bottomAxis[0].Y + scaleNorm.Y * fnt.FontSize + perpScaleNorm.Y * tickSize);


                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff < 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);

                            graphics.FillText(0, 10, units, fnt, axisColour);

                            graphics.Restore();

                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X + 10 * perpScaleNorm.X, textSize.Width * scaleNorm.Y + 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(10 * perpScaleNorm.X, 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point((10 + textSize.Height) * perpScaleNorm.X, (10 + textSize.Height) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X + (10 + textSize.Height) * perpScaleNorm.X, textSize.Width * scaleNorm.Y + (10 + textSize.Height) * perpScaleNorm.Y)));
                        }
                    }
                }

                if (showTopAxis)
                {
                    string firstLabel = null;
                    string lastLabel = null;
                    Point firstLabelPoint = new Point(0, 0);
                    Point lastLabelPoint = new Point(0, 0);

                    double axisY = center.Y - maxRadius - tickSize - 15 - spacing;

                    Point[] topAxis = new Point[]
                    {
                        new Point(center.X + scalePoint.Y, axisY),
                        new Point(center.X + maxRadius, axisY)
                    };

                    Point[] topAxis2 = new Point[]
                    {
                        new Point(center.X - scalePoint.Y, axisY),
                        new Point(center.X - maxRadius, axisY)
                    };

                    if (reverseAxis)
                    {
                        topAxis = new Point[] { topAxis[1], topAxis[0] };
                        topAxis2 = new Point[] { topAxis2[1], topAxis2[0] };
                    }

                    graphics.StrokePath(new GraphicsPath().MoveTo(topAxis[0]).LineTo(topAxis[1]), axisColour, lineWidth);
                    graphics.StrokePath(new GraphicsPath().MoveTo(topAxis2[0]).LineTo(topAxis2[1]), axisColour, lineWidth);

                    for (int i = 0; i <= tickCount; i++)
                    {
                        {
                            Point tickStart = new Point(topAxis[0].X * (1 - i * tickAmount) + topAxis[1].X * i * tickAmount, topAxis[0].Y * (1 - i * tickAmount) + topAxis[1].Y * i * tickAmount);
                            Point tickEnd = new Point(tickStart.X, tickStart.Y - tickSize * (i % labelsEvery == 0 ? 1 : 0.75));

                            if (gridType == 0)
                            {
                                tickStart = new Point(tickStart.X, tickStart.Y + tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                            updateMaxMin(tickStart);
                            updateMaxMin(tickEnd);

                            if (i % labelsEvery == 0)
                            {
                                string text = (start + i * tickSpacing).ToString(digits);

                                if (i == 0)
                                {
                                    firstLabel = text;
                                    firstLabelPoint = tickEnd;
                                }


                                Size textSize = fnt.MeasureText(text);
                                graphics.Save();
                                graphics.Translate(tickEnd);
                                graphics.FillText(-textSize.Width * 0.5, -10, text, fnt, axisColour, TextBaselines.Baseline);

                                graphics.Restore();

                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, -10 - textSize.Height)));
                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, -10 - textSize.Height)));
                            }
                        }

                        {
                            Point tickStart = new Point(topAxis2[0].X * (1 - i * tickAmount) + topAxis2[1].X * i * tickAmount, topAxis2[0].Y * (1 - i * tickAmount) + topAxis2[1].Y * i * tickAmount);
                            Point tickEnd = new Point(tickStart.X, tickStart.Y - tickSize * (i % labelsEvery == 0 ? 1 : 0.75));

                            if (gridType == 0)
                            {
                                tickStart = new Point(tickStart.X, tickStart.Y + tickSize * (i % labelsEvery == 0 ? 1 : 0.75));
                            }

                            graphics.StrokePath(new GraphicsPath().MoveTo(tickStart).LineTo(tickEnd), axisColour, lineWidth, LineCaps.Round);

                            updateMaxMin(tickStart);
                            updateMaxMin(tickEnd);

                            if (i % labelsEvery == 0)
                            {
                                string text = (start + i * tickSpacing).ToString(digits);

                                if (i == 0)
                                {
                                    lastLabel = text;
                                    lastLabelPoint = tickEnd;
                                }

                                Size textSize = fnt.MeasureText(text);
                                graphics.Save();
                                graphics.Translate(tickEnd);
                                graphics.FillText(-textSize.Width * 0.5, -10, text, fnt, axisColour, TextBaselines.Baseline);

                                graphics.Restore();

                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, 10)));
                                updateMaxMin(sumPoint(tickEnd, new Point(textSize.Width * 0.5, -10 - textSize.Height)));
                                updateMaxMin(sumPoint(tickEnd, new Point(-textSize.Width * 0.5, -10 - textSize.Height)));
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(units))
                    {
                        Size textSize = fnt.MeasureText(units);

                        Point scaleNorm = new Point(1, 0);
                        Point perpScaleNorm = new Point(0, 1);

                        if (unitPosition == 0 || unitPosition == 1)
                        {
                            Point candidatePoint = new Point(topAxis[1].X - scaleNorm.X * fnt.FontSize - perpScaleNorm.X * tickSize, topAxis[1].Y - scaleNorm.Y * fnt.FontSize - perpScaleNorm.Y * tickSize);
                            Point endPoint;

                            Size lastLabelSize = fnt.MeasureText(lastLabel);

                            endPoint = new Point(lastLabelPoint.X - lastLabelSize.Width * 0.5 * scaleNorm.X - scaleNorm.X * fnt.FontSize, lastLabelPoint.Y - lastLabelSize.Width * 0.5 * scaleNorm.Y - scaleNorm.Y * fnt.FontSize);

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff > 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);

                            graphics.FillText(-textSize.Width, -10, units, fnt, axisColour, TextBaselines.Baseline);

                            graphics.Restore();

                            double top = fnt.MeasureTextAdvanced(units).Top;

                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X - 10 * perpScaleNorm.X, -textSize.Width * scaleNorm.Y + (-10) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-10 * perpScaleNorm.X, -10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-(10 + top) * perpScaleNorm.X, -(10 + top) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-textSize.Width * scaleNorm.X - (10 + top) * perpScaleNorm.X, -textSize.Width * scaleNorm.Y - (10 + top) * perpScaleNorm.Y)));
                        }

                        if (unitPosition == 2 || unitPosition == 1)
                        {
                            Point candidatePoint = new Point(topAxis[0].X + scaleNorm.X * fnt.FontSize - perpScaleNorm.X * tickSize, topAxis[0].Y + scaleNorm.Y * fnt.FontSize - perpScaleNorm.Y * tickSize);
                            Point endPoint;

                            Size fistLabelSize = fnt.MeasureText(firstLabel);

                            endPoint = new Point(firstLabelPoint.X + fistLabelSize.Width * 0.5 * scaleNorm.X + scaleNorm.X * fnt.FontSize, firstLabelPoint.Y + fistLabelSize.Width * 0.5 * scaleNorm.Y + scaleNorm.Y * fnt.FontSize);

                            double diff = (endPoint.X - candidatePoint.X) * scaleNorm.X + (endPoint.Y - candidatePoint.Y) * scaleNorm.Y;

                            if (diff < 0)
                            {
                                endPoint = candidatePoint;
                            }

                            graphics.Save();
                            graphics.Translate(endPoint);
                            graphics.FillText(0, -10, units, fnt, axisColour, TextBaselines.Baseline);

                            graphics.Restore();

                            double top = fnt.MeasureTextAdvanced(units).Top;

                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X - 10 * perpScaleNorm.X, textSize.Width * scaleNorm.Y - 10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-10 * perpScaleNorm.X, -10 * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(-(10 + top) * perpScaleNorm.X, -(10 + top) * perpScaleNorm.Y)));
                            updateMaxMin(sumPoint(endPoint, new Point(textSize.Width * scaleNorm.X - (10 + top) * perpScaleNorm.X, textSize.Width * scaleNorm.Y - (10 + top) * perpScaleNorm.Y)));
                        }
                    }
                }
            }

            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }
    }
}