/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2022  Giorgio Bianchini, University of Bristol
 
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class CSCCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("  csc ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<path to script file>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Compiles and executes the C# code contained in the specified ", 4),
            new ConsoleTextSpan("input file", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".", 4)
        };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Compiles and executes C# scripts.")
        };

        public override string PrimaryCommand => "csc";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No input file selected!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
            else
            {
                command = Path.GetFullPath(command.Trim().Trim('\"'));
                if (File.Exists(command))
                {
                    string sourceCode = File.ReadAllText(command);

                    string[] dlls = Directory.GetFiles(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "*.dll");

                    int maxLen = dlls.Length.ToString().Length * 2 + 1;

                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.Write(new ConsoleTextSpan("Loading references: " + new string(' ', maxLen), ConsoleColor.Yellow));

                    for (int i = 0; i < dlls.Length; i++)
                    {
                        string progress = (i + 1).ToString() + "/" + dlls.Length.ToString();
                        progress += new string(' ', maxLen - progress.Length);
                        ConsoleWrapper.Write(new string('\b', maxLen) + progress);

                        if (Modules.IsDLLManaged(dlls[i]))
                        {
                            CSharpEditor.CachedMetadataReference reference = CSharpEditor.CachedMetadataReference.CreateFromFile(dlls[i]);

                            if (!Modules.CachedReferences.Contains(reference))
                            {
                                Modules.CachedReferences.Add(reference);
                            }
                        }
                    }

                    ConsoleWrapper.WriteLine(new ConsoleTextSpan(new string('\b', maxLen) + "done." + new string(' ', Math.Max(0, maxLen - 5)), ConsoleColor.Green));
                    ConsoleWrapper.WriteLine();

                    CompiledCode code = new CompiledCode(sourceCode);

                    Func<object[], object> mainMethod = null;

                    foreach (Type type in code.CompiledAssembly.DefinedTypes)
                    {
                        mainMethod = ModuleMetadata.GetMethod(type, "Main");

                        if (mainMethod != null)
                        {
                            break;
                        }
                    }

                    if (mainMethod == null)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Could not find an entry point!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        try
                        {
                            mainMethod(new object[] { new string[] { } });
                        }
                        catch (MissingMethodException)
                        {
                            mainMethod(new object[0]);
                        }
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The specified file does not exist!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*");

                List<(ConsoleTextSpan[], string)> tbr = new List<(ConsoleTextSpan[], string)>();

                foreach (string sr in directories)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Blue)
                    }, this.PrimaryCommand + " " + Path.GetFileName(sr) + Path.DirectorySeparatorChar));
                }


                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*");

                foreach (string sr in files)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                    }, this.PrimaryCommand + " " + Path.GetFileName(sr) + " "));
                }

                tbr.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                foreach ((ConsoleTextSpan[], string) item in tbr)
                {
                    yield return item;
                }
            }
            else
            {
                partialCommand = partialCommand.Trim();

                Regex reg = new Regex("^[A-Za-z]:$");
                if (reg.IsMatch(partialCommand))
                {
                    partialCommand = partialCommand + "\\";
                }

                partialCommand = partialCommand.Trim();
                string directory = null;

                directory = Path.GetDirectoryName(partialCommand);


                if (directory == null)
                {
                    reg = new Regex("^[A-Za-z]:\\\\$");
                    if (reg.IsMatch(partialCommand))
                    {
                        directory = partialCommand;
                    }
                }

                string actualDirectory = directory;

                if (string.IsNullOrEmpty(actualDirectory))
                {
                    actualDirectory = Directory.GetCurrentDirectory();
                }

                string fileName = Path.GetFileName(partialCommand);

                string[] directories = Directory.GetDirectories(actualDirectory, fileName + "*");

                List<(ConsoleTextSpan[], string)> tbr = new List<(ConsoleTextSpan[], string)>();

                foreach (string sr in directories)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Blue)
                    }, this.PrimaryCommand + " " + Path.Combine(directory, Path.GetFileName(sr)) + Path.DirectorySeparatorChar));
                }


                string[] files = Directory.GetFiles(actualDirectory, fileName + "*");

                foreach (string sr in files)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                    }, this.PrimaryCommand + " " + Path.Combine(directory, Path.GetFileName(sr)) + " "));
                }

                tbr.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                foreach ((ConsoleTextSpan[], string) item in tbr)
                {
                    yield return item;
                }
            }
        }
    }
}
