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

namespace TreeViewer
{
    public class NewAttributeWindow : ChildWindow
    {
        public NewAttributeWindow()
        {
            this.InitializeComponent();
        }

        public bool Result { get; private set; } = false;

        public string AttributeName
        {
            get { return this.FindControl<TextBox>("AttributeNameBox").Text; }
            set { this.FindControl<TextBox>("AttributeNameBox").Text = value; }
        }

        public string AttributeValue
        {
            get { return this.FindControl<TextBox>("AttributeValueBox").Text; }
            set { this.FindControl<TextBox>("AttributeValueBox").Text = value; }
        }

        public string AttributeType
        {
            get { return this.FindControl<ComboBox>("AttributeTypeBox").SelectedIndex == 0 ? "String": "Number"; }
            set { this.FindControl<ComboBox>("AttributeTypeBox").SelectedIndex = (value == "String") ? 0 : 1; }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AddAttribute")) { Width = 32, Height = 32 });
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
