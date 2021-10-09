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
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class CustomDashWindow : ChildWindow
    {
        public LineDash LineDash { get; private set; } = new LineDash(0, 0, 0);

        public bool Result { get; private set; } = false;

        public CustomDashWindow()
        {
            this.InitializeComponent();
        }

        public CustomDashWindow(LineDash lineDash)
        {
            this.InitializeComponent();

            this.FindControl<NumericUpDown>("UnitsOnBox").Value = lineDash.UnitsOn;
            this.FindControl<NumericUpDown>("UnitsOffBox").Value = lineDash.UnitsOff;
            this.FindControl<NumericUpDown>("PhaseBox").Value = lineDash.Phase;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            LineDash = new LineDash(this.FindControl<NumericUpDown>("UnitsOnBox").Value, this.FindControl<NumericUpDown>("UnitsOffBox").Value, this.FindControl<NumericUpDown>("PhaseBox").Value);

            Page page = new Page(128, 16);
            page.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: LineDash);
            this.FindControl<Viewbox>("PreviewContainer").Child = page.PaintToCanvas();
        }

        private void DashChanged(object sender, NumericUpDownValueChangedEventArgs e)
        {
            UpdatePreview();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
