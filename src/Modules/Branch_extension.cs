
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

namespace BranchExtensions
{
    /// <summary>
    /// This module can be used to "extend" the branches of the tree (e.g. to make sure that all the branches end at the same distance
    /// from the origin, despite having different lengths. The end position of the extension can be specified based on a fixed reference,
    /// or on the node's position.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// You can use this module to ensure that the branches of the tree all end up at the same distance from the origin, regardless of
    /// the branch lengths, when _Rectangular_ or _Circular_ coordinates are used.
    /// 
    /// To do so, you should set the [End anchor](#end-anchor) to `Origin`, and then set the [End](#end) offset to an appropriate point
    /// along the x axis (i.e. with a high enough value for the x coordinate and 0 for the y coordinate). Experiment with different
    /// value of the x coordinate to obtain the right distance.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Branch extensions";
        public const string HelpText = "Extends terminal branches.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const string Id = "fb385719-b376-49b0-8e99-aab7cf641966";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACFSURBVDhPpZI7EoAgDETB8XQ23oXeyp670Hg9nDBZBhOUj69JKBJ2k1hTwTkXOTXee7sdIb2vc6/mirJBi4cC+TOnnywcM72FQDUApAaKyCv8yjz9NiMdZAWjheDVQi8rx4xcYfMOymFN86eBskDIrXxaIKSCEUXqDsDsWhPDCiT9MwjxBkg2apg5bXDpAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC+SURBVEhL1ZXBDsIwCIbp4tN58V16N2q89128+HqzVGawFuYKPexLNkaTMfr/NAugEGO85nB5ZwAppXA8P2ZK4Xk/lfe1tQlvCp/iQ8g7mPGitIsfiWpZEJSGHjfTkshVFtEDS9ecA0WVxYfuKULdBUNvFLspX2sUdpPoywOvopy1g2ZGNFmSrcvkCrOxnMA73aUH04iuOepJzvLt5H+ARrfGUlrfQtnOWhGLT4tErrP/N24SSdTFR0yRUTqAF2omeii0IfLlAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADSSURBVFhH7ZUxDsIwDEVTxOlYuEt2BIg9d2HheqUpIUSugl3kbyPEk1orGZra/v4ZgpAY42kKx8fqRUpp/sbucB3njcLtshftb/JLyOJwU6YKjPkpSzXWVABCVwNcz7V4VwGTnrMtyBlrZ93iroFtiWLoJFj6wLlEVWpvrVRPaSvg4nSLFqBVT3Gfgv8PsD7A3YBIH4DMPWXoOZsV7hqo5Epw/Ubw/VPwhLsrEFNAwd4VnAZQGnHXQJ15aXbaPtFWwMT5Pub3NcDRy97SBwAaCeEOlbh6iimUHe4AAAAASUVORK5CYII=";

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
                ( "Position", "Group:5"),
                
                /// <param name="Orientation reference:">
                /// This parameter determines the direction of the branch extension. If the value is `Horizontal`, the branch extension
                /// is horizontal; if it is `Branch`, the extension follows the direction of the branch.
                /// </param>
                ( "Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),
                
                /// <param name="Start:">
                /// The offset of the start point of the branch extension, with respect to the terminal node from which the branch
                /// extension originates. The x coordinate corresponds to the reference axis (i.e. horizontal or along the branch),
                /// while the y coordinate corresponds to the direction perpendicular to this.
                /// </param>
                ( "Start:", "Point:[0,0]" ),
                
                /// <param name="End anchor:">
                /// The anchor for the end point of the branch extension. This determines the point from which the offset in [End](#end)
                /// is computed. If the selected value is `Node`, the anchor corresponds to the node from which the branch extension
                /// originates. If the value is `Origin`, this corresponds to the root node (if the Coordinates module
                /// is _Radial_ or _Circular_), or to the projection of the node along the direction of growth of the tree, onto a line that
                /// is perpendicular to this direction and passes through the root node, if the Coordinates module is _Rectangular_.
                /// </param>
                ( "End anchor:", "ComboBox:0[\"Node\",\"Origin\"]" ),
                
                /// <param name="Start:">
                /// The offset of the end point of the branch extension, with respect to the [End anchor](#end-anchor). The x coordinate
                /// corresponds to the reference axis (i.e. horizontal or along the branch), while the y coordinate corresponds to the
                /// direction perpendicular to this.
                /// </param>
                ( "End:", "Point:[0,0]" ),

                ( "Appearance", "Group:6"),
                
                /// <param name="Auto stroke colour by node">
                /// If this check box is checked, the colour of each branch extension is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible if the same tree
                /// is rendered multiple times.
                /// </param>
                ( "Auto stroke colour by node", "CheckBox:false"),
                
                /// <param name="Line opacity:">
                /// The opacity of the colour used if the [Auto stroke colour by node](#auto-stroke-colour-by-node) option is enabled.
                /// </param>
                ( "Line opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Line colour:">
                /// The colour used to draw the branch extension if the [Auto stroke colour by node](#auto-stroke-colour-by-node) option
                /// is disabled. The colour can be determined based on the value of an attribute of the nodes in the tree. For nodes that
                /// do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used.
                /// The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Line colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
                
                /// <param name="Line weight:">
                /// The thickness of the branch extensions. This can be determined based on the value of an attribute of the nodes in the tree. For nodes that
                /// do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used. The default attribute
                /// used to determine the thickness of the branches is `Thickness`.
                /// </param>
                ( "Line weight:", "NumericUpDownByNode:1[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"Thickness\",\"Number\",\"true\"]" ),
                
                /// <param name="Line cap:">
                /// The line cap to use when drawing the branch extensions.
                /// </param>
                ( "Line cap:", "ComboBox:1[\"Butt\",\"Round\",\"Square\"]" ),
                
                /// <param name="Line dash:">
                /// The line dash to use when drawing the branch extensions.
                /// </param>
                ( "Line dash:", "Dash:[0,0,0]"),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();

            if ((bool)currentParameterValues["Auto stroke colour by node"])
            {
                controlStatus.Add("Line opacity:", ControlStatus.Enabled);
                controlStatus.Add("Line colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Line opacity:", ControlStatus.Hidden);
                controlStatus.Add("Line colour:", ControlStatus.Enabled);
            }

            parametersToChange = new Dictionary<string, object>() { };

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
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

            int reference = (int)parameterValues["Orientation reference:"];

            int branchReference;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out _))
            {
                // Rectangular coordinates
                branchReference = 0;
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out _))
            {
                // Circular coordinates
                branchReference = 2;
            }
            else
            {
                // Radial coordinates
                branchReference = 1;
            }

            int endAnchor = (int)parameterValues["End anchor:"];

            Point deltaStart = (Point)parameterValues["Start:"];
            Point deltaEnd = (Point)parameterValues["End:"];

            bool autoStroke = (bool)parameterValues["Auto stroke colour by node"];
            double strokeOpacity = (double)parameterValues["Line opacity:"];

            ColourFormatterOptions Stroke = (ColourFormatterOptions)parameterValues["Line colour:"];
            Colour defaultStroke = Stroke.DefaultColour;
            Func<object, Colour?> strokeFormatter = Stroke.Formatter;

            NumberFormatterOptions WeightFO = (NumberFormatterOptions)parameterValues["Line weight:"];
            double defaultWeight = WeightFO.DefaultValue;
            Func<object, double?> weightFormatter = WeightFO.Formatter;

            LineCaps cap = (LineCaps)(int)parameterValues["Line cap:"];
            LineJoins join = (LineJoins)(2 - (int)parameterValues["Line cap:"]);
            LineDash dash = (LineDash)parameterValues["Line dash:"];

            Point rootPoint = coordinates[Modules.RootNodeId];
            coordinates.TryGetValue("", out Point circularCenter);

            foreach (TreeNode node in nodes)
            {
                if (node.Children.Count == 0)
                {
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
                        strokeColour = Modules.DefaultColours[Math.Abs(string.Join(",", node.GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(strokeOpacity);
                    }

                    double weight = defaultWeight;

                    if (node.Attributes.TryGetValue(WeightFO.AttributeName, out object weightAttributeObject) && weightAttributeObject != null)
                    {
                        weight = weightFormatter(weightAttributeObject) ?? defaultWeight;
                    }

                    Point startPoint = coordinates[node.Id];
                    Point angleStartPoint = startPoint;
                    double referenceStartAngle = 0;

                    if (reference == 0)
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

                                    double x = (m * (parentPoint.Y - startPoint.Y + m * startPoint.X) + parentPoint.X) / (m * m + 1);
                                    double y = parentPoint.Y - (x - parentPoint.X) / m;

                                    rectAnglePoint = new Point(x, y);
                                }
                                else if (Math.Abs(numerator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(startPoint.X, parentPoint.Y);
                                }
                                else if (Math.Abs(denominator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(parentPoint.X, startPoint.Y);
                                }
                                else
                                {
                                    rectAnglePoint = startPoint;
                                }

                                if (reference == 1)
                                {
                                    referenceStartAngle = Math.Atan2(startPoint.Y - rectAnglePoint.Y, startPoint.X - rectAnglePoint.X);
                                }
                            }
                            else
                            {
                                Point parentPoint = coordinates[Modules.RootNodeId];

                                if (reference == 1)
                                {
                                    referenceStartAngle = Math.Atan2(startPoint.Y - parentPoint.Y, startPoint.X - parentPoint.X);
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

                            if (reference == 1)
                            {
                                referenceStartAngle = Math.Atan2(startPoint.Y - parentPoint.Y, startPoint.X - parentPoint.X);
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

                            double myRadius = distance(startPoint, circularCenter);
                            double parentRadius = distance(parentPoint, circularCenter);

                            Point realElbowPoint = sumPoint(startPoint, multiplyPoint(subtractPoint(circularCenter, startPoint), (myRadius - parentRadius) / myRadius));

                            if (reference == 1)
                            {
                                referenceStartAngle = Math.Atan2(startPoint.Y - realElbowPoint.Y, startPoint.X - realElbowPoint.X);
                            }
                        }
                    }

                    Point endPoint = coordinates[node.Id];
                    Point angleEndPoint = startPoint;
                    double referenceEndAngle = 0;


                    if (reference == 0)
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

                                    double x = (m * (parentPoint.Y - endPoint.Y + m * endPoint.X) + parentPoint.X) / (m * m + 1);
                                    double y = parentPoint.Y - (x - parentPoint.X) / m;

                                    rectAnglePoint = new Point(x, y);
                                }
                                else if (Math.Abs(numerator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(endPoint.X, parentPoint.Y);
                                }
                                else if (Math.Abs(denominator) > 1e-5)
                                {
                                    rectAnglePoint = new Point(parentPoint.X, endPoint.Y);
                                }
                                else
                                {
                                    rectAnglePoint = endPoint;
                                }

                                if (reference == 1)
                                {
                                    referenceEndAngle = Math.Atan2(endPoint.Y - rectAnglePoint.Y, endPoint.X - rectAnglePoint.X);
                                }

                                if (endAnchor == 1)
                                {
                                    Point branchVector = new Point(Math.Cos(referenceEndAngle), Math.Sin(referenceEndAngle));

                                    double d = (rootPoint.X - endPoint.X) * branchVector.X + (rootPoint.Y - endPoint.Y) * branchVector.Y;

                                    Point proj = new Point(endPoint.X + d * branchVector.X, endPoint.Y + d * branchVector.Y);

                                    angleEndPoint = new Point(-endPoint.X + proj.X * 2, -endPoint.Y + proj.Y * 2);
                                }
                            }
                            else
                            {
                                Point parentPoint = coordinates[Modules.RootNodeId];

                                if (endAnchor == 1)
                                {
                                    angleEndPoint = new Point(-endPoint.X + parentPoint.X * 2, -endPoint.Y + parentPoint.Y * 2);
                                }

                                if (reference == 1)
                                {
                                    referenceEndAngle = Math.Atan2(endPoint.Y - parentPoint.Y, endPoint.X - parentPoint.X);
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

                            if (reference == 1)
                            {
                                referenceEndAngle = Math.Atan2(endPoint.Y - parentPoint.Y, endPoint.X - parentPoint.X);
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

                            double myRadius = distance(endPoint, circularCenter);
                            double parentRadius = distance(parentPoint, circularCenter);

                            Point realElbowPoint = sumPoint(endPoint, multiplyPoint(subtractPoint(circularCenter, endPoint), (myRadius - parentRadius) / myRadius));

                            if (endAnchor == 1)
                            {
                                angleEndPoint = new Point(-endPoint.X + circularCenter.X * 2, -endPoint.Y + circularCenter.Y * 2);
                            }

                            if (reference == 1)
                            {
                                referenceEndAngle = Math.Atan2(endPoint.Y - realElbowPoint.Y, endPoint.X - realElbowPoint.X);
                            }
                        }
                    }

                    if (double.IsNaN(referenceStartAngle))
                    {
                        referenceStartAngle = 0;
                    }

                    if (double.IsNaN(referenceEndAngle))
                    {
                        referenceEndAngle = 0;
                    }

                    startPoint = new Point((startPoint.X + angleStartPoint.X) * 0.5 + deltaStart.X * Math.Cos(referenceStartAngle) - deltaStart.Y * Math.Sin(referenceStartAngle), (startPoint.Y + angleStartPoint.Y) * 0.5 + deltaStart.X * Math.Sin(referenceStartAngle) + deltaStart.Y * Math.Cos(referenceStartAngle));
                    endPoint = new Point((endPoint.X + angleEndPoint.X) * 0.5 + deltaEnd.X * Math.Cos(referenceEndAngle) - deltaEnd.Y * Math.Sin(referenceEndAngle), (endPoint.Y + angleEndPoint.Y) * 0.5 + deltaEnd.X * Math.Sin(referenceEndAngle) + deltaEnd.Y * Math.Cos(referenceEndAngle));

                    updateMaxMin(startPoint);
                    updateMaxMin(endPoint);

                    if (strokeColour.A > 0 && weight > 0)
                    {
                        graphics.StrokePath(new GraphicsPath().MoveTo(endPoint).LineTo(startPoint), strokeColour, weight, cap, join, dash, tag: node.Id);
                    }
                }
            }

            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }
    }
}

