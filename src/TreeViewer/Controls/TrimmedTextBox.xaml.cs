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

namespace TreeViewer
{
    public class TrimmedTextBox : UserControl
    {
        public TextBlock TextContainer
        {
            get
            {
                return this.FindControl<TextBlock>("TextContainer");
            }
        }

        public TextBlock Ellipsis
        {
            get
            {
                return this.FindControl<TextBlock>("Ellipsis");
            }
        }

        public string Text
        {
            get
            {
                return TextContainer.Text;
            }

            set
            {
                TextContainer.Text = value;
            }
        }

        public TrimmedTextBox()
        {
            this.InitializeComponent();
        }

        public double MaxTextWidth { get; set; }

        public TrimmedTextBox(double maxWidth)
        {
            this.InitializeComponent();
            MaxTextWidth = maxWidth;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            TextContainer.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBlock.TextProperty)
                {
                    //FormattedText fmtText = new FormattedText(this.TextContainer.Text, new Typeface(this.TextContainer.FontFamily, this.TextContainer.FontStyle, this.TextContainer.FontWeight), this.FontSize, TextAlignment.Left, TextWrapping.NoWrap, Avalonia.Size.Infinity);
                    double width = AvaloniaBugFixes.MeasureTextWidth(this.TextContainer.Text, this.TextContainer.FontFamily, this.TextContainer.FontStyle, this.TextContainer.FontWeight, this.TextContainer.FontSize);

                    this.FindControl<TextBlock>("Ellipsis").IsVisible = width > MaxTextWidth;
                }
            };
        }

        
    }
}
