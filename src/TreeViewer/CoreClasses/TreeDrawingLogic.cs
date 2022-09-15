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
using System.Linq;
using Avalonia.Media.Transformation;

namespace TreeViewer
{
    public partial class MainWindow
    {
        Avalonia.Media.IBrush graphBackgroundBrush = Avalonia.Media.Brushes.White;

        private bool SettingGraphBackground = false;

        public Colour GraphBackground
        {
            get
            {
                return this.StateData.GraphBackgroundColour;
            }

            set
            {
                SettingGraphBackground = true;

                this.StateData.GraphBackgroundColour = value;

                graphBackgroundBrush = value.GetColourBrush();

                if (GraphBackgroundButton != null)
                {
                    GraphBackgroundButton.Color = value.ToAvalonia();
                }

                this.FindControl<Canvas>("ContainerCanvas").Background = GraphBackgroundBrush;

                SettingGraphBackground = false;
            }
        }

        Avalonia.Media.IBrush GraphBackgroundBrush
        {
            get
            {
                return graphBackgroundBrush;
            }
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

        public SkiaSharp.SKColor SelectionSKColor
        {
            get
            {
                double transp = 0.5 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return new SkiaSharp.SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
            }
        }

        public SkiaSharp.SKColor SelectionChildSKColor
        {
            get
            {
                double transp = 0.15 * GlobalSettings.Settings.SelectionColour.A;
                double r = (GlobalSettings.Settings.SelectionColour.R * transp + this.StateData.GraphBackgroundColour.R * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double g = (GlobalSettings.Settings.SelectionColour.G * transp + this.StateData.GraphBackgroundColour.G * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double b = (GlobalSettings.Settings.SelectionColour.B * transp + this.StateData.GraphBackgroundColour.B * this.StateData.GraphBackgroundColour.A * (1 - transp)) / (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));
                double a = (transp + this.StateData.GraphBackgroundColour.A * (1 - transp));

                return new SkiaSharp.SKColor((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
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

        public static DPIAwareBox GetAlertIcon()
        {
            return new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Warning"));
        }

        private bool TryRecoverSelectedNode(string id, List<string> nodeNames)
        {
            if (id != null)
            {
                TreeNode node = this.TransformedTree.GetNodeFromId(id);

                if (node != null)
                {
                    this.SetSelection(node);
                    return true;
                }
            }

            if (nodeNames?.Count > 0)
            {
                TreeNode node = this.TransformedTree.GetLastCommonAncestor(nodeNames);

                if (node != null)
                {
                    this.SetSelection(node);
                    return true;
                }
            }

            return false;
        }

        private async Task UpdateOnlyTransformedTree(ProgressWindow win = null)
        {
            if (!StopAllUpdates)
            {
                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

                bool wasWinNull = win == null;

                if (win == null)
                {
                    win = new ProgressWindow(handle) { IsIndeterminate = false, ProgressText = "Transforming trees..." };
                    win.LabelText = Modules.TransformerModules[TransformerComboBox.SelectedIndex].Name;
                }
                else
                {
                    win.IsIndeterminate = false;
                    win.ProgressText = "Transforming trees...";
                    win.LabelText = Modules.TransformerModules[TransformerComboBox.SelectedIndex].Name;
                }

                Task task = Task.Run(() =>
                {
                    handle.WaitOne();
                    FirstTransformedTree = Modules.TransformerModules[TransformerComboBox.SelectedIndex].Transform(Trees, TransformerParameters, progress =>
                    {
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            win.Progress = progress;
                        });
                    });
                });

                if (wasWinNull)
                {
                    _ = win.ShowDialog2(this);
                }
                else
                {
                    handle.Set();
                }

                await task;

                if (wasWinNull)
                {
                    win.Close();
                }
            }
        }

        private async Task UpdateTransformedTree()
        {
            if (!StopAllUpdates)
            {
                ProgressWindow win = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Transforming trees..." };
                win.LabelText = Modules.TransformerModules[TransformerComboBox.SelectedIndex].Name;

                _ = win.ShowDialog2(this);

                try
                {
                    await UpdateOnlyTransformedTree(win);
                    TransformerAlert.IsVisible = false;
                    CurrentExceptions.Remove((string)TransformerParameters[Modules.ModuleIDKey]);
                    UpdateWarningVisibility();
                }
                catch (Exception ex)
                {
                    TransformerAlert.IsVisible = true;
                    string message = GetExceptionMessage(ex);
                    AvaloniaBugFixes.SetToolTip(TransformerAlert, message);
                    CurrentExceptions[(string)TransformerParameters[Modules.ModuleIDKey]] = (Modules.TransformerModules[TransformerComboBox.SelectedIndex], message);
                    UpdateWarningVisibility();
                    win.Close();
                    return;
                }

                await UpdateFurtherTransformations(0, win);
            }
        }

        public TreeNode[] AllTransformedTrees = null;

        object FurtherTransformationLock = new object();

        private async Task UpdateOnlyFurtherTransformations(int minIndex, ProgressWindow progressWindow, Action<double> externalProgressAction = null)
        {
            if (!StopAllUpdates)
            {
                string id = null;
                List<string> nodeNames = null;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (this.IsSelectionAvailable)
                    {
                        nodeNames = this.SelectedNode.GetNodeNames();
                        id = this.SelectedNode.Id;
                    }
                });

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

                    if (externalProgressAction == null)
                    {
                        _ = Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            progressWindow.Steps = FurtherTransformations.Count - minIndex;

                        });
                    }

                    for (int i = minIndex; i < FurtherTransformations.Count; i++)
                    {
                        AllTransformedTrees[i] = TransformedTree.Clone();
                        int j = i;
                        try
                        {
                            if (externalProgressAction == null)
                            {
                                _ = Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    progressWindow.LabelText = FurtherTransformations[j].Name;

                                });
                            }

                            double progress = (double)(i - minIndex) / (FurtherTransformations.Count - minIndex);

                            Action<double> progressAction;


                            if (externalProgressAction == null)
                            {
                                progressAction = (prog) =>
                                {
                                    prog = Math.Max(0, Math.Min(prog, 1));

                                    _ = Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        progressWindow.Progress = progress + prog / (FurtherTransformations.Count - minIndex);
                                    });
                                };
                            }
                            else
                            {
                                progressAction = externalProgressAction;
                            }

                            FurtherTransformations[i].Transform(ref StateData.TransformedTree, FurtherTransformationsParameters[i], progressAction);

                            progress = (double)(i + 1 - minIndex) / (FurtherTransformations.Count - minIndex);

                            _ = Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                FurtherTransformationsAlerts[j].IsVisible = false;

                                if (externalProgressAction == null)
                                {
                                    progressWindow.Progress = progress;
                                }

                                CurrentExceptions.Remove((string)FurtherTransformationsParameters[j][Modules.ModuleIDKey]);
                                UpdateWarningVisibility();
                            });
                        }
                        catch (Exception ex)
                        {
                            _ = Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                FurtherTransformationsAlerts[j].IsVisible = true;

                                string message = GetExceptionMessage(ex);
                                AvaloniaBugFixes.SetToolTip(FurtherTransformationsAlerts[j], message);

                                CurrentExceptions[(string)FurtherTransformationsParameters[j][Modules.ModuleIDKey]] = (FurtherTransformations[j], message);
                                UpdateWarningVisibility();
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

                _ = Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    this.FindControl<TextBlock>("TreeCountLabel").Text = this.Trees.Count.ToString() + " tree" + (this.Trees.Count > 1 ? "s" : "") + " " + (this.TransformedTree.Children.Count == 2 ? "(rooted)" : "(unrooted)");
                    this.FindControl<TextBlock>("TipCountLabel").Text = this.TransformedTree.GetLeaves().Count.ToString() + " tips";
                    this.FindControl<TextBlock>("NodeCountLabel").Text = this.TransformedTree.GetChildrenRecursiveLazy().Count().ToString() + " nodes";

                    if (id != null || nodeNames != null)
                    {
                        await Task.Delay(10);
                        TryRecoverSelectedNode(id, nodeNames);
                    }
                }, DispatcherPriority.MinValue);
            }
        }

        private void UpdateWarningVisibility()
        {
            if (CurrentExceptions.Count == 0 && CurrentWarnings.Count == 0)
            {
                this.FindControl<Button>("WarningButton").IsVisible = false;
            }
            else if (CurrentExceptions.Count > 0 && CurrentWarnings.Count == 0)
            {
                this.FindControl<Canvas>("WarningIconCanvas").IsVisible = true;
                this.FindControl<Canvas>("InfoIconCanvas").IsVisible = false;
                this.FindControl<TextBlock>("WarningCountBlock").Text = CurrentExceptions.Count.ToString();
                this.FindControl<Button>("WarningButton").IsVisible = true;
            }
            else if (CurrentExceptions.Count == 0 && CurrentWarnings.Count > 0)
            {
                this.FindControl<Canvas>("WarningIconCanvas").IsVisible = false;
                this.FindControl<Canvas>("InfoIconCanvas").IsVisible = true;
                this.FindControl<TextBlock>("WarningCountBlock").Text = CurrentWarnings.Count.ToString();
                this.FindControl<Button>("WarningButton").IsVisible = true;
            }
            else
            {
                this.FindControl<Canvas>("WarningIconCanvas").IsVisible = true;
                this.FindControl<Canvas>("InfoIconCanvas").IsVisible = true;
                this.FindControl<TextBlock>("WarningCountBlock").Text = (CurrentExceptions.Count + CurrentWarnings.Count).ToString();
                this.FindControl<Button>("WarningButton").IsVisible = true;
            }
        }

        private static string GetExceptionMessage(Exception exception)
        {

            string message = exception.Message;

            Exception currEx = exception;

            while (currEx.InnerException != null)
            {
                currEx = currEx.InnerException;
                message += "\n" + currEx.Message;
            }

            return message;
        }

        public async Task UpdateFurtherTransformations(int minIndex, ProgressWindow window = null)
        {
            if (!StopAllUpdates)
            {
                minIndex = Math.Max(minIndex, 0);

                if (window == null)
                {
                    window = new ProgressWindow() { ProgressText = "Performing further transformations...", IsIndeterminate = false };
                    window.Steps = FurtherTransformations.Count - minIndex;
                    _ = window.ShowDialog2(this);
                }
                else
                {
                    window.ProgressText = "Performing further transformations...";
                    window.IsIndeterminate = false;
                    window.Steps = FurtherTransformations.Count - minIndex;
                }

                await Task.Run(async () =>
                {
                    await UpdateOnlyFurtherTransformations(minIndex, window);
                });

                window.Close();

                this.FindControl<TextBlock>("TreeCountLabel").Text = this.Trees.Count.ToString() + " tree" + (this.Trees.Count > 1 ? "s" : "") + " " + (this.TransformedTree.Children.Count == 2 ? "(rooted)" : "(unrooted)");
                this.FindControl<TextBlock>("TipCountLabel").Text = this.TransformedTree.GetLeaves().Count.ToString() + " tips";
                this.FindControl<TextBlock>("NodeCountLabel").Text = this.TransformedTree.GetChildrenRecursiveLazy().Count().ToString() + " nodes";

                await UpdateCoordinates();
            }
        }

        private void UpdateOnlyCoordinates()
        {
            if (!StopAllUpdates)
            {
                Coordinates = Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].GetCoordinates(TransformedTree, CoordinatesParameters);
            }
        }

        public async Task UpdateCoordinates()
        {
            if (!StopAllUpdates)
            {
                try
                {
                    UpdateOnlyCoordinates();
                    CoordinatesAlert.IsVisible = false;
                    CurrentExceptions.Remove((string)CoordinatesParameters[Modules.ModuleIDKey]);
                    UpdateWarningVisibility();
                }
                catch (Exception ex)
                {
                    CoordinatesAlert.IsVisible = true;
                    string message = GetExceptionMessage(ex);
                    AvaloniaBugFixes.SetToolTip(CoordinatesAlert, message);
                    CurrentExceptions[(string)CoordinatesParameters[Modules.ModuleIDKey]] = (Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex], message);
                    UpdateWarningVisibility();
                    return;
                }

                await UpdateAllPlotLayers();
            }
        }

        public Point PlotOrigin;
        public Point PlotBottomRight;
        private List<(Point, Point)> PlotBounds { get; set; }
        private List<SKRenderContext> PlotCanvases { get; set; }
        private List<SKRenderAction> LayerTransforms { get; set; }
        private List<SKRenderContext> SelectionCanvases;

        static SkiaSharp.SKColor TransparentSKColor = new SkiaSharp.SKColor(0, 0, 0, 0);

        private Dictionary<string, (SkiaSharp.SKBitmap, bool)> GlobalImages = new Dictionary<string, (SkiaSharp.SKBitmap, bool)>();

        public SKMultiLayerRenderCanvas FullPlotCanvas;
        public SKMultiLayerRenderCanvas FullSelectionCanvas;

        private SemaphoreSlim RenderingSemaphore = new SemaphoreSlim(1, 1);

        private SemaphoreSlim RenderingUpdateRequestSemaphore = new SemaphoreSlim(1, 1);
        private List<(int, bool)> PlotLayerUpdateRequests = new List<(int, bool)>();
        private EventWaitHandle RenderingUpdateRequestHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
        private EventWaitHandle RenderingUpdateRequestTerminator = new EventWaitHandle(false, EventResetMode.ManualReset);

        public event EventHandler RenderingPassCompleted;

        private void StartPlotUpdaterThread()
        {
            Thread thr = new Thread(async () =>
            {
                EventWaitHandle[] handles = new EventWaitHandle[] { RenderingUpdateRequestHandle, RenderingUpdateRequestTerminator };

                while (true)
                {
                    int result = EventWaitHandle.WaitAny(handles);

                    if (result == 1)
                    {
                        break;
                    }
                    else
                    {
                        await RenderingUpdateRequestSemaphore.WaitAsync();

                        List<(int, bool)> currPlotLayerUpdateRequests = PlotLayerUpdateRequests;
                        PlotLayerUpdateRequests = new List<(int, bool)>();

                        RenderingUpdateRequestSemaphore.Release();
                        RenderingUpdateRequestHandle.Reset();

                        HashSet<int> updated = new HashSet<int>(currPlotLayerUpdateRequests.Count);

                        for (int i = 0; i < currPlotLayerUpdateRequests.Count; i++)
                        {
                            if (updated.Add(currPlotLayerUpdateRequests[i].Item1))
                            {
                                await Dispatcher.UIThread.InvokeAsync(async () =>
                                {
                                    await ActuallyUpdatePlotLayer(currPlotLayerUpdateRequests[i].Item1, currPlotLayerUpdateRequests[i].Item2);
                                });
                            }
                        }

                        RenderingPassCompleted?.Invoke(this, new EventArgs());

                        // Limit rendering updates to 30fps
                        await Task.Delay(33);
                    }
                }


            });

            thr.Start();
        }


        private async Task UpdatePlotLayer(int layer, bool updatePlotBounds)
        {
            if (!StopAllUpdates)
            {
                await RenderingUpdateRequestSemaphore.WaitAsync();

                PlotLayerUpdateRequests.Add((layer, updatePlotBounds));

                RenderingUpdateRequestHandle.Set();
                RenderingUpdateRequestSemaphore.Release();
            }
        }

        private async Task ActuallyUpdatePlotLayer(int layer, bool updatePlotBounds, bool updatingAllLayers = false)
        {
            if (!StopAllUpdates)
            {
                await RenderingSemaphore.WaitAsync();

                try
                {
                    if (layer >= 0)
                    {
                        this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").IsVisible = true;

                        double minX = double.MaxValue;
                        double maxX = double.MinValue;
                        double minY = double.MaxValue;
                        double maxY = double.MinValue;

                        if (!updatingAllLayers)
                        {
                            this.FindControl<CoolProgressBar>("PlotProgressBar").IntermediateSteps = System.Collections.Immutable.ImmutableList<double>.Empty;
                            this.FindControl<CoolProgressBar>("PlotProgressBar").Progress = 0;
                            this.FindControl<CoolProgressBar>("PlotProgressBar").LabelText = PlottingActions[layer].Name;

                            this.FindControl<CoolProgressBar>("PlotProgressBar").Opacity = 1;
                            this.RefreshAllButton.IsEnabled = false;
                        }
                        else
                        {
                            this.FindControl<CoolProgressBar>("PlotProgressBar").LabelText = PlottingActions[layer].Name;
                        }

                        SKRenderContext newContext = null;
                        SKRenderContext newSelectionContext = null;

                        bool success = false;

                        await Task.Run(() =>
                        {
                            try
                            {
                                Page pag = new Page(1, 1);
                                Graphics plotGraphics = pag.Graphics;

                                Point[] bounds = PlottingActions[layer].PlotAction(TransformedTree, PlottingParameters[layer], Coordinates, plotGraphics);
                                minX = Math.Min(minX, bounds[0].X);
                                maxX = Math.Max(maxX, bounds[1].X);
                                minY = Math.Min(minY, bounds[0].Y);
                                maxY = Math.Max(maxY, bounds[1].Y);

                                pag.Crop(new Point(minX, minY), new Size(maxX - minX, maxY - minY));

                                Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> taggedActions = new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>();

                                Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>> selectionActions = new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>()
                                {
                            { "", new Func<SKRenderAction, IEnumerable<SKRenderAction>>((path) =>
                                {
                                    if (path.ActionType == SKRenderAction.ActionTypes.Clip || path.ActionType == SKRenderAction.ActionTypes.Restore || path.ActionType == SKRenderAction.ActionTypes.Save || path.ActionType == SKRenderAction.ActionTypes.Transform)
                                    {
                                        return new SKRenderAction[] { path };
                                    }
                                    else
                                    {
                                        return new SKRenderAction[0];
                                    }
                                    }) }
                                };

                                List<TreeNode> nodes = TransformedTree.GetChildrenRecursive();

                                Dictionary<string, List<(double, SKRenderAction)>> selectionItems = new Dictionary<string, List<(double, SKRenderAction)>>();

                                for (int i = 0; i < nodes.Count; i++)
                                {
                                    int index = i;

                                    taggedActions.Add(nodes[i].Id, new Func<SKRenderAction, IEnumerable<SKRenderAction>>((path) =>
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
                                                FullSelectionCanvas.InvalidateDirty();
                                            };

                                            return new SKRenderAction[] { path };

                                        }));

                                    selectionActions.Add(nodes[i].Id, new Func<SKRenderAction, IEnumerable<SKRenderAction>>(ctrl =>
                                        {
                                            if (ctrl.ActionType == SKRenderAction.ActionTypes.Path)
                                            {
                                                ctrl.PointerEnter += (s, e) =>
                                                {
                                                    ctrl.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                                                };

                                                ctrl.PointerLeave += (s, e) =>
                                                {
                                                    ctrl.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                                                };

                                                if (ctrl.Paint.IsStroke)
                                                {
                                                    ctrl.Paint.Style = SkiaSharp.SKPaintStyle.Stroke;
                                                    ctrl.Paint.Color = TransparentSKColor;
                                                }
                                                else
                                                {
                                                    ctrl.Paint.Style = SkiaSharp.SKPaintStyle.StrokeAndFill;
                                                    ctrl.Paint.StrokeWidth = 0;
                                                    ctrl.Paint.Color = TransparentSKColor;
                                                }

                                                ctrl.Payload = (double)ctrl.Paint.StrokeWidth;

                                                ctrl.PointerPressed += (s, e) => { SetSelection(nodes[index]); HasPointerDoneSomething = true; };

                                                if (selectionItems.TryGetValue(nodes[index].Id, out List<(double, SKRenderAction)> item))
                                                {
                                                    selectionItems[nodes[index].Id].Add((ctrl.Paint.StrokeWidth, ctrl));
                                                }
                                                else
                                                {
                                                    selectionItems.Add(nodes[index].Id, new List<(double, SKRenderAction)>() { (ctrl.Paint.StrokeWidth, ctrl) });
                                                }

                                                return new SKRenderAction[] { ctrl };
                                            }
                                            else if (ctrl.ActionType == SKRenderAction.ActionTypes.Text)
                                            {
                                                SkiaSharp.SKRect bounds = new SkiaSharp.SKRect();
                                                ctrl.Paint.MeasureText(ctrl.Text, ref bounds);

                                                SkiaSharp.SKPath geo = new SkiaSharp.SKPath();

                                                geo.AddRect(new SkiaSharp.SKRect(bounds.Left + ctrl.TextX, bounds.Top + ctrl.TextY, bounds.Right + ctrl.TextX, bounds.Bottom + ctrl.TextY));

                                                ctrl.Paint.IsStroke = false;
                                                ctrl.Paint.Style = SkiaSharp.SKPaintStyle.Fill;
                                                ctrl.Paint.Color = TransparentSKColor;

                                                SKRenderAction act = SKRenderAction.PathAction(geo, ctrl.Paint, tag: ctrl.Tag);

                                                act.Payload = 0d;

                                                act.PointerEnter += (s, e) =>
                                                {
                                                    act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                                                };

                                                act.PointerLeave += (s, e) =>
                                                {
                                                    act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                                                };

                                                act.PointerPressed += (s, e) => { SetSelection(nodes[index]); HasPointerDoneSomething = true; FullSelectionCanvas.InvalidateDirty(); };

                                                if (selectionItems.TryGetValue(nodes[index].Id, out List<(double, SKRenderAction)> item))
                                                {
                                                    item.Add((0, act));
                                                }
                                                else
                                                {
                                                    selectionItems.Add(nodes[index].Id, new List<(double, SKRenderAction)>() { (0, act) });
                                                }

                                                return new SKRenderAction[] { act };

                                            }
                                            else if (ctrl.ActionType == SKRenderAction.ActionTypes.Clip || ctrl.ActionType == SKRenderAction.ActionTypes.Restore || ctrl.ActionType == SKRenderAction.ActionTypes.Save || ctrl.ActionType == SKRenderAction.ActionTypes.Transform)
                                            {
                                                return new SKRenderAction[] { ctrl };
                                            }
                                            else
                                            {
                                                return new SKRenderAction[0];
                                            }

                                        }));
                                }


                                newContext = pag.CopyToSKRenderContext(taggedActions, GlobalImages, false);
                                newSelectionContext = pag.CopyToSKRenderContext(selectionActions, GlobalImages, false);
                                success = true;
                            }
                            catch (Exception ex)
                            {
                                try
                                {
                                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                    {
                                        if (PlotCanvases[layer] == null)
                                        {
                                            SKRenderContext plotContext = new Page(1, 1).CopyToSKRenderContext(new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>(), GlobalImages, false);
                                            SKRenderAction transform = SKRenderAction.TransformAction(SkiaSharp.SKMatrix.Identity);
                                            SKRenderContext selectionContext = new Page(1, 1).CopyToSKRenderContext(new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>(), GlobalImages, false);

                                            FullPlotCanvas.AddLayer(plotContext, transform);
                                            FullSelectionCanvas.AddLayer(selectionContext, transform);

                                            PlotCanvases[layer] = plotContext;
                                            PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                                            SelectionCanvases[layer] = selectionContext;
                                            LayerTransforms[layer] = transform;
                                        }

                                        PlottingAlerts[layer].IsVisible = true;
                                        string message = GetExceptionMessage(ex);
                                        AvaloniaBugFixes.SetToolTip(PlottingAlerts[layer], message);

                                        CurrentExceptions[(string)PlottingParameters[layer][Modules.ModuleIDKey]] = (PlottingActions[layer], message);
                                        UpdateWarningVisibility();
                                    });
                                }
                                catch { }
                            }
                        });

                        try
                        {
                            if (success)
                            {

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
                                            if (LayerTransforms[i] != null)
                                            {
                                                SkiaSharp.SKMatrix newTransformMatrix = LayerTransforms[i].Transform.Value.PreConcat(SkiaSharp.SKMatrix.CreateTranslation((float)(-newOrigin.X + PlotOrigin.X), (float)(-newOrigin.Y + PlotOrigin.Y)));
                                                LayerTransforms[i].Transform = newTransformMatrix;
                                                FullPlotCanvas.LayerTransforms[i] = LayerTransforms[i];
                                                FullSelectionCanvas.LayerTransforms[i] = LayerTransforms[i];
                                            }
                                            else
                                            {
                                                SkiaSharp.SKMatrix newTransformMatrix = SkiaSharp.SKMatrix.CreateTranslation((float)(-newOrigin.X + PlotOrigin.X), (float)(-newOrigin.Y + PlotOrigin.Y));
                                                LayerTransforms[i] = SKRenderAction.TransformAction(newTransformMatrix);
                                                FullPlotCanvas.LayerTransforms[i] = LayerTransforms[i];
                                                FullSelectionCanvas.LayerTransforms[i] = LayerTransforms[i];
                                            }
                                        }
                                    }

                                    PlotOrigin = newOrigin;
                                }

                                SKRenderAction newLayerTransform = SKRenderAction.TransformAction(SkiaSharp.SKMatrix.CreateTranslation((float)(minX - PlotOrigin.X), (float)(minY - PlotOrigin.Y)));

                                Canvas parent = this.FindControl<Canvas>("PlotCanvas");
                                Canvas selectionParent = this.FindControl<Canvas>("SelectionCanvas");

                                if (PlotCanvases[layer] != null)
                                {
                                    FullPlotCanvas.UpdateLayer(layer, newContext, newLayerTransform);
                                    FullSelectionCanvas.UpdateLayer(layer, newSelectionContext, newLayerTransform);

                                    PlotCanvases[layer] = newContext;
                                    PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                                    SelectionCanvases[layer] = newSelectionContext;
                                    LayerTransforms[layer] = newLayerTransform;
                                }
                                else
                                {
                                    FullPlotCanvas.AddLayer(newContext, newLayerTransform);
                                    FullSelectionCanvas.AddLayer(newSelectionContext, newLayerTransform);

                                    PlotCanvases[layer] = newContext;
                                    PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                                    SelectionCanvases[layer] = newSelectionContext;
                                    LayerTransforms[layer] = newLayerTransform;
                                }

                                PlottingAlerts[layer].IsVisible = false;

                                CurrentExceptions.Remove((string)PlottingParameters[layer][Modules.ModuleIDKey]);
                                UpdateWarningVisibility();

                                FullPlotCanvas.Width = PlotBottomRight.X - PlotOrigin.X;
                                FullPlotCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y;

                                FullPlotCanvas.PageWidth = PlotBottomRight.X - PlotOrigin.X;
                                FullPlotCanvas.PageHeight = PlotBottomRight.Y - PlotOrigin.Y;

                                FullSelectionCanvas.Width = PlotBottomRight.X - PlotOrigin.X;
                                FullSelectionCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y;

                                FullSelectionCanvas.PageWidth = PlotBottomRight.X - PlotOrigin.X;
                                FullSelectionCanvas.PageHeight = PlotBottomRight.Y - PlotOrigin.Y;

                                Canvas containerCanvas = this.FindControl<Canvas>("ContainerCanvas");
                                containerCanvas.Background = GraphBackgroundBrush;
                                containerCanvas.Width = PlotBottomRight.X - PlotOrigin.X + 20;
                                containerCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y + 20;

                                UpdateSelectionWidth(FullSelectionCanvas);
                            }
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                if (PlotCanvases[layer] == null)
                                {
                                    SKRenderContext plotContext = new Page(1, 1).CopyToSKRenderContext(new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>(), GlobalImages, false);
                                    SKRenderAction transform = SKRenderAction.TransformAction(SkiaSharp.SKMatrix.Identity);
                                    SKRenderContext selectionContext = new Page(1, 1).CopyToSKRenderContext(new Dictionary<string, Func<SKRenderAction, IEnumerable<SKRenderAction>>>(), GlobalImages, false);

                                    FullPlotCanvas.AddLayer(plotContext, transform);
                                    FullSelectionCanvas.AddLayer(selectionContext, transform);

                                    PlotCanvases[layer] = plotContext;
                                    PlotBounds[layer] = (new Point(minX, minY), new Point(maxX, maxY));
                                    SelectionCanvases[layer] = selectionContext;
                                    LayerTransforms[layer] = transform;
                                }

                                PlottingAlerts[layer].IsVisible = true;
                                string message = GetExceptionMessage(ex);
                                AvaloniaBugFixes.SetToolTip(PlottingAlerts[layer], message);

                                CurrentExceptions[(string)PlottingParameters[layer][Modules.ModuleIDKey]] = (PlottingActions[layer], message);
                                UpdateWarningVisibility();
                            }
                            catch { }
                        }

                        if (!updatingAllLayers)
                        {
                            this.FindControl<CoolProgressBar>("PlotProgressBar").Progress = 1;
                            this.FindControl<CoolProgressBar>("PlotProgressBar").LabelText = " ";

                            this.FindControl<CoolProgressBar>("PlotProgressBar").Opacity = 0;

                            this.RefreshAllButton.IsEnabled = true;
                        }
                    }
                    ActionsWhoseColourHasBeenChanged.RemoveWhere(act => !SelectionCanvas.Children.Contains(act.Parent));

                    if (updatePlotBounds)
                    {
                        UpdatePlotBounds();
                    }
                }
                finally
                {
                    RenderingSemaphore.Release();
                }
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
                    if (i < LayerTransforms.Count && i < FullPlotCanvas.LayerTransforms.Count && i < FullSelectionCanvas.LayerTransforms.Count)
                    {
                        if (LayerTransforms[i] != null)
                        {
                            SkiaSharp.SKMatrix newTransformMatrix = LayerTransforms[i].Transform.Value.PreConcat(SkiaSharp.SKMatrix.CreateTranslation((float)(-newOrigin.X + PlotOrigin.X), (float)(-newOrigin.Y + PlotOrigin.Y)));
                            LayerTransforms[i].Transform = newTransformMatrix;
                            FullPlotCanvas.LayerTransforms[i] = LayerTransforms[i];
                            FullSelectionCanvas.LayerTransforms[i] = LayerTransforms[i];
                        }
                        else
                        {
                            SkiaSharp.SKMatrix newTransformMatrix = SkiaSharp.SKMatrix.CreateTranslation((float)(-newOrigin.X + PlotOrigin.X), (float)(-newOrigin.Y + PlotOrigin.Y));
                            LayerTransforms[i] = SKRenderAction.TransformAction(newTransformMatrix);
                            FullPlotCanvas.LayerTransforms[i] = LayerTransforms[i];
                            FullSelectionCanvas.LayerTransforms[i] = LayerTransforms[i];
                        }
                    }
                }
            }

            PlotOrigin = newOrigin;
            PlotBottomRight = new Point(maxX, maxY);

            Canvas containerCanvas = this.FindControl<Canvas>("ContainerCanvas");
            containerCanvas.Width = PlotBottomRight.X - PlotOrigin.X + 20;
            containerCanvas.Height = PlotBottomRight.Y - PlotOrigin.Y + 20;

            if (WasAutoFitted)
            {
                AutoFit();
            }

        }

        private async Task UpdateAllPlotLayers()
        {
            if (!StopAllUpdates)
            {
                this.RefreshAllButton.IsEnabled = false;

                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").IsVisible = true;

                PlotBounds = new List<(Point, Point)>(new (Point, Point)[PlottingActions.Count]);
                PlotCanvases = new List<SKRenderContext>(new SKRenderContext[PlottingActions.Count]);
                LayerTransforms = new List<SKRenderAction>(new SKRenderAction[PlottingActions.Count]);

                while (FullPlotCanvas.LayerTransforms.Count > 0)
                {
                    FullPlotCanvas.RemoveLayer(0);
                }

                while (FullSelectionCanvas.LayerTransforms.Count > 0)
                {
                    FullSelectionCanvas.RemoveLayer(0);
                }

                SelectionCanvases = new List<SKRenderContext>(new SKRenderContext[PlottingActions.Count]);

                PlotOrigin = new Point(double.NaN, double.NaN);
                PlotBottomRight = new Point(double.NaN, double.NaN);

                if (PlottingActions.Count <= Math.Floor(this.FindControl<CoolProgressBar>("PlotProgressBar").Bounds.Width / 13))
                {
                    List<double> steps = new List<double>();

                    for (int i = 0; i < PlottingActions.Count - 1; i++)
                    {
                        steps.Add((double)(i + 1) / PlottingActions.Count);
                    }

                    this.FindControl<CoolProgressBar>("PlotProgressBar").IntermediateSteps = System.Collections.Immutable.ImmutableList.Create(steps.ToArray());
                }
                else
                {
                    this.FindControl<CoolProgressBar>("PlotProgressBar").IntermediateSteps = System.Collections.Immutable.ImmutableList<double>.Empty;
                }


                this.FindControl<CoolProgressBar>("PlotProgressBar").Progress = 0;
                this.FindControl<CoolProgressBar>("PlotProgressBar").Opacity = 1;

                for (int i = 0; i < PlottingActions.Count; i++)
                {
                    await ActuallyUpdatePlotLayer(i, false, true);
                    this.FindControl<CoolProgressBar>("PlotProgressBar").Progress = (double)(i + 1) / PlottingActions.Count;
                }

                UpdatePlotBounds();

                this.FindControl<CoolProgressBar>("PlotProgressBar").Opacity = 0;

                RenderingPassCompleted?.Invoke(this, new EventArgs());

                this.RefreshAllButton.IsEnabled = true;
            }
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
        public static List<(double, SKRenderAction)> FindPaths(SKMultiLayerRenderCanvas can, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                List<(double, SKRenderAction)> tbr = new List<(double, SKRenderAction)>();

                if (can == null)
                {
                    return tbr;
                }

                for (int i = 0; i < can.RenderActions.Count; i++)
                {
                    for (int j = 0; j < can.RenderActions[i].Count; j++)
                    {
                        if (can.RenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Path && can.RenderActions[i][j].Payload is double d)
                        {
                            tbr.Add((d, can.RenderActions[i][j]));
                        }
                    }
                }

                return tbr;
            }
            else
            {
                List<(double, SKRenderAction)> tbr = new List<(double, SKRenderAction)>();

                if (can == null)
                {
                    return tbr;
                }

                for (int i = 0; i < can.RenderActions.Count; i++)
                {
                    for (int j = 0; j < can.RenderActions[i].Count; j++)
                    {
                        if (can.RenderActions[i][j].ActionType == SKRenderAction.ActionTypes.Path && can.RenderActions[i][j].Tag == id)
                        {
                            if (can.RenderActions[i][j].Payload is double d)
                            {
                                tbr.Add((d, can.RenderActions[i][j]));
                            }
                        }
                    }
                }

                return tbr;
            }
        }
    }
}
