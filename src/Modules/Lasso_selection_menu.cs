using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace ab4411c95e6154fa980c1d01c84cf8a54
{
    /// <summary>
    /// This module can be used to perform a "Lasso selection", i.e. to select nodes based on their position in the plot.
    /// 
    /// When you enable this module, a message is shown indicating that lasso selection is active. You can then use the mouse
    /// to draw a polygon on the tree plot (every time you click, a new point is added). When you reach the last vertex of the
    /// polygon, double click to close the shape.
    /// 
    /// At this point, a new window is shown, which lets you choose one of the attributes that are present on the selected tips.
    /// When you click on `OK`, the values of the selected attribute for the nodes that fall within the selected area are copied
    /// to the clipboard and can be pasted into other software (e.g. a text editor).
    /// 
    /// **Note**: this module is a shortcut for the _Lasso selection_ Action module (id `a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6`)
    /// and requires it to be installed to work.
    /// </summary>
    /// 
    public static class MyModule
    {
        public const string Name = "Lasso selection (menu action)";
        public const string HelpText = "Selects tips from the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "b4411c95-e615-4fa9-80c1-d01c84cf8a54";

        public static string ItemText { get; } = "Lasso selection";
        public static string ParentMenu { get; } = "Edit";

        public static string GroupName { get; } = "Clipboard";

        public static double GroupIndex { get; } = 7;

        public static bool IsLargeButton { get; } = false;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.L, Avalonia.Input.KeyModifiers.Control) };

        public static bool TriggerInTextBox { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD1SURBVDhPjZIxDsIwDEVTxGE4Bqws3KU7E3vvwsIaOBQz+P98R3ZaCZ4UJXbtH9tpiczz/LF1lPkXO+3OyVaNIjhDWOaKSXtHydUWxADPy7I8m5lZCYAgAph8vt6T73G7UHBsYWRMRiIudaFWQegxll0seYrJh/eryod4VsEK4ITDFgIZbMuh7SULj20txMFBDGXDL3owQLUuhup8Bp6cJj0GS9wHn4RXhLkwWX0n4Pv1CiT2j3ajeBeAEx9lktEWfAmd03/AngYh2jpv0gUwQCn7cPxF+m3iFEXTR928+SIaYv8fMFjb6h5GYDNZsDJLbBYp5Qt5vpPtgA1bZQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFySURBVEhLtZRNcsIwDIUDJ6PbbrhL9kxgus9d2HRLb5a+JyQj27IxTPvNaBT/6UmKk6nFPM9n2KbD/4ECFLJnmXyRvfoWF9iC4If78HW6Auu6MnuK3GAfnCOsarSiZxWYCP0PvbZsgYng5+l6gG30HJc8FfD44BTUoFbdLRIZFugF//46sjoRgWUkAfZUg2S4uV7wyXxZha/AbowEpKco5zgGYXC3j1RVJAEcZmATscAXzO/g00sGWeZA9vEhqqJ8B9aGnVrVMlC90GJfti7ZEZYKtzDwfeaBViQHuW5tQsay17WIFZ/9uq8gldqArQlboUlJezmOWtXFZZjgYVg1b9h6+Q6GsSxb2PrbAqP8qUDUykqAm2DR9ezSOhNVkD62USHuhQtvYXXnDQ0u1y5Cv4eNd51j7Je7z2dg30Pzlgk8xOxUrCIKwL08w2eu9yqwjCQbmSxQAf9fymgKjAQnCODbQpKYrbUEmFk3eEkgBqbpF2l3AHugVDGTAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "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";


        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
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

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { window.IsTreeOpened };
        }

        public static Task PerformAction(int actionIndex, MainWindow window)
        {
            if (window.IsTreeOpened)
            {
                Modules.GetModule(Modules.ActionModules, "a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6").PerformAction(0, window, window.StateData);
            }

            return Task.CompletedTask;
        }
    }
}
