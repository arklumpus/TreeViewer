using System;
using TreeViewer;
using VectSharp;
using System.IO;
using PhyloTree;
using PhyloTree.Formats;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace a7476c9e405b14a928d36aa878d22cceb
{
	/// <summary>
    /// This module applies all the modules that are currently active to another tree file. This module can only be
	/// invoked from TreeViewerCommandLine. The new tree is opened in the current session.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply modules to other tree (command-line)";
        public const string HelpText = "Applies the modules that are currently enabled to another tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public const string Id = "7476c9e4-05b1-4a92-8d36-aa878d22cceb";

        public static bool IsAvailableInCommandLine { get; } = true;

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = null;

        public static Page GetIcon()
        {
            Page page = new Page(75, 42);
            return page;
        }

        public static void PerformAction(MainWindow window, InstanceStateData stateData)
        {
            ConsoleWrapper.WriteLine();
            ConsoleWrapper.Write("Enter path to the other tree file: ");
            string fileName = ConsoleWrapper.ReadLine();

            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                {
                    
                    TreeCollection coll = LoadFile(fileName);

                    if (coll != null)
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
                                    
                                    bw.Write(stateData.SerializeAllModules(MainWindow.ModuleTarget.AllModules, true));

                                    if (stateData.Attachments.Count > 0)
                                    {
                                        bw.Write("#Attachments");
                                        bw.Write(stateData.Attachments.Count);

                                        foreach (KeyValuePair<string, Attachment> kvp in stateData.Attachments)
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

                                BinaryTree.WriteAllTrees(coll, fs, additionalDataToCopy: ms);
                            }

                            System.IO.File.Delete(tempFile);
                        }

                        stateData.OpenFile(tempFileName, true);
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The selected file does not exist!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        private static TreeCollection LoadFile(string fileName)
        {
            double maxResult = 0;
            int maxIndex = -1;

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                    if (priority > maxResult)
                    {
                        maxResult = priority;
                        maxIndex = i;
                    }
                }
                catch { }
            }

            if (maxIndex >= 0)
            {
                double maxLoadResult = 0;
                int maxLoadIndex = -1;

                IEnumerable<TreeNode> loader;

                try
                {
                    List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };

                    TreeCollection coll = null;

                    Action<double> openerProgressAction = (_) => { };

                    bool askForCodePermission(RSAParameters? publicKey)
                    {
                        return false;
                    };

                    loader = Modules.FileTypeModules[maxIndex].OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

                    FileInfo finfo = new FileInfo(fileName);

                    for (int i = 0; i < Modules.LoadFileModules.Count; i++)
                    {
                        try
                        {
                            double priority = Modules.LoadFileModules[i].IsSupported(finfo, Modules.FileTypeModules[maxIndex].Id, loader);
                            if (priority > maxLoadResult)
                            {
                                maxLoadResult = priority;
                                maxLoadIndex = i;
                            }
                        }
                        catch { }
                    }

                    if (maxLoadIndex >= 0)
                    {
                        try
                        {
                            coll = Modules.LoadFileModules[maxLoadIndex].Load(null, finfo, Modules.FileTypeModules[maxIndex].Id, loader, moduleSuggestions, ref openerProgressAction, a => { });
                        }
                        catch (Exception ex)
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("An error has occurred while loading the file!\n" + ex.Message, ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file cannot be loaded by any of the currently installed modules!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }

                    if (coll != null)
                    {
                        return coll;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("An error has occurred while opening the file!\n" + ex.Message, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                    return null;
                }
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file type is not supported by any of the currently installed modules!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
                return null;
            }
        }
    }
}
