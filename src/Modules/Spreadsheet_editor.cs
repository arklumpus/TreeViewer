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
using TreeViewer;
using VectSharp;
using System.Collections.Generic;
using VectSharp.MuPDFUtils;
using System.Runtime.InteropServices;

namespace aca8a7928100d4dcc873d31d7991976b9
{
    public static class MyModule
    {
        public const string Name = "Spreadsheet editor";
        public const string HelpText = "Opens a spreadsheet editor window.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public const string Id = "ca8a7928-100d-4dcc-873d-31d7991976b9";

        public static bool IsAvailableInCommandLine { get; } = false;

        public static string GroupName { get; } = "Action";

        public static double GroupIndex { get; } = 9;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };

        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Spread sheet";

        public static bool IsLargeButton { get; } = true;
		
		public static bool EnabledWithoutTree { get; } = true;

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE0SURBVFhH7ZVBroIwFEVbXYuLYKRO1cSxK3BoAmMj5o/+ABbxd2AiTmUnLgXvLS0xPxLBPnAgJ2nKK9B7+9oHqm8W+9MM7WxDpW3fCxRHdy0jlV1+1qteDRCYiNEdykhlY3vRKWEYzoIguKHpv99dPJluuHBmY9t5BiiOzqWdHNM0jbkd2IK8UwNPxA0wUOlqOCnstRhYma4TB3MYyJ3uyAwJ0kaciBpoK07EDLwjTkQMvCtOvA34iBO+XHvzFZi8KifM8/iFI0b81fzeGcDq+HMpIEYDx3K0mTipMpAkiRmoI4oi0z8+pwHE+WdbMnbbQfECcKwON5/EIaR4hjZn0HTlDpFDiLZCyxm3ESdeBpjm/9hbjfGqAgkkzoAXXlXwjLbPfTwDg4GhCoYqGAx8exUodQe08sQZmuOLygAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiMAABYjAclRYGsAAAQWSURBVGhD7ZhvaBNnHMefu8tcYtOuVRhz1CJU3aaCFFzHUFRqZ01S01YQ3B9f6fqm4Iu20PrChlQQxLawF0XoC/8wBlsVa2Maq1ak1oo6upUymFFwG8VXglrbxsaYe/b93SWxrWl7ai6XSD7hx93z57jv957nued3ETjnLB1wNHV/h8MRxtm4IArNXrfzDNWnhQGI3w+pHTgV1BoSLjR6D+88mhYG7E2ejVB+EafZao2CjN9XYqSQktTW1pbW1NRYfc3OQYFxB0xMRJoIkYnC3pQ1UF9fv0sQBJ/ZbO5taGjI9jZXDHCZOdA0qfZgWA7sn5ScQiQeun7F6QdqDbsxNTVla29vn7A3XdiC0ehB3c0sKVgeWxRGMfsB1tXV2XE4h/hQqXjFNBOeIutE8G5n2+7nKWUAT96GchdOZ4uPMgATdjIRKWMhGEyV+3wuHTWIJ7MTkiSFsDZYLOyHug1bBBj+E19Kf/44Nja2HWJIvFltiUtvKBSqfJBbso4z8QrKeQhu2Ai8qficnJyqf5duXztNvIIxBrhwksSPj49/o0H8JRJ/O1z0RTjMZ4gnDDDATxWb/tiPJ1+KOX0eFfOJvwzxlSQes70P1y6J1MdIqgG8cE4XS8P78OS34cl3o2o+8VdgsPIOW/85nccTTyTPABd+s/qDingI0yK+wv/R5s94WOzDelkaqX+N5BiA+IAp+4eCgtsbIYzmvEVtiMt1URSr7ltLVi8kntDfAOd/Zfmnvi8a83wN8ZQCZKkNcRmAePtdy5aVYSZfXUg8oasBi/zsb5G9cHd27g5DmBVV0dwmHsou+0j6dIU5/Pi4FvFAEJCy6rmRPcFb5OPf2YYCr8vxYKE8x2Kx5GOUrqH8iVq9MLqOQH5+/qjL5XrJ5fDPjkOeg62trT4I/BZNIbWHwiCJdzgcW2FgGGXN4gndDEA8KywsHCw/2JOHxL0YW+8RfBo2trW1deEVugddyMSgyWSyVVdXrxkdHT0bCATmzIPmYsYUamlpURIkrSABi2WTca6lhuV2V/cmfL9Sbq8Q/Zalr61gMHirrKxsk9PpPIGmZWqP+aH70X2j6DmFRhAPBc52qEXA2YgsynlOt3cxRqLPZrOVDg0NedGiSXw89DTQGzkGuMCqufRyec9h53qfu6LR4ypf1dHRca+/v79rcnJSivR7K/Q0QP8isJ7mihqf2/mLz7WLUoKfEP9hGgz7/f5VsixTl3dCLwPPEE8RNFmvIp4gKJM8gChAJIwZBt5kAc9m1rU5CHolHkOUIBYhYiTwPjPfQumInmsgKaS9AT03sjmZvhnRNXStVpK5kSWFjAGjeb8MJHKDmY9E3iezkRlN2htI2EaG713lqIWU3MiiRpJNZg0YTcaA0WQ2MqNJcwOM/Q//cLy7ocxzawAAAABJRU5ErkJggg==";
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

            Page pag = new Page(32, 32);
            pag.Graphics.DrawRasterImage(0, 0, 32, 32, icon);

            return pag;
        }

        public static async void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                SpreadsheetWindow spreadsheetWindow = new SpreadsheetWindow();
                spreadsheetWindow.Show();
            });
        }
    }
}
