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
using System.Diagnostics;
using System.Text;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class SystemCommand : Command
    {
        public override string PrimaryCommand => "system";

        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
         {
            new ConsoleTextSpan("  system ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<command>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Runs a ", 4),
            new ConsoleTextSpan("command", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" in the system shell.", 4)
         };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Runs a command in the system shell.")
        };

        public override void Execute(string command)
        {
            command = command.TrimStart();

            if (Modules.IsWindows)
            {
                string args = "/S /C \"" + command + "\"";
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe", args);
                
                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
            else if (Modules.IsLinux)
            {
                string shell = GetShell();

                ProcessStartInfo info = new ProcessStartInfo(shell, "-c \"" + command.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
            else if (Modules.IsMac)
            {
                ProcessStartInfo info = new ProcessStartInfo("zsh", "-c \"" + command.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"");

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
        }

        private string GetShell()
        {
            if (Modules.IsLinux)
            {
                ProcessStartInfo info = new ProcessStartInfo("getent", "passwd " + Environment.GetEnvironmentVariable("LOGNAME"));
                info.RedirectStandardOutput = true;

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();

                string output = proc.StandardOutput.ReadToEnd();

                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);

                string shell = output.Split(':')[6].Trim();

                return shell;
            }

            return null;
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            yield break;
        }
    }
}
