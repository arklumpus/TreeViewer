/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Linq;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public partial class SpreadsheetCustomSortWindow : ChildWindow
    {
        public SpreadsheetCustomSortWindow()
        {
            InitializeComponent();
        }

        public SpreadsheetCustomSortWindow(IEnumerable<string> rows, IEnumerable<string> columns)
        {
            this.Rows = new List<string>(rows);
            this.Columns = new List<string>(columns);

            InitializeComponent();
        }

        public (bool, int)[] Result { get; private set; } = null;
        public bool SortRows = true;
        public bool CaseSensitive = false;

        public void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.CustomSort")) { Width = 32, Height = 32 });

            ComboBox orderByBox = this.FindControl<ComboBox>("OrderByBox");

            StackPanel rowsItem = new StackPanel() { Orientation = Orientation.Horizontal };
            rowsItem.Children.Add(GetRowIcon());
            rowsItem.Children.Add(new TextBlock() { Text = "Rows" });

            StackPanel columnsItem = new StackPanel() { Orientation = Orientation.Horizontal };
            columnsItem.Children.Add(GetColumnIcon());
            columnsItem.Children.Add(new TextBlock() { Text = "Columns" });

            orderByBox.Items = new List<StackPanel>()
                {
                    rowsItem,
                    columnsItem
                };

            orderByBox.SelectedIndex = 0;

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                orderByBox.Resources["ComboBoxDropDownBackground"] = Brushes.White;
                orderByBox.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                orderByBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                orderByBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(198, 198, 198));

                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                orderByBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 1) });
                orderByBox.Styles.Add(scrollBarStyle);
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                orderByBox.Resources["ComboBoxDropDownBackground"] = new SolidColorBrush(Color.FromRgb(233, 233, 233)); ;
                orderByBox.Resources["OverlayCornerRadius"] = new CornerRadius(4);
                orderByBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                orderByBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                orderByBox.Resources["ComboBoxDropdownBorderPadding"] = new Thickness(0, 4, 0, 4);


                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                orderByBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 2) });
                orderByBox.Styles.Add(scrollBarStyle);
            }

            orderByBox.SelectionChanged += (s, e) =>
            {
                BuildOrderByList();
            };

            BuildOrderByList();

            this.FindControl<Button>("AddLevelButton").Click += (s, e) =>
            {
                if (this.FindControl<ComboBox>("OrderByBox").SelectedIndex == 0)
                {
                    this.FindControl<StackPanel>("LevelContainer").Children.Add(GetLevelGrid(false, Columns));
                }
                else
                {
                    this.FindControl<StackPanel>("LevelContainer").Children.Add(GetLevelGrid(false, Rows));
                }
            };

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Result = null;
                this.Close();
            };

            this.FindControl<Button>("OKButton").Click += (s, e) =>
            {
                this.SortRows = this.FindControl<ComboBox>("OrderByBox").SelectedIndex == 0;
                this.CaseSensitive = this.FindControl<CheckBox>("CaseSensitiveBox").IsChecked == true;

                this.Result = this.OrderGrids.Select(x =>
                {
                    ComboBox[] boxes = (ComboBox[])x.Tag;

                    int index = boxes[0].SelectedIndex;
                    bool ascending = boxes[1].SelectedIndex == 0;

                    return (ascending, index);

                }).ToArray();

                this.Close();
            };
        }

        private List<string> Columns = new List<string>() { };
        private List<string> Rows = new List<string>() { };

        List<Grid> OrderGrids = new List<Grid>();

        private void BuildOrderByList()
        {
            OrderGrids.Clear();
            this.FindControl<StackPanel>("LevelContainer").Children.Clear();

            if (this.FindControl<ComboBox>("OrderByBox").SelectedIndex == 0)
            {
                this.FindControl<StackPanel>("LevelContainer").Children.Add(GetLevelGrid(true, Columns));
            }
            else
            {
                this.FindControl<StackPanel>("LevelContainer").Children.Add(GetLevelGrid(true, Rows));
            }
        }

        private Grid GetLevelGrid(bool first, List<string> items)
        {
            Grid grd = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 5) };
            grd.ColumnDefinitions.Add(new ColumnDefinition(62, GridUnitType.Pixel));
            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));

            grd.Children.Add(new TextBlock() { Text = first ? "Order by: " : "Then by: ", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right });

            ComboBox columnBox = new ComboBox() { Items = items, Padding = new Avalonia.Thickness(5, 2, 0, 2), SelectedIndex = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Margin = new Avalonia.Thickness(0, 0, 5, 0), FontSize = 13 };
            Grid.SetColumn(columnBox, 1);
            grd.Children.Add(columnBox);

            StackPanel ascendingItem = new StackPanel() { Orientation = Orientation.Horizontal };
            ascendingItem.Children.Add(GetAscendingIcon());
            ascendingItem.Children.Add(new TextBlock() { Text = "Ascending" });

            StackPanel descendingItem = new StackPanel() { Orientation = Orientation.Horizontal };
            descendingItem.Children.Add(GetDescendingIcon());
            descendingItem.Children.Add(new TextBlock() { Text = "Descending" });

            ComboBox orderBox = new ComboBox() { Items = new List<StackPanel>() { ascendingItem, descendingItem }, Padding = new Avalonia.Thickness(5, 2, 5, 2), SelectedIndex = 0, Margin = new Avalonia.Thickness(0, 0, 5, 0), FontSize = 13, Width = 135, HorizontalContentAlignment = HorizontalAlignment.Left };
            Grid.SetColumn(orderBox, 2);
            grd.Children.Add(orderBox);

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                columnBox.Resources["ComboBoxDropDownBackground"] = Brushes.White;
                columnBox.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                columnBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                columnBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(198, 198, 198));

                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                columnBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 1) });
                columnBox.Styles.Add(scrollBarStyle);
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                columnBox.Resources["ComboBoxDropDownBackground"] = new SolidColorBrush(Color.FromRgb(233, 233, 233)); ;
                columnBox.Resources["OverlayCornerRadius"] = new CornerRadius(4);
                columnBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                columnBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                columnBox.Resources["ComboBoxDropdownBorderPadding"] = new Thickness(0, 4, 0, 4);


                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                columnBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 2) });
                columnBox.Styles.Add(scrollBarStyle);
            }

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                orderBox.Resources["ComboBoxDropDownBackground"] = Brushes.White;
                orderBox.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                orderBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                orderBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(198, 198, 198));

                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                orderBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 1) });
                orderBox.Styles.Add(scrollBarStyle);
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                orderBox.Resources["ComboBoxDropDownBackground"] = new SolidColorBrush(Color.FromRgb(233, 233, 233)); ;
                orderBox.Resources["OverlayCornerRadius"] = new CornerRadius(4);
                orderBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 1, 2);
                orderBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                orderBox.Resources["ComboBoxDropdownBorderPadding"] = new Thickness(0, 4, 0, 4);


                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                orderBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 2) });
                orderBox.Styles.Add(scrollBarStyle);
            }

            if (!first)
            {
                Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
                deleteButton.Classes.Add("SideBarButton");
                Grid.SetColumn(deleteButton, 3);
                grd.Children.Add(deleteButton);

                deleteButton.Click += (s, e) =>
                {
                    ((StackPanel)grd.Parent).Children.Remove(grd);
                    OrderGrids.Remove(grd);
                };
            }

            grd.Tag = new ComboBox[] { columnBox, orderBox };

            OrderGrids.Add(grd);

            return grd;
        }

        Canvas GetRowIcon()
        {
            Page pag = new Page(16, 16);
            Graphics gpr = pag.Graphics;

            gpr.Save();
            gpr.Translate(0.5, 0.5);

            gpr.FillRectangle(0, 3, 9, 9, Colours.White);
            gpr.StrokeRectangle(0, 3, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(3, 3).LineTo(3, 6).MoveTo(6, 3).LineTo(6, 6), Colour.FromRgb(114, 114, 114));
            gpr.StrokeRectangle(0, 6, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(3, 6).LineTo(3, 9).MoveTo(6, 6).LineTo(6, 9), Colour.FromRgb(114, 114, 114));
            gpr.StrokeRectangle(0, 9, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(3, 9).LineTo(3, 12).MoveTo(6, 9).LineTo(6, 12), Colour.FromRgb(114, 114, 114));

            gpr.StrokePath(new GraphicsPath().MoveTo(13, 4).LineTo(13, 11), Colour.FromRgb(74, 125, 177));
            gpr.FillPath(new GraphicsPath().MoveTo(11, 4).LineTo(13, 2).LineTo(15, 4).Close(), Colour.FromRgb(74, 125, 177));
            gpr.FillPath(new GraphicsPath().MoveTo(11, 11).LineTo(13, 13).LineTo(15, 11).Close(), Colour.FromRgb(74, 125, 177));

            gpr.Restore();
            Canvas can = pag.PaintToCanvas(true);
            can.Margin = new Thickness(0, 0, 5, 0);
            can.UseLayoutRounding = true;

            return can;
        }

        Canvas GetColumnIcon()
        {
            Page pag = new Page(16, 16);
            Graphics gpr = pag.Graphics;

            gpr.Save();
            gpr.Translate(-0.5, 0.5);

            gpr.FillRectangle(3, 6, 9, 9, Colours.White);
            gpr.StrokeRectangle(3, 6, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(6, 6).LineTo(6, 9).MoveTo(9, 6).LineTo(9, 9), Colour.FromRgb(114, 114, 114));
            gpr.StrokeRectangle(3, 9, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(6, 9).LineTo(6, 12).MoveTo(9, 9).LineTo(9, 12), Colour.FromRgb(114, 114, 114));
            gpr.StrokeRectangle(3, 12, 9, 3, Colour.FromRgb(114, 114, 114));
            gpr.StrokePath(new GraphicsPath().MoveTo(6, 12).LineTo(6, 15).MoveTo(9, 12).LineTo(9, 15), Colour.FromRgb(114, 114, 114));

            gpr.StrokePath(new GraphicsPath().MoveTo(4, 2).LineTo(11, 2), Colour.FromRgb(74, 125, 177));
            gpr.FillPath(new GraphicsPath().MoveTo(4, 0).LineTo(2, 2).LineTo(4, 4).Close(), Colour.FromRgb(74, 125, 177));
            gpr.FillPath(new GraphicsPath().MoveTo(11, 0).LineTo(13, 2).LineTo(11, 4).Close(), Colour.FromRgb(74, 125, 177));

            gpr.Restore();
            Canvas can = pag.PaintToCanvas(true);
            can.Margin = new Thickness(0, 0, 5, 0);
            can.UseLayoutRounding = true;

            return can;
        }

        Canvas GetAscendingIcon()
        {
            Page pag = new Page(16, 16);
            Graphics gpr = pag.Graphics;

            Font fnt = new Font(Modules.UIVectSharpFontFamilyBold, 9);

            gpr.FillText(6 - fnt.MeasureText("A").Width * 0.5, 4, "A", fnt, Colour.FromRgb(74, 125, 177), TextBaselines.Middle);
            gpr.FillText(6 - fnt.MeasureText("Z").Width * 0.5, 12, "Z", fnt, Colour.FromRgb(114, 114, 114), TextBaselines.Middle);

            gpr.Translate(0.5, 0.5);
            gpr.StrokePath(new GraphicsPath().MoveTo(13, 0.5).LineTo(13, 13), Colour.FromRgb(114, 114, 114));
            gpr.FillPath(new GraphicsPath().MoveTo(10.5, 12.5).LineTo(13, 15).LineTo(15.5, 12.5).Close(), Colour.FromRgb(114, 114, 114));


            Canvas can = pag.PaintToCanvas(true);
            can.Margin = new Thickness(0, 0, 5, 0);
            can.UseLayoutRounding = true;

            return can;
        }

        Canvas GetDescendingIcon()
        {
            Page pag = new Page(16, 16);
            Graphics gpr = pag.Graphics;

            Font fnt = new Font(Modules.UIVectSharpFontFamilyBold, 9);

            gpr.FillText(6 - fnt.MeasureText("A").Width * 0.5, 12, "A", fnt, Colour.FromRgb(74, 125, 177), TextBaselines.Middle);
            gpr.FillText(6 - fnt.MeasureText("Z").Width * 0.5, 4, "Z", fnt, Colour.FromRgb(114, 114, 114), TextBaselines.Middle);

            gpr.Translate(0.5, 0.5);
            gpr.StrokePath(new GraphicsPath().MoveTo(13, 0.5).LineTo(13, 13), Colour.FromRgb(114, 114, 114));
            gpr.FillPath(new GraphicsPath().MoveTo(10.5, 12.5).LineTo(13, 15).LineTo(15.5, 12.5).Close(), Colour.FromRgb(114, 114, 114));


            Canvas can = pag.PaintToCanvas(true);
            can.Margin = new Thickness(0, 0, 5, 0);
            can.UseLayoutRounding = true;

            return can;
        }
    }
}
