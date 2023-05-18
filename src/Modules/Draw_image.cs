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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.SVG;

namespace Draw_image
{
    /// <summary>
    /// This module can be used to draw an image file that has been loaded as an attachment.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// When the chosen [Image](#image) or the [Image format](#image-format) is changed, the [Image size](#image-size) text box
    /// is updated to show the original size of the image; this can be useful in determining an appropriate [Width](#width) and
    /// [Height](#height) for the image.
    /// 
    /// If the image is in SVG format, it is loaded as a vector image. Most features of SVG are supported, but not all of them,
    /// notably gradients are not supported. If you need to embed an image with unsupported features, you will have to convert it
    /// in one of the other formats.
    /// 
    /// Even though PDF and XPS are vector formats, images in these formats are rasterised before drawing them on the tree. The
    /// [Scale factor](#scale-factor) parameter can be used to determine the resolution of the rasterisation.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Draw image";
        public const string HelpText = "Draws an image.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const string Id = "a26abc43-1b12-40c9-9e23-bd3de2718829";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAChSURBVDhPYywqKvrPQAkAGUAuAOllgppDEHx9eJDh9eFmMAaxYYBoA749OgRlobKJNgAXINoALjk7KAuVjWHA0n03oCxUwC1vzyBqWwvGIDYMoBgA0rzswC2chmAThxsA0wwC2AzBZTjcAJhmGEBWjM9wvIEIUxztpAEVgQBky/AaAAIwQ7Y2+UFFUAFBA0AAnyFEGQACOA2hNDNRmJ0ZGAB8BdOyhXKg3QAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD6SURBVEhLtZYhDsIwFIYfCBIUIHAIBJILILgCnmvMY+AswAmwhATBBRCEIBA4BKBIZiB/6VvK1m7d2n7m9S2v/9dmYqtFUfShkEAQCmTXpceJ+Hml13FN9/1CVPSMF8H7dqD4cRFrVPSMnxvIcEbtvQganYFc/VB7L4Jmb5SEoqJn/Nyg3afWcErd8UxU9IwXQR5GwXJ7kis3tAKEr3ZnL5KMgMOBrSRv5k+ghjNFEt5jmkkEunDGFKDuMc0kAlM4kw7QHUgn0b5kE2qA6UBpSSkB4IDNfCKfZFHlpQXARsJUEgBbSWUBsJE4CUCRxFkAciWhP/qBf1uIvh3/VWmXLP3CAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEmSURBVFhH5Ze7DcIwEIYNJdS00LIEIDEAC7BFUtNAnTFgA1rEYwlEBy01tKDf8oFjHNt5HJHgk6IcJ/v+T8ZNGlEUPUSNNNW7Nl4nkCSJbHyLOI7lm/0EbueduB4W8kFtwiqAwPtlr34JWZsSrAJ6OGH2ar+ErAKt7lBVb8weq0C7N0oFokZPh/0vQGBnMJOPGQ5++w6E4BVYbo6q4sEpgPDV9sQqkSlA4aCIROh6q4AeTuSRoP0h6z8EbOFEyFB9f8j6lIArnHANte33SaQEfOGEbahL3iWReQl96ENd4USWRGEBQEOn477quLFJlBIANHQ9n6iOG/OkSguAvBI6lQiAohKVCYAiEpUKgLwSlQuAPBIsAiBUgk0AhEjU/mn271/HQjwBO4zOVKtbBG4AAAAASUVORK5CYII=";

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
                ( "Image file", "Group:4"),
                
                /// <param name="Image:">
                /// This parameter is used to choose the attachment that contains the image file.
                /// </param>
                ( "Image:", "Attachment:" ),
                
                /// <param name="Image format:">
                /// This parameter selects the format of the image. Choosing the wrong format will cause the image not to
                /// be drawn on the plot.
                /// </param>
                ( "Image format:", "ComboBox:0[\"SVG\",\"PDF\",\"XPS\",\"CBZ\",\"PNG\",\"JPEG\",\"BMP\",\"GIF\",\"TIFF\",\"PNM\",\"PAM\",\"EPUB\",\"FB2\"]"),
                
                /// <param name="Image size:">
                /// This (read-only) text box contains the original size (width x height) of the image file (this appears only if
                /// the selected image format is correct).
                /// </param>
                ( "Image size:", "TextBox:" ),

                /// <param name="Scale factor:">
                /// This parameter determines the scale factor to use when drawing a raster(ised) image. It has no effect if the
                /// image is in SVG format. For image formats such as PDF and XPS, this determines the rasterisation resolution;
                /// thus, increasing this value may lead to a sharper image being drawn.
                /// </param>
                ( "Scale factor:", "NumericUpDown:1[\"0.001\",\"Infinity\"]" ),

                ( "Position", "Group:4"),
                
                /// <param name="Node:">
                /// This parameter determines the node used as an anchor for positioning the image. If only a
                /// single node is selected, the anchor corresponds to that node. If more than one node is
                /// selected, the anchor corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Anchor:">
                /// This parameter determines the anchor for the centre of the image. If the value is `Node`, the centre of the image is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the centre of the image is aligned with the midpoint of the branch connecting the node to its parent.
                /// If the value is `Centre of leaves` or `Origin`, the alignment depends on the current Coordinates module:
                /// 
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | Coordinates module                 | Centre of leaves                                                       | Origin                                                                 |
                /// +====================================+========================================================================+========================================================================+
                /// | _Rectangular_                      | The smallest rectangle containing all the leaves that descend from the | A point corresponding to the projection of the node on a line          |
                /// |                                    | current node is computed. The anchor corresponds to the centre of this | perpedicular to the direction in which the tree expands and passing    |
                /// |                                    | rectangle.                                                             | through the root node. Usually (i.e. if the tree is horizontal), this  |
                /// |                                    |                                                                        | means a point with the same horizontal coordinate as the root node and |
                /// |                                    |                                                                        | the same vertical coordinate as the current node.                      |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | _Radial_                           | The smallest rectangle containing all the leaves that descend from the | The root node.                                                         |
                /// |                                    | current node is computed. The anchor corresponds to the centre of this |                                                                        |
                /// |                                    | rectangle.                                                             |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// | _Circular_                         | The centre of leaves is computed using polar coordinates: the minimum  | The root node.                                                         |
                /// |                                    | and maximum distance of the leaves that descend from the current node  |                                                                        |
                /// |                                    | are computed, as well as the minimum and maximum angle. The anchor has |                                                                        |
                /// |                                    | a distance corresponding to the average of the minimum and maximum     |                                                                        |
                /// |                                    | distance, and an angle corresponding to the average of the maximum and |                                                                        |
                /// |                                    | minimum angle.                                                         |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+------------------------------------------------------------------------+
                /// </param>
                ( "Anchor:", "ComboBox:0[\"Node\",\"Mid-branch\",\"Centre of leaves\",\"Origin\"]" ),
                
                /// <param name="Orientation reference:">
                /// This parameter determines the direction along which the offset of the centre of the image from the anchor is computed. If the value is `Horizontal`,
                /// the offset `X` coordinate of the offset corresponds to an horizontal displacement and the `Y` coordinate to a vertical displacement; if the value is
                /// `Branch`, the `X` coordinate corresponds to a shift in the direction of the branch, while the `Y` coordinate corresponds to a shift in a direction
                /// perpendicular to the branch.
                /// </param>
                ( "Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),
                
                /// <param name="Position:">
                /// This parameter determines how shifted from the anchor point the image is. The `X` coordinate corresponds to the line determined by the [Orientation reference](#orientation-reference);
                /// the `Y` coordinate corresponds to the line perpendicular to this.
                /// </param>
                ( "Position:", "Point:[0,0]" ),

                ( "Size", "Group:2"),
                
                /// <param name="Width:">
                /// The width of the image in plot units. For best results, the ratio between width and [Height](#height) should be the same as in the original size of
                /// the image shown in the [Image size](#image-size) text box.
                /// </param>
                ( "Width:", "NumericUpDown:100[\"0\",\"Infinity\"]" ),
                
                /// <param name="Height:">
                /// The height of the image in plot units. For best results, the ratio between [Width](#width) and height should be the same as in the original size of
                /// the image shown in the [Image size](#image-size) text box.
                /// </param>
                ( "Height:", "NumericUpDown:100[\"0\",\"Infinity\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            controlStatus["Scale factor:"] = ((int)currentParameterValues["Image format:"] != 0) ? ControlStatus.Enabled : ControlStatus.Hidden;

            if (currentParameterValues["Image:"] == null)
            {
                controlStatus["Image size:"] = ControlStatus.Hidden;
            }

            if (currentParameterValues["Image:"] != previousParameterValues["Image:"] || (int)currentParameterValues["Image format:"] != (int)previousParameterValues["Image format:"])
            {
                controlStatus["Image size:"] = ControlStatus.Hidden;

                int format = (int)currentParameterValues["Image format:"];
                bool isSvg = format == 0;

                Attachment image = (Attachment)currentParameterValues["Image:"];

                try
                {
                    if (image != null)
                    {
                        byte[] imageBytes = image.GetBytes();

                        if (isSvg)
                        {
                            Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(Parser.ParseSVGURI);

                            Page imagePage;
                            using (MemoryStream ms = new MemoryStream(imageBytes))
                            {
                                imagePage = Parser.FromStream(ms);
                            }

                            parametersToChange.Add("Image size:", imagePage.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x" + imagePage.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            MuPDFCore.InputFileTypes imageType = (MuPDFCore.InputFileTypes)(format - 1);

                            VectSharp.MuPDFUtils.RasterImageStream imageStream;

                            IntPtr imagePtr = Marshal.AllocHGlobal(imageBytes.Length);
                            Marshal.Copy(imageBytes, 0, imagePtr, imageBytes.Length);

                            try
                            {
                                imageStream = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, imageBytes.Length, imageType);
                            }
                            catch (Exception ex)
                            {
                                throw ex.InnerException;
                            }
                            finally
                            {
                                Marshal.FreeHGlobal(imagePtr);
                            }

                            parametersToChange.Add("Image size:", imageStream.Width.ToString(System.Globalization.CultureInfo.InvariantCulture) + "x" + imageStream.Height.ToString(System.Globalization.CultureInfo.InvariantCulture));

                            imageStream.Dispose();
                        }
                    }

                    controlStatus["Image size:"] = ControlStatus.Disabled;
                }
                catch
                {
                    controlStatus["Image size:"] = ControlStatus.Hidden;
                }
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
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

            int format = (int)parameterValues["Image format:"];
            bool isSvg = format == 0;

            double width = (double)parameterValues["Width:"];
            double height = (double)parameterValues["Height:"];
            double scaleFactor = (double)parameterValues["Scale factor:"];

            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            if (node == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            int anchor = (int)parameterValues["Anchor:"];
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

            Point delta = (Point)parameterValues["Position:"];

            Point rootPoint = coordinates[Modules.RootNodeId];
            coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out Point circularCenter);

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
                            anglePoint = point;
                        }
                        else if (anchor == 3)
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


                        if (reference == 1)
                        {
                            referenceAngle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                        }

                        if (anchor == 1)
                        {
                            anglePoint = parentPoint;
                        }
                        else if (anchor == 2)
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
                            anglePoint = point;
                        }
                        else if (anchor == 3)
                        {
                            anglePoint = new Point(-point.X + parentPoint.X * 2, -point.Y + parentPoint.Y * 2);
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
                    else if (anchor == 2)
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
                        anglePoint = point;
                    }
                    else if (anchor == 3)
                    {
                        point = coordinates[Modules.RootNodeId];
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
                        anglePoint = point;
                        realElbowPoint = circularCenter;
                    }
                    else if (anchor == 3)
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

            point = new Point(point.X + delta.X * Math.Cos(referenceAngle) - delta.Y * Math.Sin(referenceAngle), point.Y + delta.X * Math.Sin(referenceAngle) + delta.Y * Math.Cos(referenceAngle));

            Attachment image = (Attachment)parameterValues["Image:"];

            if (image != null && width > 0 && height > 0)
            {
                byte[] imageBytes = image.GetBytes();

                if (isSvg)
                {
                    Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(Parser.ParseSVGURI);

                    Page imagePage;
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        imagePage = Parser.FromStream(ms);
                    }

                    graphics.Save();
                    graphics.Translate(point.X - width * 0.5, point.Y - height * 0.5);
                    graphics.Scale(width / imagePage.Width, height / imagePage.Height);

                    graphics.DrawGraphics(0, 0, imagePage.Graphics);

                    graphics.Restore();
                }
                else
                {
                    MuPDFCore.InputFileTypes imageType = (MuPDFCore.InputFileTypes)(format - 1);

                    VectSharp.MuPDFUtils.RasterImageStream imageStream;

                    IntPtr imagePtr = Marshal.AllocHGlobal(imageBytes.Length);
                    Marshal.Copy(imageBytes, 0, imagePtr, imageBytes.Length);

                    try
                    {
                        imageStream = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, imageBytes.Length, imageType, scale: scaleFactor);
                    }
                    catch (Exception ex)
                    {
                        throw ex.InnerException;
                    }
                    finally
                    {
                        Marshal.FreeHGlobal(imagePtr);
                    }

                    graphics.DrawRasterImage(point.X - width * 0.5, point.Y - height * 0.5, width, height, imageStream);
                }
            }

            Point topLeft = new Point(point.X - width * 0.5, point.Y - height * 0.5);
            Point bottomRight = new Point(point.X + width * 0.5, point.Y + height * 0.5);
            return new Point[] { topLeft, bottomRight };
        }
    }
}
