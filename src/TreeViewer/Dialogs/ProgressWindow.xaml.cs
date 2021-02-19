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
using System.Threading;
using System;

namespace TreeViewer
{
    public class ProgressWindow : Window
    {
        private double _progress;

        public double Progress
        {
            get
            {
                return _progress;
            }

            set
            {
                _progress = value;
                this.FindControl<ProgressBar>("ProgressBar").Value = _progress * 100;
                this.FindControl<TextBlock>("ProgressDesc").Text = _progress.ToString("0%");
            }
        }

        public string ProgressText
        {
            get
            {
                return this.FindControl<TextBlock>("ProgressText").Text;
            }

            set
            {
                this.FindControl<TextBlock>("ProgressText").Text = value;
            }
        }

        public bool IsIndeterminate
        {
            set
            {
                if (value)
                {
                    this.FindControl<TextBlock>("ProgressText").IsVisible = true;
                    this.FindControl<ProgressBar>("ProgressBar").IsIndeterminate = true;
                    this.FindControl<Grid>("MainGrid").ColumnDefinitions[1] = new ColumnDefinition(0, GridUnitType.Pixel);
                }
                else
                {
                    this.FindControl<TextBlock>("ProgressText").IsVisible = true;
                    this.FindControl<ProgressBar>("ProgressBar").IsIndeterminate = false;
                    this.FindControl<Grid>("MainGrid").ColumnDefinitions[1] = new ColumnDefinition(50, GridUnitType.Pixel);
                }
            }
        }

        public ProgressWindow()
        {
            this.InitializeComponent();
        }

        EventWaitHandle Handle;

        public ProgressWindow(EventWaitHandle handle)
        {
            this.InitializeComponent();

            Handle = handle;
        }

        private void WindowOpened(object sender, EventArgs e)
        {
            if (Handle != null)
            {
                Handle.Set();
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
