/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
using System;

namespace TreeViewer
{
    public class PreviewComboBox : UserControl
    {
        public static readonly DirectProperty<PreviewComboBox, int> SelectedIndexProperty = ComboBox.SelectedIndexProperty.AddOwner<PreviewComboBox>(x => x.SelectedIndex, (o, x) => o.SelectedIndex = x);

        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                if (ContainedBox != null)
                {
                    ContainedBox.SelectedIndex = value;
                }
            }
        }

        private ComboBox ContainedBox;

        public System.Collections.IEnumerable Items
        {
            get
            {
                return ContainedBox.Items;
            }

            set
            {
                ContainedBox.Items = value;
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<SelectionChangedEventArgs> PreviewSelectionChanged;

        public PreviewComboBox()
        {
            ContainedBox = new ComboBox() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

            this.Content = ContainedBox;

            ContainedBox.SelectionChanged += (s, e) =>
            {
                this.PreviewSelectionChanged?.Invoke(this, e);
                this.SelectedIndex = ContainedBox.SelectedIndex;
                this.SelectionChanged?.Invoke(this, e);
            };
        }



    }
}
