using System.Threading.Tasks;
using TreeViewer;
using System;

namespace ColorPickerMenuAction
{
    /// <summary>
    /// This module adds a menu option to open a colour picker dialog window. If this window is opened while a text box is selected,
    /// upon clicking `OK` the contents of the text box are replaced with an hexadecimal representation of the chosen colour. The
    /// shortcut to open this window works even if a text box is focused.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Color picker";
        public const string HelpText = "Opens a color picker window.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "5c99fbfb-a6c6-4e07-915d-670b07d255c8";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Color picker...";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupId { get; } = "99b57514-901b-4b88-8731-9206eb627a0f";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.C;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift;
        public static bool TriggerInTextBox { get; } = true;

        public static bool IsEnabled(MainWindow window)
        {
            return true;
        }

        public static async Task PerformAction(MainWindow window)
        {
            if (Avalonia.Input.KeyboardDevice.Instance.FocusedElement is Avalonia.Controls.TextBox box && box.IsEffectivelyVisible)
            {
                AvaloniaColorPicker.ColorPickerWindow win = new AvaloniaColorPicker.ColorPickerWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize };
                Avalonia.Media.Color? col = await win.ShowDialog(window);

                if (col != null)
                {
                    VectSharp.Colour colVectSharp = col.Value.ToVectSharp();
                    box.Text = colVectSharp.ToCSSString(colVectSharp.A != 1);
                }
            }
            else
            {
                Avalonia.Controls.Window win = new Avalonia.Controls.Window() { FontFamily = window.FontFamily, FontSize = window.FontSize, Title = "Color picker", SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight };
                win.Content = new AvaloniaColorPicker.ColorPicker() { FontFamily = window.FontFamily };
                await win.ShowDialog(window);
            }
        }
    }
}
