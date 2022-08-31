using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace a5a8eb0c8713945839e9e375749a98973
{
    /// <summary>
    /// This module defines a region of the plot that can be selected for export using the _Export_ module (id `d5d75840-4a71-4b81-bfc4-431736792abb`).
    /// 
    /// The rectangular region is identified in terms of an [anchor node](#anchor) and of the coordinates of the top left and bottom right corners of the
    /// rectangle, relative to the anchor node. The [`From current view` button](#from-current-view) can be used to set the crop region to the area currently visibile in the plot.
    /// 
    /// To help in setting up the crop region, this module also draws guide lines highlighting the selected region; once the region has been selected, these
    /// should be disabled by unchecking the [`Show guides` checkbox](#show-guides) (otherwise, they will show up in the exported plots, which is probably undesirable).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Crop region";
        public const string HelpText = "Specifies a region of the plot to be cropped.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "5a8eb0c8-7139-4583-9e9e-375749a98973";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABVSURBVDhPY0AGXrUb/0OZOAG6GiYoTTag2ABGYpxNNKBaGBQVFf0HYSgXg48McIZBX18fI5SJwsYLBk80YvMvyWGAHohQJn5AThgM0oRECqDQAAYGAAuBM6ehKVKdAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADGSURBVEhL3ZRBEoMgDEWx09P1OOw71gtwl56wJfqjQUig1S7om2GACPkxibo9t/vzheXHlO5eMP+M/gWGIzn/irOLfMWc4b1PDocQBixnas+ZlhpMpcuwTctOpyZADsYY7WPZbsA2xmGKmAIxSnKSiUjnOKOiCnBapAjtQeKczzbxn78Kynkca3S0ljWxUAXYIRztuyUpPJ8tYb6BdC67RRaeRTRqKVJbUYrMBoWWGlCUWQpgM50XObtN+/9dN30HR+hdwLk3/FpsEYRclm8AAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEWSURBVFhH7ZbBDsIgDIY349N58V12N3Pxzrt48fWUskJQWtrOyQ7uSwiMQfvTlmXdJ6fL/QkNH1eDs3vAfjN2Af0v8m1h8wgUtL4FR+xJhmEoNjjnehySWPdYUzBhX0OzJmERMPmTXHHMgmvUIrQCwODowysKwDWjbyoRGgHx5KKI3Lk2EkVxxEp93M7FO8JBovYO4OyaipCLhOS8hvUWvIkIEzOLnJNwH4xv4eyaI7A2iwRAzn1Lp4FxXhMWzALyggsTM+IV5TAJyJ3nBcfdDg1VAd5YCi3nPEKJgB5swJhDE4FoULxqhAjYU0WbAtF5JBcRJgQsNaDKr/bkEVMRejSG1c5JWn8J97/igtYp2DwC/y6g617vZ7kcvSir0wAAAABJRU5ErkJggg==";

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
                ( "Window", "Window:" ),
                ( "StateData", "InstanceStateData:"),

                ( "Crop region", "Group:5"),

                /// <param name="Region name:">
                /// This parameter determines the "friendly" name used to select the crop region when exporting the
                /// plot.
                /// </param>
                ( "Region name:", "TextBox:" + Id ),
                
                /// <param name="Anchor:">
                /// This parameter determines the node used as an anchor for positioning the crop guides. If only a
                /// single node is selected, the anchor corresponds to that node. If more than one node is
                /// selected, the anchor corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Anchor:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),

                /// <param name="Top left:">
                /// This parameter determines the position of the top-left corner of the crop region, with respect
                /// to the [Anchor](#anchor).
                /// </param>
                ( "Top left:", "Point:[-20,-20]"),
                
                /// <param name="Bottom right:">
                /// This parameter determines the position of the bottom-right corner of the crop region, with respect
                /// to the [Anchor](#anchor).
                /// </param>
                ( "Bottom right:", "Point:[20,20]"),
                
                /// <param name="From current view">
                /// This button sets the crop region based on the area currently visible on screen.
                /// </param>
                ( "From current view", "Button:"),

                ( "Guides", "Group:4" ),

                /// <param name="Show guides">
                /// This check box determines whether the guides are drawn or not. You probably want to hide the guides
                /// before exporting the plot.
                /// </param>
                ( "Show guides", "CheckBox:true" ),
                
                /// <param name="Guide colour:">
                /// This parameter determines the colour of the guide lines.
                /// </param>
                ( "Guide colour:", "Colour:[134, 202, 255, 255]" ),
                
                /// <param name="Guide thickness:">
                /// This parameter determines the thickness of the guide lines.
                /// </param>
                ( "Guide thickness:", "NumericUpDown:1[\"0\",\"Infinity\"]" ),

                /// <param name="Margin:">
                /// This parameter determines how much the guide lines extend beyond the crop region.
                /// </param>
                ( "Margin:", "NumericUpDown:10[\"-Infinity\",\"Infinity\"]" ),

            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "From current view", false } };

            if (!(bool)currentParameterValues["Show guides"])
            {
                controlStatus["Guide colour:"] = ControlStatus.Hidden;
                controlStatus["Guide thickness:"] = ControlStatus.Hidden;
                controlStatus["Margin:"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Guide colour:"] = ControlStatus.Enabled;
                controlStatus["Guide thickness:"] = ControlStatus.Enabled;
                controlStatus["Margin:"] = ControlStatus.Enabled;
            }

            if ((string)currentParameterValues["Region name:"] == Id)
            {
                int regionsCount;

                InstanceStateData stateData = (InstanceStateData)currentParameterValues["StateData"];

                if (!stateData.Tags.TryGetValue(Id, out object cropRegionsObject) || cropRegionsObject == null || !(cropRegionsObject is Dictionary<string, (string, Rectangle)> cropRegions))
                {
                    regionsCount = 0;
                }
                else
                {
                    regionsCount = cropRegions.Count;
                }

                parametersToChange["Region name:"] = "Region " + (regionsCount + 1).ToString();
            }

            if ((bool)currentParameterValues["From current view"])
            {
                MainWindow parentWindow = currentParameterValues["Window"] as MainWindow;
                TreeNode treeNode = tree as TreeNode;

                if (parentWindow != null && treeNode != null)
                {
                    string[] nodeElements = (string[])currentParameterValues["Anchor:"];

                    TreeNode anchorNode = treeNode.GetLastCommonAncestor(nodeElements);

                    if (anchorNode != null)
                    {
                        Point pt = parentWindow.Coordinates[anchorNode.Id];


                        Rectangle rectangle = parentWindow.GetVisibleRegion();

                        parametersToChange["Top left:"] = new Point(rectangle.Location.X - pt.X, rectangle.Location.Y - pt.Y);
                        parametersToChange["Bottom right:"] = new Point(rectangle.Location.X + rectangle.Size.Width - pt.X, rectangle.Location.Y + rectangle.Size.Height - pt.Y);
                    }
                }
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            string[] nodeElements = (string[])parameterValues["Anchor:"];

            TreeNode anchorNode = tree.GetLastCommonAncestor(nodeElements);

            if (anchorNode == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            Point topLeft = (Point)parameterValues["Top left:"];
            Point bottomRight = (Point)parameterValues["Bottom right:"];

            string regionName = (string)parameterValues["Region name:"];

            bool showGuides = (bool)parameterValues["Show guides"];

            Point pt = coordinates[anchorNode.Id];

            double top = Math.Min(pt.Y + topLeft.Y, pt.Y + bottomRight.Y);
            double left = Math.Min(pt.X + topLeft.X, pt.X + bottomRight.X);
            double right = Math.Max(pt.X + topLeft.X, pt.X + bottomRight.X);
            double bottom = Math.Max(pt.Y + topLeft.Y, pt.Y + bottomRight.Y);

            double lineWidth = (double)parameterValues["Guide thickness:"];
            Colour guideColour = (Colour)parameterValues["Guide colour:"];
            double margin = (double)parameterValues["Margin:"];

            string currInstanceGuid = (string)parameterValues[Modules.ModuleIDKey];

            if (showGuides)
            {
                graphics.StrokePath(new GraphicsPath().MoveTo(left - margin * lineWidth, top).LineTo(right + margin * lineWidth, top), guideColour, lineWidth, tag: anchorNode.Id);
                graphics.StrokePath(new GraphicsPath().MoveTo(left - margin * lineWidth, bottom).LineTo(right + margin * lineWidth, bottom), guideColour, lineWidth, tag: anchorNode.Id);
                graphics.StrokePath(new GraphicsPath().MoveTo(left, top - margin * lineWidth).LineTo(left, bottom + margin * lineWidth), guideColour, lineWidth, tag: anchorNode.Id);
                graphics.StrokePath(new GraphicsPath().MoveTo(right, top - margin * lineWidth).LineTo(right, bottom + margin * lineWidth), guideColour, lineWidth, tag: anchorNode.Id);

                Point center = new Point((left + right) * 0.5, (top + bottom) * 0.5);

                graphics.Save();
                graphics.Translate(center);

                double arrowSize = lineWidth * 3;

                void drawArrow(Point target)
                {
                    graphics.Save();
                    graphics.Rotate(Math.Atan2(target.Y, target.X));
                    double shaftLength = target.Modulus() - arrowSize * 2;
                    graphics.FillPath(new GraphicsPath().MoveTo(shaftLength, arrowSize).LineTo(shaftLength + arrowSize * 2, 0).LineTo(shaftLength, -arrowSize).LineTo(shaftLength, -lineWidth).LineTo(0, 0).LineTo(shaftLength, lineWidth).Close(), guideColour.WithAlpha(guideColour.A * 0.5), tag: anchorNode.Id);
                    graphics.Restore();
                }

                drawArrow(new Point(left - center.X + lineWidth, top - center.Y + lineWidth));
                drawArrow(new Point(left - center.X + lineWidth, bottom - center.Y - lineWidth));
                drawArrow(new Point(right - center.X - lineWidth, top - center.Y + lineWidth));
                drawArrow(new Point(right - center.X - lineWidth, bottom - center.Y - lineWidth));
                graphics.Restore();
            }

            if (!stateData.Tags.TryGetValue(Id, out object cropRegionsObject) || cropRegionsObject == null || !(cropRegionsObject is Dictionary<string, (string, Rectangle)> cropRegions))
            {
                cropRegions = new Dictionary<string, (string, Rectangle)>();
                stateData.Tags[Id] = cropRegions;
            }

            cropRegions[currInstanceGuid] = (regionName, new Rectangle(new Point(left, top), new Point(right, bottom)));

            return new Point[] { new Point(), new Point() };
        }
    }
}
