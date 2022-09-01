/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2022  Giorgio Bianchini, University of Bristol
 
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
    class RegionCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  region ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows a list of the available crop regions.\n\n", 4),

            new ConsoleTextSpan("  region ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<region ID>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<region name>\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("    Selects the specified ", 4),
            new ConsoleTextSpan("crop region", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),

            new ConsoleTextSpan("  region ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("info\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows information about the currently selected crop region.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Selects and shows information about crop regions.")
        };

        public override string PrimaryCommand => "region";



        public override void Execute(string command)
        {
            command = command.Trim();

            if (command.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.TransformedTree != null)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Available crop regions:", 2));
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("    Full plot", 4, ConsoleColor.Yellow));

                    if (Program.StateData.Tags.TryGetValue("5a8eb0c8-7139-4583-9e9e-375749a98973", out object cropRegionsObject) && cropRegionsObject != null && cropRegionsObject is Dictionary<string, (string, VectSharp.Rectangle)> cropRegions)
                    {
                        foreach (KeyValuePair<string, (string, VectSharp.Rectangle)> kvp in cropRegions)
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("    " + kvp.Value.Item1, 4, ConsoleColor.Blue),
                                new ConsoleTextSpan(" - ", 4),
                                new ConsoleTextSpan(kvp.Key, 4, ConsoleColor.Cyan)
                            });
                        }
                    }

                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.Equals("info", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Selected crop region:", 2));

                if (Program.TransformedTree != null)
                {
                    if (Program.SelectedRegion != null)
                    {
                        if (Program.StateData.Tags.TryGetValue("5a8eb0c8-7139-4583-9e9e-375749a98973", out object cropRegionsObject) && cropRegionsObject != null && cropRegionsObject is Dictionary<string, (string, VectSharp.Rectangle)> cropRegions && cropRegions.TryGetValue(Program.SelectedRegion, out (string, VectSharp.Rectangle) selectedRegion))
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("    Name: ", 4),
                                new ConsoleTextSpan(selectedRegion.Item1, 4, ConsoleColor.Blue)
                            });

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("    ID: ", 4),
                                new ConsoleTextSpan(Program.SelectedRegion, 4, ConsoleColor.Cyan)
                            });

                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("    Location:", 4)
                            });

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("      X: ", 6),
                                new ConsoleTextSpan(selectedRegion.Item2.Location.X.ToString(System.Globalization.CultureInfo.InvariantCulture), 6, ConsoleColor.Blue)
                            });

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("      Y: ", 6),
                                new ConsoleTextSpan(selectedRegion.Item2.Location.Y.ToString(System.Globalization.CultureInfo.InvariantCulture), 6, ConsoleColor.Blue)
                            });

                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("    Size:", 4)
                            });

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("      Width: ", 6),
                                new ConsoleTextSpan(selectedRegion.Item2.Size.Width.ToString(System.Globalization.CultureInfo.InvariantCulture), 6, ConsoleColor.Blue)
                            });

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("      Height: ", 6),
                                new ConsoleTextSpan(selectedRegion.Item2.Size.Height.ToString(System.Globalization.CultureInfo.InvariantCulture), 6, ConsoleColor.Blue)
                            });
                        }
                        else
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("    Full plot", 4, ConsoleColor.Yellow));
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("    Full plot", 4, ConsoleColor.Yellow));
                    }

                    ConsoleWrapper.WriteLine();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No tree has been loaded!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.StartsWith("select", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.TransformedTree != null)
                {
                    string argument = command.Substring(6).Trim(' ', '\t');
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        if (argument.Equals("full plot", StringComparison.OrdinalIgnoreCase))
                        {
                            Program.SelectedRegion = null;
                        }
                        else
                        {
                            if (Program.StateData.Tags.TryGetValue("5a8eb0c8-7139-4583-9e9e-375749a98973", out object cropRegionsObject) && cropRegionsObject != null && cropRegionsObject is Dictionary<string, (string, VectSharp.Rectangle)> cropRegions)
                            {
                                bool valid = false;

                                string key = null;

                                if (cropRegions.TryGetValue(argument, out (string, VectSharp.Rectangle) selectedRegion))
                                {
                                    key = argument;
                                    valid = true;
                                }
                                else
                                {
                                    List<KeyValuePair<string, (string, VectSharp.Rectangle)>> matches = new List<KeyValuePair<string, (string, VectSharp.Rectangle)>>();

                                    foreach (KeyValuePair<string, (string, VectSharp.Rectangle)> kvp in cropRegions)
                                    {
                                        if (kvp.Value.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase))
                                        {
                                            matches.Add(kvp);
                                        }
                                    }

                                    if (matches.Count == 1)
                                    {
                                        key = matches[0].Key;
                                        selectedRegion = matches[0].Value;
                                        valid = true;
                                    }
                                    else if (matches.Count == 0)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid crop region!", ConsoleColor.Red));
                                        ConsoleWrapper.WriteLine();
                                    }
                                    else if (matches.Count > 1)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous crop region selection! Please use the crop region ID to univocally specify a crop region!", ConsoleColor.Red));
                                        ConsoleWrapper.WriteLine();
                                    }
                                }

                                if (valid)
                                {
                                    Program.SelectedRegion = key;
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid crop region!", ConsoleColor.Red));
                                    ConsoleWrapper.WriteLine();
                                }
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan("No crop regions have been defined!", ConsoleColor.Red));
                                ConsoleWrapper.WriteLine();
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("You need to specify a region to select!", ConsoleColor.Red));
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
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "region list ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "region select ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "region info ");
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

                if (firstWord.Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "region list ");
                }
                else if (firstWord.Equals("info", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "region info ");
                }
                else if (firstWord.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(6).TrimStart().Trim();

                    List<string> names;
                    List<string> ids;

                    if (Program.TransformedTree != null && Program.StateData.Tags.TryGetValue("5a8eb0c8-7139-4583-9e9e-375749a98973", out object cropRegionsObject) && cropRegionsObject != null && cropRegionsObject is Dictionary<string, (string, VectSharp.Rectangle)> cropRegions)
                    {
                        ids = cropRegions.Keys.ToList();
                        names = (from el in cropRegions.Values select el.Item1).ToList();
                    }
                    else
                    {
                        names = new List<string>();
                        ids = new List<string>();
                    }

                    if (string.IsNullOrWhiteSpace(partialCommand))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("Full plot", ConsoleColor.Yellow) }, "region select Full plot ");

                        foreach (string sr in names)
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "region select " + sr + " ");
                        }

                        foreach (string sr in ids)
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "region select " + sr + " ");
                        }
                    }
                    else
                    {
                        if ("Full plot".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                        {
                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("Full plot", ConsoleColor.Yellow) }, "region select Full plot ");
                        }

                        foreach (string sr in names)
                        {
                            if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "region select " + sr + " ");
                            }
                        }

                        foreach (string sr in ids)
                        {
                            if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "region select " + sr + " ");
                            }
                        }
                    }

                }
                else
                {
                    if ("list".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "region list ");
                    }

                    if ("info".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("info", ConsoleColor.Yellow) }, "region info ");
                    }

                    if ("select".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("region ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "region select ");
                    }
                }
            }
        }
    }
}
