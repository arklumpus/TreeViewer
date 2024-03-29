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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace TreeViewer
{
    public class FileAssociationWindow : ChildWindow
    {
        public FileAssociationWindow()
        {
            InitializeComponent();
        }

        private string[] filesToOpen;

        public FileAssociationWindow(string[] filesToOpen)
        {
            InitializeComponent();
            this.filesToOpen = filesToOpen;
        }

        private bool IsAdmin()
        {
            if (Modules.IsWindows)
            {
#pragma warning disable CA1416 // Platform validation
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
#pragma warning restore CA1416 // Platform validation
            }
            else
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.FileTypeIcons.tre")) { Width = 32, Height = 32 });

            if (Modules.IsWindows)
            {
                if (IsAdmin())
                {
                    this.FindControl<Button>("AdminButton").IsVisible = false;
                    this.FindControl<Button>("OKButton").IsEnabled = true;
                }
                else
                {
                    this.FindControl<Button>("AdminButton").IsVisible = true;
                    this.FindControl<Button>("OKButton").IsEnabled = false;

                    this.FindControl<Button>("AdminButton").Click += (s, e) =>
                    {
                        List<string> args = new List<string>(filesToOpen.Skip(1));

                        if (Convert.ToBoolean(filesToOpen[0]))
                        {
                            args.Insert(0, "--delete");
                        }

                        Privileges.Elevate(new List<string>() { "--file-associations" }, args);
                    };
                }
            }
            else
            {
                this.FindControl<Button>("AdminButton").IsVisible = false;
                this.FindControl<Button>("OKButton").IsEnabled = true;
            }

            Dictionary<string, string> fileExtensions = new Dictionary<string, string>()
            {

            };

            foreach (FileTypeModule module in Modules.FileTypeModules)
            {
                string description = module.Extensions[0].Replace("files", "file");

                for (int i = 1; i < module.Extensions.Length; i++)
                {
                    fileExtensions.TryAdd(module.Extensions[i], description);
                }
            }

            List<KeyValuePair<string, string>> sortedFileExtensions = (from el in fileExtensions orderby el.Key ascending select el).ToList();

            Dictionary<string, CheckBox> extensionCheckboxes = new Dictionary<string, CheckBox>();

            foreach (KeyValuePair<string, string> kvp in sortedFileExtensions)
            {
                StackPanel description = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };

                description.Children.Add(new TextBlock() { Text = "." + kvp.Key, FontWeight = Avalonia.Media.FontWeight.Bold });
                description.Children.Add(new TextBlock() { Text = " - " + kvp.Value, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(114, 114, 114)) });

                CheckBox checkBox = new CheckBox() { Content = description, IsChecked = true, Margin = new Thickness(0, 0, 0, 0), FontSize = 13 };

                extensionCheckboxes.Add(kvp.Key, checkBox);

                this.FindControl<StackPanel>("ExtensionsContainer").Children.Add(checkBox);
            }

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                if (Program.WaitingForRebootAfterAdmin)
                {
                    if (!Modules.IsMac)
                    {
                        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                    }
                    else
                    {
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();

                    bool deleteFiles = System.Convert.ToBoolean(filesToOpen[0]);

                    for (int i = 1; i < filesToOpen.Length; i++)
                    {
                        string file = filesToOpen[i];

                        mainWindow.Opened += async (s, e) =>
                        {
                            await mainWindow.LoadFile(file, deleteFiles);
                        };
                    }
                    mainWindow.Show();

                    this.Close();
                }
            };

            this.FindControl<Button>("OKButton").Click += async (s, e) =>
            {
                List<(string, string)> extensionsToAssociate = new List<(string, string)>();

                foreach (KeyValuePair<string, CheckBox> kvp in extensionCheckboxes)
                {
                    if (kvp.Value.IsChecked == true)
                    {
                        extensionsToAssociate.Add((kvp.Key, fileExtensions[kvp.Key]));
                    }
                }

                try
                {
                    FileExtensions.AssociateExtensions(extensionsToAssociate);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Attention!", "An error occurred while associating the file extensions!\n" + ex.Message).ShowDialog2(this);
                }

                if (Program.WaitingForRebootAfterAdmin)
                {
                    if (!Modules.IsMac)
                    {
                        ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
                    }
                    else
                    {
                        System.Diagnostics.Process.GetCurrentProcess().Kill();
                    }
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();

                    bool deleteFiles = System.Convert.ToBoolean(filesToOpen[0]);

                    for (int i = 1; i < filesToOpen.Length; i++)
                    {
                        string file = filesToOpen[i];

                        mainWindow.Opened += async (s, e) =>
                        {
                            await mainWindow.LoadFile(file, deleteFiles);
                        };
                    }
                    mainWindow.Show();

                    this.Close();
                }
            };
        }
    }
}
