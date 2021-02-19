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
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VectSharp;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class DashControl : UserControl
    {
        static List<LineDash> StandardDashes = new List<LineDash>() { new LineDash(0, 0, 0), new LineDash(5, 5, 0), new LineDash(10, 5, 0), new LineDash(5, 10, 0), new LineDash(10, 10, 0), new LineDash(10, 20, 0) };

        private ObservableCollection<ComboBoxItem> Items;

        private LineDash? customDash = null;

        public event EventHandler DashChanged;

        public LineDash LineDash
        {
            get
            {
                return (LineDash)Items[this.FindControl<ComboBox>("DashComboBox").SelectedIndex].Tag;
            }

            set
            {
                for (int i = 0; i < StandardDashes.Count; i++)
                {
                    if (StandardDashes[i].Equals(value))
                    {
                        this.FindControl<ComboBox>("DashComboBox").SelectedIndex = i;
                        return;
                    }
                }

                if (customDash != null)
                {
                    customDash = value;
                    Page pg = new Page(128, 16);
                    pg.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: customDash);
                    Canvas item = pg.PaintToCanvas();
                    Items[^2] = new ComboBoxItem() { Tag = customDash, Content = item };
                }
                else
                {
                    customDash = value;
                    Page pg = new Page(128, 16);
                    pg.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: customDash);
                    Canvas item = pg.PaintToCanvas();
                    Items.Insert(Items.Count - 1, new ComboBoxItem() { Tag = customDash, Content = item });
                }

                DashChanged?.Invoke(this, new EventArgs());
            }
        }

        public DashControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            Items = new ObservableCollection<ComboBoxItem>();

            for (int i = 0; i < StandardDashes.Count; i++)
            {
                Page pg = new Page(128, 16);
                pg.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: StandardDashes[i]);
                Canvas item = pg.PaintToCanvas();
                Items.Add(new ComboBoxItem() { Tag = StandardDashes[i], Content = item });
            }

            Canvas customCanvas = new Canvas() { Width = 128, Height = 16 };
            Grid grd = new Grid() { Width = 128, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            customCanvas.Children.Add(grd);
            grd.Children.Add(new TextBlock() { Text = "Custom...", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left });
            Items.Add(new ComboBoxItem() { Content = customCanvas });

            this.FindControl<ComboBox>("DashComboBox").Items = Items;
            this.FindControl<ComboBox>("DashComboBox").SelectedIndex = 0;
            this.FindControl<ComboBox>("DashComboBox").SelectionChanged += async (s, e) =>
            {
                if (((ComboBoxItem)e.AddedItems[0]).Tag == null)
                {
                    int index = Math.Max(0, Items.IndexOf((ComboBoxItem)e.RemovedItems[0]));

                    CustomDashWindow win = new CustomDashWindow((LineDash)Items[index].Tag);

                    IControl parent = this.Parent;

                    while (!(parent is Window))
                    {
                        parent = parent.Parent;
                    }

                    await win.ShowDialog2((Window)parent);

                    if (win.Result)
                    {
                        if (customDash != null)
                        {
                            customDash = win.LineDash;
                            Page pg = new Page(128, 16);
                            pg.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: customDash);
                            Canvas item = pg.PaintToCanvas();
                            Items[^2] = new ComboBoxItem() { Tag = customDash, Content = item };
                            this.FindControl<ComboBox>("DashComboBox").Items = Items;
                            this.FindControl<ComboBox>("DashComboBox").SelectedIndex = Items.Count - 2;
                        }
                        else
                        {
                            customDash = win.LineDash;
                            Page pg = new Page(128, 16);
                            pg.Graphics.StrokePath(new GraphicsPath().MoveTo(5, 8).LineTo(123, 8), Colour.FromRgb(0, 0, 0), 2, lineDash: customDash);
                            Canvas item = pg.PaintToCanvas();
                            Items.Insert(Items.Count - 1, new ComboBoxItem() { Tag = customDash, Content = item });
                            this.FindControl<ComboBox>("DashComboBox").Items = Items;
                            this.FindControl<ComboBox>("DashComboBox").SelectedIndex = Items.Count - 2;
                        }
                    }
                    else
                    {
                        this.FindControl<ComboBox>("DashComboBox").SelectedIndex = index;
                    }
                }
                else
                {
                    DashChanged?.Invoke(this, new EventArgs());
                }
            };
        }
    }
}
