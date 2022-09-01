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
using System.Linq;
using TreeViewer;

namespace TreeViewerCommandLine
{
    static class Commands
    {
        public static Command[] AllAvailableCommands = (from el in new Command[]
                                                        {
                                                            new HelpCommand(),
                                                            new ExitCommand(),
                                                            new QuitCommand(),
                                                            new FileCommand(),
                                                            new SystemCommand(),
                                                            new OpenCommand(),
                                                            new LoadCommand(),
                                                            new ModuleCommand(),
                                                            new OptionCommand(),
                                                            new UpdateCommand(),
                                                            new PDFCommand(),
                                                            new SVGCommand(),
                                                            new NodeCommand(),
                                                            new NewickCommand(),
                                                            new EnwkCommand(),
                                                            new NexusCommand(),
                                                            new BinaryCommand(),
                                                            new BackgroundCommand(),
                                                            new AttachmentCommand(),
                                                            new RegionCommand(),
                                                            new ResolutionCommand(),
                                                            new PNGCommand(),
                                                            new TIFFCommand()
                                                        }
                                                        orderby el.PrimaryCommand
                                                        select el).ToArray();

        public static IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
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

            if (firstWord == partialCommand)
            {
                foreach (Command command in AllAvailableCommands)
                {
                    if (command.PrimaryCommand.StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(command.PrimaryCommand, ConsoleColor.Green) }, command.PrimaryCommand + " ");
                    }
                }
            }
            else
            {
                Command whichCommand = null;

                foreach (Command command in AllAvailableCommands)
                {
                    if (command.PrimaryCommand.Equals(firstWord, StringComparison.OrdinalIgnoreCase))
                    {
                        whichCommand = command;
                    }
                }

                if (whichCommand != null)
                {
                    string restOfCommand = partialCommand.Substring(firstWord.Length);

                    List<(ConsoleTextSpan[], string)> items = new List<(ConsoleTextSpan[], string)>();

                    try
                    {
                        items.AddRange(whichCommand.GetCompletions(restOfCommand));
                    }
                    catch { }


                    foreach ((ConsoleTextSpan[], string) sr in items)
                    {
                        yield return sr;
                    }

                }
                else
                {
                    yield break;
                }
            }
        }

        public static bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return true;
            }

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

            Command whichCommand = null;

            foreach (Command commandItem in AllAvailableCommands)
            {
                if (commandItem.PrimaryCommand.Equals(firstWord, StringComparison.OrdinalIgnoreCase))
                {
                    whichCommand = commandItem;
                }
            }

            if (whichCommand != null)
            {
                string restOfCommand = command.Substring(firstWord.Length);
                whichCommand.Execute(restOfCommand);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public abstract class Command
    {
        public abstract string PrimaryCommand { get; }
        public abstract ConsoleTextSpan[] HelpText { get; }
        public abstract ConsoleTextSpan[] ShortHelpText { get; }
        public abstract IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand);
        public abstract void Execute(string command);
    }
}
