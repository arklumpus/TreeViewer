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
using System.Runtime.InteropServices;
using System.Linq;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace aaaf3a3f5d4d941c8bd30d99e061f28cf
{
    // Do not change class name.
    public static class MyModule
    {
        public const string Name = "Reshape tree";
        public const string HelpText = "Changes the coordinate module and sets the plot actions to display the tree in a particular style.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public const string Id = "aaf3a3f5-d4d9-41c8-bd30-d99e061f28cf";

        public static bool IsAvailableInCommandLine { get; } = true;

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.R, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt), (Avalonia.Input.Key.U, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt), (Avalonia.Input.Key.Q, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt) };

        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Reshape tree";

        public static string GroupName { get; } = "Tree style";

        public static double GroupIndex { get; } = 0;

        public static bool IsLargeButton { get; } = true;

        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>() { ("Rooted style", GetRootedIcon), ("Unrooted style", GetUnrootedIcon), ("Circular style", GetCircularIcon) };

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHYSURBVFhH1ZZBUsMwDEVLh4ORLRuuQK+QPQOd7nOFcgU2bOFm8L8rexTbiuWknYE343HsOJb0JbvdXYPHl48HtB8ZdnEnfWAcR9cm0zSl72gc3RefP09Ps/08FA7ozT14I3c551VAE+VfcmTp3Z5GY5O5LhDZN7rhMuqnmYKaY9401SLPU7GXfhEa1E2mPeTKFEo1oyWdRmewRtDxlAySrj7W1oaXZmSeGpA1A+ZDhBi/oXvlc8YRa/iOa4Iyq6XVxM3QmONwKYHgEJ2jw2oNievs08PcsYolh01ogMZkmLDmxKF6ClThRLoLCAZSGvKUaaxjqI2TfLxINE7DS8abMAXy2EVNdgvXRbQGyfPqKz7wnxU4ep24iQMoPBZhcKLliHUMi49afyhgKBzdvOrpQG0OXbio7i9TBfpGI/aNBaJxtEFFnK5motaQsD/mBjMqdRnNLiF89IzurKMSo9t+C2STAxa8X2ZKonG0WXRb0EVIWc5ipGCtcSrJmhJFC/LiiLLPlNhiHN2slvLflKIGlBM53bIzcnlM5KfpKgrESK2jSkesd6kGLONExgc0Hh2uS1Rk7iJ4tWRcE5XAmhRNTeYalgLRgeYRtNAKWEZuDp3wKvHH2O1+ASOCPQJTyiyHAAAAAElFTkSuQmCC";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAOGSURBVGhDzZk9aFRBFIV3g6WFjZ2FpXZ2Wli4KChiUCsRDKQN2GTFUjRYaONqIVgqRBBBMKKxUtbCQsFCSCPYCKYQtLCwFPSctzOPybz5uTPvbz84zO7svsm5c++8mbcZDizG4/ENNNdn79KZTCaVMcnpay+OoZm+vnnW+XkuC6o1yTbvQ5ufvWsWVwb+sfXNZCq2+aYz0EUAxXgZrCFYlnMQVwnNC6JSTsqAZIHb1+WUkM6a5LsLNGxK9ftIXuAw8Q7NaPaueYY+054MeLMTQ2ei6QwkmawTQIzYYvcF4zUZoqUAQnvFSJViBdddaE21nRJYK17zJGkm2ywhjZWJoPlkGICkxLokKwMhUrKD8XajOQedhA5B+yCyDX2BNqENjPmbnS5SA0jeyIygl/DZY/Wa/WfQPIC0aR+/oBVc+2z2trj2Epp1vm6tljXmHwNL0AZ0C7rMDvAGegqx1r+yAxyEjkIXoBPsAI+gVYiBl+O1HgCxgmBpHIBYFquYWRor0Nkys4i+ZTR3oT3QN2g/RIqMdhIAsYJ4D12EgW30cx0wG4chrgdXGbLMnkDMCinLURRAW7c2GOO4DyE9q2QR5l6p11Gix2nHDjlVfbVQ5jkuzTMji9Be6AMkJpiB3O09hiqbLYjm70FXMet/0SYTy4DPPAl9FoM1r2c+2zwRL2Jko7hDYNZrL3xk4DkaLtikenfRVwDf0Tg3MASUNP48PxOL6CuAT6plCQ1NqX4xfQXwUbVXUE671Oss+grgPsRjAW/TtX4J7GURE2MjI9xP7kDFJoZS4glURG+LGCZpmo+QOhMvoZ/QEUhMcDb1rPtIyQZmvJXDXCwDoR+kxD9WWeY54zSyhf5lGPkD3YbOF59a8DtoeOzgNT/YB9bVmPE14DkPic9Blnk+0HDn5fmexgjHeQt9hlhGhAc7PmIeh/j3CZ/IVqBTUDmeqASsIJzmYbT8jlkC6NdlOL+PlKZ5YgYQAtc1+1BvzNYIF0lLxC4x8bVN4AuARI20YV5Srib2Xci8s0yVQScdmCfRp79KrUqMdWTexJsJ52JzGAxR2zxBAFmbZmcB6Bn2GTHRwUi+W9mJPeVhn9nFa4VEyqMWOwKQ1rbqEwXRpnlSpkhq3sS+RmVnB7HaDiEuoRzzxJGJJhH9p6iIEAEkbWAp2CUkmdUUKhtZk+YJDLeZpe5gJuqsBzeDwX9WvfD25CIsAQAAAABJRU5ErkJggg==";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAASESURBVHhe1Zs/aBYxGMa/iqODo4ODg4KL4CC4CLYgiKKooIibBUcH7aRDqaWLU3VwEBzsJqKgYlUEoQougoPgIrgIOrjrKOjzXPMeufuSy5/LXeIPXvJ9+e5yeZ68yV2uOjNpsbCwcAPF0ua3dKyurk5dq83xxWezKDZerpxyHpuKLarUSS7eBxG/+W08TBnwl6XPiKWiLT53BoxKrpEXsmbAAOKXkT1cw7yZEcFtTAb0WSBN7cEA47X7EDp9QqdA6gVyTpXZCJoCQ0wP0zQIHUU9k4bOgOSgw29RZMuE7AaQnCZY07yLoe4QMh3GnAImA5wrfduAmHP6EHL3cBkS1CnbIjh21pgWTgtzanpZSWpASpEuPExwiidRBtgY0wDSYYKXeBJ6F1hWZREoke27h7d4kmTEckwBHS0TgsSTpAZ0kcscF6kMGOU2iOvsRXEWcRixUwX5oeId4jGu9YWVPmQfFZU9y+i0dRuLY06joMH7qwo3nxBs8+nm12lk0EoxgEyZgN92oXiEOFBVTCbfEOuI1wiO8lcE2YNgdhxFnEDwPPIRcQ7t8rwaEc/PJRigT5/aBNQfQkHxOxA/EYuINfz+B2WFmIe6WgfqtqK4iFhByLk04T3KqetlN4C0O4V4g3iF2IbgiF+AgN8oG5gMEPAbz32AYEbw3GOII4iG2VEG9Lnt2GiZwFHj6N1GJ69WNQocRxHnEezDbtaZDBBw/C0UVxDSJqkzLdgAw9PXUCaso5Mn1Wf+th0FxTC9dV7gOI6yFZz7HIUc01hrggxI8ejpQpnAUa07iTquB0xn3vaYzncQHxBc5Hgsb4GdmNol3gZ0iBeSmaCjRv4zguLZ/jxENFb1PnjtBTzEkw11XGqY9pV4CJ9LKZ74boZc4gXf47zA6HPB45xn2s+zLjWxd4H62R9pH9WGDzDgHopLiJsY+etVZWKKeCnagUwpLniDUHoG/ELBBxoryIxe1y89Awan9AzgZodPe2cw0tadXR9KzwB5rjioyuSUbsBDVV5GNsgWNylFG4C0565wDcGF8D7rUvM/LILcDfJZfxZZsJE6E4pbBCFw1M1QURmgOsnt8BI+cwtbgU7zbc4+hEyHa4gniO+Iu4hOVFvSbsMAr9HTR9xFbEZo4klZL0RggM9ukERtiVviy3wl5mFCEvEyMqgv76VohwnRL0NEBGi8qiL4rbzX4gYTer0JUgZMidfBMWX9YUQzwSkeF6oNQ4eiriegrf/rT2O6eNLXgCEwdggdl3nJd3Cxc3tqqsS25SIkI9u4HoT46MnGg8gkngS/mLUZoP+riyATMooXgkywzskYIQWI1/GaDp2LUoigwsQLThOcq7JBmA+DiScwQBZpJzCgU2NRBsjIujrdRjck9NzOg8ecAnpaj2mA9TYYKkj9FnX30MWPjdGA2NGMMSGneDKVLn1TmbTbwPnGtBxCfK8pkEI8MWSCjWwjLzTcggH67SVKfAgDZED4f5tTZYVmwODiBZMJoWnch9Eu1EXbhDENcO0GRwGCfdeM5BRhAMljwmTyDzaayhIjT0IJAAAAAElFTkSuQmCC";

        private static string IconRooted16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABbSURBVDhPY6AUMEJpMCgqKvoPZZIHyDGACUqTDSg2gBHd2X19fSjhQghgBCKyASSHCboGYgygPAygNBhgs5FQmOCVpEuYEHQBlAkHpEYzCiDZBeiA5DAgDBgYAB1HK2AtTjNZAAAAAElFTkSuQmCC";
        private static string IconRooted24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACbSURBVEhL3ZMBCoAgDEU1OrEXqC7glWujKWKbNbXAHsiwYPX3/6zJcM6tUJbz1s5ENaVbcxZQsOOhazOcgq6M/wErzdt7f0lYDe8roBoJijgFNTuiVdC+I6U9KL2T+EFMqUaejEAT4ZlqygZHZSb8lJgu1TIFdbkCrfEi2IhrJj1Hvje5xN0oOPO1CjAA3zOOBxIlb3opELwx5gBjTT7Jz6I+nAAAAABJRU5ErkJggg==";
        private static string IconRooted32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACZSURBVFhH7ZVLDoAgDETBI3MB9QK9smIsK6VDLPWXvoQ0DQuayXSIoUJKacpl3Ds7Bq5nmD8ukhVYtsOtGZICt+AD+AAROZ2IqlnRg+cV4HqgKFNToFdSahSwTUqUhOi+FV9DHwCuIUIbVJICM9d30msNL8tXHtcmpaUJv5GUngNqEyJQTmgU+EdOuAlVH4lEq0ktFWgwaQgrVu1BKqj9TNoAAAAASUVORK5CYII=";

        private static string IconUnrooted16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACnSURBVDhPYxhwwAilSQZFRUUJQGo+M5Tz39LS8uHx48cvgPiEAEwzEDsygUWADCCeD5XAC5A19/X1HQC7AGjzA6ALDgKZ6/G5BF0zSAxsAAgQMgSbZhDACESgQgcgtR/CwwAomkEAxQAkzYlAhQvAglCAywVwL+DTDAIgL4G8BmSCvHgQ5GWQOCwa8WqGAWyGMBGrGQagahKBGBJOQAP+Q/03MgEDAwClc2sXcLnLmQAAAABJRU5ErkJggg==";
        private static string IconUnrooted24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACiSURBVEhL7ZTRDYAgDETByWQlBmEl3EwpKQS0gBWifvB+eontHbGKmHyOxDoUrfXqigU9PCA1B2KAe7CjVMaYDTWLs7lDLShSLDayoMzhoGmAwgqwQkrmILId1BpLtGYuS+aE3OklvyJi8A7kQaglD+XdV8QxD7RmYsAT80Bt1u+gxxzA3uw/wiqWXvMAEeKRLiDcQcAj8xrZVTHafPIHhDgABgdqAudtU9YAAAAASUVORK5CYII=";
        private static string IconUnrooted32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADLSURBVFhH7ZVbDoUgDETRlV22xELYku7sSk0xguVRLegH56c1sTMTpaoGg7eZsHbFGPNzZYG+e4CzOdA1QGzu0EEAd8MfW22tXbEXgTIHjxkvYhYcECFlDk0cQGMFRELkzIHLGSgNcKjRIg+hRIhajeQWPAnBmc2u4Z0Q3JlsAIAQ5FAMnFrDbnz3Fdwx93BmyQBPzD21GpcAEuaeGq0ggKS5p6R5bEELcwA1gn8M1p09QCtzDxHiYGptXgKewGvmwPlL2N18MPgASm0CWppzwshXFAAAAABJRU5ErkJggg==";

        private static string IconCircular16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACaSURBVDhPxZPRDcMgDEShk4WVWCDJAqzUbpbeIyaitJVCEin3c9jcGSOMux0+xriIQ0rpRULxJBpZN5ilYQ/NIHoq9g8tAgFJK4aZgl7sjNGM7Bez5VZU5g/8y1mRjHxKCwm2a5ROdgPzr5N3o9f81V5b4MgVujrgGVvMPUWueQUZjs+BBWWyYJBHGzEdVBpQdFlz+i8Qn4Bzb8QoZrhFFHdHAAAAAElFTkSuQmCC";
        private static string IconCircular24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHCSURBVEhLtZWhSwRBFIfvxGiwCAaDUZtNg0FR0OCBTQQPrAeWWzEbxeBqEIwKCiIIHqhN0WDQJlgEi+AFQYN/gKDfb292WNcdb2/P/cF3b26YeW/em52ZQt4qGpsoz/O6MLMwBUPQB1IdHuEcar7vf6gzSUWcfJl2mYEHpi3nM5gdCJ269A4V5h43/gZzFzD7aiuA/YPKUIM1WFIHuoAjuIYndaBBGIU5mFQH2oMqaGHWX1CiWBClPgBKu8rKNDFQmC19trT0LWI2oRueoR+koCLRgdEgNzDPgDr92gdlMwzajx8BJMaojIegrCRb7mabPIbZhXBVUonJZ6bdVB3G/pJxfgVyroxK0AO3kFqJGZiyPICcb8EKq/7EtixXBqp5uPLMziVXAG2otN6Oc8lVohdM4gEj4J8fRlzOTf4vuTI4weibb+mTTJIrgztjlwnWadqZ5AqwDTr2Ogur6sgq54ZFDpqki24DgkNG2XSDppJzk3Eip+MQZnIKbzACqWUzYMX5XXYx5+GVm+W6foVekBrXdcy5Hhx9lpqgiZJKdQn3oDJJuvj0hE6AyifpRavANFh/+T+Z+nGJgW0/+jmrUPgGVmSxynaVavYAAAAASUVORK5CYII=";
        private static string IconCircular32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJ+SURBVFhH5Zc/aBRBFMb31DJIsLKwSBFDmkAQQQ4ELyAEQSGBhJAmmN5Cr7KRM9hfUlxhl+tEEtCQhBCwUDAcgRQBG8FG0MJeCxFBf9/cvGX3dm/34PYuRT74eLdv5v2ZN2935oJzj5KXPaFarU4iFuAdeM1T+O75AW7X6/XPUvaCEk7/Idcwet5WJcGcOUQNTjtFPk6hfL5tPyaBT8WrWQJCIgnGxhBb8KZTBMFXuAcPoVb5BQrXoaozC+9D2QkncBG/sgthwfVbCYQPIEwC/W2Egl+FP+Az2GT8L9LBkkcXbiW6S4iH8AU0WyXxERkLDtacYacSvoMHcARqxcs4+IWMIS0BA2OyfQVVEdneg3dhbLHRzKNJKGtlv8GkJ07jwTw5WYIVOC5dWgIG5q8jHkPzKYSVjhl2JLHHpAf+t8ZGEXKm8kaxzzytsiuw3UXYnFivpZXODUYnoVM/qJx67VTOBjyGajLN1SuYiTS/QtfSGfzKP0EFfw9XcRLr6n5wwcssqOwuOIFnigwuZCbA6tVw2nOVfVW6opFXAXW70Ch65Ya8BPSqCWq4gSCzCdmCnwh9ULqCyuQ2chZ6acKBIq8COmz0tZtnpV1Ptn6QVwG998ItLwtHXgKvvXxENeyILRSZCVB2nYpNqEbclK5onPmnOJEAAYd6GMW2wE/ScVzjt45QB4x0m5mCth1P4Rv4Db6EmfC+zG8sgYteRoMLujzcKJfLo61WS/e/APkb7qA74vEPvAyvwAn0ukWlAr86zFagfCr5Cj5K2Lg3zG1BR/DhXsk6g9seoR/OpdSc6MGCGxgbyrVcCSSCR8Gcwf0xaT/2BowK/2t23hEE/wHvszoDSd5xtAAAAABJRU5ErkJggg==";

        public static Page GetCircularIcon(double scaling)
        {
            return GetIcon(scaling, ref IconCircular16Base64, ref IconCircular24Base64, ref IconCircular32Base64, 16);
        }

        public static Page GetUnrootedIcon(double scaling)
        {
            return GetIcon(scaling, ref IconUnrooted16Base64, ref IconUnrooted24Base64, ref IconUnrooted32Base64, 16);
        }

        public static Page GetRootedIcon(double scaling)
        {
            return GetIcon(scaling, ref IconRooted16Base64, ref IconRooted24Base64, ref IconRooted32Base64, 16);
        }

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

        public static void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
			if (InstanceStateData.IsUIAvailable)
            {
				window.PushUndoFrame(UndoFrameLevel.CoordinatesModule, 0);
			}
			
            List<PlottingModule> plottingModules = stateData.PlottingModules();

            List<(int, Dictionary<string, object>)> parametersToChange = new List<(int, Dictionary<string, object>)>();

            for (int i = 0; i < plottingModules.Count; i++)
            {
                List<(string, string)> parameters = plottingModules[i].GetParameters(stateData.TransformedTree);

                Dictionary<string, object> pars = new Dictionary<string, object>();

                for (int j = 0; j < parameters.Count; j++)
                {
                    if (parameters[j].Item2.StartsWith("ComboBox:") && parameters[j].Item2.Contains("[\"Rectangular\",\"Radial\",\"Circular\"]"))
                    {
                        pars.Add(parameters[j].Item1, actionIndex);
                    }
                    else if (parameters[j].Item2.StartsWith("Buttons:") && parameters[j].Item2.Contains("[\"Rectangular\",\"Radial\",\"Circular\"]"))
                    {
                        pars.Add(parameters[j].Item1, actionIndex);
                    }
                }

                if (pars.Count > 0)
                {
                    parametersToChange.Add((i, pars));
                }
            }

            if (actionIndex == 0)
            {
                if (stateData.Trees != null)
                {
                    for (int i = 0; i < parametersToChange.Count; i++)
                    {
                        stateData.PlottingModulesParameterUpdater(parametersToChange[i].Item1)(parametersToChange[i].Item2);
                    }

                    double defaultHeight = stateData.TransformedTree.GetLeaves().Count * 14;

                    double totalLength = stateData.TransformedTree.LongestDownstreamLength();
                    double defaultWidth = 20 * totalLength / (from el in stateData.TransformedTree.GetChildrenRecursiveLazy() where el.Length > 0 select el.Length).MinOrDefault(0);

                    if (double.IsNaN(defaultWidth))
                    {
                        defaultWidth = defaultHeight;
                    }

                    double aspectRatio = defaultWidth / defaultHeight;

                    double maxAspectRatio = 4.0 / 3;

                    if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Maximum default aspect ratio:", out object defaultAspectRatioValue))
                    {
                        if (defaultAspectRatioValue is double aspectRatioValue)
                        {
                            maxAspectRatio = aspectRatioValue;
                        }
                        else if (defaultAspectRatioValue is System.Text.Json.JsonElement element)
                        {
                            maxAspectRatio = element.GetDouble();
                        }
                    }

                    if (aspectRatio > maxAspectRatio)
                    {
                        defaultWidth = defaultHeight * maxAspectRatio;
                    }
                    else if (aspectRatio < 1 / maxAspectRatio)
                    {
                        defaultWidth = defaultHeight / maxAspectRatio;
                    }

                    CoordinateModule module = Modules.GetModule(Modules.CoordinateModules, "68e25ec6-5911-4741-8547-317597e1b792");

                    Action<Dictionary<string, object>> updater = stateData.SetCoordinatesModule(module);
                    updater(new Dictionary<string, object>() { { "Width:", defaultWidth }, { "Height:", defaultHeight }, { "Rotation:", 0.0 }, { "Apply", true } });


                    if (InstanceStateData.IsUIAvailable)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            window.AutoFit();
                        });
                    }
                }
            }
            else if (actionIndex == 1)
            {
                if (stateData.Trees != null)
                {
                    for (int i = 0; i < parametersToChange.Count; i++)
                    {
                        stateData.PlottingModulesParameterUpdater(parametersToChange[i].Item1)(parametersToChange[i].Item2);
                    }

                    double defaultHeight = Math.Min(10000, stateData.TransformedTree.GetLeaves().Count * 14);

                    CoordinateModule module = Modules.GetModule(Modules.CoordinateModules, "95b61284-b870-48b9-b51c-3276f7d89df1");

                    Action<Dictionary<string, object>> updater = stateData.SetCoordinatesModule(module);
                    updater(new Dictionary<string, object>() { { "Width:", defaultHeight }, { "Height:", defaultHeight }, { "Apply", true } });

                    if (InstanceStateData.IsUIAvailable)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            window.AutoFit();
                        });
                    }
                }
            }
            else if (actionIndex == 2)
            {
                if (stateData.Trees != null)
                {
                    for (int i = 0; i < parametersToChange.Count; i++)
                    {
                        stateData.PlottingModulesParameterUpdater(parametersToChange[i].Item1)(parametersToChange[i].Item2);
                    }

                    int leafCount = stateData.TransformedTree.GetLeaves().Count;

                    double defaultRadius = leafCount * 20 / (2 * Math.PI);
                    double innerRadius = double.IsNaN(stateData.TransformedTree.Length) ? defaultRadius * 0.1 : 0;

                    CoordinateModule module = Modules.GetModule(Modules.CoordinateModules, "92aac276-3af7-4506-a263-7220e0df5797");

                    Action<Dictionary<string, object>> updater = stateData.SetCoordinatesModule(module);
                    updater(new Dictionary<string, object>() { { "Outer radius:", defaultRadius }, { "Inner radius:", innerRadius }, { "Rotation:", 0.0 }, { "Apply", true } });

                    if (InstanceStateData.IsUIAvailable)
                    {
                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            window.AutoFit();
                        });
                    }
                }
            }
        }
    }
}
