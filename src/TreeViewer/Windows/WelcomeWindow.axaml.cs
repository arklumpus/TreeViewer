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
using System.Linq;
using System.Net;

namespace TreeViewer
{
    public class WelcomeWindow : Window
    {
        public WelcomeWindow()
        {
            InitializeComponent();
        }

        private static readonly string[] RecommendedModules = new string[] { "32914d41-b182-461e-b7c6-5f0263cc1ccd", "68e25ec6-5911-4741-8547-317597e1b792", "e56b8297-4417-4494-9369-cbe9e5d25397", "95b61284-b870-48b9-b51c-3276f7d89df1", "a99eb0c6-a69d-4785-961a-a0c247e9704d", "92aac276-3af7-4506-a263-7220e0df5797", "1f3e0b88-c42d-417c-ba14-ba228be086a7", "8de06406-68e4-4bd8-97eb-2185a0dd1127", "afb64d72-971d-4780-8dbb-a7d9248da30b" };

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<Button>("OKButton").Click += async (s, e) =>
            {
                if (this.FindControl<RadioButton>("AllModulesRadio").IsChecked == true || this.FindControl<RadioButton>("RequiredModulesRadio").IsChecked == true)
                {
                    ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Accessing module database..." };
                    _ = progressWindow.ShowDialog(this);

                    try
                    {
                        Uri moduleHeaderInfo = new Uri(new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri), "modules.json.gz");

                        List<ModuleHeader> moduleHeaders;

                        Directory.CreateDirectory(Modules.ModulePath);
                        Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "assets"));
                        Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "libraries"));
                        File.WriteAllText(Modules.ModuleListPath, "[]");
                        await Modules.LoadInstalledModules(true);

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

                        progressWindow.IsIndeterminate = false;
                        progressWindow.ProgressText = "Downloading and installing modules...";
                        progressWindow.Progress = 0;


                        for (int i = 0; i < moduleHeaders.Count; i++)
                        {
                            ModuleHeader header = moduleHeaders[i];

                            if (this.FindControl<RadioButton>("AllModulesRadio").IsChecked == true || RecommendedModules.Contains(header.Id))
                            {

                                Uri moduleFile = new Uri(new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri), header.Id + "/" + header.Id + ".v" + header.Version.ToString() + ".json.zip");

                                using (WebClient client = new WebClient())
                                {
                                    string tempFile = Path.GetTempFileName();
                                    await client.DownloadFileTaskAsync(moduleFile, tempFile);

                                    try
                                    {
                                        ModuleMetadata.Install(tempFile, false, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox message = new MessageBox("Attention", "An error occurred while installing module " + header.Name + "!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK);
                                        await message.ShowDialog(this);
                                    }

                                    File.Delete(tempFile);
                                }
                            }

                            progressWindow.Progress = (double)(i + 1) / moduleHeaders.Count;
                        }

                        progressWindow.Close();

                        if (!Modules.IsMac)
                        {
                            if (Modules.FileTypeModules.Count > 0)
                            {
                                FileAssociationWindow fileAssociationWindow = new FileAssociationWindow(new string[] { "False" });
                                fileAssociationWindow.Show();
                            }
                            else
                            {
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                            }
                        }
                        else
                        {
                            MacOSPermissionWindow window = new MacOSPermissionWindow();
                            window.Show();
                        }

                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        progressWindow.Close();

                        MessageBox message = new MessageBox("Attention", "An error occurred while accessing the module database!!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK);
                        await message.ShowDialog(this);
                    }
                }
                else if (this.FindControl<RadioButton>("NoModulesRadio").IsChecked == true)
                {
                    MessageBox message = new MessageBox("Attention", "Please make sure to install at least the required modules using the Module manager window before using the program!\nAre you sure you want to proceed with this route?", MessageBox.MessageBoxButtonTypes.YesNo);
                    await message.ShowDialog(this);
                    if (message.Result == MessageBox.Results.Yes)
                    {
                        Directory.CreateDirectory(Modules.ModulePath);
                        Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "assets"));
                        Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "libraries"));
                        File.WriteAllText(Modules.ModuleListPath, "[]");
                        await Modules.LoadInstalledModules(true);

                        if (!Modules.IsMac)
                        {
                            if (Modules.FileTypeModules.Count > 0)
                            {
                                FileAssociationWindow fileAssociationWindow = new FileAssociationWindow(new string[] { "False" });
                                fileAssociationWindow.Show();
                            }
                            else
                            {
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                            }
                        }
                        else
                        {
                            MacOSPermissionWindow window = new MacOSPermissionWindow();
                            window.Show();
                        }

                        this.Close();
                    }
                }
            };
        }
    }
}
