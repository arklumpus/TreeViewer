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
using System;
using System.Collections.Generic;
using System.Text.Json;
using VectSharp;

namespace TreeViewer
{
    public class GlobalSettingsWindow : Window
    {
        internal bool Disposing = false;

        public GlobalSettingsWindow()
        {
            this.InitializeComponent();

            this.Closing += (s, e) =>
            {
                if (!Disposing)
                {
                    this.Hide();
                    e.Cancel = true;
                }
            };

            List<Action> applyChanges = new List<Action>();

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                container.Children.Add(new TextBlock() { Text = "Autosave frequency (minutes):", Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                NumericUpDown autosaveTimerNud = new NumericUpDown() { Width = 150, Value = GlobalSettings.Settings.AutosaveInterval.TotalMinutes, Minimum = 1, Margin = new Thickness(0, 5, 5, 5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                container.Children.Add(autosaveTimerNud);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

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
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                CheckBox checkBox = new CheckBox() { Margin = new Thickness(5), Content = "Draw simple trees after opening them", IsChecked = GlobalSettings.Settings.DrawTreeWhenOpened };
                container.Children.Add(checkBox);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.DrawTreeWhenOpened = checkBox.IsChecked == true;
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                CheckBox checkBox = new CheckBox() { Margin = new Thickness(5), Content = "Show legacy up/down arrows to move modules", IsChecked = GlobalSettings.Settings.ShowLegacyUpDownArrows };
                container.Children.Add(checkBox);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.ShowLegacyUpDownArrows = checkBox.IsChecked == true;
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                container.Children.Add(new TextBlock() { Text = "Background colour:", Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                AvaloniaColorPicker.ColorButton colorButton = new AvaloniaColorPicker.ColorButton() { Color = ((Colour)GlobalSettings.Settings.BackgroundColour).ToAvalonia(), Margin = new Thickness(0, 5, 5, 5), FontFamily = this.FontFamily, FontSize = this.FontSize };
                container.Children.Add(colorButton);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.BackgroundColour = colorButton.Color.ToVectSharp();

                    foreach (MainWindow win in GlobalSettings.Settings.MainWindows)
                    {
                        win.FindControl<Canvas>("PlotBackground").Background = new SolidColorBrush(colorButton.Color);
                    }
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                container.Children.Add(new TextBlock() { Text = "Selection colour:", Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                AvaloniaColorPicker.ColorButton colorButton = new AvaloniaColorPicker.ColorButton() { Color = ((Colour)GlobalSettings.Settings.SelectionColour).ToAvalonia(), Margin = new Thickness(0, 5, 5, 5), FontFamily = this.FontFamily, FontSize = this.FontSize };
                container.Children.Add(colorButton);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.SelectionColour = colorButton.Color.ToVectSharp();
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                container.Children.Add(new TextBlock() { Text = "Module repository:", Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                TextBox textBox = new TextBox() { Text = GlobalSettings.Settings.ModuleRepositoryBaseUri, Margin = new Thickness(0, 5, 5, 5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 200 };
                container.Children.Add(textBox);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.ModuleRepositoryBaseUri = textBox.Text;
                });
            }

            {
                StackPanel container = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                container.Children.Add(new TextBlock() { Text = "Check for updates:", Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                List<string> items = new List<string>() { "Do not check", "Only for TreeViewer", "For TreeViewer and any installed module", "For TreeViewer and all modules" };

                ComboBox comboBox = new ComboBox() { Items = items, SelectedIndex = (int)GlobalSettings.Settings.UpdateCheckMode, Margin = new Thickness(0, 5, 5, 5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 200 };
                container.Children.Add(comboBox);
                this.FindControl<WrapPanel>("MainContainer").Children.Add(container);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.UpdateCheckMode = (GlobalSettings.UpdateCheckModes)comboBox.SelectedIndex;
                });
            }

            foreach (KeyValuePair<string, string> kvp in GlobalSettings.Settings.AdditionalSettingsList)
            {
                AddParameter(kvp.Key, kvp.Value, applyChanges);
            }

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Disposing = true;
                this.Close();
                GlobalSettings.Settings.GlobalSettingsWindow = null;
            };

            this.FindControl<Button>("OKButton").Click += (s, e) =>
            {
                foreach (Action act in applyChanges)
                {
                    act();
                }

                GlobalSettings.SaveSettings();

                this.Hide();
            };

            this.FindControl<Button>("ResetButton").Click += (s, e) =>
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
                GlobalSettings.Settings.BackgroundColour = Colour.FromRgb(240, 244, 250);
                GlobalSettings.Settings.ModuleRepositoryBaseUri = GlobalSettings.DefaultModuleRepository;
                GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramAndAllModules;

                foreach (MainWindow win in GlobalSettings.Settings.MainWindows)
                {
                    win.FindControl<Canvas>("PlotBackground").Background = new SolidColorBrush(Colour.FromRgb(240, 244, 250).ToAvalonia());
                }

                GlobalSettings.Settings.SelectionColour = Colour.FromRgb(35, 127, 255);

                this.Disposing = true;
                this.Close();
                GlobalSettings.Settings.GlobalSettingsWindow = null;

                GlobalSettings.Settings.UpdateAdditionalSettings();
                GlobalSettings.SaveSettings();
            };
        }

        private void AddParameter(string name, string data, List<Action> applyChanges)
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

                CheckBox control = new CheckBox() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 5, 0, 5), IsChecked = defaultValue };

                StackPanel paramPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                paramPanel.Children.Add(control);

                this.FindControl<WrapPanel>("MainContainer").Children.Add(paramPanel);

                applyChanges.Add(() =>
                {
                    GlobalSettings.Settings.AdditionalSettings[parameterName] = control.IsChecked == true;
                });
            }
            else
            {
                string parameterName = name;

                StackPanel paramPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(10) };
                paramPanel.Children.Add(new TextBlock() { Text = parameterName, Margin = new Thickness(5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                this.FindControl<WrapPanel>("MainContainer").Children.Add(paramPanel);

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

                    ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Items = items, SelectedIndex = defaultIndex, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(box, 1);

                    paramPanel.Children.Add(box);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = box.SelectedIndex;
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

                    TextBox box = new TextBox() { Margin = new Thickness(5, 0, 0, 0), Padding = new Thickness(5, 0, 5, 0), Text = defaultValue, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 100, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(box, 1);

                    paramPanel.Children.Add(box);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = box.Text;
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

                    NumericUpDown nud = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = formatString, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(nud, 1);

                    paramPanel.Children.Add(nud);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = nud.Value;
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

                    FileSizeControl control = new FileSizeControl() { Margin = new Thickness(5, 0, 0, 0), Value = defaultValue, Unit = unit, Width = 225, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(control, 1);

                    paramPanel.Children.Add(control);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = control.Value;
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

                    Slider slid = new Slider() { Margin = new Thickness(5, 0, 0, 0), Minimum = minRange, Maximum = maxRange, Value = defaultValue, LargeChange = increment, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 100 };

                    Grid.SetColumn(slid, 1);

                    paramPanel.Children.Add(slid);

                    NumericUpDown valueBlock = null;

                    if (range.Length > 2)
                    {
                        valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Value = slid.Value, FormatString = range[2], Minimum = minRange, Maximum = maxRange, Increment = increment, Width = 150 };
                        paramPanel.Children.Add(valueBlock);
                    }
                    else
                    {
                        valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), Value = slid.Value, FormatString = Extensions.GetFormatString(increment), Minimum = minRange, Maximum = maxRange, Increment = increment, Width = 150 };
                        paramPanel.Children.Add(valueBlock);
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
                    });
                }
                else if (controlType == "Font")
                {
                    string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                    VectSharp.Font fnt = new VectSharp.Font(new VectSharp.FontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                    if (!GlobalSettings.Settings.AdditionalSettings.TryGetValue(parameterName, out object valueObject))
                    {
                        GlobalSettings.Settings.AdditionalSettings.Add(parameterName, fnt);
                    }
                    else
                    {
                        if (valueObject is JsonElement element)
                        {
                            font = element.GetString().Split(',');
                            fnt = new VectSharp.Font(new VectSharp.FontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = fnt;
                        }
                        else
                        {
                            fnt = (VectSharp.Font)valueObject;
                        }
                    }

                    FontButton but = new FontButton() { FontSize = 15, Font = fnt, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(but, 1);

                    paramPanel.Children.Add(but);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = but.Font;
                    });
                }
                else if (controlType == "Point")
                {
                    double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                    Grid grid = new Grid();
                    grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(150, GridUnitType.Pixel));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    grid.ColumnDefinitions.Add(new ColumnDefinition(150, GridUnitType.Pixel));

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

                    NumericUpDown nudX = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Increment = 1, Value = pt.X, FormatString = Extensions.GetFormatString(point[0]), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    NumericUpDown nudY = new NumericUpDown() { Margin = new Thickness(5, 0, 0, 0), Increment = 1, Value = pt.Y, FormatString = Extensions.GetFormatString(point[1]), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    TextBlock blkX = new TextBlock() { Text = "X:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                    TextBlock blkY = new TextBlock() { Text = "Y:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };

                    Grid.SetColumn(blkY, 2);

                    Grid.SetColumn(grid, 1);

                    Grid.SetColumn(nudX, 1);
                    Grid.SetColumn(nudY, 3);

                    grid.Children.Add(blkX);
                    grid.Children.Add(nudX);
                    grid.Children.Add(blkY);
                    grid.Children.Add(nudY);

                    paramPanel.Children.Add(grid);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);
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
                            col = Colour.FromCSSString(element.GetString()) ?? Colour.FromRgba(0, 0, 0, 0);
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = col;
                        }
                        else
                        {
                            col = (Colour)valueObject;
                        }
                    }

                    AvaloniaColorPicker.ColorButton but = new AvaloniaColorPicker.ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontFamily = this.FontFamily, FontSize = this.FontSize, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(but, 1);

                    paramPanel.Children.Add(but);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = but.Color.ToVectSharp();
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

                            lineDash = new LineDash(double.Parse(dashStr[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dashStr[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dashStr[2], System.Globalization.CultureInfo.InvariantCulture));
                            GlobalSettings.Settings.AdditionalSettings[parameterName] = lineDash;
                        }
                        else
                        {
                            lineDash = (LineDash)valueObject;
                        }
                    }

                    DashControl control = new DashControl() { LineDash = lineDash, Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                    Grid.SetColumn(control, 1);

                    paramPanel.Children.Add(control);

                    applyChanges.Add(() =>
                    {
                        GlobalSettings.Settings.AdditionalSettings[parameterName] = control.LineDash;
                    });
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
