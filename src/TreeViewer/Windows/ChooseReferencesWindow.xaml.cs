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
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace TreeViewer
{
    public class ChooseReferencesWindow : Window
    {
        public ChooseReferencesWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public List<string> References { get; set; } = new List<string>();

        public bool Result { get; private set; } = false;

        private async void AddReferenceClicked(object sender, PointerReleasedEventArgs e)
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

                IndexedGrid grd = new IndexedGrid();
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                grd.Children.Add(new TextBlock() { Text = System.IO.Path.GetFileName(result[0]), Margin = new Thickness(5) });

                AddRemoveButton but = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove, Margin = new Thickness(5) };
                Grid.SetColumn(but, 1);
                grd.Children.Add(but);

                this.FindControl<StackPanel>("ReferenceContainer").Children.Add(grd);

                int index = References.Count - 1;

                but.PointerReleased += (s, e) =>
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


    public class IndexedGrid : Grid
    {
        public static readonly DirectProperty<IndexedGrid, int> IndexProperty = AvaloniaProperty.RegisterDirect<IndexedGrid, int>(nameof(Index), o => o.Index);

        public int Index
        {
            get
            {
                IPanel parent = (IPanel)this.Parent;
                return parent.Children.IndexOf(this);
            }
        }

        public static readonly DirectProperty<IndexedGrid, bool> IndexIsEvenProperty = AvaloniaProperty.RegisterDirect<IndexedGrid, bool>(nameof(IndexIsEven), o => o.IndexIsEven);

        public bool IndexIsEven
        {
            get
            {
                IPanel parent = (IPanel)this.Parent;
                return parent.Children.IndexOf(this) % 2 == 0;
            }
        }
    }
}
