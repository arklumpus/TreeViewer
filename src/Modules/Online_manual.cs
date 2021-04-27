using System.Threading.Tasks;
using TreeViewer;
using System;

namespace abc943abb66d94425be786ff7fb4148f0
{
    /// <summary>
    /// This module opens the system default web browser at the address of the home page of the TreeViewer
    /// manual.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Online manual";
        public const string HelpText = "Opens a web browser window at the TreeViewer manual homepage.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "bc943abb-66d9-4425-be78-6ff7fb4148f0";

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.F1;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;

        public static bool TriggerInTextBox { get; } = true;

        public static string ItemText { get; } = "Online manual";

        public static string ParentMenu { get; } = "Help";

        public static string GroupId { get; } = "bc943abb-66d9-4425-be78-6ff7fb4148f0";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;

        public static bool IsEnabled(MainWindow window)
        {
            return true;
        }

        public static Task PerformAction(MainWindow window)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "https://github.com/arklumpus/TreeViewer/wiki",
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }
    }
}
