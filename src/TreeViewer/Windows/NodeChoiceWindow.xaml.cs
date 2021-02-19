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
    public class NodeChoiceWindow : Window
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
            Grid pnl = new Grid() { Margin = new Thickness(0, 0, 17, 5) };

            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Star)));
            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(1, GridUnitType.Pixel)));
            pnl.ColumnDefinitions.Add(new ColumnDefinition(new GridLength(0, GridUnitType.Auto)));

            ComboBox box = new ComboBox() { Items = names, Margin = new Thickness(0, 0, 5, 0), SelectedIndex = index, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, ClipToBounds = false };
            pnl.Children.Add(box);

            AddRemoveButton but = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Remove, Margin = new Thickness(5, 0, 0, 0) };
            Grid.SetColumn(but, 2);
            pnl.Children.Add(but);

            but.PointerReleased += (s, e) =>
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
        }

        private void AddButtonClicked(object sender, PointerReleasedEventArgs e)
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