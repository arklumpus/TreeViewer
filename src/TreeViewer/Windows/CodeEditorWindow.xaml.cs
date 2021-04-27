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

using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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

namespace TreeViewer
{
    public class CompiledCode
    {
        public static string EmptyCode = @"using System;
using PhyloTree;
using System.Collections.Generic;
namespace a" + Guid.NewGuid().ToString().Replace("-", "") + @"
{
    public static class CustomCode
    {
        //Custom code not loaded
    }
}";

        public string SourceCode { get; }
        public Assembly CompiledAssembly { get; }

        public CompiledCode(string sourceCode)
        {
            this.CompiledAssembly = Compile(sourceCode);
            this.SourceCode = sourceCode;
        }

        public CompiledCode(Assembly compiledAssembly, string sourceCode)
        {
            this.CompiledAssembly = compiledAssembly;
            this.SourceCode = sourceCode;
        }

        public CompiledCode(string sourceCode, bool replaceErrors)
        {
            if (replaceErrors)
            {
                try
                {
                    this.CompiledAssembly = Compile(sourceCode);
                }
                catch
                {
                    this.CompiledAssembly = Compile(EmptyCode);
                }
            }
            else
            {
                this.CompiledAssembly = Compile(sourceCode);
            }

            this.SourceCode = sourceCode;
        }

        private static Assembly Compile(string source)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

            string assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            List<string> additionalReferences = new List<string>();

            StringReader reader = new StringReader(source);
            string line = reader.ReadLine();

            while (string.IsNullOrWhiteSpace(line) || line.StartsWith("//#r"))
            {
                if (!string.IsNullOrWhiteSpace(line) && line.StartsWith("//#r"))
                {
                    string fileName = line.Replace("//#r", "").Trim().Trim('\"');

                    additionalReferences.Add(ModuleMetadata.LocateReference(fileName));
                }
                line = reader.ReadLine();
            }

            List<CSharpEditor.CachedMetadataReference> references = new List<CSharpEditor.CachedMetadataReference>(Modules.CachedReferences);

            foreach (string sr in additionalReferences)
            {
                references.Add(CSharpEditor.CachedMetadataReference.CreateFromFile(ModuleMetadata.LocateReference(sr)));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, (from el in references select (MetadataReference)el), new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

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

                return assembly;
            }
        }
    }


    public class CodeEditorWindow : Window
    {
        public CompiledCode Result { get; private set; } = null;

        private CSharpEditor.InterprocessDebuggerServer DebuggerServer;

        public CodeEditorWindow()
        {
            this.InitializeComponent();
        }

        private CSharpEditor.Editor Editor;


        /*public CodeEditorWindow(string sourceCode)
        {
            this.InitializeComponent();

            this.FindControl<TextBox>("sourceBox").Text = sourceCode.Replace("\t", "    ");


            this.FindControl<TextBox>("sourceBox").KeyUp += (s, e) =>
            {
                bool needsToBeMoved = false;

                if (this.FindControl<TextBox>("sourceBox").CaretIndex > 0 && this.FindControl<TextBox>("sourceBox").Text[this.FindControl<TextBox>("sourceBox").CaretIndex - 1] == '\t')
                {
                    needsToBeMoved = true;
                }

                this.FindControl<TextBox>("sourceBox").Text = this.FindControl<TextBox>("sourceBox").Text.Replace("\t", "    ");

                if (needsToBeMoved)
                {
                    this.FindControl<TextBox>("sourceBox").CaretIndex += 3;
                }
            };
        }*/

        public async Task FinishInitialization(string sourceCode, CSharpEditor.InterprocessDebuggerServer debuggerServer, string editorId)
        {
            this.DebuggerServer = debuggerServer;
            Editor = await CSharpEditor.Editor.Create(sourceCode.Replace("\t", "    "), guid: editorId);

            //editor.IsReferencesButtonEnabled = false;

            this.FindControl<Grid>("MainGrid").Children.Add(Editor);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private async void OKClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                //Result = new CompiledCode(this.FindControl<TextBox>("sourceBox").Text);
                //Result = new CompiledCode(Editor.Text);

                (Assembly Assembly, CSharpCompilation Compilation) result = await Editor.Compile(DebuggerServer.SynchronousBreak(Editor), DebuggerServer.AsynchronousBreak(Editor));
                
                if (result.Assembly != null)
                {
                    Result = new CompiledCode(result.Assembly, Editor.Text);
                    this.Close();
                }
                else
                {
                    IEnumerable<Diagnostic> failures = result.Compilation.GetDiagnostics().Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                    StringBuilder message = new StringBuilder();

                    foreach (Diagnostic diagnostic in failures)
                    {
                        message.AppendLine(diagnostic.Id + ": " + diagnostic.GetMessage());
                    }

                    await new MessageBox("Error!", "Compilation error!\n" + message).ShowDialog2(this);
                }
            }
            catch (Exception ex)
            {
                await new MessageBox("Error!", "Compilation error!\n" + ex.Message).ShowDialog2(this);
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Result = null;
            this.Close();
        }
    }
}
