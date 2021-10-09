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
using Avalonia.Controls.Presenters;
using Avalonia.Media;
using Avalonia.Styling;
using System.Collections.Generic;
using System.Linq;

namespace TreeViewer
{
    class AddPlottingModuleWindow : ChildWindow
    {
        public PlottingModule Result { get; private set; } = null;
        private List<(Button, PlottingModule)> moduleButtons = new List<(Button, PlottingModule)>();

        public AddPlottingModuleWindow()
        {
            this.Title = "Add Plot action module...";
            this.Width = 410;
            this.Height = 450;
            this.WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner;

            Style disabledButton = new Style(x => x.OfType<Button>().PropertyEquals(Button.IsEnabledProperty, false).Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            disabledButton.Setters.Add(new Setter(ContentPresenter.BackgroundProperty, Brushes.Transparent));
            this.Styles.Add(disabledButton);

            Style disabledButtonHelp = new Style(x => x.OfType<Button>().PropertyEquals(Button.IsEnabledProperty, false).Descendant().OfType<HelpButton>());
            disabledButtonHelp.Setters.Add(new Setter(HelpButton.OpacityProperty, 0.5));
            this.Styles.Add(disabledButtonHelp);

            Style disabledButtonIcon = new Style(x => x.OfType<Button>().PropertyEquals(Button.IsEnabledProperty, false).Descendant().OfType<DPIAwareBox>());
            disabledButtonIcon.Setters.Add(new Setter(DPIAwareBox.OpacityProperty, 0.5));
            this.Styles.Add(disabledButtonIcon);

            Style macOSButton = new Style(x => x.Class("MacOSStyle").Descendant().OfType<Button>());
            macOSButton.Setters.Add(new Setter(Button.CornerRadiusProperty, new CornerRadius(10)));
            this.Styles.Add(macOSButton);

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }

            Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
            grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            Grid header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            header.Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.AddPlotAction")) { Width = 32, Height = 32, Margin = new Avalonia.Thickness(0, 0, 5, 0) });
            {
                TextBlock blk = new TextBlock() { Text = "Add Plot action", FontSize = 16, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };
                Grid.SetColumn(blk, 1);
                header.Children.Add(blk);
            }
            grd.Children.Add(header);

            Grid filterGrid = new Grid() { Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            filterGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            filterGrid.Children.Add(new FilterIcon() { Margin = new Avalonia.Thickness(0, 0, 5, 0) });
            TextBox filterBox = new TextBox() { Watermark = "Filter modules", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Padding = new Avalonia.Thickness(5, 0, 5, 0), Height = 24, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 5, 0) };
            Grid.SetColumn(filterBox, 1);
            filterGrid.Children.Add(filterBox);
            Button clearButton = new Button() { Width = 24, Height = 24, Content = new DPIAwareBox(x => Icons.GetCrossIcon(x, new SolidColorBrush(Color.FromRgb(114, 114, 114)))) { Width = 16, Height = 16 }, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Padding = new Avalonia.Thickness(0), Background = Brushes.Transparent, CornerRadius = new CornerRadius(0) };
            clearButton.Classes.Add("PlainButton");
            Grid.SetColumn(clearButton, 2);
            filterGrid.Children.Add(clearButton);
            Grid.SetRow(filterGrid, 1);

            grd.Children.Add(filterGrid);

            ScrollViewer scroller = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Visible, Margin = new Avalonia.Thickness(0, 10, 0, 0), HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, AllowAutoHide = false, Padding = new Thickness(0, 0, 17, 0) };
            Grid.SetRow(scroller, 2);
            grd.Children.Add(scroller);

            StackPanel container = new StackPanel();
            scroller.Content = container;

            foreach (PlottingModule module in from el in Modules.PlottingModules orderby el.Name ascending select el)
            {
                Button button = new Button() { MinHeight = 60, Padding = new Thickness(5), Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Margin = new Thickness(0, 0, 0, 10) };

                button.Classes.Add("PlainButton");

                Grid content = new Grid();
                button.Content = content;
                content.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                content.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

                content.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                content.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                content.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                content.Children.Add(new DPIAwareBox(module.GetIcon) { Width = 16, Height = 16, Margin = new Thickness(0, 0, 5, 0) });

                {
                    TextBlock blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = module.Name, FontSize = 14, IsHitTestVisible = false, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(blk, 1);
                    content.Children.Add(blk);
                }

                HelpButton help = new HelpButton() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                Grid.SetColumn(help, 2);
                content.Children.Add(help);


                {
                    TextBlock blk = new TextBlock() { Text = module.HelpText, TextWrapping = Avalonia.Media.TextWrapping.Wrap, IsHitTestVisible = false, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, FontSize = 13 };
                    Grid.SetColumn(blk, 1);
                    Grid.SetColumnSpan(blk, 2);
                    Grid.SetRow(blk, 1);
                    content.Children.Add(blk);
                }

                string moduleId = module.Id;

                help.PointerPressed += (s, e) =>
                {
                    e.Handled = true;
                };

                help.Click += async (s, e) =>
                {
                    e.Handled = true;
                    HelpWindow helpWindow = new HelpWindow(Modules.LoadedModulesMetadata[moduleId].BuildReadmeMarkdown(), moduleId);
                    await helpWindow.ShowDialog2(this);
                };


                button.Click += (s, e) =>
                {
                    Result = module;
                    this.Close();
                };

                container.Children.Add(button);
                moduleButtons.Add((button, module));
            }

            this.Content = grd;

            clearButton.Click += (s, e) =>
            {
                filterBox.Text = "";
            };

            filterBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                {
                    string filterText = filterBox.Text;

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

            filterBox.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    for (int i = 0; i < moduleButtons.Count; i++)
                    {
                        if (moduleButtons[i].Item1.IsVisible)
                        {
                            Result = moduleButtons[i].Item2;
                            this.Close();
                            break;
                        }
                    }
                }
            };

            this.Opened += (s, e) =>
            {
                filterBox.Focus();
            };
        }
    }
}
