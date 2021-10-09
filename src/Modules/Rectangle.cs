using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace a34e1a6277b6a4c0f80d9795eea245e1e
{
    public static class MyModule
    {
        public const string Name = "Rectangle";
        public const string HelpText = "Draws a rectangle on the plot.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "34e1a627-7b6a-4c0f-80d9-795eea245e1e";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABlSURBVDhPY/Sq3fifgQiwrdmfEcpEASwgQuPrwUYwDwe4wW1fD2ViACYoTTYAu4BYgM27JLsA6F1HGAbxKfbCwBsADkR80UQIYE0cuAAoFmCBBwJAi/cPg0AceANIjgUoEwoYGACRzhuHvr6QdQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABvSURBVEhLY/Sq3fifgUywrdmfEcrECcAWaHw92AjlEw1ucNvXE2MBE5SmGRj6FlAlDvAlFKr5AOhIR3QMEh+NZIJg1AKCgCqFHTQvgZMlMgDmlf0ECytiAD4LRiOZIBi1gCAY+hZQLaNBmWiAgQEAcUw0oksdtoMAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAB7SURBVFhHY/Sq3fifgYpgW7M/I5RJFGCC0gMG4CGg8fVgI1iETHCD274eRA+5EBh1AM3SALG5a/hHATBkHbFhqPRoIhx1wKgDBt4BNGsPIJWw8DyPDIAl534QPfAhAKWpDoZMCIw6YNQBow4YdcCoA2heFxACAxwCDAwAahU20RfyMeQAAAAASUVORK5CYII=";

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
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                ("Position", "Group:7"),
                
                /// <param name="Node:">
                /// This parameter determines the node used as an anchor for positioning the rectangle. If only a
                /// single node is selected, the anchor corresponds to that node. If more than one node is
                /// selected, the anchor corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ("Node:", "Node:[\"" + leafNames[0] + "\",\"" + leafNames[^1] + "\"]"),
                
                /// <param name="Anchor:">
                /// This parameter determines the anchor for the rectangle. If the value is `Node`, the rectangle is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the rectangle is aligned with the midpoint of the branch connecting the node to its parent.
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
                ("Anchor:", "ComboBox:0[\"Node\",\"Mid-branch\",\"Centre of leaves\",\"Origin\"]"),
                
                /// <param name="Orientation reference:">
                /// This parameter determines the direction along which the offset of the rectangle from the anchor is computed. If the value is `Horizontal`,
                /// the offset `X` coordinate corresponds to an horizontal displacement and the `Y` coordinate to a vertical displacement; if the value is
                /// `Branch`, the `X` coordinate corresponds to a shift in the direction of the branch, while the `Y` coordinate corresponds to a shift in a direction
                /// perpendicular to the branch.
                /// </param>
                ("Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]"),
                
                /// <param name="Branch reference:">
                /// This parameter determines the algorithm used to compute branch orientations. For best results, the value of this parameter should correspond to the coordinates module actually used.
                /// </param>
                ("Branch reference:", "ComboBox:0[\"Rectangular\",\"Radial\",\"Circular\"]"),
                
                /// <param name="Position:">
                /// This parameter determines how shifted from the anchor point the rectangle is. The `X` coordinate corresponds to the line determined by the [Orientation reference](#orientation-reference);
                /// the `Y` coordinate corresponds to the line perpendicular to this.
                /// </param>
                ("Position:", "Point:[0,0]"),
                
                /// <param name="Horizontal alignment:">
                /// This parameter determines the horizontal alignment of the rectangle with respect to the [Anchor](#anchor). If this is `Left`, the left side of the rectangle corresponds to the anchor; if it
                /// is `Right`, the right side of the rectangle corresponds to the anchor; if this is `Center`, the centre of the rectangle correspond to the anchor.
                /// </param>
                ( "Horizontal alignment:", "ComboBox:1[\"Left\",\"Center\",\"Right\"]" ),
                
                /// <param name="Vertical alignment:">
                /// This parameter determines the vertical alignment of the rectangle with respect to the [Anchor](#anchor). If this is `Top`, the top side of the rectangle corresponds to the anchor; if it
                /// is `Bottom`, the bottom side of the rectangle corresponds to the anchor; if this is `Middle`, the middle of the rectangle correspond to the anchor.
                /// </param>
                ( "Vertical alignment:", "ComboBox:1[\"Top\",\"Middle\",\"Bottom\"]" ),

                ("Size", "Group:2"),
                
                /// <param name="Width:">
                /// This parameter determines the width of the rectangle.
                /// </param>
                ("Width:", "NumericUpDown:50[\"-Infinity\",\"Infinity\"]"),
                
                /// <param name="Height:">
                /// This parameter determines the width of the rectangle.
                /// </param>
                ("Height:", "NumericUpDown:50[\"-Infinity\",\"Infinity\"]"),

                ("Orientation", "Group:2"),

                /// <param name="Reference:">
                /// This parameter determines the direction along which the rectangle is drawn. If the value is `Horizontal`, the [Orientation](#orientation) angle is computed starting from a horizontal line.
                /// If the value is `Axis`, the angle is computed starting from the reference used to compute the [Position](#position) of the rectangle.
                /// </param>
                ("Reference:", "ComboBox:1[\"Horizontal\",\"Axis\"]"),

                /// <param name="Orientation:">
                /// This parameter determines the orientation of the rectangle with respect to the [Reference](#reference), in degrees. If this is `0°`, the rectangle is parallel to the reference, if it is `90°`
                /// it is perpendicular to the reference and so on.
                /// </param>
                ("Orientation:", "Slider:0[\"0\",\"360\",\"0°\"]"),

                ("Appearance", "Group:5"),

                /// <param name="Fill colour:">
                /// This parameter determines the colour used to fill the rectangle.
                /// </param>
                ("Fill colour:", "Colour:[220,220,220,255]"),
                
                /// <param name="Stroke colour:">
                /// This parameter determines the colour used to stroke the rectangle.
                /// </param>
                ("Stroke colour:", "Colour:[0,0,0,255]"),
                
                /// <param name="Stroke thickness:">
                /// This parameter determines the thickness of the stroke.
                /// </param>
                ("Stroke thickness:", "NumericUpDown:0[\"0\",\"Infinity\"]"),
                
                /// <param name="Stroke style:">
                /// The line dash used to draw the rectangle.
                /// </param>
                ("Stroke style:", "Dash:[0,0,0]"),
                
                /// <param name="Line join:">
                /// This parameter determines the appearance of the corners of the rectangle.
                /// </param>
                ("Line join:", "ComboBox:0[\"Miter\",\"Round\",\"Bevel\"]"),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

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

            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            if (node == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            int anchor = (int)parameterValues["Anchor:"];
            int reference = (int)parameterValues["Orientation reference:"];
            int branchReference = (int)parameterValues["Branch reference:"];

            Point delta = (Point)parameterValues["Position:"];
            Colour fillColour = (Colour)parameterValues["Fill colour:"];
            Colour strokeColour = (Colour)parameterValues["Stroke colour:"];
            double strokeThickness = (double)parameterValues["Stroke thickness:"];
            LineJoins join = (LineJoins)((int)parameterValues["Line join:"]);
            LineDash dash = (LineDash)parameterValues["Stroke style:"];

            int rectReference = (int)parameterValues["Reference:"];
            double rectOrientation = (double)parameterValues["Orientation:"] * Math.PI / 180;
            int horizontalAlignment = (int)parameterValues["Horizontal alignment:"];
            int verticalAlignment = (int)parameterValues["Vertical alignment:"];

            double width = (double)parameterValues["Width:"];
            double height = (double)parameterValues["Height:"];

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

            if (rectReference == 1)
            {
                rectOrientation += referenceAngle;
            }

            graphics.Save();

            graphics.Translate(point);
            graphics.Rotate(rectOrientation);

            Size metrics = new Size(width, height);

            Point topLeft = point;
            Point bottomRight = point;
            Point topRight = point;
            Point bottomLeft = point;

            if (horizontalAlignment == 0)
            {
                switch (verticalAlignment)
                {
                    case 0:
                        topLeft = point;
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) - metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation) + metrics.Height * Math.Cos(rectOrientation));
                        bottomLeft = new Point(point.X - metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Height * Math.Cos(rectOrientation));
                        break;
                    case 1:
                        topLeft = new Point(point.X + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        break;
                    case 2:
                        topLeft = new Point(point.X + metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Height * Math.Cos(rectOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) + metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation) - metrics.Height * Math.Cos(rectOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation));
                        bottomLeft = point;
                        break;
                }
            }
            else if (horizontalAlignment == 1)
            {
                switch (verticalAlignment)
                {
                    case 0:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5 - metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5 + metrics.Height * Math.Cos(rectOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5 - metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5 + metrics.Height * Math.Cos(rectOrientation));
                        break;
                    case 1:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5 + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5 - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5 + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5 - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5 - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5 + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5 - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5 + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        break;
                    case 2:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5 + metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5 - metrics.Height * Math.Cos(rectOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5 + metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5 - metrics.Height * Math.Cos(rectOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(rectOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(rectOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) * 0.5);
                        break;
                }
            }
            else
            {
                switch (verticalAlignment)
                {
                    case 0:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation));
                        topRight = new Point(point.X, point.Y);
                        bottomRight = new Point(point.X - metrics.Height * Math.Sin(rectOrientation), point.Y + metrics.Height * Math.Cos(rectOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) - metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation) + metrics.Height * Math.Cos(rectOrientation));
                        break;
                    case 1:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomRight = new Point(point.X - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) - metrics.Height * Math.Sin(rectOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(rectOrientation) + metrics.Height * Math.Cos(rectOrientation) * 0.5);
                        break;
                    case 2:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation) + metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation) - metrics.Height * Math.Cos(rectOrientation));
                        topRight = new Point(point.X + metrics.Height * Math.Sin(rectOrientation), point.Y - metrics.Height * Math.Cos(rectOrientation));
                        bottomRight = new Point(point.X, point.Y);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(rectOrientation), point.Y - metrics.Width * Math.Sin(rectOrientation));
                        break;
                }
            }

            double vShift = 0;
            switch (verticalAlignment)
            {
                case 0:
                    break;
                case 2:
                    vShift = -metrics.Height;
                    break;
                case 1:
                    vShift = -metrics.Height * 0.5;
                    break;
            }

            if (horizontalAlignment == 0)
            {
                if (fillColour.A > 0)
                {
                    graphics.FillRectangle(0, vShift, metrics.Width, metrics.Height, fillColour);
                }

                if (strokeColour.A > 0 && strokeThickness > 0)
                {
                    graphics.StrokeRectangle(0, vShift, metrics.Width, metrics.Height, strokeColour, strokeThickness, lineDash: dash, lineJoin: join);
                }
            }
            else if (horizontalAlignment == 1)
            {
                if (fillColour.A > 0)
                {
                    graphics.FillRectangle(-metrics.Width * 0.5, vShift, metrics.Width, metrics.Height, fillColour);
                }

                if (strokeColour.A > 0 && strokeThickness > 0)
                {
                    graphics.StrokeRectangle(-metrics.Width * 0.5, vShift, metrics.Width, metrics.Height, strokeColour, strokeThickness, lineDash: dash, lineJoin: join);
                }
            }
            else
            {
                if (fillColour.A > 0)
                {
                    graphics.FillRectangle(-metrics.Width, vShift, metrics.Width, metrics.Height, fillColour);
                }

                if (strokeColour.A > 0 && strokeThickness > 0)
                {
                    graphics.StrokeRectangle(-metrics.Width, vShift, metrics.Width, metrics.Height, strokeColour, strokeThickness, lineDash: dash, lineJoin: join);
                }
            }

            graphics.Restore();

            double minX = Math.Min(Math.Min(topLeft.X, topRight.X), Math.Min(bottomLeft.X, bottomRight.X));
            double minY = Math.Min(Math.Min(topLeft.Y, topRight.Y), Math.Min(bottomLeft.Y, bottomRight.Y));
            double maxX = Math.Max(Math.Max(topLeft.X, topRight.X), Math.Max(bottomLeft.X, bottomRight.X));
            double maxY = Math.Max(Math.Max(topLeft.Y, topRight.Y), Math.Max(bottomLeft.Y, bottomRight.Y));

            topLeft = new Point(minX, minY);
            bottomRight = new Point(maxX, maxY);

            return new Point[] { topLeft, bottomRight };
        }
    }
}
