using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace a24093e44fdb0427ab00a21f24fb95898
{
    /// <summary>
    /// This module can be used to undo the last action that has been performed on the plot (e.g. enabling or disabling a module, or changing a module's parameters).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Undo";
        public const string HelpText = "Undoes the last action.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "24093e44-fdb0-427a-b00a-21f24fb95898";

        public static string ItemText { get; } = "Undo";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "History";
        public static double GroupIndex { get; } = 8;
        public static bool IsLargeButton { get; } = false;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>()
        {
            ( "", GetIcon ),
            ( "Undo", GetIcon ),
            ( "Undo 5 actions", GetIcon )
        };

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>()
        {
            (Avalonia.Input.Key.Z, Avalonia.Input.KeyModifiers.Control),
            (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None),
            (Avalonia.Input.Key.Z, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift)
        };

        public static bool TriggerInTextBox { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADKSURBVDhPnZHNDcIwDIUNYhC6CVy5dAUWQYBYpCtw4dpu0m4C7zlOSSj5gU+ynDh5ln9WAg6n+/Nxa/XsQWwLd4SdNeC4wjr8ndxVZG0+AuId3Ohu0ljyxl1ltHdlUYE99rA9YgNjIcE7E09RBSUxsThbYXvvFmrEAR1MZxNWUCtmFfMQN+Y9PSqxowOfo+0Q/OGG/gcJLjSev64xh82K/XMOvyUwsZ+VzmHRX4oP8TzoqgQpMSkmyIlJNkFJTJIJasQkt4WiWETkBV/BYJIGzJkTAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdsAAAXbAaSjC9AAAAI8SURBVEhLrZRPiE1RHMfPueeaJ2QjlELYWJiNlBJpRh4zenf0xELZKIWys1FGN08pTSkbCxuhlJHpvcedTI3EjmahlEkhVigLmVHGPefn+zvvPlzPnXfvfe9Tt3N+55z7/Z7zO3+kAIPDVcNlUBlyuPwfJb++RmtzjITcLgWtR5MjpHhLRM+kMtcCv/y6MTJO04C4hIGN/+agf6dnxvSMSJInEapGawv4X952f9Dx2qWhb1GbJXHGTNG/t2JWF55A/BTCEDrXMaWiUs5aUuFqKUU/vqvom0Hf4bAgnnt+dZX9OSJxBSzuhs6kkHITwjeOcA7cr5ReNHrjlM7W1oWC7sJsM8Kn39XS/sd+HyaUsIKYONHLUOltSeJM/YL3bqHWRaziPcIdi/RXTqelxaBF3DW7Jvzy56g7kbGL5S8kBacSyBMws9mIGeQVb7Lk1VyA4gO+jXv9Wi+3xQw6EWdGRw9pbOYU111DG7iMp6gD8SbIyycuDcmVUfznFLUDg2h8nsvIDAxXqxD1SJI3fn5/fd7BWeFLCfGtXCcj7c22K+gWg+dqR5DiG5CdCireFm7r2gr4rcK7dJnrJMwV2wi6YsDHW4f6AdKxDPs0hn26GXV1bvDv3SkocxQp+n1oOtqDgTPBcun+nESVL9W0UG5f4O/7aDsjchukEWdyGaQVZzIbZBFnMhlkFWdSG+QRZ1IZ5BVn2t6D4umHi6Wae4Rqr31pld6ZVpxpazAxsmdWCucWqtPCXbA72zMuxC/Sbg22UJRmegAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAABHNCSVQICAgIfAhkiAAAAAlwSFlzAAAH0AAAB9ABuYvnnwAAABl0RVh0U29mdHdhcmUAd3d3Lmlua3NjYXBlLm9yZ5vuPBoAAAKqSURBVFiFvZfLaxNhFEfPzUyNL/ABLUpRt1JbFXyAgtQiVpLYpgitIN2KCHalCEUjY4OLduFCcKH/gNBKJamJtlRKwS50IUVFCu4qaBVBigomncl10SSk70ky6W/5cbnnzB347oyQTTASU4BkNCy4SKAr6WebExDREKpHVagR8KH6E5FJgWEjRSzeF/69Wh/TDWxhVEKRocvK3B2gFgCBvLXILqBeodP28ysYid3fYqT6BqyO9HLdfMWg26xn24OR+LCij4BalPcoNzUjJ0yDWgxzt2H4jqB6TYTXwA4g+tfxvw3eSuxbrqfrCTRbgzVpxzcKNADfRehKRFufguii0hngHfAwEIk3CvoYOITPnmixhk4OWS3ThcWuJtBsDdaYtu9VFv7JzuixRE94YBn4gryIto6n06njwDhQ69iZWLvVv6EogTxcpB7Vj7bhNI3ca/viRhxgtLdjNp1OhYHPCIf/OBtvuBZYAjczZ0asCz/cwgslRLgCIHD9tNW/dU0Br+C5JHrCY8AE6M5NGX94VQGv4fmoPgGQjJ5bUaBicACVNwCIHFxRoGJwwKziK4BCdf5sSZVIPTCFWXV2xAp5Bp8H+0zIIGCvLDCf/Tj2t2AktmZTt7sDwHYye7LF33NnRV3F5UbQUwCiTObO8hMo5klKi4oQ78xenc9zp+s2gVAkflHhADC92Uy9XFeBFmtor8IDAIW7hau5wmPP3iuOMQbUCZJMRFvOFy6xik6gYIvWAR9S6X+XFm/QEr6I3CXQnawWZ24UYf5eMczm0d7w7OK6iryCQHeyWsy53PfDFIbZlLRCM8vVei5QDNxzgWLhngqUAvdMoFS4JwLlwMsWKBdeloAX8JIFvIKXJOAlHErYBT7Tbgcasj8pjeXAS07gduxqszVY40Wv/2atT5BpbKYBAAAAAElFTkSuQmCC";

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

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.CanUndoProperty;

        public static List<bool> IsEnabled(MainWindow window)
        {
            bool isAvailable = GlobalSettings.Settings.EnableUndoStack && window.CanUndo;
            return new List<bool>() { isAvailable, isAvailable, isAvailable };
        }

        public static async Task PerformAction(int actionIndex, MainWindow window)
        {
            if (actionIndex <= 0)
            {
                await window.Undo();
            }
            else if (actionIndex == 1)
            {
                for (int i = 0; i < 5; i++)
                {
                    await window.Undo();
                }
            }
        }
    }
}
