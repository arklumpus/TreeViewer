using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace a24093e44fdb0427ab00a21f24fb95898
{
    /// <summary>
    /// This module can be used to redo an action that has been undone (e.g. enabling or disabling a module, or changing a module's parameters).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Redo";
        public const string HelpText = "Redoes an action.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "8a2c7de0-58c5-4a85-9a0d-d11ce9844139";

        public static string ItemText { get; } = "Redo";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "History";
        public static double GroupIndex { get; } = 9;
        public static bool IsLargeButton { get; } = false;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>()
        {
            (Avalonia.Input.Key.Y, Avalonia.Input.KeyModifiers.Control),
        };

        public static bool TriggerInTextBox { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADLSURBVDhPlZLNDcIwDIVTxCCwCb1yYQUWQaXqIl2BC9eySdkE3kvcKD+OmnySldiJX203nQm4Pl4nLHfYYAOOETa/p9vXuQ7c/SHWHcRn4IJldZ4585Crc80q5xm2Avkyk3skfhgLkeQF5s/TClj2qCUTifewJa1kE2DPs9vqlET8DHAhGpJGKGID4CirnYMmwl5lWwaXnjRxm9haYP9D2FstVkBKV6e8R/oSs/+9RyRAWkUyAdIiogqQWpGiAKkR8S9RQ5KilxdjzB+mOmGxMAj7lgAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdsAAAXbAaSjC9AAAAJLSURBVEhLrZVNaBNBFIDn7W6N1HoSVAQVLIgHiwdBQSgVi9GuJoGiBT14EDwo9Fa8RQYj9FIQPKjgRagng61JbIoFWxBv/oAoKB4Ue/IHD4q9hJ15vje7LGoyybbZD5Y3+152vpnZmSwIC76c2Y3KuQAABwSKXZTSKOAjCHzmus6dmswth79sxi9WNMd6qeA0CfKXKxuDDNwWAs/QrW0ACgFv9jmNibIca0S5GBIgRxKAYzIReVnZFmTEc+r8LN3+BhC36DqCbrCdRr2TdFmq3aVaAAjjKyrzNCtnNpuHLcQjPCyXvF71a5Gag4jilSfgVO1a/lNY/ZeTxdo+LfQDavYLxLeBp4cX5Oi3sGqZQa/6eYnCII3w83qlsrbOmUel3OvAVYe4cwGw1wucJ7aZRAKkmcDFqDU+Ozn6w6TbwCPmkXeSGMFxWR2gsIeu5b53jTrnkpBEYgSexn6OtHAvy+Uxxe2ktJJEJYMRaIQtHGmdvnJcLf9LorTB7KKRKw9ztO2qNIPqfKlQMBULI3SI+IVFt22JdxFq+MCRnjp4Wt5fx+20iEfiF6svSLWfpniufjU/HaW7Jtqm/IL1DRMRr+dkbYdJpkAsoLWfpncwS1PapAI11+kvICmxgFYLM64+3+ngrJam3eDLua1CBUvU5IP3BoOe4flJ/7sproGW2y1NSUsBk5bEKmDSkLQVMN1KOgqYbiSJBMxaJX+dg/bU5Ykv9BUbMudEiAFwG4vZiccbwqqdxALGfHe9nqPUfA/CubcwdWwlrNgQ4g8zKCK2Y4l2vAAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAB88AAAfPAYZRWU8AAALSSURBVFhHvZddaxNBFIbPZLdWRcGKnxT0VtRKQSoIikq1pRuaFLEV/AGK4FWVgtjI2iDirb0Sf4DQgpLUNFoqUrCC4oVYLaJ3iiL1qqBgwu6O75lNN5g0H9vs5oHJnJ1pMs+e2ZxpBNUgNpLabLdSXBL1kpSdJMQ2xI6QtIT4rZQiQ8taNjtu5ApvqYqRSOHtRNPJuFq7osCgObHuj906gnAYrU0NVua7IDGWSfY/wEeqBSpRKhDhl1KMG5m9WPwNwiRamxD0End/RdMih0nTd+satUtHHCVJI2jv8TftkuR9I5F+NmA+3sKfUS9lGeg3p/bYtvMKYTvaZ0niYjYZm1OTqyJF9Gb6nJQ0joudaAuWZp+eMc8uqekSqmaA025bTgohLz6Xz+eOVF+cETIzFp+0HNmFi0W0Dt2KPO8xH+1Q0zX4T+C3vf4actKJ8AsWj8/eHVp2Z2ozc3vgG+78FLbqAx7Og/VKeAInzYlNyMlVjrHnl/wsvgKn3dKdbj8SnsAGpzWO/dyKcB4pfeGO+sevhCcgHNmrAikfqr4B/EgUnwEhDqleiteqb5B6JTwBfDe2c6+30A81EACrSRSmPIpbQGRxLymiq4GAYAnc1RmEn1jCHS3iFSIUCK58XcjEsWwyPu+O1malsPilrBChgr9TPcnjaqBJeBmIjqZiUlAKAx8zyViHUmoCXgY26rmn6L5i1QPRRPq8Oxo+nsCkOZTH4rc4Rn+PDyU1ETLeFrjgZEtMPcHRauBikWu7eopDxMuAi5C5/N8LCBbQ9tdTyxulRIBIHUKa3oNQfW91W5vtuz6tilQYlGxBEcPM7CLb4kNpH9qCtFq6s3eMX2oyQCoKMM2QqCrAhC1RU4AJU6IuASYsiboFmDAkfAkwQUv4FmCClFiTABOUxJoFmCAkykqxH6bN6E8cWCfU/3z4RRTRrUF3psnwgdU3mrpcuPQB0T8GM3WQvSSUJQAAAABJRU5ErkJggg==";

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

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.CanRedoProperty;

        public static List<bool> IsEnabled(MainWindow window)
        {
            bool isAvailable = GlobalSettings.Settings.EnableUndoStack && window.CanRedo;
            return new List<bool>() { isAvailable };
        }

        public static async Task PerformAction(int actionIndex, MainWindow window)
        {
            await window.Redo();
        }
    }
}
