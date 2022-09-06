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
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.VisualTree;

namespace TreeViewer
{
    public partial class MainWindow
    {
        private (UndoFrameLevel, int) GetModuleIndex(Dictionary<string, object> parameters)
        {
            if (parameters == this.TransformerParameters)
            {
                return (UndoFrameLevel.TransformerModule, 0);
            }

            if (parameters == this.CoordinatesParameters)
            {
                return (UndoFrameLevel.CoordinatesModule, 0);
            }

            for (int i = 0; i < this.FurtherTransformationsParameters.Count; i++)
            {
                if (parameters == this.FurtherTransformationsParameters[i])
                {
                    return (UndoFrameLevel.FurtherTransformationModule, i);
                }
            }

            for (int i = 0; i < this.PlottingParameters.Count; i++)
            {
                if (parameters == this.PlottingParameters[i])
                {
                    return (UndoFrameLevel.PlotActionModule, i);
                }
            }

            throw new ArgumentException("The parameter list was not found!");
        }

        private Dictionary<string, object> UpdateParameterPanel(GenericParameterChangeDelegate parameterChangeDelegate, List<(string, string)> parameters, Action updateAction, out Action<Dictionary<string, object>> UpdateParameterAction, out Control panel)
        {
            StackPanel controlsPanel = new StackPanel();
            Dictionary<string, object> tbr = new Dictionary<string, object>();

            if (parameters.Count > 1)
            {
                Stack<Controls> parents = new Stack<Controls>();
                parents.Push(controlsPanel.Children);

                Stack<int> childrenTillPop = new Stack<int>();
                childrenTillPop.Push(-1);

                Dictionary<string, Control> parameterControls = new Dictionary<string, Control>();
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                bool programmaticUpdate = false;

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                    else if (controlType == "TreeCollection")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this.Trees);
                    }
                    else if (controlType == "Window")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this);
                    }
                    else if (controlType == "InstanceStateData")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, this.StateData);
                    }
                    else if (controlType == "Group")
                    {
                        string parameterName = parameters[i].Item1;

                        int numChildren = int.Parse(controlParameters);
                        Border brd = new Border() { CornerRadius = new CornerRadius(10), Margin = new Thickness(0, 12, 0, 5), Padding = new Thickness(10, 0, 10, 0), BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200)), BorderThickness = new Thickness(1) };
                        StackPanel pnl = new StackPanel();
                        brd.Child = pnl;

                        Border header = new Border() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Thickness(-5, -12, 0, 5), Background = GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle ? new SolidColorBrush(Color.FromRgb(231, 231, 231)) : new SolidColorBrush(Color.FromRgb(245, 245, 245)), Padding = new Thickness(5, 0, 5, 0) };
                        header.Child = new TextBlock() { Text = parameterName };
                        pnl.Children.Add(header);

                        parents.Peek().Add(brd);

                        parameterControls.Add(parameterName, brd);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }

                        parents.Push(pnl.Children);
                        childrenTillPop.Push(numChildren);
                    }
                    else if (controlType == "Button")
                    {
                        string parameterName = parameters[i].Item1;

                        Button control = new Button() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 5, 0, 5), FontSize = 13 };
                        control.Classes.Add("PlainButton");

                        parameterControls.Add(parameterName, control);

                        parents.Peek().Add(control);

                        tbr.Add(parameterName, false);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        control.Click += (s, e) =>
                        {
                            (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                            PrepareUndoFrame(undoLevel, moduleIndex);

                            Dictionary<string, object> previousParameters = tbr.ShallowClone();

                            tbr[parameterName] = true;

                            bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                            UpdateControls(controlStatus, parameterControls);
                            UpdateParameters(parametersToChange, parameterUpdaters);

                            if (needsUpdate)
                            {
                                CommitUndoFrame();
                                updateAction();
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Buttons")
                    {
                        string parameterName = parameters[i].Item1;

                        string[] buttons = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                        Grid control = new Grid() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                        for (int j = 0; j < buttons.Length; j++)
                        {
                            Button butt = new Button() { Content = buttons[j], HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(5, 5, 5, 5), Margin = new Thickness(2.5, 5, 2.5, 5), FontSize = 13 };
                            butt.Classes.Add("PlainButton");

                            control.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                            Grid.SetColumn(butt, j);

                            control.Children.Add(butt);

                            int index = j;

                            butt.Click += (s, e) =>
                            {
                                (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                PrepareUndoFrame(undoLevel, moduleIndex);

                                Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                tbr[parameterName] = index;

                                bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                UpdateControls(controlStatus, parameterControls);
                                UpdateParameters(parametersToChange, parameterUpdaters);

                                if (needsUpdate)
                                {
                                    CommitUndoFrame();
                                    updateAction();
                                }
                            };
                        }

                        parameterControls.Add(parameterName, control);
                        parents.Peek().Add(control);
                        tbr.Add(parameterName, -1);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "CheckBox")
                    {
                        string parameterName = parameters[i].Item1;

                        CheckBox control = new CheckBox() { Content = new TextBlock() { Text = parameterName, TextWrapping = TextWrapping.Wrap }, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 0, 10, 0), Margin = new Thickness(0, 5, 0, 5), IsChecked = Convert.ToBoolean(controlParameters), FontSize = 13 };

                        parameterControls.Add(parameterName, control);

                        control.PropertyChanged += async (s, e) =>
                        {
                            if (e.Property == CheckBox.BoundsProperty && ((Rect)e.OldValue).Height != ((Rect)e.NewValue).Height)
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    control.FindAncestorOfType<Accordion>()?.InvalidateHeight();
                                }, Avalonia.Threading.DispatcherPriority.MinValue);
                            }
                        };

                        FillingControl<CheckBox> wrapper = new FillingControl<CheckBox>(control, 5) { Margin = new Thickness(-5, 0, -5, 0) };

                        parents.Peek().Add(wrapper);

                        tbr.Add(parameterName, control.IsChecked == true);

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                            control.IsChecked = (bool)value;
                        });

                        control.Click += (s, e) =>
                        {
                            (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                            PrepareUndoFrame(undoLevel, moduleIndex);

                            Dictionary<string, object> previousParameters = tbr.ShallowClone();

                            tbr[parameterName] = control.IsChecked == true;

                            bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                            UpdateControls(controlStatus, parameterControls);
                            UpdateParameters(parametersToChange, parameterUpdaters);

                            if (needsUpdate)
                            {
                                CommitUndoFrame();
                                updateAction();
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Formatter")
                    {
                        string parameterName = parameters[i].Item1;

                        Button control = new Button() { Content = parameterName, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(0, 5, 0, 5), FontSize = 13 };
                        control.Classes.Add("PlainButton");

                        parameterControls.Add(parameterName, control);

                        parents.Peek().Add(control);

                        string[] parsedParameters = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                        object[] formatterParams = new object[parsedParameters.Length - 1];

                        string attrType = (string)tbr[parsedParameters[0]];

                        if (attrType == "String")
                        {
                            formatterParams[0] = parsedParameters[1];
                            formatterParams[1] = Convert.ToBoolean(parsedParameters[2]);
                        }
                        else if (attrType == "Number")
                        {
                            formatterParams[0] = int.Parse(parsedParameters[1], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[1] = double.Parse(parsedParameters[2], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[2] = double.Parse(parsedParameters[3], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[3] = double.Parse(parsedParameters[4], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[4] = Convert.ToBoolean(parsedParameters[5]);
                            formatterParams[5] = Convert.ToBoolean(parsedParameters[6]);
                            formatterParams[6] = parsedParameters[7];
                            formatterParams[7] = Convert.ToBoolean(parsedParameters[8]);
                        }

                        tbr.Add(parameterName, new FormatterOptions(parsedParameters[parsedParameters.Length - 2]) { Parameters = formatterParams });

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        control.Click += async (s, e) =>
                        {
                            string attributeType = (string)tbr[parsedParameters[0]];

                            FormatOptionWindow win = new FormatOptionWindow();

                            string editorId = "StringFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                            await win.Initialize(attributeType, ((FormatterOptions)tbr[parameterName]).Parameters, this.DebuggerServer, editorId);

                            await win.ShowDialog2(this);

                            if (win.Result)
                            {
                                (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                PrepareUndoFrame(undoLevel, moduleIndex);

                                Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                tbr[parameterName] = win.Formatter;

                                bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                UpdateControls(controlStatus, parameterControls);
                                UpdateParameters(parametersToChange, parameterUpdaters);

                                if (needsUpdate)
                                {
                                    CommitUndoFrame();
                                    updateAction();
                                }
                            }
                        };

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else if (controlType == "Label")
                    {
                        string parameterName = parameters[i].Item1;

                        Grid paramPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        TextBlock labelBlock = new TextBlock() { Text = parameterName, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, TextWrapping = TextWrapping.Wrap };

                        labelBlock.PropertyChanged += async (s, e) =>
                        {
                            if (e.Property == TextBlock.BoundsProperty && ((Rect)e.OldValue).Height != ((Rect)e.NewValue).Height)
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    labelBlock.FindAncestorOfType<Accordion>()?.InvalidateHeight();
                                }, Avalonia.Threading.DispatcherPriority.MinValue);
                            }
                        };

                        FillingControl<TextBlock> wrapper = new FillingControl<TextBlock>(labelBlock, 5) { Margin = new Thickness(-5, 0, -5, 0) };

                        paramPanel.Children.Add(wrapper);
                        parents.Peek().Add(paramPanel);

                        if (controlParameters.StartsWith("["))
                        {
                            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            if (items.Length > 0)
                            {
                                switch (items[0])
                                {
                                    case "Left":
                                        labelBlock.TextAlignment = TextAlignment.Left;
                                        break;
                                    case "Right":
                                        labelBlock.TextAlignment = TextAlignment.Right;
                                        break;
                                    case "Center":
                                        labelBlock.TextAlignment = TextAlignment.Center;
                                        break;
                                }
                            }

                            if (items.Length > 1)
                            {
                                switch (items[1])
                                {
                                    case "Normal":
                                        labelBlock.FontStyle = FontStyle.Normal;
                                        labelBlock.FontWeight = FontWeight.Normal;
                                        break;
                                    case "Bold":
                                        labelBlock.FontStyle = FontStyle.Normal;
                                        labelBlock.FontWeight = FontWeight.Bold;
                                        break;
                                    case "Italic":
                                        labelBlock.FontStyle = FontStyle.Italic;
                                        labelBlock.FontWeight = FontWeight.Normal;
                                        break;
                                    case "BoldItalic":
                                        labelBlock.FontStyle = FontStyle.Italic;
                                        labelBlock.FontWeight = FontWeight.Bold;
                                        break;
                                }
                            }

                            if (items.Length > 2)
                            {
                                labelBlock.Foreground = new SolidColorBrush(Color.Parse(items[2]));
                            }
                        }

                        parameterControls.Add(parameterName, paramPanel);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else
                    {
                        string parameterName = parameters[i].Item1;

                        Grid paramPanel = new Grid() { Margin = new Thickness(0, 5, 0, 5) };
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        paramPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                        paramPanel.Children.Add(new TextBlock() { Text = parameterName, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                        parents.Peek().Add(paramPanel);

                        parameterControls.Add(parameterName, paramPanel);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }

                        if (controlType == "ComboBox")
                        {
                            int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            List<string> items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(controlParameters, Modules.DefaultSerializationOptions);

                            ComboBox box = new ComboBox() { Items = items, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };

                            FillingControl<ComboBox> wrapper = new FillingControl<ComboBox>(box, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameterName, box.SelectedIndex);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = (int)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = box.SelectedIndex;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "TextBox")
                        {
                            TextBox box = new TextBox() { Padding = new Thickness(5, 0, 5, 0), Text = controlParameters, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };

                            FillingControl<TextBox> wrapper = new FillingControl<TextBox>(box, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameterName, controlParameters);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.Text = (string)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.PropertyChanged += (s, e) =>
                            {
                                if (!programmaticUpdate && e.Property == TextBox.TextProperty)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = box.Text;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "AttributeSelector")
                        {
                            int defaultIndex = Math.Max(0, AttributeList.IndexOf(controlParameters));

                            ComboBox box = new ComboBox() { Items = AttributeList, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };

                            box.AttachedToVisualTree += (s, e) =>
                            {
                                AttributeSelectorTags[box] = (string)tbr[Modules.ModuleIDKey];
                            };

                            FillingControl<ComboBox> wrapper = new FillingControl<ComboBox>(box, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            AttributeSelectors.Add(box);

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameterName, controlParameters);

                            box.Tag = false;

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = AttributeList.IndexOf((string)value);
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate && !(bool)box.Tag)
                                {
                                    if (box.SelectedIndex < 0 || box.SelectedIndex >= AttributeList.Count)
                                    {
                                        if (defaultIndex < AttributeList.Count)
                                        {
                                            box.SelectedIndex = defaultIndex;
                                        }
                                        else
                                        {
                                            box.SelectedIndex = 0;
                                        }

                                        return;
                                    }

                                    string newValue = AttributeList[box.SelectedIndex];
                                    string oldValue = (string)tbr[parameterName];

                                    if (newValue != oldValue)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);

                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = newValue;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }

                            };
                        }
                        else if (controlType == "Attachment")
                        {
                            List<string> items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                            if (StateData.Attachments.Count > 0)
                            {
                                items.AddRange(StateData.Attachments.Keys);
                            }

                            ComboBox box = new ComboBox() { Items = items, SelectedIndex = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };

                            box.AttachedToVisualTree += (s, e) =>
                            {
                                box.Tag = tbr[Modules.ModuleIDKey];
                            };

                            FillingControl<ComboBox> wrapper = new FillingControl<ComboBox>(box, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            AttachmentSelectors.Add(box);

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameterName, null);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;

                                items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                                if (StateData.Attachments.Count > 0)
                                {
                                    items.AddRange(StateData.Attachments.Keys);
                                }

                                if (value is Attachment att)
                                {
                                    box.SelectedIndex = items.IndexOf(att.Name);
                                    tbr[parameterName] = value;
                                }
                                else
                                {
                                    box.SelectedIndex = 0;
                                    tbr[parameterName] = null;
                                }

                                programmaticUpdate = false;
                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    if (box.SelectedIndex <= 0 || box.SelectedIndex >= StateData.Attachments.Count + 1)
                                    {
                                        box.SelectedIndex = 0;
                                        tbr[parameterName] = null;
                                        return;
                                    }

                                    items = new List<string>(StateData.Attachments.Count + 1) { "Select attachment" };

                                    if (StateData.Attachments.Count > 0)
                                    {
                                        items.AddRange(StateData.Attachments.Keys);
                                    }

                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = StateData.Attachments[items[box.SelectedIndex]];

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Node")
                        {
                            string[] defaultValue = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            ((TextBlock)paramPanel.Children[0]).VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;

                            ((TextBlock)paramPanel.Children[0]).Margin = new Thickness(0, 10, 0, 5);

                            Accordion exp = new Accordion() { Margin = new Thickness(0, 0, 0, 0), ArrowSize = 10 };

                            Grid grd = new Grid() { Margin = new Thickness(5, 0, 0, 0) };
                            grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                            grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                            TrimmedTextBox2 blk = new TrimmedTextBox2() { Text = (defaultValue.Length > 1 ? "LCA of " : "") + defaultValue.Aggregate((a, b) => a + ", " + b), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                            Button control = new Button() { Content = "Edit", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Padding = new Thickness(5, 5, 5, 5), Margin = new Thickness(0, 5, 0, 5), FontSize = 13 };
                            control.Classes.Add("PlainButton");

                            Grid.SetColumnSpan(exp, 2);
                            grd.Children.Add(exp);


                            Grid.SetColumn(grd, 1);
                            Grid.SetRow(control, 1);

                            Button chooseSelection = new Button() { Content = "Use selection", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, Padding = new Thickness(5, 5, 5, 5), Margin = new Thickness(5, 5, 0, 5), HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
                            Grid.SetRow(chooseSelection, 1);
                            Grid.SetColumn(chooseSelection, 1);
                            chooseSelection.Classes.Add("PlainButton");

                            grd.Children.Add(control);
                            grd.Children.Add(chooseSelection);
                            chooseSelection.IsEnabled = this.IsSelectionAvailable;

                            void checkIfSelectionEnabled(object sender, AvaloniaPropertyChangedEventArgs e)
                            {
                                if (e.Property == MainWindow.IsSelectionAvailableProperty)
                                {
                                    chooseSelection.IsEnabled = this.IsSelectionAvailable;
                                }
                            }

                            this.PropertyChanged += checkIfSelectionEnabled;
                            bool hasEvent = true;

                            chooseSelection.DetachedFromLogicalTree += (s, e) =>
                            {
                                if (hasEvent)
                                {
                                    this.PropertyChanged -= checkIfSelectionEnabled;
                                    hasEvent = false;
                                }
                            };

                            chooseSelection.AttachedToLogicalTree += (s, e) =>
                            {
                                if (!hasEvent)
                                {
                                    this.PropertyChanged += checkIfSelectionEnabled;
                                    hasEvent = true;
                                }
                            };

                            exp.AccordionHeader = new FillingControl<TrimmedTextBox2>(blk, 5) { Margin = new Thickness(-5, 0, -5, 0) };

                            paramPanel.Children.Add(grd);

                            ScrollViewer valueScroller = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 0, 16) };

                            StackPanel valueContainer = new StackPanel();

                            for (int j = 0; j < defaultValue.Length; j++)
                            {
                                valueContainer.Children.Add(new TextBlock() { Text = defaultValue[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic, FontSize = 13 });
                            }

                            tbr.Add(parameterName, defaultValue);

                            parameterUpdaters.Add(parameterName, (value) =>
                            {
                                tbr[parameterName] = value;
                                blk.Text = (((string[])value).Length > 1 ? "LCA of " : "") + ((string[])value).Aggregate((a, b) => a + ", " + b);
                                valueContainer.Children.Clear();
                                for (int j = 0; j < ((string[])value).Length; j++)
                                {
                                    valueContainer.Children.Add(new TextBlock() { Text = ((string[])value)[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic, FontSize = 13 });
                                }
                            });

                            valueScroller.Content = valueContainer;

                            exp.AccordionContent = new FillingControl<ScrollViewer>(valueScroller, 5) { Margin = new Thickness(-5, 0, -5, 0) };

                            control.Click += async (s, e) =>
                            {
                                NodeChoiceWindow win;
                                int index = FurtherTransformationsParameters.IndexOf(tbr);
                                if (index >= 0 && index < AllTransformedTrees.Length)
                                {
                                    win = new NodeChoiceWindow(AllTransformedTrees[index], (string[])tbr[parameterName]);
                                }
                                else
                                {
                                    win = new NodeChoiceWindow(TransformedTree, (string[])tbr[parameterName]);
                                }

                                await win.ShowDialog2(this);

                                if (win.Result != null)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = win.Result;

                                    blk.Text = (win.Result.Length > 1 ? "LCA of " : "") + win.Result.Aggregate((a, b) => a + ", " + b);
                                    valueContainer.Children.Clear();
                                    for (int j = 0; j < win.Result.Length; j++)
                                    {
                                        valueContainer.Children.Add(new TextBlock() { Text = win.Result[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic });
                                    }

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };

                            chooseSelection.Click += (s, e) =>
                            {
                                if (this.SelectedNode != null)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    string[] nodeNames = this.SelectedNode.GetNodeNames().ToArray();

                                    tbr[parameterName] = nodeNames;

                                    blk.Text = (nodeNames.Length > 1 ? "LCA of " : "") + nodeNames.Aggregate((a, b) => a + ", " + b);
                                    valueContainer.Children.Clear();
                                    for (int j = 0; j < nodeNames.Length; j++)
                                    {
                                        valueContainer.Children.Add(new TextBlock() { Text = nodeNames[j], Margin = new Thickness(0, 0, 5, 0), FontStyle = FontStyle.Italic });
                                    }

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();

                                        updateAction();
                                    }
                                }
                            };

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

                            NumericUpDown nud = new NumericUpDown() { Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = formatString, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                            FillingControl<NumericUpDown> wrapper = new FillingControl<NumericUpDown>(nud, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameters[i].Item1, nud.Value);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nud.Value = (double)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nud.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = nud.Value;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "NumericUpDownByNode")
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

                            NumericUpDown nud = new NumericUpDown() { Minimum = minRange, Maximum = maxRange, Increment = increment, Value = defaultValue, FormatString = Extensions.GetFormatString(increment), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                            FillingControl<NumericUpDown> wrapper = new FillingControl<NumericUpDown>(nud, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            paramPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                            VerticalButton butEdit = new VerticalButton() { Margin = new Thickness(5, 0, 0, 0), FontSize = 13 };
                            Grid.SetColumn(butEdit, 2);
                            paramPanel.Children.Add(butEdit);

                            object[] formatterParams = new object[4];

                            string attrType = range[4];

                            if (attrType == "String")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }

                            tbr.Add(parameters[i].Item1, new NumberFormatterOptions(range[2]) { AttributeName = range[3], AttributeType = attrType, DefaultValue = defaultValue, Parameters = formatterParams });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nud.Value = ((NumberFormatterOptions)value).DefaultValue;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nud.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);

                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    ((NumberFormatterOptions)tbr[parameterName]).DefaultValue = nud.Value;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };

                            butEdit.Click += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    NumberFormatterOptions opt = (NumberFormatterOptions)tbr[parameterName];

                                    NumberFormatterWindow win = new NumberFormatterWindow();

                                    string editorId = "NumberFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.Initialize(opt.AttributeName, opt.AttributeType, opt.DefaultValue, opt.Parameters, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Formatter;
                                        nud.Value = win.Formatter.DefaultValue;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };
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

                            StackPanel container = new StackPanel();
                            Grid.SetColumn(container, 1);
                            paramPanel.Children.Add(container);

                            Slider slid = new Slider() { Margin = new Thickness(5, -15, 0, 0), Minimum = minRange, Maximum = maxRange, Value = defaultValue, LargeChange = increment, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                            slid.Resources.Add("SliderHorizontalThumbWidth", 5);
                            slid.Resources.Add("SliderHorizontalThumbHeight", 20);
                            slid.Resources.Add("SliderPreContentMargin", 0.0);
                            slid.Resources.Add("SliderPostContentMargin", 0.0);

                            container.Children.Add(slid);

                            NumericUpDown valueBlock = null;

                            if (range.Length > 2)
                            {
                                valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Value = slid.Value, FormatString = range[2], Minimum = minRange, Maximum = maxRange, Increment = increment, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                                FillingControl<NumericUpDown> wrapper = new FillingControl<NumericUpDown>(valueBlock, 5) { Margin = new Thickness(0, 0, -5, 0) };

                                container.Children.Add(wrapper);
                            }
                            else
                            {
                                valueBlock = new NumericUpDown() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Value = slid.Value, FormatString = Extensions.GetFormatString(increment), Minimum = minRange, Maximum = maxRange, Increment = increment, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                                FillingControl<NumericUpDown> wrapper = new FillingControl<NumericUpDown>(valueBlock, 5) { Margin = new Thickness(0, 0, -5, 0) };
                                container.Children.Add(wrapper);
                            }

                            tbr.Add(parameters[i].Item1, slid.Value);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                slid.Value = (double)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            bool causedUpdate = false;

                            slid.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == Slider.ValueProperty)
                                {
                                    bool prevProgUpd = programmaticUpdate;
                                    programmaticUpdate = true;
                                    valueBlock.Value = slid.Value;
                                    programmaticUpdate = prevProgUpd;


                                    if (!programmaticUpdate)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);

                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = slid.Value;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            causedUpdate = true;
                                            updateAction();
                                        }
                                        else
                                        {
                                            causedUpdate = false;
                                        }
                                    }
                                }
                            };

                            slid.AddValueChangeEndHandler(() => {
                                if (causedUpdate)
                                {
                                    CommitUndoFrame();
                                    causedUpdate = false;
                                }
                            });

                            if (valueBlock != null)
                            {
                                valueBlock.ValueChanged += (s, e) =>
                                {
                                    bool prevProgUpd = programmaticUpdate;
                                    programmaticUpdate = true;
                                    slid.Value = valueBlock.Value;
                                    programmaticUpdate = prevProgUpd;

                                    if (!programmaticUpdate)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = slid.Value;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                };
                            }
                        }
                        else if (controlType == "Font")
                        {
                            string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Font fnt = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                            FontButton but = new FontButton(true) { Font = fnt, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
                            but.Classes.Add("PlainButton");

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, fnt);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Font = (VectSharp.Font)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.FontChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = but.Font;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Point")
                        {
                            double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            Grid grid = new Grid();
                            grid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                            grid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                            grid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                            grid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                            NumericUpDown nudX = new NumericUpDown() { Increment = 1, Value = point[0], FormatString = Extensions.GetFormatString(point[0]), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
                            NumericUpDown nudY = new NumericUpDown() { Margin = new Thickness(0, 2, 0, 0), Increment = 1, Value = point[1], FormatString = Extensions.GetFormatString(point[1]), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                            TextBlock blkX = new TextBlock() { Text = "X:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                            TextBlock blkY = new TextBlock() { Text = "Y:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 2, 0, 0) };

                            Grid.SetColumn(grid, 1);

                            FillingControl<NumericUpDown> wrapperX = new FillingControl<NumericUpDown>(nudX, 5) { Margin = new Thickness(0, 0, -5, 0) };
                            FillingControl<NumericUpDown> wrapperY = new FillingControl<NumericUpDown>(nudY, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapperX, 1);

                            Grid.SetRow(blkY, 1);
                            Grid.SetRow(wrapperY, 1);
                            Grid.SetColumn(wrapperY, 1);



                            grid.Children.Add(blkX);
                            grid.Children.Add(wrapperX);
                            grid.Children.Add(blkY);
                            grid.Children.Add(wrapperY);

                            paramPanel.Children.Add(grid);

                            tbr.Add(parameters[i].Item1, new VectSharp.Point(nudX.Value, nudY.Value));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                nudX.Value = ((VectSharp.Point)value).X;
                                nudY.Value = ((VectSharp.Point)value).Y;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            nudX.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };

                            nudY.ValueChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = new VectSharp.Point(nudX.Value, nudY.Value);

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "Colour")
                        {
                            int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                            ColorButton but = new ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontFamily = this.FontFamily, FontSize = 13 };

                            but.Classes.Add("PlainButton");

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, col);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Color = ((VectSharp.Colour)value).ToAvalonia();
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == ColorButton.ColorProperty)
                                {
                                    if (!programmaticUpdate)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = but.Color.ToVectSharp();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "SourceCode")
                        {
                            string defaultSource = controlParameters;

                            Button but = new Button() { Content = "Edit...", Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
                            but.Classes.Add("PlainButton");

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, new CompiledCode(defaultSource));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.Click += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    CodeEditorWindow win = new CodeEditorWindow();

                                    string editorId = "CodeEditor_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.FinishInitialization(((CompiledCode)tbr[parameterName]).SourceCode, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result != null)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Result;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "Markdown")
                        {
                            string defaultSource = controlParameters;

                            Button but = new Button() { Content = "Edit...", Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
                            but.Classes.Add("PlainButton");

                            Grid.SetColumn(but, 1);

                            paramPanel.Children.Add(but);

                            tbr.Add(parameters[i].Item1, defaultSource);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.Click += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    MarkdownEditorWindow win = new MarkdownEditorWindow();

                                    string editorId = "MarkdownEditor_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.FinishInitialization((string)tbr[parameterName], editorId, this.StateData);

                                    await win.ShowDialog2(this);

                                    if (win.Result != null)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Result;

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "Dash")
                        {
                            double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);

                            DashControl control = new DashControl() { LineDash = lineDash, FontSize = 13 };

                            FillingControl<DashControl> wrapper = new FillingControl<DashControl>(control, 5) { Margin = new Thickness(0, 0, -5, 0) };

                            Grid.SetColumn(wrapper, 1);

                            paramPanel.Children.Add(wrapper);

                            tbr.Add(parameters[i].Item1, lineDash);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                control.LineDash = (VectSharp.LineDash)value;
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            control.DashChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = control.LineDash;

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }
                        else if (controlType == "ColourByNode")
                        {
                            string[] colour = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba(int.Parse(colour[3]), int.Parse(colour[4]), int.Parse(colour[5]), int.Parse(colour[6]));

                            StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                            ColorButton but = new ColorButton() { Color = col.ToAvalonia(), Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontFamily = this.FontFamily, FontSize = 13 };
                            but.Classes.Add("PlainButton");
                            pnl.Children.Add(but);

                            VerticalButton butEdit = new VerticalButton() { Margin = new Thickness(5, 0, 0, 0) };
                            pnl.Children.Add(butEdit);

                            Grid.SetColumn(pnl, 1);

                            paramPanel.Children.Add(pnl);

                            object[] formatterParams = new object[colour.Length - 5];

                            string attrType = colour[2];

                            if (attrType == "String")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = Convert.ToBoolean(colour[7]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = double.Parse(colour[7], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[2] = double.Parse(colour[8], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[3] = Modules.DefaultGradients[colour[9]];
                                formatterParams[4] = Convert.ToBoolean(colour[10]);
                            }

                            tbr.Add(parameters[i].Item1, new ColourFormatterOptions(colour[0], formatterParams) { AttributeName = colour[1], AttributeType = attrType, DefaultColour = col });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                but.Color = ((ColourFormatterOptions)value).DefaultColour.ToAvalonia();
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            but.PropertyChanged += (s, e) =>
                            {
                                if (e.Property == ColorButton.ColorProperty)
                                {
                                    if (!programmaticUpdate)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        ((ColourFormatterOptions)tbr[parameterName]).DefaultColour = but.Color.ToVectSharp();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };

                            butEdit.Click += async (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    ColourFormatterOptions opt = (ColourFormatterOptions)tbr[parameterName];

                                    ColourFormatterWindow win = new ColourFormatterWindow();

                                    string editorId = "ColourFormatter_" + parameterName.CoerceValidFileName() + "_" + (string)tbr[Modules.ModuleIDKey];
                                    await win.Initialize(opt.AttributeName, opt.AttributeType, opt.DefaultColour, opt.Parameters, this.DebuggerServer, editorId);

                                    await win.ShowDialog2(this);

                                    if (win.Result)
                                    {
                                        (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                        PrepareUndoFrame(undoLevel, moduleIndex);
                                        Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                        tbr[parameterName] = win.Formatter;
                                        but.Color = win.Formatter.DefaultColour.ToAvalonia();

                                        bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                        UpdateControls(controlStatus, parameterControls);
                                        UpdateParameters(parametersToChange, parameterUpdaters);

                                        if (needsUpdate)
                                        {
                                            CommitUndoFrame();
                                            updateAction();
                                        }
                                    }
                                }
                            };
                        }
                        else if (controlType == "AttributeType")
                        {
                            List<string> attributeTypes = new List<string>(Modules.AttributeTypes);

                            int defaultIndex = attributeTypes.IndexOf(controlParameters);

                            ComboBox box = new ComboBox() { Margin = new Thickness(5, 0, 0, 0), Items = attributeTypes, SelectedIndex = defaultIndex, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };

                            Grid.SetColumn(box, 1);

                            paramPanel.Children.Add(box);

                            tbr.Add(parameterName, attributeTypes[box.SelectedIndex]);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                programmaticUpdate = true;
                                box.SelectedIndex = attributeTypes.IndexOf((string)value);
                                tbr[parameterName] = value;
                                programmaticUpdate = false;

                            });

                            box.SelectionChanged += (s, e) =>
                            {
                                if (!programmaticUpdate)
                                {
                                    (UndoFrameLevel undoLevel, int moduleIndex) = GetModuleIndex(tbr);
                                    PrepareUndoFrame(undoLevel, moduleIndex);
                                    Dictionary<string, object> previousParameters = tbr.ShallowClone();

                                    tbr[parameterName] = attributeTypes[box.SelectedIndex];

                                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                                    UpdateControls(controlStatus, parameterControls);
                                    UpdateParameters(parametersToChange, parameterUpdaters);

                                    if (needsUpdate)
                                    {
                                        CommitUndoFrame();
                                        updateAction();
                                    }
                                }
                            };
                        }

                    }
                }

                parameterChangeDelegate(tbr, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                UpdateControls(controlStatus, parameterControls);
                UpdateParameters(parametersToChange, parameterUpdaters);

                UpdateParameterAction = (parametersToChange) =>
                {
                    Dictionary<string, object> previousParameters = tbr.ShallowClone();
                    UpdateParameters(parametersToChange, parameterUpdaters);

                    bool needsUpdate = parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange2);
                    UpdateControls(controlStatus, parameterControls);
                    UpdateParameters(parametersToChange2, parameterUpdaters);
                };
            }
            else
            {
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                }

                controlsPanel.Children.Add(new TextBlock() { Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)), Text = "No options available", FontStyle = FontStyle.Italic });
                UpdateParameterAction = (parametersToChange) =>
                {
                    UpdateParameters(parametersToChange, parameterUpdaters);
                };
            }

            panel = controlsPanel;

            return tbr;
        }
    }

    internal static class SliderExtensions
    {
        public static void AddValueChangeEndHandler(this Slider slid, Action handler)
        {
            slid.AddHandler(Avalonia.Input.InputElement.KeyUpEvent, (s, e) =>
            {
                if (e.Handled)
                {

                    handler();
                }
            }, handledEventsToo: true);

            slid.TemplateApplied += (s, e) =>
            {
                Avalonia.Controls.Primitives.Thumb tmb = e.NameScope.Find<Avalonia.Controls.Primitives.Thumb>("thumb");

                tmb.DragCompleted += (s, e) =>
                {
                    handler();
                };

                Avalonia.Controls.Primitives.Track trk = e.NameScope.Find<Avalonia.Controls.Primitives.Track>("PART_Track");

                trk.IncreaseButton.AddHandler(Avalonia.Input.InputElement.PointerReleasedEvent, (s, e) =>
                {
                    handler();
                }, handledEventsToo: true);

                trk.DecreaseButton.AddHandler(Avalonia.Input.InputElement.PointerReleasedEvent, (s, e) =>
                {
                    handler();
                }, handledEventsToo: true);
            };
        }
    }
}
