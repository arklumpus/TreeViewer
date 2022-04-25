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
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VectSharp.MarkdownCanvas;

namespace TreeViewer
{
    public class ModuleManagerWindow : ChildWindow
    {
        public ModuleManagerWindow()
        {
            this.InitializeComponent();
        }

        private readonly string[] headers = new string[] { "File type", "Load file", "Transformer", "Further transformation", "Coordinates", "Plot action", "Selection action", "Action", "Menu action" };

        private StackPanel[] moduleContainers;
        private TextBlock[] nameContainers;
        private TextBox[] idContainers;
        private TextBlock[] authorContainers;
        private TextBlock[] versionContainers;
        private Canvas[] verifiedIcons;
        private TextBlock[] helpContainers;
        private TextBox[] sourceContainers;
        private MarkdownCanvasControl[] manualContainers;
        private string[] selectedModules;
        private Grid[] noModuleGrids;
        private RibbonBar bar;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            bar = new RibbonBar((from el in headers select (el, false)).ToArray());

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
                bar.Background = new SolidColorBrush(Color.FromRgb(0, 114, 178));
                bar.Margin = new Thickness(-1, 0, -1, 0);
                bar.Classes.Add("Colorful");
            }
            else
            {
                bar.Classes.Add("Grey");
            }

            this.FindControl<Grid>("RibbonBarContainer").Children.Add(bar);

            RibbonTabContent moduleTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Module",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Load", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.LoadModule")), null, new List<(string, Control, string)>(){
                        ( "", null, null ),
                        ( "Load from source", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.LoadModule")), null ),
                        ( "Load from file", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.LoadModule")), null ),
                        ( "Load from repository", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleRepository")), null )
                    }, true, 0, (Action<int>)(async ind =>
                    {
                        if (ind < 0)
                        {
                            ind = 2;
                        }

                        switch (ind)
                        {
                            case 0:
                                LoadFromSourceClicked(null, new EventArgs());
                                break;
                            case 1:
                                LoadClicked(null, new EventArgs());
                                break;
                            case 2:
                                await LoadFromRepositoryClicked();
                                break;
                        }
                    }), "Loads a new module for the current session."),

                     ("Install", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.InstallModule")), null, new List<(string, Control, string)>(){
                        ( "", null, null ),
                        ( "Install from file", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.InstallModule")), null ),
                        ( "Install from repository", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleRepository")), null )
                    }, true, 0, (Action<int>)(async ind =>
                    {
                        if (ind < 0)
                        {
                            ind = 1;
                        }

                        switch (ind)
                        {
                            case 0:
                                InstallClicked(null, new EventArgs());
                                break;
                            case 1:
                                await InstallFromRepositoryClicked();
                                break;
                        }
                    }), "Permanently installs a new module."),

                    ("Export", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ExportModule")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        ExportClicked(null, new EventArgs());
                    }), "Exports a module as a json.zip file."),

                    ("Uninstall", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.UninstallModule")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                    {
                        UninstallClicked(null, new EventArgs());
                    }), "Uninstalls a module that is currently installed.")
                }),

            })
            { Height = 80 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(moduleTab);

            NativeMenu menu = new NativeMenu();
            NativeMenuItem moduleMenu = new NativeMenuItem() { Header = "Module" };

            NativeMenu moduleSubMenu = new NativeMenu();
            NativeMenuItem loadSourceItem = new NativeMenuItem() { Header = "Load from source...", Command = new SimpleCommand(win => true, a => LoadFromSourceClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(loadSourceItem);
            NativeMenuItem loadItem = new NativeMenuItem() { Header = "Load...", Command = new SimpleCommand(win => true, a => LoadClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(loadItem);
            NativeMenuItem loadRepositoryItem = new NativeMenuItem() { Header = "Load from repository...", Command = new SimpleCommand(win => true, async a => { await LoadFromRepositoryClicked(); }, null, null) };
            moduleSubMenu.Add(loadRepositoryItem);
            moduleSubMenu.Add(new NativeMenuItemSeparator());
            NativeMenuItem installItem = new NativeMenuItem() { Header = "Install...", Command = new SimpleCommand(win => true, a => InstallClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(installItem);
            NativeMenuItem installRepositoryItem = new NativeMenuItem() { Header = "Install from repository...", Command = new SimpleCommand(win => true, async a => { await InstallFromRepositoryClicked(); }, null, null) };
            moduleSubMenu.Add(installRepositoryItem);
            moduleSubMenu.Add(new NativeMenuItemSeparator());
            NativeMenuItem exportItem = new NativeMenuItem() { Header = "Export...", Command = new SimpleCommand(win => true, a => ExportClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(exportItem);
            moduleSubMenu.Add(new NativeMenuItemSeparator());
            NativeMenuItem uninstallItem = new NativeMenuItem() { Header = "Uninstall...", Command = new SimpleCommand(win => true, a => UninstallClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(uninstallItem);

            moduleMenu.Menu = moduleSubMenu;
            menu.Add(moduleMenu);

            NativeMenu.SetMenu(this, menu);

            moduleContainers = new StackPanel[headers.Length];
            nameContainers = new TextBlock[headers.Length];
            idContainers = new TextBox[headers.Length];
            authorContainers = new TextBlock[headers.Length];
            versionContainers = new TextBlock[headers.Length];
            verifiedIcons = new Canvas[headers.Length];
            helpContainers = new TextBlock[headers.Length];
            sourceContainers = new TextBox[headers.Length];
            manualContainers = new MarkdownCanvasControl[headers.Length];
            selectedModules = new string[headers.Length];
            noModuleGrids = new Grid[headers.Length];

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            List<Grid> tabItems = new List<Grid>();

            for (int i = 0; i < headers.Length; i++)
            {
                Grid content = new Grid();
                content.ColumnDefinitions.Add(new ColumnDefinition(250, GridUnitType.Pixel));
                content.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                content.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Canvas separator = new Canvas() { Margin = new Thickness(5, 10, 5, 10), Background = new SolidColorBrush(Color.FromRgb(191, 191, 191)), Width = 1 };
                Grid.SetColumn(separator, 1);
                content.Children.Add(separator);

                ScrollViewer scr = new ScrollViewer() { Margin = new Thickness(0, 5, 0, 5), VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, Padding = new Thickness(0, 0, 10, 0) };

                StackPanel moduleContainer = new StackPanel();
                moduleContainers[i] = moduleContainer;
                scr.Content = moduleContainer;

                content.Children.Add(scr);

                Grid grd = new Grid() { Margin = new Thickness(10, 0, 5, 5) };
                Grid.SetColumn(grd, 2);
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

                TextBlock nameContainer = new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) };
                nameContainers[i] = nameContainer;
                grd.Children.Add(nameContainer);

                TextBox idContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), Foreground = new Avalonia.Media.SolidColorBrush(0xFF707070), Background = null, BorderBrush = null, BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true, Padding = new Thickness(0), MinHeight = 0, FontSize = 12 };
                idContainers[i] = idContainer;
                Grid.SetRow(idContainer, 1);
                grd.Children.Add(idContainer);

                StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                TextBlock versionContainer = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF606060) };
                versionContainers[i] = versionContainer;
                pnl.Children.Add(versionContainer);

                pnl.Children.Add(new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontStyle = Avalonia.Media.FontStyle.Italic, Margin = new Thickness(0, 0, 5, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF606060), Text = "by" });

                TextBlock authorContainer = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF606060) };
                authorContainers[i] = authorContainer;
                pnl.Children.Add(authorContainer);

                Canvas verifiedIcon = new Canvas() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 16, Height = 16, Background = Brushes.Transparent };
                verifiedIcon.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Icons.CrossGeometry, Stroke = new SolidColorBrush(Color.FromRgb(213, 94, 0)), Margin = new Thickness(3), StrokeThickness = 2 });
                AvaloniaBugFixes.SetToolTip(verifiedIcon, "Code signature not verified!");
                verifiedIcons[i] = verifiedIcon;
                pnl.Children.Add(verifiedIcon);
                Grid.SetRow(pnl, 2);
                grd.Children.Add(pnl);

                TextBlock helpContainer = new TextBlock() { Margin = new Thickness(0, 5, 0, 5), TextWrapping = Avalonia.Media.TextWrapping.Wrap };
                helpContainers[i] = helpContainer;
                Grid.SetRow(helpContainer, 3);
                grd.Children.Add(helpContainer);

                RibbonBar modulePreviewSelector = new RibbonBar(new (string, bool)[] { ("Manual", false), ("Source code", false) }) { FontSize = 14, Margin = new Thickness(0, 5, 0, 0) };
                Grid.SetRow(modulePreviewSelector, 4);
                grd.Children.Add(modulePreviewSelector);

                ((Grid)modulePreviewSelector.Content).Background = Brushes.Transparent;
                modulePreviewSelector.Classes.Add("Grey");

                Border manualContainerContainer = new Border() { Margin = new Thickness(5), BorderBrush = Brushes.Black, BorderThickness = new Thickness(1) };
                Grid.SetRow(manualContainerContainer, 5);
                manualContainerContainer.Classes.Add("TabItem");
                grd.Children.Add(manualContainerContainer);

                MarkdownCanvasControl manualContainer = new MarkdownCanvasControl() { Background = Avalonia.Media.Brushes.White, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Top, MaxRenderWidth = 1000 };
                manualContainer.Renderer.ImageUnitMultiplier /= 1.4;
                manualContainer.Renderer.ImageMultiplier *= 1.4;
                manualContainer.FontSize = 14;
                manualContainer.Renderer.HeaderFontSizeMultipliers[0] *= 1.3;
                manualContainer.Renderer.CodeFont = Modules.CodeFontFamily;
                manualContainer.Renderer.Margins = new VectSharp.Markdown.Margins(0, 10, 0, 0);
                manualContainer.Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
                Grid.SetRow(manualContainer, 5);
                manualContainers[i] = manualContainer;

                manualContainerContainer.Child = manualContainer;

                OpenWindowButton openWindowButton = new OpenWindowButton() { Width = 20, Height = 20, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 0, 4), Padding = new Thickness(2) };
                openWindowButton.Classes.Add("PlainButton");
                ((StackPanel)modulePreviewSelector.GridItems[1].Parent).Children.Add(openWindowButton);
                openWindowButton.Click += async (s, e) =>
                {
                    await OpenSourceCodeWindow();
                };

                Grid sourceContainerContainer = new Grid() { Opacity = 0, RenderTransform = offScreen, IsHitTestVisible = false };
                Grid.SetRow(sourceContainerContainer, 5);
                sourceContainerContainer.Classes.Add("TabItem");
                grd.Children.Add(sourceContainerContainer);

                TextBox sourceContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontSize = 12, IsReadOnly = true, Margin = new Thickness(5) };
                sourceContainers[i] = sourceContainer;
                sourceContainerContainer.Children.Add(sourceContainer);


                modulePreviewSelector.PropertyChanged += (s, e) =>
                {
                    if (e.Property == RibbonBar.SelectedIndexProperty)
                    {
                        int newIndex = (int)e.NewValue;
                        if (newIndex == 0)
                        {
                            sourceContainerContainer.ZIndex = 0;
                            sourceContainerContainer.RenderTransform = offScreen;
                            sourceContainerContainer.Opacity = 0;
                            sourceContainerContainer.IsHitTestVisible = false;

                            manualContainerContainer.ZIndex = 1;
                            manualContainerContainer.RenderTransform = TransformOperations.Identity;
                            manualContainerContainer.Opacity = 1;
                            manualContainerContainer.IsHitTestVisible = true;
                        }
                        else
                        {
                            manualContainerContainer.ZIndex = 0;
                            manualContainerContainer.RenderTransform = offScreen;
                            manualContainerContainer.Opacity = 0;
                            manualContainerContainer.IsHitTestVisible = false;

                            sourceContainerContainer.ZIndex = 1;
                            sourceContainerContainer.RenderTransform = TransformOperations.Identity;
                            sourceContainerContainer.Opacity = 1;
                            sourceContainerContainer.IsHitTestVisible = true;
                        }
                    }
                };

                content.Children.Add(grd);

                Grid noModuleGrid = new Grid() { Background = new Avalonia.Media.SolidColorBrush(0xFFE7E7E7) };
                Grid.SetColumnSpan(noModuleGrid, 3);
                content.Children.Add(noModuleGrid);
                noModuleGrid.Children.Add(new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Text = "No module available!" });
                noModuleGrids[i] = noModuleGrid;

                content.Opacity = 0;
                tabItems.Add(content);
                content.IsHitTestVisible = false;
                content.Classes.Add("TabItem");

                this.FindControl<Grid>("MainGrid").Children.Add(content);
            }

            foreach (KeyValuePair<string, ModuleMetadata> module in Modules.LoadedModulesMetadata)
            {
                BuildModuleButton(module);
            }

            bar.PropertyChanged += (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    int newIndex = (int)e.NewValue;
                    for (int i = 0; i < tabItems.Count; i++)
                    {
                        if (tabItems[i] != null)
                        {
                            if (i != newIndex)
                            {
                                tabItems[i].ZIndex = 0;
                                tabItems[i].RenderTransform = offScreen;
                                tabItems[i].Opacity = 0;
                                tabItems[i].IsHitTestVisible = false;
                            }
                            else
                            {
                                tabItems[i].ZIndex = 1;
                                tabItems[i].RenderTransform = TransformOperations.Identity;
                                tabItems[i].Opacity = 1;
                                tabItems[i].IsHitTestVisible = true;
                            }
                        }
                    }
                }
            };

            for (int i = 0; i < tabItems.Count; i++)
            {
                if (tabItems[i] != null)
                {
                    if (i != 0)
                    {
                        tabItems[i].ZIndex = 0;
                        tabItems[i].RenderTransform = offScreen;
                        tabItems[i].Opacity = 0;
                        tabItems[i].IsHitTestVisible = false;
                    }
                    else
                    {
                        tabItems[i].ZIndex = 1;
                        tabItems[i].RenderTransform = TransformOperations.Identity;
                        tabItems[i].Opacity = 1;
                        tabItems[i].IsHitTestVisible = true;
                    }
                }
            }
        }

        internal async Task OpenSourceCodeWindow()
        {
            int tabInd = bar.SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                return;
            }

            ModuleMetadata selectedMetadata = Modules.LoadedModulesMetadata[selectedModules[tabInd]];

            CodeViewerWindow window = new CodeViewerWindow();
            await window.Initialize(selectedMetadata.SourceCode);
            await window.ShowDialog2(this);
        }

        Dictionary<string, Button> ModuleButtons = new Dictionary<string, Button>();

        internal void BuildModuleButton(KeyValuePair<string, ModuleMetadata> module)
        {
            Button button = new Button() { Margin = new Thickness(0, 0, 0, 5), Background = Brushes.Transparent, Padding = new Thickness(0) };
            button.Classes.Add("PlainButton");

            Style blackForeground = new Style(x => x.OfType<Button>().Class(":pointerover").Descendant().OfType<TextBlock>());
            blackForeground.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0, 114, 178))));

            button.Styles.Add(blackForeground);

            Border brd = new Border() { BorderBrush = Brushes.Transparent, BorderThickness = new Thickness(2), Padding = new Thickness(5) };

            StackPanel pnl = new StackPanel() { Margin = new Thickness(0, 0, 0, 0) };
            pnl.Children.Add(new TextBlock() { FontSize = 14, FontWeight = FontWeight.Bold, Text = module.Value.Name, Margin = new Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap });
            pnl.Children.Add(new TextBlock() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontSize = 10, Foreground = new Avalonia.Media.SolidColorBrush(0xFFA0A0A0), Text = module.Value.Id });
            pnl.Children.Add(new TextBlock() { Text = module.Value.Author, FontSize = 12, Foreground = new Avalonia.Media.SolidColorBrush(0xFF606060) });
            brd.Child = pnl;

            button.Content = brd;


            button.Click += (s, e) => { SelectModule(module.Key); };

            moduleContainers[(int)module.Value.ModuleType].Children.Add(button);

            ModuleButtons[module.Key] = button;

            if (string.IsNullOrEmpty(selectedModules[(int)module.Value.ModuleType]))
            {
                SelectModule(module.Key);
            }
        }

        private void SelectModule(string moduleId)
        {
            ModuleMetadata module = Modules.LoadedModulesMetadata[moduleId];

            int tabInd = (int)module.ModuleType;
            selectedModules[tabInd] = moduleId;
            nameContainers[tabInd].Text = module.Name;
            idContainers[tabInd].Text = module.Id;
            authorContainers[tabInd].Text = module.Author;
            versionContainers[tabInd].Text = "Version " + module.Version.ToString();
            bool isVerified = module.VerifySignature();
            verifiedIcons[tabInd].Children.Clear();

            if (isVerified)
            {
                verifiedIcons[tabInd].Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Icons.TickGeometry, Stroke = new SolidColorBrush(Color.FromRgb(0, 158, 115)), Margin = new Thickness(3), StrokeThickness = 2 });
                AvaloniaBugFixes.SetToolTip(verifiedIcons[tabInd], "Code signature verified!");
            }
            else
            {
                verifiedIcons[tabInd].Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Icons.CrossGeometry, Stroke = new SolidColorBrush(Color.FromRgb(213, 94, 0)), Margin = new Thickness(3), StrokeThickness = 2 });
                AvaloniaBugFixes.SetToolTip(verifiedIcons[tabInd], "Code signature not verified!");
            }
            helpContainers[tabInd].Text = module.HelpText;
            sourceContainers[tabInd].Text = module.SourceCode;

            int imageIndex = 0;

            Dictionary<string, string> imageLookup = new Dictionary<string, string>();

            manualContainers[tabInd].Renderer.ImageUriResolver = (imageUri, baseUri) =>
            {
                string[] compatibleFiles = System.IO.Directory.GetFiles(System.IO.Path.Combine(Modules.ModulePath, "assets"), moduleId + "_" + "image" + imageIndex + ".*");

                if (compatibleFiles.Length > 0)
                {
                    string imagePath = compatibleFiles[0];

                    imageLookup[baseUri + "|||" + imageUri] = imagePath;
                    imageIndex++;
                    return (imagePath, false);
                }
                else
                {
                    return (null, false);
                }
            };

            string mardownSource = module.BuildReadmeMarkdown();

            manualContainers[tabInd].Renderer.RenderSinglePage(mardownSource, 800, out _);

            manualContainers[tabInd].Renderer.ImageUriResolver = (imageUri, baseUri) =>
            {
                return (imageLookup.TryGetValue(baseUri + "|||" + imageUri, out string imagePath) ? imagePath : null, false);
            };

            manualContainers[tabInd].DocumentSource = mardownSource;
            noModuleGrids[tabInd].IsVisible = false;

            foreach (KeyValuePair<string, Button> kvp in ModuleButtons)
            {
                if (Modules.LoadedModulesMetadata[kvp.Key].ModuleType == module.ModuleType)
                {
                    if (kvp.Key == module.Id)
                    {
                        kvp.Value.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243));
                        ((Border)kvp.Value.Content).BorderBrush = new SolidColorBrush(Color.FromRgb(114, 114, 114));
                    }
                    else
                    {
                        kvp.Value.Background = Brushes.Transparent;
                        ((Border)kvp.Value.Content).BorderBrush = Brushes.Transparent;
                    }
                }
            }
        }

        private async void ExportClicked(object sender, EventArgs e)
        {
            int tabInd = bar.SelectedIndex;
            ModuleMetadata selectedMetadata = Modules.LoadedModulesMetadata[selectedModules[tabInd]];


            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Module file", Extensions = new List<string>() { "json.zip" } } }, Title = "Save module..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                bool includeReferences = false;

                if (selectedMetadata.AdditionalReferences.Length > 0)
                {
                    MessageBox box = new MessageBox("Question", "Would you like to include the referenced assemblies in the module file?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box.ShowDialog2(this);
                    includeReferences = box.Result == MessageBox.Results.Yes;
                }

                {
                    MessageBox box = new MessageBox("Question", "Would you like to sign the module source? If so, you will need an RSA private key file.", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box.ShowDialog2(this);
                    if (box.Result == MessageBox.Results.Yes)
                    {
                        OpenFileDialog dialog2;

                        List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Key file", Extensions = new List<string>() { "json" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

                        if (!Modules.IsMac)
                        {
                            dialog2 = new OpenFileDialog()
                            {
                                Title = "Load private key...",
                                AllowMultiple = false,
                                Filters = filters
                            };
                        }
                        else
                        {
                            dialog2 = new OpenFileDialog()
                            {
                                Title = "Load private key...",
                                AllowMultiple = false
                            };
                        }

                        string[] result2 = await dialog2.ShowAsync(this);

                        if (result2 != null && result2.Length == 1)
                        {
                            try
                            {
                                selectedMetadata.Sign(result2[0]);
                            }
                            catch (Exception ex)
                            {
                                MessageBox errBox = new MessageBox("Attention!", "An error occurred while signing the module!\n" + ex.Message + "\nDo you wish to continue anyways?", MessageBox.MessageBoxButtonTypes.YesNo);
                                await errBox.ShowDialog2(this);
                                if (errBox.Result != MessageBox.Results.Yes)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempDir);

                try
                {
                    if (includeReferences)
                    {
                        for (int i = 0; i < selectedMetadata.AdditionalReferences.Length; i++)
                        {
                            string actualPath = ModuleMetadata.LocateReference(selectedMetadata.AdditionalReferences[i]);

                            if (File.Exists(actualPath))
                            {
                                if (Path.GetDirectoryName(actualPath) != Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                                {
                                    File.Copy(actualPath, Path.Combine(tempDir, Path.GetFileName(actualPath)));
                                }
                            }
                            else
                            {
                                MessageBox box = new MessageBox("Attention", "Cannot find the referenced assembly file " + selectedMetadata.AdditionalReferences[i] + ".\nDo you wish to proceed anyways?", MessageBox.MessageBoxButtonTypes.YesNo);
                                await box.ShowDialog2(this);
                                if (box.Result != MessageBox.Results.Yes)
                                {
                                    return;
                                }
                            }
                        }
                    }

                    using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Create))
                    {
                        System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), selectedMetadata, typeof(ModuleMetadata), Modules.DefaultSerializationOptions);
                    }

                    if (File.Exists(result))
                    {
                        File.Delete(result);
                    }

                    ZipFile.CreateFromDirectory(tempDir, result, CompressionLevel.Optimal, false);

                    await new MessageBox("Success!", "Module exported succesfully!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while exporting the module!\n" + ex.Message).ShowDialog2(this);
                }
                finally
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }


        private async void LoadClicked(object sender, EventArgs e)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Module file", Extensions = new List<string>() { "json.zip" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Load module...",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Load module...",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                try
                {
                    ZipFile.ExtractToDirectory(result[0], tempDir);

                    ModuleMetadata metaData;

                    using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                    {
                        metaData = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                    }

                    if (Modules.LoadedModulesMetadata.ContainsKey(metaData.Id))
                    {
                        await new MessageBox("Attention", "A module with the same Id has already been loaded!").ShowDialog2(this);
                        return;
                    }

                    if (!metaData.VerifySignature())
                    {
                        MessageBox box = new MessageBox("Attention", "The source code of the module could not be verified. Proceed only if you trust the source from which you obtained the module. Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo);
                        await box.ShowDialog2(this);
                        if (box.Result != MessageBox.Results.Yes)
                        {
                            return;
                        }
                    }

                    Modules.LoadModule(metaData);

                    BuildModuleButton(new KeyValuePair<string, ModuleMetadata>(metaData.Id, metaData));

                    await new MessageBox("Success!", "Module loaded succesfully!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while loading the module!\n" + ex.Message).ShowDialog2(this);
                }
                finally
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }

        private async void LoadFromSourceClicked(object sender, EventArgs e)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "C# source files", Extensions = new List<string>() { "cs" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Load module...",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Load module...",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                ChooseReferencesWindow win = new ChooseReferencesWindow();
                await win.ShowDialog2(this);

                if (win.Result)
                {
                    try
                    {
                        string source = File.ReadAllText(result[0]);
                        ModuleMetadata meta = ModuleMetadata.CreateFromSource(source, win.References.ToArray());

                        if (Modules.LoadedModulesMetadata.ContainsKey(meta.Id))
                        {
                            await new MessageBox("Attention", "A module with the same Id has already been loaded!").ShowDialog2(this);
                            return;
                        }

                        Modules.LoadModule(meta);
                        BuildModuleButton(new KeyValuePair<string, ModuleMetadata>(meta.Id, meta));

                        await new MessageBox("Success!", "Module loaded succesfully!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);
                    }
                    catch (Exception ex)
                    {
                        await new MessageBox("Attention!", "An error occurred while loading the module!\nPlease, make sure that you have added all needed references!\n" + ex.Message).ShowDialog2(this);
                    }
                }
            }
        }

        private async void InstallClicked(object sender, EventArgs e)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Module file", Extensions = new List<string>() { "json.zip" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Install module...",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Install module...",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                try
                {
                    ZipFile.ExtractToDirectory(result[0], tempDir);

                    ModuleMetadata metaData;

                    using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                    {
                        metaData = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                    }

                    if (Modules.LoadedModulesMetadata.ContainsKey(metaData.Id))
                    {
                        await new MessageBox("Attention", "A module with the same Id has already been loaded!").ShowDialog2(this);
                        return;
                    }

                    if (!metaData.VerifySignature())
                    {
                        MessageBox box = new MessageBox("Attention", "The source code of the module could not be verified. Proceed only if you trust the source from which you obtained the module. Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo);
                        await box.ShowDialog2(this);
                        if (box.Result != MessageBox.Results.Yes)
                        {
                            return;
                        }
                    }

                    metaData.IsInstalled = true;

                    Modules.LoadModule(metaData);

                    BuildModuleButton(new KeyValuePair<string, ModuleMetadata>(metaData.Id, metaData));

                    bool copyReferences = false;

                    if (metaData.AdditionalReferences.Length > 0)
                    {
                        MessageBox box = new MessageBox("Question", "Do you want to install the external reference files as well?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog2(this);
                        copyReferences = box.Result == MessageBox.Results.Yes;
                    }

                    string newModuleDLL = Path.Combine(tempDir, Guid.NewGuid().ToString());

                    using (FileStream fs = new FileStream(newModuleDLL, FileMode.Create))
                    {
                        metaData.Build(fs);
                    }

                    string newModuleList = Path.Combine(tempDir, Guid.NewGuid().ToString());

                    using (FileStream fs = new FileStream(newModuleList, FileMode.Create))
                    {
                        System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), from el in Modules.LoadedModulesMetadata where el.Value.IsInstalled == true select el.Value, typeof(IEnumerable<ModuleMetadata>), Modules.DefaultSerializationOptions);
                    }

                    List<string> filesToCopy = new List<string>();
                    filesToCopy.Add(Path.GetFullPath(newModuleList));
                    filesToCopy.Add(Path.GetFullPath(Modules.ModuleListPath));
                    filesToCopy.Add(Path.GetFullPath(newModuleDLL));
                    filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, metaData.Id + ".dll")));

                    if (copyReferences)
                    {
                        for (int i = 0; i < metaData.AdditionalReferences.Length; i++)
                        {
                            string actualPath = Path.Combine(tempDir, Path.GetFileName(metaData.AdditionalReferences[i]));

                            if (File.Exists(actualPath))
                            {
                                filesToCopy.Add(Path.GetFullPath(actualPath));
                                filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, "libraries", Path.GetFileName(actualPath))));
                            }
                        }
                    }

                    if (Directory.Exists(Path.Combine(tempDir, "assets")))
                    {
                        foreach (string file in Directory.GetFiles(Path.Combine(tempDir, "assets"), "*.*"))
                        {
                            filesToCopy.Add(Path.GetFullPath(file));
                            filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, "assets", metaData.Id + "_" + Path.GetFileName(file))));
                        }
                    }

                    for (int i = 0; i < filesToCopy.Count; i += 2)
                    {
                        File.Copy(filesToCopy[i], filesToCopy[i + 1], true);
                    }


                    await new MessageBox("Success!", "Module installed succesfully!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while installing the module!\n" + ex.Message).ShowDialog2(this);
                }
                finally
                {
                    try
                    {
                        Directory.Delete(tempDir, true);
                    }
                    catch { }
                }
            }
        }

        private async void UninstallClicked(object sender, EventArgs e)
        {
            int tabInd = bar.SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                await new MessageBox("Attention!", "No module module has been selected!").ShowDialog2(this);
                return;
            }

            ModuleMetadata selectedMetadata = Modules.LoadedModulesMetadata[selectedModules[tabInd]];

            {
                MessageBox box = new MessageBox("Question", "Are you sure you wish to uninstall module \"" + selectedMetadata.Name + "\"?", MessageBox.MessageBoxButtonTypes.YesNo);
                await box.ShowDialog2(this);
                if (box.Result != MessageBox.Results.Yes)
                {
                    return;
                }
            }

            Modules.LoadedModulesMetadata.Remove(selectedMetadata.Id);
            ((IPanel)ModuleButtons[selectedMetadata.Id].Parent).Children.Remove(ModuleButtons[selectedMetadata.Id]);
            ModuleButtons.Remove(selectedMetadata.Id);
            noModuleGrids[tabInd].IsVisible = true;

            foreach (KeyValuePair<string, ModuleMetadata> module in Modules.LoadedModulesMetadata)
            {
                if ((int)module.Value.ModuleType == tabInd)
                {
                    SelectModule(module.Key);
                    break;
                }
            }

            string newModuleList = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            string fileList = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());

            try
            {
                using (FileStream fs = new FileStream(newModuleList, FileMode.Create))
                {
                    System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), from el in Modules.LoadedModulesMetadata where el.Value.IsInstalled == true select el.Value, typeof(IEnumerable<ModuleMetadata>), Modules.DefaultSerializationOptions);
                }

                List<string> filesToCopy = new List<string>();
                filesToCopy.Add(newModuleList);
                filesToCopy.Add(Path.GetFullPath(Modules.ModuleListPath));
                
                for (int i = 0; i < filesToCopy.Count; i += 2)
                {
                    if (filesToCopy[i + 1] != "-")
                    {
                        File.Copy(filesToCopy[i], filesToCopy[i + 1], true);
                    }
                    else
                    {
                        if (File.Exists(filesToCopy[i]))
                        {
                            File.Delete(filesToCopy[i]);
                        }
                    }
                }


                await new MessageBox("Success!", "Module uninstalled succesfully!\nPlease restart the program for changes to have full effect.", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);

            }
            catch (Exception ex)
            {
                await new MessageBox("Attention!", "An error occurred while uninstalling the module!\n" + ex.Message).ShowDialog2(this);
            }
            finally
            {
                try
                {
                    File.Delete(newModuleList);
                }
                catch { }

                try
                {
                    if (File.Exists(fileList))
                    {
                        File.Delete(fileList);
                    }
                }
                catch { }
            }
        }

        private async Task InstallFromRepositoryClicked()
        {
            try
            {
                ModuleRepositoryWindow win = new ModuleRepositoryWindow(ModuleRepositoryWindow.Modes.Install, this);
                await win.ShowDialog2(this);
            }
            catch (Exception ex)
            {
                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(this);
            }
        }

        private async Task LoadFromRepositoryClicked()
        {
            try
            {
                ModuleRepositoryWindow win = new ModuleRepositoryWindow(ModuleRepositoryWindow.Modes.Load, this);
                await win.ShowDialog2(this);
            }
            catch (Exception ex)
            {
                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(this);
            }
        }
    }
}
