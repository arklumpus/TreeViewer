/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeViewer;
using System.Runtime.InteropServices;

namespace AdvancedOpenFileMenuAction
{
    /// <summary>
    /// This module opens the "Advanced file open" dialog, which can be used to manually select which File type and Load file modules
    /// should be used to open the selected file.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Open file (advanced)";
        public const string HelpText = "Opens a tree file, specifying which modules should be used for the reading and loading of the file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "98804064-922f-4395-8a96-216d4b3ff259";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Advanced open";
        public static string ParentMenu { get; } = "File";
        public static string GroupName { get; } = Modules.FileMenuFirstAreaId;
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
		
		public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.O, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift ) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = 1;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACqSURBVDhPjZOBEYUwCEPRyRyt7qTzVYKpYgte3923IaRK9b7UWov+IorMgCTlh8zvWblG7LhJwjsdKspp/J5wAu1vCGUwA8owgZZt80YrBH2EPjdoJlZaD/QPli9wuQ6bVdsnpjacvl8ki/DJ9FL6UHhm9p7RoWGwjJ9C34LAgg7axkJvAE1KWRRKw/VOrjkIK+kR0gmAD0Zgsr//Athx8Udw2nrT9KPfiFzFPVfAitB//wAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAD8SURBVEhLxZQBEsIgDARbX9an0T/5P8zBpbYI5VA73RlNmuQCpMp0OTHGYJ8egeXjQE23iVLT4kF7GeoCK04hchwnInT/Qtnv/hHZhhbsSoUaJ5yOyFLefGFIBhoImwt4ASxDMtBkqWnxzfgGEojDMiQDTZZSCy85BAnEYBk6xer8Jqj/25GhKzW33KEh/UQtvy1gRto5a5zyHjs87wXyWKymbNrifQIizRxYrS/yZCiB5xzevQ8GurD2cwQFTDlhZlwCCrqJ2aC7UdaM3kUrrcpofQZHx06N/jsYBeLco8t3i1DslIt1fwRdTOdN0g7pJ2r5n0Gj3K/WcJpeHJmrLPSGe1UAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAGhSURBVFhH1ZZLksIwDEQDZ5oDZNbMoXIoWA/LWcydQrfdcoLtkPhTRfGqTGJJlvyRFYZ5nie0UqahEyd603sRJ6DXNtx6gLq7yLxq0jnOer6N6gloI0pJc0eKkiOoSdqA3CxI3u1Mt1CYJM5n5QAWMKJd3VIKkYuwE2IKdWDvXsNsxOPX9zrCCRB1s0DNla9pqoQYH/ztTgCqODh3ohqO9248LycAcVVw2P2xqRvgeDpZMW5OAKLa4OskvUq8STYJIYoT7ps/MLm7noCd5cLddObPML/y6RYB0ZJDHEDUdYZeEmA/WT1kcUV0q9V7QLL46uYngMfhbYeupSSnEwBFZ069N6ti8S1BzMvgBuxyVZEy7g5bVq/hHgnXHApuaIyRFCjKvMoj8YLkh9CQzZVLnUCdN3nC2Rd/DTGQBebie0/865kjp7vQ15n39Cga+H64fdzDiKojaEbOjOIkbNpW+OMq4ny4odmZf6Elepzmj97rQfAuhajlP2FRvYjoMoEYbm1ypJLxWLK05oAlXf3nuDcItL5yfa5aKQic/UuWMgwPWDaQrI+cA2YAAAAASUVORK5CYII=";

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

        public static Avalonia.Controls.Control GetFileMenuPage()
        {
            return new TreeViewer.AdvancedOpenPage();
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true };
        }

        public static Task PerformAction(int index, MainWindow window)
        {
			window.RibbonBar.SelectedIndex = 0;
			window.GetFilePage<AdvancedOpenPage>(out int ind);
			window.RibbonFilePage.SelectedIndex = ind;
			
            return Task.CompletedTask;
        }
    }
}
