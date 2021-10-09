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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace TreeViewer
{
    public class ChooseReferencesWindow : ChildWindow
    {
        public ChooseReferencesWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SourceCode")) { Width = 32, Height = 32 });
        }

        public List<string> References { get; set; } = new List<string>();

        public bool Result { get; private set; } = false;

        private async void AddReferenceClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "DLL file", Extensions = new List<string>() { "dll" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } };

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                References.Add(result[0]);

                Grid grd = new Grid();
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                grd.Children.Add(new TextBlock() { Text = System.IO.Path.GetFileName(result[0]), Margin = new Thickness(5) });

                Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
                deleteButton.Classes.Add("SideBarButton");
                Grid.SetColumn(deleteButton, 1);
                grd.Children.Add(deleteButton);

                this.FindControl<StackPanel>("ReferenceContainer").Children.Add(grd);

                int index = References.Count - 1;

                deleteButton.Click += (s, e) =>
                {
                    References.RemoveAt(index);
                    this.FindControl<StackPanel>("ReferenceContainer").Children.Remove(grd);
                };
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.Result = true;
            this.Close();
        }
    }
}
