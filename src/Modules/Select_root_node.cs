using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;


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

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.A, Avalonia.Input.KeyModifiers.Control ) };

        public static bool TriggerInTextBox { get; } = false;

        public static string ItemText { get; } = "Select root";

        public static string ParentMenu { get; } = "Edit";

        public static string GroupName { get; } = "Clipboard";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static double GroupIndex { get; } = 6;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACLSURBVDhPY/Sq3fifAQi2NfszgmgYHwSIEkOWJBVQohcOwM7BBYqKihqAVD2EhwB9fX1wfUwEnIGhGRmA9LJA2XgBso3ogAlKkw1QTAb6Gat3aOoCvHEJchEuV4EASC/FLiAqFtBdgZIOYGkbGfz//x+MgaARLIADYNMLBkgGEAaUZAiQXgqzMwMDAL4iUxi3a9FOAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADHSURBVEhL3ZZBDsIgEEWnxtO58S7sTSXuuYsbr1cHwhCYFuSnxRhf0kIhmWE+n7bT5fa8E9HMV+D1uE6xSzy3xG4x7qnN5eOMPfEtBR/A7CsIGfUK9yJxfQVDgVZtjCn2awvnXBETrQDer/ESIZvMEiULaik0EvccnjJ6dEbYkujQc7EqM5ehxSeJhGYCHaQn+V6bwqAJbGy7WdkUsWKLr72Lxp/k2CZ6bVrjJ1wEO6XFH3zRJJNQ+0PQFdbmdDxfwaGal5B9A1IOWckSQA1AAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEHSURBVFhH7ZdJEsIgEEVDjqcLD5O1U7kmd9GF10Mb6YihoYEIqOWrSpGB5JPfQypitT2rbsb1tBFmV1NyTm/GZkwOzFdbGtRt7kD7EJgxC6UeOTUMw+E+7PVBgHEcHb13OcCK+/iZEEz1TdkcoodyoBpEaVA32AdSkwyIdQB1Y3MgO8k4vq8RgcW4SSn1Zi5l8XIzxFyI5ykuy6nrIaj88DqADy+NswAQzhA/mjEZxxIQD1lK2ciBL2SHF6nSiEB4Lo66n1eGsFK7xJaWGYfXASpeJWgeguBrYvYu+dxyeB9m94LYTseR1AlrEZuE2Z2O4/9jUnUB1Eeu2t8xiK93F2dO4xB03Q3gaaI4W9HH6gAAAABJRU5ErkJggg==";

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

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { window.IsTreeOpened };
        }

        public static Task PerformAction(int index, MainWindow window)
        {
            window.SetSelection(window.TransformedTree);
            return Task.CompletedTask;
        }
    }
}