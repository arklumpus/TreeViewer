using System.Threading.Tasks;
using TreeViewer;
using System;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace @NamespaceHere
{
    // Do not change class name.
    public static class MyModule
    {
        public const string Name = "A name for your module.";
        public const string HelpText = "A very short description for your module.";
        public const string Author = "Your name";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // The following two properties determine the shortcut keys that can be used to perform the action without
        // having to click on the menu item.
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;

        // This property determines whether the shortcut keys should trigger this module's action even if a text box
        // is focused when they are pressed (e.g. you would not want to hook to the CTRL+A combination if a text box
        // is focused, because that is used to select all text in the text box).
        public static bool TriggerInTextBox { get; } = false;

        // The text of the menu item that triggers this action.
        public static string ItemText { get; } = "Item text";

        // The name of the parent menu item in which the item should be placed (e.g. "File", "Edit", or "Help"). If
        // the parent menu does not exist, it will be created.
        public static string ParentMenu { get; } = "Edit";

        // The Id of the group within the parent menu in which the item will be placed. Items with the same value 
        // for the following property will be placed in the same group. Different groups within the same menu will
        // be separated by separators. You can use either a human-readable string, or something more likely to be
        // unique like a Guid.
        public static string GroupId { get; } = "@GuidHere";

        // An AvaloniaProperty on the MainWindow to which the menu item belongs, which affects whether the menu item
        // action is available or not. Set to null if not applicable.
        // E.g.
        //      MainWindow.IsSelectionAvailableProperty
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;

        // This method returns true if the action is available, or false if it is not available. This method is called
        // every time the property of the MainWindow specified by the PropertyAffectingEnabled changes.
        //
        // window: the MainWindow that contains the plot.
        //
        public static bool IsEnabled(MainWindow window)
        {
            return window.IsSelectionAvailable;
        }

        // This method performs the actual action. This method returns a Task, so that it can be marked as async and
        // awaited. Return Task.CompletedTask if you do not need to run any async code.
        //
        // window: the MainWindow that contains the plot.
        //
        public static Task PerformAction(MainWindow window)
        {
            // TODO: do something.

            return Task.CompletedTask;
        }
    }
}
