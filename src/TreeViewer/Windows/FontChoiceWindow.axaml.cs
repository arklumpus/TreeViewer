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
using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VectSharp;
using VectSharp.Canvas;
using VectSharp.Raster;

namespace TreeViewer
{

    public partial class FontChoiceWindow : ChildWindow
    {
        public bool Result { get; private set; } = false;
        public Font Font { get; private set; }

        public class WebFont
        {
            public enum Categories
            {
                SansSerif,
                Monospace,
                Serif,
                Display,
                Handwriting
            }

            public Categories Category { get; set; }
            public string Name { get; set; }
            public Dictionary<string, string> Styles { get; set; }
        }

        public static List<WebFont> WebFonts;
        public static System.IO.Stream FontIconStream;
        public static Dictionary<string, long[]> FontIconOffsets;

        static FontChoiceWindow()
        {
            string fontsJson;
            using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.fonts.json")))
            {
                fontsJson = sr.ReadToEnd();
            }

            WebFonts = System.Text.Json.JsonSerializer.Deserialize<List<WebFont>>(fontsJson);
            WebFonts.Sort((a, b) => a.Name.CompareTo(b.Name));

            System.IO.MemoryStream ms = new System.IO.MemoryStream();

            using (System.IO.Stream fs = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FontIcons.stream"))
            {
                fs.CopyTo(ms);
            }

            ms.Seek(0, System.IO.SeekOrigin.Begin);
            FontIconStream = ms;

            string offsetsJson;

            using (System.IO.StreamReader sr = new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FontIcons.offsets")))
            {
                offsetsJson = sr.ReadToEnd();
            }

            FontIconOffsets = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, long[]>>(offsetsJson);
        }

        private static Dictionary<string, Bitmap> fontIcons = new Dictionary<string, Bitmap>();

        public static Bitmap GetFontIcon(string fileName)
        {
            if (!fontIcons.TryGetValue(fileName, out Bitmap bmp))
            {
                long[] offset = FontIconOffsets[fileName];

                System.IO.MemoryStream iconStream = new System.IO.MemoryStream((int)offset[1]);

                FontIconStream.Seek(offset[0], System.IO.SeekOrigin.Begin);

                byte[] buffer = new byte[(int)offset[1]];
                int bytes = (int)offset[1];
                int read;
                while ((read = FontIconStream.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
                {
                    iconStream.Write(buffer, 0, read);
                    bytes -= read;
                }

                iconStream.Seek(0, System.IO.SeekOrigin.Begin);

                bmp = new Bitmap(iconStream);

                fontIcons[fileName] = bmp;
            }
            
            return bmp;
        }

        public static Bitmap GetFontIcon(FontFamily family)
        {
            if (!fontIcons.TryGetValue(family.FileName, out Bitmap bmp))
            {
                Page pag = new Page(32, 32);

                Font fnt = new Font(family, 12);

                string testText = "Ab";

                if (family.FileName == "Symbol")
                {
                    testText = "Αβ";
                }
                else if (family.FileName == "ZapfDingbats")
                {
                    testText = "❁❐";
                }


                Size textSize = fnt.MeasureText(testText);

                double scaleFactor = pag.Width / Math.Max(textSize.Width, textSize.Height);

                pag.Graphics.Scale(scaleFactor, scaleFactor);

                pag.Graphics.FillPath(new GraphicsPath().AddText(pag.Width * 0.5 / scaleFactor - textSize.Width * 0.5, pag.Height * 0.5 / scaleFactor - textSize.Height * 0.5, testText, fnt), Colours.Black);

                System.IO.MemoryStream iconStream = new System.IO.MemoryStream();

                pag.SaveAsPNG(iconStream);

                iconStream.Seek(0, System.IO.SeekOrigin.Begin);

                bmp = new Bitmap(iconStream);

                fontIcons[family.FileName] = bmp;
            }

            return bmp;
        }

        public FontChoiceWindow()
        {
            InitializeComponent();
        }

        public class WebFontItem
        {
            public string Name { get; set; }
            public string FileName { get; set; }
        }

        public class FontItem
        {
            public string Name { get; set; }
            public Bitmap Icon { get; set; }
        }

        private List<(string, FontFamily)> AttachedFontFamilies;
        private List<FontItem> AttachedItems;
        private List<WebFontItem> WebFontItems;

        public FontChoiceWindow(Font fnt, InstanceStateData stateData)
        {
            this.InitializeComponent();

            this.FindControl<NumericUpDown>("FontSizeBox").Value = fnt.FontSize;

            if (stateData != null)
            {
                AttachedFontFamilies = new List<(string, FontFamily)>();

                foreach (KeyValuePair<string, Attachment> kvp in stateData.Attachments)
                {
                    FontFamily family = kvp.Value.GetFontFamily();

                    if (family != null && family.TrueTypeFile != null)
                    {
                        AttachedFontFamilies.Add((kvp.Key, family));
                    }
                }

                AttachedFontFamilies.Sort((a, b) => (a.Item2.TrueTypeFile.GetFontFamilyName() ?? a.Item1).CompareTo(b.Item2.TrueTypeFile.GetFontFamilyName() ?? b.Item1));

                AttachedItems = new List<FontItem>(AttachedFontFamilies.Count);

                for (int i = 0; i < AttachedFontFamilies.Count; i++)
                {
                    AttachedItems.Add(new FontItem() { Name = AttachedFontFamilies[i].Item2.TrueTypeFile.GetFontFamilyName() ?? AttachedFontFamilies[i].Item1, Icon = GetFontIcon(AttachedFontFamilies[i].Item2) });
                }


                this.FindControl<ListBox>("AttachedFontsListBox").Items = AttachedItems;

                this.FindControl<TextBlock>("LicenceBlock").PointerReleased += (s, e) =>
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = "https://fonts.google.com/attribution",
                        UseShellExecute = true
                    });
                };


                WebFontItems = (from el in WebFonts select new WebFontItem() { Name = el.Name, FileName = System.IO.Path.GetFileName(el.Styles.ElementAt(0).Value) }).ToList();
                this.FindControl<ListBox>("WebFontsListBox").Items = WebFontItems;
            }

            if (fnt.FontFamily is AttachmentFontFamily aff)
            {
                string attachmentName = aff.AttachmentName;

                int index = -1;

                for (int i = 0; i < AttachedFontFamilies.Count; i++)
                {
                    if (AttachedFontFamilies[i].Item1 == attachmentName)
                    {
                        index = i;
                        break;
                    }
                }

                if (index >= 0)
                {
                    this.FindControl<ListBox>("AttachedFontsListBox").SelectedIndex = index;
                }
                else
                {
                    fnt = new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), fnt.FontSize);
                }
            }

            if (fnt.FontFamily is WebFontFamily wff)
            {
                string fontName = wff.FamilyName;

                string style = wff.Style;

                bool found = false;

                for (int i = 0; i < FontChoiceWindow.WebFonts.Count; i++)
                {
                    if (FontChoiceWindow.WebFonts[i].Name == fontName)
                    {
                        this.FindControl<ListBox>("WebFontsListBox").SelectedIndex = i;
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    this.FindControl<ListBox>("FontStyleBox").SelectedIndex = StyleItems.IndexOf(style);
                }
                else
                {
                    fnt = new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), fnt.FontSize);
                }
            }

            if (fnt.FontFamily.IsStandardFamily)
            {
                switch (fnt.FontFamily.FileName)
                {
                    case "Times-Roman":
                    case "Times-Bold":
                    case "Times-Italic":
                    case "Times-BoldItalic":
                        this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex = 1;
                        break;

                    case "Helvetica":
                    case "Helvetica-Bold":
                    case "Helvetica-Oblique":
                    case "Helvetica-BoldOblique":
                        this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex = 0;
                        break;

                    case "Courier":
                    case "Courier-Bold":
                    case "Courier-Oblique":
                    case "Courier-BoldOblique":
                        this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex = 2;
                        break;

                    case "Symbol":
                        this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex = 3;
                        break;

                    case "ZapfDingbats":
                        this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex = 4;
                        break;
                }

                switch (fnt.FontFamily.FileName)
                {
                    case "Times-Roman":
                    case "Helvetica":
                    case "Courier":
                    case "Symbol":
                    case "ZapfDingbats":
                        this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 0;
                        break;

                    case "Times-Bold":
                    case "Helvetica-Bold":
                    case "Courier-Bold":
                        this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 1;
                        break;


                    case "Helvetica-Oblique":
                    case "Times-Italic":
                    case "Courier-Oblique":
                        this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 2;
                        break;

                    case "Times-BoldItalic":
                    case "Helvetica-BoldOblique":
                    case "Courier-BoldOblique":
                        this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 3;
                        break;
                }
            }




            this.FindControl<ListBox>("FontStyleBox").SelectionChanged += (s, e) =>
            {
                if (!programmaticChange)
                {
                    UpdatePreview();
                }
            };

            UpdatePreview();
        }

        bool programmaticChange = false;

        List<string> StyleItems = null;

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!programmaticChange)
            {
                programmaticChange = true;

                if (((ListBox)sender).Name == "StandardFontsListBox")
                {
                    this.FindControl<ListBox>("AttachedFontsListBox").UnselectAll();
                    this.FindControl<ListBox>("WebFontsListBox").UnselectAll();
                }
                else if (((ListBox)sender).Name == "AttachedFontsListBox")
                {
                    this.FindControl<ListBox>("StandardFontsListBox").UnselectAll();
                    this.FindControl<ListBox>("WebFontsListBox").UnselectAll();
                }
                else if (((ListBox)sender).Name == "WebFontsListBox")
                {
                    this.FindControl<ListBox>("StandardFontsListBox").UnselectAll();
                    this.FindControl<ListBox>("AttachedFontsListBox").UnselectAll();
                }

                if (((ListBox)sender).Name == "StandardFontsListBox")
                {
                    switch (this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex)
                    {
                        case 0:
                        case 1:
                        case 2:
                            StyleItems = new List<string>() { "Regular", "Bold", "Italic", "Bold Italic" };
                            break;

                        case 3:
                        case 4:
                            StyleItems = new List<string>() { "Regular" };
                            break;
                    }

                    this.FindControl<ListBox>("FontStyleBox").Items = StyleItems;
                    this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 0;
                }
                else if (((ListBox)sender).Name == "AttachedFontsListBox")
                {
                    FontFamily family = AttachedFontFamilies[this.FindControl<ListBox>("AttachedFontsListBox").SelectedIndex].Item2;

                    string style = "Regular";

                    if (family.IsBold && (family.IsItalic || family.IsOblique))
                    {
                        style = "Bold Italic";
                    }
                    else if (family.IsItalic || family.IsOblique)
                    {
                        style = "Italic";
                    }
                    else if (family.IsBold)
                    {
                        style = "Bold";
                    }

                    StyleItems = new List<string>() { style };

                    this.FindControl<ListBox>("FontStyleBox").Items = StyleItems;
                    this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 0;
                }
                else if (((ListBox)sender).Name == "WebFontsListBox")
                {
                    WebFont fontItem = WebFonts[this.FindControl<ListBox>("WebFontsListBox").SelectedIndex];

                    StyleItems = new List<string>();

                    foreach (KeyValuePair<string, string> kvp in fontItem.Styles)
                    {
                        StyleItems.Add(kvp.Key);
                    }

                    this.FindControl<ListBox>("FontStyleBox").Items = StyleItems;
                    this.FindControl<ListBox>("FontStyleBox").SelectedIndex = 0;
                }

                programmaticChange = false;

                UpdatePreview();
            }
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Font = GetFont();

            Result = true;
            this.Close();
        }

        private Font GetFont()
        {
            FontFamily fontFamily = null;

            if (this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex >= 0)
            {
                bool bold = this.FindControl<ListBox>("FontStyleBox").SelectedIndex == 1 || this.FindControl<ListBox>("FontStyleBox").SelectedIndex == 3;
                bool italic = this.FindControl<ListBox>("FontStyleBox").SelectedIndex == 2 || this.FindControl<ListBox>("FontStyleBox").SelectedIndex == 3;

                switch (this.FindControl<ListBox>("StandardFontsListBox").SelectedIndex)
                {
                    case 0:
                        if (bold && italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBoldOblique);
                        }
                        else if (bold)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaBold);
                        }
                        else if (italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.HelveticaOblique);
                        }
                        else
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica);
                        }
                        break;

                    case 1:
                        if (bold && italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic);
                        }
                        else if (bold)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBold);
                        }
                        else if (italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesItalic);
                        }
                        else
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesRoman);
                        }
                        break;

                    case 2:
                        if (bold && italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBoldOblique);
                        }
                        else if (bold)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierBold);
                        }
                        else if (italic)
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.CourierOblique);
                        }
                        else
                        {
                            fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Courier);
                        }
                        break;

                    case 3:
                        fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Symbol);
                        break;
                    case 4:
                        fontFamily = VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.ZapfDingbats);
                        break;
                }
            }
            else if (this.FindControl<ListBox>("AttachedFontsListBox").SelectedIndex >= 0)
            {
                fontFamily = AttachedFontFamilies[this.FindControl<ListBox>("AttachedFontsListBox").SelectedIndex].Item2;
            }
            else if (this.FindControl<ListBox>("WebFontsListBox").SelectedIndex >= 0)
            {
                try
                {
                    WebFont fontItem = WebFonts[this.FindControl<ListBox>("WebFontsListBox").SelectedIndex];
                    string style = StyleItems[this.FindControl<ListBox>("FontStyleBox").SelectedIndex];

                    fontFamily = WebFontFamily.Create(fontItem, style);
                }
                catch
                {

                }
            }

            if (fontFamily != null)
            {
                return new Font(fontFamily, this.FindControl<NumericUpDown>("FontSizeBox").Value);
            }
            else
            {
                return new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), this.FindControl<NumericUpDown>("FontSizeBox").Value);
            }
        }

        private void UpdatePreview()
        {
            string testText = "The quick brown fox jumps over the lazy dog";

            Font fnt = new Font(GetFont().FontFamily, 12);

            if (fnt.FontFamily.FileName == "Symbol")
            {
                testText = "Σψμβολ";
            }
            else if (fnt.FontFamily.FileName == "ZapfDingbats")
            {
                testText = "✺❁❐❆✤❉■❇❂❁▼▲";
            }

            Font.DetailedFontMetrics metrics = fnt.MeasureTextAdvanced(testText);

            Page pag = new Page(metrics.Width + 64, metrics.Height + 8);

            pag.Graphics.FillText(pag.Width * 0.5 - metrics.Width * 0.5, 4, testText, fnt, Colours.Black, tag: "textPath");

            pag.Graphics.StrokePath(new GraphicsPath().MoveTo(0, 4 + metrics.Top).LineTo(24, 4 + metrics.Top).MoveTo(pag.Width - 24, 4 + metrics.Top).LineTo(pag.Width, 4 + metrics.Top), Colours.Black, pag.Height / 64);

            this.FindControl<Viewbox>("PreviewBox").Child = pag.PaintToCanvas(new Dictionary<string, Delegate>()
            {
                {
                    "textPath",
                    (Action<Avalonia.Controls.Shapes.Path>)(pth =>
                    {
                        ((Avalonia.Media.PathGeometry)pth.Data).FillRule = Avalonia.Media.FillRule.NonZero;
                    })
                }

            }, textOption: AvaloniaContextInterpreter.TextOptions.AlwaysConvert);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Page pag = new Page(32, 32);

            pag.Graphics.FillText(1, 25, "A", new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), 24), Colour.FromRgb(114, 114, 114), TextBaselines.Baseline);

            pag.Graphics.FillText(19, 25, "b", new Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesBoldItalic), 24), Colour.FromRgb(114, 114, 114), TextBaselines.Baseline);

            this.FindControl<Grid>("HeaderGrid").Children.Add(pag.PaintToCanvas(AvaloniaContextInterpreter.TextOptions.AlwaysConvert));

            List<FontItem> standardFonts = new List<FontItem>();

            standardFonts.Add(new FontItem() { Name = "Helvetica", Icon = GetFontIcon(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica)) });
            standardFonts.Add(new FontItem() { Name = "Times Roman", Icon = GetFontIcon(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.TimesRoman)) });
            standardFonts.Add(new FontItem() { Name = "Courier", Icon = GetFontIcon(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Courier)) });
            standardFonts.Add(new FontItem() { Name = "Symbol", Icon = GetFontIcon(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Symbol)) });
            standardFonts.Add(new FontItem() { Name = "ZapfDingbats", Icon = GetFontIcon(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.ZapfDingbats)) });

            this.FindControl<ListBox>("StandardFontsListBox").Items = standardFonts;
        }
    }

    public class WebFontFamily : VectSharp.FontFamily
    {
        public string Style { get; }
        public new string FamilyName { get; }

        private WebFontFamily(TrueTypeFile ttf, string familyName, string style) : base(ttf)
        {
            this.Style = style;
            this.FamilyName = familyName;
        }

        public static WebFontFamily Create(FontChoiceWindow.WebFont webFont, string style)
        {
            string url = webFont.Styles[style];

            string fileName = System.IO.Path.GetFileName(url);

            string targetFile = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebFonts", fileName);

            if (!System.IO.File.Exists(targetFile))
            {
                if (!System.IO.Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebFonts")))
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WebFonts"));
                }

                Modules.HttpClient.DownloadFile(url, targetFile);
            }

            FontFamily baseFamily = FontFamily.ResolveFontFamily(targetFile);
            
            Modules.FontLibrary.Add(baseFamily);

            return new WebFontFamily(baseFamily.TrueTypeFile, webFont.Name, style);
        }
    }

    public class FontIconConverter : Avalonia.Data.Converters.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                return FontChoiceWindow.GetFontIcon(str);
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
