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

using Avalonia.Threading;
using PhyloTree;
using PhyloTree.Formats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace TreeViewer
{
    partial class MainWindow
    {
        internal DispatcherTimer AutosaveTimer;

        internal object AutosaveLock = new object();

        private void Autosave(object sender, EventArgs e)
        {
            lock (AutosaveLock)
            {
                if (this.IsTreeOpened)
                {
                    try
                    {
                        string autosavePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "Autosave", DateTime.Now.ToString("yyyy_MM_dd"), WindowGuid);

                        if (!Directory.Exists(autosavePath))
                        {
                            Directory.CreateDirectory(autosavePath);
                        }

                        AutosaveData saveData = new AutosaveData(this.OriginalFileName, DateTime.Now);

                        string autosaveFile = Path.Combine(autosavePath, WindowGuid + ".nex-2");

                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(autosaveFile))
                        {
                            sw.WriteLine("#NEXUS");
                            sw.WriteLine();
                            sw.WriteLine("Begin Trees;");
                            int count = 0;
                            foreach (TreeNode tree in this.Trees)
                            {
                                if (tree.Attributes.ContainsKey("TreeName"))
                                {
                                    sw.Write("\tTree " + tree.Attributes["TreeName"].ToString() + " = ");
                                }
                                else
                                {
                                    sw.Write("\tTree tree" + count.ToString() + " = ");
                                }

                                sw.Write(NWKA.WriteTree(tree, true));
                                sw.WriteLine();
                                count++;
                            }
                            sw.WriteLine("End;");

                            sw.WriteLine();

                            sw.WriteLine("Begin TreeViewer;");
                            string serializedModules = this.SerializeAllModules(MainWindow.ModuleTarget.AllModules, true);
                            sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                            sw.WriteLine(serializedModules);
                            sw.WriteLine("End;");


                            foreach (KeyValuePair<string, Attachment> kvp in this.StateData.Attachments)
                            {
                                sw.WriteLine();

                                sw.WriteLine("Begin Attachment;");

                                sw.WriteLine("\tName: " + kvp.Key + ";");
                                sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                kvp.Value.WriteBase64Encoded(sw);
                                sw.WriteLine();
                                sw.WriteLine("End;");
                            }
                        }

                        if (File.Exists(Path.Combine(autosavePath, WindowGuid + ".nex")))
                        {
                            File.Delete(Path.Combine(autosavePath, WindowGuid + ".nex"));
                        }

                        File.Move(Path.Combine(autosavePath, WindowGuid + ".nex-2"), Path.Combine(autosavePath, WindowGuid + ".nex"), true);

                        File.WriteAllText(Path.Combine(autosavePath, "autosave.json"), System.Text.Json.JsonSerializer.Serialize(saveData, Modules.DefaultSerializationOptions));
                    }
                    catch
                    {

                    }
                }
            }
        }
    }

    internal class AutosaveData
    {
        public string OriginalPath { get; set; }
        public DateTime SaveTime { get; set; }

        public AutosaveData()
        {

        }

        public AutosaveData(string originalPath, DateTime saveTime)
        {
            this.OriginalPath = originalPath;
            this.SaveTime = saveTime;
        }
    }
}
