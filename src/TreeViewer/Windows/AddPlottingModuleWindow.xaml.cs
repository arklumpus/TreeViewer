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
using Avalonia.Markup.Xaml;
using System.Collections.Generic;

namespace TreeViewer
{
    public class AddPlottingModuleWindow : Window
    {
        public AddPlottingModuleWindow()
        {
            this.InitializeComponent();
        }

        public PlottingModule Result { get; private set; } = null;

        private List<(CoolButton, Module)> moduleButtons = new List<(CoolButton, Module)>();

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            for (int i = 0; i < Modules.PlottingModules.Count; i++)
            {
                CoolButton button = new CoolButton() { MinHeight = 60, CornerRadius = new CornerRadius(10) };
                button.Title = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = Modules.PlottingModules[i].Name, FontSize = 18, IsHitTestVisible = false, Foreground = Avalonia.Media.Brushes.White, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = new Thickness(10, 5) };

                Grid helpPanel = new Grid() { Margin = new Thickness(10) };
                helpPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                helpPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                helpPanel.Children.Add(new TextBlock() { Text = Modules.PlottingModules[i].HelpText, TextWrapping = Avalonia.Media.TextWrapping.Wrap, IsHitTestVisible = false, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                HelpButton help = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                helpPanel.Children.Add(help);
                Grid.SetColumn(help, 1);

                string moduleId = Modules.PlottingModules[i].Id;

                help.PointerPressed += (s, e) =>
                {
                    e.Handled = true;
                };

                help.Click += async (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow helpWindow = new HelpWindow(Modules.LoadedModulesMetadata[moduleId].BuildReadmeMarkdown(), moduleId);
                    await helpWindow.ShowDialog(this);
                };

                button.ButtonContent = helpPanel;

                int j = i;

                button.Click += (s, e) =>
                {
                    Result = Modules.PlottingModules[j];
                    this.Close();
                };

                this.FindControl<StackPanel>("PlottingModulesContainer").Children.Add(button);
                moduleButtons.Add((button, Modules.PlottingModules[i]));
            }

            this.FindControl<AddRemoveButton>("ClearFilterButton").PointerPressed += (s, e) =>
            {
                this.FindControl<TextBox>("FilterBox").Text = "";
            };

            this.FindControl<TextBox>("FilterBox").PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                {
                    string filterText = this.FindControl<TextBox>("FilterBox").Text;

                    if (string.IsNullOrEmpty(filterText))
                    {
                        for (int i = 0; i < moduleButtons.Count; i++)
                        {
                            moduleButtons[i].Item1.IsVisible = true;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < moduleButtons.Count; i++)
                        {
                            if (moduleButtons[i].Item2.Name.Contains(filterText, System.StringComparison.OrdinalIgnoreCase) || moduleButtons[i].Item2.HelpText.Contains(filterText, System.StringComparison.OrdinalIgnoreCase))
                            {
                                moduleButtons[i].Item1.IsVisible = true;
                            }
                            else
                            {
                                moduleButtons[i].Item1.IsVisible = false;
                            }
                        }
                    }
                }
            };

            this.Opened += (s, e) =>
            {
                this.FindControl<TextBox>("FilterBox").Focus();
            };
        }
    }
}
