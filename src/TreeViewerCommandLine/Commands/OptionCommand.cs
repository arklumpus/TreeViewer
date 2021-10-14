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
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace TreeViewerCommandLine
{
    class OptionCommand : Command
    {
        public override ConsoleTextSpan[] HelpText => new ConsoleTextSpan[]
          {
            new ConsoleTextSpan("  option ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("list\n", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("    Shows the options that are available for the currently selected module.\n\n", 4),
            new ConsoleTextSpan("  option ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("select ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<option name>", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("|", 2),
            new ConsoleTextSpan("<option #>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Selects the specified ", 4),
            new ConsoleTextSpan("option", 4, ConsoleColor.Blue),
            new ConsoleTextSpan(".\n\n", 4),
            new ConsoleTextSpan("  option ", 2, ConsoleColor.Green),
            new ConsoleTextSpan("set ", 2, ConsoleColor.Yellow),
            new ConsoleTextSpan("<value>\n", 2, ConsoleColor.Blue),
            new ConsoleTextSpan("    Sets the ", 4),
            new ConsoleTextSpan("value ", 4, ConsoleColor.Blue),
            new ConsoleTextSpan("of the currently selected option.", 4),
          };

        public override ConsoleTextSpan[] ShortHelpText => new ConsoleTextSpan[]
        {
            new ConsoleTextSpan("Shows and changes option values.")
        };

        public override string PrimaryCommand => "option";



        public override void Execute(string command)
        {
            command = command.Trim();

            if (command.Equals("list", StringComparison.OrdinalIgnoreCase))
            {
                if (Program.SelectedModuleParameters != null)
                {
                    Program.SelectedModuleParameters.PrintParameters();
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No module has been selected!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.StartsWith("select"))
            {
                if (Program.SelectedModuleParameters != null)
                {
                    string argument = command.Substring(6).Trim(' ', '\t', ':');

                    List<(string, string)> availableOptions = new List<(string, string)>();

                    for (int i = 0; i < Program.SelectedModuleParameters.ParameterKeys.Count; i++)
                    {
                        availableOptions.Add(("#" + (i + 1), Program.SelectedModuleParameters.ParameterKeys[i]));
                        availableOptions.Add((Program.SelectedModuleParameters.ParameterKeys[i].Trim(' ', '\t', ':'), Program.SelectedModuleParameters.ParameterKeys[i]));
                    }

                    string[] selectedOptions = (from el in availableOptions where el.Item1.Equals(argument, StringComparison.OrdinalIgnoreCase) select el.Item2).ToArray();

                    if (selectedOptions.Length == 0)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Unknown option: " + argument, ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else if (selectedOptions.Length > 1)
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("Ambiguous option selection! Please use the option # to univocally specify an option!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                    else
                    {
                        Program.SelectedOption = selectedOptions[0];

                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] {
                        new ConsoleTextSpan("  Selected option ", 2),
                        new ConsoleTextSpan("#" + (Program.SelectedModuleParameters.ParameterKeys.IndexOf(selectedOptions[0]) + 1).ToString(), 2, ConsoleColor.Yellow),
                        new ConsoleTextSpan(" " + selectedOptions[0].Trim(' ','\t',':'), 2, ConsoleColor.Cyan)
                    });
                        ConsoleWrapper.WriteLine();


                        if (!Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0].StartsWith(ConsoleWrapper.Whitespace))
                        {
                            ConsoleWrapper.WriteLine("  Possible values:");
                            ConsoleWrapper.WriteList(from el in Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]] select new ConsoleTextSpan[] { new ConsoleTextSpan(el, 4, ConsoleColor.Blue) }, "    ");
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "NumericUpDown")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Range: ", 2),
                                new ConsoleTextSpan(double.Parse(Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][1]).ToString(Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][3]), 2, ConsoleColor.Blue),
                                new ConsoleTextSpan(" - ", 2),
                                new ConsoleTextSpan(double.Parse(Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][2]).ToString(Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][3]), 2, ConsoleColor.Blue),
                            });
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Colour")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Colour syntax:\n", 2),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("#RRGGBB", 4, ConsoleColor.Blue),
                                new ConsoleTextSpan(" (hex RGB syntax)\n", 4),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("#RRGGBBAA", 4, ConsoleColor.Blue),
                                new ConsoleTextSpan(" (hex RGBA syntax)\n", 4),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("rgb(r, g, b)", 4, ConsoleColor.Blue),
                                new ConsoleTextSpan(" (0 ≤ r, g, b ≤ 255)\n", 4),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("rgb(r, g, b, a)", 4, ConsoleColor.Blue),
                                new ConsoleTextSpan(" (0 ≤ r, g, b, a ≤ 255)", 4),
                            });
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Point")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Point syntax:\n", 2),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("X, Y\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("X Y\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("X\tY", 4, ConsoleColor.Blue)
                            });
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Dash")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Dash syntax:\n", 2),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("UnitsOn, UnitsOff, Phase\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("UnitsOn UnitsOff Phase\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    • ", 4),
                                new ConsoleTextSpan("UnitsOn\tUnitsOff\tPhase\n\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("  For a continuous line, ", 2),
                                new ConsoleTextSpan("UnitsOn", 2, ConsoleColor.Blue),
                                new ConsoleTextSpan(", ", 2),
                                new ConsoleTextSpan("UnitsOff", 2, ConsoleColor.Blue),
                                new ConsoleTextSpan(" and ", 2),
                                new ConsoleTextSpan("Phase", 2, ConsoleColor.Blue),
                                new ConsoleTextSpan(" should all be ", 2),
                                new ConsoleTextSpan("0", 2, ConsoleColor.Blue),
                                new ConsoleTextSpan(".", 2),
                            });
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Font")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Font syntax: ", 2),
                                new ConsoleTextSpan("FontFamily Size\n\n", 2, ConsoleColor.Blue),

                                new ConsoleTextSpan("  Available font families:", 2),
                            });
                            ConsoleWrapper.WriteList(from el in VectSharp.FontFamily.StandardFamilies select new ConsoleTextSpan[] { new ConsoleTextSpan(el, 4, ConsoleColor.Blue) }, "    ");
                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "ColourByNode")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Colour by node syntax:\n", 2),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("#RRGGBB\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("#RRGGBBAA\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("rgb(r, g, b)\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("rgba(r, g, b, a)\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Sets the ", 6),
                                new ConsoleTextSpan("default colour", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set attribute\n", 4, ConsoleColor.Yellow),

                                new ConsoleTextSpan("      Shows the current values for the attribute ", 6),
                                new ConsoleTextSpan("type", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" and ", 6),
                                new ConsoleTextSpan("name", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set attribute ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<attribute type> <attribute name>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Sets the attribute ", 6),
                                new ConsoleTextSpan("type", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" and ", 6),
                                new ConsoleTextSpan("name", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(". The attribute type must be either ", 6),
                                new ConsoleTextSpan("string", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" or ", 6),
                                new ConsoleTextSpan("number", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source\n", 4, ConsoleColor.Yellow),

                                new ConsoleTextSpan("      Opens an external text editor (", 6),
                                new ConsoleTextSpan(Utils.GetNanoName(), 6, ConsoleColor.Green),
                                new ConsoleTextSpan(") to edit the ", 6),
                                new ConsoleTextSpan("source code", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<file name>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Loads the ", 6),
                                new ConsoleTextSpan("source code ", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan("from the specified ", 6),
                                new ConsoleTextSpan("file", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".", 6),
                            });

                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Formatter" || Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "SourceCode" || Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Markdown")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Formatter syntax:\n", 2),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source\n", 4, ConsoleColor.Yellow),

                                new ConsoleTextSpan("      Opens an external text editor (", 6),
                                new ConsoleTextSpan(Utils.GetNanoName(), 6, ConsoleColor.Green),
                                new ConsoleTextSpan(") to edit the ", 6),
                                new ConsoleTextSpan("source code", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<file name>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Loads the ", 6),
                                new ConsoleTextSpan("source code ", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan("from the specified ", 6),
                                new ConsoleTextSpan("file", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".", 6),
                            });

                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "Node")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Current value:\n", 2),

                                new ConsoleTextSpan("    LCA of:\n", 4, ConsoleColor.Blue)
                            });

                            ConsoleWrapper.WriteList(from el in (string[])Program.SelectedModuleParameters.Parameters[Program.SelectedOption] select new ConsoleTextSpan[] { new ConsoleTextSpan(el, 6, ConsoleColor.Blue) }, "      ");

                            ConsoleWrapper.WriteLine();

                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Node syntax:\n", 2),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("\"Name 1\"\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("\"Name 1\" \"Name 2\" …\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Chooses the LCA of the specified ", 6),
                                new ConsoleTextSpan("node(s)", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(". Node names need to be enclosed in double quotes!", 6)

                            });

                            ConsoleWrapper.WriteLine();

                            ConsoleWrapper.WriteLine();
                        }
                        else if (Program.SelectedModuleParameters.PossibleValues[selectedOptions[0]][0] == ConsoleWrapper.Whitespace + "NumericUpDownByNode")
                        {
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                            {
                                new ConsoleTextSpan("  Number by node syntax:\n", 2),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<value>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Sets the ", 6),
                                new ConsoleTextSpan("default value", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set attribute\n", 4, ConsoleColor.Yellow),

                                new ConsoleTextSpan("      Shows the current values for the attribute ", 6),
                                new ConsoleTextSpan("type", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" and ", 6),
                                new ConsoleTextSpan("name", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set attribute ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<attribute type> <attribute name>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Sets the attribute ", 6),
                                new ConsoleTextSpan("type", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" and ", 6),
                                new ConsoleTextSpan("name", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(". The attribute type must be either ", 6),
                                new ConsoleTextSpan("string", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(" or ", 6),
                                new ConsoleTextSpan("number", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source\n", 4, ConsoleColor.Yellow),

                                new ConsoleTextSpan("      Opens an external text editor (", 6),
                                new ConsoleTextSpan(Utils.GetNanoName(), 6, ConsoleColor.Green),
                                new ConsoleTextSpan(") to edit the ", 6),
                                new ConsoleTextSpan("source code", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".\n\n", 6),

                                new ConsoleTextSpan("    option ", 4, ConsoleColor.Green),
                                new ConsoleTextSpan("set source ", 4, ConsoleColor.Yellow),
                                new ConsoleTextSpan("<file name>\n", 4, ConsoleColor.Blue),

                                new ConsoleTextSpan("      Loads the ", 6),
                                new ConsoleTextSpan("source code ", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan("from the specified ", 6),
                                new ConsoleTextSpan("file", 6, ConsoleColor.Blue),
                                new ConsoleTextSpan(".", 6),
                            });

                            ConsoleWrapper.WriteLine();
                        }
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No module has been selected!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
            else if (command.StartsWith("set"))
            {
                string argument = command.Substring(3).Trim(' ', '\t');

                if (!string.IsNullOrWhiteSpace(Program.SelectedOption))
                {
                    if (!string.IsNullOrWhiteSpace(argument))
                    {
                        if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "ComboBox")
                        {
                            string[] matches = (from el in Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption] where el.Equals(argument, StringComparison.OrdinalIgnoreCase) select el).ToArray();

                            if (matches.Length == 0)
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                            else if (matches.Length > 1)
                            {
                                int index = Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption].IndexOf(argument);

                                if (index >= 0)
                                {
                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, index } });
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                    ConsoleWrapper.WriteLine();
                                }
                            }
                            else
                            {
                                int index = Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption].IndexOf(matches[0]);
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, index } });
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "TextBox")
                        {
                            if (argument.StartsWith("\"") && argument.EndsWith("\""))
                            {
                                argument = argument.Substring(1, argument.Length - 2);
                            }

                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, argument } });
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "CheckBox")
                        {
                            try
                            {
                                bool value = Convert.ToBoolean(argument);
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, value } });
                            }
                            catch
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "NumericUpDown")
                        {
                            if (double.TryParse(argument, out double parsed) && parsed >= double.Parse(Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][1]) && parsed <= double.Parse(Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][2]))
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, parsed } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Colour")
                        {
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
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, parsed.Value } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Point")
                        {
                            argument = argument.Replace(",", " ").Replace("\t", " ").Trim();
                            while (argument.Contains("  "))
                            {
                                argument = argument.Replace("  ", " ");
                            }

                            string[] splitArgument = argument.Split(' ');

                            if (double.TryParse(splitArgument[0], out double parsedX) && double.TryParse(splitArgument[1], out double parsedY))
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, new Point(parsedX, parsedY) } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Dash")
                        {
                            argument = argument.Replace(",", " ").Replace("\t", " ").Trim();
                            while (argument.Contains("  "))
                            {
                                argument = argument.Replace("  ", " ");
                            }

                            string[] splitArgument = argument.Split(' ');

                            if (double.TryParse(splitArgument[0], out double parsedUnitsOn) && double.TryParse(splitArgument[1], out double parsedUnitsOff) && double.TryParse(splitArgument[2], out double parsedPhase))
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, new LineDash(parsedUnitsOn, parsedUnitsOff, parsedPhase) } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Node")
                        {
                            bool valid = false;

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

                                for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                                {
                                    if (Program.FurtherTransformations[i].Item2 == Program.SelectedModuleParameters)
                                    {
                                        if (i > 0 && i < Program.AllTransformedTrees.Length + 1)
                                        {
                                            referenceTree = Program.AllTransformedTrees[i - 1];
                                        }
                                        else if (i == 0)
                                        {
                                            referenceTree = Program.FirstTransformedTree;
                                        }
                                    }
                                }

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
                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, fixedNames.ToArray() } });
                                }
                            }

                            if (!valid)
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Font")
                        {
                            Font fnt = null;
                            try
                            {
                                fnt = Extensions.FromFontString(argument);
                            }
                            catch
                            {
                                fnt = null;
                            }

                            if (fnt != null)
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, fnt } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "ColourByNode")
                        {
                            StringBuilder firstWordBuilder = new StringBuilder();

                            foreach (char c in argument)
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

                            bool valid = false;

                            if (firstWord.Equals("attribute", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    argument = argument.Substring(9).Trim();

                                    if (string.IsNullOrWhiteSpace(argument))
                                    {
                                        ColourFormatterOptions prevValue = (ColourFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                        ConsoleWrapper.WriteLine();
                                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                                        {
                                            new ConsoleTextSpan("  Current attribute:\n", 2),
                                            new ConsoleTextSpan("    Type:  ", 4),
                                            new ConsoleTextSpan(prevValue.AttributeType + "\n", 4, ConsoleColor.Blue),
                                            new ConsoleTextSpan("    Name:  ", 4),
                                            new ConsoleTextSpan(prevValue.AttributeName + "\n", 4, ConsoleColor.Blue),
                                        });

                                        valid = true;
                                    }
                                    else
                                    {
                                        string attributeType = argument.Substring(0, argument.IndexOf(" "));

                                        if (attributeType.Equals("string", StringComparison.OrdinalIgnoreCase))
                                        {
                                            attributeType = "String";
                                            valid = true;
                                        }
                                        else if (attributeType.Equals("number", StringComparison.OrdinalIgnoreCase))
                                        {
                                            attributeType = "Number";
                                            valid = true;
                                        }
                                        else
                                        {
                                            valid = false;
                                        }

                                        if (valid)
                                        {
                                            string attributeName = argument.Substring(argument.IndexOf(" ") + 1);

                                            ColourFormatterOptions prevValue = (ColourFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                            ColourFormatterOptions options;

                                            if (attributeType == prevValue.AttributeType)
                                            {
                                                options = new ColourFormatterOptions(prevValue.Formatter) { AttributeName = attributeName, AttributeType = attributeType, Parameters = prevValue.Parameters, DefaultColour = prevValue.DefaultColour };
                                            }
                                            else
                                            {
                                                options = new ColourFormatterOptions(attributeType == "String" ? Modules.DefaultAttributeConvertersToColourCompiled[0].Formatter : Modules.DefaultAttributeConvertersToColourCompiled[1].Formatter) { AttributeName = attributeName, AttributeType = attributeType, Parameters = prevValue.Parameters, DefaultColour = prevValue.DefaultColour };
                                                options.Parameters[0] = attributeType == "String" ? Modules.DefaultAttributeConvertersToColour[0] : Modules.DefaultAttributeConvertersToColour[1];
                                                options.Parameters[^1] = true;
                                            }

                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                        }
                                    }
                                }
                                catch
                                {
                                    valid = false;
                                }

                            }
                            else if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                            {
                                argument = argument.Substring(6).Trim();

                                if (string.IsNullOrWhiteSpace(argument))
                                {
                                    valid = true;

                                    ColourFormatterOptions prevValue = (ColourFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                    string newSourceCode = (string)prevValue.Parameters[0];

                                    bool success = false;

                                    while (!success)
                                    {
                                        string tempFileName = Path.GetTempFileName();

                                        File.WriteAllText(tempFileName, newSourceCode);

                                        Utils.RunNano(tempFileName);

                                        newSourceCode = File.ReadAllText(tempFileName);

                                        File.Delete(tempFileName);

                                        try
                                        {
                                            ColourFormatterOptions options = new ColourFormatterOptions(newSourceCode, prevValue.Parameters) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, DefaultColour = prevValue.DefaultColour };
                                            options.Parameters[0] = newSourceCode;
                                            options.Parameters[^1] = false;
                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });

                                            success = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleWrapper.WriteLine();
                                            List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                            message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                            ConsoleWrapper.WriteLine(message);

                                            ConsoleWrapper.Write(new ConsoleTextSpan("Do you wish to edit the source code and retry? [Y(es)/N(o)] ", ConsoleColor.Yellow));

                                            char key = '?';

                                            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                            {
                                                key = ConsoleWrapper.ReadKey(true).KeyChar;
                                            }

                                            ConsoleWrapper.Write(key);
                                            ConsoleWrapper.WriteLine();

                                            if (key == 'y' || key == 'Y')
                                            {
                                                success = false;
                                            }
                                            else if (key == 'n' || key == 'N')
                                            {
                                                success = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    ColourFormatterOptions prevValue = (ColourFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    string newSourceCode = File.ReadAllText(argument.Trim('\"'));

                                    valid = true;

                                    try
                                    {
                                        ColourFormatterOptions options = new ColourFormatterOptions(newSourceCode, prevValue.Parameters) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, DefaultColour = prevValue.DefaultColour };
                                        options.Parameters[0] = newSourceCode;
                                        options.Parameters[^1] = false;
                                        Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                        message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                        ConsoleWrapper.WriteLine(message);
                                    }
                                }
                            }
                            else
                            {
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

                                if (parsed == null)
                                {
                                    valid = false;
                                }
                                else
                                {
                                    valid = true;
                                    ColourFormatterOptions prevValue = (ColourFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    ColourFormatterOptions options = new ColourFormatterOptions(prevValue.Formatter) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, Parameters = prevValue.Parameters, DefaultColour = (Colour)parsed };

                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                }
                            }

                            if (!valid)
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "AttributeType" || Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "AttributeSelector")
                        {
                            int index = Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption].IndexOf(argument, StringComparison.OrdinalIgnoreCase);

                            if (index >= 0)
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][index] } });
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Attachment")
                        {
                            if (argument == "(None)")
                            {
                                Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, null } });
                            }
                            else
                            {
                                int index = Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption].IndexOf(argument, StringComparison.OrdinalIgnoreCase);

                                if (index >= 0)
                                {
                                    string attachmentName = Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][index];
                                    if (Program.StateData.Attachments.TryGetValue(attachmentName, out Attachment att))
                                    {
                                        Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, att } });
                                    }
                                    else
                                    {
                                        ConsoleWrapper.WriteLine();
                                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                        ConsoleWrapper.WriteLine();
                                    }
                                }
                                else
                                {
                                    ConsoleWrapper.WriteLine();
                                    ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                    ConsoleWrapper.WriteLine();
                                }
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Formatter")
                        {
                            StringBuilder firstWordBuilder = new StringBuilder();

                            foreach (char c in argument)
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

                            if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                            {
                                argument = argument.Substring(6).Trim();

                                if (string.IsNullOrWhiteSpace(argument))
                                {
                                    FormatterOptions prevValue = (FormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                    string newSourceCode = (string)prevValue.Parameters[^2];

                                    bool success = false;

                                    while (!success)
                                    {

                                        string tempFileName = Path.GetTempFileName();

                                        File.WriteAllText(tempFileName, newSourceCode);

                                        Utils.RunNano(tempFileName);

                                        newSourceCode = File.ReadAllText(tempFileName);

                                        File.Delete(tempFileName);

                                        try
                                        {
                                            FormatterOptions options = new FormatterOptions(newSourceCode) { Parameters = prevValue.Parameters };
                                            options.Parameters[^2] = newSourceCode;
                                            options.Parameters[^1] = false;
                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });

                                            success = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleWrapper.WriteLine();
                                            List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                            message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                            ConsoleWrapper.WriteLine(message);


                                            ConsoleWrapper.Write(new ConsoleTextSpan("Do you wish to edit the source code and retry? [Y(es)/N(o)] ", ConsoleColor.Yellow));

                                            char key = '?';

                                            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                            {
                                                key = ConsoleWrapper.ReadKey(true).KeyChar;
                                            }

                                            ConsoleWrapper.Write(key);
                                            ConsoleWrapper.WriteLine();

                                            if (key == 'y' || key == 'Y')
                                            {
                                                success = false;
                                            }
                                            else if (key == 'n' || key == 'N')
                                            {
                                                success = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    FormatterOptions prevValue = (FormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    string newSourceCode = File.ReadAllText(argument.Trim('\"'));

                                    try
                                    {

                                        FormatterOptions options = new FormatterOptions(newSourceCode) { Parameters = prevValue.Parameters };
                                        options.Parameters[^2] = newSourceCode;
                                        options.Parameters[^1] = false;
                                        Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                        message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                        ConsoleWrapper.WriteLine(message);
                                    }
                                }
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "SourceCode")
                        {
                            StringBuilder firstWordBuilder = new StringBuilder();

                            foreach (char c in argument)
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

                            if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                            {
                                argument = argument.Substring(6).Trim();

                                if (string.IsNullOrWhiteSpace(argument))
                                {
                                    CompiledCode prevValue = (CompiledCode)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                    string newSourceCode = (string)prevValue.SourceCode;

                                    bool success = false;

                                    while (!success)
                                    {

                                        string tempFileName = Path.GetTempFileName();

                                        File.WriteAllText(tempFileName, newSourceCode);

                                        Utils.RunNano(tempFileName);

                                        newSourceCode = File.ReadAllText(tempFileName);

                                        File.Delete(tempFileName);

                                        try
                                        {
                                            CompiledCode code = new CompiledCode(newSourceCode);
                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, code } });

                                            success = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleWrapper.WriteLine();
                                            List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                            message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                            ConsoleWrapper.WriteLine(message);


                                            ConsoleWrapper.Write(new ConsoleTextSpan("Do you wish to edit the source code and retry? [Y(es)/N(o)] ", ConsoleColor.Yellow));

                                            char key = '?';

                                            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                            {
                                                key = ConsoleWrapper.ReadKey(true).KeyChar;
                                            }

                                            ConsoleWrapper.Write(key);
                                            ConsoleWrapper.WriteLine();

                                            if (key == 'y' || key == 'Y')
                                            {
                                                success = false;
                                            }
                                            else if (key == 'n' || key == 'N')
                                            {
                                                success = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    CompiledCode prevValue = (CompiledCode)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    string newSourceCode = File.ReadAllText(argument.Trim('\"'));

                                    try
                                    {
                                        CompiledCode code = new CompiledCode(newSourceCode);
                                        Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, code } });
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                        message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                        ConsoleWrapper.WriteLine(message);
                                    }
                                }
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "Markdown")
                        {
                            StringBuilder firstWordBuilder = new StringBuilder();

                            foreach (char c in argument)
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

                            if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                            {
                                argument = argument.Substring(6).Trim();

                                if (string.IsNullOrWhiteSpace(argument))
                                {
                                    string prevValue = (string)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                    string newSourceCode = prevValue;

                                    string tempFileName = Path.GetTempFileName();

                                    File.WriteAllText(tempFileName, newSourceCode);

                                    Utils.RunNano(tempFileName);

                                    newSourceCode = File.ReadAllText(tempFileName);

                                    File.Delete(tempFileName);

                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, newSourceCode } });
                                }
                                else
                                {
                                    string prevValue = (string)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    string newSourceCode = File.ReadAllText(argument.Trim('\"'));

                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, newSourceCode } });
                                }
                            }
                            else
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }
                        else if (Program.SelectedModuleParameters.ControlTypes[Program.SelectedOption] == "NumericUpDownByNode")
                        {
                            StringBuilder firstWordBuilder = new StringBuilder();

                            foreach (char c in argument)
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

                            bool valid = false;

                            if (firstWord.Equals("attribute", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    argument = argument.Substring(9).Trim();

                                    if (string.IsNullOrWhiteSpace(argument))
                                    {
                                        NumberFormatterOptions prevValue = (NumberFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                        ConsoleWrapper.WriteLine();
                                        ConsoleWrapper.WriteLine(new ConsoleTextSpan[]
                                        {
                                            new ConsoleTextSpan("  Current attribute:\n", 2),
                                            new ConsoleTextSpan("    Type:  ", 4),
                                            new ConsoleTextSpan(prevValue.AttributeType + "\n", 4, ConsoleColor.Blue),
                                            new ConsoleTextSpan("    Name:  ", 4),
                                            new ConsoleTextSpan(prevValue.AttributeName + "\n", 4, ConsoleColor.Blue),
                                        });

                                        valid = true;
                                    }
                                    else
                                    {
                                        string attributeType = argument.Substring(0, argument.IndexOf(" "));

                                        if (attributeType.Equals("string", StringComparison.OrdinalIgnoreCase))
                                        {
                                            attributeType = "String";
                                            valid = true;
                                        }
                                        else if (attributeType.Equals("number", StringComparison.OrdinalIgnoreCase))
                                        {
                                            attributeType = "Number";
                                            valid = true;
                                        }
                                        else
                                        {
                                            valid = false;
                                        }

                                        if (valid)
                                        {
                                            string attributeName = argument.Substring(argument.IndexOf(" ") + 1);

                                            NumberFormatterOptions prevValue = (NumberFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                            NumberFormatterOptions options;

                                            if (attributeType == prevValue.AttributeType)
                                            {
                                                options = new NumberFormatterOptions(prevValue.Formatter) { AttributeName = attributeName, AttributeType = attributeType, Parameters = prevValue.Parameters, DefaultValue = prevValue.DefaultValue };
                                            }
                                            else
                                            {
                                                options = new NumberFormatterOptions(attributeType == "String" ? Modules.DefaultAttributeConvertersToDouble[0] : Modules.DefaultAttributeConvertersToDouble[1]) { AttributeName = attributeName, AttributeType = attributeType, Parameters = prevValue.Parameters, DefaultValue = prevValue.DefaultValue };
                                                options.Parameters[0] = attributeType == "String" ? Modules.DefaultAttributeConvertersToDouble[0] : Modules.DefaultAttributeConvertersToDouble[1];
                                                options.Parameters[^1] = true;
                                            }

                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                        }
                                    }
                                }
                                catch
                                {
                                    valid = false;
                                }

                            }
                            else if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                            {
                                argument = argument.Substring(6).Trim();

                                if (string.IsNullOrWhiteSpace(argument))
                                {
                                    valid = true;

                                    NumberFormatterOptions prevValue = (NumberFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];

                                    string newSourceCode = (string)prevValue.Parameters[0];

                                    bool success = false;

                                    while (!success)
                                    {

                                        string tempFileName = Path.GetTempFileName();

                                        File.WriteAllText(tempFileName, newSourceCode);

                                        Utils.RunNano(tempFileName);

                                        newSourceCode = File.ReadAllText(tempFileName);

                                        File.Delete(tempFileName);

                                        try
                                        {
                                            NumberFormatterOptions options = new NumberFormatterOptions(newSourceCode) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, Parameters = prevValue.Parameters, DefaultValue = prevValue.DefaultValue };
                                            options.Parameters[0] = newSourceCode;
                                            options.Parameters[^1] = false;
                                            Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });

                                            success = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            ConsoleWrapper.WriteLine();
                                            List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                            message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                            ConsoleWrapper.WriteLine(message);


                                            ConsoleWrapper.Write(new ConsoleTextSpan("Do you wish to edit the source code and retry? [Y(es)/N(o)] ", ConsoleColor.Yellow));

                                            char key = '?';

                                            while (key != 'y' && key != 'Y' && key != 'n' && key != 'N')
                                            {
                                                key = ConsoleWrapper.ReadKey(true).KeyChar;
                                            }

                                            ConsoleWrapper.Write(key);
                                            ConsoleWrapper.WriteLine();

                                            if (key == 'y' || key == 'Y')
                                            {
                                                success = false;
                                            }
                                            else if (key == 'n' || key == 'N')
                                            {
                                                success = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    NumberFormatterOptions prevValue = (NumberFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    string newSourceCode = File.ReadAllText(argument.Trim('\"'));

                                    valid = true;

                                    try
                                    {
                                        NumberFormatterOptions options = new NumberFormatterOptions(newSourceCode) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, Parameters = prevValue.Parameters, DefaultValue = prevValue.DefaultValue };
                                        options.Parameters[0] = newSourceCode;
                                        options.Parameters[^1] = false;
                                        Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                    }
                                    catch (Exception ex)
                                    {
                                        ConsoleWrapper.WriteLine();
                                        List<ConsoleTextSpan> message = new List<ConsoleTextSpan>()
                                            {
                                                new ConsoleTextSpan("  Compilation error!\n", 2, ConsoleColor.Red),
                                            };

                                        message.AddRange(from el in ex.Message.Split('\n') select new ConsoleTextSpan("    " + el + "\n", 4, ConsoleColor.Red));

                                        ConsoleWrapper.WriteLine(message);
                                    }
                                }
                            }
                            else
                            {
                                if (double.TryParse(argument, out double parsed))
                                {
                                    valid = true;
                                    NumberFormatterOptions prevValue = (NumberFormatterOptions)Program.SelectedModuleParameters.Parameters[Program.SelectedOption];
                                    NumberFormatterOptions options = new NumberFormatterOptions(prevValue.Formatter) { AttributeName = prevValue.AttributeName, AttributeType = prevValue.AttributeType, Parameters = prevValue.Parameters, DefaultValue = parsed };

                                    Program.SelectedModuleParameters.UpdateParameterAction(new Dictionary<string, object>() { { Program.SelectedOption, options } });
                                }
                                else
                                {
                                    valid = false;
                                }
                            }

                            if (!valid)
                            {
                                ConsoleWrapper.WriteLine();
                                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan(argument, ConsoleColor.Blue), new ConsoleTextSpan(" is not a valid value for option ", ConsoleColor.Red), new ConsoleTextSpan(Program.SelectedOption.Trim(' ', '\t', ':'), ConsoleColor.Cyan), new ConsoleTextSpan("!", ConsoleColor.Red) });
                                ConsoleWrapper.WriteLine();
                            }
                        }

                        if (Program.SelectedModuleParameters == Program.TransformerParameters)
                        {
                            PendingUpdates.Transformer = true;
                        }
                        else if (Program.SelectedModuleParameters == Program.CoordinatesParameters)
                        {
                            PendingUpdates.Coordinates = true;
                        }
                        else
                        {
                            foreach ((string, ModuleParametersContainer) item in Program.FurtherTransformations)
                            {
                                if (item.Item2 == Program.SelectedModuleParameters)
                                {
                                    PendingUpdates.FurtherTransformations = true;
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("You need to specify a value for the option!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("No option has been selected!", ConsoleColor.Red));
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
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "option list ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "option select ");
                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set", ConsoleColor.Yellow) }, "option set ");
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
                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "option list ");
                }
                else if (firstWord.Equals("select", StringComparison.OrdinalIgnoreCase))
                {
                    if (Program.SelectedModuleParameters != null)
                    {
                        partialCommand = partialCommand.Substring(6).TrimStart().Trim(' ', '\t', ':');

                        SortedSet<string> availableOptions = new SortedSet<string>(Comparer<string>.Create((a, b) =>
                        {
                            if (a.StartsWith("#") && !b.StartsWith("#"))
                            {
                                return 1;
                            }
                            else if (!a.StartsWith("#") && b.StartsWith("#"))
                            {
                                return -1;
                            }
                            else
                            {
                                return a.CompareTo(b);
                            }
                        }));

                        for (int i = 0; i < Program.SelectedModuleParameters.ParameterKeys.Count; i++)
                        {
                            availableOptions.Add("#" + (i + 1));
                            availableOptions.Add(Program.SelectedModuleParameters.ParameterKeys[i].Trim(' ', '\t', ':'));
                        }

                        foreach (string sr in availableOptions)
                        {
                            if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                            {
                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "option select " + sr + " ");
                            }
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
                else if (firstWord.Equals("set", StringComparison.OrdinalIgnoreCase))
                {
                    if (Program.SelectedModuleParameters != null)
                    {
                        partialCommand = partialCommand.Substring(3).TrimStart().Trim();

                        if (!string.IsNullOrEmpty(Program.SelectedOption))
                        {
                            if (!Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0].StartsWith(ConsoleWrapper.Whitespace))
                            {
                                foreach (string sr in Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption])
                                {
                                    if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                                    {
                                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "option set " + sr + " ");
                                    }
                                }
                            }
                            else if (Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "Font")
                            {
                                foreach (string sr in FontFamily.StandardFamilies)
                                {
                                    if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                                    {
                                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "option set " + sr + " ");
                                    }
                                }
                            }
                            else if (Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "Node")
                            {
                                TreeNode referenceTree = Program.TransformedTree;

                                for (int i = 0; i < Program.FurtherTransformations.Count; i++)
                                {
                                    if (Program.FurtherTransformations[i].Item2 == Program.SelectedModuleParameters)
                                    {
                                        if (i > 0 && i < Program.AllTransformedTrees.Length + 1)
                                        {
                                            referenceTree = Program.AllTransformedTrees[i - 1];
                                        }
                                        else if (i == 0)
                                        {
                                            referenceTree = Program.FirstTransformedTree;
                                        }
                                    }
                                }

                                List<string> names = referenceTree.GetNodeNames();

                                if (string.IsNullOrWhiteSpace(partialCommand))
                                {
                                    foreach (string sr in names)
                                    {
                                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "option set \"" + sr + "\" ");
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
                                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "option set " + prevCommand + " \"" + sr + "\" ");
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
                                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "option set " + prevCommand + sr + "\" ");
                                            }
                                        }
                                        else
                                        {
                                            foreach (string sr in names)
                                            {
                                                if (sr.StartsWith(partialCommand))
                                                {
                                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("\"" + sr + "\"", ConsoleColor.Blue) }, "option set " + prevCommand + sr + "\" ");
                                                }
                                            }
                                        }
                                    }

                                }
                                else
                                {
                                    yield break;
                                }

                            }
                            else if (Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "ColourByNode" || Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "NumericUpDownByNode")
                            {
                                if (string.IsNullOrWhiteSpace(partialCommand))
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set attribute", ConsoleColor.Yellow) }, "option set attribute ");
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
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

                                    if (firstWord.Equals("attribute", StringComparison.OrdinalIgnoreCase))
                                    {
                                        partialCommand = partialCommand.Substring(9).TrimStart();

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

                                        if (firstWord.Equals("string", StringComparison.OrdinalIgnoreCase) || firstWord.Equals("number", StringComparison.OrdinalIgnoreCase))
                                        {
                                            partialCommand = partialCommand.Substring(6).TrimStart();

                                            if (string.IsNullOrWhiteSpace(partialCommand))
                                            {
                                                foreach (string sr in Program.AttributeList)
                                                {
                                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "option set attribute " + firstWord + " " + sr);
                                                }
                                            }
                                            else
                                            {
                                                foreach (string sr in Program.AttributeList)
                                                {
                                                    if (sr.StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                                                    {
                                                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan(sr, ConsoleColor.Blue) }, "option set attribute " + firstWord + " " + sr);
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {

                                            if ("number".StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                                            {
                                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("number", ConsoleColor.Blue) }, "option set attribute number ");
                                            }

                                            if ("string".StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                                            {
                                                yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("string", ConsoleColor.Blue) }, "option set attribute string ");
                                            }

                                        }
                                    }
                                    else if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                                    {
                                        partialCommand = partialCommand.Substring(6).TrimStart();

                                        if (string.IsNullOrWhiteSpace(partialCommand))
                                        {
                                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
                                        }

                                        foreach ((ConsoleTextSpan[], string) item in GetFileCompletion(partialCommand, "option set source"))
                                        {
                                            yield return item;
                                        }

                                    }
                                    else
                                    {
                                        if ("attribute".StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                                        {
                                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set attribute", ConsoleColor.Yellow) }, "option set attribute ");
                                        }

                                        if ("source".StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                                        {
                                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
                                        }
                                    }
                                }
                            }
                            else if (Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "Formatter" || Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "SourceCode" || Program.SelectedModuleParameters.PossibleValues[Program.SelectedOption][0] == ConsoleWrapper.Whitespace + "Markdown")
                            {
                                if (string.IsNullOrWhiteSpace(partialCommand))
                                {
                                    yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
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

                                    if (firstWord.Equals("source", StringComparison.OrdinalIgnoreCase))
                                    {
                                        partialCommand = partialCommand.Substring(6).TrimStart();

                                        if (string.IsNullOrWhiteSpace(partialCommand))
                                        {
                                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
                                        }

                                        foreach ((ConsoleTextSpan[], string) item in GetFileCompletion(partialCommand, "option set source"))
                                        {
                                            yield return item;
                                        }

                                    }
                                    else
                                    {
                                        if ("source".StartsWith(firstWord, StringComparison.OrdinalIgnoreCase))
                                        {
                                            yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set source", ConsoleColor.Yellow) }, "option set source ");
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            yield break;
                        }
                    }
                    else
                    {
                        yield break;
                    }
                }
                else
                {
                    if ("list".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("list", ConsoleColor.Yellow) }, "option list ");
                    }

                    if ("select".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("select", ConsoleColor.Yellow) }, "option select ");
                    }

                    if ("set".StartsWith(partialCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return (new ConsoleTextSpan[] { new ConsoleTextSpan("option ", ConsoleColor.Green), new ConsoleTextSpan("set", ConsoleColor.Yellow) }, "option set ");
                    }
                }
            }
        }

        public static IEnumerable<(ConsoleTextSpan[], string)> GetFileCompletion(string partialCommand, string prefix)
        {
            if (string.IsNullOrWhiteSpace(partialCommand))
            {
                string[] directories = Directory.GetDirectories(Directory.GetCurrentDirectory(), "*");

                List<(ConsoleTextSpan[], string)> tbr = new List<(ConsoleTextSpan[], string)>();

                foreach (string sr in directories)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Blue)
                    }, prefix + " " + Path.GetFileName(sr) + Path.DirectorySeparatorChar));
                }


                string[] files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*");

                foreach (string sr in files)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                    }, prefix + " " + Path.GetFileName(sr) + " "));
                }

                tbr.Sort((a, b) => a.Item2.CompareTo(b.Item2));

                foreach ((ConsoleTextSpan[], string) item in tbr)
                {
                    yield return item;
                }
            }
            else
            {
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
                    }, prefix + " " + Path.Combine(directory, Path.GetFileName(sr)) + Path.DirectorySeparatorChar));
                }


                string[] files = Directory.GetFiles(actualDirectory, fileName + "*");

                foreach (string sr in files)
                {
                    tbr.Add((new ConsoleTextSpan[]
                    {
                        new ConsoleTextSpan(Path.GetFileName(sr) + " ", ConsoleColor.Red)
                    }, prefix + " " + Path.Combine(directory, Path.GetFileName(sr)) + " "));
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
