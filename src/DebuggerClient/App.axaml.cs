using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Windows.Input;
using TreeViewer;

namespace DebuggerClient
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
            NativeMenu.SetMenu(this, menu);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
