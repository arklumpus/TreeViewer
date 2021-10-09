using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

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
        public static Version Version = new Version("1.0.1");
        public const string Id = "5c99fbfb-a6c6-4e07-915d-670b07d255c8";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Color picker";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "Color picker";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.C, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift) };
        public static bool TriggerInTextBox { get; } = true;

        public static double GroupIndex { get; } = 3;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon32Base64 = "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";
        private static string Icon48Base64 = "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";
        private static string Icon64Base64 = "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";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon32Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon48Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon64Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true };
        }

        public static async Task PerformAction(int index, MainWindow window)
        {
            if (Avalonia.Input.KeyboardDevice.Instance.FocusedElement is Avalonia.Controls.TextBox box && box.IsEffectivelyVisible)
            {
                ColourPickerWindow win = new ColourPickerWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Icon = window.Icon };
                Avalonia.Media.Color? col = await win.ShowDialog(window);

                if (col != null)
                {
                    VectSharp.Colour colVectSharp = col.Value.ToVectSharp();
                    box.Text = colVectSharp.ToCSSString(colVectSharp.A != 1);
                }
            }
            else
            {
                ChildWindow win = new ChildWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Title = "Color picker", SizeToContent = Avalonia.Controls.SizeToContent.WidthAndHeight, Icon = window.Icon };
                win.Content = new AvaloniaColorPicker.ColorPicker() { FontFamily = window.FontFamily };
                await win.ShowDialog(window);
            }
        }
    }
}