using System.Threading.Tasks;
using TreeViewer;
using System;

namespace CopySelectedNodeMenuAction
{
    /// <summary>
    /// This module copies the currently selected node to the clipboard in Newick-with-attributes format. The node can then be pasted
    /// e.g. in a text editor or in another tree viewer progam.
    /// 
    /// **Note**: this module is a shortcut for the _Copy selected node_ Selection action module (id `debd9130-8451-4413-88f0-6357ec817021`)
    /// and requires it to be installed to work.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Copy selected node";
        public const string HelpText = "Copies the selected node to the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "650a0ef5-5322-4511-ae86-68bd87b47ecd";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Copy";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupId { get; } = "bf8d109a-5f52-4d26-ae3d-28b289bcb3f4";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsSelectionAvailableProperty;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.C;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
        public static bool TriggerInTextBox { get; } = false;

        public static bool IsEnabled(MainWindow window)
        {
            return window.IsSelectionAvailable;
        }

        public static Task PerformAction(MainWindow window)
        {
            Modules.GetModule(Modules.SelectionActionModules, "debd9130-8451-4413-88f0-6357ec817021").PerformAction(window.SelectedNode, window, window.StateData);
            return Task.CompletedTask;
        }
    }
}
