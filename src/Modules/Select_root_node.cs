using System.Threading.Tasks;
using TreeViewer;
using System;

namespace ac0459ddc8d6b4614bc8fc824d63959f6
{
	/// <summary>
	/// This module is used to select the root node of the tree.
	/// </summary>
    public static class MyModule
    {
        public const string Name = "Select root node";
        public const string HelpText = "Selects the root node of the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "c0459ddc-8d6b-4614-bc8f-c824d63959f6";

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.A;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;

        public static bool TriggerInTextBox { get; } = false;

        public static string ItemText { get; } = "Select root node";

        public static string ParentMenu { get; } = "Edit";

        public static string GroupId { get; } = "bf8d109a-5f52-4d26-ae3d-28b289bcb3f4";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static bool IsEnabled(MainWindow window)
        {
            return window.IsTreeOpened;
        }

        public static Task PerformAction(MainWindow window)
        {
            window.SetSelection(window.TransformedTree);
            return Task.CompletedTask;
        }
    }
}