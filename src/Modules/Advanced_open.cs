using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeViewer;

namespace AdvancedOpenFileMenuAction
{
    /// <summary>
    /// This module opens the "Advanced file open" dialog, which can be used to manually select which File type and Load file modules
    /// should be used to open the selected file.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Open file (advanced)";
        public const string HelpText = "Opens a tree file, specifying which modules should be used for the reading and loading of the file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "98804064-922f-4395-8a96-216d4b3ff259";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Advanced open...";
        public static string ParentMenu { get; } = "File";
        public static string GroupId { get; } = "48171c7a-a759-4d04-84b0-546d82b36a19";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.O;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift;
        public static bool TriggerInTextBox { get; } = false;

        public static bool IsEnabled(MainWindow window)
        {
            return true;
        }

        public static async Task PerformAction(MainWindow window)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>();

            List<string> allExtensions = new List<string>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                filters.Add(new FileDialogFilter() { Name = Modules.FileTypeModules[i].Extensions[0], Extensions = new List<string>(Modules.FileTypeModules[i].Extensions.Skip(1)) });
                allExtensions.AddRange(Modules.FileTypeModules[i].Extensions.Skip(1));
            }

            filters.Insert(0, new FileDialogFilter() { Name = "All tree files", Extensions = allExtensions });
            filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(window);

            if (result != null && result.Length == 1)
            {
                AdvancedOpenWindow win = new AdvancedOpenWindow(result[0]);
                await win.ShowDialog2(window);

                if (win.LoadedTrees != null)
                {
                    if (window.Trees == null)
                    {
                        window.Trees = win.LoadedTrees;
                        window.FileOpened(win.ModuleSuggestions);
                    }
                    else
                    {
                        MainWindow win2 = new MainWindow(win.LoadedTrees, win.ModuleSuggestions, result[0]);
                        win2.Show();
                    }
                }

            }
        }
    }
}
