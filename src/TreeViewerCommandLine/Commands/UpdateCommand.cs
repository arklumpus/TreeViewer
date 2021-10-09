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
using System.Text;
using PhyloTree;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class UpdateCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  update\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Performs any pending updates.\n\n", 4),

            new ConsoleTextSpan("  update ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows the pending updates.\n\n", 4),

            new ConsoleTextSpan("  update ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("all\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Updates everything.\n\n", 4),

            new ConsoleTextSpan("  update ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("transformer\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Updates only the Transformer module.\n", 4),
            new ConsoleTextSpan("    Warning! ", 4, ConsoleColor.Red),
            new ConsoleTextSpan("This action may leave the program in an inconsistent state! Use ", 4),
            new ConsoleTextSpan("update ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("all ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(" to correct this.\n\n", 4),

            new ConsoleTextSpan("  update ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("further transformations\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Updates only the Further transformations module(s).\n", 4),
            new ConsoleTextSpan("    Warning! ", 4, ConsoleColor.Red),
            new ConsoleTextSpan("This action may leave the program in an inconsistent state! Use ", 4),
            new ConsoleTextSpan("update ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("all ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(" to correct this.\n\n", 4),

            new ConsoleTextSpan("  update ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("coordinates\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Updates only the Coordinates module.\n", 4),
            new ConsoleTextSpan("    Warning! ", 4, ConsoleColor.Red),
            new ConsoleTextSpan("This action may leave the program in an inconsistent state! Use ", 4),
            new ConsoleTextSpan("update ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("all ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan(" to correct this.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Updates module states.")
        };

        public override string PrimaryCommand => "update";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                UpdateRequired();
            }
            else
            {
                command = command.Trim();

                if (command.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Pending updates:", 2));
                    ConsoleWrapper.WriteLine();

                    if (PendingUpdates.Transformer)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("                Transformer: ", 4),
                            new ConsoleTextSpan("pending", 4, ConsoleColor.Red)
                        });
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("                Transformer: ", 4),
                            new ConsoleTextSpan("updated", 4, ConsoleColor.Green)
                        });
                    }


                    if (PendingUpdates.FurtherTransformations)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("    Further transformations: ", 4),
                            new ConsoleTextSpan("pending", 4, ConsoleColor.Red)
                        });
                    }
                    else if (PendingUpdates.Transformer)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("    Further transformations: ", 4),
                            new ConsoleTextSpan("required", 4, ConsoleColor.Yellow)
                        });
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("    Further transformations: ", 4),
                            new ConsoleTextSpan("updated", 4, ConsoleColor.Green)
                        });
                    }

                    if (PendingUpdates.Coordinates)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("                Coordinates: ", 4),
                            new ConsoleTextSpan("pending", 4, ConsoleColor.Red)
                        });
                    }
                    else if (PendingUpdates.Transformer || PendingUpdates.FurtherTransformations)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("                Coordinates: ", 4),
                            new ConsoleTextSpan("required", 4, ConsoleColor.Yellow)
                        });
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("                Coordinates: ", 4),
                            new ConsoleTextSpan("updated", 4, ConsoleColor.Green)
                        });
                    }

                    ConsoleWrapper.WriteLine();

                }
                else if (command.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateTransformer();
                    UpdateFurtherTransformations();

                    if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                    {
                        UpdateCoordinates();
                    }
                }
                else if (command.Equals("transformer", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateTransformer();
                }
                else if (command.Equals("further transformations", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateFurtherTransformations();
                }
                else if (command.Equals("coordinates", StringComparison.OrdinalIgnoreCase))
                {
                    UpdateCoordinates();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown action: " + command, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        public static void UpdateRequired()
        {
            if (PendingUpdates.Transformer)
            {
                UpdateTransformer();
                UpdateFurtherTransformations();

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    UpdateCoordinates();
                }
            }
            else if (PendingUpdates.FurtherTransformations)
            {
                UpdateFurtherTransformations();

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    UpdateCoordinates();
                }
            }
            else if (PendingUpdates.Coordinates)
            {
                UpdateCoordinates();
            }
        }

        public static void UpdateTransformer()
        {
            if (!string.IsNullOrEmpty(Program.TransformerModuleId))
            {
                Program.FirstTransformedTree = Modules.GetModule(Modules.TransformerModules, Program.TransformerModuleId).Transform(Program.Trees, Program.TransformerParameters.Parameters, (prog) => { });
                PendingUpdates.Transformer = false;
                PendingUpdates.FurtherTransformations = true;
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No Transformer module enabled!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }


        public static void UpdateFurtherTransformations(int minIndex = 0)
        {
            if (Program.FirstTransformedTree != null)
            {                
                TreeNode[] prevTransformedTrees = Program.AllTransformedTrees;

                if (minIndex > 0)
                {
                    if (minIndex < prevTransformedTrees.Length)
                    {
                        Program.TransformedTree = prevTransformedTrees[minIndex];
                    }
                    else
                    {
                        Program.TransformedTree = Program.TransformedTree.Clone();
                    }
                }
                else
                {
                    Program.TransformedTree = Program.FirstTransformedTree.Clone();
                }


                Program.AllTransformedTrees = new TreeNode[Program.FurtherTransformations.Count];

                for (int i = 0; i < minIndex; i++)
                {
                    Program.AllTransformedTrees[i] = prevTransformedTrees[i];
                }

                List<(string, string)> errors = new List<(string, string)>();

                for (int i = minIndex; i < Program.FurtherTransformations.Count; i++)
                {
                    Program.AllTransformedTrees[i] = Program.TransformedTree.Clone();
                    try
                    {
                        Modules.GetModule(Modules.FurtherTransformationModules, Program.FurtherTransformations[i].Item1).Transform(ref Program.StateData.TransformedTree, Program.FurtherTransformations[i].Item2.Parameters, (a) => { });
                    }
                    catch (Exception ex)
                    {
                        errors.Add((Program.FurtherTransformations[i].Item1, ex.Message));
                    }
                }

                List<TreeNode> nodes = Program.TransformedTree.GetChildrenRecursive();

                HashSet<string> allAttributes = new HashSet<string>();
                foreach (TreeNode node in nodes)
                {
                    foreach (KeyValuePair<string, object> kvp in node.Attributes)
                    {
                        allAttributes.Add(kvp.Key);
                    }
                }

                Program.AttributeList.Clear();
                Program.AttributeList.AddRange(allAttributes);
                Program.AttributeList.Sort();

                if (errors.Count > 0)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Some errors occurred while updating the Further transformations:", ConsoleColor.Red));
                    for (int i = 0; i < errors.Count; i++)
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                        new ConsoleTextSpan("    " + Modules.LoadedModulesMetadata[errors[i].Item1].Name, 4, ConsoleColor.Cyan),
                        new ConsoleTextSpan(": ", 4),
                        new ConsoleTextSpan(errors[i].Item2)
                        });
                    }
                    ConsoleWrapper.WriteLine();
                }

                PendingUpdates.FurtherTransformations = false;

                if (Program.SelectedNode != null)
                {
                    Program.SelectedNode = Program.TransformedTree.GetLastCommonAncestor(Program.SelectedNode.GetNodeNames());
                }

                if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
                {
                    PendingUpdates.Coordinates = true;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(Program.TransformerModuleId))
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No Transformer module enabled!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The Transformer module has never been updated!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        public static void UpdateCoordinates()
        {
            if (!string.IsNullOrEmpty(Program.CoordinatesModuleId))
            {
                Program.Coordinates = Modules.GetModule(Modules.CoordinateModules, Program.CoordinatesModuleId).GetCoordinates(Program.TransformedTree, Program.CoordinatesParameters.Parameters);
                PendingUpdates.Coordinates = false;
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No Coordinates module enabled!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update", ConsoleColor.Green) }, "update ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("all", ConsoleColor.Yellow) }, "update all ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "update list ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("transformer", ConsoleColor.Yellow) }, "update transformer ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("further transformations", ConsoleColor.Yellow) }, "update further transformations ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("coordinates", ConsoleColor.Yellow) }, "update coordinates ");
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

                if (firstWord.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("all", ConsoleColor.Yellow) }, "update all ");
                }
                else if (firstWord.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "update list ");
                }
                else if (firstWord.Equals("transformer", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("transformer", ConsoleColor.Yellow) }, "update transformer ");
                }
                else if (firstWord.Equals("further transformations", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("further transformations", ConsoleColor.Yellow) }, "update further transformations ");
                }
                else if (firstWord.Equals("coordinates", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("coordinates", ConsoleColor.Yellow) }, "update coordinates ");
                }
                else
                {
                    if ("all".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("all", ConsoleColor.Yellow) }, "update all ");
                    }

                    if ("list".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("all", ConsoleColor.Yellow) }, "update list ");
                    }

                    if ("transformer".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("transformer", ConsoleColor.Yellow) }, "update transformer ");
                    }

                    if ("further transformations".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("further transformations", ConsoleColor.Yellow) }, "update further transformations ");
                    }

                    if ("coordinates".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("update ", ConsoleColor.Green), new ConsoleTextSpan("coordinates", ConsoleColor.Yellow) }, "update coordinates ");
                    }
                }
            }
        }
    }
}
