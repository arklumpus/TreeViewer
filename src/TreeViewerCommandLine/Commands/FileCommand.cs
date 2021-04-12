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
using System.Text.RegularExpressions;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class FileCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("  file\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Shows the current input file path.\n\n", 4),
            new ConsoleTextSpan("  file ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<path to file>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Sets the current ", 4),
            new ConsoleTextSpan("input file path", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(". Note that this does not open the file: you must use the ", 4),
            new ConsoleTextSpan("open ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command next.", 4),
        };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Sets/shows the current input file path.")
        };

        public override string PrimaryCommand => "file";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                if (!string.IsNullOrEmpty(Program.InputFileName))
                {
                    ConsoleWrapper.WriteLine(Program.InputFileName);
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
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("file ", ConsoleColor.Green) }, "file ");

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
