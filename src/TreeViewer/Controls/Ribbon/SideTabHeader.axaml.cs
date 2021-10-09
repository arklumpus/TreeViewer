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
using Avalonia.Markup.Xaml;
using Avalonia.Media.Transformation;
using System.Collections.Generic;

namespace TreeViewer
{
    public partial class SideTabHeader : UserControl
    {
        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<SideTabHeader, int>(nameof(SelectedIndex), -1);

        public int SelectedIndex
        {
            get => GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }

        public SideTabHeader()
        {
            InitializeComponent(new List<Control>());
        }

        public SideTabHeader(List<Control> buttons)
        {
            InitializeComponent(buttons);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SideTabHeader.SelectedIndexProperty)
            {
                int oldValue = change.OldValue.GetValueOrDefault<int>();

                if (oldValue >= 0)
                {
                    this.ButtonBorders[oldValue].Classes.Remove("active");
                }

                int newValue = change.NewValue.GetValueOrDefault<int>();

                if (newValue >= 0)
                {
                    this.ButtonBorders[newValue].Classes.Add("active");

                    TransformOperations.Builder builder = new TransformOperations.Builder(1);
                    builder.AppendTranslate(20 + 50 * newValue, 0);
                    this.FindControl<Canvas>("ArrowCanvas").RenderTransform = builder.Build();

                }
            }
        }

        public List<Border> ButtonBorders { get; } = new List<Border>();

        private void InitializeComponent(List<Control> buttons)
        {
            AvaloniaXamlLoader.Load(this);

            for (int i = 0; i < buttons.Count; i++)
            {
                Border buttonBorder = new Border() { Width = 40, Height = 40, Padding = new Thickness(4) };
                buttonBorder.Child = new Viewbox() { Width = 32, Height = 32, Child = buttons[i] }; ;
                buttonBorder.Classes.Add("ButtonBorder");

                ButtonBorders.Add(buttonBorder);

                buttonBorder.PointerPressed += (s, e) =>
                {
                    buttonBorder.Classes.Add("pressed");
                };

                int index = i;

                buttonBorder.PointerReleased += (s, e) =>
                {
                    buttonBorder.Classes.Remove("pressed");

                    Point pos = e.GetCurrentPoint(buttonBorder).Position;

                    if (pos.X >= 0 && pos.Y >= 0 && pos.X < 40 && pos.Y < 40)
                    {
                        this.SelectedIndex = index;
                    }
                };

                this.FindControl<StackPanel>("ButtonStackPanel").Children.Add(buttonBorder);
            }

            this.SelectedIndex = 0;
        }
    }
}
