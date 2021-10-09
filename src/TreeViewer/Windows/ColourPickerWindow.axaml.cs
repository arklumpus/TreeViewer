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
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaColorPicker;
using System.Threading.Tasks;

namespace TreeViewer
{
    public partial class ColourPickerWindow : ChildWindow, IColorPickerWindow
    {
        /// <summary>
        /// The color that is currently selected in the <see cref="ColorPicker"/>.
        /// </summary>
        public Color Color
        {
            get => this.FindControl<ColorPicker>("ColorPicker").Color;
            set
            {
                this.FindControl<ColorPicker>("ColorPicker").Color = value;
            }
        }

        /// <summary>
        /// Represents the previously selected <see cref="Avalonia.Media.Color"/> (e.g. if the <see cref="ColorPicker"/> is being used to change the colour of an object, it would represent the previous colour of the object. Set to <see langword="null" /> to hide the previous colour display.
        /// </summary>
        public Color? PreviousColor
        {
            get => this.FindControl<ColorPicker>("ColorPicker").PreviousColor;
            set
            {
                this.FindControl<ColorPicker>("ColorPicker").PreviousColor = value;
            }
        }

        private bool Result = false;

        /// <summary>
        /// Creates a new <see cref="ColorPickerWindow"/> instance.
        /// </summary>
        public ColourPickerWindow()
        {
            this.InitializeComponent();

            this.FindControl<ColorPicker>("ColorPicker").FindControl<TextBox>("Hex_Box").Padding = new Thickness(5, 2, 5, 2);

            this.FindControl<Button>("OKButton").Click += (s, e) =>
            {
                this.Result = true;
                this.Close();
            };

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Result = false;
                this.Close();
            };
        }

        /// <summary>
        /// Creates a new <see cref="ColorPickerWindow"/> instance, setting the <see cref="Color"/> and <see cref="PreviousColor"/> to the specified value.
        /// </summary>
        /// <param name="previousColor"></param>
        public ColourPickerWindow(Color? previousColor) : this()
        {
            this.PreviousColor = previousColor;

            if (previousColor != null)
            {
                this.Color = previousColor.Value;
            }
        }

        /// <summary>
        /// Shows the <see cref="ColorPickerWindow"/> as a dialog.
        /// </summary>
        /// <param name="parent">The <see cref="ColorPickerWindow"/>'s owner window.</param>
        /// <returns>The selected <see cref="Avalonia.Media.Color"/> if the user clicks on the "OK" button; <see langword="null"/> otherwise.</returns>
        public new async Task<Color?> ShowDialog(Window parent)
        {
            await this.ShowDialog2(parent);

            if (this.Result)
            {
                return this.Color;
            }
            else
            {
                return null;
            }
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
