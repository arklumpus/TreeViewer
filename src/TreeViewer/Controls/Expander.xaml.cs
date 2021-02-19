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
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System;
using System.Globalization;

namespace TreeViewer
{
    public class Expander : UserControl
    {
        public IControl Label
        {
            get
            {
                return this.FindControl<Border>("LabelBorder").Child;
            }

            set
            {
                this.FindControl<Border>("LabelBorder").Child = value;
            }
        }

        public IControl Child
        {
            get
            {
                return this.FindControl<Border>("ContentBorder").Child;
            }

            set
            {
                this.FindControl<Border>("ContentBorder").Child = value;
            }
        }

        public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<Expander, bool>(nameof(IsExpanded), false);

        public bool IsExpanded
        {
            get { return GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public Expander()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ExpanderClicked(object sender, PointerReleasedEventArgs e)
        {
            this.IsExpanded = !this.IsExpanded;
            this.FindControl<Border>("ContentBorder").Opacity = this.IsExpanded ? 1 : 0;
        }
    }

    public class BoolToAngleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? 180 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (double)value != 0;
        }
    }
}
