using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp.Canvas;
using System.Linq;
using System.Threading;
using PhyloTree.Formats;
using System.Text;
using System.Text.Json;

namespace CompressedMemoryLoader
{
    /// <summary>
    /// This module loads the trees read from a tree file in memory in a compressed format. While the compression causes some
    /// performance degradation, this makes it possible to work with files containing a large number of trees without running
    /// into memory overflow issues.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// This module works by reading one tree at a time from the file and storing them in Binary format in memory. When a tree
    /// needs to be accessed, the Binary format tree in memory is parsed; when the tree is not necessary any more, the memory
    /// it used is discarded. Therefore, the amount of memory necessary to use this module is approximately equal to the size
    /// of the tree file in Binary format, plus the memory size of a single tree.
    /// 
    /// This module has high priority when the size of the file being read is greater than the [Large file threshold](#large-file-threshold).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Compressed memory loader";
        public const string HelpText = "Loads the tree in memory in a compressed format.\nSafe even when using large files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "3174e194-24a5-46f9-9836-b706cf0be326";
        public const ModuleTypes ModuleType = ModuleTypes.LoadFile;

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Large file threshold:">
                /// This global setting provides a file size threshold above which the priority of this module increases.
                /// If the file that is being opened is larger than this threshold, a dialog is also shown to the user,
                /// asking them if they want to read all the trees from the file or skip some of them. The value can be
                /// changed from the global settings window accessible from Edit > Preferences...
                /// 
                /// This setting affects the _Memory loader_, _Compressed memory loader_ and _Disk loader_ modules.
                /// </param>
                ("Large file threshold:", "FileSize:26214400"),
            };
        }

        public static double IsSupported(FileInfo fileInfo, string loaderModuleId, IEnumerable<TreeNode> treeLoader)
        {
            long largeFileThreshold = 26214400;

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Large file threshold:", out object largeFileThresholdValue))
            {
                if (largeFileThresholdValue is long threshValue)
                {
                    largeFileThreshold = threshValue;
                }
                else if (largeFileThresholdValue is JsonElement element)
                {
                    largeFileThreshold = element.GetInt64();
                }
            }

            if (fileInfo.Length > largeFileThreshold || treeLoader is TreeCollection)
            {
                return 0.75;
            }
            else
            {
                return 0.2;
            }
        }

        public static async Task<(TreeCollection, Action<double>)> Load(Avalonia.Controls.Window parentWindow, FileInfo fileInfo, string filetypeModuleId, IEnumerable<TreeNode> treeLoader, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> openerProgressAction, Action<double> progressAction)
        {
            long largeFileThreshold = 26214400;

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Large file threshold:", out object largeFileThresholdValue))
            {
                if (largeFileThresholdValue is long threshValue)
                {
                    largeFileThreshold = threshValue;
                }
                else if (largeFileThresholdValue is JsonElement element)
                {
                    largeFileThreshold = element.GetInt64();
                }
            }

            if (fileInfo.Length > largeFileThreshold)
            {
                bool result = false;
                int skip = 0;
                int every = 1;
                int until = -1;

                if (InstanceStateData.IsUIAvailable)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        ChildWindow settingsWindow = new ChildWindow() { Width = 450, SizeToContent = SizeToContent.Height, FontFamily = Avalonia.Media.FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans"), FontSize = 14, Title = "Skip trees...", WindowStartupLocation = WindowStartupLocation.CenterOwner };

                        Grid mainGrid = new Grid() { Margin = new Avalonia.Thickness(10) };
                        mainGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                        mainGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                        settingsWindow.Content = mainGrid;

                        StackPanel panel = new StackPanel();
                        mainGrid.Children.Add(panel);


                        Grid alertPanel = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                        alertPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        alertPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        alertPanel.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                        alertPanel.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                        Viewbox alert = MainWindow.GetAlertIcon();
                        alert.Width = 32;
                        alert.Height = 32;

                        alertPanel.Children.Add(alert);

                        TextBlock alertTitle = new TextBlock() { Text = "Large tree file", FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)), Margin = new Avalonia.Thickness(10, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        Grid.SetColumn(alertTitle, 1);
                        alertPanel.Children.Add(alertTitle);

                        TextBlock alertBlock = new TextBlock() { Text = "The file you are trying to open is very large (" + (fileInfo.Length / 1024 / 1024).ToString() + "MB). The trees are going to be loaded in compressed format, so there should not be any memory issues; however, to speed things up, you may want to skip some of the trees.", TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 5, 0, 0), FontSize = 13 };

                        Grid.SetRow(alertBlock, 1);
                        Grid.SetColumnSpan(alertBlock, 2);
                        alertPanel.Children.Add(alertBlock);
                        panel.Children.Add(alertPanel);

                        Grid skipPanel = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                        skipPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        skipPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        skipPanel.Children.Add(new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = "Trees to skip:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                        NumericUpDown skipNud = new NumericUpDown() { Minimum = 0, FormatString = "0", Value = 0, Padding = new Avalonia.Thickness(5, 0, 5, 0) };
                        Grid.SetColumn(skipNud, 1);
                        skipPanel.Children.Add(skipNud);
                        panel.Children.Add(skipPanel);

                        Grid everyPanel = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                        everyPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        everyPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        everyPanel.Children.Add(new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = "Sample a tree every:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                        NumericUpDown everyNud = new NumericUpDown() { Minimum = 1, FormatString = "0", Value = 1, Padding = new Avalonia.Thickness(5, 0, 5, 0) };
                        Grid.SetColumn(everyNud, 1);
                        everyPanel.Children.Add(everyNud);
                        panel.Children.Add(everyPanel);

                        Grid untilPanel = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                        untilPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        untilPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        CheckBox untilBox = new CheckBox() { FontWeight = Avalonia.Media.FontWeight.Bold, Content = "Up to tree:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        untilPanel.Children.Add(untilBox);
                        NumericUpDown untilNud = new NumericUpDown() { Minimum = 1, FormatString = "0", Value = 1, Padding = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        Grid.SetColumn(untilNud, 1);
                        untilPanel.Children.Add(untilNud);
                        panel.Children.Add(untilPanel);

                        Grid buttonPanel = new Grid();
                        buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        Button okButton = new Button() { Width = 100, Content = "OK", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
                        okButton.Classes.Add("PlainButton");
                        Grid.SetColumn(okButton, 1);
                        buttonPanel.Children.Add(okButton);
                        Button cancelButton = new Button() { Width = 100, Content = "Cancel", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
                        cancelButton.Classes.Add("PlainButton");
                        Grid.SetColumn(cancelButton, 3);
                        buttonPanel.Children.Add(cancelButton);
                        Grid.SetRow(buttonPanel, 1);
                        mainGrid.Children.Add(buttonPanel);


                        okButton.Click += (s, e) =>
                        {
                            result = true;
                            settingsWindow.Close();
                        };

                        cancelButton.Click += (s, e) =>
                        {
                            result = false;
                            settingsWindow.Close();
                        };

                        await settingsWindow.ShowDialog2(parentWindow);

                        skip = (int)Math.Round(skipNud.Value);
                        every = (int)Math.Round(everyNud.Value);
                        if (untilBox.IsChecked == true)
                        {
                            until = (int)Math.Round(untilNud.Value);
                        }
                    });
                }
                else
                {
                    result = true;
                }

                if (!result)
                {
                    return (null, openerProgressAction);
                }
                else
                {
                    MemoryStream ms = new MemoryStream();

                    if (until == -1)
                    {
                        if (!(treeLoader is TreeCollection))
                        {
                            openerProgressAction = (val) => { progressAction(val); };

                            BinaryTree.WriteAllTrees(treeLoader.Skip(skip).Where((item, index) => index % every == 0), ms, true);
                        }
                        else if (skip == 0 && every == 1)
                        {
                            openerProgressAction = (_) => { };

                            ((TreeCollection)treeLoader).UnderlyingStream.Seek(0, SeekOrigin.Begin);
                            ((TreeCollection)treeLoader).UnderlyingStream.CopyToWithProgress(ms, progressAction);
                        }
                        else
                        {
                            openerProgressAction = (_) => { };

                            double totalTrees = (((TreeCollection)treeLoader).Count - skip) / every;

                            BinaryTree.WriteAllTrees(treeLoader.Skip(skip).Where((item, index) => index % every == 0), ms, true, (count) =>
                            {
                                double progress = Math.Max(0, Math.Min(1, count / totalTrees));
                                progressAction(progress);
                            });
                        }

                    }
                    else
                    {
                        openerProgressAction = (_) => { };

                        double totalTrees = (until - skip) / every;

                        BinaryTree.WriteAllTrees(treeLoader.Take(until).Skip(skip).Where((item, index) => index % every == 0), ms, true, (count) =>
                        {
                            double progress = Math.Max(0, Math.Min(1, count / totalTrees));
                            progressAction(progress);
                        });
                    }

                    ms.Seek(0, SeekOrigin.Begin);

                    TreeCollection tbr = new TreeCollection(ms);

                    if (tbr[0].Children.Count > 2)
                    {
                        if (moduleSuggestions[1].Item1 == "68e25ec6-5911-4741-8547-317597e1b792" && moduleSuggestions[1].Item2.Count == 0)
                        {
                            moduleSuggestions[1] = ("95b61284-b870-48b9-b51c-3276f7d89df1", new Dictionary<string, object>());
                        }
                    }

                    if (treeLoader is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return (tbr, openerProgressAction);
                }
            }
            else
            {
                if (!(treeLoader is TreeCollection))
                {
                    openerProgressAction = (val) => { progressAction(val); };

                    MemoryStream ms = new MemoryStream();

                    BinaryTree.WriteAllTrees(treeLoader, ms, true);

                    ms.Seek(0, SeekOrigin.Begin);

                    TreeCollection tbr = new TreeCollection(ms);

                    if (tbr[0].Children.Count > 2)
                    {
                        if (moduleSuggestions[1].Item1 == "68e25ec6-5911-4741-8547-317597e1b792" && moduleSuggestions[1].Item2.Count == 0)
                        {
                            moduleSuggestions[1] = ("95b61284-b870-48b9-b51c-3276f7d89df1", new Dictionary<string, object>());
                        }
                    }

                    if (treeLoader is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return (tbr, openerProgressAction);
                }
                else
                {
                    openerProgressAction = (_) => { };

                    MemoryStream ms = new MemoryStream();

                    ((TreeCollection)treeLoader).UnderlyingStream.Seek(0, SeekOrigin.Begin);
                    ((TreeCollection)treeLoader).UnderlyingStream.CopyToWithProgress(ms, progressAction);

                    ms.Seek(0, SeekOrigin.Begin);

                    TreeCollection tbr = new TreeCollection(ms);

                    if (tbr[0].Children.Count > 2)
                    {
                        if (moduleSuggestions[1].Item1 == "68e25ec6-5911-4741-8547-317597e1b792" && moduleSuggestions[1].Item2.Count == 0)
                        {
                            moduleSuggestions[1] = ("95b61284-b870-48b9-b51c-3276f7d89df1", new Dictionary<string, object>());
                        }
                    }

                    if (treeLoader is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return (tbr, openerProgressAction);
                }
            }
        }
    }
}
