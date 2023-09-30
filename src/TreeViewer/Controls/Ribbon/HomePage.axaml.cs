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
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace TreeViewer
{
    public partial class HomePage : UserControl
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<UniformGrid>("RecentFilesGrid").PropertyChanged += (s, e) =>
            {
                if (e.Property == UniformGrid.BoundsProperty)
                {
                    if (((Rect)e.NewValue).Width >= 650)
                    {
                        this.FindControl<UniformGrid>("RecentFilesGrid").Columns = 2;
                    }
                    else
                    {
                        this.FindControl<UniformGrid>("RecentFilesGrid").Columns = 1;
                    }
                }
            };

            this.FindControl<TextBlock>("MoreExamplesBlock").PointerPressed += (s, e) =>
            {
                e.GetCurrentPoint((TextBlock)s).Pointer.Capture((TextBlock)s);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "https://github.com/arklumpus/TreeViewer/wiki/Workload-examples",
                    UseShellExecute = true
                });
            };

            BuildExamples();

            UpdateRecentFiles();
        }

        List<(string, RecentFile)> RecentFiles;

        List<Grid> RecentFilesGrids;
        List<Action<string, RecentFile>> UpdateGrid;

        public static List<(string, string)> Examples = new List<(string, string)>()
        {
            ("Figures", "Include images in the plot"),
            ("Character_states", "Display character states"),
            ("Alignment", "Plot an alignment with the tree"),
            ("Clock", "Show posterior age distributions"),
            ("AgeDistributions", "Plot multiple age distributions"),
            ("BLAST_scores", "Highlight BLAST scores"),
            ("Support", "Highlight support values")
        };

        private void BuildExamples()
        {
            for (int i = 0; i < Examples.Count; i++)
            {
                Button but = new Button() { Margin = new Thickness(5, 0, 5, 0), Background = Brushes.Transparent, Width = 130 };
                but.Classes.Add("SideBarButton");

                Grid buttonContent = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
                buttonContent.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                buttonContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                buttonContent.Children.Add(new Viewbox() { Width = 100, Height = 100, Child = new Image() { Source = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Examples." + Examples[i].Item1 + ".png")) } });

                TextBlock description = new TextBlock() { Foreground = Brushes.Black, FontSize = 13, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 3, 0, 0), Text = Examples[i].Item2 };
                Grid.SetRow(description, 1);
                buttonContent.Children.Add(description);
                but.Content = buttonContent;

                this.FindControl<StackPanel>("ExamplesContainer").Children.Add(but);

                string exampleName = Examples[i].Item1;

                but.Click += async (s, e) =>
                {
                    string tmp = Path.GetTempFileName();

                    using (FileStream fs = new FileStream(tmp, FileMode.Create))
                    {
                        Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Examples." + exampleName + ".tbi").CopyTo(fs);
                    }

                    await this.FindAncestorOfType<MainWindow>().LoadFile(tmp, true, exampleName);
                };
            }
        }

        public void UpdateRecentFiles()
        {
            RecentFiles = RecentFile.GetRecentFiles().ToList();

            if (RecentFilesGrids == null)
            {
                RecentFilesGrids = new List<Grid>(RecentFiles.Count);
                UpdateGrid = new List<Action<string, RecentFile>>();
            }

            if (RecentFilesGrids.Count > RecentFiles.Count)
            {
                this.FindControl<UniformGrid>("RecentFilesGrid").Children.RemoveRange(RecentFiles.Count, RecentFilesGrids.Count - RecentFiles.Count);
                RecentFilesGrids.RemoveRange(RecentFiles.Count, RecentFilesGrids.Count - RecentFiles.Count);
                UpdateGrid.RemoveRange(RecentFiles.Count, UpdateGrid.Count - RecentFiles.Count);
            }

            for (int i = 0; i < RecentFiles.Count; i++)
            {
                if (i > RecentFilesGrids.Count - 1)
                {

                    Grid itemGrid = new Grid() { Height = 102, Margin = new Thickness(0, 0, 5, 5), Background = Brushes.Transparent };
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(97, GridUnitType.Pixel));
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    itemGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    StackPanel namePanel = new StackPanel() { Margin = new Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(namePanel, 1);
                    itemGrid.Children.Add(namePanel);

                    TrimmedTextBox2 nameBlock;
                    TrimmedTextBox2 pathBlock;
                    TrimmedTextBox2 dateBlock;

                    {
                        nameBlock = new TrimmedTextBox2() { Text = Path.GetFileName(RecentFiles[i].Item2.FilePath), FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        AvaloniaBugFixes.SetToolTip(nameBlock, new TextBlock() { Text = RecentFiles[i].Item2.FilePath, TextWrapping = TextWrapping.NoWrap });
                        namePanel.Children.Add(nameBlock);
                    }

                    {
                        pathBlock = new TrimmedTextBox2() { Text = Path.GetDirectoryName(RecentFiles[i].Item2.FilePath), Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14, Margin = new Thickness(0, 0, 0, 5) };
                        AvaloniaBugFixes.SetToolTip(pathBlock, new TextBlock() { Text = RecentFiles[i].Item2.FilePath, TextWrapping = TextWrapping.NoWrap });
                        namePanel.Children.Add(pathBlock);
                    }

                    string lastModified = new DateTime(RecentFiles[i].Item2.ModifiedDate).ToString("f");

                    {
                        dateBlock = new TrimmedTextBox2() { Text = lastModified, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 12 };
                        AvaloniaBugFixes.SetToolTip(dateBlock, new TextBlock() { Text = lastModified, TextWrapping = TextWrapping.NoWrap });
                        namePanel.Children.Add(dateBlock);
                    }

                    Image img = new Image() { };

                    using (MemoryStream ms = new MemoryStream(RecentFiles[i].Item2.Preview))
                    {
                        img.Source = new Bitmap(ms);
                    }

                    itemGrid.Children.Add(new Border() { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), MaxWidth = 92, MaxHeight = 92, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Child = new Viewbox { Child = img } });

                    StackPanel buttonsPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(buttonsPanel, 2);
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

                    string treeFile = RecentFiles[i].Item2.FilePath;

                    itemGrid.PointerReleased += async (s, e) =>
                    {
                        itemGrid.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));

                        Point pos = e.GetCurrentPoint(itemGrid).Position;

                        if (pos.X >= 0 && pos.Y >= 0 && pos.X <= itemGrid.Bounds.Width && pos.Y <= itemGrid.Bounds.Height)
                        {
                            await this.FindAncestorOfType<MainWindow>().LoadFile(treeFile, false);
                        }
                    };

                    string fileToDelete = RecentFiles[i].Item1;

                    deleteButton.Click += async (s, e) =>
                    {
                        try
                        {
                            File.Delete(fileToDelete);

                            int index = RecentFilesGrids.IndexOf(itemGrid);

                            RecentFilesGrids.RemoveAt(index);
                            UpdateGrid.RemoveAt(index);
                            this.FindControl<UniformGrid>("RecentFilesGrid").Children.RemoveAt(index);
                        }
                        catch (Exception ex)
                        {
                            await new MessageBox("Attention!", "An error occurred while deleting the files!\n" + ex.Message).ShowDialog2(this.FindAncestorOfType<Window>());
                        }
                    }; ;

                    RecentFilesGrids.Add(itemGrid);

                    UpdateGrid.Add((item1, item2) =>
                    {

                        nameBlock.Text = Path.GetFileName(item2.FilePath);
                        AvaloniaBugFixes.SetToolTip(nameBlock, new TextBlock() { Text = item2.FilePath, TextWrapping = TextWrapping.NoWrap });

                        pathBlock.Text = Path.GetDirectoryName(item2.FilePath);
                        AvaloniaBugFixes.SetToolTip(pathBlock, new TextBlock() { Text = item2.FilePath, TextWrapping = TextWrapping.NoWrap });

                        string lastModified = new DateTime(item2.ModifiedDate).ToString("f");
                        dateBlock.Text = lastModified;
                        AvaloniaBugFixes.SetToolTip(dateBlock, new TextBlock() { Text = lastModified, TextWrapping = TextWrapping.NoWrap });

                        using (MemoryStream ms = new MemoryStream(item2.Preview))
                        {
                            img.Source = new Bitmap(ms);
                        }

                        fileToDelete = item1;
                        treeFile = item2.FilePath;
                    });

                    this.FindControl<UniformGrid>("RecentFilesGrid").Children.Add(itemGrid);
                }
                else
                {
                    UpdateGrid[i](RecentFiles[i].Item1, RecentFiles[i].Item2);
                }
            }
        }
    }
}
