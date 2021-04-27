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
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;

namespace TreeViewer
{
    public class ModuleRepositoryWindow : Window
    {
        public enum Modes
        {
            Load, Install
        }

        public Modes Mode { get; set; } = Modes.Load;

        public ModuleRepositoryWindow(Modes mode, ModuleManagerWindow parent)
        {
            this.Mode = mode;
            this.managerWindowParent = parent;
            this.InitializeComponent();
        }

        private enum NotificationType
        {
            None = 0,
            Updates = 1,
            NewModules = 2,
            Both = 3
        }

        public ModuleRepositoryWindow()
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
        private string[] selectedModules;
        private Grid[] noModuleGrids;
        private Avalonia.Controls.Shapes.Ellipse[] notificationEllipses;
        private Dictionary<string, ModuleHeader> repositoryModuleHeaders = new Dictionary<string, ModuleHeader>();
        private NotificationType[] notifications;
        private ModuleManagerWindow managerWindowParent;

        private Uri ModuleRepositoryBaseUri = new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri);

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.Title = this.Mode.ToString() + " from repository...";

            moduleContainers = new StackPanel[headers.Length];
            nameContainers = new TextBlock[headers.Length];
            idContainers = new TextBox[headers.Length];
            authorContainers = new TextBlock[headers.Length];
            versionContainers = new TextBlock[headers.Length];
            verifiedIcons = new AddRemoveButton[headers.Length];
            helpContainers = new TextBlock[headers.Length];
            sourceContainers = new TextBox[headers.Length];
            selectedModules = new string[headers.Length];
            notificationEllipses = new Avalonia.Controls.Shapes.Ellipse[headers.Length];
            noModuleGrids = new Grid[headers.Length];
            notifications = new NotificationType[headers.Length];

            List<TabItem> tabItems = new List<TabItem>();

            for (int i = 0; i < headers.Length; i++)
            {
                StackPanel tabHeader = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

                tabHeader.Children.Add(new TextBlock() { Text = headers[i], FontSize = 20 });

                Avalonia.Controls.Shapes.Ellipse notificationEllipse = new Avalonia.Controls.Shapes.Ellipse() { Width = 15, Height = 15, Margin = new Thickness(5, 0, 0, 0) };
                notificationEllipse.Classes.Add("Notification");
                notificationEllipses[i] = notificationEllipse;

                tabHeader.Children.Add(notificationEllipse);

                TabItem item = new TabItem() { Header = tabHeader };
                tabItems.Add(item);

                Grid content = new Grid();
                content.ColumnDefinitions.Add(new ColumnDefinition(250, GridUnitType.Pixel));
                content.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                ScrollViewer scr = new ScrollViewer() { Margin = new Thickness(0, 5, 10, 5), VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, Padding = new Thickness(0, 0, 10, 0) };

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
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

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

                /* TextBox sourceContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontSize = 12, IsReadOnly = true, Margin = new Thickness(0, 5, 0, 0) };
                 sourceContainers[i] = sourceContainer;
                 Grid.SetRow(sourceContainer, 4);
                 grd.Children.Add(sourceContainer);*/

                TabControl modulePreviewSelector = new TabControl();
                Grid.SetRow(modulePreviewSelector, 4);
                grd.Children.Add(modulePreviewSelector);

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

                TextBox sourceContainer = new TextBox() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontSize = 12, IsReadOnly = true, Margin = new Thickness(0, 5, 0, 0) };
                sourceContainers[i] = sourceContainer;
                moduleSourceItem.Content = sourceContainer;

                modulePreviewSelector.Items = new List<TabItem>() { moduleSourceItem };

                /*Grid.SetRow(sourceContainer, 4);
                grd.Children.Add(sourceContainer);*/

                Button moduleLoadInstallButton = new Button() { Content = this.Mode.ToString(), Width = 150, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 5) };
                Grid.SetRow(moduleLoadInstallButton, 5);
                grd.Children.Add(moduleLoadInstallButton);

                moduleLoadInstallButton.Click += (s, e) =>
                {
                    if (this.Mode == Modes.Load)
                    {
                        LoadClicked();
                    }
                    else if (this.Mode == Modes.Install)
                    {
                        InstallClicked();
                    }
                };

                content.Children.Add(grd);

                Grid noModuleGrid = new Grid() { Background = new Avalonia.Media.SolidColorBrush(0xFFF5F5F5) };
                Grid.SetColumnSpan(noModuleGrid, 2);
                content.Children.Add(noModuleGrid);
                noModuleGrid.Children.Add(new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Text = "No module available!" });
                noModuleGrids[i] = noModuleGrid;

                item.Content = content;
            }

            this.FindControl<TabControl>("MainTabControl").Items = tabItems;

            this.Opened += async (s, e) =>
            {
                await GetModuleRepositoryDatabase();
            };
        }

        private async Task GetModuleRepositoryDatabase()
        {
            Uri moduleHeaderInfo = new Uri(ModuleRepositoryBaseUri, "modules.json.gz");

            List<ModuleHeader> moduleHeaders;

            ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Accessing module database..." };
            _ = progressWindow.ShowDialog(this);

            try
            {
                using (WebClient client = new WebClient())
                {
                    string tempFile = Path.GetTempFileName();
                    await client.DownloadFileTaskAsync(moduleHeaderInfo, tempFile);

                    using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                    {
                        using (GZipStream decompressionStream = new GZipStream(fs, CompressionMode.Decompress))
                        {
                            moduleHeaders = await System.Text.Json.JsonSerializer.DeserializeAsync<List<ModuleHeader>>(decompressionStream, Modules.DefaultSerializationOptions);
                        }
                    }

                    File.Delete(tempFile);
                }
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(this);
                this.Close();
                return;
            }

            progressWindow.IsIndeterminate = false;
            progressWindow.Progress = 0;

            for (int i = 0; i < moduleHeaders.Count; i++)
            {
                ModuleHeader header = moduleHeaders[i];

                bool newModule = !Modules.LoadedModulesMetadata.TryGetValue(header.Id, out ModuleMetadata loadedModuleMetadata);

                if (newModule || loadedModuleMetadata.Version < header.Version)
                {
                    if (newModule)
                    {
                        notifications[(int)header.ModuleType] |= NotificationType.NewModules;
                    }
                    else
                    {
                        notifications[(int)header.ModuleType] |= NotificationType.Updates;
                    }

                    repositoryModuleHeaders.Add(header.Id, header);
                    await BuildModuleButton(header);
                }

                progressWindow.Progress = (double)(from el in Enumerable.Range(0, headers.Length) where !string.IsNullOrEmpty(selectedModules[el]) select 1).Count() / headers.Length;
            }

            for (int i = 0; i < headers.Length; i++)
            {
                switch (notifications[i])
                {
                    case NotificationType.None:
                        notificationEllipses[i].Classes.Remove("Updates");
                        notificationEllipses[i].Classes.Remove("NewModules");
                        break;
                    case NotificationType.Updates:
                        notificationEllipses[i].Classes.Add("Updates");
                        notificationEllipses[i].Classes.Remove("NewModules");
                        break;
                    case NotificationType.NewModules:
                        notificationEllipses[i].Classes.Remove("Updates");
                        notificationEllipses[i].Classes.Add("NewModules");
                        break;
                    case NotificationType.Both:
                        notificationEllipses[i].Classes.Add("Updates");
                        notificationEllipses[i].Classes.Add("NewModules");
                        break;
                }
            }

            progressWindow.Close();
        }

        //Dictionary<string, Border> ModuleButtons = new Dictionary<string, Border>();
        Dictionary<string, CoolButton> ModuleButtons = new Dictionary<string, CoolButton>();

        private async Task BuildModuleButton(ModuleHeader header)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                CoolButton button = new CoolButton() { CornerRadius = new CornerRadius(10), Margin = new Thickness(-5, 0, 0, 0), Hue = 0.98 };

                button.Title = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Text = header.Name, Foreground = Brushes.White, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Thickness(10, 5), TextWrapping = TextWrapping.Wrap, TextAlignment = TextAlignment.Center };

                StackPanel pnl = new StackPanel() { Margin = new Thickness(10, 5) };
                //pnl.Children.Add(new TextBlock() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontStyle = Avalonia.Media.FontStyle.Italic, FontSize = 12, Foreground = new Avalonia.Media.SolidColorBrush(0xFFA0A0A0), Text = header.Id.Substring(header.Id.Length - 22) });

                StackPanel idPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                idPanel.Children.Add(new TextBlock() { FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontStyle = Avalonia.Media.FontStyle.Italic, FontSize = 12, Foreground = new Avalonia.Media.SolidColorBrush(0xFFA0A0A0), Text = header.Id.Substring(header.Id.Length - 22) });
                Avalonia.Controls.Shapes.Ellipse notification = new Avalonia.Controls.Shapes.Ellipse() { Width = 10, Height = 10, Margin = new Thickness(5, 0, 0, 0) };
                notification.Classes.Add("Notification");
                if (!Modules.LoadedModulesMetadata.ContainsKey(header.Id))
                {
                    notification.Classes.Add("NewModules");
                }
                else
                {
                    notification.Classes.Add("Updates");
                }

                idPanel.Children.Add(notification);
                pnl.Children.Add(idPanel);

                pnl.Children.Add(new TextBlock() { Text = header.Author });
                button.ButtonContent = pnl;

                button.Click += async (s, e) =>
                {
                    ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Downloading module file..." };
                    _ = progressWindow.ShowDialog(this);

                    await SelectModule(header.Id, header.Version.ToString());

                    progressWindow.Close();
                };

                moduleContainers[(int)header.ModuleType].Children.Add(button);

                ModuleButtons[header.Id] = button;
            });

            if (string.IsNullOrEmpty(selectedModules[(int)header.ModuleType]))
            {
                await SelectModule(header.Id, header.Version.ToString());
            }
        }

        private async Task SelectModule(string moduleId, string moduleVersion)
        {
            Uri moduleFile = new Uri(ModuleRepositoryBaseUri, moduleId + "/" + moduleId + ".v" + moduleVersion + ".json.zip");

            ModuleMetadata module;

            using (WebClient client = new WebClient())
            {
                string tempFile = Path.GetTempFileName();
                await client.DownloadFileTaskAsync(moduleFile, tempFile);

                string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                ZipFile.ExtractToDirectory(tempFile, tempDir);

                File.Delete(tempFile);

                using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                {
                    module = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                }

                File.Delete(tempFile);
                Directory.Delete(tempDir, true);
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
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

                noModuleGrids[tabInd].IsVisible = false;
            });
        }

        private async void LoadClicked()
        {
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                await new MessageBox("Attention!", "No module has been selected!").ShowDialog2(this);
                return;
            }

            string moduleId = selectedModules[tabInd];


            Uri moduleFile = new Uri(ModuleRepositoryBaseUri, moduleId + "/" + moduleId + ".v" + repositoryModuleHeaders[moduleId].Version.ToString() + ".json.zip");

            string tempFile = Path.GetTempFileName();

            ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Downloading module file..." };
            _ = progressWindow.ShowDialog(this);

            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(moduleFile, tempFile);
                }
                progressWindow.Close();
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                await new MessageBox("Attention!", "An error occurred while downloading the module file!\n" + ex.Message).ShowDialog2(this);
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                ZipFile.ExtractToDirectory(tempFile, tempDir);

                ModuleMetadata metaData;

                using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                {
                    metaData = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                }

                if (Modules.LoadedModulesMetadata.ContainsKey(metaData.Id))
                {
                    await new MessageBox("Attention", "A module with the same Id has already been loaded! If you wish to update the module, please Install the new version instead.").ShowDialog2(this);
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

                managerWindowParent.BuildModuleButton(new KeyValuePair<string, ModuleMetadata>(metaData.Id, metaData));
                repositoryModuleHeaders.Remove(moduleId);
                ((IPanel)ModuleButtons[moduleId].Parent).Children.Remove(ModuleButtons[moduleId]);
                ModuleButtons.Remove(moduleId);
                noModuleGrids[tabInd].IsVisible = true;

                foreach (KeyValuePair<string, ModuleHeader> module in repositoryModuleHeaders)
                {
                    if ((int)module.Value.ModuleType == tabInd)
                    {
                        progressWindow = new ProgressWindow() { ProgressText = "Downloading module file..." };
                        _ = progressWindow.ShowDialog(this);

                        await SelectModule(module.Key, module.Value.Version.ToString());

                        progressWindow.Close();
                        break;
                    }
                }

                notifications = new NotificationType[headers.Length];

                foreach (ModuleHeader header in repositoryModuleHeaders.Values)
                {
                    bool newModule = !Modules.LoadedModulesMetadata.TryGetValue(header.Id, out ModuleMetadata loadedModuleMetadata);

                    if (newModule || loadedModuleMetadata.Version < header.Version)
                    {
                        if (newModule)
                        {
                            notifications[(int)header.ModuleType] |= NotificationType.NewModules;
                        }
                        else
                        {
                            notifications[(int)header.ModuleType] |= NotificationType.Updates;
                        }
                    }
                }

                for (int i = 0; i < headers.Length; i++)
                {
                    switch (notifications[i])
                    {
                        case NotificationType.None:
                            notificationEllipses[i].Classes.Remove("Updates");
                            notificationEllipses[i].Classes.Remove("NewModules");
                            break;
                        case NotificationType.Updates:
                            notificationEllipses[i].Classes.Add("Updates");
                            notificationEllipses[i].Classes.Remove("NewModules");
                            break;
                        case NotificationType.NewModules:
                            notificationEllipses[i].Classes.Remove("Updates");
                            notificationEllipses[i].Classes.Add("NewModules");
                            break;
                        case NotificationType.Both:
                            notificationEllipses[i].Classes.Add("Updates");
                            notificationEllipses[i].Classes.Add("NewModules");
                            break;
                    }
                }

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
                    File.Delete(tempFile);
                }
                catch { }

                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        private async void InstallClicked()
        {
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                await new MessageBox("Attention!", "No module has been selected!").ShowDialog2(this);
                return;
            }

            string moduleId = selectedModules[tabInd];


            Uri moduleFile = new Uri(ModuleRepositoryBaseUri, moduleId + "/" + moduleId + ".v" + repositoryModuleHeaders[moduleId].Version.ToString() + ".json.zip");

            string tempFile = Path.GetTempFileName();

            ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Downloading module file..." };
            _ = progressWindow.ShowDialog(this);

            try
            {
                using (WebClient client = new WebClient())
                {
                    await client.DownloadFileTaskAsync(moduleFile, tempFile);
                }

                progressWindow.Close();
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                await new MessageBox("Attention!", "An error occurred while downloading the module file!\n" + ex.Message).ShowDialog2(this);
                return;
            }

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            bool keepModuleFile = false;
            try
            {
                ZipFile.ExtractToDirectory(tempFile, tempDir);

                ModuleMetadata metaData;

                using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                {
                    metaData = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                }

                if (Modules.LoadedModulesMetadata.ContainsKey(metaData.Id))
                {
                    bool installUnsigned = false;

                    if (!metaData.VerifySignature())
                    {
                        MessageBox box = new MessageBox("Attention", "The source code of the module could not be verified. Proceed only if you trust the source from which you obtained the module. Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo);
                        await box.ShowDialog2(this);
                        if (box.Result != MessageBox.Results.Yes)
                        {
                            return;
                        }
                        else
                        {
                            installUnsigned = true;
                        }
                    }

                    {
                        MessageBox box = new MessageBox("Question", "A previous version of the same module is already installed. We will now uninstall the previous version and install the current version.\nThe program will be rebooted in the process (we will do our best to recover the files that are currently open). Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog2(this);
                        if (box.Result != MessageBox.Results.Yes)
                        {
                            return;
                        }
                    }

                    bool copyReferences = false;

                    if (metaData.AdditionalReferences.Length > 0)
                    {
                        MessageBox box = new MessageBox("Question", "Do you want to install the external reference files as well?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog2(this);
                        copyReferences = box.Result == MessageBox.Results.Yes;
                    }

                    try
                    {
                        ModuleMetadata.Uninstall(metaData.Id);
                    }
                    catch (Exception ex)
                    {
                        await new MessageBox("Attention!", "An error occurred while uninstalling the previous version of the module!\n" + ex.Message).ShowDialog2(this);
                    }

                    await new MessageBox("Success!", "The previous version of the module was successfully uninstalled! The program will now reboot.\nPlease be patient for a few seconds...", iconType: MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(this);

                    keepModuleFile = true;

                    List<string> newArgs = new List<string>()
                    {
                        "-i", tempFile, "-b", "--delete-modules"
                    };

                    if (installUnsigned)
                    {
                        newArgs.Add("--install-unsigned");
                    }

                    if (copyReferences)
                    {
                        newArgs.Add("--install-references");
                    }

                    Program.Reboot(newArgs.ToArray(), true);
                    ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                }
                else
                {
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

                    managerWindowParent.BuildModuleButton(new KeyValuePair<string, ModuleMetadata>(metaData.Id, metaData));
                    repositoryModuleHeaders.Remove(moduleId);
                    ((IPanel)ModuleButtons[moduleId].Parent).Children.Remove(ModuleButtons[moduleId]);
                    ModuleButtons.Remove(moduleId);
                    noModuleGrids[tabInd].IsVisible = true;

                    foreach (KeyValuePair<string, ModuleHeader> module in repositoryModuleHeaders)
                    {
                        if ((int)module.Value.ModuleType == tabInd)
                        {
                            progressWindow = new ProgressWindow() { ProgressText = "Downloading module file..." };
                            _ = progressWindow.ShowDialog(this);

                            await SelectModule(module.Key, module.Value.Version.ToString());

                            progressWindow.Close();
                            break;
                        }
                    }

                    notifications = new NotificationType[headers.Length];

                    foreach (ModuleHeader header in repositoryModuleHeaders.Values)
                    {
                        bool newModule = !Modules.LoadedModulesMetadata.TryGetValue(header.Id, out ModuleMetadata loadedModuleMetadata);

                        if (newModule || loadedModuleMetadata.Version < header.Version)
                        {
                            if (newModule)
                            {
                                notifications[(int)header.ModuleType] |= NotificationType.NewModules;
                            }
                            else
                            {
                                notifications[(int)header.ModuleType] |= NotificationType.Updates;
                            }
                        }
                    }

                    for (int i = 0; i < headers.Length; i++)
                    {
                        switch (notifications[i])
                        {
                            case NotificationType.None:
                                notificationEllipses[i].Classes.Remove("Updates");
                                notificationEllipses[i].Classes.Remove("NewModules");
                                break;
                            case NotificationType.Updates:
                                notificationEllipses[i].Classes.Add("Updates");
                                notificationEllipses[i].Classes.Remove("NewModules");
                                break;
                            case NotificationType.NewModules:
                                notificationEllipses[i].Classes.Remove("Updates");
                                notificationEllipses[i].Classes.Add("NewModules");
                                break;
                            case NotificationType.Both:
                                notificationEllipses[i].Classes.Add("Updates");
                                notificationEllipses[i].Classes.Add("NewModules");
                                break;
                        }
                    }

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
            }
            catch (Exception ex)
            {
                await new MessageBox("Attention!", "An error occurred while installing the module!\n" + ex.Message).ShowDialog2(this);
            }
            finally
            {
                if (!keepModuleFile)
                {
                    try
                    {
                        File.Delete(tempFile);
                    }
                    catch { }
                }

                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        internal async Task OpenSourceCodeWindow()
        {
            int tabInd = this.FindControl<TabControl>("MainTabControl").SelectedIndex;
            if (string.IsNullOrEmpty(selectedModules[tabInd]))
            {
                return;
            }

            string moduleId = selectedModules[tabInd];
            string moduleVersion = repositoryModuleHeaders[moduleId].Version.ToString();


            ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Accessing module database..." };
            _ = progressWindow.ShowDialog(this);

            ModuleMetadata module;

            try
            {
                Uri moduleFile = new Uri(ModuleRepositoryBaseUri, moduleId + "/" + moduleId + ".v" + moduleVersion + ".json.zip");

                using (WebClient client = new WebClient())
                {
                    string tempFile = Path.GetTempFileName();
                    await client.DownloadFileTaskAsync(moduleFile, tempFile);

                    string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    ZipFile.ExtractToDirectory(tempFile, tempDir);

                    File.Delete(tempFile);

                    using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                    {
                        module = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                    }

                    File.Delete(tempFile);
                    Directory.Delete(tempDir, true);
                }
                progressWindow.Close();
            }
            catch (Exception ex)
            {
                progressWindow.Close();
                await new MessageBox("Attention!", "An error occurred while downloading the module file!\n" + ex.Message).ShowDialog2(this);
                return;
            }

            CodeViewerWindow window = new CodeViewerWindow();
            await window.Initialize(module.SourceCode);
            await window.ShowDialog2(this);
        }
    }
}
