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
using Avalonia.VisualTree;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TreeViewer
{
    internal interface IWindowWithToolTips
    {
        List<Control> ControlsWithToolTips { get; }
    }

    public static class AvaloniaBugFixes
    {
        private static readonly Dictionary<string, VectSharp.FontFamily> manifestFonts = new Dictionary<string, VectSharp.FontFamily>();

        public static Avalonia.Size MeasureText(string text, Avalonia.Media.FontFamily fontFamily, Avalonia.Media.FontStyle fontStyle, Avalonia.Media.FontWeight fontWeight, double fontSize, Size? availableSize = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new Avalonia.Size(0, 0);
            }

            Size avail = availableSize ?? Size.Infinity;

            return new Avalonia.Media.FormattedText(text, new Avalonia.Media.Typeface(fontFamily, fontStyle, fontWeight), fontSize, Avalonia.Media.TextAlignment.Left, Avalonia.Media.TextWrapping.NoWrap, avail).Bounds.Size;
        }

        public static double MeasureTextWidth(string text, Avalonia.Media.FontFamily fontFamily, Avalonia.Media.FontStyle fontStyle, Avalonia.Media.FontWeight fontWeight, double fontSize)
        {
            return MeasureText(text, fontFamily, fontStyle, fontWeight, fontSize).Width;
        }


        private static Stream GetManifestResourceStream(string name)
        {
            return typeof(AvaloniaBugFixes).Assembly.GetManifestResourceStream(name);
        }

        public static Task ShowDialog2(this Window subject, Window owner)
        {
            if (owner.IsVisible)
            {
                if (Modules.IsMac)
                {
                    if (owner is IWindowWithToolTips win)
                    {
                        foreach (Control control in win.ControlsWithToolTips)
                        {
                            if (ToolTip.GetIsOpen(control))
                            {
                                ToolTip.SetIsOpen(control, false);
                            }
                        }
                    }
                }

                return subject.ShowDialog(owner);
            }
            else
            {
                return Task.CompletedTask;
            }
        }

        public static void SetToolTip(Control control, object tip)
        {
            if (Modules.IsMac)
            {
                if (control.FindAncestorOfType<Window>() is IWindowWithToolTips win)
                {
                    win.ControlsWithToolTips.Add(control);
                }
                else
                {
                    control.AttachedToVisualTree += (s, e) =>
                    {
                        if (control.FindAncestorOfType<Window>() is IWindowWithToolTips win)
                        {
                            win.ControlsWithToolTips.Add(control);
                        }
                    };
                }

                ToolTip.SetTip(control, tip);
            }
            // Workaround for https://github.com/AvaloniaUI/Avalonia/issues/3536
            else if (!Modules.IsLinux)
            {
                ToolTip.SetTip(control, tip);
            }
            
        }
    }
}
