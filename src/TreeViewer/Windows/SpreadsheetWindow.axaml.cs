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

using Accord.Math;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.Styling;
using Spreadalonia;
using CSharpEditor;
using Markdig.Helpers;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class SpreadsheetWindow : ChildWindow
    {
        static List<string> InstalledFontFamilies;

        static SpreadsheetWindow()
        {
            InstalledFontFamilies = new string[] { "Open Sans", "Roboto Mono" }.Concat(FontManager.Current.GetInstalledFontFamilyNames().ToList()).ToList();
        }

        public SpreadsheetWindow()
        {
            InitializeComponent(0, true, true);
        }


        public SpreadsheetWindow(bool robotoMono, bool canOpenSave, bool canFormat)
        {
            CanOpenSave = canOpenSave;
            CanFormat = canFormat;

            InitializeComponent(robotoMono ? 1 : 0, canOpenSave, canFormat);
        }

        public bool Result { get; private set; } = false;
        public Spreadsheet Spreadsheet { get; private set; }
        private bool CanOpenSave { get; }
        private bool CanFormat { get; }

        NumericUpDown fontSizeNud;
        bool programmaticChange;

        private void InitializeComponent(int initialFontFamily, bool canOpenSave, bool canFormat)
        {
            AvaloniaXamlLoader.Load(this);

            bool isOpen = true;

            this.Closed += (s, e) =>
            {
                isOpen = false;
            };

            this.FindControl<Button>("OKButton").Click += (s, e) =>
            {
                this.Result = true;
                this.Close();
            };

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Result = false;
                this.Close();
            };

            Spreadsheet spreadsheet = this.FindControl<Spreadsheet>("spreadsheetControl");

            Spreadsheet = spreadsheet;

            if (initialFontFamily == 0)
            {
                spreadsheet.FontFamily = FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans");
            }
            else if (initialFontFamily == 1)
            {
                spreadsheet.FontFamily = FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono");
            }

            programmaticChange = false;

            RibbonBar bar = new RibbonBar(new (string, bool)[] { ("Home", false), ("Edit", false) });

            bar.FontSize = 14;

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                this.Classes.Add("WindowsStyle");
            }

            if (GlobalSettings.Settings.RibbonStyle == GlobalSettings.RibbonStyles.Colourful)
            {
                bar.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178));
                bar.Margin = new Thickness(-1, 0, -1, 0);
                bar.Classes.Add("Colorful");
            }
            else
            {
                bar.Classes.Add("Grey");
            }

            this.FindControl<Grid>("RibbonBarContainer").Children.Add(bar);

            fontSizeNud = new NumericUpDown() { Width = 76, Minimum = 1, Increment = 1, FormatString = "0", Value = 14 };
            AvaloniaBugFixes.SetToolTip(fontSizeNud, "Font size");
            ColorButton foregroundButton = ColorButton.TextColorButton(this.Background, out Button applyForegroundButton);
            foregroundButton.UseLayoutRounding = true;
            foregroundButton.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center;
            foregroundButton.Margin = new Thickness(5, 0, 5, 0);
            AvaloniaBugFixes.SetToolTip(foregroundButton, "Text colour");
            ComboBox fontFamilyBox = new ComboBox() { Items = InstalledFontFamilies, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, SelectedIndex = initialFontFamily, Height = 26 };
            AvaloniaBugFixes.SetToolTip(fontFamilyBox, "Font family");
            ToggleButton italicButton = new ToggleButton() { Content = new TextBlock() { Text = "I", FontFamily = FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), FontStyle = FontStyle.Italic, FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 12, Height = 16, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black } };
            AvaloniaBugFixes.SetToolTip(italicButton, "Italic");
            ToggleButton boldButton = new ToggleButton() { Content = new TextBlock() { Text = "B", FontWeight = FontWeight.Bold, FontSize = 16, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 12, Height = 16, TextAlignment = TextAlignment.Center, Foreground = Brushes.Black }, Margin = new Thickness(0, 0, 5, 0) };
            AvaloniaBugFixes.SetToolTip(boldButton, "Bold");
            NumericUpDown columnWidthNud = new NumericUpDown() { Value = 65, Minimum = 32, Increment = 1, FormatString = "0" };
            AvaloniaBugFixes.SetToolTip(columnWidthNud, "Column width");
            NumericUpDown rowHeightNud = new NumericUpDown() { Value = 25, Minimum = 10, Increment = 1, FormatString = "0" };
            AvaloniaBugFixes.SetToolTip(rowHeightNud, "Row height");
            TextBox searchBox = new TextBox() { Padding = new Thickness(5, 1, 5, 1), Width = 100, FontSize = 12, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = "" };
            AvaloniaBugFixes.SetToolTip(searchBox, "Search text");
            TextBox replaceBox = new TextBox() { Padding = new Thickness(5, 1, 5, 1), Width = 100, FontSize = 12, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = "" };
            AvaloniaBugFixes.SetToolTip(replaceBox, "Replacement text");
            CheckBox regexBox = new CheckBox() { Content = "Regex", FontSize = 12 };
            AvaloniaBugFixes.SetToolTip(regexBox, "Interpret the search text as a regular expression");
            TextBox colSepBox = new TextBox() { Padding = new Thickness(5, 0, 5, 0), Width = 50, Height = 22, Text = "\\t", FontSize = 12, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
            AvaloniaBugFixes.SetToolTip(colSepBox, "Column separator");
            TextBox rowSepBox = new TextBox() { Padding = new Thickness(5, 0, 5, 0), Width = 50, Height = 22, Text = "\\n", FontSize = 12, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
            AvaloniaBugFixes.SetToolTip(rowSepBox, "Row separator");

            StackPanel colSepPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            colSepPanel.Children.Add(new TextBlock() { Text = "Column: ", FontSize = 12, TextAlignment = TextAlignment.Right, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 50 });
            colSepPanel.Children.Add(colSepBox);

            StackPanel rowSepPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 0) };
            rowSepPanel.Children.Add(new TextBlock() { Text = "Row: ", FontSize = 12, TextAlignment = TextAlignment.Right, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Width = 50 });
            rowSepPanel.Children.Add(rowSepBox);

            Grid alignmentPanel = new Grid() { VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            alignmentPanel.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            alignmentPanel.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            alignmentPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            alignmentPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            alignmentPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            ToggleButton topAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.TopAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 5, 5) };
            AvaloniaBugFixes.SetToolTip(topAlignButton, "Top align");
            alignmentPanel.Children.Add(topAlignButton);
            ToggleButton middleAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.MiddleAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 5, 5), IsChecked = true, IsEnabled = false };
            AvaloniaBugFixes.SetToolTip(middleAlignButton, "Middle align");
            Grid.SetColumn(middleAlignButton, 1);
            alignmentPanel.Children.Add(middleAlignButton);
            ToggleButton bottomAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.BottomAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 0, 5) };
            AvaloniaBugFixes.SetToolTip(bottomAlignButton, "Bottom align");
            Grid.SetColumn(bottomAlignButton, 2);
            alignmentPanel.Children.Add(bottomAlignButton);

            ToggleButton leftAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.LeftAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 0, 5), IsChecked = true, IsEnabled = false };
            AvaloniaBugFixes.SetToolTip(leftAlignButton, "Left align");
            Grid.SetRow(leftAlignButton, 1);
            alignmentPanel.Children.Add(leftAlignButton);
            ToggleButton centerAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.CenterAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 0, 5) };
            AvaloniaBugFixes.SetToolTip(centerAlignButton, "Center align");
            Grid.SetRow(centerAlignButton, 1);
            Grid.SetColumn(centerAlignButton, 1);
            alignmentPanel.Children.Add(centerAlignButton);
            ToggleButton rightAlignButton = new ToggleButton() { Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.RightAlign")) { Width = 16, Height = 16, Padding = new Thickness(0) }, Margin = new Thickness(0, 0, 0, 5) };
            AvaloniaBugFixes.SetToolTip(rightAlignButton, "Right align");
            Grid.SetRow(rightAlignButton, 1);
            Grid.SetColumn(rightAlignButton, 2);
            alignmentPanel.Children.Add(rightAlignButton);



            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                fontFamilyBox.Resources["ComboBoxDropDownBackground"] = Brushes.White;
                fontFamilyBox.Resources["OverlayCornerRadius"] = new CornerRadius(0);
                fontFamilyBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 0, 2);
                fontFamilyBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(198, 198, 198));

                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                fontFamilyBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 1) });
                fontFamilyBox.Styles.Add(scrollBarStyle);
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                fontFamilyBox.Resources["ComboBoxDropDownBackground"] = new SolidColorBrush(Color.FromRgb(233, 233, 233)); ;
                fontFamilyBox.Resources["OverlayCornerRadius"] = new CornerRadius(4);
                fontFamilyBox.Resources["ComboBoxDropdownBorderThickness"] = new Thickness(1, 1, 0, 2);
                fontFamilyBox.Resources["ComboBoxDropDownBorderBrush"] = new SolidColorBrush(Color.FromRgb(195, 195, 195));
                fontFamilyBox.Resources["ComboBoxDropdownBorderPadding"] = new Thickness(0, 4, 0, 4);


                Style scrollViewerStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>());
                scrollViewerStyle.Setters.Add(new Setter() { Property = ScrollBar.AllowAutoHideProperty, Value = false });
                fontFamilyBox.Styles.Add(scrollViewerStyle);

                Style scrollBarStyle = new Style(x => x.OfType<ComboBox>().Template().OfType<ScrollViewer>().Template().OfType<ScrollBar>());
                scrollBarStyle.Setters.Add(new Setter() { Property = ScrollBar.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 2) });
                fontFamilyBox.Styles.Add(scrollBarStyle);
            }

            SetupToggleButton(italicButton);
            SetupToggleButton(boldButton);

            SetupToggleButton(topAlignButton);
            SetupToggleButton(middleAlignButton);
            SetupToggleButton(bottomAlignButton);

            SetupToggleButton(leftAlignButton);
            SetupToggleButton(centerAlignButton);
            SetupToggleButton(rightAlignButton);

            StackPanel textFormatPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Thickness(0, 10, 0, 0), Height = 26 };
            textFormatPanel.Children.Add(boldButton);
            textFormatPanel.Children.Add(italicButton);
            textFormatPanel.Children.Add(foregroundButton);
            textFormatPanel.Children.Add(fontSizeNud);





            RibbonTabContent homeTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("File",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Open", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.OpenColour")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async ind =>
                    {
                        await Open(spreadsheet);
                        spreadsheet.Focus();
                    }), "Open a saved spreadsheet."),

                    ("Save", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.SaveColour")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async ind =>
                    {
                        await Save(spreadsheet);
                        spreadsheet.Focus();
                    }), "Save the current spreadsheet."),
                }),

                ("Format",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("@ComboBox:FontFamily", fontFamilyBox, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Font family"),

                    ("@", textFormatPanel, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), null),
                }),

                ("Cells",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("@", alignmentPanel, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), null),

                    (null, null, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), null),

                    (null, null, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), null),

                    ("Rows", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Row")), null, new List<(string, Control, string)>()
                    {
                        ( "Insert rows", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.InsertRow")), null ),
                        ( "Delete rows", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.DeleteRow")), null ),

                    }, false, 0, (Action<int>)(ind =>
                    {
                        if (ConvertSelectionToRows(spreadsheet, false))
                        {
                            ImmutableList<SelectionRange> currSelection = spreadsheet.Selection;

                            for (int i = currSelection.Count - 1; i >= 0; i--)
                            {
                                spreadsheet.Selection = ImmutableList.Create(currSelection[i]);

                                if (ind == 0)
                                {
                                    spreadsheet.InsertRows();
                                }
                                else if (ind == 1)
                                {
                                    spreadsheet.DeleteRows();
                                }
                            }

                            spreadsheet.Focus();
                        }
                    }), "Insert or delete rows."),

                    ("Columns", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Column")), null, new List<(string, Control, string)>()
                    {
                        ( "Insert columns", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.InsertColumn")), null ),
                        ( "Delete columns", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.DeleteColumn")), null ),
                    }, false, 0, (Action<int>)(ind =>
                    {
                        if (ConvertSelectionToColumns(spreadsheet, false))
                        {
                            ImmutableList<SelectionRange> currSelection = spreadsheet.Selection;

                            for (int i = currSelection.Count - 1; i >= 0; i--)
                            {
                                spreadsheet.Selection = ImmutableList.Create(currSelection[i]);

                                if (ind == 0)
                                {
                                    spreadsheet.InsertColumns();
                                }
                                else if (ind == 1)
                                {
                                    spreadsheet.DeleteColumns();
                                }
                            }

                            spreadsheet.Focus();
                        }
                    }), "Insert or delete columns."),

                    ("Clear", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ClearContents")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Clear contents", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ClearContents")), null ),
                        ( "Clear format", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ClearFormat")), null ),
                    }, false, 0, (Action<int>)(ind =>
                    {
                        if (ind <= 0)
                        {
                            spreadsheet.ClearContents();
                            spreadsheet.Focus();
                        }
                        else if (ind == 1)
                        {
                            spreadsheet.ResetFormat();
                            spreadsheet.Focus();
                        }

                    }), "Clear contents or format."),
                }),

                ("Data",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Colour picker", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ColorPicker")), null, new List<(string, Control, string)>(), true, 0, (Action<int>)(async ind =>
                    {
                        await OpenColourPickerWindow(spreadsheet);
                        spreadsheet.Focus();
                    }), "Use a colour picker to set the value of the selected cell."),

                    ("Sort", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Sort")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Sort ascending", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Ascending")), null ),
                        ( "Sort descending", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Descending")), null ),
                        ( "Custom sort...", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.CustomSort")), null ),
                    }, true, 0, (Action<int>)(async ind =>
                    {
                        if (CanSort(spreadsheet.Selection))
                        {
                            if (ind == -1)
                            {
                                Sort(spreadsheet, null, false, null );
                                spreadsheet.Focus();
                            }
                            else if (ind == 0)
                            {
                                Sort(spreadsheet, null, false, new (bool, int)[]{ (true, 0) } );
                                spreadsheet.Focus();
                            }
                            else if (ind == 1)
                            {
                                Sort(spreadsheet, null, false, new (bool, int)[]{ (false, 0) } );
                                spreadsheet.Focus();
                            }
                            else if (ind == 2)
                            {
                                await ShowCustomSortDialog(spreadsheet);
                                spreadsheet.Focus();
                            }
                        }
                    }), "Sort data in the selected cells."),
                }),

                 ("Clipboard",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Paste", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PasteDoc")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Paste", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteDoc")), null ),
                        ( "Paste (skip blanks)", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteSkipBlanks")), null ),
                        ( "Paste transposed", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteTranspose")), null ),
                        ( "Transform and paste", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteTransformed")), null ),
                    }, true, 0, (Action<int>)(async ind =>
                    {
                        if (ind <= 0)
                        {
                            await spreadsheet.Paste(true);
                            spreadsheet.Focus();
                        }
                        else if (ind == 1)
                        {
                            await spreadsheet.Paste(false);
                            spreadsheet.Focus();
                        }
                        else if (ind == 2)
                        {
                            string text = await Application.Current.Clipboard.GetTextAsync();
                            if (!string.IsNullOrEmpty(text))
                            {
                                spreadsheet.Paste(Transpose(text, spreadsheet.RowSeparator, spreadsheet.ColumnSeparator, spreadsheet.QuoteSymbol), true);
                                spreadsheet.Focus();
                            }
                        }
                        else if (ind == 3)
                        {
                            await TransformAndPaste(spreadsheet);
                            spreadsheet.Focus();
                        }

                    }), "Paste data from the clipboard."),

                    ("Copy", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.CopyDoc")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Copy();
                        spreadsheet.Focus();
                    }), "Copy the selected cells to the clipboard."),

                    ("Cut", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Cut")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Cut();
                        spreadsheet.Focus();
                    }), "Copy the selected cells to the clipboard and clear them."),
                }),

            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(homeTab);

            RibbonTabContent editTab = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("Clipboard",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Paste", new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PasteDoc")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Paste", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteDoc")), null ),
                        ( "Paste (skip blanks)", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteSkipBlanks")), null ),
                        ( "Paste transposed", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteTranspose")), null ),
                        ( "Transform and paste", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.PasteTransformed")), null ),
                    }, true, 0, (Action<int>)(async ind =>
                    {
                        if (ind <= 0)
                        {
                            await spreadsheet.Paste(true);
                            spreadsheet.Focus();
                        }
                        else if (ind == 1)
                        {
                            await spreadsheet.Paste(false);
                            spreadsheet.Focus();
                        }
                        else if (ind == 2)
                        {
                            string text = await Application.Current.Clipboard.GetTextAsync();
                            if (!string.IsNullOrEmpty(text))
                            {
                                spreadsheet.Paste(Transpose(text, spreadsheet.RowSeparator, spreadsheet.ColumnSeparator, spreadsheet.QuoteSymbol), true);
                                spreadsheet.Focus();
                            }
                        }
                        else if (ind == 3)
                        {
                            await TransformAndPaste(spreadsheet);
                            spreadsheet.Focus();
                        }

                    }), "Paste data from the clipboard."),

                    ("Copy", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.CopyDoc")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Copy();
                        spreadsheet.Focus();
                    }), "Copy the selected cells to the clipboard."),

                    ("Cut", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Cut")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Cut();
                        spreadsheet.Focus();
                    }), "Copy the selected cells to the clipboard and clear them."),
                }),

                ("Separators",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("@", colSepPanel, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Column separator"),

                    ("@", rowSepPanel, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Row separator"),

                    ("Reload", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Refresh")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        Load(spreadsheet, this.OriginalText, true);
                        spreadsheet.Focus();
                    }), "Reload the document using the currently specified separators."),
                }),

                ("Row height",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("AutoFit height", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.AutoHeight")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        if (ConvertSelectionToRows(spreadsheet, true))
                        {
                            spreadsheet.AutoFitHeight();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            rowHeightNud.Value = h;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                    }), "Determine row height based on the height of the tallest cell."),

                    ("Reset height", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ResetHeight")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        if (spreadsheet.Selection.Any(x => (x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight && x.Left == 0 && x.Right == spreadsheet.MaxTableWidth)))
                        {
                            spreadsheet.ResetHeight();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            rowHeightNud.Value = h;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                        else if (ConvertSelectionToRows(spreadsheet, true))
                        {
                            spreadsheet.ResetHeight();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            rowHeightNud.Value = h;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                    }), "Reset the height for the selected rows to the default value."),

                    ("@", rowHeightNud, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Default row height"),
                }),
                ("Column width",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("AutoFit width", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.AutoWidth")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        if (ConvertSelectionToColumns(spreadsheet, true))
                        {
                            spreadsheet.AutoFitWidth();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            columnWidthNud.Value = w;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                    }), "Determine column width based on the width of the widest cell."),

                    ("Reset width", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.ResetWidth")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        if (spreadsheet.Selection.Any(x => (x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight && x.Left == 0 && x.Right == spreadsheet.MaxTableWidth)))
                        {
                            spreadsheet.ResetWidth();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            columnWidthNud.Value = w;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                        else if (ConvertSelectionToColumns(spreadsheet, true))
                        {
                            spreadsheet.ResetWidth();
                            programmaticChange = true;
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                            columnWidthNud.Value = w;
                            programmaticChange = false;
                            spreadsheet.Focus();
                        }
                    }), "Reset the width for the selected columns to the default value."),

                    ("@", columnWidthNud, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Default column width"),
                }),

                ("History",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Undo", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Undo")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Undo();
                        spreadsheet.Focus();
                    }), "Undo the last action."),

                    ("Redo", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Redo")), null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {
                        spreadsheet.Redo();
                        spreadsheet.Focus();
                    }), "Redo the last undone action."),
                }),

                ("Search and replace",
                new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("@", searchBox, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Search text"),

                    ("Search", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Find")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Find next", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Find")), null ),
                        ( "Find all", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Find")), null ),
                    }, false, 0, (Action<int>)(async ind =>
                    {
                        if (ind <= 0)
                        {
                            if (regexBox.IsChecked == true)
                            {
                                try
                                {
                                    Regex regex = new Regex(searchBox.Text ?? "", RegexOptions.IgnoreCase);

                                    List<(int, int)> matches = new List<(int, int)>();
                                    int currMatch = -1;

                                    if (spreadsheet.Selection.Count == 0 ||
                                       (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);

                                                if (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Left == kvp.Key.Item1 && spreadsheet.Selection[0].Top == kvp.Key.Item2)
                                                {
                                                    currMatch = matches.Count - 1;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (Contains(spreadsheet.Selection, kvp.Key) && regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);
                                            }
                                        }
                                    }

                                    if (matches.Count > 0)
                                    {
                                        spreadsheet.Selection = ImmutableList.Create(new SelectionRange(matches[(currMatch + 1) % matches.Count]));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await new MessageBox("Attention!", "Invalid regular expression!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Warning).ShowDialog2(this);
                                }

                                spreadsheet.Focus();
                            }
                            else
                            {
                                string searchText = searchBox.Text ?? "";

                                List<(int, int)> matches = new List<(int, int)>();
                                int currMatch = -1;

                                if (spreadsheet.Selection.Count == 0 ||
                                   (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);

                                            if (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Left == kvp.Key.Item1 && spreadsheet.Selection[0].Top == kvp.Key.Item2)
                                            {
                                                currMatch = matches.Count - 1;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (Contains(spreadsheet.Selection, kvp.Key) && kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);
                                        }
                                    }
                                }

                                if (matches.Count > 0)
                                {
                                    spreadsheet.Selection = ImmutableList.Create(new SelectionRange(matches[(currMatch + 1) % matches.Count]));
                                }

                                spreadsheet.Focus();
                            }
                        }
                        else if (ind == 1)
                        {
                            if (regexBox.IsChecked == true)
                            {
                                try
                                {
                                    Regex regex = new Regex(searchBox.Text ?? "", RegexOptions.IgnoreCase);

                                    List<(int, int)> matches = new List<(int, int)>();

                                    if (spreadsheet.Selection.Count == 0 ||
                                       (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (Contains(spreadsheet.Selection, kvp.Key) && regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);
                                            }
                                        }
                                    }

                                    if (matches.Count > 0)
                                    {
                                        spreadsheet.Selection = Consolidate(matches.Select(x => new SelectionRange(x))).ToImmutableList();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await new MessageBox("Attention!", "Invalid regular expression!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Warning).ShowDialog2(this);
                                }

                                spreadsheet.Focus();
                            }
                            else
                            {
                                string searchText = searchBox.Text ?? "";

                                List<(int, int)> matches = new List<(int, int)>();

                                if (spreadsheet.Selection.Count == 0 ||
                                   (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (Contains(spreadsheet.Selection, kvp.Key) && kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);
                                        }
                                    }
                                }

                                if (matches.Count > 0)
                                {
                                    spreadsheet.Selection = Consolidate(matches.Select(x => new SelectionRange(x))).ToImmutableList();
                                }

                                spreadsheet.Focus();
                            }
                        }


                    }), "Locate matches to the search text."),

                    ("@", regexBox, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Interpret the search text as a regular expression"),

                    ("@", replaceBox, null, new List<(string, Control, string)>(), false, 0, (Action<int>)(ind =>
                    {

                    }), "Replacement text"),

                    ("Replace", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Replace")), null, new List<(string, Control, string)>()
                    {
                        ( "", null, null ),
                        ( "Replace next", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Replace")), null ),
                        ( "Replace all", new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Replace")), null ),
                    }, false, 0, (Action<int>)(async ind =>
                    {
                        if (ind <= 0)
                        {
                            if (regexBox.IsChecked == true)
                            {
                                try
                                {
                                    Regex regex = new Regex(searchBox.Text ?? "", RegexOptions.IgnoreCase);

                                    List<(int, int)> matches = new List<(int, int)>();
                                    int currMatch = -1;

                                    if (spreadsheet.Selection.Count == 0 ||
                                       (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);

                                                if (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Left == kvp.Key.Item1 && spreadsheet.Selection[0].Top == kvp.Key.Item2)
                                                {
                                                    currMatch = matches.Count - 1;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (Contains(spreadsheet.Selection, kvp.Key) && regex.IsMatch(kvp.Value))
                                            {
                                                matches.Add(kvp.Key);
                                            }
                                        }
                                    }

                                    if (matches.Count > 0 && currMatch >= 0)
                                    {
                                        string replacement = regex.Replace(spreadsheet.Data[matches[currMatch]], replaceBox.Text ?? "");
                                        spreadsheet.SetData(new List<KeyValuePair<(int,int), string>>(){ new KeyValuePair<(int, int), string>(matches[currMatch], replacement) });
                                    }
                                    else if (matches.Count > 0)
                                    {
                                        spreadsheet.Selection = ImmutableList.Create(new SelectionRange(matches[(currMatch + 1) % matches.Count]));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await new MessageBox("Attention!", "Invalid regular expression!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Warning).ShowDialog2(this);
                                }

                                spreadsheet.Focus();
                            }
                            else
                            {
                                string searchText = searchBox.Text ?? "";

                                List<(int, int)> matches = new List<(int, int)>();
                                int currMatch = -1;

                                if (spreadsheet.Selection.Count == 0 ||
                                   (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);

                                            if (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Left == kvp.Key.Item1 && spreadsheet.Selection[0].Top == kvp.Key.Item2)
                                            {
                                                currMatch = matches.Count - 1;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (Contains(spreadsheet.Selection, kvp.Key) && kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp.Key);
                                        }
                                    }
                                }

                                if (matches.Count > 0 && currMatch >= 0)
                                {
                                    string replacement = spreadsheet.Data[matches[currMatch]].Replace(searchText, replaceBox.Text ?? "", StringComparison.OrdinalIgnoreCase);
                                    spreadsheet.SetData(new List<KeyValuePair<(int,int), string>>(){ new KeyValuePair<(int, int), string>(matches[currMatch], replacement) });
                                }
                                else if (matches.Count > 0)
                                {
                                    spreadsheet.Selection = ImmutableList.Create(new SelectionRange(matches[(currMatch + 1) % matches.Count]));
                                }

                                spreadsheet.Focus();
                            }
                        }
                        else if (ind == 1)
                        {
                            if (regexBox.IsChecked == true)
                            {
                                try
                                {
                                    Regex regex = new Regex(searchBox.Text ?? "", RegexOptions.IgnoreCase);

                                    Dictionary<(int, int), string> replacements = new Dictionary<(int, int), string>();

                                    if (spreadsheet.Selection.Count == 0 ||
                                       (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (regex.IsMatch(kvp.Value))
                                            {
                                                replacements.Add(kvp.Key, regex.Replace(kvp.Value, replaceBox.Text ?? ""));
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                        {
                                            if (Contains(spreadsheet.Selection, kvp.Key) && regex.IsMatch(kvp.Value))
                                            {
                                                replacements.Add(kvp.Key, regex.Replace(kvp.Value, replaceBox.Text ?? ""));
                                            }
                                        }
                                    }

                                    if (replacements.Count > 0)
                                    {
                                        spreadsheet.SetData(replacements);
                                        spreadsheet.Selection = Consolidate(replacements.Select(x => new SelectionRange(x.Key))).ToImmutableList();
                                    }
                                }
                                catch (Exception ex)
                                {
                                    await new MessageBox("Attention!", "Invalid regular expression!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Warning).ShowDialog2(this);
                                }

                                spreadsheet.Focus();
                            }
                            else
                            {
                                string searchText = searchBox.Text ?? "";

                                Dictionary<(int, int), string> replacements = new Dictionary<(int, int), string>();

                                if (spreadsheet.Selection.Count == 0 ||
                                   (spreadsheet.Selection.Count == 1 && spreadsheet.Selection[0].Width == 1 && spreadsheet.Selection[0].Height == 1))
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            replacements.Add(kvp.Key, kvp.Value.Replace(searchText, replaceBox.Text ?? "", StringComparison.OrdinalIgnoreCase));
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                                    {
                                        if (Contains(spreadsheet.Selection, kvp.Key) && kvp.Value.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                                        {
                                            replacements.Add(kvp.Key, kvp.Value.Replace(searchText, replaceBox.Text ?? "", StringComparison.OrdinalIgnoreCase));
                                        }
                                    }
                                }

                                if (replacements.Count > 0)
                                {


                                    spreadsheet.SetData(replacements);
                                    spreadsheet.Selection = Consolidate(replacements.Select(x => new SelectionRange(x.Key))).ToImmutableList();
                                }

                                spreadsheet.Focus();
                            }
                        }
                    }), "Replace occurrences of the search text with the replacement text."),
                }),
            })
            { Height = 100 };

            this.FindControl<Grid>("RibbonTabContainer").Children.Add(editTab);



            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            editTab.ZIndex = 0;
            editTab.RenderTransform = offScreen;
            editTab.Opacity = 0;
            editTab.IsHitTestVisible = false;

            homeTab.ZIndex = 1;
            homeTab.RenderTransform = TransformOperations.Identity;
            homeTab.Opacity = 1;
            homeTab.IsHitTestVisible = true;

            bar.PropertyChanged += (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    switch (bar.SelectedIndex)
                    {
                        case 0:
                            editTab.ZIndex = 0;
                            editTab.RenderTransform = offScreen;
                            editTab.Opacity = 0;
                            editTab.IsHitTestVisible = false;

                            homeTab.ZIndex = 1;
                            homeTab.RenderTransform = TransformOperations.Identity;
                            homeTab.Opacity = 1;
                            homeTab.IsHitTestVisible = true;
                            break;
                        case 1:
                            homeTab.ZIndex = 0;
                            homeTab.RenderTransform = offScreen;
                            homeTab.Opacity = 0;
                            homeTab.IsHitTestVisible = false;

                            editTab.ZIndex = 1;
                            editTab.RenderTransform = TransformOperations.Identity;
                            editTab.Opacity = 1;
                            editTab.IsHitTestVisible = true;
                            break;
                    }

                    spreadsheet.Focus();
                }
            };

            spreadsheet.PropertyChanged += async (s, e) =>
            {
                if (e.Property == Spreadsheet.SelectionProperty)
                {
                    programmaticChange = true;
                    if (spreadsheet.Selection.Count > 0)
                    {
                        Typeface currFace = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                        fontFamilyBox.IsEnabled = true;
                        fontFamilyBox.SelectedIndex = InstalledFontFamilies.IndexOf(currFace.FontFamily.Name);

                        boldButton.IsEnabled = true;
                        boldButton.IsChecked = currFace.Weight == FontWeight.Bold;

                        italicButton.IsEnabled = true;
                        italicButton.IsChecked = currFace.Style == FontStyle.Italic;

                        foregroundButton.IsEnabled = true;

                        (TextAlignment hor, Avalonia.Layout.VerticalAlignment ver) = spreadsheet.GetAlignment(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                        topAlignButton.IsChecked = ver == Avalonia.Layout.VerticalAlignment.Top;
                        middleAlignButton.IsChecked = ver == Avalonia.Layout.VerticalAlignment.Center;
                        bottomAlignButton.IsChecked = ver == Avalonia.Layout.VerticalAlignment.Bottom;

                        leftAlignButton.IsChecked = hor == TextAlignment.Left;
                        centerAlignButton.IsChecked = hor == TextAlignment.Center;
                        rightAlignButton.IsChecked = hor == TextAlignment.Right;

                        topAlignButton.IsEnabled = topAlignButton.IsChecked == false;
                        middleAlignButton.IsEnabled = middleAlignButton.IsChecked == false;
                        bottomAlignButton.IsEnabled = bottomAlignButton.IsChecked == false;
                        leftAlignButton.IsEnabled = leftAlignButton.IsChecked == false;
                        centerAlignButton.IsEnabled = centerAlignButton.IsChecked == false;
                        rightAlignButton.IsEnabled = rightAlignButton.IsChecked == false;

                        homeTab.RibbonGroups[2].RibbonButtons[0].IsEnabled = !spreadsheet.Selection.All(x => x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight);
                        homeTab.RibbonGroups[2].RibbonButtons[1].IsEnabled = !spreadsheet.Selection.All(x => x.Left == 0 && x.Right == spreadsheet.MaxTableWidth);
                        homeTab.RibbonGroups[2].RibbonButtons[2].IsEnabled = true;

                        homeTab.RibbonGroups[4].RibbonButtons[0].IsEnabled = spreadsheet.Selection.Count == 1 && (await Application.Current.Clipboard.GetFormatsAsync()).Contains("Text");
                        homeTab.RibbonGroups[4].RibbonButtons[1].IsEnabled = true;
                        homeTab.RibbonGroups[4].RibbonButtons[2].IsEnabled = true;

                        editTab.RibbonGroups[0].RibbonButtons[0].IsEnabled = spreadsheet.Selection.Count == 1 && (await Application.Current.Clipboard.GetFormatsAsync()).Contains("Text");
                        editTab.RibbonGroups[0].RibbonButtons[1].IsEnabled = true;
                        editTab.RibbonGroups[0].RibbonButtons[2].IsEnabled = true;

                        editTab.RibbonGroups[2].RibbonButtons[0].IsEnabled = !spreadsheet.Selection.All(x => x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight && !(x.Left == 0 && x.Right == spreadsheet.MaxTableWidth));
                        editTab.RibbonGroups[2].RibbonButtons[1].IsEnabled = !spreadsheet.Selection.All(x => x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight && !(x.Left == 0 && x.Right == spreadsheet.MaxTableWidth));
                        rowHeightNud.IsEnabled = !spreadsheet.Selection.All(x => x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight && !(x.Left == 0 && x.Right == spreadsheet.MaxTableWidth));

                        editTab.RibbonGroups[3].RibbonButtons[0].IsEnabled = !spreadsheet.Selection.All(x => x.Left == 0 && x.Right == spreadsheet.MaxTableWidth && !(x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight));
                        editTab.RibbonGroups[3].RibbonButtons[1].IsEnabled = !spreadsheet.Selection.All(x => x.Left == 0 && x.Right == spreadsheet.MaxTableWidth && !(x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight));
                        columnWidthNud.IsEnabled = !spreadsheet.Selection.All(x => x.Left == 0 && x.Right == spreadsheet.MaxTableWidth && !(x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight));

                        homeTab.RibbonGroups[3].RibbonButtons[0].IsEnabled = true;
                        homeTab.RibbonGroups[3].RibbonButtons[1].IsEnabled = CanSort(spreadsheet.Selection);

                        if (spreadsheet.Selection.Any(x => x.Top == 0 && x.Left == 0 && x.Bottom == spreadsheet.MaxTableHeight && x.Right == spreadsheet.MaxTableWidth))
                        {
                            columnWidthNud.Value = spreadsheet.DefaultColumnWidth;
                            rowHeightNud.Value = spreadsheet.DefaultRowHeight;
                        }
                        else
                        {
                            (double w, double h) = spreadsheet.GetCellSize(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                            columnWidthNud.Value = w;
                            rowHeightNud.Value = h;
                        }
                    }
                    else
                    {
                        fontFamilyBox.IsEnabled = false;
                        boldButton.IsEnabled = false;
                        italicButton.IsEnabled = false;
                        foregroundButton.IsEnabled = false;

                        topAlignButton.IsEnabled = false;
                        middleAlignButton.IsEnabled = false;
                        bottomAlignButton.IsEnabled = false;
                        leftAlignButton.IsEnabled = false;
                        centerAlignButton.IsEnabled = false;
                        rightAlignButton.IsEnabled = false;

                        homeTab.RibbonGroups[2].RibbonButtons[0].IsEnabled = false;
                        homeTab.RibbonGroups[2].RibbonButtons[1].IsEnabled = false;
                        homeTab.RibbonGroups[2].RibbonButtons[2].IsEnabled = false;

                        editTab.RibbonGroups[0].RibbonButtons[0].IsEnabled = false;
                        editTab.RibbonGroups[0].RibbonButtons[1].IsEnabled = false;
                        editTab.RibbonGroups[0].RibbonButtons[2].IsEnabled = false;

                        editTab.RibbonGroups[2].RibbonButtons[0].IsEnabled = false;
                        editTab.RibbonGroups[2].RibbonButtons[1].IsEnabled = false;

                        editTab.RibbonGroups[3].RibbonButtons[0].IsEnabled = false;
                        editTab.RibbonGroups[3].RibbonButtons[1].IsEnabled = false;

                        homeTab.RibbonGroups[3].RibbonButtons[0].IsEnabled = false;
                        homeTab.RibbonGroups[3].RibbonButtons[1].IsEnabled = false;

                        rowHeightNud.IsEnabled = false;
                        columnWidthNud.IsEnabled = false;
                    }
                    programmaticChange = false;
                }
                else if (e.Property == Spreadsheet.CanUndoProperty)
                {
                    editTab.RibbonGroups[4].RibbonButtons[0].IsEnabled = spreadsheet.CanUndo;
                }
                else if (e.Property == Spreadsheet.CanRedoProperty)
                {
                    editTab.RibbonGroups[4].RibbonButtons[1].IsEnabled = spreadsheet.CanRedo;
                }
                else if (e.Property == Spreadsheet.ColumnSeparatorProperty)
                {
                    programmaticChange = true;
                    colSepBox.Text = spreadsheet.ColumnSeparator.ToString();
                    programmaticChange = false;
                }
            };

            spreadsheet.CellSizeChanged += (s, e) =>
            {
                programmaticChange = true;
                columnWidthNud.Value = e.Width;
                rowHeightNud.Value = e.Height;
                programmaticChange = false;
            };

            Task.Run(async () =>
            {
                while (isOpen)
                {
                    await Task.Delay(100);
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        editTab.RibbonGroups[0].RibbonButtons[0].IsEnabled = spreadsheet.Selection.Count == 1 && (await Application.Current.Clipboard.GetFormatsAsync()).Contains("Text");
                    });
                }
            });

            fontFamilyBox.SelectionChanged += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    Typeface currTypeface = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                    if (fontFamilyBox.SelectedIndex > 1)
                    {
                        spreadsheet.SetTypeface(new Typeface(FontFamily.Parse(InstalledFontFamilies[fontFamilyBox.SelectedIndex]), currTypeface.Style, currTypeface.Weight));
                    }
                    else if (fontFamilyBox.SelectedIndex == 0)
                    {
                        spreadsheet.SetTypeface(new Typeface(FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans"), currTypeface.Style, currTypeface.Weight));
                    }
                    else if (fontFamilyBox.SelectedIndex == 1)
                    {
                        spreadsheet.SetTypeface(new Typeface(FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"), currTypeface.Style, currTypeface.Weight));
                    }

                }
            };

            boldButton.Click += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    Typeface currTypeface = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                    spreadsheet.SetTypeface(new Typeface(currTypeface.FontFamily, currTypeface.Style, boldButton.IsChecked == true ? FontWeight.Bold : FontWeight.Regular));
                }

                spreadsheet.Focus();
            };

            italicButton.Click += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    Typeface currTypeface = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);

                    spreadsheet.SetTypeface(new Typeface(currTypeface.FontFamily, italicButton.IsChecked == true ? FontStyle.Italic : FontStyle.Normal, currTypeface.Weight));
                }

                spreadsheet.Focus();
            };

            foregroundButton.PropertyChanged += (s, e) =>
            {
                if (e.Property == ColorButton.ColorProperty && !programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    spreadsheet.SetForeground(new SolidColorBrush(foregroundButton.Color));

                    spreadsheet.Focus();
                }
            };

            fontSizeNud.ValueChanged += (s, e) =>
            {
                spreadsheet.FontSize = fontSizeNud.Value;
            };

            applyForegroundButton.Click += (s, e) =>
            {
                if (spreadsheet.Selection.Count > 0)
                {
                    spreadsheet.SetForeground(new SolidColorBrush(foregroundButton.Color));
                }

                spreadsheet.Focus();
            };

            topAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetVerticalAlignment(Avalonia.Layout.VerticalAlignment.Top);
                    topAlignButton.IsChecked = true;
                    middleAlignButton.IsChecked = false;
                    bottomAlignButton.IsChecked = false;

                    topAlignButton.IsEnabled = topAlignButton.IsChecked == false;
                    middleAlignButton.IsEnabled = middleAlignButton.IsChecked == false;
                    bottomAlignButton.IsEnabled = bottomAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            middleAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetVerticalAlignment(Avalonia.Layout.VerticalAlignment.Center);
                    topAlignButton.IsChecked = false;
                    middleAlignButton.IsChecked = true;
                    bottomAlignButton.IsChecked = false;

                    topAlignButton.IsEnabled = topAlignButton.IsChecked == false;
                    middleAlignButton.IsEnabled = middleAlignButton.IsChecked == false;
                    bottomAlignButton.IsEnabled = bottomAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            bottomAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetVerticalAlignment(Avalonia.Layout.VerticalAlignment.Bottom);
                    topAlignButton.IsChecked = false;
                    middleAlignButton.IsChecked = false;
                    bottomAlignButton.IsChecked = true;

                    topAlignButton.IsEnabled = topAlignButton.IsChecked == false;
                    middleAlignButton.IsEnabled = middleAlignButton.IsChecked == false;
                    bottomAlignButton.IsEnabled = bottomAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            leftAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetTextAlignment(TextAlignment.Left);
                    leftAlignButton.IsChecked = true;
                    centerAlignButton.IsChecked = false;
                    rightAlignButton.IsChecked = false;

                    leftAlignButton.IsEnabled = leftAlignButton.IsChecked == false;
                    centerAlignButton.IsEnabled = centerAlignButton.IsChecked == false;
                    rightAlignButton.IsEnabled = rightAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            centerAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetTextAlignment(TextAlignment.Center);
                    leftAlignButton.IsChecked = false;
                    centerAlignButton.IsChecked = true;
                    rightAlignButton.IsChecked = false;

                    leftAlignButton.IsEnabled = leftAlignButton.IsChecked == false;
                    centerAlignButton.IsEnabled = centerAlignButton.IsChecked == false;
                    rightAlignButton.IsEnabled = rightAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            rightAlignButton.Checked += (s, e) =>
            {
                if (!programmaticChange && spreadsheet.Selection.Count > 0)
                {
                    programmaticChange = true;

                    spreadsheet.SetTextAlignment(TextAlignment.Right);
                    leftAlignButton.IsChecked = false;
                    centerAlignButton.IsChecked = false;
                    rightAlignButton.IsChecked = true;

                    leftAlignButton.IsEnabled = leftAlignButton.IsChecked == false;
                    centerAlignButton.IsEnabled = centerAlignButton.IsChecked == false;
                    rightAlignButton.IsEnabled = rightAlignButton.IsChecked == false;

                    programmaticChange = false;
                }

                spreadsheet.Focus();
            };

            colSepBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                {
                    if (!programmaticChange)
                    {
                        try
                        {
                            spreadsheet.ColumnSeparator = new Regex(colSepBox.Text ?? "", RegexOptions.Compiled);
                            colSepBox.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                        }
                        catch
                        {
                            try
                            {
                                spreadsheet.ColumnSeparator = new Regex(Regex.Escape(colSepBox.Text ?? ""), RegexOptions.Compiled);
                                colSepBox.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                            }
                            catch { colSepBox.BorderBrush = Brushes.Red; }
                        }
                    }
                }
            };

            rowSepBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == TextBox.TextProperty)
                {
                    try
                    {
                        spreadsheet.RowSeparator = new Regex(rowSepBox.Text ?? "", RegexOptions.Compiled);
                        rowSepBox.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                    }
                    catch
                    {
                        try
                        {
                            spreadsheet.RowSeparator = new Regex(Regex.Escape(rowSepBox.Text ?? ""), RegexOptions.Compiled);
                            rowSepBox.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                        }
                        catch { rowSepBox.BorderBrush = Brushes.Red; }
                    }
                }
            };

            rowHeightNud.ValueChanged += (s, e) =>
            {
                if (!programmaticChange)
                {
                    if (spreadsheet.Selection.Any(x => x.Top == 0 && x.Left == 0 && x.Bottom == spreadsheet.MaxTableHeight && x.Right == spreadsheet.MaxTableWidth))
                    {
                        spreadsheet.DefaultRowHeight = rowHeightNud.Value;
                    }
                    else
                    {
                        double val = rowHeightNud.Value;
                        programmaticChange = true;
                        if (ConvertSelectionToRows(spreadsheet, false))
                        {
                            programmaticChange = true;
                            rowHeightNud.Value = val;
                            spreadsheet.SetHeight(new Dictionary<int, double>(spreadsheet.Selection.Select(x => Enumerable.Range(x.Top, x.Height).Select(y => new KeyValuePair<int, double>(y, rowHeightNud.Value))).Aggregate((a, b) => a.Concat(b))));
                        }
                        programmaticChange = false;
                    }
                }
            };

            columnWidthNud.ValueChanged += (s, e) =>
            {
                if (!programmaticChange)
                {
                    if (spreadsheet.Selection.Any(x => x.Top == 0 && x.Left == 0 && x.Bottom == spreadsheet.MaxTableHeight && x.Right == spreadsheet.MaxTableWidth))
                    {
                        spreadsheet.DefaultColumnWidth = columnWidthNud.Value;
                    }
                    else
                    {
                        double val = columnWidthNud.Value;
                        programmaticChange = true;
                        if (ConvertSelectionToColumns(spreadsheet, false))
                        {
                            programmaticChange = true;
                            columnWidthNud.Value = val;
                            spreadsheet.SetWidth(new Dictionary<int, double>(spreadsheet.Selection.Select(x => Enumerable.Range(x.Left, x.Width).Select(y => new KeyValuePair<int, double>(y, columnWidthNud.Value))).Aggregate((a, b) => a.Concat(b))));
                        }
                        programmaticChange = false;
                    }
                }
            };

            editTab.RibbonGroups[4].RibbonButtons[0].IsEnabled = spreadsheet.CanUndo;
            editTab.RibbonGroups[4].RibbonButtons[1].IsEnabled = spreadsheet.CanUndo;

            if (!canOpenSave)
            {
                homeTab.RibbonGroups[0].IsVisible = false;
            }

            if (!canFormat)
            {
                homeTab.RibbonGroups[1].IsVisible = false;
                alignmentPanel.IsVisible = false;
            }

            this.KeyDown += async (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.B && e.KeyModifiers == Modules.ControlModifier)
                {
                    if (CanFormat && spreadsheet.Selection.Count > 0)
                    {
                        boldButton.IsChecked = !boldButton.IsChecked;
                        Typeface currTypeface = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                        spreadsheet.SetTypeface(new Typeface(currTypeface.FontFamily, currTypeface.Style, boldButton.IsChecked == true ? FontWeight.Bold : FontWeight.Regular));
                    }
                }
                else if (e.Key == Avalonia.Input.Key.I && e.KeyModifiers == Modules.ControlModifier)
                {
                    if (CanFormat && spreadsheet.Selection.Count > 0)
                    {
                        italicButton.IsChecked = !italicButton.IsChecked;
                        Typeface currTypeface = spreadsheet.GetTypeface(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top);
                        spreadsheet.SetTypeface(new Typeface(currTypeface.FontFamily, italicButton.IsChecked == true ? FontStyle.Italic : FontStyle.Normal, currTypeface.Weight));
                    }
                }
                else if (e.Key == Avalonia.Input.Key.F && e.KeyModifiers == Modules.ControlModifier)
                {
                    bar.SelectedIndex = 1;
                    searchBox.Focus();
                }
                else if (e.Key == Avalonia.Input.Key.H && e.KeyModifiers == Modules.ControlModifier)
                {
                    bar.SelectedIndex = 1;
                    replaceBox.Focus();
                }
                else if (e.Key == Avalonia.Input.Key.C && e.KeyModifiers == (Avalonia.Input.KeyModifiers.Shift | Modules.ControlModifier))
                {
                    if (spreadsheet.Selection.Count > 0)
                    {
                        await OpenColourPickerWindow(spreadsheet);
                        spreadsheet.Focus();
                    }
                }
            };

            spreadsheet.ColorDoubleTapped += async (s, e) =>
            {
                e.Handled = true;
                ColourPickerWindow cpw = new ColourPickerWindow(e.Color);
                Color? col = await cpw.ShowDialog(this);
                if (col != null && col != e.Color)
                {
                    string colString = "#" + col.Value.R.ToString("X2") + col.Value.G.ToString("X2") + col.Value.B.ToString("X2") + (col.Value.A != 255 ? col.Value.A.ToString("X2") : "");
                    spreadsheet.SetData(new KeyValuePair<(int, int), string>[] { new KeyValuePair<(int, int), string>((e.Left, e.Top), colString) });
                }

                spreadsheet.Focus();
            };
        }

        private static void SetupToggleButton(ToggleButton button)
        {
            button.Classes.Add("PlainButton");
            Style noBorderStyle = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyle.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Thickness(0) });
            Style noBorderStyleChecked = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Class(":checked").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyleChecked.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Thickness(0) });
            Style noBorderStyleFocus = new Style(x => x.OfType<ToggleButton>().Class("PlainButton").Class(":checked:focus").Template().OfType<ContentPresenter>().Name("PART_ContentPresenter"));
            noBorderStyleFocus.Setters.Add(new Setter() { Property = ContentPresenter.BorderThicknessProperty, Value = new Thickness(0) });
            button.Styles.Add(noBorderStyle);
            button.Styles.Add(noBorderStyleChecked);
            button.Styles.Add(noBorderStyleFocus);
        }

        private static bool ConvertSelectionToColumns(Spreadsheet spreadsheet, bool allowAll)
        {
            if (allowAll)
            {
                foreach (SelectionRange range in spreadsheet.Selection)
                {
                    if (range.Top == 0 && range.Bottom == spreadsheet.MaxTableHeight && range.Left == 0 && range.Right == spreadsheet.MaxTableWidth)
                    {
                        int maxX = 0;

                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                        {
                            maxX = Math.Max(maxX, kvp.Key.Item1);
                        }

                        spreadsheet.Selection = ImmutableList.Create(new SelectionRange(0, 0, maxX, spreadsheet.MaxTableHeight));
                    }
                }
            }

            HashSet<int> columns = new HashSet<int>();

            foreach (SelectionRange range in spreadsheet.Selection)
            {
                if (range.Left != 0 || range.Right != spreadsheet.MaxTableWidth)
                {
                    for (int i = range.Left; i <= range.Right; i++)
                    {
                        columns.Add(i);
                    }
                }
            }

            if (columns.Count > 0)
            {
                List<SelectionRange> selection = new List<SelectionRange>();

                List<int> sortedColumns = new List<int>(columns.Order());
                int currMin = sortedColumns[0];
                int currMax = currMin;

                for (int i = 1; i < sortedColumns.Count; i++)
                {
                    if (sortedColumns[i] == currMax + 1)
                    {
                        currMax++;
                    }
                    else
                    {
                        selection.Add(new SelectionRange(currMin, 0, currMax, spreadsheet.MaxTableHeight));
                        currMin = sortedColumns[i];
                        currMax = currMin;
                    }
                }

                selection.Add(new SelectionRange(currMin, 0, currMax, spreadsheet.MaxTableHeight));

                spreadsheet.Selection = selection.ToImmutableList();

                return true;
            }

            return false;
        }

        private static bool ConvertSelectionToRows(Spreadsheet spreadsheet, bool allowAll)
        {
            if (allowAll)
            {
                foreach (SelectionRange range in spreadsheet.Selection)
                {
                    if (range.Top == 0 && range.Bottom == spreadsheet.MaxTableHeight && range.Left == 0 && range.Right == spreadsheet.MaxTableWidth)
                    {
                        int maxY = 0;

                        foreach (KeyValuePair<(int, int), string> kvp in spreadsheet.Data)
                        {
                            maxY = Math.Max(maxY, kvp.Key.Item2);
                        }

                        spreadsheet.Selection = ImmutableList.Create(new SelectionRange(0, 0, spreadsheet.MaxTableWidth, maxY));
                    }
                }
            }

            HashSet<int> rows = new HashSet<int>();

            foreach (SelectionRange range in spreadsheet.Selection)
            {
                if (range.Top != 0 || range.Bottom != spreadsheet.MaxTableHeight)
                {
                    for (int i = range.Top; i <= range.Bottom; i++)
                    {
                        rows.Add(i);
                    }
                }
            }

            if (rows.Count > 0)
            {
                List<SelectionRange> selection = new List<SelectionRange>();

                List<int> sortedRows = new List<int>(rows.Order());
                int currMin = sortedRows[0];
                int currMax = currMin;

                for (int i = 1; i < sortedRows.Count; i++)
                {
                    if (sortedRows[i] == currMax + 1)
                    {
                        currMax++;
                    }
                    else
                    {
                        selection.Add(new SelectionRange(0, currMin, spreadsheet.MaxTableWidth, currMax));
                        currMin = sortedRows[i];
                        currMax = currMin;
                    }
                }

                selection.Add(new SelectionRange(0, currMin, spreadsheet.MaxTableWidth, currMax));

                spreadsheet.Selection = selection.ToImmutableList();
                return true;
            }

            return false;
        }

        private static string Transpose(string text, Regex rowSeparator, Regex columnSeparator, string quoteSymbol)
        {
            string[][] cells = Spreadsheet.SplitData(text, rowSeparator, columnSeparator, quoteSymbol, out int width);

            int height = cells.Length;

            string[,] outputMatrix = new string[height, width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < cells[y].Length; x++)
                {
                    outputMatrix[y, x] = cells[y][x];
                }
            }

            StringBuilder tbr = new StringBuilder();

            for (int y = 0; y < width; y++)
            {
                for (int x = 0; x < height; x++)
                {
                    string cell = outputMatrix[x, y];

                    if (string.IsNullOrEmpty(cell) || (!columnSeparator.IsMatch(cell) && !rowSeparator.IsMatch(cell)))
                    {
                        tbr.Append(outputMatrix[x, y]);
                    }
                    else
                    {
                        tbr.Append(quoteSymbol);
                        tbr.Append(outputMatrix[x, y]);
                        tbr.Append(quoteSymbol);
                    }

                    if (x < height - 1)
                    {
                        tbr.Append(columnSeparator);
                    }
                }

                if (y < width - 1)
                {
                    tbr.Append(rowSeparator);
                }
            }

            return tbr.ToString();
        }

        private static bool Contains(IEnumerable<SelectionRange> selection, (int x, int y) pos)
        {
            foreach (SelectionRange range in selection)
            {
                if (range.Left <= pos.x && range.Right >= pos.x && range.Top <= pos.y && range.Bottom >= pos.y)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<SelectionRange> Consolidate(IEnumerable<SelectionRange> selections)
        {
            List<SelectionRange> tbr = new List<SelectionRange>(selections);

            if (tbr.Count > 1000)
            {
                return tbr;
            }

            bool foundAny = true;

            while (foundAny)
            {
                foundAny = false;
                for (int i = 0; i < tbr.Count; i++)
                {
                    for (int j = 0; j < tbr.Count; j++)
                    {
                        if ((tbr[i].Right == tbr[j].Left - 1 && tbr[i].Top == tbr[j].Top && tbr[i].Bottom == tbr[j].Bottom) ||
                            (tbr[i].Bottom == tbr[j].Top - 1 && tbr[i].Left == tbr[j].Left && tbr[i].Right == tbr[j].Right))
                        {
                            tbr[i] = new SelectionRange(tbr[i].Left, tbr[i].Top, tbr[j].Right, tbr[j].Bottom);
                            tbr.RemoveAt(j);
                            foundAny = true;
                            break;
                        }
                    }

                    if (foundAny)
                    {
                        break;
                    }
                }
            }

            return tbr;
        }

        private static bool CanSort(IEnumerable<SelectionRange> selections)
        {
            List<SelectionRange> consolidated = Consolidate(selections);

            bool byRow = true;
            bool byColumn = true;

            for (int i = 1; i < consolidated.Count; i++)
            {
                if (!(consolidated[i].Top == consolidated[0].Top && consolidated[i].Bottom == consolidated[0].Bottom))
                {
                    byRow = false;
                }

                if (!(consolidated[i].Left == consolidated[0].Left && consolidated[i].Right == consolidated[0].Right))
                {
                    byColumn = false;
                }

                if (!byRow && !byColumn)
                {
                    break;
                }
            }

            return (byRow || byColumn) && consolidated.Count > 0;
        }

        private void Sort(Spreadsheet spreadsheet, bool? sortByRow, bool caseSensitive, (bool, int)[] indices)
        {
            string[,] data = spreadsheet.GetSelectedData(out (int, int)[,] coordinates);

            bool byRow;

            if (sortByRow == null)
            {
                byRow = data.GetLength(1) != 1;
            }
            else
            {
                byRow = sortByRow.Value;
            }

            string[][] sortableData;

            if (byRow)
            {
                sortableData = new string[data.GetLength(1)][];

                for (int i = 0; i < sortableData.Length; i++)
                {
                    sortableData[i] = new string[data.GetLength(0)];

                    for (int j = 0; j < sortableData[i].Length; j++)
                    {
                        sortableData[i][j] = data[j, i];
                    }
                }
            }
            else
            {
                sortableData = new string[data.GetLength(0)][];

                for (int i = 0; i < sortableData.Length; i++)
                {
                    sortableData[i] = new string[data.GetLength(1)];

                    for (int j = 0; j < sortableData[i].Length; j++)
                    {
                        sortableData[i][j] = data[i, j];
                    }
                }
            }

            if (indices == null)
            {
                string[] keys = sortableData.Select(x => x[0]).ToArray();
                string[] sortedKeys = keys.Order(Comparer<string>.Create((a, b) => CompareValues(a, b, caseSensitive))).ToArray();

                if (keys.SequenceEqual(sortedKeys))
                {
                    indices = new (bool, int)[] { (false, 0) };
                }
                else
                {
                    indices = new (bool, int)[] { (true, 0) };
                }
            }

            IOrderedEnumerable<string[]> sortingData = null;

            for (int i = 0; i < indices.Length; i++)
            {
                int ind = i;

                if (i == 0)
                {
                    if (indices[ind].Item1)
                    {
                        sortingData = sortableData.OrderBy(x => x[indices[ind].Item2], Comparer<string>.Create((a, b) => CompareValues(a, b, caseSensitive)));
                    }
                    else
                    {
                        sortingData = sortableData.OrderByDescending(x => x[indices[ind].Item2], Comparer<string>.Create((a, b) => CompareValues(a, b, caseSensitive)));
                    }
                }
                else
                {
                    if (indices[ind].Item1)
                    {
                        sortingData = sortingData.ThenBy(x => x[indices[ind].Item2], Comparer<string>.Create((a, b) => CompareValues(a, b, caseSensitive)));
                    }
                    else
                    {
                        sortingData = sortingData.ThenByDescending(x => x[indices[ind].Item2], Comparer<string>.Create((a, b) => CompareValues(a, b, caseSensitive)));
                    }
                }
            }

            string[][] sortedData = sortingData.ToArray();

            Dictionary<(int, int), string> newValues = new Dictionary<(int, int), string>();

            if (byRow)
            {
                for (int i = 0; i < sortedData.Length; i++)
                {
                    for (int j = 0; j < sortedData[i].Length; j++)
                    {
                        if (coordinates[j, i].Item1 >= 0 && coordinates[j, i].Item2 >= 0)
                        {
                            newValues.Add(coordinates[j, i], sortedData[i][j]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < sortedData.Length; i++)
                {
                    for (int j = 0; j < sortedData[i].Length; j++)
                    {
                        if (coordinates[i, j].Item1 >= 0 && coordinates[i, j].Item2 >= 0)
                        {
                            newValues.Add(coordinates[i, j], sortedData[i][j]);
                        }
                    }
                }
            }

            spreadsheet.SetData(newValues);
        }

        private static Regex LastBoundaryRegex = new Regex("\\b", RegexOptions.RightToLeft | RegexOptions.Compiled);

        private static object GetSortingKey(string value)
        {
            if (decimal.TryParse(value, out decimal v))
            {
                return v;
            }
            else
            {
                try
                {
                    int ind = LastBoundaryRegex.Match(value).Index;
                    if (decimal.TryParse(value.Substring(ind), out v))
                    {
                        return v;
                    }

                    return value;
                }
                catch
                {
                    return value;
                }
            }
        }

        private static int CompareValues(string value1, string value2, bool caseSensitive)
        {
            object v1 = GetSortingKey(value1);
            object v2 = GetSortingKey(value2);

            if (v1 is decimal val1 && v2 is decimal val2)
            {
                return Math.Sign(val1 - val2);
            }
            else
            {
                if (caseSensitive)
                {
                    return StringComparer.Ordinal.Compare(value1, value2);
                }
                else
                {
                    return StringComparer.OrdinalIgnoreCase.Compare(value1, value2);
                }
            }
        }

        private string GetLetter(int x)
        {
            if (x < 26)
            {
                return ((char)(x + 65)).ToString();
            }
            else
            {
                StringBuilder tbr = new StringBuilder();

                while (x >= 26)
                {
                    tbr.Insert(0, (char)((x % 26) + 65));
                    x = x / 26;
                }

                tbr.Insert(0, (char)(x + 64));

                return tbr.ToString();
            }
        }

        private async Task ShowCustomSortDialog(Spreadsheet spreadsheet)
        {
            spreadsheet.GetSelectedData(out (int, int)[,] coordinates);

            HashSet<int> columns = new HashSet<int>();
            HashSet<int> rows = new HashSet<int>();

            for (int x = 0; x < coordinates.GetLength(0); x++)
            {
                for (int y = 0; y < coordinates.GetLength(1); y++)
                {
                    columns.Add(coordinates[x, y].Item1);
                    rows.Add(coordinates[x, y].Item2);
                }
            }

            SpreadsheetCustomSortWindow win = new SpreadsheetCustomSortWindow(rows.Where(x => x >= 0).Select(x => "Row " + (x + 1).ToString()), columns.Where(x => x >= 0).Select(x => "Column " + GetLetter(x)));

            await win.ShowDialog2(this);

            if (win.Result != null)
            {
                Sort(spreadsheet, win.SortRows, win.CaseSensitive, win.Result);
            }
        }

        private async Task Save(Spreadsheet spreadsheet)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Formatted CSV file", Extensions = new List<string>() { "csvf" } }, new FileDialogFilter() { Name = "CSV file", Extensions = new List<string>() { "csv" } } }, Title = "Export spreadsheet..." };

            string result = await dialog.ShowAsync(this);

            if (!string.IsNullOrEmpty(result))
            {
                using (StreamWriter sw = new StreamWriter(result))
                {

                    sw.Write(spreadsheet.SerializeData());
                    if (Path.GetExtension(result).Equals(".csvf", StringComparison.OrdinalIgnoreCase))
                    {
                        sw.Write(spreadsheet.RowSeparator);
                        sw.Write("----------");
                        sw.Write(spreadsheet.RowSeparator);
                        sw.Write("## FORMAT");
                        sw.Write(spreadsheet.RowSeparator);
                        sw.Write(spreadsheet.SerializeFormat());
                    }
                }
            }
        }

        static Regex[] DefaultSeparators = new Regex[] { new Regex("\\t", RegexOptions.Compiled), new Regex(",", RegexOptions.Compiled), new Regex(";", RegexOptions.Compiled), new Regex(" ", RegexOptions.Compiled), new Regex("[\\t ]+", RegexOptions.Compiled), new Regex(":", RegexOptions.Compiled) };

        private static Regex GetColumnSeparator(string text, Regex hintSeparator, Regex rowSeparator, string quote)
        {
            Regex[] possibleSeparators = new Regex[] { hintSeparator }.Concat(DefaultSeparators).ToArray();

            double minVariance = double.MaxValue;
            int maxCount = 0;
            int bestIndex = -1;

            for (int i = 0; i < possibleSeparators.Length; i++)
            {
                try
                {
                    double variance = Spreadsheet.SplitData(text, rowSeparator, possibleSeparators[i], quote, out int width).Select(x => (double)x.Length).Variance();

                    if ((width > 1 || maxCount == 0) && (variance < minVariance || (variance == minVariance && width > maxCount)))
                    {
                        minVariance = variance;
                        maxCount = width;
                        bestIndex = i;
                    }
                }
                catch { }
            }

            return possibleSeparators[Math.Max(0, bestIndex)];
        }

        public string OriginalText { get; set; } = "";

        private async Task Open(Spreadsheet spreadsheet)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>()
            {
                new FileDialogFilter() { Name = "All spreadsheets", Extensions = new List<string>() { "csv", "csvf", "tsv", "txt" } },
                new FileDialogFilter() { Name = "CSV file", Extensions = new List<string>() { "csv", "tsv" } },
                new FileDialogFilter() { Name = "Formatted CSV file", Extensions = new List<string>() { "csvf" } },
                new FileDialogFilter() { Name = "Text file", Extensions = new List<string>() { "txt" } },
                new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } }
            };

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open spreadsheet",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open spreadsheet",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                string text = File.ReadAllText(result[0]);
                this.OriginalText = text;

                Load(spreadsheet, text, false);
            }
        }

        public void Load(Spreadsheet spreadsheet, string text, bool forceSeparator)
        {
            string[] lines = spreadsheet.RowSeparator.Split(text);

            if (!forceSeparator)
            {
                string rowSep = spreadsheet.GetRowSeparator();
                Regex columnSeparator = GetColumnSeparator(lines.Take(Math.Min(lines.Length, 50)).TakeWhile(x => !x.StartsWith("----------")).Aggregate((a, b) => a + rowSep + b), spreadsheet.ColumnSeparator, spreadsheet.RowSeparator, spreadsheet.QuoteSymbol);
                spreadsheet.ColumnSeparator = columnSeparator;
            }

            int i = 0;

            StringBuilder data = new StringBuilder();

            while (i < lines.Length)
            {
                if (!lines[i].StartsWith("----------") || i >= lines.Length - 1 || !lines[i + 1].StartsWith("## FORMAT"))
                {
                    data.Append(lines[i]);
                    data.Append(spreadsheet.RowSeparator);

                    i++;
                }
                else
                {
                    break;
                }
            }

            StringBuilder format = null;

            if (i < lines.Length - 1 && lines[i].StartsWith("----------") && lines[i + 1].StartsWith("## FORMAT"))
            {
                format = new StringBuilder();

                for (int j = i; j < lines.Length; j++)
                {
                    if (spreadsheet.ColumnSeparator.IsMatch("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans"))
                    {
                        lines[j] = lines[j].Replace(spreadsheet.ColumnSeparator + "Open Sans" + spreadsheet.ColumnSeparator, spreadsheet.ColumnSeparator + spreadsheet.QuoteSymbol + "resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans".Replace(spreadsheet.QuoteSymbol, spreadsheet.QuoteSymbol + spreadsheet.QuoteSymbol) + spreadsheet.QuoteSymbol + spreadsheet.ColumnSeparator);
                    }
                    else
                    {
                        lines[j] = lines[j].Replace(spreadsheet.ColumnSeparator + "Open Sans" + spreadsheet.ColumnSeparator, spreadsheet.ColumnSeparator + "resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans" + spreadsheet.ColumnSeparator);
                    }

                    if (spreadsheet.ColumnSeparator.IsMatch("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono"))
                    {
                        lines[j] = lines[j].Replace(spreadsheet.ColumnSeparator + "Roboto Mono" + spreadsheet.ColumnSeparator, spreadsheet.ColumnSeparator + spreadsheet.QuoteSymbol + "resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono".Replace(spreadsheet.QuoteSymbol, spreadsheet.QuoteSymbol + spreadsheet.QuoteSymbol) + spreadsheet.QuoteSymbol + spreadsheet.ColumnSeparator);
                    }
                    else
                    {
                        lines[j] = lines[j].Replace(spreadsheet.ColumnSeparator + "Roboto Mono" + spreadsheet.ColumnSeparator, spreadsheet.ColumnSeparator + "resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono" + spreadsheet.ColumnSeparator);
                    }


                    format.Append(lines[j]);
                    format.Append(spreadsheet.RowSeparator);
                }
            }

            spreadsheet.Load(data.ToString(), format?.ToString() ?? "");



            programmaticChange = true;
            fontSizeNud.Value = spreadsheet.FontSize;
            programmaticChange = false;
        }

        InterprocessDebuggerServer DebuggerServer = null;

        private async Task TransformAndPaste(Spreadsheet spreadsheet)
        {
            SpreadsheetTransformTemplateDialog dialog = new SpreadsheetTransformTemplateDialog();

            await dialog.ShowDialog2(this);

            if (dialog.Result >= 0)
            {
                this.FindControl<Canvas>("BusyOverlay").IsVisible = true;
                Avalonia.Input.Cursor previousCursor = this.Cursor;
                this.Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Wait);

                string text = await Application.Current.Clipboard.GetTextAsync();

                await Task.Delay(10);

                CodeEditorWindow win = new CodeEditorWindow();

                if (this.DebuggerServer == null)
                {
                    this.DebuggerServer = Modules.GetNewDebuggerServer();
                }

                await win.FinishInitialization(TransformTemplates[dialog.Result], this.DebuggerServer, "CodeEditor_TransformAndPaste_Spreadsheet");

                win.Opened += (s, e) =>
                {
                    this.FindControl<Canvas>("BusyOverlay").IsVisible = false;
                    this.Cursor = previousCursor;
                };

                await win.ShowDialog2(this);

                if (win.Result != null && win.Result.CompiledAssembly != null)
                {
                    int methodType = -1;

                    Type type = win.Result.CompiledAssembly.GetType("TransformAndPaste.Transformation");
                    System.Reflection.MethodInfo mInfo = type.GetMethod("Transform");
                    System.Reflection.ParameterInfo[] parameters = mInfo?.GetParameters();


                    if (parameters != null)
                    {
                        if (parameters.Length == 3 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(int))
                        {
                            if (mInfo.ReturnType == typeof(string))
                            {
                                methodType = 0;
                            }
                            else if (mInfo.ReturnType == typeof(decimal?))
                            {
                                methodType = 1;
                            }
                        }
                        else if (parameters.Length == 3 && parameters[0].ParameterType == typeof(decimal?) && parameters[1].ParameterType == typeof(int) && parameters[1].ParameterType == typeof(int))
                        {
                            if (mInfo.ReturnType == typeof(string))
                            {
                                methodType = 3;
                            }
                            else if (mInfo.ReturnType == typeof(decimal?))
                            {
                                methodType = 2;
                            }
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string[,]))
                        {
                            if (mInfo.ReturnType == typeof(string[,]))
                            {
                                methodType = 4;
                            }
                            else if (mInfo.ReturnType == typeof(decimal?[,]))
                            {
                                methodType = 5;
                            }
                        }
                        else if (parameters.Length == 1 && parameters[0].ParameterType == typeof(decimal?[,]))
                        {
                            if (mInfo.ReturnType == typeof(string[,]))
                            {
                                methodType = 7;
                            }
                            else if (mInfo.ReturnType == typeof(decimal?[,]))
                            {
                                methodType = 6;
                            }
                        }
                    }

                    if (methodType < 0)
                    {
                        await new MessageBox("Attention!", "Could not find a valid transformation method! Please use one of the templates!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Warning).ShowDialog2(this);
                    }
                    else
                    {
                        try
                        {
                            string[][] cells = Spreadsheet.SplitData(text, spreadsheet.RowSeparator, spreadsheet.ColumnSeparator, spreadsheet.QuoteSymbol, out int width);
                            int height = cells.Length;

                            string[,] matrix = null;

                            if (methodType < 6)
                            {
                                matrix = new string[width, height];

                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < cells[y].Length; x++)
                                    {
                                        matrix[x, y] = cells[y][x];
                                    }
                                }

                                if (methodType < 4)
                                {
                                    for (int x = 0; x < matrix.GetLength(0); x++)
                                    {
                                        for (int y = 0; y < matrix.GetLength(1); y++)
                                        {
                                            switch (methodType)
                                            {
                                                case 0:
                                                    matrix[x, y] = (string)mInfo.Invoke(null, new object[] { matrix[x, y], x, y });
                                                    break;
                                                case 1:
                                                    matrix[x, y] = ((decimal?)mInfo.Invoke(null, new object[] { matrix[x, y], x, y }))?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    break;
                                                case 2:
                                                    if (decimal.TryParse(matrix[x, y], System.Globalization.CultureInfo.InvariantCulture, out decimal v))
                                                    {
                                                        matrix[x, y] = ((decimal?)mInfo.Invoke(null, new object[] { (decimal?)v, x, y }))?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    }
                                                    else
                                                    {
                                                        matrix[x, y] = ((decimal?)mInfo.Invoke(null, new object[] { (decimal?)null, x, y }))?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                    }
                                                    break;
                                                case 3:
                                                    if (decimal.TryParse(matrix[x, y], System.Globalization.CultureInfo.InvariantCulture, out v))
                                                    {
                                                        matrix[x, y] = (string)mInfo.Invoke(null, new object[] { (decimal?)v, x, y });
                                                    }
                                                    else
                                                    {
                                                        matrix[x, y] = (string)mInfo.Invoke(null, new object[] { (decimal?)null, x, y });
                                                    }
                                                    break;
                                            }
                                        }
                                    }
                                }
                                else if (methodType == 4)
                                {
                                    matrix = (string[,])mInfo.Invoke(null, new object[] { matrix });
                                }
                                else if (methodType == 5)
                                {
                                    decimal?[,] decimalMatrix = (decimal?[,])mInfo.Invoke(null, new object[] { matrix });

                                    matrix = new string[decimalMatrix.GetLength(0), decimalMatrix.GetLength(1)];

                                    for (int x = 0; x < decimalMatrix.GetLength(0); x++)
                                    {
                                        for (int y = 0; y < decimalMatrix.GetLength(1); y++)
                                        {
                                            matrix[x, y] = decimalMatrix[x, y]?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                    }
                                }
                            }
                            else if (methodType == 6 || methodType == 7)
                            {
                                decimal?[,] decimalMatrix = new decimal?[width, height];

                                for (int y = 0; y < height; y++)
                                {
                                    for (int x = 0; x < cells[y].Length; x++)
                                    {
                                        if (decimal.TryParse(cells[y][x], out decimal v))
                                        {
                                            decimalMatrix[x, y] = v;
                                        }
                                        else
                                        {
                                            decimalMatrix[x, y] = null;
                                        }
                                    }
                                }

                                if (methodType == 6)
                                {
                                    decimalMatrix = (decimal?[,])mInfo.Invoke(null, new object[] { decimalMatrix });

                                    matrix = new string[decimalMatrix.GetLength(0), decimalMatrix.GetLength(1)];

                                    for (int x = 0; x < decimalMatrix.GetLength(0); x++)
                                    {
                                        for (int y = 0; y < decimalMatrix.GetLength(1); y++)
                                        {
                                            matrix[x, y] = decimalMatrix[x, y]?.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                        }
                                    }
                                }
                                else if (methodType == 7)
                                {
                                    matrix = (string[,])mInfo.Invoke(null, new object[] { decimalMatrix });
                                }
                            }

                            StringBuilder tbr = new StringBuilder();

                            for (int y = 0; y < matrix.GetLength(1); y++)
                            {
                                for (int x = 0; x < matrix.GetLength(0); x++)
                                {
                                    string cell = matrix[x, y];

                                    if (string.IsNullOrEmpty(cell) || (!spreadsheet.ColumnSeparator.IsMatch(cell) && !spreadsheet.RowSeparator.IsMatch(cell)))
                                    {
                                        tbr.Append(matrix[x, y]);
                                    }
                                    else
                                    {
                                        tbr.Append(spreadsheet.QuoteSymbol);
                                        tbr.Append(matrix[x, y]);
                                        tbr.Append(spreadsheet.QuoteSymbol);
                                    }

                                    if (x < matrix.GetLength(0) - 1)
                                    {
                                        tbr.Append(spreadsheet.ColumnSeparator);
                                    }
                                }

                                if (y < matrix.GetLength(1) - 1)
                                {
                                    tbr.Append(spreadsheet.RowSeparator);
                                }
                            }

                            spreadsheet.Paste(tbr.ToString(), true);
                        }
                        catch (Exception ex)
                        {
                            string message = "An error occurred while transforming the data!\n" + ex.Message;

                            if (ex.InnerException != null)
                            {
                                message += "\n" + ex.InnerException.Message;
                            }

                            await new MessageBox("Attention!", message).ShowDialog2(this);
                        }
                    }
                }
            }
        }

        private async Task OpenColourPickerWindow(Spreadsheet spreadsheet)
        {
            if (spreadsheet.Selection.Count > 0)
            {
                Color? previousColor = null;

                if (spreadsheet.Data.TryGetValue((spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top), out string colTxt) && !string.IsNullOrEmpty(colTxt) && colTxt.StartsWith("#") && (colTxt.Length == 7 || colTxt.Length == 9))
                {
                    try
                    {
                        previousColor = SolidColorBrush.Parse(colTxt.Length == 7 ? colTxt : ("#" + colTxt.Substring(7, 2) + colTxt.Substring(1, 6))).Color;
                    }
                    catch { }
                }

                ColourPickerWindow cpw = new ColourPickerWindow(previousColor);
                Color? col = await cpw.ShowDialog(this);
                if (col != null && col != previousColor)
                {
                    string colString = "#" + col.Value.R.ToString("X2") + col.Value.G.ToString("X2") + col.Value.B.ToString("X2") + (col.Value.A != 255 ? col.Value.A.ToString("X2") : "");
                    if (spreadsheet.Selection.Any(x => (x.Top == 0 && x.Bottom == spreadsheet.MaxTableHeight) || (x.Left == 0 && x.Right == spreadsheet.MaxTableWidth)))
                    {
                        spreadsheet.SetData(new KeyValuePair<(int, int), string>[] { new KeyValuePair<(int, int), string>((spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top), colString) });
                        spreadsheet.Selection = ImmutableList.Create(new SelectionRange(spreadsheet.Selection[0].Left, spreadsheet.Selection[0].Top));
                    }
                    else
                    {
                        Dictionary<(int, int), string> newData = new Dictionary<(int, int), string>();

                        foreach (SelectionRange range in spreadsheet.Selection)
                        {
                            for (int y = range.Top; y <= range.Bottom; y++)
                            {
                                for (int x = range.Left; x <= range.Right; x++)
                                {
                                    newData[(x, y)] = colString;
                                }
                            }
                        }
                        spreadsheet.SetData(newData);
                    }
                }
                spreadsheet.Focus();
            }
        }

        static readonly string[] TransformTemplates = new string[]
        {
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the input value appropriately,
        // and return a string containing the updated value. Return
        // null to signify an empty cell.
        //
        // Parameters:
        //     *  value: the value of a single cell. If the cell was empty,
        //               the value will be null.
        //     * column: the horizontal coordinate of the cell
        //               (starting from 0).
        //     *    row: the vertical coordinate of the cell (starting
        //               from 0).
        //
        // Note that column and row are relative to the copied area. For
        // example, if you copy the region B3:D8, cell B3 will have
        // column 0 and row 0, cell C4 will have column 1 and row 1,
        // cell B6 will have column 3 and row 0, and so on.
        public static string Transform(string value, int column, int row)
        {
            return value;
        }
    }
}",
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the input value appropriately,
        // and return the updated value as a decimal. Return
        // null to signify an empty cell.
        //
        // Parameters:
        //     *  value: the value of a single cell. If the cell was empty,
        //               the value will be null.
        //     * column: the horizontal coordinate of the cell
        //               (starting from 0).
        //     *    row: the vertical coordinate of the cell (starting
        //               from 0).
        //
        // Note that column and row are relative to the copied area. For
        // example, if you copy the region B3:D8, cell B3 will have
        // column 0 and row 0, cell C4 will have column 1 and row 1,
        // cell B6 will have column 3 and row 0, and so on.
        public static decimal? Transform(string value, int column, int row)
        {
            if (decimal.TryParse(value, out decimal parsedValue))
            {
                return parsedValue;
            }
            else
            {
                return null;
            }
        }
    }
}",
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the input value appropriately,
        // and return the updated value as a decimal. Return
        // null to signify an empty cell.
        //
        // Parameters:
        //     *  value: the value of a single cell. If the cell did not
        //               contain a number, the value will be null.
        //     * column: the horizontal coordinate of the cell
        //               (starting from 0).
        //     *    row: the vertical coordinate of the cell (starting
        //               from 0).
        //
        // Note that column and row are relative to the copied area. For
        // example, if you copy the region B3:D8, cell B3 will have
        // column 0 and row 0, cell C4 will have column 1 and row 1,
        // cell B6 will have column 3 and row 0, and so on.
        public static decimal? Transform(decimal? value, int column, int row)
        {
            return value;
        }
    }
}",
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the input value appropriately,
        // and return the updated value as a string. Return
        // null to signify an empty cell.
        //
        // Parameters:
        //     *  value: the value of a single cell. If the cell did not
        //               contain a number, the value will be null.
        //     * column: the horizontal coordinate of the cell
        //               (starting from 0).
        //     *    row: the vertical coordinate of the cell (starting
        //               from 0).
        //
        // Note that column and row are relative to the copied area. For
        // example, if you copy the region B3:D8, cell B3 will have
        // column 0 and row 0, cell C4 will have column 1 and row 1,
        // cell B6 will have column 3 and row 0, and so on.
        public static string Transform(decimal? value, int column, int row)
        {
            return value?.ToString();
        }
    }
}",

            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the values in the input array
        // appropriately and return an array containing the transformed
        // values. The arrays may have a different size. Empty cells
        // should have a value of null.
        //
        // Parameters:
        //     * values: the values of all the cells that have been copied.
        //               If a cell was empty, its value will be null.
        //
        public static string[,] Transform(string[,] values)
        {
            return values;
        }
    }
}",
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the values in the input array
        // appropriately and return an array of decimals containing the
        // transformed values. The arrays may have a different size. Empty
        // cells should have a value of null.
        //
        // Parameters:
        //     * values: the values of all the cells that have been copied.
        //               If a cell was empty, its value will be null.
        //
        public static decimal?[,] Transform(string[,] values)
        {
            decimal?[,] newValues = new decimal?[values.GetLength(0), values.GetLength(1)];

            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    if (decimal.TryParse(values[i, j], out decimal v))
                    {
                        newValues[i, j] = v;
                    }
                    else
                    {
                        newValues[i, j] = null;
                    }
                }
            }
            
            return newValues;
        }
    }
}",

            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the values in the input array
        // appropriately and return an array of decimals containing the
        // transformed values. The arrays may have a different size. Empty
        // cells should have a value of null.
        //
        // Parameters:
        //     * values: the values of all the cells that have been copied.
        //               If a cell did not contain a number, its value will be null.
        //
        public static decimal?[,] Transform(decimal?[,] values)
        {
            return values;
        }
    }
}",
            @"using System;
using System.Linq;

// Do not change namespace
namespace TransformAndPaste
{
    // Do not change class name
    public static class Transformation
    {
        // This method should transform the values in the input array
        // appropriately and return an array of strings containing the
        // transformed values. The arrays may have a different size. Empty
        // cells should have a value of null.
        //
        // Parameters:
        //     * values: the values of all the cells that have been copied.
        //               If a cell did not contain a number, its value will be null.
        //
        public static string[,] Transform(decimal?[,] values)
        {
            string[,] newValues = new string[values.GetLength(0), values.GetLength(1)];

            for (int i = 0; i < values.GetLength(0); i++)
            {
                for (int j = 0; j < values.GetLength(1); j++)
                {
                    newValues[i, j] = values[i, j]?.ToString();
                }
            }
            
            return newValues;
        }
    }
}",
        };

    }
}
