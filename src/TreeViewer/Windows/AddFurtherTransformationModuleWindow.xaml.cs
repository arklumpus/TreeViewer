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
using System.Collections.Generic;

namespace TreeViewer
{
    public class AddFurtherTransformationModuleWindow : Window
    {
        public AddFurtherTransformationModuleWindow()
        {
            this.InitializeComponent();
        }

        public AddFurtherTransformationModuleWindow(List<FurtherTransformationModule> currentModules)
        {
            this.InitializeComponent();

            for (int i = 0; i < Modules.FurtherTransformationModules.Count; i++)
            {
                Border border = new Border();




                StackPanel panel = new StackPanel() { Margin = new Thickness(10) };
                border.Child = panel;

                panel.Children.Add(new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = Modules.FurtherTransformationModules[i].Name, FontSize = 18, IsHitTestVisible = false });

                Grid helpPanel = new Grid();
                helpPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                helpPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                helpPanel.Children.Add(new TextBlock() { Text = Modules.FurtherTransformationModules[i].HelpText, TextWrapping = Avalonia.Media.TextWrapping.Wrap, IsHitTestVisible = false, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                HelpButton help = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                helpPanel.Children.Add(help);
                Grid.SetColumn(help, 1);

                string moduleId = Modules.FurtherTransformationModules[i].Id;

                help.PointerPressed += async (s, e) =>
                {
                    HelpWindow helpWindow = new HelpWindow(Modules.LoadedModulesMetadata[moduleId].BuildReadmeMarkdown(), moduleId);
                    await helpWindow.ShowDialog(this);
                };

                panel.Children.Add(helpPanel);

                if (Modules.FurtherTransformationModules[i].Repeatable || !currentModules.Contains(Modules.FurtherTransformationModules[i]))
                {
                    border.Classes = new Classes("ModuleButton");
                    int j = i;

                    border.PointerReleased += (s, e) =>
                    {
                        Result = Modules.FurtherTransformationModules[j];
                        this.Close();
                    };
                }
                else
                {
                    border.Classes = new Classes("DisabledModuleButton");
                    panel.Children.Add(new TextBlock() { Text = "This module can only be used once!", TextWrapping = Avalonia.Media.TextWrapping.Wrap, IsHitTestVisible = false, FontWeight = Avalonia.Media.FontWeight.Bold, Foreground = new SolidColorBrush(Color.FromRgb(234, 28, 36)) });
                }



                this.FindControl<StackPanel>("TransformationModulesContainer").Children.Add(border);
            }
        }

        public FurtherTransformationModule Result { get; private set; } = null;


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
