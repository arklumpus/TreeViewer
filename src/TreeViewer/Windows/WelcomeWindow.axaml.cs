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
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class WelcomeWindow : ChildWindow
    {
        public WelcomeWindow()
        {
            InitializeComponent(false, null);
        }

        public WelcomeWindow(bool consentOnly, string[] filesToOpen)
        {
            InitializeComponent(consentOnly, filesToOpen);
        }

        private void InitializeComponent(bool consentOnly, string[] filesToOpen)
        {
            AvaloniaXamlLoader.Load(this);

            try
            {
                string localDownloadFile = null;

                if (Modules.IsWindows)
                {
                    localDownloadFile = Path.Combine(Path.GetTempPath(), "TreeViewer-Win-x64.msi");
                }
                else if (Modules.IsLinux)
                {
                    localDownloadFile = Path.Combine(Path.GetTempPath(), "TreeViewer-Linux-x64.run");
                }
                else if (Modules.IsMac)
                {
                    localDownloadFile = Path.Combine(Path.GetTempPath(), "TreeViewer-Mac-x64.pkg");
                }

                if (File.Exists(localDownloadFile))
                {
                    File.Delete(localDownloadFile);
                }
            }
            catch { }

            if (consentOnly)
            {
                this.FindControl<TextBlock>("Preamble1").IsVisible = false;
                this.FindControl<TextBlock>("Preamble2").IsVisible = false;
                this.FindControl<TextBlock>("Preamble3").IsVisible = false;
                this.FindControl<StackPanel>("Preamble4").IsVisible = false;
            }

            this.FindControl<TextBlock>("GitHubPrivacyLink").PointerPressed += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "https://docs.github.com/en/site-policy/privacy-policies/github-privacy-statement",
                    UseShellExecute = true
                });
            };

            this.FindControl<TextBlock>("GoogleFontsPrivacyLink").PointerPressed += (s, e) =>
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                {
                    FileName = "https://policies.google.com/privacy",
                    UseShellExecute = true
                });
            };

            this.FindControl<CheckBox>("ConsentBox").PropertyChanged += ConsentBoxPropertyChanged;
            this.FindControl<CheckBox>("GoogleFontsBox").PropertyChanged += ConsentBoxPropertyChanged;
            this.FindControl<CheckBox>("GoogleFontsConsentBox").PropertyChanged += ConsentBoxPropertyChanged;

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Close();
            };

            this.FindControl<Button>("OKButton").Click += async (s, e) =>
            {
                switch (this.FindControl<ComboBox>("UpdatesBox").SelectedIndex)
                {
                    case 0:
                        GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramAndAllModules;
                        break;
                    case 1:
                        GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramAndInstalledModules;
                        break;
                    case 2:
                        GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramOnly;
                        break;
                    case 3:
                        GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.DontCheck;
                        break;
                }

                if (this.FindControl<CheckBox>("GoogleFontsBox").IsChecked == true && this.FindControl<CheckBox>("GoogleFontsConsentBox").IsChecked == true)
                {
                    GlobalSettings.Settings.AllowGoogleFonts = true;
                }
                else
                {
                    GlobalSettings.Settings.AllowGoogleFonts = false;
                }

                if (this.FindControl<CheckBox>("ConsentBox").IsChecked == true)
                {
                    GlobalSettings.Settings.PrivacyConsent = true;
                }

                GlobalSettings.SaveSettings();

                if (!consentOnly)
                {
                    if (this.FindControl<RadioButton>("AllModulesRadio").IsChecked == true)
                    {
                        ProgressWindow progressWindow = new ProgressWindow() { ProgressText = "Accessing module database..." };
                        _ = progressWindow.ShowDialog2(this);

                        try
                        {
                            Uri moduleHeaderInfo = new Uri(new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri), "modules.json.gz");

                            List<ModuleHeader> moduleHeaders;

                            Directory.CreateDirectory(Modules.ModulePath);
                            Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "assets"));
                            Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "libraries"));
                            File.WriteAllText(Modules.ModuleListPath, "[]");
                            await Modules.LoadInstalledModules(true, null);

                            {
                                string tempFile = Path.GetTempFileName();
                                await Modules.HttpClient.DownloadFileTaskAsync(moduleHeaderInfo, tempFile);

                                using (FileStream fs = new FileStream(tempFile, FileMode.Open))
                                {
                                    using (GZipStream decompressionStream = new GZipStream(fs, CompressionMode.Decompress))
                                    {
                                        moduleHeaders = await System.Text.Json.JsonSerializer.DeserializeAsync<List<ModuleHeader>>(decompressionStream, Modules.DefaultSerializationOptions);
                                    }
                                }

                                File.Delete(tempFile);
                            }

                            SemaphoreSlim semaphore = new SemaphoreSlim(0, 1);

                            progressWindow.IsIndeterminate = false;
                            progressWindow.ProgressText = "Downloading and installing modules...";
                            progressWindow.Progress = 0;

                            Thread thr = new Thread(async () =>
                            {
                                for (int i = 0; i < moduleHeaders.Count; i++)
                                {
                                    ModuleHeader header = moduleHeaders[i];
                                    Uri moduleFile = new Uri(new Uri(GlobalSettings.Settings.ModuleRepositoryBaseUri), header.Id + "/" + header.Id + ".v" + header.Version.ToString() + ".json.zip");

                                    {
                                        string tempFile = Path.GetTempFileName();
                                        await Modules.HttpClient.DownloadFileTaskAsync(moduleFile, tempFile);

                                        try
                                        {
                                            ModuleMetadata.Install(tempFile, false, true);
                                        }
                                        catch (Exception ex)
                                        {
                                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                                            {
                                                MessageBox message = new MessageBox("Attention", "An error occurred while installing module " + header.Name + "!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK);
                                                await message.ShowDialog2(this);
                                            });
                                        }

                                        File.Delete(tempFile);
                                    }

                                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWindow.Progress = (double)(i + 1) / moduleHeaders.Count; });
                                }

                                semaphore.Release();
                            });

                            thr.Start();

                            await semaphore.WaitAsync();

                            progressWindow.Close();


                            if (!Modules.IsMac && Modules.FileTypeModules.Count > 0)
                            {
                                FileAssociationWindow fileAssociationWindow = new FileAssociationWindow(new string[] { "False" });
                                fileAssociationWindow.Show();
                            }
                            else
                            {
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                            }

                            this.Close();
                        }
                        catch (Exception ex)
                        {
                            progressWindow.Close();

                            MessageBox message = new MessageBox("Attention", "An error occurred while accessing the module database!!\n" + ex.Message, MessageBox.MessageBoxButtonTypes.OK);
                            await message.ShowDialog2(this);
                        }
                    }
                    else if (this.FindControl<RadioButton>("NoModulesRadio").IsChecked == true)
                    {
                        MessageBox message = new MessageBox("Attention", "Please make sure to install the required modules using the Module manager window before using the program!\nYou can open again this window by starting TreeViewer with the \"--welcome\" command-line option.\nAre you sure you want to proceed with this route?", MessageBox.MessageBoxButtonTypes.YesNo);
                        await message.ShowDialog2(this);
                        if (message.Result == MessageBox.Results.Yes)
                        {
                            Directory.CreateDirectory(Modules.ModulePath);
                            Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "assets"));
                            Directory.CreateDirectory(Path.Combine(Modules.ModulePath, "libraries"));
                            File.WriteAllText(Modules.ModuleListPath, "[]");
                            await Modules.LoadInstalledModules(true, null);

                            if (!Modules.IsMac && Modules.FileTypeModules.Count > 0)
                            {
                                FileAssociationWindow fileAssociationWindow = new FileAssociationWindow(new string[] { "False" });
                                fileAssociationWindow.Show();
                            }
                            else
                            {
                                MainWindow mainWindow = new MainWindow();
                                mainWindow.Show();
                            }

                            this.Close();
                        }
                    }
                }
                else
                {
                    SplashScreen splashScreen = new SplashScreen(filesToOpen);
                    splashScreen.Show();

                    this.Close();
                }
            };
        }


        private void ConsentBoxPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == CheckBox.IsCheckedProperty)
            {
                this.FindControl<Button>("OKButton").IsEnabled = this.FindControl<CheckBox>("ConsentBox").IsChecked == true && (this.FindControl<CheckBox>("GoogleFontsBox").IsChecked == false || this.FindControl<CheckBox>("GoogleFontsConsentBox").IsChecked == true);

                if (this.FindControl<CheckBox>("GoogleFontsBox").IsChecked == true)
                {
                    this.FindControl<CheckBox>("GoogleFontsConsentBox").IsEnabled = true;
                }
                else
                {
                    this.FindControl<CheckBox>("GoogleFontsConsentBox").IsEnabled = false;
                    this.FindControl<CheckBox>("GoogleFontsConsentBox").IsChecked = false;
                }
            }
        }
    }
}
