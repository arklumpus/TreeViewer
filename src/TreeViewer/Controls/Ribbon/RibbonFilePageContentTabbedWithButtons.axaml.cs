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
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using System;
using System.Collections.Generic;

namespace TreeViewer
{
    public partial class RibbonFilePageContentTabbedWithButtons : UserControl
    {
        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<RibbonFilePageContentTabbedWithButtons, int>(nameof(SelectedIndex), 0);

        public int SelectedIndex
        {
            get { return GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }


        public RibbonFilePageContentTabbedWithButtons()
        {
            InitializeComponent(new List<(string, string, Control, Control)>()
            {
                
            });
        }

        public RibbonFilePageContentTabbedWithButtons(List<(string, string, Control, Control)> buttons)
        {
            InitializeComponent(buttons);
        }


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RibbonFilePageContentTabbedWithButtons.SelectedIndexProperty)
            {
                int oldValue = change.OldValue.GetValueOrDefault<int>();
                int newValue = change.NewValue.GetValueOrDefault<int>();

                buttonBorders[oldValue].Classes.Remove("active");
                buttonBorders[newValue].Classes.Add("active");

                if (clientControls[oldValue] != null)
                {
                    clientControls[oldValue].Opacity = 0;
                    clientControls[oldValue].RenderTransform = OffScreen;

                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(200);
                        clientControls[oldValue].IsVisible = false;
                    });
                }

                if (clientControls[newValue] != null)
                {
                    clientControls[newValue].IsVisible = true;
                    clientControls[newValue].Opacity = 1;
                    clientControls[newValue].RenderTransform = TransformOperations.Identity;
                }
            }
        }

        List<Border> buttonBorders = new List<Border>();
        List<Control> clientControls = new List<Control>();

        TransformOperations OffScreen;

        private void InitializeComponent(List<(string, string, Control, Control)> buttons)
        {
            AvaloniaXamlLoader.Load(this);
            this.Focusable = true;

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(32, 0);
            OffScreen = builder.Build();

            for (int i = 0; i < buttons.Count; i++)
            {
                Border brd = new Border() { BorderThickness = new Thickness(2), Focusable = true, Height = 55 };
                brd.Classes.Add("ButtonBorder");

                if (i == 0)
                {
                    brd.Classes.Add("active");
                }

                clientControls.Add(buttons[i].Item4);

                if (buttons[i].Item4 != null)
                {
                    if (i == 0)
                    {
                        buttons[i].Item4.RenderTransform = TransformOperations.Identity;
                    }
                    else
                    {
                        buttons[i].Item4.Opacity = 0;
                        buttons[i].Item4.RenderTransform = OffScreen;
                        buttons[i].Item4.IsVisible = false;
                    }

                    buttons[i].Item4.Transitions = new Transitions() { new DoubleTransition() { Property = Control.OpacityProperty, Duration = TimeSpan.FromMilliseconds(200) }, new TransformOperationsTransition() { Property = Control.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(200) } };

                    this.FindControl<Grid>("ClientContainer").Children.Add(buttons[i].Item4);
                }

                Grid grd = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(8, 0, 8, 0) };
                grd.ColumnDefinitions.Add(new ColumnDefinition(32, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(15, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Viewbox iconBox = new Viewbox() { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Child = buttons[i].Item3 };
                grd.Children.Add(iconBox);

                if (!string.IsNullOrEmpty(buttons[i].Item2))
                {
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    Grid.SetRowSpan(iconBox, 2);
                }

                {
                    TextBlock blk = new TextBlock() { Text = buttons[i].Item1, FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(38, 38, 38)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(blk, 2);
                    grd.Children.Add(blk);
                }

                if (!string.IsNullOrEmpty(buttons[i].Item2))
                {
                    TextBlock blk = new TextBlock() { Text = buttons[i].Item2, FontSize = 13, Foreground = new SolidColorBrush(Color.FromRgb(128, 128, 128)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                    Grid.SetColumn(blk, 2);
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                brd.Child = grd;

                int index = i;

                brd.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(brd).Pointer.Capture(brd);
                    this.SelectedIndex = index;
                };

                this.FindControl<StackPanel>("ButtonContainerStackPanel").Children.Add(brd);

                buttonBorders.Add(brd);
            }
        }
    }
}
