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
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace TreeViewer
{
    public class ColorButton : AvaloniaColorPicker.ColorButton<ColourPickerWindow>
    {
        public ColorButton()
        {
            ToggleButton toggleButton = (ToggleButton)typeof(AvaloniaColorPicker.ColorButton<ColourPickerWindow>).GetProperty("ContentButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(this);
            Border paletteBorder = (Border)((Popup)((Grid)toggleButton.Content).Children[2]).Child;

            paletteBorder.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243));
            ((Grid)paletteBorder.Child).Children[0].Classes.Add("PlainButton");
        }
    }
}