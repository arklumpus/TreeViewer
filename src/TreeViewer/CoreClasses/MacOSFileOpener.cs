using Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
                                await GlobalSettings.Settings.MainWindows[0].LoadFile(uri.AbsolutePath, false);
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
