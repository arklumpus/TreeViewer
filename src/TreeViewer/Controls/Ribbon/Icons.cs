/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
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

using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using System;
using System.Linq;
using System.Reflection;

namespace TreeViewer
{
    public class Icons
    {
        public static Canvas GetPlusIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 16, Height = 16 };

            Path pth = new Path() { Data = Geometry.Parse("M7,2 L9,2 L9,7 L14,7 L14,9 L9,9 L9,14 L7,14 L7,9 L2,9 L2,7 L7,7 Z") };

            pth.Fill = new SolidColorBrush(Color.FromRgb(0, 158, 115));

            can.Children.Add(pth);
            return can;
        }

        public static Canvas GetAttachmentIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 32, Height = 32 };

            Path pth = new Path() { Data = Geometry.Parse("M-8,0 L1,-9 A3.5,3.5 0 0 1 8,-2 L-4,10 A2.5,2.5 0 0 1 -9,5 L0,-4 A1.5,1.5 0 0 1 3,-1 L-3,5"), Margin = new Avalonia.Thickness(16, 16, 0, 0) };

            pth.Stroke = new SolidColorBrush(Color.FromRgb(114, 114, 114));
            pth.StrokeThickness = 1.5;

            can.Children.Add(pth);
            return can;
        }

        public static Canvas GetAddAttachmentIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 32, Height = 32 };

            Path pth = new Path() { Data = Geometry.Parse("M-8,0 L1,-9 A3.5,3.5 0 0 1 8,-2 L-4,10 A2.5,2.5 0 0 1 -9,5 L0,-4 A1.5,1.5 0 0 1 3,-1 L-3,5"), Margin = new Avalonia.Thickness(16, 16, 0, 0) };

            pth.Stroke = new SolidColorBrush(Color.FromRgb(114, 114, 114));
            pth.StrokeThickness = 1.5;

            can.Children.Add(pth);

            Path plus = new Path() { Data = Geometry.Parse("M7,2 L9,2 L9,7 L14,7 L14,9 L9,9 L9,14 L7,14 L7,9 L2,9 L2,7 L7,7 Z"), RenderTransform = new TranslateTransform(16, 16) };
            plus.Fill = new SolidColorBrush(Color.FromRgb(0, 158, 115));
            can.Children.Add(plus);
            return can;
        }

        public static Canvas GetDownloadIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 16, Height = 16 };

            Path pth = new Path() { Data = Geometry.Parse("M6, 0 L10, 0 L10, 7 L13, 7 L8, 13 L3, 7 L6, 7 Z M1, 14 L15, 14 L15, 16 L1, 16 Z") };

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                Style fill = new Style(x => x.OfType<Path>());
                fill.Setters.Add(new Setter(Path.FillProperty, new SolidColorBrush(Color.FromRgb(0, 158, 115))));
                pth.Styles.Add(fill);
            }
            else
            {
                pth.Fill = new SolidColorBrush(Color.FromRgb(0, 158, 115));
            }

            can.Children.Add(pth);
            return can;
        }

        public static Canvas GetMinusIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 16, Height = 16 };

            Path pth = new Path() { Data = Geometry.Parse("M2,7 L14,7 L14,9 L2,9 Z") };

            pth.Fill = new SolidColorBrush(Color.FromRgb(213, 94, 0));

            can.Children.Add(pth);
            return can;
        }

        public static Canvas GetCrossIcon(double scaling, Brush fill = null)
        {
            Canvas can = new Canvas() { Width = 16, Height = 16 };

            Path pth = new Path() { Data = Geometry.Parse("M7,0 L9,0 L9,7 L16,7 L16,9 L9,9 L9,16 L7,16 L7,9 L0,9 L0,7 L7,7 Z"), RenderTransform = new RotateTransform() { Angle = 45 } };

            if (fill == null)
            {
                if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                {
                    Style fillStyle = new Style(x => x.OfType<Path>());
                    fillStyle.Setters.Add(new Setter(Path.FillProperty, new SolidColorBrush(Color.FromRgb(213, 94, 0))));
                    pth.Styles.Add(fillStyle);
                }
                else
                {
                    pth.Fill = new SolidColorBrush(Color.FromRgb(213, 94, 0));
                }
            }
            else
            {
                if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                {
                    Style fillStyle = new Style(x => x.OfType<Path>());
                    fillStyle.Setters.Add(new Setter(Path.FillProperty, fill));
                    pth.Styles.Add(fillStyle);
                }
                else
                {
                    pth.Fill = fill;
                }
            }

            can.Children.Add(pth);
            return can;
        }

        public static Control GetDuplicateIcon(double scaling)
        {
            if (scaling <= 1)
            {
                return new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, Data = Geometry.Parse("M0,0.5 L7,0.5 M6.5,0 L6.5,7 M7,6.5 L0,6.5 M0.5,7 L0.5,0 M3.5,8 L3.5,10 M3.5,9.5 L10,9.5 M9.5,9.5 L9.5,3 M9.5,3.5 L8,3.5"), StrokeThickness = 1, StrokeJoin = PenLineJoin.Miter, StrokeLineCap = PenLineCap.Square, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            }
            else if (scaling <= 1.5)
            {
                return new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, Data = Geometry.Parse("M-0.25,0.3333 L7.25,0.3333 M7,-0.25 L7,7.25 M7.25,7 L-0.25,7 M0.3333,7.25 L0.3333,-0.25 M3,8 L3,10 M3,9.6667 L9.6667,9.6667 M9.6667,10 L9.6667,2.6667 M9.6667,3 L8,3"), StrokeThickness = 1 / scaling, StrokeJoin = PenLineJoin.Miter, StrokeLineCap = PenLineCap.Square, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            }
            else
            {
                return new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, Data = Geometry.Parse("M0,0.5 L6.5,0.5 M6.5,0 L6.5,6.5 M6.5,6.5 L0,6.5 M0.5,6.5 L0.5,0 M3.5,8 L3.5,10 M3.5,9.5 L10,9.5 M9.5,9.5 L9.5,3.5 M9.5,3.5 L8,3.5"), StrokeThickness = 1, StrokeJoin = PenLineJoin.Miter, StrokeLineCap = PenLineCap.Square, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            }
            
        }

        public static Canvas GetReplaceIcon(double scaling)
        {
            Canvas can = new Canvas() { Width = 16, Height = 16 };

            Path pth = new Path() { Data = Geometry.Parse("M2,3 L11,3 L11,0 L16,5 L11,9 L11,6 L2,6 Z M14,13 L5,13 L5,16 L0,11 L5,7 L5,10 L14,10 Z") };

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                Style fill = new Style(x => x.OfType<Path>());
                fill.Setters.Add(new Setter(Path.FillProperty, new SolidColorBrush(Color.FromRgb(0, 114, 178))));
                pth.Styles.Add(fill);
            }
            else
            {
                pth.Fill = new SolidColorBrush(Color.FromRgb(0, 114, 178));
            }

            can.Children.Add(pth);
            return can;
        }

        public static Func<double, Image> GetIcon16(string imageName)
        {
            return scaling =>
            {
                Image image = new Image() { Width = 16, Height = 16 };

                if (scaling <= 1)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-16.png"));
                }
                else if (scaling <= 1.5)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-24.png"));
                }
                else
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-32.png"));
                }

                return image;
            };
        }

        public static Func<double, Image> GetIcon32(string imageName)
        {
            return scaling =>
            {
                Image image = new Image() { Width = 16, Height = 16 };

                if (scaling <= 1)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-32.png"));
                }
                else if (scaling <= 1.5)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-48.png"));
                }
                else
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName + "-64.png"));
                }

                return image;
            };
        }

        public static Func<double, Image> GetIcon(string imageName1, string imageName15, string imageName2, double width, double height)
        {
            return scaling =>
            {
                Image image = new Image() { Width = width, Height = height };

                if (scaling <= 1)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName1));
                }
                else if (scaling <= 1.5)
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName15));
                }
                else
                {
                    image.Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream(imageName2));
                }

                return image;
            };
        }

        public static Geometry CrossGeometry = Geometry.Parse("M0,0 L10,10 M10,0 L0,10");

        public static Geometry TickGeometry = Geometry.Parse("M0,5 L5,10 L10,0");
    }
}
