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
using VectSharp;

namespace TreeViewer
{
    public class ColourFormatterOptions
    {
        public Colour DefaultColour { get; set; }
        public string AttributeName { get; set; }
        public string AttributeType { get; set; }

        private Func<object, Colour?> formatter;
        public Func<object, Colour?> Formatter
        {
            get
            {
                if (compileOnDemand && formatter == null)
                {
                    formatter = Compile(compileOnDemandSource.ToString(), "FormatterModule", out string actualSource);
                    actualSource = actualSource.Substring(compileOnDemandPrevLength);
                    actualSource = actualSource.Substring(0, actualSource.Length - 1 - Environment.NewLine.Length);

                    this.Parameters[0] = actualSource;
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
        private int compileOnDemandPrevLength = 0;

        private ColourFormatterOptions()
        {

        }

        public ColourFormatterOptions(string sourceCode, object[] parameters, bool compileOnDemand)
        {
            StringBuilder source = new StringBuilder();

            using StringReader sourceReader = new StringReader(sourceCode);

            string line = sourceReader.ReadLine();

            source.AppendLine("using TreeViewer;");
            source.AppendLine("using VectSharp;");
            source.AppendLine("using System;");
            source.AppendLine("using System.Collections.Generic;");

            source.AppendLine("public static class FormatterModule { ");

            int prevLength = source.Length;

            source.AppendLine(line);
            source.AppendLine(sourceReader.ReadToEnd());
            source.Append("}");

            if (!compileOnDemand)
            {
                this.Formatter = Compile(source.ToString(), "FormatterModule", out string actualSource);
                actualSource = actualSource.Substring(prevLength);
                actualSource = actualSource.Substring(0, actualSource.Length - 1 - Environment.NewLine.Length);

                parameters[0] = actualSource;
            }
            else
            {
                this.compileOnDemand = true;
                this.compileOnDemandSource = source.ToString();
                this.compileOnDemandPrevLength = prevLength;
            }

            this.Parameters = parameters;
        }

        public ColourFormatterOptions(string sourceCode, object[] parameters)
        {
            StringBuilder source = new StringBuilder();

            using StringReader sourceReader = new StringReader(sourceCode);

            string line = sourceReader.ReadLine();

            source.AppendLine("using TreeViewer;");
            source.AppendLine("using VectSharp;");
            source.AppendLine("using System;");
            source.AppendLine("using System.Collections.Generic;");

            source.AppendLine("public static class FormatterModule { ");

            int prevLength = source.Length;

            source.AppendLine(line);
            source.AppendLine(sourceReader.ReadToEnd());
            source.Append("}");

            this.Formatter = Compile(source.ToString(), "FormatterModule", out string actualSource);

            actualSource = actualSource.Substring(prevLength);
            actualSource = actualSource.Substring(0, actualSource.Length - 1 - Environment.NewLine.Length);

            parameters[0] = actualSource;

            this.Parameters = parameters;
        }

        public ColourFormatterOptions(Func<object, Colour?> formatter)
        {
            this.Formatter = formatter;
        }

        public static async Task<ColourFormatterOptions> Create(Editor editor, InterprocessDebuggerServer debuggerServer)
        {
            ColourFormatterOptions tbr = new ColourFormatterOptions();

            Assembly assembly = (await editor.Compile(debuggerServer.SynchronousBreak(editor), debuggerServer.AsynchronousBreak(editor))).Assembly;

            Type type = assembly.GetType("FormatterModule");
            tbr.Formatter = arg =>
            {
                return (Colour?)type.InvokeMember("Format",
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    null,
                    new object[] { arg });
            };

            return tbr;
        }

        private static Func<object, Colour?> Compile(string source, string requestedType, out string actualSource, bool firstTry = true)
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

                string messageText = message.ToString();

                if (firstTry && messageText.Contains("TreeViewer.GradientStop") && messageText.Contains("VectSharp.GradientStop"))
                {
                    // Hack for backwards compatibility

                    source = source.Replace(" GradientStop", " TreeViewer.GradientStop").Replace("<GradientStop", "<TreeViewer.GradientStop");
                    return Compile(source, requestedType, out actualSource, false);
                }
                else
                {
                    throw new Exception(message.ToString());
                }
            }
            else
            {
                ms.Seek(0, SeekOrigin.Begin);
                Assembly assembly = Assembly.Load(ms.ToArray());

                Type type = assembly.GetType(requestedType);

                actualSource = source;

                return arg =>
                {
                    return (Colour?)type.InvokeMember("Format",
                        BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        null,
                        new object[] { arg });
                };
            }
        }
    }

    public class ColourFormatterWindow : ChildWindow
    {

        public bool Result { get; private set; } = false;

        public ColourFormatterOptions Formatter { get; private set; }

        private Editor ConverterCodeBox;
        private InterprocessDebuggerServer DebuggerServer;

        private Func<object[]> GetParameters;

        public ColourFormatterWindow()
        {
            this.InitializeComponent();

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.ColourFormatter")) { Width = 32, Height = 32 });

            this.FindControl<Button>("GradientButton").Click += async (s, e) =>
            {
                EditGradientWindow win = new EditGradientWindow(new Gradient(this.FindControl<GradientControl>("GradientButtonControl").Gradient.GradientStops), this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").Value, this.FindControl<NumericUpDown>("NumberOptionsMaximumBox").Value);
                await win.ShowDialog2(this);
                if (win.Result)
                {
                    this.FindControl<GradientControl>("GradientButtonControl").Gradient = win.Gradient;
                    await ConverterCodeBox.SetText(BuildCode());
                    this.FindControl<Grid>("AlertContainer").IsVisible = false;
                }
            };
        }


        public async Task Initialize(string attributeName, string attributeType, Colour defaultColour, object[] parameters, InterprocessDebuggerServer debuggerServer, string editorId)
        {
            this.DebuggerServer = debuggerServer;

            this.FindControl<ColorButton>("DefaultColourButton").Color = defaultColour.ToAvalonia();
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
                initialText = SetupNumber(parameters);
            }

            ConverterCodeBox = await Editor.Create(initialText, "using TreeViewer;\nusing VectSharp;\nusing System;\nusing System.Collections.Generic;\npublic static class FormatterModule {", "}", guid: editorId);
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
            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToColour[0]);
        }

        private string SetupString(object[] parameters)
        {
            this.FindControl<Grid>("StringOptions").IsVisible = true;
            this.FindControl<Grid>("NumberOptions").IsVisible = false;

            if (ConverterCodeBox != null)
            {
                ConverterCodeBox.TextChanged -= CheckNumber;
                ConverterCodeBox.TextChanged += CheckString;
            }

            GetParameters = () =>
            {
                return new object[] {
                        ConverterCodeBox.Text,
                        !Extensions.IsCodeDifferent(ConverterCodeBox.Text, Modules.DefaultAttributeConvertersToColour[0])
                };
            };

            if (this.FindControl<Grid>("AlertContainer").Children.Count > 2)
            {
                this.FindControl<Grid>("AlertContainer").Children.RemoveRange(2, this.FindControl<Grid>("AlertContainer").Children.Count - 2);
            }

            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent((string)parameters[0], Modules.DefaultAttributeConvertersToColour[0]);
            initialized = true;
            return (string)parameters[0];
        }

        private Func<string> BuildCode;
        void CheckNumber(object sender, EventArgs e)
        {
            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode());
        }

        private string SetupNumber(object[] parameters)
        {
            this.FindControl<Grid>("StringOptions").IsVisible = false;
            this.FindControl<Grid>("NumberOptions").IsVisible = true;

            try
            {
                this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").Value = (double)parameters[1];
                this.FindControl<NumericUpDown>("NumberOptionsMaximumBox").Value = (double)parameters[2];
                this.FindControl<GradientControl>("GradientButtonControl").Gradient = (Gradient)parameters[3];
            }
            catch { }

            BuildCode = () =>
            {
                Gradient gradient = this.FindControl<GradientControl>("GradientButtonControl").Gradient;

                StringBuilder codeBuilder = new StringBuilder();
                codeBuilder.AppendLine(@"static Gradient gradient = new Gradient(
new List<TreeViewer.GradientStop>()
{");
                int indentationLevel = 1;

                for (int i = 0; i < gradient.GradientStops.Count; i++)
                {
                    codeBuilder.Append(' ', indentationLevel * 4);
                    codeBuilder.Append("new TreeViewer.GradientStop(");
                    codeBuilder.Append(gradient.GradientStops[i].Position.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    codeBuilder.Append(", Colour.FromRgba(");
                    codeBuilder.Append(gradient.GradientStops[i].Colour.R.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    codeBuilder.Append("d, ");
                    codeBuilder.Append(gradient.GradientStops[i].Colour.G.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    codeBuilder.Append("d, ");
                    codeBuilder.Append(gradient.GradientStops[i].Colour.B.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    codeBuilder.Append("d, ");
                    codeBuilder.Append(gradient.GradientStops[i].Colour.A.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    codeBuilder.Append("d))");
                    if (i < gradient.GradientStops.Count - 1)
                    {
                        codeBuilder.AppendLine(",");
                    }
                    else
                    {
                        codeBuilder.AppendLine();
                    }
                }

                codeBuilder.AppendLine("});");

                codeBuilder.Append(@"
public static Colour? Format(object attribute)
{
    if (attribute is double attributeValue &&
    !double.IsNaN(attributeValue))
    {
");
                indentationLevel++;
                codeBuilder.Append(' ', indentationLevel * 4);
                codeBuilder.Append("double position = (attributeValue - ");
                codeBuilder.Append(this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
                codeBuilder.Append(") / ");
                codeBuilder.Append((this.FindControl<NumericUpDown>("NumberOptionsMaximumBox").Value - this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").Value).ToString(System.Globalization.CultureInfo.InvariantCulture));
                codeBuilder.AppendLine(";");
                codeBuilder.Append(' ', indentationLevel * 4);
                codeBuilder.AppendLine("position = Math.Max(Math.Min(position, 1), 0);");
                codeBuilder.Append(' ', indentationLevel * 4);
                codeBuilder.AppendLine("return gradient.GetColour(position);");
                codeBuilder.AppendLine(@"    }
    else
    {
        return null;
    }");
                codeBuilder.Append("}");

                return codeBuilder.ToString();
            };

            this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };
            this.FindControl<NumericUpDown>("NumberOptionsMaximumBox").ValueChanged += async (s, e) => { await ConverterCodeBox.SetText(BuildCode()); this.FindControl<Grid>("AlertContainer").IsVisible = false; };

            Button regenerateButton = new Button() { Content = "Regenerate default code", Margin = new Thickness(0, 5, 0, 0), FontSize = 13, Padding = new Thickness(5, 2, 5, 2), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            regenerateButton.Click += async (s, e) =>
            {
                await ConverterCodeBox.SetText(BuildCode());
                this.FindControl<Grid>("AlertContainer").IsVisible = false;
            };

            Grid.SetRow(regenerateButton, 1);
            Grid.SetColumnSpan(regenerateButton, 2);

            if (this.FindControl<Grid>("AlertContainer").Children.Count > 2)
            {
                this.FindControl<Grid>("AlertContainer").Children.RemoveRange(2, this.FindControl<Grid>("AlertContainer").Children.Count - 2);
            }

            this.FindControl<Grid>("AlertContainer").Children.Add(regenerateButton);

            this.FindControl<Grid>("AlertContainer").IsVisible = false;

            if (ConverterCodeBox != null)
            {
                ConverterCodeBox.TextChanged -= CheckString;
                ConverterCodeBox.TextChanged += CheckNumber;
            }

            GetParameters = () =>
            {
                return new object[] { ConverterCodeBox.Text, this.FindControl<NumericUpDown>("NumberOptionsMinimumBox").Value,
                    this.FindControl<NumericUpDown>("NumberOptionsMaximumBox").Value,
                    this.FindControl<GradientControl>("GradientButtonControl").Gradient, !Extensions.IsCodeDifferent(ConverterCodeBox.Text, BuildCode()) };
            };

            this.FindControl<Grid>("AlertContainer").IsVisible = Extensions.IsCodeDifferent((string)parameters[0], BuildCode());
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
                Formatter = await ColourFormatterOptions.Create(ConverterCodeBox, DebuggerServer);

                Formatter.Parameters = GetParameters();
                Formatter.AttributeName = this.FindControl<TextBox>("AttributeNameContainer").Text.Trim();
                Formatter.AttributeType = this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex == 0 ? "String" : this.FindControl<ComboBox>("AttributeTypeContainer").SelectedIndex == 1 ? "Number" : "";
                Formatter.DefaultColour = this.FindControl<ColorButton>("DefaultColourButton").Color.ToVectSharp();

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
                        await ConverterCodeBox.SetText(SetupString(new object[] { Modules.DefaultAttributeConvertersToColour[0] }));
                        break;
                    case 1:
                        await ConverterCodeBox.SetText(SetupNumber(new object[] { Modules.DefaultAttributeConvertersToColour[1], 0.0, 1.0, Modules.DefaultGradients["TransparentToBlack"] }));
                        break;
                }
            }
        }
    }
}
