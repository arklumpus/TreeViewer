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

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.IO;

namespace TreeViewer
{
    public partial class UpdateWindow : ChildWindow
    {
        private string DownloadUrl;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        public UpdateWindow(string versionName, string downloadUrl)
        {
            InitializeComponent();
            this.FindControl<TextBlock>("VersionNameBlock").Text = versionName;
            this.DownloadUrl = downloadUrl;
            if (Modules.IsLinux)
            {
                this.FindControl<Button>("UpdateButton").IsVisible = false;
                this.FindControl<TextBlock>("WarningBlock").IsVisible = false;
                this.FindControl<Grid>("ButtonGrid").ColumnDefinitions[0].Width = new GridLength(0, GridUnitType.Pixel);
                this.FindControl<Grid>("ButtonGrid").ColumnDefinitions[1].Width = new GridLength(0, GridUnitType.Pixel);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.Updates")) { Width = 32, Height = 32 });
            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<Button>("DownloadButton").Click += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = DownloadUrl,
                    UseShellExecute = true
                });
                this.Close();
            };

            this.FindControl<Button>("UpdateButton").Click += async (s, e) =>
            {
                string remoteDownloadFile = null;
                string localDownloadFile = null;

                if (Modules.IsWindows)
                {
                    remoteDownloadFile = "https://github.com/arklumpus/TreeViewer/releases/latest/download/TreeViewer-Win-x64.msi";
                    localDownloadFile = Path.Combine(Path.GetTempPath(), "TreeViewer-Win-x64.msi");
                }
                else if (Modules.IsMac)
                {
                    remoteDownloadFile = "https://github.com/arklumpus/TreeViewer/releases/latest/download/TreeViewer-Mac-x64.pkg";
                    localDownloadFile = Path.Combine(Path.GetTempPath(), "TreeViewer-Mac-x64.pkg");
                }

                ProgressWindow win = new ProgressWindow() { IsIndeterminate = false, Progress = 0, ProgressText = "Downloading update...", LabelText = Path.GetFileName(remoteDownloadFile) };
                _ = win.ShowDialog2(this);

                double lastProgress = 0;

                await Modules.HttpClient.DownloadFileTaskAsync(new System.Uri(remoteDownloadFile), localDownloadFile, new System.Progress<double>(async progress =>
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (progress > lastProgress + 0.005)
                        {
                            win.Progress = progress;
                            lastProgress = progress;
                        }
                    });
                }));

                win.Close();

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = localDownloadFile,
                    UseShellExecute = true
                });

                System.Diagnostics.Process.GetCurrentProcess().Kill();
            };
        }
    }
}
