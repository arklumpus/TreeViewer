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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class EditGradientWindow : Window
    {
        private Gradient gradient = new Gradient(Modules.DefaultGradients["Viridis"].GradientStops);
        public Gradient Gradient
        {
            get
            {
                return gradient;
            }

            set
            {
                gradient = value;
                BuildGradient();
            }
        }

        public bool Result { get; set; } = false;

        public EditGradientWindow()
        {
            this.InitializeComponent();
        }

        public EditGradientWindow(Gradient gradient)
        {
            this.InitializeComponent();

            Gradient = gradient;

            BuildGradientStopContainer();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            BuildGradient();

            BuildGradientStopContainer();

            Page pg = new Page(480, 64);
            int width = 16;
            for (int x = 0; x < pg.Width; x += width * 2)
            {
                for (int y = 0; y < pg.Height / width; y++)
                {
                    if (y % 2 == 0)
                    {
                        pg.Graphics.FillRectangle(x, y * width, width, width, Colour.FromRgb(220, 220, 220));
                        pg.Graphics.FillRectangle(x + width, y * width, width, width, Colour.FromRgb(255, 255, 255));
                    }
                    else
                    {
                        pg.Graphics.FillRectangle(x, y * width, width, width, Colour.FromRgb(255, 255, 255));
                        pg.Graphics.FillRectangle(x + width, y * width, width, width, Colour.FromRgb(220, 220, 220));
                    }
                }
            }

            this.FindControl<Canvas>("GradientPreviewBackground").Children.Add(pg.PaintToCanvas());


            int index = 0;
            foreach (KeyValuePair<string, Gradient> kvp in Modules.DefaultGradients)
            {
                int row = index / 3;
                int column = index % 3;

                if (this.FindControl<Grid>("DefaultGradientGrid").RowDefinitions.Count < row + 1)
                {
                    this.FindControl<Grid>("DefaultGradientGrid").RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                }

                pg = new Page(49, 49);
                width = 7;
                for (int x = 0; x < pg.Width; x += width * 2)
                {
                    for (int y = 0; y < pg.Height / width; y++)
                    {
                        if (y % 2 == 0)
                        {
                            pg.Graphics.FillRectangle(x, y * width, width, width, Colour.FromRgb(220, 220, 220));
                            pg.Graphics.FillRectangle(x + width, y * width, width, width, Colour.FromRgb(255, 255, 255));
                        }
                        else
                        {
                            pg.Graphics.FillRectangle(x, y * width, width, width, Colour.FromRgb(255, 255, 255));
                            pg.Graphics.FillRectangle(x + width, y * width, width, width, Colour.FromRgb(220, 220, 220));
                        }
                    }
                }

                Canvas can = pg.PaintToCanvas();

                can.Children.Add(new GradientControl() { Gradient = kvp.Value, Width = 49, Height = 49 });

                Button but = new Button() { Width = 64, Height = 64, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch, Margin = new Thickness(5), Padding = new Thickness(7.5), Content = can };

                Grid.SetRow(but, row);
                Grid.SetColumn(but, column);

                this.FindControl<Grid>("DefaultGradientGrid").Children.Add(but);

                but.Click += (s, e) =>
                {
                    this.Gradient = new Gradient(kvp.Value.GradientStops);
                    BuildGradientStopContainer();
                };

                index++;
            }
        }


        private void BuildGradient()
        {
            this.FindControl<GradientControl>("GradientPreview").Gradient = Gradient;

            Page pg = new Page(480, 64);
            Graphics gpr = pg.Graphics;

            for (int i = 0; i < this.Gradient.GradientStops.Count; i++)
            {
                gpr.StrokePath(new GraphicsPath().MoveTo(479 * this.Gradient.GradientStops[i].Position, 5).LineTo(479 * this.Gradient.GradientStops[i].Position, 63), this.Gradient.GradientStops[i].Colour.Reverse().WithAlpha(1.0), lineWidth: 1.5, lineCap: LineCaps.Round);

                GraphicsPath pth = new GraphicsPath().MoveTo(479 * this.Gradient.GradientStops[i].Position, 5).LineTo(479 * this.Gradient.GradientStops[i].Position - 5, 0).LineTo(479 * this.Gradient.GradientStops[i].Position - 5, -5).LineTo(479 * this.Gradient.GradientStops[i].Position + 5, -5).LineTo(479 * this.Gradient.GradientStops[i].Position + 5, 0).Close();
                gpr.FillPath(pth, this.Gradient.GradientStops[i].Colour.WithAlpha(1.0));
                gpr.StrokePath(pth, this.Gradient.GradientStops[i].Colour.Reverse().WithAlpha(1.0), lineWidth: 1.5, lineCap: LineCaps.Round, lineJoin: LineJoins.Round);
            }

            Canvas can = pg.PaintToCanvas();
            can.ClipToBounds = false;

            this.FindControl<Canvas>("GradientPreviewForeground").Children.Clear();

            this.FindControl<Canvas>("GradientPreviewForeground").Children.Add(can);
        }

        private void BuildGradientStopContainer()
        {
            this.FindControl<StackPanel>("GradientStopContainer").Children.Clear();
            for (int i = 0; i < this.Gradient.GradientStops.Count; i++)
            {
                GradientStop stop = this.Gradient.GradientStops[i];

                Grid grd = new Grid();

                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));

                NumericUpDown nud = new NumericUpDown() { Minimum = 0, Maximum = 1, Margin = new Thickness(5), FormatString = "0.###", Increment = 0.01, Value = this.Gradient.GradientStops[i].Position };
                nud.ValueChanged += (s, e) =>
                {
                    stop.Position = nud.Value;
                    this.Gradient.SortGradient();
                    BuildGradient();
                };

                grd.Children.Add(nud);

                /*ColourButton but = new ColourButton() { Color = this.Gradient.GradientStops[i].Colour, Margin = new Thickness(5) };
                but.PointerReleased += async (s, e) =>
                {
                    ColorPickerWindow win = new ColorPickerWindow() { Color = stop.Colour };
                    await win.ShowDialog2(this);

                    if (win.Result)
                    {
                        stop.Colour = (Colour)win.Color;
                        BuildGradient();
                    }
                };*/

                AvaloniaColorPicker.ColorButton but = new AvaloniaColorPicker.ColorButton() { Color = this.Gradient.GradientStops[i].Colour.ToAvalonia(), Margin = new Thickness(5), FontFamily = this.FontFamily, FontSize = this.FontSize };
                but.PropertyChanged += (s, e) =>
                {
                    if (e.Property == AvaloniaColorPicker.ColorButton.ColorProperty)
                    {
                        stop.Colour = but.Color.ToVectSharp();
                        BuildGradient();
                    }
                };

                Grid.SetColumn(but, 1);

                grd.Children.Add(but);

                AddRemoveButton remBut = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove };

                remBut.PointerReleased += (s, e) =>
                {
                    if (this.Gradient.GradientStops.Count > 2)
                    {
                        this.Gradient.GradientStops.Remove(stop);
                        this.FindControl<StackPanel>("GradientStopContainer").Children.Remove(grd);
                        BuildGradient();
                    }
                };

                Grid.SetColumn(remBut, 2);
                grd.Children.Add(remBut);

                this.FindControl<StackPanel>("GradientStopContainer").Children.Add(grd);
            }


            Grid finalGrid = new Grid();
            finalGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            finalGrid.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));
            AddRemoveButton addBut = new AddRemoveButton();
            Grid.SetColumn(addBut, 1);
            finalGrid.Children.Add(addBut);
            addBut.PointerReleased += (s, e) =>
            {
                double newPos = (from el in this.Gradient.GradientStops select el.Position).Average();
                Colour col = this.Gradient.GetColour(newPos);

                GradientStop stop = new GradientStop(newPos, col);

                this.Gradient.GradientStops.Add(stop);
                this.Gradient.SortGradient();

                Grid grd = new Grid();

                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));

                NumericUpDown nud = new NumericUpDown() { Minimum = 0, Maximum = 1, Margin = new Thickness(5), FormatString = "0.###", Increment = 0.01, Value = stop.Position, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
                nud.ValueChanged += (s, e) =>
                {
                    stop.Position = nud.Value;
                    this.Gradient.SortGradient();
                    BuildGradient();
                };

                grd.Children.Add(nud);

                /*ColourButton but = new ColourButton() { Color = stop.Colour, Margin = new Thickness(5) };
                but.PointerReleased += async (s, e) =>
                {
                    ColorPickerWindow win = new ColorPickerWindow() { Color = stop.Colour };
                    await win.ShowDialog2(this);

                    if (win.Result)
                    {
                        stop.Colour = (Colour)win.Color;
                        BuildGradient();
                    }
                };*/

                AvaloniaColorPicker.ColorButton but = new AvaloniaColorPicker.ColorButton() { Color = stop.Colour.ToAvalonia(), Margin = new Thickness(5), FontFamily = this.FontFamily, FontSize = this.FontSize };
                but.PropertyChanged += (s, e) =>
                {
                    if (e.Property == AvaloniaColorPicker.ColorButton.ColorProperty)
                    {
                        stop.Colour = but.Color.ToVectSharp();
                        BuildGradient();
                    }
                };

                Grid.SetColumn(but, 1);

                grd.Children.Add(but);

                AddRemoveButton remBut = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove };

                remBut.PointerReleased += (s, e) =>
                {
                    if (this.Gradient.GradientStops.Count > 2)
                    {
                        this.Gradient.GradientStops.Remove(stop);
                        this.FindControl<StackPanel>("GradientStopContainer").Children.Remove(grd);
                        BuildGradient();
                    }
                };

                Grid.SetColumn(remBut, 2);
                grd.Children.Add(remBut);

                this.FindControl<StackPanel>("GradientStopContainer").Children.Insert(this.Gradient.GradientStops.Count - 1, grd);

                BuildGradient();
            };

            this.FindControl<StackPanel>("GradientStopContainer").Children.Add(finalGrid);

        }

        private void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Gradient = new Gradient(this.Gradient.GradientStops);
            this.Close();
        }
        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
