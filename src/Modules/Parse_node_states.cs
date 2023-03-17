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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using Avalonia;
using Avalonia.Controls;
using VectSharp;
using System.Runtime.InteropServices;

namespace ParseTipStates
{
    /// <summary>
    /// This module can be used to parse attributes for the nodes of the tree from a separate file loaded as an attachment.
    /// </summary>
    /// 
    /// <description>
    /// <![CDATA[
    /// ## Further information
    /// 
    /// This module can be used to read "complex" attributes from a text file. If you just wish to load "presence-absence"
    /// data, the _Add attribute_ module may also be suited to your needs.
    /// 
    /// This module reads each line of the text file and splits it using the selected separator. You can choose which column of
    /// the file contains the value that will be matched against the specified attribute of the nodes. Other columns represent the
    /// attributes that will be attached to the matched taxon.
    /// 
    /// More than one attribute can be parsed at once; if a header row is present, this can be used to specify the names of the
    /// attributes that are loaded. Alternatively, the attributes that are loaded can be specified by the [column header(s)](#column-headers)
    /// parameter. These should correspond to the headers of the columns in the file. If the [match column](#match-column) is `1`, the
    /// header for the first column (which is matched against the [match attribute](#match-attribute)) can be skipped. Otherwise, a
    /// header must be provided for the [match column](#match-column), even though it is not used.
    /// 
    /// For example, assume that:
    ///  * the [separator](#separator) is `\s` (i.e. whitespace)
    ///  * the [match column](#match-column) is `1`
    ///  * the [match attribute](#match-attribute) is `Name` (and the [match attribute type](#match-attribute-type) is `String`)
    ///  * the [column header(s)](#column-headers) are `State1 State2`
    /// 
    /// In this case, the following file would assign a `State1` of `A` and a `State2` of `5` to the taxon named `Nostoc`, and a
    /// `State1` of `B` and a `State2` of `3` to the taxon named `Synechococcus`:
    /// 
    /// ```
    /// Nostoc          A   5
    /// Synechococcus   B   3
    /// ```
    ///
    /// Instead, consider the case in which:
    ///  * the [separator](#separator) is `,` (i.e. a comma)
    ///  * the [match column](#match-column) is `2`
    ///  * the [match attribute](#match-attribute) is `Name` (and the [match attribute type](#match-attribute-type) is `String`)
    ///  * the [column header(s)](#column-headers) are `State1,Genus,State2`
    /// 
    /// Here, the following file would assign a `State1` of `A` and a `State2` of `5` to the taxon named `Nostoc`, and a
    /// `State1` of `B` and a `State2` of `3` to the taxon named `Synechococcus`:
    /// 
    /// ```
    /// A,Nostoc,5
    /// B,Synechococcus,3
    /// ```
    /// 
    /// The `Preview` button can be used to display a preview of how the data in the file will be parsed based on the current settings.
    /// ]]>
    /// </description>

    public static class MyModule
    {
        public const string Name = "Parse node states";
        public const string HelpText = "Loads node state data from an attachment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("2.1.3");
        public const string Id = "716b55a3-02d9-4007-a830-8326d407b24c";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABlSURBVDhPY2SYV/yfARdI6mUEUUVFRTjVMEFpssGoAVQwYOABY2fkVpyJpHy5NzghIQOv2o3/tzX7w8VHUjSC/A7EDlC2A4gPYpPiAkcg3g9hgmkQn3gDgCF/AEiBNYFoCJ+BAQA6NRzgaIL55gAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACISURBVEhLY2SYV/yfgViQ1MsIooqKiojWwwSlaQZGLSAIRi0gCEYtGAGAsTNyK9EFV/lyb3Bhhw141W78v63ZH0N+NBURBKMWEAQUWQBKmkDsAOWCAYgPEodyKfaBIxDvh1kCpfdDxcGAIguAGesAkAJbAhaAGg4VBwOK4wDJEhBAMZyBgYEBAM31JvCuv471AAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACzSURBVFhHY2SYV/yfgRyQ1MsIZYFBUVERWeYwQekBA6MOGHXAqANGHTDqgFEHjIJRwNgZuZWslkz5cm+UFhEu4FW7EWz+tmZ/rOpHs+GoA0YdMPwcAMz3W0B5H4gdoEJwABKDym2BCtEkBLyh9H5kR0DZ+yE8uBqaOMARSoNACZQGAWQ2XA3VHQAscg8AKZgFcJ8CAYztCFUDBjRJhGiOQAYoloMATRwAAlgcgWE5AwMDAwDUYDD82LV19wAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEMSURBVGhD7ZixDoIwFEWLX6arDs6ufgIOTkaNk4O/4OrsoCv+GfbBk5RYWjSEC8k9CXktgXJPgDxCYq6b3HTN+pLoqEaapp1fa6J1tFAADQXQUAANBdBQAA0F0FAADQXQUADN6AUIIQRLcl49Ov/dt70tvL8WY8x394Mt+3Jmjs/TUuZBhtYHPuEFd9wIOzEaCqChABoKoOldQLqt3XLdpro7ihzrnFd1aMQdcDts1kZCj8nKWUG1xhAeoaCEJ3wNhMBMq4tXIhC+WqN3AfuF+bLFK6HVxRte1yiAPEIBiRi18ALsHfhD4iu8AH2Jf5DwhhegAkILicbwAlxACEgEwwuDEBA8EtHwxhjzBue3Vijn46yZAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFuSURBVHhe7ZqxbsIwGISdPlm7tgNz1z5COnSqoGLqwCuwMneAlb4ZzcGBkta4NqDIv/77pOjiSLFynyCypTRh+boLY/KyaHgWpW3bUZ/njukWCWC6RQKYbpEAplskgOkWCWC6RQKYbpEAplskgOkWCWC6RQKYbpEAplskgOkWCWAKIYQQQrij+Xxej/pNztvqKfmNUCmP71+zLqaHUfjYzCcYZ2N6JfirPJjyWjbWl8L98kdi186izRDTLRLAdIsEMN0iAUy3SADTLVULwLq+O3Y87nm5GNzbm2ewV6hWAB+0v67fogjPs+E928Noz2DDVPMvILapKZIQKX/kNLfFd0CWhET5ATULeGDGSErIKH+au1oBm/nku4ukBGaMZHnOvafqv0CGhFIG5UH174AbSvhTHph4Cd5AQrQ8MCEAXCHhbHlgRgC4QEKyPDAlABRI+Lc8MCcAZEjIKg9MCgAJCdnlgVkBICKhqHwIIfwAVld0RFDqXp8AAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static Page GetIcon32(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon32Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon48Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon64Base64);
            }

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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "ParentWindow", "Window:" ),

                ("Parsing", "Group:4"),
                
                /// <param name="Data file:">
                /// This parameter is used to select the attachment that contains the data file to parse.
                /// </param>
                ("Data file:", "Attachment:"),
                
                /// <param name="Lines to skip:">
                /// This parameter determines the lines to skip at the start of the file (useful e.g. if the data file contains
                /// header lines). Note that if you want to [use the header row to specify the column headers](#use-first-row-as-header),
                /// that row must NOT be skipped.
                /// </param>
                ("Lines to skip:", "NumericUpDown:0[\"0\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="Separator:" default="`\s`">
                /// This parameter contains the separator used to split the lines of the data file. If the [Regex](#regex) checkbox
                /// is checked, regex escape characters can be used. These include:
                /// 
                /// * `\t` matches a tabulation
                /// * `\s` matches a whitespace character (e.g. a space or a tabulation)
                /// 
                /// Note that, since empty elements are discarded anyways, it is not a problem if multiple instances of the separator
                /// occur in sequence (e.g. `A     B` is parsed just as well as `A B`).
                /// </param>
                ("Separator:", "TextBox:\\s"),
                
                /// <param name="Regex">
                /// If this check box is checked, the separator is matched using a regular expression. This makes it possible e.g. to
                /// use escape characters or to perform advanced matching (for example, if this option is active, a separator of `\s|,|;`
                /// could be used to parse a file in which the states are separated by spaces, commas and/or semicolons).
                /// </param>
                ("Regex", "CheckBox:true"),

                ("Matching", "Group:3"),

                /// <param name="Match column:">
                /// This parameter determines the column that contains the values that are matched against the [match attribute](#match-attribute).
                /// </param>
                ("Match column:", "NumericUpDown:1[\"1\",\"Infinity\",\"1\",\"0\"]"),

                /// <param name="Match attribute:">
                /// This parameter determines the attribute that is matched against the values in the [match column](#match-column).
                /// </param>
                ("Match attribute:", "AttributeSelector:Name"),

                /// <param name="Match attribute type:">
                /// This parameter determines the type of the [match attribute](#match-attribute).
                /// </param>
                ("Match attribute type:", "AttributeType:String"),

                ("New attribute", "Group:4"),
                
                /// <param name="Attribute(s):" default="`State`" display="">
                /// This parameter determines the name of the attribute in which the parsed states are stored. If more than one attribute
                /// should be parsed from the file, the value of this parameter can be set to a string that will be split based on the same
                /// separator that is used for the data (e.g. if the separator is `;` and the attributes to be parsed are called `State1`
                /// and `State2`, a possible value for this parameter could be `State1;State2`).
                /// </param>
                ("Attribute(s):", "TextBox:State"),

                /// <param name="Use first row as header">
                /// If this check box is checked, the first row in the data file is assumed to be a header row, containing the names of the
                /// attribute(s) where the parsed states are stored. If this check box is unchecked, the column headers need to be specified
                /// manually in the [column header(s)](#column-headers) parameter.
                /// </param>
                ("Use first row as header", "CheckBox:false" ),

                /// <param name="Column header(s):" default="`State`">
                /// This parameter should contain the headers for the columns of data in the file. These will also be used as the names of
                /// the attribute(s) where the parsed states are stored. The names of different columns should be separated using the same
                /// separator that is used for the data (e.g. if the separator is `;` and the attributes to be parsed are called `State1`
                /// and `State2`, a possible value for this parameter could be `State1;State2`).
                /// 
                /// If the [match column](#match-column) is `1`, the header for the first column can be omitted. Otherwise, headers should
                /// be provided for all columns; columns in the data without a corresponding header will be ignored.
                /// </param>
                ("Column header(s):", "TextBox:State"),

                ("Multiple attributes should use the same separator as the data", "Label:[\"Left\",\"Italic\",\"#808080\"]"),

                /// <param name="Attribute type: ">
                /// This parameter determines the type of the attribute that is parsed. If this is `String`, the attribute is stored as a
                /// string, even if the contents represent a number (e.g. the number `1` would be stored as the string `"1"`). Depending on
                /// how you intend to analyse the data, this may or may not be your intended behaviour - e.g. if the attribute represents
                /// a discrete character state, it is appropriate to parse it as a string; if it is instead a continuous character state,
                /// parsing it as a number may be more appropriate.
                /// 
                /// If the selected value is `String` or `Number`, this will be applied to all the columns in the data. Instead, if the
                /// selected value is `Auto`, each attribute will be assessed independently to determine whether it can be represented as
                /// a String or a Number.
                /// </param>
                ("Attribute type: ", "ComboBox:2[\"String\",\"Number\",\"Auto\"]"),
                
                /// <param name="PreviewApplyButtons" display="Preview/Apply">
                /// The `Preview` button shows a preview of the data that will be parsed from the selected attachment file. The `Apply`
                /// button applies the changes to the other parameters and signals to the downstream modules that the tree should be
                /// redrawn.
                /// </param>
                ("PreviewApplyButtons", "Buttons:[\"Preview\",\"Apply\"]")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { { "Attribute(s):", ControlStatus.Hidden } };
            parametersToChange = new Dictionary<string, object>() { { "PreviewApplyButtons", -1 } };

            if ((string)currentParameterValues["Attribute(s):"] != "State" && (string)currentParameterValues["Column header(s):"] == "State")
            {
                parametersToChange["Column header(s):"] = (string)currentParameterValues["Attribute(s):"];
            }

            if ((string)previousParameterValues["Match attribute:"] != (string)currentParameterValues["Match attribute:"])
            {
                string attributeName = (string)currentParameterValues["Match attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Match attribute type:"] == (string)currentParameterValues["Match attribute type:"])
                {
                    parametersToChange.Add("Match attribute type:", attrType);
                }
            }

            Attachment previousAttachment = (Attachment)previousParameterValues["Data file:"];
            Attachment newAttachment = (Attachment)currentParameterValues["Data file:"];

            if ((int)currentParameterValues["PreviewApplyButtons"] == 0)
            {
                ShowPreview((MainWindow)currentParameterValues["ParentWindow"], currentParameterValues);
            }

            if ((bool)currentParameterValues["Use first row as header"])
            {
                controlStatus["Column header(s):"] = ControlStatus.Hidden;
                controlStatus["Multiple attributes should use the same separator as the data"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Column header(s):"] = ControlStatus.Enabled;
                controlStatus["Multiple attributes should use the same separator as the data"] = ControlStatus.Enabled;
            }

            return (int)currentParameterValues["PreviewApplyButtons"] == 1 || (previousAttachment != newAttachment);
        }

        private static async void ShowPreview(MainWindow window, Dictionary<string, object> parameterValues)
        {
            if (window != null)
            {
                try
                {
                    int skipLines = (int)(double)parameterValues["Lines to skip:"];
                    string separatorString = (string)parameterValues["Separator:"];
                    bool separatorRegex = (bool)parameterValues["Regex"];

                    int attributeType = (int)parameterValues["Attribute type: "];

                    string attributeName = (string)parameterValues["Column header(s):"];
                    Attachment attachment = (Attachment)parameterValues["Data file:"];
                    int matchColumn = (int)(double)parameterValues["Match column:"];
                    string matchAttribute = (string)parameterValues["Match attribute:"];
                    string matchAttributeType = (string)parameterValues["Match attribute type:"];

                    bool useHeaderRow = (bool)parameterValues["Use first row as header"];

                    if (attachment != null)
                    {
                        Regex separator;

                        if (separatorRegex)
                        {
                            separator = new Regex(separatorString);
                        }
                        else
                        {
                            separator = new Regex(Regex.Escape(separatorString));
                        }

                        string[] lines = attachment.GetLines();

                        List<object[]> attributes = new List<object[]>();

                        string[] attributeNames;

                        if (useHeaderRow)
                        {
                            try
                            {
                                string[] splitLine = (from el in separator.Split(lines[skipLines]) where !string.IsNullOrEmpty(el) select el).ToArray();
                                attributeNames = splitLine;
                            }
                            catch
                            {
                                attributeNames = new string[0];
                            }
                        }
                        else
                        {
                            attributeNames = (from el in separator.Split(Regex.Unescape(attributeName)) where !string.IsNullOrEmpty(el) select el.Trim()).ToArray();
                        }

                        for (int i = skipLines + (useHeaderRow ? 1 : 0); i < lines.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(lines[i]))
                            {
                                string[] splitLine = (from el in separator.Split(lines[i]) where !string.IsNullOrEmpty(el) select el).ToArray();
                                if (splitLine.Length > matchColumn)
                                {
                                    object[] attribute = new object[splitLine.Length];
                                    for (int j = 0; j < splitLine.Length; j++)
                                    {
                                        if (attributeType == 0)
                                        {
                                            attribute[j] = splitLine[j];
                                        }
                                        else if (attributeType == 1)
                                        {
                                            if (double.TryParse(splitLine[j], out double parsed))
                                            {
                                                attribute[j] = parsed;
                                            }
                                            else
                                            {
                                                attribute[j] = double.NaN;
                                            }
                                        }
                                        else if (attributeType == 2)
                                        {
                                            if (double.TryParse(splitLine[j], out double parsed))
                                            {
                                                attribute[j] = parsed;
                                            }
                                            else
                                            {
                                                attribute[j] = splitLine[j];
                                            }
                                        }
                                    }

                                    attributes.Add(attribute);
                                }
                            }
                        }


                        ChildWindow previewWindow = new ChildWindow() { FontFamily = window.FontFamily, FontSize = 14, Icon = window.Icon, Width = 800, Height = 450, Title = "Data preview", WindowStartupLocation = WindowStartupLocation.CenterOwner };

                        Grid windowGrid = new Grid() { Margin = new Thickness(10) };
                        windowGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                        windowGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                        windowGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                        windowGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                        previewWindow.Content = windowGrid;

                        Avalonia.Controls.Grid headerGrid = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                        headerGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
                        headerGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
                        headerGrid.Children.Add(new DPIAwareBox(GetIcon32) { Width = 32, Height = 32 });

                        {
                            Avalonia.Controls.TextBlock blk = new Avalonia.Controls.TextBlock() { FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)), Text = "Data preview", Margin = new Avalonia.Thickness(10, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                            Avalonia.Controls.Grid.SetColumn(blk, 1);
                            headerGrid.Children.Add(blk);
                        }

                        windowGrid.Children.Add(headerGrid);



                        StackPanel rowCountPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };
                        rowCountPanel.Children.Add(new TextBlock() { Text = "Number of rows to preview:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                        NumericUpDown rowCountNud = new NumericUpDown() { Minimum = 1, Maximum = attributes.Count, Value = Math.Min(50, attributes.Count), Margin = new Thickness(5, 0, 0, 0), Width = 150, FormatString = "0" };
                        rowCountPanel.Children.Add(rowCountNud);
                        Grid.SetRow(rowCountPanel, 1);
                        windowGrid.Children.Add(rowCountPanel);

                        StackPanel legendPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 5), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left };

                        legendPanel.Children.Add(new Avalonia.Controls.Shapes.Rectangle() { Width = 12, Height = 12, Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 78, 138)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                        legendPanel.Children.Add(new TextBlock() { Text = "String values", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(5, 0, 0, 0) });

                        legendPanel.Children.Add(new Avalonia.Controls.Shapes.Rectangle() { Width = 12, Height = 12, Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(166, 55, 0)), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(15, 0, 0, 0) });
                        legendPanel.Children.Add(new TextBlock() { Text = "Number values", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Thickness(5, 0, 0, 0) });

                        Grid.SetRow(legendPanel, 2);
                        windowGrid.Children.Add(legendPanel);

                        ScrollViewer scroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Avalonia.Thickness(0, 0, 16, 16), AllowAutoHide = false };
                        Grid.SetRow(scroller, 3);
                        windowGrid.Children.Add(scroller);

                        BuildPreviewTable(rowCountNud, attributes, matchColumn, attributeNames, scroller);

                        rowCountNud.ValueChanged += (s, e) =>
                        {
                            BuildPreviewTable(rowCountNud, attributes, matchColumn, attributeNames, scroller);
                        };

                        await previewWindow.ShowDialog2(window);
                    }
                    else
                    {
                        await new MessageBox("Attention!", "No attachment has been selected!").ShowDialog2(window);
                    }
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "An error occurred while loading the preview!\n" + ex.Message).ShowDialog2(window);
                }
            }

        }

        private static void BuildPreviewTable(NumericUpDown rowCountNud, List<object[]> attributes, int matchColumn, string[] attributeNames, ScrollViewer scroller)
        {
            int previewRows = Math.Min((int)rowCountNud.Value, attributes.Count);

            Grid previewGrid = new Grid();
            previewGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            int columnCount = (from el in attributes select el.Length).Max();

            for (int i = 0; i < columnCount; i++)
            {
                previewGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star) { MinWidth = 150 });
            }

            if (matchColumn == 1 && attributeNames.Length < columnCount)
            {
                {
                    TextBlock blk = new TextBlock()
                    {
                        FontWeight = Avalonia.Media.FontWeight.Bold,
                        FontStyle = Avalonia.Media.FontStyle.Italic,
                        Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128)),
                        Text = "[match column]",
                        Margin = new Thickness(5),
                        TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                        VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
                    };
                    previewGrid.Children.Add(blk);
                }

                for (int i = 0; i < attributeNames.Length; i++)
                {
                    TextBlock blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = attributeNames[i], Margin = new Thickness(5), TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetColumn(blk, i + 1);
                    previewGrid.Children.Add(blk);
                }
            }
            else
            {
                for (int i = 0; i < Math.Min(attributeNames.Length, columnCount); i++)
                {
                    TextBlock blk;

                    if (i != matchColumn - 1)
                    {
                        blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, Text = attributeNames[i], Margin = new Thickness(5), TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    }
                    else
                    {
                        blk = new TextBlock() { FontWeight = Avalonia.Media.FontWeight.Bold, FontStyle = Avalonia.Media.FontStyle.Italic, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128)), Text = attributeNames[i] + " [match column]", Margin = new Thickness(5), TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    }

                    Grid.SetColumn(blk, i);
                    previewGrid.Children.Add(blk);
                }
            }

            previewGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Pixel));

            {
                Canvas canvas = new Canvas() { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 0, 0)), Height = 1 };
                Grid.SetRow(canvas, 1);
                Grid.SetColumnSpan(canvas, columnCount);
                previewGrid.Children.Add(canvas);
            }

            for (int i = 0; i < previewRows; i++)
            {
                previewGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));


                {
                    Canvas canvas = new Canvas() { Background = Avalonia.Media.Brushes.Transparent, ZIndex = -100 };
                    Grid.SetRow(canvas, i + 2);
                    Grid.SetColumnSpan(canvas, columnCount);

                    canvas.PointerEnter += (s, e) =>
                    {
                        canvas.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(220, 220, 220));
                    };

                    canvas.PointerLeave += (s, e) =>
                    {
                        canvas.Background = Avalonia.Media.Brushes.Transparent;
                    };

                    previewGrid.Children.Add(canvas);
                }

                for (int j = 0; j < attributes[i].Length; j++)
                {
                    string stringValue;
                    bool isString;

                    if (attributes[i][j] is string strVal)
                    {
                        stringValue = strVal;
                        isString = true;
                    }
                    else if (attributes[i][j] is double dblVal)
                    {
                        stringValue = dblVal.ToString(System.Globalization.CultureInfo.InvariantCulture);
                        isString = false;
                    }
                    else
                    {
                        continue;
                    }

                    TextBlock blk;

                    if (j != matchColumn - 1)
                    {
                        blk = new TextBlock() { Text = stringValue, Margin = new Thickness(5), TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, IsHitTestVisible = false };

                        if (isString)
                        {
                            blk.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 78, 138));
                        }
                        else
                        {
                            blk.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(166, 55, 0));
                        }
                    }
                    else
                    {
                        blk = new TextBlock() { FontStyle = Avalonia.Media.FontStyle.Italic, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128)), Text = stringValue, Margin = new Thickness(5), TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, IsHitTestVisible = false };
                    }

                    if (j >= attributeNames.Length + 1 || (matchColumn != 1 && j >= attributeNames.Length))
                    {
                        blk.FontStyle = Avalonia.Media.FontStyle.Italic;
                        blk.Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(128, 128, 128));
                    }

                    Grid.SetColumn(blk, j);
                    Grid.SetRow(blk, i + 2);
                    previewGrid.Children.Add(blk);
                }
            }

            scroller.Content = previewGrid;
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            int skipLines = (int)(double)parameterValues["Lines to skip:"];
            string separatorString = (string)parameterValues["Separator:"];
            bool separatorRegex = (bool)parameterValues["Regex"];

            int attributeType = (int)parameterValues["Attribute type: "];

            string attributeName = (string)parameterValues["Column header(s):"];
            Attachment attachment = (Attachment)parameterValues["Data file:"];
            int matchColumn = (int)(double)parameterValues["Match column:"];
            string matchAttribute = (string)parameterValues["Match attribute:"];
            string matchAttributeType = (string)parameterValues["Match attribute type:"];
            bool useHeaderRow = (bool)parameterValues["Use first row as header"];

            if (attachment != null)
            {
                Regex separator;

                if (separatorRegex)
                {
                    separator = new Regex(separatorString);
                }
                else
                {
                    separator = new Regex(Regex.Escape(separatorString));
                }

                string[] lines = attachment.GetLines();

                Dictionary<string, object[]> attributes = new Dictionary<string, object[]>();

                string[] attributeNames;

                if (useHeaderRow)
                {
                    try
                    {
                        string[] splitLine = (from el in separator.Split(lines[skipLines]) where !string.IsNullOrEmpty(el) select el).ToArray();
                        attributeNames = splitLine;
                    }
                    catch
                    {
                        attributeNames = new string[0];
                    }
                }
                else
                {
                    attributeNames = (from el in separator.Split(Regex.Unescape(attributeName)) where !string.IsNullOrEmpty(el) select el.Trim()).ToArray();
                }

                for (int i = skipLines + (useHeaderRow ? 1 : 0); i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        string[] splitLine = (from el in separator.Split(lines[i]) where !string.IsNullOrEmpty(el) select el).ToArray();
                        if (splitLine.Length > matchColumn)
                        {
                            object[] attribute = new object[splitLine.Length];
                            for (int j = 0; j < splitLine.Length; j++)
                            {
                                if (attributeType == 0)
                                {
                                    attribute[j] = splitLine[j];
                                }
                                else if (attributeType == 1)
                                {
                                    if (double.TryParse(splitLine[j], out double parsed))
                                    {
                                        attribute[j] = parsed;
                                    }
                                    else
                                    {
                                        attribute[j] = double.NaN;
                                    }
                                }
                                else if (attributeType == 2)
                                {
                                    if (double.TryParse(splitLine[j], out double parsed))
                                    {
                                        attribute[j] = parsed;
                                    }
                                    else
                                    {
                                        attribute[j] = splitLine[j];
                                    }
                                }
                            }

                            attributes.Add(splitLine[matchColumn - 1], attribute);
                        }
                    }
                }

                Dictionary<double, object[]> doubleAttributes = null;

                if (matchAttributeType == "Number")
                {
                    doubleAttributes = new Dictionary<double, object[]>();

                    foreach (KeyValuePair<string, object[]> kvp in attributes)
                    {
                        if (double.TryParse(kvp.Key, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double parsedKey) && !double.IsNaN(parsedKey))
                        {
                            doubleAttributes[parsedKey] = kvp.Value;
                        }
                    }
                }

                List<TreeNode> nodes = tree.GetChildrenRecursive();

                for (int i = 0; i < nodes.Count; i++)
                {
                    if (matchAttributeType == "Number")
                    {
                        if (nodes[i].Attributes.TryGetValue(matchAttribute, out object matchAttributeObject) && matchAttributeObject != null && matchAttributeObject is double matchAttributeValue && !double.IsNaN(matchAttributeValue))
                        {
                            if (doubleAttributes.TryGetValue(matchAttributeValue, out object[] attribute))
                            {
                                for (int j = 0; j < attribute.Length; j++)
                                {
                                    if (j != matchColumn - 1)
                                    {
                                        int attributeIndex = j;

                                        if (j >= matchColumn - 1 && matchColumn == 1 && attributeNames.Length < attribute.Length)
                                        {
                                            attributeIndex--;
                                        }

                                        nodes[i].Attributes[attributeNames[attributeIndex]] = attribute[j];
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (nodes[i].Attributes.TryGetValue(matchAttribute, out object matchAttributeObject) && matchAttributeObject != null && matchAttributeObject is string matchAttributeValue && !string.IsNullOrEmpty(matchAttributeValue))
                        {
                            if (attributes.TryGetValue(matchAttributeValue, out object[] attribute))
                            {
                                for (int j = 0; j < attribute.Length; j++)
                                {
                                    if (j != matchColumn - 1)
                                    {
                                        int attributeIndex = j;

                                        if (j >= matchColumn - 1 && matchColumn == 1 && attributeNames.Length < attribute.Length)
                                        {
                                            attributeIndex--;
                                        }

                                        if (attributeIndex < attributeNames.Length)
                                        {
                                            nodes[i].Attributes[attributeNames[attributeIndex]] = attribute[j];
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}