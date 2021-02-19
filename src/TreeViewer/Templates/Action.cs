using System;
using TreeViewer;
using VectSharp;

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
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // This property determines whether the action defined by this module can be performed when the program is run
        // in command-line mode or not.
        public static bool IsAvailableInCommandLine { get; } = false;

        // The following two properties determine the shortcut keys that can be used to perform the action without
        // having to click on the button.
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;

        // This property determines whether the shortcut keys should trigger this module's action even if a text box
        // is focused when they are pressed (e.g. you would not want to hook to the CTRL+A combination if a text box
        // is focused, because that is used to select all text in the text box.
        public static bool TriggerInTextBox { get; } = false;

        // A short label that will appear on the button for the action. The label can span two lines (use "\n" to enter
        // a line break, but the text it contains must be very short, in order not to overflow the button's client area.
        public static string ButtonText { get; } = "Short\ntext";

        // This method returns the icon for the button, as a Page object. The maximum available space is 75x42
        // device-independent pixels; Pages larger than this will be scaled uniformly to fit in this area.
        public static Page GetIcon()
        {
            Page page = new Page(75, 42);

            // TODO: draw something on the page's Graphics.
            // E.g.
            //      page.Graphics.FillRectangle(10, 10, 55, 22, Colours.Red);

            return page;
        }

        // This method performs the actual action.
        //
        // window: the MainWindow that contains the plot. If the program is running in command-line mode and this
        //         module has signalled that it is available in command-line mode, this may be null.
        //
        // stateData: an InstanceStateData object that can be used to access features in way that does not depend
        //            on the program running in command-line or GUI mode.
        public static void PerformAction(MainWindow window, InstanceStateData stateData)
        {
            // TODO: do something.
        }
    }
}
