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
using System.IO;
using System.Text;
using PhyloTree;
using PhyloTree.Formats;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class BinaryCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
         {
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("]\n", 2),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the final transformed tree in Binary format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" binary ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, the currently enabled Coordinates and Plot action modules are also exported.\n\n", 4),


            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("loaded stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the loaded tree(s) in Binary format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" binary ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, all the currently enabled modules are also exported.\n\n", 4),

            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("  binary ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("[", 2),
            new ConsoleTextSpan("modules", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("] ", 2),
            new ConsoleTextSpan("transformed stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Outputs the first transformed tree in Binary format to the specified ", 4),
            new ConsoleTextSpan("file ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("or to the standard ", 4),
            new ConsoleTextSpan("output", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(". If a destination is not specified, the same destination as the previous", 4),
            new ConsoleTextSpan(" binary ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command is used (default: standard output). If the optional ", 4),
            new ConsoleTextSpan("modules ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("keyword is specified, the currently enabled Further transformation, Coordinates and Plot action modules are also exported.", 4),
         };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Exports a tree in Binary format.")
        };

        public override string PrimaryCommand => "binary";


        static string lastFileName = null;

        public override void Execute(string command)
        {
            UpdateCommand.UpdateRequired();

            if (string.IsNullOrWhiteSpace(command))
            {
                if (Program.TransformedTree != null)
                {
                    if (string.IsNullOrEmpty(lastFileName))
                    {
                        using (Stream sr = Console.OpenStandardOutput())
                        {
                            ExportToStream(sr, 2, false, false, false);
                        }
                    }
                    else
                    {
                        using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                        {
                            ExportToStream(fs, 2, false, false, false);
                        }
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else
            {
                command = command.Trim();

                StringBuilder firstWordBuilder = new StringBuilder();

                foreach (char c in command)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        firstWordBuilder.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }

                string firstWord = firstWordBuilder.ToString();

                int treeIndex = 2;

                bool modules = false;

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("modules", StringComparison.OrdinalIgnoreCase))
                {
                    modules = true;

                    command = command.Substring(7).TrimStart();

                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

                        foreach (char c in command)
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                firstWordBuilder.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase))
                {
                    treeIndex = 0;

                    command = command.Substring(6).TrimStart();

                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

                        foreach (char c in command)
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                firstWordBuilder.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (!string.IsNullOrWhiteSpace(firstWord) && firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                {
                    treeIndex = 1;

                    command = command.Substring(11).TrimStart();


                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        firstWordBuilder = new StringBuilder();

                        foreach (char c in command)
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                firstWordBuilder.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }

                        firstWord = firstWordBuilder.ToString();
                    }
                    else
                    {
                        firstWord = null;
                    }
                }

                if (string.IsNullOrWhiteSpace(firstWord))
                {
                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    lastFileName = null;
                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    lastFileName = command.Trim('\"');

                    if ((treeIndex == 0 && Program.Trees != null) || (treeIndex == 1 && Program.FirstTransformedTree != null) || (treeIndex == 2 && Program.TransformedTree != null))
                    {
                        if (string.IsNullOrEmpty(lastFileName))
                        {
                            using (Stream sr = Console.OpenStandardOutput())
                            {
                                ExportToStream(sr, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                        else
                        {
                            using (FileStream fs = new FileStream(lastFileName, FileMode.Create))
                            {
                                ExportToStream(fs, treeIndex, modules, modules && NexusCommand.AskIfShouldAddSignature(), NexusCommand.AskIfShouldAddAttachments());
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
            }

        }

        void ExportToStream(Stream stream, int subject, bool modules, bool addSignature, bool includeAttachments)
        {
            if (subject == 0)
            {
                if (modules)
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
                            bw.Write(Program.SerializeAllModules(MainWindow.ModuleTarget.AllModules, addSignature));

                            if (includeAttachments)
                            {
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                        BinaryTree.WriteAllTrees(Program.Trees, stream, additionalDataToCopy: ms);
                    }

                    System.IO.File.Delete(tempFile);
                }
                else
                {
                    if (!includeAttachments)
                    {
                        BinaryTree.WriteAllTrees(Program.Trees, stream);
                    }
                    else
                    {
                        string tempFile = System.IO.Path.GetTempFileName();
                        using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                        {
                            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                            {
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                            BinaryTree.WriteAllTrees(Program.Trees, stream, additionalDataToCopy: ms);
                        }

                        System.IO.File.Delete(tempFile);
                    }
                }
            }
            else if (subject == 1)
            {
                if (modules)
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
                            bw.Write(Program.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, addSignature));

                            if (includeAttachments)
                            {
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                        BinaryTree.WriteAllTrees(new TreeNode[] { Program.FirstTransformedTree }, stream, additionalDataToCopy: ms);
                    }
                    System.IO.File.Delete(tempFile);
                }
                else
                {
                    if (!includeAttachments)
                    {
                        BinaryTree.WriteAllTrees(new TreeNode[] { Program.FirstTransformedTree }, stream);
                    }
                    else
                    {

                        string tempFile = System.IO.Path.GetTempFileName();
                        using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                        {
                            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                            {
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                            BinaryTree.WriteAllTrees(new TreeNode[] { Program.FirstTransformedTree }, stream, additionalDataToCopy: ms);
                        }

                        System.IO.File.Delete(tempFile);
                    }
                }
            }
            else if (subject == 2)
            {
                if (modules)
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
                            bw.Write(Program.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, addSignature));

                            if (includeAttachments)
                            {
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                        BinaryTree.WriteAllTrees(new TreeNode[] { Program.TransformedTree }, stream, additionalDataToCopy: ms);
                    }

                    System.IO.File.Delete(tempFile);
                }
                else
                {
                    if (!includeAttachments)
                    {
                        BinaryTree.WriteAllTrees(new TreeNode[] { Program.TransformedTree }, stream);
                    }
                    else
                    {

                        string tempFile = System.IO.Path.GetTempFileName();
                        using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                        {
                            using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                            {
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write((byte)0);
                                bw.Write("#Attachments");
                                bw.Write(Program.StateData.Attachments.Count);

                                foreach (KeyValuePair<string, Attachment> kvp in Program.StateData.Attachments)
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

                            BinaryTree.WriteAllTrees(new TreeNode[] { Program.TransformedTree }, stream, additionalDataToCopy: ms);
                        }

                        System.IO.File.Delete(tempFile);
                    }
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary", ConsoleColor.Green) }, "binary ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "binary modules ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "binary stdout ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "binary loaded ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "binary transformed ");

                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary"))
                {
                    yield return item;
                }
            }
            else
            {
                partialCommand = partialCommand.TrimStart();

                StringBuilder firstWordBuilder = new StringBuilder();

                foreach (char c in partialCommand)
                {
                    if (!char.IsWhiteSpace(c))
                    {
                        firstWordBuilder.Append(c);
                    }
                    else
                    {
                        break;
                    }
                }

                string firstWord = firstWordBuilder.ToString();

                if (firstWord.Equals("modules", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(7).TrimStart();

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "binary modules ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "binary modules stdout ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules loaded", ConsoleColor.Yellow) }, "binary modules loaded ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules transformed", ConsoleColor.Yellow) }, "binary modules transformed ");

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary modules"))
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        firstWordBuilder = new StringBuilder();

                        foreach (char c in partialCommand)
                        {
                            if (!char.IsWhiteSpace(c))
                            {
                                firstWordBuilder.Append(c);
                            }
                            else
                            {
                                break;
                            }
                        }

                        firstWord = firstWordBuilder.ToString();

                        if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "binary modules stdout ");
                        }
                        else if (firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                        {
                            partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                            if (string.IsNullOrWhiteSpace(partialCommand))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan(firstWord, ConsoleColor.Yellow) }, "binary modules " + firstWord + " ");
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules " + firstWord + " stdout", ConsoleColor.Yellow) }, "binary modules " + firstWord + " stdout ");

                                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary modules " + firstWord))
                                {
                                    yield return item;
                                }
                            }
                            else
                            {
                                if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules " + firstWord + " stdout", ConsoleColor.Yellow) }, "binary modules " + firstWord + " stdout ");
                                }

                                foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary modules " + firstWord))
                                {
                                    yield return item;
                                }
                            }
                        }
                        else
                        {
                            if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules stdout", ConsoleColor.Yellow) }, "binary modules stdout ");
                            }

                            if ("loaded".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules loaded", ConsoleColor.Yellow) }, "binary modules loaded ");
                            }

                            if ("transformed".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules transformed", ConsoleColor.Yellow) }, "binary modules transformed ");
                            }

                            foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary modules"))
                            {
                                yield return item;
                            }
                        }
                    }
                }
                else if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "binary stdout ");
                }
                else if (firstWord.Equals("loaded", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("transformed", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan(firstWord, ConsoleColor.Yellow) }, "binary " + firstWord + " ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "binary " + firstWord + " stdout ");

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary " + firstWord))
                        {
                            yield return item;
                        }
                    }
                    else
                    {
                        if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "binary " + firstWord + " stdout ");
                        }

                        foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary " + firstWord))
                        {
                            yield return item;
                        }
                    }
                }
                else
                {
                    if ("modules".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("modules", ConsoleColor.Yellow) }, "binary modules ");
                    }

                    if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "binary stdout ");
                    }

                    if ("loaded".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("loaded", ConsoleColor.Yellow) }, "binary loaded ");
                    }

                    if ("transformed".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("binary ", ConsoleColor.Green), new ConsoleTextSpan("transformed", ConsoleColor.Yellow) }, "binary transformed ");
                    }

                    foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "binary"))
                    {
                        yield return item;
                    }
                }

            }
        }
    }
}
