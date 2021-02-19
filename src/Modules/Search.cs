using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.Canvas;
using System.Runtime.InteropServices;

namespace Search
{
    /// <summary>
    /// This module is used to search and highlight nodes in the tree.
    /// 
    /// When this module's button is clicked, a search bar appears above the tree plot. This can be used to search for specific values in the attributes
    /// of the tree. The default is to search within the names of taxa, but it is possible to perform the search in any attribute. For numerical attributes,
    /// it is also possible to highlight nodes where the attribute is greater than or smaller than the specified value.
    /// 
    /// When nodes are matched by the search criterion, they are highlighted in the tree. It is then possible to copy their names to the clipboard (using the
    /// `Copy` button, or to use the `Replace` button to replace the value of the attribute. This enables the _Replace attribute_ module (id `f17160ad-0462-449a-8a57-e1af775c92ba`).
    /// The settings for the _Replace attribute_ module can then be changed in order e.g. to add a new attribute to the nodes that have been matched by the
    /// search criterion.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Search";
        public const string HelpText = "Searches leaves in the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "5f3a7147-f706-43dc-9f57-18ade0c7b15d";
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static string ButtonText { get; } = "Search";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.F;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
        public static bool TriggerInTextBox { get; } = false;

        private const string IconBase64 = "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";

        public static Page GetIcon()
        {
            byte[] bytes = Convert.FromBase64String(IconBase64);

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(icon.Width, icon.Height);
            pag.Graphics.DrawRasterImage(0, 0, icon);

            return pag;
        }

        public static void PerformAction(MainWindow window, InstanceStateData stateData)
        {
            if (window.TransformedTree == null || window.PlottingActions.Count == 0 || (stateData.Tags.TryGetValue("5f3a7147-f706-43dc-9f57-18ade0c7b15d", out object searchTag) && (bool)searchTag))
            {
                return;
            }
            stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = true;

            Avalonia.Controls.PanAndZoom.ZoomBorder zom = window.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");

            Border searchBord = new Border() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Margin = new Avalonia.Thickness(50, 15, 50, 10), Padding = new Avalonia.Thickness(10, 10), CornerRadius = new Avalonia.CornerRadius(10), Background = window.SelectionChildBrush, Opacity = 0, BorderBrush = window.SelectionBrush, BorderThickness = new Avalonia.Thickness(2) };
            Grid searchGrid = new Grid();
            searchBord.Child = searchGrid;

            searchGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            searchGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            searchGrid.Children.Add(new TextBlock() { Text = "Search:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

            TextBox searchBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(searchBox, 1);
            searchGrid.Children.Add(searchBox);

            {
                TextBlock blk = new TextBlock() { Text = "Replace with:", Margin = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                searchGrid.Children.Add(blk);
            }

            TextBox replaceBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(replaceBox, 3);
            searchGrid.Children.Add(replaceBox);

            Grid buttonGrid = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, -28), Width = 160 };
            buttonGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            buttonGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(65, GridUnitType.Pixel));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetColumn(buttonGrid, 4);
            searchGrid.Children.Add(buttonGrid);

            CheckBox autoSearchBox = new CheckBox() { Content = "Auto", Padding = new Avalonia.Thickness(5, 0, 5, 0), Margin = new Avalonia.Thickness(5, 0, 5, 0), IsChecked = true };
            buttonGrid.Children.Add(autoSearchBox);

            Button findAllButton = new Button() { Content = "Find all", Padding = new Avalonia.Thickness(5, 5, 5, 5), Margin = new Avalonia.Thickness(5, 0, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            Grid.SetColumn(findAllButton, 1);
            buttonGrid.Children.Add(findAllButton);

            Button replaceAllButton = new Button() { Content = "Replace all", Padding = new Avalonia.Thickness(5, 5, 5, 5), Margin = new Avalonia.Thickness(5, 5, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            Grid.SetRow(replaceAllButton, 1);
            Grid.SetColumn(replaceAllButton, 1);
            buttonGrid.Children.Add(replaceAllButton);

            Button copyButton = new Button() { Content = "Copy", Padding = new Avalonia.Thickness(5, 5, 5, 5), Margin = new Avalonia.Thickness(5, 5, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            Grid.SetRow(copyButton, 1);
            buttonGrid.Children.Add(copyButton);

            AddRemoveButton closeButton = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Cancel, Margin = new Avalonia.Thickness(5, 0, 5, 0) };
            Grid.SetColumn(closeButton, 5);
            searchGrid.Children.Add(closeButton);

            TreeViewer.Expander advancedExpander = new TreeViewer.Expander() { Label = new TextBlock() { Text = "Advanced", FontWeight = Avalonia.Media.FontWeight.Bold }, Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            Grid advancedContent = new Grid();
            advancedExpander.Child = advancedContent;
            Grid.SetRow(advancedExpander, 1);
            Grid.SetColumnSpan(advancedExpander, 8);
            searchGrid.Children.Add(advancedExpander);

            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            advancedContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            advancedContent.Children.Add(new TextBlock() { Text = "Attribute:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

            List<TreeNode> nodes = window.TransformedTree.GetChildrenRecursive();

            HashSet<string> attributes = new HashSet<string>();

            foreach (TreeNode node in nodes)
            {
                foreach (KeyValuePair<string, object> attribute in node.Attributes)
                {
                    attributes.Add(attribute.Key);
                }
            }

            List<string> sortedAttributes = new List<string>(attributes);
            sortedAttributes.Sort();

            ComboBox attributeBox = new ComboBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), Items = sortedAttributes, SelectedIndex = sortedAttributes.IndexOf("Name"), Background = Avalonia.Media.Brushes.White };
            Grid.SetColumn(attributeBox, 1);
            advancedContent.Children.Add(attributeBox);

            {
                TextBlock blk = new TextBlock() { Text = "Type:", Margin = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                advancedContent.Children.Add(blk);
            }

            ComboBox attributeTypeBox = new ComboBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), Items = new List<string>() { "String", "Number" }, SelectedIndex = 0, Background = Avalonia.Media.Brushes.White };
            Grid.SetColumn(attributeTypeBox, 3);
            advancedContent.Children.Add(attributeTypeBox);

            {
                TextBlock blk = new TextBlock() { Text = "Comparison:", Margin = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 4);
                advancedContent.Children.Add(blk);
            }

            List<string> stringComparisons = new List<string>() { "Normal", "Case-insensitive", "Culture-aware", "Culture-aware, case-insensitive" };
            List<string> numberComparisons = new List<string>() { "Equal", "Smaller than", "Greater than" };

            ComboBox comparisonTypeBox = new ComboBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), Items = stringComparisons, SelectedIndex = 1, Background = Avalonia.Media.Brushes.White };
            Grid.SetColumn(comparisonTypeBox, 5);
            advancedContent.Children.Add(comparisonTypeBox);

            CheckBox regexBox = new CheckBox() { Content = "Regex", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 75, Width = 75 };
            Grid.SetColumn(regexBox, 6);
            advancedContent.Children.Add(regexBox);

            Grid.SetColumn(searchBord, 2);
            Grid.SetColumnSpan(searchBord, Grid.GetColumnSpan(zom));
            Grid.SetRow(searchBord, 1);
            searchBord.Transitions = new Avalonia.Animation.Transitions();
            searchBord.Transitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Avalonia.Controls.Shapes.Path.OpacityProperty, Duration = new TimeSpan(5000000) });
            searchBord.Opacity = 1;
            window.FindControl<Grid>("MainGrid").Children.Add(searchBord);

            window.SetSelection(null);

            closeButton.PointerReleased += (s, e) =>
            {

                System.Threading.Thread thr = new System.Threading.Thread(async () =>
                {
                    System.Threading.Thread.Sleep(1000);

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        window.FindControl<Grid>("MainGrid").Children.Remove(searchBord);
                    });

                    stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = false;
                });
                thr.Start();

                searchBord.IsEnabled = false;
                searchBord.Opacity = 0;
            };

            attributeBox.SelectionChanged += (s, e) =>
            {
                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                if (window.TransformedTree.GetAttributeType(attribute) == "String")
                {
                    attributeTypeBox.SelectedIndex = 0;
                }
                else if (window.TransformedTree.GetAttributeType(attribute) == "Number")
                {
                    attributeTypeBox.SelectedIndex = 1;
                }
            };

            attributeTypeBox.SelectionChanged += (s, e) =>
            {
                if (attributeTypeBox.SelectedIndex == 0)
                {
                    comparisonTypeBox.Items = stringComparisons;
                    comparisonTypeBox.SelectedIndex = 0;
                }
                else if (attributeTypeBox.SelectedIndex == 1)
                {
                    comparisonTypeBox.Items = numberComparisons;
                    comparisonTypeBox.SelectedIndex = 0;
                }
            };

            void findAll()
            {
                window.ResetActionColours(true);

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;

                double numberNeedle = -1;

                if (attributeTypeIndex == 1 && !double.TryParse(needle, out numberNeedle))
                {
                    return;
                }

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }


                StringComparison comparison = StringComparison.InvariantCulture;
                RegexOptions options = RegexOptions.CultureInvariant;
                switch (comparisonType)
                {
                    case 0:
                        comparison = StringComparison.InvariantCulture;
                        options = RegexOptions.CultureInvariant;
                        break;
                    case 1:
                        comparison = StringComparison.InvariantCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;
                    case 2:
                        comparison = StringComparison.CurrentCulture;
                        options = RegexOptions.None;
                        break;
                    case 3:
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase;
                        break;
                }


                Regex reg = regex ? new Regex(needle, options) : null;

                Avalonia.Media.IBrush selectionChildBrush = window.SelectionChildBrush;
                List<string> matchedIds = new List<string>();
                foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                {
                    if (node.Attributes.TryGetValue(attribute, out object attributeValue))
                    {
                        if (attributeTypeIndex == 0 && attributeValue is string actualValue)
                        {
                            if (regex)
                            {
                                if (reg.IsMatch(actualValue))
                                {
                                    matchedIds.Add(node.Id);
                                }
                            }
                            else
                            {
                                if (actualValue.Contains(needle, comparison))
                                {
                                    matchedIds.Add(node.Id);
                                }
                            }
                        }
                        else if (attributeTypeIndex == 1 && attributeValue is double actualNumber)
                        {
                            switch (comparisonType)
                            {
                                case 0:
                                    if (actualNumber == numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                    }
                                    break;
                                case 1:
                                    if (actualNumber < numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                    }
                                    break;
                                case 2:
                                    if (actualNumber > numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                    }
                                    break;
                            }
                        }
                    }
                }

                /*foreach (Avalonia.Controls.Shapes.Path selPath in MainWindow.FindPaths(window.SelectionCanvas))
                {
                    if (selPath.Tag is object[] tag)
                    {
                        if (matchedIds.Contains((string)tag[0]))
                        {
                            selPath.ZIndex = 90;

                            Canvas can = selPath.Parent as Canvas;

                            while (can != window.SelectionCanvas)
                            {
                                can.ZIndex = 90;
                                can = can.Parent as Canvas;
                            }

                            if (selPath.Fill != null)
                            {
                                selPath.Fill = selectionChildBrush;
                            }

                            selPath.Stroke = selectionChildBrush;
                        }
                    }
                }*/
                /*foreach ((double, RenderAction) selPath in MainWindow.FindPaths(window.SelectionCanvas, null))
                {
                    if (matchedIds.Contains((string)selPath.Item2.Tag))
                    {
                        selPath.Item2.BringToFront();
                        Canvas can = selPath.Item2.Parent;

                        while (can != window.SelectionCanvas)
                        {
                            can.ZIndex = 90;
                            can = can.Parent as Canvas;
                        }

                        if (selPath.Item2.Fill != null)
                        {
                            window.ChangeActionFill(selPath.Item2, selectionChildBrush);
                        }

                        if (selPath.Item2.Stroke != null)
                        {
                            window.ChangeActionStroke(selPath.Item2, selectionChildBrush);
                        }
                    }
                }*/

                foreach (string id in matchedIds)
                {
                    foreach ((double, RenderAction) selPath in MainWindow.FindPaths(window.SelectionCanvas, id))
                    {
                        selPath.Item2.BringToFront();
                        Canvas can = selPath.Item2.Parent;
                        can.InvalidateVisual();

                        while (can != window.SelectionCanvas)
                        {
                            can.ZIndex = 90;
                            can = can.Parent as Canvas;
                        }

                        if (selPath.Item2.Fill != null)
                        {
                            window.ChangeActionFill(selPath.Item2, selectionChildBrush);
                        }

                        if (selPath.Item2.Stroke != null)
                        {
                            window.ChangeActionStroke(selPath.Item2, selectionChildBrush);
                        }
                    }
                }

                window.SelectionCanvas.InvalidateVisual();
            };

            findAllButton.Click += (s, e) =>
            {
                findAll();
            };

            searchBox.PropertyChanged += (s, e) =>
            {
                if (autoSearchBox.IsChecked == true && e.Property == TextBox.TextProperty)
                {
                    findAll();
                }
            };

            copyButton.Click += (s, e) =>
            {
                window.ResetActionColours(true);

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;

                double numberNeedle = -1;

                if (attributeTypeIndex == 1 && !double.TryParse(needle, out numberNeedle))
                {
                    return;
                }

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }


                StringComparison comparison = StringComparison.InvariantCulture;
                RegexOptions options = RegexOptions.CultureInvariant;
                switch (comparisonType)
                {
                    case 0:
                        comparison = StringComparison.InvariantCulture;
                        options = RegexOptions.CultureInvariant;
                        break;
                    case 1:
                        comparison = StringComparison.InvariantCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;
                    case 2:
                        comparison = StringComparison.CurrentCulture;
                        options = RegexOptions.None;
                        break;
                    case 3:
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase;
                        break;
                }


                Regex reg = regex ? new Regex(needle, options) : null;

                Avalonia.Media.IBrush selectionChildBrush = window.SelectionChildBrush;
                List<string> matchedIds = new List<string>();
                List<string> matchedNames = new List<string>();
                foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                {
                    if (node.Attributes.TryGetValue(attribute, out object attributeValue))
                    {
                        if (attributeTypeIndex == 0 && attributeValue is string actualValue)
                        {
                            if (regex)
                            {
                                if (reg.IsMatch(actualValue))
                                {
                                    matchedIds.Add(node.Id);
                                    if (!string.IsNullOrEmpty(node.Name))
                                    {
                                        matchedNames.Add(node.Name);
                                    }
                                }
                            }
                            else
                            {
                                if (actualValue.Contains(needle, comparison))
                                {
                                    matchedIds.Add(node.Id);
                                    if (!string.IsNullOrEmpty(node.Name))
                                    {
                                        matchedNames.Add(node.Name);
                                    }
                                }
                            }
                        }
                        else if (attributeTypeIndex == 1 && attributeValue is double actualNumber)
                        {
                            switch (comparisonType)
                            {
                                case 0:
                                    if (actualNumber == numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }
                                    }
                                    break;
                                case 1:
                                    if (actualNumber < numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }
                                    }
                                    break;
                                case 2:
                                    if (actualNumber > numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }
                                    }
                                    break;
                            }
                        }


                    }
                }

                foreach (string id in matchedIds)
                {
                    foreach ((double, RenderAction) selPath in MainWindow.FindPaths(window.SelectionCanvas, id))
                    {
                        selPath.Item2.BringToFront();
                        Canvas can = selPath.Item2.Parent;

                        while (can != window.SelectionCanvas)
                        {
                            can.ZIndex = 90;
                            can = can.Parent as Canvas;
                        }

                        if (selPath.Item2.Fill != null)
                        {
                            window.ChangeActionFill(selPath.Item2, selectionChildBrush);
                        }

                        if (selPath.Item2.Stroke != null)
                        {
                            window.ChangeActionStroke(selPath.Item2, selectionChildBrush);
                        }
                    }
                }

                window.SelectionCanvas.InvalidateVisual();


                if (matchedNames.Count > 0)
                {
                    _ = Avalonia.Application.Current.Clipboard.SetTextAsync(matchedNames.Aggregate((a, b) => a + "\n" + b));

                    Border bord = new Border() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(10, 10, 10, 20), Padding = new Avalonia.Thickness(50, 10), CornerRadius = new Avalonia.CornerRadius(10), Background = window.SelectionChildBrush, BorderBrush = window.SelectionBrush, BorderThickness = new Avalonia.Thickness(2) };
                    bord.Child = new TextBlock() { Text = "The " + matchedNames.Count.ToString() + " selected tips have been copied to the clipboard!" };
                    Grid.SetColumn(bord, 2);
                    Grid.SetColumnSpan(bord, Grid.GetColumnSpan(zom));
                    Grid.SetRow(bord, 1);
                    bord.Transitions = new Avalonia.Animation.Transitions();
                    bord.Transitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Avalonia.Controls.Shapes.Path.OpacityProperty, Duration = new TimeSpan(15000000) });
                    window.FindControl<Grid>("MainGrid").Children.Add(bord);

                    System.Threading.Thread thr = new System.Threading.Thread(async () =>
                    {
                        System.Threading.Thread.Sleep(1000);

                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            bord.Opacity = 0;
                        });

                        System.Threading.Thread.Sleep(2000);

                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            window.FindControl<Grid>("MainGrid").Children.Remove(bord);
                        });
                    });
                    thr.Start();
                }
            };

            replaceAllButton.Click += (s, e) =>
            {
                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;
                string replacement = replaceBox.Text;

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }

                Dictionary<string, object> parametersToChange = new Dictionary<string, object>()
                {
                    { "Attribute:", attribute },
                    { "Attribute type:", attributeTypeIndex == 0 ? "String" : "Number" },
                    { "Value:", needle },
                    { "Attribute: ", attribute },
                    { "Attribute type: ", attributeTypeIndex == 0 ? "String" : "Number" },
                    { "Value: ", replacement }
                };

                if (attributeTypeIndex == 0)
                {
                    parametersToChange.Add("Comparison type:", comparisonType);
                    parametersToChange.Add("Regex", regex);
                }
                else
                {
                    parametersToChange.Add("Comparison type: ", comparisonType);
                }

                parametersToChange.Add("Apply", true);


                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "f17160ad-0462-449a-8a57-e1af775c92ba");
                Action<Dictionary<string, object>> changeParameter = window.AddFurtherTransformation(module);

                changeParameter(parametersToChange);
                _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
            };

        }
    }
}
