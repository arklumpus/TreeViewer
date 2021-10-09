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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TreeViewer
{
    class AutosavesPage : RibbonFilePageContentTemplate
    {
        private Grid TreesGrid;
        private Grid CodeGrid;

        public AutosavesPage() : base("Autosaves")
        {
            TreesGrid = new Grid();
            CodeGrid = new Grid();

            BuildTreesGrid();
            BuildCodeGrid();

            this.PageContent = new RibbonFilePageContentTabbedWithButtons(new List<(string, string, Avalonia.Controls.Control, Avalonia.Controls.Control)>()
            {
                ("Autosaved trees", null, new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AutosavedTrees")){ Width = 32, Height = 32}, TreesGrid),
                ("Autosaved code", null, new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AutosavedCode")){ Width = 32, Height = 32}, CodeGrid)
            });
        }

        private void BuildTreesGrid()
        {
            TreesGrid.Margin = new Thickness(25, 0, 0, 0);

            TreesGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            TreesGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            Grid titleGrid = new Grid() { Margin = new Thickness(0, 0, 0, 5) };
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            TreesGrid.Children.Add(titleGrid);

            titleGrid.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Autosaved trees" });


            StackPanel deleteAllTreesButtonContent = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            deleteAllTreesButtonContent.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")));

            deleteAllTreesButtonContent.Children.Add(new TextBlock() { Text = "Delete all", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) });

            Button deleteAllTreesButton = new Button() { Content = deleteAllTreesButtonContent, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Background = Brushes.Transparent, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow) };
            deleteAllTreesButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteAllTreesButton, 1);
            titleGrid.Children.Add(deleteAllTreesButton);



            StackPanel treeContainer = new StackPanel();

            ScrollViewer scroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 17, 0), AllowAutoHide = false };
            scroller.Content = treeContainer;
            Grid.SetRow(scroller, 1);
            TreesGrid.Children.Add(scroller);

            string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "Autosave");

            deleteAllTreesButton.Click += async (s, e) =>
            {
                try
                {
                    string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "Autosave");

                    if (Directory.Exists(autosavePath))
                    {
                        Dictionary<DateTime, List<(string, string, string, int)>> items = new Dictionary<DateTime, List<(string, string, string, int)>>();

                        foreach (string directory in Directory.GetDirectories(autosavePath))
                        {
                            Directory.Delete(directory, true);
                        }
                    }
                    treeContainer.Children.Clear();

                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                }
            };

            if (Directory.Exists(autosavePath))
            {
                foreach (string directory in Directory.GetDirectories(autosavePath).OrderByDescending(a => a))
                {
                    if (Directory.EnumerateDirectories(directory).Count() > 0)
                    {
                        string dateString = Path.GetFileName(directory);

                        int[] dateItems = (from el in dateString.Split('_') select int.Parse(el)).ToArray();

                        DateTime date = new DateTime(dateItems[0], dateItems[1], dateItems[2]);

                        Accordion exp = new Accordion() { Margin = new Thickness(0, 5, 0, 5), ArrowSize = 12, HeaderForegroundOpen = new SolidColorBrush(Colors.Black) };
                        exp.HeaderHoverBackground = new SolidColorBrush(Color.FromRgb(210, 210, 210));

                        Grid header = new Grid() { Height = 27 };
                        header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                        header.Children.Add(new TextBlock() { Text = date.ToString("D"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 });

                        StackPanel deleteAllButtonContent = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

                        deleteAllButtonContent.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")));

                        deleteAllButtonContent.Children.Add(new TextBlock() { Text = "Delete all", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) });

                        Button deleteAllButton = new Button() { Content = deleteAllButtonContent, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Background = Brushes.Transparent, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow), IsVisible = false };
                        deleteAllButton.Classes.Add("SideBarButton");
                        Grid.SetColumn(deleteAllButton, 1);
                        header.Children.Add(deleteAllButton);

                        exp.AccordionHeader = header;

                        bool contentsBuilt = false;

                        exp.PointerEnter += (s, e) =>
                        {
                            deleteAllButton.IsVisible = true;
                        };

                        exp.PointerLeave += (s, e) =>
                        {
                            deleteAllButton.IsVisible = false;
                        };

                        exp.PropertyChanged += (s, e) =>
                        {
                            if (e.Property == Accordion.IsOpenProperty)
                            {
                                if (!contentsBuilt)
                                {
                                    StackPanel contents = new StackPanel();

                                    deleteAllButton.Click += async (s, e) =>
                                    {
                                        try
                                        {
                                            Directory.Delete(directory, true);

                                            treeContainer.Children.Remove(exp);
                                        }
                                        catch (Exception ex)
                                        {
                                            await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                                        }
                                    };

                                    int index = 0;

                                    List<string> fileDirs = new List<string>(Directory.GetDirectories(directory));
                                    Dictionary<string, AutosaveData> saveDatas = new Dictionary<string, AutosaveData>();

                                    foreach (string fileDirectory in fileDirs)
                                    {
                                        string autosaveFile = Path.Combine(fileDirectory, "autosave.json");

                                        if (File.Exists(autosaveFile))
                                        {
                                            saveDatas[fileDirectory] = System.Text.Json.JsonSerializer.Deserialize<AutosaveData>(File.ReadAllText(autosaveFile), Modules.DefaultSerializationOptions);
                                        }
                                    }

                                    foreach (string fileDirectory in (from el in fileDirs where saveDatas.ContainsKey(el) orderby saveDatas[el].SaveTime descending select el))
                                    {
                                        string treeFile = Path.Combine(fileDirectory, Path.GetFileName(fileDirectory) + ".nex");
                                        string autosaveFile = Path.Combine(fileDirectory, "autosave.json");

                                        if (File.Exists(autosaveFile) && File.Exists(treeFile))
                                        {
                                            string extension = "tree";

                                            Grid itemGrid = new Grid() { Height = 53, Margin = new Thickness(0, 0, 0, 4), Background = Brushes.Transparent };
                                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition(50, GridUnitType.Pixel));
                                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition(80, GridUnitType.Pixel));
                                            itemGrid.ColumnDefinitions.Add(new ColumnDefinition(29, GridUnitType.Pixel));

                                            AutosaveData saveData = saveDatas[fileDirectory];

                                            FileInfo info = new FileInfo(treeFile);

                                            long size = info.Length;

                                            {
                                                Canvas can = new Canvas() { Height = 1, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
                                                Grid.SetColumnSpan(can, 4);
                                                itemGrid.Children.Add(can);
                                            }

                                            StackPanel namePanel = new StackPanel() { Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                            Grid.SetColumn(namePanel, 1);
                                            itemGrid.Children.Add(namePanel);

                                            if (!string.IsNullOrEmpty(saveData.OriginalPath))
                                            {
                                                extension = Path.GetExtension(saveData.OriginalPath).ToLowerInvariant().Replace(".", "");

                                                if (!FileExtensions.EmbeddedFileTypeIcons.Contains(extension))
                                                {
                                                    extension = "tree";
                                                }

                                                TrimmedTextBox2 block = new TrimmedTextBox2() { Text = Path.GetFileName(saveData.OriginalPath), FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                                AvaloniaBugFixes.SetToolTip(block, new TextBlock() { Text = saveData.OriginalPath, TextWrapping = TextWrapping.NoWrap });
                                                namePanel.Children.Add(block);
                                            }
                                            else
                                            {
                                                extension = "nex";

                                                TextBlock block = new TextBlock() { Text = "(None)", FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                                namePanel.Children.Add(block);
                                            }

                                            if (!string.IsNullOrEmpty(saveData.OriginalPath))
                                            {
                                                TrimmedTextBox2 block = new TrimmedTextBox2() { Text = Path.GetDirectoryName(saveData.OriginalPath), Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                                                AvaloniaBugFixes.SetToolTip(block, new TextBlock() { Text = saveData.OriginalPath, TextWrapping = TextWrapping.NoWrap });
                                                namePanel.Children.Add(block);
                                            }
                                            else
                                            {
                                                TextBlock block = new TextBlock() { Text = "(None)", FontStyle = FontStyle.Italic, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                                namePanel.Children.Add(block);
                                            }

                                            itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FileTypeIcons." + extension)) { Width = 32, Height = 32 });

                                            StackPanel timeSizePanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                            Grid.SetColumn(timeSizePanel, 2);
                                            itemGrid.Children.Add(timeSizePanel);

                                            {
                                                TextBlock block = new TextBlock() { Text = saveData.SaveTime.ToString("t"), Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                                                timeSizePanel.Children.Add(block);
                                            }

                                            {
                                                TextBlock block = new TextBlock() { Text = GetHumanReadableSize(size), Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                                                timeSizePanel.Children.Add(block);
                                            }

                                            StackPanel buttonsPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                            Grid.SetColumn(buttonsPanel, 3);
                                            itemGrid.Children.Add(buttonsPanel);

                                            Button deleteButton = new Button() { Foreground = Brushes.Black, Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")) { Width = 16, Height = 16 }, Background = Brushes.Transparent, Padding = new Thickness(0), Width = 24, Height = 24, Margin = new Thickness(0, 0, 0, 2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, IsVisible = false };
                                            buttonsPanel.Children.Add(deleteButton);
                                            AvaloniaBugFixes.SetToolTip(deleteButton, new TextBlock() { Text = "Delete", Foreground = Brushes.Black });
                                            deleteButton.Classes.Add("SideBarButton");

                                            itemGrid.PointerEnter += (s, e) =>
                                            {
                                                deleteButton.IsVisible = true;
                                                itemGrid.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                                            };

                                            itemGrid.PointerLeave += (s, e) =>
                                            {
                                                deleteButton.IsVisible = false;
                                                itemGrid.Background = Brushes.Transparent;
                                            };

                                            itemGrid.PointerPressed += (s, e) =>
                                            {
                                                itemGrid.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                                            };

                                            itemGrid.PointerReleased += async (s, e) =>
                                            {
                                                itemGrid.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));

                                                Point pos = e.GetCurrentPoint(itemGrid).Position;

                                                if (pos.X >= 0 && pos.Y >= 0 && pos.X <= itemGrid.Bounds.Width && pos.Y <= itemGrid.Bounds.Height)
                                                {
                                                    await this.FindAncestorOfType<MainWindow>().LoadFile(treeFile, false, saveData.OriginalPath);
                                                }
                                            };

                                            deleteButton.Click += async (s, e) =>
                                            {
                                                try
                                                {
                                                    int ind = Grid.GetRow(deleteButton);

                                                    Directory.Delete(fileDirectory, true);

                                                    contents.Children.Remove(itemGrid);

                                                    if (contents.Children.Count == 0)
                                                    {
                                                        treeContainer.Children.Remove(exp);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                                                }

                                                exp.InvalidateHeight();

                                            };

                                            contents.Children.Add(itemGrid);

                                            index++;
                                        }
                                    }

                                    exp.AccordionContent = contents;

                                    contentsBuilt = true;
                                }
                            }
                        };

                        treeContainer.Children.Add(exp);
                    }
                }
            }
        }

        private void BuildCodeGrid()
        {
            CodeGrid.Margin = new Thickness(25, 0, 0, 0);

            CodeGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            CodeGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            Grid titleGrid = new Grid() { Margin = new Thickness(0, 0, 0, 5) };
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            titleGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            CodeGrid.Children.Add(titleGrid);

            titleGrid.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Autosaved code" });


            StackPanel deleteAllCodeButtonContent = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

            deleteAllCodeButtonContent.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")));

            deleteAllCodeButtonContent.Children.Add(new TextBlock() { Text = "Delete all", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) });

            Button deleteAllCodeButton = new Button() { Content = deleteAllCodeButtonContent, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Background = Brushes.Transparent, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow) };
            deleteAllCodeButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteAllCodeButton, 1);
            titleGrid.Children.Add(deleteAllCodeButton);



            StackPanel codeContainer = new StackPanel();

            ScrollViewer scroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 17, 0), AllowAutoHide = false };
            scroller.Content = codeContainer;
            Grid.SetRow(scroller, 1);
            CodeGrid.Children.Add(scroller);

            string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);

            deleteAllCodeButton.Click += async (s, e) =>
            {
                try
                {
                    string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name);

                    if (Directory.Exists(autosavePath))
                    {
                        Dictionary<DateTime, List<(string, string, string, int)>> items = new Dictionary<DateTime, List<(string, string, string, int)>>();

                        foreach (string directory in Directory.GetDirectories(autosavePath))
                        {
                            string name = Path.GetFileName(directory);

                            if (name != "Autosave" && name != "Keys" && name != "modules" && name != "Recent")
                            {
                                Directory.Delete(directory, true);
                            }
                        }
                    }
                    codeContainer.Children.Clear();

                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                }
            };

            if (Directory.Exists(autosavePath))
            {
                Dictionary<DateTime, List<(string, string, string, int)>> items = new Dictionary<DateTime, List<(string, string, string, int)>>();

                foreach (string directory in Directory.GetDirectories(autosavePath))
                {
                    string editorId = Path.GetFileName(directory);

                    if (editorId != "Autosave" && editorId.Contains("_") && !editorId.StartsWith("ModuleCreator"))
                    {
                        try
                        {
                            string type = editorId.Substring(0, editorId.IndexOf("_"));

                            editorId = editorId.Substring(editorId.IndexOf("_") + 1);

                            string parameterName = editorId.Substring(0, editorId.LastIndexOf("_")).Replace("_", " ").Trim();

                            DirectoryInfo info = new DirectoryInfo(directory);

                            DateTime date = info.LastWriteTime.Date;

                            if (!items.TryGetValue(date, out List<(string, string, string, int)> list))
                            {
                                list = new List<(string, string, string, int)>();
                                items[date] = list;
                            }

                            int count = info.EnumerateFiles().Count();

                            if (count > 0)
                            {
                                list.Add((directory, type, parameterName, count));
                            }
                        }
                        catch { }
                    }
                }

                foreach (KeyValuePair<DateTime, List<(string, string, string, int)>> kvp in items.OrderByDescending(a => a.Key))
                {
                    if (kvp.Value.Count > 0)
                    {

                        Accordion exp = new Accordion() { Margin = new Thickness(0, 5, 0, 5), ArrowSize = 12, HeaderForegroundOpen = new SolidColorBrush(Colors.Black) };
                        exp.HeaderHoverBackground = new SolidColorBrush(Color.FromRgb(210, 210, 210));

                        Grid header = new Grid() { Height = 27 };
                        header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                        header.Children.Add(new TextBlock() { Text = kvp.Key.ToString("D"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 });

                        StackPanel deleteAllButtonContent = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

                        deleteAllButtonContent.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")));

                        deleteAllButtonContent.Children.Add(new TextBlock() { Text = "Delete all", FontSize = 12, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(2, 0, 0, 0) });

                        Button deleteAllButton = new Button() { Content = deleteAllButtonContent, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Background = Brushes.Transparent, Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow), IsVisible = false };
                        deleteAllButton.Classes.Add("SideBarButton");
                        Grid.SetColumn(deleteAllButton, 1);
                        header.Children.Add(deleteAllButton);

                        exp.AccordionHeader = header;

                        bool contentsBuilt = false;

                        exp.PointerEnter += (s, e) =>
                        {
                            deleteAllButton.IsVisible = true;
                        };

                        exp.PointerLeave += (s, e) =>
                        {
                            deleteAllButton.IsVisible = false;
                        };

                        exp.PropertyChanged += (s, e) =>
                        {
                            if (e.Property == Accordion.IsOpenProperty)
                            {
                                if (!contentsBuilt)
                                {
                                    StackPanel contents = new StackPanel();

                                    deleteAllButton.Click += async (s, e) =>
                                    {
                                        try
                                        {
                                            foreach ((string, string, string, int) item in kvp.Value)
                                            {
                                                if (Directory.Exists(item.Item1))
                                                {
                                                    Directory.Delete(item.Item1, true);
                                                }
                                            }

                                            codeContainer.Children.Remove(exp);
                                        }
                                        catch (Exception ex)
                                        {
                                            await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                                        }
                                    };

                                    int index = 0;

                                    foreach ((string, string, string, int) item in kvp.Value)
                                    {
                                        Grid itemGrid = new Grid() { Height = 53, Margin = new Thickness(0, 0, 0, 4), Background = Brushes.Transparent };
                                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(50, GridUnitType.Pixel));
                                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(50, GridUnitType.Pixel));
                                        itemGrid.ColumnDefinitions.Add(new ColumnDefinition(29, GridUnitType.Pixel));

                                        {
                                            Canvas can = new Canvas() { Height = 1, Background = new SolidColorBrush(Color.FromRgb(210, 210, 210)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
                                            Grid.SetColumnSpan(can, 4);
                                            itemGrid.Children.Add(can);
                                        }

                                        StackPanel namePanel = new StackPanel() { Margin = new Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                        Grid.SetColumn(namePanel, 1);
                                        itemGrid.Children.Add(namePanel);

                                        {
                                            string text = item.Item3;
                                            if (text.EndsWith("..."))
                                            {
                                                text = text.Substring(0, text.Length - 3);
                                            }
                                            if (text.EndsWith(":"))
                                            {
                                                text = text.Substring(0, text.Length - 1);
                                            }

                                            TrimmedTextBox2 block = new TrimmedTextBox2() { Text = text, FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                            AvaloniaBugFixes.SetToolTip(block, new TextBlock() { Text = text, TextWrapping = TextWrapping.NoWrap });
                                            namePanel.Children.Add(block);
                                        }

                                        {
                                            string text = "";

                                            switch (item.Item2)
                                            {
                                                case "CodeEditor":
                                                    text = "Custom script";
                                                    itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SourceCode")) { Width = 32, Height = 32 });
                                                    break;
                                                case "StringFormatter":
                                                    text = "String formatter";
                                                    itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringFormatter")) { Width = 32, Height = 32 });
                                                    break;
                                                case "NumberFormatter":
                                                    text = "Number formatter";
                                                    itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumberFormatter")) { Width = 32, Height = 32 });
                                                    break;
                                                case "ColourFormatter":
                                                    text = "Colour formatter";
                                                    itemGrid.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ColourFormatter")) { Width = 32, Height = 32 });
                                                    break;
                                            }

                                            TrimmedTextBox2 block = new TrimmedTextBox2() { Text = text, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                                            AvaloniaBugFixes.SetToolTip(block, new TextBlock() { Text = text, TextWrapping = TextWrapping.NoWrap });
                                            namePanel.Children.Add(block);
                                        }

                                        StackPanel timeSizePanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Orientation = Avalonia.Layout.Orientation.Horizontal, Background = Brushes.Transparent };

                                        AvaloniaBugFixes.SetToolTip(timeSizePanel, "Saved " + item.Item4.ToString() + " times");
                                        Grid.SetColumn(timeSizePanel, 2);
                                        itemGrid.Children.Add(timeSizePanel);

                                        {
                                            timeSizePanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.History")) { Width = 16, Height = 16, Margin = new Thickness(0, 0, 5, 0) });
                                            TextBlock block = new TextBlock() { Text = item.Item4.ToString(), Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                                            timeSizePanel.Children.Add(block);
                                        }

                                        StackPanel buttonsPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                                        Grid.SetColumn(buttonsPanel, 3);
                                        itemGrid.Children.Add(buttonsPanel);

                                        Button deleteButton = new Button() { Foreground = Brushes.Black, Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Trash")) { Width = 16, Height = 16 }, Background = Brushes.Transparent, Padding = new Thickness(0), Width = 24, Height = 24, Margin = new Thickness(0, 0, 0, 2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, IsVisible = false };
                                        buttonsPanel.Children.Add(deleteButton);
                                        AvaloniaBugFixes.SetToolTip(deleteButton, new TextBlock() { Text = "Delete", Foreground = Brushes.Black });
                                        deleteButton.Classes.Add("SideBarButton");

                                        itemGrid.PointerEnter += (s, e) =>
                                        {
                                            deleteButton.IsVisible = true;
                                            itemGrid.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                                        };

                                        itemGrid.PointerLeave += (s, e) =>
                                        {
                                            deleteButton.IsVisible = false;
                                            itemGrid.Background = Brushes.Transparent;
                                        };

                                        itemGrid.PointerPressed += (s, e) =>
                                        {
                                            itemGrid.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                                        };

                                        itemGrid.PointerReleased += async (s, e) =>
                                        {
                                            itemGrid.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));

                                            Point pos = e.GetCurrentPoint(itemGrid).Position;

                                            if (pos.X >= 0 && pos.Y >= 0 && pos.X <= itemGrid.Bounds.Width && pos.Y <= itemGrid.Bounds.Height)
                                            {
                                                CodeViewerWindow win = new CodeViewerWindow();
                                                await win.Initialize(item.Item2, item.Item1);
                                                await win.ShowDialog2(this.FindAncestorOfType<Window>());
                                            }
                                        };

                                        deleteButton.Click += async (s, e) =>
                                        {
                                            try
                                            {
                                                int ind = Grid.GetRow(deleteButton);

                                                Directory.Delete(item.Item1, true);
                                                contents.Children.Remove(itemGrid);

                                                if (contents.Children.Count == 0)
                                                {
                                                    this.FindControl<StackPanel>("CodeContainer").Children.Remove(exp);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                                            }

                                            exp.InvalidateHeight();

                                        };

                                        contents.Children.Add(itemGrid);

                                        index++;
                                    }

                                    exp.AccordionContent = contents;

                                    contentsBuilt = true;
                                }
                            }
                        };

                        codeContainer.Children.Add(exp);
                    }
                }
            }
        }

        private static string GetHumanReadableSize(long size)
        {
            if (size < 1024)
            {
                return size + " B";
            }
            else
            {
                double longSize = size / 1024.0;

                if (longSize < 1024)
                {
                    return longSize.ToString("0.#") + " kiB";
                }
                else
                {
                    longSize /= 1024.0;

                    if (longSize < 1024)
                    {
                        return longSize.ToString("0.#") + " MiB";
                    }
                    else
                    {
                        longSize /= 1024.0;
                        return longSize.ToString("0.#") + " GiB";
                    }
                }
            }
        }
    }
}
