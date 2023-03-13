/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
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
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using System.Text.Json;
using System.Reflection;
using VectSharp;
using Mono.Options;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class Program
    {
        public static string Version
        {
            get
            {
                return AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location).Version.ToString(3);
            }
        }

        internal static bool WaitingForRebootAfterAdmin = false;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static int Main(string[] args)
        {
            bool showHelp = false;
            bool rebuildAllModules = false;
            bool showUsage = false;
            bool reboot = false;
            bool installUnsigned = false;
            bool installReferences = false;
            bool deleteFiles = false;
            bool deleteModules = false;

            List<string> filesToOpen = new List<string>();
            List<string> modulesToInstall = new List<string>();
            List<string> modulesToUninstall = new List<string>();

            bool clean = false;
            bool cleanCache = false;
            bool fetchAssets = false;
            bool showWelcome = false;
            bool showFileAssociations = false;
            bool moduleCreator = false;
            bool startNewProcess = false;

            int delay = 0;

            string keyOutputPrefix = null;

            string globalSettingName = null;
            string globalSettingValue = null;

            bool logExceptions = false;

            OptionSet argParser = new OptionSet()
            {
                { "h|help", "Print this message and exit.", v => { showHelp = v != null; } },
                { "rebuild-all-modules", "Recompile all installed modules from the stored source code.", v => { rebuildAllModules = v != null; } },
                { "i|install=", "Install a module from a module.json.zip file and close the program (unless the -b/--boot option is also specified). This option can be specified multiple times.", v => { modulesToInstall.Add(v); } },
                { "install-references", "When installing modules, also install the module's additional references, if available.",  v => { installReferences = v != null; } },
                { "install-unsigned", "Allow installing modules without a signature or with an invalid signature.",  v => { installUnsigned = v != null; } },
                { "u|uninstall=", "Uninstall the module with the specified id and close the program (unless the -b/--boot option is also specified). If modules are being uninstalled and installed at the same time, the modules will be uninstalled first, then the program will be rebooted and the new modules installed. This option can be specified multiple times.", v => { modulesToUninstall.Add(v); } },
                { "b|boot", "Reboot the program after the specified module(s) have been installed or uninstalled.", v => { reboot = v != null; } },
                { "d|delete", "Deletes the tree files after opening them.", v => { deleteFiles = v != null; } },
                { "delete-modules", "Deletes the module files after installing them.", v => { deleteModules = v != null; } },
                { "w|wait=", "Wait for the specified number of milliseconds before doing anything.", v => { delay = int.Parse(v); } },
                { "clean", "Export all the currently installed modules, then uninstall all of them, delete the compiled module cache, and reinstall them.", v => { clean = v != null; } },
                { "fetch-assets", "When exporting currently installed modules, fetch the assets from their remote location, rather than retrieving them from the cache.", v => { fetchAssets = v != null; } },
                { "clean-cache", "Delete the compiled module cache. This will also remove all assets. You probably want to use the --clean command, instead.", v => { cleanCache = v != null; } },
                { "k|key=", "Create a public/private RSA key pair to be used when signing modules.", v => { keyOutputPrefix = v; } },
                { "welcome", "Show the welcome window.", v => { showWelcome = v != null; } },
                { "file-associations", "Show the file associations window.", v => { showFileAssociations = v != null; } },
                { "module-creator", "Start with the module creator window.", v => { moduleCreator = v != null; } },
                { "set-global={/}{=}", "Set the value of a global setting.", (n, v) => { globalSettingName = n; globalSettingValue = v; } },
                { "new-process", "Start a new process, even if another instance of TreeViewer is already running.", (v) => { startNewProcess = v != null; } },
                { "log-exceptions", "Log all handled and unhandles exceptions to the console.", (v) => { logExceptions = v != null; } },
            };

            List<string> unrecognised = argParser.Parse(args);

            if (showWelcome || showFileAssociations)
            {
                moduleCreator = false;
            }

            if (unrecognised.Contains("--4e83aefc-1b77-4144-aa81-dc55cbca0329"))
            {
                WaitingForRebootAfterAdmin = true;
                unrecognised.Remove("--4e83aefc-1b77-4144-aa81-dc55cbca0329");
            }

            if (unrecognised.Count > 0)
            {
                List<string> reallyUnrecognised = new List<string>();

                for (int i = 0; i < unrecognised.Count; i++)
                {
                    if (unrecognised[i].StartsWith("-"))
                    {
                        reallyUnrecognised.Add(unrecognised[i]);
                    }
                    else
                    {
                        filesToOpen.Add(unrecognised[i]);
                    }
                }

                if (reallyUnrecognised.Count > 0)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("Unrecognised argument" + (reallyUnrecognised.Count > 1 ? "s" : "") + ": " + reallyUnrecognised.Aggregate((a, b) => a + " " + b));
                    showUsage = true;
                }
            }

            if (showUsage || showHelp)
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("TreeViewer version {0}", Version);
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Usage:");
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("  TreeViewer {-h|--help}");
                ConsoleWrapperUI.WriteLine("  TreeViewer [options...] [tree_file, [tree_file_2, ...]]");
                ConsoleWrapperUI.WriteLine("  TreeViewer [-b] -i <module_file.json.zip> [-i <module_file_2.json.zip> ...]");
                ConsoleWrapperUI.WriteLine("  TreeViewer [-b] -u <module_ID> [-u <module_ID_2> ...]");
            }

            if (showHelp)
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Options:");
                ConsoleWrapperUI.WriteLine();
                argParser.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (showUsage)
            {
                return 64;
            }

            if (delay > 0)
            {
                ConsoleWrapperUI.EnableConsole();
                System.Threading.Thread.Sleep(delay);
            }

            if (logExceptions)
            {
                System.Diagnostics.Stopwatch runTime = System.Diagnostics.Stopwatch.StartNew();

                AppDomain.CurrentDomain.FirstChanceException += (s, e) =>
                {
                    long elapsed = runTime.ElapsedMilliseconds;
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("Exception (first chance): " + e.Exception.GetType().Name + " @ T+" + TimeSpan.FromMilliseconds(elapsed).ToString());
                    ConsoleWrapperUI.WriteLine("\t" + e.Exception.Message.Replace("\n", "\n\t"));
                    ConsoleWrapperUI.WriteLine("\tin " + e.Exception.Source);
                    ConsoleWrapperUI.WriteLine("\t" + e.Exception.StackTrace.Replace("\n", "\n\t"));
                    ConsoleWrapperUI.WriteLine();
                };

                AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                {
                    long elapsed = runTime.ElapsedMilliseconds;
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("Exception (unhandled): " + e.ExceptionObject.GetType().Name + " @ T+" + TimeSpan.FromMilliseconds(elapsed).ToString());

                    if (e.ExceptionObject is Exception ex)
                    {
                        ConsoleWrapperUI.WriteLine("\t " + ex.Message.Replace("\n", "\n\t"));
                        ConsoleWrapperUI.WriteLine("\tin " + ex.Source);
                        ConsoleWrapperUI.WriteLine("\t" + ex.StackTrace.Replace("\n", "\n\t"));
                        ConsoleWrapperUI.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapperUI.WriteLine("\t Unknown exception type!");
                        ConsoleWrapperUI.WriteLine();
                    }

                    if (e.IsTerminating)
                    {
                        ConsoleWrapperUI.WriteLine("Shutting down due to the exception!");
                        ConsoleWrapperUI.WriteLine();
                    }
                };

                ConsoleWrapperUI.EnableConsole();

                try
                {
                    throw new Exception("Making sure that the exception log system works.");
                }
                catch { }
            }

            // Force loading the System.Threading.Tasks.Parallel assembly
            Parallel.For(0, 0, (i) => { });

            FontFamily.DefaultFontLibrary = new SimpleFontLibrary();

            List<string> pipeClientArgument = new List<string>();

            if (deleteFiles)
            {
                pipeClientArgument.Add("::DeleteFiles");
            }
            else
            {
                pipeClientArgument.Add("::NoDeleteFiles");
            }

            pipeClientArgument.AddRange(from el in filesToOpen select System.IO.Path.GetFullPath(el));

            if (NamedPipes.TryStartClient(pipeClientArgument.ToArray(), startNewProcess))
            {
                if (!startNewProcess)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("Another instance of TreeViewer is already running. That instance has been signalled.");
                    ConsoleWrapperUI.WriteLine("Run TreeViewer with the --new-process switch to force a new process to spawn.");
                    ConsoleWrapperUI.WriteLine();

                    return 0;
                }
            }
            else
            {
                NamedPipes.StartServer();
            }

            if (!string.IsNullOrEmpty(keyOutputPrefix))
            {
                try
                {
                    CryptoUtils.CreateKeyPair(keyOutputPrefix);
                    return 0;
                }
                catch (Exception ex)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("\tAn error has occurred while generating the RSA key pair!");
                    ConsoleWrapperUI.WriteLine("\t\t" + ex.Message);
                    ConsoleWrapperUI.WriteLine();

                    return 1;
                }
            }

            VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);

            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

            if (!string.IsNullOrEmpty(globalSettingName))
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Loading installed modules...");
                ConsoleWrapperUI.WriteLine();

                _ = Modules.LoadInstalledModules(true, null);

                try
                {

                    GlobalSettings.SetSetting(globalSettingName, globalSettingValue);
                    GlobalSettings.SaveSettings();

                    if (!string.IsNullOrEmpty(globalSettingValue))
                    {
                        ConsoleWrapperUI.WriteLine("\tGlobal setting \"" + globalSettingName + "\" has been successfully changed to \"" + globalSettingValue + "\"!");
                        ConsoleWrapperUI.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapperUI.WriteLine("\tGlobal setting \"" + globalSettingName + "\" has been reset to the default value!");
                        ConsoleWrapperUI.WriteLine();
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    ConsoleWrapperUI.WriteLine("\tAn error has occurred while setting the global setting!");
                    ConsoleWrapperUI.WriteLine("\t\t" + ex.Message);
                    ConsoleWrapperUI.WriteLine();

                    return 1;
                }
            }

            if (rebuildAllModules)
            {
                Modules.RebuildAllModules();
            }

            if (clean)
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Loading installed modules...");
                ConsoleWrapperUI.WriteLine();

                _ = Modules.LoadInstalledModules(false, null);

                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("\tExporting all currently installed modules...");
                ConsoleWrapperUI.WriteLine();

                List<string> exportedModules = ExportAllModules(fetchAssets);

                ConsoleWrapperUI.WriteLine("\tResetting installed module database...");
                System.IO.File.WriteAllText(Modules.ModuleListPath, "[]");


                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("\tRebooting...");
                ConsoleWrapperUI.WriteLine();

                List<string> newArgs = new List<string>();

                if (deleteFiles)
                {
                    newArgs.Add("-d");
                }

                if (reboot)
                {
                    newArgs.Add("-b");
                }

                newArgs.Add("--clean-cache");

                if (installReferences)
                {
                    newArgs.Add("--install-references");
                }

                if (installUnsigned)
                {
                    newArgs.Add("--install-unsigned");
                }

                for (int i = 0; i < exportedModules.Count; i++)
                {
                    newArgs.Add("-i");
                    newArgs.Add(exportedModules[i]);
                }

                newArgs.Add("--delete-modules");

                newArgs.AddRange(filesToOpen);

                Reboot(newArgs.ToArray(), false);
                return 0;
            }

            if (modulesToUninstall.Count > 0)
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Loading installed modules...");
                ConsoleWrapperUI.WriteLine();

                _ = Modules.LoadInstalledModules(false, null);
                ConsoleWrapperUI.WriteLine();
                for (int i = 0; i < modulesToUninstall.Count; i++)
                {
                    try
                    {
                        ModuleMetadata.Uninstall(modulesToUninstall[i]);
                        ConsoleWrapperUI.WriteLine();
                        ConsoleWrapperUI.WriteLine("\tModule {0} uninstalled successfully!", modulesToUninstall[i]);
                        ConsoleWrapperUI.WriteLine();
                    }
                    catch (Exception ex)
                    {
                        ConsoleWrapperUI.WriteLine();
                        ConsoleWrapperUI.WriteLine("\tCould not uninstall module {0}!", modulesToUninstall[i]);
                        ConsoleWrapperUI.WriteLine("\t\t" + ex.Message);
                        ConsoleWrapperUI.WriteLine();

                        return 1;
                    }
                }

                if (reboot || modulesToInstall.Count > 0 || cleanCache)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("\tRebooting...");
                    ConsoleWrapperUI.WriteLine();

                    List<string> newArgs = new List<string>();

                    if (deleteFiles)
                    {
                        newArgs.Add("-d");
                    }

                    if (reboot)
                    {
                        newArgs.Add("-b");
                    }

                    if (cleanCache)
                    {
                        newArgs.Add("--clean-cache");
                    }

                    if (deleteModules)
                    {
                        newArgs.Add("--delete-modules");
                    }

                    if (installReferences)
                    {
                        newArgs.Add("--install-references");
                    }

                    if (installUnsigned)
                    {
                        newArgs.Add("--install-unsigned");
                    }

                    for (int i = 0; i < modulesToInstall.Count; i++)
                    {
                        newArgs.Add("-i");
                        newArgs.Add(modulesToInstall[i]);
                    }

                    newArgs.AddRange(filesToOpen);

                    Reboot(newArgs.ToArray(), false);
                    return 0;
                }
                else
                {
                    return 0;
                }
            }

            if (cleanCache)
            {
                try
                {
                    System.IO.Directory.Delete(Modules.ModulePath, true);
                    System.IO.Directory.CreateDirectory(Modules.ModulePath);
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Modules.ModulePath, "assets"));
                    System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Modules.ModulePath, "libraries"));

                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("\tModule cache cleaned!");
                    ConsoleWrapperUI.WriteLine();

                    if (!reboot && modulesToInstall.Count == 0)
                    {
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("\tCould not clean the module cache!");
                    ConsoleWrapperUI.WriteLine("\t\t" + ex.Message);
                    ConsoleWrapperUI.WriteLine();

                    return 1;
                }
            }

            if (modulesToInstall.Count > 0)
            {
                ConsoleWrapperUI.WriteLine();
                ConsoleWrapperUI.WriteLine("Loading installed modules...");
                ConsoleWrapperUI.WriteLine();

                _ = Modules.LoadInstalledModules(false, null);
                ConsoleWrapperUI.WriteLine();
                for (int i = 0; i < modulesToInstall.Count; i++)
                {
                    try
                    {
                        ModuleMetadata.Install(modulesToInstall[i], installUnsigned, installReferences);
                        ConsoleWrapperUI.WriteLine();
                        ConsoleWrapperUI.WriteLine("\tModule {0} installed successfully!", modulesToInstall[i]);
                        ConsoleWrapperUI.WriteLine();

                        if (deleteModules)
                        {
                            try
                            {
                                System.IO.File.Delete(modulesToInstall[i]);
                            }
                            catch { }
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleWrapperUI.WriteLine();
                        ConsoleWrapperUI.WriteLine("\tCould not install module {0}!", modulesToInstall[i]);
                        ConsoleWrapperUI.WriteLine("\t\t" + ex.Message);
                        ConsoleWrapperUI.WriteLine();
                        return 1;
                    }
                }

                if (reboot)
                {
                    ConsoleWrapperUI.WriteLine();
                    ConsoleWrapperUI.WriteLine("\tRebooting...");
                    ConsoleWrapperUI.WriteLine();

                    List<string> newArgs = new List<string>();

                    if (deleteFiles)
                    {
                        newArgs.Add("-d");
                    }

                    newArgs.AddRange(filesToOpen);

                    Reboot(newArgs.ToArray(), false);
                    return 0;
                }
                else
                {
                    return 0;
                }
            }

            if (Modules.IsWindows)
            {
                ConsoleWrapperUI.WriteLine();
            }

            filesToOpen.Insert(0, showWelcome.ToString());
            filesToOpen.Insert(1, showFileAssociations.ToString());
            filesToOpen.Insert(2, moduleCreator.ToString());
            filesToOpen.Insert(3, deleteFiles.ToString());

            if (!Modules.IsMac)
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(filesToOpen.ToArray());
            }
            else
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(filesToOpen.ToArray(), Avalonia.Controls.ShutdownMode.OnExplicitShutdown);
            }

            return 0;
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();

        public static void Reboot(string[] args, bool restoreOpenFiles)
        {
            string executablePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

            System.Diagnostics.ProcessStartInfo psi;

            if (!Modules.IsMac)
            {
                psi = new System.Diagnostics.ProcessStartInfo(executablePath);
            }
            else
            {
                psi = new System.Diagnostics.ProcessStartInfo("open");
                psi.ArgumentList.Add("-n");
                psi.ArgumentList.Add(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(executablePath), "..", ".."));
                psi.ArgumentList.Add("--args");
            }

            foreach (string arg in args)
            {
                psi.ArgumentList.Add(arg);
            }

            if (restoreOpenFiles)
            {
                List<string> treeFiles = ExportCurrentlyOpenTrees();

                psi.ArgumentList.Add("-d");

                foreach (string file in treeFiles)
                {
                    psi.ArgumentList.Add(file);
                }
            }

            psi.ArgumentList.Add("-w");
            psi.ArgumentList.Add("1100");

            NamedPipes.StopServer();

            System.Diagnostics.Process.Start(psi);
            System.Threading.Thread.Sleep(100);
        }

        private static List<string> ExportCurrentlyOpenTrees()
        {
            List<string> treeFiles = new List<string>();

            foreach (MainWindow window in GlobalSettings.Settings.MainWindows)
            {
                if (window.Trees != null && window.Trees.Count > 0)
                {
                    string targetFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");

                    try
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(targetFile, System.IO.FileMode.Create))
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

                                    bw.Flush();
                                    bw.Write(ms.Position - 3);
                                }

                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                PhyloTree.Formats.BinaryTree.WriteAllTrees(window.Trees, fs, additionalDataToCopy: ms);
                            }

                            System.IO.File.Delete(tempFile);
                        }

                        treeFiles.Add(targetFile);
                    }
                    catch
                    {

                    }
                }
            }

            return treeFiles;
        }

        private static List<string> ExportAllModules(bool fetchAssets)
        {
            List<string> moduleFiles = new List<string>();

            foreach (KeyValuePair<string, ModuleMetadata> kvp in Modules.LoadedModulesMetadata)
            {
                if (kvp.Value.IsInstalled)
                {
                    string modulePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), kvp.Key + ".json.zip");
                    kvp.Value.Export(modulePath, true, true, fetchAssets);
                    moduleFiles.Add(modulePath);
                }
            }

            return moduleFiles;
        }
    }
}
