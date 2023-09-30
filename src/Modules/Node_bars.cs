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

namespace NodeBars
{
    /// <summary>
    /// This module draws "bars" at nodes in the tree, used to show e.g. an age range associated to each node.
    /// 
    /// This module can only be applied if _Rectangular_ or _Circular_ coordinates are being used.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The attribute that is used to determine the range associated to each node must be a string that can be
    /// parsed as a "Range". This means a list of two integers separated by commas (`,`), semicolons (`;`), or
    /// underscores (`_`) and optionally enclosed within square (`[]`) or curly (`{}`) brackets. The following
    /// are all examples of valid ranges:
    /// 
    /// * `[ 2.5, 3 ]`
    /// * `2.5,3`
    /// * `{ 4.67_6.5 }`
    /// * `1;3`
    /// * `2_4`
    /// 
    /// The order of the elements in the range does not matter. If the list contains more than two elements,
    /// it is not a valid range.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Node bars";
        public const string HelpText = "Draws node bars.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.4");
        public const string Id = "319e8f63-d6c9-4dac-9419-0b621dcd5f23";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABuSURBVDhPY2QgEhQVFf2HMskDuAxggtK0AV61G7HaiizOAqXhAObUvr4+cPjgMgQnQPYrMS7AALgCi2aBCE8HIGdta/bHycflAkaQBCjAsPkL2QCCAN0AvAGFD+ByKi5AcSAOvAEYoUxaGDAwAAD7sTf88fSNCQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACzSURBVEhL5ZRBDoQgDEXBeDo33oX9RI177uLG6zlUO4aY0oIdTYwvIZUghU8/WKPAOdeH0G09mgrjWdjkaoKCBRp2SbQKRC5fIJvmMy3QsEtC/ZN0UewQ772VksfMY7vn5Y7oLw6pMSaB3ePnSry7IyUqsyz4DptyLlqlH2sQIx0PsE/+FShVRGo8LCA+dhZ2UeJzzkUU9z0VoIBTIY0XA0eXU0SJ59+DF9xkLZyCAaMCY77qgFMBXTuLNQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADESURBVFhH7ZU7DoMwDIaTHq8Ld8lelYo9d2HhejSmHiiP2A4OMPiTosgiUn5+P+KdMiGENm3vX0TzwF0T9uVVSA6MsDAkqeGACBNQLOD56kdYGJLsnVd3IMboYWFIQh7c6mu4QPL3c4au+buT48A9+/q2NXDaIIJcLvOZY++8ugNSTABnDkwFxR0ukgIEVg5Q7UV9T3xwL0NBgAg/t0wyYiUtmOPyIlxxdgpIB6SjVZ3aAmwSmgATwH4Na8Fx4NjrlsW5L2y3b0k6H05nAAAAAElFTkSuQmCC";

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
            string putativeAttribute = "Name";

            foreach (KeyValuePair<string, object> kvp in tree.Attributes)
            {
                double[] parsed = ParseRange(kvp.Value);
                if (parsed != null && parsed.Length >= 2)
                {
                    putativeAttribute = kvp.Key;
                    break;
                }
            }

            return new List<(string, string)>()
            {
                /// <param name="Attribute:" default="any range attribute">
                /// This parameter determines the attribute used to specify the range associated to each node.
                /// If any attribute that can be parsed as a range is present in the tree, that attribute is
                /// used as the default value for this parameter.
                /// </param>
                ( "Attribute:", "AttributeSelector:" + putativeAttribute ),

                ( "Appearance", "Group:5" ),
                
                /// <param name="Show on:">
                /// This parameter determines on which nodes the bars are shown. If the value is `Leaves`, the
                /// bars are only shown for terminal nodes (nodes with no child nodes). If the value is `Internal
                /// nodes` the bars are shown only for internal nodes (nodes which have at least one child).
                /// If the value is `All nodes`, bars are shown for both leaves and internal nodes.
                /// </param>
                ( "Show on:", "ComboBox:1[\"Leaves\",\"Internal nodes\",\"All nodes\"]" ),
                
                /// <param name="Exclude cartoon nodes">
                /// This parameter determines whether bars are shown for nodes which have been "cartooned" or
                /// collapsed. If the check box is checked, bars are not shown for nodes that have been "cartooned".
                /// </param>
                ( "Exclude cartoon nodes", "CheckBox:true" ),
                
                /// <param name="Auto colour by node">
                /// If this check box is checked, the colour of each bar is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible
                /// if the same tree is rendered multiple times.
                /// </param>
                ( "Auto colour by node", "CheckBox:true"),
                
                /// <param name="Opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto colour by node](#auto-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to draw each bar (if the [Auto colour by node](#auto-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
                
                /// <param name="Thickness:">
                /// This parameter determines the thickness of the bar.
                /// </param>
                ( "Thickness:", "NumericUpDown:2[\"1\",\"Infinity\",\"1\",\"0\"]"),

                ( "Whiskers", "Group:3" ),
                
                /// <param name="Whiskers " display="Whiskers">
                /// If this check box is checked, the bars will have "whiskers", i.e. little perpendicular markers indicating the
                /// start and end of each bar.
                /// </param>
                ( "Whiskers ", "CheckBox:true"),
                
                /// <param name="Whisker size:">
                /// This parameter controls the height of the whiskers.
                /// </param>
                ( "Whisker size:", "NumericUpDown:10[\"1\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="Whisker thickness:">
                /// This parameter controls the thickness of the whiskers.
                /// </param>
                ( "Whisker thickness:", "NumericUpDown:2[\"1\",\"Infinity\",\"1\",\"0\"]")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { };

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

            parametersToChange = new Dictionary<string, object>() { };

            return true;
        }

        static double[] ParseRange(object range)
        {
            if (!(range is string stringRange))
            {
                return null;
            }
            else
            {
                stringRange = stringRange.Replace("{", "").Replace("}", "").Replace("_", ",").Replace(";", ",").Replace("[", "").Replace("]", "");
                stringRange = "[" + stringRange + "]";
                List<double> tbr = null;

                try
                {
                    tbr = System.Text.Json.JsonSerializer.Deserialize<List<double>>(stringRange, Modules.DefaultSerializationOptions);
                    tbr.Sort((a, b) => Math.Sign(b - a));
                }
                catch
                {

                }

                if (tbr != null)
                {
                    return tbr.ToArray();
                }
                else
                {
                    return null;
                }
            }
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            double thickness = (double)parameterValues["Thickness:"];
            double whiskerSize = (double)parameterValues["Whisker size:"];
            double whiskerThickness = (double)parameterValues["Whisker thickness:"];
            bool whiskers = (bool)parameterValues["Whiskers "];

            int showOn = (int)parameterValues["Show on:"];
            bool excludeCartoonNodes = (bool)parameterValues["Exclude cartoon nodes"];

            bool autoColourByNode = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];
            ColourFormatterOptions customColour = (ColourFormatterOptions)parameterValues["Colour:"];
            Colour defaultFill = customColour.DefaultColour;
            Func<object, Colour?> fillFormatter = customColour.Formatter;
            string attributeName = (string)parameterValues["Attribute:"];

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

            bool anyPoint = false;

            void updateMaxMin(Point pt)
            {
                anyPoint = true;
                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static Point sumPoint(Point pt1, Point pt2)
            {
                return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
            }

            static double distance(Point pt1, Point pt2)
            {
                return Math.Sqrt((pt1.X - pt2.X) * (pt1.X - pt2.X) + (pt1.Y - pt2.Y) * (pt1.Y - pt2.Y));
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

            double totalTreeLength = tree.LongestDownstreamLength();

            if (!circularCoordinates)
            {
                Point perpScale = normalizePoint(rotatePoint(scalePoint, Math.PI / 2));
                Point parallScale = normalizePoint(scalePoint);

                for (int i = 0; i < nodes.Count; i++)
                {
                    if ((showOn == 2 || (showOn == 0 && nodes[i].Children.Count == 0) || (showOn == 1 && nodes[i].Children.Count > 0)) && (!excludeCartoonNodes || !nodes[i].Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe")))
                    {
                        if (nodes[i].Attributes.TryGetValue(attributeName, out object attribute) && attribute != null)
                        {
                            double[] range = ParseRange(attribute);

                            if (range != null && range.Length == 2 && range[0] != range[1])
                            {
                                Colour colour = defaultFill;

                                if (autoColourByNode)
                                {
                                    colour = Modules.AutoColour(nodes[i]).WithAlpha(opacity);
                                }
                                else if (nodes[i].Attributes.TryGetValue(customColour.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                                {
                                    colour = fillFormatter(fillAttributeObject) ?? defaultFill;
                                }


                                double age = totalTreeLength - nodes[i].UpstreamLength();
                                double deltaLeft = age - range[0];
                                double deltaRight = range[1] - age;


                                Point leftPoint = sumPoint(new Point(-scalePoint.X * deltaRight, -scalePoint.Y * deltaRight), coordinates[nodes[i].Id]);
                                Point rightPoint = sumPoint(new Point(scalePoint.X * deltaLeft, scalePoint.Y * deltaLeft), coordinates[nodes[i].Id]);

                                GraphicsPath pth = new GraphicsPath();

                                if (!whiskers)
                                {
									updateMaxMin(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), leftPoint));
									
                                    pth.MoveTo(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), leftPoint)).Close();
                                }
                                else
                                {
									updateMaxMin(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
									
                                    pth.MoveTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint));
                                    pth.LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * thickness * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * thickness * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * thickness * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * thickness * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint)).Close();
                                }

                                graphics.FillPath(pth, colour, tag: nodes[i].Id);
                            }
                        }
                    }
                }
            }
            else
            {
                Point rootPoint = coordinates[Modules.RootNodeId];

                for (int i = 0; i < nodes.Count; i++)
                {
                    if ((showOn == 2 || (showOn == 0 && nodes[i].Children.Count == 0) || (showOn == 1 && nodes[i].Children.Count > 0)) && (!excludeCartoonNodes || !nodes[i].Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe")))
                    {
                        if (nodes[i].Attributes.TryGetValue(attributeName, out object attribute) && attribute != null)
                        {
                            double[] range = ParseRange(attribute);

                            if (range != null && range.Length == 2 && range[0] != range[1])
                            {
                                Colour colour = defaultFill;

                                if (autoColourByNode)
                                {
                                    colour = Modules.AutoColour(nodes[i]).WithAlpha(opacity);
                                }
                                else if (nodes[i].Attributes.TryGetValue(customColour.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                                {
                                    colour = fillFormatter(fillAttributeObject) ?? defaultFill;
                                }


                                double age = totalTreeLength - nodes[i].UpstreamLength();
                                double deltaRight = age - range[0];
                                double deltaLeft = range[1] - age;

                                Point point = coordinates[nodes[i].Id];

                                double r = distance(point, rootPoint);
                                double theta = Math.Atan2(point.Y - rootPoint.Y, point.X - rootPoint.X);

                                Point leftPoint = sumPoint(rootPoint, new Point((r - scalePoint.X * deltaLeft) * Math.Cos(theta), (r - scalePoint.X * deltaLeft) * Math.Sin(theta)));
                                Point rightPoint = sumPoint(rootPoint, new Point((r + scalePoint.X * deltaRight) * Math.Cos(theta), (r + scalePoint.X * deltaRight) * Math.Sin(theta)));

                                Point perpScale = new Point(Math.Sin(theta), -Math.Cos(theta));
                                Point parallScale = new Point(Math.Cos(theta), Math.Sin(theta));

                                GraphicsPath pth = new GraphicsPath();

                                if (!whiskers)
                                {
									updateMaxMin(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), leftPoint));
									
                                    pth.MoveTo(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5, perpScale.Y * thickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5, -perpScale.Y * thickness * 0.5), leftPoint)).Close();
                                }
                                else
                                {
									updateMaxMin(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint));
									updateMaxMin(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
									updateMaxMin(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
									
                                    pth.MoveTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint));
                                    pth.LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * thickness * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(perpScale.X * thickness * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * thickness * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * thickness * 0.5 + parallScale.Y * whiskerThickness * 0.5), rightPoint)).LineTo(sumPoint(new Point(-perpScale.X * thickness * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * thickness * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint));
                                    pth.LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 - parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 - parallScale.Y * whiskerThickness * 0.5), leftPoint)).LineTo(sumPoint(new Point(-perpScale.X * (thickness + whiskerSize) * 0.5 + parallScale.X * whiskerThickness * 0.5, -perpScale.Y * (thickness + whiskerSize) * 0.5 + parallScale.Y * whiskerThickness * 0.5), leftPoint)).Close();
                                }

                                graphics.FillPath(pth, colour, tag: nodes[i].Id);
                            }
                        }
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
