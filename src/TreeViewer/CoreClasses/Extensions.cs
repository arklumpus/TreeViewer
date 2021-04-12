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

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using VectSharp;
using Avalonia.Media;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using System.Text.RegularExpressions;

namespace TreeViewer
{
    public static class Extensions
    {
        //Adapted from https://github.com/microsoft/referencesource/blob/master/mscorlib/system/io/stream.cs
        public static void CopyToWithProgress(this System.IO.Stream source, System.IO.Stream destination, Action<double> progressAction = null, int bufferSize = 81920)
        {
            byte[] buffer = new byte[bufferSize];
            int read;
            double length = source.Length;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
            {
                destination.Write(buffer, 0, read);
                double progress = Math.Max(0, Math.Min(1, source.Position / length));
                progressAction?.Invoke(progress);
            }

        }

        public static string GetAttributeType(this TreeNode node, string attributeName)
        {
            if (node.Children.Count == 0)
            {
                if (node.Attributes.TryGetValue(attributeName, out object value))
                {
                    return value.GetAttributeType();
                }
                else
                {
                    return null;
                }
            }
            else
            {
                Dictionary<string, int> counts = new Dictionary<string, int>();
                for (int i = 0; i < Modules.AttributeTypes.Length; i++)
                {
                    counts.Add(Modules.AttributeTypes[i], 0);
                }

                foreach (TreeNode nod in node.Children)
                {
                    string type = nod.GetAttributeType(attributeName);
                    if (!string.IsNullOrEmpty(type))
                    {
                        counts[type]++;
                    }
                }

                if (node.Attributes.TryGetValue(attributeName, out object value))
                {
                    string type = value.GetAttributeType();
                    if (!string.IsNullOrEmpty(type))
                    {
                        counts[type]++;
                    }
                }

                KeyValuePair<string, int> first = (from el in counts orderby el.Value descending select el).First();

                return first.Value > 0 ? first.Key : null;
            }
        }

        public static string GetAttributeType(this object obj)
        {
            if (obj is double)
            {
                return "Number";
            }
            else if (obj is string)
            {
                return "String";
            }
            else
            {
                return null;
            }
        }

        public static double MinOrDefault(this IEnumerable<double> array, double defaultValue)
        {
            return array.Any() ? array.Min() : defaultValue;
        }

        public static Dictionary<string, object> ShallowClone(this Dictionary<string, object> dict)
        {
            Dictionary<string, object> tbr = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> kvp in dict)
            {
                tbr.Add(kvp.Key, kvp.Value);
            }

            return tbr;
        }

        public static string GetFormatString(this double val)
        {
            if (val == 0 || double.IsNaN(val) || double.IsInfinity(val))
            {
                return "0";
            }
            else if (Math.Abs(val) >= 1)
            {
                return "0";
            }
            else
            {
                int OoM = -(int)Math.Floor(Math.Log10(Math.Abs(val)));
                return "0." + new string('0', OoM);
            }
        }


        public static string ToString(this double val, int significantDigits, bool decimalDigits = true)
        {
            if (decimalDigits)
            {
                if (val == 0 || double.IsNaN(val) || double.IsInfinity(val))
                {
                    return val.ToString("0." + new string('0', significantDigits), System.Globalization.CultureInfo.InvariantCulture);
                }
                else if (val >= 1)
                {
                    return val.ToString("0." + new string('0', significantDigits), System.Globalization.CultureInfo.InvariantCulture);
                }
                else
                {
                    int OoM = -(int)Math.Floor(Math.Log10(Math.Abs(val)));

                    if (significantDigits - OoM >= 0)
                    {
                        return val.ToString("0." + new string('0', significantDigits), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return (val * Math.Pow(10, OoM)).ToString("0." + new string('0', significantDigits), System.Globalization.CultureInfo.InvariantCulture) + "e-" + OoM.ToString();
                    }
                }
            }
            else
            {
                if (val == 0 || double.IsNaN(val) || double.IsInfinity(val))
                {
                    return "0";
                }
                else if (val >= 1)
                {
                    int OoM = (int)Math.Floor(Math.Log10(Math.Abs(val)));

                    val = Math.Round(val / Math.Pow(10, OoM - significantDigits + 1)) * Math.Pow(10, OoM - significantDigits + 1);

                    if (OoM + 1 >= significantDigits)
                    {
                        return val.ToString("0", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return val.ToString("0." + new string('0', significantDigits - OoM - 1));
                    }
                }
                else
                {
                    int OoM = -(int)Math.Floor(Math.Log10(Math.Abs(val)));

                    if (OoM + significantDigits <= 8)
                    {
                        return val.ToString("0." + new string('0', OoM + significantDigits - 1), System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        for (int i = 0; i < OoM; i++)
                        {
                            val *= 10;
                        }
                        return val.ToString("0." + new string('0', significantDigits), System.Globalization.CultureInfo.InvariantCulture) + "e-" + OoM.ToString();
                    }
                }
            }
        }

        public static bool IsCodeDifferent(string source1, string source2)
        {
            try
            {
                SyntaxTree syntaxTree1 = SyntaxFactory.ParseSyntaxTree(SourceText.From(source1));
                SyntaxTree syntaxTree2 = SyntaxFactory.ParseSyntaxTree(SourceText.From(source2));

                return !syntaxTree1.IsEquivalentTo(syntaxTree2);
            }
            catch
            {
                return true;
            }
        }

        public static Avalonia.Media.Color ToAvalonia(this Colour col)
        {
            return Avalonia.Media.Color.FromArgb((byte)Math.Round(col.A * 255), (byte)Math.Round(col.R * 255), (byte)Math.Round(col.G * 255), (byte)Math.Round(col.B * 255));
        }

        public static Colour ToVectSharp(this Avalonia.Media.Color col)
        {
            return Colour.FromRgba(col.R, col.G, col.B, col.A);
        }

        public static string ToHexString(this Colour col)
        {
            return col.ToCSSString(col.A < 1);
        }

        public static Colour Reverse(this Colour col)
        {
            return Colour.FromRgba(1 - col.R, 1 - col.G, 1 - col.B, col.A);
        }


        /// <summary>
        /// Strip illegal chars and reserved words from a candidate filename (should not include the directory path)
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/309485/c-sharp-sanitize-file-name
        /// </remarks>
        public static string CoerceValidFileName(this string filename)
        {
            var invalidChars = Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            var invalidReStr = string.Format(@"[{0}]+", invalidChars);

            var reservedWords = new[]
            {
                "CON", "PRN", "AUX", "CLOCK$", "NUL", "COM0", "COM1", "COM2", "COM3", "COM4",
                "COM5", "COM6", "COM7", "COM8", "COM9", "LPT0", "LPT1", "LPT2", "LPT3", "LPT4",
                "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
            };

            var sanitisedNamePart = Regex.Replace(filename, invalidReStr, "_");
            foreach (var reservedWord in reservedWords)
            {
                var reservedWordPattern = string.Format("^{0}\\.", reservedWord);
                sanitisedNamePart = Regex.Replace(sanitisedNamePart, reservedWordPattern, "_reservedWord_.", RegexOptions.IgnoreCase);
            }

            return sanitisedNamePart;
        }

        public static IBrush GetColourBrush(this Colour col)
        {
            if (col.A == 1)
            {
                return new SolidColorBrush(Color.FromRgb((byte)(col.R * 255), (byte)(col.G * 255), (byte)(col.B * 255)));
            }
            else
            {
                byte whiteR = (byte)(col.R * col.A * 255 + 255 * (1 - col.A));
                byte whiteG = (byte)(col.G * col.A * 255 + 255 * (1 - col.A));
                byte whiteB = (byte)(col.B * col.A * 255 + 255 * (1 - col.A));

                byte greyR = (byte)(col.R * col.A * 255 + 230 * (1 - col.A));
                byte greyG = (byte)(col.G * col.A * 255 + 230 * (1 - col.A));
                byte greyB = (byte)(col.B * col.A * 255 + 230 * (1 - col.A));

                Canvas tile = new Canvas() { Width = 32, Height = 32, Background = new SolidColorBrush(Color.FromRgb(whiteR, whiteG, whiteB)) };
                SolidColorBrush greyBrush = new SolidColorBrush(Color.FromRgb(greyR, greyG, greyB));

                tile.Children.Add(new Rectangle() { Width = 16, Height = 16, Fill = greyBrush });
                tile.Children.Add(new Rectangle() { Width = 16, Height = 16, Fill = greyBrush, RenderTransform = new TranslateTransform(16, 16) });

                return new VisualBrush(tile) { TileMode = TileMode.Tile, Stretch = Stretch.None, DestinationRect = new Avalonia.RelativeRect(0, 0, 20, 20, Avalonia.RelativeUnit.Absolute) };
            }
        }
    }
}
