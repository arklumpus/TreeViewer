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
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Media.Transformation;
using PhyloTree;
using Spreadalonia;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TreeViewer
{
    public partial class MainWindow
    {
        private RibbonTabContent RibbonActionPanel;
        private RibbonTabContent RibbonSelectionActionPanel;
        private RibbonTabContent RibbonAttachmentPanel;
        private RibbonTabContent RibbonModulesPanel;
        private SideTabHeader ModulesHeader;
        private SideTabHeader SelectionHeader;
        public RibbonFilePage RibbonFilePage;
        private HomePage HomePage;

        private RibbonTabContent[] RibbonTabs;

        public RibbonBar RibbonBar;

        private void BuildRibbon()
        {
            BuildRibbonActionPanel();
            BuildRibbonAttachmentPanel();
            BuildRibbonModulesPanel();
            BuildRibbonSelectionActionPanel();

            List<(string, RibbonTabContent)> menuTabs = BuildRibbonMenuPanels();

            RibbonBar bar = BuildRibbonBar(menuTabs);

            this.FindControl<Grid>("RibbonBarContainer").Children.Add(bar);
        }

        private RibbonBar BuildRibbonBar(List<(string, RibbonTabContent)> menuTabs)
        {
            RibbonBar bar;

            List<(string, bool)> barItems;

            int[] contextualIndices;
            int initialSelectedIndex;
            int selectionActionsIndices;

            barItems = new List<(string, bool)> { ("File", false), ("Actions", false), ("Attachments", false), ("Modules", false) };
            barItems.AddRange(from el in menuTabs select (el.Item1, false));

            barItems.Add(("Selection actions", true));

            contextualIndices = new int[] { 4 + menuTabs.Count };
            initialSelectedIndex = 1;
            selectionActionsIndices = contextualIndices[0];

            RibbonTabs = new RibbonTabContent[barItems.Count];

            RibbonTabs[0] = null;
            RibbonTabs[1] = RibbonActionPanel;
            RibbonTabs[2] = RibbonAttachmentPanel;
            RibbonTabs[3] = RibbonModulesPanel;
            RibbonTabs[selectionActionsIndices] = RibbonSelectionActionPanel;

            for (int i = 0; i < menuTabs.Count; i++)
            {
                RibbonTabs[4 + i] = menuTabs[i].Item2;
            }

            bar = new RibbonBar(barItems.ToArray());

            if (GlobalSettings.Settings.RibbonStyle == GlobalSettings.RibbonStyles.Colourful)
            {
                bar.Classes.Add("Colorful");
                this.FindControl<Grid>("CommandsContainer").Classes.Add("Colorful");
            }
            else
            {
                bar.Classes.Add("Grey");
                this.FindControl<Grid>("CommandsContainer").Classes.Add("Grey");
                this.FindControl<Canvas>("RibbonBarBackground").Background = new SolidColorBrush(Color.FromRgb(243, 243, 243));
            }

            bar.SelectedIndex = initialSelectedIndex;

            foreach (int i in contextualIndices)
            {
                bar.GridItems[i].IsVisible = false;
            }

            this.RibbonBar = bar;

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            bar.PropertyChanged += (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    int newIndex = (int)e.NewValue;
                    if (newIndex == 0)
                    {
                        bar.SelectedIndex = (int)e.OldValue;

                        this.HomePage.UpdateRecentFiles();

                        this.RibbonFilePage.IsVisible = true;
                        this.RibbonFilePage.Opacity = 1;
                        this.RibbonFilePage.RenderTransform = TransformOperations.Identity;
                        this.RibbonFilePage.FindControl<Grid>("ThemeGrid").RenderTransform = TransformOperations.Identity;

                        if (Modules.IsWindows)
                        {
                            this.FindControl<Grid>("TitleBarContainer2").IsVisible = true;
                            this.FindControl<Grid>("TitleBarContainer2").Opacity = 1;
                        }
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
            };

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == MainWindow.IsSelectionAvailableProperty)
                {
                    bool isAvailable = (bool)e.NewValue;

                    bar.GridItems[selectionActionsIndices].IsVisible = isAvailable;

                    if (isAvailable && bar.SelectedIndex == initialSelectedIndex)
                    {
                        bar.SelectedIndex = selectionActionsIndices;
                    }
                    else if (!isAvailable && bar.SelectedIndex == selectionActionsIndices)
                    {
                        bar.SelectedIndex = initialSelectedIndex;
                    }
                }
            };

            for (int i = 0; i < RibbonTabs.Length; i++)
            {
                if (RibbonTabs[i] != null)
                {
                    if (i != initialSelectedIndex)
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

            List<(string, string[], RibbonButton, int, Control, string)> commandList = new List<(string, string[], RibbonButton, int, Control, string)>();

            for (int i = 0; i < RibbonTabs.Length; i++)
            {
                if (RibbonTabs[i] != null)
                {
                    foreach (RibbonGroup g in RibbonTabs[i].RibbonGroups)
                    {
                        foreach (RibbonButton btn in g.RibbonButtons)
                        {
                            if (btn.SubItemsText.Count == 0)
                            {
                                commandList.Add((btn.ButtonText, new string[] { barItems[i].Item1, g.GroupName }, btn, 0, btn, btn.ShortcutText));
                            }
                            else if (!string.IsNullOrEmpty(btn.SubItemsText[0]))
                            {
                                for (int j = 0; j < btn.SubItemsText.Count; j++)
                                {
                                    commandList.Add((btn.SubItemsText[j], new string[] { barItems[i].Item1, g.GroupName, btn.ButtonText }, btn, j, btn.SubItems[j], btn.SubItemsShortcuts[j]));
                                }
                            }
                            else
                            {
                                commandList.Add((btn.ButtonText, new string[] { barItems[i].Item1, g.GroupName }, btn, -1, btn, btn.ShortcutText));

                                for (int j = 1; j < btn.SubItemsText.Count; j++)
                                {
                                    commandList.Add((btn.SubItemsText[j], new string[] { barItems[i].Item1, g.GroupName, btn.ButtonText }, btn, j - 1, btn.SubItems[j - 1], btn.SubItemsShortcuts[j - 1]));
                                }
                            }
                        }
                    }
                }
            }

            BuildCommandsBox(commandList);

            return bar;
        }

        private void BuildCommandsBox(List<(string, string[], RibbonButton, int, Control, string)> commandList)
        {
            Border popupBorder = new Border() { BorderThickness = new Thickness(1), Focusable = true, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, BoxShadow = new BoxShadows(new BoxShadow() { Blur = 4, Color = Color.FromArgb(80, 0, 0, 0), Spread = 0 }) };

            popupBorder.Classes.Add("MenuBorder");

            StackPanel subMenu = new StackPanel() { Background = Brushes.Transparent };

            List<StackPanel> menuItems = new List<StackPanel>();

            List<Control> commandItems = new List<Control>(commandList.Count);
            List<double> commandItemWidths = new List<double>();

            for (int i = 0; i < commandList.Count; i++)
            {
                Grid itemContainer = new Grid();

                StackPanel item = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Height = 24 };
                item.Classes.Add("MenuItem");

                double pathWidth = 0;

                for (int j = 0; j < commandList[i].Item2.Length; j++)
                {
                    TextBlock path = new TextBlock() { Text = commandList[i].Item2[j], VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(5, 0, 5, 0), Opacity = 0.55 };

                    if (j == 0 && GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                    {
                        path.Margin = new Thickness(10, 0, 5, 0);
                    }

                    item.Children.Add(path);
                    Canvas can = new Canvas() { Width = 4, Height = 8, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    can.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Geometry.Parse("M0,0 L4,4 L0,8"), StrokeThickness = 1, Stroke = Brushes.Black, Fill = Brushes.Transparent, Opacity = 0.55 });
                    item.Children.Add(can);

                    pathWidth += AvaloniaBugFixes.MeasureTextWidth(commandList[i].Item2[j], Modules.UIFontFamily, FontStyle.Normal, FontWeight.Regular, 13) + 14;
                }

                TextBlock text = new TextBlock() { Text = commandList[i].Item1, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(5, 0, 5, 0) };
                item.Children.Add(text);

                if (!string.IsNullOrEmpty(commandList[i].Item6))
                {
                    TextBlock shortcut = new TextBlock() { Text = commandList[i].Item6, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(15, 0, 5, 0), Opacity = 0.55, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right };
                    item.Children.Add(shortcut);

                    commandItemWidths.Add(34 + (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle ? 8 : 0) + AvaloniaBugFixes.MeasureTextWidth(commandList[i].Item1, Modules.UIFontFamily, FontStyle.Normal, FontWeight.Regular, 13) + AvaloniaBugFixes.MeasureTextWidth(commandList[i].Item6, Modules.UIFontFamily, FontStyle.Normal, FontWeight.Regular, 13) + pathWidth);

                    if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                    {
                        shortcut.Margin = new Thickness(15, 0, 10, 0);
                    }
                }
                else
                {
                    commandItemWidths.Add(15 + (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle ? 8 : 0) + AvaloniaBugFixes.MeasureTextWidth(commandList[i].Item1, Modules.UIFontFamily, FontStyle.Normal, FontWeight.Regular, 13) + pathWidth);
                    if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                    {
                        text.Margin = new Thickness(5, 0, 10, 0);
                    }
                }

                commandItems.Add(item);

                int index = i;

                item.PointerPressed += (s, e) =>
                {
                    e.Pointer.Capture(item);
                };

                item.PointerReleased += (s, e) =>
                {
                    Point pos = e.GetCurrentPoint(item).Position;

                    if (pos.X >= 0 && pos.Y >= 0 && pos.X <= item.Bounds.Width && pos.Y <= item.Bounds.Height)
                    {
                        commandList[index].Item3.InvokeAction(commandList[index].Item4);
                    }

                    CloseSubMenu();
                };

                item.PropertyChanged += (s, e) =>
                {
                    if (e.Property == StackPanel.IsEnabledProperty)
                    {
                        if ((bool)e.NewValue)
                        {
                            item.Opacity = 1;
                        }
                        else
                        {
                            item.Opacity = 0.35;
                        }
                    }
                };
            }

            ScrollViewer menuScrollViewer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };

            menuScrollViewer.Content = subMenu;
            popupBorder.Child = menuScrollViewer;

            bool isPopupOpen = false;

            Grid commandsContainer = this.FindControl<Grid>("CommandsContainer");

            void OpenSubMenu(double width)
            {
                Grid popupLayer = this.FindControl<Grid>("PopupLayer");
                Point finalPos = popupLayer.PointToClient(commandsContainer.PointToScreen(new Point(0, commandsContainer.Bounds.Height + 1)));

                TransformOperations.Builder builder;

                if (!isPopupOpen)
                {
                    (popupBorder.Parent as Grid)?.Children.Remove(popupBorder);

                    Point pos = popupLayer.PointToClient(commandsContainer.PointToScreen(new Point(0, commandsContainer.Bounds.Height + 1 - 16)));

                    builder = new TransformOperations.Builder(1);
                    builder.AppendTranslate(Math.Max(Math.Min(pos.X, popupLayer.Bounds.Width - width), 0), pos.Y);
                    popupBorder.RenderTransform = builder.Build();
                    popupBorder.Opacity = 0;


                    popupLayer.Children.Add(popupBorder);
                }

                double maxHeight = popupLayer.Bounds.Height - finalPos.Y - 10;
                popupBorder.MaxHeight = maxHeight;

                builder = new TransformOperations.Builder(1);
                builder.AppendTranslate(Math.Max(Math.Min(finalPos.X, popupLayer.Bounds.Width - width), 0), finalPos.Y);
                popupBorder.RenderTransform = builder.Build();
                popupBorder.Opacity = 1;
                isPopupOpen = true;
            }

            int defaultCommand = -1;

            void CloseSubMenu()
            {
                (popupBorder.Parent as Grid)?.Children.Remove(popupBorder);
                isPopupOpen = false;
                defaultCommand = -1;
            }

            popupBorder.LostFocus += (s, e) => CloseSubMenu();

            popupBorder.Transitions = new Avalonia.Animation.Transitions()
            {
                new Avalonia.Animation.TransformOperationsTransition() { Property = Border.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new Avalonia.Animation.DoubleTransition() { Property = Border.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }
            };


            this.FindControl<TextBox>("CommandBox").PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.IsFocusedProperty)
                {
                    if ((bool)e.NewValue)
                    {
                        this.FindControl<Grid>("CommandsContainer").Classes.Add("focus");
                    }
                    else
                    {
                        this.FindControl<Grid>("CommandsContainer").Classes.Remove("focus");
                        this.FindControl<TextBox>("CommandBox").Text = "";
                    }
                }
                else if (e.Property == TextBox.TextProperty)
                {
                    string text = (string)e.NewValue;

                    if (string.IsNullOrEmpty(text))
                    {
                        CloseSubMenu();
                    }
                    else
                    {
                        List<(string, int, int)> matchedItems = new List<(string, int, int)>();

                        for (int i = 0; i < commandList.Count; i++)
                        {
                            if (commandList[i].Item3.IsEnabled && commandList[i].Item5.IsEnabled)
                            {
                                if (commandList[i].Item1.Contains(text, StringComparison.OrdinalIgnoreCase))
                                {
                                    matchedItems.Add((commandList[i].Item1, i, 0));
                                }
                                else
                                {
                                    for (int j = 0; j < commandList[i].Item2.Length; j++)
                                    {
                                        if (commandList[i].Item2[j].Contains(text, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matchedItems.Add((commandList[i].Item1, i, 1));
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        if (matchedItems.Count > 0)
                        {

                            double width = 0;

                            matchedItems = matchedItems.OrderBy(x => x.Item3).ThenBy(x => x.Item1).ThenBy(x => x.Item2).ToList();

                            subMenu.Children.Clear();

                            for (int i = 0; i < Math.Min(5, matchedItems.Count); i++)
                            {
                                subMenu.Children.Add(commandItems[matchedItems[i].Item2]);
                                width = Math.Max(width, commandItemWidths[matchedItems[i].Item2]);
                            }

                            defaultCommand = matchedItems[0].Item2;

                            OpenSubMenu(width);
                        }
                        else
                        {
                            CloseSubMenu();
                        }
                    }
                }
            };

            this.FindControl<TextBox>("CommandBox").KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    if (defaultCommand >= 0)
                    {
                        commandList[defaultCommand].Item3.InvokeAction(commandList[defaultCommand].Item4);
                        this.FindControl<TextBox>("CommandBox").Text = "";
                    }
                }
            };

        }

        public void BuildRibbonActionPanel()
        {
            Dictionary<string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>> tempStructure = new Dictionary<string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>>();

            for (int i = 0; i < Modules.ActionModules.Count; i++)
            {
                int index = i;

                if (!string.IsNullOrEmpty(Modules.ActionModules[i].GroupName))
                {
                    if (!tempStructure.TryGetValue(Modules.ActionModules[i].GroupName, out List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)> buttonList))
                    {
                        buttonList = new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>();
                        tempStructure[Modules.ActionModules[i].GroupName] = buttonList;
                    }

                    buttonList.Add((Modules.ActionModules[i].ButtonText, new DPIAwareBox(Modules.ActionModules[i].GetIcon), Modules.GetShortcutString(Modules.ActionModules[i].ShortcutKeys[0]), (from el in Enumerable.Range(0, Modules.ActionModules[i].SubItems.Count) select (Modules.ActionModules[i].SubItems[el].Item1, (Control)new DPIAwareBox(Modules.ActionModules[i].SubItems[el].Item2), Modules.GetShortcutString(Modules.ActionModules[i].ShortcutKeys[el + (string.IsNullOrEmpty(Modules.ActionModules[i].SubItems[0].Item1) ? 1 : 0)]))).ToList(), Modules.ActionModules[i].IsLargeButton, Modules.ActionModules[i].GroupIndex, async (ind) =>
                    {
                        try
                        {
                            Modules.ActionModules[index].PerformAction(ind, this, this.StateData);
                        }
                        catch (Exception ex)
                        {
                            await new MessageBox("Attention!", "An error occurred while performing the action!\n" + ex.Message).ShowDialog2(this);
                        }
                    }, Modules.ActionModules[i].HelpText));
                }
            }

            List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> structure = (from el in tempStructure orderby (from el2 in el.Value select el2.Item6).Min() ascending select (el.Key, el.Value)).ToList();

            foreach ((string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>) el in structure)
            {
                el.Item2.Sort((a, b) => a.Item6.CompareTo(b.Item6));
            }

            RibbonTabContent actionTab = new RibbonTabContent(structure);

            for (int i = 0; i < actionTab.RibbonButtons.Count; i++)
            {
                for (int j = 0; j < actionTab.RibbonButtons[i].Count; j++)
                {
                    actionTab.RibbonButtons[i][j].IsEnabled = false;
                }
            }

            Grid parent = (this.RibbonActionPanel?.Parent as Grid);

            int? childIndex = parent?.Children.IndexOf(this.RibbonActionPanel);

            parent?.Children.Remove(this.RibbonActionPanel);

            this.RibbonActionPanel = actionTab;

            if (childIndex != null && childIndex.Value > 0)
            {
                parent.Children.Insert(childIndex.Value, actionTab);
            }
            else
            {
                this.FindControl<Grid>("RibbonTabContainer").Children.Add(actionTab);
            }
        }

        public void BuildRibbonAttachmentPanel()
        {
            List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> structure = new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("New", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>() { ( "Add attachment", Icons.GetAddAttachmentIcon(1), null, new List<(string, Control, string)>() {
                   ("", null, null),
                   ("Add attachment from file", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.AddAttachment")) { Width = 16, Height = 16 }, null),
                   ("Open text editor", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.TextEditor")) { Width = 16, Height = 16 }, null),
                   ("Open spreadsheet editor", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.SpreadsheetEditor")) { Width = 16, Height = 16 }, null),
                }, true, 0, (Action<int>)(async ind =>
                {
                    if (ind < 1)
                    {
                        AddAttachmentClicked(null, null);
                    }
                    else if (ind == 1)
                    {
                        TextEditorWindow editorWin = new TextEditorWindow();

                        await editorWin.ShowDialog2(this);

                        if (editorWin.Result)
                        {
                            string attachmentText = editorWin.Text;

                            bool validResult = false;

                            string defaultName = "attachment";
                            bool loadInMemory = true;
                            bool cacheResults = true;

                            while (!validResult)
                            {
                                AddAttachmentWindow win = new AddAttachmentWindow(defaultName, loadInMemory, cacheResults);
                                await win.ShowDialog2(this);

                                if (win.Result)
                                {
                                    if (!StateData.Attachments.ContainsKey(win.AttachmentName))
                                    {
                                        Stream ms = new MemoryStream();
                                        StreamWriter writer = new StreamWriter(ms, leaveOpen: true);
                                        writer.Write(attachmentText);
                                        writer.Dispose();
                                        ms.Seek(0, SeekOrigin.Begin);

                                        validResult = true;

                                        this.PushUndoFrame(UndoFrameLevel.Attachment, 0);

                                        Attachment attachment = new Attachment(win.AttachmentName, win.CacheResults, win.LoadInMemory, ref ms);
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
                    else if (ind == 2)
                    {
                        SpreadsheetWindow editorWin = new SpreadsheetWindow(true, false, false);

                        await editorWin.ShowDialog2(this);

                        if (editorWin.Result)
                        {
                            string attachmentText = editorWin.Spreadsheet.SerializeData();

                            bool validResult = false;

                            string defaultName = "attachment";
                            bool loadInMemory = true;
                            bool cacheResults = true;

                            while (!validResult)
                            {
                                AddAttachmentWindow win = new AddAttachmentWindow(defaultName, loadInMemory, cacheResults);
                                await win.ShowDialog2(this);

                                if (win.Result)
                                {
                                    if (!StateData.Attachments.ContainsKey(win.AttachmentName))
                                    {
                                        Stream ms = new MemoryStream();
                                        StreamWriter writer = new StreamWriter(ms, leaveOpen: true);
                                        writer.Write(attachmentText);
                                        writer.Dispose();
                                        ms.Seek(0, SeekOrigin.Begin);

                                        validResult = true;

                                        this.PushUndoFrame(UndoFrameLevel.Attachment, 0);

                                        Attachment attachment = new Attachment(win.AttachmentName, win.CacheResults, win.LoadInMemory, ref ms);
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
                }), "Adds an attachment to the plot.") })
            };

            List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)> attachments = new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>();

            if (StateData?.Attachments?.Count > 0)
            {
                foreach (KeyValuePair<string, Attachment> kvp in StateData.Attachments)
                {
                    string attachmentName = kvp.Key;

                    attachments.Add((attachmentName, Icons.GetAttachmentIcon(1), null, new List<(string, Control, string)>()
                    {
                        ("Text editor", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.TextEditor")) { Width = 16, Height = 16 }, null),
                        ("Spreadsheet editor", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.SpreadsheetEditor")) { Width = 16, Height = 16 }, null),
                        ("Export attachment", Icons.GetDownloadIcon(1), null),
                        ("Replace attachment", Icons.GetReplaceIcon(1), null),
                        ("Delete attachment", Icons.GetCrossIcon(1), null),
                    }, true, 0, (Action<int>)(async (ind) =>
                                        {
                                            if (ind == 0)
                                            {
                                                Attachment att = this.StateData.Attachments[attachmentName];

                                                TextEditorWindow win = new TextEditorWindow() { Text = att.GetText() };

                                                await win.ShowDialog2(this);

                                                if (win.Result)
                                                {
                                                    Stream ms = new MemoryStream();
                                                    StreamWriter writer = new StreamWriter(ms, leaveOpen: true);
                                                    writer.Write(win.Text);
                                                    writer.Dispose();
                                                    ms.Seek(0, SeekOrigin.Begin);

                                                    Attachment attachment = new Attachment(att.Name, att.CacheResults, att.StoreInMemory, ref ms);
                                                    this.StateData.Attachments[attachment.Name] = attachment;
                                                    att.Dispose();
                                                    await UpdateOnlyTransformedTree();
                                                    RefreshAttachmentSelectors(attachment.Name);
                                                }
                                            }
                                            else if (ind == 1)
                                            {
                                                Attachment att = this.StateData.Attachments[attachmentName];

                                                SpreadsheetWindow win = new SpreadsheetWindow(true, false, false);
                                                win.Load(win.Spreadsheet, att.GetText(), false);

                                                int maxX = 0;

                                                foreach (KeyValuePair<(int, int), string> kvp in win.Spreadsheet.Data)
                                                {
                                                    maxX = Math.Max(maxX, kvp.Key.Item1);
                                                }

                                                maxX = Math.Min(maxX, 50);
                                                win.Spreadsheet.Selection = ImmutableList.Create(new SelectionRange(0, 0, maxX, win.Spreadsheet.MaxTableHeight));
                                                win.Spreadsheet.AutoFitWidth();
                                                win.Spreadsheet.Selection = ImmutableList.Create(new SelectionRange(0, 0));
                                                win.Spreadsheet.ScrollTopLeft();

                                                await win.ShowDialog2(this);

                                                if (win.Result)
                                                {
                                                    Stream ms = new MemoryStream();
                                                    StreamWriter writer = new StreamWriter(ms, leaveOpen: true);
                                                    writer.Write(win.Spreadsheet.SerializeData());
                                                    writer.Dispose();
                                                    ms.Seek(0, SeekOrigin.Begin);

                                                    Attachment attachment = new Attachment(att.Name, att.CacheResults, att.StoreInMemory, ref ms);
                                                    this.StateData.Attachments[attachment.Name] = attachment;
                                                    att.Dispose();
                                                    await UpdateOnlyTransformedTree();
                                                    RefreshAttachmentSelectors(attachment.Name);
                                                }
                                            }
                                            else if (ind == 4)
                                            {
                                                Attachment att = this.StateData.Attachments[attachmentName];
                                                this.StateData.Attachments.Remove(attachmentName);
                                                att.Dispose();
                                                BuildRibbonAttachmentPanel();
                                                await UpdateTransformedTree();
                                                UpdateAttachmentSelectors();
                                            }
                                            else if (ind == 3)
                                            {
                                                Attachment att = this.StateData.Attachments[attachmentName];

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

                                                    string defaultName = att.Name;
                                                    bool loadInMemory = att.StoreInMemory;
                                                    bool cacheResults = att.CacheResults;

                                                    while (!validResult)
                                                    {
                                                        AddAttachmentWindow win = new AddAttachmentWindow(defaultName, loadInMemory, cacheResults, false);
                                                        await (win.ShowDialog2(this));

                                                        if (win.Result)
                                                        {
                                                            validResult = true;
                                                            Attachment attachment = new Attachment(win.AttachmentName, win.CacheResults, win.LoadInMemory, result[0]);
                                                            this.StateData.Attachments[attachment.Name] = attachment;
                                                            att.Dispose();
                                                            await UpdateOnlyTransformedTree();
                                                            StopAllUpdates = true;
                                                            RefreshAttachmentSelectors(attachment.Name);
                                                            StopAllUpdates = false;
                                                            await UpdateFurtherTransformations(0);
                                                        }
                                                        else
                                                        {
                                                            validResult = true;
                                                        }
                                                    }
                                                }
                                            }
                                            else if (ind == 2)
                                            {
                                                SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string>() { "*" }, Name = "All files" } }, Title = "Export attachment" };

                                                string result = await dialog.ShowAsync(this);

                                                if (!string.IsNullOrEmpty(result))
                                                {
                                                    using (FileStream fs = new FileStream(result, FileMode.Create))
                                                    {
                                                        this.StateData.Attachments[attachmentName].WriteToStream(fs);
                                                    }
                                                }
                                            }
                                        }), kvp.Key));
                }

                structure.Add(("Attachments", attachments));
            }

            RibbonTabContent attachmentTab = new RibbonTabContent(structure);

            Grid parent = (this.RibbonAttachmentPanel?.Parent as Grid);

            int? childIndex = parent?.Children.IndexOf(this.RibbonAttachmentPanel);
            parent?.Children.Remove(this.RibbonAttachmentPanel);

            if (childIndex != null && childIndex.Value > 0)
            {
                parent.Children.Insert(childIndex.Value, attachmentTab);
            }
            else
            {
                this.FindControl<Grid>("RibbonTabContainer").Children.Add(attachmentTab);
            }

            if (this.RibbonTabs != null)
            {
                for (int i = 0; i < this.RibbonTabs.Length; i++)
                {
                    if (this.RibbonTabs[i] == this.RibbonAttachmentPanel)
                    {
                        this.RibbonTabs[i] = attachmentTab;
                    }
                }
            }

            this.RibbonAttachmentPanel = attachmentTab;
            attachmentTab.RibbonButtons[0][0].IsEnabled = this.IsTreeOpened;

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == IsTreeOpenedProperty)
                {
                    attachmentTab.RibbonButtons[0][0].IsEnabled = (bool)e.NewValue;
                }
            };


            if (this.AttachmentSelectors != null)
            {
                UpdateAttachmentSelectors();
            }
        }


        public List<(string, RibbonTabContent)> BuildRibbonMenuPanels()
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

            Dictionary<string, List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>)>> structure = new Dictionary<string, List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>)>>();


            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].Item1 != "File")
                {
                    List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>)> groups = new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>)>();

                    for (int j = 0; j < menuItems[i].Item2.Count; j++)
                    {
                        List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)> groupItems = new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>();

                        for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                        {
                            Func<int, MainWindow, Task> action = menuItems[i].Item2[j].Item2[k].PerformAction;

                            string shortcutString = null;

                            if (menuItems[i].Item2[j].Item2[k].ShortcutKeys.Count > 0)
                            {
                                shortcutString = Modules.GetShortcutString(menuItems[i].Item2[j].Item2[k].ShortcutKeys[0]);
                            }

                            groupItems.Add((menuItems[i].Item2[j].Item2[k].ItemText, new DPIAwareBox(menuItems[i].Item2[j].Item2[k].GetIcon), shortcutString, (from el in Enumerable.Range(0, menuItems[i].Item2[j].Item2[k].SubItems.Count) select (menuItems[i].Item2[j].Item2[k].SubItems[el].Item1, (Control)new DPIAwareBox(menuItems[i].Item2[j].Item2[k].SubItems[el].Item2), Modules.GetShortcutString(menuItems[i].Item2[j].Item2[k].ShortcutKeys[el]))).ToList(), menuItems[i].Item2[j].Item2[k].IsLargeButton, menuItems[i].Item2[j].Item2[k].GroupIndex, (Action<int>)(async ind =>
                            {
                                await action(ind, this);
                            }), menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled, menuItems[i].Item2[j].Item2[k].IsEnabled, menuItems[i].Item2[j].Item2[k].HelpText));
                        }

                        groups.Add((menuItems[i].Item2[j].Item1, groupItems));
                    }

                    structure.Add(menuItems[i].Item1, groups);
                }
            }

            helpFound = false;

            foreach ((string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>) group in structure["Help"])
            {
                if (group.Item1 == "Help")
                {
                    helpFound = true;

                    group.Item2.Add(("About", new DPIAwareBox((scaling) =>
                    {
                        if (scaling <= 1)
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-32.png")), Width = 32, Height = 32 };
                        }
                        else if (scaling <= 1.5)
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-48.png")), Width = 32, Height = 32 };
                        }
                        else
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-64.png")), Width = 32, Height = 32 };
                        }
                    }), Modules.GetShortcutString((Avalonia.Input.Key.H, Avalonia.Input.KeyModifiers.Control)), new List<(string, Control, string)>(), true, 10000, (Action<int>)(async ind => { AboutWindow win2 = new AboutWindow(); await win2.ShowDialog2(this); }), null, a => new List<bool> { true }, "Displays information about TreeViewer."));
                }
            }

            if (!helpFound)
            {
                structure["Help"].Add(("Help", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>() { ("About", new DPIAwareBox((scaling) =>
                    {
                        if (scaling <= 1)
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-32.png")), Width = 32, Height = 32 };
                        }
                        else if (scaling <= 1.5)
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-48.png")), Width = 32, Height = 32 };
                        }
                        else
                        {
                            return new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.About-64.png")), Width = 32, Height = 32 };
                        }
                    }), Modules.GetShortcutString((Avalonia.Input.Key.H, Avalonia.Input.KeyModifiers.Control)), new List<(string, Control, string)>(), true, 10000, (Action<int>)(async ind => { AboutWindow win2 = new AboutWindow(); await win2.ShowDialog2(this); }), null, a => new List<bool>(){ true }, "Opens the TreeViewer online manual.") }));
            }

            List<(string, RibbonTabContent)> tbr = new List<(string, RibbonTabContent)>();

            foreach (KeyValuePair<string, List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>)>> tab in structure)
            {
                tab.Value.Sort((el1, el2) => (from el in el1.Item2 select el.Item6).Min().CompareTo((from el in el2.Item2 select el.Item6).Min()));

                foreach ((string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, List<AvaloniaProperty>, Func<MainWindow, List<bool>>, string)>) el in tab.Value)
                {
                    el.Item2.Sort((a, b) => a.Item6.CompareTo(b.Item6));
                }


                RibbonTabContent menuTab = new RibbonTabContent((from el in tab.Value select (el.Item1, (from el2 in el.Item2 select (el2.Item1, el2.Item2, el2.Item3, el2.Item4, el2.Item5, el2.Item6, el2.Item7, el2.Item10)).ToList())).ToList());
                this.FindControl<Grid>("RibbonTabContainer").Children.Add(menuTab);

                List<List<RibbonButton>> buttons = menuTab.RibbonButtons;


                for (int i = 0; i < tab.Value.Count; i++)
                {
                    for (int j = 0; j < tab.Value[i].Item2.Count; j++)
                    {
                        if (tab.Value[i].Item2[j].Item7 != null)
                        {
                            List<AvaloniaProperty> property = tab.Value[i].Item2[j].Item8;
                            Func<MainWindow, List<bool>> isEnabled = tab.Value[i].Item2[j].Item9;
                            RibbonButton relevantButton = buttons[i][j];

                            if (property != null && property.Count > 0)
                            {
                                this.PropertyChanged += (s, e) =>
                                {
                                    if (property.Contains(e.Property))
                                    {
                                        List<bool> enabled = isEnabled(this);

                                        relevantButton.IsEnabled = enabled[0];

                                        for (int k = 1; k < enabled.Count; k++)
                                        {
                                            relevantButton.SubItems[k - 1].IsEnabled = enabled[k];
                                        }
                                    }
                                };
                            }

                            List<bool> enabled = isEnabled(this);

                            relevantButton.IsEnabled = enabled[0];

                            for (int k = 1; k < enabled.Count; k++)
                            {
                                relevantButton.SubItems[k - 1].IsEnabled = enabled[k];
                            }
                        }
                    }
                }

                tbr.Add((tab.Key, menuTab));
            }

            BuildRibbonFilePage(menuItems);


            return tbr;
        }


        public void BuildRibbonSelectionActionPanel()
        {
            Dictionary<string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)>> tempStructure = new Dictionary<string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)>>();

            for (int i = 0; i < Modules.SelectionActionModules.Count; i++)
            {
                int index = i;

                if (!string.IsNullOrEmpty(Modules.SelectionActionModules[i].GroupName))
                {
                    if (!tempStructure.TryGetValue(Modules.SelectionActionModules[i].GroupName, out List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)> buttonList))
                    {
                        buttonList = new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)>();
                        tempStructure[Modules.SelectionActionModules[i].GroupName] = buttonList;
                    }

                    buttonList.Add((Modules.SelectionActionModules[i].ButtonText, new DPIAwareBox(Modules.SelectionActionModules[i].GetIcon), Modules.GetShortcutString((Modules.SelectionActionModules[i].ShortcutKey, Modules.SelectionActionModules[i].ShortcutModifier)), (from el in Enumerable.Range(0, Modules.SelectionActionModules[i].SubItems.Count) select (Modules.SelectionActionModules[i].SubItems[el].Item1, (Control)new DPIAwareBox(Modules.SelectionActionModules[i].SubItems[el].Item2), (string)null)).ToList(), Modules.SelectionActionModules[i].IsLargeButton, Modules.SelectionActionModules[i].GroupIndex, async (ind) =>
                    {
                        try
                        {
                            Modules.SelectionActionModules[index].PerformAction(ind, this.SelectedNode, this, this.StateData);
                        }
                        catch (Exception ex)
                        {
                            await new MessageBox("Attention!", "An error occurred while performing the action!\n" + ex.Message).ShowDialog2(this);
                        }
                    }, Modules.SelectionActionModules[i].IsAvailable, Modules.SelectionActionModules[i].HelpText));
                }
            }

            List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)>)> structure = (from el in tempStructure orderby (from el2 in el.Value select el2.Item6).Min() ascending select (el.Key, el.Value)).ToList();

            foreach ((string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, Func<TreeNode, MainWindow, InstanceStateData, List<bool>>, string)>) el in structure)
            {
                el.Item2.Sort((a, b) => a.Item6.CompareTo(b.Item6));
            }

            List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> realStructure = new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ( "Selection", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                    {
                        ("Selection info", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SelectionInfo")), null, new List<(string, Control, string)>(), true, -1, (ind) =>
                        {
                            if (this.IsSelectionAvailable)
                            {
                                this.IsSelectionPanelOpen = true;
                                this.SelectionHeader.SelectedIndex = 0;
                            }
                        }, "Opens the selection information panel."),
                        ("Attributes", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Attributes")), null, new List<(string, Control, string)>(), true, -1, (ind) =>
                        {
                            if (this.IsSelectionAvailable)
                            {
                                this.IsSelectionPanelOpen = true;
                                this.SelectionHeader.SelectedIndex = 1;
                            }
                        }, "Opens the attributes panel")
                    }
                )
            };

            realStructure.AddRange(from el in structure select (el.Item1, (from el2 in el.Item2 select (el2.Item1, el2.Item2, el2.Item3, el2.Item4, el2.Item5, el2.Item6, el2.Item7, el2.Item9)).ToList()));

            RibbonTabContent selectionActionTab = new RibbonTabContent(realStructure);

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == MainWindow.SelectedNodeProperty)
                {
                    TreeNode selectedNode = (TreeNode)e.NewValue;

                    selectionActionTab.RibbonButtons[0][0].IsEnabled = selectedNode != null;
                    selectionActionTab.RibbonButtons[0][1].IsEnabled = selectedNode != null;

                    for (int i = 0; i < structure.Count; i++)
                    {
                        for (int j = 0; j < structure[i].Item2.Count; j++)
                        {
                            List<bool> enabledStatus = structure[i].Item2[j].Item8(selectedNode, this, this.StateData);

                            selectionActionTab.RibbonButtons[i + 1][j].IsEnabled = enabledStatus[0];

                            for (int k = 1; k < enabledStatus.Count; k++)
                            {
                                selectionActionTab.RibbonButtons[i + 1][j].SubItems[k - 1].IsEnabled = enabledStatus[k];
                            }
                        }
                    }
                }
            };

            selectionActionTab.RibbonButtons[0][0].IsEnabled = false;
            selectionActionTab.RibbonButtons[0][1].IsEnabled = false;

            for (int i = 0; i < structure.Count; i++)
            {
                for (int j = 0; j < structure[i].Item2.Count; j++)
                {

                    selectionActionTab.RibbonButtons[i + 1][j].IsEnabled = false;

                    for (int k = 0; k < selectionActionTab.RibbonButtons[i + 1][j].SubItems.Count; k++)
                    {
                        selectionActionTab.RibbonButtons[i + 1][j].SubItems[k].IsEnabled = false;
                    }
                }
            }

            this.RibbonSelectionActionPanel = selectionActionTab;
            this.FindControl<Grid>("RibbonTabContainer").Children.Add(selectionActionTab);

            SideTabHeader selectionHeader = new SideTabHeader(new List<(Control, string)>()
            {
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SelectionInfo")), "Selection information"),
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Attributes")), "Attributes"),
            });

            this.FindControl<Grid>("SelectionTabHeaderContainer").Children.Add(selectionHeader);

            Grid[] tabs = new Grid[] { this.FindControl<Grid>("SelectionInfoTabContainer"), this.FindControl<Grid>("AttributeTabContainer") };

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            selectionHeader.PropertyChanged += (s, e) =>
            {
                if (e.Property == SideTabHeader.SelectedIndexProperty)
                {
                    int oldIndex = (int)e.OldValue;
                    int newIndex = (int)e.NewValue;

                    if (oldIndex >= 0)
                    {
                        tabs[oldIndex].Opacity = 0;
                        tabs[oldIndex].RenderTransform = offScreen;
                        tabs[oldIndex].IsHitTestVisible = false;
                        tabs[oldIndex].ZIndex = 0;

                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await Task.Delay(200);
                            tabs[oldIndex].IsVisible = false;
                        });
                    }

                    if (newIndex >= 0)
                    {
                        tabs[newIndex].IsVisible = true;
                        tabs[newIndex].ZIndex = 1;
                        tabs[newIndex].Opacity = 1;
                        tabs[newIndex].RenderTransform = TransformOperations.Identity;
                        tabs[newIndex].IsHitTestVisible = true;

                    }
                }
            };

            this.SelectionHeader = selectionHeader;

        }

        public void BuildRibbonModulesPanel()
        {
            List<(string, Control, string)> furtherTransformations = new List<(string, Control, string)>() { ("", null, null) };

            List<FurtherTransformationModule> builtFurtherTransformationModules = new List<FurtherTransformationModule>();

            foreach (FurtherTransformationModule module in from el in Modules.FurtherTransformationModules orderby el.Name ascending select el)
            {
                furtherTransformations.Add((module.Name, new DPIAwareBox(module.GetIcon) { Width = 16, Height = 16 }, null));
                builtFurtherTransformationModules.Add(module);
            }

            List<(string, Control, string)> plotActions = new List<(string, Control, string)>() { ("", null, null) };

            List<PlottingModule> builtPlotActionModules = new List<PlottingModule>();

            foreach (PlottingModule module in from el in Modules.PlottingModules orderby el.Name ascending select el)
            {
                plotActions.Add((module.Name, new DPIAwareBox(module.GetIcon) { Width = 16, Height = 16 }, null));
                builtPlotActionModules.Add(module);
            }

            List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> structure = new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Tree structure", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                    {
                        ( "Transformer module", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Transformer")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                        {
                            IsModulesPanelOpen = true;
                            this.ModulesHeader.SelectedIndex = 0;
                        }), "Opens the settings for the Transformer module."),
                        ( "Further transformations", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FurtherTransformations")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                        {
                            IsModulesPanelOpen = true;
                            this.ModulesHeader.SelectedIndex = 1;
                        }), "Opens the settings for the Further transformation modules."),
                        ( "Add further transformation", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AddFurtherTransformation")), null, furtherTransformations, true, 0, async ind => {
                            if (ind < 0)
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
                            }
                            else
                            {
                                PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, this.FurtherTransformations.Count);

                                List<string> childNames = null;

                                if (this.IsSelectionAvailable)
                                {
                                    childNames = this.SelectedNode.GetNodeNames();
                                }

                                AddFurtherTransformation(builtFurtherTransformationModules[ind]);
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
                            }, "Adds a new Further transformation module." )
                         }),
                ("Plot design", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                    {
                        ( "Coordinates module", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Coordinates")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(ind =>
                        {
                            IsModulesPanelOpen = true;
                            this.ModulesHeader.SelectedIndex = 2;
                        }), "Opens the settings for the Coordinates module."),
                        ( "Plot actions", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PlotActions")), null, new List<(string, Control, string)>(), true, 0,(Action<int>)(ind =>
                        {
                            IsModulesPanelOpen = true;
                            this.ModulesHeader.SelectedIndex = 3;
                        }), "Opens the settings for the Plot action modules."),
                        (  "Add plot action", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AddPlotAction")), null, plotActions, true, 0, async ind => {
                            if (ind < 0)
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
                            }
                            else
                            {
                                PushUndoFrame(UndoFrameLevel.PlotActionModule, this.PlottingActions.Count);
                                AddPlottingModule(builtPlotActionModules[ind]);

                                PlotCanvases.Add(null);
                                PlotBounds.Add((new VectSharp.Point(), new VectSharp.Point()));
                                SelectionCanvases.Add(null);
                                LayerTransforms.Add(null);

                                await ActuallyUpdatePlotLayer(PlotCanvases.Count - 1, false);
                            }
                        }, "Adds a new Plot action module.")
                    }),
                ("Manage", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                    {
                        ( "Module manager", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleManager")), Modules.GetShortcutString((Avalonia.Input.Key.M, Avalonia.Input.KeyModifiers.Control)), new List<(string, Control, string)>(), false, 0, (Action<int>)(async ind => {
                            ModuleManagerWindow win2 = new ModuleManagerWindow();
                            await win2.ShowDialog2(this);
                        }), "Opens the module manager window." ),
                        ( "Module repository", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleRepository")), null, new List<(string, Control, string)>()
                        {
                            ("", null, null),
                            ("Open module repository to install modules", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleRepository")), null),
                            ("Open module repository to temporarily load modules", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleRepository")), null),
                        }, false, 0, (Action<int>)(async ind => {
                             ModuleManagerWindow win2 = new ModuleManagerWindow();
                            _ = win2.ShowDialog2(this);

                            try
                            {
                                ModuleRepositoryWindow win = new ModuleRepositoryWindow(ind < 1 ? ModuleRepositoryWindow.Modes.Install : ModuleRepositoryWindow.Modes.Load, win2);
                                await win.ShowDialog2(win2);
                            }
                            catch (Exception ex)
                            {
                                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(win2);
                            }
                        }), "Opens the module repository window." ),
                        ( "Module creator", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ModuleCreator")), Modules.GetShortcutString((Avalonia.Input.Key.M, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift)), new List<(string, Control, string)>(), false, 0, (Action<int>)(async ind => {
                            MessageBox box = new MessageBox("Attention", "The program will now be rebooted to open the module creator (we will do our best to recover the files that are currently open). Do you wish to proceed?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                            await box.ShowDialog2(this);

                            if (box.Result == MessageBox.Results.Yes)
                            {
                                Program.Reboot(new string[] { "--module-creator" }, true);
                                if (!Modules.IsMac)
                                {
                                    ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                                }
                                else
                                {
                                    System.Diagnostics.Process.GetCurrentProcess().Kill();
                                }
                            }
                        }), "Closes the program and restarts it opening the module creator window." )
                    }),
            };

            RibbonTabContent modulesTab = new RibbonTabContent(structure);

            modulesTab.RibbonButtons[0][0].FindControl<Grid>("ButtonGrid").MaxWidth += 25;
            modulesTab.RibbonButtons[0][1].FindControl<Grid>("ButtonGrid").MaxWidth += 30;
            modulesTab.RibbonButtons[0][2].FindControl<Grid>("ButtonGrid").MaxWidth += 25;

            modulesTab.RibbonButtons[1][0].FindControl<Grid>("ButtonGrid").MaxWidth += 25;

            modulesTab.RibbonButtons[0][0].IsEnabled = false;
            modulesTab.RibbonButtons[0][1].IsEnabled = false;
            modulesTab.RibbonButtons[0][2].IsEnabled = false;

            modulesTab.RibbonButtons[1][0].IsEnabled = false;
            modulesTab.RibbonButtons[1][1].IsEnabled = false;
            modulesTab.RibbonButtons[1][2].IsEnabled = false;

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == IsTreeOpenedProperty)
                {
                    modulesTab.RibbonButtons[0][0].IsEnabled = (bool)e.NewValue;
                    modulesTab.RibbonButtons[0][1].IsEnabled = (bool)e.NewValue;
                    modulesTab.RibbonButtons[0][2].IsEnabled = (bool)e.NewValue;

                    modulesTab.RibbonButtons[1][0].IsEnabled = (bool)e.NewValue;
                    modulesTab.RibbonButtons[1][1].IsEnabled = (bool)e.NewValue;
                    modulesTab.RibbonButtons[1][2].IsEnabled = (bool)e.NewValue;
                }
            };


            this.FindControl<Grid>("RibbonTabContainer").Children.Add(modulesTab);

            SideTabHeader modulesHeader = new SideTabHeader(new List<(Control, string)>()
            {
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Transformer")), "Transformer module"),
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FurtherTransformations")), "Further transformations"),
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Coordinates")), "Coordinates module" ),
                (new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PlotActions")), "Plot actions")
            });

            this.FindControl<Grid>("ModuleTabHeaderContainer").Children.Add(modulesHeader);

            ScrollViewer[] tabs = new ScrollViewer[] { (ScrollViewer)this.FindControl<StackPanel>("TransformerModuleContainerPanel").Parent, (ScrollViewer)this.FindControl<StackPanel>("FurtherTransformationsContainerPanel").Parent, (ScrollViewer)this.FindControl<StackPanel>("CoordinatesModuleContainerPanel").Parent, (ScrollViewer)this.FindControl<StackPanel>("PlotActionsContainerPanel").Parent };

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            modulesHeader.PropertyChanged += (s, e) =>
            {
                if (e.Property == SideTabHeader.SelectedIndexProperty)
                {
                    int oldIndex = (int)e.OldValue;
                    int newIndex = (int)e.NewValue;

                    if (oldIndex >= 0)
                    {
                        tabs[oldIndex].Opacity = 0;
                        tabs[oldIndex].RenderTransform = offScreen;
                        tabs[oldIndex].IsHitTestVisible = false;
                        tabs[oldIndex].ZIndex = 0;

                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await Task.Delay(200);
                            tabs[oldIndex].IsVisible = false;
                        });
                    }

                    if (newIndex >= 0)
                    {
                        tabs[newIndex].IsVisible = true;
                        tabs[newIndex].ZIndex = 1;
                        tabs[newIndex].Opacity = 1;
                        tabs[newIndex].RenderTransform = TransformOperations.Identity;
                        tabs[newIndex].IsHitTestVisible = true;

                    }
                }
            };

            this.ModulesHeader = modulesHeader;

            this.RibbonModulesPanel = modulesTab;

            this.FindControl<Grid>("ParameterContainerGrid").IsHitTestVisible = false;
        }



        public static readonly Avalonia.StyledProperty<bool> IsModulesPanelOpenProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(IsModulesPanelOpen), false);
        public bool IsModulesPanelOpen
        {
            get { return GetValue(IsModulesPanelOpenProperty); }
            set { SetValue(IsModulesPanelOpenProperty, value); }
        }

        public static readonly Avalonia.StyledProperty<bool> IsSelectionPanelOpenProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(IsSelectionPanelOpen), false);
        public bool IsSelectionPanelOpen
        {
            get { return GetValue(IsSelectionPanelOpenProperty); }
            set { SetValue(IsSelectionPanelOpenProperty, value); }
        }

        private void OpenModulesPanel()
        {
            this.FindControl<GridSplitter>("ModulesGridSplitter").IsVisible = true;
            this.FindControl<Canvas>("ModulesGridSplitterCanvas").IsVisible = true;
            this.FindControl<Grid>("ParameterContainerGrid").MinWidth = 300;
            this.FindControl<Grid>("ParameterContainerGrid").IsHitTestVisible = true;

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(200);
                this.FindControl<Grid>("ParameterContainerGrid").Width = double.NaN;
            });
        }

        private void CloseModulesPanel()
        {
            double totalWidth = this.FindControl<Grid>("ParameterContainerGrid").Bounds.Width;
            Avalonia.Animation.Transitions myTransitions = this.FindControl<Grid>("ParameterContainerGrid").Transitions;

            this.FindControl<Grid>("ParameterContainerGrid").Transitions = null;

            this.FindControl<Grid>("ParameterContainerGrid").MinWidth = totalWidth;
            this.FindControl<Grid>("ParameterContainerGrid").Width = 0;

            this.FindControl<Grid>("MainGrid").ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);

            this.FindControl<Grid>("ParameterContainerGrid").Transitions = myTransitions;
            this.FindControl<Grid>("ParameterContainerGrid").MinWidth = 0;

            this.FindControl<GridSplitter>("ModulesGridSplitter").IsVisible = false;
            this.FindControl<Canvas>("ModulesGridSplitterCanvas").IsVisible = false;
            this.FindControl<Grid>("ParameterContainerGrid").IsHitTestVisible = false;
        }

        private void OpenSelectionPanel()
        {
            this.FindControl<GridSplitter>("SelectionGridSplitter").IsVisible = true;
            this.FindControl<Canvas>("SelectionGridSplitterCanvas").IsVisible = true;
            this.FindControl<Canvas>("SelectionGridBackground").IsVisible = true;
            this.FindControl<Grid>("SelectionGrid").MinWidth = 300;
            this.FindControl<Grid>("SelectionGrid").IsHitTestVisible = true;

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(200);
                this.FindControl<Grid>("SelectionGrid").Width = double.NaN;
            });
        }

        private void CloseSelectionPanel()
        {
            double totalWidth = this.FindControl<Grid>("SelectionGrid").Bounds.Width;
            Avalonia.Animation.Transitions myTransitions = this.FindControl<Grid>("SelectionGrid").Transitions;

            this.FindControl<Grid>("SelectionGrid").Transitions = null;

            this.FindControl<Grid>("SelectionGrid").MinWidth = totalWidth;
            this.FindControl<Grid>("SelectionGrid").Width = 0;

            this.FindControl<Grid>("MainGrid").ColumnDefinitions[5].Width = new GridLength(0, GridUnitType.Pixel);

            this.FindControl<Grid>("SelectionGrid").Transitions = myTransitions;
            this.FindControl<Grid>("SelectionGrid").MinWidth = 0;

            this.FindControl<GridSplitter>("SelectionGridSplitter").IsVisible = false;
            this.FindControl<Canvas>("SelectionGridBackground").IsVisible = false;
            this.FindControl<Canvas>("SelectionGridSplitterCanvas").IsVisible = false;
            this.FindControl<Grid>("SelectionGrid").IsHitTestVisible = false;
        }


        private void CloseModulePanelClicked(object sender, RoutedEventArgs e)
        {
            this.IsModulesPanelOpen = false;
        }

        private bool SelectionPanelManuallyClosed = false;

        private void CloseSelectionPanelClicked(object sender, RoutedEventArgs e)
        {
            this.IsSelectionPanelOpen = false;

            SelectionPanelManuallyClosed = true;
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == MainWindow.IsModulesPanelOpenProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    OpenModulesPanel();
                    if (WasAutoFitted)
                    {
                        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await Task.Delay(300);
                            AutoFit();
                        });
                    }
                }
                else
                {
                    CloseModulesPanel();

                    if (WasAutoFitted)
                    {
                        AutoFit();
                    }
                }


            }
            else if (change.Property == MainWindow.IsTreeOpenedProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    this.FindControl<Grid>("ZoomPanel").IsVisible = true;
                    this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer").IsVisible = true;
                }
            }
            else if (change.Property == MainWindow.IsSelectionPanelOpenProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    OpenSelectionPanel();
                    if (WasAutoFitted)
                    {
                        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await Task.Delay(300);
                            AutoFit();
                        });
                    }
                }
                else
                {
                    CloseSelectionPanel();

                    if (WasAutoFitted)
                    {
                        AutoFit();
                    }
                }


            }
        }

        public T GetFilePage<T>(out int index) where T : Control
        {
            for (int i = 0; i < this.RibbonFilePage.ClientItems.Count; i++)
            {
                if (this.RibbonFilePage.ClientItems[i] is T tbr)
                {
                    index = i;
                    return tbr;
                }
            }

            index = -1;
            return null;
        }

        private void BuildRibbonFilePage(List<(string, List<(string, List<MenuActionModule>)>)> menuItems)
        {
            HomePage home = new HomePage();

            List<(double, string, Control, Control, Func<int, MainWindow, Task>)> firstArea = new List<(double, string, Control, Control, Func<int, MainWindow, Task>)>()
            {
                (double.NegativeInfinity, "Home", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Home")){ Width = 16, Height = 16 }, home, (ind, win) => { home.UpdateRecentFiles(); return Task.CompletedTask; }),
                (double.NegativeInfinity, "Autosaves", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Autosaves")){ Width = 16, Height = 16 }, new AutosavesPage(), null)
            };

            List<(double, string, Control, Control, Func<int, MainWindow, Task>)> secondArea = new List<(double, string, Control, Control, Func<int, MainWindow, Task>)>();

            List<(double, string, Control, Control, Func<int, MainWindow, Task>)> thirdArea = new List<(double, string, Control, Control, Func<int, MainWindow, Task>)>();

            List<Action> enablers = new List<Action>();

            for (int i = 0; i < menuItems.Count; i++)
            {
                if (menuItems[i].Item1 == "File")
                {
                    menuItems[i] = (menuItems[i].Item1, menuItems[i].Item2.OrderBy(el => (from el2 in el.Item2 select el2.GroupIndex).Min()).ToList());

                    for (int j = 0; j < menuItems[i].Item2.Count; j++)
                    {
                        menuItems[i].Item2[j] = (menuItems[i].Item2[j].Item1, menuItems[i].Item2[j].Item2.OrderBy(el => el.GroupIndex).ToList());

                        if (menuItems[i].Item2[j].Item1 == Modules.FileMenuFirstAreaId)
                        {
                            for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                            {
                                firstArea.Add((menuItems[i].Item2[j].Item2[k].GroupIndex, menuItems[i].Item2[j].Item2[k].ItemText, new DPIAwareBox(menuItems[i].Item2[j].Item2[k].GetIcon) { Width = 16, Height = 16 }, menuItems[i].Item2[j].Item2[k].GetFileMenuPage?.Invoke(), menuItems[i].Item2[j].Item2[k].PerformAction));

                                string text = menuItems[i].Item2[j].Item2[k].ItemText;
                                Func<MainWindow, List<bool>> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                                if (menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled != null)
                                {
                                    List<AvaloniaProperty> prop = menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled;
                                    this.PropertyChanged += (s, e) =>
                                    {
                                        if (prop.Contains(e.Property))
                                        {
                                            this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                        }
                                    };
                                }

                                enablers.Add(() =>
                                {
                                    this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                });
                            }
                        }
                        else if (menuItems[i].Item2[j].Item1 == Modules.FileMenuThirdAreaId)
                        {
                            for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                            {
                                thirdArea.Add((menuItems[i].Item2[j].Item2[k].GroupIndex, menuItems[i].Item2[j].Item2[k].ItemText, new DPIAwareBox(menuItems[i].Item2[j].Item2[k].GetIcon) { Width = 16, Height = 16 }, menuItems[i].Item2[j].Item2[k].GetFileMenuPage?.Invoke(), menuItems[i].Item2[j].Item2[k].PerformAction));

                                string text = menuItems[i].Item2[j].Item2[k].ItemText;
                                Func<MainWindow, List<bool>> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                                if (menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled != null)
                                {
                                    List<AvaloniaProperty> prop = menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled;
                                    this.PropertyChanged += (s, e) =>
                                    {
                                        if (prop.Contains(e.Property))
                                        {
                                            this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                        }
                                    };
                                }

                                enablers.Add(() =>
                                {
                                    this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                });
                            }
                        }
                        else
                        {
                            for (int k = 0; k < menuItems[i].Item2[j].Item2.Count; k++)
                            {
                                secondArea.Add((menuItems[i].Item2[j].Item2[k].GroupIndex, menuItems[i].Item2[j].Item2[k].ItemText, new DPIAwareBox(menuItems[i].Item2[j].Item2[k].GetIcon) { Width = 16, Height = 16 }, menuItems[i].Item2[j].Item2[k].GetFileMenuPage?.Invoke(), menuItems[i].Item2[j].Item2[k].PerformAction));

                                string text = menuItems[i].Item2[j].Item2[k].ItemText;
                                Func<MainWindow, List<bool>> isEnabled = menuItems[i].Item2[j].Item2[k].IsEnabled;
                                if (menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled != null)
                                {
                                    List<AvaloniaProperty> prop = menuItems[i].Item2[j].Item2[k].PropertiesAffectingEnabled;
                                    this.PropertyChanged += (s, e) =>
                                    {
                                        if (prop.Contains(e.Property))
                                        {
                                            this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                        }
                                    };
                                }

                                enablers.Add(() =>
                                {
                                    this.RibbonFilePage.SetEnabled(text, isEnabled(this)[0]);
                                });
                            }
                        }
                    }
                }
            }

            secondArea.Add((double.PositiveInfinity, "Close", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Close")) { Width = 16, Height = 16 }, null, (ind, win) => { win.Close(); return Task.CompletedTask; }));

            thirdArea.Add((double.PositiveInfinity, "Preferences", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Preferences")) { Width = 16, Height = 16 }, new PreferencesPage(), null));

            firstArea = firstArea.OrderBy(x => x.Item1).ToList();
            secondArea = secondArea.OrderBy(x => x.Item1).ToList();
            thirdArea = thirdArea.OrderBy(x => x.Item1).ToList();

            RibbonFilePage pag = new RibbonFilePage((from el in firstArea select (el.Item1, el.Item2, el.Item3, el.Item4)).ToList(), (from el in secondArea select (el.Item1, el.Item2, el.Item3, el.Item4)).ToList(), (from el in thirdArea select (el.Item1, el.Item2, el.Item3, el.Item4)).ToList());

            pag.PropertyChanged += async (s, e) =>
            {
                if (e.Property == RibbonFilePage.SelectedIndexProperty)
                {
                    int index = (int)e.NewValue;

                    if (index < firstArea.Count)
                    {
                        await (firstArea[index].Item5?.Invoke(0, this) ?? Task.CompletedTask);
                    }
                    else if (index < firstArea.Count + secondArea.Count)
                    {
                        await (secondArea[index - firstArea.Count].Item5?.Invoke(0, this) ?? Task.CompletedTask);
                    }
                    else
                    {
                        await (thirdArea[index - firstArea.Count - secondArea.Count].Item5?.Invoke(0, this) ?? Task.CompletedTask);
                    }
                }
            };

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreenLeft = builder.Build();

            builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(16, 0);
            TransformOperations offScreenRight = builder.Build();

            pag.BackButtonPressed += (s, e) =>
            {
                pag.RenderTransform = offScreenLeft;
                pag.Opacity = 0;
                pag.FindControl<Grid>("ThemeGrid").RenderTransform = offScreenRight;

                if (Modules.IsWindows)
                {
                    this.FindControl<Grid>("TitleBarContainer2").Opacity = 0;
                }


                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await Task.Delay(200);
                    pag.IsVisible = false;
                    pag.SelectedIndex = 0;
                    if (Modules.IsWindows)
                    {
                        this.FindControl<Grid>("TitleBarContainer2").IsVisible = false;
                    }
                });
            };

            this.RibbonFilePage = pag;
            this.HomePage = home;
            pag.RenderTransform = TransformOperations.Identity;

            for (int i = 0; i < enablers.Count; i++)
            {
                enablers[i]();
            }

            this.FindControl<Grid>("RibbonFilePageContainer").Children.Insert(0, pag);
        }
    }
}
