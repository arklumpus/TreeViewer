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
using System.Threading.Tasks;

namespace TreeViewer
{
    internal interface IWindowWithToolTips
    {
        List<Control> ControlsWithToolTips { get; }
    }

    public static class AvaloniaBugFixes
    {
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

        public static async Task ShowDialog2(this Window subject, Window owner)
        {
            if (owner.IsVisible)
            {
                Dictionary<Control, object> toolTips = null;

                if (Modules.IsMac)
                {
                    if (owner is IWindowWithToolTips win)
                    {
                        toolTips = new Dictionary<Control, object>();

                        foreach (Control control in win.ControlsWithToolTips)
                        {
                            object tip = ToolTip.GetTip(control);
                            
                            if (tip != null)
                            {
                                ToolTip.SetIsOpen(control, false);
                                toolTips[control] = tip;
                                ToolTip.SetTip(control, null);
                            }
                        }
                    }
                }

                await subject.ShowDialog(owner);

                if (Modules.IsMac)
                {
                    if (owner is IWindowWithToolTips win)
                    {
                        foreach (KeyValuePair<Control, object> kvp in toolTips)
                        {
                            ToolTip.SetTip(kvp.Key, kvp.Value);
                        }
                    }
                }
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

                ToolTip.SetTip(control, new ToolTip() { Content = tip });
            }
            else
            {
                ToolTip.SetTip(control, new ToolTip() { Content = tip });
            }
        }
    }
}
