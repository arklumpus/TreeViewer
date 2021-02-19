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
    class EnwkCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
         {
            new ConsoleTextSpan("  enwk\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the final transformed tree in Extended Newick format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" enwk ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output).\n\n", 4),

            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("loaded\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("loaded ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("loaded stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the loaded tree(s) in Extended Newick format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" enwk ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output).\n\n", 4),

            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("transformed\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("transformed ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  enwk ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("transformed stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the first transformed tree in Extended Newick format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" enwk ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output).", 4),
         };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Exports a tree in Extended Newick format.")
        };

        public override string PrimaryCommand => "enwk";


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
                            ExportToStream(sr, 2);
                        }
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                        {
                            ExportToStream(fs, 2);
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
                                ExportToStream(sr, treeIndex);
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex);
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
                                ExportToStream(sr, treeIndex);
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex);
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
                                ExportToStream(sr, treeIndex);
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex);
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

        void ExportToStream(Stream stream, int subject)
        {
            using (StreamWriter sw = new StreamWriter(stream))
            {
                if (subject == 0)
                {
                    foreach (TreeNode tree in Program.Trees)
                    {
                        sw.WriteLine(NWKA.WriteTree(tree, true));
                    }
                }
                else if (subject == 1)
                {
                    sw.WriteLine(NWKA.WriteTree(Program.FirstTransformedTree, true));
                }
                else if (subject == 2)
                {
                    sw.WriteLine(NWKA.WriteTree(Program.TransformedTree, true));
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk", ConsoleColor.Green) }, "enwk ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "enwk stdout ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "enwk loaded ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "enwk transformed ");

                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "enwk"))
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


                if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "enwk stdout ");
                }
                else if (firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan(firstWord, ConsoleColor.Yellow) }, "enwk " + firstWord + " ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "enwk " + firstWord + " stdout ");

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "enwk " + firstWord))
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "enwk " + firstWord + " stdout ");
                        }

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "enwk " + firstWord))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "enwk stdout ");
                    }

                    if ("loaded".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "enwk loaded ");
                    }

                    if ("transformed".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("enwk ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "enwk transformed ");
                    }

                    foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "enwk"))
                    {
                        yield return item;
                    }
                }

            }
        }
    }
}
