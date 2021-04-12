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
    public class ModuleCreatorWindow : Window
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
                    this.FindControl<Grid>("RecentFilesGrid").RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                    TextBlock moduleType = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5), Text = moduleTypeTranslations[item.Item2] };
                    Grid.SetRow(moduleType, index);
                    this.FindControl<Grid>("RecentFilesGrid").Children.Add(moduleType);

                    TextBlock lastWrite = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5), Text = item.Item4.ToString("g") };
                    Grid.SetRow(lastWrite, index);
                    Grid.SetColumn(lastWrite, 1);
                    this.FindControl<Grid>("RecentFilesGrid").Children.Add(lastWrite);

                    Button openButton = new Button() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = "Open", Margin = new Thickness(5) };
                    Grid.SetRow(openButton, index);
                    Grid.SetColumn(openButton, 2);
                    this.FindControl<Grid>("RecentFilesGrid").Children.Add(openButton);

                    Button deleteButton = new Button() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Content = "Delete", Margin = new Thickness(5) };
                    Grid.SetRow(deleteButton, index);
                    Grid.SetColumn(deleteButton, 3);
                    this.FindControl<Grid>("RecentFilesGrid").Children.Add(deleteButton);

                    openButton.Click += async (s, e) =>
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
                    };

                    deleteButton.Click += async (s, e) =>
                    {
                        try
                        {
                            Directory.Delete(item.Item1, true);

                            this.FindControl<Grid>("RecentFilesGrid").Children.Remove(moduleType);
                            this.FindControl<Grid>("RecentFilesGrid").Children.Remove(lastWrite);
                            this.FindControl<Grid>("RecentFilesGrid").Children.Remove(openButton);
                            this.FindControl<Grid>("RecentFilesGrid").Children.Remove(deleteButton);
                            this.FindControl<Grid>("RecentFilesGrid").RowDefinitions.RemoveAt(this.FindControl<Grid>("RecentFilesGrid").RowDefinitions.Count - 1);
                        }
                        catch (Exception ex)
                        {
                            MessageBox box = new MessageBox("Attention", "An error occurred while deleting the file!\n" + ex.Message);
                            await box.ShowDialog(this);
                        }
                    };

                    index++;
                }

                this.FindControl<Button>("DeleteAllButton").Click += async (s, e) =>
                {
                    try
                    {
                        foreach ((string, string, int, DateTime) item in recentItems)
                        {
                            Directory.Delete(item.Item1, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox box = new MessageBox("Attention", "An error occurred while deleting the files!\n" + ex.Message);
                        await box.ShowDialog(this);
                    }

                    this.FindControl<Expander>("RecentFilesExpander").IsVisible = false;
                };
            }
            else
            {
                Directory.CreateDirectory(autosavePath);
            }

            this.FindControl<Expander>("RecentFilesExpander").IsVisible = recentItems.Count > 0;

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
            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "AvaloniaColorPicker.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "AvaloniaColorPicker.xml")));
            Modules.CachedReferences.Add(CachedMetadataReference.CreateFromFile(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "Avalonia.Controls.PanAndZoom.dll"), Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "Avalonia.Controls.PanAndZoom.xml")));

            Editor editor = await Editor.Create(moduleSource, references: Modules.CachedReferences, guid: editorId);

            this.CodeEditor = editor;

            this.FindControl<TabItem>("EditorContainer").Content = editor;
            this.FindControl<Grid>("EditorContainerGrid").IsVisible = true;

            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);
            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.ImageMultiplier *= 1.8;
            this.FindControl<MarkdownCanvasControl>("ManualCanvas").Renderer.ImageUnitMultiplier /= 1.8;

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<TabControl>("EditorContainerTabControl").SelectionChanged += async (s, e) =>
            {
                if (this.FindControl<TabControl>("EditorContainerTabControl").SelectedIndex == 1)
                {
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
                        await box.ShowDialog(this);
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

                Thread thr = new Thread(async () =>
                {
                    handle.WaitOne();

                    foreach (Microsoft.CodeAnalysis.MetadataReference reference in editor.References)
                    {
                        try
                        {
                            originalReferences.Add(reference.Display);
                        }
                        catch { }
                    }

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

                await window.ShowDialog(this);

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
                    await box.ShowDialog(this);
                }
            };
        }
    }
}
