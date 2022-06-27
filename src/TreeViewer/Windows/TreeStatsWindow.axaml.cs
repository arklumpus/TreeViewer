using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Markdig;
using MathNet.Numerics.Statistics;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VectSharp;
using VectSharp.Canvas;
using VectSharp.Markdown;
using VectSharp.SVG;
using VectSharp.PDF;
using Avalonia.Media.Transformation;
using Avalonia.Animation;
using VectSharp.Raster.ImageSharp;
using SixLabors.ImageSharp;
using Avalonia.Controls.PanAndZoom;
using Avalonia.Input;
using Avalonia.Interactivity;
using TreeViewer.Stats;
using System.Threading.Tasks;
using static TreeViewer.Stats.TreeReport;

namespace TreeViewer
{
    public partial class TreeStatsWindow : ChildWindow
    {
        public TreeStatsWindow()
        {
            InitializeComponent();
        }

        public TreeStatsWindow(MainWindow parentWindow)
        {
            InitializeComponent();

            if (parentWindow.Trees.Count < 2)
            {
                bar.GridItems[1].IsVisible = false;
            }

            MarkdownRenderer renderer = new MarkdownRenderer();
            renderer.TableVAlign = MarkdownRenderer.VerticalAlignment.Top;
            renderer.BackgroundColour = Colour.FromRgba(0, 0, 0, 0);
            renderer.ImageUnitMultiplier /= 1.4;
            renderer.ImageMultiplier *= 1.4;
            renderer.HeaderFontSizeMultipliers[0] *= 1.3;
            renderer.BaseFontSize = 14;
            renderer.Margins = new Margins(24, 10, 24, 10);

            renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);

            renderer.ThematicBreakLineColour = Colour.FromRgb(180, 180, 180);
            renderer.ThematicBreakThickness = 1;

            this.MDRenderer = renderer;

            ReportRenderer = new MarkdownRenderer();
            ReportRenderer.TableVAlign = MarkdownRenderer.VerticalAlignment.Top;
            ReportRenderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
            ReportRenderer.ThematicBreakLineColour = Colour.FromRgb(180, 180, 180);
            ReportRenderer.ThematicBreakThickness = 1;

            this.parentWindow = parentWindow;


            this.Opened += async (s, e) =>
            {
                ProgressWindow win = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 2, LabelText = "Sampling trees under the YHK model" };

                _ = win.ShowDialog2(this);

                await Task.Run(() => CreateFinalTreeReport(parentWindow, win));

                lock (DocumentLock)
                {
                    lock (FinalTreeReportLock)
                    {
                        this.Document = FinalTreeReport;

                        RenderDocument();
                    }
                }

                this.PropertyChanged += (s, e) =>
                {
                    if (e.Property == Window.BoundsProperty)
                    {
                        this.FindControl<Grid>("PlotGrid").MaxWidth = this.FindControl<Grid>("MainGrid").Bounds.Width;
                        this.FindControl<Grid>("PlotGrid").MaxHeight = this.FindControl<Grid>("MainGrid").Bounds.Height;
                        this.FindControl<Grid>("PlotGrid").MinWidth = this.FindControl<Grid>("MainGrid").Bounds.Width;
                        this.FindControl<Grid>("PlotGrid").MinHeight = this.FindControl<Grid>("MainGrid").Bounds.Height;

                        Vector offset = this.FindControl<ScrollViewer>("ScrollViewer").Offset;
                        Avalonia.Size extent = this.FindControl<ScrollViewer>("ScrollViewer").Extent;

                        RenderDocument();

                        Avalonia.Size newExtent = this.FindControl<ScrollViewer>("ScrollViewer").Extent;
                        this.FindControl<ScrollViewer>("ScrollViewer").Offset = new Vector(offset.X / extent.Width * newExtent.Width, offset.Y / extent.Height * newExtent.Height);
                    }
                };

                win.Close();
            };
        }

        private void CreateFinalTreeReport(MainWindow parentWindow, ProgressWindow progressWindow)
        {
            lock (FinalTreeReportLock)
            {
                if (FinalTreeReport == null)
                {
                    (this.FinalTreeReport, this.FinalTreeReportSource) = SingleTreeReport.CreateReport(parentWindow.TransformedTree, (text, progress) =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                progressWindow.LabelText = text;
                            }

                            progressWindow.Progress = progress;
                        });
                    }, Plots, Data);
                }
            }
        }

        private void CreateLoadedTreesReport(MainWindow parentWindow, ProgressWindow progressWindow)
        {
            lock (LoadedTreesReportLock)
            {
                if (LoadedTreesReport == null)
                {
                    if (parentWindow.Trees.Count > 2)
                    {
                        (this.LoadedTreesReport, this.LoadedTreesReportSource) = MultipleTreesReport.CreateReport(parentWindow.Trees, (text, progress) =>
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (!string.IsNullOrEmpty(text))
                                {
                                    progressWindow.LabelText = text;
                                }

                                progressWindow.Progress = progress;
                            });
                        }, Plots, Data);
                    }
                    else if (parentWindow.Trees.Count == 2)
                    {
                        (this.LoadedTreesReport, this.LoadedTreesReportSource) = TwoTreesReport.CreateReport(parentWindow.Trees[0], parentWindow.Trees[1], (text, progress) =>
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (!string.IsNullOrEmpty(text))
                                {
                                    progressWindow.LabelText = text;
                                }

                                progressWindow.Progress = progress;
                            });
                        }, Plots, Data);
                    }
                }
            }
        }


        private void CreateCustomTreesReport(MainWindow parentWindow, ProgressWindow progressWindow, List<TreeNode> trees)
        {
            lock (CustomTreesReportLock)
            {
                if (trees.Count == 1)
                {
                    (this.CustomTreesReport, this.CustomTreesReportSource) = SingleTreeReport.CreateReport(trees[0], (text, progress) =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                progressWindow.LabelText = text;
                            }

                            progressWindow.Progress = progress;
                        });
                    }, Plots, Data);
                }
                else if (trees.Count > 2)
                {
                    (this.CustomTreesReport, this.CustomTreesReportSource) = MultipleTreesReport.CreateReport(trees, (text, progress) =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                progressWindow.LabelText = text;
                            }

                            progressWindow.Progress = progress;
                        });
                    }, Plots, Data);
                }
                else if (trees.Count == 2)
                {
                    (this.CustomTreesReport, this.CustomTreesReportSource) = TwoTreesReport.CreateReport(trees[0], trees[1], (text, progress) =>
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            if (!string.IsNullOrEmpty(text))
                            {
                                progressWindow.LabelText = text;
                            }

                            progressWindow.Progress = progress;
                        });
                    }, Plots, Data);
                }
            }
        }

        private MainWindow parentWindow;

        private Markdig.Syntax.MarkdownDocument Document = null;
        private object DocumentLock = new object();

        private Markdig.Syntax.MarkdownDocument FinalTreeReport = null;
        private string FinalTreeReportSource = null;
        private object FinalTreeReportLock = new object();

        private Markdig.Syntax.MarkdownDocument LoadedTreesReport = null;
        private string LoadedTreesReportSource = null;
        private object LoadedTreesReportLock = new object();

        private Markdig.Syntax.MarkdownDocument CustomTreesReport = null;
        private string CustomTreesReportSource = null;
        private object CustomTreesReportLock = new object();

        private double MaxRenderWidth = 1000;
        private double MinRenderWidth = 200;
        private double MinVariation = 10;
        private bool forcedRerender = false;
        private double lastRenderedWidth = double.NaN;
        private MarkdownRenderer MDRenderer = null;
        private MarkdownRenderer ReportRenderer = null;
        private Dictionary<string, GetPlot> Plots = new Dictionary<string, GetPlot>();
        private Dictionary<string, Func<(string header, IEnumerable<double[]>)>> Data = new Dictionary<string, Func<(string header, IEnumerable<double[]>)>>();
        private Dictionary<string, Rect> PlotBounds = new Dictionary<string, Rect>();

        private void RenderDocument()
        {
            if (Document != null)
            {
                double width = Math.Min(MaxRenderWidth, Math.Max(MinRenderWidth, this.Bounds.Width - MinVariation - 13));

                if (forcedRerender || double.IsNaN(lastRenderedWidth) || width != lastRenderedWidth && width < lastRenderedWidth - MinVariation || width > lastRenderedWidth + MinVariation)
                {
                    Page pag;
                    Dictionary<string, string> linkDestinations;

                    try
                    {
                        pag = MDRenderer.RenderSinglePage(this.Document, width, out linkDestinations);
                    }
                    catch
                    {
                        pag = new Page(width, 0);
                        linkDestinations = new Dictionary<string, string>();
                    }

                    Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>();
                    Dictionary<string, Avalonia.Point> linkDestinationPoints = new Dictionary<string, Avalonia.Point>();

                    foreach (KeyValuePair<string, GetPlot> plot in Plots)
                    {
                        Dictionary<string, List<RenderAction>> buttonActions = new Dictionary<string, List<RenderAction>>();

                        for (int i = 0; i < 4; i++)
                        {
                            buttonActions["buttonBg[" + i.ToString() + "]/" + plot.Key] = new List<RenderAction>();
                            buttonActions["buttonSymbol[" + i.ToString() + "]/" + plot.Key] = new List<RenderAction>();

                            int buttonIndex = i;

                            void highlightButton()
                            {
                                foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                {
                                    for (int j = 0; j < kvp.Value.Count; j++)
                                    {
                                        string ind = kvp.Key.Substring(kvp.Key.IndexOf("[") + 1);
                                        ind = ind.Substring(0, ind.IndexOf("]"));

                                        if (int.Parse(ind) != buttonIndex)
                                        {
                                            if (kvp.Key.StartsWith("buttonBg"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(230, 230, 230));
                                            }
                                            else if (kvp.Key.StartsWith("buttonSymbol"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(115, 115, 115));
                                            }

                                            kvp.Value[j].Stroke = null;
                                        }
                                        else
                                        {
                                            if (kvp.Key.StartsWith("buttonBg"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(200, 200, 200));
                                                kvp.Value[j].Stroke = new Avalonia.Media.Pen(new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(115, 115, 115)));
                                            }
                                            else if (kvp.Key.StartsWith("buttonSymbol"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178));
                                            }
                                        }
                                    }
                                }
                            };

                            void pressButton()
                            {
                                foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                {
                                    for (int j = 0; j < kvp.Value.Count; j++)
                                    {
                                        string ind = kvp.Key.Substring(kvp.Key.IndexOf("[") + 1);
                                        ind = ind.Substring(0, ind.IndexOf("]"));

                                        if (int.Parse(ind) == buttonIndex)
                                        {
                                            if (kvp.Key.StartsWith("buttonBg"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(115, 115, 115));
                                                kvp.Value[j].Stroke = new Avalonia.Media.Pen(new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(115, 115, 115)));
                                            }
                                            else if (kvp.Key.StartsWith("buttonSymbol"))
                                            {
                                                kvp.Value[j].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(28, 91, 153));
                                            }
                                        }
                                    }
                                }
                            }

                            taggedActions.Add("buttonBg[" + i.ToString() + "]/" + plot.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                            {
                                buttonActions[act.Tag].Add(act);

                                act.PointerEnter += (s, e) =>
                                {
                                    highlightButton();
                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerLeave += (s, e) =>
                                {
                                    foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                    {
                                        for (int i = 0; i < kvp.Value.Count; i++)
                                        {
                                            kvp.Value[i].Fill = Avalonia.Media.Brushes.Transparent;
                                            kvp.Value[i].Stroke = null;
                                        }
                                    }

                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerPressed += (s, e) =>
                                {
                                    pressButton();
                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerReleased += (s, e) =>
                                {
                                    highlightButton();
                                    act.Parent.InvalidateVisual();

                                    switch (buttonIndex)
                                    {
                                        case 0:
                                            ExportPDF(plot.Key);
                                            break;
                                        case 1:
                                            ExportSVG(plot.Key);
                                            break;
                                        case 2:
                                            ExportCSV(plot.Key);
                                            break;
                                        case 3:
                                            OpenPlot(plot.Key);
                                            break;
                                    }
                                };

                                return new RenderAction[] { act };
                            }));

                            taggedActions.Add("buttonSymbol[" + i.ToString() + "]/" + plot.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                            {
                                buttonActions[act.Tag].Add(act);

                                act.PointerEnter += (s, e) =>
                                {
                                    highlightButton();
                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerLeave += (s, e) =>
                                {
                                    foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                    {
                                        for (int i = 0; i < kvp.Value.Count; i++)
                                        {
                                            kvp.Value[i].Fill = Avalonia.Media.Brushes.Transparent;
                                            kvp.Value[i].Stroke = null;
                                        }
                                    }

                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerPressed += (s, e) =>
                                {
                                    pressButton();
                                    act.Parent.InvalidateVisual();
                                };

                                act.PointerReleased += (s, e) =>
                                {
                                    highlightButton();
                                    act.Parent.InvalidateVisual();

                                    switch (buttonIndex)
                                    {
                                        case 0:
                                            ExportPDF(plot.Key);
                                            break;
                                        case 1:
                                            ExportSVG(plot.Key);
                                            break;
                                        case 2:
                                            ExportCSV(plot.Key);
                                            break;
                                        case 3:
                                            OpenPlot(plot.Key);
                                            break;
                                    }
                                };

                                return new RenderAction[] { act };
                            }));
                        }

                        taggedActions.Add("plotBounds/" + plot.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                        {
                            Avalonia.Point topLeft = act.Geometry.Bounds.TopLeft.Transform(act.Transform);
                            Avalonia.Point bottomRight = act.Geometry.Bounds.BottomRight.Transform(act.Transform);

                            PlotBounds[plot.Key] = new Rect(topLeft, bottomRight);

                            act.PointerEnter += (s, e) =>
                            {
                                foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                {
                                    for (int i = 0; i < kvp.Value.Count; i++)
                                    {
                                        if (kvp.Key.StartsWith("buttonBg"))
                                        {
                                            kvp.Value[i].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(230, 230, 230));
                                        }
                                        else if (kvp.Key.StartsWith("buttonSymbol"))
                                        {
                                            kvp.Value[i].Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(115, 115, 115));
                                        }

                                        kvp.Value[i].Stroke = null;
                                    }
                                }

                                act.Parent.InvalidateVisual();
                            };

                            act.PointerLeave += (s, e) =>
                            {
                                foreach (KeyValuePair<string, List<RenderAction>> kvp in buttonActions)
                                {
                                    for (int i = 0; i < kvp.Value.Count; i++)
                                    {
                                        kvp.Value[i].Fill = Avalonia.Media.Brushes.Transparent;
                                        kvp.Value[i].Stroke = null;
                                    }
                                }

                                act.Parent.InvalidateVisual();
                            };

                            return new RenderAction[] { act };
                        }));
                    }

                    foreach (KeyValuePair<string, string> linkDestination in linkDestinations)
                    {
                        string url = linkDestination.Value;

                        taggedActions.Add(linkDestination.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                        {
                            act.PointerEnter += (s, e) =>
                            {
                                act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                            };

                            act.PointerLeave += (s, e) =>
                            {
                                act.Parent.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
                            };

                            act.PointerPressed += (s, e) =>
                            {
                                if (url.StartsWith("#"))
                                {
                                    if (linkDestinationPoints.TryGetValue(url.Substring(1), out Avalonia.Point target))
                                    {
                                        ScrollViewer scrollViewer = this.FindControl<ScrollViewer>("ScrollViewer");

                                        scrollViewer.Offset = new Vector(Math.Max(Math.Min(scrollViewer.Offset.X, target.X), target.X - scrollViewer.Viewport.Width), target.Y);
                                    }
                                }
                                else
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = url, UseShellExecute = true });
                                }
                            };

                            if (act.ActionType == RenderAction.ActionTypes.Path)
                            {
                                linkDestinationPoints[linkDestination.Key] = act.Geometry.Bounds.TopLeft.Transform(act.Transform);
                            }
                            else if (act.ActionType == RenderAction.ActionTypes.Text)
                            {
                                linkDestinationPoints[linkDestination.Key] = act.Text.Bounds.TopLeft.Transform(act.Transform);
                            }
                            else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                            {
                                linkDestinationPoints[linkDestination.Key] = act.ImageDestination.Value.TopLeft.Transform(act.Transform);
                            }

                            return new RenderAction[] { act };
                        }));

                        if (url.StartsWith("#"))
                        {
                            if (!taggedActions.ContainsKey(url.Substring(1)))
                            {
                                taggedActions.Add(url.Substring(1), (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                                {
                                    if (act.ActionType == RenderAction.ActionTypes.Path)
                                    {
                                        linkDestinationPoints[url.Substring(1)] = act.Geometry.Bounds.TopLeft.Transform(act.Transform);
                                    }
                                    else if (act.ActionType == RenderAction.ActionTypes.Text)
                                    {
                                        linkDestinationPoints[url.Substring(1)] = act.Text.Bounds.TopLeft.Transform(act.Transform);
                                    }
                                    else if (act.ActionType == RenderAction.ActionTypes.RasterImage)
                                    {
                                        linkDestinationPoints[url.Substring(1)] = act.ImageDestination.Value.TopLeft.Transform(act.Transform);
                                    }

                                    return new RenderAction[] { act };
                                }));
                            }
                        }
                    }

                    Avalonia.Controls.Canvas can = pag.PaintToCanvas(false, taggedActions, false, AvaloniaContextInterpreter.TextOptions.ConvertIfNecessary);

                    this.FindControl<ScrollViewer>("ScrollViewer").Content = can;
                    this.FindControl<ScrollViewer>("ScrollViewer").Padding = new Thickness(0, 0, 0, 0);
                    lastRenderedWidth = width;
                    forcedRerender = false;
                }
                else
                {
                    this.FindControl<ScrollViewer>("ScrollViewer").Padding = new Thickness(0, 0, width - lastRenderedWidth, 0);
                }
            }
        }

        private async void ExportCSV(string plotID)
        {
            (string header, IEnumerable<double[]> data) = Data[plotID]();

            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "CSV file", Extensions = new List<string>() { "csv" } } }, Title = "Export CSV file..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                using (FileStream fs = new FileStream(result, FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        sw.WriteLine(header);

                        foreach (double[] d in data)
                        {
                            sw.WriteLine(d.Aggregate("", (a, b) => a + "\t" + b.ToString(System.Globalization.CultureInfo.InvariantCulture), x => x.Substring(1)));
                        }
                    }
                }
            }
        }

        private async void ExportSVG(string plotID, SVGContextInterpreter.TextOptions textOption = SVGContextInterpreter.TextOptions.ConvertIntoPaths)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "SVG file", Extensions = new List<string>() { "svg" } } }, Title = "Export plot as SVG..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                Page pag = Plots[plotID](false, out _);
                pag.SaveAsSVG(result, textOption);
            }
        }

        private async void ExportPDF(string plotID)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "PDF file", Extensions = new List<string>() { "pdf" } } }, Title = "Export plot as PDF..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                Page pag = Plots[plotID](false, out _);
                Document doc = new Document();
                doc.Pages.Add(pag);
                doc.SaveAsPDF(result);
            }
        }

        private async void ExportImage(string plotID, OutputFormats outputFormat)
        {
            List<string> extensions = new List<string>() { "png" };
            string formatName = "PNG image";

            switch (outputFormat)
            {
                case OutputFormats.TIFF:
                    extensions = new List<string>() { "tiff", "tif" };
                    formatName = "TIFF image";
                    break;

                case OutputFormats.JPEG:
                    extensions = new List<string>() { "jpg", "jpeg" };
                    formatName = "JPEG image";
                    break;
            }

            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = formatName, Extensions = extensions } }, Title = "Export plot as image..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                Page pag = Plots[plotID](false, out _);

                if (outputFormat != OutputFormats.PNG)
                {
                    pag.Background = Colours.White;
                }

                double scale = 17 / 2.54 * 600 / pag.Width;
                SixLabors.ImageSharp.Image image = pag.SaveAsImage(scale);

                image.Metadata.HorizontalResolution = 600;
                image.Metadata.VerticalResolution = 600;



                if (outputFormat == OutputFormats.TIFF)
                {
                    image.SaveAsTiff(result, new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder() { Compression = SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Deflate });
                }
                else
                {
                    image.Save(result);
                }
            }
        }

        private async void ExportMarkdown(string markdownSource)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Markdown file", Extensions = new List<string>() { "md" } } }, Title = "Export Markdown file..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                File.WriteAllText(result, markdownSource);
            }
        }

        private async void ExportPDF(Markdig.Syntax.MarkdownDocument document, bool usLetter)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "PDF file", Extensions = new List<string>() { "pdf" } } }, Title = "Export report as PDF..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                if (usLetter)
                {
                    ReportRenderer.PageSize = new VectSharp.Size(612, 792);
                }
                else
                {
                    ReportRenderer.PageSize = new VectSharp.Size(595, 842);
                }

                Document doc = ReportRenderer.Render(document, out Dictionary<string, string> linkDestinations);

                doc.SaveAsPDF(result, linkDestinations: linkDestinations);
            }
        }

        private string CurrentPlotID = null;

        private void OpenPlot(string plotID)
        {
            this.FindControl<Grid>("PlotGrid").Transitions = null;
            this.FindControl<ScrollViewer>("ScrollViewer").IsHitTestVisible = false;

            Avalonia.Point topLeft = PlotBounds[plotID].TopLeft;
            topLeft = this.FindControl<Grid>("MainGrid").PointToClient(((Control)this.FindControl<ScrollViewer>("ScrollViewer").Content).PointToScreen(topLeft));

            Avalonia.Point bottomRight = PlotBounds[plotID].BottomRight;
            bottomRight = this.FindControl<Grid>("MainGrid").PointToClient(((Control)this.FindControl<ScrollViewer>("ScrollViewer").Content).PointToScreen(bottomRight));

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(topLeft.X, topLeft.Y);

            this.FindControl<Grid>("PlotGrid").RenderTransform = builder.Build();

            double width = this.FindControl<Grid>("PlotGrid").Width;

            this.FindControl<Grid>("PlotGrid").MaxWidth = bottomRight.X - topLeft.X;
            this.FindControl<Grid>("PlotGrid").MaxHeight = bottomRight.Y - topLeft.Y;

            this.FindControl<Grid>("PlotGrid").MinWidth = bottomRight.X - topLeft.X;
            this.FindControl<Grid>("PlotGrid").MinHeight = bottomRight.Y - topLeft.Y;

            this.FindControl<Grid>("PlotGrid").Transitions = new Transitions()
            {
                new TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) },
                //new DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(1000) },
                new DoubleTransition() { Property = Grid.MaxWidthProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new DoubleTransition() { Property = Grid.MaxHeightProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new DoubleTransition() { Property = Grid.MinWidthProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new DoubleTransition() { Property = Grid.MinHeightProperty, Duration = TimeSpan.FromMilliseconds(100) },
            };

            Page pag = Plots[plotID](true, out Dictionary<string, (Colour, Colour, string)> descriptions);


            Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>();

            foreach (KeyValuePair<string, (Colour, Colour, string)> kvp in descriptions)
            {
                taggedActions.Add(kvp.Key, (Func<RenderAction, IEnumerable<RenderAction>>)(act =>
                {
                    act.PointerEnter += (s, e) =>
                    {
                        act.Fill = new Avalonia.Media.SolidColorBrush(kvp.Value.Item2.ToAvalonia());
                        act.Parent.InvalidateVisual();

                        this.FindControl<Border>("HoverBorder").IsVisible = true;
                        this.FindControl<Border>("HoverBorder").Transitions = new Transitions()
                        {
                            new DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                        };

                        this.FindControl<TextBlock>("HoverDescription").Text = descriptions[kvp.Key].Item3;
                        this.FindControl<Border>("HoverBorder").Opacity = 1;

                        Avalonia.Input.PointerPoint pt = e.GetCurrentPoint(this.FindControl<Grid>("PlotGrid"));

                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            this.FindControl<Border>("HoverBorder").RenderTransform = new Avalonia.Media.TranslateTransform(Math.Min(pt.Position.X, this.FindControl<Grid>("PlotGrid").Bounds.Width - this.FindControl<Border>("HoverBorder").Bounds.Width), pt.Position.Y + 15);
                        });
                    };

                    act.PointerLeave += (s, e) =>
                    {
                        act.Fill = new Avalonia.Media.SolidColorBrush(kvp.Value.Item1.ToAvalonia());
                        act.Parent.InvalidateVisual();
                        this.FindControl<Border>("HoverBorder").IsVisible = false;
                        this.FindControl<Border>("HoverBorder").Transitions = null;
                        this.FindControl<Border>("HoverBorder").Opacity = 0;
                    };

                    return new RenderAction[] { act };
                }));
            }


            Canvas can = pag.PaintToCanvas(false, taggedActions, false);

            this.FindControl<ZoomBorder>("PlotViewBox").Child = can;

            this.FindControl<Grid>("PlotGrid").IsVisible = true;
            this.FindControl<Grid>("ZoomPanel").IsVisible = true;

            this.FindControl<Grid>("PlotGrid").RenderTransform = TransformOperations.Identity;

            this.FindControl<Grid>("PlotGrid").MaxWidth = this.FindControl<Grid>("MainGrid").Bounds.Width;
            this.FindControl<Grid>("PlotGrid").MaxHeight = this.FindControl<Grid>("MainGrid").Bounds.Height;
            this.FindControl<Grid>("PlotGrid").MinWidth = this.FindControl<Grid>("MainGrid").Bounds.Width;
            this.FindControl<Grid>("PlotGrid").MinHeight = this.FindControl<Grid>("MainGrid").Bounds.Height;

            _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(100);

                this.FindControl<Grid>("PlotGrid").Transitions = null;

                can.Transitions = new Transitions()
                {
                    new TransformOperationsTransition() { Property = Canvas.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
                };
            });

            CurrentPlotID = plotID;

            bar.GridItems[3].IsVisible = true;
            bar.SelectedIndex = 3;
        }


        private int PlotType = 0;

        private RibbonBar bar;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Grid>("PlotGrid").PointerMoved += (s, e) =>
            {
                if (this.FindControl<Border>("HoverBorder").IsVisible)
                {
                    Avalonia.Input.PointerPoint pt = e.GetCurrentPoint(this.FindControl<Grid>("PlotGrid"));

                    this.FindControl<Border>("HoverBorder").RenderTransform = new Avalonia.Media.TranslateTransform(Math.Min(pt.Position.X, this.FindControl<Grid>("PlotGrid").Bounds.Width - this.FindControl<Border>("HoverBorder").Bounds.Width), pt.Position.Y + 15);
                }
            };


            bar = new RibbonBar(new (string, bool)[] { ("Final tree", false), ("Loaded trees", false), ("Compare", false), ("Plot", true) });

            bar.FontSize = 14;

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                this.Classes.Add("WindowsStyle");
            }

            if (GlobalSettings.Settings.RibbonStyle == GlobalSettings.RibbonStyles.Colourful)
            {
                bar.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178));
                bar.Margin = new Thickness(-1, 0, -1, 0);
                bar.Classes.Add("Colorful");
            }
            else
            {
                bar.Classes.Add("Grey");
            }

            bar.GridItems[3].IsVisible = false;

            this.FindControl<Grid>("RibbonBarContainer").Children.Add(bar);

            RibbonTabContent finalTreeTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Export",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Export Markdown", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportMDReport")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        lock (FinalTreeReportLock)
                        {
                            ExportMarkdown(FinalTreeReportSource);
                        }
                    }), "Exports the Markdown source for the report."),

                    ("Export as PDF", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportPDFReport")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "A4 paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                        ( "US letter paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                    }, true, 0, (Action<int>)(ind =>
                    {
                        switch (ind)
                        {
                            case -1:
                            case 0:
                                lock (FinalTreeReportLock)
                                {
                                    ExportPDF(FinalTreeReport, false);
                                }
                                break;

                            case 1:
                                lock (FinalTreeReportLock)
                                {
                                    ExportPDF(FinalTreeReport, true);
                                }
                                break;
                        }
                    }), "Exports the the report as a PDF file.")
                }),

            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(finalTreeTab);

            RibbonTabContent loadedTreesTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Export",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Export Markdown", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportMDReport")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        lock (LoadedTreesReportLock)
                        {
                            ExportMarkdown(LoadedTreesReportSource);
                        }
                    }), "Exports the Markdown source for the report."),

                    ("Export as PDF", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportPDFReport")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "A4 paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                        ( "US letter paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                    }, true, 0, (Action<int>)(ind =>
                    {
                        switch (ind)
                        {
                            case -1:
                            case 0:
                                lock (LoadedTreesReportLock)
                                {
                                    ExportPDF(LoadedTreesReport, false);
                                }
                                break;

                            case 1:
                                lock (FinalTreeReportLock)
                                {
                                    ExportPDF(LoadedTreesReport, true);
                                }
                                break;
                        }
                    }), "Exports the the report as a PDF file.")
                }),

            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(loadedTreesTab);

            RibbonTabContent compareTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("New",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Create new report", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NewReport")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async ind =>
                    {
                        CreateReportWindow win = new CreateReportWindow(this.parentWindow);

                        await win.ShowDialog2(this);

                        if (win.Result && win.Trees != null && win.Trees.Count > 0)
                        {
                            int totalCount = 0;

                            for (int i = 0; i < win.Trees.Count; i++)
                            {
                                if (win.Trees[i] != null)
                                {
                                    totalCount += win.Trees[i].Count;
                                }
                            }

                            if (totalCount > 0)
                            {
                                List<TreeNode> trees = new List<TreeNode>(totalCount);

                                for (int i = 0; i < win.Trees.Count; i++)
                                {
                                    if (win.Trees[i] != null)
                                    {
                                        trees.AddRange(win.Trees[i]);
                                    }
                                }

                                ProgressWindow pwin;

                                if (trees.Count > 2)
                                {
                                    pwin = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 4, LabelText = "Computing tree splits" };
                                }
                                else
                                {
                                    pwin = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 2, LabelText = "Sampling trees under the YHK model" };
                                }

                                _ = pwin.ShowDialog2(this);

                                await Task.Run(() => CreateCustomTreesReport(parentWindow, pwin, trees));

                                pwin.Close();

                                lock (CustomTreesReportLock)
                                {
                                    lock (DocumentLock)
                                    {
                                        this.Document = CustomTreesReport;

                                        forcedRerender = true;
                                        RenderDocument();
                                    }
                                }
                            }
                        }


                    }), "Creates a new report.")
                }),

                ("Export",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Export Markdown", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportMDReport")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {

                    }), "Exports the Markdown source for the report."),

                    ("Export as PDF", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportPDFReport")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "A4 paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                        ( "US letter paper size", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportPDFReport")), null ),
                    }, true, 0, (Action<int>)(ind =>
                    {

                    }), "Exports the the report as a PDF file.")
                }),

            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(compareTab);

            RibbonTabContent plotTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Plot view",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Close plot", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.CloseDark")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        bar.SelectedIndex = PlotType;
                    }), "Closes the plot view.")
                }),

                ("Plot view",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Export as CSV", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportCSV")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        ExportCSV(CurrentPlotID);
                    }), "Exports the data used to create the plot as a CSV file."),

                    ("Export as SVG", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportSVG")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Transform text into paths", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportSVG")), null ),
                        ( "Embed subsetted fonts", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportSVG")), null ),
                        ( "Embed full fonts", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ExportSVG")), null ),
                    }, true, 0, (Action<int>)(ind =>
                    {
                        switch (ind)
                        {
                            case -1:
                            case 0:
                                ExportSVG(CurrentPlotID, SVGContextInterpreter.TextOptions.ConvertIntoPaths);
                                break;
                            case 1:
                                ExportSVG(CurrentPlotID, SVGContextInterpreter.TextOptions.SubsetFonts);
                                break;
                            case 2:
                                ExportSVG(CurrentPlotID, SVGContextInterpreter.TextOptions.EmbedFonts);
                                break;
                        }
                    }), "Exports the plot as a SVG file."),

                    ("Export as PDF", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportPDF")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        ExportPDF(CurrentPlotID);
                    }), "Exports the plot as a PDF file."),

                    ("Export as image", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Image")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Export as PNG", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Image")), null ),
                        ( "Export as TIFF", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Image")), null ),
                        ( "Export as JPEG", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Image")), null ),
                    }, true, 0, (Action<int>)(ind =>
                    {
                        switch (ind)
                        {
                            case -1:
                            case 0:
                                ExportImage(CurrentPlotID, OutputFormats.PNG);
                                break;

                            case 1:
                                ExportImage(CurrentPlotID, OutputFormats.TIFF);
                                break;
                            case 2:
                                ExportImage(CurrentPlotID, OutputFormats.JPEG);
                                break;
                        }
                    }), "Exports the plot as a raster image file."),
                }),

            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(plotTab);

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            loadedTreesTab.ZIndex = 0;
            loadedTreesTab.RenderTransform = offScreen;
            loadedTreesTab.Opacity = 0;
            loadedTreesTab.IsHitTestVisible = false;

            compareTab.ZIndex = 0;
            compareTab.RenderTransform = offScreen;
            compareTab.Opacity = 0;
            compareTab.IsHitTestVisible = false;

            plotTab.ZIndex = 0;
            plotTab.RenderTransform = offScreen;
            plotTab.Opacity = 0;
            plotTab.IsHitTestVisible = false;

            finalTreeTab.ZIndex = 1;
            finalTreeTab.RenderTransform = TransformOperations.Identity;
            finalTreeTab.Opacity = 1;
            finalTreeTab.IsHitTestVisible = true;

            bar.PropertyChanged += async (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    bool cancelled = false;

                    switch (bar.SelectedIndex)
                    {
                        case 0:
                            loadedTreesTab.ZIndex = 0;
                            loadedTreesTab.RenderTransform = offScreen;
                            loadedTreesTab.Opacity = 0;
                            loadedTreesTab.IsHitTestVisible = false;

                            compareTab.ZIndex = 0;
                            compareTab.RenderTransform = offScreen;
                            compareTab.Opacity = 0;
                            compareTab.IsHitTestVisible = false;

                            plotTab.ZIndex = 0;
                            plotTab.RenderTransform = offScreen;
                            plotTab.Opacity = 0;
                            plotTab.IsHitTestVisible = false;

                            finalTreeTab.ZIndex = 1;
                            finalTreeTab.RenderTransform = TransformOperations.Identity;
                            finalTreeTab.Opacity = 1;
                            finalTreeTab.IsHitTestVisible = true;

                            lock (DocumentLock)
                            {
                                lock (FinalTreeReportLock)
                                {
                                    this.Document = FinalTreeReport;

                                    forcedRerender = true;
                                    RenderDocument();
                                }
                            }
                            break;
                        case 1:
                            finalTreeTab.ZIndex = 0;
                            finalTreeTab.RenderTransform = offScreen;
                            finalTreeTab.Opacity = 0;
                            finalTreeTab.IsHitTestVisible = false;

                            compareTab.ZIndex = 0;
                            compareTab.RenderTransform = offScreen;
                            compareTab.Opacity = 0;
                            compareTab.IsHitTestVisible = false;

                            plotTab.ZIndex = 0;
                            plotTab.RenderTransform = offScreen;
                            plotTab.Opacity = 0;
                            plotTab.IsHitTestVisible = false;

                            loadedTreesTab.ZIndex = 1;
                            loadedTreesTab.RenderTransform = TransformOperations.Identity;
                            loadedTreesTab.Opacity = 1;
                            loadedTreesTab.IsHitTestVisible = true;

                            if (LoadedTreesReport == null)
                            {
                                ProgressWindow win;

                                if (parentWindow.Trees.Count > 2)
                                {
                                    win = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 4, LabelText = "Computing tree splits" };
                                }
                                else
                                {
                                    win = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 2, LabelText = "Sampling trees under the YHK model" };
                                }

                                _ = win.ShowDialog2(this);

                                await Task.Run(() => CreateLoadedTreesReport(parentWindow, win));

                                win.Close();
                            }

                            lock (LoadedTreesReportLock)
                            {
                                lock (DocumentLock)
                                {
                                    this.Document = LoadedTreesReport;

                                    forcedRerender = true;
                                    RenderDocument();
                                }
                            }

                            break;

                        case 2:
                            finalTreeTab.ZIndex = 0;
                            finalTreeTab.RenderTransform = offScreen;
                            finalTreeTab.Opacity = 0;
                            finalTreeTab.IsHitTestVisible = false;

                            loadedTreesTab.ZIndex = 0;
                            loadedTreesTab.RenderTransform = offScreen;
                            loadedTreesTab.Opacity = 0;
                            loadedTreesTab.IsHitTestVisible = false;

                            plotTab.ZIndex = 0;
                            plotTab.RenderTransform = offScreen;
                            plotTab.Opacity = 0;
                            plotTab.IsHitTestVisible = false;

                            compareTab.ZIndex = 1;
                            compareTab.RenderTransform = TransformOperations.Identity;
                            compareTab.Opacity = 1;
                            compareTab.IsHitTestVisible = true;

                            if (CustomTreesReport == null)
                            {
                                CreateReportWindow win = new CreateReportWindow(this.parentWindow);

                                await win.ShowDialog2(this);

                                if (win.Result && win.Trees != null && win.Trees.Count > 0)
                                {
                                    int totalCount = 0;

                                    for (int i = 0; i < win.Trees.Count; i++)
                                    {
                                        if (win.Trees[i] != null)
                                        {
                                            totalCount += win.Trees[i].Count;
                                        }
                                    }

                                    if (totalCount > 0)
                                    {
                                        List<TreeNode> trees = new List<TreeNode>(totalCount);

                                        for (int i = 0; i < win.Trees.Count; i++)
                                        {
                                            if (win.Trees[i] != null)
                                            {
                                                trees.AddRange(win.Trees[i]);
                                            }
                                        }

                                        ProgressWindow pwin;

                                        if (trees.Count > 2)
                                        {
                                            pwin = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 4, LabelText = "Computing tree splits" };
                                        }
                                        else
                                        {
                                            pwin = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Creating tree report...", Steps = 2, LabelText = "Sampling trees under the YHK model" };
                                        }

                                        _ = pwin.ShowDialog2(this);

                                        await Task.Run(() => CreateCustomTreesReport(parentWindow, pwin, trees));

                                        pwin.Close();
                                    }
                                }
                            }

                            if (CustomTreesReport == null)
                            {
                                bar.SelectedIndex = PlotType;
                                cancelled = true;
                            }
                            else
                            {
                                lock (CustomTreesReportLock)
                                {
                                    lock (DocumentLock)
                                    {
                                        this.Document = CustomTreesReport;

                                        forcedRerender = true;
                                        RenderDocument();
                                    }
                                }
                            }
                            break;

                        case 3:
                            finalTreeTab.ZIndex = 0;
                            finalTreeTab.RenderTransform = offScreen;
                            finalTreeTab.Opacity = 0;
                            finalTreeTab.IsHitTestVisible = false;

                            loadedTreesTab.ZIndex = 0;
                            loadedTreesTab.RenderTransform = offScreen;
                            loadedTreesTab.Opacity = 0;
                            loadedTreesTab.IsHitTestVisible = false;

                            compareTab.ZIndex = 0;
                            compareTab.RenderTransform = offScreen;
                            compareTab.Opacity = 0;
                            compareTab.IsHitTestVisible = false;

                            plotTab.ZIndex = 1;
                            plotTab.RenderTransform = TransformOperations.Identity;
                            plotTab.Opacity = 1;
                            plotTab.IsHitTestVisible = true;
                            break;
                    }

                    if ((int)e.OldValue == 3)
                    {
                        this.FindControl<Grid>("PlotGrid").Transitions = new Transitions()
                            {
                                new DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                            };

                        this.FindControl<Grid>("PlotGrid").Opacity = 0;
                        this.FindControl<Grid>("PlotGrid").IsHitTestVisible = false;
                        this.FindControl<ScrollViewer>("ScrollViewer").IsHitTestVisible = true;

                        bar.GridItems[3].IsVisible = false;

                        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await System.Threading.Tasks.Task.Delay(100);
                            this.FindControl<Grid>("PlotGrid").IsVisible = false;
                            this.FindControl<Grid>("ZoomPanel").IsVisible = false;
                            this.FindControl<Grid>("PlotGrid").Opacity = 1;
                            this.FindControl<Grid>("PlotGrid").IsHitTestVisible = true;
                        });
                    }

                    if ((int)e.NewValue != 3 && !cancelled)
                    {
                        PlotType = (int)e.NewValue;
                    }
                }
            };

            this.FindControl<Grid>("LeftMouseButtonContainerGrid").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.LeftMouseButton")));
            this.FindControl<Grid>("MouseWheelContainerGrid").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.MouseWheel")));
        }

        private bool programmaticZoomUpdate = false;

        private void ZoomPropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Avalonia.Controls.PanAndZoom.ZoomBorder.ZoomXProperty && !programmaticZoomUpdate)
            {
                double zoom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomX;
                zoom = Math.Min(100, Math.Max(zoom, 0.01));
                SetZoom(zoom, zoom == this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomX);
            }
        }


        public void SetZoom(double zoom, bool skipActualZoom)
        {
            programmaticZoomUpdate = true;
            this.FindControl<NumericUpDown>("ZoomNud").Value = zoom;
            this.FindControl<Slider>("ZoomSlider").Value = Math.Log10(zoom);
            if (!skipActualZoom)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomTo(zoom / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Height * 0.5);
            }
            programmaticZoomUpdate = false;
        }

        private void ZoomPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomX >= 100 && e.Delta.Y > 0)
            {
                e.Handled = true;
            }
        }

        private void ZoomPlusClicked(object sender, RoutedEventArgs e)
        {
            this.FindControl<NumericUpDown>("ZoomNud").Value *= 1.15;
        }

        private void ZoomMinusClicked(object sender, RoutedEventArgs e)
        {
            this.FindControl<NumericUpDown>("ZoomNud").Value /= 1.15;
        }

        private void ZoomSliderChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Slider.ValueProperty && !programmaticZoomUpdate)
            {
                SetZoom(Math.Pow(10, this.FindControl<Slider>("ZoomSlider").Value), false);
            }
        }

        private void ZoomNudChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            if (!programmaticZoomUpdate)
            {
                SetZoom(this.FindControl<NumericUpDown>("ZoomNud").Value, false);
            }
        }

        private void FitZoomButtonClicked(object sender, RoutedEventArgs e)
        {
            AutoFit();
        }

        public void AutoFit()
        {
            double availableWidth = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Bounds.Width;
            double availableHeight = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Bounds.Height;

            double maxZoomX = availableWidth / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Width;
            double maxZoomY = availableHeight / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Height;

            double zoom = Math.Min(maxZoomX, maxZoomY);

            if (!double.IsNaN(zoom) && !double.IsInfinity(zoom) && zoom > 0)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomTo(1 / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Child.Height * 0.5);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").BeginPanTo(0, 0);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").ContinuePanTo(-this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").OffsetX, -this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").OffsetY);

                SetZoom(zoom, false);
            }

            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotViewBox").Transitions = null;

            //WasAutoFitted = true;
        }
    }
}
