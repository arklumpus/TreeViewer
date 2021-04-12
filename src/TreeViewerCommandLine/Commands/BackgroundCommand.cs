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
using VectSharp;
using TreeViewer;

namespace TreeViewerCommandLine
{
    class BackgroundCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  background\n", 2, ConsoleColor.Green),
            new ConsoleTextSpan("    Displays the current background colour.\n\n", 4),

            new ConsoleTextSpan("  background ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("#RRGGBB\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("  background ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("#RRGGBBAA\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("  background ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("rgb(r, g, b)\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("  background ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("rgba(r, g, b, a)\n", 2, ConsoleColor.Blue),

            new ConsoleTextSpan("    Sets the ", 4),
            new ConsoleTextSpan("background colour", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Displays or sets the background colour.")
        };

        public override string PrimaryCommand => "background";

        public override void Execute(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                    new ConsoleTextSpan("  Background colour:\n", 2),
                    new ConsoleTextSpan("    #" + ((int)(Program.StateData.GraphBackgroundColour.R * 255)).ToString("X2") +((int)(Program.StateData.GraphBackgroundColour.G * 255)).ToString("X2")+((int)(Program.StateData.GraphBackgroundColour.B * 255)).ToString("X2")+ (Program.StateData.GraphBackgroundColour.A != 1 ? ((int)(Program.StateData.GraphBackgroundColour.A * 255)).ToString("X2") : "")+ "\n", 4, ConsoleColor.Blue ),
                    new ConsoleTextSpan("    R: ", 4),
                    new ConsoleTextSpan(((int)(Program.StateData.GraphBackgroundColour.R * 255)).ToString(), 4, ConsoleColor.Blue),
                    new ConsoleTextSpan("    G: ", 4),
                    new ConsoleTextSpan(((int)(Program.StateData.GraphBackgroundColour.G * 255)).ToString(), 4, ConsoleColor.Blue),
                    new ConsoleTextSpan("    B: ", 4),
                    new ConsoleTextSpan(((int)(Program.StateData.GraphBackgroundColour.B * 255)).ToString(), 4, ConsoleColor.Blue),
                    new ConsoleTextSpan("    A: ", 4),
                    new ConsoleTextSpan(((int)(Program.StateData.GraphBackgroundColour.A * 255)).ToString(), 4, ConsoleColor.Blue)
                });
                ConsoleWrapper.WriteLine();
            }
            else
            {
                string argument = command.Trim();
                Colour? parsed = null;

                try
                {
                    if (argument.StartsWith("#"))
                    {
                        string r = argument.Substring(1, 2);
                        string g = argument.Substring(3, 2);
                        string b = argument.Substring(5, 2);

                        string a = "ff";

                        if (argument.Length > 7)
                        {
                            a = argument.Substring(7, 2);
                        }

                        parsed = Colour.FromRgba(Convert.ToInt32(r, 16) / 255.0, Convert.ToInt32(g, 16) / 255.0, Convert.ToInt32(b, 16) / 255.0, Convert.ToInt32(a, 16) / 255.0);
                    }
                    else if (argument.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
                    {
                        argument = argument.Substring(4).Trim(')');
                        int[] values = (from el in argument.Split(',') select int.Parse(el.Trim())).ToArray();

                        parsed = Colour.FromRgb(values[0], values[1], values[2]);
                    }
                    else if (argument.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
                    {
                        argument = argument.Substring(5).Trim(')');
                        int[] values = (from el in argument.Split(',') select int.Parse(el.Trim())).ToArray();

                        parsed = Colour.FromRgba((values[0], values[1], values[2], values[3] / 255.0));
                    }
                }
                catch
                {
                    parsed = null;
                }

                if (parsed != null)
                {
                    Program.StateData.GraphBackgroundColour = parsed.Value;
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid background colour!", ConsoleColor.Red) });
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        public override IEnumerable<(ConsoleTextSpan[], string)> GetCompletions(string partialCommand)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("background", ConsoleColor.Green) }, "background ");
            }
        }
    }
}
