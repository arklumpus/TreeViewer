using System.Threading.Tasks;
using TreeViewer;
using System;
using System.IO;
using Avalonia.Controls;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using PhyloTree.Formats;
using System.Threading;
using System.Security.Cryptography;

namespace abb4eb8d419254f088881c8dd6531e5c4
{
    /// <summary>
    /// This module applies all the modules that are currently active to another tree file. The other tree file is opened in a new window.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply modules to other tree";
        public const string HelpText = "Applies the modules that are currently enabled to another tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "bb4eb8d4-1925-4f08-8881-c8dd6531e5c4";

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;

        public static bool TriggerInTextBox { get; } = false;

        public static string ItemText { get; } = "Apply modules to other tree";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupId { get; } = "cdd01933-9e17-438a-ae45-57178e3d30cd";

        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;

        public static bool IsEnabled(MainWindow window)
        {
            return window.IsTreeOpened;
        }

        public static async Task PerformAction(MainWindow window)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>();

            List<string> allExtensions = new List<string>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                filters.Add(new FileDialogFilter() { Name = Modules.FileTypeModules[i].Extensions[0], Extensions = new List<string>(Modules.FileTypeModules[i].Extensions.Skip(1)) });
                allExtensions.AddRange(Modules.FileTypeModules[i].Extensions.Skip(1));
            }

            filters.Insert(0, new FileDialogFilter() { Name = "All tree files", Extensions = allExtensions });
            filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(window);

            if (result != null && result.Length == 1)
            {
                TreeCollection coll = await LoadFile(result[0], window);

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
                                bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.AllModules, true));

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

                            BinaryTree.WriteAllTrees(coll, fs, additionalDataToCopy: ms);
                        }

                        System.IO.File.Delete(tempFile);
                    }
                    
                    await window.LoadFile(tempFileName, true);
                }
            }
        }



        private static async Task<TreeCollection> LoadFile(string fileName, MainWindow window)
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

                    EventWaitHandle progressWindowHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    ProgressWindow progressWin = new ProgressWindow(progressWindowHandle) { ProgressText = "Opening and loading file..." };
                    Action<double> progressAction = (progress) =>
                    {
                        if (progress >= 0)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progressWin.IsIndeterminate = false;
                                progressWin.Progress = progress;
                            });
                        }
                        else
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progressWin.IsIndeterminate = true;
                            });
                        }
                    };

                    TreeCollection coll = null;

                    Thread thr = new Thread(async () =>
                    {
                        progressWindowHandle.WaitOne();

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
                                coll = Modules.LoadFileModules[maxLoadIndex].Load(window, finfo, Modules.FileTypeModules[maxIndex].Id, loader, moduleSuggestions, ref openerProgressAction, progressAction);
                            }
                            catch (Exception ex)
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Error!", "An error has occurred while loading the file!\n" + ex.Message).ShowDialog2(window); return Task.CompletedTask; });
                            }

                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                        }
                        else
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Attention!", "The file cannot be loaded by any of the currently installed modules!").ShowDialog2(window); return Task.CompletedTask; });
                        }
                    });

                    thr.Start();

                    await progressWin.ShowDialog2(window);

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
                    await new MessageBox("Error!", "An error has occurred while opening the file!\n" + ex.Message).ShowDialog2(window);
                    return null;
                }
            }
            else
            {
                await new MessageBox("Attention!", "The file type is not supported by any of the currently installed modules!").ShowDialog2(window);
                return null;
            }
        }
    }
}
