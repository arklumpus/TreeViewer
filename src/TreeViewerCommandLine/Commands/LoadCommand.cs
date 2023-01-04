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
using System.Text;
using TreeViewer;
using System.Linq;
using PhyloTree;

namespace TreeViewerCommandLine
{
    class LoadCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  load\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Loads the currently opened file automatically choosing the most appropriate module.\n\n", 4),
            new ConsoleTextSpan("  load ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("info\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows the scores detailing how appropriate each module is to load the file, without actually loading it.\n\n", 4),
            new ConsoleTextSpan("  load ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("with ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Loads the file with the specified module.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Loads a tree file.")
        };

        public override string PrimaryCommand => "load";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                if (Program.FileOpener != null)
                {
                    _ = LoadFile(null);
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No file has been opened!", ConsoleColor.Red));
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
                    if (Program.FileOpener != null)
                    {
                        command = command.Substring(4).Trim();

                        bool found = false;

                        foreach (LoadFileModule mod in Modules.LoadFileModules)
                        {
                            if (mod.Name.Equals(command, StringComparison.OrdinalIgnoreCase) || mod.Id.Equals(command, StringComparison.OrdinalIgnoreCase))
                            {
                                _ = LoadFile(mod.Id);
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
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No file has been opened!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    if (Program.FileOpener != null)
                    {
                        Dictionary<string, double> info = GetLoadInfo();
                        (string, string, double)[] scores = (from el in info orderby el.Value descending select (el.Key, Modules.GetModule(Modules.LoadFileModules, el.Key).Name, el.Value)).ToArray();

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
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No file has been opened!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown action: " + command, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        internal static Dictionary<string, double> GetLoadInfo()
        {
            Dictionary<string, double> priorities = new Dictionary<string, double>();

            for (int i = 0; i < Modules.LoadFileModules.Count; i++)
            {
                try
                {
                    double priority = Modules.LoadFileModules[i].IsSupported(Program.OpenedFileInfo, Program.OpenerModuleId, Program.FileOpener);
                    priorities.Add(Modules.LoadFileModules[i].Id, priority);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);

                    priorities.Add(Modules.LoadFileModules[i].Id, 0);
                }
            }

            return priorities;
        }

        internal static async System.Threading.Tasks.Task LoadFile(string moduleId)
        {
            if (string.IsNullOrEmpty(moduleId))
            {
                Dictionary<string, double> priorities = GetLoadInfo();
                KeyValuePair<string, double> bestModule = (from el in priorities orderby el.Value descending select el).FirstOrDefault();
                if (bestModule.Value == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file is not supported by any of the currently installed modules!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                    return;
                }
                moduleId = bestModule.Key;
            }

            TreeCollection coll;

            List<(string, Dictionary<string, object>)> moduleSuggestions = Program.ModuleSuggestions;

            Action<double> progressAction = (_) => { };

            (coll, progressAction) = await Modules.GetModule(Modules.LoadFileModules, moduleId).Load(null, Program.OpenedFileInfo, Program.OpenerModuleId, Program.FileOpener, moduleSuggestions, progressAction, (val) => { progressAction(val); });

            Program.Trees = coll;
            Program.LoaderModuleId = moduleId;

            Program.TransformerModuleId = null;
            Program.TransformerParameters = null;
            Program.FurtherTransformations.Clear();
            Program.CoordinatesModuleId = null;
            Program.CoordinatesParameters = null;
            Program.PlotActions.Clear();
            Program.StateData.Attachments.Clear();

            ConsoleWrapper.WriteLine();
            ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Suggested modules:", 2));
            ConsoleWrapper.WriteLine();

            for (int i = 0; i < moduleSuggestions.Count; i++)
            {
                if (moduleSuggestions[i].Item1 != "@Background" && moduleSuggestions[i].Item1 != "@Attachment")
                {
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                    {
                    new ConsoleTextSpan("    " + Modules.LoadedModulesMetadata[moduleSuggestions[i].Item1].Name, 4, ConsoleColor.Cyan),
                    new ConsoleTextSpan(" (" + moduleSuggestions[i].Item1 + ")", 4),
                    });
                }

            }

            ConsoleWrapper.WriteLine();
            ConsoleWrapper.Write(new ConsoleTextSpan("  Would you like to load the suggested modules? [Y(es)/N(o)] ", 2));


            char key = '?';

            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
            {
                key = ConsoleWrapper.ReadKey(true).KeyChar;
            }

            ConsoleWrapper.Write(key);
            ConsoleWrapper.WriteLine();

            bool loadModules = false;

            if (key == 'y' || key == 'Y')
            {
                loadModules = true;
            }
            else if (key == 'n' || key == 'N')
            {
                loadModules = false;
            }



            if (loadModules)
            {
                for (int i = 0; i < moduleSuggestions.Count; i++)
                {
                    if (moduleSuggestions[i].Item1 == "@Attachment")
                    {
                        Attachment att = (Attachment)moduleSuggestions[i].Item2["Attachment"];
                        Program.StateData.Attachments.Add(att.Name, att);
                    }
                }

                for (int i = 0; i < moduleSuggestions.Count; i++)
                {
                    MainWindow.UpdateAttachmentLinks(moduleSuggestions[i].Item2, Program.StateData);
                }

                for (int i = 0; i < moduleSuggestions.Count; i++)
                {
                    if (moduleSuggestions[i].Item1 == "@Background")
                    {
                        Program.StateData.GraphBackgroundColour = (VectSharp.Colour)moduleSuggestions[i].Item2["Colour"];
                    }
                    else if (moduleSuggestions[i].Item1 != "@Attachment")
                    {
                        ModuleCommand.EnableModule(Modules.LoadedModulesMetadata[moduleSuggestions[i].Item1], -1, moduleSuggestions[i].Item2);
                        if (i == 0)
                        {
                            UpdateCommand.UpdateTransformer();
                            UpdateCommand.UpdateFurtherTransformations();
                        }

                        if (Modules.LoadedModulesMetadata[moduleSuggestions[i].Item1].ModuleType == ModuleTypes.FurtherTransformation)
                        {
                            UpdateCommand.UpdateFurtherTransformations(Program.FurtherTransformations.Count - 1);
                        }
                        else if (Modules.LoadedModulesMetadata[moduleSuggestions[i].Item1].ModuleType == ModuleTypes.Coordinate)
                        {
                            UpdateCommand.UpdateCoordinates();
                        }

                    }
                }
            }

        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                if (!string.IsNullOrEmpty(Program.InputFileName))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load", ConsoleColor.Green) }, "load ");
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "load info ");
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("with", ConsoleColor.Yellow) }, "load with ");
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

                    foreach (LoadFileModule mod in Modules.LoadFileModules)
                    {
                        if (mod.Name.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("with ", ConsoleColor.Yellow), new ConsoleTextSpan(mod.Name + " ", ConsoleColor.Blue) }, "load with " + mod.Name + " ");
                        }
                    }

                    foreach (LoadFileModule mod in Modules.LoadFileModules)
                    {
                        if (mod.Id.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("with ", ConsoleColor.Yellow), new ConsoleTextSpan(mod.Id + " ", ConsoleColor.Blue) }, "load with " + mod.Id + " ");
                        }
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "load info ");
                }
                else
                {
                    if ("with".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("with", ConsoleColor.Yellow) }, "load with ");
                    }

                    if ("info".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("load ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "load info ");
                    }
                }
            }
        }
    }
}
