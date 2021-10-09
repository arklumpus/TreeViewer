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

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Threading;
using System;
using Avalonia.Media.Transformation;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace TreeViewer
{
    public class ProgressWindow : ChildWindow
    {
        private static TransformOperations offScreenTop;
        private static TransformOperations offScreenLeft;
        private static TransformOperations offScreenRight;

        static ProgressWindow()
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


        private double _progress;

        public double Progress
        {
            get
            {
                return _progress;
            }

            set
            {
                _progress = value;
                this.FindControl<CoolProgressBar>("ProgressBar").Progress = _progress;
                this.FindControl<TextBlock>("ProgressDesc").Text = _progress.ToString("0%");
            }
        }

        private string progressText = null;

        private TextBlock previousProgressBlock = null;

        public string ProgressText
        {
            get
            {
                return progressText;
            }

            set
            {
                if (previousProgressBlock == null)
                {
                    previousProgressBlock = new TextBlock() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Text = value, Opacity = 1, RenderTransform = TransformOperations.Identity };

                    previousProgressBlock.Transitions = new Avalonia.Animation.Transitions()
                    {
                        new Avalonia.Animation.DoubleTransition(){ Property = TextBlock.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                        new Avalonia.Animation.TransformOperationsTransition() { Property = TextBlock.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
                    };

                    this.FindControl<Grid>("ProgressTextContainer").Children.Add(previousProgressBlock);
                }
                else
                {
                    previousProgressBlock.Opacity = 0;
                    previousProgressBlock.RenderTransform = offScreenTop;

                    TextBlock prevBlk = previousProgressBlock;

                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await System.Threading.Tasks.Task.Delay(200);
                        this.FindControl<Grid>("ProgressTextContainer").Children.Remove(prevBlk);
                    });

                    previousProgressBlock = new TextBlock() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Text = value, Opacity = 0, RenderTransform = offScreenTop };

                    previousProgressBlock.Transitions = new Avalonia.Animation.Transitions()
                    {
                        new Avalonia.Animation.DoubleTransition(){ Property = TextBlock.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                        new Avalonia.Animation.TransformOperationsTransition() { Property = TextBlock.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
                    };

                    this.FindControl<Grid>("ProgressTextContainer").Children.Add(previousProgressBlock);

                    previousProgressBlock.Opacity = 1;
                    previousProgressBlock.RenderTransform = TransformOperations.Identity;
                }

                progressText = value;
            }
        }

        public string LabelText
        {
            get
            {
                return this.FindControl<CoolProgressBar>("ProgressBar").LabelText;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    this.FindControl<CoolProgressBar>("ProgressBar").Height = 24;
                    this.FindControl<TextBlock>("ProgressDesc").Margin = new Avalonia.Thickness(0, 0, 10, 10);
                }
                else
                {
                    this.FindControl<CoolProgressBar>("ProgressBar").Height = 70;
                    this.FindControl<TextBlock>("ProgressDesc").Margin = new Avalonia.Thickness(0, 33, 10, 10);
                }

                this.FindControl<CoolProgressBar>("ProgressBar").LabelText = value;
            }
        }

        private int steps = 1;

        public int Steps
        {
            get { return steps; }
            set
            {
                if (value <= 23)
                {
                    List<double> steps = new List<double>();

                    for (int i = 0; i < value - 1; i++)
                    {
                        steps.Add((double)(i + 1) / value);
                    }

                    this.FindControl<CoolProgressBar>("ProgressBar").IntermediateSteps = ImmutableList.Create(steps.ToArray());
                }
                else
                {
                    this.FindControl<CoolProgressBar>("ProgressBar").IntermediateSteps = ImmutableList<double>.Empty;
                }

                this.steps = value;
            }
        }

        public bool IsIndeterminate
        {
            set
            {
                if (value)
                {
                    this.FindControl<CoolProgressBar>("ProgressBar").IsIndeterminate = true;
                    this.FindControl<Grid>("MainGrid").ColumnDefinitions[1] = new ColumnDefinition(0, GridUnitType.Pixel);
                }
                else
                {
                    this.FindControl<CoolProgressBar>("ProgressBar").IsIndeterminate = false;
                    this.FindControl<Grid>("MainGrid").ColumnDefinitions[1] = new ColumnDefinition(50, GridUnitType.Pixel);
                }
            }
        }

        public ProgressWindow()
        {
            this.InitializeComponent();
        }

        EventWaitHandle Handle;

        public ProgressWindow(EventWaitHandle handle)
        {
            this.InitializeComponent();

            Handle = handle;
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            if (Handle != null)
            {
                Handle.Set();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
