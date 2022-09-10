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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using System.Linq;
using System.Security.Cryptography;

namespace TreeViewerCommandLine
{
    class OpenCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  open ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<path to file>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Sets the current ", 4),
            new ConsoleTextSpan("input file path", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" and opens and loads the file automatically choosing the most appropriate modules. Equivalent to:\n", 4),
            new ConsoleTextSpan("      file ", 6, ConsoleColor.Green),
            new ConsoleTextSpan("<path to file>\n", 6, ConsoleColor.Blue),
            new ConsoleTextSpan("      open\n", 6, ConsoleColor.Green),
            new ConsoleTextSpan("      load\n\n", 6, ConsoleColor.Green),
            new ConsoleTextSpan("  open\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Opens the currently selected input file automatically choosing the most appropriate module.\n\n", 4),
            new ConsoleTextSpan("  open ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("info\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows the scores detailing how appropriate each module is to open the file, without actually opening it.\n\n", 4),
            new ConsoleTextSpan("  open ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("with ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Opens the file with the specified module.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Opens a tree file.")
        };

        public override string PrimaryCommand => "open";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!string.IsNullOrEmpty(Program.InputFileName))
                {
                    OpenFile(Program.InputFileName, null);
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No input file selected!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else
            {
                command = command.TrimStart();

                StringBuilder firstWordBuilder = new StringBuilder();

                foreach (char c in command)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        firstWordBuilder.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }

                string firstWord = firstWordBuilder.ToString();

                if (firstWord.Equals("with", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(Program.InputFileName))
                    {
                        command = command.Substring(4).Trim();

                        bool found = false;

                        foreach (FileTypeModule mod in Modules.FileTypeModules)
                        {
                            if (mod.Name.Equals(command, StringComparison.OrdinalIgnoreCase) || mod.Id.Equals(command, StringComparison.OrdinalIgnoreCase))
                            {
                                OpenFile(Program.InputFileName, mod.Id);
                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("Could not find the specified module!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No input file selected!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(Program.InputFileName))
                    {
                        Dictionary<string, double> info = GetOpenInfo(Program.InputFileName);
                        (string, string, double)[] scores = (from el in info orderby el.Value descending select (el.Key, Modules.GetModule(Modules.FileTypeModules, el.Key).Name, el.Value)).ToArray();

                        int maxNameLength = (from el in scores select el.Item2.Length).Max() + 4;
                        int maxKeyLength = (from el in scores select el.Item1.Length).Max();
                        int maxValueLength = (from el in scores select el.Item3.ToString("0.####").Length).Max() + 4;

                        maxNameLength = Math.Max(maxNameLength, "Module name".Length + 4);
                        maxKeyLength = Math.Max(maxKeyLength, "Module id".Length);
                        maxValueLength = Math.Max(maxValueLength, "Priority".Length + 4);

                        ConsoleWrapper.WriteLine();

                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                                new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2) + "Priority" + new string(ConsoleWrapper.Whitespace, maxValueLength - "Priority".Length), 2),
                                new ConsoleTextSpan("Module name" + new string(ConsoleWrapper.Whitespace, maxNameLength - "Module name".Length), 2),
                                new ConsoleTextSpan("Module id" + new string(ConsoleWrapper.Whitespace, maxKeyLength - "Module id".Length), 2)
                                });


                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                                new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2) + new string('─', maxNameLength + maxKeyLength + maxValueLength), 2)
                                });


                        for (int i = 0; i < scores.Length; i++)
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                                new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)),
                                new ConsoleTextSpan(scores[i].Item3.ToString("0.####") + new string(ConsoleWrapper.Whitespace, maxValueLength - scores[i].Item3.ToString("0.####").Length), 2, scores[i].Item3 <= 0 ? ConsoleColor.Red : i == 0 ? ConsoleColor.Green : ConsoleColor.Yellow),
                                new ConsoleTextSpan(scores[i].Item2 + new string(ConsoleWrapper.Whitespace, maxNameLength - scores[i].Item2.Length), 2),
                                new ConsoleTextSpan(scores[i].Item1 + new string(ConsoleWrapper.Whitespace, maxKeyLength - scores[i].Item1.Length), 2)
                                });
                        }

                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No input file selected!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    command = Path.GetFullPath(command.Trim().Trim('\"'));
                    if (File.Exists(command))
                    {
                        Program.InputFileName = command;
                        OpenFile(Program.InputFileName, null);
                        _ = LoadCommand.LoadFile(null);
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The specified file does not exist!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
            }
        }

        internal static Dictionary<string, double> GetOpenInfo(string fileName)
        {
            Dictionary<string, double> priorities = new Dictionary<string, double>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                    priorities.Add(Modules.FileTypeModules[i].Id, priority);
                }
                catch
                {
                    priorities.Add(Modules.FileTypeModules[i].Id, 0);
                }
            }

            return priorities;
        }

        internal static void OpenFile(string fileName, string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId))
            {
                Dictionary<string, double> priorities = GetOpenInfo(fileName);
                KeyValuePair<string, double> bestModule = (from el in priorities orderby el.Value descending select el).FirstOrDefault();
                if (bestModule.Value == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file type is not supported by any of the currently installed modules!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                    return;
                }
                moduleId = bestModule.Key;
            }

            IEnumerable<TreeNode> opener;

            List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
            {
                ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
            };

            Action<double> openerProgressAction = (_) => { };

            bool? codePermissionGranted = null;

            bool askForCodePermission(RSAParameters? publicKey)
            {
                if (codePermissionGranted.HasValue)
                {
                    return codePermissionGranted.Value;
                }
                else
                {
                    ConsoleWrapper.Write(new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan("Attention! The selected file contains source code and its signature does not match any known keys. Do you want to load and compile it? You should only do this if you trust the source of the file and/or you have accurately reviewed the code. [Y(es)/N(o)] ", ConsoleColor.Yellow)
                    });

                    char key = '?';

                    while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                    {
                        key = ConsoleWrapper.ReadKey(true).KeyChar;
                    }

                    ConsoleWrapper.Write(key);
                    ConsoleWrapper.WriteLine();

                    if (key == 'y' || key == 'Y')
                    {
                        codePermissionGranted = true;

                        if (publicKey.HasValue)
                        {
                            ConsoleWrapper.Write(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("Would you like to add the file's public key to the local storage? This will allow you to open other files produced by the same author without seeing this message. You should only do this if you trust the source of the file. [Y(es)/N(o)] ", ConsoleColor.Yellow)
                            });

                            char key2 = '?';

                            while (key2 != 'y' && key2 != 'Y' && key2 != 'n' && key2 != 'N')
                            {
                                key2 = ConsoleWrapper.ReadKey(true).KeyChar;
                            }

                            ConsoleWrapper.Write(key2);
                            ConsoleWrapper.WriteLine();

                            if (key2 == 'y' || key2 == 'Y')
                            {
                                TreeViewer.CryptoUtils.AddPublicKey(publicKey.Value);
                            }
                        }

                        return true;
                    }
                    else if (key == 'n' || key == 'N')
                    {
                        codePermissionGranted = false;
                        return false;
                    }
                    else
                    {
                        return false;
                    }
                }
            };

            opener = Modules.GetModule(Modules.FileTypeModules, moduleId).OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

            FileInfo finfo = new FileInfo(fileName);

            Program.FileOpener = opener;
            Program.OpenedFileInfo = finfo;
            Program.ModuleSuggestions = moduleSuggestions;
            Program.OpenerModuleId = moduleId;

        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                if (!string.IsNullOrEmpty(Program.InputFileName))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open", ConsoleColor.Green) }, "open ");
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "open info ");
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("with", ConsoleColor.Yellow) }, "open with ");
                }

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
                partialCommand = partialCommand.TrimStart();

                StringBuilder firstWordBuilder = new StringBuilder();

                foreach (char c in partialCommand)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        firstWordBuilder.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }

                string firstWord = firstWordBuilder.ToString();

                if (firstWord.Equals("with", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    foreach (FileTypeModule mod in Modules.FileTypeModules)
                    {
                        if (mod.Name.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("with ", ConsoleColor.Yellow), new ConsoleTextSpan(mod.Name + " ", ConsoleColor.Blue) }, "open with " + mod.Name + " ");
                        }
                    }

                    foreach (FileTypeModule mod in Modules.FileTypeModules)
                    {
                        if (mod.Id.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("with ", ConsoleColor.Yellow), new ConsoleTextSpan(mod.Id + " ", ConsoleColor.Blue) }, "open with " + mod.Id + " ");
                        }
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "open info ");
                }
                else
                {
                    if ("with".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("with", ConsoleColor.Yellow) }, "open with ");
                    }

                    if ("info".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("open ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "open info ");
                    }

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
}
