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
using System;

namespace TreeViewer
{
    public class VerticalButton : UserControl
    {
        public VerticalButton()
        {
            InitializeComponent();
        }

        public event EventHandler<RoutedEventArgs> Click;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Button>("MainContent").Content = new DPIAwareBox(Icons.GetIcon("TreeViewer.Assets.Wrench-16.png", "TreeViewer.Assets.Wrench-24.png", "TreeViewer.Assets.Wrench-32.png", 8, 16)) { Width = 8, Height = 16 };
            this.FindControl<Button>("MainContent").Click += (s, e) =>
            {
                this.Click?.Invoke(this, e);
            };
        }
    }
}
