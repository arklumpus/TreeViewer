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

namespace TreeViewer
{
    public class FillingControl<T> : Grid where T : Control
    {
        public T Child { get; }

        public FillingControl(T child, double margin)
        {
            child.MaxWidth = 0;
            this.Children.Add(child);

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == FillingControl<T>.BoundsProperty)
                {
                    child.MaxWidth = this.Bounds.Width - margin * 2;
                }
            };
        }
    }
}
