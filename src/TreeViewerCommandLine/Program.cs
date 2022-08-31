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
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace TreeViewerCommandLine
{
    public class Program
    {
        public static string Version
        {
            get
            {
                return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString();
            }
        }

        public static EventWaitHandle ExitCommandLoopHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        public static string InputFileName { get; set; } = null;
        public static IEnumerable<TreeNode> FileOpener = null;
        public static FileInfo OpenedFileInfo = null;
        public static string OpenerModuleId = null;
        public static string LoaderModuleId = null;
        public static List<(string, Dictionary<string, object>)> ModuleSuggestions = null;

        public static string TransformerModuleId = null;
        public static ModuleParametersContainer TransformerParameters = null;

        public static string CoordinatesModuleId = null;
        public static ModuleParametersContainer CoordinatesParameters = null;

        public static List<(string, ModuleParametersContainer)> FurtherTransformations = new List<(string, ModuleParametersContainer)>();
        public static List<(string, ModuleParametersContainer)> PlotActions = new List<(string, ModuleParametersContainer)>();

        public static InstanceStateData StateData = new InstanceStateData();

        public static List<string> AttributeList { get; } = new List<string>();
        public static List<string> AttachmentList { get; } = new List<string>() { "(None)" };

        private static ModuleParametersContainer _selectedModuleParameters = null;

        public static TreeNode SelectedNode;
        public static string SelectedRegion = null;

        public static ModuleParametersContainer SelectedModuleParameters
        {
            get
            {
                return _selectedModuleParameters;
            }
            set
            {
                _selectedModuleParameters = value;
                SelectedOption = null;
            }
        }

        public static string SelectedOption = null;

        public static TreeCollection Trees
        {
            get
            {
                return StateData.Trees;
            }
            set
            {
                StateData.Trees = value;
            }
        }

        public static TreeNode FirstTransformedTree = null;
        public static TreeNode TransformedTree
        {
            get
            {
                return StateData.TransformedTree;
            }

            set
            {
                StateData.TransformedTree = value;
            }
        }



        public static TreeNode[] AllTransformedTrees = new TreeNode[0];
        public static Dictionary<string, Point> Coordinates = null;

        static void Main(string[] args)
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NO_COLOR")))
            {
                ConsoleWrapper.ColourEnabled = false;
            }

            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            InstanceStateData.IsUIAvailable = false;

            if (Console.IsInputRedirected)
            {
                InstanceStateData.IsInteractive = false;
            }

            Console.OutputEncoding = Encoding.UTF8;
            ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);

            ConsoleWrapper.WriteLine("TreeViewer v" + TreeViewer.Program.Version + " (command-line v" + Version + ")");
            ConsoleWrapper.WriteLine();

            ConsoleWrapper.Write("Loading installed modules...");

            _ = Modules.LoadInstalledModules(true, null);

            ConsoleWrapper.WriteLine(" Done.");
            ConsoleWrapper.WriteLine();

            StateData.AddPlottingModule = (module) =>
            {
                ModuleCommand.EnableModule(Modules.LoadedModulesMetadata[module.Id], -1, new Dictionary<string, object>());

                return PlotActions[^1].Item2.UpdateParameterAction;
            };

            StateData.RemovePlottingModule = (index) =>
            {
                ModuleCommand.DisableModule(PlotActions[index].Item2);
            };

            StateData.AddFurtherTransformationModule = (module) =>
            {
                ModuleCommand.EnableModule(Modules.LoadedModulesMetadata[module.Id], -1, new Dictionary<string, object>());

                return FurtherTransformations[^1].Item2.UpdateParameterAction;
            };

            StateData.RemoveFurtherTransformationModule = (index) =>
            {
                ModuleCommand.DisableModule(FurtherTransformations[index].Item2);
            };

            StateData.SetCoordinatesModule = (module) =>
            {
                ModuleCommand.EnableModule(Modules.LoadedModulesMetadata[module.Id], -1, new Dictionary<string, object>());

                return CoordinatesParameters.UpdateParameterAction;
            };

            StateData.SetTransformerModule = (module) =>
            {
                ModuleCommand.EnableModule(Modules.LoadedModulesMetadata[module.Id], -1, new Dictionary<string, object>());

                return TransformerParameters.UpdateParameterAction;
            };

            StateData.TransformerModule = () =>
            {
                if (!string.IsNullOrEmpty(TransformerModuleId))
                {
                    return Modules.GetModule(Modules.TransformerModules, TransformerModuleId);
                }
                else
                {
                    return null;
                }

            };

            StateData.CoordinateModule = () =>
            {
                if (!string.IsNullOrEmpty(CoordinatesModuleId))
                {
                    return Modules.GetModule(Modules.CoordinateModules, TransformerModuleId);
                }
                else
                {
                    return null;
                }

            };

            StateData.PlottingModules = () => (from el in PlotActions select Modules.GetModule(Modules.PlottingModules, el.Item1)).ToList();
            StateData.FurtherTransformationModules = () => (from el in FurtherTransformations select Modules.GetModule(Modules.FurtherTransformationModules, el.Item1)).ToList();

            StateData.GetSelectedNode = () => SelectedNode;
            StateData.SetSelectedNode = (value) => SelectedNode = value;

            StateData.TransformerModuleParameterUpdater = () => TransformerParameters.UpdateParameterAction;
            StateData.CoordinatesModuleParameterUpdater = () => CoordinatesParameters.UpdateParameterAction;

            StateData.FurtherTransformationModulesParameterUpdater = (index) => FurtherTransformations[index].Item2.UpdateParameterAction;
            StateData.PlottingModulesParameterUpdater = (index) => PlotActions[index].Item2.UpdateParameterAction;

            StateData.GetTransformerModuleParameters = () => TransformerParameters.Parameters;
            StateData.GetCoordinatesModuleParameters = () => CoordinatesParameters.Parameters;

            StateData.GetFurtherTransformationModulesParamters = (index) => FurtherTransformations[index].Item2.Parameters;
            StateData.GetPlottingModulesParameters = (index) => PlotActions[index].Item2.Parameters;

            StateData.OpenFile = (fileName, deleteAfter) =>
                {
                    try
                    {
                        Program.InputFileName = fileName;
                        OpenCommand.OpenFile(Program.InputFileName, null);
                        LoadCommand.LoadFile(null);

                        if (deleteAfter)
                        {
                            try
                            {
                                File.Delete(fileName);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("    An error occurred while opening the file!\n" + ex.Message, 4, ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                };

            StateData.SerializeAllModules = Program.SerializeAllModules;

            ConsoleWrapper.TreatControlCAsInput = true;

            

            ConsoleWrapper.WriteLine("Type \"help\" for a list of available commands");
            ConsoleWrapper.WriteLine();

            while (!ExitCommandLoopHandle.WaitOne(0))
            {
                string command = ReadCommand();

                if (ConsoleWrapper.CursorTop >= ConsoleWrapper.BufferHeight - ConsoleWrapper.WindowHeight)
                {
                    ConsoleWrapper.MoveBufferAndClear();
                }

                try
                {
                    bool result = Commands.ExecuteCommand(command);
                    if (!result)
                    {
                        ConsoleWrapper.WriteLine("Unknown command: " + command);
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[]{
                    new ConsoleTextSpan("An error occurred while executing command: ", ConsoleColor.Red),
                    new ConsoleTextSpan(command + "\n"),
                    new ConsoleTextSpan("Error message:", ConsoleColor.Red) });

                    ConsoleWrapper.WriteLine(from el in ex.Message.Split('\n') select new ConsoleTextSpan("  " + el + "\n", 2));
                }
            }
        }

        static List<string> CommandHistory = new List<string>();

        public static string SerializeAllModules(MainWindow.ModuleTarget target, bool addSignature)
        {
            List<List<string[]>> allModules = new List<List<string[]>>();

            List<string[]> transformerModule = new List<string[]>();

            if (target == MainWindow.ModuleTarget.AllModules)
            {
                transformerModule.Add(new string[] { TransformerModuleId, MainWindow.SerializeParameters(TransformerParameters.Parameters) });
            }

            allModules.Add(transformerModule);

            List<string[]> furtherTransformationModules = new List<string[]>();

            if (target == MainWindow.ModuleTarget.AllModules || target == MainWindow.ModuleTarget.ExcludeTransform)
            {
                for (int i = 0; i < FurtherTransformations.Count; i++)
                {
                    furtherTransformationModules.Add(new string[] { FurtherTransformations[i].Item1, MainWindow.SerializeParameters(FurtherTransformations[i].Item2.Parameters) });
                }
            }

            allModules.Add(furtherTransformationModules);

            List<string[]> coordinatesModule = new List<string[]>() { new string[] { CoordinatesModuleId, MainWindow.SerializeParameters(CoordinatesParameters.Parameters) } };
            allModules.Add(coordinatesModule);

            List<string[]> plottingActionModules = new List<string[]>();
            plottingActionModules.Add(new string[] { "@Background", MainWindow.SerializeParameters(new Dictionary<string, object>() { { "Colour", StateData.GraphBackgroundColour } }) });
            for (int i = 0; i < PlotActions.Count; i++)
            {
                plottingActionModules.Add(new string[] { PlotActions[i].Item1, MainWindow.SerializeParameters(PlotActions[i].Item2.Parameters) });
            }
            allModules.Add(plottingActionModules);

            if (!addSignature)
            {
                return System.Text.Json.JsonSerializer.Serialize(allModules);
            }
            else
            {
                string serializedModules = System.Text.Json.JsonSerializer.Serialize(allModules);

                string signature = CryptoUtils.SignString(serializedModules, CryptoUtils.FileRSAEncrypter);

                string publicKeySerialized = System.Text.Json.JsonSerializer.Serialize(new CryptoUtils.PublicKeyHolder(CryptoUtils.UserPublicKey));

                allModules.Insert(0, new List<string[]>
                {
                    new string[]
                    {
                        CryptoUtils.FileSignatureGuid,
                        signature,
                        publicKeySerialized
                    }
                });

                return System.Text.Json.JsonSerializer.Serialize(allModules);
            }
        }



        static string ReadCommand()
        {
            ConsoleWrapper.Write(">");

            int commandStart = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;

            int lastCursorPos = commandStart;

            StringBuilder command = new StringBuilder();

            bool finishedCommand = false;

            void onWindowResize(object sender, ConsoleWindowResizedEventArgs e)
            {
                int newCursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;

                int newCommandStart = newCursorPos - lastCursorPos + commandStart;

                commandStart = newCommandStart;
                lastCursorPos = newCursorPos;
            }


            if (!Console.IsOutputRedirected && !Console.IsInputRedirected)
            {

                int historyIndex = -1;
                Dictionary<int, string> tempHistoryOverrides = new Dictionary<int, string>() { { -1, "" } };

                ConsoleKeyInfo ki;

                int tabCount = 0;


                ConsoleWrapper.ConsoleWindowResized += onWindowResize;

                while (!finishedCommand)
                {
                    ki = ConsoleWrapper.ReadKey(true);

                    if (ki.Key != ConsoleKey.Tab)
                    {
                        tabCount = 0;
                    }

                    if (ki.Key == ConsoleKey.UpArrow || ki.Key == ConsoleKey.DownArrow)
                    {
                        if (ki.Key == ConsoleKey.UpArrow)
                        {
                            historyIndex = Math.Min(historyIndex + 1, CommandHistory.Count - 1);
                        }
                        else if (ki.Key == ConsoleKey.DownArrow)
                        {
                            historyIndex = Math.Max(historyIndex - 1, -1);
                        }

                        int oldCommandLength = command.Length;

                        if (tempHistoryOverrides.TryGetValue(historyIndex, out string cmd))
                        {
                            command.Clear();
                            command.Append(cmd);
                        }
                        else
                        {
                            command.Clear();
                            command.Append(CommandHistory[CommandHistory.Count - 1 - historyIndex]);
                        }

                        ConsoleWrapper.SetCursorPosition(commandStart % ConsoleWrapper.WindowWidth, commandStart / ConsoleWrapper.WindowWidth);
                        ConsoleWrapper.Write(command.ToString());
                        if (oldCommandLength > command.Length)
                        {
                            ConsoleWrapper.CursorVisible = false;
                            ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, oldCommandLength - command.Length));
                            ConsoleWrapper.SetCursorPosition((commandStart + command.Length) % ConsoleWrapper.WindowWidth, (commandStart + command.Length) / ConsoleWrapper.WindowWidth);
                            ConsoleWrapper.CursorVisible = true;
                        }
                    }
                    else
                    {
                        if (ki.Key == ConsoleKey.Backspace)
                        {
                            int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                            if (cursorPos > commandStart)
                            {
                                command.Remove(cursorPos - commandStart - 1, 1);

                                ConsoleWrapper.CursorVisible = false;
                                ConsoleWrapper.Write('\b');
                                ConsoleWrapper.Write(command.ToString(cursorPos - commandStart - 1, command.Length - cursorPos + commandStart + 1));
                                ConsoleWrapper.Write(ConsoleWrapper.Whitespace);

                                int newPos = Math.Max(cursorPos - 1, commandStart);
                                ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                                ConsoleWrapper.CursorVisible = true;
                            }
                        }
                        else if (ki.Key == ConsoleKey.Delete)
                        {
                            int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                            if (cursorPos - commandStart < command.Length)
                            {
                                command.Remove(cursorPos - commandStart, 1);

                                ConsoleWrapper.CursorVisible = false;
                                ConsoleWrapper.Write(command.ToString(cursorPos - commandStart, command.Length - cursorPos + commandStart));
                                ConsoleWrapper.Write(ConsoleWrapper.Whitespace);

                                int newPos = Math.Max(cursorPos, commandStart);
                                ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                                ConsoleWrapper.CursorVisible = true;
                            }
                        }
                        else if (ki.Key == ConsoleKey.Home)
                        {
                            ConsoleWrapper.SetCursorPosition(commandStart % ConsoleWrapper.WindowWidth, commandStart / ConsoleWrapper.WindowWidth);
                        }
                        else if (ki.Key == ConsoleKey.End)
                        {
                            ConsoleWrapper.SetCursorPosition((commandStart + command.Length) % ConsoleWrapper.WindowWidth, (commandStart + command.Length) / ConsoleWrapper.WindowWidth);

                        }
                        else if (ki.Key == ConsoleKey.LeftArrow)
                        {
                            int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                            int newPos = Math.Max(cursorPos - 1, commandStart);
                            ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                        }
                        else if (ki.Key == ConsoleKey.RightArrow)
                        {
                            int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                            int newPos = Math.Min(cursorPos + 1, commandStart + command.Length);
                            ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                        }
                        else if (ki.Key == ConsoleKey.Tab)
                        {
                            List<(ConsoleTextSpan[], string)> completions = new List<(ConsoleTextSpan[], string)>(Commands.GetCompletions(command.ToString()));

                            if (completions.Count == 0)
                            {
                                Console.Write('\a');
                            }
                            else if (completions.Count == 1)
                            {
                                int oldCommandLength = command.Length;

                                command.Clear();
                                command.Append(completions[0].Item2);

                                ConsoleWrapper.SetCursorPosition(commandStart % ConsoleWrapper.WindowWidth, commandStart / ConsoleWrapper.WindowWidth);
                                ConsoleWrapper.Write(command.ToString());
                                if (oldCommandLength > command.Length)
                                {
                                    ConsoleWrapper.CursorVisible = false;
                                    ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, oldCommandLength - command.Length));
                                    ConsoleWrapper.SetCursorPosition((commandStart + command.Length) % ConsoleWrapper.WindowWidth, (commandStart + command.Length) / ConsoleWrapper.WindowWidth);
                                    ConsoleWrapper.CursorVisible = true;
                                }
                            }
                            else
                            {
                                if (tabCount == 0)
                                {
                                    Console.Write('\a');

                                    string newCommand = (from el in completions select el.Item2).CommonStart(false);

                                    int oldCommandLength = command.Length;

                                    command.Clear();
                                    command.Append(newCommand);

                                    ConsoleWrapper.SetCursorPosition(commandStart % ConsoleWrapper.WindowWidth, commandStart / ConsoleWrapper.WindowWidth);
                                    ConsoleWrapper.Write(command.ToString());
                                    if (oldCommandLength > command.Length)
                                    {
                                        ConsoleWrapper.CursorVisible = false;
                                        ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, oldCommandLength - command.Length));
                                        ConsoleWrapper.SetCursorPosition((commandStart + command.Length) % ConsoleWrapper.WindowWidth, (commandStart + command.Length) / ConsoleWrapper.WindowWidth);
                                        ConsoleWrapper.CursorVisible = true;
                                    }

                                    tabCount++;
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteList(from el in completions select el.Item1, "  ");
                                    ConsoleWrapper.Write(">");
                                    commandStart = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                                    ConsoleWrapper.Write(command.ToString());
                                }
                            }
                        }
                        else if (ki.Key == ConsoleKey.C && ki.Modifiers == ConsoleModifiers.Control)
                        {
                            ConsoleWrapper.WriteLine("^C");
                            return null;
                        }
                        else if (ki.Key == ConsoleKey.D && ki.Modifiers == ConsoleModifiers.Control)
                        {
                            ConsoleWrapper.WriteLine("^D");
                            ConsoleWrapper.WriteLine(">exit");
                            return "Exit";
                        }
                        else
                        {
                            if (ki.Key == ConsoleKey.Enter)
                            {
                                ConsoleWrapper.WriteLine();
                                finishedCommand = true;
                            }
                            else if (ConsoleWrapper.IsPrintable(ki.KeyChar))
                            {
                                int cursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                                command.Insert(cursorPos - commandStart, ki.KeyChar);

                                ConsoleWrapper.CursorVisible = false;
                                ConsoleWrapper.Write(command.ToString(cursorPos - commandStart, command.Length - cursorPos + commandStart));

                                int newPos = Math.Min(cursorPos + 1, commandStart + command.Length);
                                ConsoleWrapper.SetCursorPosition(newPos % ConsoleWrapper.WindowWidth, newPos / ConsoleWrapper.WindowWidth);
                                ConsoleWrapper.CursorVisible = true;
                            }
                        }

                        tempHistoryOverrides[historyIndex] = command.ToString();
                    }

                    lastCursorPos = ConsoleWrapper.CursorLeft + ConsoleWrapper.CursorTop * ConsoleWrapper.WindowWidth;
                }
            }
            else
            {
                ConsoleKeyInfo ki;

                while (!finishedCommand)
                {
                    ki = ConsoleWrapper.ReadKey(true);

                    if (ki.Key == ConsoleKey.C && ki.Modifiers == ConsoleModifiers.Control)
                    {
                        ConsoleWrapper.WriteLine("^C");
                        return null;
                    }
                    else if (ki.Key == ConsoleKey.D && ki.Modifiers == ConsoleModifiers.Control)
                    {
                        ConsoleWrapper.WriteLine("^D");
                        ConsoleWrapper.WriteLine(">exit");
                        return "Exit";
                    }
                    else
                    {
                        if (ki.Key == ConsoleKey.Enter)
                        {
                            ConsoleWrapper.WriteLine();
                            finishedCommand = true;
                        }
                        else if (ConsoleWrapper.IsPrintable(ki.KeyChar))
                        {
                            command.Append(ki.KeyChar);

                            ConsoleWrapper.Write(ki.KeyChar);
                        }
                    }
                }
            }

            CommandHistory.Add(command.ToString());

            if (!Console.IsOutputRedirected)
            {
                ConsoleWrapper.ConsoleWindowResized -= onWindowResize;
            }

            return command.ToString();
        }
    }
}
