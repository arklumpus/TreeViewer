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

namespace TreeViewer
{
    public class FileSizeControl : UserControl
    {
        public enum FileSizeUnit
        {
            B = 0, kiB = 1, MiB = 2, GiB = 3
        }

        private bool programmaticChange = false;

        private long value = 0;
        public long Value
        {
            get
            {
                return this.value;
            }

            set
            {
                if (!programmaticChange)
                {
                    double val = value;

                    for (int i = 0; i < this.FindControl<ComboBox>("UnitBox").SelectedIndex; i++)
                    {
                        val /= 1024.0;
                    }

                    programmaticChange = true;
                    this.FindControl<NumericUpDown>("ValueNud").Value = val;
                    programmaticChange = false;
                }
                this.value = value;
            }
        }

        public FileSizeUnit Unit
        {
            get
            {
                return (FileSizeUnit)this.FindControl<ComboBox>("UnitBox").SelectedIndex;
            }

            set
            {
                this.FindControl<ComboBox>("UnitBox").SelectedIndex = (int)value;
            }
        }

        public FileSizeControl()
        {
            this.InitializeComponent();

            this.FindControl<ComboBox>("UnitBox").SelectionChanged += (s, e) =>
            {
                double value = this.Value;

                for (int i = 0; i < this.FindControl<ComboBox>("UnitBox").SelectedIndex; i++)
                {
                    value /= 1024.0;
                }

                programmaticChange = true;
                this.FindControl<NumericUpDown>("ValueNud").Value = value;
                programmaticChange = false;
            };

            this.FindControl<NumericUpDown>("ValueNud").ValueChanged += (s, e) =>
            {
                if (!programmaticChange)
                {
                    double value = this.FindControl<NumericUpDown>("ValueNud").Value;

                    for (int i = 0; i < this.FindControl<ComboBox>("UnitBox").SelectedIndex; i++)
                    {
                        value *= 1024.0;
                    }

                    programmaticChange = true;
                    this.Value = (long)Math.Round(value);
                    programmaticChange = false;
                }
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
