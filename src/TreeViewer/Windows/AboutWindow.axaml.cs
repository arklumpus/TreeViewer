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

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Net;

namespace TreeViewer
{
    public class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.FindControl<TextBlock>("VersionTextBlock").Text = "Version " + Program.Version;

            this.FindControl<TextBlock>("GitHubTextBlock").PointerPressed += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "https://github.com/" + GlobalSettings.ProgramRepository,
                    UseShellExecute = true
                });
            };

            this.FindControl<Button>("CloseButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<Button>("CheckUpdatesButton").Click += async (s, e) =>
            {
                string releaseJson;

                ProgressWindow win = new ProgressWindow() { ProgressText = "Checking for updates..." };
                _ = win.ShowDialog(this);

                try
                {

                    using (WebClient client = new WebClient())
                    {
                        client.Headers.Add("User-Agent", "arklumpus/TreeViewer");

                        releaseJson = await client.DownloadStringTaskAsync("https://api.github.com/repos/" + GlobalSettings.ProgramRepository + "/releases");
                    }

                    ReleaseHeader[] releases = System.Text.Json.JsonSerializer.Deserialize<ReleaseHeader[]>(releaseJson);

                    win.Close();

                    Version currVers = new Version(Program.Version);

                    bool found = false;

                    for (int i = 0; i < releases.Length; i++)
                    {
                        try
                        {
                            if (!releases[i].prerelease)
                            {
                                Version version = new Version(releases[i].tag_name.Substring(1));

                                if (version > currVers)
                                {
                                    found = true;
                                    MessageBox box = new MessageBox("Check for updates", "A new version of TreeViewer has been released: " + releases[i].name + "!\nYou should keep the program updated to use the latest features and avoid security issues. Do you wish to open the download page?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                                    await box.ShowDialog(this);
                                    if (box.Result == MessageBox.Results.Yes)
                                    {
                                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                                        {
                                            FileName = releases[i].html_url,
                                            UseShellExecute = true
                                        });
                                    }
                                    break;
                                }
                            }
                        }
                        catch { }
                    }

                    if (!found)
                    {
                        MessageBox box = new MessageBox("Check for updates", "The program is up to date!", MessageBox.MessageBoxButtonTypes.OK, MessageBox.MessageBoxIconTypes.Tick);
                        await box.ShowDialog2(this);
                    }
                }
                catch (Exception ex)
                {
                    win.Close();

                    MessageBox box = new MessageBox("Attention", "An error occurred while checking for updates!\n" + ex.Message);
                    await box.ShowDialog2(this);
                }
            };
        }
    }
    internal class ReleaseHeader
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public bool prerelease { get; set; }
        public string html_url { get; set; }
    }
}
