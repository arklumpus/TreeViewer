using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using PhyloTree;
using PhyloTree.Formats;
using TreeViewer;

namespace OpenNexus
{
    /// <summary>
    /// This module adds support for the NEXUS tree file format.
    /// 
    /// To avoid memory overflow issues, the module does not read all the trees from the file into memory at once; instead, it only reads them
    /// one at a time when requested by the chosen Load file module.
    /// 
    /// Trees within `Trees` blocks in the NEXUS file are parsed as NWKA trees (see [this link](https://github.com/arklumpus/TreeNode/blob/master/NWKA.md)
    /// for further information on this format).
    /// 
    /// This module is capable of reading information about TreeViewer modules stored in the NEXUS file alongside in `TreeViewer` blocks. This
    /// can be used to reproduce the tree plot as it was saved.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "NEXUS";
        public const string HelpText = "Opens a file in the NEXUS format, reading the trees in the \"Trees\" blocks and the module information in the \"TreeViewer\" blocks.\nSafe even when opening huge files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "31fdfc2f-1921-432e-bb47-51362dd4fabb";
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public static string[] Extensions { get; } = { "NEXUS files", "nex" };
        public static double IsSupported(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                StringBuilder sb = new StringBuilder();

                int c = sr.Read();

                while (c >= 0 && char.IsWhiteSpace((char)c))
                {
                    c = sr.Read();
                }

                sb.Append((char)c);
                for (int i = 0; i < 5; i++)
                {
                    c = sr.Read();
                    if (c >= 0)
                    {
                        sb.Append((char)c);
                    }
                }

                return sb.ToString().Equals("#NEXUS", StringComparison.OrdinalIgnoreCase) ? 0.5 : 0;
            }
        }

        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                string line = sr.ReadLine();

                List<Attachment> attachments = new List<Attachment>();
                string serializedModules = null;

                while (!sr.EndOfStream)
                {
                    while (!sr.EndOfStream && !line.Trim().Equals("begin treeviewer;", StringComparison.OrdinalIgnoreCase) && !line.Trim().Equals("begin attachment;", StringComparison.OrdinalIgnoreCase))
                    {
                        line = sr.ReadLine();
                    }

                    if (!sr.EndOfStream && line.Trim().Equals("begin treeviewer;", StringComparison.OrdinalIgnoreCase))
                    {
                        line = "";
                        string lengthLine = sr.ReadLine();
                        lengthLine = lengthLine.Replace("Length:", "", StringComparison.OrdinalIgnoreCase).Replace(";", "").Trim();
                        int length = int.Parse(lengthLine);
                        StringBuilder sb = new StringBuilder(length);

                        int read = 0;
                        char[] buffer = new char[1024];

                        while (read < length)
                        {
                            int currRead = sr.Read(buffer, 0, Math.Min(buffer.Length, length - read));
                            read += currRead;
                            sb.Append(buffer, 0, currRead);
                        }

                        serializedModules = sb.ToString();
                    }
                    else if (!sr.EndOfStream && line.Trim().Equals("begin attachment;", StringComparison.OrdinalIgnoreCase))
                    {
                        line = "";
                        string nameLine = sr.ReadLine();
                        nameLine = nameLine.Replace("Name:", "", StringComparison.OrdinalIgnoreCase).Replace(";", "").Trim();

                        string flagLine = sr.ReadLine();
                        flagLine = flagLine.Replace("Flags:", "", StringComparison.OrdinalIgnoreCase).Replace(";", "").Trim().Replace(" ", "");

                        bool storeInMemory = flagLine[0] == '1';
                        bool cacheResults = flagLine[1] == '1';

                        string lengthLine = sr.ReadLine();
                        lengthLine = lengthLine.Replace("Length:", "", StringComparison.OrdinalIgnoreCase).Replace(";", "").Trim();
                        int length = int.Parse(lengthLine);

                        int characterLength = 4 * ((length + 3 - 1) / 3);

                        if (storeInMemory)
                        {
                            int readChars = 0;
                            char[] buffer = new char[characterLength];

                            while (readChars < characterLength)
                            {
                                readChars += sr.ReadBlock(buffer, readChars, characterLength - readChars);
                            }

                            byte[] bytes = Convert.FromBase64CharArray(buffer, 0, characterLength);

                            Stream ms = new MemoryStream(bytes);

                            Attachment att = new Attachment(nameLine, cacheResults, storeInMemory, ref ms);

                            attachments.Add(att);
                        }
                        else
                        {
                            string tempFileName = Path.GetTempFileName();

                            using (FileStream fs = new FileStream(tempFileName, FileMode.Create))
                            {
                                int readChars = 0;
                                char[] buffer = new char[8193];
                                int bufferShift = 0;

                                while (readChars < characterLength)
                                {
                                    int read = sr.ReadBlock(buffer, readChars + bufferShift, Math.Min(8193 - bufferShift, characterLength - readChars)) + bufferShift;

                                    if (read == 8193)
                                    {
                                        byte[] bufferBytes = Convert.FromBase64CharArray(buffer, 0, 8193);
                                        fs.Write(bufferBytes, 0, bufferBytes.Length);
                                        readChars += read;
                                        bufferShift = 0;
                                    }
                                    else
                                    {
                                        if (read % 3 == 0)
                                        {
                                            byte[] bufferBytes = Convert.FromBase64CharArray(buffer, 0, read);
                                            fs.Write(bufferBytes, 0, bufferBytes.Length);
                                            readChars += read;
                                            bufferShift = 0;
                                        }
                                        else
                                        {
                                            byte[] bufferBytes = Convert.FromBase64CharArray(buffer, 0, (read / 3) * 3);
                                            fs.Write(bufferBytes, 0, bufferBytes.Length);

                                            char[] tempBuffer = new char[read - (read / 3) * 3];

                                            for (int i = 0; i < tempBuffer.Length; i++)
                                            {
                                                tempBuffer[i] = buffer[(read / 3) * 3 + i];
                                            }

                                            for (int i = 0; i < tempBuffer.Length; i++)
                                            {
                                                buffer[i] = tempBuffer[i];
                                            }

                                            bufferShift += tempBuffer.Length;
                                            readChars += (read / 3) * 3;
                                        }
                                    }
                                }
                            }

                            Attachment att = new Attachment(nameLine, cacheResults, storeInMemory, tempFileName, true);
                            attachments.Add(att);
                        }
                    }
                }
                
                if (serializedModules != null)
                {
                    List<List<(string, Dictionary<string, object>)>> modules = ModuleUtils.DeserializeModules(serializedModules, attachments, askForCodePermission);
                    if (modules[0].Count > 0)
                    {
                        moduleSuggestions[0] = modules[0][0];
                    }

                    if (modules[2].Count > 0)
                    {
                        moduleSuggestions[1] = modules[2][0];
                    }

                    for (int i = 0; i < modules[1].Count; i++)
                    {
                        moduleSuggestions.Add(modules[1][i]);
                    }

                    for (int i = 0; i < modules[3].Count; i++)
                    {
                        moduleSuggestions.Add(modules[3][i]);
                    }
                }

                if (attachments.Count > 0)
                {
                    foreach (Attachment att in attachments)
                    {
                        moduleSuggestions.Add(("@Attachment", new Dictionary<string, object>() { { "Attachment", att } }));
                    }
                }
            }

            return NEXUS.ParseTrees(fileName, progressAction);
        }
    }
}
