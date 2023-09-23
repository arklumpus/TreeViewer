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
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Styling;

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


        public static ColorButton TextColorButton(IBrush background, out Button applyButton)
        {
            ColorButton tbr = new ColorButton();

            ToggleButton toggleButton = (ToggleButton)typeof(AvaloniaColorPicker.ColorButton<ColourPickerWindow>).GetProperty("ContentButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(tbr);
            toggleButton.Classes.Add("PlainButton");

            toggleButton.Padding = new Avalonia.Thickness(0);

            toggleButton.Background = background;
            Border paletteBorder = (Border)((Popup)((Grid)toggleButton.Content).Children[2]).Child;

            paletteBorder.Background = new SolidColorBrush(Color.FromRgb(243, 243, 243));
            ((Grid)paletteBorder.Child).Children[0].Classes.Add("PlainButton");

            Grid toggleButtonContentGrid = (Grid)toggleButton.Content;

            Border colorPreview = (Border)((Grid)toggleButton.Content).Children[0];
            toggleButtonContentGrid.Children.Remove(colorPreview);
            colorPreview.MinHeight = 5;
            colorPreview.MinWidth = 16;
            colorPreview.Height = 5;
            colorPreview.Width = 16;

            StackPanel letterPanel = new StackPanel();
            letterPanel.Children.Add(new TextBlock() { Text = "A", Width = 16, TextAlignment = TextAlignment.Center, Margin = new Avalonia.Thickness(-2) });

            letterPanel.Children.Add(colorPreview);

            applyButton = new Button() { Content = letterPanel, Padding = new Avalonia.Thickness(3), Background = Brushes.Transparent };
            applyButton.Classes.Add("PlainButton");

            toggleButtonContentGrid.Children.Insert(0, applyButton);

            Style noBorderStyle = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyle.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Avalonia.Thickness(0) });
            Style noBorderStyleChecked = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Class(":checked").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyleChecked.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Avalonia.Thickness(0) });
            Style noBorderStyleFocus = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Class(":checked:focus").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyleFocus.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Avalonia.Thickness(0) });
            toggleButton.Styles.Add(noBorderStyle);
            toggleButton.Styles.Add(noBorderStyleChecked);
            toggleButton.Styles.Add(noBorderStyleFocus);

            return tbr;
        }
    }
}