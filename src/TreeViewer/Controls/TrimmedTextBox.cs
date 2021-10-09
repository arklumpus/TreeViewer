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

namespace TreeViewer
{
    public class TrimmedTextBox2 : TrimmedTextBox
    {
        public TrimmedTextBox2() : base()
        {

        }

        protected override Size MeasureOverride(Size availableSize)
        {
            base.MeasureOverride(availableSize);

            Size size = AvaloniaBugFixes.MeasureText(this.Text, this.FontFamily, this.FontStyle, this.FontWeight, this.FontSize);

            return new Size(System.Math.Min(size.Width + 2, availableSize.Width), size.Height);
        }

        public override void Render(DrawingContext context)
        {
            FormattedText txt = new FormattedText(this.Text, new Typeface(this.FontFamily, this.FontStyle, this.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, this.Bounds.Size);

            if (this.Bounds.Width < _size.Width)
            {
                using (context.PushClip(new Rect(0, 0, this.Bounds.Width - _ellipsisSize.Width - 7, this.Bounds.Height)))
                {
                    context.DrawText(this.Foreground, new Point(0, this.Bounds.Height * 0.5 - txt.Bounds.Height * 0.5), txt);
                }

                FormattedText ell = new FormattedText(this.EllipsisText, new Typeface(this.FontFamily, this.FontStyle, this.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, this.Bounds.Size);

                context.DrawText(this.Foreground, new Point(this.Bounds.Width - _ellipsisSize.Width - 2, this.Bounds.Height * 0.5 - ell.Bounds.Height * 0.5), ell);
            }
            else
            {
                context.DrawText(this.Foreground, new Point(0, this.Bounds.Height * 0.5 - txt.Bounds.Height * 0.5), txt);
            }
        }
    }

    public class TrimmedTextBox : UserControl
    {
        public static readonly StyledProperty<string> EllipsisTextProperty = AvaloniaProperty.Register<TrimmedTextBox, string>(nameof(EllipsisText), "...");

        public string EllipsisText
        {
            get { return GetValue(EllipsisTextProperty); }
            set { SetValue(EllipsisTextProperty, value); }
        }

        public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<TrimmedTextBox, string>(nameof(Text), "");

        public string Text
        {
            get { return GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public TrimmedTextBox()
        {
            this.InitializeComponent();
        }
     
        private bool sizeDirty = true;
        protected Size _size = new Size(0, 0);
        protected Size _ellipsisSize = new Size(0, 0);

        protected override Size MeasureOverride(Size availableSize)
        {
            if (sizeDirty)
            {
                _size = AvaloniaBugFixes.MeasureText(this.Text, this.FontFamily, this.FontStyle, this.FontWeight, this.FontSize);
                _ellipsisSize = AvaloniaBugFixes.MeasureText(this.EllipsisText, this.FontFamily, this.FontStyle, this.FontWeight, this.FontSize);
            }

            return new Size(0, _size.Height);
        }

        public override void Render(DrawingContext context)
        {
            if (this.Bounds.Width < _size.Width)
            {
                using (context.PushClip(new Rect(0, 0, this.Bounds.Width - _ellipsisSize.Width - 7, this.Bounds.Height)))
                {
                    context.DrawText(this.Foreground, new Point(0, -3), new FormattedText(this.Text, new Typeface(this.FontFamily, this.FontStyle, this.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, this.Bounds.Size));
                }

                context.DrawText(this.Foreground, new Point(this.Bounds.Width - _ellipsisSize.Width - 2, -3), new FormattedText(this.EllipsisText, new Typeface(this.FontFamily, this.FontStyle, this.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, this.Bounds.Size));
            }
            else
            {
                context.DrawText(this.Foreground, new Point(0, -3), new FormattedText(this.Text, new Typeface(this.FontFamily, this.FontStyle, this.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, this.Bounds.Size));
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TrimmedTextBox.TextProperty || change.Property == TrimmedTextBox.EllipsisTextProperty || change.Property == TrimmedTextBox.FontFamilyProperty || change.Property == TrimmedTextBox.FontStyleProperty || change.Property == TrimmedTextBox.FontWeightProperty || change.Property == TrimmedTextBox.FontSizeProperty)
            {
                sizeDirty = true;
            }
        }

        private void InitializeComponent()
        {
            AffectsRender<TrimmedTextBox>(TextProperty, EllipsisTextProperty);
            AffectsMeasure<TrimmedTextBox>(TextProperty, EllipsisTextProperty);
        }
    }
}
