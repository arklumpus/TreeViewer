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

namespace TreeViewerCommandLine
{
    class ModuleCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list available ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("<module type>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("]\n", 2),
            new ConsoleTextSpan("    Shows the modules that are currently installed and loaded, filtering them by ", 4),
            new ConsoleTextSpan("module type", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" (if applicable).\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list enabled\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows the modules that are currently enabled.\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("help ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Shows the help text for the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("enable ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Enables the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("disable ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module #>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Disables the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("move up ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module #>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Moves the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(" up in the order of execution.\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("move down ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module #>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Moves the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(" down in the order of execution.\n\n", 4),
            new ConsoleTextSpan("  module ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<module name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module id>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<module #>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Selects the specified ", 4),
            new ConsoleTextSpan("module", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Performs actions on modules.")
        };

        public override string PrimaryCommand => "module";

        private static string[] ModuleTypeStrings = new[] { "File type", "Load file", "Transformer", "Further transformation", "Coordinates", "Plot action", "Selection action", "Action" };

        public override void Execute(string command)
        {
            command = command.Trim();

            if (command.StartsWith("list available", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(14).Trim();

                if (string.IsNullOrWhiteSpace(argument))
                {

                    int maxModuleNameLength = (from el in Modules.LoadedModulesMetadata select el.Value.Name.Length).Max();
                    int totalLength = maxModuleNameLength + 4 + 36;

                    ConsoleWrapper.WriteLine();

                    for (int i = 0; i < ModuleTypeStrings.Length; i++)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[i], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });

                        ModuleMetadata[] items;
                        
                        if (ModuleTypeStrings[i] == "Action")
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == i && Modules.GetModule(Modules.ActionModules, el.Key).IsAvailableInCommandLine select el.Value).ToArray();
                        }
                        else if(ModuleTypeStrings[i] == "Selection Action")
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == i && Modules.GetModule(Modules.SelectionActionModules, el.Key).IsAvailableInCommandLine select el.Value).ToArray();
                        }
                        else
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == i select el.Value).ToArray();
                        }

                        PrintModules(items, maxModuleNameLength, false);
                        ConsoleWrapper.WriteLine();

                        if (i < ModuleTypeStrings.Length - 1)
                        {
                            ConsoleWrapper.WriteLine();
                        }
                    }
                }
                else
                {
                    int index = -1;
                    for (int i = 0; i < ModuleTypeStrings.Length; i++)
                    {
                        if (ModuleTypeStrings[i].Equals(argument, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index >= 0)
                    {
                        ModuleMetadata[] items;

                        if (ModuleTypeStrings[index] == "Action")
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == index && Modules.GetModule(Modules.ActionModules, el.Key).IsAvailableInCommandLine select el.Value).ToArray();
                        }
                        else if (ModuleTypeStrings[index] == "SelectionAction")
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == index && Modules.GetModule(Modules.SelectionActionModules, el.Key).IsAvailableInCommandLine select el.Value).ToArray();
                        }
                        else
                        {
                            items = (from el in Modules.LoadedModulesMetadata where (int)el.Value.ModuleType == index select el.Value).ToArray();
                        }

                        int maxModuleNameLength = (from el in items select el.Name.Length).Max();
                        int totalLength = maxModuleNameLength + 4 + 36;

                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[index], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                        PrintModules(items, maxModuleNameLength, false);
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module type: " + argument, ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
            }
            else if (command.Equals("list enabled", StringComparison.OrdinalIgnoreCase))
            {
                int maxModuleNameLength = 10;

                if (!string.IsNullOrEmpty(Program.OpenerModuleId))
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.FileTypeModules, Program.OpenerModuleId).Name.Length);
                }

                if (!string.IsNullOrEmpty(Program.LoaderModuleId))
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.LoadFileModules, Program.LoaderModuleId).Name.Length);
                }

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.TransformerModules, Program.TransformerModuleId).Name.Length);
                }

                foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.FurtherTransformationModules, item.Item1).Name.Length);
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.CoordinateModules, Program.CoordinatesModuleId).Name.Length);
                }

                foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                {
                    maxModuleNameLength = Math.Max(maxModuleNameLength, Modules.GetModule(Modules.PlottingModules, item.Item1).Name.Length);
                }

                int totalLength = 8 + maxModuleNameLength + 4 + 36;

                int startIndex = 1;

                if (!string.IsNullOrEmpty(Program.OpenerModuleId))
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.FileType], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules(new ModuleMetadata[] { Modules.LoadedModulesMetadata[Program.OpenerModuleId] }, maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex++;
                }


                if (!string.IsNullOrEmpty(Program.LoaderModuleId))
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.LoadFile], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules(new ModuleMetadata[] { Modules.LoadedModulesMetadata[Program.LoaderModuleId] }, maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex++;
                }

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.Transformer], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules(new ModuleMetadata[] { Modules.LoadedModulesMetadata[Program.TransformerModuleId] }, maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex++;
                }

                if (Program.FurtherTransformations.Count > 0)
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.FurtherTransformation], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules((from el in Program.FurtherTransformations select Modules.LoadedModulesMetadata[el.Item1]).ToArray(), maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex += Program.FurtherTransformations.Count;
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.Coordinate], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules(new ModuleMetadata[] { Modules.LoadedModulesMetadata[Program.CoordinatesModuleId] }, maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex++;
                }

                if (Program.PlotActions.Count > 0)
                {
                    Console.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 2)), new ConsoleTextSpan(CenterString(ModuleTypeStrings[(int)ModuleTypes.Plotting], totalLength), 2, ConsoleColor.Black, ConsoleColor.White) });
                    PrintModules((from el in Program.PlotActions select Modules.LoadedModulesMetadata[el.Item1]).ToArray(), maxModuleNameLength, true, startIndex);
                    ConsoleWrapper.WriteLine();
                    startIndex += Program.PlotActions.Count;
                }

            }
            else if (command.StartsWith("enable", StringComparison.OrdinalIgnoreCase))
            {
                bool proceed = true;

                if (PendingUpdates.Any && !PendingUpdates.NeverAsk)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("The state of some modules needs to be updated! Use the ", ConsoleColor.Yellow), new ConsoleTextSpan("update", ConsoleColor.Green), new ConsoleTextSpan(" to update them.", ConsoleColor.Yellow) });
                    ConsoleWrapper.Write(new ConsoleTextSpan[] { new ConsoleTextSpan("Do you wish to proceed anyways? [Y(es)/N(o)/I( know what I'm doing, never ask again)] ", ConsoleColor.Yellow) });

                    char key = '?';

                    while (key != 'y' && key != 'Y' && key != 'n' && key != 'N' && key != 'i' && key != 'I')
                    {
                        key = ConsoleWrapper.ReadKey(true).KeyChar;
                    }

                    ConsoleWrapper.Write(key);
                    ConsoleWrapper.WriteLine();

                    if (key == 'y' || key == 'Y')
                    {
                        proceed = true;
                    }
                    else if (key == 'n' || key == 'N')
                    {
                        proceed = false;
                    }
                    else if (key == 'i' || key == 'i')
                    {
                        PendingUpdates.NeverAsk = true;
                        proceed = true;
                    }
                }

                if (proceed)
                {
                    string argument = command.Substring(6).Trim();

                    ModuleMetadata[] selectedModules = (from el in Modules.LoadedModulesMetadata where el.Value.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Value.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el.Value).ToArray();

                    if (selectedModules.Length == 0)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else if (selectedModules.Length > 1)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module ID to univocally specify a module!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ModuleMetadata selectedModule = selectedModules[0];

                        EnableModule(selectedModule, new Dictionary<string, object>());
                    }
                }

            }
            else if (command.StartsWith("help", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(4).Trim();

                ModuleMetadata[] selectedModules = (from el in Modules.LoadedModulesMetadata where el.Value.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Value.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el.Value).ToArray();

                if (selectedModules.Length == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else if (selectedModules.Length > 1)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module ID to univocally specify a module!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ModuleMetadata selectedModule = selectedModules[0];

                    List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                    {
                        new ConsoleTextSpan("  Module ", 2),
                        new ConsoleTextSpan(selectedModule.Name, 2, ConsoleColor.Cyan),
                        new ConsoleTextSpan(" (" + selectedModule.Id + "):\n", 2),
                        new ConsoleTextSpan("    By ", 4),
                        new ConsoleTextSpan(selectedModule.Author, 4, ConsoleColor.Yellow)

                    };

                    if (selectedModule.VerifySignature())
                    {
                        message.Add(new ConsoleTextSpan(" (signed)\n\n", 4, ConsoleColor.Green));
                    }
                    else
                    {
                        message.Add(new ConsoleTextSpan(" (NOT signed)\n\n", 4, ConsoleColor.Red));
                    }

                    message.AddRange(from el in selectedModule.HelpText.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4));

                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(message);
                }
            }
            else if (command.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(6).Trim();

                Dictionary<(string, ModuleMetadata), ModuleParametersContainer> enabledModules = new Dictionary<(string, ModuleMetadata), ModuleParametersContainer>();

                int startInd = 3;

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.TransformerModuleId]), Program.TransformerParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.CoordinatesModuleId]), Program.CoordinatesParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }


                KeyValuePair<(string, ModuleMetadata), ModuleParametersContainer>[] selectedModules = (from el in enabledModules where el.Key.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el).ToArray();

                if (selectedModules.Length == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else if (selectedModules.Length > 1)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module # to univocally specify a module!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    Program.SelectedModuleParameters = selectedModules[0].Value;

                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                        new ConsoleTextSpan("  Selected module ", 2),
                        new ConsoleTextSpan(selectedModules[0].Key.Item1 + " ", 2, ConsoleColor.Yellow),
                        new ConsoleTextSpan(selectedModules[0].Key.Item2.Name, 2, ConsoleColor.Cyan),
                        new ConsoleTextSpan(" (" + selectedModules[0].Key.Item2.Id + ")", 2)
                    });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.StartsWith("disable", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(7).Trim();

                Dictionary<(string, ModuleMetadata), ModuleParametersContainer> enabledModules = new Dictionary<(string, ModuleMetadata), ModuleParametersContainer>();

                int startInd = 3;

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.TransformerModuleId]), Program.TransformerParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.CoordinatesModuleId]), Program.CoordinatesParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }


                KeyValuePair<(string, ModuleMetadata), ModuleParametersContainer>[] selectedModules = (from el in enabledModules where el.Key.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el).ToArray();

                if (selectedModules.Length == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else if (selectedModules.Length > 1)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module # to univocally specify a module!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ModuleParametersContainer moduleToDisable = selectedModules[0].Value;

                    DisableModule(moduleToDisable);
                }
            }
            else if (command.StartsWith("move up", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(7).Trim();

                Dictionary<(string, ModuleMetadata), ModuleParametersContainer> enabledModules = new Dictionary<(string, ModuleMetadata), ModuleParametersContainer>();

                int startInd = 3;

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.TransformerModuleId]), Program.TransformerParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.CoordinatesModuleId]), Program.CoordinatesParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }


                KeyValuePair<(string, ModuleMetadata), ModuleParametersContainer>[] selectedModules = (from el in enabledModules where el.Key.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el).ToArray();

                if (selectedModules.Length == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else if (selectedModules.Length > 1)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module # to univocally specify a module!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ModuleParametersContainer moduleToMove = selectedModules[0].Value;

                    if (moduleToMove == Program.TransformerParameters || moduleToMove == Program.CoordinatesParameters)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("This module cannot be moved!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                        {
                            if (Program.FurtherTransformations[i].Item2 == moduleToMove)
                            {
                                int index = Math.Max(0, i - 1);
                                (string, ModuleParametersContainer) item = Program.FurtherTransformations[i];
                                Program.FurtherTransformations.RemoveAt(i);
                                Program.FurtherTransformations.Insert(index, item);
                                PendingUpdates.FurtherTransformations = true;
                                break;
                            }
                        }

                        for (int i = 0; i < Program.PlotActions.Count; i++)
                        {
                            if (Program.PlotActions[i].Item2 == moduleToMove)
                            {
                                int index = Math.Max(0, i - 1);
                                (string, ModuleParametersContainer) item = Program.PlotActions[i];
                                Program.PlotActions.RemoveAt(i);
                                Program.PlotActions.Insert(index, item);
                                break;
                            }
                        }
                    }
                }
            }
            else if (command.StartsWith("move down", StringComparison.OrdinalIgnoreCase))
            {
                string argument = command.Substring(9).Trim();

                Dictionary<(string, ModuleMetadata), ModuleParametersContainer> enabledModules = new Dictionary<(string, ModuleMetadata), ModuleParametersContainer>();

                int startInd = 3;

                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.TransformerModuleId]), Program.TransformerParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[Program.CoordinatesModuleId]), Program.CoordinatesParameters);
                    startInd++;
                }

                foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                {
                    enabledModules.Add(("#" + startInd.ToString(), Modules.LoadedModulesMetadata[item.Item1]), item.Item2);
                    startInd++;
                }


                KeyValuePair<(string, ModuleMetadata), ModuleParametersContainer>[] selectedModules = (from el in enabledModules where el.Key.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Name.Equals(argument, StringComparison.OrdinalIgnoreCase) || el.Key.Item2.Id.Equals(argument, StringComparison.OrdinalIgnoreCase) select el).ToArray();

                if (selectedModules.Length == 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown module: " + argument, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else if (selectedModules.Length > 1)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous module selection! Please use the module # to univocally specify a module!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ModuleParametersContainer moduleToMove = selectedModules[0].Value;

                    if (moduleToMove == Program.TransformerParameters || moduleToMove == Program.CoordinatesParameters)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("This module cannot be moved!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                        {
                            if (Program.FurtherTransformations[i].Item2 == moduleToMove)
                            {
                                int index = Math.Min(Program.FurtherTransformations.Count - 1, i + 1);
                                (string, ModuleParametersContainer) item = Program.FurtherTransformations[i];
                                Program.FurtherTransformations.RemoveAt(i);
                                Program.FurtherTransformations.Insert(index, item);
                                PendingUpdates.FurtherTransformations = true;
                                break;
                            }
                        }

                        for (int i = 0; i < Program.PlotActions.Count; i++)
                        {
                            if (Program.PlotActions[i].Item2 == moduleToMove)
                            {
                                int index = Math.Min(Program.PlotActions.Count - 1, i + 1);
                                (string, ModuleParametersContainer) item = Program.PlotActions[i];
                                Program.PlotActions.RemoveAt(i);
                                Program.PlotActions.Insert(index, item);
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown action: " + command, ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }


        public static void DisableModule(ModuleParametersContainer moduleToDisable)
        {
            if (moduleToDisable == Program.TransformerParameters || moduleToDisable == Program.CoordinatesParameters)
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("This module cannot be disabled! Please enable an alternative module instead!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
            else
            {
                if (moduleToDisable == Program.SelectedModuleParameters)
                {
                    Program.SelectedModuleParameters = null;
                }

                for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                {
                    if (Program.FurtherTransformations[i].Item2 == moduleToDisable)
                    {
                        Program.FurtherTransformations.RemoveAt(i);
                        PendingUpdates.FurtherTransformations = true;
                        break;
                    }
                }

                for (int i = 0; i < Program.PlotActions.Count; i++)
                {
                    if (Program.PlotActions[i].Item2 == moduleToDisable)
                    {
                        Program.PlotActions.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public static void EnableModule(ModuleMetadata selectedModule, Dictionary<string, object> parametersToChange)
        {
            if (selectedModule.ModuleType == ModuleTypes.FileType)
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("File type modules cannot be enabled in this way! Please use the ", ConsoleColor.Red), new ConsoleTextSpan("open", ConsoleColor.Green), new ConsoleTextSpan(" command!", ConsoleColor.Red) });
                ConsoleWrapper.WriteLine();
            }
            else if (selectedModule.ModuleType == ModuleTypes.LoadFile)
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("Load file modules cannot be enabled in this way! Please use the ", ConsoleColor.Red), new ConsoleTextSpan("load", ConsoleColor.Green), new ConsoleTextSpan(" command!", ConsoleColor.Red) });
                ConsoleWrapper.WriteLine();
            }
            else if (selectedModule.ModuleType == ModuleTypes.Transformer)
            {
                if (Program.Trees != null)
                {
                    Program.TransformerModuleId = selectedModule.Id;

                    TransformerModule actualModule = Modules.GetModule(Modules.TransformerModules, selectedModule.Id);

                    List<(string, string)> parameters = actualModule.GetParameters(Program.Trees);
                    parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

                    GenericParameterChangeDelegate transformerParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
                    {
                        return actualModule.OnParameterChange(Program.Trees, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                    };

                    Program.TransformerParameters = Utils.GetParameters(transformerParameterChange, parameters, actualModule.Name, actualModule.Id);

                    Program.SelectedModuleParameters = Program.TransformerParameters;

                    Program.TransformerParameters.UpdateParameterAction(parametersToChange);

                    Program.TransformerParameters.PrintParameters();

                    PendingUpdates.Transformer = true;
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("No file has been loaded!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (selectedModule.ModuleType == ModuleTypes.FurtherTransformation)
            {
                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    string moduleId = selectedModule.Id;

                    (string, ModuleParametersContainer) moduleToBeAdded = (moduleId, new ModuleParametersContainer());

                    FurtherTransformationModule actualModule = Modules.GetModule(Modules.FurtherTransformationModules, selectedModule.Id);

                    bool canBeAdded = true;

                    if (!actualModule.Repeatable)
                    {
                        for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                        {
                            if (Program.FurtherTransformations[i].Item1 == moduleId)
                            {
                                canBeAdded = false;
                                break;
                            }
                        }
                    }

                    if (canBeAdded)
                    {

                        List<(string, string)> parameters = actualModule.GetParameters(Program.TransformedTree);
                        parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

                        GenericParameterChangeDelegate furtherTransformationParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
                        {
                            int index = Program.FurtherTransformations.IndexOf(moduleToBeAdded);
                            if (index > 0 && index < Program.AllTransformedTrees.Length + 1)
                            {
                                return actualModule.OnParameterChange(Program.AllTransformedTrees[index - 1], previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                            }
                            else
                            {
                                return actualModule.OnParameterChange(Program.FirstTransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                            }
                        };

                        ModuleParametersContainer moduleParameters = Utils.GetParameters(furtherTransformationParameterChange, parameters, actualModule.Name, actualModule.Id);

                        moduleToBeAdded.Item2.ControlTypes = moduleParameters.ControlTypes;
                        moduleToBeAdded.Item2.ParameterKeys = moduleParameters.ParameterKeys;
                        moduleToBeAdded.Item2.Parameters = moduleParameters.Parameters;
                        moduleToBeAdded.Item2.PossibleValues = moduleParameters.PossibleValues;
                        moduleToBeAdded.Item2.PrintParameters = moduleParameters.PrintParameters;
                        moduleToBeAdded.Item2.UpdateParameterAction = moduleParameters.UpdateParameterAction;

                        Program.FurtherTransformations.Add(moduleToBeAdded);

                        Program.SelectedModuleParameters = moduleToBeAdded.Item2;

                        moduleToBeAdded.Item2.UpdateParameterAction(parametersToChange);

                        moduleToBeAdded.Item2.PrintParameters();

                        PendingUpdates.FurtherTransformations = true;
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                                        new ConsoleTextSpan("The module ", ConsoleColor.Red),
                                        new ConsoleTextSpan(actualModule.Name, ConsoleColor.Cyan),
                                        new ConsoleTextSpan(" (" + moduleId + ") can only be enabled once!", ConsoleColor.Red),
                        });
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("No Transformer module has been enabled!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (selectedModule.ModuleType == ModuleTypes.Coordinate)
            {
                if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    Program.CoordinatesModuleId = selectedModule.Id;

                    CoordinateModule actualModule = Modules.GetModule(Modules.CoordinateModules, selectedModule.Id);

                    List<(string, string)> parameters = actualModule.GetParameters(Program.TransformedTree);
                    parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

                    GenericParameterChangeDelegate coordinateParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
                    {
                        return actualModule.OnParameterChange(Program.TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                    };

                    Program.CoordinatesParameters = Utils.GetParameters(coordinateParameterChange, parameters, actualModule.Name, actualModule.Id);

                    Program.SelectedModuleParameters = Program.CoordinatesParameters;

                    Program.CoordinatesParameters.UpdateParameterAction(parametersToChange);

                    Program.CoordinatesParameters.PrintParameters();

                    PendingUpdates.Coordinates = true;
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("No Transformer module has been enabled!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (selectedModule.ModuleType == ModuleTypes.Plotting)
            {
                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    string moduleId = selectedModule.Id;

                    (string, ModuleParametersContainer) moduleToBeAdded = (moduleId, new ModuleParametersContainer());

                    PlottingModule actualModule = Modules.GetModule(Modules.PlottingModules, selectedModule.Id);

                    List<(string, string)> parameters = actualModule.GetParameters(Program.TransformedTree);
                    parameters.Add((Modules.ModuleIDKey, "Id:" + Guid.NewGuid().ToString()));

                    GenericParameterChangeDelegate plottingParameterChange = (Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange) =>
                    {
                        return actualModule.OnParameterChange(Program.TransformedTree, previousParameterValues, currentParameterValues, out controlStatus, out parametersToChange);
                    };

                    ModuleParametersContainer moduleParameters = Utils.GetParameters(plottingParameterChange, parameters, actualModule.Name, actualModule.Id);

                    moduleToBeAdded.Item2.ControlTypes = moduleParameters.ControlTypes;
                    moduleToBeAdded.Item2.ParameterKeys = moduleParameters.ParameterKeys;
                    moduleToBeAdded.Item2.Parameters = moduleParameters.Parameters;
                    moduleToBeAdded.Item2.PossibleValues = moduleParameters.PossibleValues;
                    moduleToBeAdded.Item2.PrintParameters = moduleParameters.PrintParameters;
                    moduleToBeAdded.Item2.UpdateParameterAction = moduleParameters.UpdateParameterAction;

                    Program.PlotActions.Add(moduleToBeAdded);

                    Program.SelectedModuleParameters = moduleToBeAdded.Item2;

                    moduleToBeAdded.Item2.UpdateParameterAction(parametersToChange);

                    moduleToBeAdded.Item2.PrintParameters();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("No Coordinates module has been enabled!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (selectedModule.ModuleType == ModuleTypes.Action)
            {
                ActionModule actualModule = Modules.GetModule(Modules.ActionModules, selectedModule.Id);

                if (actualModule.IsAvailableInCommandLine)
                {
                    actualModule.PerformAction(null, Program.StateData);
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("The specified module is not available in command-line mode!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (selectedModule.ModuleType == ModuleTypes.SelectionAction)
            {
                if (Program.SelectedNode != null && Program.TransformedTree.GetChildrenRecursive().Contains(Program.SelectedNode))
                {
                    SelectionActionModule actualModule = Modules.GetModule(Modules.SelectionActionModules, selectedModule.Id);

                    if (actualModule.IsAvailableInCommandLine)
                    {
                        actualModule.PerformAction(Program.SelectedNode, null, Program.StateData);
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("The specified module is not available in command-line mode!", ConsoleColor.Red) });
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("No node has been selected!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
        }
        private string CenterString(string toCenter, int totalWidth)
        {
            return new string(ConsoleWrapper.Whitespace, (totalWidth - toCenter.Length) / 2) + toCenter + new string(ConsoleWrapper.Whitespace, totalWidth - (totalWidth - toCenter.Length) / 2 - toCenter.Length);
        }

        private void PrintModules(IReadOnlyList<ModuleMetadata> modules, int maxModuleNameLength, bool printNumbers, int startIndex = 0)
        {

            maxModuleNameLength = Math.Max(maxModuleNameLength, 10);

            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
            {
                new ConsoleTextSpan("  " + new string('─', (printNumbers ? 8 : 0) + maxModuleNameLength + 4 + 36), 2)
            });

            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
            {
                new ConsoleTextSpan((printNumbers ? "  #       Module name" : "  Module name") + new string(ConsoleWrapper.Whitespace, maxModuleNameLength + 4 - "Module name".Length) + "Module Id", 2)
            });

            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
            {
                new ConsoleTextSpan("  " + new string('─', (printNumbers ? 8 : 0) + maxModuleNameLength + 4 + 36), 2)
            });

            for (int i = 0; i < modules.Count; i++)
            {
                ConsoleWrapper.WriteLine(new ConsoleTextSpan((printNumbers ? ("  #" + (i + startIndex).ToString() + new string(ConsoleWrapper.Whitespace, 7 - (i + startIndex).ToString().Length)) : "  ") + modules[i].Name + new string(ConsoleWrapper.Whitespace, maxModuleNameLength + 4 - modules[i].Name.Length) + modules[i].Id, 2, i % 2 == 0 ? ConsoleColor.Gray : ConsoleColor.White));
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "module list ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("enable", ConsoleColor.Yellow) }, "module enable ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("help", ConsoleColor.Yellow) }, "module help ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("disable", ConsoleColor.Yellow) }, "module disable ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("move", ConsoleColor.Yellow) }, "module move ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "module select ");
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

                if (firstWord.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    firstWordBuilder = new StringBuilder();

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

                    firstWord = firstWordBuilder.ToString();

                    if (firstWord.Equals("available", StringComparison.OrdinalIgnoreCase))
                    {
                        partialCommand = partialCommand.Substring(9).TrimStart();

                        foreach (string sr in ModuleTypeStrings)
                        {
                            if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("list available ", ConsoleColor.Yellow), new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "module list available " + sr + " ");
                            }
                        }

                    }
                    else
                    {
                        if ("available".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("list available", ConsoleColor.Yellow) }, "module list available");
                        }

                        if ("enabled".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("list enabled", ConsoleColor.Yellow) }, "module list enabled");
                        }
                    }
                }
                else if (firstWord.Equals("enable", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(6).TrimStart();

                    foreach (KeyValuePair<string, ModuleMetadata> mod in Modules.LoadedModulesMetadata)
                    {
                        if (mod.Value.ModuleType != ModuleTypes.FileType && mod.Value.ModuleType != ModuleTypes.LoadFile && (mod.Value.ModuleType != ModuleTypes.Action || Modules.GetModule(Modules.ActionModules, mod.Key).IsAvailableInCommandLine) && (mod.Value.ModuleType != ModuleTypes.SelectionAction || Modules.GetModule(Modules.SelectionActionModules, mod.Key).IsAvailableInCommandLine))
                        {
                            if (mod.Value.Name.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod.Value.Name, ConsoleColor.Blue) }, "module enable " + mod.Value.Name + " ");
                            }
                        }
                    }

                    foreach (KeyValuePair<string, ModuleMetadata> mod in Modules.LoadedModulesMetadata)
                    {
                        if (mod.Value.ModuleType != ModuleTypes.FileType && mod.Value.ModuleType != ModuleTypes.LoadFile && (mod.Value.ModuleType != ModuleTypes.Action || Modules.GetModule(Modules.ActionModules, mod.Key).IsAvailableInCommandLine) && (mod.Value.ModuleType != ModuleTypes.SelectionAction || Modules.GetModule(Modules.SelectionActionModules, mod.Key).IsAvailableInCommandLine))
                        {
                            if (mod.Value.Id.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod.Value.Id, ConsoleColor.Blue) }, "module enable " + mod.Value.Id + " ");
                            }
                        }
                    }
                }
                else if (firstWord.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    foreach (KeyValuePair<string, ModuleMetadata> mod in Modules.LoadedModulesMetadata)
                    {
                        if (mod.Value.Name.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod.Value.Name, ConsoleColor.Blue) }, "module help " + mod.Value.Name + " ");
                        }
                    }

                    foreach (KeyValuePair<string, ModuleMetadata> mod in Modules.LoadedModulesMetadata)
                    {
                        if (mod.Value.Id.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod.Value.Id, ConsoleColor.Blue) }, "module help " + mod.Value.Id + " ");
                        }
                    }
                }
                else if (firstWord.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(6).TrimStart();

                    List<string> currentModuleNames = new List<string>();
                    List<string> currentModuleIds = new List<string>();
                    List<string> currentModuleNumbers = new List<string>();

                    int startInd = 3;

                    if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.TransformerModuleId].Name);
                        currentModuleIds.Add(Program.TransformerModuleId);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                        currentModuleIds.Add(item.Item1);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.CoordinatesModuleId].Name);
                        currentModuleIds.Add(Program.CoordinatesModuleId);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                        currentModuleIds.Add(item.Item1);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach (string mod in currentModuleNames)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module select " + mod + " ");
                        }
                    }

                    foreach (string mod in currentModuleNumbers)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module select " + mod + " ");
                        }
                    }

                    foreach (string mod in currentModuleIds)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module select " + mod + " ");
                        }
                    }
                }
                else if (firstWord.Equals("disable", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(7).TrimStart();

                    List<string> currentModuleNames = new List<string>();
                    List<string> currentModuleIds = new List<string>();
                    List<string> currentModuleNumbers = new List<string>();

                    int startInd = 3;

                    if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.TransformerModuleId].Name);
                        currentModuleIds.Add(Program.TransformerModuleId);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                        currentModuleIds.Add(item.Item1);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.CoordinatesModuleId].Name);
                        currentModuleIds.Add(Program.CoordinatesModuleId);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                    {
                        currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                        currentModuleIds.Add(item.Item1);
                        currentModuleNumbers.Add("#" + startInd.ToString());
                        startInd++;
                    }

                    foreach (string mod in currentModuleNames)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module disable " + mod + " ");
                        }
                    }

                    foreach (string mod in currentModuleNumbers)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module disable " + mod + " ");
                        }
                    }

                    foreach (string mod in currentModuleIds)
                    {
                        if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module disable " + mod + " ");
                        }
                    }
                }
                else if (firstWord.Equals("move", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    firstWordBuilder = new StringBuilder();

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

                    firstWord = firstWordBuilder.ToString();

                    if (firstWord.Equals("up", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("down", StringComparison.OrdinalIgnoreCase))
                    {
                        partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                        List<string> currentModuleNames = new List<string>();
                        List<string> currentModuleIds = new List<string>();
                        List<string> currentModuleNumbers = new List<string>();

                        int startInd = 3;

                        if (!string.IsNullOrEmpty(Program.TransformerModuleId))
                        {
                            currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.TransformerModuleId].Name);
                            currentModuleIds.Add(Program.TransformerModuleId);
                            currentModuleNumbers.Add("#" + startInd.ToString());
                            startInd++;
                        }

                        foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                        {
                            currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                            currentModuleIds.Add(item.Item1);
                            currentModuleNumbers.Add("#" + startInd.ToString());
                            startInd++;
                        }

                        if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                        {
                            currentModuleNames.Add(Modules.LoadedModulesMetadata[Program.CoordinatesModuleId].Name);
                            currentModuleIds.Add(Program.CoordinatesModuleId);
                            currentModuleNumbers.Add("#" + startInd.ToString());
                            startInd++;
                        }

                        foreach ((string, ModuleParametersContainer) item in Program.PlotActions)
                        {
                            currentModuleNames.Add(Modules.LoadedModulesMetadata[item.Item1].Name);
                            currentModuleIds.Add(item.Item1);
                            currentModuleNumbers.Add("#" + startInd.ToString());
                            startInd++;
                        }

                        foreach (string mod in currentModuleNames)
                        {
                            if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module move " + firstWord + " " + mod + " ");
                            }
                        }

                        foreach (string mod in currentModuleNumbers)
                        {
                            if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module move " + firstWord + " " + mod + " ");
                            }
                        }

                        foreach (string mod in currentModuleIds)
                        {
                            if (mod.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(mod, ConsoleColor.Blue) }, "module move " + firstWord + " " + mod + " ");
                            }
                        }
                    }
                    else
                    {
                        if ("up".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("move up", ConsoleColor.Yellow) }, "module move up ");
                        }

                        if ("down".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("move down", ConsoleColor.Yellow) }, "module move down ");
                        }
                    }
                }
                else
                {
                    if ("list".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "module list ");
                    }

                    if ("enable".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("enable", ConsoleColor.Yellow) }, "module enable ");
                    }

                    if ("help".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("help", ConsoleColor.Yellow) }, "module help ");
                    }

                    if ("disable".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("disable", ConsoleColor.Yellow) }, "module disable ");
                    }

                    if ("select".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "module select ");
                    }

                    if ("move".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("module ", ConsoleColor.Green), new ConsoleTextSpan("move", ConsoleColor.Yellow) }, "module move ");
                    }
                }
            }
        }
    }
}
