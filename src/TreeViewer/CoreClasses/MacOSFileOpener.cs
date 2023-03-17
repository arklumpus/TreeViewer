/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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

using Avalonia;
using System;
using System.Threading;

namespace TreeViewer
{
    internal static class MacOSFileOpener
    {
        private static SemaphoreSlim InitialisationSemaphore { get; } = new SemaphoreSlim(0, 1);
        private static SemaphoreSlim EventSemaphore { get; } = new SemaphoreSlim(1, 1);
        public static bool InitialisationCompleted { get; private set; } = false;

        public static void Initialise()
        {
            Application.Current.UrlsOpened += async (s, e) =>
            {
                await EventSemaphore.WaitAsync();
                await InitialisationSemaphore.WaitAsync();

                foreach (string url in e.Urls)
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        try
                        {
                            Uri uri = new Uri(url);
                            if (uri.IsFile)
                            {
                                if (GlobalSettings.Settings.MainWindows.Count > 0)
                                {
                                    await GlobalSettings.Settings.MainWindows[0].LoadFile(uri.AbsolutePath, false);
                                }
                                else
                                {
                                    MainWindow mainWindow = new MainWindow();
                                    mainWindow.Show();
                                    await mainWindow.LoadFile(uri.AbsolutePath, false);
                                }
                            }
                        }
                        catch { }
                    });
                }

                InitialisationSemaphore.Release();
                EventSemaphore.Release();
            };
        }

        public static void Proceed()
        {
            if (!InitialisationCompleted)
            {
                InitialisationCompleted = true;
                InitialisationSemaphore.Release();
            }
            
        }
    }
}
