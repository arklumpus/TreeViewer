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
using System;
using System.Collections.Generic;
using System.Linq;
using VectSharp;

namespace TreeViewer
{
    public class GradientStop
    {
        public double Position { get; set; }
        public Colour Colour { get; set; }
        public string Tag { get; set; }

        public GradientStop(double position, Colour colour)
        {
            this.Position = Math.Min(1, Math.Max(position, 0));
            this.Colour = colour;
        }

        public string SerializeJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(new string[] { System.Text.Json.JsonSerializer.Serialize(this.Position, Modules.DefaultSerializationOptions), System.Text.Json.JsonSerializer.Serialize(new double[] { this.Colour.R, this.Colour.G, this.Colour.B, this.Colour.A }, Modules.DefaultSerializationOptions) }, Modules.DefaultSerializationOptions);
        }

        public static GradientStop DeserializeJson(string serialized)
        {
            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(serialized, Modules.DefaultSerializationOptions);
            double position = System.Text.Json.JsonSerializer.Deserialize<double>(items[0], Modules.DefaultSerializationOptions);
            double[] colour = System.Text.Json.JsonSerializer.Deserialize<double[]>(items[1], Modules.DefaultSerializationOptions);

            return new GradientStop(position, Colour.FromRgba(colour[0], colour[1], colour[2], colour[3]));
        }
    }

    public class Gradient
    {
        public List<GradientStop> GradientStops { get; set; }

        public void SortGradient()
        {
            this.GradientStops.Sort((a, b) => Math.Sign(a.Position - b.Position));
        }

        public Gradient(List<GradientStop> gradientStops)
        {
            this.GradientStops = new List<GradientStop>();
            foreach (GradientStop st in gradientStops)
            {
                this.GradientStops.Add(new GradientStop(st.Position, st.Colour));
            }

            SortGradient();

            if (this.GradientStops[0].Position > 0)
            {
                this.GradientStops.Insert(0, new GradientStop(0, this.GradientStops[0].Colour));
            }

            if (this.GradientStops[^1].Position < 1)
            {
                this.GradientStops.Add(new GradientStop(1, this.GradientStops[^1].Colour));
            }
        }

        public Colour GetColour(double position)
        {
            position = Math.Max(Math.Min(position, 1), 0);

            double prevPos = 0;
            double pos = GradientStops[1].Position;
            int index = 1;

            while (pos < position && index < GradientStops.Count - 1)
            {
                prevPos = pos;
                index++;
                pos = this.GradientStops[index].Position;
            }

            if (pos - prevPos > 0)
            {
                double factor = (position - prevPos) / (pos - prevPos);

                return Colour.FromRgba(this.GradientStops[index].Colour.R * factor + this.GradientStops[index - 1].Colour.R * (1 - factor),
                    this.GradientStops[index].Colour.G * factor + this.GradientStops[index - 1].Colour.G * (1 - factor),
                    this.GradientStops[index].Colour.B * factor + this.GradientStops[index - 1].Colour.B * (1 - factor),
                    this.GradientStops[index].Colour.A * factor + this.GradientStops[index - 1].Colour.A * (1 - factor));
            }
            else
            {
                return this.GradientStops[index].Colour;
            }

        }

        public string SerializeJson()
        {
            return System.Text.Json.JsonSerializer.Serialize((from el in this.GradientStops select el.SerializeJson()).ToArray(), Modules.DefaultSerializationOptions);
        }

        public static Gradient DeserializeJson(string serialized)
        {
            return new Gradient((from el in System.Text.Json.JsonSerializer.Deserialize<string[]>(serialized, Modules.DefaultSerializationOptions) select GradientStop.DeserializeJson(el)).ToList());
        }
    }

    public class GradientControl : UserControl
    {
        private Gradient gradient = Modules.DefaultGradients["TransparentToBlack"];
        public Gradient Gradient
        {
            get
            {
                return gradient;
            }
            set
            {
                gradient = value;
                UpdateGradient();
            }
        }

        public GradientControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            UpdateGradient();
        }

        private void UpdateGradient()
        {
            Avalonia.Media.LinearGradientBrush brs = new Avalonia.Media.LinearGradientBrush() { StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative), EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative), SpreadMethod = GradientSpreadMethod.Pad };

            for (int i = 0; i < Gradient.GradientStops.Count; i++)
            {
                brs.GradientStops.Add(new Avalonia.Media.GradientStop(Color.FromArgb((byte)(255 * Gradient.GradientStops[i].Colour.A), (byte)(255 * Gradient.GradientStops[i].Colour.R), (byte)(255 * Gradient.GradientStops[i].Colour.G), (byte)(255 * Gradient.GradientStops[i].Colour.B)), Gradient.GradientStops[i].Position));
            }

            this.FindControl<Border>("Main").Background = brs;
        }
    }
}
