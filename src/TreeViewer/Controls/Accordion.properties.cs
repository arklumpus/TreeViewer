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

using Avalonia.Data.Converters;
using System;
using System.Globalization;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Transformation;

namespace TreeViewer
{
    public partial class Accordion
    {
        /// <summary>
        /// Defines the <see cref="IsOpen"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Accordion, bool>(nameof(IsOpen), false);

        /// <summary>
        /// Determines whether the <see cref="Accordion"/> is open or closed.
        /// </summary>
        public bool IsOpen
        {
            get => GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="TransitionDuration"/> property.
        /// </summary>
        public static readonly StyledProperty<TimeSpan> TransitionDurationProperty = AvaloniaProperty.Register<Accordion, TimeSpan>(nameof(TransitionDuration), TimeSpan.FromMilliseconds(100));

        /// <summary>
        /// Determines the length of the animation when the <see cref="Accordion"/> is opened or closed.
        /// </summary>
        public TimeSpan TransitionDuration
        {
            get => GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="AccordionContent"/> property.
        /// </summary>
        public static readonly StyledProperty<Control> AccordionContentProperty = AvaloniaProperty.Register<Accordion, Control>(nameof(AccordionContent), null);

        /// <summary>
        /// Determines the content of the <see cref="Accordion"/>.
        /// </summary>
        public Control AccordionContent
        {
            get => GetValue(AccordionContentProperty);
            set => SetValue(AccordionContentProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="AccordionHeader"/> property.
        /// </summary>
        public static readonly StyledProperty<object> AccordionHeaderProperty = AvaloniaProperty.Register<Accordion, object>(nameof(AccordionHeader), null);

        /// <summary>
        /// Determines the header of the <see cref="Accordion"/>.
        /// </summary>
        public object AccordionHeader
        {
            get => GetValue(AccordionHeaderProperty);
            set => SetValue(AccordionHeaderProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="HeaderBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderBackgroundProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderBackground), new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/>.
        /// </summary>
        public Brush HeaderBackground
        {
            get => GetValue(HeaderBackgroundProperty);
            set => SetValue(HeaderBackgroundProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="HeaderHoverBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderHoverBackgroundProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderHoverBackground), new SolidColorBrush(Color.FromRgb(197, 197, 197)));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/> when the mouse is over it.
        /// </summary>
        public Brush HeaderHoverBackground
        {
            get => GetValue(HeaderHoverBackgroundProperty);
            set => SetValue(HeaderHoverBackgroundProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="HeaderPressedBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderPressedBackgroundProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderPressedBackground), new SolidColorBrush(Color.FromRgb(150, 150, 150)));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/> when the mouse is over it.
        /// </summary>
        public Brush HeaderPressedBackground
        {
            get => GetValue(HeaderPressedBackgroundProperty);
            set => SetValue(HeaderPressedBackgroundProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="BottomBarBrush"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> BottomBarBrushProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(BottomBarBrush), new SolidColorBrush(Color.FromRgb(0, 114, 178)));

        /// <summary>
        /// Determines the colour of the bar shown at the bottom of the <see cref="Accordion"/> when it is open.
        /// </summary>
        public Brush BottomBarBrush
        {
            get => GetValue(BottomBarBrushProperty);
            set => SetValue(BottomBarBrushProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="ContentBackground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> ContentBackgroundProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(ContentBackground), new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)));

        /// <summary>
        /// Determines the background colour of the content of the <see cref="Accordion"/>.
        /// </summary>
        public Brush ContentBackground
        {
            get => GetValue(ContentBackgroundProperty);
            set => SetValue(ContentBackgroundProperty, value);
        }

       /* public static readonly StyledProperty<IBrush> ForegroundProperty = TextBlock.ForegroundProperty.AddOwner<Accordion>();

        public IBrush Foreground
        {
            get { return GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }*/



        /// <summary>
        /// Defines the <see cref="HeaderForeground"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderForegroundProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderForeground), new SolidColorBrush(Colors.Black));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/>.
        /// </summary>
        public Brush HeaderForeground
        {
            get => GetValue(HeaderForegroundProperty);
            set => SetValue(HeaderForegroundProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="HeaderForegroundOpen"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderForegroundOpenProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderForegroundOpen), new SolidColorBrush(Color.FromRgb(0, 114, 178)));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/> when the accordion is open.
        /// </summary>
        public Brush HeaderForegroundOpen
        {
            get => GetValue(HeaderForegroundOpenProperty);
            set => SetValue(HeaderForegroundOpenProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="HeaderForegroundClosed"/> property.
        /// </summary>
        public static readonly StyledProperty<Brush> HeaderForegroundClosedProperty = AvaloniaProperty.Register<Accordion, Brush>(nameof(HeaderForegroundClosed), new SolidColorBrush(Colors.Black));

        /// <summary>
        /// Determines the background colour of the header of the <see cref="Accordion"/> when the accordion is open.
        /// </summary>
        public Brush HeaderForegroundClosed
        {
            get => GetValue(HeaderForegroundClosedProperty);
            set => SetValue(HeaderForegroundClosedProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="BottomBarThickness"/> property.
        /// </summary>
        public static readonly StyledProperty<double> BottomBarThicknessProperty = AvaloniaProperty.Register<Accordion, double>(nameof(BottomBarThickness), 4);

        /// <summary>
        /// Determines the thickness of the bar shown at the bottom of the <see cref="Accordion"/> when it is open.
        /// </summary>
        public double BottomBarThickness
        {
            get => GetValue(BottomBarThicknessProperty);
            set => SetValue(BottomBarThicknessProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="ArrowSize"/> property.
        /// </summary>
        public static readonly StyledProperty<double> ArrowSizeProperty = AvaloniaProperty.Register<Accordion, double>(nameof(ArrowSize), 18);

        /// <summary>
        /// Determines the size of the arrow showing the status of the <see cref="Accordion"/>.
        /// </summary>
        public double ArrowSize
        {
            get => GetValue(ArrowSizeProperty);
            set => SetValue(ArrowSizeProperty, value);
        }

        /// <summary>
        /// The possible positions of the arrows in the accordion header.
        /// </summary>
        public enum ArrowPositions
        {
            /// <summary>
            /// No arrows are shown.
            /// </summary>
            Neither = 0b00,

            /// <summary>
            /// An arrow is shown on the left.
            /// </summary>
            Left = 0b01,

            /// <summary>
            /// An arrow is shown on the right.
            /// </summary>
            Right = 0b10,

            /// <summary>
            /// Arrows are shown both on the left and on the right of the accordion header.
            /// </summary>
            Both = 0b11
        }

        /// <summary>
        /// Defines the <see cref="ArrowPosition"/> property.
        /// </summary>
        public static readonly StyledProperty<ArrowPositions> ArrowPositionProperty = AvaloniaProperty.Register<Accordion, ArrowPositions>(nameof(ArrowPosition), ArrowPositions.Left);

        /// <summary>
        /// Determines the position of the arrow showing the status of the <see cref="Accordion"/>.
        /// </summary>
        public ArrowPositions ArrowPosition
        {
            get => GetValue(ArrowPositionProperty);
            set => SetValue(ArrowPositionProperty, value);
        }
    }

    /// <summary>
    /// Converts <see cref="Accordion.ArrowPositions"/> into <see cref="bool"/>s.
    /// </summary>
    public class AccordionArrowPositionConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Accordion.ArrowPositions pos && parameter is string s && int.TryParse(s, out int i))
            {
                return (((int)pos) & i) != 0;
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b && b)
            {
                return Accordion.ArrowPositions.Both;
            }
            else
            {
                return Accordion.ArrowPositions.Neither;
            }
        }
    }

    /// <summary>
    /// Converts the accordion status into an angle for the accordion arrows.
    /// </summary>
    public class AccordionAngleConverter
    {
        internal bool ReturnLastValue { private get; set; } = false;

        private TransformOperations LastValue = TransformOperations.Identity;
        private static TransformOperations Rotate180Transform { get; }

        static AccordionAngleConverter()
        {
            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendRotate(Math.PI);
            Rotate180Transform = builder.Build();
        }

        /// <inheritdoc/>
        public TransformOperations Convert(bool value)
        {
            if (ReturnLastValue)
            {
                return LastValue;
            }

            if (value)
            {
                LastValue = Rotate180Transform;
                return Rotate180Transform;
            }
            else
            {
                LastValue = TransformOperations.Identity;
                return TransformOperations.Identity;
            }
        }
    }


    /// <summary>
    /// Converts the accordion status into the height of the container.
    /// </summary>
    public class AccordionHeightConverter
    {
        internal Border ContentGrid { private get; set; } = null;
        internal Accordion Parent { private get; set; } = null;

        internal bool ReturnLastValue { private get; set; } = false;

        private double LastValue = 0;

        /// <inheritdoc/>
        public double Convert(bool value)
        {
            if (ReturnLastValue)
            {
                return LastValue;
            }

            if (value)
            {
                if (ContentGrid != null && Parent != null)
                {
                    ContentGrid.Measure(new Avalonia.Size(Parent.Bounds.Width - 20, double.PositiveInfinity));

                    LastValue = ContentGrid.DesiredSize.Height;
                    return LastValue;
                }
                else
                {
                    LastValue = 0;
                    return 0;
                }
            }
            else
            {
                LastValue = 0;
                return 0;
            }
        }
    }
}
