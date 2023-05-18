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

namespace aebafe997c220425aae3906e731de1a07
{
    public static class MyModule
    {
        public const string Name = "Text element";
        public const string HelpText = "Draws a text element on the plot.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.0");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "ebafe997-c220-425a-ae39-06e731de1a07";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVDhPYxj6gBFEFBQUmDAzM/OARZDA//////T19R2BcrECJjDBxFT879+/ehAGatoPxP1QdjNYFSmgqKjof3FxsQOUSxCwQGm8oKysTOb379/9jIyMJkBXPQHixgkTJuwByYG9gA/U19ez/PnzZz9Q88m/f/8aAoVWAtmbS0pKJEDyBA349OmTC9BGUGD2TJw48UN/f/8UoPADoKEBIHmCXgDaJgM0QAEYNvehQjDwBUQQNACoGaTwDtAFuhARVEDQC8D0cQBIqQDTigtEhIEhPz9fARg2HCA2VgNAfoYyGXp6el4AqUSgV5aDvFFYWPgYmG62f/78GSPhEQQgm0FRCuUOCsDAAACV4nQCScQ1LwAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHhSURBVEhL7ZQ9SwNBEIaTSyBXpLgipYWCRQoLxcYUglbW/gPtJE1I0KBVDBZqkUgQIlb5AVpYWFiIHyAcgqCghYWgWAlaHCFgIF8+s7cpItEk19j4wtw7N7s3c/Ox6/vHn8Mvj1QqFWk0GmFl6QHLst6y2WxVv/aEIY9Wq3UQCASe+5FyubysvuwTAXnEYrEGdI9civj9/jt4CnEIvt2267Uz27Zf0fuCKtF3JBKJYflb1Jd8Pj/iWr0hqNkzpH9kNVav1x3680B/6npJQfXAC5LJ5CjOb1HfKeM5Gd/Sn09sq+4OF54CpNPpIf5anI8ia+iz8DzyiGwSfB1W8BSgVqttQGEcL9KjrVwudwEfNZvNaewO9kQ8HldjP3CATCYTxMEC6hOOD12ri0Kh4EDHiGWa5rjYBm5ypVKJClP3MKUoKWMnZLxlfRi68lKiiGaLTGa+C/Ygzh9FZNPAGcg4MjGi3lB3qfmv8JLBi+ao9EPrP2LgALqRp5QjwtwvudZOtCdI4Okkc/OuGIZhE2SXRk9S72veq7D0ZY4tMgjqiumaQSgUah/3jmPfBlnc4WgC9QRewPkeegl9Bxbn+4hC18tOIHdMFRSLxYo2dQV9MB3HieI8SCC5HD/00j/6gc/3BQJxslFEjiBGAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJjSURBVFhH7VY5aBtREF1dSIFdkcKFAy4c4kKFCxdx58IuDDYk4CKBBFykViN0IBNiUITdWScYtSlSJH0CcmGIIY2LQAhJYXACKQMJeCW2WKEr731GBuNLa76xCz2Ynfnzd3eeZubPyhhhhJuGj5dMJjPe7/djyuMBeKZjWdZ+Pp/viMszFIFkMvnB5/M9Uh6PAInn5XL5vSw9wy/aFe0ZIH7lZwmVgfOAzLxGgBztbrd7v1qt/lYbGjHIwI3hWgnE43GTIsszERStDYlEYjIQCGygOWdQvmn6UMoDqB+Ql2jYn/QNoDUDqVTqBYJ/h7kqwR0QcWHHIE8gX3HPirpZoI1AOp2eh3oDYcp3e73ebKlUsqLRqIXAC/CxgU0Qese5A1tBGwG8eEv0HobTcqVS+cI1h1SxWNyDfxHSAZkIyK1xj9BCAGnlEHtI2+/3I+bpySi1/0gbRJ5RE7oyoMY4f6FpmvvKcza+8YIsjA9Oh65T8EB0p9ls1tH1sjwJBJ4U0wiFQmNQjq4MqBezvpD582RwH+C02+1/NHQRUC8DbI7sy8R13Xu1Ws3hA1q+BWjCDah12jgBd9CEQ3+gdGXgl2ij0WjMiTkUtBDA0duBsmkjY69yudzQza2FQKFQ+IMjWKTNZsNJ+ITyTalNAUhF4FtCud5Cr4pbWwkMjNwCSKhBAxJzkEME+gv5jKCHIHUEXx3bDH5M7jICxxMtHA5f+L+PjYdp9xhBnoIIv34kMkYyMKegI+LfhL3NfeLCU0Bks9mJVqvl4ASoGg8Lpty27VgwGLzLNYIesFRqc4QRbg8M4z9GFft0SNvjAQAAAABJRU5ErkJggg==";

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
                /// <param name="Text:">
                /// This text box contains the text that will be drawn on the plot.
                /// </param>
                ("Text:", "TextBox:Text"),

                ( "Position", "Group:6"),
                
                /// <param name="Node:">
                /// This parameter determines the node used as an anchor for positioning the text. If only a
                /// single node is selected, the anchor corresponds to that node. If more than one node is
                /// selected, the anchor corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Anchor:">
                /// This parameter determines the anchor for the text. If the value is `Node`, the text is anchored to the corresponding node.
                /// If the value is `Mid-branch`, the text is aligned with the midpoint of the branch connecting the node to its parent.
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
                /// This parameter determines the direction along which the offset of the text from the anchor is computed. If the value is `Horizontal`,
                /// the offset `X` coordinate corresponds to an horizontal displacement and the `Y` coordinate to a vertical displacement; if the value is
                /// `Branch`, the `X` coordinate corresponds to a shift in the direction of the branch, while the `Y` coordinate corresponds to a shift in a direction
                /// perpendicular to the branch.
                /// </param>
                ( "Orientation reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),
              
                /// <param name="Position:">
                /// This parameter determines how shifted from the anchor point the text is. The `X` coordinate corresponds to the line determined by the [Orientation reference](#orientation-reference);
                /// the `Y` coordinate corresponds to the line perpendicular to this.
                /// </param>
                ( "Position:", "Point:[0,0]" ),
                
                /// <param name="Horizontal alignment:">
                /// This parameter determines the horizontal alignment of the text with respect to the [Anchor](#anchor). If this is `Left`, the left side of the text corresponds to the anchor; if it
                /// is `Right`, the right side of the text corresponds to the anchor; if this is `Center`, the centre of the text correspond to the anchor.
                /// </param>
                ( "Horizontal alignment:", "ComboBox:1[\"Left\",\"Center\",\"Right\"]" ),
                
                /// <param name="Vertical alignment:">
                /// This parameter determines the vertical alignment of the text with respect to the [Anchor](#anchor). If this is `Top`, the top side of the text corresponds to the anchor; if it
                /// is `Bottom`, the bottom side of the text corresponds to the anchor; if this is `Middle`, the middle of the text correspond to the anchor; if it is `Baseline`, the text baseline
                /// corresponds to the anchor. If you wish to align multiple text elements vertically, use `Baseline`.
                /// </param>
                ( "Vertical alignment:", "ComboBox:1[\"Top\",\"Middle\",\"Baseline\",\"Bottom\"]" ),

                ( "Orientation", "Group:2" ),

                /// <param name="Reference:">
                /// This parameter determines the direction along which the text is drawn. If the value is `Horizontal`, the [Orientation](#orientation) angle is computed starting from a horizontal line.
                /// If the value is `Axis`, the angle is computed starting from the reference used to compute the [Position](#position) of the text.
                /// </param>
                ( "Reference:", "ComboBox:1[\"Horizontal\",\"Axis\"]" ),

                /// <param name="Orientation:">
                /// This parameter determines the orientation of the label with respect to the [Reference](#reference), in degrees. If this is `0°`, the label is parallel to the reference, if it is `90°`
                /// it is perpendicular to the reference and so on.
                /// </param>
                ( "Orientation:", "Slider:0[\"0\",\"360\",\"0°\"]" ),

                ( "Text appearance", "Group:2"),

                /// <param name="Font:">
                /// This parameter determines the font (font family and size) used to draw the text.
                /// </param>
                ( "Font:", "Font:[\"Helvetica\",\"10\"]" ),

                /// <param name="Text colour:">
                /// This parameter determines the colour used to draw the text.
                /// </param>
                ( "Text colour:", "Colour:[0,0,0,255]" ),

                ( "Background appearance", "Group:6"),

                /// <param name="Background colour:">
                /// This parameter determines the colour used as a background for the text.
                /// </param>
                ( "Background colour:", "Colour:[0,0,0,0]" ),
                
                /// <param name="Margin:">
                /// This parameter determines the margin between the background and the text.
                /// </param>
                ( "Margin:", "Point:[10,5]" ),
                
                /// <param name="Border colour:">
                /// This parameter determines the colour used for the border of text.
                /// </param>
                ( "Border colour:", "Colour:[0,0,0,255]" ),
                
                /// <param name="Border thickness:">
                /// This parameter determines the thickness of the border around the text.
                /// </param>
                ( "Border thickness:", "NumericUpDown:0[\"0\",\"Infinity\"]" ),
                
                /// <param name="Border style:">
                /// The line dash used to draw the border.
                /// </param>
                ( "Border style:", "Dash:[0,0,0]"),
                
                /// <param name="Line join:">
                /// This parameter determines the appearance of the corners of the text border.
                /// </param>
                ( "Line join:", "ComboBox:0[\"Miter\",\"Round\",\"Bevel\"]" ),
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
            Point margin = (Point)parameterValues["Margin:"];
            Colour backgroundColour = (Colour)parameterValues["Background colour:"];
            Colour borderColour = (Colour)parameterValues["Border colour:"];
            double borderThickness = (double)parameterValues["Border thickness:"];
            LineJoins join = (LineJoins)((int)parameterValues["Line join:"]);
            LineDash dash = (LineDash)parameterValues["Border style:"];

            int textReference = (int)parameterValues["Reference:"];
            double textOrientation = (double)parameterValues["Orientation:"] * Math.PI / 180;
            int horizontalAlignment = (int)parameterValues["Horizontal alignment:"];
            int verticalAlignment = (int)parameterValues["Vertical alignment:"];

            Font font = (Font)parameterValues["Font:"];
            Colour colour = (Colour)parameterValues["Text colour:"];
            string text = (string)parameterValues["Text:"];

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

            if (textReference == 1)
            {
                textOrientation += referenceAngle;
            }

            graphics.Save();

            graphics.Translate(point);
            graphics.Rotate(textOrientation);

            TextBaselines baseline = TextBaselines.Baseline;

            switch (verticalAlignment)
            {
                case 0:
                    baseline = TextBaselines.Top;
                    break;
                case 1:
                    baseline = TextBaselines.Middle;
                    break;
                case 2:
                    baseline = TextBaselines.Baseline;
                    break;
                case 3:
                    baseline = TextBaselines.Bottom;
                    break;
            }

            IEnumerable<FormattedText> formattedText;

            if (font.FontFamily.IsStandardFamily)
            {
                formattedText = FormattedText.Format(text, (FontFamily.StandardFontFamilies)Array.IndexOf(FontFamily.StandardFamilies, font.FontFamily.FileName), font.FontSize, defaultBrush: colour);
            }
            else
            {
                formattedText = FormattedText.Format(text, font, font, font, font, colour);
            }

            Font.DetailedFontMetrics metrics = formattedText.Measure();

            Point topLeft = point;
            Point bottomRight = point;
            Point topRight = point;
            Point bottomLeft = point;

            if (horizontalAlignment == 0)
            {
                switch (baseline)
                {
                    case TextBaselines.Top:
                        topLeft = point;
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) - metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) + metrics.Height * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X - metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Height * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Middle:
                        topLeft = new Point(point.X + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        break;
                    case TextBaselines.Baseline:
                        topLeft = new Point(point.X + metrics.Top * Math.Sin(textOrientation), point.Y - metrics.Top * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) + metrics.Top * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) - metrics.Top * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) + metrics.Bottom * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) - metrics.Bottom * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X + metrics.Bottom * Math.Sin(textOrientation), point.Y - metrics.Bottom * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Bottom:
                        topLeft = new Point(point.X + metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Height * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) + metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) - metrics.Height * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation));
                        bottomLeft = point;
                        break;
                }
            }
            else if (horizontalAlignment == 1)
            {
                switch (baseline)
                {
                    case TextBaselines.Top:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 - metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 + metrics.Height * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 - metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 + metrics.Height * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Middle:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        break;
                    case TextBaselines.Baseline:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Top * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Top * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Top * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Top * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Bottom * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Bottom * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Bottom * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Bottom * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Bottom:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Height * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5 + metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5 - metrics.Height * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X + metrics.Width * Math.Cos(textOrientation) * 0.5, point.Y + metrics.Width * Math.Sin(textOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) * 0.5);
                        break;
                }
            }
            else
            {
                switch (baseline)
                {
                    case TextBaselines.Top:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation));
                        topRight = new Point(point.X, point.Y);
                        bottomRight = new Point(point.X - metrics.Height * Math.Sin(textOrientation), point.Y + metrics.Height * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) - metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) + metrics.Height * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Middle:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        topRight = new Point(point.X + metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomRight = new Point(point.X - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) - metrics.Height * Math.Sin(textOrientation) * 0.5, point.Y - metrics.Width * Math.Sin(textOrientation) + metrics.Height * Math.Cos(textOrientation) * 0.5);
                        break;
                    case TextBaselines.Baseline:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) + metrics.Top * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) - metrics.Top * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Top * Math.Sin(textOrientation), point.Y - metrics.Top * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X + metrics.Bottom * Math.Sin(textOrientation), point.Y - metrics.Bottom * Math.Cos(textOrientation));
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) + metrics.Bottom * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) - metrics.Bottom * Math.Cos(textOrientation));
                        break;
                    case TextBaselines.Bottom:
                        topLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation) + metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation) - metrics.Height * Math.Cos(textOrientation));
                        topRight = new Point(point.X + metrics.Height * Math.Sin(textOrientation), point.Y - metrics.Height * Math.Cos(textOrientation));
                        bottomRight = new Point(point.X, point.Y);
                        bottomLeft = new Point(point.X - metrics.Width * Math.Cos(textOrientation), point.Y - metrics.Width * Math.Sin(textOrientation));
                        break;
                }
            }

            topLeft = new Point(topLeft.X - margin.X * Math.Cos(textOrientation) + margin.Y * Math.Sin(textOrientation), topLeft.Y - margin.X * Math.Sin(textOrientation) - margin.Y * Math.Cos(textOrientation));
            topRight = new Point(topRight.X + margin.X * Math.Cos(textOrientation) + margin.Y * Math.Sin(textOrientation), topRight.Y + margin.X * Math.Sin(textOrientation) - margin.Y * Math.Cos(textOrientation));
            bottomLeft = new Point(bottomLeft.X - margin.X * Math.Cos(textOrientation) - margin.Y * Math.Sin(textOrientation), bottomLeft.Y - margin.X * Math.Sin(textOrientation) + margin.Y * Math.Cos(textOrientation));
            bottomRight = new Point(bottomRight.X + margin.X * Math.Cos(textOrientation) - margin.Y * Math.Sin(textOrientation), bottomRight.Y + margin.X * Math.Sin(textOrientation) + margin.Y * Math.Cos(textOrientation));

            double vShift = 0;
            switch (baseline)
            {
                case TextBaselines.Top:
                    break;
                case TextBaselines.Bottom:
                    vShift = -metrics.Height;
                    break;
                case TextBaselines.Middle:
                    vShift = -metrics.Height * 0.5;
                    break;
                case TextBaselines.Baseline:
                    vShift = -metrics.Top;
                    break;
            }

            if (horizontalAlignment == 0)
            {
                if (backgroundColour.A > 0)
                {
                    graphics.FillRectangle(-margin.X, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, backgroundColour);
                }

                if (borderColour.A > 0 && borderThickness > 0)
                {
                    graphics.StrokeRectangle(-margin.X, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, borderColour, borderThickness, lineDash: dash, lineJoin: join);
                }

                graphics.FillText(0, 0, formattedText, colour, baseline);
            }
            else if (horizontalAlignment == 1)
            {
                if (backgroundColour.A > 0)
                {
                    graphics.FillRectangle(-margin.X - metrics.Width * 0.5, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, backgroundColour);
                }

                if (borderColour.A > 0 && borderThickness > 0)
                {
                    graphics.StrokeRectangle(-margin.X - metrics.Width * 0.5, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, borderColour, borderThickness, lineDash: dash, lineJoin: join);
                }

                graphics.FillText(-metrics.Width * 0.5, 0, formattedText, colour, baseline);
            }
            else
            {
                if (backgroundColour.A > 0)
                {
                    graphics.FillRectangle(-margin.X - metrics.Width, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, backgroundColour);
                }

                if (borderColour.A > 0 && borderThickness > 0)
                {
                    graphics.StrokeRectangle(-margin.X - metrics.Width, -margin.Y + vShift, metrics.Width + margin.X * 2, metrics.Height + margin.Y * 2, borderColour, borderThickness, lineDash: dash, lineJoin: join);
                }
                graphics.FillText(-metrics.Width, 0, formattedText, colour, baseline);
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

