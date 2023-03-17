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

using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace af10d9b5ba9ff403690d523f73042fec5
{
	/// <summary>
    /// This module opens the system default web browser at the address of the "issues" page in
    /// the TreeViewer GitHub repository.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Feedback & questions";
        public const string HelpText = "Opens a web browser at the TreeViewer feedback/questions page.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "f10d9b5b-a9ff-4036-90d5-23f73042fec5";

        public static string ItemText { get; } = "Feedback / questions";
        public static string ParentMenu { get; } = "Help";
        public static string GroupName { get; } = "Help";
        public static double GroupIndex { get; } = 1;

        public static bool IsLargeButton { get; } = true;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };
        public static bool TriggerInTextBox { get; } = false;

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAI2SURBVFhH7ZdLaxNRFMfvuUPbhZsiRXQRSCj9ELp0V7CCCq58rQUXSXwQoxBKYlETshK3Pnam0EVQV+rOLxFMloKvlZuZdO71f+begmbGzMydYhH6g5vzyOSe/9zHgYiDhqx1olarPYC5ibEQJbIz1Vo/7vf7TRu7AQHBZDJRmCwX4/FY1et1n+eQ0UzuLJTL5dyrWKlUCDoW2S8qoDCHAg4F/N8CiGgXfcBG2UEf4N9O2S/UCdFMtmBu4U57JiNEr9cTQRCIRqNhM3FQPMR41O1279rU/oDOqH3f12zRZp/Z9FwKrcAsXJhtqVR6Ua1Wr0bJFFIFrN/bWRVSnpaKjiqpf0jpvXvdOjO2X/8BC0Dx5yh+zaZS+auAjdZwRYXqKV7pAsLfn+O33PY8eX3Y2vhmUoZms/mw0+ncsWEmEgVw8TBUH+GumUwiI4g4NSsiL4nXkN8cZl5xZg0in1jfmdgK8J4TyRHc1PMBtKdpddg+m78ZWOIrgAOHzyzFGVJS8PPOxASQEivWzYRW+ph1nYivAOmv1ssESfpiXSdiAtBWP7AxUSpaKvHe+k7EBLxtn/sEs22iVAZFDiAT3wLATQaGb8I8Rku7IT9XiEQB3Fww+Um4A4zZ7eD4FX+/s3X+u0m5E1239RtvlsRy0CZBlxAe55wDnzFeHvH8+4PWxcCk0olWgJanmyjO/3BcizMnMG7/VIubJszG3hZctrYwpOmKdTOxJ4DV7xe55ko8hP+SAxYgxC8GDNqQKkUi5QAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAL6SURBVGhD7ZnNaxNBGMZndtMo+IGIUIroIQoVvKTm2D/AWlNbi/Ggdz0nGAVrdRstCiHk7qHowYgVxFpSvHrQQ01ugr2IRT2YXqxEQcnuju/svhG/JpvM7mQ97O8y7zMLs/NsZmafJSQiZCi2Sshmswal9BrKwGCMzZXLZQOlOnK5HKvVasw0Tbinf/g4fDw+Lt6CaNgqI5lMEl3XUfmDj5NKpVC5KDcQ1ORFKDegmshA2EQGwiYyEDaRgbCJDHhRr9exCoY/xwstTpdKJazcSVUqFVTeQDDtT5wWweNwm3Y8zufzB/ByT4S6B9pPHhLrwWKx+Aa7eyI0A+3Ja5qWkJ08R2oPZDKLevPwwL6YFRtiOm0OkNaHx8bJTbzsSfuLys+Tb9OTgQlj+ZBlsouEsgmQe9xeBxtGWiU2u9PYaCzUb59vYf8/4QaCmDynKwOGYWir1pF52P8XQMbcXiFrVNczVSP9CrVSPA3wyb+0Rh7Ab34Ku7qhSW02Vp2feoFaGZ6bmD/5HifP2cE0+ig9s7wXtTI6Ghi7sjSMy0aGQZtaN7FWRkcDGqGXoPFa82IoPQsbfz8qJQgN8KMSTxs/aLbJJrFWgtDAt+Gt/Mn9elRKAftnBEslCA2YjAxi6Q/KhrBSgtAA1eyvWPqlia0ShAbiOnsPje0qeSgl77BUgtCAk214PPCLzZ5ipQShAQfINljJsv5xo/EMayV0NMCDGTRrrpKBzXgFO790NMBvTpk1DeVnt6d7GCN3V65Pdf+dKEnnJQRUb0y/hrP8OJQNt8cbSIgL22Pfz6FUStffAzyYOdkG4gFIkfF1uH55pXDiPmrl/Gbg2OzSKCXsKnSPgtzm9gYH3OwLnMvPNZsVgoraPw2Mzz45DSv3HpTy4a17THg/nKkWJh+ilsYxcNRY3K1bW95CuZPrPrFJW3qieiv9CbUUzlrWrHgamn5OnrPLjlvjWEvjGKCEKs3sQmySwEoa9zShfVn3f0E14vs/WM/3wP9OZCBcCPkBUXM7h2rG/uIAAAAASUVORK5CYII=";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAB2HAAAdhwGP5fFlAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAABCdJREFUeJztmE9MHFUcxz9vdmsNwixHpbVpUg/FVOOtV08mgMA2zWrSqheT6sWom9o2KaHDFjyJhxogMTExRS9LSITlj/bi2VtjTNxT06SVEk67w3poOzO/HmApBJXH7Lx9Je7nBLMzv+/v993f+83bBy3+3yhbwp7nPef7/nXgfeClJsmuANOu6w57nvcIIN0k4V34vl8ALjVZtgu47Ps+wBWwaAAb3zzd3d2Sy+VUJpMxKlatVikWi1Iul9Wm9hUAx6jqf9MF0IziATKZDLlcrr7ku+rXbRoAbCTWLDo7O3dds26AbVoG2E7ANi0DbCdgm5YBthOwTcsA2wnYpmWA7QRs0zLAdgK2sW5AtVptmlalUtl1zaYBKwDFYrEpJlQqFWZmZnZog90ToWngcrlcplAo/OtN4+PjO/5fXV1lamqKWq0WW1hEbtb/tmaA67rDm2dzH6B5KJpA8Q+Am5lM5lr9grVT4b3I5/MCTztgbW2NyclJ1tfXcV133PO8i0noWB+COpgqHg6AASaLB7tDUIuJiQlqtRrt7e2JFw8HoANqtRqu644XCoXEi4cEOqDfKx2LAhmMlPQpOA4c3fzovsBdYCGMZO7WWPZenPgm2n47sd8Cb18tHQmdaFjBh0Bqj9sjYDZKRZd+9s7c1Ymfz+elo6Pjq5GRkS/i5qhDrCXQM1TKihOVFVxg7+LrOjkndH7vG5ob0JQZMV08xOiA3qG5T1F8Tfz5EYF8vnQ9eyPm84myLwN6hkpZpaJZGh+ekRLOLI4OzjcYp2G0DejxZo86YfpPgfaEtNfTKU7Oe4Mre99qDu1vUgWHCgkWD9ARhGokwXix0OqAfq90LAyjO+gNvP0QSio4vuydvZ9wXG20OiAMwyzJFw+QUkFq0EBcbTSXgOoxlYBSTq+p2DpoGaDgFVMJCHLCVGwddIfgi6YSUHDEVGwdrP8Yko1tsjW0DJBth4gGOBD7gDumElAoY7F10O2ARVMJRCILpmLroLcPiGQOCA3oB6SDkoG42mgZcGssew/h+8TVlfrO5i4Q9vEWcMS5BqwnqO2HwWPrvwW0DVgY6/9LlHqXZJZCJEre++XLsw8SiNUQ+9oHLBcGlkHyNPbujpSoz5YLWatrv06sM8G+obkBUfwAdOzzUR9R55dGB6xO/u3E2gkujg7OS3DoBKgbQKDxSITIdBgGJ5+l4uEfOqDnk6XDKvP4Yxx1DpFTQFuTc/ob+EOJ+jGqpr9d/qb3oUmxHQa8dfWnl9OOWgReMymqjXBb0kG/yVfllgFver8+3xb4v6F43ZRYLITbL6Qfnp7x3nlkIvzWDGgL/I+eueIBFG/UwsMXTIV/OgQddc6USKMoxXlTsbcMUCKvmhJpFCWcMhV7y4CEj7wTxWRu1k+EbNMywHYCtmkZYDsB2zwBTEJWtQmQGIsAAAAASUVORK5CYII=";

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

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { true };
        }

        public static Task PerformAction(int actionIndex, MainWindow window)
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = "https://github.com/arklumpus/TreeViewer/issues",
                UseShellExecute = true
            });

            return Task.CompletedTask;
        }
    }
}
