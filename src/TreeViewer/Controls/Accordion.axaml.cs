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
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;

namespace TreeViewer
{
    public partial class Accordion : UserControl
    {
        /// <summary>
        /// If the accordion is open, forces a recomputation of the height to make sure that all the contents fit.
        /// </summary>
        public void InvalidateHeight()
        {
            if (this.IsOpen)
            {
                this.ContentGridParent.Height = this.AccordionHeightConverter.Convert(true);

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Accordion parentAccordion = this.FindAncestorOfType<Accordion>();

                    if (parentAccordion != null)
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Math.Max(10, this.TransitionDuration.TotalMilliseconds * 2)));
                        parentAccordion.InvalidateHeight();
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Math.Max(10, this.TransitionDuration.TotalMilliseconds)));
                        parentAccordion.InvalidateHeight();
                    }

                }, Avalonia.Threading.DispatcherPriority.MinValue);
            }
        }


        private Grid ContentGridParent;
        private AccordionHeightConverter AccordionHeightConverter;
        private AccordionAngleConverter AccordionAngleConverter;

        /// <inheritdoc/>
        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == Accordion.TransitionDurationProperty)
            {
                if (ContentGridParent != null)
                {
                    ((Avalonia.Animation.DoubleTransition)ContentGridParent.Transitions[0]).Duration = change.NewValue.GetValueOrDefault<TimeSpan>();

                    ((Avalonia.Animation.TransformOperationsTransition)(this.FindControl<Path>("ArrowPathLeft").Transitions[0])).Duration = change.NewValue.GetValueOrDefault<TimeSpan>();
                    ((Avalonia.Animation.TransformOperationsTransition)(this.FindControl<Path>("ArrowPathLeft").Transitions[0])).Duration = change.NewValue.GetValueOrDefault<TimeSpan>();
                }
            }
            else if (change.Property == Accordion.AccordionContentProperty)
            {
                this.FindControl<Border>("ContentGrid").Child = change.NewValue.GetValueOrDefault<Control>();
                this.InvalidateHeight();
            }
            else if (change.Property == Accordion.IsOpenProperty)
            {
                bool newValue = change.NewValue.GetValueOrDefault<bool>();

                this.ContentGridParent.Height = this.AccordionHeightConverter.Convert(newValue);

                if (newValue)
                {
                    this.ContentGridParent.IsVisible = true;
                    this.HeaderForeground = this.HeaderForegroundOpen;
                }
                else
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(100);
                        this.ContentGridParent.IsVisible = false;
                    });
                    this.HeaderForeground = this.HeaderForegroundClosed;
                }

                this.FindControl<Path>("ArrowPathLeft").RenderTransform = this.AccordionAngleConverter.Convert(newValue);

                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    Accordion parentAccordion = this.FindAncestorOfType<Accordion>();

                    if (parentAccordion != null)
                    {
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Math.Max(10, this.TransitionDuration.TotalMilliseconds * 2)));
                        parentAccordion.InvalidateHeight();
                        await System.Threading.Tasks.Task.Delay(TimeSpan.FromMilliseconds(Math.Max(10, this.TransitionDuration.TotalMilliseconds)));
                        parentAccordion.InvalidateHeight();
                    }
                });
            }
            else if (change.Property == Accordion.ArrowSizeProperty)
            {
                this.FindControl<Viewbox>("LeftArrowViewBox").Width = change.NewValue.GetValueOrDefault<double>();
                this.FindControl<Viewbox>("RightArrowViewBox").Width = change.NewValue.GetValueOrDefault<double>();
            }
            else if (change.Property == Accordion.ArrowPositionProperty)
            {
                this.FindControl<Viewbox>("LeftArrowViewBox").IsVisible = (change.NewValue.GetValueOrDefault<ArrowPositions>() & ArrowPositions.Left) != 0;
                this.FindControl<Viewbox>("RightArrowViewBox").IsVisible = (change.NewValue.GetValueOrDefault<ArrowPositions>() & ArrowPositions.Right) != 0;
            }
            else if (change.Property == Accordion.HeaderForegroundProperty)
            {
                this.FindControl<Path>("ArrowPathLeft").Stroke = change.NewValue.GetValueOrDefault<Brush>();
                this.FindControl<Path>("ArrowPathRight").Stroke = change.NewValue.GetValueOrDefault<Brush>();
                this.FindControl<ContentControl>("HeaderPresenter").Foreground = change.NewValue.GetValueOrDefault<Brush>();
            }
            else if (change.Property == Accordion.AccordionHeaderProperty)
            {
                this.FindControl<ContentControl>("HeaderPresenter").Content = change.NewValue.GetValueOrDefault<object>();
            }
            else if (change.Property == Accordion.ContentBackgroundProperty)
            {
                this.ContentGridParent.Background = change.NewValue.GetValueOrDefault<Brush>();
            }
        }

        /// <summary>
        /// Creates a new <see cref="Accordion"/> instance.
        /// </summary>
        public Accordion()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.AccordionHeightConverter = new AccordionHeightConverter();

            AccordionHeightConverter.ContentGrid = this.FindControl<Border>("ContentGrid");
            AccordionHeightConverter.Parent = this;

            this.AccordionAngleConverter = new AccordionAngleConverter();

            this.ContentGridParent = this.FindControl<Grid>("ContentGridParent");

            ((Avalonia.Animation.DoubleTransition)ContentGridParent.Transitions[0]).Duration = this.TransitionDuration;
            ((Avalonia.Controls.PanAndZoom.TransformOperationsTransition)(this.FindControl<Path>("ArrowPathLeft").Transitions[0])).Duration = this.TransitionDuration;
            ((Avalonia.Controls.PanAndZoom.TransformOperationsTransition)(this.FindControl<Path>("ArrowPathLeft").Transitions[0])).Duration = this.TransitionDuration;

            this.FindControl<Grid>("HeaderGrid").PointerPressed += (s, e) =>
            {
                this.FindControl<Grid>("HeaderGrid").Classes.Add("Pressed");
            };

            this.FindControl<Grid>("HeaderGrid").PointerCaptureLost += (s, e) =>
            {
                this.FindControl<Grid>("HeaderGrid").Classes.Remove("Pressed");
            };

            this.FindControl<Grid>("HeaderGrid").PointerReleased += (s, e) =>
            {
                this.FindControl<Grid>("HeaderGrid").Classes.Remove("Pressed");

                this.IsOpen = !this.IsOpen;
            };
        }

    }
}
