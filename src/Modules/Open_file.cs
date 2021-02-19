using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeViewer;
using System;

namespace OpenFileMenuAction
{
    /// <summary>
    /// This module shows an "Open file" dialog that is used to select tree files to open with TreeViewer.
    /// 
    /// Once a tree file is selected, the installed File type and Load file modules are queried to determine
    /// how appropriate they are to opening the file. The most appropriate modules are then used to read the
    /// tree file.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Open file";
        public const string HelpText = "Opens a tree file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "078318bc-907f-4ada-b1e5-171799957b2a";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Open...";
        public static string ParentMenu { get; } = "File";
        public static string GroupId { get; } = "48171c7a-a759-4d04-84b0-546d82b36a19";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.O;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
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
                await window.LoadFile(result[0], false);
            }
        }
    }
}
