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
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp.Canvas;
using System.Linq;
using System.Threading;
using System.Text.Json;

namespace MemoryLoader
{
    /// <summary>
    /// This module loads all the trees that were read from the tree file into memory. This results in the best performance for accessing trees;
    /// however, if the tree file is too large, it could cause memory overflow problems.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// This module has the highest priority of all the Load file modules when the size of the file is smaller than the [Large file threshold](#large-file-threshold).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Memory loader";
        public const string HelpText = "Loads all the trees from the file into memory.\nHuge files may cause the program to run out of memory.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.2");
        public const string Id = "a22ff194-c486-4215-a4bf-7a006d6f88fa";
        public const ModuleTypes ModuleType = ModuleTypes.LoadFile;

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Large file threshold:">
                /// This global setting provides a file size threshold below which this module has high priority. If the file that is being
                /// opened is larger than this threshold, a dialog is also shown to the user, asking them if they want to read all the trees 
                /// from the file or skip some of them. The value can be changed from the global settings window accessible from Edit > Preferences...
                /// 
                /// This setting affects the _Memory loader_, _Compressed memory loader_ and _Disk loader_ modules.
                /// </param>
                ("Large file threshold:", "FileSize:26214400")
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

            if (fileInfo.Length <= largeFileThreshold && !(treeLoader is TreeCollection))
            {
                return 0.5;
            }
            else if (fileInfo.Length <= largeFileThreshold)
            {
                return 0.8;
            }
            else
            {
                return 0.1;
            }
        }

        public static async Task<(TreeCollection, Action<double>)> Load(Window parentWindow, FileInfo fileInfo, string loaderModuleId, IEnumerable<TreeNode> treeLoader, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> openerProgressAction, Action<double> progressAction)
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

                        TextBlock alertBlock = new TextBlock() { Text = "The file you are trying to open is very large (" + (fileInfo.Length / 1024 / 1024).ToString() + "MB). The trees are going to be loaded into memory, which may cause the program to run out of memory. To reduce memory pressure, you may want to skip some of the trees. You may also click 'Cancel' to close this window and then used the 'Advanced open...' menu option to specify the Disk loader.", TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 5, 0, 0), FontSize = 13 };

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
                    if (treeLoader is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }

                    return (null, openerProgressAction);
                }
                else
                {

                    if (until == -1)
                    {
                        openerProgressAction = (val) => { progressAction(val); };

                        if (skip == 0 && every == 1)
                        {
                            TreeCollection tbr = new TreeCollection(treeLoader as List<TreeNode> ?? treeLoader.ToList());

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
                            TreeCollection tbr = new TreeCollection(treeLoader.Skip(skip).Where((item, index) => index % every == 0).ToList());

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
                        openerProgressAction = (_) => { };

                        double totalTrees = (until - skip) / every;

                        List<TreeNode> trees = new List<TreeNode>((until - skip) / every + 1);

                        foreach (TreeNode tree in treeLoader.Take(until).Skip(skip).Where((item, index) => index % every == 0))
                        {
                            trees.Add(tree);
                            double progress = Math.Max(0, Math.Min(1, trees.Count / totalTrees));
                            progressAction(progress);
                        }

                        TreeCollection tbr = new TreeCollection(trees);

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
            else
            {
                openerProgressAction = (val) => { progressAction(val); };

                TreeCollection tbr = new TreeCollection(treeLoader as List<TreeNode> ?? treeLoader.ToList());

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

