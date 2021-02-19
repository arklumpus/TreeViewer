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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhyloTree;
using System.Security.Cryptography;

namespace TreeViewer
{
    public class AdvancedOpenWindow : Window
    {
        public AdvancedOpenWindow()
        {
            this.InitializeComponent();
        }

        List<double> OpenPriorities = new List<double>();
        List<double> LoadPriorities = new List<double>();

        string FileName;
        int LoaderModuleIndex = -1;

        IEnumerable<TreeNode> OpenedFile;
        FileInfo OpenedFileInfo;
        string OpenerModuleId;

        Action<double> OpenerProgressAction = (_) => { };

        public TreeCollection LoadedTrees = null;

        public List<(string, Dictionary<string, object>)> ModuleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };


        public AdvancedOpenWindow(string fileName)
        {
            FileName = fileName;

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    OpenPriorities.Add(Modules.FileTypeModules[i].IsSupported(FileName));
                }
                catch
                {
                    OpenPriorities.Add(0);
                }
            }


            this.InitializeComponent();
        }

        //Page HelpIconPage;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Grid openModulesGrid = this.FindControl<Grid>("OpenModuleContainer");

            List<(Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>)> borders = new List<(Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>)>();

       /*    HelpIconPage = new Page(24, 24);
            HelpIconPage.Graphics.FillPath(new GraphicsPath().Arc(12, 12, 12, 0, 2 * Math.PI), Colour.FromRgb(63, 72, 204));
            HelpIconPage.Graphics.StrokePath(new GraphicsPath().Arc(12, 12, 9.5, 0, 2 * Math.PI), Colour.FromRgb(255, 255, 255), 1.5);
            Font helpFont = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 14);
            HelpIconPage.Graphics.FillPath(new GraphicsPath().AddText(12 - helpFont.MeasureText("?").Width / 2, 12, "?", helpFont, TextBaselines.Middle), Colour.FromRgb(255, 255, 255));
       */

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                openModulesGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star) { MinWidth = 160 });

                Border brd = new Border() { Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(10), Padding = new Thickness(10) };

                SolidColorBrush backgroundBrush;
                SolidColorBrush hoverBrush;

                if (OpenPriorities[i] <= 0)
                {
                    backgroundBrush = new SolidColorBrush(Color.FromRgb(253, 227, 228));
                    hoverBrush = new SolidColorBrush(Color.FromRgb(204, 221, 239));
                }
                else
                {
                    backgroundBrush = new SolidColorBrush(Color.FromRgb((byte)(228 * OpenPriorities[i] / OpenPriorities.Max() + 240 * (1 - OpenPriorities[i] / OpenPriorities.Max())), (byte)(250 * OpenPriorities[i] / OpenPriorities.Max() + 240 * (1 - OpenPriorities[i] / OpenPriorities.Max())), (byte)(235 * OpenPriorities[i] / OpenPriorities.Max() + 240 * (1 - OpenPriorities[i] / OpenPriorities.Max()))));
                    hoverBrush = new SolidColorBrush(Color.FromRgb(191, 232, 243));
                }

                brd.Background = backgroundBrush;

                Grid grd = new Grid();
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                //StackPanel pnl = new StackPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                
                {
                    TextBlock blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = Modules.FileTypeModules[i].Name, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    Grid.SetColumnSpan(blk, 2);
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { FontStyle = FontStyle.Italic, Text = "Priority: " + OpenPriorities[i].ToString(1), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                HelpButton helpCanvas = new HelpButton();
                ToolTip.SetTip(helpCanvas, Modules.FileTypeModules[i].HelpText);
                int index = i;
                helpCanvas.PointerReleased += (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.FileTypeModules[index].Id].BuildReadmeMarkdown(), Modules.FileTypeModules[index].Id);
                    win.Show(this);
                };
                Grid.SetRow(helpCanvas, 1);
                Grid.SetColumn(helpCanvas, 1);
                grd.Children.Add(helpCanvas);

                brd.Child = grd;

                //Canvas helpCanvas = HelpIconPage.PaintToCanvas();
                

                Grid.SetColumn(brd, i);
                openModulesGrid.Children.Add(brd);

                Canvas can = new Canvas() { Width = 2, Background = new SolidColorBrush(Color.FromRgb(200, 200, 200)), Margin = new Thickness(0, 0, 0, 5) };
                Grid.SetColumn(can, i);
                Grid.SetRow(can, 1);
                openModulesGrid.Children.Add(can);
                Avalonia.Controls.Shapes.Path pth = new Avalonia.Controls.Shapes.Path() { Data = PathGeometry.Parse("M6,12 L0,0 L12,0"), Width = 12, Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
                Grid.SetColumn(pth, i);
                Grid.SetRow(pth, 1);
                openModulesGrid.Children.Add(pth);

                int j = i;

                void enterHandler(object s, Avalonia.Input.PointerEventArgs e)
                {
                    brd.BorderThickness = new Thickness(3);
                    brd.BorderBrush = can.Background = pth.Fill = new SolidColorBrush(Color.FromRgb(154, 214, 250));
                    brd.Background = hoverBrush;
                    brd.Margin = new Thickness(10, 8);
                    brd.Padding = new Thickness(8, 10);
                }

                void leaveHandler(object s, Avalonia.Input.PointerEventArgs e)
                {
                    brd.BorderThickness = new Thickness(1);
                    brd.BorderBrush = can.Background = pth.Fill = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                    brd.Background = backgroundBrush;
                    brd.Margin = new Thickness(10);
                    brd.Padding = new Thickness(10);
                }

                async void clickHandler(object s, Avalonia.Input.PointerReleasedEventArgs e)
                {
                    if (OpenPriorities[j] <= 0)
                    {
                        MessageBox box = new MessageBox("Confirm action", "The file opener module has reported that it is not able to handle this file. Are you sure you wish to proceed using this module?", MessageBox.MessageBoxButtonTypes.YesNo);
                        await box.ShowDialog2(this);
                        if (box.Result != MessageBox.Results.Yes)
                        {
                            return;
                        }
                    }

                    brd.BorderThickness = new Thickness(3);
                    brd.BorderBrush = can.Background = pth.Fill = new SolidColorBrush(Color.FromRgb(154, 214, 250));
                    brd.Background = hoverBrush;
                    brd.Margin = new Thickness(10, 8);
                    brd.Padding = new Thickness(8, 10);

                    foreach ((Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>) br in borders)
                    {
                        br.Item1.Cursor = Avalonia.Input.Cursor.Default;
                        br.Item1.PointerEnter -= br.Item2;
                        br.Item1.PointerLeave -= br.Item3;
                        br.Item1.PointerReleased -= br.Item4;
                    }

                    await LoadFile(j);
                }

                borders.Add((brd, enterHandler, leaveHandler, clickHandler));

                brd.PointerEnter += enterHandler;
                brd.PointerLeave += leaveHandler;
                brd.PointerReleased += clickHandler;
            }

            BuildLoadModules();
        }

        private void BuildLoadModules()
        {
            Grid loadModulesGrid = this.FindControl<Grid>("LoadModuleContainer");

            while (loadModulesGrid.Children.Count > 0)
            {
                loadModulesGrid.Children.RemoveAt(0);
            }

            while (loadModulesGrid.ColumnDefinitions.Count > 0)
            {
                loadModulesGrid.ColumnDefinitions.RemoveAt(0);
            }

            List<(Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>)> borders = new List<(Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>)>();

            for (int i = 0; i < Modules.LoadFileModules.Count; i++)
            {
                loadModulesGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star) { MinWidth = 160 });

                Border brd = new Border() { Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(10), Padding = new Thickness(10) };

                SolidColorBrush backgroundBrush = null;
                SolidColorBrush hoverBrush = null;

                if (LoadPriorities.Count == 0)
                {
                    brd.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
                }
                else
                {
                    if (LoadPriorities[i] == 0)
                    {
                        backgroundBrush = new SolidColorBrush(Color.FromRgb(253, 227, 228));
                        hoverBrush = new SolidColorBrush(Color.FromRgb(204, 221, 239));
                    }
                    else
                    {
                        backgroundBrush = new SolidColorBrush(Color.FromRgb((byte)(228 * LoadPriorities[i] / LoadPriorities.Max() + 240 * (1 - LoadPriorities[i] / LoadPriorities.Max())), (byte)(250 * LoadPriorities[i] / LoadPriorities.Max() + 240 * (1 - LoadPriorities[i] / LoadPriorities.Max())), (byte)(235 * LoadPriorities[i] / LoadPriorities.Max() + 240 * (1 - LoadPriorities[i] / LoadPriorities.Max()))));
                        hoverBrush = new SolidColorBrush(Color.FromRgb(191, 232, 243));
                    }

                    brd.Background = backgroundBrush;
                }

                Grid grd = new Grid();

                //StackPanel pnl = new StackPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                //StackPanel pnl = new StackPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                {
                    TextBlock blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = Modules.LoadFileModules[i].Name, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    Grid.SetColumnSpan(blk, 2);
                    grd.Children.Add(blk);
                }

                {
                    
                }

                
                if (LoadPriorities.Count > 0)
                {
                    TextBlock blk = new TextBlock() { FontStyle = FontStyle.Italic, Text = "Priority: " + LoadPriorities[i].ToString(1), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }
                else
                {
                    TextBlock blk = new TextBlock() { FontStyle = FontStyle.Italic, Text = "Priority: ?", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                //grd.Children.Add(pnl);

                HelpButton helpCanvas = new HelpButton();
                Grid.SetRow(helpCanvas, 1);
                Grid.SetColumn(helpCanvas, 1);
                ToolTip.SetTip(helpCanvas, Modules.LoadFileModules[i].HelpText);
                int index = i;
                helpCanvas.PointerReleased += (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.LoadFileModules[index].Id].BuildReadmeMarkdown(), Modules.LoadFileModules[index].Id);
                    win.Show(this);
                };
                grd.Children.Add(helpCanvas);

                brd.Child = grd;
                Grid.SetColumn(brd, i);

                //Canvas helpCanvas = HelpIconPage.PaintToCanvas();
                

                loadModulesGrid.Children.Add(brd);

                if (LoadPriorities.Count > 0)
                {
                    int j = i;

                    void enterHandler(object s, Avalonia.Input.PointerEventArgs e)
                    {
                        brd.BorderThickness = new Thickness(3);
                        brd.BorderBrush = new SolidColorBrush(Color.FromRgb(154, 214, 250));
                        brd.Background = hoverBrush;
                        brd.Margin = new Thickness(10, 8);
                        brd.Padding = new Thickness(8, 10);
                    }

                    void leaveHandler(object s, Avalonia.Input.PointerEventArgs e)
                    {
                        brd.BorderThickness = new Thickness(1);
                        brd.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                        brd.Background = backgroundBrush;
                        brd.Margin = new Thickness(10);
                        brd.Padding = new Thickness(10);
                    }

                    async void clickHandler(object s, Avalonia.Input.PointerReleasedEventArgs e)
                    {
                        if (LoadPriorities[j] <= 0)
                        {
                            MessageBox box = new MessageBox("Confirm action", "The file loader module has reported that it is not able to handle this data. Are you sure you wish to proceed using this module?", MessageBox.MessageBoxButtonTypes.YesNo);
                            await box.ShowDialog2(this);
                            if (box.Result != MessageBox.Results.Yes)
                            {
                                leaveHandler(s, null);
                                return;
                            }
                        }

                        brd.BorderThickness = new Thickness(3);
                        brd.BorderBrush = new SolidColorBrush(Color.FromRgb(154, 214, 250));
                        brd.Background = hoverBrush;
                        brd.Margin = new Thickness(10, 8);
                        brd.Padding = new Thickness(8, 10);

                        foreach ((Border, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerEventArgs>, EventHandler<Avalonia.Input.PointerReleasedEventArgs>) br in borders)
                        {
                            br.Item1.Cursor = Avalonia.Input.Cursor.Default;
                            br.Item1.PointerEnter -= br.Item2;
                            br.Item1.PointerLeave -= br.Item3;
                            br.Item1.PointerReleased -= br.Item4;
                        }

                        LoaderModuleIndex = j;

                        this.FindControl<Button>("OKButton").IsEnabled = true;
                    }

                    borders.Add((brd, enterHandler, leaveHandler, clickHandler));

                    brd.PointerEnter += enterHandler;
                    brd.PointerLeave += leaveHandler;
                    brd.PointerReleased += clickHandler;
                }
            }

            if (LoadPriorities.Count == 0)
            {
                Grid grd = new Grid() { Background = new SolidColorBrush(Color.FromArgb(220, 255, 255, 255)), Margin = new Thickness(10) };
                grd.Children.Add(new TextBlock() { FontSize = 18, FontStyle = FontStyle.Italic, Text = "Please choose a file type module!", TextWrapping = TextWrapping.Wrap, Foreground = new SolidColorBrush(Color.FromRgb(148, 12, 18)), FontWeight = FontWeight.Bold, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                Grid.SetColumnSpan(grd, Modules.LoadFileModules.Count);
                loadModulesGrid.Children.Add(grd);
            }
        }

        private async void CompileCodePermissionChecked(object sender, RoutedEventArgs e)
        {
            await new MessageBox("Attention!", "You should only load and compile source code if you trust the source of the file and/or you have accurately reviewed the code!").ShowDialog2(this);
        }

        private async void AddKeyPermissionChecked(object sender, RoutedEventArgs e)
        {
            await new MessageBox("Attention!", "You should only add the public key if you trust the source of the file!").ShowDialog2(this);
        }

        private void CompileCodePermissionUnchecked(object sender, RoutedEventArgs e)
        {
            this.FindControl<CheckBox>("AddKeyPermission").IsChecked = false;
        }

        private async Task LoadFile(int moduleIndex)
        {
            try
            {
                bool codePermissionGranted = this.FindControl<CheckBox>("CompileCodePermission").IsChecked == true;

                bool addKeyPermissionGranted = this.FindControl<CheckBox>("AddKeyPermission").IsChecked == true;

                this.FindControl<CheckBox>("CompileCodePermission").IsEnabled = false;
                this.FindControl<CheckBox>("AddKeyPermission").IsEnabled = false;

                bool askForCodePermission(RSAParameters? publicKey)
                {
                    if (publicKey.HasValue && addKeyPermissionGranted)
                    {
                        CryptoUtils.AddPublicKey(publicKey.Value);
                    }

                    return codePermissionGranted;
                };


                OpenedFile = Modules.FileTypeModules[moduleIndex].OpenFile(FileName, ModuleSuggestions, (val) => { OpenerProgressAction(val); }, askForCodePermission);

                OpenedFileInfo = new FileInfo(FileName);

                OpenerModuleId = Modules.FileTypeModules[moduleIndex].Id;

                for (int i = 0; i < Modules.LoadFileModules.Count; i++)
                {
                    try
                    {
                        LoadPriorities.Add(Modules.LoadFileModules[i].IsSupported(OpenedFileInfo, OpenerModuleId, OpenedFile));
                    }
                    catch
                    {
                        LoadPriorities.Add(0);
                    }
                }

                BuildLoadModules();
            }
            catch (Exception e)
            {
                await new MessageBox("Error!", "An error has occurred while opening the file!\n" + e.Message).ShowDialog2(this);
                this.Close();
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                EventWaitHandle progressWindowHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                ProgressWindow progressWin = new ProgressWindow(progressWindowHandle) { ProgressText = "Opening and loading file..." };
                Action<double> progressAction = (progress) =>
                {
                    if (progress >= 0)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            progressWin.IsIndeterminate = false;
                            progressWin.Progress = progress;
                        });
                    }
                    else
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            progressWin.IsIndeterminate = true;
                        });
                    }
                };

                Thread thr = new Thread(async () =>
                {
                    LoadedTrees = Modules.LoadFileModules[LoaderModuleIndex].Load(this, OpenedFileInfo, OpenerModuleId, OpenedFile, ModuleSuggestions, ref OpenerProgressAction, progressAction);

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                });

                thr.Start();

                await progressWin.ShowDialog2(this);
            }
            catch (Exception ex)
            {
                LoadedTrees = null;
                await new MessageBox("Error!", "An error has occurred while loading the trees!\n" + ex.Message).ShowDialog2(this);
            }

            this.Close();
        }
    }
}
