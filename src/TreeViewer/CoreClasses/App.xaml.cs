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

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            NativeMenu menu = new NativeMenu();
            menu.Add(new NativeMenuItem()
            {
                Header = "About...",
                Command = new SimpleCommand(win => true, a =>
                {
                    AboutWindow about = new AboutWindow();
                    about.Show();
                },
                null)
            });
            menu.Add(new NativeMenuItem()
            {
                Header = "New window...",
                Command = new SimpleCommand(win => true, a =>
                {
                    if (Modules.IsMac && GlobalSettings.Settings.MainWindows.Count == 1 && GlobalSettings.Settings.MainWindows[0].WindowState == WindowState.FullScreen)
                    {
                        GlobalSettings.Settings.MainWindows[0].WindowState = WindowState.Normal;

                        _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            GlobalSettings.Settings.MainWindows[0].PlatformImpl.GetType().InvokeMember("SetTitleBarColor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, GlobalSettings.Settings.MainWindows[0].PlatformImpl, new object[] { Avalonia.Media.Color.FromRgb(0, 114, 178) });

                            // Make sure that the pesky full screen animation has finished.
                            await Task.Delay(750);

                            MainWindow window = new MainWindow();
                            window.Show();
                        }, Avalonia.Threading.DispatcherPriority.MinValue);
                    }
                    else
                    {
                        MainWindow window = new MainWindow();
                        window.Show();
                    }
                },
                null)
            });
            NativeMenu.SetMenu(this, menu);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (Modules.IsMac)
                {
                    MacOSFileOpener.Initialise();
                }

                if (!File.Exists(Modules.ModuleListPath) || !Directory.Exists(Modules.ModulePath) || System.Convert.ToBoolean(desktop.Args[0]) || !GlobalSettings.Settings.PrivacyConsent)
                {
                    if (!File.Exists(Modules.ModuleListPath) || !Directory.Exists(Modules.ModulePath) || System.Convert.ToBoolean(desktop.Args[0]))
                    {
                        WelcomeWindow welcome = new WelcomeWindow(false, new string[0]);
                        desktop.MainWindow = welcome;
                    }
                    else if (!GlobalSettings.Settings.PrivacyConsent)
                    {
                        WelcomeWindow welcome = new WelcomeWindow(true, desktop.Args.Skip(3).ToArray());
                        desktop.MainWindow = welcome;
                    }
                }
                else if (System.Convert.ToBoolean(desktop.Args[1]))
                {
                    SplashScreen splashScreen = new SplashScreen(true, desktop.Args.Skip(3).ToArray());
                    desktop.MainWindow = splashScreen;
                }
                else if (System.Convert.ToBoolean(desktop.Args[2]))
                {
                    ModuleCreatorWindow window = new ModuleCreatorWindow(desktop.Args.Skip(3).ToArray());
                    desktop.MainWindow = window;
                }
                else
                {
                    SplashScreen splashScreen = new SplashScreen(desktop.Args.Skip(3).ToArray());
                    desktop.MainWindow = splashScreen;
                }

                base.OnFrameworkInitializationCompleted();
            }
            else
            {
                base.OnFrameworkInitializationCompleted();
            }
        }
    }
}
