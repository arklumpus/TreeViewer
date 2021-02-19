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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TreeViewer
{
    public static class AvaloniaBugFixes
    {
        private static readonly Dictionary<string, VectSharp.FontFamily> manifestFonts = new Dictionary<string, VectSharp.FontFamily>();

        public static double MeasureTextWidth(string text, Avalonia.Media.FontFamily fontFamily, Avalonia.Media.FontStyle fontStyle, Avalonia.Media.FontWeight fontWeight, double fontSize)
        {
            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            string fullFamily = fontFamily.FamilyNames + "-" + fontWeight.ToString() + fontStyle.ToString();

            if (!manifestFonts.ContainsKey(fullFamily))
            {
                string stream = null;

                switch (fontFamily.FamilyNames[0])
                {
                    case "Open Sans":
                    case "$Default":
                        switch (fontStyle)
                        {
                            case Avalonia.Media.FontStyle.Normal:
                                switch (fontWeight)
                                {
                                    case Avalonia.Media.FontWeight.Bold:
                                        stream = "TreeViewer.Fonts.OpenSans-Bold.ttf";
                                        break;
                                    case Avalonia.Media.FontWeight.Regular:
                                        stream = "TreeViewer.Fonts.OpenSans-Regular.ttf";
                                        break;
                                }
                                break;
                            case Avalonia.Media.FontStyle.Italic:
                            case Avalonia.Media.FontStyle.Oblique:
                                switch (fontWeight)
                                {
                                    case Avalonia.Media.FontWeight.Bold:
                                        stream = "TreeViewer.Fonts.OpenSans-BoldItalic.ttf";
                                        break;
                                    case Avalonia.Media.FontWeight.Regular:
                                        stream = "TreeViewer.Fonts.OpenSans-Italic.ttf";
                                        break;
                                }
                                break;
                        }
                        break;
                    case "Roboto Mono":
                        switch (fontStyle)
                        {
                            case Avalonia.Media.FontStyle.Normal:
                                switch (fontWeight)
                                {
                                    case Avalonia.Media.FontWeight.Regular:
                                        stream = "TreeViewer.Fonts.RobotoMono-Regular.ttf";
                                        break;
                                }
                                break;
                            case Avalonia.Media.FontStyle.Italic:
                            case Avalonia.Media.FontStyle.Oblique:
                                switch (fontWeight)
                                {
                                    case Avalonia.Media.FontWeight.Regular:
                                        stream = "TreeViewer.Fonts.RobotoMono-Italic.ttf";
                                        break;
                                }
                                break;
                        }
                        break;
                }

                if (!string.IsNullOrEmpty(stream))
                {
                    manifestFonts.Add(fullFamily, new VectSharp.FontFamily(GetManifestResourceStream(stream)));
                }
            }

            VectSharp.Font fnt = new VectSharp.Font(manifestFonts[fullFamily], fontSize);

            return fnt.MeasureText(text).Width;
        }


        private static Stream GetManifestResourceStream(string name)
        {
            return typeof(AvaloniaBugFixes).Assembly.GetManifestResourceStream(name);
        }

        public static Task ShowDialog2(this Window subject, Window owner)
        {
            /*subject.Opened += async (s, e) =>
            {
                subject.Width = subject.Width + 1;
                await Task.Delay(10);
                subject.Height = subject.Height + 1;
                await Task.Delay(100);
                subject.Width = subject.Width - 1;
                subject.Height = subject.Height - 1;
            };*/

            return subject.ShowDialog(owner);
        }
    }
}
