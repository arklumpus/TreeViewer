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
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TreeViewer
{
    internal static class NamedPipes
    {
        public static string PipeName = @"TreeViewer-2d73e050-9900-4af4-9a71";

        private static EventWaitHandle serverHasStoppedHandle;


        public static void StartServer()
        {
            serverHasStoppedHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

            Task.Run(() =>
                {
                    bool hasBeenKilled = false;

                    while (!hasBeenKilled)
                    {
                        using (NamedPipeServerStream server = new NamedPipeServerStream(PipeName))
                        {
                            server.WaitForConnection();

                            try
                            {
                                using (StreamReader reader = new StreamReader(server))
                                {
                                    string command = reader.ReadToEnd();
                                    string[] files = command.Split("\t");

                                    bool deleteFiles = false;

                                    for (int i = 0; i < files.Length; i++)
                                    {
                                        if (files[i] == "::OpenWindow")
                                        {
                                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                            {
                                                MainWindow window = new MainWindow();
                                                window.Show();
                                            });
                                        }
                                        else if (files[i] == "::DeleteFiles")
                                        {
                                            deleteFiles = true;
                                        }
                                        else if (files[i] == "::NoDeleteFiles")
                                        {
                                            deleteFiles = false;
                                        }
                                        else if (files[i] == "::DoNothing")
                                        {

                                        }
                                        else if (files[i] == "::Die")
                                        {
                                            hasBeenKilled = true;
                                        }
                                        else
                                        {
                                            string file = files[i];

                                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                                            {
                                                try
                                                {
                                                    await GlobalSettings.Settings.MainWindows[0].LoadFile(file, deleteFiles);
                                                }
                                                catch { }
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ConsoleWrapper.WriteLine("The pipe server generated an error and it will be restarted!\n" + ex.Message);
                            }
                        }
                    }

                    serverHasStoppedHandle.Set();
                });
        }

        public static void StopServer()
        {
            TryStartClient(new string[] { "::DoNothing", "::Die" }, false);
            serverHasStoppedHandle.WaitOne();
        }

        public static bool TryStartClient(string[] files, bool startNewProcess)
        {
            if (files.Length == 1)
            {
                files = new string[] { files[0], "::OpenWindow" };
            }

            try
            {
                using (NamedPipeClientStream client = new NamedPipeClientStream(PipeName))
                {
                    client.Connect(100);
                    using (StreamWriter writer = new StreamWriter(client))
                    {
                        if (!startNewProcess)
                        {
                            writer.Write(files.Aggregate((a, b) => a + "\t" + b));
                        }
                        else
                        {
                            writer.Write("::DoNothing");
                        }

                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
