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

using Accord.Collections;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TreeViewer;
using VectSharp;
using VectSharp.Raster.ImageSharp;

namespace TreeViewerCommandLine
{
    class DistancesCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  distances ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<metric> ", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Computes a distance matrix using the specified ", 4),
            new ConsoleTextSpan("metric ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("to compute distances between the loaded trees and saves it with the specified ", 4),
            new ConsoleTextSpan("file name", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),

            new ConsoleTextSpan("  distances ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<metric> ", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Computes a distance matrix using the specified ", 4),
            new ConsoleTextSpan("metric ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("to compute distances between the loaded trees and writes it to the standard output.\n\n", 4),

            new ConsoleTextSpan("  distances ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<metric>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Computes a distance matrix using the specified ", 4),
            new ConsoleTextSpan("metric ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("to compute distances between the loaded trees and writes it to the same destination as the previous ", 4),
            new ConsoleTextSpan("distances ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command.\n\n", 4),

            new ConsoleTextSpan("  The available ", 2),
            new ConsoleTextSpan("metrics ", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("are:\n", 2),

            new ConsoleTextSpan("    RF  ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("- Unweighted Robinson-Foulds distances.\n", 10),
            new ConsoleTextSpan("    wRF ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("- Weighted Robinson-Foulds distances.\n", 10),
            new ConsoleTextSpan("    EL  ", 4, ConsoleColor.Yellow),
            new ConsoleTextSpan("- Edge-length distances (only suited for trees with identical topologies).\n", 10),

          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Computes distance matrices between trees.")
        };

        public override string PrimaryCommand => "distances";


        static string lastFileName = null;

        public override void Execute(string command)
        {
            if (Program.Trees == null || Program.Trees.Count == 0)
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No trees have been loaded!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
                return;
            }

            if (Program.Trees.Count == 1)
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Only one tree has been loaded!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
                return;
            }

            if (string.IsNullOrEmpty(command))
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No distance metric selected!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
                return;
            }

            command = command.Trim();
            string distanceType = command;
            
            if (distanceType.Contains(" "))
            {
                distanceType = distanceType.Substring(0, distanceType.IndexOf(" "));
            }

            if (distanceType.Equals("RF", StringComparison.OrdinalIgnoreCase) || distanceType.Equals("wRF", StringComparison.OrdinalIgnoreCase) || distanceType.Equals("EL", StringComparison.OrdinalIgnoreCase))
            {
                string outputFile = command.Substring(distanceType.Length).Trim();

                if (string.IsNullOrEmpty(outputFile))
                {
                    outputFile = lastFileName;
                }

                if ("stdout".Equals(outputFile, StringComparison.OrdinalIgnoreCase))
                {
                    outputFile = null;
                }

                double[,] distanceMatrix = null;

                if (distanceType.Equals("RF", StringComparison.OrdinalIgnoreCase))
                {
                    distanceMatrix = TreeNode.RobinsonFouldsDistances(Program.Trees, false, GlobalSettings.Settings.PairwiseTreeComparisons ? TreeNode.TreeComparisonPruningMode.Pairwise : TreeNode.TreeComparisonPruningMode.Global);
                }
                else if (distanceType.Equals("wRF", StringComparison.OrdinalIgnoreCase))
                {
                    distanceMatrix = TreeNode.RobinsonFouldsDistances(Program.Trees, true, GlobalSettings.Settings.PairwiseTreeComparisons ? TreeNode.TreeComparisonPruningMode.Pairwise : TreeNode.TreeComparisonPruningMode.Global);
                }
                else if (distanceType.Equals("EL", StringComparison.OrdinalIgnoreCase))
                {
                    distanceMatrix = TreeNode.EdgeLengthDistances(Program.Trees, GlobalSettings.Settings.PairwiseTreeComparisons ? TreeNode.TreeComparisonPruningMode.Pairwise : TreeNode.TreeComparisonPruningMode.Global);
                }

                if (string.IsNullOrEmpty(outputFile))
                {
                    using (Stream sr = Console.OpenStandardOutput())
                    {
                        SaveDistanceMatrixToStream(sr, distanceMatrix, Program.Trees);
                    }
                }
                else
                {
                    using (FileStream fs = File.Create(outputFile))
                    {
                        SaveDistanceMatrixToStream(fs, distanceMatrix, Program.Trees);
                    }
                }

                lastFileName = outputFile;
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown distance metric: " + distanceType + "!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
            }
        }

        private static void SaveDistanceMatrixToStream(Stream sr, double[,] distanceMatrix, TreeCollection trees)
        {
            using (StreamWriter sw = new StreamWriter(sr))
            {
                sw.WriteLine(distanceMatrix.GetLength(0).ToString());

                for (int i = 0; i < distanceMatrix.GetLength(0); i++)
                {
                    TreeNode tree = trees[i];

                    if (tree.Attributes.ContainsKey("TreeName"))
                    {
                        sw.Write(tree.Attributes["TreeName"].ToString());
                    }
                    else
                    {
                        sw.Write("Tree" + i.ToString());
                    }

                    for (int j = 0; j < i; j++)
                    {
                        sw.Write("\t" + distanceMatrix[i, j].ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }

                    sw.WriteLine();
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("RF", ConsoleColor.Yellow) }, "distances RF ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("wRF", ConsoleColor.Yellow) }, "distances wRF ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("EL", ConsoleColor.Yellow) }, "distances EL ");
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

                if (firstWord.Equals("RF", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("wRF", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("EL", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(firstWord.Length).TrimStart();

                    if (firstWord.Equals("RF", StringComparison.OrdinalIgnoreCase))
                    {
                        firstWord = "RF";
                    }

                    if (firstWord.Equals("wRF", StringComparison.OrdinalIgnoreCase))
                    {
                        firstWord = "wRF";
                    }

                    if (firstWord.Equals("EL", StringComparison.OrdinalIgnoreCase))
                    {
                        firstWord = "EL";
                    }

                    if (partialCommand.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "distances " + firstWord + " stdout ");
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(partialCommand) || "stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan(firstWord + " stdout", ConsoleColor.Yellow) }, "distances " + firstWord + " stdout ");
                        }

                        foreach ((ConsoleTextSpan[], string) completion in OptionCommand.GetFileCompletion(partialCommand, "distances " + firstWord))
                        {
                            yield return completion;
                        }
                    }
                }
                else
                {
                    if ("RF".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("RF", ConsoleColor.Yellow) }, "distances RF ");
                    }

                    if ("wRF".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("wRF", ConsoleColor.Yellow) }, "distances wRF ");
                    }

                    if ("EL".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("distances ", ConsoleColor.Green), new ConsoleTextSpan("EL", ConsoleColor.Yellow) }, "distances EL ");
                    }
                }
            }
        }
    }
}
