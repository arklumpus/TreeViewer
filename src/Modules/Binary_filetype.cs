using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using PhyloTree;
using PhyloTree.Formats;
using TreeViewer;

namespace OpenBinaryTree
{
    /// <summary>
    /// This module adds support for the Binary tree file format (see [this link](https://github.com/arklumpus/TreeNode/blob/master/BinaryTree.md)
    /// for further information on this format).
    /// 
    /// To avoid memory overflow issues, the module does not read all the trees from the file into memory at once; instead, it only reads them
    /// one at a time when requested by the chosen Load file module.
    /// 
    /// This module is capable of reading information about TreeViewer modules stored in the tree file alongside the tree(s). This can be used
    /// to reproduce the tree plot as it was saved.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Binary tree";
        public const string HelpText = "Opens a file containing one or more trees in binary format.\nSafe even when opening huge files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8ccec519-3d83-4617-824f-dd474c88bdea";
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public static string[] Extensions { get; } = { "Binary tree files", "tbi" };
        public static double IsSupported(string fileName)
        {
            using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                return BinaryTree.IsValidStream(fs) ? 0.8 : 0;
            }
        }

        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (BinaryTree.HasValidTrailer(fs, true))
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(fs, System.Text.Encoding.UTF8, true))
                    {
                        fs.Seek(-12, SeekOrigin.End);
                        long labelAddress = reader.ReadInt64();
                        fs.Seek(labelAddress - 8, SeekOrigin.Begin);
                        long dataLength = reader.ReadInt64();
                        if (dataLength > 11)
                        {
                            fs.Seek(labelAddress - 8 - dataLength, SeekOrigin.Begin);
                            string header = reader.ReadString();
                            if (header == "#TreeViewer")
                            {
                                string serializedModules = reader.ReadString();

                                List<List<(string, Dictionary<string, object>)>> modules = ModuleUtils.DeserializeModules(serializedModules, askForCodePermission);

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

                                if (reader.BaseStream.Position - (labelAddress - 8 - dataLength) < dataLength)
                                {
                                    try
                                    {
                                        header = reader.ReadString();
                                    }
                                    catch { }
                                }
                            }


                            if (header == "#Attachments")
                            {
                                int attachmentCount = reader.ReadInt32();

                                for (int i = 0; i < attachmentCount; i++)
                                {
                                    Attachment att = ReadAttachment(reader, fileName);

                                    moduleSuggestions.Add(("@Attachment", new Dictionary<string, object>() { { "Attachment", att } }));
                                }
                            }
                        }
                    }
                }
                catch { }

                fs.Seek(0, SeekOrigin.Begin);

                return new TreeCollection(fs);
            }
            else
            {
                return BinaryTree.ParseTrees(fs, progressAction: progressAction);
            }
        }

        private static Attachment ReadAttachment(BinaryReader reader, string fileName)
        {
            string name = reader.ReadString();

            int flagCount = reader.ReadInt32();

            bool storeInMemory = false;
            bool cacheResults = false;

            for (int i = 0; i < flagCount; i++)
            {
                bool flag = reader.ReadBoolean();

                if (i == 0)
                {
                    storeInMemory = flag;
                }
                else if (i == 1)
                {
                    cacheResults = flag;
                }
            }

            int length = reader.ReadInt32();

            if (storeInMemory)
            {
                byte[] data = reader.ReadBytes(length);

                Stream ms = new MemoryStream(data);

                return new Attachment(name, cacheResults, storeInMemory, ref ms);
            }
            else
            {
                long streamStart = reader.BaseStream.Position;

                return new Attachment(name, cacheResults, fileName, streamStart, length);
            }
        }
    }
}
