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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using CSharpEditor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VectSharp.Canvas;

namespace TreeViewer
{
    public class FormatterOptions
    {
        public Func<object, string> Formatter { get; set; }
        public object[] Parameters { get; set; }

        private FormatterOptions()
        {

        }

        public FormatterOptions(string sourceCode)
        {
            StringBuilder source = new StringBuilder();

            using StringReader sourceReader = new StringReader(sourceCode);

            string line = sourceReader.ReadLine();

            source.AppendLine("using TreeViewer;");

            source.AppendLine("public static class FormatterModule { ");
            source.AppendLine(line);
            source.AppendLine(sourceReader.ReadToEnd());
            source.Append("}");

            this.Formatter = Compile(source.ToString(), "FormatterModule");
        }

        public static async Task<FormatterOptions> Create(Editor editor, InterprocessDebuggerServer debuggerServer)
        {
            FormatterOptions tbr = new FormatterOptions();

            Assembly assembly = (await editor.Compile(debuggerServer.SynchronousBreak(editor), debuggerServer.AsynchronousBreak(editor))).Assembly;

            Type type = assembly.GetType("FormatterModule");
            tbr.Formatter = arg =>
            {
                return (string)type.InvokeMember("Format",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    null,
                    new object[] { arg });
            };

            return tbr;
        }

        private static Func<object, string> Compile(string source, string requestedType)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

            string assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, (from el in Modules.CachedReferences select (MetadataReference)el), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            using var ms = new MemoryStream();

            EmitResult result = compilation.Emit(ms);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                StringBuilder message = new StringBuilder();

                foreach (Diagnostic diagnostic in failures)
                {
                    message.AppendLine(diagnostic.Id + ": " + diagnostic.GetMessage());
                }
                throw new Exception(message.ToString());
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                Type type = assembly.GetType(requestedType);
                return arg =>
                {
                    return (string)type.InvokeMember("Format",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { arg });
                };
            }
        }
    }



    public class FormatOptionWindow : ChildWindow
    {
        public bool Result { get; private set; } = false;

        public FormatterOptions Formatter { get; private set; }

        private InterprocessDebuggerServer DebuggerServer;

        private Func<object[]> GetParameters;

        private Editor ConverterCodeBox;

        public FormatOptionWindow()
        {
            this.InitializeComponent();

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringFormatter")) { Width = 32, Height = 32 });
        }

        public async Task Initialize(string attributeType, object[] parameters, InterprocessDebuggerServer debuggerServer, string editorId)
        {
            this.DebuggerServer = debuggerServer;

            this.FindControl<TextBlock>("AttributeTypeContainer").Text = attributeType;

            Viewbox alertIcon = MainWindow.GetAlertIcon();
            alertIcon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            alertIcon.Width = 16;
            alertIcon.Height = 16;

            this.FindControl<Grid>("AlertContainer").Children.Add(alertIcon);

            {
                TextBlock blk = new TextBlock() { Text = "Using custom formatter code!", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), FontSize = 13, TextWrapping = Avalonia.Media.TextWrapping.Wrap };
                Grid.SetColumn(blk, 1);
                this.FindControl<Grid>("AlertContainer").Children.Add(blk);
            }

            string initialText = "";
            EventHandler<EventArgs> textChanged = null;

            if (attributeType == "String")
            {
                this.FindControl<Grid>("OptionsContainer").Children.Add(new TextBlock() { FontStyle = Avalonia.Media.FontStyle.Italic, Text = "No options (return unchanged)" });
                initialText = (string)parameters[0];

                textChanged = (s, e) =>
                {
                    this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConverters[0]);
                };

                GetParameters = () =>
                {
                    return new object[] {
                        ConverterCodeBox.Text,
                        !Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConverters[0])
                };
                };

                this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(initialText, Modules.DefaultAttributeConverters[0]);
            }
            else if (attributeType == "Number")
            {
                StackPanel pnl = new StackPanel();

                Grid digitsPnl = new Grid() { };

                digitsPnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                digitsPnl.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                digitsPnl.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                ComboBox digitsTypeBox = new ComboBox() { Padding = new Thickness(5, 2, 5, 2), Items = new List<string>() { "Significant", "Decimal" }, SelectedIndex = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };

                digitsPnl.Children.Add(digitsTypeBox);

                {
                    TextBlock blk = new TextBlock() { Text = "digits:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                    Grid.SetColumn(blk, 1);
                    digitsPnl.Children.Add(blk);
                }

                NumericUpDown digitsCountNud = new NumericUpDown() { Minimum = 0, FormatString = "0", Increment = 1, Value = 2, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
                Grid.SetColumn(digitsCountNud, 2);
                digitsPnl.Children.Add(digitsCountNud);

                pnl.Children.Add(digitsPnl);

                Grid leqPanel = new Grid();
                leqPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                leqPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                leqPanel.Children.Add(new TextBlock() { Text = "Only if ≤", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });
                NumericUpDown leqNud = new NumericUpDown() { Value = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
                Grid.SetColumn(leqNud, 1);
                leqPanel.Children.Add(leqNud);

                CheckBox leqBox = new CheckBox() { Content = leqPanel, Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                pnl.Children.Add(leqBox);


                Grid geqPanel = new Grid();
                geqPanel.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                geqPanel.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                geqPanel.Children.Add(new TextBlock() { Text = "Only if ≥", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });
                NumericUpDown geqNud = new NumericUpDown() { Value = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13 };
                Grid.SetColumn(geqNud, 1);
                geqPanel.Children.Add(geqNud);

                CheckBox geqBox = new CheckBox() { Content = geqPanel, IsChecked = true, Margin = new Thickness(0, 5, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

                pnl.Children.Add(geqBox);

                this.FindControl<Grid>("OptionsContainer").Children.Add(pnl);


                string BuildCode()
                {
                    StringBuilder codeBuilder = new StringBuilder();
                    codeBuilder.Append(@"public static string Format(object attribute)
{
    if (attribute is double attributeValue &&
        !double.IsNaN(attributeValue))
    {
");

                    int indentationLevel = 2;

                    if (leqBox.IsChecked == true)
                    {
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.Append("if (attributeValue <= ");
                        codeBuilder.Append(leqNud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        codeBuilder.AppendLine(")");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("{");
                        indentationLevel++;
                    }

                    if (geqBox.IsChecked == true)
                    {
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.Append("if (attributeValue >= ");
                        codeBuilder.Append(geqNud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                        codeBuilder.AppendLine(")");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("{");
                        indentationLevel++;
                    }

                    codeBuilder.Append(' ', indentationLevel * 4);
                    codeBuilder.AppendLine("return attributeValue.ToString(" + digitsCountNud.Value.ToString("0") + ", " + (digitsTypeBox.SelectedIndex == 1).ToString().ToLowerInvariant() + ");");

                    if (geqBox.IsChecked == true)
                    {
                        indentationLevel--;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("}");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("else");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("{");
                        indentationLevel++;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("return null;");
                        indentationLevel--;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("}");
                    }

                    if (leqBox.IsChecked == true)
                    {
                        indentationLevel--;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("}");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("else");
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("{");
                        indentationLevel++;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("return null;");
                        indentationLevel--;
                        codeBuilder.Append(' ', indentationLevel * 4);
                        codeBuilder.AppendLine("}");
                    }

                    codeBuilder.AppendLine(@"   }
    else
    {
        return null;
    }");
                    codeBuilder.Append("}");

                    return codeBuilder.ToString();
                };

                try
                {

                    digitsTypeBox.SelectedIndex = (int)parameters[0];
                    digitsCountNud.Value = (double)parameters[1];
                    leqNud.Value = (double)parameters[2];
                    geqNud.Value = (double)parameters[3];
                    leqBox.IsChecked = (bool)parameters[4];
                    geqBox.IsChecked = (bool)parameters[5];

                }
                catch { }

                digitsTypeBox.SelectionChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
                digitsCountNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
                leqNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
                geqNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
                leqBox.Click += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
                geqBox.Click += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };

                Button regenerateButton = new Button() { Content = "Regenerate default code", Margin = new Thickness(0, 5, 0, 0), FontSize = 13, Padding = new Thickness(5, 2, 5, 2), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };

                Grid.SetRow(regenerateButton, 1);
                Grid.SetColumnSpan(regenerateButton, 2);

                regenerateButton.Click += async (s, e) =>
                {
                    await ConverterCodeBox.SetText(BuildCode());
                    this.FindControl<Grid>("AlertContainer").IsVisible = false;
                };

                this.FindControl<Grid>("AlertContainer").Children.Add(regenerateButton);

                try
                {
                    initialText = (string)parameters[6];
                }
                catch { }

                this.FindControl<Grid>("AlertContainer").IsVisible = false;

                textChanged = (s, e) =>
                {
                    this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode());
                };

                GetParameters = () =>
                {
                    return new object[] { digitsTypeBox.SelectedIndex, digitsCountNud.Value, leqNud.Value, geqNud.Value, leqBox.IsChecked == true, geqBox.IsChecked == true, ConverterCodeBox.Text, !Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode()) };
                };

                this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(initialText, BuildCode());
            }

            this.ConverterCodeBox = await Editor.Create(initialText, "using TreeViewer;\npublic static class FormatterModule { ", "}", guid: editorId);
            ConverterCodeBox.IsReferencesButtonEnabled = false;
            ConverterCodeBox.Background = this.Background;
            ConverterCodeBox.TextChanged += textChanged;
            this.FindControl<Grid>("CodeContainer").Children.Add(ConverterCodeBox);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void OKButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Formatter = await FormatterOptions.Create(ConverterCodeBox, DebuggerServer);
                Formatter.Parameters = GetParameters();

                this.Result = true;
                this.Close();
            }
            catch (Exception ex)
            {
                await new MessageBox("Error!", "Compilation error!\n" + ex.Message).ShowDialog2(this);
            }
        }
    }
}
