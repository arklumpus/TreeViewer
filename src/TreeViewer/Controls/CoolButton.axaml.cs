using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Styling;
using System;
using VectSharp;

namespace TreeViewer
{
    public class CoolButton : UserControl
    {
        public static readonly StyledProperty<IControl> TitleProperty = AvaloniaProperty.Register<CoolButton, IControl>(nameof(Title), null);

        public IControl Title
        {
            get { return GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly StyledProperty<IControl> ButtonContentProperty = AvaloniaProperty.Register<CoolButton, IControl>(nameof(ButtonContent), null);

        public IControl ButtonContent
        {
            get { return GetValue(ButtonContentProperty); }
            set { SetValue(ButtonContentProperty, value); }
        }

        public static readonly StyledProperty<CornerRadius> CornerRadiusProperty = AvaloniaProperty.Register<CoolButton, CornerRadius>(nameof(CornerRadius), new CornerRadius(15));

        public CornerRadius CornerRadius
        {
            get { return GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly StyledProperty<double> HueProperty = AvaloniaProperty.Register<CoolButton, double>(nameof(Hue), 0.63762580995930973);

        public double Hue
        {
            get { return GetValue(HueProperty); }
            set { SetValue(HueProperty, value); }
        }

        public event EventHandler<PointerReleasedEventArgs> Click;

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TitleProperty)
            {
                if ((IControl)change.NewValue.Value == null)
                {
                    this.FindControl<Grid>("TitleContainer").Children.Clear();
                    this.FindControl<Border>("TitleBorder").IsVisible = false;
                }
                else
                {
                    this.FindControl<Grid>("TitleContainer").Children.Clear();
                    this.FindControl<Grid>("TitleContainer").Children.Add((IControl)change.NewValue.Value);
                    this.FindControl<Border>("TitleBorder").IsVisible = true;
                }
            }
            else if (change.Property == ButtonContentProperty)
            {
                if ((IControl)change.NewValue.Value == null)
                {
                    this.FindControl<Grid>("ContentContainer").Children.Clear();
                }
                else
                {
                    this.FindControl<Grid>("ContentContainer").Children.Clear();
                    this.FindControl<Grid>("ContentContainer").Children.Add((IControl)change.NewValue.Value);
                }
            }
            else if (change.Property == CornerRadiusProperty)
            {
                CornerRadius cornerRadius = change.NewValue.Value as CornerRadius? ?? new CornerRadius(0);

                this.FindControl<Border>("Border1").CornerRadius = cornerRadius;
                this.FindControl<Border>("Border2").CornerRadius = cornerRadius;
                this.FindControl<Border>("Border3").CornerRadius = cornerRadius;

                this.FindControl<Border>("TitleBorder").CornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, 0);
                this.FindControl<Border>("TitleBorder2").CornerRadius = new CornerRadius(cornerRadius.TopLeft, cornerRadius.TopRight, 0, 0);
            }
            else if (change.Property == IsEnabledProperty)
            {
                this.Opacity = this.IsEnabled ? 1 : 0.4;
            }
            else if (change.Property == HueProperty)
            {
                this.SetHue(this.Hue);
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            this.FindControl<Border>("Border1").Classes.Add("Pressed");
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);

            this.FindControl<Border>("Border1").Classes.Remove("Pressed");

            if (!e.Handled)
            {
                PointerPoint point = e.GetCurrentPoint(this);

                if (point.Position.X >= 0 && point.Position.Y >= 0 && point.Position.X < this.Bounds.Width && point.Position.Y < this.Bounds.Height)
                {
                    Click?.Invoke(this, e);
                }
            }
        }

        public CoolButton()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Border>("Border2").Transitions = new Avalonia.Animation.Transitions() { new BrushTransition() { Property = Border.BackgroundProperty, Duration = new TimeSpan(0, 0, 0, 0, 75) } };
            this.FindControl<Border>("TitleBorder2").Transitions = new Avalonia.Animation.Transitions() { new BrushTransition() { Property = Border.BackgroundProperty, Duration = new TimeSpan(0, 0, 0, 0, 75) } };
        }

        public void SetHue(double hue)
        {
            double delta = hue - 0.63762580995930973;

            double hue1 = Cycle(0.552631578947368 + delta);
            double hue2 = Cycle(0.660919540229885 + delta);
            double hue3 = Cycle(0.777027027027027 + delta);
            double hue4 = Cycle(0.559925093632959 + delta);

            Colour border1 = Colour.FromHSL(hue1, 0.703703703703703, 0.947058823529412);

            this.FindControl<Border>("Border1").Background = new SolidColorBrush(border1.ToAvalonia());

            Styles styles = this.Styles;

            LinearGradientBrush border2Brush = new LinearGradientBrush() { StartPoint = new RelativePoint(0.731, 0.131, RelativeUnit.Relative), EndPoint = new RelativePoint(0.938, 1.249, RelativeUnit.Relative) };
            border2Brush.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue2, 1, 0.227450980392157).WithAlpha(0).ToAvalonia(), 0));
            border2Brush.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue3, 0.948717948717949, 0.152941176470588).WithAlpha(0.2).ToAvalonia(), 1));

            ((Setter)((Style)this.Styles[6]).Setters[0]).Value = border2Brush;


            LinearGradientBrush border2BrushOver = new LinearGradientBrush() { StartPoint = new RelativePoint(0.731, 0.131, RelativeUnit.Relative), EndPoint = new RelativePoint(0.938, 1.249, RelativeUnit.Relative) };
            border2BrushOver.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue2, 1, 0.227450980392157).WithAlpha(0.0627450980392).ToAvalonia(), 0));
            border2BrushOver.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue3, 0.948717948717949, 0.152941176470588).WithAlpha(0.270588235).ToAvalonia(), 1));

            ((Setter)((Style)this.Styles[2]).Setters[0]).Value = border2BrushOver;

            LinearGradientBrush border2BrushPressed = new LinearGradientBrush() { StartPoint = new RelativePoint(0.731, 0.131, RelativeUnit.Relative), EndPoint = new RelativePoint(0.938, 1.249, RelativeUnit.Relative) };
            border2BrushPressed.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue2, 1, 0.227450980392157).WithAlpha(0.125490196).ToAvalonia(), 0));
            border2BrushPressed.GradientStops.Add(new Avalonia.Media.GradientStop(Colour.FromHSL(hue3, 0.948717948717949, 0.152941176470588).WithAlpha(0.333333333).ToAvalonia(), 1));

            ((Setter)((Style)this.Styles[4]).Setters[0]).Value = border2BrushPressed;

            this.FindControl<Border>("TitleBorder").Background = new SolidColorBrush(Colour.FromHSL(hue4, 1, 0.349019607843137).ToAvalonia());
        }

        private static double Cycle(double value)
        {
            while (value < 0)
            {
                value += 1;
            }
            
            while (value > 1)
            {
                value -= 1;
            }

            return value;
        }
    }
}
