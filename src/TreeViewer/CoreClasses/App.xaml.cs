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
                null, null)
            });
            NativeMenu.SetMenu(this, menu);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (!File.Exists(Modules.ModuleListPath) || !Directory.Exists(Modules.ModulePath) || System.Convert.ToBoolean(desktop.Args[0]))
                {
                    WelcomeWindow welcome = new WelcomeWindow();
                    desktop.MainWindow = welcome;
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
