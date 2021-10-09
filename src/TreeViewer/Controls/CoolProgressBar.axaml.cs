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
using Avalonia.Media;
using Avalonia.Media.Transformation;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TreeViewer
{
    public partial class CoolProgressBar : UserControl
    {
        private static TransformOperations offScreenTop;
        private static TransformOperations offScreenLeft;
        private static TransformOperations offScreenRight;

        static CoolProgressBar()
        {
            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(0, -16);
            offScreenTop = builder.Build();

            builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            offScreenLeft = builder.Build();

            builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(16, 0);
            offScreenRight = builder.Build();
        }

        public static readonly StyledProperty<double> ProgressProperty = AvaloniaProperty.Register<CoolProgressBar, double>(nameof(Progress), 0);

        public double Progress
        {
            get => GetValue(ProgressProperty);
            set => SetValue(ProgressProperty, value);
        }

        public static readonly StyledProperty<double> OverlapProperty = AvaloniaProperty.Register<CoolProgressBar, double>(nameof(Overlap), 12);

        public double Overlap
        {
            get => GetValue(OverlapProperty);
            set => SetValue(OverlapProperty, value);
        }

        public static readonly StyledProperty<double> SpacingProperty = AvaloniaProperty.Register<CoolProgressBar, double>(nameof(Spacing), 5);

        public double Spacing
        {
            get => GetValue(SpacingProperty);
            set => SetValue(SpacingProperty, value);
        }

        public static readonly StyledProperty<bool> IsIndeterminateProperty = AvaloniaProperty.Register<CoolProgressBar, bool>(nameof(IsIndeterminate), false);

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public static readonly StyledProperty<IBrush> ProgressBarBackgroundProperty = AvaloniaProperty.Register<CoolProgressBar, IBrush>(nameof(ProgressBarBackground), new SolidColorBrush(Color.FromRgb(195, 195, 195)));

        public IBrush ProgressBarBackground
        {
            get => GetValue(ProgressBarBackgroundProperty);
            set => SetValue(ProgressBarBackgroundProperty, value);
        }

        public static readonly StyledProperty<IBrush> ProgressBarForegroundProperty = AvaloniaProperty.Register<CoolProgressBar, IBrush>(nameof(ProgressBarForeground), new SolidColorBrush(Color.FromRgb(0, 114, 178)));

        public IBrush ProgressBarForeground
        {
            get => GetValue(ProgressBarForegroundProperty);
            set => SetValue(ProgressBarForegroundProperty, value);
        }

        public static readonly StyledProperty<ImmutableList<double>> IntermediateStepsProperty = AvaloniaProperty.Register<CoolProgressBar, ImmutableList<double>>(nameof(IntermediateSteps), ImmutableList<double>.Empty);

        public ImmutableList<double> IntermediateSteps
        {
            get => GetValue(IntermediateStepsProperty);
            set => SetValue(IntermediateStepsProperty, value);
        }

        public static readonly StyledProperty<string> LabelTextProperty = AvaloniaProperty.Register<CoolProgressBar, string>(nameof(LabelText), null);

        public string LabelText
        {
            get => GetValue(LabelTextProperty);
            set => SetValue(LabelTextProperty, value);
        }

        private Control previousProgressBarContainer = null;
        private TextBlock previousLabel = null;

        private void SetProgress()
        {
            double newValue = this.Progress;

            double prevProgress = 0;

            int barIndex = -1;

            for (int i = 0; i < IntermediateSteps.Count; i++)
            {
                if (IntermediateSteps[i] < newValue)
                {
                    prevProgress = IntermediateSteps[i];
                    barIndex = i;
                }
            }

            barIndex++;

            double nextProgress;

            if (barIndex < IntermediateSteps.Count)
            {
                nextProgress = IntermediateSteps[barIndex];
            }
            else
            {
                nextProgress = 1;
            }

            for (int i = 0; i < barIndex; i++)
            {
                progressBars[i].Value = 1;
            }

            progressBars[barIndex].Value = (newValue - prevProgress) / (nextProgress - prevProgress);

            for (int i = barIndex + 1; i < progressBars.Count; i++)
            {
                progressBars[i].Value = 0;
            }

            if (!string.IsNullOrEmpty(LabelText))
            {
                double trX = -this.FindControl<Grid>("LabelLabelContainer").Bounds.Width * 0.5 + this.Bounds.Width * newValue;
                double actualTrX = trX;
                trX = Math.Max(0, Math.Min(trX, this.Bounds.Width - this.FindControl<Grid>("LabelLabelContainer").Bounds.Width));

                this.FindControl<Grid>("LabelLabelContainer").RenderTransform = new TranslateTransform(trX, 0);

                this.FindControl<Canvas>("LabelPath").RenderTransform = new TranslateTransform(Math.Max(5, Math.Min(actualTrX - trX + this.FindControl<Grid>("LabelLabelContainer").Bounds.Width * 0.5 - 9, this.Bounds.Width - 23 - trX)), 0);
            }
        }

        private void SetClipGeometries()
        {
            if (progressBars.Count > 1)
            {
                for (int i = 0; i < progressBars.Count; i++)
                {
                    Avalonia.Controls.ProgressBar bar = progressBars[i];

                    bar.Height = previousProgressBarContainer.Bounds.Height;

                    bar.Width = previousProgressBarContainer.Bounds.Width / progressBars.Count + Overlap - Spacing;

                    if (i == 0)
                    {
                        bar.RenderTransform = new TranslateTransform(0, 0);

                        List<Point> points = new List<Point>(5);
                        points.Add(new Point(0, 0));
                        points.Add(new Point(bar.Bounds.Width - Overlap, 0));
                        points.Add(new Point(bar.Bounds.Width, bar.Bounds.Height * 0.5));
                        points.Add(new Point(bar.Bounds.Width - Overlap, bar.Bounds.Height));
                        points.Add(new Point(0, bar.Bounds.Height));

                        PolylineGeometry geo = new PolylineGeometry(points, true);
                        bar.Clip = geo;
                    }
                    else if (i == progressBars.Count - 1)
                    {
                        bar.RenderTransform = new TranslateTransform(previousProgressBarContainer.Bounds.Width / progressBars.Count * i, 0);

                        List<Point> points = new List<Point>(5);
                        points.Add(new Point(0, 0));
                        points.Add(new Point(bar.Bounds.Width, 0));
                        points.Add(new Point(bar.Bounds.Width, bar.Bounds.Height));
                        points.Add(new Point(0, bar.Bounds.Height));
                        points.Add(new Point(Overlap, bar.Bounds.Height * 0.5));

                        PolylineGeometry geo = new PolylineGeometry(points, true);
                        bar.Clip = geo;
                    }
                    else
                    {
                        bar.RenderTransform = new TranslateTransform(previousProgressBarContainer.Bounds.Width / progressBars.Count * i, 0);

                        List<Point> points = new List<Point>(6);
                        points.Add(new Point(0, 0));
                        points.Add(new Point(bar.Bounds.Width - Overlap, 0));
                        points.Add(new Point(bar.Bounds.Width, bar.Bounds.Height * 0.5));
                        points.Add(new Point(bar.Bounds.Width - Overlap, bar.Bounds.Height));
                        points.Add(new Point(0, bar.Bounds.Height));
                        points.Add(new Point(Overlap, bar.Bounds.Height * 0.5));

                        PolylineGeometry geo = new PolylineGeometry(points, true);
                        bar.Clip = geo;
                    }
                }
            }
            else
            {
                Avalonia.Controls.ProgressBar bar = progressBars[0];

                bar.Height = previousProgressBarContainer.Bounds.Height;
                bar.Width = previousProgressBarContainer.Bounds.Width;
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ProgressProperty)
            {
                SetProgress();
            }
            else if (change.Property == IsIndeterminateProperty)
            {
                for (int i = 0; i < progressBars.Count; i++)
                {
                    progressBars[i].IsIndeterminate = this.IsIndeterminate;
                }
            }
            else if (change.Property == IntermediateStepsProperty)
            {
                this.progressBars.Clear();

                this.previousProgressBarContainer.Opacity = 0;

                Control prevPBContainer = this.previousProgressBarContainer;

                Canvas newBarContainer = new Canvas() { Opacity = 0 };

                this.previousProgressBarContainer = newBarContainer;

                newBarContainer.Transitions = new Avalonia.Animation.Transitions()
                {
                    new Avalonia.Animation.DoubleTransition(){ Property = Avalonia.Controls.ProgressBar.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100)  }
                };

                double progress = 0;

                for (int i = 0; i < IntermediateSteps.Count; i++)
                {
                    Avalonia.Controls.ProgressBar bar = new Avalonia.Controls.ProgressBar() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch, Minimum = 0, Maximum = 1, Background = this.ProgressBarBackground, Foreground = this.ProgressBarForeground, CornerRadius = new CornerRadius(0), IsIndeterminate = this.IsIndeterminate, MinWidth = 0 };
                    this.progressBars.Add(bar);
                    newBarContainer.Children.Add(bar);

                    progress = IntermediateSteps[i];
                }

                {
                    Avalonia.Controls.ProgressBar bar = new Avalonia.Controls.ProgressBar() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch, Minimum = 0, Maximum = 1, Background = this.ProgressBarBackground, Foreground = this.ProgressBarForeground, CornerRadius = new CornerRadius(0), IsIndeterminate = this.IsIndeterminate, MinWidth = 0 };
                    this.progressBars.Add(bar);
                    newBarContainer.Children.Add(bar);
                }

                newBarContainer.PropertyChanged += (s, e) =>
                {
                    if (e.Property == Grid.BoundsProperty)
                    {
                        SetClipGeometries();
                    }
                };

                this.FindControl<Grid>("ProgressBarContainer").Children.Add(newBarContainer);

                newBarContainer.Opacity = 1;

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    SetClipGeometries();
                    await System.Threading.Tasks.Task.Delay(200);
                    this.FindControl<Grid>("ProgressBarContainer").Children.Remove(prevPBContainer);
                }, Avalonia.Threading.DispatcherPriority.MinValue);

                SetProgress();
            }
            else if (change.Property == LabelTextProperty)
            {
                string newValue = change.NewValue.GetValueOrDefault<string>();

                if (!string.IsNullOrEmpty(newValue))
                {
                    this.FindControl<Grid>("LabelLabelContainer").Opacity = 1;
                    this.FindControl<Grid>("LabelLabelContainer").Height = 38;

                    TextBlock prevLbl = previousLabel;

                    if (prevLbl != null)
                    {
                        prevLbl.Opacity = 0;
                        prevLbl.RenderTransform = offScreenLeft;
                    }

                    previousLabel = new TextBlock() { Text = newValue, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, RenderTransform = offScreenRight, Opacity = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                    previousLabel.Transitions = new Avalonia.Animation.Transitions()
                    {
                        new Avalonia.Animation.DoubleTransition(){ Property = TextBlock.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                        new Avalonia.Animation.TransformOperationsTransition(){ Property = TextBlock.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) },
                    };

                    this.FindControl<Grid>("LabelContainer").Children.Add(previousLabel);
                    this.FindControl<Grid>("LabelLabelContainer").IsVisible = true;
                    previousLabel.Opacity = 1;
                    previousLabel.RenderTransform = TransformOperations.Identity;


                    FormattedText frm = new FormattedText(newValue, new Typeface(this.FontFamily), 13, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);

                    this.FindControl<Grid>("LabelLabelContainer").Width = frm.Bounds.Width + 40;

                    if (prevLbl != null)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await System.Threading.Tasks.Task.Delay(200);
                            this.FindControl<Grid>("LabelContainer").Children.Remove(prevLbl);
                        }, Avalonia.Threading.DispatcherPriority.MinValue);
                    }
                }
                else
                {
                    this.FindControl<Grid>("LabelLabelContainer").Opacity = 0;
                    this.FindControl<Grid>("LabelLabelContainer").Height = 0;

                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(200);
                        this.FindControl<Grid>("LabelLabelContainer").IsVisible = false;
                    }, Avalonia.Threading.DispatcherPriority.MinValue);

                }
            }
        }

        private List<Avalonia.Controls.ProgressBar> progressBars = new List<Avalonia.Controls.ProgressBar>();

        public CoolProgressBar()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Grid previousProgressBarContainer = new Grid();
            previousProgressBarContainer.Transitions = new Avalonia.Animation.Transitions()
            {
                new Avalonia.Animation.DoubleTransition(){ Property = Avalonia.Controls.ProgressBar.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100)  }
            };

            Avalonia.Controls.ProgressBar baseBar = new Avalonia.Controls.ProgressBar() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch, Minimum = 0, Maximum = 1, Background = this.ProgressBarBackground, Foreground = this.ProgressBarForeground, CornerRadius = new CornerRadius(0), MinWidth = 0 };
            this.progressBars.Add(baseBar);
            previousProgressBarContainer.Children.Add(baseBar);
            this.previousProgressBarContainer = previousProgressBarContainer;

            this.FindControl<Grid>("ProgressBarContainer").Children.Add(previousProgressBarContainer);
            this.FindControl<Grid>("LabelLabelContainer").PropertyChanged += (s, e) =>
            {
                if (e.Property == Grid.BoundsProperty && ((Rect)e.OldValue).Width != ((Rect)e.NewValue).Width)
                {
                    SetProgress();
                }
            };

            this.AttachedToVisualTree += (s, e) =>
            {
                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    SetProgress();
                }, Avalonia.Threading.DispatcherPriority.MinValue);
            };
        }
    }
}
