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
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using VectSharp.MarkdownCanvas;

namespace TreeViewer
{
    public class ModuleManagerWindow : Window
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
        private AddRemoveButton[] verifiedIcons;
        private TextBlock[] helpContainers;
        private TextBox[] sourceContainers;
        //private MarkdownCanvasControl[] sourceContainers;
        private MarkdownCanvasControl[] manualContainers;
        private string[] selectedModules;
        private Grid[] noModuleGrids;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            /*  <NativeMenu.Menu>
    <NativeMenu>
      <NativeMenuItem Header="Module">
        <NativeMenuItem.Menu>
          <NativeMenu>
            <NativeMenuItem Header="Load from source..." Clicked="LoadFromSourceClicked"></NativeMenuItem>
            <NativeMenuItem Header="Load..." Clicked="LoadClicked"></NativeMenuItem>
            <NativeMenuItem Header="Load from repository..."></NativeMenuItem>
            <NativeMenuItemSeperator></NativeMenuItemSeperator>
            <NativeMenuItem Header="Install..." Clicked="InstallClicked"></NativeMenuItem>
            <NativeMenuItem Header="Install from repository..."></NativeMenuItem>
            <NativeMenuItemSeperator></NativeMenuItemSeperator>
            <NativeMenuItem Header="Export..." Clicked="ExportClicked"></NativeMenuItem>
            <NativeMenuItemSeperator></NativeMenuItemSeperator>
            <NativeMenuItem Header="Uninstall..." Clicked="UninstallClicked"></NativeMenuItem>
          </NativeMenu>
        </NativeMenuItem.Menu>
      </NativeMenuItem>
    </NativeMenu>
  </NativeMenu.Menu>*/

            NativeMenu menu = new NativeMenu();
            NativeMenuItem moduleMenu = new NativeMenuItem() { Header = "Module" };

            NativeMenu moduleSubMenu = new NativeMenu();
            NativeMenuItem loadSourceItem = new NativeMenuItem() { Header = "Load from source...", Command = new SimpleCommand(win => true, a => LoadFromSourceClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(loadSourceItem);
            NativeMenuItem loadItem = new NativeMenuItem() { Header = "Load...", Command = new SimpleCommand(win => true, a => LoadClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(loadItem);
            NativeMenuItem loadRepositoryItem = new NativeMenuItem() { Header = "Load from repository...", Command = new SimpleCommand(win => true, async a => { await LoadFromRepositoryClicked(); }, null, null) };
            moduleSubMenu.Add(loadRepositoryItem);
            moduleSubMenu.Add(new NativeMenuItemSeperator());
            NativeMenuItem installItem = new NativeMenuItem() { Header = "Install...", Command = new SimpleCommand(win => true, a => InstallClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(installItem);
            NativeMenuItem installRepositoryItem = new NativeMenuItem() { Header = "Install from repository...", Command = new SimpleCommand(win => true, async a => { await InstallFromRepositoryClicked(); }, null, null) };
            moduleSubMenu.Add(installRepositoryItem);
            moduleSubMenu.Add(new NativeMenuItemSeperator());
            NativeMenuItem exportItem = new NativeMenuItem() { Header = "Export...", Command = new SimpleCommand(win => true, a => ExportClicked(null, new EventArgs()), null, null) };
            moduleSubMenu.Add(exportItem);
            moduleSubMenu.Add(new NativeMenuItemSeperator());
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
            verifiedIcons = new AddRemoveButton[headers.Length];
            helpContainers = new TextBlock[headers.Length];
            sourceContainers = new TextBox[headers.Length];
            //sourceContainers = new MarkdownCanvasControl[headers.Length];
            manualContainers = new MarkdownCanvasControl[headers.Length];
            selectedModules = new string[headers.Length];
            noModuleGrids = new Grid[headers.Length];

            List<TabItem> tabItems = new List<TabItem>();

            for (int i = 0; i < headers.Length; i++)
            {
                TabItem item = new TabItem() { Header = headers[i], FontSize = 20 };
                tabItems.Add(item);

                Grid content = new Grid();
                content.ColumnDefinitions.Add(new ColumnDefinition(250, GridUnitType.Pixel));
                content.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                ScrollViewer scr = new ScrollViewer() { Margin = new Thickness(0, 5, 10, 5), VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled };

                StackPanel moduleContainer = new StackPanel();
                moduleContainers[i] = moduleContainer;
                scr.Content = moduleContainer;

                content.Children.Add(scr);

                Grid grd = new Grid() { Margin = new Thickness(10, 0, 5, 5) };
                Grid.SetColumn(grd, 1);
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

                TextBlock nameContainer = new TextBlock() { FontSize = 24, FontWeight = Avalonia.Media.FontWeight.Bold };
                nameContainers[i] = nameContainer;
                grd.Children.Add(nameContainer);

                TextBox idContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontStyle = Avalonia.Media.FontStyle.Italic, Foreground = new Avalonia.Media.SolidColorBrush(0xFFA0A0A0), Background = null, BorderBrush = null, BorderThickness = new Thickness(0, 0, 0, 0), IsReadOnly = true, Padding = new Thickness(0), MinHeight = 0 };
                idContainers[i] = idContainer;
                Grid.SetRow(idContainer, 1);
                grd.Children.Add(idContainer);

                StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 10, 0, 0) };
                TextBlock versionContainer = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF808080) };
                versionContainers[i] = versionContainer;
                pnl.Children.Add(versionContainer);

                pnl.Children.Add(new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontStyle = Avalonia.Media.FontStyle.Italic, Margin = new Thickness(0, 0, 5, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF808080), Text = "by" });

                TextBlock authorContainer = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0), Foreground = new Avalonia.Media.SolidColorBrush(0xFF808080) };
                authorContainers[i] = authorContainer;
                pnl.Children.Add(authorContainer);

                AddRemoveButton verifiedIcon = new AddRemoveButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, ButtonType = AddRemoveButton.ButtonTypes.Cancel };
                ToolTip.SetTip(verifiedIcon, "Code signature not verified!");
                verifiedIcons[i] = verifiedIcon;
                pnl.Children.Add(verifiedIcon);
                Grid.SetRow(pnl, 2);
                grd.Children.Add(pnl);

                TextBlock helpContainer = new TextBlock() { Margin = new Thickness(0, 5, 0, 5), TextWrapping = Avalonia.Media.TextWrapping.Wrap };
                helpContainers[i] = helpContainer;
                Grid.SetRow(helpContainer, 3);
                grd.Children.Add(helpContainer);

                TabControl modulePreviewSelector = new TabControl();
                Grid.SetRow(modulePreviewSelector, 4);
                grd.Children.Add(modulePreviewSelector);

                TabItem moduleManualItem = new TabItem() { Header = "Manual", FontSize = 18, Height = 30, MinHeight = 30 };

                MarkdownCanvasControl manualContainer = new MarkdownCanvasControl() { Background = Avalonia.Media.Brushes.White, Margin = new Thickness(-12, -2, -12, -2), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Top, MaxRenderWidth = 1000 };
                manualContainer.Renderer.ImageUnitMultiplier /= 1.8;
                manualContainer.Renderer.ImageMultiplier *= 1.8;
                manualContainer.FontSize = 18;
                manualContainer.Renderer.CodeFont = Modules.CodeFontFamily;
                manualContainer.Renderer.Margins = new VectSharp.Markdown.Margins(0, 10, 0, 0);
                manualContainer.Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);

                manualContainers[i] = manualContainer;
                moduleManualItem.Content = manualContainer;

                StackPanel sourceCodeHeader = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                sourceCodeHeader.Children.Add(new TextBlock() { Text = "Source code", FontSize = 18 });

                OpenWindowButton openWindowButton = new OpenWindowButton() { Width = 24, Height = 24, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                sourceCodeHeader.Children.Add(openWindowButton);
                openWindowButton.Click += async (s, e) =>
                {
                    await OpenSourceCodeWindow();
                };

                TabItem moduleSourceItem = new TabItem() { Header = sourceCodeHeader, Height = 30, MinHeight = 30 };

                /*MarkdownCanvasControl sourceContainer = new MarkdownCanvasControl() { Background = Avalonia.Media.Brushes.White, MinRenderWidth = 800, Margin = new Thickness(-12, -2, -12, -2), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Top };
                sourceContainer.Renderer.BaseFontSize = 16;
                sourceContainer.Renderer.CodeFont = Modules.CodeFontFamily;
                sourceContainer.Renderer.Margins = new VectSharp.Markdown.Margins(0, 10, 0, 0);
                sourceContainer.Renderer.CodeBlockBackgroundColour = VectSharp.Colour.FromRgb(255, 255, 255);*/
                //sourceContainers[i] = sourceContainer;
                //moduleSourceItem.Content = sourceContainer;

                TextBox sourceContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontSize = 12, IsReadOnly = true, Margin = new Thickness(0, 5, 0, 0) };
                sourceContainers[i] = sourceContainer;
                moduleSourceItem.Content = sourceContainer;

                modulePreviewSelector.Items = new List<TabItem>() { moduleManualItem, moduleSourceItem };

                /*Grid.SetRow(sourceContainer, 4);
                grd.Children.Add(sourceContainer);*/

                content.Children.Add(grd);

                Grid noModuleGrid = new Grid() { Background = new Avalonia.Media.SolidColorBrush(0xFFF5F5F5) };
                Grid.SetColumnSpan(noModuleGrid, 2);
                content.Children.Add(noModuleGrid);
                noModuleGrid.Children.Add(new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Text = "No module available!" });
                noModuleGrids[i] = noModuleGrid;

                item.Content = content;
            }

            foreach (KeyValuePair<string, ModuleMetadata> module in Modules.LoadedModulesMetadata)
            {
                BuildModuleButton(module);
            }

            this.FindControl<TabControl>("MainTabControl").Items = tabItems;
        }

        internal async Task OpenSourceCodeWindow()
        {
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                return;
            }

            ModuleMetadata selectedMetadata = Modules.LoadedModulesMetadata[selectedModules[tabInd]];

            CodeViewerWindow window = new CodeViewerWindow();
            await window.Initialize(selectedMetadata.SourceCode);
            await window.ShowDialog2(this);
        }

        Dictionary<string, Border> ModuleButtons = new Dictionary<string, Border>();

        internal void BuildModuleButton(KeyValuePair<string, ModuleMetadata> module)
        {
            Border brd = new Border() { Classes = new Classes("ModuleButton") };
            StackPanel pnl = new StackPanel();
            pnl.Children.Add(new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Text = module.Value.Name });
            pnl.Children.Add(new TextBlock() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontStyle = Avalonia.Media.FontStyle.Italic, FontSize = 12, Foreground = new Avalonia.Media.SolidColorBrush(0xFFA0A0A0), Text = module.Value.Id.Substring(Math.Max(0, module.Value.Id.Length - 22)) });
            pnl.Children.Add(new TextBlock() { Text = module.Value.Author });
            brd.Child = pnl;

            brd.PointerReleased += (s, e) => { SelectModule(module.Key); };

            moduleContainers[(int)module.Value.ModuleType].Children.Add(brd);

            if (string.IsNullOrEmpty(selectedModules[(int)module.Value.ModuleType]))
            {
                SelectModule(module.Key);
            }

            ModuleButtons[module.Key] = brd;
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
            verifiedIcons[tabInd].ButtonType = isVerified ? AddRemoveButton.ButtonTypes.OK : AddRemoveButton.ButtonTypes.Cancel;
            if (isVerified)
            {
                ToolTip.SetTip(verifiedIcons[tabInd], "Code signature verified!");
            }
            else
            {
                ToolTip.SetTip(verifiedIcons[tabInd], "Code signature not verified!");
            }
            helpContainers[tabInd].Text = module.HelpText;
            sourceContainers[tabInd].Text = module.SourceCode;
            //sourceContainers[tabInd].DocumentSource = "```CSharp\n" + module.SourceCode + "\n```\n";


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
        }

        private async void ExportClicked(object sender, EventArgs e)
        {
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
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
                                File.Copy(actualPath, Path.Combine(tempDir, Path.GetFileName(actualPath)));
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
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
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

            /*bool removeReferences = false;
            List<string> unneededReferences = new List<string>(selectedMetadata.AdditionalReferences);

            if (selectedMetadata.AdditionalReferences.Length > 0)
            {
                List<string> neededReferences = new List<string>();

                for (int i = 0; i < selectedMetadata.AdditionalReferences.Length; i++)
                {
                    foreach (KeyValuePair<string, ModuleMetadata> kvp in Modules.LoadedModulesMetadata)
                    {
                        if (kvp.Key != selectedMetadata.Id)
                        {
                            if ((from el in kvp.Value.AdditionalReferences where Path.GetFileName(el) == Path.GetFileName(selectedMetadata.AdditionalReferences[i]) select el).Any())
                            {
                                neededReferences.Add(selectedMetadata.AdditionalReferences[i]);
                                break;
                            }
                        }
                    }
                }

                for (int i = 0; i < neededReferences.Count; i++)
                {
                    unneededReferences.Remove(neededReferences[i]);
                }

                if (unneededReferences.Count > 0)
                {
                    MessageBox box = new MessageBox("Question", "Do you wish to remove unnecessary references?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box.ShowDialog2(this);
                    removeReferences = box.Result == MessageBox.Results.Yes;
                }
            }*/

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
                /*filesToCopy.Add(Path.Combine(Modules.ModulePath, selectedMetadata.Id + ".dll"));
                filesToCopy.Add("-");*/

                /*if (removeReferences)
                {
                    for (int i = 0; i < unneededReferences.Count; i++)
                    {
                        filesToCopy.Add(ModuleMetadata.LocateReference(unneededReferences[i]));
                        filesToCopy.Add("-");
                    }
                }*/

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
                await win.ShowDialog(this);
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
                await win.ShowDialog(this);
            }
            catch (Exception ex)
            {
                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(this);
            }
        }
    }
}
