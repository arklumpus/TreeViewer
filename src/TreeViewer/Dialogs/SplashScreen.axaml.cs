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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();
        }

        private string[] filesToOpen;
        private bool openFileAssociations = false;

        public SplashScreen(string[] filesToOpen)
        {
            InitializeComponent();
            this.filesToOpen = filesToOpen;
        }

        public SplashScreen(bool openFileAssociations, string[] filesToOpen)
        {
            InitializeComponent();
            this.filesToOpen = filesToOpen;
            this.openFileAssociations = openFileAssociations;
        }

        public Func<Task> OnModulesLoaded = null;

        EventWaitHandle ProgramStartHandle = new EventWaitHandle(false, EventResetMode.ManualReset);

        protected override async void OnOpened(EventArgs e)
        {
            base.OnOpened(e);

            await System.Threading.Tasks.Task.Delay(250);

            System.Threading.Thread thr = new System.Threading.Thread(async () =>
            {
                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await Modules.LoadInstalledModules(true);
                    if (OnModulesLoaded != null)
                    {
                        await OnModulesLoaded();
                    }
                });

                if (!this.openFileAssociations)
                {
                    CheckForUpdates();

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        MainWindow mainWindow = new MainWindow();

                        bool deleteFiles = System.Convert.ToBoolean(filesToOpen[0]);

                        for (int i = 1; i < filesToOpen.Length; i++)
                        {
                            _ = mainWindow.LoadFile(filesToOpen[i], deleteFiles);
                        }
                        mainWindow.Show();

                        ProgramStartHandle.Set();

                        this.Close();
                    });
                }
                else
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        FileAssociationWindow window = new FileAssociationWindow(filesToOpen);
                        window.Show();

                        this.Close();
                    });

                }
            });

            thr.Start();
        }

        private void CheckForUpdates()
        {
            Thread thr = new Thread(async () =>
            {
                try
                {
                    if (GlobalSettings.Settings.UpdateCheckDate < DateTime.Today.Ticks && (GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramOnly || GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndInstalledModules || GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndAllModules))
                    {
                        ReleaseHeader programUpdate = null;

                        string releaseJson;

                        using (WebClient client = new WebClient())
                        {
                            client.Headers.Add("User-Agent", "arklumpus/TreeViewer");

                            releaseJson = await client.DownloadStringTaskAsync("https://api.github.com/repos/" + GlobalSettings.ProgramRepository + "/releases");
                        }

                        ReleaseHeader[] releases = System.Text.Json.JsonSerializer.Deserialize<ReleaseHeader[]>(releaseJson);

                        Version currVers = new Version(Program.Version);

                        for (int i = 0; i < releases.Length; i++)
                        {
                            try
                            {
                                if (!releases[i].prerelease)
                                {
                                    Version version = new Version(releases[i].tag_name.Substring(1));

                                    if (version > currVers)
                                    {
                                        programUpdate = releases[i];
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }

                        if (programUpdate != null)
                        {
                            ProgramStartHandle.WaitOne();

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                            {
                                MessageBox box = new MessageBox("Check for updates", "A new version of TreeViewer has been released: " + programUpdate.name + "!\nYou should keep the program updated to use the latest features and avoid security issues. Do you wish to open the download page?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                                await box.ShowDialog(GlobalSettings.Settings.MainWindows[0]);
                                if (box.Result == MessageBox.Results.Yes)
                                {
                                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                                    {
                                        FileName = programUpdate.html_url,
                                        UseShellExecute = true
                                    });
                                }
                            });
                        }
                        else if (GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndInstalledModules || GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndAllModules)
                        {
                            Uri moduleHeaderInfo = new Uri(new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri), "modules.json.gz");

                            List<ModuleHeader> moduleHeaders;

                            using (WebClient client = new WebClient())
                            {
                                string tempFile = Path.GetTempFileName();
                                await client.DownloadFileTaskAsync(moduleHeaderInfo, tempFile);

                                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                                {
                                    using (GZipStream decompressionStream = new GZipStream(fs, CompressionMode.Decompress))
                                    {
                                        moduleHeaders = await System.Text.Json.JsonSerializer.DeserializeAsync<List<ModuleHeader>>(decompressionStream, Modules.DefaultSerializationOptions);
                                    }
                                }

                                File.Delete(tempFile);
                            }

                            bool newModules = false;
                            bool updatedModules = false;

                            for (int i = 0; i < moduleHeaders.Count; i++)
                            {
                                ModuleHeader header = moduleHeaders[i];

                                bool newModule = !Modules.LoadedModulesMetadata.TryGetValue(header.Id, out ModuleMetadata loadedModuleMetadata);

                                if (newModule)
                                {
                                    newModules = true;
                                }
                                else if (loadedModuleMetadata.Version < header.Version)
                                {
                                    updatedModules = true;
                                }
                            }

                            if ((newModules && GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndAllModules) || updatedModules)
                            {
                                string message = null;

                                if (updatedModules)
                                {
                                    message = "Updates are available for some of the installed modules!";
                                }

                                if (newModules && GlobalSettings.Settings.UpdateCheckMode == GlobalSettings.UpdateCheckModes.ProgramAndAllModules)
                                {
                                    if (!string.IsNullOrEmpty(message))
                                    {
                                        message += "\n";
                                    }
                                    message += "New modules have been released. Check them out!";
                                }

                                if (!string.IsNullOrEmpty(message))
                                {
                                    message += "\nDo you wish to open the Module Repository?";

                                    ProgramStartHandle.WaitOne();

                                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        MessageBox box = new MessageBox("Check for updates", message, MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                                        await box.ShowDialog(GlobalSettings.Settings.MainWindows[0]);
                                        if (box.Result == MessageBox.Results.Yes)
                                        {
                                            ModuleManagerWindow win2 = new ModuleManagerWindow();
                                            _ = win2.ShowDialog2(GlobalSettings.Settings.MainWindows[0]);

                                            try
                                            {
                                                ModuleRepositoryWindow win = new ModuleRepositoryWindow(updatedModules ? ModuleRepositoryWindow.Modes.Install : ModuleRepositoryWindow.Modes.Load, win2);
                                                await win.ShowDialog(win2);
                                            }
                                            catch (Exception ex)
                                            {
                                                await new MessageBox("Attention!", "An error occurred while looking up the module repository! Please check the address of the module repository.\n" + ex.Message).ShowDialog2(win2);
                                            }
                                        }
                                    });
                                }
                            }
                        }
                        GlobalSettings.Settings.UpdateCheckDate = DateTime.Today.Ticks;
                        GlobalSettings.SaveSettings();
                    }
                }
                catch { }
            });
            thr.Start();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
