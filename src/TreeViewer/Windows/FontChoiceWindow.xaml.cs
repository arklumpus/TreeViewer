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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using VectSharp;
using VectSharp.Canvas;
using System.Linq;

namespace TreeViewer
{
    public class FontChoiceWindow : Window
    {
        public bool Result { get; private set; } = false;
        public Font Font { get; private set; }

        public FontChoiceWindow()
        {
            this.InitializeComponent();
        }

        public FontChoiceWindow(Font fnt)
        {
            this.InitializeComponent();

            if (!fnt.FontFamily.IsStandardFamily)
            {
                this.FindControl<RadioButton>("HelveticaButton").IsChecked = true;
            }
            else
            {
                if (fnt.FontFamily.FileName == "Times-Roman")
                {
                    this.FindControl<RadioButton>("TimesButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Times-Bold")
                {
                    this.FindControl<RadioButton>("TimesBButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Times-Italic")
                {
                    this.FindControl<RadioButton>("TimesIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Times-BoldItalic")
                {
                    this.FindControl<RadioButton>("TimesBIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Helvetica")
                {
                    this.FindControl<RadioButton>("HelveticaButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Helvetica-Bold")
                {
                    this.FindControl<RadioButton>("HelveticaBButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Helvetica-Oblique")
                {
                    this.FindControl<RadioButton>("HelveticaIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Helvetica-BoldOblique")
                {
                    this.FindControl<RadioButton>("HelveticaBIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Courier")
                {
                    this.FindControl<RadioButton>("CourierButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Courier-Bold")
                {
                    this.FindControl<RadioButton>("CourierBButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Courier-Oblique")
                {
                    this.FindControl<RadioButton>("CourierIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Courier-BoldOblique")
                {
                    this.FindControl<RadioButton>("CourierBIButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "Symbol")
                {
                    this.FindControl<RadioButton>("SymbolButton").IsChecked = true;
                }
                else if (fnt.FontFamily.FileName == "ZapfDingbats")
                {
                    this.FindControl<RadioButton>("ZapfdingbatsButton").IsChecked = true;
                }
            }

            this.FindControl<NumericUpDown>("FontSizeBox").Value = fnt.FontSize;

        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Font fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesRoman), 18);
            VectSharp.Size size = fnt.MeasureText("Times-Roman");
            Page pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Times-Roman", fnt, Colour.FromRgb(0, 0, 0));
            Canvas can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("TimesButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBold), 18);
            size = fnt.MeasureText("Times-Bold");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Times-Bold", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("TimesBButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesItalic), 18);
            size = fnt.MeasureText("Times-Italic");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Times-Italic", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("TimesIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic), 18);
            size = fnt.MeasureText("Times-BoldItalic");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Times-BoldItalic", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("TimesBIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 18);
            size = fnt.MeasureText("Helvetica");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Helvetica", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("HelveticaButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold), 18);
            size = fnt.MeasureText("Helvetica-Bold");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Helvetica-Bold", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("HelveticaBButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaOblique), 18);
            size = fnt.MeasureText("Helvetica-Oblique");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Helvetica-Oblique", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("HelveticaIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique), 18);
            size = fnt.MeasureText("Helvetica-BoldOblique");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Helvetica-BoldOblique", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("HelveticaBIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Courier), 18);
            size = fnt.MeasureText("Courier");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Courier", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("CourierButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBold), 18);
            size = fnt.MeasureText("Courier-Bold");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Courier-Bold", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("CourierBButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierOblique), 18);
            size = fnt.MeasureText("Courier-Oblique");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Courier-Oblique", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("CourierIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBoldOblique), 18);
            size = fnt.MeasureText("Courier-BoldOblique");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Courier-BoldOblique", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("CourierBIButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.Symbol), 18);
            size = fnt.MeasureText("Σψμβολ");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "Σψμβολ", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("SymbolButton").Content = can;


            fnt = new Font(new VectSharp.FontFamily(VectSharp.FontFamily.StandardFontFamilies.ZapfDingbats), 18);
            size = fnt.MeasureText("✺❁❐❆✤❉■❇❂❁▼▲");
            pg = new Page(size.Width, size.Height);
            pg.Graphics.FillText(0, 0, "✺❁❐❆✤❉■❇❂❁▼▲", fnt, Colour.FromRgb(0, 0, 0));
            can = pg.PaintToCanvas();
            can.Margin = new Thickness(5, 0, 0, 0);
            can.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            this.FindControl<RadioButton>("ZapfdingbatsButton").Content = can;
            
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            string fontName = (from el in ((Grid)this.Content).Children where el is RadioButton && ((RadioButton)el).IsChecked == true select (string)((RadioButton)el).Tag).First();
            this.Font = new Font(new VectSharp.FontFamily(fontName), this.FindControl<NumericUpDown>("FontSizeBox").Value);
            
            Result = true;
            this.Close();
        }
    }
}
