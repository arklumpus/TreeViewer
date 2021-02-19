using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.Canvas;
using System.Runtime.InteropServices;

namespace LassoSelection
{
    /// <summary>
    /// This module can be used to perform a "Lasso selection", i.e. to select nodes based on their position in the plot.
    /// 
    /// When you enable this module, a message is shown indicating that lasso selection is active. You can then use the mouse
    /// to draw a polygon on the tree plot (every time you click, a new point is added). When you reach the last vertex of the
    /// polygon, double click to close the shape. The names of the nodes that fall within the selected area are then copied
    /// to the clipboard and can be pasted into other software (e.g. a text editor).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Lasso selection";
        public const string HelpText = "Selects tips from the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6";
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static string ButtonText { get; } = "Lasso\nselection";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        private const string IconBase64 = "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";

        public static Page GetIcon()
        {
            byte[] bytes = Convert.FromBase64String(IconBase64);

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

            Page pag = new Page(icon.Width, icon.Height);
            pag.Graphics.DrawRasterImage(0, 0, icon);

            return pag;
        }

        public static void PerformAction(MainWindow window, InstanceStateData stateData)
        {
            if (window.TransformedTree == null || window.PlottingActions.Count == 0 || (stateData.Tags.TryGetValue("a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6", out object lassoTag) && (bool)lassoTag))
            {
                return;
            }
            stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = true;

            Avalonia.Controls.PanAndZoom.ZoomBorder zom = window.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");

            Border lassoBord = new Border() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(10, 15, 10, 10), Padding = new Avalonia.Thickness(50, 10), CornerRadius = new Avalonia.CornerRadius(10), Background = window.SelectionChildBrush, Opacity = 0, BorderBrush = window.SelectionBrush, BorderThickness = new Avalonia.Thickness(2) };

            StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            pnl.Children.Add(new TextBlock() { Text = "Lasso selection currently active", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
            HelpButton help = new HelpButton() { Margin = new Avalonia.Thickness(10, 0, 0, 0) };
            ToolTip.SetTip(help, HelpText);
            help.PointerPressed += async (s, e) =>
            {
                HelpWindow helpWindow = new HelpWindow(Modules.LoadedModulesMetadata[Id].BuildReadmeMarkdown(), Id);
                await helpWindow.ShowDialog(window);
            };
            pnl.Children.Add(help);
            lassoBord.Child = pnl;
            Grid.SetColumn(lassoBord, 2);
            Grid.SetColumnSpan(lassoBord, Grid.GetColumnSpan(zom));
            Grid.SetRow(lassoBord, 1);
            lassoBord.Transitions = new Avalonia.Animation.Transitions();
            lassoBord.Transitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Border.OpacityProperty, Duration = new TimeSpan(5000000) });
            lassoBord.Opacity = 1;
            window.FindControl<Grid>("MainGrid").Children.Add(lassoBord);

            window.SetSelection(null);
            Canvas can = new Canvas() { Width = window.FindControl<Canvas>("ContainerCanvas").Width, Height = window.FindControl<Canvas>("ContainerCanvas").Height };

            can.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 255, 255), 0);

            window.FindControl<Canvas>("ContainerCanvas").Children.Add(can);

            List<Point> selectionPoints = new List<Point>();

            void pointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
            {
                Avalonia.Point pt = e.GetCurrentPoint(can).Position;

                bool isClosed = selectionPoints.Count > 0 && ((pt.X == selectionPoints[^1].X && pt.Y == selectionPoints[^1].Y) || Math.Sqrt((pt.X - selectionPoints[0].X) * (pt.X - selectionPoints[0].X) + (pt.Y - selectionPoints[0].Y) * (pt.Y - selectionPoints[0].Y)) * zom.ZoomX <= 25);


                if (!isClosed)
                {
                    selectionPoints.Add(new Point(pt.X, pt.Y));
                }

                Page pg = new Page(can.Width, can.Height);

                if (selectionPoints.Count > 1)
                {
                    GraphicsPath pth = new GraphicsPath();

                    for (int i = 0; i < selectionPoints.Count; i++)
                    {
                        if (i == 0)
                        {
                            pth.MoveTo(selectionPoints[i]);
                        }
                        else
                        {
                            pth.LineTo(selectionPoints[i]);
                        }
                    }

                    if (isClosed)
                    {
                        pth.Close();
                    }

                    pg.Graphics.StrokePath(pth, window.SelectionColour, lineWidth: 5 / zom.ZoomX, lineCap: LineCaps.Round, lineJoin: LineJoins.Round, tag: "selectionOutline");
                }
                else if (selectionPoints.Count == 1)
                {
                    pg.Graphics.StrokePath(new GraphicsPath().MoveTo(selectionPoints[0]).LineTo(selectionPoints[0]), window.SelectionColour, lineWidth: 5 / zom.ZoomX, lineCap: LineCaps.Round, lineJoin: LineJoins.Round, tag: "selectionOutline");
                }



                can.Children.Clear();
                can.Children.Add(pg.PaintToCanvas(new Dictionary<string, Delegate>()
                {
                    {
                        "selectionOutline",
                        new Action<Avalonia.Controls.Shapes.Path>((Avalonia.Controls.Shapes.Path path) =>
                        {
                            void zoomHandler(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
                            {
                                if (e.Property == Avalonia.Controls.PanAndZoom.ZoomBorder.ZoomXProperty)
                                {
                                    path.StrokeThickness = 5 / zom.ZoomX;
                                }
                            };

                            if (isClosed)
                            {
                                Avalonia.Media.IBrush selectionChildBrush = window.SelectionChildBrush;
                                List<string> tipsInside = new List<string>();
                                List<string> idsInside = new List<string>();
                                foreach (KeyValuePair<string, Point> kvp in window.Coordinates)
                                {
                                    if (path.RenderedGeometry.FillContains(new Avalonia.Point(kvp.Value.X - window.PlotOrigin.X + 10, kvp.Value.Y - window.PlotOrigin.Y + 10)))
                                    {
                                        TreeNode node = window.TransformedTree.GetNodeFromId(kvp.Key);
                                        if (node != null && node.Children.Count == 0)
                                        {
                                            if (!string.IsNullOrEmpty(node.Name))
                                            {
                                                tipsInside.Add(node.Name);
                                                idsInside.Add(node.Id);
                                            }
                                        }
                                    }
                                }

                                foreach (string id in idsInside)
                                {
                                    foreach ((double, RenderAction) selPath in MainWindow.FindPaths(window.SelectionCanvas, id))
                                    {
                                        Canvas can = selPath.Item2.Parent as Canvas;

                                        while (can != window.SelectionCanvas)
                                        {
                                            can.ZIndex = 90;
                                            can = can.Parent as Canvas;
                                        }

                                        if (selPath.Item2.Fill != null)
                                        {
                                            window.ChangeActionFill(selPath.Item2, selectionChildBrush);
                                        }

                                        if (selPath.Item2.Stroke != null)
                                        {
                                            window.ChangeActionStroke(selPath.Item2, selectionChildBrush);
                                        }
                                    }
                                }

                                if (tipsInside.Count > 0)
                                {
                                    _ = Avalonia.Application.Current.Clipboard.SetTextAsync(tipsInside.Aggregate((a, b) => a + "\n"+ b));

                                    Border bord = new Border(){ VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(10, 10, 10, 20), Padding = new Avalonia.Thickness(50, 10), CornerRadius = new Avalonia.CornerRadius(10), Background = window.SelectionChildBrush, BorderBrush = window.SelectionBrush, BorderThickness = new Avalonia.Thickness(2) };
                                    bord.Child= new TextBlock(){ Text = "The " + tipsInside.Count.ToString() + " selected tips have been copied to the clipboard!" };
                                    Grid.SetColumn(bord, 2);
                                    Grid.SetColumnSpan(bord, Grid.GetColumnSpan(zom));
                                    Grid.SetRow(bord, 1);
                                    bord.Transitions = new Avalonia.Animation.Transitions();
                                    bord.Transitions.Add(new Avalonia.Animation.DoubleTransition(){ Property = Avalonia.Controls.Shapes.Path.OpacityProperty, Duration = new TimeSpan(15000000) });
                                    window.FindControl<Grid>("MainGrid").Children.Add(bord);

                                    System.Threading.Thread thr = new System.Threading.Thread(async () =>
                                    {
                                        System.Threading.Thread.Sleep(1000);

                                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            window.FindControl<Grid>("MainGrid").Children.Remove(lassoBord);
                                            ((Canvas)window.SelectionCanvas.Parent).Children.Remove(can);
                                            bord.Opacity = 0;
                                        });

                                        System.Threading.Thread.Sleep(2000);

                                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            window.FindControl<Grid>("MainGrid").Children.Remove(bord);
                                        });

                                        stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = false;
                                    });
                                    thr.Start();
                                }
                                else
                                {
                                    System.Threading.Thread thr = new System.Threading.Thread(async () =>
                                    {
                                        System.Threading.Thread.Sleep(1000);

                                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                        {
                                            window.FindControl<Grid>("MainGrid").Children.Remove(lassoBord);
                                            ((Canvas)window.SelectionCanvas.Parent).Children.Remove(can);
                                        });

                                        stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = false;
                                    });
                                    thr.Start();
                                }

                                can.PointerPressed -= pointerPressed;

                                window.HasPointerDoneSomething = true;

                                path.Transitions = new Avalonia.Animation.Transitions();
                                path.Transitions.Add(new Avalonia.Animation.DoubleTransition(){ Property = Avalonia.Controls.Shapes.Path.OpacityProperty, Duration = new TimeSpan(5000000) });

                                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { path.Opacity = 0; });
                                lassoBord.Opacity = 0;
                            }

                            zom.PropertyChanged += zoomHandler;

                            can.DetachedFromLogicalTree += (s, e) =>
                            {
                                zom.PropertyChanged -= zoomHandler;
                            };
                        })
                    }
                }
                ));
            };

            can.PointerPressed += pointerPressed;
        }

    }
}
