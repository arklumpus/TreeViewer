using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using AvaloniaAccordion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer
{
    public partial class MainWindow
    {
        private void StartDrag(Accordion exp, StackPanel parent, ref Func<PointerReleasedEventArgs, (int, int)> onPointerReleased, PointerEventArgs currArgs, double moduleHeight = 34)
        {
            parent.Cursor = new Cursor(StandardCursorType.SizeAll);

            Canvas placeHolder = new Canvas() { Width = exp.Bounds.Width, Height = moduleHeight, IsHitTestVisible = false, Background = new SolidColorBrush(Color.FromArgb(102, 0, 114, 178)), ClipToBounds = false, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, RenderTransform = Avalonia.Media.Transformation.TransformOperations.Identity };

            Grid placeHolderContent = new Grid() { Width = exp.Bounds.Width, Height = moduleHeight };
            placeHolderContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            placeHolderContent.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            Viewbox arrowBox = new Viewbox() { Width = 18, Margin = new Thickness(5, 3, 5, 3) };

            Canvas arrowCanvas = new Canvas() { Width = 16, Height = 16 };
            arrowCanvas.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Geometry.Parse("M2,6 L8,12 L14,6"), StrokeThickness = 2, Stroke = new SolidColorBrush(Color.FromRgb(48, 48, 48)), StrokeLineCap = PenLineCap.Round, StrokeJoin = PenLineJoin.Round, Width = 16, Height = 16 });
            arrowBox.Child = arrowCanvas;

            placeHolderContent.Children.Add(arrowBox);

            Grid header = (Grid)exp.AccordionHeader;
            exp.AccordionHeader = null;
            Grid.SetColumn(header, 1);
            placeHolderContent.Children.Add(header);

            Canvas upArrowCanvas = new Canvas() { Width = 16, Height = 16, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, RenderTransform = new TranslateTransform(0, -6) };
            upArrowCanvas.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Geometry.Parse("M2,10 L8,4 L14,10"), StrokeThickness = 3, Stroke = new SolidColorBrush(Color.FromRgb(0, 114, 178)), StrokeLineCap = PenLineCap.Round, StrokeJoin = PenLineJoin.Round, Width = 16, Height = 16 });
            Grid.SetColumnSpan(upArrowCanvas, 2);
            placeHolderContent.Children.Add(upArrowCanvas);

            Canvas downArrowCanvas = new Canvas() { Width = 16, Height = 16, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom, RenderTransform = new TranslateTransform(0, 6) };
            downArrowCanvas.Children.Add(new Avalonia.Controls.Shapes.Path() { Data = Geometry.Parse("M2,6 L8,12 L14,6"), StrokeThickness = 3, Stroke = new SolidColorBrush(Color.FromRgb(0, 114, 178)), StrokeLineCap = PenLineCap.Round, StrokeJoin = PenLineJoin.Round, Width = 16, Height = 16 });
            Grid.SetColumnSpan(downArrowCanvas, 2);
            placeHolderContent.Children.Add(downArrowCanvas);


            placeHolder.Children.Add(placeHolderContent);

            bool transitionsSet = false;

            Grid.SetRowSpan(placeHolder, 100);
            Grid.SetColumnSpan(placeHolder, 100);

            exp.IsVisible = false;

            double lastTranslationY = double.NaN;

            void parentMouseMoved(object sender, PointerEventArgs e)
            {
                int index = -1;
                int lastIndex = -1;
                int firstIndex = -1;

                bool below = false;

                bool belowAll = false;

                bool found = false;

                for (int i = 0; i < parent.Children.Count; i++)
                {
                    if (parent.Children[i] is Accordion acc && acc != exp)
                    {
                        if (firstIndex < 0)
                        {
                            firstIndex = i;
                        }

                        Point pos = e.GetCurrentPoint(acc).Position;

                        if (pos.Y >= -acc.Margin.Top && pos.Y <= acc.Bounds.Height + acc.Margin.Bottom)
                        {
                            if (!found)
                            {
                                found = true;
                                index = i;

                                if (pos.Y > acc.Bounds.Height * 0.5)
                                {
                                    below = true;
                                }
                            }
                        }

                        if (pos.Y > acc.Bounds.Height)
                        {
                            belowAll = true;
                        }

                        lastIndex = i;
                    }
                }

                if (index >= 0 && index == lastIndex && below)
                {
                    index = -1;
                    belowAll = true;
                }

                Func<Point?> getTargetPosition = null;

                if (index >= 0)
                {
                    for (int i = 0; i < parent.Children.Count; i++)
                    {
                        if (parent.Children[i] is Accordion acc && acc != exp)
                        {
                            if (i == index)
                            {
                                if (below)
                                {
                                    acc.Margin = new Thickness(5, 0, 0, 5 + moduleHeight + 5);

                                    getTargetPosition = () => acc.TranslatePoint(new Point(0, acc.Bounds.Height + 5), this.FindControl<Grid>("RootGrid"));
                                }
                                else
                                {
                                    acc.Margin = new Thickness(5, moduleHeight + 5, 0, 5);

                                    getTargetPosition = () => acc.TranslatePoint(new Point(0, -5 - placeHolder.Height), this.FindControl<Grid>("RootGrid"));
                                }
                            }
                            else
                            {
                                acc.Margin = new Thickness(5, 0, 0, 5);
                            }
                        }
                    }
                }
                else if (belowAll)
                {
                    for (int i = 0; i < parent.Children.Count; i++)
                    {
                        if (parent.Children[i] is Accordion acc)
                        {
                            if (i == lastIndex)
                            {
                                acc.Margin = new Thickness(5, 0, 0, 5 + moduleHeight + 5);

                                getTargetPosition = () => acc.TranslatePoint(new Point(0, acc.Bounds.Height + 5), this.FindControl<Grid>("RootGrid"));
                            }
                            else
                            {
                                acc.Margin = new Thickness(5, 0, 0, 5);
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < parent.Children.Count; i++)
                    {
                        if (parent.Children[i] is Accordion acc)
                        {
                            if (i == firstIndex)
                            {
                                acc.Margin = new Thickness(5, moduleHeight + 5, 0, 5);

                                getTargetPosition = () => acc.TranslatePoint(new Point(0, -5 - placeHolder.Height), this.FindControl<Grid>("RootGrid"));
                            }
                            else
                            {
                                acc.Margin = new Thickness(5, 0, 0, 5);
                            }
                        }
                    }
                }

                if (getTargetPosition != null)
                {
                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Point absolutePosition = getTargetPosition().Value;

                        if (absolutePosition.Y != lastTranslationY)
                        {
                            lastTranslationY = absolutePosition.Y;

                            Avalonia.Media.Transformation.TransformOperations.Builder builder = Avalonia.Media.Transformation.TransformOperations.CreateBuilder(1);

                            builder.AppendTranslate(absolutePosition.X, absolutePosition.Y);

                            placeHolder.RenderTransform = builder.Build();
                        }

                        if (!transitionsSet)
                        {
                            transitionsSet = true;
                            placeHolder.Transitions = new Avalonia.Animation.Transitions() { new Avalonia.Animation.TransformOperationsTransition() { Property = Canvas.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(50) } };
                            this.FindControl<Grid>("RootGrid").Children.Add(placeHolder);
                        }
                    }, Avalonia.Threading.DispatcherPriority.MinValue);
                }
            }

            parent.PointerMoved += parentMouseMoved;

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => parentMouseMoved(parent, currArgs), Avalonia.Threading.DispatcherPriority.MinValue);

            onPointerReleased = (e) =>
            {
                parent.PointerMoved -= parentMouseMoved;
                parent.Cursor = new Cursor(StandardCursorType.Arrow);

                int index = -1;
                int lastIndex = -1;
                int firstIndex = -1;

                bool below = false;

                bool belowAll = false;

                for (int i = 0; i < parent.Children.Count; i++)
                {
                    if (parent.Children[i] is Accordion acc && acc != exp)
                    {
                        if (firstIndex < 0)
                        {
                            firstIndex = i;
                        }

                        Point pos = e.GetCurrentPoint(acc).Position;

                        if (pos.Y >= -acc.Margin.Top && pos.Y <= acc.Bounds.Height + acc.Margin.Bottom)
                        {
                            index = i;

                            if (pos.Y > acc.Bounds.Height * 0.5)
                            {
                                below = true;
                            }
                        }

                        if (pos.Y > acc.Bounds.Height)
                        {
                            belowAll = true;
                        }

                        lastIndex = i;
                    }
                }

                if (index >= 0 && index == lastIndex && below)
                {
                    index = -1;
                    belowAll = true;
                }

                for (int i = 0; i < parent.Children.Count; i++)
                {
                    if (parent.Children[i] is Accordion acc && acc != exp)
                    {
                        acc.Margin = new Thickness(5, 0, 0, 5);
                    }
                }


                int previousIndex = parent.Children.IndexOf(exp);

                this.FindControl<Grid>("RootGrid").Children.Remove(placeHolder);

                IControl reference = null;

                if (index >= 0)
                {
                    reference = parent.Children[index];
                }
                else if (belowAll)
                {
                    reference = parent.Children[lastIndex];
                }
                else
                {
                    reference = parent.Children[firstIndex];
                }

                parent.Children.RemoveAt(previousIndex);

                if (index >= 0 && !below)
                {
                    parent.Children.Insert(parent.Children.IndexOf(reference), exp);
                }
                else if (index >= 0 && below)
                {
                    parent.Children.Insert(parent.Children.IndexOf(reference) + 1, exp);
                }
                else if (belowAll)
                {
                    parent.Children.Insert(parent.Children.IndexOf(reference) + 1, exp);
                }
                else
                {
                    parent.Children.Insert(parent.Children.IndexOf(reference), exp);
                }

                int newIndex = parent.Children.IndexOf(exp);

                placeHolderContent.Children.Clear();

                exp.AccordionHeader = header;

                exp.IsVisible = true;

                return (previousIndex, newIndex);

                //System.Diagnostics.Debug.WriteLine(previousIndex + " --> " + newIndex);
            };
        }
    }
}
