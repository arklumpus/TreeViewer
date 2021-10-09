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

using Avalonia.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class RecentFile
    {
        public string FilePath { get; set; }
        public long ModifiedDate { get; set; }
        public byte[] Preview { get; set; }

        public RecentFile()
        {

        }

        public static RecentFile Create(string filePath, ref RenderTargetBitmap renderedPreview)
        {
            RecentFile tbr = new RecentFile()
            {
                FilePath = filePath,
                ModifiedDate = DateTime.Now.Ticks
            };

            using (MemoryStream ms = new MemoryStream())
            {
                renderedPreview.Save(ms);

                tbr.Preview = ms.ToArray();
            }

            renderedPreview.Dispose();

            return tbr;
        }

        public void Save()
        {
            string guid = Guid.NewGuid().ToString("N");

            string recentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "Recent");

            if (!Directory.Exists(recentDirectory))
            {
                Directory.CreateDirectory(recentDirectory);
            }

            using (FileStream fs = new FileStream(Path.Combine(recentDirectory, guid + ".json"), FileMode.Create))
            using (Utf8JsonWriter writer = new Utf8JsonWriter(fs))
            {
                JsonSerializer.Serialize(writer, this);
            }
        }

        public static RecentFile Load(string path)
        {
            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<RecentFile>(json);
        }

        public static IEnumerable<(string, RecentFile)> GetRecentFiles()
        {
            string recentDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetEntryAssembly().GetName().Name, "Recent");

            if (Directory.Exists(recentDirectory))
            {
                string[] recentFiles = Directory.GetFiles(recentDirectory, "*.json");

                Dictionary<string, (string, RecentFile)> fileData = new Dictionary<string, (string, RecentFile)>();

                for (int i = 0; i < recentFiles.Length; i++)
                {
                    RecentFile file = Load(recentFiles[i]);

                    if ((DateTime.Now - new DateTime(file.ModifiedDate)).TotalDays > GlobalSettings.Settings.KeepRecentFilesFor || !File.Exists(file.FilePath))
                    {
                        try
                        {
                            File.Delete(recentFiles[i]);
                        }
                        catch { }
                    }
                    else
                    {
                        if (!fileData.TryGetValue(file.FilePath, out (string, RecentFile) previousFile))
                        {
                            fileData.Add(file.FilePath, (recentFiles[i], file));
                        }
                        else
                        {
                            if (file.ModifiedDate > previousFile.Item2.ModifiedDate)
                            {
                                fileData[file.FilePath] = (recentFiles[i], file);

                                try
                                {
                                    File.Delete(previousFile.Item1);
                                }
                                catch { }
                            }
                            else
                            {
                                try
                                {
                                    File.Delete(recentFiles[i]);
                                }
                                catch { }
                            }
                        }
                    }
                }

                return from el in fileData orderby el.Value.Item2.ModifiedDate descending select el.Value;
            }
            else
            {
                return Enumerable.Empty<(string, RecentFile)>();
            }
        }
    }
}
