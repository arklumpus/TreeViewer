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
using System.IO;
using System.Collections.Generic;
using PhyloTree;
using PhyloTree.Formats;
using VectSharp;
using System.Runtime.InteropServices;

namespace a9b49587c3d024fbe859ffeed56dca92d
{
    /// <summary>
    /// This module definitively applies the actions performed by the current Transformer and Further transformation modules. This discards the
    /// information about the tree(s) that were originally loaded from the tree file. For example, if the tree file contained 1000 trees, the
    /// Transformer module computed the consensus tree out of them, and Further transformation modules were used to add attributes to the tree,
    /// using this module will discard the 1000 trees and the Further transformation modules, only keeping the final transformed tree with the new
    /// attributes.
    /// 
    /// The tree will be opened in a new window and you will have the option of keeping the window with the original tree open or closing it. The
    /// Coordinates and Plot action modules will be applied to the new tree as well.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply Further transformations";
        public const string HelpText = "Applies the actions performed by the Further transformation modules.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "9b49587c-3d02-4fbe-859f-feed56dca92d";

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None ) };
        public static bool TriggerInTextBox { get; } = false;

        public static string ItemText { get; } = "Apply further transformations";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "Apply";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static double GroupIndex { get; } = 2;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACQSURBVDhPpZJNCoAgEEa14xXUXdzXotZ6l4K6njWDE2Xjbw8kRf3mOSTFA6WUdVMWrfXr/AcIuBAwUtA56VelKlLGiwGfQjGDdlwtDLdE0MDNEa4HZNRNG+7tc59W4+AMgMZ9q0nqcFWBYxnwbtZ7/BC6XESoB7+5VWK/sTGmXJmIBQPJ5FBAtlW2QV0PhDgBSulUC+7ANwAAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADxSURBVEhLxZVbDgIhDEXB5Wmie4FvH6PfsBdNdHtIGWqA8CiQiSeZQCeh5ZYWOKsghLjZ4bpafWitne+ds8oMOQ+JFBhj/GxFSul+4G5G4OgUneWYCdBK0TyoIE0PYA/ZwOfNIrm1SKuKms4BpRS3eCumlaLFj8NUFdSkp5QUkKoDAx0uLzd5349uXclpCKmKwFHoLLVrbF6mpG3sz8/sYXwep+Z6mk5LGoTivBsIUlLzN34yqV0bsNhLEN6LKjNVRHorooOidm7POzFUCa10wuUHIzTjJo0WdnpWQZiq3JWACrpSNFBFpAAzKSK8FYx9AdSPV79zHIMfAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEOSURBVFhH1ZfNDoMgDIBlj7fL3kXPizM7w7vsstdjVItxzcBSQNyXEO1B6D9VdQGstfi2MAzDwz3GRcrHGDOffZmlHyilvpaj2OEi+r63sFAsRtADR7HmANc6H7tSNPcAm6Y5QEuyBH5PdjxTrdda7+4N5Z2SAxM+i4C9he8BCbHQeQWyuN5fFhaKQbaKUKUOKcOttdTy5n3gPxWAONK40thyESkAcaRxpbHlkvzVXta/n7ekPUVqh5RIPRyQ+c1BlZAcng23EZ2a1W0V7vrJTU8wSUep2YhGaW/Ixk9PHAWat+IqpcPJJ5iYoHs290AVfA6gGOUcOQDZ6v5+i9eMn4xjN2VND8xTdPya7roPeHJqs36CU1MAAAAASUVORK5CYII=";

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
                        bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, true));

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

                    BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, additionalDataToCopy: ms);
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