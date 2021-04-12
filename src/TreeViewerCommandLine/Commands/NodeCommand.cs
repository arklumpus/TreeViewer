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
using System.Text;
using PhyloTree;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class NodeCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  node ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("\"Name 1\"\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("  node ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("\"Name 1\" \"Name 2\" …\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("    Selects the LCA of the specified ", 4),
            new ConsoleTextSpan("node(s)", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(". Node names need to be enclosed in double quotes!\n\n", 4),

            new ConsoleTextSpan("  node ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("root\n", 2, ConsoleColor.Yellow),

            new ConsoleTextSpan("    Selects the root node.\n\n", 4),

            new ConsoleTextSpan("  node ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<node #>\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("    Selects the specified ", 4),
            new ConsoleTextSpan("node ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("(the root node is ", 4),
            new ConsoleTextSpan("#0", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(").\n\n", 4),

            new ConsoleTextSpan("  node ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("info\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows information about the currently selected node.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Selects and shows information about nodes.")
        };

        public override string PrimaryCommand => "node";



        public override void Execute(string command)
        {
            command = command.Trim();

            if (command.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.SelectedNode != null)
                {
                    ConsoleWrapper.WriteLine();

                    if (Program.SelectedNode.Children.Count > 0)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  LCA of:\n", 2));
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Leaf node:\n", 2));
                    }

                    List<string> childNodeNames = Program.SelectedNode.GetNodeNames();

                    ConsoleWrapper.WriteList(from el in childNodeNames select new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + el + "\"", 4, ConsoleColor.Blue) }, "    ");

                    if (Program.SelectedNode.Children.Count > 0)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Descendants:\n", 2));

                        List<TreeNode> nodes = Program.TransformedTree.GetChildrenRecursive();
                        ConsoleWrapper.WriteList(from el in Enumerable.Range(0, nodes.Count) where nodes[el].Parent == Program.SelectedNode select new ConsoleTextSpan[] { new ConsoleTextSpan("#" + el.ToString(), 4, ConsoleColor.Blue) }, "    ");
                    }
                   

                    List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                    {
                        new ConsoleTextSpan("    Direct descendants: ", 4),
                        new ConsoleTextSpan(Program.SelectedNode.Children.Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                        new ConsoleTextSpan("    Total descendants: ", 4),
                        new ConsoleTextSpan(Program.SelectedNode.GetChildrenRecursiveLazy().Count().ToString() + "\n", 4, ConsoleColor.Cyan),
                        new ConsoleTextSpan("    Descendant leaves: ", 4),
                        new ConsoleTextSpan(Program.SelectedNode.GetLeaves().Count.ToString() + "\n", 4, ConsoleColor.Cyan)
                    };

                    if (childNodeNames.Count > 0 && Program.SelectedNode.Children.Count > 0)
                    {
                        message.AddRange(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("    Defining children: ", 4),
                            new ConsoleTextSpan("\"" + childNodeNames[0] + "\" \"" + childNodeNames[^1] + "\"\n", 4, ConsoleColor.Cyan)
                        });
                    }

                    ConsoleWrapper.WriteLine();

                    if (Program.SelectedNode.Children.Count > 0)
                    {
                        ConsoleWrapper.WriteLine(message);
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No node has been selected!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.StartsWith("select"))
            {
                if (Program.TransformedTree != null)
                {
                    string argument = command.Substring(6).Trim(' ', '\t');
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        bool valid = false;

                        if (argument == "root")
                        {
                            valid = true;
                            Program.SelectedNode = Program.TransformedTree;

                            List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                            {
                                new ConsoleTextSpan("  Selected root node\n", 2),
                                new ConsoleTextSpan("    Direct descendants: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.Children.Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                                new ConsoleTextSpan("    Total descendants: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.GetChildrenRecursiveLazy().Count().ToString() + "\n", 4, ConsoleColor.Cyan),
                                new ConsoleTextSpan("    Descendant leaves: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.GetLeaves().Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                            };

                            List<string> childNodeNames = Program.SelectedNode.GetNodeNames();

                            if (childNodeNames.Count > 0)
                            {
                                message.AddRange(new ConsoleTextSpan[]
                                {
                                    new ConsoleTextSpan("    Defining children: ", 4),
                                    new ConsoleTextSpan("\"" + childNodeNames[0] + "\" \"" + childNodeNames[^1] + "\"\n", 4, ConsoleColor.Cyan)
                                });
                            }

                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(message);
                        }
                        else if (argument.StartsWith("#"))
                        {
                            int index;
                            if (int.TryParse(argument.Substring(1).Trim(), out index))
                            {
                                int count = 0;
                                bool found = false;

                                foreach (TreeNode node in Program.TransformedTree.GetChildrenRecursiveLazy())
                                {
                                    if (count == index)
                                    {
                                        Program.SelectedNode = node;
                                        found = true;
                                        break;
                                    }
                                    count++;
                                }

                                if (found)
                                {
                                    valid = true;

                                    List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                    {
                                        new ConsoleTextSpan("  Selected node #" + index.ToString() + "\n", 2),
                                        new ConsoleTextSpan("    Direct descendants: ", 4),
                                        new ConsoleTextSpan(Program.SelectedNode.Children.Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                                        new ConsoleTextSpan("    Total descendants: ", 4),
                                        new ConsoleTextSpan(Program.SelectedNode.GetChildrenRecursiveLazy().Count().ToString() + "\n", 4, ConsoleColor.Cyan),
                                        new ConsoleTextSpan("    Descendant leaves: ", 4),
                                        new ConsoleTextSpan(Program.SelectedNode.GetLeaves().Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                                    };

                                    List<string> childNodeNames = Program.SelectedNode.GetNodeNames();

                                    if (childNodeNames.Count > 0)
                                    {
                                        message.AddRange(new ConsoleTextSpan[]
                                        {
                                    new ConsoleTextSpan("    Defining children: ", 4),
                                    new ConsoleTextSpan("\"" + childNodeNames[0] + "\" \"" + childNodeNames[^1] + "\"\n", 4, ConsoleColor.Cyan)
                                        });
                                    }

                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(message);
                                }
                            }
                            else
                            {
                                valid = false;
                            }
                        }
                        else
                        {
                            List<string> names = new List<string>();

                            int position = 0;

                            StringBuilder currentNameBuilder = new StringBuilder();

                            while (position < argument.Length)
                            {
                                while (position < argument.Length && argument[position] != '\"')
                                {
                                    position++;
                                }

                                position++;

                                while (position < argument.Length && argument[position] != '\"')
                                {
                                    currentNameBuilder.Append(argument[position]);
                                    position++;
                                }

                                position++;

                                if (currentNameBuilder.Length > 0)
                                {
                                    names.Add(currentNameBuilder.ToString());
                                    currentNameBuilder.Clear();
                                }
                            }


                            if (names.Count > 0)
                            {
                                TreeNode referenceTree = Program.TransformedTree;

                                List<string> actualNames = referenceTree.GetNodeNames();

                                List<string> fixedNames = new List<string>();

                                bool missedMatch = false;

                                for (int i = 0; i < names.Count; i++)
                                {
                                    string[] matches = (from el in actualNames where el.Equals(names[i], StringComparison.OrdinalIgnoreCase) select el).ToArray();

                                    if (matches.Length == 0)
                                    {
                                        missedMatch = true;
                                        break;
                                    }
                                    else if (matches.Length == 1)
                                    {
                                        fixedNames.Add(matches[0]);
                                    }
                                    else
                                    {
                                        int index = actualNames.IndexOf(names[i]);
                                        if (index >= 0)
                                        {
                                            fixedNames.Add(actualNames[index]);
                                        }
                                        else
                                        {
                                            missedMatch = true;
                                            break;
                                        }
                                    }
                                }


                                if (!missedMatch)
                                {
                                    valid = true;
                                    Program.SelectedNode = referenceTree.GetLastCommonAncestor(fixedNames);

                                    List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                            {
                                new ConsoleTextSpan("  Selected LCA of " + fixedNames.Count + " nodes\n", 2),
                                new ConsoleTextSpan("    Direct descendants: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.Children.Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                                new ConsoleTextSpan("    Total descendants: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.GetChildrenRecursiveLazy().Count().ToString() + "\n", 4, ConsoleColor.Cyan),
                                new ConsoleTextSpan("    Descendant leaves: ", 4),
                                new ConsoleTextSpan(Program.SelectedNode.GetLeaves().Count.ToString() + "\n", 4, ConsoleColor.Cyan),
                            };

                                    List<string> childNodeNames = Program.SelectedNode.GetNodeNames();

                                    if (childNodeNames.Count > 0)
                                    {
                                        message.AddRange(new ConsoleTextSpan[]
                                        {
                                    new ConsoleTextSpan("    Defining children: ", 4),
                                    new ConsoleTextSpan("\"" + childNodeNames[0] + "\" \"" + childNodeNames[^1] + "\"\n", 4, ConsoleColor.Cyan)
                                        });
                                    }

                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(message);
                                }
                            }
                        }

                        if (!valid)
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid node selection!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }

                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("You need to specify a node to select!", ConsoleColor.Red));
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
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown action: " + command, ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("node ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "node select ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("node ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "node info ");
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

                if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("node ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "node info ");
                }
                else if (firstWord.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(6).TrimStart().Trim();

                    TreeNode referenceTree = Program.TransformedTree;

                    List<string> names;

                    if (referenceTree != null)
                    {
                        names = referenceTree.GetNodeNames();
                    }
                    else
                    {
                        names = new List<string>();
                    }

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("root", ConsoleColor.Yellow) }, "node select root ");
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("#", ConsoleColor.Blue) }, "node select #");
                        foreach (string sr in names)
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "node select \"" + sr + "\" ");
                        }
                    }
                    else if (partialCommand.Contains("\""))
                    {
                        int count = partialCommand.Length - partialCommand.Replace("\"", "").Length;

                        string prevCommand = partialCommand.Substring(0, partialCommand.LastIndexOf("\"") + 1);

                        partialCommand = partialCommand.Substring(partialCommand.LastIndexOf("\"") + 1).TrimStart();

                        if (count % 2 == 0)
                        {
                            if (string.IsNullOrWhiteSpace(partialCommand))
                            {
                                foreach (string sr in names)
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "node select " + prevCommand + " \"" + sr + "\" ");
                                }
                            }
                            else
                            {
                                yield break;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(partialCommand))
                            {
                                foreach (string sr in names)
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "node select " + prevCommand + sr + "\" ");
                                }
                            }
                            else
                            {
                                foreach (string sr in names)
                                {
                                    if (sr.StartsWith(partialCommand))
                                    {
                                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "node select " + prevCommand + sr + "\" ");
                                    }
                                }
                            }
                        }

                    }
                    else
                    {
                        if (partialCommand == "#")
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("#", ConsoleColor.Blue) }, "node select #");
                        }
                        else if ("root".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("root", ConsoleColor.Yellow) }, "node select root ");
                        }

                        yield break;
                    }
                }
                else
                {
                    if ("info".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("node ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "node info ");
                    }

                    if ("select".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("node ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "node select ");
                    }
                }
            }
        }
    }
}
