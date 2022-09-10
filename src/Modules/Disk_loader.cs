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
using System.Text.Json;

namespace DiskLoader
{
    /// <summary>
    /// Rather than loading the tree file in memory, this module reads it from the disk. This causes a further performance
    /// reduction, even when compared to the _Compressed memory loader_, but makes it possible to work with _huge_ files.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// If the tree file that has been opened is in Binary format, it is read directly from the disk. Otherwise, it is
    /// converted to Binary format in a temporary file (the compression is streamed, so that no more than one tree is loaded
    /// in memory at any one time). When a tree needs to be accessed, it is read from the file on disk. Therefore, the amount
    /// of memory necessary to use this module is approximately equal to the memory size of a single tree.
    /// 
    /// This module has a high priority when the size of the file being read is greater than the [Huge file threshold](#huge-file-threshold).
    /// </description>
    public static class MyModule
    {
        public const string Name = "Disk loader";
        public const string HelpText = "Lazily accesses the trees from disk.\nSafe even when using huge files.\nIf the input file is not in binary tree format, it will be converted to a temporary file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "71727eb5-550d-435e-8e9b-37606d3b0a4e";
        public const ModuleTypes ModuleType = ModuleTypes.LoadFile;

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Large file threshold:">
                /// This global setting provides a file size threshold. If the file that is being opened is larger than
                /// this threshold, a dialog is also shown to the user, asking them if they want to read all the trees 
                /// from the file or skip some of them. The value can be changed from the global settings window accessible
                /// from Edit > Preferences...
                /// 
                /// This setting affects the _Memory loader_, _Compressed memory loader_ and _Disk loader_ modules.
                /// </param>
                ("Large file threshold:", "FileSize:26214400"),
                
                /// <param name="Huge file threshold:">
                /// This global setting provides a file size threshold above which the priority of this module increases.
                /// The value can be changed from the global settings window accessible from Edit > Preferences...
                /// </param>
                ("Huge file threshold:", "FileSize:1073741824"),
            };
        }

        public static double IsSupported(FileInfo fileInfo, string loaderModuleId, IEnumerable<TreeNode> treeLoader)
        {
            long hugeFileThreshold = 1073741824;

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Huge file threshold:", out object hugeFileThresholdValue))
            {
                if (hugeFileThresholdValue is long threshValue)
				{
					hugeFileThreshold = threshValue;
				}
				else if (hugeFileThresholdValue is JsonElement element)
				{
					hugeFileThreshold = element.GetInt64();
				}
            }

            if (fileInfo.Length > hugeFileThreshold)
            {
                return 0.8;
            }
            else if (treeLoader is TreeCollection)
            {
                return 0.5;
            }
            else
            {
                return 0.05;
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

            if (treeLoader is TreeCollection coll)
            {
                openerProgressAction = (val) => { progressAction(val); };

                if (coll[0].Children.Count > 2)
                {
                    if (moduleSuggestions[1].Item1 == "68e25ec6-5911-4741-8547-317597e1b792" && moduleSuggestions[1].Item2.Count == 0)
                    {
                        moduleSuggestions[1] = ("95b61284-b870-48b9-b51c-3276f7d89df1", new Dictionary<string, object>());
                    }
                }

                return (coll, openerProgressAction);
            }
            else
            {
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
                            Window settingsWindow = new Window() { Width = 450, Height = 300, FontFamily = Avalonia.Media.FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans"), FontSize = 15, Title = "Skip trees...", WindowStartupLocation = WindowStartupLocation.CenterOwner };

                            Grid mainGrid = new Grid() { Margin = new Avalonia.Thickness(10) };
                            mainGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                            mainGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                            settingsWindow.Content = mainGrid;

                            StackPanel panel = new StackPanel();
                            mainGrid.Children.Add(panel);


                            Grid alertPanel = new Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                            alertPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            alertPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            alertPanel.Children.Add(MainWindow.GetAlertIcon());
                            TextBlock alertBlock = new TextBlock() { Text = "The file you are trying to open is very large (" + (fileInfo.Length / 1024 / 1024).ToString() + "MB). The trees are going to be read from the disk, so there should not be any memory issues; however, the input file needs to be converted to a binary format: to speed things up, you may want to skip some of the trees.", TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(5, 0, 0, 0) };

                            Grid.SetColumn(alertBlock, 1);
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
                            NumericUpDown untilNud = new NumericUpDown() { Minimum = 1, FormatString = "0", Value = 1, Padding = new Avalonia.Thickness(5, 0, 5, 0) };
                            Grid.SetColumn(untilNud, 1);
                            untilPanel.Children.Add(untilNud);
                            panel.Children.Add(untilPanel);

                            Grid buttonPanel = new Grid();
                            buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            buttonPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            Button okButton = new Button() { Width = 100, Content = "OK" };
                            Grid.SetColumn(okButton, 1);
                            buttonPanel.Children.Add(okButton);
                            Button cancelButton = new Button() { Width = 100, Content = "Cancel" };
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
                        string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                        using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                        {
                            if (until == -1)
                            {
                                openerProgressAction = (val) => { progressAction(val); };

                                BinaryTree.WriteAllTrees(treeLoader.Skip(skip).Where((item, index) => index % every == 0), fs);
                            }
                            else
                            {
                                openerProgressAction = (_) => { };

                                double totalTrees = (until - skip) / every;

                                BinaryTree.WriteAllTrees(treeLoader.Take(until).Skip(skip).Where((item, index) => index % every == 0), fs, false, (count) =>
                                {
                                    double progress = Math.Max(0, Math.Min(1, count / totalTrees));
                                    progressAction(progress);
                                });
                            }
                        }

                        FileStream readerFs = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read);

                        TreeCollection tbr = new TreeCollection(readerFs) { TemporaryFile = tempFile };

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
                    openerProgressAction = (val) => { progressAction(val); };

                    string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

                    using (FileStream fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        BinaryTree.WriteAllTrees(treeLoader, fs);
                    }

                    FileStream readerFs = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    TreeCollection tbr = new TreeCollection(readerFs) { TemporaryFile = tempFile };

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
