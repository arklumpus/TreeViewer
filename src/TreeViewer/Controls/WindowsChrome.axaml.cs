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
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using System;
using System.Globalization;

namespace TreeViewer
{
    public partial class WindowsChrome : UserControl
    {
        public static readonly Avalonia.StyledProperty<bool> CanMaximizeMinimizeProperty = Avalonia.AvaloniaProperty.Register<WindowsChrome, bool>(nameof(CanMaximizeMinimize), true);
        public bool CanMaximizeMinimize
        {
            get
            {
                return GetValue(CanMaximizeMinimizeProperty);
            }
            set
            {
                SetValue(CanMaximizeMinimizeProperty, value);
            }
        }

        public WindowsChrome()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Canvas>("MinimizeButton").PointerPressed += AddPressed;
            this.FindControl<Canvas>("MaximizeButton").PointerPressed += AddPressed;
            this.FindControl<Canvas>("CloseButton").PointerPressed += AddPressed;

            this.FindControl<Canvas>("MinimizeButton").PointerReleased += RemovePressed;
            this.FindControl<Canvas>("MaximizeButton").PointerReleased += RemovePressed;
            this.FindControl<Canvas>("CloseButton").PointerReleased += RemovePressed;

            this.FindControl<Canvas>("CloseButton").PointerReleased += (s, e) =>
            {
                Canvas button = this.FindControl<Canvas>("CloseButton");

                Point pos = e.GetPosition(button);
                if (pos.X >= 0 && pos.Y >= 0 && pos.X < button.Width && pos.Y < button.Height)
                {
                    this.FindAncestorOfType<Window>().Close();
                }
            };

            this.FindControl<Canvas>("MinimizeButton").PointerReleased += (s, e) =>
            {
                Canvas button = this.FindControl<Canvas>("MinimizeButton");

                Point pos = e.GetPosition(button);
                if (pos.X >= 0 && pos.Y >= 0 && pos.X < button.Width && pos.Y < button.Height)
                {
                    this.FindAncestorOfType<Window>().WindowState = WindowState.Minimized;
                }
            };

            this.FindControl<Canvas>("MaximizeButton").PointerReleased += (s, e) =>
            {
                Canvas button = this.FindControl<Canvas>("MaximizeButton");

                Point pos = e.GetPosition(button);
                if (pos.X >= 0 && pos.Y >= 0 && pos.X < button.Width && pos.Y < button.Height)
                {
                    Window win = this.FindAncestorOfType<Window>();

                    if (win.WindowState == WindowState.Maximized || win.WindowState == WindowState.FullScreen)
                    {
                        win.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        win.WindowState = WindowState.Maximized;
                    }
                }
            };

            if (Modules.IsWindows)
            {
                this.AttachedToVisualTree += (s, e) =>
                {
                    Window win = this.FindAncestorOfType<Window>();
                    win.PropertyChanged += (s, e) =>
                    {
                        if (e.Property == Window.WindowStateProperty)
                        {
                            if ((e.NewValue is WindowState state && (state == WindowState.Maximized || state == WindowState.FullScreen)) &&
                                (e.OldValue is WindowState state2 && !(state2 == WindowState.Maximized || state2 == WindowState.FullScreen)))
                            {
                                win.Padding = new Thickness(7);
                            }
                            else if ((e.NewValue is WindowState state3 && !(state3 == WindowState.Maximized || state3 == WindowState.FullScreen)) &&
                                (e.OldValue is WindowState state4 && (state4 == WindowState.Maximized || state4 == WindowState.FullScreen)))
                            {
                                win.Padding = new Thickness(0);
                            }
                        }
                    };
                };
            }
        }

        private static void AddPressed(object sender, PointerPressedEventArgs e)
        {
            ((Canvas)sender!)?.Classes.Add("pressed");
        }

        private static void RemovePressed(object sender, PointerReleasedEventArgs e)
        {
            ((Canvas)sender!)?.Classes.Remove("pressed");
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CanMaximizeMinimizeProperty)
            {
                bool newVal = change.NewValue.GetValueOrDefault<bool>();

                this.FindControl<Canvas>("MinimizeButton").IsVisible = newVal;
                this.FindControl<Canvas>("MaximizeButton").IsVisible = newVal;
            }
        }
    }

    public class MaximizeWindowVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is WindowState state && (state == WindowState.Maximized || state == WindowState.FullScreen))
            {
                return true == System.Convert.ToBoolean(parameter);
            }
            else
            {
                return false == System.Convert.ToBoolean(parameter);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool state && state)
            {
                return WindowState.Maximized;
            }
            else
            {
                return WindowState.Normal;
            }
        }
    }
}
