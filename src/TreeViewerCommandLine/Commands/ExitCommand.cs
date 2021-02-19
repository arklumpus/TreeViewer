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

namespace TreeViewerCommandLine
{
    class ExitCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("  exit\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Closes the program (same as ", 4),
            new ConsoleTextSpan("quit", 4, ConsoleColor.Green),
            new ConsoleTextSpan(").", 4)
        };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Closes the program.")
        };


        public override string PrimaryCommand => "exit";

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            yield break;
        }

        public override void Execute(string command)
        {
            Program.ExitCommandLoopHandle.Set();
        }
    }

    class QuitCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("  quit\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Closes the program (same as ", 4),
            new ConsoleTextSpan("exit", 4, ConsoleColor.Green),
            new ConsoleTextSpan(").", 4)
        };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Closes the program.")
        };

        public override string PrimaryCommand => "quit";

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            yield break;
        }

        public override void Execute(string command)
        {
            Program.ExitCommandLoopHandle.Set();
        }
    }

}
