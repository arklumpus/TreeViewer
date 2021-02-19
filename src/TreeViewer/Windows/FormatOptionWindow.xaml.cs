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

            /*while (line.Trim().StartsWith("using"))
            {
                source.AppendLine(line);
                line = sourceReader.ReadLine();
            }*/

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



    public class FormatOptionWindow : Window
    {
        public bool Result { get; private set; } = false;

        public FormatterOptions Formatter { get; private set; }

        private InterprocessDebuggerServer DebuggerServer;

        private Func<object[]> GetParameters;

        private Editor ConverterCodeBox;

        public FormatOptionWindow()
        {
            this.InitializeComponent();
        }

        public async Task Initialize(string attributeType, object[] parameters, InterprocessDebuggerServer debuggerServer, string editorId)
        {
            this.DebuggerServer = debuggerServer;

            this.FindControl<TextBlock>("AttributeTypeContainer").Text = attributeType;

            this.FindControl<StackPanel>("AlertContainer").Children.Add(MainWindow.AlertPage.PaintToCanvas());
            this.FindControl<StackPanel>("AlertContainer").Children.Add(new TextBlock() { Text = "Note: custom formatter code will be used!", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) });

            string initialText = "";
            EventHandler<EventArgs> textChanged = null;

            if (attributeType == "String")
            {
                this.FindControl<Grid>("OptionsContainer").Children.Add(new TextBlock() { FontStyle = Avalonia.Media.FontStyle.Italic, Text = "No options (return unchanged)" });
                initialText = (string)parameters[0];

                textChanged = (s, e) =>
                {
                    this.FindControl<StackPanel>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConverters[0]);
                };

                GetParameters = () =>
                {
                    return new object[] {
                        ConverterCodeBox.Text,
                        !Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConverters[0])
                };
                };

                this.FindControl<StackPanel>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(initialText, Modules.DefaultAttributeConverters[0]);
            }
            else if (attributeType == "Number")
            {
                StackPanel pnl = new StackPanel();

                StackPanel digitsPnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

                ComboBox digitsTypeBox = new ComboBox() { Padding = new Thickness(5, 0, 5, 0), Items = new List<string>() { "Significant", "Decimal" }, SelectedIndex = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                digitsPnl.Children.Add(digitsTypeBox);

                digitsPnl.Children.Add(new TextBlock() { Text = "digits:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) });

                NumericUpDown digitsCountNud = new NumericUpDown() { Minimum = 0, FormatString = "0", Increment = 1, Value = 2, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };

                digitsPnl.Children.Add(digitsCountNud);

                pnl.Children.Add(digitsPnl);

                StackPanel leqPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                leqPanel.Children.Add(new TextBlock() { Text = "Only if ≤", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                NumericUpDown leqNud = new NumericUpDown() { Value = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                leqPanel.Children.Add(leqNud);

                CheckBox leqBox = new CheckBox() { Content = leqPanel, Margin = new Thickness(0, 5, 0, 0) };

                pnl.Children.Add(leqBox);


                StackPanel geqPanel = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
                geqPanel.Children.Add(new TextBlock() { Text = "Only if ≥", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });
                NumericUpDown geqNud = new NumericUpDown() { Value = 0, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0) };
                geqPanel.Children.Add(geqNud);

                CheckBox geqBox = new CheckBox() { Content = geqPanel, IsChecked = true, Margin = new Thickness(0, 5, 0, 0) };

                pnl.Children.Add(geqBox);

                this.FindControl<Grid>("OptionsContainer").Children.Add(pnl);


                string BuildCode()
                {
                    StringBuilder codeBuilder = new StringBuilder();
                    codeBuilder.Append(@"public static string Format(object attribute)
{
    if (attribute is double attributeValue && !double.IsNaN(attributeValue))
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

                digitsTypeBox.SelectionChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };
                digitsCountNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };
                leqNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };
                geqNud.ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };
                leqBox.Click += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };
                geqBox.Click += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<StackPanel>("AlertContainer").IsVisible = false; };

                Button regenerateButton = new Button() { Content = "Regenerate code", Margin = new Thickness(5, 0, 0, 0) };
                regenerateButton.Click += async (s, e) =>
                {
                    await ConverterCodeBox.SetText(BuildCode());
                    this.FindControl<StackPanel>("AlertContainer").IsVisible = false;
                };

                this.FindControl<StackPanel>("AlertContainer").Children.Add(regenerateButton);

                try
                {
                    initialText = (string)parameters[6];
                }
                catch { }

                this.FindControl<StackPanel>("AlertContainer").IsVisible = false;

                textChanged = (s, e) =>
                {
                    this.FindControl<StackPanel>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode());
                };

                GetParameters = () =>
                {
                    return new object[] { digitsTypeBox.SelectedIndex, digitsCountNud.Value, leqNud.Value, geqNud.Value, leqBox.IsChecked == true, geqBox.IsChecked == true, ConverterCodeBox.Text, !Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode()) };
                };

                this.FindControl<StackPanel>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(initialText, BuildCode());
            }

            this.ConverterCodeBox = await Editor.Create(initialText, "using TreeViewer;\npublic static class FormatterModule { ", "}", guid: editorId);
            ConverterCodeBox.IsReferencesButtonEnabled = false;
            ConverterCodeBox.TextChanged += textChanged;
            this.FindControl<Expander>("CustomFormatterExpander").Child = ConverterCodeBox;
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
