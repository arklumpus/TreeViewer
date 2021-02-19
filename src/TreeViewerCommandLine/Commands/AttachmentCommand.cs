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
using System.Linq;
using System.Text;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class AttachmentCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  attachment ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("add ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<path to file>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Adds a ", 4),
            new ConsoleTextSpan("file", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" as an attachment.\n\n", 4),
            new ConsoleTextSpan("  attachment ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Lists the attachments that are currently loaded.\n\n", 4),
            new ConsoleTextSpan("  attachment ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("info ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<attachment name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Shows information about the specified ", 4),
            new ConsoleTextSpan("attachment", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
            new ConsoleTextSpan("  attachment ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("delete ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<attachment name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Deletes the specified ", 4),
            new ConsoleTextSpan("attachment", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Performs operations on attachments.")
        };

        public override string PrimaryCommand => "attachment";

        public override void Execute(string command)
        {
            if (Program.TransformedTree != null)
            {
                command = command.TrimStart();

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

                if (firstWord.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    if (Program.StateData.Attachments.Count > 0)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Loaded attachments:"));
                        ConsoleWrapper.WriteList(from el in Program.StateData.Attachments select new ConsoleTextSpan[] { new ConsoleTextSpan(el.Key, 4, ConsoleColor.Blue) }, "    ");
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  No attachment has been loaded!"));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    command = command.Substring(4).Trim();

                    if (Program.StateData.Attachments.TryGetValue(command, out Attachment att))
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("  Attachment: ", 2), new ConsoleTextSpan(att.Name, 2, ConsoleColor.Blue) });
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("    Size: ", 4), new ConsoleTextSpan(GetHumanReadableSize(att.StreamLength), 4, ConsoleColor.Cyan) });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("    Store in memory: ", 4), new ConsoleTextSpan(att.StoreInMemory ? "yes" : "no", 4, ConsoleColor.Cyan) });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("    Cache results: ", 4), new ConsoleTextSpan(att.CacheResults ? "yes" : "no", 4, ConsoleColor.Cyan) });
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("The specified attachment ", ConsoleColor.Red), new ConsoleTextSpan(command, ConsoleColor.Blue), new ConsoleTextSpan(" does not exist!", ConsoleColor.Red) });
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    command = command.Substring(6).Trim();

                    if (Program.StateData.Attachments.Remove(command, out Attachment removed))
                    {
                        removed.Dispose();
                        Program.AttachmentList.Remove(removed.Name);
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("The specified attachment ", ConsoleColor.Red), new ConsoleTextSpan(command, ConsoleColor.Blue), new ConsoleTextSpan(" does not exist!", ConsoleColor.Red) });
                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (firstWord == "add")
                {
                    command = command.Substring(3).Trim();
                    if (!string.IsNullOrEmpty(command))
                    {
                        command = Path.GetFullPath(command.Trim().Trim('\"'));
                        if (File.Exists(command))
                        {
                            ConsoleWrapper.WriteLine();

                            string attachmentName = null;
                            bool cancelled = false;

                            while (string.IsNullOrWhiteSpace(attachmentName))
                            {
                                ConsoleWrapper.Write(new ConsoleTextSpan("  Enter attachment name: "));
                                attachmentName = ConsoleWrapper.ReadLine();

                                if (!string.IsNullOrEmpty(attachmentName))
                                {
                                    attachmentName = attachmentName.Replace("\b", "").Replace(";", "_");
                                }

                                if (attachmentName == null)
                                {
                                    cancelled = true;
                                    break;
                                }
                                else if (string.IsNullOrWhiteSpace(attachmentName))
                                {
                                    attachmentName = null;
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Invalid attachment name!", 2, ConsoleColor.Red));
                                    ConsoleWrapper.WriteLine();
                                }
                                else if (Program.StateData.Attachments.ContainsKey(attachmentName))
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("  There is another attachment with the same name!", 2, ConsoleColor.Red));
                                    ConsoleWrapper.WriteLine();
                                    attachmentName = null;
                                }
                            }

                            ConsoleWrapper.WriteLine();

                            if (!cancelled)
                            {
                                ConsoleWrapper.Write(new ConsoleTextSpan("  Should the attachment be stored in memory? [Y(es)/N(o)] ", 2));

                                char key = '?';

                                while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                {
                                    key = ConsoleWrapper.ReadKey(true).KeyChar;
                                }

                                ConsoleWrapper.Write(key);
                                ConsoleWrapper.WriteLine();

                                bool storeInMemory = false;

                                if (key == 'y' || key == 'Y')
                                {
                                    storeInMemory = true;
                                }
                                else if (key == 'n' || key == 'N')
                                {
                                    storeInMemory = false;
                                }

                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.Write(new ConsoleTextSpan("  Should the results of parsing the attachment be cached? [Y(es)/N(o)] ", 2));

                                key = '?';

                                while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                {
                                    key = ConsoleWrapper.ReadKey(true).KeyChar;
                                }

                                ConsoleWrapper.Write(key);
                                ConsoleWrapper.WriteLine();

                                bool cacheResults = false;

                                if (key == 'y' || key == 'Y')
                                {
                                    cacheResults = true;
                                }
                                else if (key == 'n' || key == 'N')
                                {
                                    cacheResults = false;
                                }

                                Program.StateData.Attachments.Add(attachmentName, new Attachment(attachmentName, cacheResults, storeInMemory, command));
                                Program.AttachmentList.Add(attachmentName);
                                Program.AttachmentList.Sort();

                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("The specified file does not exist!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("No file has been specified!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown action: " + command, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }

        private static string GetHumanReadableSize(long size)
        {
            if (size < 1024)
            {
                return size + " B";
            }
            else
            {
                double longSize = size / 1024.0;

                if (longSize < 1024)
                {
                    return longSize.ToString("0.#") + " kiB";
                }
                else
                {
                    longSize /= 1024.0;

                    if (longSize < 1024)
                    {
                        return longSize.ToString("0.#") + " MiB";
                    }
                    else
                    {
                        longSize /= 1024.0;
                        return longSize.ToString("0.#") + " GiB";
                    }
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("add", ConsoleColor.Yellow) }, "attachment add ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("delete", ConsoleColor.Yellow) }, "attachment delete ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "attachment info ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "attachment list ");
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

                if (firstWord.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(3).TrimStart();

                    foreach ((ConsoleTextSpan[], string) item in OptionCommand.GetFileCompletion(partialCommand, "attachment " + firstWord))
                    {
                        yield return item;
                    }
                }
                else if (firstWord.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "attachment list ");
                }
                else if (firstWord.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(6).TrimStart();

                    foreach (KeyValuePair<string, Attachment> att in Program.StateData.Attachments)
                    {
                        if (att.Key.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("delete ", ConsoleColor.Yellow), new ConsoleTextSpan(att.Key + " ", ConsoleColor.Blue) }, "attachment delete " + att.Key + " ");
                        }
                    }
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    foreach (KeyValuePair<string, Attachment> att in Program.StateData.Attachments)
                    {
                        if (att.Key.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("info ", ConsoleColor.Yellow), new ConsoleTextSpan(att.Key + " ", ConsoleColor.Blue) }, "attachment info " + att.Key + " ");
                        }
                    }
                }
                else
                {
                    if ("add".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("add", ConsoleColor.Yellow) }, "attachment add ");
                    }

                    if ("delete".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("delete", ConsoleColor.Yellow) }, "attachment delete ");
                    }

                    if ("info".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "attachment info ");
                    }

                    if ("list".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("attachment ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "attachment list ");
                    }
                }
            }
        }
    }
}
