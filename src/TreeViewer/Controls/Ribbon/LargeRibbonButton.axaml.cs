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
using System;
using System.Collections.Generic;
using Avalonia.Media;
using Avalonia.VisualTree;
using Avalonia.Media.Transformation;
using Avalonia.Input;
using System.Linq;
using Avalonia.Controls.Shapes;

namespace TreeViewer
{
    public abstract class RibbonButton : UserControl
    {
        public abstract List<Control> SubItems { get; }
        public abstract string ButtonText { get; set; }
        public abstract List<string> SubItemsText { get; }

        public abstract void InvokeAction(int index);

        public abstract string ShortcutText { get; set; }
        public abstract List<string> SubItemsShortcuts { get; }
    }

    public partial class LargeRibbonButton : RibbonButton
    {
        public static readonly StyledProperty<string> ButtonTextProperty = AvaloniaProperty.Register<LargeRibbonButton, string>(nameof(ButtonText), "");
        public override string ButtonText
        {
            get { return GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly StyledProperty<Control> IconProperty = AvaloniaProperty.Register<LargeRibbonButton, Control>(nameof(Icon), null);
        public Control Icon
        {
            get { return GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<LargeRibbonButton, bool>(nameof(IsActive), false);
        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly StyledProperty<string> ShortcutTextProperty = AvaloniaProperty.Register<LargeRibbonButton, string>(nameof(ShortcutText), "");
        public override string ShortcutText
        {
            get { return GetValue(ShortcutTextProperty); }
            set { SetValue(ShortcutTextProperty, value); }
        }

        public override List<Control> SubItems { get; } = new List<Control>();
        public override List<string> SubItemsText { get; } = new List<string>();
        public override List<string> SubItemsShortcuts { get; } = new List<string>();


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == LargeRibbonButton.ButtonTextProperty)
            {
                this.FindControl<TextBlock>("TextBlock").Text = change.NewValue.GetValueOrDefault<string>();
            }
            else if (change.Property == LargeRibbonButton.IconProperty)
            {
                this.FindControl<Viewbox>("IconBox").Child = change.NewValue.GetValueOrDefault<Control>();
            }
            else if (change.Property == LargeRibbonButton.IsEnabledProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    this.Opacity = 1;
                }
                else
                {
                    this.Opacity = 0.35;
                }
            }
            else if (change.Property == LargeRibbonButton.IsActiveProperty)
            {
                if (this.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
                {
                    if (this.ButtonType == ButtonTypes.ButtonWithActions)
                    {
                        Border buttonBorder = this.FindControl<Border>("ButtonBorder");

                        if (change.NewValue.GetValueOrDefault<bool>())
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                            buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                        }
                        else if (this.IsPointerOver)
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                            buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                            buttonBorder.BorderBrush = Brushes.Transparent;
                        }
                    }
                    else if (this.ButtonType == ButtonTypes.ButtonWithDefaultAction)
                    {
                        Border buttonBorder = this.FindControl<Border>("ButtonBorder");

                        if (change.NewValue.GetValueOrDefault<bool>())
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                            buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                        }
                        else if (this.IsPointerOver)
                        {
                            buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                            buttonBorder.Background = Brushes.Transparent;
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                            buttonBorder.BorderBrush = Brushes.Transparent;
                        }
                    }
                }
                else if (this.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
                {
                    if (this.ButtonType == ButtonTypes.ButtonWithActions)
                    {
                        Border buttonBorder = this.FindControl<Border>("ButtonBorder");

                        if (change.NewValue.GetValueOrDefault<bool>())
                        {
                            buttonBorder.Background = MacOSBackgroundBrush;
                        }
                        else if (this.IsPointerOver)
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                        }
                    }
                    else if (this.ButtonType == ButtonTypes.ButtonWithDefaultAction)
                    {
                        Border buttonBorder = this.FindControl<Border>("ButtonBorder");

                        if (change.NewValue.GetValueOrDefault<bool>())
                        {
                            buttonBorder.Background = MacOSBackgroundBrush;
                            buttonBorder.BorderBrush = Brushes.Transparent;
                        }
                        else if (this.IsPointerOver)
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                        }
                    }
                }
            }
        }

        public override void InvokeAction(int index)
        {
            ButtonPressed?.Invoke(this, new ButtonPressedEventArgs(index));
        }

        private enum ButtonTypes { SimpleButton, ButtonWithActions, ButtonWithDefaultAction }

        private ButtonTypes ButtonType;

        public event EventHandler<ButtonPressedEventArgs> ButtonPressed;

        public LargeRibbonButton()
        {
            InitializeComponent(new List<(string, Control, string)>() { });
        }

        public LargeRibbonButton(List<(string, Control, string)> subItems)
        {
            InitializeComponent(subItems);
        }
        private void InitializeComponent(List<(string, Control, string)> subItems)
        {
            AvaloniaXamlLoader.Load(this);

            SubItemsText.AddRange(from el in subItems select el.Item1);

            if (subItems.Count == 0)
            {
                this.ButtonType = ButtonTypes.SimpleButton;


                this.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(this.FindControl<Border>("ButtonBorder")).Pointer.Capture(this.FindControl<Border>("ButtonBorder"));
                };

                this.PointerReleased += (s, e) =>
                {
                    Point point = e.GetCurrentPoint(this).Position;

                    if (point.X >= 0 && point.Y >= 0 && point.X <= this.Bounds.Width && point.Y <= this.Bounds.Height)
                    {
                        ButtonPressed?.Invoke(this, new ButtonPressedEventArgs(-1));
                    }
                };
            }
            else if (!string.IsNullOrEmpty(subItems[0].Item1))
            {
                CreateSubMenu(subItems);
                SubItemsShortcuts.AddRange(from el in subItems select el.Item3);

                this.ButtonType = ButtonTypes.ButtonWithActions;

                this.PointerPressed += (s, e) =>
                {
                    if (ToolTip.GetTip(this) != null)
                    {
                        ToolTip.SetIsOpen(this, false);
                    }
                    
                    e.GetCurrentPoint(this.FindControl<Border>("ButtonBorder")).Pointer.Capture(this.FindControl<Border>("ButtonBorder"));

                    if (!this.IsActive)
                    {
                        OpenSubMenu();
                    }
                    else
                    {
                        CloseSubMenu();
                    }
                };

                this.LostFocus += (s, e) =>
                {
                    if (FocusManager.Instance.Current != this.SubMenu)
                    {
                        this.CloseSubMenu();
                    }
                };
            }
            else
            {
                CreateSubMenu(subItems.Skip(1).ToList());
                SubItemsShortcuts.AddRange(from el in subItems.Skip(1) select el.Item3);

                this.ButtonType = ButtonTypes.ButtonWithDefaultAction;

                Border buttonIconNamePanel = this.FindControl<Border>("IconParent");
                Border arrowGrid = this.FindControl<Border>("TextArrowContainer");

                buttonIconNamePanel.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(buttonIconNamePanel).Pointer.Capture(buttonIconNamePanel);
                    CloseSubMenu();
                };

                buttonIconNamePanel.PointerReleased += (s, e) =>
                {
                    Point point = e.GetCurrentPoint(this).Position;

                    if (point.X >= 0 && point.Y >= 0 && point.X <= this.Bounds.Width && point.Y <= this.Bounds.Height)
                    {
                        ButtonPressed?.Invoke(this, new ButtonPressedEventArgs(-1));
                    }
                };

                arrowGrid.PointerPressed += (s, e) =>
                {
                    if (ToolTip.GetTip(this) != null)
                    {
                        ToolTip.SetIsOpen(this, false);
                    }

                    e.GetCurrentPoint(arrowGrid).Pointer.Capture(arrowGrid);

                    if (!this.IsActive)
                    {
                        OpenSubMenu();
                    }
                    else
                    {
                        CloseSubMenu();
                    }
                };

                this.LostFocus += (s, e) =>
                {
                    if (FocusManager.Instance.Current != this.SubMenu)
                    {
                        this.CloseSubMenu();
                    }
                };
            }

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                SetStylesWindows();
            }
            else if  (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                SetStylesMacOS();
            }
        }

        private Control SubMenu;
        private double SubMenuWidth = 0;

        private void CreateSubMenu(List<(string, Control, string)> items)
        {
            Border subMenuBorder = new Border() { BorderThickness = new Thickness(1), Focusable = true, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, BoxShadow = new BoxShadows(new BoxShadow() { Blur = 4, Color = Color.FromArgb(80, 0, 0, 0), Spread = 0 }) };

            subMenuBorder.Classes.Add("MenuBorder");

            StackPanel subMenu = new StackPanel() { Background = Brushes.Transparent };

            List<StackPanel> menuItems = new List<StackPanel>();

            double width = 0;

            for (int i = 0; i < items.Count; i++)
            {
                StackPanel item = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Height = 24 };
                item.Classes.Add("MenuItem");

                Viewbox iconBox = new Viewbox() { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                iconBox.Child = items[i].Item2;

                TextBlock text = new TextBlock() { Text = items[i].Item1, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(5, 0, 24, 0) };

                width = Math.Max(60 + (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle ? 13 : 0) + AvaloniaBugFixes.MeasureTextWidth(items[i].Item1, Modules.UIFontFamily, FontStyle.Normal, FontWeight.Regular, 13), width);

                item.Children.Add(iconBox);
                item.Children.Add(text);

                subMenu.Children.Add(item);

                int index = i;

                item.PointerReleased += (s, e) =>
                {
                    Point pos = e.GetCurrentPoint(item).Position;

                    if (pos.X >= 0 && pos.Y >= 0 && pos.X <= item.Bounds.Width && pos.Y <= item.Bounds.Height)
                    {
                        ButtonPressed?.Invoke(this, new ButtonPressedEventArgs(index));
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

                SubItems.Add(item);
            }

            ScrollViewer menuScrollViewer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };

            menuScrollViewer.Content = subMenu;
            subMenuBorder.Child = menuScrollViewer;

            SubMenu = subMenuBorder;
            SubMenuWidth = Math.Floor(width);
            SubMenu.LostFocus += (s, e) => CloseSubMenu();

            SubMenu.Transitions = new Avalonia.Animation.Transitions()
            {
                new Avalonia.Animation.TransformOperationsTransition() { Property = Border.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new Avalonia.Animation.DoubleTransition() { Property = Border.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }
            };
        }
        private void OpenSubMenu()
        {
            double width = SubMenuWidth;
            (this.SubMenu.Parent as Grid)?.Children.Remove(this.SubMenu);

            Grid popupLayer = this.FindAncestorOfType<Window>().FindControl<Grid>("PopupLayer");
            Point finalPos = popupLayer.PointToClient(this.PointToScreen(new Point(4, this.Bounds.Height)));

            double maxHeight = popupLayer.Bounds.Height - finalPos.Y - 10;
            this.SubMenu.MaxHeight = maxHeight;

            Point pos = popupLayer.PointToClient(this.PointToScreen(new Point(4, this.Bounds.Height - 16)));

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(Math.Max(Math.Min(pos.X, popupLayer.Bounds.Width - width), 0), pos.Y);
            this.SubMenu.RenderTransform = builder.Build();
            this.SubMenu.Opacity = 0;

            
            popupLayer.Children.Add(this.SubMenu);

            builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(Math.Max(Math.Min(finalPos.X, popupLayer.Bounds.Width - width), 0), finalPos.Y);
            this.SubMenu.RenderTransform = builder.Build();
            this.SubMenu.Opacity = 1;
            this.IsActive = true;
        }



        private void CloseSubMenu()
        {

            (this.SubMenu.Parent as Grid)?.Children.Remove(this.SubMenu);
            this.IsActive = false;
        }

        private GlobalSettings.InterfaceStyles InterfaceStyle;

        private void SetStylesWindows()
        {
            this.InterfaceStyle = GlobalSettings.InterfaceStyles.WindowsStyle;
            this.FindControl<Path>("ArrowDownPath").Stroke = new SolidColorBrush(Color.FromRgb(89, 89, 89));

            if (this.ButtonType == ButtonTypes.SimpleButton)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 75;

                this.FindControl<Grid>("ArrowGrid").IsVisible = false;

                Border buttonBorder = this.FindControl<Border>("ButtonBorder");
                buttonBorder.PointerEnter += (s, e) =>
                {
                    buttonBorder.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    buttonBorder.Background = Brushes.Transparent;
                };

                buttonBorder.PointerPressed += (s, e) =>
                {
                    buttonBorder.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                };

                buttonBorder.PointerReleased += (s, e) =>
                {
                    if (buttonBorder.IsPointerOver)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                    }
                    else
                    {
                        buttonBorder.Background = Brushes.Transparent;
                    }
                };
            }
            else if (this.ButtonType == ButtonTypes.ButtonWithActions)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 87;
                buttonGrid.Margin = new Thickness(-1);

                Border buttonBorder = this.FindControl<Border>("ButtonBorder");
                buttonBorder.BorderBrush = Brushes.Transparent;
                buttonBorder.BorderThickness = new Thickness(1);


                buttonBorder.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                    }
                    else
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = Brushes.Transparent;
                        buttonBorder.BorderBrush = Brushes.Transparent;
                    }
                    else
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };

                buttonBorder.PointerPressed += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                    }
                    else
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };

                buttonBorder.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (buttonBorder.IsPointerOver)
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                            buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                            buttonBorder.BorderBrush = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(198, 198, 198));
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };
            }
            else if (this.ButtonType == ButtonTypes.ButtonWithDefaultAction)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 87;
                buttonGrid.Margin = new Thickness(-1);

                Border iconParent = this.FindControl<Border>("IconParent");
                iconParent.Background = Brushes.Transparent;

                Border textArrowContainer = this.FindControl<Border>("TextArrowContainer");
                textArrowContainer.Background = Brushes.Transparent;

                Border buttonBorder = this.FindControl<Border>("ButtonBorder");
                buttonBorder.BorderBrush = Brushes.Transparent;
                buttonBorder.BorderThickness = new Thickness(1);


                buttonBorder.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                    }
                    else
                    {
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.BorderBrush = Brushes.Transparent;
                    }
                    else
                    {
                        buttonBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                    }
                };

                iconParent.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        iconParent.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                iconParent.PointerLeave += (s, e) =>
                {
                    iconParent.Background = Brushes.Transparent;
                };

                textArrowContainer.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        textArrowContainer.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerLeave += (s, e) =>
                {
                    textArrowContainer.Background = Brushes.Transparent;
                };

                iconParent.PointerPressed += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        iconParent.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                iconParent.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (iconParent.IsPointerOver)
                        {
                            iconParent.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                        }
                        else
                        {
                            iconParent.Background = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerPressed += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        textArrowContainer.Background = new SolidColorBrush(Color.FromRgb(177, 177, 177));
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (textArrowContainer.IsPointerOver)
                        {
                            textArrowContainer.Background = new SolidColorBrush(Color.FromRgb(210, 210, 210));
                        }
                        else
                        {
                            textArrowContainer.Background = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

            }
        }

        static LinearGradientBrush MacOSBackgroundBrush = new LinearGradientBrush() { StartPoint = new RelativePoint(0, 0, RelativeUnit.Absolute), EndPoint = new RelativePoint(0, 80, RelativeUnit.Absolute), GradientStops = new GradientStops() { new Avalonia.Media.GradientStop(Color.FromRgb(157, 157, 157), 0), new Avalonia.Media.GradientStop(Color.FromRgb(182, 182, 182), 0.114), new Avalonia.Media.GradientStop(Color.FromRgb(173, 173, 173), 1) } };

        private void SetStylesMacOS()
        {
            this.InterfaceStyle = GlobalSettings.InterfaceStyles.MacOSStyle;
            this.FindControl<Path>("ArrowDownPath").Stroke = Brushes.Black;

            Border buttonBorder = this.FindControl<Border>("ButtonBorder");
            buttonBorder.CornerRadius = new CornerRadius(4);

            Border iconParent = this.FindControl<Border>("IconParent");
            iconParent.CornerRadius = new CornerRadius(3, 3, 0, 0);

            Border textArrowContainer = this.FindControl<Border>("TextArrowContainer");
            textArrowContainer.CornerRadius = new CornerRadius(0, 0, 3, 3);

            if (this.ButtonType == ButtonTypes.SimpleButton)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 75;

                this.FindControl<Grid>("ArrowGrid").IsVisible = false;

                
                buttonBorder.PointerEnter += (s, e) =>
                {
                    buttonBorder.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    buttonBorder.Background = Brushes.Transparent;
                };

                buttonBorder.PointerPressed += (s, e) =>
                {
                    buttonBorder.Background = MacOSBackgroundBrush;
                };

                buttonBorder.PointerReleased += (s, e) =>
                {
                    if (buttonBorder.IsPointerOver)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                    }
                    else
                    {
                        buttonBorder.Background = Brushes.Transparent;
                    }
                };
            }
            else if (this.ButtonType == ButtonTypes.ButtonWithActions)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 85;

                buttonBorder.BorderBrush = Brushes.Transparent;

                buttonBorder.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                    }
                    else
                    {
                        buttonBorder.Background = MacOSBackgroundBrush;
                    }
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = Brushes.Transparent;
                    }
                    else
                    {
                        buttonBorder.Background = MacOSBackgroundBrush;
                    }
                };

                buttonBorder.PointerPressed += (s, e) =>
                {
                    buttonBorder.Background = MacOSBackgroundBrush;
                };

                buttonBorder.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (buttonBorder.IsPointerOver)
                        {
                            buttonBorder.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                        }
                        else
                        {
                            buttonBorder.Background = Brushes.Transparent;
                            buttonBorder.BorderBrush = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        buttonBorder.Background = MacOSBackgroundBrush;
                    }
                };
            }
            else if (this.ButtonType == ButtonTypes.ButtonWithDefaultAction)
            {
                Grid buttonGrid = this.FindControl<Grid>("ButtonGrid");
                buttonGrid.Background = Brushes.Transparent;
                buttonGrid.MaxWidth = 85;

                iconParent.Background = Brushes.Transparent;
                
                textArrowContainer.Background = Brushes.Transparent;

                buttonBorder.BorderBrush = Brushes.Transparent;


                buttonBorder.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = new SolidColorBrush(Color.FromRgb(230, 230, 230));
                    }
                    else
                    {
                        buttonBorder.Background = MacOSBackgroundBrush;
                    }
                };

                buttonBorder.PointerLeave += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        buttonBorder.Background = Brushes.Transparent;
                    }
                    else
                    {
                        buttonBorder.Background = MacOSBackgroundBrush;
                    }
                };

                iconParent.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        iconParent.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                iconParent.PointerLeave += (s, e) =>
                {
                    iconParent.Background = Brushes.Transparent;
                };

                textArrowContainer.PointerEnter += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        textArrowContainer.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerLeave += (s, e) =>
                {
                    textArrowContainer.Background = Brushes.Transparent;
                };

                iconParent.PointerPressed += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        iconParent.Background = MacOSBackgroundBrush;
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                iconParent.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (iconParent.IsPointerOver)
                        {
                            iconParent.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                        }
                        else
                        {
                            iconParent.Background = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        iconParent.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerPressed += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        textArrowContainer.Background = MacOSBackgroundBrush;
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

                textArrowContainer.PointerReleased += (s, e) =>
                {
                    if (!this.IsActive)
                    {
                        if (textArrowContainer.IsPointerOver)
                        {
                            textArrowContainer.Background = new SolidColorBrush(Color.FromRgb(212, 212, 212));
                        }
                        else
                        {
                            textArrowContainer.Background = Brushes.Transparent;
                        }
                    }
                    else
                    {
                        textArrowContainer.Background = Brushes.Transparent;
                    }
                };

            }
        }
    }
}