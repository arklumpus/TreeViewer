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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Transformation;
using Avalonia.Threading;
using CSharpEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using VectSharp.MarkdownCanvas;

namespace TreeViewer
{
    public class ModuleCreatorWindow : ChildWindow
    {
        private string[] SubsequentArgs;

        public ModuleCreatorWindow(string[] subsequentArgs)
        {
            InitializeComponent();
            this.SubsequentArgs = subsequentArgs;
        }

        public ModuleCreatorWindow()
        {
            InitializeComponent();
        }

        private Func<Task> loadAdditionalModule = null;

        private static InterprocessDebuggerServer DebuggerServer = null;

        private static Dictionary<string, string> moduleTypeTranslations = new Dictionary<string, string>()
        {
            { "FileType" , "File type" },
            { "LoadFile", "Load file" },
            { "Transformer", "Transformer" },
            { "FurtherTransformation", "Further transformation" },
            { "Coordinates", "Coordinates" },
            { "PlotAction", "Plot action"},
            { "SelectionAction", "Selection action" },
            { "Action", "Action" },
            { "MenuAction", "Menu action" }
        };

        private static Dictionary<string, string> moduleTypeIcons = new Dictionary<string, string>()
        {
            { "FileType" , "FileType" },
            { "LoadFile", "LoadFile" },
            { "Transformer", "Transformer" },
            { "FurtherTransformation", "FurtherTransformations" },
            { "Coordinates", "Coordinates" },
            { "PlotAction", "PlotActions"},
            { "SelectionAction", "SelectionAction" },
            { "Action", "Action" },
            { "MenuAction", "MenuAction" }
        };

        public static readonly DirectProperty<ModuleCreatorWindow, Editor> CodeEditorProperty = AvaloniaProperty.RegisterDirect<ModuleCreatorWindow, Editor>(nameof(CodeEditor), o => o.CodeEditor);

        private Editor _codeEditor;

        public Editor CodeEditor
        {
            get { return _codeEditor; }
            private set { SetAndRaise(CodeEditorProperty, ref _codeEditor, value); }
        }

        private async Task EncodeBinaryFileInBase64()
        {
            if (CodeEditor != null)
            {
                try
                {
                    OpenFileDialog dialog;

                    if (!Modules.IsMac)
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Encode binary file in Base64...",
                            AllowMultiple = false,
                            Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string>() { "*" }, Name = "All files" } }
                        };
                    }
                    else
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Encode binary file in Base64...",
                            AllowMultiple = false
                        };
                    }

                    string[] result = await dialog.ShowAsync(this);

                    if (result != null && result.Length == 1)
                    {
                        byte[] bytes = File.ReadAllBytes(result[0]);
                        string converted = Convert.ToBase64String(bytes);

                        await CodeEditor.SetText(CodeEditor.SourceText.Replace(CodeEditor.Selection, converted));
                        CodeEditor.Selection = new Microsoft.CodeAnalysis.Text.TextSpan(CodeEditor.Selection.End, 0);
                    }
                }
                catch
                {

                }

            }
        }

        private async Task EncodeTextFileInBase64()
        {
            if (CodeEditor != null)
            {
                try
                {
                    OpenFileDialog dialog;

                    if (!Modules.IsMac)
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Encode text file in Base64...",
                            AllowMultiple = false,
                            Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string>() { "*" }, Name = "All files" } }
                        };
                    }
                    else
                    {
                        dialog = new OpenFileDialog()
                        {
                            Title = "Encode text file in Base64...",
                            AllowMultiple = false
                        };
                    }

                    string[] result = await dialog.ShowAsync(this);

                    if (result != null && result.Length == 1)
                    {
                        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(File.ReadAllText(result[0]));
                        string converted = Convert.ToBase64String(bytes);

                        await CodeEditor.SetText(CodeEditor.SourceText.Replace(CodeEditor.Selection, converted));
                        CodeEditor.Selection = new Microsoft.CodeAnalysis.Text.TextSpan(CodeEditor.Selection.End, 0);
                    }
                }
                catch
                {

                }

            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                this.Classes.Add("WindowsStyle");
            }

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ModuleCreator")) { Width = 32, Height = 32 });



            ((Grid)this.FindControl<Button>("FileTypeModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FileType")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("LoadFileModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.LoadFile")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("TransformerModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Transformer")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("FurtherTransformationModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FurtherTransformations")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("CoordinatesModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Coordinates")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("PlotActionModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PlotActions")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("SelectionActionModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SelectionAction")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("ActionModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Action")) { Width = 32, Height = 32 });
            ((Grid)this.FindControl<Button>("MenuActionModuleButton").Content).Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.MenuAction")) { Width = 32, Height = 32 });

            if (DebuggerServer == null)
            {
                DebuggerServer = Modules.GetNewDebuggerServer();
            }

            ((IControlledApplicationLifetime)Application.Current.ApplicationLifetime).Exit += (s, e) =>
            {
                DebuggerServer.Dispose();
            };

            NativeMenu menu = new NativeMenu();
            NativeMenuItem moduleMenu = new NativeMenuItem() { Header = "Edit" };

            NativeMenu moduleSubMenu = new NativeMenu();

            NativeMenuItem encodeBinaryFile = new NativeMenuItem() { Header = "Encode binary file in Base64...", Command = new SimpleCommand(win => ((ModuleCreatorWindow)win).CodeEditor != null, async a => await EncodeBinaryFileInBase64(), this, CodeEditorProperty) };
            moduleSubMenu.Add(encodeBinaryFile);

            NativeMenuItem encodeTextFile = new NativeMenuItem() { Header = "Encode text file in Base64...", Command = new SimpleCommand(win => ((ModuleCreatorWindow)win).CodeEditor != null, async a => await EncodeTextFileInBase64(), this, CodeEditorProperty) };
            moduleSubMenu.Add(encodeTextFile);

            moduleMenu.Menu = moduleSubMenu;
            menu.Add(moduleMenu);

            NativeMenu.SetMenu(this, menu);

            this.Closing += (s, e) =>
            {
                SplashScreen splash = new SplashScreen(SubsequentArgs);
                splash.OnModulesLoaded = loadAdditionalModule;

                splash.Show();
            };

            List<(string, string, int, DateTime)> recentItems = new List<(string, string, int, DateTime)>();

            string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);

            if (Directory.Exists(autosavePath))
            {
                foreach (string directory in Directory.GetDirectories(autosavePath))
                {
                    string editorId = Path.GetFileName(directory);

                    if (editorId.StartsWith("ModuleCreator"))
                    {
                        try
                        {
                            editorId = editorId.Substring(editorId.IndexOf("_") + 1);

                            string moduleType = editorId.Substring(0, editorId.IndexOf("_"));

                            DirectoryInfo info = new DirectoryInfo(directory);

                            DateTime date = info.LastWriteTime;

                            int count = info.EnumerateFiles().Count();

                            if (count > 0)
                            {
                                recentItems.Add((directory, moduleType, count, date));
                            }
                        }
                        catch { }
                    }
                }

                int index = 2;

                foreach ((string, string, int, DateTime) item in recentItems.OrderByDescending(a => a.Item4))
                {
                    Grid itemGrid = new Grid() { Height = 42, Width = 260, Margin = new Thickness(0, 0, 5, 5), Background = Avalonia.Media.Brushes.Transparent };
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(37, GridUnitType.Pixel));
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    StackPanel namePanel = new StackPanel() { Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(namePanel, 1);
                    itemGrid.Children.Add(namePanel);

                    TrimmedTextBox2 nameBlock;
                    TrimmedTextBox2 dateBlock;

                    {
                        nameBlock = new TrimmedTextBox2() { Text = moduleTypeTranslations[item.Item2], FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        AvaloniaBugFixes.SetToolTip(nameBlock, new TextBlock() { Text = moduleTypeTranslations[item.Item2], TextWrapping = Avalonia.Media.TextWrapping.NoWrap });
                        namePanel.Children.Add(nameBlock);
                    }

                    {
                        dateBlock = new TrimmedTextBox2() { Text = item.Item4.ToString("f"), Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                        AvaloniaBugFixes.SetToolTip(dateBlock, new TextBlock() { Text = item.Item4.ToString("f"), TextWrapping = Avalonia.Media.TextWrapping.NoWrap });
                        namePanel.Children.Add(dateBlock);
                    }

                    itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets." + moduleTypeIcons[item.Item2])) { Width = 32, Height = 32, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                    StackPanel buttonsPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(buttonsPanel, 2);
                    itemGrid.Children.Add(buttonsPanel);

                    Button deleteButton = new Button() { Foreground = Avalonia.Media.Brushes.Black, Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")) { Width = 16, Height = 16 }, Background = Avalonia.Media.Brushes.Transparent, Padding = new Thickness(0), Width = 24, Height = 24, Margin = new Thickness(0, 0, 0, 2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, IsVisible = false };
                    buttonsPanel.Children.Add(deleteButton);
                    AvaloniaBugFixes.SetToolTip(deleteButton, new TextBlock() { Text = "Delete", Foreground = Avalonia.Media.Brushes.Black });
                    deleteButton.Classes.Add("SideBarButton");

                    itemGrid.PointerEnter += (s, e) =>
                    {
                        deleteButton.IsVisible = true;
                        itemGrid.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(210, 210, 210));
                    };

                    itemGrid.PointerLeave += (s, e) =>
                    {
                        deleteButton.IsVisible = false;
                        itemGrid.Background = Avalonia.Media.Brushes.Transparent;
                    };

                    itemGrid.PointerPressed += (s, e) =>
                    {
                        itemGrid.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(177, 177, 177));
                    };

                    itemGrid.PointerReleased += async (s, e) =>
                    {
                        itemGrid.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(210, 210, 210));

                        Point pos = e.GetCurrentPoint(itemGrid).Position;

                        if (pos.X >= 0 && pos.Y >= 0 && pos.X <= itemGrid.Bounds.Width && pos.Y <= itemGrid.Bounds.Height)
                        {
                            string guid = Path.GetFileName(item.Item1);

                            string oldestFile = null;
                            DateTime oldestTime = DateTime.UnixEpoch;

                            foreach (string sr in Directory.GetFiles(item.Item1))
                            {
                                FileInfo fi = new FileInfo(sr);

                                if (fi.LastWriteTime.CompareTo(oldestTime) == 1)
                                {
                                    oldestTime = fi.LastWriteTime;
                                    oldestFile = sr;
                                }
                            }

                            string moduleSource = File.ReadAllText(oldestFile);

                            await InitializeEditor(moduleSource, guid);
                        }
                    };

                    deleteButton.Click += async (s, e) =>
                    {
                        try
                        {
                            Directory.Delete(item.Item1, true);

                            this.FindControl<WrapPanel>("RecentFilesGrid").Children.Remove(itemGrid);
                        }
                        catch (Exception ex)
                        {
                            MessageBox box = new MessageBox("Attention", "An error occurred while deleting the file!\n" + ex.Message);
                            await box.ShowDialog2(this);
                        }
                    }; ;


                    this.FindControl<WrapPanel>("RecentFilesGrid").Children.Add(itemGrid);

                    index++;
                }
            }
            else
            {
                Directory.CreateDirectory(autosavePath);
            }

            this.FindControl<Button>("FileTypeModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.FileType.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_FileType_" + guid.ToString());
            };

            this.FindControl<Button>("PlotActionModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.PlotAction.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_PlotAction_" + guid.ToString());
            };

            this.FindControl<Button>("LoadFileModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.LoadFile.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_LoadFile_" + guid.ToString());
            };

            this.FindControl<Button>("TransformerModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.Transformer.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_Transformer_" + guid.ToString());
            };

            this.FindControl<Button>("FurtherTransformationModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.FurtherTransformation.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_FurtherTransformation_" + guid.ToString());
            };

            this.FindControl<Button>("CoordinatesModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.Coordinates.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_Coordinates_" + guid.ToString());
            };

            this.FindControl<Button>("SelectionActionModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.SelectionAction.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_SelectionAction_" + guid.ToString());
            };

            this.FindControl<Button>("ActionModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.Action.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_Action_" + guid.ToString());
            };

            this.FindControl<Button>("MenuActionModuleButton").Click += async (s, e) =>
            {
                string moduleSource;
                using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Templates.MenuAction.cs")))
                {
                    moduleSource = reader.ReadToEnd();
                }

                Guid guid = Guid.NewGuid();

                moduleSource = moduleSource.Replace("@NamespaceHere", "a" + guid.ToString("N"));
                moduleSource = moduleSource.Replace("@GuidHere", guid.ToString());

                await InitializeEditor(moduleSource, "ModuleCreator_MenuAction_" + guid.ToString());
            };
        }

        private async Task InitializeEditor(string moduleSource, string editorId)
        {
            this.FindControl<ScrollViewer>("TemplateScrollViewer").IsVisible = false;

            Modules.RebuildCachedReferences();

            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "PhyloTree.TreeNode.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "PhyloTree.TreeNode.xml")));
            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "MuPDFCore.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "MuPDFCore.xml")));
            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "Avalonia.Controls.PanAndZoom.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "Avalonia.Controls.PanAndZoom.xml")));
            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "MathNet.Numerics.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "MathNet.Numerics.xml")));

            Editor editor = await Editor.Create(moduleSource, references: Modules.CachedReferences, guid: editorId);
            editor.Background = this.Background;
            editor.Margin = new Thickness(0, -10, 0, -10);

            this.CodeEditor = editor;
            Grid.SetRow(editor, 2);

            this.FindControl<Grid>("EditorContainer").Children.Add(editor);
            this.FindControl<Grid>("EditorContainerGrid").IsVisible = true;

            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.ImageMultiplier *= 1.4;
            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.ImageUnitMultiplier /= 1.4;

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Close();
            };

            RibbonBar bar = new RibbonBar(new (string, bool)[] { ("Source code", false), ("Manual", false) }) { FontSize = 15 };

            if (GlobalSettings.Settings.RibbonStyle == GlobalSettings.RibbonStyles.Colourful)
            {
                this.FindControl<Grid>("RibbonBarContainer").Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178));
                bar.Margin = new Thickness(-1, 0, -1, 0);
                bar.Classes.Add("Colorful");
            }
            else
            {
                bar.Classes.Add("Grey");
            }

            this.FindControl<Grid>("RibbonBarContainer").Children.Add(bar);

            RibbonTabContent tabContent = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Encode in Base64", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Encode binary file", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.BinaryFile")) { Width = 32, Height = 32 }, null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async _ =>{ await EncodeBinaryFileInBase64(); }), "Embeds a binary file in the code using base-64 encoding."),

                    ("Encode text file", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.TextFile")) { Width = 32, Height = 32 }, null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async _ =>{ await EncodeTextFileInBase64(); }), "Embeds a text file in the code using base-64 encoding."),
                })
            });

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(tabContent);

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            bar.PropertyChanged += async (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    int newIndex = (int)e.NewValue;
                    if (newIndex == 0)
                    {
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").ZIndex = 0;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").RenderTransform = offScreen;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").Opacity = 0;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").IsHitTestVisible = false;

                        this.FindControl<Grid>("EditorContainer").ZIndex = 1;
                        this.FindControl<Grid>("EditorContainer").RenderTransform = TransformOperations.Identity;
                        this.FindControl<Grid>("EditorContainer").Opacity = 1;
                        this.FindControl<Grid>("EditorContainer").IsHitTestVisible = true;
                    }
                    else
                    {
                        this.FindControl<Grid>("EditorContainer").ZIndex = 0;
                        this.FindControl<Grid>("EditorContainer").RenderTransform = offScreen;
                        this.FindControl<Grid>("EditorContainer").Opacity = 0;
                        this.FindControl<Grid>("EditorContainer").IsHitTestVisible = false;

                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").ZIndex = 1;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").RenderTransform = TransformOperations.Identity;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").Opacity = 1;
                        this.FindControl<MarkdownCanvasControl>("ManualCanvas").IsHitTestVisible = true;

                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await Task.Delay(150);

                            string markdownSource = "";

                            try
                            {
                                List<string> originalReferences = new List<string>();
                                string fullSource = editor.FullSource;

                                foreach (Microsoft.CodeAnalysis.MetadataReference reference in editor.References)
                                {
                                    try
                                    {
                                        originalReferences.Add(reference.Display);
                                    }
                                    catch { }
                                }

                                ModuleMetadata metadata = ModuleMetadata.CreateFromSource(fullSource, originalReferences.ToArray());
                                markdownSource = metadata.BuildReadmeMarkdown();

                                this.FindControl<MarkdownCanvasControl>("ManualCanvas").DocumentSource = markdownSource;
                            }
                            catch (Exception ex)
                            {
                                this.FindControl<MarkdownCanvasControl>("ManualCanvas").DocumentSource = "";
                                MessageBox box = new MessageBox("Attention", "An error occurred while creating the manual for the module!\n" + ex.Message + "\n" + ex.StackTrace + "\n" + markdownSource);
                                await box.ShowDialog2(this);
                            }
                        });
                    }
                }
            };

            this.FindControl<Button>("OKButton").Click += async (s, e) =>
            {
                editor.Save();

                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

                ProgressWindow window = new ProgressWindow(handle) { IsIndeterminate = false, ProgressText = "Removing unnecessary references..." };

                List<string> originalReferences = new List<string>();
                string fullSource = editor.FullSource;

                foreach (Microsoft.CodeAnalysis.MetadataReference reference in editor.References)
                {
                    try
                    {
                        originalReferences.Add(reference.Display);
                    }
                    catch { }
                }
                
                try
                {
                    ModuleMetadata.CreateFromSource(fullSource, originalReferences.ToArray());

                    Thread thr = new Thread(async () =>
                    {
                        handle.WaitOne();

                        List<string> toBeRemoved = new List<string>();
                        List<string> newReferences = new List<string>(originalReferences);

                        for (int j = 0; j < originalReferences.Count; j++)
                        {
                            List<string> currentReferences = new List<string>(newReferences);
                            currentReferences.Remove(originalReferences[j]);

                            try
                            {
                                ModuleMetadata.CreateFromSource(fullSource, currentReferences.ToArray());
                                toBeRemoved.Add(originalReferences[j]);
                                newReferences = currentReferences;
                            }
                            catch
                            {

                            }

                            await Dispatcher.UIThread.InvokeAsync(() => window.Progress = (double)(j + 1) / originalReferences.Count);
                        }

                        for (int j = 0; j < toBeRemoved.Count; j++)
                        {
                            originalReferences.Remove(toBeRemoved[j]);
                        }

                        await Dispatcher.UIThread.InvokeAsync(() => { window.Close(); });
                    });

                    thr.Start();

                    await window.ShowDialog2(this);

                    try
                    {
                        (Assembly assembly, Microsoft.CodeAnalysis.CSharp.CSharpCompilation compilation) = await editor.Compile(DebuggerServer.SynchronousBreak(editor), DebuggerServer.AsynchronousBreak(editor));

                        Type[] types = assembly.GetTypes();

                        Type moduleType = null;

                        foreach (Type type in types)
                        {
                            if (type.Name == "MyModule")
                            {
                                moduleType = type;
                                break;
                            }
                        }

                        ModuleMetadata metadata = ModuleMetadata.CreateFromSource(fullSource, originalReferences.ToArray());

                        this.loadAdditionalModule = () =>
                        {
                            Module loadedModule = metadata.Load(moduleType, true);
                            Modules.LoadModule(metadata, loadedModule);

                            return Task.CompletedTask;
                        };

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox box = new MessageBox("Attention", "An error occurred while compiling the module!\n" + ex.Message);
                        await box.ShowDialog2(this);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox box = new MessageBox("Attention", "An error occurred while compiling the module!\n" + ex.Message);
                    await box.ShowDialog2(this);
                }
            };  
        }
    }
}
