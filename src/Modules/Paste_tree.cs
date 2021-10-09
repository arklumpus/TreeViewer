using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace PasteTreeAction
{
    /// <summary>
    /// This module can be used to paste a tree from the clipboard.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// When this module is invoked (by clicking on the button, or by using the keyboard shortcut), the text contents of the clipboard
    /// are saved to a temporary file, which is then opened using the installed File type and Load file modules. If the tree is in any
    /// known format (e.g. Newick or NEXUS format), these modules are able to load it. If the text in the clipboard does not represent
    /// a valid tree that can be read from any File type module, an error message is displayed.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Paste tree";
        public const string HelpText = "Loads a tree from the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "a916ad8e-2f22-439f-b764-8beb54673f7d";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Paste";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "Clipboard";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.V, Avalonia.Input.KeyModifiers.Control) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = 4;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>() { };

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEpSURBVFhH3ZfhDYIwEIXBOIPjuICJUzAB/EfhP0zAFCYu4BAM4RLaq4W0x9GWcg2GL7mcFFNe36sIydakqnuT5/ldtNvvaKRq2xbGF3NQfQn44gA15kWIAIlYcQqlDoMxJni/asPe07lMZyyXAqCL8x85YCIjEfPp5yox3yQm7IC3vXDhmYsD3vMER8DF5gLwHjAshT2gPgbhM59VADeUgP+OwIe1MbEIsPwcrcC9hE1A0zRqxI+iKKSAqHvgUj4mhYm+CZ/11SjMvu6ELrsp2B2w2U2xrwgocCw4mqgC9Dj00tl/BC6iCqDyh9KJ7oBrDxxVZwOvcAD+fChYHbCtFMiybKwBLKBSnQ3Kka7rxvcKFuB5wAV8p+/7ybsEi4olT0SsK19PknwBx1S+bG45zP0AAAAASUVORK5CYII=";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHYSURBVGhD7ZlNToQwGIZBXbP0Bh6CzcS9ie48ASeA/Qjs4QRs3LpwYXTnwkxiuAEujBfwDP7xdgop/9S2lEn6JA0tzKTf235vS6htCeD7flhebvatBlGapnimnGN65WYkeHDuuq6d5/kLbSvjiF7/Axs8RhyzGe2bhCFxUhERUFOly1Jpw0I88LmLe9PhdLMlzyfSBZCcn/s7VMo+f8mdJlHZJ9cg2EPBAwiYERQvRMSAAMAlAik0FZzM4IHU/qR4QCdGgG5g4iEz1auQCmT1OypAFzwCjAd0oySFVHqnjTIB5Q6+iLeUCkiShN5RQxAExsTaWUUK3T6/WXe7d9oa5ym+pLUVpdDc4PswKdQHbwpdbB9orZkiU5hViAcY9Sp6JKPdLiIsJuD+9cP6+v6hLXksJkBF8ECLB2BUtohgTCybPpOzpY2ZARlcb85ojZ/FdmJ2+kWNW2F24jVgBOjm4AWsdhXCCjOHVc+A53mjBUAAe7J4MGRZhkt0Uk43zqvQkH2UNEjfOw3LVIpVweOsTdk3zDEPTAEB8ECVJixs8Kgs5gGR952KdvBKwQyIgP8XRVEXtMvSCfwg9gFm5DsLzuoFtILv7FlKTUyrokSO48RhGPZ8FbCsP5QoPQL7cL4JAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJPSURBVHhe7ZsxT8JAFMdbnFycjF/DmcWwk+ju4sLqAswgzMCiI7s7CZOLcWFw0tFvoNGFRSf13vW10eYo78q1veu9X9K8a9Mj9//z7t2V0DAwSLfbvRJhGJ1lMprNZnBv5exh3BkN8UCr2WyGq9XqHs8ro4HRBCrx8E1Dlo2i039QzSoUkwakSdIco8qEyinMgPQct2XOp5FF8O1h3BKhL442nKs4OhnIe/MWOs0akfQVY/uRV9QsxTERY8tdSxoU8TGaIoZ4v24/IOm7BRhzHzXkAqYASTyiW7hACHyDuv0Aah9pQtTUBwygireZ3BqKXAWcgA3A6C3eGxBuWWcT4n1A2RQ9PrIBtpPXAK4BGL2FDcDoLaUWwapWkixKNwAfjqyBp0BVGTCdTuW1quj1ejLyKoDRW7gGuFYDHl9eg+vFU/Cx/sIrNJbjU2xFOFsDbhbP2uKzcM6A9/UntszANcC1GtAeLLAVkZ7bVHgfgFiTAaaqOxXrMsB0dadijQGmqzsV72uAtQbA3FYdpuFVwJZVgLq+p++jkv483gcgzhlweLCPLTM4Z8Dl2bFRE5yrAabgGoCwARi9hQ3A6C1sAEZvqc0+IF7XdeEMqFsGdDodGbOYz+fYijIA/nPvDX/Fh2F4DhlAfl9gV3QygEqcKZQMSIsXY7htiEHB2xYTcdQ6E1TiZVteKZFNGXAxucv1yzAlAzaJB6xZBUw/58dkiQesyYBdUWXANvFAbfcBFPFALQ2gigdqZ4COeKCyGlA0FPFA7TJACP+mig+CIPgF3ww5F68XgAYAAAAASUVORK5CYII=";


        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Base64, ref Icon48Base64, ref Icon64Base64, 32);
        }

        public static Page GetIcon(double scaling, ref string icon1, ref string icon15, ref string icon2, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon1);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon15);
            }
            else
            {
                bytes = Convert.FromBase64String(icon2);
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

            Page pag = new Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true };
        }

        public static async Task PerformAction(int index, MainWindow window)
        {
            string text = await Avalonia.Application.Current.Clipboard.GetTextAsync();

            string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

            System.IO.File.WriteAllText(tempFile, text);

            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await window.LoadFile(tempFile, true); });
        }
    }
}