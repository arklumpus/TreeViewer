using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System.Collections.Generic;
using System.Linq;

namespace TreeViewer
{
    public partial class PlotWarningWindow : ChildWindow
    {
        public PlotWarningWindow(MainWindow parent, Dictionary<string, (Module, string)> exceptions) : this()
        {
            List<(string, TransformerModule, string)> transformers = new List<(string, TransformerModule, string)>();
            List<(string, FurtherTransformationModule, string, int)> furtherTransformations = new List<(string, FurtherTransformationModule, string, int)>();
            List<(string, CoordinateModule, string)> coordinates = new List<(string, CoordinateModule, string)>();
            List<(string, PlottingModule, string, int)> plotActions = new List<(string, PlottingModule, string, int)>();

            foreach (KeyValuePair<string, (Module, string)> kvp in exceptions)
            {
                if (kvp.Value.Item1 is TransformerModule transf)
                {
                    transformers.Add((kvp.Key, transf, kvp.Value.Item2));
                }
                else if (kvp.Value.Item1 is FurtherTransformationModule ftransf)
                {
                    int index = parent.FurtherTransformationsParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    furtherTransformations.Add((kvp.Key, ftransf, kvp.Value.Item2, index));
                }
                else if (kvp.Value.Item1 is CoordinateModule coord)
                {
                    coordinates.Add((kvp.Key, coord, kvp.Value.Item2));
                }
                else if (kvp.Value.Item1 is PlottingModule plot)
                {
                    int index = parent.PlottingParameters.FindIndex(x => (string)x[Modules.ModuleIDKey] == kvp.Key);
                    plotActions.Add((kvp.Key, plot, kvp.Value.Item2, index));
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

                    DPIAwareBox icon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Transformer")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 10, 5, 0) };
                    Grid.SetRowSpan(icon, 2);
                    pnl.Children.Add(icon);

                    TextBlock name = new TextBlock() { Text = transformers[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    TextBlock description = new TextBlock() { Text = transformers[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(description, 1);
                    Grid.SetRow(description, 1);
                    pnl.Children.Add(description);

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

                    DPIAwareBox icon = new DPIAwareBox(furtherTransformations[i].Item2.GetIcon) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 10, 5, 0) };
                    Grid.SetRowSpan(icon, 2);
                    pnl.Children.Add(icon);

                    TextBlock name = new TextBlock() { Text = furtherTransformations[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    TextBlock description = new TextBlock() { Text = furtherTransformations[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(description, 1);
                    Grid.SetRow(description, 1);
                    pnl.Children.Add(description);

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

                    DPIAwareBox icon = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Coordinates")) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 10, 5, 0) };
                    Grid.SetRowSpan(icon, 2);
                    pnl.Children.Add(icon);

                    TextBlock name = new TextBlock() { Text = coordinates[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    TextBlock description = new TextBlock() { Text = coordinates[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(description, 1);
                    Grid.SetRow(description, 1);
                    pnl.Children.Add(description);

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

                    DPIAwareBox icon = new DPIAwareBox(plotActions[i].Item2.GetIcon) { Width = 16, Height = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(0, 10, 5, 0) };
                    Grid.SetRowSpan(icon, 2);
                    pnl.Children.Add(icon);

                    TextBlock name = new TextBlock() { Text = plotActions[i].Item2.Name, FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 0), TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black };
                    Grid.SetColumn(name, 1);
                    pnl.Children.Add(name);

                    TextBlock description = new TextBlock() { Text = plotActions[i].Item3, FontSize = 12, Margin = new Avalonia.Thickness(0, 0, 0, 5), TextWrapping = TextWrapping.Wrap };
                    Grid.SetColumn(description, 1);
                    Grid.SetRow(description, 1);
                    pnl.Children.Add(description);

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
            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Warning")) { Width = 32, Height = 32 });
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
