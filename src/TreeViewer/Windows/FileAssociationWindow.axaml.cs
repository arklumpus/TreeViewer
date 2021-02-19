using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;

namespace TreeViewer
{
    public class FileAssociationWindow : Window
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
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
            else
            {
                return false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
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
                description.Children.Add(new TextBlock() { Text = " - " + kvp.Value, FontStyle = Avalonia.Media.FontStyle.Italic });

                CheckBox checkBox = new CheckBox() { Content = description, IsChecked = true, Margin = new Thickness(0, 5, 0, 0) };

                extensionCheckboxes.Add(kvp.Key, checkBox);

                this.FindControl<StackPanel>("ExtensionsContainer").Children.Add(checkBox);
            }

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                if (Program.WaitingForRebootAfterAdmin)
                {
                    ((IControlledApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();

                    bool deleteFiles = System.Convert.ToBoolean(filesToOpen[0]);

                    for (int i = 1; i < filesToOpen.Length; i++)
                    {
                        _ = mainWindow.LoadFile(filesToOpen[i], deleteFiles);
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
                    await new MessageBox("Attention!", "An error occurred while associating the file extensions!\n" + ex.Message).ShowDialog(this);
                }

                if (Program.WaitingForRebootAfterAdmin)
                {
                    ((IControlledApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown();
                }
                else
                {
                    MainWindow mainWindow = new MainWindow();

                    bool deleteFiles = System.Convert.ToBoolean(filesToOpen[0]);

                    for (int i = 1; i < filesToOpen.Length; i++)
                    {
                        _ = mainWindow.LoadFile(filesToOpen[i], deleteFiles);
                    }
                    mainWindow.Show();

                    this.Close();
                }
            };
        }
    }
}
