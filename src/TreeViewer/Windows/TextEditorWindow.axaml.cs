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
    public partial class TextEditorWindow : ChildWindow
    {
        public bool Result { get; private set; } = false;
        public string Text
        {
            get => this.FindControl<TextBox>("sourceBox").Text;
            set => this.FindControl<TextBox>("sourceBox").Text = value;
        }
        public TextEditorWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            Result = true;
            this.Close();
        }


        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Result = false;
            this.Close();
        }
    }
}
