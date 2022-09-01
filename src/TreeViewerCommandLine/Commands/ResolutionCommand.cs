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
using VectSharp;

namespace TreeViewerCommandLine
{
    class ResolutionCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  resolution\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Shows the currently selected resolution for exporting raster images.\n\n", 4),

            new ConsoleTextSpan("  resolution ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("width ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<width>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(" px", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("cm", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("mm", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("in\n", 2, ConsoleColor.Yellow),

            new ConsoleTextSpan("    Sets the ", 4),
            new ConsoleTextSpan("width", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" of the plot, in pixels or in physical units (centimetres, millimetres or inches). If a width in pixels is specified, the DPI are updated as necessary, keeping the physical width unchanged. If a width in physical units is specified, the width in pixels is updated as necessary, keeping the DPI unchanged. The height is always adjusted to keep the correct aspect ratio.\n\n", 4),

            new ConsoleTextSpan("  resolution ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("height ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<height>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan(" px", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("cm", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("mm", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("in\n", 2, ConsoleColor.Yellow),

            new ConsoleTextSpan("    Sets the ", 4),
            new ConsoleTextSpan("height", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" of the plot, in pixels or in physical units (centimetres, millimetres or inches). If a height in pixels is specified, the DPI are updated as necessary, keeping the physical height unchanged. If a height in physical units is specified, the height in pixels is updated as necessary, keeping the DPI unchanged. The width is always adjusted to keep the correct aspect ratio.\n\n", 4),

            new ConsoleTextSpan("  resolution ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("dpi ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<dpi>\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("    Sets the resolution of the plot in ", 4),
            new ConsoleTextSpan("dpi", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(" (dots per inches). The width and height in pixels of the plot are adjusted to keep the physical dimensions unchanged.", 4),

          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Defines and shows information about the resolution used to export raster images.")
        };

        public override string PrimaryCommand => "resolution";



        public override void Execute(string command)
        {
            command = command.Trim();

            if (string.IsNullOrWhiteSpace(command))
            {
                if (Program.TransformedTree != null)
                {
                    if (Program.PlotActions.Count > 0)
                    {
                        Size plotSize = GetPlotSize();

                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("  Plot size:", 2));

                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("     Width: ", 4),
                            new ConsoleTextSpan(Math.Floor(plotSize.Width * Program.Scale).ToString() + " px", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Width * Program.Scale) / Program.DPI * 2.54).ToString("0.##") + " cm", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Width * Program.Scale) / Program.DPI * 25.4).ToString("0.##") + " mm", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Width * Program.Scale) / Program.DPI).ToString("0.##") + " in", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine();

                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("    Height: ", 4),
                            new ConsoleTextSpan(Math.Floor(plotSize.Height * Program.Scale).ToString() + " px", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Height * Program.Scale) / Program.DPI * 2.54).ToString("0.##") + " cm", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Height * Program.Scale) / Program.DPI * 25.4).ToString("0.##") + " mm", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("            ", 4),
                            new ConsoleTextSpan((Math.Floor(plotSize.Height * Program.Scale) / Program.DPI).ToString("0.##") + " in", 12, ConsoleColor.Blue)
                        });
                        ConsoleWrapper.WriteLine();

                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                        {
                            new ConsoleTextSpan("       DPI: ", 4),
                            new ConsoleTextSpan(Program.DPI.ToString("0.##"), 12, ConsoleColor.Blue)
                        });

                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The plot is empty!", ConsoleColor.Red));
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
            else if (command.StartsWith("width", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.TransformedTree != null)
                {
                    if (Program.PlotActions.Count > 0)
                    {
                        string argument = command.Substring(5).Trim(' ', '\t');
                        if (!string.IsNullOrWhiteSpace(argument))
                        {
                            string unit = "";

                            if (argument.Contains("px"))
                            {
                                unit = "px";
                                argument = argument.Replace("px", "").Trim();
                            }
                            else if (argument.Contains("cm"))
                            {
                                unit = "cm";
                                argument = argument.Replace("cm", "").Trim();
                            }
                            else if (argument.Contains("mm"))
                            {
                                unit = "mm";
                                argument = argument.Replace("mm", "").Trim();
                            }
                            else if (argument.Contains("in"))
                            {
                                unit = "in";
                                argument = argument.Replace("in", "").Trim();
                            }

                            if (unit != "")
                            {
                                if (double.TryParse(argument, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value) && value > 0)
                                {
                                    Size plotSize = GetPlotSize();

                                    switch (unit)
                                    {
                                        case "px":
                                            double oldInchWidth = plotSize.Width * Program.Scale / Program.DPI;
                                            Program.Scale = value / plotSize.Width;
                                            Program.DPI = (plotSize.Width * Program.Scale) / oldInchWidth;
                                            break;
                                        case "in":
                                            Program.Scale = value * Program.DPI / plotSize.Width;
                                            break;
                                        case "cm":
                                            Program.Scale = value / 2.54 * Program.DPI / plotSize.Width;
                                            break;
                                        case "mm":
                                            Program.Scale = value / 25.4 * Program.DPI / plotSize.Width;
                                            break;
                                    }
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                                    ConsoleWrapper.WriteLine();
                                }

                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid unit specified!", ConsoleColor.Red));
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The plot is empty!", ConsoleColor.Red));
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
            else if (command.StartsWith("height", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.TransformedTree != null)
                {
                    if (Program.PlotActions.Count > 0)
                    {
                        string argument = command.Substring(6).Trim(' ', '\t');
                        if (!string.IsNullOrWhiteSpace(argument))
                        {
                            string unit = "";

                            if (argument.Contains("px"))
                            {
                                unit = "px";
                                argument = argument.Replace("px", "").Trim();
                            }
                            else if (argument.Contains("cm"))
                            {
                                unit = "cm";
                                argument = argument.Replace("cm", "").Trim();
                            }
                            else if (argument.Contains("mm"))
                            {
                                unit = "mm";
                                argument = argument.Replace("mm", "").Trim();
                            }
                            else if (argument.Contains("in"))
                            {
                                unit = "in";
                                argument = argument.Replace("in", "").Trim();
                            }

                            if (unit != "")
                            {
                                if (double.TryParse(argument, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value) && value > 0)
                                {
                                    Size plotSize = GetPlotSize();

                                    switch (unit)
                                    {
                                        case "px":
                                            double oldInchHeight = plotSize.Height * Program.Scale / Program.DPI;
                                            Program.Scale = value / plotSize.Height;
                                            Program.DPI = (plotSize.Height * Program.Scale) / oldInchHeight;
                                            break;
                                        case "in":
                                            Program.Scale = value * Program.DPI / plotSize.Height;
                                            break;
                                        case "cm":
                                            Program.Scale = value / 2.54 * Program.DPI / plotSize.Height;
                                            break;
                                        case "mm":
                                            Program.Scale = value / 25.4 * Program.DPI / plotSize.Height;
                                            break;
                                    }
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                                    ConsoleWrapper.WriteLine();
                                }

                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid unit specified!", ConsoleColor.Red));
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The plot is empty!", ConsoleColor.Red));
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
            else if (command.StartsWith("dpi", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.TransformedTree != null)
                {
                    if (Program.PlotActions.Count > 0)
                    {
                        string argument = command.Substring(3).Trim(' ', '\t');
                        if (!string.IsNullOrWhiteSpace(argument))
                        {
                            if (double.TryParse(argument, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double value) && value > 0)
                            {
                                double oldScaleFactor = Program.Scale / Program.DPI;
                                Program.DPI = value;
                                Program.Scale = value * oldScaleFactor;
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                                ConsoleWrapper.WriteLine();
                            }

                        }
                        else
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("Invalid value specified!", ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The plot is empty!", ConsoleColor.Red));
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

        private Size GetPlotSize()
        {
            Page pag = new Page(1, 1) { Background = Program.StateData.GraphBackgroundColour };

            if (Program.PlotActions.Count > 0)
            {
                double maxX = double.MinValue;
                double maxY = double.MinValue;
                double minX = double.MaxValue;
                double minY = double.MaxValue;

                for (int i = 0; i < Program.PlotActions.Count; i++)
                {
                    Point[] bounds = Modules.GetModule(Modules.PlottingModules, Program.PlotActions[i].Item1).PlotAction(Program.TransformedTree, Program.PlotActions[i].Item2.Parameters, Program.Coordinates, pag.Graphics);
                    minX = Math.Min(minX, bounds[0].X);
                    maxX = Math.Max(maxX, bounds[1].X);
                    minY = Math.Min(minY, bounds[0].Y);
                    maxY = Math.Max(maxY, bounds[1].Y);
                }

                pag.Crop(new Point(minX - 10, minY - 10), new Size(maxX - minX + 20, maxY - minY + 20));

                if (!string.IsNullOrEmpty(Program.SelectedRegion) && Program.StateData.Tags.TryGetValue("5a8eb0c8-7139-4583-9e9e-375749a98973", out object cropRegionsObject) && cropRegionsObject != null && cropRegionsObject is Dictionary<string, (string, VectSharp.Rectangle)> cropRegions && cropRegions.TryGetValue(Program.SelectedRegion, out (string, VectSharp.Rectangle) selectedRegion))
                {
                    pag = ApplyCrop(pag, selectedRegion.Item2, new Point(minX, minY));
                }
            }

            return new Size(pag.Width, pag.Height);
        }

        private static Page ApplyCrop(Page pag, Rectangle cropRegion, VectSharp.Point origin)
        {
            VectSharp.Point location = cropRegion.Location;
            VectSharp.Size size = cropRegion.Size;

            pag.Crop(new VectSharp.Point(-origin.X + 10 + location.X, -origin.Y + 10 + location.Y), size);

            Page pag2 = new Page(size.Width, size.Height);

            pag2.Graphics.SetClippingPath(0, 0, size.Width, size.Height);

            pag2.Graphics.DrawGraphics(0, 0, pag.Graphics);

            pag2.Background = pag.Background;

            return pag2;
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green) }, "resolution ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("width", ConsoleColor.Yellow) }, "resolution width ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("height", ConsoleColor.Yellow) }, "resolution height ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("dpi", ConsoleColor.Yellow) }, "resolution dpi ");
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

                if (firstWord.Equals("width", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("width", ConsoleColor.Yellow) }, "resolution width ");
                }
                else if (firstWord.Equals("height", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("height", ConsoleColor.Yellow) }, "resolution height ");
                }
                else if (firstWord.Equals("dpi", StringComparison.OrdinalIgnoreCase))
                {
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("dpi", ConsoleColor.Yellow) }, "resolution dpi ");
                }
                else
                {
                    if ("width".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("width", ConsoleColor.Yellow) }, "resolution width ");
                    }

                    if ("height".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("height", ConsoleColor.Yellow) }, "resolution height ");
                    }

                    if ("dpi".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("resolution ", ConsoleColor.Green), new ConsoleTextSpan("dpi", ConsoleColor.Yellow) }, "resolution dpi ");
                    }
                }
            }
        }
    }
}
