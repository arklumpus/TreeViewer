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
    public class NumberFormatterOptions
    {
        public double DefaultValue { get; set; }
        public string AttributeName { get; set; }
        public string AttributeType { get; set; }

        private Func<object, double?> formatter;
        public Func<object, double?> Formatter
        {
            get
            {
                if (compileOnDemand && formatter == null)
                {
                    formatter = Compile(compileOnDemandSource.ToString(), "FormatterModule");
                }

                return formatter;
            }
            set
            {
                formatter = value;
            }
        }

        public object[] Parameters { get; set; }

        private bool compileOnDemand = false;
        private string compileOnDemandSource = null;

        private NumberFormatterOptions()
        {

        }

        public NumberFormatterOptions(string sourceCode, bool compileOnDemand)
        {
            StringBuilder source = new StringBuilder();

            StringReader sourceReader = new StringReader(sourceCode);

            string line = sourceReader.ReadLine();

            source.AppendLine("using TreeViewer;");

            while (line.Trim().StartsWith("using"))
            {
                source.AppendLine(line);
                line = sourceReader.ReadLine();
            }

            source.AppendLine("public static class FormatterModule { ");
            source.AppendLine(line);
            source.AppendLine(sourceReader.ReadToEnd());
            source.Append("}");

            if (!compileOnDemand)
            {
                this.Formatter = Compile(source.ToString(), "FormatterModule");
            }
            else
            {
                this.compileOnDemand = true;
                this.compileOnDemandSource = source.ToString();
            }
        }

        public NumberFormatterOptions(string sourceCode)
        {
            StringBuilder source = new StringBuilder();

            StringReader sourceReader = new StringReader(sourceCode);

            string line = sourceReader.ReadLine();

            source.AppendLine("using TreeViewer;");

            while (line.Trim().StartsWith("using"))
            {
                source.AppendLine(line);
                line = sourceReader.ReadLine();
            }

            source.AppendLine("public static class FormatterModule { ");
            source.AppendLine(line);
            source.AppendLine(sourceReader.ReadToEnd());
            source.Append("}");

            this.Formatter = Compile(source.ToString(), "FormatterModule");
        }

        public NumberFormatterOptions(Func<object, double?> formatter)
        {
            this.Formatter = formatter;
        }

        public static async Task<NumberFormatterOptions> Create(Editor editor, InterprocessDebuggerServer debuggerServer)
        {
            NumberFormatterOptions tbr = new NumberFormatterOptions();

            Assembly assembly = (await editor.Compile(debuggerServer.SynchronousBreak(editor), debuggerServer.AsynchronousBreak(editor))).Assembly;

            Type type = assembly.GetType("FormatterModule");
            tbr.Formatter = arg =>
            {
                return (double?)type.InvokeMember("Format",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    null,
                    new object[] { arg });
            };

            return tbr;
        }

        private static Func<object, double?> Compile(string source, string requestedType)
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
                    return (double?)type.InvokeMember("Format",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { arg });
                };
            }
        }
    }

    public class NumberFormatterWindow : ChildWindow
    {
        public bool Result { get; private set; } = false;

        public NumberFormatterOptions Formatter { get; private set; }

        private Editor ConverterCodeBox;
        private InterprocessDebuggerServer DebuggerServer;

        private Func<object[]> GetParameters;

        public NumberFormatterWindow()
        {
            this.InitializeComponent();

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumberFormatter")) { Width = 32, Height = 32 });
        }

        public async Task Initialize(string attributeName, string attributeType, double defaultValue, object[] parameters, InterprocessDebuggerServer debuggerServer, string editorId)
        {
            this.DebuggerServer = debuggerServer;

            this.FindControl<NumericUpDown>("DefaultValueNud").Value = defaultValue;
            this.FindControl<TextBox>("AttributeNameContainer").Text = attributeName;
            this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex = attributeType == "String" ? 0 : attributeType == "Number" ? 1 : -1;

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

            if (attributeType == "String")
            {
                initialText = SetupString(parameters);
            }
            else if (attributeType == "Number")
            {
                this.FindControl<NumericUpDown>("DefaultValueNud").Minimum = (double)parameters[1];
                this.FindControl<NumericUpDown>("DefaultValueNud").Maximum = (double)parameters[2];
                initialText = SetupNumber(parameters);
            }

            ConverterCodeBox = await Editor.Create(initialText, "using TreeViewer;\npublic static class FormatterModule {", "}", guid: editorId);
            ConverterCodeBox.IsReferencesButtonEnabled = false;
            ConverterCodeBox.Background = this.Background;

            if (attributeType == "String")
            {
                ConverterCodeBox.TextChanged += CheckString;
            }
            else if (attributeType == "Number")
            {
                ConverterCodeBox.TextChanged += CheckNumber;
            }

            this.FindControl<Grid>("CodeContainer").Children.Add(ConverterCodeBox);
        }

        void CheckString(object sender, EventArgs e)
        {
            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToDouble[0]);
        }

        void CheckNumber(object sender, EventArgs e)
        {
            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToDouble[1]);
        }

        private string SetupString(object[] parameters)
        {
            if (ConverterCodeBox != null)
            {
                ConverterCodeBox.TextChanged -= CheckNumber;
                ConverterCodeBox.TextChanged += CheckString;
            }

            GetParameters = () =>
            {
                return new object[] {
                        ConverterCodeBox.Text,
                        !Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToDouble[0])
                };
            };

            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent((string)parameters[0], Modules.DefaultAttributeConvertersToDouble[0]);
            initialized = true;

            return (string)parameters[0];
        }

        private string SetupNumber(object[] parameters)
        {
            if (ConverterCodeBox != null)
            {
                ConverterCodeBox.TextChanged -= CheckString;
                ConverterCodeBox.TextChanged += CheckNumber;

            }

            GetParameters = () =>
            {
                return new object[] {
                        ConverterCodeBox.Text,
                        this.FindControl<NumericUpDown>("DefaultValueNud").Minimum,
                        this.FindControl<NumericUpDown>("DefaultValueNud").Maximum,
                        !Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToDouble[1])
                };
            };

            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent((string)parameters[0], Modules.DefaultAttributeConvertersToDouble[1]);
            initialized = true;

            return (string)parameters[0];
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
                Formatter = await NumberFormatterOptions.Create(ConverterCodeBox, DebuggerServer);

                Formatter.Parameters = GetParameters();
                Formatter.AttributeName = this.FindControl<TextBox>("AttributeNameContainer").Text;
                Formatter.AttributeType = this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex == 0 ? "String" : this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex == 1 ? "Number" : "";
                Formatter.DefaultValue = this.FindControl<NumericUpDown>("DefaultValueNud").Value;

                this.Result = true;
                this.Close();
            }
            catch (Exception ex)
            {
                await new MessageBox("Error!", "Compilation error!\n" + ex.Message).ShowDialog2(this);
            }
        }

        bool initialized = false;

        private async void AttributeTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (initialized)
            {
                switch (this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex)
                {
                    case 0:
                        await ConverterCodeBox.SetText(SetupString(new object[] { Modules.DefaultAttributeConvertersToDouble[0] }));
                        break;
                    case 1:
                        await ConverterCodeBox.SetText(SetupNumber(new object[] { Modules.DefaultAttributeConvertersToDouble[1], 0.0, 1.0, Modules.DefaultGradients["TransparentToBlack"] }));
                        break;
                }
            }
        }
    }
}
