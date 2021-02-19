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
using PhyloTree;
using PhyloTree.Formats;

namespace TreeViewerCommandLine
{
    class NexusCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
         {
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("]\n", 2),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the final transformed tree in NEXUS format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" nexus ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, the currently enabled Coordinates and Plot action modules are also exported.\n\n", 4),


            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the loaded tree(s) in NEXUS format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" nexus ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, all the currently enabled modules are also exported.\n\n", 4),

            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  nexus ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the first transformed tree in NEXUS format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" nexus ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, the currently enabled Further transformation, Coordinates and Plot action modules are also exported.", 4),
         };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Exports a tree in NEXUS format.")
        };

        public override string PrimaryCommand => "nexus";


        static string lastFileName = null;

        public override void Execute(string command)
        {
            UpdateCommand.UpdateRequired();

            if (string.IsNullOrWhiteSpace(command))
            {
                if (Program.TransformedTree != null)
                {
                    if (string.IsNullOrEmpty(lastFileName))
                    {
                        using (Stream sr = Console.OpenStandardOutput())
                        {
                            ExportToStream(sr, 2, false, false);
                        }
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                        {
                            ExportToStream(fs, 2, false, false);
                        }
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else
            {
                command = command.Trim();

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

                int treeIndex = 2;

                bool modules = false;

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("modules", StringComparison.OrdinalIgnoreCase))
                {
                    modules = true;

                    command = command.Substring(7).TrimStart();

                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

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

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase))
                {
                    treeIndex = 0;

                    command = command.Substring(6).TrimStart();

                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

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

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                {
                    treeIndex = 1;

                    command = command.Substring(11).TrimStart();


                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

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

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (string.IsNullOrWhiteSpace(firstWord))
                {
                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    lastFileName = null;
                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    lastFileName = command.Trim('\"');

                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && AskIfShouldAddSignature());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
            }

        }

        public static bool AskIfShouldAddSignature()
        {
            ConsoleWrapper.Write(new ConsoleTextSpan[]
            {
                            new ConsoleTextSpan("Would you like to sign the file? [Y(es)/N(o)] ", ConsoleColor.Yellow)
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
                return true;
            }
            else
            {
                return false;
            }
        }

        void ExportToStream(Stream stream, int subject, bool modules, bool addSignature)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                if (subject == 0)
                {
                    sw.WriteLine("#NEXUS");
                    sw.WriteLine();
                    sw.WriteLine("Begin Trees;");
                    int count = 0;
                    foreach (TreeNode tree in Program.Trees)
                    {
                        if (tree.Attributes.ContainsKey("TreeName"))
                        {
                            sw.Write("\tTree " + tree.Attributes["TreeName"].ToString() + " = ");
                        }
                        else
                        {
                            sw.Write("\tTree tree" + count.ToString() + " = ");
                        }

                        sw.Write(NWKA.WriteTree(tree, true));
                        sw.WriteLine();
                        count++;
                    }
                    sw.WriteLine("End;");

                    if (modules)
                    {
                        sw.WriteLine();
                        sw.WriteLine("Begin TreeViewer;");
                        string serializedModules = Program.SerializeAllModules(TreeViewer.MainWindow.ModuleTarget.AllModules, addSignature);
                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                        sw.WriteLine(serializedModules);
                        sw.WriteLine("End;");
                    }
                }
                else if (subject == 1)
                {
                    sw.WriteLine("#NEXUS");
                    sw.WriteLine();
                    sw.WriteLine("Begin Trees;");
                    if (Program.FirstTransformedTree.Attributes.ContainsKey("TreeName"))
                    {
                        sw.Write("\tTree " + Program.FirstTransformedTree.Attributes["TreeName"].ToString() + " = ");
                    }
                    else
                    {
                        sw.Write("\tTree tree = ");
                    }

                    sw.Write(NWKA.WriteTree(Program.FirstTransformedTree, true));
                    sw.WriteLine();
                    sw.WriteLine("End;");

                    if (modules)
                    {
                        sw.WriteLine();
                        sw.WriteLine("Begin TreeViewer;");
                        string serializedModules = Program.SerializeAllModules(TreeViewer.MainWindow.ModuleTarget.ExcludeTransform, addSignature);
                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                        sw.WriteLine(serializedModules);
                        sw.WriteLine("End;");
                    }
                }
                else if (subject == 2)
                {
                    sw.WriteLine("#NEXUS");
                    sw.WriteLine();
                    sw.WriteLine("Begin Trees;");
                    if (Program.TransformedTree.Attributes.ContainsKey("TreeName"))
                    {
                        sw.Write("\tTree " + Program.TransformedTree.Attributes["TreeName"].ToString() + " = ");
                    }
                    else
                    {
                        sw.Write("\tTree tree = ");
                    }

                    sw.Write(NWKA.WriteTree(Program.TransformedTree, true));
                    sw.WriteLine();
                    sw.WriteLine("End;");

                    if (modules)
                    {
                        sw.WriteLine();
                        sw.WriteLine("Begin TreeViewer;");
                        string serializedModules = Program.SerializeAllModules(TreeViewer.MainWindow.ModuleTarget.ExcludeFurtherTransformation, addSignature);
                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                        sw.WriteLine(serializedModules);
                        sw.WriteLine("End;");
                    }
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus", ConsoleColor.Green) }, "nexus ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "nexus modules ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "nexus stdout ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "nexus loaded ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "nexus transformed ");

                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus"))
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

                if (firstWord.Equals("modules", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(7).TrimStart();

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "nexus modules ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "nexus modules stdout ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules loaded", ConsoleColor.Yellow) }, "nexus modules loaded ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules transformed", ConsoleColor.Yellow) }, "nexus modules transformed ");

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus modules"))
                        {
                            yield return item;
                        }
                    }
                    else
                    {
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

                        if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "nexus modules stdout ");
                        }
                        else if (firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                        {
                            partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                            if (string.IsNullOrWhiteSpace(partialCommand))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan(firstWord, ConsoleColor.Yellow) }, "nexus modules " + firstWord + " ");
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules " + firstWord + " stdout", ConsoleColor.Yellow) }, "nexus modules " + firstWord + " stdout ");

                                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus modules " + firstWord))
                                {
                                    yield return item;
                                }
                            }
                            else
                            {
                                if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules " + firstWord + " stdout", ConsoleColor.Yellow) }, "nexus modules " + firstWord + " stdout ");
                                }

                                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus modules " + firstWord))
                                {
                                    yield return item;
                                }
                            }
                        }
                        else
                        {
                            if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "nexus modules stdout ");
                            }

                            if ("loaded".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules loaded", ConsoleColor.Yellow) }, "nexus modules loaded ");
                            }

                            if ("transformed".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules transformed", ConsoleColor.Yellow) }, "nexus modules transformed ");
                            }

                            foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus modules"))
                            {
                                yield return item;
                            }
                        }
                    }
                }
                else if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "nexus stdout ");
                }
                else if (firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan(firstWord, ConsoleColor.Yellow) }, "nexus " + firstWord + " ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "nexus " + firstWord + " stdout ");

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus " + firstWord))
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "nexus " + firstWord + " stdout ");
                        }

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus " + firstWord))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    if ("modules".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "nexus modules ");
                    }

                    if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "nexus stdout ");
                    }

                    if ("loaded".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "nexus loaded ");
                    }

                    if ("transformed".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("nexus ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "nexus transformed ");
                    }

                    foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "nexus"))
                    {
                        yield return item;
                    }
                }

            }
        }
    }
}
