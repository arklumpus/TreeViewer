using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Runtime.InteropServices;

namespace OpenFileMenuAction
{
    /// <summary>
    /// This module shows an "Open file" dialog that is used to select tree files to open with TreeViewer.
    /// 
    /// Once a tree file is selected, the installed File type and Load file modules are queried to determine
    /// how appropriate they are to opening the file. The most appropriate modules are then used to read the
    /// tree file.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Open file";
        public const string HelpText = "Opens a tree file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "078318bc-907f-4ada-b1e5-171799957b2a";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Open";
        public static string ParentMenu { get; } = "File";
        public static string GroupName { get; } = Modules.FileMenuFirstAreaId;
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.O, Avalonia.Input.KeyModifiers.Control ) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = 0;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAB3SURBVDhP3ZBBDoAgDATBl/E0fHlloBA0tJGTiZM0C2VbN4YfICK51IqsFpeIE23XGxILejY5VFecNceakc5L4DHSWQlSKRZbxb+D3A9or0QDxWjBO6bngq1hdF6wP6yNeulamwa8N9vk0wbsD4M23+J+5AtCuAC4cAah719krAAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACaSURBVEhL7ZLRCsMgDEXtvqyf5r7c5cwbmGOlCcV2Dz0QYjT3isFycz2ttWqxR1V7mgU1uZebtMXQOsVDeRrRC57vQcUYxhkdUYZhnH8xotWCV0aDn+lUL8i/YrUALkmBBiELz99x2JxMAZ/GY0MSNF0qrYo55qCNOeagzTnm4AeetR0GTZduaHUIKXPr3zcHNRwh/eqbMynlBYpuuPVjLffPAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADtSURBVFhH7dZtDoIwDAZg8GR4Mw7lf70Z9l3fjQaIsA6HJnuSxrGPdsIidk1zuWmaRolcI5cX65GN7Sy9YLOI3cDRhGH+WRu48fMy7g3gzjmszw4HAO0j4Tm0CcsmnjPgtXl2/u4MDBIPCXyb3AjCc5iNOY8AxZ/aPFHYh7IHbRmDhFX0SyjrUz5cRLagjWVx3Ak3rNc0Ch2RLRrjq8XFgM7IFt6eXADrNU2i+XgBH4sjwgIHrrfmXOyA+sWBnbA/ORPWa5pknY8DUL84cHCpTnHgBKteceH+S3bQXV5+L7Y3ffNtuFu8aX5A170BVLYPhdVMqX8AAAAASUVORK5CYII=";

        public static VectSharp.Page GetIcon(double scaling)
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

            VectSharp.RasterImage icon;

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

            VectSharp.Page pag = new VectSharp.Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }


        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true };
        }

        public static async Task PerformAction(int index, MainWindow window)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>();

            List<string> allExtensions = new List<string>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                filters.Add(new FileDialogFilter() { Name = Modules.FileTypeModules[i].Extensions[0], Extensions = new List<string>(Modules.FileTypeModules[i].Extensions.Skip(1)) });
                allExtensions.AddRange(Modules.FileTypeModules[i].Extensions.Skip(1));
            }

            filters.Insert(0, new FileDialogFilter() { Name = "All tree files", Extensions = allExtensions });
            filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(window);

            if (result != null && result.Length == 1)
            {
                await window.LoadFile(result[0], false);
            }
        }
    }
}
