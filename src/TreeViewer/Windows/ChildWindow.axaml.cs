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
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;

namespace TreeViewer
{
    public partial class ChildWindow : Window, IWindowWithToolTips
    {
        public List<Control> ControlsWithToolTips { get; } = new List<Control>();

        public static readonly Avalonia.StyledProperty<bool> CanMaximizeMinimizeProperty = Avalonia.AvaloniaProperty.Register<ChildWindow, bool>(nameof(CanMaximizeMinimize), true);
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

        public static readonly Avalonia.StyledProperty<bool> HasControlBoxProperty = Avalonia.AvaloniaProperty.Register<ChildWindow, bool>(nameof(HasControlBox), true);
        public bool HasControlBox
        {
            get
            {
                return GetValue(HasControlBoxProperty);
            }
            set
            {
                SetValue(HasControlBoxProperty, value);
            }
        }


        public static readonly Avalonia.StyledProperty<Avalonia.Layout.HorizontalAlignment> TitleAlignmentProperty = Avalonia.AvaloniaProperty.Register<ChildWindow, Avalonia.Layout.HorizontalAlignment>(nameof(TitleAlignment), Avalonia.Layout.HorizontalAlignment.Left);
        public Avalonia.Layout.HorizontalAlignment TitleAlignment
        {
            get
            {
                return GetValue(TitleAlignmentProperty);
            }
            set
            {
                SetValue(TitleAlignmentProperty, value);
            }
        }

        public ChildWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            Modules.SetIcon(this);
            SetupPlatform();
        }


        private object content;
        public new object Content 
        {
            get => content;

            set
            {
                content = value;
                contentContainer.Content = content;
            }
        }

        ContentPresenter contentContainer;

        private void SetupPlatform()
        {
            contentContainer = this.FindControl<ContentPresenter>("ContentPresenter");

            if (Modules.IsWindows)
            {
                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                this.ExtendClientAreaTitleBarHeightHint = -1;

                this.FindControl<Control>("TitleBar").PointerPressed += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(0);
                    }

                    PlatformImpl?.BeginMoveDrag(e);

                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(7);
                    }
                };

                this.FindControl<Control>("TitleBar").DoubleTapped += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                };
            }
            else if (Modules.IsMac)
            {
                this.FindControl<Control>("TitleBarBG").IsVisible = false;
                this.FindControl<Border>("MainBorder").BorderThickness = new Thickness(0);

                this.PlatformImpl.GetType().InvokeMember("SetTitleBarColor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, this.PlatformImpl, new object[] { Avalonia.Media.Color.FromRgb(0, 114, 178) });
            }
            else if (Modules.IsLinux)
            {
                this.FindControl<Control>("TitleBarBG").IsVisible = false;
                this.FindControl<Border>("MainBorder").BorderThickness = new Thickness(0);

                // Workaround for https://github.com/AvaloniaUI/Avalonia/issues/6159
                this.Opened += (s, e) =>
                {
                    if (this.WindowStartupLocation == WindowStartupLocation.CenterOwner)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Window parent = (Window)this.Owner;

                            this.Position = new PixelPoint((int)Math.Round(parent.Position.X + parent.Bounds.Width * 0.5 - this.Bounds.Width * 0.5), (int)Math.Round(parent.Position.Y + parent.Bounds.Height * 0.5 - this.Bounds.Height * 0.5));

                            this.InvalidateArrange();
                            this.InvalidateMeasure();
                        }, Avalonia.Threading.DispatcherPriority.MinValue);
                    }
                };
            }

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                this.Classes.Add("WindowsStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == CanMaximizeMinimizeProperty)
            {
                this.FindControl<WindowsChrome>("WindowsChromeDark").CanMaximizeMinimize = this.CanMaximizeMinimize;
                this.FindControl<WindowsChrome>("WindowsChromeLight").CanMaximizeMinimize = this.CanMaximizeMinimize;
            }
            else if (change.Property == HasControlBoxProperty)
            {
                this.FindControl<WindowsChrome>("WindowsChromeDark").IsVisible = this.HasControlBox;
                this.FindControl<WindowsChrome>("WindowsChromeLight").CanMaximizeMinimize = this.HasControlBox;
            }
        }
    }
}
