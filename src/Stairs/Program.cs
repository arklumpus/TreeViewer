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

using System.IO;

namespace Stairs
{
    class Program
    {
        static void Main(string[] args)
        {
            int argsForElevator = int.Parse(args[0]);
            int argsForNormal = int.Parse(args[1]);

            string executablePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            string exePath;

#if DEBUG
            exePath = Path.Combine(executablePath, "..", "..", "..", "..", "Elevator", "bin", "Debug", "net5.0", "Elevator.exe");
#else
            exePath = Path.Combine(executablePath, "Elevator.exe");
#endif
            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(exePath);

            for (int i = 2; i < 2 + argsForElevator; i++)
            {
                info.ArgumentList.Add(args[i]);
            }

            info.UseShellExecute = true;

            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);

            proc.WaitForExit();

#if DEBUG
            exePath = Path.Combine(executablePath, "..", "..", "..", "..", "TreeViewer", "bin", "Debug", "net5.0", "TreeViewer.exe");
#else
            exePath = Path.Combine(executablePath, "TreeViewer.exe");
#endif
            info = new System.Diagnostics.ProcessStartInfo(exePath);

            for (int i = 2 + argsForElevator; i < 2 + argsForElevator + argsForNormal; i++)
            {
                info.ArgumentList.Add(args[i]);
            }

            info.UseShellExecute = false;

            System.Diagnostics.Process.Start(info);
        }
    }
}
