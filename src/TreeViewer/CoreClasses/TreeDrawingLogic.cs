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
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhyloTree;
using VectSharp;
using VectSharp.Canvas;
using Avalonia.VisualTree;

namespace TreeViewer
{
    public partial class MainWindow
    {
        private static Page alertPage = null;

        //Colour graphBackgroundColour = Colour.FromRgb(255, 255, 255);
        Avalonia.Media.IBrush graphBackgroundBrush = Avalonia.Media.Brushes.White;

        Colour GraphBackground
        {
            get
            {
                return this.StateData.GraphBackgroundColour;
            }

            set
            {
                this.StateData.GraphBackgroundColour = value;

                graphBackgroundBrush = value.GetColourBrush();

                this.FindControl<Canvas>("ContainerCanvas").Background = GraphBackgroundBrush;

                //graphBackgroundBrush = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb((byte)(value.R * 255), (byte)(value.G * 255), (byte)(value.B * 255)));
            }
        }

        Avalonia.Media.IBrush GraphBackgroundBrush
        {
            get
            {
                return graphBackgroundBrush;
            }
            /*
            set
            {
                graphBackgroundColour = Colour.FromRgb(value.Color.R, value.Color.G, value.Color.B);
                graphBackgroundBrush = value;
            }*/
        }

        public Avalonia.Media.IBrush SelectionBrush
        {
            get
            {
                double transp = 0.5 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return new SolidColorBrush(Color.FromArgb((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255)));
            }
        }

        public Avalonia.Media.IBrush SelectionChildBrush
        {
            get
            {
                double transp = 0.15 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return new SolidColorBrush(Color.FromArgb((byte)(a * 255), (byte)(r * 255), (byte)(g * 255), (byte)(b * 255)));
            }
        }

        public Colour SelectionColour
        {
            get
            {
                double transp = 0.5 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return Colour.FromRgba(r, g, b, a);
            }
        }

        public Colour SelectionChildColour
        {
            get
            {
                double transp = 0.15 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return Colour.FromRgba(r, g, b, a);
            }
        }

        public static Page AlertPage
        {
            get
            {
                if (alertPage != null)
                {
                    return alertPage;
                }
                else
                {
                    alertPage = new Page(24, 24);

                    Graphics gpr = alertPage.Graphics;

                    gpr.FillPath(new GraphicsPath().MoveTo(12, 2).LineTo(2, 21).LineTo(21, 21).Close(), Colour.FromRgb(255, 201, 14));
                    gpr.StrokePath(new GraphicsPath().MoveTo(12, 2).LineTo(2, 21).LineTo(21, 21).Close(), Colour.FromRgb(255, 201, 14), lineWidth: 4, lineJoin: LineJoins.Round);
                    gpr.FillPath(new GraphicsPath().AddText(10.5, 13, "!", new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 18), TextBaselines.Middle), Colour.FromRgb(0, 0, 0));

                    return alertPage;
                }
            }
        }

        private async Task UpdateOnlyTransformedTree()
        {
            EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

            ProgressWindow win = null;

            win = new ProgressWindow(handle) { IsIndeterminate = true, ProgressText = "Transforming trees..." };

            Thread thr = new Thread(() =>
            {
                handle.WaitOne();
                FirstTransformedTree = Modules.TransformerModules[TransformerComboBox.SelectedIndex].Transform(Trees, TransformerParameters);
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    win.Close();
                });
            });

            thr.Start();

            await win.ShowDialog2(this);
        }

        private async Task UpdateTransformedTree()
        {
            try
            {
                await UpdateOnlyTransformedTree();

                TransformerAlert.IsVisible = false;
            }
            catch (Exception ex)
            {
                TransformerAlert.IsVisible = true;
                Avalonia.Controls.ToolTip.SetTip(TransformerAlert, ex.Message);
                return;
            }

            await UpdateFurtherTransformations(0);
        }

        TreeNode[] AllTransformedTrees = null;

        object FurtherTransformationLock = new object();

        private Task UpdateOnlyFurtherTransformations(int minIndex, ProgressWindow progressWindow)
        {
            lock (FurtherTransformationLock)
            {
                TreeNode[] prevTransformedTrees = AllTransformedTrees;

                AllTransformedTrees = new TreeNode[FurtherTransformations.Count];

                for (int i = 0; i < minIndex; i++)
                {
                    AllTransformedTrees[i] = prevTransformedTrees[i];
                }

                if (minIndex > 0)
                {
                    if (minIndex < prevTransformedTrees.Length)
                    {
                        TransformedTree = prevTransformedTrees[minIndex];
                    }
                    else
                    {
                        TransformedTree = TransformedTree.Clone();
                    }
                }
                else
                {
                    TransformedTree = FirstTransformedTree.Clone();
                }

                for (int i = minIndex; i < FurtherTransformations.Count; i++)
                {
                    AllTransformedTrees[i] = TransformedTree.Clone();
                    try
                    {
                        FurtherTransformations[i].Transform(ref StateData.TransformedTree, FurtherTransformationsParameters[i]);
                        double progress = (double)(i + 1) / (FurtherTransformations.Count - minIndex);
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            FurtherTransformationsAlerts[i].IsVisible = false;
                            progressWindow.Progress = progress;
                        });
                    }
                    catch (Exception ex)
                    {
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            FurtherTransformationsAlerts[i].IsVisible = true;
                            ToolTip.SetTip(FurtherTransformationsAlerts[i], ex.Message);
                        });
                    }
                }

                SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

                _ = Dispatcher.UIThread.InvokeAsync(() =>
                {
                    string[] selectedAttributes = new string[AttributeSelectors.Count];

                    for (int i = 0; i < AttributeSelectors.Count; i++)
                    {
                        if (AttributeSelectors[i].SelectedIndex >= 0 && AttributeSelectors[i].SelectedIndex < AttributeList.Count)
                        {
                            selectedAttributes[i] = AttributeList[AttributeSelectors[i].SelectedIndex];
                        }
                        else
                        {
                            selectedAttributes[i] = AttributeList[0];
                        }
                    }

                    List<TreeNode> nodes = TransformedTree.GetChildrenRecursive();

                    HashSet<string> allAttributes = new HashSet<string>();
                    foreach (TreeNode node in nodes)
                    {
                        foreach (KeyValuePair<string, object> kvp in node.Attributes)
                        {
                            allAttributes.Add(kvp.Key);
                        }
                    }

                    AttributeList = new List<string>(allAttributes);
                    AttributeList.Sort();


                    List<ComboBox> selectorsToRemove = new List<ComboBox>();

                    for (int i = 0; i < AttributeSelectors.Count; i++)
                    {
                        if (AttributeSelectors[i].FindAncestorOfType<MainWindow>() == this)
                        {
                            AttributeSelectors[i].Tag = true;
                            AttributeSelectors[i].Items = AttributeList;
                            AttributeSelectors[i].Tag = false;
                            int ind = AttributeList.IndexOf(selectedAttributes[i]);
                            AttributeSelectors[i].SelectedIndex = Math.Max(0, ind);
                        }
                        else
                        {
                            selectorsToRemove.Add(AttributeSelectors[i]);
                        }
                    }

                    foreach (ComboBox box in selectorsToRemove)
                    {
                        AttributeSelectors.Remove(box);
                    }

                    semaphore.Release();
                });

                semaphore.Wait();
                semaphore.Release();
            }

            return Task.CompletedTask;
        }

        public async Task UpdateFurtherTransformations(int minIndex)
        {
            minIndex = Math.Max(minIndex, 0);

            ProgressWindow window = new ProgressWindow() { ProgressText = "Performing further transformations...", IsIndeterminate = false };
            _ = window.ShowDialog(this);

            SemaphoreSlim semaphore2 = new SemaphoreSlim(0, 1);

            Thread thr = new Thread(async () =>
            {
                await UpdateOnlyFurtherTransformations(minIndex, window);
                semaphore2.Release();
            });
            thr.Start();

            await semaphore2.WaitAsync();
            semaphore2.Release();

            window.Close();

            UpdateCoordinates();
        }

        private void UpdateOnlyCoordinates()
        {
            Coordinates = Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].GetCoordinates(TransformedTree, CoordinatesParameters);
        }

        public void UpdateCoordinates()
        {
            try
            {
                UpdateOnlyCoordinates();
                CoordinatesAlert.IsVisible = false;
            }
            catch (Exception ex)
            {
                CoordinatesAlert.IsVisible = true;
                Avalonia.Controls.ToolTip.SetTip(CoordinatesAlert, ex.Message);
                return;
            }

            UpdateAllPlotLayers();
        }

        public Point PlotOrigin;
        public Point PlotBottomRight;
        private List<(Point, Point)> PlotBounds { get; set; }
        private List<Canvas> PlotCanvases { get; set; }
        private List<Canvas> SelectionCanvases;

        private void UpdatePlotLayer(int layer, bool updatePlotBounds)
        {
            if (layer >= 0)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").IsVisible = true;

                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;


                try
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    Page pag = new Page(1, 1);
                    Graphics plotGraphics = pag.Graphics;
                    Point[] bounds = PlottingActions[layer].PlotAction(TransformedTree, PlottingParameters[layer], Coordinates, plotGraphics);
                    minX = Math.Min(minX, bounds[0].X);
                    maxX = Math.Max(maxX, bounds[1].X);
                    minY = Math.Min(minY, bounds[0].Y);
                    maxY = Math.Max(maxY, bounds[1].Y);

                    pag.Crop(new Point(minX, minY), new Size(maxX - minX, maxY - minY));

                    Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>();

                    Dictionary<string, Delegate> selectionActions = new Dictionary<string, Delegate>()
                    {
                        { "", new Func<RenderAction, IEnumerable<RenderAction>>((path) => { return new RenderAction[0]; }) }
                    };

                    List<TreeNode> nodes = TransformedTree.GetChildrenRecursive();

                    ISolidColorBrush transparentBrush = new SolidColorBrush(0x00000000);

                    Dictionary<string, List<(double, RenderAction)>> selectionItems = new Dictionary<string, List<(double, RenderAction)>>();

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        int index = i;
                        /*taggedActions.Add(nodes[i].Id, new Action<Control>((path) =>
                        {
                            path.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);

                            path.PointerPressed += (s, e) =>
                            {
                                SetSelection(nodes[index]);
                                HasPointerDoneSomething = true;
                            };
                        }));*/

                        taggedActions.Add(nodes[i].Id, new Func<RenderAction, IEnumerable<RenderAction>>((path) =>
                        {
                            path.PointerEnter += (s, e) =>
                            {
                                path.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                            };

                            path.PointerLeave += (s, e) =>
                            {
                                path.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                            };

                            path.PointerPressed += (s, e) =>
                            {
                                SetSelection(nodes[index]);
                                HasPointerDoneSomething = true;
                            };

                            return new RenderAction[] { path };

                        }));

                        /*selectionActions.Add(nodes[i].Id, new Action<Control>((ctrl) =>
                        {
                            ctrl.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);

                            if (ctrl is Avalonia.Controls.Shapes.Path path)
                            {
                                path.Tag = new object[] { nodes[index].Id, path.StrokeThickness };
                                path.Stroke = transparentBrush;

                                if (path.Fill != null)
                                {
                                    path.Fill = transparentBrush;
                                }

                                path.StrokeLineCap = Avalonia.Media.PenLineCap.Round;
                                path.StrokeJoin = Avalonia.Media.PenLineJoin.Round;
                            }
                            else if (ctrl is Avalonia.Controls.TextBlock block)
                            {
                                Canvas parent = (Canvas)block.Parent;

                                parent.Children.Remove(block);

                                Geometry geo = new RectangleGeometry(block.FormattedText.Bounds);

                                parent.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = geo, Fill = transparentBrush, RenderTransform = block.RenderTransform, RenderTransformOrigin = block.RenderTransformOrigin, Tag = new object[] { nodes[index].Id, 0d } });
                            }

                            ctrl.PointerPressed += (s, e) =>
                            {
                                SetSelection(nodes[index]);
                                HasPointerDoneSomething = true;
                            };
                        }));*/

                        selectionActions.Add(nodes[i].Id, new Func<RenderAction, IEnumerable<RenderAction>>(ctrl =>
                        {
                            if (ctrl.ActionType == RenderAction.ActionTypes.Path)
                            {
                                ctrl.PointerEnter += (s, e) =>
                                {
                                    ctrl.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                                };

                                ctrl.PointerLeave += (s, e) =>
                                {
                                    ctrl.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                                };

                                if (ctrl.Stroke != null)
                                {
                                    ctrl.Stroke.Brush = transparentBrush;
                                    ctrl.Stroke.LineCap = PenLineCap.Round;
                                    ctrl.Stroke.LineJoin = PenLineJoin.Round;
                                }
                                else
                                {
                                    ctrl.Stroke = new Pen(transparentBrush, thickness: 0, lineCap: PenLineCap.Round, lineJoin: PenLineJoin.Round);
                                }

                                if (ctrl.Fill != null)
                                {
                                    ctrl.Fill = transparentBrush;
                                }

                                ctrl.PointerPressed += (s, e) => { SetSelection(nodes[index]); HasPointerDoneSomething = true; };

                                if (selectionItems.TryGetValue(nodes[index].Id, out List<(double, RenderAction)> item))
                                {
                                    selectionItems[nodes[index].Id].Add((ctrl.Stroke.Thickness, ctrl));
                                }
                                else
                                {
                                    selectionItems.Add(nodes[index].Id, new List<(double, RenderAction)>() { (ctrl.Stroke.Thickness, ctrl) });
                                }

                                return new RenderAction[] { ctrl };
                            }
                            else if (ctrl.ActionType == RenderAction.ActionTypes.Text)
                            {
                                Geometry geo = new RectangleGeometry(ctrl.Text.Bounds);

                                RenderAction act = RenderAction.PathAction(geo, null, transparentBrush, ctrl.Transform, ctrl.ClippingPath, tag: ctrl.Tag);

                                act.PointerEnter += (s, e) =>
                                {
                                    act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                                };

                                act.PointerLeave += (s, e) =>
                                {
                                    act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                                };

                                act.PointerPressed += (s, e) => { SetSelection(nodes[index]); HasPointerDoneSomething = true; };

                                if (selectionItems.TryGetValue(nodes[index].Id, out List<(double, RenderAction)> item))
                                {
                                    item.Add((0, act));
                                }
                                else
                                {
                                    selectionItems.Add(nodes[index].Id, new List<(double, RenderAction)>() { (0, act) });
                                }

                                return new RenderAction[] { act };

                            }
                            else
                            {
                                return null;
                            }

                        }));
                    }


                    //Canvas newCanvas = pag.PaintToCanvas(taggedActions, false);
                    Canvas newCanvas = pag.PaintToCanvas(false, taggedActions, false);
                    newCanvas.Background = null;
                    newCanvas.Width = 1;
                    newCanvas.Height = 1;
                    newCanvas.ClipToBounds = false;
                    /*Needed until Avalonia bug https://github.com/AvaloniaUI/Avalonia/issues/3732 is fixed*/
                    newCanvas.IsHitTestVisible = false;

                    //Canvas newSelectionCanvas = pag.PaintToCanvas(selectionActions, false);
                    Canvas newSelectionCanvas = pag.PaintToCanvas(false, selectionActions, false);
                    newSelectionCanvas.Background = null;
                    newSelectionCanvas.Width = 1;
                    newSelectionCanvas.Height = 1;
                    newSelectionCanvas.ClipToBounds = false;
                    newSelectionCanvas.Tag = selectionItems;


                    /*foreach (Avalonia.Controls.Shapes.Path pth in FindPaths(newCanvas))
                    {
                        if (pth.Tag == null)
                        {
                            pth.IsHitTestVisible = false;
                        }
                    }

                    foreach (Avalonia.Controls.Shapes.Path pth in FindPaths(newSelectionCanvas))
                    {
                        if (pth.Tag == null)
                        {
                            (pth.Parent as Canvas).Children.Remove(pth);
                        }
                    }*/

                    Point newOrigin = new Point(Math.Min(minX, PlotOrigin.X), Math.Min(minY, PlotOrigin.Y));

                    if (double.IsNaN(PlotOrigin.X) || double.IsNaN(PlotOrigin.Y))
                    {
                        newOrigin = new Point(minX, minY);
                    }

                    PlotBottomRight = new Point(Math.Max(maxX, PlotBottomRight.X), Math.Max(maxY, PlotBottomRight.Y));
                    if (double.IsNaN(PlotBottomRight.X) || double.IsNaN(PlotBottomRight.Y))
                    {
                        PlotBottomRight = new Point(maxX, maxY);
                    }

                    if (newOrigin.Y != PlotOrigin.Y || newOrigin.X != PlotOrigin.X)
                    {
                        for (int i = 0; i < PlotCanvases.Count; i++)
                        {
                            if (i != layer && PlotCanvases[i] != null)
                            {
                                TranslateTransform prevTransform = (TranslateTransform)PlotCanvases[i].RenderTransform;
                                PlotCanvases[i].RenderTransform = new TranslateTransform(prevTransform.X - newOrigin.X + PlotOrigin.X, prevTransform.Y - newOrigin.Y + PlotOrigin.Y);
                                SelectionCanvases[i].RenderTransform = new TranslateTransform(prevTransform.X - newOrigin.X + PlotOrigin.X, prevTransform.Y - newOrigin.Y + PlotOrigin.Y);
                            }
                        }

                        PlotOrigin = newOrigin;
                    }

                    newCanvas.RenderTransform = new TranslateTransform(minX - PlotOrigin.X, minY - PlotOrigin.Y);
                    newSelectionCanvas.RenderTransform = new TranslateTransform(minX - PlotOrigin.X, minY - PlotOrigin.Y);

                    Canvas parent = this.FindControl<Canvas>("PlotCanvas");
                    Canvas selectionParent = this.FindControl<Canvas>("SelectionCanvas");

                    if (PlotCanvases[layer] != null)
                    {
                        int index = parent.Children.IndexOf(PlotCanvases[layer]);

                        parent.Children.RemoveAt(index);
                        parent.Children.Insert(index, newCanvas);

                        selectionParent.Children.RemoveAt(index);
                        selectionParent.Children.Insert(index, newSelectionCanvas);

                        PlotCanvases[layer] = newCanvas;
                        PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                        SelectionCanvases[layer] = newSelectionCanvas;
                    }
                    else
                    {
                        parent.Children.Add(newCanvas);
                        selectionParent.Children.Add(newSelectionCanvas);

                        PlotCanvases[layer] = newCanvas;
                        PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                        SelectionCanvases[layer] = newSelectionCanvas;
                    }

                    PlottingAlerts[layer].IsVisible = false;

                    sw.Stop();
                    //PlottingTimings[layer].Text = sw.ElapsedMilliseconds < 2000 ? (sw.ElapsedMilliseconds.ToString() + "ms") : ((sw.ElapsedMilliseconds / 1000.0).ToString("0.00") + "ms");

                    Canvas containerCanvas = this.FindControl<Canvas>("ContainerCanvas");
                    containerCanvas.Background = GraphBackgroundBrush;
                    containerCanvas.Width = PlotBottomRight.X - PlotOrigin.X + 20;
                    containerCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y + 20;

                    UpdateSelectionWidth(newSelectionCanvas);
                }
                catch (Exception ex)
                {
                    try
                    {
                        PlottingAlerts[layer].IsVisible = true;
                        ToolTip.SetTip(PlottingAlerts[layer], ex.Message);
                        //PlottingTimings[layer].Text = "?ms";
                    }
                    catch { }
                }
            }
            ActionsWhoseColourHasBeenChanged.RemoveWhere(act => !SelectionCanvas.Children.Contains(act.Parent));

            if (updatePlotBounds)
            {
                UpdatePlotBounds();
            }
        }

        private void UpdatePlotBounds()
        {
            double minX = double.MaxValue;
            double minY = double.MaxValue;
            double maxX = double.MinValue;
            double maxY = double.MinValue;
            
            for (int i = 0; i < PlotBounds.Count; i++)
            {
                minX = Math.Min(minX, PlotBounds[i].Item1.X);
                minY = Math.Min(minY, PlotBounds[i].Item1.Y);

                maxX = Math.Max(maxX, PlotBounds[i].Item2.X);
                maxY = Math.Max(maxY, PlotBounds[i].Item2.Y);
            }

            Point newOrigin = new Point(minX, minY);

            for (int i = 0; i < PlotCanvases.Count; i++)
            {
                if (PlotCanvases[i] != null)
                {
                    TranslateTransform prevTransform = (TranslateTransform)PlotCanvases[i].RenderTransform;
                    PlotCanvases[i].RenderTransform = new TranslateTransform(prevTransform.X - newOrigin.X + PlotOrigin.X, prevTransform.Y - newOrigin.Y + PlotOrigin.Y);
                    SelectionCanvases[i].RenderTransform = new TranslateTransform(prevTransform.X - newOrigin.X + PlotOrigin.X, prevTransform.Y - newOrigin.Y + PlotOrigin.Y);
                }
            }

            PlotOrigin = newOrigin;
            PlotBottomRight = new Point(maxX, maxY);

            Canvas containerCanvas = this.FindControl<Canvas>("ContainerCanvas");
            containerCanvas.Width = PlotBottomRight.X - PlotOrigin.X + 20;
            containerCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y + 20;
        }

        private void UpdateAllPlotLayers()
        {
            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").IsVisible = true;

            PlotBounds = new List<(Point, Point)>(new (Point, Point)[PlottingActions.Count]);
            PlotCanvases = new List<Canvas>(new Canvas[PlottingActions.Count]);
            this.FindControl<Canvas>("PlotCanvas").Children.Clear();

            SelectionCanvases = new List<Canvas>(new Canvas[PlottingActions.Count]);
            this.FindControl<Canvas>("SelectionCanvas").Children.Clear();

            PlotOrigin = new Point(double.NaN, double.NaN);
            PlotBottomRight = new Point(double.NaN, double.NaN);

            for (int i = 0; i < PlottingActions.Count; i++)
            {
                UpdatePlotLayer(i, false);
            }

            UpdatePlotBounds();
        }

        public Page RenderPlotToPage()
        {
            UpdateOnlyCoordinates();

            Page pag = new Page(1, 1) { Background = StateData.GraphBackgroundColour };

            if (PlottingActions.Count > 0)
            {
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                double minX = double.MaxValue;
                double minY = double.MaxValue;

                for (int i = 0; i < PlottingActions.Count; i++)
                {
                    Point[] bounds = PlottingActions[i].PlotAction(TransformedTree, PlottingParameters[i], Coordinates, pag.Graphics);
                    minX = Math.Min(minX, bounds[0].X);
                    maxX = Math.Max(maxX, bounds[1].X);
                    minY = Math.Min(minY, bounds[0].Y);
                    maxY = Math.Max(maxY, bounds[1].Y);
                }

                pag.Crop(new Point(minX - 10, minY - 10), new Size(maxX - minX + 20, maxY - minY + 20));
            }

            return pag;
        }

        public static List<(double, RenderAction)> FindPaths(Canvas can, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                List<(double, RenderAction)> tbr = new List<(double, RenderAction)>();

                if (can == null)
                {
                    return tbr;
                }

                if (can.Tag is Dictionary<string, List<(double, RenderAction)>> dict)
                {
                    foreach (KeyValuePair<string, List<(double, RenderAction)>> kvp in dict)
                    {
                        tbr.AddRange(kvp.Value);
                    }
                }

                for (int i = 0; i < can.Children.Count; i++)
                {
                    if (can.Children[i] is Canvas ccan)
                    {
                        tbr.AddRange(FindPaths(ccan, id));
                    }
                }

                return tbr;
            }
            else
            {
                List<(double, RenderAction)> tbr = new List<(double, RenderAction)>();

                if (can == null)
                {
                    return tbr;
                }

                if (can.Tag is Dictionary<string, List<(double, RenderAction)>> dict && dict.TryGetValue(id, out List<(double, RenderAction)> list))
                {
                    tbr.AddRange(list);
                }

                for (int i = 0; i < can.Children.Count; i++)
                {
                    if (can.Children[i] is Canvas ccan)
                    {
                        tbr.AddRange(FindPaths(ccan, id));
                    }
                }

                return tbr;
            }


        }
    }
}
