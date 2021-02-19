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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class FontButton : UserControl
    {
        private Font font;

        public Font Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
                Font tempFont = new Font(font.FontFamily, this.FontSize);

                string example = "AaBbCc";

                if (font.FontFamily.FileName == "Symbol")
                {
                    example = "ΑαΒβΓγ";
                }
                else if (font.FontFamily.FileName == "ZapfDingbats")
                {
                    example = "✺❁❐❆✤❉";
                }

                VectSharp.Size s = tempFont.MeasureText(example);
                Page pg = new Page(s.Width + 10, s.Height + 10);

                Avalonia.Media.Immutable.ImmutableSolidColorBrush immutableBrush = ((Avalonia.Media.Immutable.ImmutableSolidColorBrush)this.Foreground.ToImmutable());

                Colour foreground = Colour.FromRgb(immutableBrush.Color.R, immutableBrush.Color.G, immutableBrush.Color.B);

                pg.Graphics.FillText(5, 5, example, tempFont, foreground);

                Canvas can = pg.PaintToCanvas();

                this.FindControl<Button>("MainButton").Content = can;

                FontChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler FontChanged;

        public FontButton()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void ButtonClicked(object sender, RoutedEventArgs e)
        {
            FontChoiceWindow win = new FontChoiceWindow(this.Font);

            IControl windowParent = this.Parent;

            while (!(windowParent is Window))
            {
                windowParent = windowParent.Parent;
            }

            await win.ShowDialog2((Window)windowParent);

            if (win.Result)
            {
                this.Font = win.Font;
            }
        }
    }
}
