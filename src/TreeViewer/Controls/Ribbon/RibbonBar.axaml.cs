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
using Avalonia.Media.Transformation;

namespace TreeViewer
{
    public partial class RibbonBar : UserControl
    {
        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<RibbonBar, int>(nameof(SelectedIndex), 0);

        public int SelectedIndex
        {
            get { return GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public Grid[] GridItems;

        public RibbonBar()
        {
            InitializeComponent(new (string, bool)[] { });
        }

        public RibbonBar((string, bool)[] items)
        {
            InitializeComponent(items);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == SelectedIndexProperty)
            {
                int prevInd = change.OldValue.GetValueOrDefault<int>();
                int newInd = change.NewValue.GetValueOrDefault<int>();

                GridItems[prevInd].Classes.Remove("Selected");
                GridItems[newInd].Classes.Add("Selected");

                if (this.Classes.Contains("Grey"))
                {
                    GridItems[prevInd].PropertyChanged -= UpdateWhenBoundsUpdated;
                    GridItems[newInd].PropertyChanged += UpdateWhenBoundsUpdated;

                    UpdateSelectionGrid(newInd);
                }
            }
        }

        private void UpdateSelectionGrid(int index)
        {
            if (this.Classes.Contains("Grey"))
            {
                this.FindControl<Grid>("SelectionGrid").Width = GridItems[index].Bounds.Width;

                TransformOperations.Builder builder = TransformOperations.CreateBuilder(1);
                builder.AppendTranslate(GridItems[index].Bounds.Left, 0);

                this.FindControl<Grid>("SelectionGrid").RenderTransform = builder.Build();
            }
        }

        private void UpdateWhenBoundsUpdated(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == Grid.BoundsProperty)
            {
                UpdateSelectionGrid(this.SelectedIndex);
            }
        }

        private void InitializeComponent((string, bool)[] items)
        {
            AvaloniaXamlLoader.Load(this);

            GridItems = new Grid[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                Grid itemGrid = new Grid() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                if (items[i].Item2)
                {
                    itemGrid.Classes.Add("Contextual");
                }

                if (i == SelectedIndex)
                {
                    itemGrid.Classes.Add("Selected");
                }

                {
                    TextBlock blk = new TextBlock() { Text = items[i].Item1, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Foreground = Brushes.Transparent, TextAlignment = TextAlignment.Center };
                    itemGrid.Children.Add(blk);
                    blk.Classes.Add("SizingBlock");
                }

                {
                    TextBlock blk = new TextBlock() { Text = items[i].Item1, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, TextAlignment = TextAlignment.Center };
                    itemGrid.Children.Add(blk);
                    blk.Classes.Add("ActualBlock");
                }

                this.FindControl<StackPanel>("ItemsContainer").Children.Add(itemGrid);

                int index = i;

                itemGrid.PointerPressed += (s, e) =>
                {
                    this.SelectedIndex = index;
                };

                GridItems[i] = itemGrid;
            }

            GridItems[this.SelectedIndex].PropertyChanged += UpdateWhenBoundsUpdated;

            UpdateSelectionGrid(this.SelectedIndex);
        }
    }
}
