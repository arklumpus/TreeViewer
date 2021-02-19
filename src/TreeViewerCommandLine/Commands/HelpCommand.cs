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
using System.Linq;
using System.Text;

namespace TreeViewerCommandLine
{
    class HelpCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("  help\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Provides a list of available commands.\n\n", 4),
            new ConsoleTextSpan("  help ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<command name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Describes the function of a command.", 4)
        };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Describes the function of a command.")
        };
        public override string PrimaryCommand => "help";

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
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

            if (string.IsNullOrWhiteSpace(firstWord))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(this.PrimaryCommand, ConsoleColor.Green) }, this.PrimaryCommand + " ");
            }

            if (firstWord == partialCommand)
            {
                foreach (Command command in Commands.AllAvailableCommands)
                {
                    if (command.PrimaryCommand.StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(this.PrimaryCommand + " ", ConsoleColor.Green), new ConsoleTextSpan(command.PrimaryCommand, ConsoleColor.Blue) }, this.PrimaryCommand + " " + command.PrimaryCommand + " ");
                    }
                }
            }
            else
            {
                yield break;
            }
        }

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine("  Available commands:");
                ConsoleWrapper.WriteLine();

                int maxCommandLength = (from el in Commands.AllAvailableCommands select el.PrimaryCommand.Length).Max();

                foreach (Command cmd in Commands.AllAvailableCommands)
                {
                    ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, 4));
                    ConsoleWrapper.Write(new ConsoleTextSpan(cmd.PrimaryCommand, ConsoleColor.Green));
                    ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, maxCommandLength - cmd.PrimaryCommand.Length + 4));
                    ConsoleWrapper.WriteLine(from el in cmd.ShortHelpText select new ConsoleTextSpan(el.Text, maxCommandLength + 8, el.Foreground, el.Background));
                }
                ConsoleWrapper.WriteLine();
            }
            else
            {
                command = command.Trim();
                ConsoleWrapper.WriteLine();
                bool found = false;
                foreach (Command cmd in Commands.AllAvailableCommands)
                {
                    if (cmd.PrimaryCommand.Equals(command, StringComparison.OrdinalIgnoreCase))
                    {
                        ConsoleWrapper.WriteLine(cmd.HelpText);
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    ConsoleWrapper.WriteLine("  Unknown command: " + command);
                }
                ConsoleWrapper.WriteLine();
            }
        }
    }
}
