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
using System.Linq;
using PhyloTree;

namespace TreeViewer
{
    public class NodeChoiceWindow : ChildWindow
    {
        public string[] Result = null;

        List<string> names;

        List<Grid> nodeNameContainers;

        public NodeChoiceWindow()
        {
            this.InitializeComponent();
        }

        public NodeChoiceWindow(TreeNode tree, string[] currentValue)
        {
            this.InitializeComponent();

            names = new List<string>(from el in tree.GetChildrenRecursiveLazy() where !string.IsNullOrEmpty(el.Name) select el.Name);

            names.Sort();

            nodeNameContainers = new List<Grid>();

            for (int i = 0; i < currentValue.Length; i++)
            {
                int ind;
                if ((ind = names.IndexOf(currentValue[i])) >= 0)
                {
                    AddNodeItem(ind);
                }
            }
        }

        private void AddNodeItem(int index)
        {
            Grid pnl = new Grid() { Margin = new Thickness(0, 0, 0, 5) };

            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Pixel)));
            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0, GridUnitType.Auto)));

            ComboBox box = new ComboBox() { Items = names, Margin = new Thickness(0, 0, 5, 0), SelectedIndex = index, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, ClipToBounds = false, FontSize = 13, Padding = new Thickness(5, 2, 5, 2) };
            pnl.Children.Add(box);

            Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
            deleteButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteButton, 2);
            pnl.Children.Add(deleteButton);

            deleteButton.Click += (s, e) =>
            {
                nodeNameContainers.Remove(pnl);
                this.FindControl<StackPanel>("NodeContainer").Children.Remove(pnl);
            };

            nodeNameContainers.Add(pnl);

            this.FindControl<StackPanel>("NodeContainer").Children.Add(pnl);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SelectNode")) { Width = 32, Height = 32 });
        }

        private void AddButtonClicked(object sender, RoutedEventArgs e)
        {
            AddNodeItem(0);
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            List<string> tbr = new List<string>();

            for (int i = 0; i < nodeNameContainers.Count; i++)
            {
                tbr.Add(names[((ComboBox)nodeNameContainers[i].Children[0]).SelectedIndex]);
            }

            this.Result = tbr.ToArray();
            this.Close();
        }
    }
}