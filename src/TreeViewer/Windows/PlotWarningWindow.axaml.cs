/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace TreeViewer
{
    public partial class PlotWarningWindow : ChildWindow
    {
        public PlotWarningWindow(MainWindow parent, Dictionary<string, (Module, string)> exceptions, Dictionary<string, (Module, string)> warnings) : this()
        {
            if (exceptions.Count > 0)
            {
                this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Warning")) { Width = 32, Height = 32 });
                this.FindControl<TextBlock>("WarningMessageBlock").Text = "Errors occurred while creating the tree plot.";
            }
            else
            {
                this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Information")) { Width = 32, Height = 32 });
                this.FindControl<TextBlock>("WarningMessageBlock").Text = "Warnings occurred while creating the tree plot.";
            }

            List<(string, TransformerModule, string, bool)> transformers = new List<(string, TransformerModule, string, bool)>();
            List<(string, FurtherTransformationModule, string, int, bool)> furtherTransformations = new List<(string, FurtherTransformationModule, string, int, bool)>();
            List<(string, CoordinateModule, string, bool)> coordinates = new List<(string, CoordinateModule, string, bool)>();
            List<(string, PlottingModule, string, int, bool)> plotActions = new List<(string, PlottingModule, string, int, bool)>();

            foreach (KeyValuePair<string, (Module, string)> kvp in exceptions)
            {
                if (kvp.Value.Item1 is TransformerModule transf)
                {
                    transformers.Add((kvp.Key, transf, kvp.Value.Item2, false));
                }
                else if (kvp.Value.Item1 is FurtherTransformationModule ftransf)
                {
                    int index = parent.FurtherTransformationsParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    furtherTransformations.Add((kvp.Key, ftransf, kvp.Value.Item2, index, false));
                }
                else if (kvp.Value.Item1 is CoordinateModule coord)
                {
                    coordinates.Add((kvp.Key, coord, kvp.Value.Item2, false));
                }
                else if (kvp.Value.Item1 is PlottingModule plot)
                {
                    int index = parent.PlottingParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    plotActions.Add((kvp.Key, plot, kvp.Value.Item2, index, false));
                }
            }

            foreach (KeyValuePair<string, (Module, string)> kvp in warnings)
            {
                if (kvp.Value.Item1 is TransformerModule transf)
                {
                    transformers.Add((kvp.Key, transf, kvp.Value.Item2, true));
                }
                else if (kvp.Value.Item1 is FurtherTransformationModule ftransf)
                {
                    int index = parent.FurtherTransformationsParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    furtherTransformations.Add((kvp.Key, ftransf, kvp.Value.Item2, index, true));
                }
                else if (kvp.Value.Item1 is CoordinateModule coord)
                {
                    coordinates.Add((kvp.Key, coord, kvp.Value.Item2, true));
                }
                else if (kvp.Value.Item1 is PlottingModule plot)
                {
                    int index = parent.PlottingParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    plotActions.Add((kvp.Key, plot, kvp.Value.Item2, index, true));
                }
            }

            if (transformers.Count > 0)
            {
                this.FindControl<StackPanel>("ErrorContainer").Children.Add(new TextBlock() { Text = "Transformer module", Margin = new Avalonia.Thickness(0, 5, 0, 5), FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

                for (int i = 0; i < transformers.Count; i++)
                {
                    Grid pnl = new Grid();

                    pnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    pnl.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                    StackPanel iconPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 5, 5, 0) };
                    Grid.SetRowSpan(iconPanel, 2);
                    pnl.Children.Add(iconPanel);

                    DPIAwareBox icon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Transformer")) { Width = 16, Height = 16, Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                    iconPanel.Children.Add(icon);

                    if (transformers[i].Item4)
                    {
                        iconPanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Information")) { Width = 16, Height = 16 });

                        VectSharp.MarkdownCanvas.MarkdownCanvasControl description = new VectSharp.MarkdownCanvas.MarkdownCanvasControl() { Margin = new Avalonia.Thickness(0, 0, 0, 5), FontSize = 12 };
                        description.TextConversionOption = VectSharp.Canvas.AvaloniaContextInterpreter.TextOptions.NeverConvert;
                        description.Renderer.IndentWidth = 15;
                        description.Renderer.RegularFontFamily = Modules.UIVectSharpFontFamily;
                        description.Renderer.ItalicFontFamily = Modules.UIVectSharpFontFamilyItalic;
                        description.Renderer.BoldFontFamily = Modules.UIVectSharpFontFamilyBold;
                        description.Renderer.BoldItalicFontFamily = Modules.UIVectSharpFontFamilyBoldItalic;

                        for (int j = 0; j < description.Renderer.Bullets.Count; j++)
                        {
                            Action<VectSharp.Graphics, VectSharp.Colour> originalBullet = description.Renderer.Bullets[j];

                            description.Renderer.Bullets[j] = (g, c) => { g.Scale(0.75, 0.75); g.Translate(0, 0.1); originalBullet(g, c); };
                        }

                        description.Renderer.SpaceAfterParagraph = 0.25;
                        description.Renderer.BackgroundColour = VectSharp.Colour.FromRgba(0, 0, 0, 0);
                        description.Renderer.Margins = new VectSharp.Markdown.Margins(-3, 0, 0, -7);
                        description.DocumentSource = transformers[i].Item3;

                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }
                    else
                    {
                        iconPanel.Children.Add(MainWindow.GetAlertIcon());

                        TextBlock description = new TextBlock() { Text = transformers[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }

                    TextBlock name = new TextBlock() { Text = transformers[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    Button btn = new Button() { Margin = new Avalonia.Thickness(0, 0, 0, 5), Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    btn.Classes.Add("SideBarButton");
                    btn.Content = pnl;

                    string key = transformers[i].Item1;
                    Module module = transformers[i].Item2;

                    btn.Click += async (s, e) =>
                    {
                        this.Close();
                        await parent.HighlightModule(key, module);
                    };

                    this.FindControl<StackPanel>("ErrorContainer").Children.Add(btn);
                }
            }

            if (furtherTransformations.Count > 0)
            {
                furtherTransformations.Sort((a, b) => a.Item4.CompareTo(b.Item4));

                this.FindControl<StackPanel>("ErrorContainer").Children.Add(new TextBlock() { Text = "Further transformations", Margin = new Avalonia.Thickness(0, 5, 0, 5), FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

                for (int i = 0; i < furtherTransformations.Count; i++)
                {
                    Grid pnl = new Grid();

                    pnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    pnl.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                    StackPanel iconPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 5, 5, 0) };
                    Grid.SetRowSpan(iconPanel, 2);
                    pnl.Children.Add(iconPanel);

                    DPIAwareBox icon = new DPIAwareBox(furtherTransformations[i].Item2.GetIcon) { Width = 16, Height = 16, Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                    iconPanel.Children.Add(icon);

                    if (furtherTransformations[i].Item5)
                    {
                        iconPanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Information")) { Width = 16, Height = 16 });

                        VectSharp.MarkdownCanvas.MarkdownCanvasControl description = new VectSharp.MarkdownCanvas.MarkdownCanvasControl() { Margin = new Avalonia.Thickness(0, 0, 0, 5), FontSize = 12 };
                        description.TextConversionOption = VectSharp.Canvas.AvaloniaContextInterpreter.TextOptions.NeverConvert;
                        description.Renderer.IndentWidth = 15;
                        description.Renderer.RegularFontFamily = Modules.UIVectSharpFontFamily;
                        description.Renderer.ItalicFontFamily = Modules.UIVectSharpFontFamilyItalic;
                        description.Renderer.BoldFontFamily = Modules.UIVectSharpFontFamilyBold;
                        description.Renderer.BoldItalicFontFamily = Modules.UIVectSharpFontFamilyBoldItalic;

                        for (int j = 0; j < description.Renderer.Bullets.Count; j++)
                        {
                            Action<VectSharp.Graphics, VectSharp.Colour> originalBullet = description.Renderer.Bullets[j];

                            description.Renderer.Bullets[j] = (g, c) => { g.Scale(0.75, 0.75); g.Translate(0, 0.1); originalBullet(g, c); };
                        }

                        description.Renderer.SpaceAfterParagraph = 0.25;
                        description.Renderer.BackgroundColour = VectSharp.Colour.FromRgba(0, 0, 0, 0);
                        description.Renderer.Margins = new VectSharp.Markdown.Margins(-3, 0, 0, -7);
                        description.DocumentSource = furtherTransformations[i].Item3;

                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }
                    else
                    {
                        iconPanel.Children.Add(MainWindow.GetAlertIcon());

                        TextBlock description = new TextBlock() { Text = furtherTransformations[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }

                    TextBlock name = new TextBlock() { Text = furtherTransformations[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    Button btn = new Button() { Margin = new Avalonia.Thickness(0, 0, 0, 5), Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    btn.Classes.Add("SideBarButton");
                    btn.Content = pnl;

                    string key = furtherTransformations[i].Item1;
                    Module module = furtherTransformations[i].Item2;

                    btn.Click += async (s, e) =>
                    {
                        this.Close();
                        await parent.HighlightModule(key, module);
                    };

                    this.FindControl<StackPanel>("ErrorContainer").Children.Add(btn);
                }
            }

            if (coordinates.Count > 0)
            {
                this.FindControl<StackPanel>("ErrorContainer").Children.Add(new TextBlock() { Text = "Coordinates module", Margin = new Avalonia.Thickness(0, 5, 0, 5), FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

                for (int i = 0; i < coordinates.Count; i++)
                {
                    Grid pnl = new Grid();

                    pnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    pnl.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                    StackPanel iconPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 5, 5, 0) };
                    Grid.SetRowSpan(iconPanel, 2);
                    pnl.Children.Add(iconPanel);

                    DPIAwareBox icon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Coordinates")) { Width = 16, Height = 16, Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                    iconPanel.Children.Add(icon);

                    if (coordinates[i].Item4)
                    {
                        iconPanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Information")) { Width = 16, Height = 16 });

                        VectSharp.MarkdownCanvas.MarkdownCanvasControl description = new VectSharp.MarkdownCanvas.MarkdownCanvasControl() { Margin = new Avalonia.Thickness(0, 0, 0, 5), FontSize = 12 };
                        description.TextConversionOption = VectSharp.Canvas.AvaloniaContextInterpreter.TextOptions.NeverConvert;
                        description.Renderer.IndentWidth = 15;
                        description.Renderer.RegularFontFamily = Modules.UIVectSharpFontFamily;
                        description.Renderer.ItalicFontFamily = Modules.UIVectSharpFontFamilyItalic;
                        description.Renderer.BoldFontFamily = Modules.UIVectSharpFontFamilyBold;
                        description.Renderer.BoldItalicFontFamily = Modules.UIVectSharpFontFamilyBoldItalic;


                        for (int j = 0; j < description.Renderer.Bullets.Count; j++)
                        {
                            Action<VectSharp.Graphics, VectSharp.Colour> originalBullet = description.Renderer.Bullets[j];

                            description.Renderer.Bullets[j] = (g, c) => { g.Scale(0.75, 0.75); g.Translate(0, 0.1); originalBullet(g, c); };
                        }

                        description.Renderer.SpaceAfterParagraph = 0.25;
                        description.Renderer.BackgroundColour = VectSharp.Colour.FromRgba(0, 0, 0, 0);
                        description.Renderer.Margins = new VectSharp.Markdown.Margins(-3, 0, 0, -7);
                        description.DocumentSource = coordinates[i].Item3;

                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }
                    else
                    {
                        iconPanel.Children.Add(MainWindow.GetAlertIcon());

                        TextBlock description = new TextBlock() { Text = coordinates[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }

                    TextBlock name = new TextBlock() { Text = coordinates[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    Button btn = new Button() { Margin = new Avalonia.Thickness(0, 0, 0, 5), Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    btn.Classes.Add("SideBarButton");
                    btn.Content = pnl;

                    string key = coordinates[i].Item1;
                    Module module = coordinates[i].Item2;

                    btn.Click += async (s, e) =>
                    {
                        this.Close();
                        await parent.HighlightModule(key, module);
                    };

                    this.FindControl<StackPanel>("ErrorContainer").Children.Add(btn);
                }
            }

            if (plotActions.Count > 0)
            {
                plotActions.Sort((a, b) => a.Item4.CompareTo(b.Item4));

                this.FindControl<StackPanel>("ErrorContainer").Children.Add(new TextBlock() { Text = "Plot actions", Margin = new Avalonia.Thickness(0, 5, 0, 5), FontSize = 15, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)) });

                for (int i = 0; i < plotActions.Count; i++)
                {
                    Grid pnl = new Grid();

                    pnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    pnl.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    pnl.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                    StackPanel iconPanel = new StackPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 5, 5, 0) };
                    Grid.SetRowSpan(iconPanel, 2);
                    pnl.Children.Add(iconPanel);

                    DPIAwareBox icon = new DPIAwareBox(plotActions[i].Item2.GetIcon) { Width = 16, Height = 16, Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                    iconPanel.Children.Add(icon);

                    if (plotActions[i].Item5)
                    {
                        iconPanel.Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Information")) { Width = 16, Height = 16 });

                        VectSharp.MarkdownCanvas.MarkdownCanvasControl description = new VectSharp.MarkdownCanvas.MarkdownCanvasControl() { Margin = new Avalonia.Thickness(0, 0, 0, 5), FontSize = 12 };
                        description.TextConversionOption = VectSharp.Canvas.AvaloniaContextInterpreter.TextOptions.NeverConvert;
                        description.Renderer.IndentWidth = 15;
                        description.Renderer.RegularFontFamily = Modules.UIVectSharpFontFamily;
                        description.Renderer.ItalicFontFamily = Modules.UIVectSharpFontFamilyItalic;
                        description.Renderer.BoldFontFamily = Modules.UIVectSharpFontFamilyBold;
                        description.Renderer.BoldItalicFontFamily = Modules.UIVectSharpFontFamilyBoldItalic;

                        for (int j = 0; j < description.Renderer.Bullets.Count; j++)
                        {
                            Action<VectSharp.Graphics, VectSharp.Colour> originalBullet = description.Renderer.Bullets[j];

                            description.Renderer.Bullets[j] = (g, c) => { g.Scale(0.75, 0.75); g.Translate(0, 0.1); originalBullet(g, c); };
                        }

                        description.Renderer.SpaceAfterParagraph = 0.25;
                        description.Renderer.BackgroundColour = VectSharp.Colour.FromRgba(0, 0, 0, 0);
                        description.Renderer.Margins = new VectSharp.Markdown.Margins(-10, 0, 0, -7);
                        description.DocumentSource = plotActions[i].Item3;

                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }
                    else
                    {
                        iconPanel.Children.Add(MainWindow.GetAlertIcon());

                        TextBlock description = new TextBlock() { Text = plotActions[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                        Grid.SetColumn(description, 1);
                        Grid.SetRow(description, 1);
                        pnl.Children.Add(description);
                    }

                    TextBlock name = new TextBlock() { Text = plotActions[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    Button btn = new Button() { Margin = new Avalonia.Thickness(0, 0, 0, 5), Background = Brushes.Transparent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    btn.Classes.Add("SideBarButton");
                    btn.Content = pnl;

                    string key = plotActions[i].Item1;
                    Module module = plotActions[i].Item2;

                    btn.Click += async (s, e) =>
                    {
                        this.Close();
                        await parent.HighlightModule(key, module);
                    };

                    this.FindControl<StackPanel>("ErrorContainer").Children.Add(btn);
                }
            }
        }

        public PlotWarningWindow()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
