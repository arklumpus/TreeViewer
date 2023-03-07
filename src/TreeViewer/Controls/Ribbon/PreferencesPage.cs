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
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.VisualTree;
using AvaloniaColorPicker;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace TreeViewer
{
    class PreferencesPage : RibbonFilePageContentTemplate
    {
        public PreferencesPage() : base("Preferences")
        {
            BuildInterface();

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == PreferencesPage.IsVisibleProperty)
                {
                    if ((bool)e.NewValue)
                    {
                        this.BuildInterface();
                    }
                }
            };
        }

        private void BuildInterface()
        {

            Grid pageContent = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            pageContent.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto) { MinWidth = 250, MaxWidth = 350 });
            pageContent.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            ScrollViewer mainScrollViewer = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, AllowAutoHide = false, Padding = new Thickness(0, 0, 17, 0) };
            mainScrollViewer.Content = pageContent;

            DockPanel parentContainer = new DockPanel() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Width = 700, LastChildFill = true, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

            this.PageContent = parentContainer;

            List<Func<bool>> applyChanges = new List<Func<bool>>();

            int currRow = 0;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Interface style:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                List<string> items = new List<string>() { "Windows", "macOS" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = (int)GlobalSettings.Settings.InterfaceStyle, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                Grid.SetRow(comboBox, currRow);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(comboBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    bool hasChanged = (GlobalSettings.Settings.InterfaceStyle != (GlobalSettings.InterfaceStyles)comboBox.SelectedIndex);

                    GlobalSettings.Settings.InterfaceStyle = (GlobalSettings.InterfaceStyles)comboBox.SelectedIndex;
                    return hasChanged;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Ribbon style:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                List<string> items = new List<string>() { "Colourful", "Grey" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = (int)GlobalSettings.Settings.RibbonStyle, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                Grid.SetRow(comboBox, currRow);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(comboBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    bool hasChanged = (GlobalSettings.Settings.RibbonStyle != (GlobalSettings.RibbonStyles)comboBox.SelectedIndex);

                    GlobalSettings.Settings.RibbonStyle = (GlobalSettings.RibbonStyles)comboBox.SelectedIndex;
                    return hasChanged;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Autosave frequency (minutes):", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                NumericUpDown autosaveTimerNud = new NumericUpDown() { MinWidth = 200, Value = GlobalSettings.Settings.AutosaveInterval.TotalMinutes, Minimum = 1, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 14 };

                Grid.SetRow(blk, currRow);
                Grid.SetColumn(autosaveTimerNud, 1);
                Grid.SetRow(autosaveTimerNud, currRow);
                pageContent.Children.Add(autosaveTimerNud);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.AutosaveInterval = TimeSpan.FromMinutes(autosaveTimerNud.Value);

                    foreach (MainWindow win in GlobalSettings.Settings.MainWindows)
                    {
                        lock (win.AutosaveLock)
                        {
                            win.AutosaveTimer.Stop();
                            win.AutosaveTimer.Interval = GlobalSettings.Settings.AutosaveInterval;
                            win.AutosaveTimer.Start();
                        }
                    }

                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Keep recent files for (days):", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                NumericUpDown recentFilesNud = new NumericUpDown() { MinWidth = 200, Value = GlobalSettings.Settings.KeepRecentFilesFor, Minimum = 1, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 14, FormatString = "{0:0}" };

                Grid.SetRow(blk, currRow);
                Grid.SetColumn(recentFilesNud, 1);
                Grid.SetRow(recentFilesNud, currRow);
                pageContent.Children.Add(recentFilesNud);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.KeepRecentFilesFor = (int)Math.Round(recentFilesNud.Value);

                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                CheckBox checkBox = new CheckBox() { Margin = new Thickness(0, 0, 0, 10), Content = "Draw simple trees after opening them", IsChecked = GlobalSettings.Settings.DrawTreeWhenOpened, FontSize = 14 };
                Grid.SetColumnSpan(checkBox, 2);
                Grid.SetRow(checkBox, currRow);
                pageContent.Children.Add(checkBox);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.DrawTreeWhenOpened = checkBox.IsChecked == true;
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                CheckBox checkBox = new CheckBox() { Margin = new Thickness(0, 0, 0, 10), Content = "Show legacy up/down arrows to move modules", IsChecked = GlobalSettings.Settings.ShowLegacyUpDownArrows, FontSize = 14 };
                Grid.SetColumnSpan(checkBox, 2);
                Grid.SetRow(checkBox, currRow);
                pageContent.Children.Add(checkBox);

                applyChanges.Add(() =>
                {
                    bool hasChanged = GlobalSettings.Settings.ShowLegacyUpDownArrows != (checkBox.IsChecked == true);

                    GlobalSettings.Settings.ShowLegacyUpDownArrows = checkBox.IsChecked == true;
                    return hasChanged;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Dragging start interval (ms):", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                NumericUpDown dragIntervalNud = new NumericUpDown() { MinWidth = 200, Value = GlobalSettings.Settings.DragInterval, Minimum = 1, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 14, FormatString = "0" };

                Grid.SetRow(blk, currRow);
                Grid.SetColumn(dragIntervalNud, 1);
                Grid.SetRow(dragIntervalNud, currRow);
                pageContent.Children.Add(dragIntervalNud);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.DragInterval = (int)dragIntervalNud.Value;
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Selection colour:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                ColorButton colorButton = new ColorButton() { Color = ((VectSharp.Colour)GlobalSettings.Settings.SelectionColour).ToAvalonia(), Margin = new Thickness(0, 0, 0, 10), FontSize = 14 };
                colorButton.Classes.Add("PlainButton");
                Grid.SetRow(colorButton, currRow);
                Grid.SetColumn(colorButton, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(colorButton);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.SelectionColour = colorButton.Color.ToVectSharp();
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                CheckBox checkBox = new CheckBox() { Margin = new Thickness(0, 0, 0, 10), Content = "Enable undo/redo stack", IsChecked = GlobalSettings.Settings.EnableUndoStack, FontSize = 14 };
                Grid.SetColumnSpan(checkBox, 2);
                Grid.SetRow(checkBox, currRow);
                pageContent.Children.Add(checkBox);

                applyChanges.Add(() =>
                {
                    bool hasChanged = GlobalSettings.Settings.EnableUndoStack != (checkBox.IsChecked == true);

                    GlobalSettings.Settings.EnableUndoStack = checkBox.IsChecked == true;
                    return hasChanged;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Tree comparisons:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                List<string> items = new List<string>() { "Pairwise", "Globally shared tips" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = GlobalSettings.Settings.PairwiseTreeComparisons ? 0 : 1, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                Grid.SetRow(comboBox, currRow);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(comboBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.PairwiseTreeComparisons = (comboBox.SelectedIndex == 0);
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Tree clustering metric:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                List<string> items = new List<string>() { "Raw distances", "2D metric" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = GlobalSettings.Settings.ClusterAccordingToRawDistances ? 0 : 1, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                Grid.SetRow(comboBox, currRow);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(comboBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.ClusterAccordingToRawDistances = (comboBox.SelectedIndex == 0);
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Module repository:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };
                TextBox textBox = new TextBox() { Text = GlobalSettings.Settings.ModuleRepositoryBaseUri, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14, Padding = new Thickness(5, 2, 5, 2), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                Grid.SetRow(textBox, currRow);
                Grid.SetColumn(textBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(textBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.ModuleRepositoryBaseUri = textBox.Text;
                    return false;
                });
            }

            currRow++;
            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                TextBlock blk = new TextBlock() { Text = "Check for updates:", Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                List<string> items = new List<string>() { "Do not check", "Only for TreeViewer", "For TreeViewer and any installed module", "For TreeViewer and all modules" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = (int)GlobalSettings.Settings.UpdateCheckMode, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                Grid.SetRow(comboBox, currRow);
                Grid.SetColumn(comboBox, 1);
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(comboBox);
                pageContent.Children.Add(blk);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.UpdateCheckMode = (GlobalSettings.UpdateCheckModes)comboBox.SelectedIndex;
                    return false;
                });
            }

            currRow++;

            pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            {
                Button button = new Button() { Margin = new Thickness(0, 0, 0, 10), Content = "Restore all dismissed module messages", FontSize = 14 };
                button.Classes.Add("PlainButton");
                Grid.SetColumnSpan(button, 2);
                Grid.SetRow(button, currRow);
                pageContent.Children.Add(button);

                button.Click += (s, e) =>
                {
                    GlobalSettings.Settings.CurrentlyDismissedMessages.Clear();
                    GlobalSettings.Settings.PermanentlyDismissedMessages.Clear();
                    GlobalSettings.SaveSettings();
                };
            }

            currRow++;

            foreach (KeyValuePair<string, string> kvp in GlobalSettings.Settings.AdditionalSettingsList)
            {
                pageContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                AddParameter(kvp.Key, kvp.Value, applyChanges, pageContent, currRow);

                currRow++;
            }

            Grid buttonGrid = new Grid() { Margin = new Thickness(0, 10, 0, 0) };
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            Button OKButton = new Button() { Content = "OK", FontSize = 14, Width = 100, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            OKButton.Classes.Add("PlainButton");
            Grid.SetColumn(OKButton, 1);
            buttonGrid.Children.Add(OKButton);

            Button CancelButton = new Button() { Content = "Cancel", FontSize = 14, Width = 100, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            CancelButton.Classes.Add("PlainButton");
            Grid.SetColumn(CancelButton, 3);
            buttonGrid.Children.Add(CancelButton);

            Button ResetButton = new Button() { Content = "Reset all to default", FontSize = 14, Width = 150, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            ResetButton.Classes.Add("PlainButton");
            Grid.SetColumn(ResetButton, 5);
            buttonGrid.Children.Add(ResetButton);

            DockPanel.SetDock(buttonGrid, Dock.Bottom);
            parentContainer.Children.Add(buttonGrid);

            parentContainer.Children.Add(mainScrollViewer);

            CancelButton.Click += (s, e) =>
            {
                this.FindAncestorOfType<RibbonFilePage>().Close();
            };

            OKButton.Click += async (s, e) =>
            {
                bool needsReboot = false;

                foreach (Func<bool> act in applyChanges)
                {
                    if (act())
                    {
                        needsReboot = true;
                    }
                }

                GlobalSettings.SaveSettings();

                if (needsReboot)
                {
                    MessageBox box = new MessageBox("Question", "The program needs to be rebooted to fully apply the requested changes.\nDo you want to reboot now (we will do our best to recover the files that are currently open)?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box.ShowDialog2(this.FindAncestorOfType<Window>());

                    if (box.Result == MessageBox.Results.Yes)
                    {
                        Program.Reboot(new string[] { }, true);

                        if (!Modules.IsMac)
                        {
                            ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                        }
                        else
                        {
                            System.Diagnostics.Process.GetCurrentProcess().Kill();
                        }
                    }
                    else
                    {
                        this.FindAncestorOfType<RibbonFilePage>().Close();
                    }
                }
                else
                {
                    this.FindAncestorOfType<RibbonFilePage>().Close();
                }
            };

            ResetButton.Click += (s, e) =>
            {
                GlobalSettings.Settings.AdditionalSettings.Clear();

                GlobalSettings.Settings.AutosaveInterval = new TimeSpan(0, 10, 0);

                foreach (MainWindow win in GlobalSettings.Settings.MainWindows)
                {
                    lock (win.AutosaveLock)
                    {
                        win.AutosaveTimer.Stop();
                        win.AutosaveTimer.Interval = GlobalSettings.Settings.AutosaveInterval;
                        win.AutosaveTimer.Start();
                    }
                }

                GlobalSettings.Settings.DrawTreeWhenOpened = true;
                GlobalSettings.Settings.ShowLegacyUpDownArrows = false;
                GlobalSettings.Settings.EnableUndoStack = true;
                GlobalSettings.Settings.ModuleRepositoryBaseUri = GlobalSettings.DefaultModuleRepository;
                GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramAndAllModules;
                GlobalSettings.Settings.DragInterval = 250;
                GlobalSettings.Settings.InterfaceStyle = Modules.IsMac ? GlobalSettings.InterfaceStyles.MacOSStyle : GlobalSettings.InterfaceStyles.WindowsStyle;
                GlobalSettings.Settings.RibbonStyle = Modules.IsMac ? GlobalSettings.RibbonStyles.Grey : GlobalSettings.RibbonStyles.Colourful;
                GlobalSettings.Settings.PairwiseTreeComparisons = false;
                GlobalSettings.Settings.ClusterAccordingToRawDistances = false;

                GlobalSettings.Settings.SelectionColour = VectSharp.Colour.FromRgb(35, 127, 255);

                GlobalSettings.Settings.UpdateAdditionalSettings();
                GlobalSettings.SaveSettings();

                BuildInterface();
            };
        }


        private void AddParameter(string name, string data, List<Func<bool>> applyChanges, Grid pageContent, int currRow)
        {
            string controlType = data.Substring(0, data.IndexOf(":"));
            string controlParameters = data.Substring(data.IndexOf(":") + 1);

            if (controlType == "CheckBox")
            {
                string parameterName = name;

                bool defaultValue = Convert.ToBoolean(controlParameters);

                if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                {
                    GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultValue);
                }
                else
                {
                    if (valueObject is JsonElement element)
                    {
                        defaultValue = element.GetBoolean();
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultValue;
                    }
                    else
                    {
                        defaultValue = (bool)valueObject;
                    }
                }

                CheckBox control = new CheckBox() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Thickness(0, 0, 0, 10), IsChecked = defaultValue, FontSize = 14 };

                Grid.SetRow(control, currRow);
                Grid.SetColumnSpan(control, 2);
                pageContent.Children.Add(control);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.AdditionalSettings[parameterName] = control.IsChecked == true;
                    return false;
                });
            }
            else
            {
                string parameterName = name;

                TextBlock blk = new TextBlock() { Text = parameterName, Margin = new Thickness(0, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };
                Grid.SetRow(blk, currRow);
                pageContent.Children.Add(blk);

                if (controlType == "ComboBox")
                {
                    int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                    controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                    List<string> items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(controlParameters, Modules.DefaultSerializationOptions);

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultIndex);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            defaultIndex = element.GetInt32();
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultIndex;
                        }
                        else
                        {
                            defaultIndex = (int)valueObject;
                        }
                    }

                    ComboBox box = new ComboBox() { Margin = new Thickness(0, 0, 0, 10), Items = items, SelectedIndex = defaultIndex, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14 };

                    Grid.SetRow(box, currRow);
                    Grid.SetColumn(box, 1);
                    pageContent.Children.Add(box);


                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = box.SelectedIndex;
                        return false;
                    });
                }
                else if (controlType == "TextBox")
                {
                    string defaultValue = controlParameters;

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultValue);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            defaultValue = element.GetString();
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultValue;
                        }
                        else
                        {
                            defaultValue = (string)valueObject;
                        }
                    }

                    TextBox box = new TextBox() { Margin = new Thickness(0, 0, 0, 10), Padding = new Thickness(5, 2, 5, 2), Text = defaultValue, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                    Grid.SetRow(box, currRow);
                    Grid.SetColumn(box, 1);
                    pageContent.Children.Add(box);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = box.Text;
                        return false;
                    });
                }
                else if (controlType == "NumericUpDown")
                {
                    double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                    controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                    string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                    double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                    double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                    double increment = (maxRange - minRange) * 0.01;

                    if (range.Length > 2)
                    {
                        increment = double.Parse(range[2], System.Globalization.CultureInfo.InvariantCulture);
                    }

                    if (double.IsNaN(increment) || double.IsInfinity(increment))
                    {
                        increment = 1;
                    }

                    string formatString = Extensions.GetFormatString(increment);

                    if (range.Length > 3)
                    {
                        formatString = range[3];
                    }

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultValue);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            defaultValue = element.GetDouble();
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultValue;
                        }
                        else
                        {
                            defaultValue = (double)valueObject;
                        }
                    }

                    NumericUpDown nud = new NumericUpDown() { Margin = new Thickness(0, 0, 0, 10), Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = formatString, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 200, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                    Grid.SetRow(nud, currRow);
                    Grid.SetColumn(nud, 1);
                    pageContent.Children.Add(nud);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = nud.Value;
                        return false;
                    });
                }
                else if (controlType == "FileSize")
                {
                    long defaultValue = long.Parse(controlParameters);

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultValue);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            defaultValue = element.GetInt64();
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultValue;
                        }
                        else
                        {
                            defaultValue = (long)valueObject;
                        }
                    }

                    FileSizeControl.FileSizeUnit unit;

                    if (defaultValue < 1024)
                    {
                        unit = FileSizeControl.FileSizeUnit.B;
                    }
                    else
                    {
                        double longSize = defaultValue / 1024.0;

                        if (longSize < 1024)
                        {
                            unit = FileSizeControl.FileSizeUnit.kiB;
                        }
                        else
                        {
                            longSize /= 1024.0;

                            if (longSize < 1024)
                            {
                                unit = FileSizeControl.FileSizeUnit.MiB;
                            }
                            else
                            {
                                longSize /= 1024.0;
                                unit = FileSizeControl.FileSizeUnit.GiB;
                            }
                        }
                    }

                    FileSizeControl control = new FileSizeControl() { Margin = new Thickness(0, 0, 0, 10), Value = defaultValue, Unit = unit, MinWidth = 200, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                    Grid.SetRow(control, currRow);
                    Grid.SetColumn(control, 1);
                    pageContent.Children.Add(control);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = control.Value;
                        return false;
                    });
                }
                else if (controlType == "Slider")
                {
                    double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                    controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                    string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                    double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                    double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                    double increment = (maxRange - minRange) * 0.01;

                    if (double.IsNaN(increment) || double.IsInfinity(increment))
                    {
                        increment = 1;
                    }

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, defaultValue);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            defaultValue = element.GetDouble();
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = defaultValue;
                        }
                        else
                        {
                            defaultValue = (double)valueObject;
                        }
                    }

                    Grid sliderGrid = new Grid();
                    sliderGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    sliderGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    Grid.SetRow(sliderGrid, currRow);
                    Grid.SetColumn(sliderGrid, 1);
                    pageContent.Children.Add(sliderGrid);

                    Slider slid = new Slider() { Margin = new Thickness(0, 0, 0, 10), Minimum = minRange, Maximum = maxRange, Value = defaultValue, LargeChange = increment, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };
                    sliderGrid.Children.Add(slid);


                    NumericUpDown valueBlock = null;

                    if (range.Length > 2)
                    {
                        valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 10), Value = slid.Value, FormatString = range[2], Minimum = minRange, Maximum = maxRange, Increment = increment, MinWidth = 100, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                        Grid.SetColumn(valueBlock, 1);
                        sliderGrid.Children.Add(valueBlock);
                    }
                    else
                    {
                        valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 10), Value = slid.Value, FormatString = Extensions.GetFormatString(increment), Minimum = minRange, Maximum = maxRange, Increment = increment, MinWidth = 100, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                        Grid.SetColumn(valueBlock, 1);
                        sliderGrid.Children.Add(valueBlock);
                    }

                    slid.PropertyChanged += (s, e) =>
                    {
                        if (e.Property == Slider.ValueProperty)
                        {
                            valueBlock.Value = slid.Value;
                        }
                    };


                    if (valueBlock != null)
                    {
                        valueBlock.ValueChanged += (s, e) =>
                        {
                            slid.Value = valueBlock.Value;
                        };
                    }

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = valueBlock.Value;
                        return false;
                    });
                }
                else if (controlType == "Font")
                {
                    string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                    VectSharp.Font fnt = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, fnt);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            font = element.GetString().Split(',');
                            fnt = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = fnt;
                        }
                        else
                        {
                            fnt = (VectSharp.Font)valueObject;
                        }
                    }

                    FontButton but = new FontButton(false) { FontSize = 14, Font = fnt, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                    Grid.SetRow(but, currRow);
                    Grid.SetColumn(but, 1);
                    pageContent.Children.Add(but);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = but.Font;
                        return false;
                    });
                }
                else if (controlType == "Point")
                {
                    double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                    Grid grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    VectSharp.Point pt = new VectSharp.Point(point[0], point[1]);

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, pt);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            string str = element.GetString();
                            string[] pointStr = str.Split(',');

                            pt = new VectSharp.Point(double.Parse(pointStr[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(pointStr[1], System.Globalization.CultureInfo.InvariantCulture));

                            GlobalSettings.Settings.AdditionalSettings[parameterName] = pt;
                        }
                        else
                        {
                            pt = (VectSharp.Point)valueObject;
                        }
                    }

                    NumericUpDown nudX = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 10), Increment = 1, Value = pt.X, FormatString = Extensions.GetFormatString(point[0]), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, MinWidth = 100 };
                    NumericUpDown nudY = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 10), Increment = 1, Value = pt.Y, FormatString = Extensions.GetFormatString(point[1]), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, MinWidth = 100 };

                    TextBlock blkX = new TextBlock() { Text = "X:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 10), FontSize = 14 };
                    TextBlock blkY = new TextBlock() { Text = "Y:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 10), FontSize = 14 };

                    Grid.SetColumn(blkY, 2);

                    Grid.SetColumn(grid, 1);

                    Grid.SetColumn(nudX, 1);
                    Grid.SetColumn(nudY, 3);

                    grid.Children.Add(blkX);
                    grid.Children.Add(nudX);
                    grid.Children.Add(blkY);
                    grid.Children.Add(nudY);

                    Grid.SetRow(grid, currRow);
                    Grid.SetColumn(grid, 1);
                    pageContent.Children.Add(grid);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);
                        return false;
                    });
                }
                else if (controlType == "Colour")
                {
                    int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters, Modules.DefaultSerializationOptions);

                    VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, col);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            col = VectSharp.Colour.FromCSSString(element.GetString()) ?? VectSharp.Colour.FromRgba(0, 0, 0, 0);
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = col;
                        }
                        else
                        {
                            col = (VectSharp.Colour)valueObject;
                        }
                    }

                    ColorButton but = new ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(0, 0, 0, 10), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 14, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    but.Classes.Add("PlainButton");
                    Grid.SetRow(but, currRow);
                    Grid.SetColumn(but, 1);
                    pageContent.Children.Add(but);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = but.Color.ToVectSharp();
                        return false;
                    });
                }
                else if (controlType == "Dash")
                {
                    double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                    VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, lineDash);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            string str = element.GetString();
                            string[] dashStr = str.Split(',');

                            lineDash = new VectSharp.LineDash(double.Parse(dashStr[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dashStr[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dashStr[2], System.Globalization.CultureInfo.InvariantCulture));
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = lineDash;
                        }
                        else
                        {
                            lineDash = (VectSharp.LineDash)valueObject;
                        }
                    }

                    DashControl control = new DashControl() { LineDash = lineDash, Margin = new Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 14 };

                    Grid.SetRow(control, currRow);
                    Grid.SetColumn(control, 1);
                    pageContent.Children.Add(control);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = control.LineDash;
                        return false;
                    });
                }
            }
        }
    }
}
