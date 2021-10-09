using System.Threading.Tasks;
using TreeViewer;
using System;
using System.IO;
using System.Collections.Generic;
using PhyloTree;
using PhyloTree.Formats;
using VectSharp;
using System.Runtime.InteropServices;

namespace acdd019339e17438aae4557178e3d30cd
{
    /// <summary>
    /// This module definitively applies the action performed by the current Transformer module. This discards the information about the tree(s)
    /// that were originally loaded from the tree file. For example, if the tree file contained 1000 trees and the Transformer module computed the
    /// consensus tree out of them, using this module will discard the 1000 trees and only keep the consensus tree.
    /// 
    /// The tree with the Transformer action applied will be opened in a new window and you will have the option of keeping the window with the
    /// original tree open or closing it. The Further transformation, Coordinates and Plot action modules will be applied to the new tree as well.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply Transformer action";
        public const string HelpText = "Applies the action performed by the Transformer module.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "cdd01933-9e17-438a-ae45-57178e3d30cd";

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None ) };
        public static bool TriggerInTextBox { get; } = false;
        public static string ItemText { get; } = "Apply transformer action";

        public static string ParentMenu { get; } = "Edit";

        public static string GroupName { get; } = "Apply";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static double GroupIndex { get; } = 1;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAB5SURBVDhPY6AUMEJphqKiov9QJhj09fXB5YgC6AYQCxiRNZJsKzIg2wVQGmsYeNVu/L+t2R+vq/BKggwA0eiGIFuGYQBMEzpANgRkACy8mMAiRAKQRpLCCuQabC5CNoQFSmMNRBCNKxAxXEKS05DAIE9IUCYtAQMDAJSAQ4LrDM0kAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC+SURBVEhL7ZRBCgIxDEVb8XRuvEv3Mor73sWN14vJmOI0TemMTYUBH5RQSJPmJ8SNxrOdCSFc0Uzv24cYY+a3hQPbRBHcFKwA6PDVBK8F7JFEIiUayxCJ2M7UgvdIdmSbuOEpJgkAnPftHPjBYsyrryjokpTgdHnA835W35nISwno8DVD62Gz7lowgirRfr3s2W/HdA3mEkko+JYmZ47o0LVNtfcygVq62W6iBLUk3/Lfpk2GN1lKRNt0Tzj3Arf9Y0UqdZeuAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADtSURBVFhH7ZZNCsMgFIS1x2uhvYv7pn9779JCez2rrQEhvnkqGrPwAzEukjdMJkOkIFBKXe12+Z/iaK3J+1PZ+T0GHN4c64Bxyx+bgRxYhe4CJGdzjaAhujtAMkK4FqgJWftTA2qfRbYqcuDmdxJjkiPSrlX356fVgYWgQBdlwA0Mhx6mFyuCokiAlPK3QkpFZLWcs9tfknweJ7JdY6Hd7meYSujK+360b2b5yNmRpg5QwzmqCCgd7iDvQu01k9GEZHiRAzXbi23VBU41Ul6L7p9h/x7gbB7/hP7YjBFC1ISs/TUCihzIb69shPgCUnRjnFZC6lAAAAAASUVORK5CYII=";

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

        public static async Task PerformAction(int index, MainWindow window)
        {
            string tempFileName = Path.GetTempFileName();

            using (System.IO.FileStream fs = new System.IO.FileStream(tempFileName, System.IO.FileMode.Create))
            {
                string tempFile = System.IO.Path.GetTempFileName();
                using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                    {
                        bw.Write((byte)0);
                        bw.Write((byte)0);
                        bw.Write((byte)0);
                        bw.Write("#TreeViewer");
                        bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, true));

                        if (window.StateData.Attachments.Count > 0)
                        {
                            bw.Write("#Attachments");
                            bw.Write(window.StateData.Attachments.Count);

                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                            {
                                bw.Write(kvp.Key);
                                bw.Write(2);
                                bw.Write(kvp.Value.StoreInMemory);
                                bw.Write(kvp.Value.CacheResults);
                                bw.Write(kvp.Value.StreamLength);
                                bw.Flush();

                                kvp.Value.WriteToStream(ms);
                            }
                        }

                        bw.Flush();
                        bw.Write(ms.Position - 3);
                    }

                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                    BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, additionalDataToCopy: ms);
                }
                System.IO.File.Delete(tempFile);
            }

            Avalonia.Controls.Window targetWindow = await window.LoadFile(tempFileName, true);

            MessageBox box = new MessageBox("Question", "Would you like to close the window with the original file?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);

            await box.ShowDialog2(targetWindow);

            if (box.Result == MessageBox.Results.Yes)
            {
                window.Close();
            }
        }
    }
}
