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
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class DPIAwareBox : Viewbox
    {
        private Grid container;

        private Dictionary<double, Control> cachedControls;

        private double lastScaling = double.NaN;

        public DPIAwareBox()
        {
            container = new Grid();
            this.Child = container;

            cachedControls = new Dictionary<double, Control>();
        }

        public DPIAwareBox(Func<double, Control> getControlAtResolution)
        {
            container = new Grid();
            this.Child = container;

            cachedControls = new Dictionary<double, Control>();

            this.GetControlAtResolution = getControlAtResolution;
        }

        public DPIAwareBox(Func<double, Page> getControlAtResolution)
        {
            container = new Grid();
            this.Child = container;

            cachedControls = new Dictionary<double, Control>();

            if (getControlAtResolution != null)
            {
                this.GetControlAtResolution = scaling => getControlAtResolution(scaling).PaintToCanvas();
            }
            else
            {
                this.GetControlAtResolution = scaling => new Canvas() { Width = 16, Height = 16 };
            }
        }

        public Func<double, Control> GetControlAtResolution { get; set; }

        public override void Render(DrawingContext context)
        {
            double scaling = (this.VisualRoot as ILayoutRoot)?.LayoutScaling ?? 1;

            if (scaling != lastScaling)
            {
                ShowControl(scaling);   
            }

            base.Render(context);
        }

        public void ShowControl(double scaling)
        {
            if (cachedControls.TryGetValue(lastScaling, out Control lastControl))
            {
                lastControl.IsVisible = false;
            }

            if (cachedControls.TryGetValue(scaling, out Control control))
            {
                control.IsVisible = true;
            }
            else
            {
                control = this.GetControlAtResolution(scaling);
                control.IsVisible = true;
                container.Children.Add(control);

                cachedControls[scaling] = control;
            }

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { this.InvalidateVisual(); });

            lastScaling = scaling;
        }
    }
}
