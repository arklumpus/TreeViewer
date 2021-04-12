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

namespace TreeViewer
{
    public partial class MainWindow : Window
    {
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

        internal CSharpEditor.InterprocessDebuggerServer DebuggerServer
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

        public ComboBox TransformerComboBox;
        Canvas TransformerAlert;
        public Dictionary<string, object> TransformerParameters;
        public Action<Dictionary<string, object>> UpdateTransformerParameters;

        public ComboBox CoordinatesComboBox;
        Canvas CoordinatesAlert;
        public Dictionary<string, object> CoordinatesParameters;
        public Action<Dictionary<string, object>> UpdateCoordinatesParameters;

        StackPanel PlottingActionsContainer;
        public List<PlottingModule> PlottingActions = new List<PlottingModule>();

        List<Canvas> PlottingAlerts;
        public List<Dictionary<string, object>> PlottingParameters;
        public List<Action<Dictionary<string, object>>> UpdatePlottingParameters;
        //List<TextBlock> PlottingTimings;

        StackPanel FurtherTransformationsContainer;
        public List<FurtherTransformationModule> FurtherTransformations = new List<FurtherTransformationModule>();

        List<Canvas> FurtherTransformationsAlerts;
        public List<Dictionary<string, object>> FurtherTransformationsParameters;
        public List<Action<Dictionary<string, object>>> UpdateFurtherTransformationParameters;

        List<Action> SelectionActionActions;

        private string WindowGuid = System.Guid.NewGuid().ToString();
        private string OriginalFileName = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(TreeCollection trees, List<(string, Dictionary<string, object>)> moduleSuggestions, string fileName)
        {
            InitializeComponent();

            Trees = trees;
            FileOpened(moduleSuggestions);

            if (!string.IsNullOrEmpty(fileName))
            {
                this.Title = "TreeViewer - " + Path.GetFileName(fileName);
                this.OriginalFileName = fileName;
            }
        }

        private void WindowClosed(object sender, EventArgs e)
        {
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

        public async Task LoadFile(string fileName, bool deleteAfter)
        {
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
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Error!", "An error has occurred while loading the file!\n" + ex.Message).ShowDialog2(this); return Task.CompletedTask; });
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                        }
                        else
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Attention!", "The file cannot be loaded by any of the currently installed modules!").ShowDialog2(this); return Task.CompletedTask; });
                        }
                    });

                    thr.Start();

                    await progressWin.ShowDialog2(this);

                    if (coll != null)
                    {
                        if (Trees == null)
                        {
                            Trees = coll;
                            FileOpened(moduleSuggestions);
                            if (!deleteAfter)
                            {
                                this.Title = "TreeViewer - " + Path.GetFileName(fileName);
                                this.OriginalFileName = fileName;
                            }
                        }
                        else
                        {
                            MainWindow win = new MainWindow(coll, moduleSuggestions, deleteAfter ? "" : fileName);
                            win.Show();
                        }
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
                File.Delete(fileName);
            }
        }


        private void CloseMenuClicked(object sender, EventArgs e)
        {
            this.Close();
        }



        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            GlobalSettings.Settings.MainWindows.Add(this);

            this.FindControl<Canvas>("PlotBackground").Background = new SolidColorBrush(((Colour)GlobalSettings.Settings.BackgroundColour).ToAvalonia());

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


            /* if (Modules.MissingModules == null)
             {
                 Modules.LoadInstalledModules();
             }*/


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

            for (int i = 0; i < Modules.SelectionActionModules.Count; i++)
            {
                BuildSelectionActionModuleButton(Modules.SelectionActionModules[i], i);
            }

            for (int i = 0; i < Modules.ActionModules.Count; i++)
            {
                BuildActionModuleButton(Modules.ActionModules[i], i);
            }

            BuildAllMenuModules();

            Modules.ModuleLoaded += (s, e) =>
            {
                if (e.LoadedModuleMetadata.ModuleType == ModuleTypes.Action)
                {
                    BuildActionModuleButton((ActionModule)e.LoadedModule, Modules.ActionModules.Count - 1);
                }
                else if (e.LoadedModuleMetadata.ModuleType == ModuleTypes.SelectionAction)
                {
                    BuildSelectionActionModuleButton((SelectionActionModule)e.LoadedModule, Modules.SelectionActionModules.Count - 1);
                }
                else if (e.LoadedModuleMetadata.ModuleType == ModuleTypes.MenuAction)
                {
                    BuildAllMenuModules();
                }
                else if (e.LoadedModuleMetadata.ModuleType == ModuleTypes.Transformer && TransformerComboBox != null)
                {
                    ((List<string>)TransformerComboBox.Items).Add(e.LoadedModule.Name);
                }
                else if (e.LoadedModuleMetadata.ModuleType == ModuleTypes.Coordinate && CoordinatesComboBox != null)
                {
                    ((List<string>)CoordinatesComboBox.Items).Add(e.LoadedModule.Name);
                }
            };

            this.AutosaveTimer = new Avalonia.Threading.DispatcherTimer(GlobalSettings.Settings.AutosaveInterval, Avalonia.Threading.DispatcherPriority.Background, Autosave);
            this.AutosaveTimer.Start();
        }

        private void BuildAllMenuModules()
        {
            List<(string, List<(string, List<MenuActionModule>)>)> menuItems = new List<(string, List<(string, List<MenuActionModule>)>)>();

            menuItems.Add(("File", new List<(string, List<MenuActionModule>)>()));
            menuItems.Add(("Edit", new List<(string, List<MenuActionModule>)>()));

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

                string groupId = Modules.MenuActionModules[i].GroupId;

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
                    NativeMenuItem autosavesItem = new NativeMenuItem("Autosaves...");
                    autosavesItem.Command = new SimpleCommand((win) => true, async (win) => { AutosaveWindow window = new AutosaveWindow(this); await window.ShowDialog((MainWindow)win); }, this, null);
                    autosavesItem.CommandParameter = this;
                    autosavesItem.Gesture = new KeyGesture(Key.A, Modules.ControlModifier | KeyModifiers.Shift);

                    subMenu.Menu.Items.Add(autosavesItem);

                    subMenu.Menu.Items.Add(new NativeMenuItemSeperator());
                }

                List<Func<MainWindow, bool>> enabledActions = new List<Func<MainWindow, bool>>();

                for (int j = 0; j < menuItems[i].Item2.Count; j++)
                {
                    for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                    {
                        NativeMenuItem item = new NativeMenuItem(menuItems[i].Item2[j].Item2[k].ItemText);
                        Func<MainWindow, Task> clickAction = menuItems[i].Item2[j].Item2[k].PerformAction;

                        item.Gesture = new KeyGesture(menuItems[i].Item2[j].Item2[k].ShortcutKey, Modules.GetModifier(menuItems[i].Item2[j].Item2[k].ShortcutModifier));

                        Func<MainWindow, bool> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                        enabledActions.Add(menuItems[i].Item2[j].Item2[k].IsEnabled);
                        item.Command = new SimpleCommand((win) => isEnabled((MainWindow)win), async (win) => { await clickAction((MainWindow)win); }, this, menuItems[i].Item2[j].Item2[k].PropertyAffectingEnabled);

                        item.CommandParameter = this;
                        subMenu.Menu.Items.Add(item);

                    }
                    if (j < menuItems[i].Item2.Count - 1)
                    {
                        subMenu.Menu.Items.Add(new NativeMenuItemSeperator());
                    }
                }

                if (menuItems[i].Item1 == "File")
                {
                    subMenu.Menu.Items.Add(new NativeMenuItemSeperator());

                    NativeMenuItem exitItem = new NativeMenuItem("Exit");
                    exitItem.Command = new SimpleCommand((win) => true, (win) => { this.Close(); }, this, null);
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
                        subMenu.Menu.Items.Add(new NativeMenuItemSeperator());
                    }

                    NativeMenuItem aboutItem = new NativeMenuItem("About...");
                    aboutItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        AboutWindow win2 = new AboutWindow();
                        await win2.ShowDialog2((MainWindow)win);
                    }, this, null);
                    aboutItem.CommandParameter = this;
                    aboutItem.Gesture = new KeyGesture(Key.H, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(aboutItem);
                }

                if (menuItems[i].Item1 == "Edit")
                {
                    if (subMenu.Menu.Items.Count > 0)
                    {
                        subMenu.Menu.Items.Add(new NativeMenuItemSeperator());
                    }

                    NativeMenuItem preferencesItem = new NativeMenuItem("Preferences...");
                    preferencesItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        await GlobalSettings.Settings.ShowGlobalSettingsWindow((MainWindow)win);
                    }, this, null);
                    preferencesItem.CommandParameter = this;
                    preferencesItem.Gesture = new KeyGesture(Key.R, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(preferencesItem);

                    NativeMenuItem managerItem = new NativeMenuItem("Module manager...");
                    managerItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        ModuleManagerWindow win2 = new ModuleManagerWindow();
                        await win2.ShowDialog2((MainWindow)win);
                    }, this, null);
                    managerItem.CommandParameter = this;
                    managerItem.Gesture = new KeyGesture(Key.M, Modules.ControlModifier);

                    subMenu.Menu.Items.Add(managerItem);

                    NativeMenuItem creatorItem = new NativeMenuItem("Module creator...");
                    creatorItem.Command = new SimpleCommand((win) => true, async (win) =>
                    {
                        MessageBox box = new MessageBox("Attention", "The program will now be rebooted to open the module creator (we will do our best to recover the files that are currently open). Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                        await box.ShowDialog((MainWindow)win);

                        if (box.Result == MessageBox.Results.Yes)
                        {
                            Program.Reboot(new string[] { "--module-creator" }, true);
                            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                        }
                    }, this, null);
                    creatorItem.CommandParameter = this;
                    creatorItem.Gesture = new KeyGesture(Key.M, Modules.ControlModifier | KeyModifiers.Shift);

                    subMenu.Menu.Items.Add(creatorItem);
                }

                menu.Items.Add(subMenu);
            }

            NativeMenu.SetMenu(this, menu);
        }

        private void BuildSelectionActionModuleButton(SelectionActionModule module, int i)
        {
            Grid content = new Grid();

            content.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            content.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            content.Children.Add(new Viewbox() { Child = module.GetIcon().PaintToCanvas(), Height = 42 });
            TextBlock blk = new TextBlock() { Text = module.ButtonText, Margin = new Thickness(0, 5, 0, 0), TextAlignment = TextAlignment.Center };
            Grid.SetRow(blk, 1);
            content.Children.Add(blk);

            Button btn = new Button() { Content = content, Padding = new Thickness(5), Width = 80, Height = 100, Margin = new Thickness(0, 5, 0, 5) };

            ToolTip.SetTip(btn, module.HelpText);

            int row = i / 3;
            int column = i % 3;

            Grid.SetRow(btn, row);
            Grid.SetColumn(btn, column);

            int index = i;

            SelectionActionActions.Add(() => { });

            btn.Click += (s, e) =>
            {
                SelectionActionActions[index]();
            };

            while (this.FindControl<Grid>("SelectionActionsContainerGrid").RowDefinitions.Count < row + 1)
            {
                this.FindControl<Grid>("SelectionActionsContainerGrid").RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            }

            this.FindControl<Grid>("SelectionActionsContainerGrid").Children.Add(btn);
        }

        private void BuildActionModuleButton(ActionModule module, int i)
        {
            if (Modules.ActionModules[i].ButtonText != null)
            {
                Grid content = new Grid();

                content.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                content.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                content.Children.Add(new Viewbox() { Child = module.GetIcon().PaintToCanvas(), Height = 42 });
                TextBlock blk = new TextBlock() { Text = module.ButtonText, Margin = new Thickness(0, 5, 0, 0), TextAlignment = TextAlignment.Center };
                Grid.SetRow(blk, 1);
                content.Children.Add(blk);

                Button btn = new Button() { Content = content, Padding = new Thickness(5), Width = 80, Height = 100, Margin = new Thickness(5) };

                ToolTip.SetTip(btn, module.HelpText);

                int index = i;

                btn.Click += async (s, e) =>
                {
                    try
                    {
                        Modules.ActionModules[index].PerformAction(this, this.StateData);
                    }
                    catch (Exception ex)
                    {
                        await new MessageBox("Attention!", "An error occurred while performing the action!\n" + ex.Message).ShowDialog2(this);
                    }
                };

                this.FindControl<StackPanel>("ActionsContainerGrid").Children.Add(btn);
            }
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

        public async void FileOpened(List<(string, Dictionary<string, object>)> suggestedModules)
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

            this.FindControl<AddRemoveButton>("AddAttachmentButton").IsVisible = true;

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

            await UpdateOnlyTransformedTree();

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(new Canvas() { Background = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Height = 1, Margin = new Thickness(0, 5, 0, 5) });

            BuildFurtherTransformationPanel();

            ProgressWindow window = new ProgressWindow() { ProgressText = "Loading further transformations..." };
            _ = window.ShowDialog(this);

            SemaphoreSlim semaphore2 = new SemaphoreSlim(0, 1);

            Thread thr = new Thread(async () =>
            {
                await UpdateOnlyFurtherTransformations(0, window);

                for (int i = 0; i < suggestedModules.Count; i++)
                {
                    FurtherTransformationModule mod = Modules.GetModule(Modules.FurtherTransformationModules, suggestedModules[i].Item1);
                    if (mod != null)
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            try
                            {
                                Action<Dictionary<string, object>> updater = AddFurtherTransformation(mod);
                                updater(suggestedModules[i].Item2);
                            }
                            catch { }
                        });

                        await UpdateOnlyFurtherTransformations(FurtherTransformations.Count - 1, window);
                    }
                }

                semaphore2.Release();
            });
            thr.Start();

            await semaphore2.WaitAsync();
            semaphore2.Release();

            window.Close();


            //UpdateOnlyFurtherTransformations();

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(new Canvas() { Background = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Height = 1, Margin = new Thickness(0, 5, 0, 5) });

            List<string> coordinateModules = (from el in Modules.CoordinateModules select el.Name).ToList();
            BuildCoordinatesPanel(suggestedModules[1].Item1);
            UpdateCoordinatesParameters(suggestedModules[1].Item2);

            UpdateOnlyCoordinates();

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(new Canvas() { Background = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Height = 1, Margin = new Thickness(0, 5, 0, 5) });

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                if (suggestedModules[i].Item1 == "@Background")
                {
                    GraphBackground = (Colour)suggestedModules[i].Item2["Colour"];
                }
            }

            BuildPlottingPanel(GraphBackground);

            for (int i = 0; i < suggestedModules.Count; i++)
            {
                if (suggestedModules[i].Item1 != "@Background" && suggestedModules[i].Item1 != "@Attachment")
                {
                    PlottingModule mod = Modules.GetModule(Modules.PlottingModules, suggestedModules[i].Item1);
                    if (mod != null)
                    {
                        try
                        {
                            Action<Dictionary<string, object>> updater = AddPlottingModule(mod);
                            updater(suggestedModules[i].Item2);
                        }
                        catch { }
                    }
                }
            }

            foreach ((string, Dictionary<string, object>) item in suggestedModules.Skip(2))
            {
                try
                {
                    Modules.GetModule(Modules.ActionModules, item.Item1)?.PerformAction(this, this.StateData);
                }
                catch { }
            }

            UpdateAllPlotLayers();
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
                    parameter[2] = System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize(valueFont.FontFamily.FileName), System.Text.Json.JsonSerializer.Serialize(valueFont.FontSize) }, Modules.DefaultSerializationOptions);
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



        private Dictionary<string, object> UpdateParameterPanel(Expander parent, GenericParameterChangeDelegate parameterChangeDelegate, List<(string, string)> parameters, Action updateAction, out Action<Dictionary<string, object>> UpdateParameterAction)
        {
            StackPanel controlsPanel = new StackPanel();
            Dictionary<string, object> tbr = new Dictionary<string, object>();

            if (parameters.Count > 1)
            {
                Stack<Controls> parents = new Stack<Controls>();
                parents.Push(controlsPanel.Children);

                Stack<int> childrenTillPop = new Stack<int>();
                childrenTillPop.Push(-1);

                Dictionary<string, Control> parameterControls = new Dictionary<string, Control>();
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                bool programmaticUpdate = false;

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                    else if (controlType == "TreeCollection")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this.Trees);
                    }
                    else if (controlType == "Window")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this);
                    }
                    else if (controlType == "InstanceStateData")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this.StateData);
                    }
                    else if (controlType == "Group")
                    {
                        string parameterName = parameters[i].Item1;

                        int numChildren = int.Parse(controlParameters);
                        Border brd = new Border() { CornerRadius = new CornerRadius(10), Margin = new Thickness(0, 12, 0, 5), Padding = new Thickness(10, 0, 10, 0), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1) };
                        StackPanel pnl = new StackPanel();
                        brd.Child = pnl;

                        Border header = new Border() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Thickness(-5, -12, 0, 5), Background = Brushes.White, Padding = new Thickness(5, 0, 5, 0) };
                        header.Child = new TextBlock() { Text = parameterName };
                        pnl.Children.Add(header);

                        parents.Peek().Add(brd);

                        parameterControls.Add(parameterName, brd);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }

                        parents.Push(pnl.Children);
                        childrenTillPop.Push(numChildren);
                    }
                    else if (controlType == "Button")
                    {
                        string parameterName = parameters[i].Item1;

                        Button control = new Button() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 5, 0, 5) };

                        parameterControls.Add(parameterName, control);

                        parents.Peek().Add(control);

                        tbr.Add(parameterName, false);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        control.Click += (s, e) =>
                        {
                            Dictionary<string, object> previousParameters = tbr.ShallowClone();

                            tbr[parameterName] = true;

                            bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                            UpdateControls(controlStatus, parameterControls);
                            UpdateParameters(parametersToChange, parameterUpdaters);

                            if (needsUpdate)
                            {
                                updateAction();
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Buttons")
                    {
                        string parameterName = parameters[i].Item1;

                        string[] buttons = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                        Grid control = new Grid() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                        for (int j = 0; j < buttons.Length; j++)
                        {
                            Button butt = new Button() { Content = buttons[j], HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(5, 5, 5, 5), Margin = new Thickness(2.5, 5, 2.5, 5) };

                            control.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                            Grid.SetColumn(butt, j);

                            control.Children.Add(butt);

                            int index = j;

                            butt.Click += (s, e) =>
                            {
                                Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                tbr[parameterName] = index;

                                bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                UpdateControls(controlStatus, parameterControls);
                                UpdateParameters(parametersToChange, parameterUpdaters);

                                if (needsUpdate)
                                {
                                    updateAction();
                                }
                            };
                        }

                        parameterControls.Add(parameterName, control);
                        parents.Peek().Add(control);
                        tbr.Add(parameterName, -1);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "CheckBox")
                    {
                        string parameterName = parameters[i].Item1;

                        CheckBox control = new CheckBox() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 5, 0, 5), IsChecked = Convert.ToBoolean(controlParameters) };

                        parameterControls.Add(parameterName, control);

                        parents.Peek().Add(control);

                        tbr.Add(parameterName, control.IsChecked == true);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                            control.IsChecked = (bool)value;
                        });

                        control.Click += (s, e) =>
                        {
                            Dictionary<string, object> previousParameters = tbr.ShallowClone();

                            tbr[parameterName] = control.IsChecked == true;

                            bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                            UpdateControls(controlStatus, parameterControls);
                            UpdateParameters(parametersToChange, parameterUpdaters);

                            if (needsUpdate)
                            {
                                updateAction();
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Formatter")
                    {
                        string parameterName = parameters[i].Item1;

                        Button control = new Button() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 5, 0, 5) };

                        parameterControls.Add(parameterName, control);

                        parents.Peek().Add(control);

                        string[] parsedParameters = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                        object[] formatterParams = new object[parsedParameters.Length - 1];

                        string attrType = (string)tbr[parsedParameters[0]];

                        if (attrType == "String")
                        {
                            formatterParams[0] = parsedParameters[1];
                            formatterParams[1] = Convert.ToBoolean(parsedParameters[2]);
                        }
                        else if (attrType == "Number")
                        {
                            formatterParams[0] = int.Parse(parsedParameters[1], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[1] = double.Parse(parsedParameters[2], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[2] = double.Parse(parsedParameters[3], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[3] = double.Parse(parsedParameters[4], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[4] = Convert.ToBoolean(parsedParameters[5]);
                            formatterParams[5] = Convert.ToBoolean(parsedParameters[6]);
                            formatterParams[6] = parsedParameters[7];
                            formatterParams[7] = Convert.ToBoolean(parsedParameters[8]);
                        }

                        tbr.Add(parameterName, new FormatterOptions(parsedParameters[parsedParameters.Length - 2]) { Parameters = formatterParams });

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        control.Click += async (s, e) =>
                        {
                            string attributeType = (string)tbr[parsedParameters[0]];

                            FormatOptionWindow win = new FormatOptionWindow();

                            string editorId = "StringFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                            await win.Initialize(attributeType, ((FormatterOptions)tbr[parameterName]).Parameters, this.DebuggerServer, editorId);

                            await win.ShowDialog2(this);

                            if (win.Result)
                            {
                                Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                tbr[parameterName] = win.Formatter;

                                bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                UpdateControls(controlStatus, parameterControls);
                                UpdateParameters(parametersToChange, parameterUpdaters);

                                if (needsUpdate)
                                {
                                    updateAction();
                                }
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Label")
                    {
                        string parameterName = parameters[i].Item1;

                        Grid paramPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        TextBlock labelBlock = new TextBlock() { Text = parameterName, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                        paramPanel.Children.Add(labelBlock);
                        parents.Peek().Add(paramPanel);

                        if (controlParameters.StartsWith("["))
                        {
                            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            if (items.Length > 0)
                            {
                                switch (items[0])
                                {
                                    case "Left":
                                        labelBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
                                        break;
                                    case "Right":
                                        labelBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
                                        break;
                                    case "Center":
                                        labelBlock.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
                                        break;
                                }
                            }

                            if (items.Length > 1)
                            {
                                switch (items[1])
                                {
                                    case "Normal":
                                        labelBlock.FontStyle = FontStyle.Normal;
                                        labelBlock.FontWeight = FontWeight.Normal;
                                        break;
                                    case "Bold":
                                        labelBlock.FontStyle = FontStyle.Normal;
                                        labelBlock.FontWeight = FontWeight.Bold;
                                        break;
                                    case "Italic":
                                        labelBlock.FontStyle = FontStyle.Italic;
                                        labelBlock.FontWeight = FontWeight.Normal;
                                        break;
                                    case "BoldItalic":
                                        labelBlock.FontStyle = FontStyle.Italic;
                                        labelBlock.FontWeight = FontWeight.Bold;
                                        break;
                                }
                            }

                            if (items.Length > 2)
                            {
                                labelBlock.Foreground = new SolidColorBrush(Color.Parse(items[2]));
                            }
                        }

                        parameterControls.Add(parameterName, paramPanel);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else
                    {
                        string parameterName = parameters[i].Item1;

                        Grid paramPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        paramPanel.Children.Add(new TextBlock() { Text = parameterName, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                        parents.Peek().Add(paramPanel);

                        parameterControls.Add(parameterName, paramPanel);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }

                        if (controlType == "ComboBox")
                        {
                            int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            List<string> items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(controlParameters, Modules.DefaultSerializationOptions);

                            ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = items, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, box.SelectedIndex);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = (int)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = box.SelectedIndex;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "TextBox")
                        {
                            TextBox box = new TextBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Text = controlParameters, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, controlParameters);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.Text = (string)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.PropertyChanged += (s, e) =>
                            {
                                if (!programmaticUpdate && e.Property == TextBox.TextProperty)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = box.Text;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "AttributeSelector")
                        {
                            int defaultIndex = Math.Max(0, AttributeList.IndexOf(controlParameters));

                            ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = AttributeList, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            AttributeSelectors.Add(box);

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, controlParameters);

                            box.Tag = false;

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = AttributeList.IndexOf((string)value);
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate && !(bool)box.Tag)
                                {
                                    if (box.SelectedIndex < 0 || box.SelectedIndex >= AttributeList.Count)
                                    {
                                        if (defaultIndex < AttributeList.Count)
                                        {
                                            box.SelectedIndex = defaultIndex;
                                        }
                                        else
                                        {
                                            box.SelectedIndex = 0;
                                        }

                                        return;
                                    }

                                    string newValue = AttributeList[box.SelectedIndex];
                                    string oldValue = (string)tbr[parameterName];

                                    if (newValue != oldValue)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = newValue;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }

                            };
                        }
                        else if (controlType == "Attachment")
                        {
                            List<string> items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                            if (StateData.Attachments.Count > 0)
                            {
                                items.AddRange(StateData.Attachments.Keys);
                            }

                            ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = items, SelectedIndex = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            AttachmentSelectors.Add(box);

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, null);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;

                                items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                                if (StateData.Attachments.Count > 0)
                                {
                                    items.AddRange(StateData.Attachments.Keys);
                                }

                                if (value is Attachment att)
                                {
                                    box.SelectedIndex = items.IndexOf(att.Name);
                                    tbr[parameterName] = value;
                                }
                                else
                                {
                                    box.SelectedIndex = 0;
                                    tbr[parameterName] = null;
                                }

                                programmaticUpdate = false;
                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    if (box.SelectedIndex <= 0 || box.SelectedIndex >= StateData.Attachments.Count + 1)
                                    {
                                        box.SelectedIndex = 0;
                                        tbr[parameterName] = null;
                                        return;
                                    }

                                    items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                                    if (StateData.Attachments.Count > 0)
                                    {
                                        items.AddRange(StateData.Attachments.Keys);
                                    }

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = StateData.Attachments[items[box.SelectedIndex]];

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Node")
                        {
                            string[] defaultValue = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            ((TextBlock)paramPanel.Children[0]).VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                            ((TextBlock)paramPanel.Children[0]).Margin = new Thickness(0, 10, 0, 5);

                            Expander exp = new Expander() { Margin = new Thickness(5, 0, 0, 0) };

                            Grid grd = new Grid();
                            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                            TrimmedTextBox blk = new TrimmedTextBox(108) { Text = (defaultValue.Length > 1 ? "LCA of " : "") + defaultValue.Aggregate((a, b) => a + ", " + b), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            grd.Children.Add(blk);

                            Button control = new Button() { Content = "Edit", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(5, 5, 0, 5) };

                            Grid.SetColumn(exp, 1);
                            Grid.SetColumn(control, 1);

                            grd.Children.Add(control);

                            exp.Label = grd;

                            paramPanel.Children.Add(exp);

                            ScrollViewer valueScroller = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 0, 16) };

                            StackPanel valueContainer = new StackPanel();

                            for (int j = 0; j < defaultValue.Length; j++)
                            {
                                valueContainer.Children.Add(new TextBlock() { Text = defaultValue[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic });
                            }

                            tbr.Add(parameterName, defaultValue);

                            parameterUpdaters.Add(parameterName, (value) =>
                            {
                                tbr[parameterName] = value;
                                blk.Text = (((string[])value).Length > 1 ? "LCA of " : "") + ((string[])value).Aggregate((a, b) => a + ", " + b);
                                valueContainer.Children.Clear();
                                for (int j = 0; j < ((string[])value).Length; j++)
                                {
                                    valueContainer.Children.Add(new TextBlock() { Text = ((string[])value)[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic });
                                }
                            });

                            valueScroller.Content = valueContainer;

                            exp.Child = valueScroller;

                            control.Click += async (s, e) =>
                            {
                                NodeChoiceWindow win;
                                int index = FurtherTransformationsParameters.IndexOf(tbr);
                                if (index >= 0 && index < AllTransformedTrees.Length)
                                {
                                    win = new NodeChoiceWindow(AllTransformedTrees[index], (string[])tbr[parameterName]);
                                }
                                else
                                {
                                    win = new NodeChoiceWindow(TransformedTree, (string[])tbr[parameterName]);
                                }

                                await win.ShowDialog2(this);

                                if (win.Result != null)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = win.Result;

                                    blk.Text = (win.Result.Length > 1 ? "LCA of " : "") + win.Result.Aggregate((a, b) => a + ", " + b);
                                    valueContainer.Children.Clear();
                                    for (int j = 0; j < win.Result.Length; j++)
                                    {
                                        valueContainer.Children.Add(new TextBlock() { Text = win.Result[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic });
                                    }

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };

                        }
                        else if (controlType == "NumericUpDown")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (range.Length > 2)
                            {
                                increment = double.Parse(range[2], System.Globalization.CultureInfo.InvariantCulture);
                            }

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = Extensions.GetFormatString(increment);

                            if (range.Length > 3)
                            {
                                formatString = range[3];
                            }

                            NumericUpDown nud = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = formatString, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            Grid.SetColumn(nud, 1);

                            paramPanel.Children.Add(nud);

                            tbr.Add(parameters[i].Item1, nud.Value);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nud.Value = (double)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nud.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = nud.Value;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "NumericUpDownByNode")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            NumericUpDown nud = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = Extensions.GetFormatString(increment), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            Grid.SetColumn(nud, 1);

                            paramPanel.Children.Add(nud);

                            paramPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));


                            //Button butEdit = new Button() { Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Content = "..." };
                            VerticalButton butEdit = new VerticalButton() { Margin = new Thickness(5, 0, 0, 0), Width = 6, Height = 30 };
                            Grid.SetColumn(butEdit, 2);
                            paramPanel.Children.Add(butEdit);

                            object[] formatterParams = new object[4];

                            string attrType = range[4];

                            if (attrType == "String")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }

                            tbr.Add(parameters[i].Item1, new NumberFormatterOptions(range[2]) { AttributeName = range[3], AttributeType = attrType, DefaultValue = defaultValue, Parameters = formatterParams });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nud.Value = ((NumberFormatterOptions)value).DefaultValue;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nud.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    ((NumberFormatterOptions)tbr[parameterName]).DefaultValue = nud.Value;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };

                            butEdit.PointerReleased += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    NumberFormatterOptions opt = (NumberFormatterOptions)tbr[parameterName];

                                    NumberFormatterWindow win = new NumberFormatterWindow();

                                    string editorId = "NumberFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.Initialize(opt.AttributeName, opt.AttributeType, opt.DefaultValue, opt.Parameters, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result)
                                    {

                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Formatter;
                                        nud.Value = win.Formatter.DefaultValue;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "Slider")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            StackPanel container = new StackPanel();
                            Grid.SetColumn(container, 1);
                            paramPanel.Children.Add(container);

                            Slider slid = new Slider() { Margin = new Thickness(5, -15, 0, 0), Minimum = minRange, Maximum = maxRange, Value = defaultValue, LargeChange = increment, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            slid.Resources.Add("SliderHorizontalThumbWidth", 5);
                            slid.Resources.Add("SliderHorizontalThumbHeight", 20);
                            slid.Resources.Add("SliderPreContentMargin", 0.0);
                            slid.Resources.Add("SliderPostContentMargin", 0.0);

                            container.Children.Add(slid);

                            NumericUpDown valueBlock = null;

                            if (range.Length > 2)
                            {
                                valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Value = slid.Value, FormatString = range[2], Minimum = minRange, Maximum = maxRange, Increment = increment, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                container.Children.Add(valueBlock);
                            }
                            else
                            {
                                valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Value = slid.Value, FormatString = Extensions.GetFormatString(increment), Minimum = minRange, Maximum = maxRange, Increment = increment, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                container.Children.Add(valueBlock);
                            }

                            tbr.Add(parameters[i].Item1, slid.Value);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                slid.Value = (double)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            slid.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == Slider.ValueProperty)
                                {
                                    bool prevProgUpd = programmaticUpdate;
                                    programmaticUpdate = true;
                                    valueBlock.Value = slid.Value;
                                    programmaticUpdate = prevProgUpd;


                                    if (!programmaticUpdate)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = slid.Value;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };


                            if (valueBlock != null)
                            {
                                valueBlock.ValueChanged += (s, e) =>
                                {
                                    bool prevProgUpd = programmaticUpdate;
                                    programmaticUpdate = true;
                                    slid.Value = valueBlock.Value;
                                    programmaticUpdate = prevProgUpd;

                                    if (!programmaticUpdate)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = slid.Value;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                };
                            }
                        }
                        else if (controlType == "Font")
                        {
                            string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Font fnt = new VectSharp.Font(new VectSharp.FontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                            FontButton but = new FontButton() { FontSize = 15, Font = fnt, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, fnt);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Font = (VectSharp.Font)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.FontChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = but.Font;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Point")
                        {
                            double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            Grid grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            grid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                            grid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                            NumericUpDown nudX = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Increment = 1, Value = point[0], FormatString = Extensions.GetFormatString(point[0]), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
                            NumericUpDown nudY = new NumericUpDown() { Margin = new Thickness(5, 2, 0, 0), Padding = new Thickness(5, 0, 5, 0), Increment = 1, Value = point[1], FormatString = Extensions.GetFormatString(point[1]), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            TextBlock blkX = new TextBlock() { Text = "X:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                            TextBlock blkY = new TextBlock() { Text = "Y:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 2, 0, 0) };

                            Grid.SetColumn(grid, 1);

                            Grid.SetColumn(nudX, 1);

                            Grid.SetRow(blkY, 1);
                            Grid.SetRow(nudY, 1);
                            Grid.SetColumn(nudY, 1);



                            grid.Children.Add(blkX);
                            grid.Children.Add(nudX);
                            grid.Children.Add(blkY);
                            grid.Children.Add(nudY);

                            paramPanel.Children.Add(grid);

                            tbr.Add(parameters[i].Item1, new VectSharp.Point(nudX.Value, nudY.Value));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nudX.Value = ((VectSharp.Point)value).X;
                                nudY.Value = ((VectSharp.Point)value).Y;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nudX.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };

                            nudY.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Colour")
                        {
                            int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                            AvaloniaColorPicker.ColorButton but = new AvaloniaColorPicker.ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontFamily = this.FontFamily, FontSize = this.FontSize };

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, col);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Color = ((VectSharp.Colour)value).ToAvalonia();
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == AvaloniaColorPicker.ColorButton.ColorProperty)
                                {
                                    if (!programmaticUpdate)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = but.Color.ToVectSharp();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "SourceCode")
                        {
                            string defaultSource = controlParameters;

                            Button but = new Button() { Content = "Edit...", Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, new CompiledCode(defaultSource));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.Click += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    CodeEditorWindow win = new CodeEditorWindow();

                                    string editorId = "CodeEditor_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.FinishInitialization(((CompiledCode)tbr[parameterName]).SourceCode, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result != null)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Result;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "Dash")
                        {
                            double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);

                            DashControl control = new DashControl() { LineDash = lineDash, Margin = new Thickness(5, 0, 0, 0) };

                            Grid.SetColumn(control, 1);

                            paramPanel.Children.Add(control);

                            tbr.Add(parameters[i].Item1, lineDash);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                control.LineDash = (VectSharp.LineDash)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            control.DashChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = control.LineDash;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "ColourByNode")
                        {
                            string[] colour = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba(int.Parse(colour[3]), int.Parse(colour[4]), int.Parse(colour[5]), int.Parse(colour[6]));

                            StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                            AvaloniaColorPicker.ColorButton but = new AvaloniaColorPicker.ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontFamily = this.FontFamily, FontSize = this.FontSize };
                            pnl.Children.Add(but);

                            //Button butEdit = new Button() { Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Content = "...", Padding = new Thickness(5, 0, 5, 5) };
                            VerticalButton butEdit = new VerticalButton() { Margin = new Thickness(5, 0, 0, 0), Width = 6, Height = 30 };
                            pnl.Children.Add(butEdit);

                            Grid.SetColumn(pnl, 1);

                            paramPanel.Children.Add(pnl);

                            object[] formatterParams = new object[colour.Length - 5];

                            string attrType = colour[2];

                            if (attrType == "String")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = Convert.ToBoolean(colour[7]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = double.Parse(colour[7], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[2] = double.Parse(colour[8], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[3] = Modules.DefaultGradients[colour[9]];
                                formatterParams[4] = Convert.ToBoolean(colour[10]);
                            }

                            tbr.Add(parameters[i].Item1, new ColourFormatterOptions(colour[0]) { AttributeName = colour[1], AttributeType = attrType, DefaultColour = col, Parameters = formatterParams });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Color = ((ColourFormatterOptions)value).DefaultColour.ToAvalonia();
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == AvaloniaColorPicker.ColorButton.ColorProperty)
                                {
                                    if (!programmaticUpdate)
                                    {
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        ((ColourFormatterOptions)tbr[parameterName]).DefaultColour = but.Color.ToVectSharp();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };

                            butEdit.PointerReleased += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    ColourFormatterOptions opt = (ColourFormatterOptions)tbr[parameterName];

                                    ColourFormatterWindow win = new ColourFormatterWindow();

                                    string editorId = "ColourFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.Initialize(opt.AttributeName, opt.AttributeType, opt.DefaultColour, opt.Parameters, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result)
                                    {

                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Formatter;
                                        but.Color = win.Formatter.DefaultColour.ToAvalonia();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "AttributeType")
                        {
                            List<string> attributeTypes = new List<string>(Modules.AttributeTypes);

                            int defaultIndex = attributeTypes.IndexOf(controlParameters);

                            ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = attributeTypes, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, attributeTypes[box.SelectedIndex]);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = attributeTypes.IndexOf((string)value);
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = attributeTypes[box.SelectedIndex];

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        updateAction();
                                    }
                                }
                            };
                        }

                    }
                }

                parameterChangeDelegate(tbr, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                UpdateControls(controlStatus, parameterControls);
                UpdateParameters(parametersToChange, parameterUpdaters);

                UpdateParameterAction = (parametersToChange) =>
                {
                    Dictionary<string, object> previousParameters = tbr.ShallowClone();
                    UpdateParameters(parametersToChange, parameterUpdaters);

                    bool needsUpdate = parameterChangeDelegate(tbr, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange2);
                    UpdateControls(controlStatus, parameterControls);
                    UpdateParameters(parametersToChange2, parameterUpdaters);
                };
            }
            else
            {
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                }

                controlsPanel.Children.Add(new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Text = "No options available", FontStyle = FontStyle.Italic });
                UpdateParameterAction = (parametersToChange) =>
                {
                    UpdateParameters(parametersToChange, parameterUpdaters);
                };
            }

            parent.Child = controlsPanel;

            return tbr;
        }

        private void BuildTransformerPanel(string suggestedModuleId)
        {
            Expander exp = new Expander() { Margin = new Thickness(5, 0, 0, 5) };

            Grid transformerPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            transformerPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel) };

            transformerPanel.Children.Add(new TextBlock() { Text = "Transformer", FontWeight = Avalonia.Media.FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 18 });

            TransformerAlert = AlertPage.PaintToCanvas();
            TransformerAlert.Margin = new Thickness(5, 0, 0, 0);
            TransformerAlert.IsVisible = false;
            Grid.SetColumn(TransformerAlert, 1);

            transformerPanel.Children.Add(TransformerAlert);

            int moduleIndex = Math.Max(0, Modules.TransformerModules.IndexOf(Modules.GetModule(Modules.TransformerModules, suggestedModuleId)));

            List<string> transformerModules = (from el in Modules.TransformerModules select el.Name).ToList();
            TransformerComboBox = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = transformerModules, SelectedIndex = moduleIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
            Grid.SetColumn(TransformerComboBox, 2);
            transformerPanel.Children.Add(TransformerComboBox);

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 3);
            transformerPanel.Children.Add(helpButton);

            ToolTip.SetTip(helpButton, Modules.TransformerModules[TransformerComboBox.SelectedIndex].HelpText);

            helpButton.PointerReleased += (s, e) =>
            {
                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.TransformerModules[TransformerComboBox.SelectedIndex].Id].BuildReadmeMarkdown(), Modules.TransformerModules[TransformerComboBox.SelectedIndex].Id);

                win.Show(this);
            };

            exp.Label = transformerPanel;

            List<(string, string)> transformerParameters = Modules.TransformerModules[moduleIndex].GetParameters(Trees);

            transformerParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate transformerParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
                {
                    return Modules.TransformerModules[TransformerComboBox.SelectedIndex].OnParameterChange(Trees, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                };

            TransformerComboBox.SelectionChanged += async (s, e) =>
            {
                ToolTip.SetTip(helpButton, Modules.TransformerModules[TransformerComboBox.SelectedIndex].HelpText);
                List<(string, string)> parameters = Modules.TransformerModules[TransformerComboBox.SelectedIndex].GetParameters(Trees);
                parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));
                TransformerParameters = UpdateParameterPanel(exp, transformerParameterChange, parameters, async () => { await UpdateTransformedTree(); }, out UpdateTransformerParameters);
                await UpdateTransformedTree();
            };

            TransformerParameters = UpdateParameterPanel(exp, transformerParameterChange, transformerParameters, async () => { await UpdateTransformedTree(); }, out UpdateTransformerParameters);

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(exp);
        }


        private void BuildFurtherTransformationPanel()
        {
            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(new TextBlock() { Text = "Further transformations", FontWeight = Avalonia.Media.FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 18 });

            FurtherTransformations = new List<FurtherTransformationModule>();
            FurtherTransformationsAlerts = new List<Canvas>();
            FurtherTransformationsParameters = new List<Dictionary<string, object>>();
            UpdateFurtherTransformationParameters = new List<Action<Dictionary<string, object>>>();

            FurtherTransformationsContainer = new StackPanel();
            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(FurtherTransformationsContainer);

            StackPanel addButtonContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(2, 0, 0, 0) };

            AddRemoveButton addTransformationModule = new AddRemoveButton();

            addTransformationModule.PointerReleased += async (s, e) =>
            {
                AddFurtherTransformationModuleWindow win = new AddFurtherTransformationModuleWindow(FurtherTransformations);

                await win.ShowDialog2(this);

                if (win.Result != null)
                {
                    AddFurtherTransformation(win.Result);
                    await UpdateFurtherTransformations(FurtherTransformations.Count - 1);
                }
            };

            addButtonContainer.Children.Add(addTransformationModule);
            addButtonContainer.Children.Add(new TextBlock() { Text = "Add module", Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)), FontStyle = FontStyle.Italic, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) });

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(addButtonContainer);
        }

        public Action<Dictionary<string, object>> AddFurtherTransformation(FurtherTransformationModule module)
        {
            FurtherTransformations.Add(module);

            Expander exp = new Expander() { Margin = new Thickness(5, 0, 0, 5) };

            Grid modulePanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            modulePanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(24, GridUnitType.Pixel) };

            Canvas alert = AlertPage.PaintToCanvas();
            FurtherTransformationsAlerts.Add(alert);
            alert.Margin = new Thickness(5, 0, 0, 0);
            alert.IsVisible = false;

            modulePanel.Children.Add(alert);

            TextBlock moduleName = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = module.Name, FontWeight = FontWeight.Bold };

            Grid.SetColumn(moduleName, 1);

            modulePanel.Children.Add(moduleName);

            AddRemoveButton moveUp = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Up };
            Grid.SetColumn(moveUp, 3);
            modulePanel.Children.Add(moveUp);
            if (FurtherTransformations.Count <= 1)
            {
                moveUp.IsVisible = false;
            }

            moveUp.PointerReleased += async (s, e) =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                if (index > 0)
                {
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

                    Canvas alert = FurtherTransformationsAlerts[index];
                    FurtherTransformationsAlerts.RemoveAt(index);
                    FurtherTransformationsAlerts.Insert(index - 1, alert);

                    for (int i = 0; i < FurtherTransformationsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[2].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[2].IsVisible = true;
                        }

                        if (i == FurtherTransformationsContainer.Children.Count - 1)
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[3].IsVisible = true;
                        }
                    }

                    await UpdateFurtherTransformations(index - 1);
                }
            };

            AddRemoveButton moveDown = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Down };
            Grid.SetColumn(moveDown, 4);
            modulePanel.Children.Add(moveDown);
            moveDown.IsVisible = false;

            moveDown.PointerReleased += async (s, e) =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                if (index < FurtherTransformationsContainer.Children.Count - 1)
                {
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

                    Canvas alert = FurtherTransformationsAlerts[index];
                    FurtherTransformationsAlerts.RemoveAt(index);
                    FurtherTransformationsAlerts.Insert(index + 1, alert);

                    for (int i = 0; i < FurtherTransformationsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[2].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[2].IsVisible = true;
                        }

                        if (i == FurtherTransformationsContainer.Children.Count - 1)
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)FurtherTransformationsContainer.Children[i]).Label).Children[3].IsVisible = true;
                        }
                    }

                    await UpdateFurtherTransformations(index);
                }
            };

            AddRemoveButton remove = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove };
            Grid.SetColumn(remove, 5);
            modulePanel.Children.Add(remove);

            remove.PointerReleased += async (s, e) =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);

                FurtherTransformations.RemoveAt(index);
                FurtherTransformationsParameters.RemoveAt(index);
                UpdateFurtherTransformationParameters.RemoveAt(index);
                FurtherTransformationsContainer.Children.RemoveAt(index);
                FurtherTransformationsAlerts.RemoveAt(index);

                if (FurtherTransformations.Count > 0)
                {
                    ((Grid)((Expander)FurtherTransformationsContainer.Children[0]).Label).Children[2].IsVisible = false;
                    ((Grid)((Expander)FurtherTransformationsContainer.Children.Last()).Label).Children[3].IsVisible = false;
                }

                await UpdateFurtherTransformations(index);
            };

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 2);
            modulePanel.Children.Add(helpButton);

            ToolTip.SetTip(helpButton, module.HelpText);

            helpButton.PointerReleased += (s, e) =>
            {
                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[module.Id].BuildReadmeMarkdown(), module.Id);

                win.Show(this);
            };

            exp.Label = modulePanel;

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

            FurtherTransformationsParameters.Add(UpdateParameterPanel(exp, plottingParameterChange, moduleParameters, async () =>
            {
                int index = FurtherTransformationsContainer.Children.IndexOf(exp);
                await UpdateFurtherTransformations(index);
            }, out Action<Dictionary<string, object>> tbr));
            UpdateFurtherTransformationParameters.Add(tbr);

            FurtherTransformationsContainer.Children.Add(exp);

            if (FurtherTransformations.Count > 1)
            {
                ((Grid)((Expander)FurtherTransformationsContainer.Children[0]).Label).Children[3].IsVisible = true;
            }

            if (FurtherTransformations.Count > 2)
            {
                ((Grid)((Expander)FurtherTransformationsContainer.Children[FurtherTransformationsContainer.Children.Count - 2]).Label).Children[3].IsVisible = true;
            }

            return tbr;
        }

        public void RemoveFurtherTransformation(int index)
        {
            FurtherTransformations.RemoveAt(index);
            FurtherTransformationsParameters.RemoveAt(index);
            UpdateFurtherTransformationParameters.RemoveAt(index);
            FurtherTransformationsContainer.Children.RemoveAt(index);
            FurtherTransformationsAlerts.RemoveAt(index);

            if (FurtherTransformations.Count > 0)
            {
                ((Grid)((Expander)FurtherTransformationsContainer.Children[0]).Label).Children[2].IsVisible = false;
                ((Grid)((Expander)FurtherTransformationsContainer.Children.Last()).Label).Children[3].IsVisible = false;
            }
        }

        private void BuildCoordinatesPanel(string suggestedModuleId)
        {
            Expander exp = new Expander() { Margin = new Thickness(5, 0, 0, 5) };

            Grid coordinatesPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            coordinatesPanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(24, GridUnitType.Pixel) };

            coordinatesPanel.Children.Add(new TextBlock() { Text = "Coordinates", FontWeight = Avalonia.Media.FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 18 });

            CoordinatesAlert = AlertPage.PaintToCanvas();
            CoordinatesAlert.Margin = new Thickness(5, 0, 0, 0);
            CoordinatesAlert.IsVisible = false;
            Grid.SetColumn(CoordinatesAlert, 1);

            coordinatesPanel.Children.Add(CoordinatesAlert);

            int moduleIndex = Math.Max(0, Modules.CoordinateModules.IndexOf(Modules.GetModule(Modules.CoordinateModules, suggestedModuleId)));

            List<string> coordinateModules = (from el in Modules.CoordinateModules select el.Name).ToList();
            CoordinatesComboBox = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Items = coordinateModules, SelectedIndex = moduleIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
            Grid.SetColumn(CoordinatesComboBox, 2);
            coordinatesPanel.Children.Add(CoordinatesComboBox);


            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 3);
            coordinatesPanel.Children.Add(helpButton);

            ToolTip.SetTip(helpButton, Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].HelpText);

            helpButton.PointerReleased += (s, e) =>
            {
                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].Id].BuildReadmeMarkdown(), Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].Id);

                win.Show(this);
            };

            exp.Label = coordinatesPanel;

            List<(string, string)> coordinateParameters = Modules.CoordinateModules[moduleIndex].GetParameters(TransformedTree);
            coordinateParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate coordinateParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                return Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].OnParameterChange(TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
            };

            CoordinatesComboBox.SelectionChanged += (s, e) =>
            {
                ToolTip.SetTip(helpButton, Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].HelpText);
                List<(string, string)> parameters = Modules.CoordinateModules[CoordinatesComboBox.SelectedIndex].GetParameters(TransformedTree);
                parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));
                CoordinatesParameters = UpdateParameterPanel(exp, coordinateParameterChange, parameters, UpdateCoordinates, out UpdateCoordinatesParameters);
                UpdateCoordinates();
            };

            CoordinatesParameters = UpdateParameterPanel(exp, coordinateParameterChange, coordinateParameters, UpdateCoordinates, out UpdateCoordinatesParameters);

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(exp);
        }

        public Action<Dictionary<string, object>> SetCoordinateModule(CoordinateModule module)
        {
            int index = Modules.CoordinateModules.IndexOf(module);

            if (index != CoordinatesComboBox.SelectedIndex)
            {
                CoordinatesComboBox.SelectedIndex = index;
                return UpdateCoordinatesParameters;
            }
            else
            {
                return arg => { UpdateCoordinatesParameters(arg); UpdateCoordinates(); };
            }
        }

        public Action<Dictionary<string, object>> SetTransformerModule(TransformerModule module)
        {
            int index = Modules.TransformerModules.IndexOf(module);

            if (index != TransformerComboBox.SelectedIndex)
            {
                TransformerComboBox.SelectedIndex = index;
                return UpdateTransformerParameters;
            }
            else
            {
                return async arg => { UpdateTransformerParameters(arg); await UpdateTransformedTree(); };
            }
        }


        private void BuildPlottingPanel(Colour backgroundColour)
        {
            Grid plotElementsGrid = new Grid();
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            plotElementsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            plotElementsGrid.Children.Add(new TextBlock() { Text = "Plot elements", FontWeight = Avalonia.Media.FontWeight.Bold, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 5), FontSize = 18 });

            {
                TextBlock blk = new TextBlock() { Text = "Background: ", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(0, 0, 5, 0), FontSize = 13 };
                Grid.SetColumn(blk, 2);
                plotElementsGrid.Children.Add(blk);

                AvaloniaColorPicker.ColorButton btn = new AvaloniaColorPicker.ColorButton() { Color = backgroundColour.ToAvalonia(), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontFamily = this.FontFamily, FontSize = this.FontSize };
                Grid.SetColumn(btn, 3);
                plotElementsGrid.Children.Add(btn);

                btn.PropertyChanged += (s, e) =>
                {
                    if (e.Property == AvaloniaColorPicker.ColorButton.ColorProperty)
                    {
                        GraphBackground = btn.Color.ToVectSharp();
                    }
                };
            }


            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(plotElementsGrid);

            PlottingActions = new List<PlottingModule>();
            PlottingAlerts = new List<Canvas>();
            PlottingParameters = new List<Dictionary<string, object>>();
            UpdatePlottingParameters = new List<Action<Dictionary<string, object>>>();
            //PlottingTimings = new List<TextBlock>();

            PlottingActionsContainer = new StackPanel();
            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(PlottingActionsContainer);

            StackPanel addButtonContainer = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(2, 0, 0, 0) };

            AddRemoveButton addPlottingModule = new AddRemoveButton();

            addPlottingModule.PointerReleased += async (s, e) =>
            {
                AddPlottingModuleWindow win = new AddPlottingModuleWindow();

                await win.ShowDialog2(this);

                if (win.Result != null)
                {
                    AddPlottingModule(win.Result);

                    PlotCanvases.Add(null);
                    PlotBounds.Add((new VectSharp.Point(), new VectSharp.Point()));
                    SelectionCanvases.Add(null);

                    UpdatePlotLayer(PlotCanvases.Count - 1, false);
                }
            };

            addButtonContainer.Children.Add(addPlottingModule);
            addButtonContainer.Children.Add(new TextBlock() { Text = "Add module", Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)), FontStyle = FontStyle.Italic, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) });

            this.FindControl<StackPanel>("ParameterContainerPanel").Children.Add(addButtonContainer);
        }

        public void RemovePlottingModule(int index)
        {
            PlottingActions.RemoveAt(index);
            PlottingParameters.RemoveAt(index);
            PlottingActionsContainer.Children.RemoveAt(index);
            PlottingAlerts.RemoveAt(index);
            UpdatePlottingParameters.RemoveAt(index);
            //PlottingTimings.RemoveAt(index);

            if (PlottingActions.Count > 0)
            {
                ((Grid)((Expander)PlottingActionsContainer.Children[0]).Label).Children[2].IsVisible = false;
                ((Grid)((Expander)PlottingActionsContainer.Children.Last()).Label).Children[3].IsVisible = false;
            }
        }

        public Action<Dictionary<string, object>> AddPlottingModule(PlottingModule module)
        {
            if (module == null)
            {
                throw new NullReferenceException();
            }

            PlottingActions.Add(module);

            Expander exp = new Expander() { Margin = new Thickness(5, 0, 0, 5) };

            Grid modulePanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
            modulePanel.ColumnDefinitions = new ColumnDefinitions() { new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(1, GridUnitType.Star), new ColumnDefinition(0, GridUnitType.Auto), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(24, GridUnitType.Pixel), new ColumnDefinition(24, GridUnitType.Pixel) };

            Canvas alert = AlertPage.PaintToCanvas();
            PlottingAlerts.Add(alert);
            alert.Margin = new Thickness(5, 0, 0, 0);
            alert.IsVisible = false;

            modulePanel.Children.Add(alert);

            TextBlock moduleName = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = module.Name, FontWeight = FontWeight.Bold };

            Grid.SetColumn(moduleName, 1);

            modulePanel.Children.Add(moduleName);

            /*TextBlock timingBlock = new TextBlock() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = "?ms", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(0xFFC0C0C0), FontSize = 12 };

            Grid.SetColumn(timingBlock, 2);
            modulePanel.Children.Add(timingBlock);
            PlottingTimings.Add(timingBlock);*/

            HelpButton helpButton = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(helpButton, 2);
            modulePanel.Children.Add(helpButton);

            ToolTip.SetTip(helpButton, module.HelpText);

            helpButton.PointerReleased += (s, e) =>
            {
                HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[module.Id].BuildReadmeMarkdown(), module.Id);

                win.Show(this);
            };

            AddRemoveButton moveUp = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Up };
            Grid.SetColumn(moveUp, 3);
            modulePanel.Children.Add(moveUp);
            if (PlottingActions.Count <= 1)
            {
                moveUp.IsVisible = false;
            }

            moveUp.PointerReleased += (s, e) =>
            {
                int index = PlottingActionsContainer.Children.IndexOf(exp);

                if (index > 0)
                {
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

                    Canvas alert = PlottingAlerts[index];
                    PlottingAlerts.RemoveAt(index);
                    PlottingAlerts.Insert(index - 1, alert);

                    /*TextBlock timing = PlottingTimings[index];
                    PlottingTimings.RemoveAt(index);
                    PlottingTimings.Insert(index - 1, timing);*/

                    for (int i = 0; i < PlottingActionsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[3].IsVisible = true;
                        }

                        if (i == PlottingActionsContainer.Children.Count - 1)
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[4].IsVisible = true;
                        }
                    }

                    Canvas plotCanvas = PlotCanvases[index];
                    PlotCanvases.RemoveAt(index);
                    PlotCanvases.Insert(index - 1, plotCanvas);

                    (VectSharp.Point, VectSharp.Point) bounds = PlotBounds[index];
                    PlotBounds.RemoveAt(index);
                    PlotBounds.Insert(index - 1, bounds);

                    Canvas selectionCanvas = SelectionCanvases[index];
                    SelectionCanvases.RemoveAt(index);
                    SelectionCanvases.Insert(index - 1, selectionCanvas);

                    Canvas parentCanvas = this.FindControl<Canvas>("PlotCanvas");
                    parentCanvas.Children.RemoveAt(index);
                    parentCanvas.Children.Insert(index - 1, plotCanvas);

                    SelectionCanvas.Children.RemoveAt(index);
                    SelectionCanvas.Children.Insert(index - 1, selectionCanvas);

                    //UpdateAllPlotLayers();
                }
            };

            AddRemoveButton moveDown = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Down };
            Grid.SetColumn(moveDown, 4);
            modulePanel.Children.Add(moveDown);
            moveDown.IsVisible = false;

            moveDown.PointerReleased += (s, e) =>
            {
                int index = PlottingActionsContainer.Children.IndexOf(exp);

                if (index < PlottingActionsContainer.Children.Count - 1)
                {
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

                    Canvas alert = PlottingAlerts[index];
                    PlottingAlerts.RemoveAt(index);
                    PlottingAlerts.Insert(index + 1, alert);

                    /*TextBlock timing = PlottingTimings[index];
                    PlottingTimings.RemoveAt(index);
                    PlottingTimings.Insert(index + 1, timing);*/

                    for (int i = 0; i < PlottingActionsContainer.Children.Count; i++)
                    {
                        if (i == 0)
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[3].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[3].IsVisible = true;
                        }

                        if (i == PlottingActionsContainer.Children.Count - 1)
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[4].IsVisible = false;
                        }
                        else
                        {
                            ((Grid)((Expander)PlottingActionsContainer.Children[i]).Label).Children[4].IsVisible = true;
                        }
                    }

                    Canvas plotCanvas = PlotCanvases[index];
                    PlotCanvases.RemoveAt(index);
                    PlotCanvases.Insert(index + 1, plotCanvas);

                    (VectSharp.Point, VectSharp.Point) bounds = PlotBounds[index];
                    PlotBounds.RemoveAt(index);
                    PlotBounds.Insert(index + 1, bounds);

                    Canvas selectionCanvas = SelectionCanvases[index];
                    SelectionCanvases.RemoveAt(index);
                    SelectionCanvases.Insert(index + 1, selectionCanvas);


                    Canvas parentCanvas = this.FindControl<Canvas>("PlotCanvas");
                    parentCanvas.Children.RemoveAt(index);
                    parentCanvas.Children.Insert(index + 1, plotCanvas);

                    SelectionCanvas.Children.RemoveAt(index);
                    SelectionCanvas.Children.Insert(index + 1, selectionCanvas);

                    //UpdateAllPlotLayers();
                }
            };

            AddRemoveButton remove = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove };
            Grid.SetColumn(remove, 5);
            modulePanel.Children.Add(remove);

            remove.PointerReleased += (s, e) =>
            {
                int index = PlottingActionsContainer.Children.IndexOf(exp);

                PlottingActions.RemoveAt(index);
                PlottingParameters.RemoveAt(index);
                PlottingActionsContainer.Children.RemoveAt(index);
                PlottingAlerts.RemoveAt(index);
                UpdatePlottingParameters.RemoveAt(index);
                //PlottingTimings.RemoveAt(index);

                if (PlottingActions.Count > 0)
                {
                    ((Grid)((Expander)PlottingActionsContainer.Children[0]).Label).Children[3].IsVisible = false;
                    ((Grid)((Expander)PlottingActionsContainer.Children.Last()).Label).Children[4].IsVisible = false;
                }

                Canvas plotCanvas = PlotCanvases[index];
                PlotCanvases.RemoveAt(index);

                PlotBounds.RemoveAt(index);

                Canvas selectionCanvas = SelectionCanvases[index];
                SelectionCanvases.RemoveAt(index);

                Canvas parentCanvas = this.FindControl<Canvas>("PlotCanvas");

                if (plotCanvas != null)
                {
                    parentCanvas.Children.Remove(plotCanvas);
                }

                if (selectionCanvas != null)
                {
                    SelectionCanvas.Children.Remove(selectionCanvas);
                }

                UpdatePlotBounds();
            };

            exp.Label = modulePanel;

            List<(string, string)> moduleParameters = module.GetParameters(TransformedTree);
            moduleParameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

            GenericParameterChangeDelegate plottingParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
            {
                return module.OnParameterChange(TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
            };

            PlottingParameters.Add(UpdateParameterPanel(exp, plottingParameterChange, moduleParameters, () => { UpdatePlotLayer(PlottingActionsContainer.Children.IndexOf(exp), true); }, out Action<Dictionary<string, object>> updateParameterAction));

            UpdatePlottingParameters.Add(updateParameterAction);

            PlottingActionsContainer.Children.Add(exp);

            if (PlottingActions.Count > 1)
            {
                ((Grid)((Expander)PlottingActionsContainer.Children[0]).Label).Children[4].IsVisible = true;
            }

            if (PlottingActions.Count > 2)
            {
                ((Grid)((Expander)PlottingActionsContainer.Children[PlottingActionsContainer.Children.Count - 2]).Label).Children[4].IsVisible = true;
            }

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

        bool IsSelectionExpanded = false;

        public void ExpandSelection()
        {
            ((TranslateTransform)this.FindControl<Grid>("SelectionGrid").RenderTransform).X = 0;
            ((TranslateTransform)this.FindControl<Button>("ShowHideSelectionButton").RenderTransform).X = 0;
            ((RotateTransform)this.FindControl<Canvas>("ShowHideSelectionIconCanvas").RenderTransform).Angle = 0;
            IsSelectionExpanded = true;
        }

        public void ReduceSelection()
        {
            ((TranslateTransform)this.FindControl<Grid>("SelectionGrid").RenderTransform).X = 300;
            ((TranslateTransform)this.FindControl<Button>("ShowHideSelectionButton").RenderTransform).X = 300;
            ((RotateTransform)this.FindControl<Canvas>("ShowHideSelectionIconCanvas").RenderTransform).Angle = 180;
            IsSelectionExpanded = false;
        }

        private void ShowHideSelectionClicked(object sender, RoutedEventArgs e)
        {
            if (IsSelectionExpanded)
            {
                ReduceSelection();
            }
            else
            {
                ExpandSelection();
            }
        }

        private void TranslationPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == TranslateTransform.XProperty)
            {
                if ((double)e.NewValue == 300)
                {
                    Grid.SetColumnSpan(this.FindControl<StackPanel>("ActionsPanel"), 3);
                    this.FindControl<StackPanel>("ActionsPanel").Margin = new Thickness(10, 0, 20, 0);
                    Grid.SetColumnSpan(this.FindControl<Grid>("ZoomPanel"), 3);
                    this.FindControl<Grid>("ZoomPanel").Margin = new Thickness(10, 0, 20, 0);

                }
                else
                {
                    Grid.SetColumnSpan(this.FindControl<StackPanel>("ActionsPanel"), 1);
                    this.FindControl<StackPanel>("ActionsPanel").Margin = new Thickness(10, 0, 10, 0);
                    if (this.Width >= 976)
                    {
                        Grid.SetColumnSpan(this.FindControl<Grid>("ZoomPanel"), 1);
                        this.FindControl<Grid>("ZoomPanel").Margin = new Thickness(10, 0, 10, 0);

                    }
                }
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
                            SelectionActionActions[i]();
                            e.Handled = true;
                        }
                    }
                }

                for (int i = 0; i < Modules.ActionModules.Count; i++)
                {
                    if (e.Key == Modules.ActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutModifier))
                    {
                        Modules.ActionModules[i].PerformAction(this, this.StateData);
                        e.Handled = true;
                    }
                }

                for (int i = 0; i < Modules.MenuActionModules.Count; i++)
                {
                    if (e.Key == Modules.MenuActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutModifier))
                    {
                        await Modules.MenuActionModules[i].PerformAction(this);
                        e.Handled = true;
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
                            SelectionActionActions[i]();
                            e.Handled = true;
                        }
                    }
                }

                for (int i = 0; i < Modules.ActionModules.Count; i++)
                {
                    if (Modules.ActionModules[i].TriggerInTextBox && e.Key == Modules.ActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.ActionModules[i].ShortcutModifier))
                    {
                        Modules.ActionModules[i].PerformAction(this, this.StateData);
                        e.Handled = true;
                    }
                }

                for (int i = 0; i < Modules.MenuActionModules.Count; i++)
                {
                    if (Modules.MenuActionModules[i].TriggerInTextBox && e.Key == Modules.MenuActionModules[i].ShortcutKey && e.KeyModifiers == Modules.GetModifier(Modules.MenuActionModules[i].ShortcutModifier))
                    {
                        await Modules.MenuActionModules[i].PerformAction(this);
                        e.Handled = true;
                    }
                }
            }

            if (!e.Handled)
            {
                if (e.Key == Key.A && e.KeyModifiers == (Modules.ControlModifier | KeyModifiers.Shift))
                {
                    AutosaveWindow window = new AutosaveWindow(this);
                    await window.ShowDialog(this);
                }
                else if (e.Key == Key.R && e.KeyModifiers == Modules.ControlModifier)
                {
                    await GlobalSettings.Settings.ShowGlobalSettingsWindow(this);
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
                    await box.ShowDialog(this);

                    if (box.Result == MessageBox.Results.Yes)
                    {
                        Program.Reboot(new string[] { "--module-creator" }, true);
                        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                    }
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
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomTo(zoom / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height * 0.5, true);
            }
            UpdateSelectionWidth();
            programmaticZoomUpdate = false;
        }

        public void AutoFit()
        {
            double availableWidth = this.Width - (IsSelectionExpanded ? 633 : 333) - 30;
            double availableHeight = this.Height - 265 - 10;

            double maxZoomX = availableWidth / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width;
            double maxZoomY = availableHeight / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height;

            double deltaX = IsSelectionExpanded ? -150 : 0;

            double zoom = Math.Min(maxZoomX, maxZoomY);

            if (!double.IsNaN(zoom) && !double.IsInfinity(zoom) && zoom > 0)
            {
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomTo(1 / this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ZoomX, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Width * 0.5, this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").Child.Height * 0.5, true);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").BeginPanTo(0, 0);
                this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").ContinuePanTo(-this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetX + deltaX, -this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").OffsetY, true);

                SetZoom(zoom, false);
            }
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
                    await (win.ShowDialog(this));

                    if (win.Result)
                    {
                        if (!StateData.Attachments.ContainsKey(win.AttachmentName))
                        {
                            validResult = true;
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

                            await box.ShowDialog(this);
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
            this.FindControl<StackPanel>("AttachmentContainerPanel").Children.Clear();

            foreach (KeyValuePair<string, Attachment> kvp in StateData.Attachments)
            {
                AttachmentItem attachmentPanel = new AttachmentItem(kvp.Key);
                attachmentPanel.ItemDeleted += async (s, e) => { await UpdateTransformedTree(); UpdateAttachmentSelectors(); };

                this.FindControl<StackPanel>("AttachmentContainerPanel").Children.Add(attachmentPanel);

                UpdateAttachmentSelectors();
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

        private void FitZoomButtonClicked(object sender, RoutedEventArgs e)
        {
            AutoFit();
        }
    }
}
