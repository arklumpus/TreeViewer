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
using System.Linq;
using Accord.Statistics.Analysis;
using Accord.Statistics.Models.Regression.Linear;

namespace a28e76a5769304c49974c639c104dc4ba
{
    ///<summary>
    /// This module adds shapes that highlight nodes in the tree. The shapes are rectangles if the tree is drawn using
    /// rectangular coordinates, or "wedges" if the tree is drawn with circular coordinates. If the tree is drawn
    /// using radial coordinates, the shapes of the highlight either follow the positions of the leaves that descend
    /// from the highlighted nodes, or their convex hull.
    /// 
    /// To draw highlights using this module, you need to add an attribute called `Highlight` to the node(s) that you
    /// wish to highlight, containing the colour for the highlight. You can similarly specify the highlight stroke by
    /// adding a `HighlightStroke` attribute. You can change the names of the attribute by changing the parameters of
    /// this module.
    /// 
    /// If only wish to higlight a single node, the _Highlight node_ module (id `64769664-d163-4fce-b7ba-18fd9445fcfb`) might be more appropriate.
    ///</summary>

    public static class MyModule
    {
        public const string Name = "Highlights";
        public const string HelpText = "Highlights multiple nodes in a tree based on attribute values.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;
        public const string Id = "28e76a57-6930-4c49-974c-639c104dc4ba";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACTSURBVDhPjZI7EoAgDETR8XQ23oXeQcaeu9h4PTUIyGcNeU1gCDtLlkFVzOuxPcW8uy52DIsc6WXCIAcX1XNfmrOc2Df53Q9aa99U45xL4qwAkTcj2ENyQALRLiIJcHY5geIJtd0/0ZxmBigFlIgoBQK5KFLgbPYSIFKDZOII9JVtqBK+XsnEEcUTwrKLZDZClLoBRj4+97EFOdoAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADWSURBVEhLtZTBDsMgCIZ16dP1snfxvrRmd99ll71eJw4XElGBrl/SiAcEfqDeCVgfrz0f2/emIt7QmGF5HNikFRxoxvfzDtUMoRWrAuTHfQhhKldKyVcfqUQUlVwLnmogSzQbUKKCRaJiUxlGWCRSIQoAmcOHVxWNk2RKgDMSWZeKpTtFvSmpTZYi6gFIIZGD4/IpMi8a0JusXO1vUBatppWRX8qBc5ASwCJRxFMEtwclMzpFpMHnf9eTAGqkEqlkIbR+UIG18RyX70G3B/+Cq8CqN4NzH3m5XpX1MoTyAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADhSURBVFhH7ZY9DsMgDIWhyumy9C7sVYK6c5cuvV6LU4gYSvAfYuGTIkik4Bfbz4o1TNbHa4/L9rtj429pw0EaHNgkGfikrX8/75ANNGX2xAJicOucI5UjhGDz+5ISlLDLoZWBYw9fBmuLHiU4BeTnWLRKwGYKYAuA2sOVbtlUD6B6G9BuQra3KVxlAO3tITaEQNRg/5g2HC5gSasamNkQe+e0+JI7WAPsWSGKjCIOAVol8Gklw54DhQX7/JIRBIiQlICd9oL6GZABzQatMQfRFDBcQHMO9CLPl6sMaPi8gTFfHD5tlSodNYEAAAAASUVORK5CYII=";

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
                ("Appearance", "Group:6"),

                /// <param name="Stroke colour:">
                /// This colour is used to stroke the outline of the highlight.
                /// </param>
                ( "Stroke colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"HighlightStroke\",\"String\",\"0\",\"0\",\"0\",\"0\",\"true\"]" ),
                
                /// <param name="Stroke thickness:">
                /// The thickness of the outline of the highlight.                
                /// </param>
                ( "Stroke thickness:", "NumericUpDownByNode:1[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"Thickness\",\"Number\",\"true\"]" ),
                
                /// <param name="Line dash:">
                /// The line dash to use when drawing the outline of the highlight.
                /// </param>
                ( "Line dash:", "Dash:[0,0,0]"),
                
                /// <param name="Fill type:">
                /// This parameter determines whether the highlight is filled with a solid colour or with a gradient.
                /// </param>
                ( "Fill type:", "ComboBox:0[\"Solid colour\",\"Gradient\"]" ),
                
                /// <param name="Fill colour:">
                /// This colour is used to fill the highlight with a solid colour.
                /// </param>
                ( "Fill colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Highlight\",\"String\",\"0\",\"0\",\"0\",\"0\",\"true\"]" ),

                ("Gradient", "Group:5"),
                
                /// <param name="Start:">
                /// This colour represents the starting colour in the gradient.
                /// </param>
                ( "Start:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"HighlightStart\",\"String\",\"0\",\"0\",\"0\",\"0\",\"true\"]" ),
                
                /// <param name="End:">
                /// This colour represents the final colour in the gradient.
                /// </param>
                ( "End:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Highlight\",\"String\",\"0\",\"0\",\"0\",\"0\",\"true\"]" ),
                
                /// <param name="Midpoint:">
                /// This parameter determines where the midpoint of the gradient (i.e., the colour halfway through the [gradient Start](#start)
                /// and the [gradient End](#end)) is located, relative to the gradient. `0` refers to the start of the gradient and `1` to its end.
                /// </param>
                ( "Midpoint:", "Slider:0.5[\"0\",\"1\"]" ),
                
                /// <param name="Direction:">
                /// This parameter represents the direction of the gradient. `Root to leaves` means that the [gradient Start](#start) refers to
                /// the most ancestral node in the selection, while the [gradient End](#end) refers the most distant leaf node, hence the 
                /// gradient is parallel to the "growing direction" of the tree. `Leaf to leaf` means that the [Gradient Start](#start) refers
                /// to the first selected leaf node, while the [gradient End](#end) refers to the last selected leaf node, hence the gradient
                /// is perpendicular to the "growing direction" of the tree.
                /// </param>
                ( "Direction:", "ComboBox:0[\"Root to leaves\",\"Leaf to leaf\"]" ),

                /// <param name="Gradient version:">
                /// This hidden parameter is used internally to ensure forwards compatibility.
                /// </param>
                ( "Gradient version:", "TextBox:1.1.0"),

                ( "Margins", "Group:4" ),

                /// <param name="Left:">
                /// The left margin of the highlight. If the tree is drawn using circular coordinates, this is the
                /// margin on the inner radius.
                /// 
                /// This setting has no effect if the tree is drawn using radial coordinates.
                /// </param>
                ( "Left:", "NumericUpDown:5[\"-Infinity\",\"Infinity\"]" ),
                
                /// <param name="Top:">
                /// The top margin of the highlight. If the tree is drawn using circular coordinates, this is the
                /// margin on the start angle.
                /// 
                /// This setting has no effect if the tree is drawn using radial coordinates.
                /// </param>
                ( "Top:", "NumericUpDown:5[\"-Infinity\",\"Infinity\"]" ),
                
                /// <param name="Right:">
                /// The right margin of the highlight. If the tree is drawn using circular coordinates, this is the
                /// margin on the outer radius.
                /// 
                /// This setting has no effect if the tree is drawn using radial coordinates.
                /// </param>
                ( "Right:", "NumericUpDown:5[\"-Infinity\",\"Infinity\"]" ),
                
                /// <param name="Bottom:">
                /// The bottom margin of the highlight. If the tree is drawn using circular coordinates, this is the
                /// margin on the end angle.
                /// 
                /// This setting has no effect if the tree is drawn using radial coordinates.
                /// </param>
                ( "Bottom:", "NumericUpDown:5[\"-Infinity\",\"Infinity\"]" ),

                ( "Radial coordinates", "Group:3" ),
                
                /// <param name="Mode:">
                /// Determines the algorithm used to draw the highlight in a tree drawn using radial coordinates. If
                /// the selected value is `Leaf points`, the highlight path tightly envelopes the descendants of the
                /// selected node and it is not possible to specify a margin (because the path may not be convex). If
                /// the selected value is `Convex hull`, the convex hull of the coordinates of all the points that
                /// descend from the selected node is used instead. In this case, it is possible to specify a margin.
                /// 
                /// This setting has no effect if the tree is not drawn using radial coordinates.
                /// </param>
                ( "Mode:", "ComboBox:1[\"Leaf points\",\"Convex hull\"]" ),
                
                /// <param name="Margin:">
                /// The margin to use when highlighting the convex hull of the descendants of the selected node in a
                /// tree drawn using radial coordinates. This parameter is not available if the tree is not drawn using
                /// radial coordinates, or if the [Mode](#mode) is set to `Leaf points`.
                /// 
                /// This setting has no effect if the tree is not drawn using radial coordinates.
                /// </param>
                ( "Margin:", "NumericUpDown:5[\"0\",\"Infinity\"]" ),
                
                /// <param name="Margin balance:">
                /// <![CDATA[
                /// This parameter determines the balance of the margin applied to the convex hull. If the value of this
                /// parameter is 0.5, the same margin is applied to the root side of the node and to the leaf-side of the
                /// node. If this value is &lt; 0.5, a higher margin is applied to the root side of the node than to the
                /// leaf side. If the value is &gt; 0.5, a higher margin is applied to the leaf side of the node than to
                /// the root side. Changing this parameter is useful e.g. if you want to have a large margin towards the
                /// tips of the tree, but a smaller margin at the position of the selected node itself (maybe because
                /// there are other branches close by that could cause confusion).
                /// 
                /// This setting has no effect if the tree is not drawn using radial coordinates.
                /// ]]>
                /// </param>
                ( "Margin balance:", "Slider:0.5[\"0\",\"1\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { { "Gradient version:", ControlStatus.Hidden } };
            parametersToChange = new Dictionary<string, object>();


            if ((int)currentParameterValues["Mode:"] == 0)
            {
                controlStatus["Margin:"] = ControlStatus.Hidden;
                controlStatus["Margin balance:"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Margin:"] = ControlStatus.Enabled;
                controlStatus["Margin balance:"] = ControlStatus.Enabled;
            }

            if ((int)currentParameterValues["Fill type:"] == 1)
            {
                controlStatus["Fill colour:"] = ControlStatus.Hidden;
                controlStatus["Gradient"] = ControlStatus.Enabled;
            }
            else
            {
                controlStatus["Fill colour:"] = ControlStatus.Enabled;
                controlStatus["Gradient"] = ControlStatus.Hidden;
            }



            return true;
        }


        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            ColourFormatterOptions strokeFO = (ColourFormatterOptions)parameterValues["Stroke colour:"];
            Colour defaultStroke = strokeFO.DefaultColour;

            ColourFormatterOptions fillFO = (ColourFormatterOptions)parameterValues["Fill colour:"];
            Colour defaultFill = fillFO.DefaultColour;

            NumberFormatterOptions lineWidthFO = (NumberFormatterOptions)parameterValues["Stroke thickness:"];
            double defaultLineWidth = lineWidthFO.DefaultValue;


            LineDash dash = (LineDash)parameterValues["Line dash:"];
            double marginLeft = (double)parameterValues["Left:"];
            double marginRight = (double)parameterValues["Right:"];
            double marginTop = (double)parameterValues["Top:"];
            double marginBottom = (double)parameterValues["Bottom:"];

            bool convexHull = (int)parameterValues["Mode:"] == 1;
            double margin = (double)parameterValues["Margin:"];
            double marginBalance = 1 - (double)parameterValues["Margin balance:"];

            bool gradient = (int)parameterValues["Fill type:"] == 1;
            ColourFormatterOptions gradientStartColourFO = (ColourFormatterOptions)parameterValues["Start:"];
            ColourFormatterOptions gradientEndColourFO = (ColourFormatterOptions)parameterValues["End:"];
            int gradientDirection = (int)parameterValues["Direction:"];
            double gradientMidPoint = (double)parameterValues["Midpoint:"];


            double ptMinX = double.MaxValue;
            double ptMaxX = double.MinValue;
            double ptMinY = double.MaxValue;
            double ptMaxY = double.MinValue;

            bool anyMaxMin = false;

            void updateMaxMin(Point pt)
            {
                anyMaxMin = true;
                ptMinX = Math.Min(ptMinX, pt.X);
                ptMaxX = Math.Max(ptMaxX, pt.X);
                ptMinY = Math.Min(ptMinY, pt.Y);
                ptMaxY = Math.Max(ptMaxY, pt.Y);
            }

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {

                Colour fill = defaultFill;

                if (node.Attributes.TryGetValue(fillFO.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                {
                    fill = fillFO.Formatter(fillAttributeObject) ?? defaultFill;
                }

                Colour stroke = defaultStroke;

                if (node.Attributes.TryGetValue(strokeFO.AttributeName, out object strokeAttributeObject) && strokeAttributeObject != null)
                {
                    stroke = strokeFO.Formatter(strokeAttributeObject) ?? defaultStroke;
                }

                double lineWidth = defaultLineWidth;

                if (node.Attributes.TryGetValue(lineWidthFO.AttributeName, out object weightAttributeObject) && weightAttributeObject != null)
                {
                    lineWidth = lineWidthFO.Formatter(weightAttributeObject) ?? defaultLineWidth;
                }

                Colour gradientStartColour = gradientStartColourFO.DefaultColour;

                if (node.Attributes.TryGetValue(gradientStartColourFO.AttributeName, out object gradientStartAttributeObject) && gradientStartAttributeObject != null)
                {
                    gradientStartColour = gradientStartColourFO.Formatter(gradientStartAttributeObject) ?? gradientStartColourFO.DefaultColour;
                }

                Colour gradientEndColour = gradientEndColourFO.DefaultColour;

                if (node.Attributes.TryGetValue(gradientEndColourFO.AttributeName, out object gradientEndAttributeObject) && gradientEndAttributeObject != null)
                {
                    gradientEndColour = gradientEndColourFO.Formatter(gradientEndAttributeObject) ?? gradientEndColourFO.DefaultColour;
                }

                (double L1, double a1, double b1) = gradientStartColour.ToLab();
                (double L2, double a2, double b2) = gradientEndColour.ToLab();

                Colour midpointColour = Colour.FromLab((L1 + L2) * 0.5, (a1 + a2) * 0.5, (b1 + b2) * 0.5).WithAlpha((gradientStartColour.A + gradientEndColour.A) * 0.5);

                GradientStops gradientStops = new GradientStops(new VectSharp.GradientStop(gradientStartColour, 0), new VectSharp.GradientStop(midpointColour, gradientMidPoint), new VectSharp.GradientStop(gradientEndColour, 1));

                Brush gradientFill = null;

                GraphicsPath pth = new GraphicsPath();

                if ((fill.A > 0 || (gradient && (gradientStartColour.A > 0 || gradientEndColour.A > 0))) || (stroke.A > 0 && lineWidth > 0))
                {

                    if (coordinates.TryGetValue("95b61284-b870-48b9-b51c-3276f7d89df1", out Point scale))
                    {
                        // Radial coordinates

                        if (node.Children.Count == 0)
                        {
                            pth.Arc(coordinates[node.Id], margin, 0, 2 * Math.PI);
                            pth.Close();

                            if (gradient)
                            {
                                Point nodePt = coordinates[node.Id];

                                Point parentPt;

                                if (node.Parent != null)
                                {
                                    parentPt = coordinates[node.Parent.Id];
                                }
                                else
                                {
                                    parentPt = coordinates[Modules.RootNodeId];
                                }

                                double angle = Math.Atan2(nodePt.Y - parentPt.Y, nodePt.X - parentPt.X);

                                Point p1 = new Point();
                                Point p2 = new Point();

                                if (gradientDirection == 0)
                                {
                                    p1 = new Point(nodePt.X - margin * Math.Cos(angle), nodePt.Y - margin * Math.Sin(angle));
                                    p2 = new Point(nodePt.X + margin * Math.Cos(angle), nodePt.Y + margin * Math.Sin(angle));
                                }
                                else if (gradientDirection == 1)
                                {
                                    p1 = new Point(nodePt.X - margin * Math.Cos(angle + Math.PI / 2), nodePt.Y - margin * Math.Sin(angle + Math.PI / 2));
                                    p2 = new Point(nodePt.X + margin * Math.Cos(angle + Math.PI / 2), nodePt.Y + margin * Math.Sin(angle + Math.PI / 2));
                                }

                                gradientFill = new LinearGradientBrush(p1, p2, gradientStops);

                            }
                        }
                        else
                        {
                            Point nodePoint = coordinates[node.Id];

                            List<Point> points = new List<Point>();

                            double maxDistance = double.MinValue;

                            if (convexHull)
                            {
                                foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                                {
                                    Point pt = coordinates[child.Id];
                                    points.Add(pt);

                                    maxDistance = Math.Max(maxDistance, new Point(pt.X - nodePoint.X, pt.Y - nodePoint.Y).Modulus());
                                }

                                points = CreateConvexHull(points);
                            }
                            else
                            {
                                margin = 0;
                                points.Add(nodePoint);
                                foreach (TreeNode leaf in node.GetLeaves())
                                {
                                    points.Add(coordinates[leaf.Id]);
                                }
                            }

                            double[][] finalPoints = null;

                            if (margin > 0)
                            {
                                List<Point> pathWithMargin = new List<Point>();

                                for (int i = 0; i < points.Count; i++)
                                {
                                    Point start = points[i];
                                    Point end = points[(i + 1) % points.Count];

                                    Point direction = new Point(end.X - start.X, end.Y - start.Y);
                                    double length = direction.Modulus();

                                    direction = new Point(direction.X / length, direction.Y / length);
                                    Point normal = new Point(direction.Y, -direction.X);

                                    double startDistance = Math.Sqrt((start.X - nodePoint.X) * (start.X - nodePoint.X) + (start.Y - nodePoint.Y) * (start.Y - nodePoint.Y));
                                    double endDistance = Math.Sqrt((end.X - nodePoint.X) * (end.X - nodePoint.X) + (end.Y - nodePoint.Y) * (end.Y - nodePoint.Y));

                                    double startWeight = Math.Min(1, ((1 - marginBalance) * (1 - startDistance / maxDistance) + marginBalance * startDistance / maxDistance) / Math.Max(marginBalance, 1 - marginBalance));
                                    double endWeight = Math.Min(1, ((1 - marginBalance) * (1 - endDistance / maxDistance) + marginBalance * endDistance / maxDistance) / Math.Max(marginBalance, 1 - marginBalance));

                                    pathWithMargin.Add(new Point(start.X + normal.X * margin * startWeight, start.Y + normal.Y * margin * startWeight));
                                    pathWithMargin.Add(new Point(end.X + normal.X * margin * endWeight, end.Y + normal.Y * margin * endWeight));
                                }



                                for (int i = 0; i < pathWithMargin.Count; i += 2)
                                {
                                    if (i == 0)
                                    {
                                        pth.MoveTo(pathWithMargin[i]);
                                        pth.LineTo(pathWithMargin[i + 1]);

                                        updateMaxMin(pathWithMargin[i]);
                                        updateMaxMin(pathWithMargin[i + 1]);
                                    }
                                    else
                                    {
                                        Point previousDirection = new Point(pathWithMargin[i - 1].X - pathWithMargin[i - 2].X, pathWithMargin[i - 1].Y - pathWithMargin[i - 2].Y);
                                        double prevDirMod = previousDirection.Modulus();
                                        previousDirection = new Point(previousDirection.X / prevDirMod, previousDirection.Y / prevDirMod);

                                        Point currentDirection = new Point(pathWithMargin[i + 1].X - pathWithMargin[i].X, pathWithMargin[i + 1].Y - pathWithMargin[i].Y);
                                        double currDirMod = currentDirection.Modulus();
                                        currentDirection = new Point(currentDirection.X / currDirMod, currentDirection.Y / currDirMod);

                                        double t = ((pathWithMargin[i].X - pathWithMargin[i - 2].X) * currentDirection.Y - (pathWithMargin[i].Y - pathWithMargin[i - 2].Y) * currentDirection.X) / (previousDirection.X * currentDirection.Y - previousDirection.Y * currentDirection.X);

                                        Point vertex = new Point(pathWithMargin[i - 2].X + t * previousDirection.X, pathWithMargin[i - 2].Y + t * previousDirection.Y);

                                        pth.CubicBezierTo(vertex, vertex, pathWithMargin[i]);
                                        pth.LineTo(pathWithMargin[i + 1]);

                                        updateMaxMin(vertex);
                                        updateMaxMin(pathWithMargin[i]);
                                        updateMaxMin(pathWithMargin[i + 1]);
                                    }

                                    if (i == pathWithMargin.Count - 2)
                                    {
                                        Point previousDirection = new Point(pathWithMargin[i + 1].X - pathWithMargin[i].X, pathWithMargin[i + 1].Y - pathWithMargin[i].Y);
                                        double prevDirMod = previousDirection.Modulus();
                                        previousDirection = new Point(previousDirection.X / prevDirMod, previousDirection.Y / prevDirMod);

                                        Point currentDirection = new Point(pathWithMargin[1].X - pathWithMargin[0].X, pathWithMargin[1].Y - pathWithMargin[0].Y);
                                        double currDirMod = currentDirection.Modulus();
                                        currentDirection = new Point(currentDirection.X / currDirMod, currentDirection.Y / currDirMod);

                                        double t = ((pathWithMargin[0].X - pathWithMargin[i].X) * currentDirection.Y - (pathWithMargin[0].Y - pathWithMargin[i].Y) * currentDirection.X) / (previousDirection.X * currentDirection.Y - previousDirection.Y * currentDirection.X);

                                        Point vertex = new Point(pathWithMargin[i].X + t * previousDirection.X, pathWithMargin[i].Y + t * previousDirection.Y);

                                        pth.CubicBezierTo(vertex, vertex, pathWithMargin[0]);

                                        updateMaxMin(vertex);
                                    }
                                }

                                if (gradient)
                                {
                                    finalPoints = pathWithMargin.Select(x => new double[] { x.X, x.Y }).ToArray();
                                }
                            }
                            else
                            {
                                for (int i = 0; i < points.Count; i++)
                                {
                                    pth.LineTo(points[i]);
                                    updateMaxMin(points[i]);
                                }

                                if (gradient)
                                {
                                    finalPoints = points.Select(x => new double[] { x.X, x.Y }).ToArray();
                                }
                            }
                            pth.Close();

                            if (gradient)
                            {
                                PrincipalComponentAnalysis pca = new PrincipalComponentAnalysis();
                                MultivariateLinearRegression transform = pca.Learn(finalPoints);
                                MultivariateLinearRegression inverseTransform = transform.Inverse();

                                GraphicsPath transformedPth = pth.Transform(x =>
                                {
                                    double[] d = transform.Transform(new double[] { x.X, x.Y });
                                    return new Point(d[0], d[1]);
                                });

                                Rectangle bounds = transformedPth.GetBounds();

                                double[] p1D = inverseTransform.Transform(new double[] { bounds.Location.X, bounds.Location.Y + bounds.Size.Height * 0.5 });
                                double[] p2D = inverseTransform.Transform(new double[] { bounds.Location.X + bounds.Size.Width, bounds.Location.Y + bounds.Size.Height * 0.5 });

                                double[] p3D = inverseTransform.Transform(new double[] { bounds.Location.X + bounds.Size.Width * 0.5, bounds.Location.Y });
                                double[] p4D = inverseTransform.Transform(new double[] { bounds.Location.X + bounds.Size.Width * 0.5, bounds.Location.Y + bounds.Size.Height });

                                Point p1 = new Point(p1D[0], p1D[1]);
                                Point p2 = new Point(p2D[0], p2D[1]);
                                Point p3 = new Point(p3D[0], p3D[1]);
                                Point p4 = new Point(p4D[0], p4D[1]);

                                Point nodePt = coordinates[node.Id];

                                Point parentPt;

                                if (node.Parent != null)
                                {
                                    parentPt = coordinates[node.Parent.Id];
                                }
                                else
                                {
                                    parentPt = coordinates[Modules.RootNodeId];
                                }

                                double[] transfParentPt = transform.Transform(new double[] { parentPt.X, parentPt.Y });
                                double[] transfNodePt = transform.Transform(new double[] { nodePt.X, nodePt.Y });

                                Font fnt = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 8);

                                if (Math.Abs(transfNodePt[0] - transfParentPt[0]) >= Math.Abs(transfNodePt[1] - transfParentPt[1]))
                                {
                                    if (gradientDirection == 0)
                                    {
                                        if (transfNodePt[0] - transfParentPt[0] > 0)
                                        {
                                            gradientFill = new LinearGradientBrush(p1, p2, gradientStops);
                                        }
                                        else
                                        {
                                            gradientFill = new LinearGradientBrush(p2, p1, gradientStops);
                                        }
                                    }
                                    else if (gradientDirection == 1)
                                    {
                                        Point M1;
                                        Point M2;

                                        if (transfNodePt[0] - transfParentPt[0] > 0)
                                        {
                                            M1 = p1;
                                            M2 = p2;
                                        }
                                        else
                                        {
                                            M1 = p2;
                                            M2 = p1;
                                        }

                                        Point m1;
                                        Point m2;

                                        if ((p4.X - p3.X) * (M2.Y - M1.Y) - (p4.Y - p3.Y) * (M2.X - M1.X) < 0)
                                        {
                                            m1 = p3;
                                            m2 = p4;
                                        }
                                        else
                                        {
                                            m1 = p4;
                                            m2 = p3;
                                        }

                                        gradientFill = new LinearGradientBrush(m1, m2, gradientStops);
                                    }
                                }
                                else
                                {
                                    if (gradientDirection == 0)
                                    {
                                        if (transfNodePt[1] - transfParentPt[1] > 0)
                                        {
                                            gradientFill = new LinearGradientBrush(p3, p4, gradientStops);
                                        }
                                        else
                                        {
                                            gradientFill = new LinearGradientBrush(p4, p3, gradientStops);
                                        }
                                    }
                                    else if (gradientDirection == 1)
                                    {
                                        Point M1;
                                        Point M2;

                                        if (transfNodePt[1] - transfParentPt[1] > 0)
                                        {
                                            M1 = p3;
                                            M2 = p4;
                                        }
                                        else
                                        {
                                            M1 = p4;
                                            M2 = p3;
                                        }

                                        Point m1;
                                        Point m2;

                                        if ((p2.X - p1.X) * (M2.Y - M1.Y) - (p2.Y - p1.Y) * (M2.X - M1.X) < 0)
                                        {
                                            m1 = p1;
                                            m2 = p2;
                                        }
                                        else
                                        {
                                            m1 = p2;
                                            m2 = p1;
                                        }

                                        gradientFill = new LinearGradientBrush(m1, m2, gradientStops);

                                    }
                                }
                            }
                        }
                    }
                    else if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out scale))
                    {
                        // Rectangular coordinates

                        double scaleMod = scale.Modulus();
                        scale = new Point(scale.X / scaleMod, scale.Y / scaleMod);
                        Point perpScale = new Point(-scale.Y, scale.X);
                        Point nodePoint = coordinates[node.Id];

                        double minX = double.MaxValue;
                        double minY = double.MaxValue;
                        double maxX = double.MinValue;
                        double maxY = double.MinValue;

                        foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                        {
                            Point pt = coordinates[child.Id];
                            double x = (pt.X - nodePoint.X) * scale.X + (pt.Y - nodePoint.Y) * scale.Y;
                            double y = (pt.X - nodePoint.X) * perpScale.X + (pt.Y - nodePoint.Y) * perpScale.Y;

                            minX = Math.Min(minX, x);
                            minY = Math.Min(minY, y);
                            maxX = Math.Max(maxX, x);
                            maxY = Math.Max(maxY, y);
                        }

                        minX -= marginLeft;
                        minY -= marginTop;
                        maxX += marginRight;
                        maxY += marginBottom;

                        pth.MoveTo(nodePoint.X + minX * scale.X + minY * perpScale.X, nodePoint.Y + minX * scale.Y + minY * perpScale.Y);
                        pth.LineTo(nodePoint.X + maxX * scale.X + minY * perpScale.X, nodePoint.Y + maxX * scale.Y + minY * perpScale.Y);
                        pth.LineTo(nodePoint.X + maxX * scale.X + maxY * perpScale.X, nodePoint.Y + maxX * scale.Y + maxY * perpScale.Y);
                        pth.LineTo(nodePoint.X + minX * scale.X + maxY * perpScale.X, nodePoint.Y + minX * scale.Y + maxY * perpScale.Y);
                        pth.Close();

                        if (gradient)
                        {
                            Point p1 = new Point();
                            Point p2 = new Point();

                            if (gradientDirection == 0)
                            {
                                p1 = new Point(nodePoint.X + minX * scale.X + (minY + maxY) * 0.5 * perpScale.X, nodePoint.Y + minX * scale.Y + (minY + maxY) * 0.5 * perpScale.Y);
                                p2 = new Point(nodePoint.X + maxX * scale.X + (minY + maxY) * 0.5 * perpScale.X, nodePoint.Y + maxX * scale.Y + (minY + maxY) * 0.5 * perpScale.Y);
                            }
                            else if (gradientDirection == 1)
                            {
                                p1 = new Point(nodePoint.X + (minX + maxX) * 0.5 * scale.X + minY * perpScale.X, nodePoint.Y + (minX + maxX) * 0.5 * scale.Y + minY * perpScale.Y);
                                p2 = new Point(nodePoint.X + (minX + maxX) * 0.5 * scale.X + maxY * perpScale.X, nodePoint.Y + (minX + maxX) * 0.5 * scale.Y + maxY * perpScale.Y);
                            }

                            gradientFill = new LinearGradientBrush(p1, p2, gradientStops);
                        }

                        updateMaxMin(new Point(nodePoint.X + minX * scale.X + minY * perpScale.X, nodePoint.Y + minX * scale.Y + minY * perpScale.Y));
                        updateMaxMin(new Point(nodePoint.X + maxX * scale.X + minY * perpScale.X, nodePoint.Y + maxX * scale.Y + minY * perpScale.Y));
                        updateMaxMin(new Point(nodePoint.X + maxX * scale.X + maxY * perpScale.X, nodePoint.Y + maxX * scale.Y + maxY * perpScale.Y));
                        updateMaxMin(new Point(nodePoint.X + minX * scale.X + maxY * perpScale.X, nodePoint.Y + minX * scale.Y + maxY * perpScale.Y));
                    }
                    else if (coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out scale))
                    {
                        // Circular coordinates

                        List<TreeNode> leaves = node.GetLeaves();

                        Point p1 = coordinates[leaves[0].Id];
                        Point p2 = coordinates[leaves[^1].Id];

                        double minR = double.MaxValue;
                        double maxR = 0;

                        foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                        {
                            Point pt = coordinates[child.Id];
                            double r = pt.Modulus();
                            double theta = Math.Atan2(pt.Y, pt.X);

                            minR = Math.Min(minR, r);
                            maxR = Math.Max(maxR, r);

                            updateMaxMin(coordinates[child.Id]);
                            updateMaxMin(new Point((r - marginLeft) * Math.Cos(theta), (r - marginLeft) * Math.Sin(theta)));
                            updateMaxMin(new Point((r + marginRight) * Math.Cos(theta), (r + marginRight) * Math.Sin(theta)));
                        }

                        double d1 = Math.Atan2(p1.Y, p1.X);
                        double d2 = Math.Atan2(p2.Y, p2.X);

                        if (d2 < d1)
                        {
                            d2 += 2 * Math.PI;
                        }

                        while (d1 < 0)
                        {
                            d1 += 2 * Math.PI;
                            d2 += 2 * Math.PI;
                        }

                        if (d2 > d1 + 2 * Math.PI)
                        {
                            d1 = 0.0001;
                            d2 = 2 * Math.PI + 0.0001;
                        }

                        minR -= marginLeft;
                        maxR += marginRight;
                        d1 -= marginTop / ((minR + maxR) * 0.5);
                        d2 += marginBottom / ((minR + maxR) * 0.5);

                        pth.MoveTo(minR * Math.Cos(d1), minR * Math.Sin(d1));
                        pth.Arc(new Point(0, 0), minR, d1, d2);
                        pth.LineTo(maxR * Math.Cos(d2), maxR * Math.Sin(d2));
                        pth.Arc(new Point(0, 0), maxR, d2, d1);
                        pth.Close();

                        if (gradient)
                        {
                            if (gradientDirection == 0 && maxR > 0)
                            {
                                gradientFill = new RadialGradientBrush(new Point(0, 0), new Point(0, 0), maxR, new VectSharp.GradientStop(gradientStartColour, 0), new VectSharp.GradientStop(gradientStartColour, minR / maxR), new VectSharp.GradientStop(midpointColour, minR / maxR + (1 - minR / maxR) * gradientMidPoint), new VectSharp.GradientStop(gradientEndColour, 1));
                            }
                            else if (gradientDirection == 1)
                            {
                                Point gp1 = new Point((minR + maxR) * 0.5 * Math.Cos(d1), (minR + maxR) * 0.5 * Math.Sin(d1));
                                Point gp2 = new Point((minR + maxR) * 0.5 * Math.Cos(d2), (minR + maxR) * 0.5 * Math.Sin(d2));

                                gradientFill = new LinearGradientBrush(gp1, gp2, gradientStops);
                            }
                        }

                        updateMaxMin(new Point(minR * Math.Cos(d1), minR * Math.Sin(d1)));
                        updateMaxMin(new Point(maxR * Math.Cos(d1), maxR * Math.Sin(d1)));
                        updateMaxMin(new Point(maxR * Math.Cos(d2), maxR * Math.Sin(d2)));
                        updateMaxMin(new Point(minR * Math.Cos(d2), minR * Math.Sin(d2)));
                    }

                    if (!gradient && fill.A > 0)
                    {
                        graphics.FillPath(pth, fill, tag: node.Id);
                    }
                    else if (gradient && (gradientStartColour.A > 0 || gradientEndColour.A > 0) && gradientFill != null)
                    {
                        graphics.FillPath(pth, gradientFill, tag: node.Id);
                    }

                    if (stroke.A > 0 && lineWidth > 0)
                    {
                        graphics.StrokePath(pth, stroke, lineWidth, lineDash: dash, lineJoin: LineJoins.Round, tag: node.Id);
                    }
                }

            }


            if (anyMaxMin)
            {
                return new Point[] { new Point(ptMinX - defaultLineWidth, ptMinY - defaultLineWidth), new Point(ptMaxX + defaultLineWidth, ptMaxY + defaultLineWidth) };
            }
            else
            {
                return new Point[] { new Point(), new Point() };
            }
        }


        private static bool Equal(this Point p1, Point p2)
        {
            return p1.X == p2.X && p1.Y == p2.Y;
        }

        private static double Determinant(Point p1, Point p2, Point x)
        {
            return (p2.X - p1.X) * (x.Y - p1.Y) - (p2.Y - p1.Y) * (x.X - p1.X);
        }

        private static bool IsCompatible(Point p1, Point p2, List<Point> points)
        {
            for (int i = 0; i < points.Count; i++)
            {
                if (Determinant(p1, p2, points[i]) < 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static List<Point> CreateConvexHull(List<Point> points)
        {
            List<Point> workingPoints = new List<Point>(points);
            workingPoints.Sort((a, b) => a.X == b.X ? a.Y.CompareTo(b.Y) : (a.X > b.X ? 1 : -1));

            List<Point> actualPoints = new List<Point>(workingPoints.Count);

            for (int i = 0; i < workingPoints.Count; i++)
            {
                if (actualPoints.Count == 0 || !actualPoints[^1].Equal(workingPoints[i]))
                {
                    actualPoints.Add(workingPoints[i]);
                }
            }

            workingPoints = actualPoints;

            List<Point> hull = new List<Point>() { workingPoints[0] };

            bool closed = false;

            while (!closed)
            {
                Point currentEnd = hull[^1];

                for (int i = 0; i < workingPoints.Count; i++)
                {
                    if (!currentEnd.Equal(workingPoints[i]))
                    {
                        if (IsCompatible(currentEnd, workingPoints[i], points))
                        {
                            hull.Add(workingPoints[i]);
                            break;
                        }
                    }
                }

                closed = hull[0].Equal(hull[^1]);
            }

            hull.RemoveAt(hull.Count - 1);

            return hull;
        }
    }
}
