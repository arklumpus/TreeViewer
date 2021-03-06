using System.Threading.Tasks;
using TreeViewer;
using System;
using System.IO;
using System.Collections.Generic;
using PhyloTree;
using PhyloTree.Formats;

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
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "9b49587c-3d02-4fbe-859f-feed56dca92d";

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string ItemText { get; } = "Apply Further transformations";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupId { get; } = "cdd01933-9e17-438a-ae45-57178e3d30cd";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static bool IsEnabled(MainWindow window)
        {
            return window.IsTreeOpened;
        }

        public static async Task PerformAction(MainWindow window)
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

            await window.LoadFile(tempFileName, true);

            MessageBox box = new MessageBox("Question", "Would you like to close the window with the original file?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);

            await box.ShowDialog(window);

            if (box.Result == MessageBox.Results.Yes)
            {
                window.Close();
            }
        }
    }
}
