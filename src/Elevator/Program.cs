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

using System.Collections.Generic;
using System.IO;

namespace Elevator
{
    class Program
    {
        static void Main(string[] args)
        {
            string executablePath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            string exePath;

#if DEBUG
            exePath = Path.Combine(executablePath, "..", "..", "..", "..", "TreeViewer", "bin", "Debug", "net5.0", "TreeViewer.exe");
#else
                exePath = Path.Combine(executablePath, "TreeViewer.exe");
#endif

            List<string> newArgs = new List<string>();
            newArgs.Add("--4e83aefc-1b77-4144-aa81-dc55cbca0329");
            newArgs.AddRange(args);
            args = newArgs.ToArray();

            System.Diagnostics.ProcessStartInfo info = new System.Diagnostics.ProcessStartInfo(exePath);

            foreach (string arg in args)
            {
                info.ArgumentList.Add(arg);
            }

            System.Diagnostics.Process proc = System.Diagnostics.Process.Start(info);

            proc.WaitForExit();
        }
    }
}
