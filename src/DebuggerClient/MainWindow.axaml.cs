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
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CSharpEditor;
using System.Threading.Tasks;
using System;
using TreeViewer;

namespace DebuggerClient
{
    public class MainWindow : TreeViewer.ChildWindow
    {
        bool terminating = false;
        public MainWindow()
        {
            InitializeComponent();

            InterprocessDebuggerClient client = new InterprocessDebuggerClient(Program.CallingArguments);
            this.Content = client;

            client.BreakpointHit += async (s, e) =>
            {
                this.Show();
                this.WindowState = WindowState.Normal;
                this.Activate();

                // AvaloniaBugFixes:
                // Fix issue on Linux that causes the debugger interface not to be updated until the user interacts with the window.
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                {
                    await Task.Delay(100);
                    this.Width++;
                    await Task.Delay(100);
                    this.Width--;
                }
            };

            client.BreakpointResumed += (s, e) =>
            {
                this.Hide();
            };

            client.ParentProcessExited += (s, e) =>
            {
                terminating = true;
                this.Close();
            };


            this.Closing += (s, e) =>
            {
                if (!terminating)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };

            this.Opened += (s, e) =>
            {
                ((Editor)client.Content).Background = this.Background;
            };
        }

        bool initialized = false;
        public override void Show()
        {
            if (!initialized)
            {
                initialized = true;
            }
            else
            {
                base.Show();
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == Window.IsVisibleProperty)
            {
                if (this.IsVisible)
                {
                    this.ShowInTaskbar = true;

                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    {
                        this.SystemDecorations = SystemDecorations.Full;
                        this.WindowState = WindowState.Normal;
                    }
                }
                else
                {
                    this.ShowInTaskbar = false;

                    if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
                    {
                        this.SystemDecorations = SystemDecorations.None;
                        this.WindowState = WindowState.Minimized;
                    }
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
