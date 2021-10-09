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
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Transformation;
using System;
using System.Collections.Generic;
using System.Linq;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class EditGradientWindow : ChildWindow
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

            Page pg = new Page(340, 64);
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

                pg = new Page(24, 24);
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

                can.Children.Add(new GradientControl() { Gradient = kvp.Value, Width = 24, Height = 24 });

                Button but = new Button() { Width = 32, Height = 32, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch, Margin = new Thickness(5), Padding = new Thickness(7.5), Content = can };

                but.Classes.Add("PlainButton");

                this.FindControl<WrapPanel>("DefaultGradientGrid").Children.Add(but);

                but.Click += (s, e) =>
                {
                    this.Gradient = new Gradient(kvp.Value.GradientStops);
                    BuildGradientStopContainer();
                };

                index++;
            }

            this.FindControl<Canvas>("GradientPreviewForeground").PointerMoved += (s, e) =>
            {
                if (MovingTag != null)
                {
                    double dx = (e.GetCurrentPoint(this.FindControl<Canvas>("GradientPreviewForeground")).Position.X - MoveStartPoint) / 340;

                    MoveStartPoint = Math.Max(0, Math.Min(340, e.GetCurrentPoint(this.FindControl<Canvas>("GradientPreviewForeground")).Position.X));

                    Grid container = this.FindControl<Grid>("GradientStopContainer");

                    for (int i = 0; i < container.Children.Count; i++)
                    {
                        Grid grd = (Grid)container.Children[i];

                        if ((string)grd.Tag == MovingTag)
                        {
                            ((NumericUpDown)grd.Children[0]).Value = Math.Max(0, Math.Min(1, ((NumericUpDown)grd.Children[0]).Value + dx));
                            break;
                        }
                    }
                }
            };

            this.FindControl<Canvas>("GradientPreviewForeground").PointerReleased += (s, e) =>
            {
                MovingTag = null;
                this.FindControl<Canvas>("GradientPreviewForeground").Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
            };

            this.FindControl<Canvas>("GradientPreviewForeground").PointerLeave += (s, e) =>
            {
                MovingTag = null;
                this.FindControl<Canvas>("GradientPreviewForeground").Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Arrow);
            };

            this.FindControl<Canvas>("GradientPreviewForeground").DoubleTapped += (s, e) =>
            {
                double xPos = ((TappedEventArgs)e).GetPosition(this.FindControl<Canvas>("GradientPreviewForeground")).X;
                double pos = xPos / 340;
                MovingTag = AddGradientStop(pos).Tag;
                MoveStartPoint = xPos;
                this.FindControl<Canvas>("GradientPreviewForeground").Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
            };
        }

        private string MovingTag = null;
        private double MoveStartPoint;

        private void BuildGradient()
        {
            this.FindControl<GradientControl>("GradientPreview").Gradient = Gradient;

            Page pg = new Page(345, 64);
            Graphics gpr = pg.Graphics;
            gpr.Translate(0, 5);

            Dictionary<string, Delegate> taggedActions = new Dictionary<string, Delegate>();

            Canvas can = null;

            for (int i = 0; i < this.Gradient.GradientStops.Count; i++)
            {
                if (string.IsNullOrEmpty(this.Gradient.GradientStops[i].Tag))
                {
                    this.Gradient.GradientStops[i].Tag = Guid.NewGuid().ToString();
                }

                gpr.StrokePath(new GraphicsPath().MoveTo(339 * this.Gradient.GradientStops[i].Position, 5).LineTo(339 * this.Gradient.GradientStops[i].Position, 63), this.Gradient.GradientStops[i].Colour.Reverse().WithAlpha(1.0), lineWidth: 1.5, lineCap: LineCaps.Round);

                GraphicsPath pth = new GraphicsPath().MoveTo(339 * this.Gradient.GradientStops[i].Position, 5).LineTo(339 * this.Gradient.GradientStops[i].Position - 5, 0).LineTo(339 * this.Gradient.GradientStops[i].Position - 5, -5).LineTo(339 * this.Gradient.GradientStops[i].Position + 5, -5).LineTo(339 * this.Gradient.GradientStops[i].Position + 5, 0).Close();
                gpr.FillPath(pth, this.Gradient.GradientStops[i].Colour.WithAlpha(1.0), this.Gradient.GradientStops[i].Tag);
                gpr.StrokePath(pth, this.Gradient.GradientStops[i].Colour.Reverse().WithAlpha(1.0), lineWidth: 1.5, lineCap: LineCaps.Round, lineJoin: LineJoins.Round, tag: this.Gradient.GradientStops[i].Tag);

                string tag = this.Gradient.GradientStops[i].Tag;

                taggedActions.Add(this.Gradient.GradientStops[i].Tag, (Action<Avalonia.Controls.Shapes.Path>)(pth =>
                {
                    pth.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                    pth.PointerPressed += (s, e) =>
                    {
                        e.Pointer.Capture(this.FindControl<Canvas>("GradientPreviewForeground"));
                        MoveStartPoint = e.GetCurrentPoint(this.FindControl<Canvas>("GradientPreviewForeground")).Position.X;
                        MovingTag = tag;
                        this.FindControl<Canvas>("GradientPreviewForeground").Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand);
                    };
                }));
            }

            can = pg.PaintToCanvas(taggedActions, false);
            can.Margin = new Thickness(0, -5, 0, 0);
            can.ClipToBounds = false;

            this.FindControl<Canvas>("GradientPreviewForeground").Children.Clear();

            this.FindControl<Canvas>("GradientPreviewForeground").Children.Add(can);
        }

        Dictionary<string, int> stopIndices = new Dictionary<string, int>();

        private void SortGradientStopControls(bool skipTransitions = false)
        {
            List<Grid> controls = new List<Grid>();

            Grid container = this.FindControl<Grid>("GradientStopContainer");

            for (int i = 0; i < container.Children.Count; i++)
            {
                controls.Add((Grid)container.Children[i]);
            }

            controls.Sort((a, b) =>
            {
                return ((NumericUpDown)a.Children[0]).Value.CompareTo(((NumericUpDown)b.Children[0]).Value);
            });

            for (int i = 0; i < controls.Count; i++)
            {
                string tag = (string)controls[i].Tag;

                if (!stopIndices.TryGetValue(tag, out int index) || index != i)
                {
                    TransformOperations.Builder builder = new TransformOperations.Builder(1);
                    builder.AppendTranslate(0, 30 * i);

                    if (!skipTransitions)
                    {
                        controls[i].RenderTransform = builder.Build();
                    }
                    else
                    {
                        Transitions prevTransitions = controls[i].Transitions;
                        controls[i].Transitions = null;
                        controls[i].RenderTransform = builder.Build();
                        controls[i].Transitions = prevTransitions;
                    }
                    stopIndices[tag] = i;
                }
            }

            container.Height = 30 * controls.Count;
        }

        private void BuildGradientStopContainer()
        {
            Transitions prevTransitions = this.FindControl<Grid>("GradientStopContainer").Transitions;

            this.FindControl<Grid>("GradientStopContainer").Transitions = null;

            this.FindControl<Grid>("GradientStopContainer").Children.Clear();
            for (int i = 0; i < this.Gradient.GradientStops.Count; i++)
            {
                GradientStop stop = this.Gradient.GradientStops[i];
                if (string.IsNullOrEmpty(stop.Tag))
                {
                    stop.Tag = Guid.NewGuid().ToString();
                }

                Grid grd = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Tag = stop.Tag };

                grd.Transitions = new Avalonia.Animation.Transitions()
                {
                    new TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
                };


                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));

                NumericUpDown nud = new NumericUpDown() { Minimum = 0, Maximum = 1, Margin = new Thickness(0, 2.5, 5, 2.5), FormatString = "0.###", Increment = 0.01, Value = this.Gradient.GradientStops[i].Position, FontSize = 13, Padding = new Thickness(5, 2, 5, 2) };
                nud.ValueChanged += (s, e) =>
                {
                    stop.Position = nud.Value;
                    this.Gradient.SortGradient();
                    BuildGradient();
                    SortGradientStopControls();
                };

                grd.Children.Add(nud);

                ColorButton but = new ColorButton() { Color = this.Gradient.GradientStops[i].Colour.ToAvalonia(), Margin = new Thickness(5, 2.5, 0, 2.5), FontFamily = this.FontFamily, FontSize = this.FontSize, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                but.Classes.Add("PlainButton");
                but.PropertyChanged += (s, e) =>
                {
                    if (e.Property == ColorButton.ColorProperty)
                    {
                        stop.Colour = but.Color.ToVectSharp();
                        BuildGradient();
                    }
                };

                Grid.SetColumn(but, 1);

                grd.Children.Add(but);

                Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                deleteButton.Classes.Add("SideBarButton");
                Grid.SetColumn(deleteButton, 2);
                grd.Children.Add(deleteButton);

                deleteButton.Click += (s, e) =>
                {
                    if (this.Gradient.GradientStops.Count > 2)
                    {
                        this.Gradient.GradientStops.Remove(stop);
                        this.FindControl<Grid>("GradientStopContainer").Children.Remove(grd);
                        BuildGradient();
                        SortGradientStopControls();
                    }
                };

                this.FindControl<Grid>("GradientStopContainer").Children.Add(grd);
            }

            SortGradientStopControls(true);
            this.FindControl<Grid>("GradientStopContainer").Transitions = prevTransitions;
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            double newPos = (from el in this.Gradient.GradientStops select el.Position).Average();
            AddGradientStop(newPos);
        }

        private GradientStop AddGradientStop(double newPos)
        {
            Colour col = this.Gradient.GetColour(newPos);

            GradientStop stop = new GradientStop(newPos, col);
            stop.Tag = Guid.NewGuid().ToString();

            this.Gradient.GradientStops.Add(stop);
            this.Gradient.SortGradient();

            Grid grd = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Tag = stop.Tag, Opacity = 0 };

            grd.Transitions = new Avalonia.Animation.Transitions()
            {
                new TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }
            };

            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grd.ColumnDefinitions.Add(new ColumnDefinition(64, GridUnitType.Pixel));
            grd.ColumnDefinitions.Add(new ColumnDefinition(24, GridUnitType.Pixel));

            NumericUpDown nud = new NumericUpDown() { Minimum = 0, Maximum = 1, Margin = new Thickness(0, 2.5, 5, 2.5), FormatString = "0.###", Increment = 0.01, Value = stop.Position, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
            nud.ValueChanged += (s, e) =>
            {
                stop.Position = nud.Value;
                this.Gradient.SortGradient();
                BuildGradient();
                SortGradientStopControls();
            };

            grd.Children.Add(nud);

            ColorButton but = new ColorButton() { Color = stop.Colour.ToAvalonia(), Margin = new Thickness(5, 2.5, 0, 2.5), FontFamily = this.FontFamily, FontSize = this.FontSize, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            but.Classes.Add("PlainButton");
            but.PropertyChanged += (s, e) =>
            {
                if (e.Property == ColorButton.ColorProperty)
                {
                    stop.Colour = but.Color.ToVectSharp();
                    BuildGradient();
                }
            };

            Grid.SetColumn(but, 1);

            grd.Children.Add(but);

            Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            deleteButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteButton, 2);
            grd.Children.Add(deleteButton);

            deleteButton.Click += (s, e) =>
            {
                if (this.Gradient.GradientStops.Count > 2)
                {
                    this.Gradient.GradientStops.Remove(stop);
                    this.FindControl<Grid>("GradientStopContainer").Children.Remove(grd);
                    BuildGradient();
                    SortGradientStopControls();
                }
            };

            this.FindControl<Grid>("GradientStopContainer").Children.Insert(this.Gradient.GradientStops.Count - 2, grd);

            BuildGradient();

            SortGradientStopControls();

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(100);

                grd.Opacity = 1;
            });

            return stop;
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
