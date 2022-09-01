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
using System.Text.RegularExpressions;
using TreeViewer;
using VectSharp;
using VectSharp.Raster.ImageSharp;

namespace TreeViewerCommandLine
{
    class TIFFCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  tiff ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("<file name>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Saves a plot of the tree in TIFF format with the specified ", 4),
            new ConsoleTextSpan("file name", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),

            new ConsoleTextSpan("  tiff ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("stdout\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Writes a plot of the tree in TIFF format to the standard output.\n\n", 4),

            new ConsoleTextSpan("  tiff\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Outputs a plot of the tree in TIFF format to the same destination as the previous ", 4),
            new ConsoleTextSpan("tiff ", 4, ConsoleColor.Green),
            new ConsoleTextSpan("command (default: standard output).", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Plots the tree in TIFF format.")
        };

        public override string PrimaryCommand => "tiff";


        static string lastFileName = null;

        public override void Execute(string command)
        {
            UpdateCommand.UpdateRequired();

            if (string.IsNullOrWhiteSpace(command))
            {
                if (string.IsNullOrEmpty(lastFileName))
                {
                    using (Stream sr = Console.OpenStandardOutput())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            OutputTIFFToStream(ms);

                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(sr);
                        }
                    }
                }
                else
                {
                    using (FileStream stream = new FileStream(lastFileName.Trim('\"', ' '), FileMode.Create))
                    {
                        OutputTIFFToStream(stream);
                    }
                }
            }
            else
            {
                command = command.Trim();

                if (command.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    lastFileName = null;
                    using (Stream sr = Console.OpenStandardOutput())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            OutputTIFFToStream(ms);

                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(sr);
                        }
                    }
                }
                else
                {
                    lastFileName = command;
                    using (FileStream stream = new FileStream(lastFileName.Trim('\"', ' '), FileMode.Create))
                    {
                        OutputTIFFToStream(stream);
                    }
                }
            }
        }

        private void OutputTIFFToStream(Stream sr)
        {
            Page pag = RenderPlotToPage();

            pag.Background = pag.Background.WithAlpha(1.0);

            double scale = Program.Scale;

            SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32> image = pag.SaveAsImage(scale);

            image.Metadata.HorizontalResolution = Program.DPI;
            image.Metadata.VerticalResolution = Program.DPI;

            SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile profile = new SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifProfile();
            profile.SetValue(SixLabors.ImageSharp.Metadata.Profiles.Exif.ExifTag.Software, "TreeViewer");

            image.Frames.RootFrame.Metadata.ExifProfile = profile;

            SixLabors.ImageSharp.ImageExtensions.SaveAsTiff(image, sr, new SixLabors.ImageSharp.Formats.Tiff.TiffEncoder() { Compression = SixLabors.ImageSharp.Formats.Tiff.Constants.TiffCompression.Lzw });

        }

        private Page RenderPlotToPage()
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

            return pag;
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
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("tiff", ConsoleColor.Green) }, "tiff ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("tiff ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "tiff stdout ");

                string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*");

                List<(ConsoleTextSpan[], string)> tbr = new List<(ConsoleTextSpan[], string)>();

                foreach (string sr in directories)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Blue)
                    }, this.PrimaryCommand + " " + Path.GetFileName(sr) + Path.DirectorySeparatorChar));
                }


                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*");

                foreach (string sr in files)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                    }, this.PrimaryCommand + " " + Path.GetFileName(sr) + " "));
                }

                tbr.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                foreach ((ConsoleTextSpan[], string) item in tbr)
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

                if (firstWord.Equals("stdout", StringComparison.OrdinalIgnoreCase))
                {
                    partialCommand = partialCommand.Substring(4).TrimStart();

                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("tiff ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "tiff stdout ");
                }
                else
                {
                    if ("stdout".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("tiff ", ConsoleColor.Green), new ConsoleTextSpan("stdout", ConsoleColor.Yellow) }, "tiff stdout ");
                    }

                    Regex reg = new Regex("^[A-Za-z]:$");
                    if (reg.IsMatch(partialCommand))
                    {
                        partialCommand = partialCommand + "\\";
                    }

                    partialCommand = partialCommand.Trim();
                    string directory = null;

                    directory = Path.GetDirectoryName(partialCommand);


                    if (directory == null)
                    {
                        reg = new Regex("^[A-Za-z]:\\\\$");
                        if (reg.IsMatch(partialCommand))
                        {
                            directory = partialCommand;
                        }
                    }

                    string actualDirectory = directory;

                    if (string.IsNullOrEmpty(actualDirectory))
                    {
                        actualDirectory = Directory.GetCurrentDirectory();
                    }

                    string fileName = Path.GetFileName(partialCommand);

                    string[] directories = Directory.GetDirectories(actualDirectory, fileName + "*");

                    List<(ConsoleTextSpan[], string)> tbr = new List<(ConsoleTextSpan[], string)>();

                    foreach (string sr in directories)
                    {
                        tbr.Add((new ConsoleTextSpan[]
                        {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Blue)
                        }, this.PrimaryCommand + " " + Path.Combine(directory, Path.GetFileName(sr)) + Path.DirectorySeparatorChar));
                    }


                    string[] files = Directory.GetFiles(actualDirectory, fileName + "*");

                    foreach (string sr in files)
                    {
                        tbr.Add((new ConsoleTextSpan[]
                        {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                        }, this.PrimaryCommand + " " + Path.Combine(directory, Path.GetFileName(sr)) + " "));
                    }

                    tbr.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                    foreach ((ConsoleTextSpan[], string) item in tbr)
                    {
                        yield return item;
                    }
                }
            }
        }
    }
}
