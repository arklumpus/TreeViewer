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
using Avalonia.Media;
using Avalonia.VisualTree;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;

namespace TreeViewer
{
    public class AdvancedOpenPage : RibbonFilePageContentTemplate
    {
        public static readonly StyledProperty<int> SelectedFileTypeModuleProperty = AvaloniaProperty.Register<AdvancedOpenPage, int>(nameof(SelectedFileTypeModule), -1);

        public int SelectedFileTypeModule
        {
            get { return GetValue(SelectedFileTypeModuleProperty); }
            set { SetValue(SelectedFileTypeModuleProperty, value); }
        }

        public static readonly StyledProperty<int> SelectedLoadFileModuleProperty = AvaloniaProperty.Register<AdvancedOpenPage, int>(nameof(SelectedLoadFileModule), -1);

        public int SelectedLoadFileModule
        {
            get { return GetValue(SelectedLoadFileModuleProperty); }
            set { SetValue(SelectedLoadFileModuleProperty, value); }
        }

        public AdvancedOpenPage() : base("Advanced open")
        {
            BuildInterface();
        }

        private void BuildInterface()
        {
            Grid parentContainer = new Grid();
            parentContainer.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star) { MaxWidth = 700 });

            ScrollViewer mainContainer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, AllowAutoHide = false, Padding = new Avalonia.Thickness(0, 0, 17, 0) };
            parentContainer.Children.Add(mainContainer);

            this.PageContent = parentContainer;

            StackPanel contentPanel = new StackPanel();
            mainContainer.Content = contentPanel;


            contentPanel.Children.Add(new TextBlock() { Text = "Tree file", FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Margin = new Avalonia.Thickness(0, 0, 0, 0) });

            Grid treeFilePanel = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Height = 50 };
            treeFilePanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            treeFilePanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            treeFilePanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            contentPanel.Children.Add(treeFilePanel);

            StackPanel browseButtonContentPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            browseButtonContentPanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.OpenDark")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 5, 0) });
            browseButtonContentPanel.Children.Add(new TextBlock() { Text = "Browse...", FontSize = 14, Foreground = Brushes.Black });

            Button browseButton = new Button() { Content = browseButtonContentPanel, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            browseButton.Classes.Add("SideBarButton");
            Grid.SetColumn(browseButton, 2);
            treeFilePanel.Children.Add(browseButton);

            ContentControl iconContainer = new ContentControl() { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 9, 0), IsVisible = false };

            treeFilePanel.Children.Add(iconContainer);

            StackPanel treeFileName = new StackPanel() { Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, IsVisible = false };

            TextBlock treeNameBlock = new TextBlock() { Text = "", FontSize = 16 };
            TextBlock treePathBlock = new TextBlock() { Text = "G:\\OneDrive - University of Bristol", Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), FontSize = 12 };
            treeFileName.Children.Add(treeNameBlock);
            treeFileName.Children.Add(treePathBlock);
            Grid.SetColumn(treeFileName, 1);
            treeFilePanel.Children.Add(treeFileName);


            TextBlock fileTypeModuleHeader = new TextBlock() { Text = "File type module", FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Margin = new Avalonia.Thickness(0, 10, 0, 0), IsVisible = false };
            contentPanel.Children.Add(fileTypeModuleHeader);

            ScrollViewer filetypeModuleScrollViewer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, AllowAutoHide = false, Padding = new Avalonia.Thickness(0, 0, 0, 17), IsVisible = false };
            StackPanel filetypeModulesContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            filetypeModuleScrollViewer.Content = filetypeModulesContainer;
            contentPanel.Children.Add(filetypeModuleScrollViewer);

            CheckBox compileCode = new CheckBox() { VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = "Compile and load source code", Margin = new Avalonia.Thickness(0, 5, 0, 0), IsVisible = false };
            contentPanel.Children.Add(compileCode);

            CheckBox storeKey = new CheckBox() { VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = "Store public key", IsVisible = false };
            contentPanel.Children.Add(storeKey);

            Button continueButton = new Button() { Content = new TextBlock() { Text = "Continue", FontSize = 14, Foreground = Brushes.Black }, IsVisible = false, Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            continueButton.Classes.Add("SideBarButton");
            contentPanel.Children.Add(continueButton);

            List<double> OpenPriorities = null;

            TextBlock loadFileModuleHeader = new TextBlock() { Text = "Load file module", FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Margin = new Avalonia.Thickness(0, 5, 0, 0), IsVisible = false };
            contentPanel.Children.Add(loadFileModuleHeader);

            ScrollViewer loadFileModuleScrollViewer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, AllowAutoHide = false, Padding = new Avalonia.Thickness(0, 0, 0, 17), IsVisible = false };
            StackPanel loadFileModulesContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            loadFileModuleScrollViewer.Content = loadFileModulesContainer;
            contentPanel.Children.Add(loadFileModuleScrollViewer);

            Button finishButton = new Button() { Content = new TextBlock() { Text = "Finish", FontSize = 14, Foreground = Brushes.Black }, IsVisible = false };
            finishButton.Classes.Add("SideBarButton");
            contentPanel.Children.Add(finishButton);

            string currentFile = null;

            browseButton.Click += async (s, e) =>
            {
                OpenFileDialog dialog;

                List<FileDialogFilter> filters = new List<FileDialogFilter>();

                List<string> allExtensions = new List<string>();

                for (int i = 0; i < Modules.FileTypeModules.Count; i++)
                {
                    filters.Add(new FileDialogFilter() { Name = Modules.FileTypeModules[i].Extensions[0], Extensions = new List<string>(Modules.FileTypeModules[i].Extensions.Skip(1)) });
                    allExtensions.AddRange(Modules.FileTypeModules[i].Extensions.Skip(1));
                }

                filters.Insert(0, new FileDialogFilter() { Name = "All tree files", Extensions = allExtensions });
                filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

                if (!Modules.IsMac)
                {
                    dialog = new OpenFileDialog()
                    {
                        Title = "Open tree file",
                        AllowMultiple = false,
                        Filters = filters
                    };
                }
                else
                {
                    dialog = new OpenFileDialog()
                    {
                        Title = "Open tree file",
                        AllowMultiple = false
                    };
                }

                string[] result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                if (result != null && result.Length == 1)
                {
                    this.SelectedFileTypeModule = -1;

                    string fileName = System.IO.Path.GetFileName(result[0]);
                    string directoryName = System.IO.Path.GetDirectoryName(result[0]);

                    treeNameBlock.Text = fileName;
                    treePathBlock.Text = directoryName;

                    treeFilePanel.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);

                    string extension = System.IO.Path.GetExtension(result[0]).ToLowerInvariant().Replace(".", "");

                    if (!FileExtensions.EmbeddedFileTypeIcons.Contains(extension))
                    {
                        extension = "tree";
                    }

                    iconContainer.Content = new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FileTypeIcons." + extension)) { Width = 32, Height = 32 };

                    treeFileName.IsVisible = true;
                    iconContainer.IsVisible = true;

                    OpenPriorities = new List<double>();

                    for (int i = 0; i < Modules.FileTypeModules.Count; i++)
                    {
                        try
                        {
                            OpenPriorities.Add(Modules.FileTypeModules[i].IsSupported(result[0]));
                        }
                        catch
                        {
                            OpenPriorities.Add(0);
                        }
                    }

                    currentFile = result[0];

                    BuildFileTypeModuleList(filetypeModulesContainer, OpenPriorities);

                    fileTypeModuleHeader.IsVisible = true;
                    filetypeModuleScrollViewer.IsVisible = true;
                    compileCode.IsVisible = true;
                    storeKey.IsVisible = true;
                    compileCode.IsHitTestVisible = true;
                    storeKey.IsHitTestVisible = true;
                    loadFileModuleHeader.IsVisible = false;
                    loadFileModuleScrollViewer.IsVisible = false;
                    finishButton.IsVisible = false;
                }
            };

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == AdvancedOpenPage.SelectedFileTypeModuleProperty)
                {
                    if ((int)e.NewValue >= 0)
                    {
                        continueButton.IsVisible = true;
                    }
                    else
                    {
                        continueButton.IsVisible = false;
                    }
                }
                else if (e.Property == AdvancedOpenPage.SelectedLoadFileModuleProperty)
                {
                    if ((int)e.NewValue >= 0)
                    {
                        finishButton.IsVisible = true;
                    }
                    else
                    {
                        finishButton.IsVisible = false;
                    }
                }
            };

            System.IO.FileInfo OpenedFileInfo = null;
            Action<double> OpenerProgressAction = null;
            List<(string, Dictionary<string, object>)> ModuleSuggestions = null;
            IEnumerable<TreeNode> OpenedFile = null;

            continueButton.Click += async (s, e) =>
            {
                if (OpenPriorities[this.SelectedFileTypeModule] <= 0)
                {
                    MessageBox box = new MessageBox("Confirm action", "The file opener module has reported that it is not able to handle this file. Are you sure you wish to proceed using this module?", MessageBox.MessageBoxButtonTypes.YesNo);
                    await box.ShowDialog2(this.FindAncestorOfType<Window>());
                    if (box.Result != MessageBox.Results.Yes)
                    {
                        return;
                    }
                }

                try
                {
                    bool codePermissionGranted = compileCode.IsChecked == true;

                    bool addKeyPermissionGranted = storeKey.IsChecked == true;

                    bool askForCodePermission(RSAParameters? publicKey)
                    {
                        if (publicKey.HasValue && addKeyPermissionGranted)
                        {
                            CryptoUtils.AddPublicKey(publicKey.Value);
                        }

                        return codePermissionGranted;
                    };

                    ModuleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };

                    OpenerProgressAction = (_) => { };

                    OpenedFile = Modules.FileTypeModules[this.SelectedFileTypeModule].OpenFile(currentFile, ModuleSuggestions, (val) => { OpenerProgressAction(val); }, askForCodePermission);

                    OpenedFileInfo = new System.IO.FileInfo(currentFile);

                    string OpenerModuleId = Modules.FileTypeModules[SelectedFileTypeModule].Id;

                    List<double> LoadPriorities = new List<double>();

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

                    BuildLoadFileModules(loadFileModulesContainer, LoadPriorities);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "An error has occurred while opening the file!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                    return;
                }


                continueButton.IsVisible = false;
                for (int i = 0; i < FileTypeModuleButtons.Count; i++)
                {
                    FileTypeModuleButtons[i].IsEnabled = false;
                }
                compileCode.IsHitTestVisible = false;
                storeKey.IsHitTestVisible = false;

                loadFileModuleHeader.IsVisible = true;
                loadFileModuleScrollViewer.IsVisible = true;


            };

            finishButton.Click += async (s, e) =>
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

                    TreeCollection LoadedTrees = null;

                    MainWindow parent = this.FindAncestorOfType<MainWindow>();
                    int sftm = this.SelectedFileTypeModule;
                    int slfm = this.SelectedLoadFileModule;

                    SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

                    Thread thr = new Thread(async () =>
                    {
                        (LoadedTrees, OpenerProgressAction) = await Modules.LoadFileModules[slfm].Load(parent, OpenedFileInfo, Modules.FileTypeModules[sftm].Id, OpenedFile, ModuleSuggestions, OpenerProgressAction, progressAction);

                        semaphore.Release();
                    });

                    thr.Start();

                    _ = progressWin.ShowDialog2(parent);

                    await semaphore.WaitAsync();
                    semaphore.Release();
                    semaphore.Dispose();

                    if (LoadedTrees != null)
                    {

                        if (parent.Trees == null)
                        {
                            parent.Trees = LoadedTrees;
                            parent.FileOpened(ModuleSuggestions, currentFile, progressWin);

                            if (!string.IsNullOrEmpty(currentFile))
                            {
                                parent.Title = "TreeViewer - " + System.IO.Path.GetFileName(currentFile);
                                parent.OriginalFileName = currentFile;
                            }
                        }
                        else
                        {
                            progressWin.Close();
                            MainWindow win2 = new MainWindow(LoadedTrees, ModuleSuggestions, currentFile);
                            win2.Show();
                        }
                    }
                    else
                    {
                        progressWin.Close();
                    }

                    this.Reset();
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "An error has occurred while loading the trees!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                }
            };

            this.Reset = () =>
            {
                continueButton.IsVisible = false;
                filetypeModuleScrollViewer.IsVisible = false;
                compileCode.IsVisible = false;
                storeKey.IsVisible = false;
                loadFileModuleHeader.IsVisible = false;
                loadFileModuleScrollViewer.IsVisible = false;
                finishButton.IsVisible = false;
                fileTypeModuleHeader.IsVisible = false;
                treeFileName.IsVisible = false;
                iconContainer.IsVisible = false;
                treeFilePanel.ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Auto);
                this.SelectedFileTypeModule = -1;
                this.SelectedLoadFileModule = -1;
            };

            this.AttachedToVisualTree += (s, e) =>
            {
                this.FindAncestorOfType<RibbonFilePage>().PropertyChanged += (s, e) =>
                {
                    if (e.Property == RibbonFilePage.IsVisibleProperty)
                    {
                        if ((bool)e.NewValue)
                        {
                            this.Reset?.Invoke();
                        }
                    }
                };
            };
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == AdvancedOpenPage.SelectedFileTypeModuleProperty)
            {
                int oldValue = change.OldValue.GetValueOrDefault<int>();
                int newValue = change.NewValue.GetValueOrDefault<int>();

                if (oldValue >= 0)
                {
                    FileTypeModuleButtons[oldValue].Classes.Remove("active");
                }

                if (newValue >= 0)
                {
                    FileTypeModuleButtons[newValue].Classes.Add("active");
                }
            }
            else if (change.Property == AdvancedOpenPage.SelectedLoadFileModuleProperty)
            {
                int oldValue = change.OldValue.GetValueOrDefault<int>();
                int newValue = change.NewValue.GetValueOrDefault<int>();

                if (oldValue >= 0)
                {
                    LoadFileModuleButtons[oldValue].Classes.Remove("active");
                }

                if (newValue >= 0)
                {
                    LoadFileModuleButtons[newValue].Classes.Add("active");
                }
            }
            else if (change.Property == AdvancedOpenPage.IsVisibleProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    this.Reset?.Invoke();
                }
            }
        }

        public Action Reset;

        List<Button> FileTypeModuleButtons;
        List<Button> LoadFileModuleButtons;

        private void BuildFileTypeModuleList(StackPanel container, List<double> OpenPriorities)
        {
            container.Children.Clear();

            FileTypeModuleButtons = new List<Button>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                Button brd = new Button() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Width = 100, Height = 80, Padding = new Thickness(0) };
                brd.Classes.Add("SideBarButtonNoForeground");
                FileTypeModuleButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5) };
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                TextBlock titleBlock = new TextBlock() { Text = Modules.FileTypeModules[i].Name, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Foreground = Brushes.Black, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 14 };
                Grid.SetColumnSpan(titleBlock, 2);

                grd.Children.Add(titleBlock);

                {
                    TextBlock blk = new TextBlock() { FontStyle = FontStyle.Italic, Text = OpenPriorities[i].ToString(1), Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                HelpButton helpCanvas = new HelpButton();
                Grid.SetColumn(helpCanvas, 1);
                Grid.SetRow(helpCanvas, 1);
                AvaloniaBugFixes.SetToolTip(helpCanvas, Modules.FileTypeModules[i].HelpText);
                int index = i;
                helpCanvas.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(helpCanvas).Pointer.Capture(helpCanvas);
                    e.Handled = true;
                };

                helpCanvas.Click += (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.FileTypeModules[index].Id].BuildReadmeMarkdown(), Modules.FileTypeModules[index].Id);
                    win.Show(this.FindAncestorOfType<Window>());
                };
                grd.Children.Add(helpCanvas);

                brd.Content = grd;

                container.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedFileTypeModule = index;
                };
            }
        }

        private void BuildLoadFileModules(StackPanel container, List<double> LoadPriorities)
        {
            container.Children.Clear();

            LoadFileModuleButtons = new List<Button>();

            for (int i = 0; i < Modules.LoadFileModules.Count; i++)
            {
                Button brd = new Button() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Width = 100, Height = 80, Padding = new Thickness(0) };
                brd.Classes.Add("SideBarButtonNoForeground");
                LoadFileModuleButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5) };
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                TextBlock titleBlock = new TextBlock() { Text = Modules.LoadFileModules[i].Name, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Foreground = Brushes.Black, TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center, FontSize = 14 };
                Grid.SetColumnSpan(titleBlock, 2);

                grd.Children.Add(titleBlock);

                {
                    TextBlock blk = new TextBlock() { FontStyle = FontStyle.Italic, Text = LoadPriorities[i].ToString(1), Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                HelpButton helpCanvas = new HelpButton();
                Grid.SetColumn(helpCanvas, 1);
                Grid.SetRow(helpCanvas, 1);
                AvaloniaBugFixes.SetToolTip(helpCanvas, Modules.LoadFileModules[i].HelpText);
                int index = i;
                helpCanvas.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(helpCanvas).Pointer.Capture(helpCanvas);
                    e.Handled = true;
                };

                helpCanvas.Click += (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.LoadFileModules[index].Id].BuildReadmeMarkdown(), Modules.LoadFileModules[index].Id);
                    win.Show(this.FindAncestorOfType<Window>());
                };
                grd.Children.Add(helpCanvas);

                brd.Content = grd;

                container.Children.Add(brd);

                brd.Click += (s, e) =>
                {
                    this.SelectedLoadFileModule = index;
                };
            }
        }
    }
}
