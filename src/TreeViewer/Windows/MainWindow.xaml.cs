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
using Avalonia.Input;
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
using VectSharp;
using VectSharp.Canvas;
using System.Security.Cryptography;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using Avalonia.Media.Transformation;
using Avalonia.Styling;

namespace TreeViewer
{
    public partial class MainWindow : Window, IWindowWithToolTips
    {
        public List<Control> ControlsWithToolTips { get; } = new List<Control>();


        public static readonly Avalonia.StyledProperty<bool> IsTreeOpenedProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(IsTreeOpenedProperty), false);
        public bool IsTreeOpened
        {
            get
            {
                return GetValue(IsTreeOpenedProperty);
            }
            set
            {
                SetValue(IsTreeOpenedProperty, value);
            }
        }

        private CSharpEditor.InterprocessDebuggerServer cachedDebuggerServer = null;

        public CSharpEditor.InterprocessDebuggerServer DebuggerServer
        {
            get
            {
                if (cachedDebuggerServer == null)
                {
                    cachedDebuggerServer = Modules.GetNewDebuggerServer();
                }

                return cachedDebuggerServer;
            }
        }


        public InstanceStateData StateData = new InstanceStateData();

        public TreeCollection Trees
        {
            get { return StateData.Trees; }
            set
            {
                StateData.Trees = value;
                if (StateData.Trees != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsTreeOpened = true;
                    });
                }
                else
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        IsTreeOpened = false;
                    });
                }
            }
        }

        public TreeNode FirstTransformedTree = null;
        public TreeNode TransformedTree
        {
            get
            {
                return StateData.TransformedTree;
            }
            set
            {
                StateData.TransformedTree = value;
            }
        }

        List<string> AttributeList = null;
        List<ComboBox> AttributeSelectors = null;

        List<ComboBox> AttachmentSelectors = null;

        public Dictionary<string, VectSharp.Point> Coordinates = null;

        public PreviewComboBox TransformerComboBox;
        Control TransformerAlert;
        public Dictionary<string, object> TransformerParameters;
        public Action<Dictionary<string, object>> UpdateTransformerParameters;

        public PreviewComboBox CoordinatesComboBox;
        Control CoordinatesAlert;
        public Dictionary<string, object> CoordinatesParameters;
        public Action<Dictionary<string, object>> UpdateCoordinatesParameters;

        ColorButton GraphBackgroundButton;

        StackPanel PlottingActionsContainer;
        public List<PlottingModule> PlottingActions = new List<PlottingModule>();

        List<Control> PlottingAlerts;
        public List<Dictionary<string, object>> PlottingParameters;
        public List<Action<Dictionary<string, object>>> UpdatePlottingParameters;

        StackPanel FurtherTransformationsContainer;
        public List<FurtherTransformationModule> FurtherTransformations = new List<FurtherTransformationModule>();

        List<Control> FurtherTransformationsAlerts;
        public List<Dictionary<string, object>> FurtherTransformationsParameters;
        public List<Action<Dictionary<string, object>>> UpdateFurtherTransformationParameters;

        List<Action> SelectionActionActions;

        private string WindowGuid = System.Guid.NewGuid().ToString();
        public string OriginalFileName { get; set; } = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(TreeCollection trees, List<(string, Dictionary<string, object>)> moduleSuggestions, string fileName, string nameOverride = null)
        {
            InitializeComponent();

            this.Opened += (s, e) =>
            {
                Trees = trees;
                FileOpened(moduleSuggestions, fileName);

                if (string.IsNullOrEmpty(nameOverride))
                {
                    if (!string.IsNullOrEmpty(fileName))
                    {
                        this.Title = "TreeViewer - " + Path.GetFileName(fileName);
                        this.OriginalFileName = fileName;
                    }
                }
                else
                {
                    this.Title = "TreeViewer - " + Path.GetFileName(nameOverride);
                    this.OriginalFileName = nameOverride;
                }
            };
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            RenderingUpdateRequestTerminator.Set();

            lock (AutosaveLock)
            {
                AutosaveTimer?.Stop();
                GlobalSettings.Settings.MainWindows.Remove(this);
                Trees?.Dispose();

                if (cachedDebuggerServer != null)
                {
                    DebuggerServer.Dispose();
                }

                foreach (KeyValuePair<string, Attachment> kvp in StateData.Attachments)
                {
                    kvp.Value.Dispose();
                }
            }
        }

        public async Task<Window> LoadFile(string fileName, bool deleteAfter, string nameOverride = null)
        {
            Window tbr = null;

            double maxResult = 0;
            int maxIndex = -1;

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                    if (priority > maxResult)
                    {
                        maxResult = priority;
                        maxIndex = i;
                    }
                }
                catch { }
            }

            if (maxIndex >= 0)
            {
                double maxLoadResult = 0;
                int maxLoadIndex = -1;

                IEnumerable<TreeNode> loader;

                try
                {
                    List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };

                    EventWaitHandle progressWindowHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    ProgressWindow progressWin = new ProgressWindow(progressWindowHandle) { ProgressText = "Opening and loading file..." };
                    SemaphoreSlim progressSemaphore = new SemaphoreSlim(0, 1);
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

                    TreeCollection coll = null;

                    Thread thr = new Thread(async () =>
                    {
                        progressWindowHandle.WaitOne();

                        Action<double> openerProgressAction = (_) => { };

                        bool? codePermissionGranted = null;

                        bool askForCodePermission(RSAParameters? publicKey)
                        {
                            if (codePermissionGranted.HasValue)
                            {
                                return codePermissionGranted.Value;
                            }
                            else
                            {
                                MessageBox box = null;

                                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

                                async void showDialog()
                                {
                                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        box = new MessageBox("Attention!", "The selected file contains source code and its signature does not match any known keys. Do you want to load and compile it? You should only do this if you trust the source of the file and/or you have accurately reviewed the code.", MessageBox.MessageBoxButtonTypes.YesNo);
                                        await box.ShowDialog2(this);

                                        if (box.Result == MessageBox.Results.Yes && publicKey.HasValue)
                                        {
                                            MessageBox box2 = new MessageBox("Question", "Would you like to add the file's public key to the local storage? This will allow you to open other files produced by the same author without seeing this dialog. You should only do this if you trust the source of the file.", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                                            await box2.ShowDialog2(this);
                                            if (box2.Result == MessageBox.Results.Yes)
                                            {
                                                CryptoUtils.AddPublicKey(publicKey.Value);
                                            }
                                        }
                                    });

                                    handle.Set();
                                }

                                showDialog();

                                handle.WaitOne();

                                if (box.Result == MessageBox.Results.Yes)
                                {
                                    codePermissionGranted = true;
                                    return true;
                                }
                                else
                                {
                                    codePermissionGranted = false;
                                    return false;
                                }
                            }
                        };

                        loader = Modules.FileTypeModules[maxIndex].OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

                        FileInfo finfo = new FileInfo(fileName);

                        for (int i = 0; i < Modules.LoadFileModules.Count; i++)
                        {
                            try
                            {
                                double priority = Modules.LoadFileModules[i].IsSupported(finfo, Modules.FileTypeModules[maxIndex].Id, loader);
                                if (priority > maxLoadResult)
                                {
                                    maxLoadResult = priority;
                                    maxLoadIndex = i;
                                }
                            }
                            catch { }
                        }

                        if (maxLoadIndex >= 0)
                        {
                            try
                            {
                                coll = Modules.LoadFileModules[maxLoadIndex].Load(this, finfo, Modules.FileTypeModules[maxIndex].Id, loader, moduleSuggestions, ref openerProgressAction, progressAction);
                            }
                            catch (Exception ex)
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Error!", "An error has occurred while loading the file!\n" + ex.Message).ShowDialog2(this); return Task.CompletedTask; });
                            }

                            progressSemaphore.Release();
                        }
                        else
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Attention!", "The file cannot be loaded by any of the currently installed modules!").ShowDialog2(this); return Task.CompletedTask; });
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                        }
                    });

                    thr.Start();

                    _ = progressWin.ShowDialog2(this);

                    await progressSemaphore.WaitAsync();
                    progressSemaphore.Release();
                    progressSemaphore.Dispose();

                    if (coll != null)
                    {
                        if (Trees == null)
                        {
                            Trees = coll;

                            FileOpened(moduleSuggestions, fileName, progressWin);

                            if (!deleteAfter)
                            {
                                if (string.IsNullOrEmpty(nameOverride))
                                {
                                    this.Title = "TreeViewer - " + Path.GetFileName(fileName);
                                    this.OriginalFileName = fileName;
                                }
                                else
                                {
                                    this.Title = "TreeViewer - " + Path.GetFileName(nameOverride);
                                    this.OriginalFileName = nameOverride;
                                }
                            }

                            tbr = this;
                        }
                        else
                        {
                            progressWin.Close();
                            MainWindow win = new MainWindow(coll, moduleSuggestions, deleteAfter ? "" : fileName, nameOverride);
                            win.Show();
                            tbr = win;
                        }
                    }
                    else
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                    }
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "An error has occurred while opening the file!\n" + ex.Message).ShowDialog2(this);
                }
            }
            else
            {
                await new MessageBox("Attention!", "The file type is not supported by any of the currently installed modules!").ShowDialog2(this);
            }

            if (deleteAfter)
            {
                try
                {
                    File.Delete(fileName);
                }
                catch { }
            }

            return tbr;
        }


        private void CloseMenuClicked(object sender, EventArgs e)
        {
            this.Close();
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            bool closing = false;

            this.Closing += async (s, e) =>
            {
                if (!closing && IsTreeOpened)
                {
                    int totalElements = PlottingActionsContainer.Children.Count + FurtherTransformationsContainer.Children.Count;

                    System.Diagnostics.Debug.WriteLine(totalElements);

                    if (totalElements > 20)
                    {
                        e.Cancel = true;

                        if (GlobalSettings.Settings.MainWindows.Count == 1)
                        {
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                            return;
                        }

                        SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

                        ProgressWindow win = new ProgressWindow() { IsIndeterminate = false, ProgressText = "Unloading tree plot, please wait..." };
                        win.Opened += (s2, e2) =>
                        {
                            semaphore.Release();
                        };

                        _ = win.ShowDialog2(this);

                        await semaphore.WaitAsync();
                        semaphore.Release();
                        semaphore.Dispose();

                        int removedElements = 0;

                        while (FurtherTransformationsContainer.Children.Count > 0)
                        {
                            await Task.Run(async () =>
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    FurtherTransformationsContainer.Children.RemoveAt(0);
                                    removedElements++;
                                });
                                await Task.Delay(10);
                            });

                            win.Progress = (double)removedElements / totalElements;
                        }

                        while (PlottingActionsContainer.Children.Count > 0)
                        {
                            await Task.Run(async () =>
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    PlottingActionsContainer.Children.RemoveAt(0);
                                    removedElements++;
                                });
                                await Task.Delay(10);
                            });

                            win.Progress = (double)removedElements / totalElements;
                        }

                        win.Close();


                        closing = true;
                        this.Close();
                    }
                }
            };

            SetupPlatform();

            this.FindControl<Canvas>("StatsIconCanvas").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Stats")));
            this.FindControl<Button>("TreeStatsButton").Click += async (s, e) =>
            {
                TreeStatsWindow win = new TreeStatsWindow(this);

                await win.ShowDialog2(this);
            };

            this.FindControl<Grid>("LeftMouseButtonContainerGrid").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.LeftMouseButton")));
            this.FindControl<Grid>("MouseWheelContainerGrid").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.MouseWheel")));

            this.FindControl<Grid>("SelectionGrid").PropertyChanged += (s, e) =>
            {
                if (e.Property == Grid.BoundsProperty)
                {
                    double maxWidth = ((Rect)e.NewValue).Width - (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle ? 30 : 35);
                    this.FindControl<ScrollViewer>("SelectedNodeScrollViewer").MaxWidth = maxWidth;
                    this.FindControl<TextBox>("SelectedNodeLeavesBox").MaxWidth = maxWidth;
                }
            };

            SelectionCanvas = this.FindControl<Canvas>("SelectionCanvas");

            FullPlotCanvas = new SKMultiLayerRenderCanvas(new List<SKRenderContext>(), new List<SKRenderAction>(), Colour.FromRgba(0, 0, 0, 0), 1, 1);
            FullSelectionCanvas = new SKMultiLayerRenderCanvas(new List<SKRenderContext>(), new List<SKRenderAction>(), Colour.FromRgba(0, 0, 0, 0), 1, 1);

            SelectionCanvas.Children.Add(FullSelectionCanvas);
            this.FindControl<Canvas>("PlotCanvas").Children.Add(FullPlotCanvas);

            GlobalSettings.Settings.MainWindows.Add(this);

            //this.FindControl<Canvas>("PlotBackground").Background = new SolidColorBrush(((Colour)GlobalSettings.Settings.BackgroundColour).ToAvalonia());

            StateData.AddPlottingModule = this.AddPlottingModule;
            StateData.RemovePlottingModule = this.RemovePlottingModule;

            StateData.AddFurtherTransformationModule = this.AddFurtherTransformation;
            StateData.RemoveFurtherTransformationModule = this.RemoveFurtherTransformation;

            StateData.SetCoordinatesModule = this.SetCoordinateModule;
            StateData.SetTransformerModule = this.SetTransformerModule;

            StateData.TransformerModule = () =>
            {
                if (TransformerComboBox != null && TransformerComboBox.SelectedIndex >= 0)
                {
                    return Modules.TransformerModules[TransformerComboBox.SelectedIndex];
                }
                else
                {
                    return null;
                }

            };

            StateData.CoordinateModule = () =>
            {
                if (CoordinatesComboBox != null && CoordinatesComboBox.SelectedIndex >= 0)
                {
                    return Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex];
                }
                else
                {
                    return null;
                }

            };

            StateData.PlottingModules = () => this.PlottingActions;
            StateData.FurtherTransformationModules = () => this.FurtherTransformations;

            StateData.GetSelectedNode = () => this.SelectedNode;
            StateData.SetSelectedNode = (value) => this.SelectedNode = value;

            StateData.TransformerModuleParameterUpdater = () => this.UpdateTransformerParameters;
            StateData.CoordinatesModuleParameterUpdater = () => this.UpdateCoordinatesParameters;

            StateData.FurtherTransformationModulesParameterUpdater = (index) => this.UpdateFurtherTransformationParameters[index];
            StateData.PlottingModulesParameterUpdater = (index) => this.UpdatePlottingParameters[index];

            StateData.GetTransformerModuleParameters = () => this.TransformerParameters;
            StateData.GetCoordinatesModuleParameters = () => this.CoordinatesParameters;

            StateData.GetFurtherTransformationModulesParamters = (index) => this.FurtherTransformationsParameters[index];
            StateData.GetPlottingModulesParameters = (index) => this.PlottingParameters[index];

            StateData.OpenFile = (fileName, deleteAfter) => { _ = this.LoadFile(fileName, deleteAfter); };

            StateData.SerializeAllModules = this.SerializeAllModules;

            if (Modules.MissingModules == null)
            {
                this.Opened += async (s, e) =>
                {
                    this.Hide();
                    await new MessageBox("Attention", "The installed modules have not been loaded!").ShowDialog2(this);
                    this.Close();
                };

                return;
            }

            if (Modules.MissingModules.Count > 0)
            {
                this.Opened += async (s, e) =>
                {
                    string message = "Some installed modules could not be loaded. Please run the program with the --rebuild-all-modules switch to fix this.\n" + (from el in Modules.MissingModules select "\"" + el.Name + "\" (" + el.Id + ")").Aggregate((a, b) => a + "\n" + b);

                    await new MessageBox("Attention", message).ShowDialog2(this);
                };
            }

            SelectionActionActions = new List<Action>();

            BuildAllMenuModules();

            this.AutosaveTimer = new Avalonia.Threading.DispatcherTimer(GlobalSettings.Settings.AutosaveInterval, Avalonia.Threading.DispatcherPriority.Background, Autosave);
            this.AutosaveTimer.Start();

            this.PropertyChanged += (s, e) =>
            {
                if (WasAutoFitted)
                {
                    if (e.Property == Window.BoundsProperty && (((Rect)e.OldValue).Width != ((Rect)e.NewValue).Width || ((Rect)e.OldValue).Height != ((Rect)e.NewValue).Height))
                    {
                        AutoFit();
                    }
                }
            };

            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").PointerWheelChanged += (s, e) =>
            {
                WasAutoFitted = false;
            };
        }

        private void BuildAllMenuModules()
        {
            List<(string, List<(string, List<MenuActionModule>)>)> menuItems = new List<(string, List<(string, List<MenuActionModule>)>)>();

            menuItems.Add(("File", new List<(string, List<MenuActionModule>)>()));

            for (int i = 0; i < Modules.MenuActionModules.Count; i++)
            {
                string parentMenu = Modules.MenuActionModules[i].ParentMenu;

                List<(string, List<MenuActionModule>)> subMenu = null;

                for (int j = 0; j < menuItems.Count; j++)
                {
                    if (menuItems[j].Item1 == parentMenu)
                    {
                        subMenu = menuItems[j].Item2;
                        break;
                    }
                }

                if (subMenu == null)
                {
                    subMenu = new List<(string, List<MenuActionModule>)>();
                    menuItems.Add((parentMenu, subMenu));
                }

                string groupId = Modules.MenuActionModules[i].GroupName;

                List<MenuActionModule> subSubMenu = null;

                for (int j = 0; j < subMenu.Count; j++)
                {
                    if (subMenu[j].Item1 == groupId)
                    {
                        subSubMenu = subMenu[j].Item2;
                        break;
                    }
                }

                if (subSubMenu == null)
                {
                    subSubMenu = new List<MenuActionModule>();
                    subMenu.Add((groupId, subSubMenu));
                }

                subSubMenu.Add(Modules.MenuActionModules[i]);
            }

            bool helpFound = false;

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].Item1 == "Help")
                {
                    helpFound = true;
                }
            }

            if (!helpFound)
            {
                menuItems.Add(("Help", new List<(string, List<MenuActionModule>)>()));
            }

            NativeMenu menu = new NativeMenu();

            for (int i = 0; i < menuItems.Count; i++)
            {
                NativeMenuItem subMenu = new NativeMenuItem(menuItems[i].Item1);
                subMenu.Menu = new NativeMenu();

                if (menuItems[i].Item1 == "File")
                {
                    {
                        NativeMenuItem homeItem = new NativeMenuItem("Home");
                        homeItem.Command = new SimpleCommand((win) => true, (win) => { this.RibbonBar.SelectedIndex = 0; this.RibbonFilePage.SelectedIndex = 0; }, this);
                        homeItem.CommandParameter = this;
                        homeItem.Gesture = new KeyGesture(Key.H, Modules.ControlModifier | KeyModifiers.Shift);
                        subMenu.Menu.Items.Add(homeItem);

                        NativeMenuItem autosavesItem = new NativeMenuItem("Autosaves") { Menu = new NativeMenu() };

                        NativeMenuItem autosavedTrees = new NativeMenuItem("Autosaved trees");
                        autosavedTrees.Command = new SimpleCommand((win) => true, (win) =>
                        {
                            this.RibbonBar.SelectedIndex = 0;
                            AutosavesPage page = this.GetFilePage<AutosavesPage>(out int ind);
                            this.RibbonFilePage.SelectedIndex = ind;
                            ((RibbonFilePageContentTabbedWithButtons)page.PageContent).SelectedIndex = 0;
                        }, this);
                        autosavedTrees.CommandParameter = this;
                        autosavedTrees.Gesture = new KeyGesture(Key.A, Modules.ControlModifier | KeyModifiers.Shift);
                        autosavesItem.Menu.Items.Add(autosavedTrees);

                        NativeMenuItem autosavedCode = new NativeMenuItem("Autosaved code");
                        autosavedCode.Command = new SimpleCommand((win) => true, (win) =>
                        {
                            this.RibbonBar.SelectedIndex = 0;
                            AutosavesPage page = this.GetFilePage<AutosavesPage>(out int ind);
                            this.RibbonFilePage.SelectedIndex = ind;
                            ((RibbonFilePageContentTabbedWithButtons)page.PageContent).SelectedIndex = 1;

                        }, this);
                        autosavedCode.CommandParameter = this;
                        autosavesItem.Menu.Items.Add(autosavedCode);

                        subMenu.Menu.Items.Add(autosavesItem);

                        subMenu.Menu.Items.Add(new NativeMenuItemSeparator());
                    }
                }

                for (int j = 0; j < menuItems[i].Item2.Count; j++)
                {
                    for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                    {
                        NativeMenuItem item = new NativeMenuItem(menuItems[i].Item2[j].Item2[k].ItemText);

                        if (menuItems[i].Item2[j].Item2[k].SubItems.Count == 0)
                        {
                            Func<MainWindow, Task> clickAction;
                            Func<int, MainWindow, Task> performAction = menuItems[i].Item2[j].Item2[k].PerformAction;

                            clickAction = a => performAction(0, a);

                            if (menuItems[i].Item2[j].Item2[k].ShortcutKeys[0].Item1 != Key.None)
                            {
                                item.Gesture = new KeyGesture(menuItems[i].Item2[j].Item2[k].ShortcutKeys[0].Item1, Modules.GetModifier(menuItems[i].Item2[j].Item2[k].ShortcutKeys[0].Item2));
                            }

                            Func<MainWindow, List<bool>> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                            item.Command = new SimpleCommand((win) => isEnabled((MainWindow)win)[0], async (win) => { await clickAction((MainWindow)win); }, this, menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled);

                            item.CommandParameter = this;
                        }
                        else
                        {
                            item.Menu = new NativeMenu();

                            for (int l = 0; l < menuItems[i].Item2[j].Item2[k].SubItems.Count; l++)
                            {
                                NativeMenuItem subItem = new NativeMenuItem(menuItems[i].Item2[j].Item2[k].SubItems[l].Item1);

                                if (!string.IsNullOrEmpty(menuItems[i].Item2[j].Item2[k].SubItems[l].Item1))
                                {
                                    int lIndex = l;

                                    Func<MainWindow, Task> clickAction;
                                    Func<int, MainWindow, Task> performAction = menuItems[i].Item2[j].Item2[k].PerformAction;

                                    clickAction = a => performAction(lIndex - 1, a);

                                    if (menuItems[i].Item2[j].Item2[k].ShortcutKeys[l].Item1 != Key.None)
                                    {
                                        subItem.Gesture = new KeyGesture(menuItems[i].Item2[j].Item2[k].ShortcutKeys[l].Item1, Modules.GetModifier(menuItems[i].Item2[j].Item2[k].ShortcutKeys[l].Item2));
                                    }

                                    Func<MainWindow, List<bool>> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                                    subItem.Command = new SimpleCommand((win) => isEnabled((MainWindow)win)[lIndex], async (win) => { await clickAction((MainWindow)win); }, this, menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled);

                                    subItem.CommandParameter = this;
                                    item.Menu.Items.Add(subItem);
                                }
                            }
                        }

                        subMenu.Menu.Items.Add(item);
                    }
                    if (j < menuItems[i].Item2.Count - 1)
                    {
                        subMenu.Menu.Items.Add(new NativeMenuItemSeparator());
                    }
                }

                if (menuItems[i].Item1 == "File")
                {
                    subMenu.Menu.Items.Add(new NativeMenuItemSeparator());

                    NativeMenuItem exitItem = new NativeMenuItem("Close");
                    exitItem.Command = new SimpleCommand((win) => true, (win) => { this.Close(); }, this);
                    exitItem.CommandParameter = this;

                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                    {
                        exitItem.Gesture = new KeyGesture(Key.W, KeyModifiers.Meta);
                    }
                    else
                    {
                        exitItem.Gesture = new KeyGesture(Key.F4, KeyModifiers.Alt);
                    }

                    subMenu.Menu.Items.Add(exitItem);
                }

                if (menuItems[i].Item1 == "Help")
                {
                    if (subMenu.Menu.Items.Count > 0)
                    {
                        subMenu.Menu.Items.Add(new NativeMenuItemSeparator());
                    }

                    NativeMenuItem aboutItem = new NativeMenuItem("About...");
                    aboutItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        AboutWindow win2 = new AboutWindow();
                        await win2.ShowDialog2((MainWindow)win);
                    }, this);
                    aboutItem.CommandParameter = this;
                    aboutItem.Gesture = new KeyGesture(Key.H, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(aboutItem);
                }

                if (menuItems[i].Item1 == "Edit")
                {
                    if (subMenu.Menu.Items.Count > 0)
                    {
                        subMenu.Menu.Items.Add(new NativeMenuItemSeparator());
                    }

                    NativeMenuItem preferencesItem = new NativeMenuItem("Preferences...");
                    preferencesItem.Command = new SimpleCommand((win) => true, (win) =>
                    {
                        this.RibbonBar.SelectedIndex = 0;
                        this.GetFilePage<PreferencesPage>(out int ind);
                        this.RibbonFilePage.SelectedIndex = ind;
                    }, this);
                    preferencesItem.CommandParameter = this;
                    preferencesItem.Gesture = new KeyGesture(Key.R, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(preferencesItem);

                    NativeMenuItem managerItem = new NativeMenuItem("Module manager...");
                    managerItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        ModuleManagerWindow win2 = new ModuleManagerWindow();
                        await win2.ShowDialog2((MainWindow)win);
                    }, this);
                    managerItem.CommandParameter = this;
                    managerItem.Gesture = new KeyGesture(Key.M, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(managerItem);

                    NativeMenuItem creatorItem = new NativeMenuItem("Module creator...");
                    creatorItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        MessageBox box = new MessageBox("Attention", "The program will now be rebooted to open the module creator (we will do our best to recover the files that are currently open). Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog2((MainWindow)win);

                        if (box.Result == MessageBox.Results.Yes)
                        {
                            Program.Reboot(new string[] { "--module-creator" }, true);
                            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                        }
                    }, this);
                    creatorItem.CommandParameter = this;
                    creatorItem.Gesture = new KeyGesture(Key.M, Modules.ControlModifier | KeyModifiers.Shift);

                    subMenu.Menu.Items.Add(creatorItem);
                }

                menu.Items.Add(subMenu);
            }

            NativeMenu.SetMenu(this, menu);
        }
        public static void UpdateAttachmentLinks(Dictionary<string, object> parameters, InstanceStateData stateData)
        {
            foreach (KeyValuePair<string, object> parameter in parameters)
            {
                if (parameter.Value is string sr && sr.StartsWith("b0a73d1a-ff8a-4512-a481-9bccbba629bd://"))
                {
                    string attachmentName = sr.Substring(39);

                    if (stateData.Attachments.TryGetValue(attachmentName, out Attachment att))
                    {
                        parameters[parameter.Key] = att;
                    }
                    else
                    {
                        parameters[parameter.Key] = null;
                    }
                }
            }
        }

        public async void FileOpened(List<(string, Dictionary<string, object>)> suggestedModules, string path, ProgressWindow window = null)
        {
            if (GlobalSettings.Settings.DrawTreeWhenOpened && suggestedModules.Count == 2)
            {
                //Rectangular
                if (suggestedModules[1].Item1 == "68e25ec6-5911-4741-8547-317597e1b792")
                {
                    suggestedModules.Add(("e56b8297-4417-4494-9369-cbe9e5d25397", new Dictionary<string, object>()));
                }
                //Radial
                else if (suggestedModules[1].Item1 == "95b61284-b870-48b9-b51c-3276f7d89df1")
                {
                    suggestedModules.Add(("a99eb0c6-a69d-4785-961a-a0c247e9704d", new Dictionary<string, object>()));
                }
                //Circular
                else if (suggestedModules[1].Item1 == "92aac276-3af7-4506-a263-7220e0df5797")
                {
                    suggestedModules.Add(("1f3e0b88-c42d-417c-ba14-ba228be086a7", new Dictionary<string, object>()));
                }
            }

            AttributeSelectors = new List<ComboBox>();
            AttachmentSelectors = new List<ComboBox>();

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                if (suggestedModules[i].Item1 == "@Attachment")
                {
                    Attachment att = (Attachment)suggestedModules[i].Item2["Attachment"];
                    this.StateData.Attachments.Add(att.Name, att);
                }
            }

            BuildAttachmentList();

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                UpdateAttachmentLinks(suggestedModules[i].Item2, this.StateData);
            }

            List<string> transformerModules = (from el in Modules.TransformerModules select el.Name).ToList();
            BuildTransformerPanel(suggestedModules[0].Item1);
            UpdateTransformerParameters(suggestedModules[0].Item2);

            if (window == null)
            {
                window = new ProgressWindow
                {

                };

                _ = window.ShowDialog2(this);
            }

            await UpdateOnlyTransformedTree(window);

            BuildFurtherTransformationPanel();

            if (window == null)
            {
                window = new ProgressWindow
                {
                    ProgressText = "Loading further transformations...",
                    IsIndeterminate = false,
                    Progress = 0
                };

                _ = window.ShowDialog2(this);
            }
            else
            {
                window.ProgressText = "Loading further transformations...";
                window.IsIndeterminate = false;
                window.Progress = 0;
            }

            SemaphoreSlim semaphore2 = new SemaphoreSlim(0, 1);

            Thread thr = new Thread(async () =>
            {
                await UpdateOnlyFurtherTransformations(0, window);

                List<(FurtherTransformationModule, Dictionary<string, object>)> furtherTransformations = new List<(FurtherTransformationModule, Dictionary<string, object>)>();

                for (int i = 0; i < suggestedModules.Count; i++)
                {
                    FurtherTransformationModule mod = Modules.GetModule(Modules.FurtherTransformationModules, suggestedModules[i].Item1);
                    if (mod != null)
                    {
                        furtherTransformations.Add((mod, suggestedModules[i].Item2));
                    }
                }

                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.Steps = furtherTransformations.Count;
                });

                for (int i = 0; i < furtherTransformations.Count; i++)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        window.LabelText = furtherTransformations[i].Item1.Name;

                        try
                        {
                            Action<Dictionary<string, object>> updater = AddFurtherTransformation(furtherTransformations[i].Item1);
                            updater(furtherTransformations[i].Item2);
                        }
                        catch { }
                    });

                    await UpdateOnlyFurtherTransformations(FurtherTransformations.Count - 1, window, (prog) =>
                    {
                        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            window.Progress = (double)(i + prog) / furtherTransformations.Count;
                        });
                    });

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        window.Progress = (double)(i + 1) / furtherTransformations.Count;
                    });
                }

                semaphore2.Release();
            });
            thr.Start();

            await semaphore2.WaitAsync();
            semaphore2.Release();
            semaphore2.Dispose();

            List<string> coordinateModules = (from el in Modules.CoordinateModules select el.Name).ToList();
            BuildCoordinatesPanel(suggestedModules[1].Item1);
            UpdateCoordinatesParameters(suggestedModules[1].Item2);

            UpdateOnlyCoordinates();

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                if (suggestedModules[i].Item1 == "@Background")
                {
                    GraphBackground = (Colour)suggestedModules[i].Item2["Colour"];
                }
            }

            BuildPlottingPanel(GraphBackground);

            List<(PlottingModule, Dictionary<string, object>)> plotActionModules = new List<(PlottingModule, Dictionary<string, object>)>();

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                if (suggestedModules[i].Item1 != "@Background" && suggestedModules[i].Item1 != "@Attachment")
                {
                    PlottingModule mod = Modules.GetModule(Modules.PlottingModules, suggestedModules[i].Item1);
                    if (mod != null)
                    {
                        plotActionModules.Add((mod, suggestedModules[i].Item2));
                    }
                }
            }

            window.ProgressText = "Loading plot actions...";
            window.IsIndeterminate = false;
            window.Progress = 0;
            window.LabelText = " ";
            window.Steps = plotActionModules.Count;

            for (int i = 0; i < plotActionModules.Count; i++)
            {
                window.LabelText = plotActionModules[i].Item1.Name;

                await Task.Run(async () =>
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            Action<Dictionary<string, object>> updater = AddPlottingModule(plotActionModules[i].Item1);
                            updater(plotActionModules[i].Item2);
                        }
                        catch { }
                    });
                    await Task.Delay(10);
                });

                window.Progress = (double)(i + 1) / plotActionModules.Count;
            }

            window.Close();

            foreach ((string, Dictionary<string, object>) item in suggestedModules.Skip(2))
            {
                try
                {
                    Modules.GetModule(Modules.ActionModules, item.Item1)?.PerformAction(0, this, this.StateData);
                }
                catch { }
            }

            Avalonia.Media.Transformation.TransformOperations.Builder builder = new Avalonia.Media.Transformation.TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            Avalonia.Media.Transformation.TransformOperations offScreen = builder.Build();

            for (int i = 0; i < RibbonTabs.Length; i++)
            {
                if (RibbonTabs[i] != null)
                {
                    if (i != RibbonBar.SelectedIndex)
                    {
                        RibbonTabs[i].ZIndex = 0;
                        RibbonTabs[i].RenderTransform = offScreen;
                        RibbonTabs[i].Opacity = 0;
                        RibbonTabs[i].IsHitTestVisible = false;
                    }
                    else
                    {
                        RibbonTabs[i].ZIndex = 1;
                        RibbonTabs[i].RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity;
                        RibbonTabs[i].Opacity = 1;
                        RibbonTabs[i].IsHitTestVisible = true;
                    }
                }
            }

            foreach (List<RibbonButton> l in RibbonActionPanel.RibbonButtons)
            {
                foreach (RibbonButton b in l)
                {
                    b.IsEnabled = true;
                }
            }

            RibbonFilePage.Close();

            await UpdateAllPlotLayers();

            _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                Avalonia.Media.Imaging.RenderTargetBitmap bmp = FullPlotCanvas.RenderAtResolution(184, 184, new SkiaSharp.SKColor((byte)(this.StateData.GraphBackgroundColour.R * 255), (byte)(this.StateData.GraphBackgroundColour.G * 255), (byte)(this.StateData.GraphBackgroundColour.B * 255), (byte)(this.StateData.GraphBackgroundColour.A * 255)));

                RecentFile.Create(path, ref bmp).Save();
                AutoFit();

                StartPlotUpdaterThread();

                this.UndoStack.Clear();
                this.CanUndo = false;
            }, Avalonia.Threading.DispatcherPriority.MinValue);
        }

        private void UpdateControls(Dictionary<string, ControlStatus> controlStatus, Dictionary<string, Control> controls)
        {
            foreach (KeyValuePair<string, ControlStatus> kvp in controlStatus)
            {
                switch (kvp.Value)
                {
                    case ControlStatus.Disabled:
                        controls[kvp.Key].IsVisible = true;
                        controls[kvp.Key].IsEnabled = false;
                        break;
                    case ControlStatus.Enabled:
                        controls[kvp.Key].IsVisible = true;
                        controls[kvp.Key].IsEnabled = true;
                        break;
                    case ControlStatus.Hidden:
                        controls[kvp.Key].IsVisible = false;
                        controls[kvp.Key].IsEnabled = false;
                        break;
                }
            }

            foreach (KeyValuePair<string, Control> kvp in controls)
            {
                if (kvp.Value != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        kvp.Value.FindAncestorOfType<Accordion>()?.InvalidateHeight();
                    }, Avalonia.Threading.DispatcherPriority.MinValue);
                    break;
                }
            }
        }

        private void UpdateParameters(Dictionary<string, object> parametersToUpdate, Dictionary<string, Action<object>> parameterUpdaters)
        {
            foreach (KeyValuePair<string, object> kvp in parametersToUpdate)
            {
                if (parameterUpdaters.ContainsKey(kvp.Key) || kvp.Key == Modules.ModuleIDKey)
                {
                    parameterUpdaters[kvp.Key](kvp.Value);
                }
            }
        }

        public enum ModuleTarget
        {
            AllModules,
            ExcludeTransform,
            ExcludeFurtherTransformation
        }

        public string SerializeAllModules(ModuleTarget target, bool addSignature)
        {
            List<List<string[]>> allModules = new List<List<string[]>>();

            List<string[]> transformerModule = new List<string[]>();

            if (target == ModuleTarget.AllModules)
            {
                transformerModule.Add(new string[] { Modules.TransformerModules[TransformerComboBox.SelectedIndex].Id, SerializeParameters(TransformerParameters) });
            }

            allModules.Add(transformerModule);

            List<string[]> furtherTransformationModules = new List<string[]>();

            if (target == ModuleTarget.AllModules || target == ModuleTarget.ExcludeTransform)
            {
                for (int i = 0; i < FurtherTransformations.Count; i++)
                {
                    furtherTransformationModules.Add(new string[] { FurtherTransformations[i].Id, SerializeParameters(FurtherTransformationsParameters[i]) });
                }
            }

            allModules.Add(furtherTransformationModules);

            List<string[]> coordinatesModule = new List<string[]>() { new string[] { Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].Id, SerializeParameters(CoordinatesParameters) } };
            allModules.Add(coordinatesModule);

            List<string[]> plottingActionModules = new List<string[]>();
            plottingActionModules.Add(new string[] { "@Background", SerializeParameters(new Dictionary<string, object>() { { "Colour", this.StateData.GraphBackgroundColour } }) });
            for (int i = 0; i < PlottingActions.Count; i++)
            {
                plottingActionModules.Add(new string[] { PlottingActions[i].Id, SerializeParameters(PlottingParameters[i]) });
            }
            allModules.Add(plottingActionModules);

            if (!addSignature)
            {
                return System.Text.Json.JsonSerializer.Serialize(allModules, Modules.DefaultSerializationOptions);
            }
            else
            {
                string serializedModules = System.Text.Json.JsonSerializer.Serialize(allModules, Modules.DefaultSerializationOptions);

                string signature = CryptoUtils.SignString(serializedModules, CryptoUtils.FileRSAEncrypter);

                string publicKeySerialized = System.Text.Json.JsonSerializer.Serialize(new CryptoUtils.PublicKeyHolder(CryptoUtils.UserPublicKey), Modules.DefaultSerializationOptions);

                allModules.Insert(0, new List<string[]>
                {
                    new string[]
                    {
                        CryptoUtils.FileSignatureGuid,
                        signature,
                        publicKeySerialized
                    }
                });

                return System.Text.Json.JsonSerializer.Serialize(allModules, Modules.DefaultSerializationOptions);
            }
        }



        public static string SerializeParameters(Dictionary<string, object> parameters)
        {
            List<string[]> allParameters = new List<string[]>();

            foreach (KeyValuePair<string, object> kvp in parameters)
            {
                string[] parameter = new string[3];
                parameter[0] = kvp.Key;

                if (kvp.Value is bool valueBool)
                {
                    parameter[1] = "bool";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueBool, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is int valueInt)
                {
                    parameter[1] = "int";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueInt, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is string valueString)
                {
                    parameter[1] = "string";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueString, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is string[] valueStrings)
                {
                    parameter[1] = "string[]";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueStrings, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is double valueDouble)
                {
                    parameter[1] = "double";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueDouble, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is VectSharp.Font valueFont)
                {
                    parameter[1] = "font";

                    if (valueFont.FontFamily is AttachmentFontFamily aff)
                    {
                        parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize("attachment://" + aff.AttachmentName), System.Text.Json.JsonSerializer.Serialize(valueFont.FontSize) }, Modules.DefaultSerializationOptions);
                    }
                    else if (valueFont.FontFamily is WebFontFamily wff)
                    {
                        parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize("webfont://" + wff.FamilyName + "[" + wff.Style + "]"), System.Text.Json.JsonSerializer.Serialize(valueFont.FontSize) }, Modules.DefaultSerializationOptions);
                    }
                    else
                    {
                        parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize(valueFont.FontFamily.FileName), System.Text.Json.JsonSerializer.Serialize(valueFont.FontSize) }, Modules.DefaultSerializationOptions);
                    }
                }
                else if (kvp.Value is VectSharp.Point valuePoint)
                {
                    parameter[1] = "point";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new double[] { valuePoint.X, valuePoint.Y }, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is VectSharp.Colour valueColour)
                {
                    parameter[1] = "colour";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new double[] { valueColour.R, valueColour.G, valueColour.B, valueColour.A }, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is VectSharp.LineDash valueDash)
                {
                    parameter[1] = "dash";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new double[] { valueDash.UnitsOn, valueDash.UnitsOff, valueDash.Phase }, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is FormatterOptions valueFormatter)
                {
                    parameter[1] = "formatterOptions";
                    parameter[2] = SerializeList(valueFormatter.Parameters);
                }
                else if (kvp.Value is NumberFormatterOptions valueNumberFormatter)
                {
                    parameter[1] = "numberFormatterOptions";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize(valueNumberFormatter.AttributeName, Modules.DefaultSerializationOptions),
                        System.Text.Json.JsonSerializer.Serialize(valueNumberFormatter.AttributeType, Modules.DefaultSerializationOptions),
                        System.Text.Json.JsonSerializer.Serialize(valueNumberFormatter.DefaultValue, Modules.DefaultSerializationOptions),
                        SerializeList(valueNumberFormatter.Parameters) }, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is ColourFormatterOptions valueColourFormatter)
                {
                    parameter[1] = "colourFormatterOptions";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize(valueColourFormatter.AttributeName, Modules.DefaultSerializationOptions),
                        System.Text.Json.JsonSerializer.Serialize(valueColourFormatter.AttributeType, Modules.DefaultSerializationOptions),
                        System.Text.Json.JsonSerializer.Serialize(new double[] { valueColourFormatter.DefaultColour.R, valueColourFormatter.DefaultColour.G, valueColourFormatter.DefaultColour.B, valueColourFormatter.DefaultColour.A }, Modules.DefaultSerializationOptions),
                        SerializeList(valueColourFormatter.Parameters) }, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is CompiledCode valueCompiledCode)
                {
                    parameter[1] = "compiledCode";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(valueCompiledCode.SourceCode, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value is Attachment attachment)
                {
                    parameter[1] = "attachment";
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(attachment.Name, Modules.DefaultSerializationOptions);
                }
                else if (kvp.Value == null)
                {
                    parameter[1] = "null";
                    parameter[2] = "null";
                }

                allParameters.Add(parameter);
            }

            return System.Text.Json.JsonSerializer.Serialize(allParameters, Modules.DefaultSerializationOptions);
        }

        private static string SerializeList(IEnumerable<object> objects)
        {
            List<string[]> allObjects = new List<string[]>();

            foreach (object obj in objects)
            {
                string[] currObject = new string[2];
                if (obj is bool valueBool)
                {
                    currObject[0] = "bool";
                    currObject[1] = System.Text.Json.JsonSerializer.Serialize(valueBool, Modules.DefaultSerializationOptions);
                }
                else if (obj is int valueInt)
                {
                    currObject[0] = "int";
                    currObject[1] = System.Text.Json.JsonSerializer.Serialize(valueInt, Modules.DefaultSerializationOptions);
                }
                else if (obj is string valueString)
                {
                    currObject[0] = "string";
                    currObject[1] = System.Text.Json.JsonSerializer.Serialize(valueString, Modules.DefaultSerializationOptions);
                }
                else if (obj is double valueDouble)
                {
                    currObject[0] = "double";
                    if (!double.IsNaN(valueDouble) && !double.IsInfinity(valueDouble))
                    {
                        currObject[1] = System.Text.Json.JsonSerializer.Serialize(valueDouble, Modules.DefaultSerializationOptions);
                    }
                    else
                    {
                        currObject[1] = valueDouble.ToString();
                    }

                }
                else if (obj is Gradient valueGradient)
                {
                    currObject[0] = "gradient";
                    currObject[1] = valueGradient.SerializeJson();
                }

                allObjects.Add(currObject);
            }

            return System.Text.Json.JsonSerializer.Serialize(allObjects, Modules.DefaultSerializationOptions);
        }

        private void BuildTransformerPanel(string suggestedModuleId)
        {
            this.FindControl<StackPanel>("TransformerModuleContainerPanel").Children.Add(new TextBlock() { Text = "Transformer module", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

            Grid transformerPanel = new Grid() { Margin = new Thickness(5, 5, 0, 5) };
            transformerPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star) };

            transformerPanel.Children.Add(new TextBlock() { Text = "Choose module:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            TransformerAlert = GetAlertIcon();
            TransformerAlert.Width = 16;
            TransformerAlert.Height = 16;
            TransformerAlert.Margin = new Thickness(0, 0, 5, 0);
            TransformerAlert.IsVisible = false;
            Grid.SetColumn(TransformerAlert, 1);

            transformerPanel.Children.Add(TransformerAlert);

            int moduleIndex = Math.Max(0, Modules.TransformerModules.IndexOf(Modules.GetModule(Modules.TransformerModules, suggestedModuleId)));

            List<string> transformerModules = (from el in Modules.TransformerModules select el.Name).ToList();
            TransformerComboBox = new PreviewComboBox() { Margin = new Thickness(5, 0, 0, 0), Items = transformerModules, SelectedIndex = moduleIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
            Grid.SetColumn(TransformerComboBox, 2);
            transformerPanel.Children.Add(TransformerComboBox);

            TransformerComboBox.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            this.FindControl<StackPanel>("TransformerModuleContainerPanel").Children.Add(transformerPanel);

            Grid parametersHeaderPanel = new Grid() { Margin = new Thickness(0, 0, 0, 0) };
            parametersHeaderPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel) };

            parametersHeaderPanel.Children.Add(new TextBlock() { Text = "Parameters", FontWeight = FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 1);
            parametersHeaderPanel.Children.Add(helpButton);

            AvaloniaBugFixes.SetToolTip(helpButton, Modules.TransformerModules[TransformerComboBox.SelectedIndex].HelpText);

            helpButton.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            helpButton.Click += (s, e) =>
            {
                e.Handled = true;

                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.TransformerModules[TransformerComboBox.SelectedIndex].Id].BuildReadmeMarkdown(), Modules.TransformerModules[TransformerComboBox.SelectedIndex].Id);

                win.Show(this);
            };

            Accordion exp = new Accordion() { Margin = new Thickness(5, 0, 0, 0), ArrowSize = 10 };

            exp.AccordionHeader = parametersHeaderPanel;



            List<(string, string)> transformerParameters = Modules.TransformerModules[moduleIndex].GetParameters(Trees);

            transformerParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate transformerParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                return Modules.TransformerModules[TransformerComboBox.SelectedIndex].OnParameterChange(Trees, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
            };

            TransformerComboBox.PreviewSelectionChanged += (s, e) =>
            {
                if (!SettingTransformerModule)
                {
                    PushUndoFrame(UndoFrameLevel.TransformerModule, 0);
                }
            };

            TransformerComboBox.SelectionChanged += async (s, e) =>
            {
                AvaloniaBugFixes.SetToolTip(helpButton, Modules.TransformerModules[TransformerComboBox.SelectedIndex].HelpText);
                List<(string, string)> parameters = Modules.TransformerModules[TransformerComboBox.SelectedIndex].GetParameters(Trees);
                parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));
                TransformerParameters = UpdateParameterPanel(transformerParameterChange, parameters, async () => { await UpdateTransformedTree(); }, out UpdateTransformerParameters, out Control content);
                exp.AccordionContent = content;
                await UpdateTransformedTree();
            };

            TransformerParameters = UpdateParameterPanel(transformerParameterChange, transformerParameters, async () => { await UpdateTransformedTree(); }, out UpdateTransformerParameters, out Control content);
            exp.AccordionContent = content;

            this.FindControl<StackPanel>("TransformerModuleContainerPanel").Children.Add(exp);
        }


        private void BuildFurtherTransformationPanel()
        {
            this.FindControl<StackPanel>("FurtherTransformationsContainerPanel").Children.Add(new TextBlock() { Text = "Further transformations", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

            FurtherTransformations = new List<FurtherTransformationModule>();
            FurtherTransformationsAlerts = new List<Control>();
            FurtherTransformationsParameters = new List<Dictionary<string, object>>();
            UpdateFurtherTransformationParameters = new List<Action<Dictionary<string, object>>>();

            FurtherTransformationsContainer = new StackPanel();
            this.FindControl<StackPanel>("FurtherTransformationsContainerPanel").Children.Add(FurtherTransformationsContainer);

            Button addModuleButton = new Button() { Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Padding = new Thickness(5, 3, 5, 3), RenderTransform = null, Margin = new Thickness(5, 0, 0, 5) };
            addModuleButton.Classes.Add("SideBarButton");

            StackPanel addButtonContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            Avalonia.Controls.Shapes.Path addPlottingModule = new Avalonia.Controls.Shapes.Path() { Width = 8, Height = 8, Data = Geometry.Parse("M 4,0 L4,8 M0,4 L8,4"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, StrokeThickness = 2 };

            addModuleButton.Click += async (s, e) =>
            {
                AddFurtherTransformationModuleWindow win = new AddFurtherTransformationModuleWindow(FurtherTransformations);

                await win.ShowDialog2(this);

                if (win.Result != null)
                {
                    PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, this.FurtherTransformations.Count);

                    List<string> childNames = null;

                    if (this.IsSelectionAvailable)
                    {
                        childNames = this.SelectedNode.GetNodeNames();
                    }

                    AddFurtherTransformation(win.Result);
                    await UpdateFurtherTransformations(FurtherTransformations.Count - 1);

                    if (childNames?.Count > 0)
                    {
                        TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                        if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                        {
                            this.SetSelection(candidate);
                        }
                        else
                        {
                            this.SetSelection(null);
                        }
                    }
                    else
                    {
                        this.SetSelection(null);
                    }
                }
            };

            addButtonContainer.Children.Add(addPlottingModule);
            addButtonContainer.Children.Add(new TextBlock() { Text = "Add module", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), FontSize = 13 });

            addModuleButton.Content = addButtonContainer;

            this.FindControl<StackPanel>("FurtherTransformationsContainerPanel").Children.Add(addModuleButton);
        }

        public Action<Dictionary<string, object>> AddFurtherTransformation(FurtherTransformationModule module)
        {
            FurtherTransformations.Add(module);

            Accordion exp = new Accordion() { Margin = new Thickness(5, 0, 0, 5), ArrowSize = 10 };

            Grid modulePanel = new Grid() { Margin = new Thickness(0, 0, 0, 0) };
            modulePanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(GlobalSettings.Settings.ShowLegacyUpDownArrows ? 24 : 0, GridUnitType.Pixel), new ColumnDefinition(GlobalSettings.Settings.ShowLegacyUpDownArrows ? 24 : 0, GridUnitType.Pixel), new ColumnDefinition(0, GridUnitType.Auto) };

            Control alert = GetAlertIcon();
            alert.Width = 16;
            alert.Height = 16;
            FurtherTransformationsAlerts.Add(alert);
            alert.Margin = new Thickness(0, 0, 5, 0);
            alert.IsVisible = false;

            modulePanel.Children.Add(alert);

            DPIAwareBox icon = new DPIAwareBox(module.GetIcon) { Width = 16, Height = 16, Margin = new Thickness(0, 0, 5, 0) };
            Grid.SetColumn(icon, 1);
            modulePanel.Children.Add(icon);
            AvaloniaBugFixes.SetToolTip(icon, module.Name);

            FillingControl<TrimmedTextBox2> moduleName = new FillingControl<TrimmedTextBox2>(new TrimmedTextBox2() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = module.Name, FontWeight = FontWeight.Bold, FontSize = 13 }, 5) { Margin = new Thickness(-5, 0, -5, 0) };
            AvaloniaBugFixes.SetToolTip(moduleName, module.Name);

            Grid.SetColumn(moduleName, 2);

            modulePanel.Children.Add(moduleName);

            AddRemoveButton moveUp = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Up };
            Grid.SetColumn(moveUp, 4);
            modulePanel.Children.Add(moveUp);
            if (FurtherTransformations.Count <= 1)
            {
                moveUp.IsVisible = false;
            }

            moveUp.Click += async (s, e) =>
            {
                e.Handled = true;

                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                if (index > 0)
                {
                    List<string> childNames = null;

                    if (this.IsSelectionAvailable)
                    {
                        childNames = this.SelectedNode.GetNodeNames();
                    }

                    this.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, index - 1);

                    FurtherTransformationModule mod = FurtherTransformations[index];
                    FurtherTransformations.RemoveAt(index);
                    FurtherTransformations.Insert(index - 1, mod);

                    Dictionary<string, object> param = FurtherTransformationsParameters[index];
                    FurtherTransformationsParameters.RemoveAt(index);
                    FurtherTransformationsParameters.Insert(index - 1, param);


                    Action<Dictionary<string, object>> paramUpd = UpdateFurtherTransformationParameters[index];
                    UpdateFurtherTransformationParameters.RemoveAt(index);
                    UpdateFurtherTransformationParameters.Insert(index - 1, paramUpd);

                    IControl pan = FurtherTransformationsContainer.Children[index];
                    FurtherTransformationsContainer.Children.RemoveAt(index);
                    FurtherTransformationsContainer.Children.Insert(index - 1, pan);

                    Control alert = FurtherTransformationsAlerts[index];
                    FurtherTransformationsAlerts.RemoveAt(index);
                    FurtherTransformationsAlerts.Insert(index - 1, alert);

                    for (int i = 0; i < FurtherTransformationsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[3].IsVisible = true;
                        }

                        if (i == FurtherTransformationsContainer.Children.Count - 1)
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = true;
                        }
                    }

                    await UpdateFurtherTransformations(index - 1);

                    if (childNames?.Count > 0)
                    {
                        TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                        if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                        {
                            this.SetSelection(candidate);
                        }
                        else
                        {
                            this.SetSelection(null);
                        }
                    }
                    else
                    {
                        this.SetSelection(null);
                    }
                }
            };

            AddRemoveButton moveDown = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Down };
            Grid.SetColumn(moveDown, 5);
            modulePanel.Children.Add(moveDown);
            moveDown.IsVisible = false;

            moveDown.Click += async (s, e) =>
            {
                e.Handled = true;

                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                if (index < FurtherTransformationsContainer.Children.Count - 1)
                {
                    List<string> childNames = null;

                    if (this.IsSelectionAvailable)
                    {
                        childNames = this.SelectedNode.GetNodeNames();
                    }

                    this.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, index);

                    FurtherTransformationModule mod = FurtherTransformations[index];
                    FurtherTransformations.RemoveAt(index);
                    FurtherTransformations.Insert(index + 1, mod);

                    Dictionary<string, object> param = FurtherTransformationsParameters[index];
                    FurtherTransformationsParameters.RemoveAt(index);
                    FurtherTransformationsParameters.Insert(index + 1, param);

                    IControl pan = FurtherTransformationsContainer.Children[index];
                    FurtherTransformationsContainer.Children.RemoveAt(index);
                    FurtherTransformationsContainer.Children.Insert(index + 1, pan);

                    Action<Dictionary<string, object>> paramUpd = UpdateFurtherTransformationParameters[index];
                    UpdateFurtherTransformationParameters.RemoveAt(index);
                    UpdateFurtherTransformationParameters.Insert(index + 1, paramUpd);

                    Control alert = FurtherTransformationsAlerts[index];
                    FurtherTransformationsAlerts.RemoveAt(index);
                    FurtherTransformationsAlerts.Insert(index + 1, alert);

                    for (int i = 0; i < FurtherTransformationsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[3].IsVisible = true;
                        }

                        if (i == FurtherTransformationsContainer.Children.Count - 1)
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)FurtherTransformationsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = true;
                        }
                    }

                    await UpdateFurtherTransformations(index);

                    if (childNames?.Count > 0)
                    {
                        TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                        if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                        {
                            this.SetSelection(candidate);
                        }
                        else
                        {
                            this.SetSelection(null);
                        }
                    }
                    else
                    {
                        this.SetSelection(null);
                    }
                }
            };

            Button remove = new Button() { Margin = new Thickness(0, 0, 4, 0), Width = 16, Height = 16, Background = Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 8, Height = 8, Data = Geometry.Parse("M0,0 L8,8 M8,0 L0,8"), StrokeThickness = 2, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center } };
            remove.Classes.Add("SideBarButton");
            AvaloniaBugFixes.SetToolTip(remove, new TextBlock() { Text = "Remove", Foreground = Brushes.Black });

            Grid.SetColumn(remove, 6);
            modulePanel.Children.Add(remove);

            remove.Click += async (s, e) =>
            {
                e.Handled = true;

                List<string> childNames = null;

                if (this.IsSelectionAvailable)
                {
                    childNames = this.SelectedNode.GetNodeNames();
                }

                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                this.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, index);

                FurtherTransformations.RemoveAt(index);
                FurtherTransformationsParameters.RemoveAt(index);
                UpdateFurtherTransformationParameters.RemoveAt(index);
                FurtherTransformationsContainer.Children.RemoveAt(index);
                FurtherTransformationsAlerts.RemoveAt(index);

                if (FurtherTransformations.Count > 0)
                {
                    ((Grid)((Accordion)FurtherTransformationsContainer.Children[0]).AccordionHeader).Children[3].IsVisible = false;
                    ((Grid)((Accordion)FurtherTransformationsContainer.Children.Last()).AccordionHeader).Children[4].IsVisible = false;
                }

                await UpdateFurtherTransformations(index);

                if (childNames?.Count > 0)
                {
                    TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                    if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                    {
                        this.SetSelection(candidate);
                    }
                    else
                    {
                        this.SetSelection(null);
                    }
                }
                else
                {
                    this.SetSelection(null);
                }
            };

            remove.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            moveUp.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            moveDown.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 3);
            modulePanel.Children.Add(helpButton);

            AvaloniaBugFixes.SetToolTip(helpButton, module.HelpText);

            helpButton.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            helpButton.Click += (s, e) =>
            {
                e.Handled = true;

                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[module.Id].BuildReadmeMarkdown(), module.Id);

                win.Show(this);
            };

            exp.AccordionHeader = modulePanel;

            PointerPressedEventArgs pressEventArgs = null;

            Avalonia.Threading.DispatcherTimer pressTimer = new Avalonia.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(GlobalSettings.Settings.DragInterval) };

            if (!GlobalSettings.Settings.ShowLegacyUpDownArrows)
            {
                moveUp.Width = 0;
                moveDown.Width = 0;
                moveUp.Height = 0;
                moveDown.Height = 0;
                moveUp.Opacity = 0;
                moveDown.Opacity = 0;

                pressTimer.Tick += (s, e) =>
                {
                    pressTimer.Stop();

                    if (pressEventArgs != null)
                    {
                        pressEventArgs.Pointer.Capture(exp);
                        pressEventArgs.Handled = true;

                        if (exp.IsOpen)
                        {
                            exp.IsOpen = false;
                        }

                        Func<PointerReleasedEventArgs, (int, int)> act = null;

                        async void StopDrag(object sender, PointerReleasedEventArgs e)
                        {
                            exp.PointerReleased -= StopDrag;

                            if (act != null)
                            {
                                (int oldIndex, int newIndex) = act.Invoke(e);

                                if (oldIndex != newIndex)
                                {
                                    this.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, Math.Min(oldIndex, newIndex));

                                    List<string> childNames = null;

                                    if (this.IsSelectionAvailable)
                                    {
                                        childNames = this.SelectedNode.GetNodeNames();
                                    }

                                    FurtherTransformationModule mod = FurtherTransformations[oldIndex];
                                    FurtherTransformations.RemoveAt(oldIndex);
                                    FurtherTransformations.Insert(newIndex, mod);

                                    Dictionary<string, object> param = FurtherTransformationsParameters[oldIndex];
                                    FurtherTransformationsParameters.RemoveAt(oldIndex);
                                    FurtherTransformationsParameters.Insert(newIndex, param);


                                    Action<Dictionary<string, object>> paramUpd = UpdateFurtherTransformationParameters[oldIndex];
                                    UpdateFurtherTransformationParameters.RemoveAt(oldIndex);
                                    UpdateFurtherTransformationParameters.Insert(newIndex, paramUpd);

                                    Control alert = FurtherTransformationsAlerts[oldIndex];
                                    FurtherTransformationsAlerts.RemoveAt(oldIndex);
                                    FurtherTransformationsAlerts.Insert(newIndex, alert);

                                    await UpdateFurtherTransformations(Math.Min(newIndex, oldIndex));

                                    if (childNames?.Count > 0)
                                    {
                                        TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                                        if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                                        {
                                            this.SetSelection(candidate);
                                        }
                                        else
                                        {
                                            this.SetSelection(null);
                                        }
                                    }
                                    else
                                    {
                                        this.SetSelection(null);
                                    }
                                }
                            }
                        }

                        StartDrag(exp, FurtherTransformationsContainer, ref act, pressEventArgs);

                        exp.PointerReleased += StopDrag;
                    }
                };

                exp.PointerPressed += (s, e) =>
                {
                    if (!helpButton.IsPointerOver && !remove.IsPointerOver && FurtherTransformations.Count > 1)
                    {
                        if (exp.FindControl<Grid>("HeaderGrid").IsPointerOver)
                        {
                            pressEventArgs = e;
                            pressTimer.Start();
                        }
                    }
                };

                exp.PointerReleased += (s, e) =>
                {
                    pressEventArgs = null;
                    pressTimer.Stop();
                };
            }



            List<(string, string)> moduleParameters = module.GetParameters(TransformedTree);
            moduleParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate plottingParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);
                if (index > 0)
                {
                    return module.OnParameterChange(AllTransformedTrees[index - 1], previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                }
                else
                {
                    return module.OnParameterChange(FirstTransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                }
            };

            FurtherTransformationsParameters.Add(UpdateParameterPanel(plottingParameterChange, moduleParameters, async () =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);
                await UpdateFurtherTransformations(index);
            }, out Action<Dictionary<string, object>> tbr, out Control content));
            exp.AccordionContent = content;
            UpdateFurtherTransformationParameters.Add(tbr);

            FurtherTransformationsContainer.Children.Add(exp);

            if (FurtherTransformations.Count > 1)
            {
                ((Grid)((Accordion)FurtherTransformationsContainer.Children[0]).AccordionHeader).Children[4].IsVisible = true;
            }

            if (FurtherTransformations.Count > 2)
            {
                ((Grid)((Accordion)FurtherTransformationsContainer.Children[FurtherTransformationsContainer.Children.Count - 2]).AccordionHeader).Children[4].IsVisible = true;
            }

            return tbr;
        }

        public void RemoveFurtherTransformation(int index)
        {
            List<string> childNames = null;

            if (this.IsSelectionAvailable)
            {
                childNames = this.SelectedNode.GetNodeNames();
            }

            FurtherTransformations.RemoveAt(index);
            FurtherTransformationsParameters.RemoveAt(index);
            UpdateFurtherTransformationParameters.RemoveAt(index);
            FurtherTransformationsContainer.Children.RemoveAt(index);
            FurtherTransformationsAlerts.RemoveAt(index);

            if (FurtherTransformations.Count > 0)
            {
                ((Grid)((Accordion)FurtherTransformationsContainer.Children[0]).AccordionHeader).Children[3].IsVisible = false;
                ((Grid)((Accordion)FurtherTransformationsContainer.Children.Last()).AccordionHeader).Children[4].IsVisible = false;
            }

            if (childNames?.Count > 0)
            {
                TreeNode candidate = this.TransformedTree.GetLastCommonAncestor(childNames);
                if (candidate != null && candidate.GetNodeNames().Count == childNames.Count)
                {
                    this.SetSelection(candidate);
                }
                else
                {
                    this.SetSelection(null);
                }
            }
            else
            {
                this.SetSelection(null);
            }
        }

        private void BuildCoordinatesPanel(string suggestedModuleId)
        {
            Grid panelHeader = new Grid();
            panelHeader.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            panelHeader.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            panelHeader.Children.Add(new TextBlock() { Text = "Coordinates module", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

            StackPanel refreshAllButtonContents = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            Canvas refreshButtonIcon = new Canvas() { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            DPIAwareBox blurIcon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.RefreshGrey")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            blurIcon.Classes.Add("BlurIcon");

            DPIAwareBox hoverIcon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Refresh")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            hoverIcon.Classes.Add("HoverIcon");

            refreshButtonIcon.Children.Add(blurIcon);
            refreshButtonIcon.Children.Add(hoverIcon);

            refreshAllButtonContents.Children.Add(refreshButtonIcon);
            refreshAllButtonContents.Children.Add(new TextBlock() { Text = "Reset", Margin = new Thickness(5, 0, 0, 0) });

            ResetCoordinatesButton = new Button() { Content = refreshAllButtonContents, Background = Brushes.Transparent, Padding = new Thickness(5, 2, 5, 2) };
            Grid.SetColumn(ResetCoordinatesButton, 1);
            ResetCoordinatesButton.Classes.Add("SideBarButton");

            Style hoverIconStyle = new Style(x => x.Class("HoverIcon"));
            hoverIconStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, false));
            ResetCoordinatesButton.Styles.Add(hoverIconStyle);

            Style hoverIconHoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover").Descendant().Class("HoverIcon"));
            hoverIconHoverStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, true));
            ResetCoordinatesButton.Styles.Add(hoverIconHoverStyle);

            Style blurIconHoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover").Descendant().Class("BlurIcon"));
            blurIconHoverStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, false));
            ResetCoordinatesButton.Styles.Add(blurIconHoverStyle);

            Style disabledBackgroundStyle = new Style(x => x.OfType<Button>().Class(":disabled").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>());
            disabledBackgroundStyle.Setters.Add(new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, Brushes.Transparent));
            disabledBackgroundStyle.Setters.Add(new Setter(Avalonia.Controls.Presenters.ContentPresenter.OpacityProperty, 0.5));
            ResetCoordinatesButton.Styles.Add(disabledBackgroundStyle);

            ResetCoordinatesButton.Click += async (s, e) =>
            {
                await ResetDefaultCoordinateModuleParameters();
            };

            panelHeader.Children.Add(ResetCoordinatesButton);

            this.FindControl<StackPanel>("CoordinatesModuleContainerPanel").Children.Add(panelHeader);

            Grid coordinatesPanel = new Grid() { Margin = new Thickness(5, 5, 0, 5) };
            coordinatesPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star) };

            coordinatesPanel.Children.Add(new TextBlock() { Text = "Choose module:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            CoordinatesAlert = GetAlertIcon();
            CoordinatesAlert.Width = 16;
            CoordinatesAlert.Height = 16;
            CoordinatesAlert.Margin = new Thickness(0, 0, 5, 0);
            CoordinatesAlert.IsVisible = false;
            Grid.SetColumn(CoordinatesAlert, 1);

            coordinatesPanel.Children.Add(CoordinatesAlert);

            int moduleIndex = Math.Max(0, Modules.CoordinateModules.IndexOf(Modules.GetModule(Modules.CoordinateModules, suggestedModuleId)));

            List<string> coordinateModules = (from el in Modules.CoordinateModules select el.Name).ToList();
            //CoordinatesComboBox = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Items = coordinateModules, SelectedIndex = moduleIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
            CoordinatesComboBox = new PreviewComboBox() { Margin = new Thickness(5, 0, 0, 0), Items = coordinateModules, SelectedIndex = moduleIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
            Grid.SetColumn(CoordinatesComboBox, 2);
            coordinatesPanel.Children.Add(CoordinatesComboBox);

            CoordinatesComboBox.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            this.FindControl<StackPanel>("CoordinatesModuleContainerPanel").Children.Add(coordinatesPanel);

            Accordion exp = new Accordion() { Margin = new Thickness(5, 0, 0, 0), ArrowSize = 10 };

            Grid parametersHeaderPanel = new Grid() { Margin = new Thickness(0, 0, 0, 0) };
            parametersHeaderPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel) };

            parametersHeaderPanel.Children.Add(new TextBlock() { Text = "Parameters", FontWeight = FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 1);
            parametersHeaderPanel.Children.Add(helpButton);

            AvaloniaBugFixes.SetToolTip(helpButton, Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].HelpText);

            helpButton.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            helpButton.Click += (s, e) =>
            {
                e.Handled = true;

                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].Id].BuildReadmeMarkdown(), Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].Id);

                win.Show(this);
            };

            exp.AccordionHeader = parametersHeaderPanel;

            List<(string, string)> coordinateParameters = Modules.CoordinateModules[moduleIndex].GetParameters(TransformedTree);
            coordinateParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate coordinateParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                return Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].OnParameterChange(TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
            };

            CoordinatesComboBox.PreviewSelectionChanged += (s, e) =>
            {
                if (!SettingCoordinatesModule)
                {
                    PushUndoFrame(UndoFrameLevel.CoordinatesModule, 0);
                }
            };

            CoordinatesComboBox.SelectionChanged += async (s, e) =>
            {
                await ResetDefaultCoordinateModuleParameters();
            };

            resetDefaultCoordinateModuleParameters = async () =>
            {
                AvaloniaBugFixes.SetToolTip(helpButton, Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].HelpText);
                List<(string, string)> parameters = Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].GetParameters(TransformedTree);
                parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));
                CoordinatesParameters = UpdateParameterPanel(coordinateParameterChange, parameters, async () => await UpdateCoordinates(), out UpdateCoordinatesParameters, out Control content);
                exp.AccordionContent = content;
                await UpdateCoordinates();
            };

            CoordinatesParameters = UpdateParameterPanel(coordinateParameterChange, coordinateParameters, async () => await UpdateCoordinates(), out UpdateCoordinatesParameters, out Control content);
            exp.AccordionContent = content;

            this.FindControl<StackPanel>("CoordinatesModuleContainerPanel").Children.Add(exp);
        }

        private Func<Task> resetDefaultCoordinateModuleParameters;

        public async Task ResetDefaultCoordinateModuleParameters()
        {
            ResetCoordinatesButton.IsEnabled = false;
            await resetDefaultCoordinateModuleParameters();
            ResetCoordinatesButton.IsEnabled = true;
        }

        bool SettingCoordinatesModule = false;

        public Action<Dictionary<string, object>> SetCoordinateModule(CoordinateModule module)
        {
            int index = Modules.CoordinateModules.IndexOf(module);

            if (index != CoordinatesComboBox.SelectedIndex)
            {
                SettingCoordinatesModule = true;
                CoordinatesComboBox.SelectedIndex = index;
                SettingCoordinatesModule = false;
                return UpdateCoordinatesParameters;
            }
            else
            {
                return async arg => { UpdateCoordinatesParameters(arg); await UpdateCoordinates(); };
            }
        }

        bool SettingTransformerModule = false;

        public Action<Dictionary<string, object>> SetTransformerModule(TransformerModule module)
        {
            int index = Modules.TransformerModules.IndexOf(module);

            if (index != TransformerComboBox.SelectedIndex)
            {
                SettingTransformerModule = true;
                TransformerComboBox.SelectedIndex = index;
                SettingTransformerModule = false;
                return UpdateTransformerParameters;
            }
            else
            {
                return async arg => { UpdateTransformerParameters(arg); await UpdateTransformedTree(); };
            }
        }

        private Button RefreshAllButton;
        private Button ResetCoordinatesButton;

        private void BuildPlottingPanel(Colour backgroundColour)
        {
            Grid panelHeader = new Grid();
            panelHeader.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            panelHeader.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            panelHeader.Children.Add(new TextBlock() { Text = "Plot elements", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

            StackPanel refreshAllButtonContents = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            Canvas refreshButtonIcon = new Canvas() { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            DPIAwareBox blurIcon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.RefreshGrey")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            blurIcon.Classes.Add("BlurIcon");

            DPIAwareBox hoverIcon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Refresh")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            hoverIcon.Classes.Add("HoverIcon");

            refreshButtonIcon.Children.Add(blurIcon);
            refreshButtonIcon.Children.Add(hoverIcon);

            refreshAllButtonContents.Children.Add(refreshButtonIcon);
            refreshAllButtonContents.Children.Add(new TextBlock() { Text = "Redraw all", Margin = new Thickness(5, 0, 0, 0) });

            RefreshAllButton = new Button() { Content = refreshAllButtonContents, Background = Brushes.Transparent, Padding = new Thickness(5, 2, 5, 2) };
            Grid.SetColumn(RefreshAllButton, 1);
            RefreshAllButton.Classes.Add("SideBarButton");

            Style hoverIconStyle = new Style(x => x.Class("HoverIcon"));
            hoverIconStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, false));
            RefreshAllButton.Styles.Add(hoverIconStyle);

            Style hoverIconHoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover").Descendant().Class("HoverIcon"));
            hoverIconHoverStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, true));
            RefreshAllButton.Styles.Add(hoverIconHoverStyle);

            Style blurIconHoverStyle = new Style(x => x.OfType<Button>().Class(":pointerover").Descendant().Class("BlurIcon"));
            blurIconHoverStyle.Setters.Add(new Setter(DPIAwareBox.IsVisibleProperty, false));
            RefreshAllButton.Styles.Add(blurIconHoverStyle);

            Style disabledBackgroundStyle = new Style(x => x.OfType<Button>().Class(":disabled").Template().OfType<Avalonia.Controls.Presenters.ContentPresenter>());
            disabledBackgroundStyle.Setters.Add(new Setter(Avalonia.Controls.Presenters.ContentPresenter.BackgroundProperty, Brushes.Transparent));
            disabledBackgroundStyle.Setters.Add(new Setter(Avalonia.Controls.Presenters.ContentPresenter.OpacityProperty, 0.5));
            RefreshAllButton.Styles.Add(disabledBackgroundStyle);

            RefreshAllButton.Click += async (s, e) =>
            {
                await UpdateAllPlotLayers();
            };

            panelHeader.Children.Add(RefreshAllButton);

            this.FindControl<StackPanel>("PlotActionsContainerPanel").Children.Add(panelHeader);

            Grid plotElementsGrid = new Grid() { Margin = new Thickness(5, 0, 0, 5) };
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            {
                TextBlock blk = new TextBlock() { Text = "Background: ", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), FontSize = 13 };
                plotElementsGrid.Children.Add(blk);

                GraphBackgroundButton = new ColorButton() { Color = backgroundColour.ToAvalonia(), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontFamily = this.FontFamily, FontSize = this.FontSize };
                GraphBackgroundButton.Classes.Add("PlainButton");
                Grid.SetColumn(GraphBackgroundButton, 1);
                plotElementsGrid.Children.Add(GraphBackgroundButton);

                GraphBackgroundButton.PropertyChanged += (s, e) =>
                {
                    if (e.Property == ColorButton.ColorProperty)
                    {
                        if (!SettingGraphBackground)
                        {
                            PushUndoFrame(UndoFrameLevel.PlotActionModule, -1);
                            GraphBackground = GraphBackgroundButton.Color.ToVectSharp();
                        }
                    }
                };
            }

            this.FindControl<StackPanel>("PlotActionsContainerPanel").Children.Add(plotElementsGrid);

            PlottingActions = new List<PlottingModule>();
            PlottingAlerts = new List<Control>();
            PlottingParameters = new List<Dictionary<string, object>>();
            UpdatePlottingParameters = new List<Action<Dictionary<string, object>>>();

            PlottingActionsContainer = new StackPanel();
            this.FindControl<StackPanel>("PlotActionsContainerPanel").Children.Add(PlottingActionsContainer);

            Button addModuleButton = new Button() { Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Padding = new Thickness(5, 3, 5, 3), RenderTransform = null, Margin = new Thickness(5, 0, 0, 5) };
            addModuleButton.Classes.Add("SideBarButton");

            StackPanel addButtonContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            Avalonia.Controls.Shapes.Path addPlottingModule = new Avalonia.Controls.Shapes.Path() { Width = 8, Height = 8, Data = Geometry.Parse("M 4,0 L4,8 M0,4 L8,4"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, StrokeThickness = 2 };

            addModuleButton.Click += async (s, e) =>
            {
                AddPlottingModuleWindow win = new AddPlottingModuleWindow();

                await win.ShowDialog2(this);

                if (win.Result != null)
                {
                    PushUndoFrame(UndoFrameLevel.PlotActionModule, this.PlottingActions.Count);

                    AddPlottingModule(win.Result);

                    PlotCanvases.Add(null);
                    PlotBounds.Add((new VectSharp.Point(), new VectSharp.Point()));
                    SelectionCanvases.Add(null);
                    LayerTransforms.Add(null);

                    await ActuallyUpdatePlotLayer(PlotCanvases.Count - 1, false);
                }
            };

            addButtonContainer.Children.Add(addPlottingModule);
            addButtonContainer.Children.Add(new TextBlock() { Text = "Add module", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), FontSize = 13 });

            addModuleButton.Content = addButtonContainer;

            this.FindControl<StackPanel>("PlotActionsContainerPanel").Children.Add(addModuleButton);
        }

        public void RemovePlottingModule(int index)
        {
            PlottingActions.RemoveAt(index);
            PlottingParameters.RemoveAt(index);
            PlottingActionsContainer.Children.RemoveAt(index);
            PlottingAlerts.RemoveAt(index);
            UpdatePlottingParameters.RemoveAt(index);

            if (PlottingActions.Count > 0)
            {
                ((Grid)((Accordion)PlottingActionsContainer.Children[0]).AccordionHeader).Children[4].IsVisible = false;
                ((Grid)((Accordion)PlottingActionsContainer.Children.Last()).AccordionHeader).Children[5].IsVisible = false;
            }
        }

        public async void AddPlottingModuleAccessoriesAndUpdate()
        {
            PlotCanvases.Add(null);
            PlotBounds.Add((new VectSharp.Point(), new VectSharp.Point()));
            SelectionCanvases.Add(null);
            LayerTransforms.Add(null);

            await ActuallyUpdatePlotLayer(PlotCanvases.Count - 1, false);
        }

        public Action<Dictionary<string, object>> AddPlottingModule(PlottingModule module)
        {
            if (module == null)
            {
                throw new NullReferenceException();
            }

            PlottingActions.Add(module);

            Accordion exp = new Accordion() { Margin = new Thickness(5, 0, 0, 5), ArrowSize = 10 };

            Grid modulePanel = new Grid() { Margin = new Thickness(0, 0, 0, 0) };
            modulePanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(GlobalSettings.Settings.ShowLegacyUpDownArrows ? 24 : 0, GridUnitType.Pixel), new ColumnDefinition(GlobalSettings.Settings.ShowLegacyUpDownArrows ? 24 : 0, GridUnitType.Pixel), new ColumnDefinition(16, GridUnitType.Pixel), new ColumnDefinition(0, GridUnitType.Auto) };

            Control alert = GetAlertIcon();
            alert.Width = 16;
            alert.Height = 16;
            PlottingAlerts.Add(alert);
            alert.Margin = new Thickness(0, 0, 5, 0);
            alert.IsVisible = false;

            modulePanel.Children.Add(alert);

            DPIAwareBox icon = new DPIAwareBox(module.GetIcon) { Width = 16, Height = 16, Margin = new Thickness(0, 0, 5, 0) };
            Grid.SetColumn(icon, 1);
            modulePanel.Children.Add(icon);
            AvaloniaBugFixes.SetToolTip(icon, module.Name);

            FillingControl<TrimmedTextBox2> moduleName = new FillingControl<TrimmedTextBox2>(new TrimmedTextBox2() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = module.Name, FontWeight = FontWeight.Bold, FontSize = 13 }, 5) { Margin = new Thickness(-5, 0, -5, 0) };

            AvaloniaBugFixes.SetToolTip(moduleName, module.Name);

            Grid.SetColumn(moduleName, 2);

            modulePanel.Children.Add(moduleName);

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 3);
            modulePanel.Children.Add(helpButton);

            AvaloniaBugFixes.SetToolTip(helpButton, module.HelpText);

            helpButton.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            helpButton.Click += (s, e) =>
            {
                e.Handled = true;

                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[module.Id].BuildReadmeMarkdown(), module.Id);

                win.Show(this);
            };

            AddRemoveButton moveUp = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Up };
            Grid.SetColumn(moveUp, 4);
            modulePanel.Children.Add(moveUp);
            if (PlottingActions.Count <= 1)
            {
                moveUp.IsVisible = false;
            }

            moveUp.Click += (s, e) =>
            {
                e.Handled = true;

                int index = PlottingActionsContainer.Children.IndexOf(exp);

                if (index > 0)
                {
                    this.PushUndoFrame(UndoFrameLevel.PlotActionModule, index - 1, new int[] { index - 1, index });

                    PlottingModule mod = PlottingActions[index];
                    PlottingActions.RemoveAt(index);
                    PlottingActions.Insert(index - 1, mod);

                    Dictionary<string, object> param = PlottingParameters[index];
                    PlottingParameters.RemoveAt(index);
                    PlottingParameters.Insert(index - 1, param);

                    Action<Dictionary<string, object>> updater = UpdatePlottingParameters[index];
                    UpdatePlottingParameters.RemoveAt(index);
                    UpdatePlottingParameters.Insert(index - 1, updater);

                    IControl pan = PlottingActionsContainer.Children[index];
                    PlottingActionsContainer.Children.RemoveAt(index);
                    PlottingActionsContainer.Children.Insert(index - 1, pan);

                    Control alert = PlottingAlerts[index];
                    PlottingAlerts.RemoveAt(index);
                    PlottingAlerts.Insert(index - 1, alert);

                    for (int i = 0; i < PlottingActionsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = true;
                        }

                        if (i == PlottingActionsContainer.Children.Count - 1)
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[5].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[5].IsVisible = true;
                        }
                    }

                    SKRenderContext plotCanvas = PlotCanvases[index];
                    PlotCanvases.RemoveAt(index);
                    PlotCanvases.Insert(index - 1, plotCanvas);

                    (VectSharp.Point, VectSharp.Point) bounds = PlotBounds[index];
                    PlotBounds.RemoveAt(index);
                    PlotBounds.Insert(index - 1, bounds);

                    SKRenderContext selectionCanvas = SelectionCanvases[index];
                    SelectionCanvases.RemoveAt(index);
                    SelectionCanvases.Insert(index - 1, selectionCanvas);

                    SKRenderAction transform = LayerTransforms[index];
                    LayerTransforms.RemoveAt(index);
                    LayerTransforms.Insert(index - 1, transform);

                    FullPlotCanvas.MoveLayer(index, index - 1);
                    FullSelectionCanvas.MoveLayer(index, index - 1);
                }
            };

            AddRemoveButton moveDown = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Down };
            Grid.SetColumn(moveDown, 5);
            modulePanel.Children.Add(moveDown);
            moveDown.IsVisible = false;

            moveDown.Click += (s, e) =>
            {
                e.Handled = true;

                int index = PlottingActionsContainer.Children.IndexOf(exp);

                if (index < PlottingActionsContainer.Children.Count - 1)
                {
                    this.PushUndoFrame(UndoFrameLevel.PlotActionModule, index, new int[] { index, index + 1 });

                    PlottingModule mod = PlottingActions[index];
                    PlottingActions.RemoveAt(index);
                    PlottingActions.Insert(index + 1, mod);

                    Dictionary<string, object> param = PlottingParameters[index];
                    PlottingParameters.RemoveAt(index);
                    PlottingParameters.Insert(index + 1, param);

                    Action<Dictionary<string, object>> updater = UpdatePlottingParameters[index];
                    UpdatePlottingParameters.RemoveAt(index);
                    UpdatePlottingParameters.Insert(index + 1, updater);

                    IControl pan = PlottingActionsContainer.Children[index];
                    PlottingActionsContainer.Children.RemoveAt(index);
                    PlottingActionsContainer.Children.Insert(index + 1, pan);

                    Control alert = PlottingAlerts[index];
                    PlottingAlerts.RemoveAt(index);
                    PlottingAlerts.Insert(index + 1, alert);

                    for (int i = 0; i < PlottingActionsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[4].IsVisible = true;
                        }

                        if (i == PlottingActionsContainer.Children.Count - 1)
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[5].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Accordion)PlottingActionsContainer.Children[i]).AccordionHeader).Children[5].IsVisible = true;
                        }
                    }

                    SKRenderContext plotCanvas = PlotCanvases[index];
                    PlotCanvases.RemoveAt(index);
                    PlotCanvases.Insert(index + 1, plotCanvas);

                    (VectSharp.Point, VectSharp.Point) bounds = PlotBounds[index];
                    PlotBounds.RemoveAt(index);
                    PlotBounds.Insert(index + 1, bounds);

                    SKRenderContext selectionCanvas = SelectionCanvases[index];
                    SelectionCanvases.RemoveAt(index);
                    SelectionCanvases.Insert(index + 1, selectionCanvas);

                    SKRenderAction transform = LayerTransforms[index];
                    LayerTransforms.RemoveAt(index);
                    LayerTransforms.Insert(index + 1, transform);

                    FullPlotCanvas.MoveLayer(index, index + 1);
                    FullSelectionCanvas.MoveLayer(index, index + 1);
                }
            };

            if (!GlobalSettings.Settings.ShowLegacyUpDownArrows)
            {
                moveUp.Width = 0;
                moveDown.Width = 0;
                moveUp.Height = 0;
                moveDown.Height = 0;
                moveUp.Opacity = 0;
                moveDown.Opacity = 0;
            }

            Button remove = new Button() { Margin = new Thickness(0, 0, 2, 0), Width = 16, Height = 16, Background = Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 8, Height = 8, Data = Geometry.Parse("M0,0 L8,8 M8,0 L0,8"), StrokeThickness = 2, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center } };
            remove.Classes.Add("SideBarButton");

            remove.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            moveUp.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            moveDown.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            AvaloniaBugFixes.SetToolTip(remove, new TextBlock() { Text = "Remove", Foreground = Brushes.Black });

            Grid.SetColumn(remove, 7);
            modulePanel.Children.Add(remove);

            remove.Click += (s, e) =>
            {
                e.Handled = true;

                int index = PlottingActionsContainer.Children.IndexOf(exp);

                this.PushUndoFrame(UndoFrameLevel.PlotActionModule, index);

                PlottingActions.RemoveAt(index);
                PlottingParameters.RemoveAt(index);
                PlottingActionsContainer.Children.RemoveAt(index);
                PlottingAlerts.RemoveAt(index);
                UpdatePlottingParameters.RemoveAt(index);

                if (PlottingActions.Count > 0)
                {
                    ((Grid)((Accordion)PlottingActionsContainer.Children[0]).AccordionHeader).Children[4].IsVisible = false;
                    ((Grid)((Accordion)PlottingActionsContainer.Children.Last()).AccordionHeader).Children[5].IsVisible = false;
                }

                PlotCanvases.RemoveAt(index);
                LayerTransforms.RemoveAt(index);
                PlotBounds.RemoveAt(index);

                SelectionCanvases.RemoveAt(index);

                FullPlotCanvas.RemoveLayer(index);
                FullSelectionCanvas.RemoveLayer(index);

                UpdatePlotBounds();
            };

            exp.AccordionHeader = modulePanel;

            PointerPressedEventArgs pressEventArgs = null;

            Avalonia.Threading.DispatcherTimer pressTimer = new Avalonia.Threading.DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(GlobalSettings.Settings.DragInterval) };

            if (!GlobalSettings.Settings.ShowLegacyUpDownArrows)
            {

                pressTimer.Tick += (s, e) =>
                {
                    pressTimer.Stop();

                    if (pressEventArgs != null)
                    {
                        pressEventArgs.Pointer.Capture(exp);
                        pressEventArgs.Handled = true;

                        if (exp.IsOpen)
                        {
                            exp.IsOpen = false;
                        }

                        Func<PointerReleasedEventArgs, (int, int)> act = null;

                        void StopDrag(object sender, PointerReleasedEventArgs e)
                        {
                            exp.PointerReleased -= StopDrag;

                            if (act != null)
                            {
                                (int oldIndex, int newIndex) = act.Invoke(e);

                                if (oldIndex != newIndex)
                                {
                                    this.PushUndoFrame(UndoFrameLevel.PlotActionModule, Math.Min(oldIndex, newIndex), Enumerable.Range(Math.Min(oldIndex, newIndex), Math.Abs(oldIndex - newIndex) + 1));

                                    PlottingModule mod = PlottingActions[oldIndex];
                                    PlottingActions.RemoveAt(oldIndex);
                                    PlottingActions.Insert(newIndex, mod);

                                    Dictionary<string, object> param = PlottingParameters[oldIndex];
                                    PlottingParameters.RemoveAt(oldIndex);
                                    PlottingParameters.Insert(newIndex, param);

                                    Action<Dictionary<string, object>> updater = UpdatePlottingParameters[oldIndex];
                                    UpdatePlottingParameters.RemoveAt(oldIndex);
                                    UpdatePlottingParameters.Insert(newIndex, updater);

                                    Control alert = PlottingAlerts[oldIndex];
                                    PlottingAlerts.RemoveAt(oldIndex);
                                    PlottingAlerts.Insert(newIndex, alert);

                                    SKRenderContext plotCanvas = PlotCanvases[oldIndex];
                                    PlotCanvases.RemoveAt(oldIndex);
                                    PlotCanvases.Insert(newIndex, plotCanvas);

                                    (VectSharp.Point, VectSharp.Point) bounds = PlotBounds[oldIndex];
                                    PlotBounds.RemoveAt(oldIndex);
                                    PlotBounds.Insert(newIndex, bounds);

                                    SKRenderContext selectionCanvas = SelectionCanvases[oldIndex];
                                    SelectionCanvases.RemoveAt(oldIndex);
                                    SelectionCanvases.Insert(newIndex, selectionCanvas);

                                    SKRenderAction transform = LayerTransforms[oldIndex];
                                    LayerTransforms.RemoveAt(oldIndex);
                                    LayerTransforms.Insert(newIndex, transform);

                                    FullPlotCanvas.MoveLayer(oldIndex, newIndex);
                                    FullSelectionCanvas.MoveLayer(oldIndex, newIndex);
                                }
                            }
                        }

                        StartDrag(exp, PlottingActionsContainer, ref act, pressEventArgs);

                        exp.PointerReleased += StopDrag;
                    }
                };

                exp.PointerPressed += (s, e) =>
                {
                    if (!helpButton.IsPointerOver && !remove.IsPointerOver && PlottingActions.Count > 1)
                    {
                        if (exp.FindControl<Grid>("HeaderGrid").IsPointerOver)
                        {
                            pressEventArgs = e;
                            pressTimer.Start();
                        }
                    }
                };

                exp.PointerReleased += (s, e) =>
                {
                    pressEventArgs = null;
                    pressTimer.Stop();
                };
            }

            List<(string, string)> moduleParameters = module.GetParameters(TransformedTree);
            moduleParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate plottingParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                return module.OnParameterChange(TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
            };

            PlottingParameters.Add(UpdateParameterPanel(plottingParameterChange, moduleParameters, () => { _ = UpdatePlotLayer(PlottingActionsContainer.Children.IndexOf(exp), true); }, out Action<Dictionary<string, object>> updateParameterAction, out Control content));
            exp.AccordionContent = content;

            UpdatePlottingParameters.Add(updateParameterAction);

            PlottingActionsContainer.Children.Add(exp);

            if (PlottingActions.Count > 1)
            {
                ((Grid)((Accordion)PlottingActionsContainer.Children[0]).AccordionHeader).Children[5].IsVisible = true;
            }

            if (PlottingActions.Count > 2)
            {
                ((Grid)((Accordion)PlottingActionsContainer.Children[PlottingActionsContainer.Children.Count - 2]).AccordionHeader).Children[5].IsVisible = true;
            }

            Button duplicate = new Button() { Width = 16, Height = 16, Background = Brushes.Transparent, Content = Icons.GetDuplicateIcon(), Padding = new Thickness(0) };
            duplicate.Classes.Add("SideBarButton");

            duplicate.PointerPressed += (s, e) =>
            {
                e.Handled = true;
            };

            AvaloniaBugFixes.SetToolTip(duplicate, new TextBlock() { Text = "Duplicate", Foreground = Brushes.Black });

            Grid.SetColumn(duplicate, 6);
            modulePanel.Children.Add(duplicate);

            duplicate.Click += async (s, e) =>
            {
                int index = PlottingActionsContainer.Children.IndexOf(exp);

                Dictionary<string, object> parameters = PlottingParameters[index].DeepClone(false);

                Action<Dictionary<string, object>> updater = this.StateData.AddPlottingModule(module);

                parameters[Modules.ModuleIDKey] = this.PlottingParameters[this.PlottingParameters.Count - 1][Modules.ModuleIDKey];

                updater(parameters);

                PlotCanvases.Add(null);
                PlotBounds.Add((new VectSharp.Point(), new VectSharp.Point()));
                SelectionCanvases.Add(null);
                LayerTransforms.Add(null);

                await ActuallyUpdatePlotLayer(PlotCanvases.Count - 1, false);
            };

            return updateParameterAction;
        }

        private void ZoomPropertyChanged(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Avalonia.Controls.PanAndZoom.ZoomBorder.ZoomXProperty && !programmaticZoomUpdate)
            {
                double zoom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX;
                zoom = Math.Min(100, Math.Max(zoom, 0.01));
                SetZoom(zoom, zoom == this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX);
            }
        }
        private async void KeyPressed(object sender, KeyEventArgs e)
        {
            if (!(e.Device.FocusedElement is TextBox element && element.IsEffectivelyVisible == true) && !(e.Device.FocusedElement is NumericUpDown element2 && element2.IsEffectivelyVisible == true))
            {
                if (IsSelectionAvailable)
                {
                    for (int i = 0; i < Modules.SelectionActionModules.Count; i++)
                    {
                        if (e.Key == Modules.SelectionActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.SelectionActionModules[i].ShortcutModifier))
                        {
                            Modules.SelectionActionModules[i].PerformAction(0, this.SelectedNode, this, this.StateData);
                            e.Handled = true;
                        }
                    }
                }

                for (int i = 0; i < Modules.ActionModules.Count; i++)
                {
                    if (Modules.ActionModules[i].SubItems.Count == 0)
                    {
                        if (e.Key == Modules.ActionModules[i].ShortcutKeys[0].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutKeys[0].Item2))
                        {
                            Modules.ActionModules[i].PerformAction(0, this, this.StateData);
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        int shift = string.IsNullOrEmpty(Modules.ActionModules[i].SubItems[0].Item1) ? -1 : 0;

                        for (int j = 0; j < Modules.ActionModules[i].SubItems.Count; j++)
                        {
                            if (e.Key == Modules.ActionModules[i].ShortcutKeys[j].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutKeys[j].Item2))
                            {
                                Modules.ActionModules[i].PerformAction(j + shift, this, this.StateData);
                                e.Handled = true;
                            }
                        }
                    }
                }

                for (int i = 0; i < Modules.MenuActionModules.Count; i++)
                {
                    if (Modules.MenuActionModules[i].SubItems.Count == 0)
                    {
                        if (e.Key == Modules.MenuActionModules[i].ShortcutKeys[0].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutKeys[0].Item2))
                        {
                            await Modules.MenuActionModules[i].PerformAction(0, this);
                            e.Handled = true;
                        }
                    }
                    else
                    {
                        int shift = string.IsNullOrEmpty(Modules.MenuActionModules[i].SubItems[0].Item1) ? -1 : 0;

                        for (int j = 0; j < Modules.MenuActionModules[i].SubItems.Count; j++)
                        {
                            if (e.Key == Modules.MenuActionModules[i].ShortcutKeys[j].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutKeys[j].Item2))
                            {
                                await Modules.MenuActionModules[i].PerformAction(j + shift, this);
                                e.Handled = true;
                            }
                        }
                    }
                }
            }
            else if (!(e.Device.FocusedElement is NumericUpDown element3 && element3.IsEffectivelyVisible == true))
            {
                if (IsSelectionAvailable)
                {
                    for (int i = 0; i < Modules.SelectionActionModules.Count; i++)
                    {
                        if (Modules.SelectionActionModules[i].TriggerInTextBox && e.Key == Modules.SelectionActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.SelectionActionModules[i].ShortcutModifier))
                        {
                            Modules.SelectionActionModules[i].PerformAction(0, this.SelectedNode, this, this.StateData);
                            e.Handled = true;
                        }
                    }
                }

                for (int i = 0; i < Modules.ActionModules.Count; i++)
                {
                    if (Modules.ActionModules[i].TriggerInTextBox)
                    {
                        if (Modules.ActionModules[i].SubItems.Count == 0)
                        {
                            if (e.Key == Modules.ActionModules[i].ShortcutKeys[0].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutKeys[0].Item2))
                            {
                                Modules.ActionModules[i].PerformAction(0, this, this.StateData);
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            int shift = string.IsNullOrEmpty(Modules.ActionModules[i].SubItems[0].Item1) ? -1 : 0;

                            for (int j = 0; j < Modules.ActionModules[i].SubItems.Count; j++)
                            {
                                if (e.Key == Modules.ActionModules[i].ShortcutKeys[j].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutKeys[j].Item2))
                                {
                                    Modules.ActionModules[i].PerformAction(j + shift, this, this.StateData);
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }

                for (int i = 0; i < Modules.MenuActionModules.Count; i++)
                {
                    if (Modules.MenuActionModules[i].TriggerInTextBox)
                    {
                        if (Modules.MenuActionModules[i].SubItems.Count == 0)
                        {
                            if (e.Key == Modules.MenuActionModules[i].ShortcutKeys[0].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutKeys[0].Item2))
                            {
                                await Modules.MenuActionModules[i].PerformAction(0, this);
                                e.Handled = true;
                            }
                        }
                        else
                        {
                            int shift = string.IsNullOrEmpty(Modules.MenuActionModules[i].SubItems[0].Item1) ? -1 : 0;

                            for (int j = 0; j < Modules.MenuActionModules[i].SubItems.Count; j++)
                            {
                                if (e.Key == Modules.MenuActionModules[i].ShortcutKeys[j].Item1 && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutKeys[j].Item2))
                                {
                                    await Modules.MenuActionModules[i].PerformAction(j + shift, this);
                                    e.Handled = true;
                                }
                            }
                        }
                    }
                }
            }

            if (!e.Handled)
            {
                if (e.Key == Key.H && e.KeyModifiers == (Modules.ControlModifier | KeyModifiers.Shift))
                {
                    this.RibbonBar.SelectedIndex = 0;
                    this.RibbonFilePage.SelectedIndex = 0;
                }
                else if (e.Key == Key.A && e.KeyModifiers == (Modules.ControlModifier | KeyModifiers.Shift))
                {
                    this.RibbonBar.SelectedIndex = 0;
                    AutosavesPage page = this.GetFilePage<AutosavesPage>(out int ind);
                    this.RibbonFilePage.SelectedIndex = ind;
                    ((RibbonFilePageContentTabbedWithButtons)page.PageContent).SelectedIndex = 0;
                }
                else if (e.Key == Key.R && e.KeyModifiers == Modules.ControlModifier)
                {
                    this.RibbonBar.SelectedIndex = 0;
                    this.GetFilePage<PreferencesPage>(out int ind);
                    this.RibbonFilePage.SelectedIndex = ind;
                }
                else if (e.Key == Key.M && e.KeyModifiers == Modules.ControlModifier)
                {
                    ModuleManagerWindow win2 = new ModuleManagerWindow();
                    await win2.ShowDialog2(this);
                }
                else if (e.Key == Key.H && e.KeyModifiers == Modules.ControlModifier)
                {
                    AboutWindow win2 = new AboutWindow();
                    await win2.ShowDialog2(this);
                }
                else if (e.Key == Key.M && e.KeyModifiers == (Modules.ControlModifier | KeyModifiers.Shift))
                {
                    MessageBox box = new MessageBox("Attention", "The program will now be rebooted to open the module creator (we will do our best to recover the files that are currently open). Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box.ShowDialog2(this);

                    if (box.Result == MessageBox.Results.Yes)
                    {
                        Program.Reboot(new string[] { "--module-creator" }, true);
                        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                    }
                }
                else if (e.Key == Key.K && e.KeyModifiers == Modules.ControlModifier)
                {
                    this.FindControl<TextBox>("CommandBox").Focus();
                }
            }
        }

        private bool programmaticZoomUpdate = false;

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

        public void SetZoom(double zoom, bool skipActualZoom)
        {
            programmaticZoomUpdate = true;
            this.FindControl<NumericUpDown>("ZoomNud").Value = zoom;
            this.FindControl<Slider>("ZoomSlider").Value = Math.Log10(zoom);
            if (!skipActualZoom)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomTo(zoom / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height * 0.5);
            }
            UpdateSelectionWidth();
            programmaticZoomUpdate = false;
        }

        public Rectangle GetVisibleRegion()
        {
            double marginLeft = this.IsModulesPanelOpen ? this.FindControl<Grid>("ParameterContainerGrid").Bounds.Width + 6 : 0;
            double marginRight = this.IsSelectionPanelOpen ? this.FindControl<Grid>("SelectionGrid").Bounds.Width + 16 : 0;

            double deltaX = (marginLeft - marginRight) * 0.5;

            double zoom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX;

            double offsetX = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetX;
            double offsetY = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetY;

            double centerX = -(offsetX - deltaX - (PlotOrigin.X + PlotBottomRight.X) * 0.5 - (PlotOrigin.X - 10) * (zoom - 1)) / zoom;
            double centerY = -(offsetY - (PlotOrigin.Y + PlotBottomRight.Y) * 0.5 - (PlotOrigin.Y - 10) * (zoom - 1)) / zoom;

            double width = (this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Bounds.Width - marginLeft - marginRight) / zoom;
            double height = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Bounds.Height / zoom;

            return new Rectangle(new VectSharp.Point(Math.Max(centerX - width * 0.5, PlotOrigin.X), Math.Max(centerY - height * 0.5, PlotOrigin.Y)), new VectSharp.Point(Math.Min(PlotBottomRight.X, centerX + width * 0.5), Math.Min(PlotBottomRight.Y, centerY + height * 0.5)));
        }

        public void CenterAt(double x, double y)
        {
            double marginLeft = this.IsModulesPanelOpen ? this.FindControl<Grid>("ParameterContainerGrid").Bounds.Width + 6 : 0;
            double marginRight = this.IsSelectionPanelOpen ? this.FindControl<Grid>("SelectionGrid").Bounds.Width + 16 : 0;

            double deltaX = (marginLeft - marginRight) * 0.5;

            double zoom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX;

            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomTo(1 / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height * 0.5);
            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").BeginPanTo(0, 0);
            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ContinuePanTo(-this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetX + deltaX + (PlotBottomRight.X - PlotOrigin.X) * 0.5 * zoom - (x - PlotOrigin.X) * zoom, -this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetY + (PlotBottomRight.Y - PlotOrigin.Y) * 0.5 * zoom - (y - PlotOrigin.Y) * zoom);

            SetZoom(zoom, false);

            WasAutoFitted = false;
        }

        private static bool WasAutoFitted = false;

        public void AutoFit()
        {
            double availableWidth = this.FindControl<Canvas>("PlotBackground").Bounds.Width;
            double availableHeight = this.FindControl<Canvas>("PlotBackground").Bounds.Height;

            double marginLeft = this.IsModulesPanelOpen ? this.FindControl<Grid>("ParameterContainerGrid").Bounds.Width + 6 : 0;
            double marginRight = this.IsSelectionPanelOpen ? this.FindControl<Grid>("SelectionGrid").Bounds.Width + 16 : 0;

            availableWidth -= marginLeft + marginRight;

            double maxZoomX = availableWidth / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width;
            double maxZoomY = availableHeight / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height;

            double deltaX = (marginLeft - marginRight) * 0.5;

            double zoom = Math.Min(maxZoomX, maxZoomY);

            if (!double.IsNaN(zoom) && !double.IsInfinity(zoom) && zoom > 0)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomTo(1 / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height * 0.5);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").BeginPanTo(0, 0);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ContinuePanTo(-this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetX + deltaX, -this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetY);

                SetZoom(zoom, false);
            }

            this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Transitions = null;

            WasAutoFitted = true;
        }

        private async void AddAttachmentClicked(object sender, PointerReleasedEventArgs e)
        {
            OpenFileDialog dialog;

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Add attachment",
                    AllowMultiple = false,
                    Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string>() { "*" }, Name = "All files" } }
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Add attachment",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                bool validResult = false;

                string defaultName = Path.GetFileNameWithoutExtension(result[0]);
                bool loadInMemory = true;
                bool cacheResults = true;

                while (!validResult)
                {
                    AddAttachmentWindow win = new AddAttachmentWindow(defaultName, loadInMemory, cacheResults);
                    await (win.ShowDialog2(this));

                    if (win.Result)
                    {
                        if (!StateData.Attachments.ContainsKey(win.AttachmentName))
                        {
                            validResult = true;

                            this.PushUndoFrame(UndoFrameLevel.Attachment, 0);

                            Attachment attachment = new Attachment(win.AttachmentName, win.CacheResults, win.LoadInMemory, result[0]);
                            this.StateData.Attachments.Add(attachment.Name, attachment);

                            BuildAttachmentList();

                            await UpdateTransformedTree();
                        }
                        else
                        {
                            validResult = false;
                            loadInMemory = win.LoadInMemory;
                            cacheResults = win.CacheResults;
                            defaultName = win.AttachmentName;

                            MessageBox box = new MessageBox("Attention", "There is another attachment with the same name!");

                            await box.ShowDialog2(this);
                        }

                    }
                    else
                    {
                        validResult = true;
                    }
                }
            }
        }

        public void BuildAttachmentList()
        {
            BuildRibbonAttachmentPanel();

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            int newIndex = RibbonBar.SelectedIndex;
            if (newIndex == 0)
            {
                this.HomePage.UpdateRecentFiles();

                this.RibbonFilePage.IsVisible = true;
                this.RibbonFilePage.Opacity = 1;
                this.RibbonFilePage.RenderTransform = TransformOperations.Identity;
                this.RibbonFilePage.FindControl<Grid>("ThemeGrid").RenderTransform = TransformOperations.Identity;

                this.FindControl<Grid>("TitleBarContainer2").IsVisible = true;
                this.FindControl<Grid>("TitleBarContainer2").Opacity = 1;
            }
            else
            {
                for (int i = 0; i < RibbonTabs.Length; i++)
                {
                    if (RibbonTabs[i] != null)
                    {
                        if (i != newIndex)
                        {
                            RibbonTabs[i].ZIndex = 0;
                            RibbonTabs[i].RenderTransform = offScreen;
                            RibbonTabs[i].Opacity = 0;
                            RibbonTabs[i].IsHitTestVisible = false;
                        }
                        else
                        {
                            RibbonTabs[i].ZIndex = 1;
                            RibbonTabs[i].RenderTransform = TransformOperations.Identity;
                            RibbonTabs[i].Opacity = 1;
                            RibbonTabs[i].IsHitTestVisible = true;
                        }
                    }
                }
            }
        }

        private void UpdateAttachmentSelectors()
        {
            List<string> items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

            if (StateData.Attachments.Count > 0)
            {
                items.AddRange(StateData.Attachments.Keys);
            }

            foreach (ComboBox box in AttachmentSelectors)
            {
                string selectedItem = (string)box.SelectedItem;

                box.Items = items;

                int index = items.IndexOf(selectedItem);
                box.SelectedIndex = Math.Max(index, 0);
            }
        }

        private void RefreshAttachmentSelectors(string replacedAttachmentName)
        {
            foreach (ComboBox box in AttachmentSelectors)
            {
                string selectedItem = (string)box.SelectedItem;

                if (selectedItem == replacedAttachmentName)
                {
                    int index = box.SelectedIndex;
                    box.SelectedIndex = 0;
                    box.SelectedIndex = index;
                }
            }
        }

        private void FitZoomButtonClicked(object sender, RoutedEventArgs e)
        {
            AutoFit();
        }

        private void ZoomPointerWheelChanged(object sender, PointerWheelEventArgs e)
        {
            if (this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX >= 100 && e.Delta.Y > 0)
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
    }

    public class MyZoomBorder : Avalonia.Controls.PanAndZoom.ZoomBorder
    {
        protected override Avalonia.Size MeasureOverride(Avalonia.Size availableSize)
        {
            return new Avalonia.Size(0, 0);
        }
    }
}
