using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

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
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "bc943abb-66d9-4425-be78-6ff7fb4148f0";

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.F1, Avalonia.Input.KeyModifiers.None ) };

        public static bool TriggerInTextBox { get; } = true;

        public static string ItemText { get; } = "Online manual";

        public static string ParentMenu { get; } = "Help";

        public static string GroupName { get; } = "Help";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;

        public static double GroupIndex { get; } = 0;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJiSURBVFhHtZe/TtxAEMb3Ih6BF6AKukN5ARpMBw3UiAqd0t9VKRIFCQqqux6dqBA1NNBhmjxAotwJmuQByDtc/C3zHeNhvbbP5ietPLPe9czOzv5xxwnT6XQuYiG9Xm/R3tK0v/9AGWhjDbGuDNuPdAaDgX/R7/ddt9v1lWXMZjORXK0+k8nEy+PxeBGJD/KsBY1WNR5jRZ6NGQ6HIuUZjUYihQk6cHn/6K7SJ9GcO0g+usPtddHClBkq4s0UWOMA+u63G9Ha5Y0DduQoBM61TWEO2LDDMZSiqWgtB25P9kR6BUbttFhay4EQ7xF6UuqATkqdD20R3Qes8dhSbHUfIFWNg3fNgTLjmk4BOAn1GUCiEagLDO18vd7KxPSlJkdyd7r/YJ2IRgBLMrQsLTCc7ZRJZhwna/p5Z8PXs6/oKd6LgwsqTUEMGfVxJqZnR5sO5fzut38CrUsdHEF7T9QB7P+xM0CMY0Tf8fEvFz98gfxpbdW3wdO+Q3tGYukIqOuVHzkNARj69fefl/GEDkw7nyfRJCybf46CoyUwQKPA6mzv++NKhlLlXqeRD8x//nmWmjkSLKdrUI/3hHrjJLSjtzoparfUpRRIAgZvunXI5YC+7YYIOajzBCsmljf2PfSFA7wyx0CUgHYEWa7Da3XCVUGo18oBOImiIpWEsh0f5x6CAh31svw80i9ZKgkZLezteNrR0RjCbZcgYHv0L/5Xq0iWiP7w0YYg22nR7zh6DqAxmRPHWBFY21zf3A+0ThntpatrHAGSfdRHAjJOPxxAmALkAHUhN/LWHCDaEUMg5M79B1I6D6cM2akPAAAAAElFTkSuQmCC";
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

        public static Task PerformAction(int index, MainWindow window)
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
