using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Formats.Tar;
using VectSharp.SVG;

namespace a86fab195286343edbbf5ed12a3cc3994
{
    /// <summary>
    /// This module can be used to draw images at multiple nodes, loading them from an archive loaded from an attachment.
    /// </summary>
    ///
    /// <description>
    /// ## Further information
    /// 
    /// All images within the attachment must be in the same format. If they are in SVG format, they are loaded as vector
    /// images (most features are supported, but there are some that are not), other formats are loaded as raster images.
    /// 
    /// Even though PDF and XPS are vector formats, images in these formats are rasterised before being drawn on the tree.
    /// The [Scale factor](#scale-factor) parameter can be used to determine the resolution of the rasterisation.
    /// 
    /// The [Attachment](#attachment) should be an archive in ZIP, TAR or TAR.GZ format (detected automatically). Files
    /// within the archive can be organised in subfolders, but the [Image name](#image-name) attribute needs to include
    /// the full path to each file.
    /// </description>
    public static class MyModule
    {
        public const string Name = "Node images";
        public const string HelpText = "Draws images at multiple nodes.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;
        public const string Id = "86fab195-2863-43ed-bbf5-ed12a3cc3994";

        // These variables hold a PNG icon at three resolutions (16x16px, 24x24px and 32x32px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAMAAAAoLQ9TAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAASUExURXJycv///+rCgqS910p9sQAAAPPrDQgAAAAGdFJOU///////ALO/pL8AAAAJcEhZcwAADsMAAA7DAcdvqGQAAAAYdEVYdFNvZnR3YXJlAFBhaW50Lk5FVCA1LjEuMvu8A7YAAAC2ZVhJZklJKgAIAAAABQAaAQUAAQAAAEoAAAAbAQUAAQAAAFIAAAAoAQMAAQAAAAIAAAAxAQIAEAAAAFoAAABphwQAAQAAAGoAAAAAAAAAYAAAAAEAAABgAAAAAQAAAFBhaW50Lk5FVCA1LjEuMgADAACQBwAEAAAAMDIzMAGgAwABAAAAAQAAAAWgBAABAAAAlAAAAAAAAAACAAEAAgAEAAAAUjk4AAIABwAEAAAAMDEwMAAAAADp1fY4ytpsegAAAEpJREFUKFN9zzkSwDAIBEFZwP+/7D0oCSeesDeBVWjNBM+J0Isy9IgIM0OP6AN7/0CYLqQlLkgiB0AiBYwAMfBuAYW74ORHZ1VVL56lAViFKcOeAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAMAAADXqc3KAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAVUExURXJycv////TgwOrCgqS910p9sQAAAKogHYYAAAAHdFJOU////////wAaSwNGAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAGHRFWHRTb2Z0d2FyZQBQYWludC5ORVQgNS4xLjL7vAO2AAAAtmVYSWZJSSoACAAAAAUAGgEFAAEAAABKAAAAGwEFAAEAAABSAAAAKAEDAAEAAAACAAAAMQECABAAAABaAAAAaYcEAAEAAABqAAAAAAAAAGAAAAABAAAAYAAAAAEAAABQYWludC5ORVQgNS4xLjIAAwAAkAcABAAAADAyMzABoAMAAQAAAAEAAAAFoAQAAQAAAJQAAAAAAAAAAgABAAIABAAAAFI5OAACAAcABAAAADAxMDAAAAAA6dX2OMrabHoAAABsSURBVChTldFBDgAhCANAV9D/P1kpKESMyfbAoQNeLB0pOQbfkTfYbcgC29p5Q84C29q5QSWqV6CZf5CfYvSaCNyCBODWTGQ6SK+CSwf0InqZYHYyrY2gAsBfOECuIIJ3EkzBusKRDSlS9z4AIu4ET1GudZsAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAMAAABEpIrGAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAVUExURXJycv////TgwOrCgqS910p9sQAAAKogHYYAAAAHdFJOU////////wAaSwNGAAAACXBIWXMAAA7DAAAOwwHHb6hkAAAAGHRFWHRTb2Z0d2FyZQBQYWludC5ORVQgNS4xLjL7vAO2AAAAtmVYSWZJSSoACAAAAAUAGgEFAAEAAABKAAAAGwEFAAEAAABSAAAAKAEDAAEAAAACAAAAMQECABAAAABaAAAAaYcEAAEAAABqAAAAAAAAAGAAAAABAAAAYAAAAAEAAABQYWludC5ORVQgNS4xLjIAAwAAkAcABAAAADAyMzABoAMAAQAAAAEAAAAFoAQAAQAAAJQAAAAAAAAAAgABAAIABAAAAFI5OAACAAcABAAAADAxMDAAAAAA6dX2OMrabHoAAACKSURBVDhPndJBDsMgDETRgA33P3IwHlAgxpD8RVp1nqwsemV0rcJ+AoLRKainjL4AXBw6Bau+AFwc8kCk6IJIVIQDyk7kge2F7TtoPwDXZ+8FODWhnzPglCAgJyC7iiZHoLssXfZRCvVXifGVV6ALAP0zPUETawCB8xZQ4YEqcF4SYDQAO+w7kPMNIzUGsP0IGSsAAAAASUVORK5CYII=";

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

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {

            };
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "StateData", "InstanceStateData:" ),

                ( "Image data", "Group:3"),

                /// <param name="Attachment:">
                /// This parameter is used to specify the attachment containing the image files. This should be an archive in ZIP, TAR or TAR.GZ format that contains the images.
                /// </param>
                ( "Attachment:", "Attachment:" ),
                
                /// <param name="Image format:">
                /// This parameter selects the format of the images. Choosing the wrong format will cause the images not to
                /// be drawn on the plot. All images must be in the same format.
                /// </param>
                ( "Image format:", "ComboBox:0[\"SVG\",\"PDF\",\"XPS\",\"CBZ\",\"PNG\",\"JPEG\",\"BMP\",\"GIF\",\"TIFF\",\"PNM\",\"PAM\",\"EPUB\",\"FB2\"]"),

                /// <param name="Scale factor:">
                /// This parameter determines the scale factor to use when drawing a raster(ised) image. It has no effect if the
                /// image is in SVG format. For image formats such as PDF and XPS, this determines the rasterisation resolution;
                /// thus, increasing this value may lead to a sharper image being drawn.
                /// </param>
                ( "Scale factor:", "NumericUpDown:1[\"0.001\",\"Infinity\"]" ),

                /// <param name="Image name:">
                /// This parameter specifies the attribute that contains the image names for each node. The image name should be the name of a file within the [Attachment](#attachment)
                /// file; the file extension can be omitted, as long as it is exactly the same as the specified [Image format](#image-format) (i.e., if the attribute value is `file` and
                /// the image format is `JPEG`, then `file.jpeg` will work, but `file.jpg` will not). Subfolders within the archive are supported, but the attribute values should include
                /// the full path to the file, with `/` as the directory separator (for example, if the archive contains a folder called `images`, which contains a file called `taxon.svg`,
                /// the attribute value should be `images/taxon.svg` or `images/taxon`).
                /// </param>
                ( "Image name:", "AttributeSelector:Name" ),

                ( "Position", "Group:4"),

                /// <param name="Anchor:">
                /// This parameter determines the anchor for the image. If the value is `Node`, the image is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the image is aligned with the midpoint of the branch connecting the node to its parent.
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
                /// This parameter determines the direction along which the offset of the image from the anchor is computed. If the value is `Horizontal`,
                /// the `X` coordinate of the offset corresponds to an horizontal displacement and the `Y` coordinate to a vertical displacement; if the value is
                /// `Branch`, the `X` coordinate corresponds to a shift in the direction of the branch, while the `Y` coordinate corresponds to a shift in a direction
                /// perpendicular to the branch.
                /// </param>
                ( "Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),

                /// <param name="Rotation:">
                /// This parameter specifies how much the image should be rotated compared to the [reference](#orientation-reference). The rotation always happens
                /// around the centre of each image.
                /// </param>
                ( "Rotation:", "Slider:0[\"0\",\"360\",\"0°\"]" ),

                /// <param name="Alignment:">
                /// This parameter controls to which point on the image the selected [Anchor](#anchor) corresponds.
                /// </param>
                ( "Alignment:","ComboBox:4[\"Top-left\",\"Top-center\",\"Top-right\",\"Middle-left\",\"Middle-center\",\"Middle-right\",\"Bottom-left\",\"Bottom-center\",\"Bottom-right\"]" ),

                ( "Position shift", "Group:2" ),

                /// <param name="X:">
                /// This parameter determines how much the image is shifted from the anchor point along the direction determined by the [Orientation reference](#orientation-reference).
                /// </param>
                ( "X:", "NumericUpDownByNode:0[\"-Infinity\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"ImageX\",\"Number\",\"true\"]" ),

                /// <param name="Y:">
                /// This parameter determines how much the image is shifted from the anchor point along the direction perpendicular to the one determined by the [Orientation reference](#orientation-reference).
                /// </param>
                ( "Y:", "NumericUpDownByNode:0[\"-Infinity\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"ImageY\",\"Number\",\"true\"]" ),

                ( "Scale (%)", "Group:2"),

                /// <param name="Width:">
                /// This parameter determines how much the image is stretched along the horizontal axis.
                /// </param>
                ( "Width:", "NumericUpDownByNode:100[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"ImageWidth\",\"Number\",\"true\"]" ),

                /// <param name="Height:">
                /// This parameter determines how much the image is stretched along the vertical axis.
                /// </param>
                ( "Height:", "NumericUpDownByNode:100[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"ImageHeight\",\"Number\",\"true\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            controlStatus["Scale factor:"] = ((int)currentParameterValues["Image format:"] != 0) ? ControlStatus.Enabled : ControlStatus.Hidden;

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


            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];
            Attachment attachment = (Attachment)parameterValues["Attachment:"];
            int imageFormat = (int)parameterValues["Image format:"];
            string attribute = (string)parameterValues["Image name:"];
            int anchor = (int)parameterValues["Anchor:"];
            int reference = (int)parameterValues["Orientation reference:"];
            int alignment = (int)parameterValues["Alignment:"];
            double rotation = (double)parameterValues["Rotation:"] * Math.PI / 180;
            double scaleFactor = (double)parameterValues["Scale factor:"];

            NumberFormatterOptions imageXFO = (NumberFormatterOptions)parameterValues["X:"];
            NumberFormatterOptions imageYFO = (NumberFormatterOptions)parameterValues["Y:"];

            NumberFormatterOptions scaleXFO = (NumberFormatterOptions)parameterValues["Width:"];
            NumberFormatterOptions scaleYFO = (NumberFormatterOptions)parameterValues["Height:"];


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

            Point rootPoint = coordinates[Modules.RootNodeId];
            coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out Point circularCenter);

            // Annoying issue: while for ZIP archives we can just request a file from its name, tar(.gz) archives are sequential.
            // Hence, we first need to figure out all the images that need to be extracted, and then extract them.
            Dictionary<string, List<TreeNode>> imageNames = new Dictionary<string, List<TreeNode>>();

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (node.Attributes.TryGetValue(attribute, out object imageNameObj))
                {
                    string imageName = null;

                    if (imageNameObj is string imageNameStr)
                    {
                        imageName = imageNameStr;
                    }
                    else if (imageNameObj is double imageNameDbl)
                    {
                        imageName = imageNameDbl.ToString();
                    }

                    if (imageName != null)
                    {
                        if (!imageNames.TryGetValue(imageName, out List<TreeNode> list))
                        {
                            list = new List<TreeNode>();
                            imageNames.Add(imageName, list);
                        }

                        list.Add(node);
                    }
                }
            }

            (Dictionary<string, Page> loadedImages, bool unknownFormat) = CreateImageCache(attachment, imageNames, imageFormat, scaleFactor, stateData);

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            bool anyDrawn = false;

            foreach (KeyValuePair<string, List<TreeNode>> kvp in imageNames)
            {
                if (loadedImages.TryGetValue(kvp.Key, out Page imagePage) && imagePage != null)
                {
                    foreach (TreeNode node in kvp.Value)
                    {
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

                        double deltaX = imageXFO.DefaultValue;
                        if (node.Attributes.TryGetValue(imageXFO.AttributeName, out object xObj) && xObj != null)
                        {
                            deltaX = imageXFO.Formatter(xObj) ?? imageXFO.DefaultValue;
                        }

                        double deltaY = imageYFO.DefaultValue;
                        if (node.Attributes.TryGetValue(imageYFO.AttributeName, out object yObj) && yObj != null)
                        {
                            deltaY = imageYFO.Formatter(yObj) ?? imageYFO.DefaultValue;
                        }

                        point = new Point(point.X + deltaX * Math.Cos(referenceAngle) - deltaY * Math.Sin(referenceAngle), point.Y + deltaX * Math.Sin(referenceAngle) + deltaY * Math.Cos(referenceAngle));

                        double scaleX = scaleXFO.DefaultValue;
                        if (node.Attributes.TryGetValue(scaleXFO.AttributeName, out object scaleXObj) && scaleXObj != null)
                        {
                            scaleX = scaleXFO.Formatter(scaleXObj) ?? scaleXFO.DefaultValue;
                        }

                        double scaleY = scaleYFO.DefaultValue;
                        if (node.Attributes.TryGetValue(scaleYFO.AttributeName, out object scaleYObj) && scaleYObj != null)
                        {
                            scaleY = scaleYFO.Formatter(scaleYObj) ?? scaleYFO.DefaultValue;
                        }

                        scaleX /= 100;
                        scaleY /= 100;

                        if (scaleX > 0 && scaleY > 0)
                        {
                            Point topLeftPt = point;

                            switch (alignment)
                            {
                                case 0:
                                    break;
                                case 1:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * 0.5 * scaleX, 0));
                                    break;
                                case 2:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * scaleX, 0));
                                    break;
                                case 3:
                                    topLeftPt = sumPoint(topLeftPt, new Point(0, -imagePage.Height * 0.5 * scaleY));
                                    break;
                                case 4:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * 0.5 * scaleX, -imagePage.Height * 0.5 * scaleY));
                                    break;
                                case 5:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * scaleX, -imagePage.Height * 0.5 * scaleY));
                                    break;
                                case 6:
                                    topLeftPt = sumPoint(topLeftPt, new Point(0, -imagePage.Height * scaleY));
                                    break;
                                case 7:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * 0.5 * scaleX, -imagePage.Height * scaleY));
                                    break;
                                case 8:
                                    topLeftPt = sumPoint(topLeftPt, new Point(-imagePage.Width * scaleX, -imagePage.Height * scaleY));
                                    break;
                            }

                            Point centerPt = sumPoint(topLeftPt, new Point(imagePage.Width * scaleX * 0.5, imagePage.Height * scaleY * 0.5));

                            topLeftPt = sumPoint(centerPt, new Point(-imagePage.Width * scaleX * 0.5 * Math.Cos(referenceAngle + rotation) + imagePage.Height * scaleY * 0.5 * Math.Sin(referenceAngle + rotation), -imagePage.Width * scaleX * 0.5 * Math.Sin(referenceAngle + rotation) - imagePage.Height * scaleY * 0.5 * Math.Cos(referenceAngle + rotation)));
                            Point topRightPt = sumPoint(centerPt, new Point(imagePage.Width * scaleX * 0.5 * Math.Cos(referenceAngle + rotation) + imagePage.Height * scaleY * 0.5 * Math.Sin(referenceAngle + rotation), imagePage.Width * scaleX * 0.5 * Math.Sin(referenceAngle + rotation) - imagePage.Height * scaleY * 0.5 * Math.Cos(referenceAngle + rotation)));
                            Point bottomLeftPt = sumPoint(centerPt, new Point(-imagePage.Width * scaleX * 0.5 * Math.Cos(referenceAngle + rotation) - imagePage.Height * scaleY * 0.5 * Math.Sin(referenceAngle + rotation), -imagePage.Width * scaleX * 0.5 * Math.Sin(referenceAngle + rotation) + imagePage.Height * scaleY * 0.5 * Math.Cos(referenceAngle + rotation)));
                            Point bottomRightPt = sumPoint(centerPt, new Point(imagePage.Width * scaleX * 0.5 * Math.Cos(referenceAngle + rotation) - imagePage.Height * scaleY * 0.5 * Math.Sin(referenceAngle + rotation), imagePage.Width * scaleX * 0.5 * Math.Sin(referenceAngle + rotation) + imagePage.Height * scaleY * 0.5 * Math.Cos(referenceAngle + rotation)));

                            graphics.Save();
                            graphics.Translate(centerPt);
                            graphics.Rotate(referenceAngle + rotation);
                            graphics.Scale(scaleX, scaleY);
                            graphics.DrawGraphics(-imagePage.Width * 0.5, -imagePage.Height * 0.5, imagePage.Graphics, tag: node.Id);
                            graphics.Restore();

                            minX = Math.Min(minX, Math.Min(Math.Min(topLeftPt.X, topRightPt.X), Math.Min(bottomLeftPt.X, bottomRightPt.X)));
                            minY = Math.Min(minY, Math.Min(Math.Min(topLeftPt.Y, topRightPt.Y), Math.Min(bottomLeftPt.Y, bottomRightPt.Y)));

                            maxX = Math.Max(maxX, Math.Max(Math.Max(topLeftPt.X, topRightPt.X), Math.Max(bottomLeftPt.X, bottomRightPt.X)));
                            maxY = Math.Max(maxY, Math.Max(Math.Max(topLeftPt.Y, topRightPt.Y), Math.Max(bottomLeftPt.Y, bottomRightPt.Y)));

                            anyDrawn = true;
                        }
                    }
                }
            }

            Point topLeft = new Point();
            Point bottomRight = new Point();

            if (anyDrawn)
            {
                topLeft = new Point(minX, minY);
                bottomRight = new Point(maxX, maxY);
            }

            return new Point[] { topLeft, bottomRight };
        }

        private static readonly Dictionary<string, Page> CachedImages = new Dictionary<string, Page>();
        private static readonly HashSet<string> AttachmentsInCache = new HashSet<string>();

        // Create the image cache from the attachment.
        private static (Dictionary<string, Page>, bool unknownFormat) CreateImageCache(Attachment attachment, Dictionary<string, List<TreeNode>> imageNames, int imageFormat, double scaleFactor, InstanceStateData stateData)
        {
            Dictionary<string, Page> tbr = new Dictionary<string, Page>();

            bool unknownFormat = false;

            string extension = imageFormat == 0 ? "svg" : ((MuPDFCore.InputFileTypes)(imageFormat - 1)).ToString().ToLowerInvariant();

            if (imageFormat == 0)
            {
                scaleFactor = 1;
            }

            lock (CachedImages)
            {
                List<string> attachmentsToUnCache = new List<string>();

                foreach (string att in AttachmentsInCache)
                {
                    if (!stateData.Attachments.Any(x => x.Value.Id == att))
                    {
                        attachmentsToUnCache.Add(att);
                    }
                }

                for (int i = 0; i < attachmentsToUnCache.Count; i++)
                {
                    string[] itemsToRemove = CachedImages.Keys.Where(x => x.StartsWith(attachmentsToUnCache[i] + "://")).ToArray();

                    foreach (string it in itemsToRemove)
                    {
                        CachedImages.Remove(it);
                    }
                }

                if (attachment == null)
                {
                    return (tbr, unknownFormat);
                }

                List<string> imagesToSeek = imageNames.Keys.ToList();


                foreach (string image in imagesToSeek)
                {
                    if (CachedImages.TryGetValue(attachment.Id + "://" + scaleFactor.ToString() + "//" + image, out Page pag))
                    {
                        tbr.Add(image, pag);
                    }
                }

                foreach (KeyValuePair<string, Page> kvp in tbr)
                {
                    imagesToSeek.Remove(kvp.Key);
                }

                Stream attachmentStream = attachment.Stream;
                attachmentStream.Seek(attachment.StreamStart, SeekOrigin.Begin);

                int b0 = attachmentStream.ReadByte();
                int b1 = attachmentStream.ReadByte();

                int b257 = -1;
                int b258 = -1;

                if (attachment.StreamLength > 258)
                {
                    attachmentStream.Seek(attachment.StreamStart + 257, SeekOrigin.Begin);
                    b257 = attachmentStream.ReadByte();
                    b258 = attachmentStream.ReadByte();
                }

                if (b0 == 0x50 && b1 == 0x4B) // ZIP
                {
                    attachmentStream.Seek(attachment.StreamStart, SeekOrigin.Begin);
                    using (ZipArchive archive = new ZipArchive(attachmentStream, ZipArchiveMode.Read, true))
                    {
                        for (int i = 0; i < imagesToSeek.Count; i++)
                        {
                            ZipArchiveEntry entry = archive.GetEntry(imagesToSeek[i]);

                            if (entry == null)
                            {
                                entry = archive.GetEntry(imagesToSeek[i] + "." + extension);
                            }

                            if (entry == null)
                            {
                                entry = archive.GetEntry(imagesToSeek[i] + "." + extension.ToUpperInvariant());
                            }

                            if (entry != null)
                            {
                                Page image = null;

                                try
                                {
                                    using (Stream imageStream = entry.Open())
                                    {
                                        image = ParseImage(imageStream, imageFormat, scaleFactor);
                                    }
                                }
                                catch { }

                                if (image != null)
                                {
                                    tbr.Add(imagesToSeek[i], image);

                                    if (attachment.CacheResults)
                                    {
                                        CachedImages[attachment.Id + "://" + scaleFactor.ToString() + "//" + imagesToSeek[i]] = image;
                                        AttachmentsInCache.Add(attachment.Id);
                                    }
                                }
                            }
                        }
                    }
                }
                else if ((b257 == 0x75 && b258 == 0x73) || (b0 == 0x1F && b1 == 0x8B)) // TAR or GZ
                {
                    attachmentStream.Seek(attachment.StreamStart, SeekOrigin.Begin);

                    Stream tarStream;
                    bool wasCompressed = false;

                    if (b0 == 0x1F && b1 == 0x8B) // GZ
                    {
                        tarStream = new GZipStream(attachmentStream, CompressionMode.Decompress, true);
                        wasCompressed = true;
                    }
                    else // TAR
                    {
                        tarStream = attachmentStream;
                    }

                    try
                    {
                        using (TarReader reader = new TarReader(tarStream, true))
                        {
                            TarEntry entry = reader.GetNextEntry(wasCompressed);

                            while (entry != null && imagesToSeek.Count > 0)
                            {
                                string found = null;

                                if (imagesToSeek.Contains(entry.Name))
                                {
                                    found = entry.Name;
                                }
                                else if (Path.GetExtension(entry.Name).ToLower() == "." + extension && imagesToSeek.Contains(Path.Combine(Path.GetDirectoryName(entry.Name), Path.GetFileNameWithoutExtension(entry.Name)).Replace(Path.DirectorySeparatorChar, '/')))
                                {
                                    found = Path.Combine(Path.GetDirectoryName(entry.Name), Path.GetFileNameWithoutExtension(entry.Name)).Replace(Path.DirectorySeparatorChar, '/');
                                }

                                if (found != null)
                                {
                                    Page image = null;

                                    try
                                    {
                                        image = ParseImage(entry.DataStream, imageFormat, scaleFactor);
                                    }
                                    catch { }

                                    if (image != null)
                                    {
                                        tbr.Add(found, image);
                                        imagesToSeek.Remove(found);

                                        if (attachment.CacheResults)
                                        {
                                            CachedImages[attachment.Id + "://" + scaleFactor.ToString() + "//" + found] = image;
                                            AttachmentsInCache.Add(attachment.Id);
                                        }
                                    }
                                }

                                try
                                {
                                    entry = reader.GetNextEntry(wasCompressed);
                                }
                                catch (Exception ex)
                                {
                                    Exception ex2 = ex;
                                    entry = null;
                                }
                            }
                        }
                    }
                    catch
                    {
                        unknownFormat = true;
                    }
                    finally
                    {
                        if (wasCompressed)
                        {
                            tarStream.Dispose();
                        }
                    }
                }
                else
                {
                    // Unknown format.
                    unknownFormat = true;
                }

                attachmentStream.Position = attachment.StreamStart;
            }


            return (tbr, unknownFormat);
        }

        private static Page ParseImage(Stream imageStream, int imageFormat, double scaleFactor)
        {
            if (imageFormat == 0) // SVG
            {
                Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(Parser.ParseSVGURI);
                Page pag = Parser.FromStream(imageStream);
                pag.Graphics.UseUniqueTags = false;
                return pag;
            }
            else
            {
                using MemoryStream copiedStream = new MemoryStream();
                imageStream.CopyTo(copiedStream);
                copiedStream.Seek(0, SeekOrigin.Begin);

                MuPDFCore.InputFileTypes imageType = (MuPDFCore.InputFileTypes)(imageFormat - 1);

                VectSharp.MuPDFUtils.RasterImageStream rasterImageStream;

                try
                {
                    rasterImageStream = new VectSharp.MuPDFUtils.RasterImageStream(copiedStream, imageType, scale: scaleFactor);
                }
                catch (Exception ex)
                {
                    throw ex.InnerException;
                }

                Page pag = new Page(rasterImageStream.Width, rasterImageStream.Height);
                pag.Graphics.UseUniqueTags = false;
                pag.Graphics.DrawRasterImage(0, 0, rasterImageStream);
                return pag;
            }
        }
    }
}
