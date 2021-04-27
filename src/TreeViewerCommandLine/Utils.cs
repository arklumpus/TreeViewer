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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using TreeViewer;

namespace TreeViewerCommandLine
{
    static class Extensions
    {
        public static string CommonStart(this IEnumerable<string> items, bool caseSensitive)
        {
            string seed = "";
            bool initialised = false;

            foreach (string sr in items)
            {
                if (!initialised)
                {
                    initialised = true;
                    seed = sr;
                }
                else
                {
                    StringBuilder newSeed = new StringBuilder();

                    for (int i = 0; i < Math.Min(seed.Length, sr.Length); i++)
                    {
                        if ((caseSensitive && seed[i] == sr[1]) || (!caseSensitive && seed.Substring(i, 1).Equals(sr.Substring(i, 1), StringComparison.OrdinalIgnoreCase)))
                        {
                            newSeed.Append(seed[i]);
                        }
                        else
                        {
                            break;
                        }
                    }

                    seed = newSeed.ToString();
                }
            }

            return seed;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> haystack, T needle)
        {
            for (int i = 0; i < haystack.Count; i++)
            {
                if (haystack[i].Equals(needle))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int IndexOf(this IReadOnlyList<string> haystack, string needle, StringComparison comparisonType)
        {
            for (int i = 0; i < haystack.Count; i++)
            {
                if (haystack[i].Equals(needle, comparisonType))
                {
                    return i;
                }
            }
            return -1;
        }

        public static string ToFontString(this VectSharp.Font fnt)
        {
            return fnt.FontFamily.FileName + " " + fnt.FontSize.ToString();
        }

        public static VectSharp.Font FromFontString(string sr)
        {
            double fontSize = double.Parse(sr.Substring(sr.LastIndexOf(" ") + 1));
            string fontFamily = sr.Substring(0, sr.LastIndexOf(" "));
            return new VectSharp.Font(new VectSharp.FontFamily(fontFamily), fontSize);
        }
    }

    static class PendingUpdates
    {
        public static bool Transformer { get; set; } = false;
        public static bool FurtherTransformations { get; set; } = false;
        public static bool Coordinates { get; set; } = false;

        public static bool Any
        {
            get
            {
                return Transformer || FurtherTransformations || Coordinates;
            }
        }

        public static bool NeverAsk { get; set; } = false;
    }

    public class ModuleParametersContainer
    {
        public Dictionary<string, object> Parameters { get; set; }
        public Action<Dictionary<string, object>> UpdateParameterAction { get; set; }
        public Action PrintParameters { get; set; }
        public List<string> ParameterKeys { get; set; }
        public Dictionary<string, IReadOnlyList<string>> PossibleValues { get; set; }
        public Dictionary<string, string> ControlTypes { get; set; }
    }

    static class Utils
    {
        public static string GetNanoName()
        {
            if (Modules.IsWindows)
            {
                return "nano";
            }
            else if (Modules.IsLinux)
            {
                string editor = Environment.GetEnvironmentVariable("EDITOR");

                if (string.IsNullOrEmpty(editor))
                {
                    if (File.Exists("/usr/bin/editor"))
                    {
                        try
                        {
                            ProcessStartInfo info = new ProcessStartInfo("readlink", "-f /usr/bin/editor");
                            info.RedirectStandardOutput = true;

                            Process proc = System.Diagnostics.Process.Start(info);
                            proc.WaitForExit();

                            string output = proc.StandardOutput.ReadToEnd();

                            ConsoleWrapper.TreatControlCAsInput = true;
                            Console.OutputEncoding = Encoding.UTF8;
                            ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);

                            return Path.GetFileName(output);
                        }
                        catch
                        {
                            return "editor";
                        }
                    }
                    else
                    {
                        return "nano";
                    }
                }
                else
                {
                    return Path.GetFileName(editor);
                }
            }
            else
            {
                return "nano";
            }
        }

        public static void RunNano(string fileToEdit)
        {
            if (Modules.IsWindows)
            {
                string command = "\"" + Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "nano.exe\" \"" + fileToEdit + "\"");

                string args = "/S /C \"" + command + "\"";
                ProcessStartInfo info = new ProcessStartInfo("cmd.exe", args);

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
            else if (Modules.IsLinux)
            {
                string editor = Environment.GetEnvironmentVariable("EDITOR");

                if (string.IsNullOrEmpty(editor))
                {
                    if (File.Exists("/usr/bin/editor"))
                    {
                        editor = "/usr/bin/editor";
                    }
                    else
                    {
                        editor = "nano";
                    }
                }

                ProcessStartInfo info = new ProcessStartInfo(editor, "\"" + fileToEdit + "\"");

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
            else if (Modules.IsMac)
            {
                ProcessStartInfo info = new ProcessStartInfo("nano", "\"" + fileToEdit + "\"");

                Process proc = System.Diagnostics.Process.Start(info);
                proc.WaitForExit();
                ConsoleWrapper.TreatControlCAsInput = true;
                Console.OutputEncoding = Encoding.UTF8;
                ConsoleWrapper.SetOutputMode(ConsoleWrapper.OutputModes.Error);
            }
        }


        private static void UpdateParameters(Dictionary<string, object> parametersToUpdate, Dictionary<string, Action<object>> parameterUpdaters)
        {
            foreach (KeyValuePair<string, object> kvp in parametersToUpdate)
            {
                if (parameterUpdaters.ContainsKey(kvp.Key) || kvp.Key == Modules.ModuleIDKey)
                {
                    parameterUpdaters[kvp.Key](kvp.Value);
                }
            }
        }

        public static ModuleParametersContainer GetParameters(GenericParameterChangeDelegate parameterChangeDelegate, List<(string, string)> parameters, string moduleName, string moduleId)
        {
            Dictionary<string, object> tbr = new Dictionary<string, object>();
            List<string> keys = new List<string>();
            List<(Func<Dictionary<string, ControlStatus>, ConsoleTextSpan[]>, int)> printParameterLines = new List<(Func<Dictionary<string, ControlStatus>, ConsoleTextSpan[]>, int)>();
            Dictionary<string, IReadOnlyList<string>> possibleValues = new Dictionary<string, IReadOnlyList<string>>();
            Dictionary<string, string> controlTypes = new Dictionary<string, string>();

            Action<Dictionary<string, object>> UpdateParameterAction;

            if (parameters.Count > 1)
            {
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                    else if (controlType == "TreeCollection")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, Program.Trees);
                    }
                    else if (controlType == "Window")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, null);
                    }
                    else if (controlType == "InstanceStateData")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, Program.StateData);
                    }
                    else if (controlType == "Group" || controlType == "Expander")
                    {
                        string parameterName = parameters[i].Item1;

                        int numChildren = int.Parse(controlParameters);

                        tbr.Add(parameterName, null);

                        printParameterLines.Add(((status) =>
                        {
                            if (status[parameterName] == ControlStatus.Enabled)
                            {
                                return new ConsoleTextSpan[] { new ConsoleTextSpan(parameterName) };
                            }
                            else if (status[parameterName] == ControlStatus.Disabled)
                            {
                                return new ConsoleTextSpan[] { new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray) };
                            }
                            else
                            {
                                return null;
                            }
                        }, numChildren));
                    }
                    else if (controlType == "Button")
                    {
                        string parameterName = parameters[i].Item1;

                        tbr.Add(parameterName, false);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        printParameterLines.Add(((status) => null, -1));
                    }
                    else if (controlType == "Buttons")
                    {
                        string parameterName = parameters[i].Item1;

                        string[] buttons = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                        tbr.Add(parameterName, -1);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                        printParameterLines.Add(((status) => null, -1));
                    }
                    else if (controlType == "CheckBox")
                    {
                        string parameterName = parameters[i].Item1;

                        bool defaultValue = Convert.ToBoolean(controlParameters);

                        keys.Add(parameterName);
                        tbr.Add(parameterName, defaultValue);
                        possibleValues.Add(parameterName, new string[] { "true", "false" });
                        controlTypes.Add(parameterName, "CheckBox");

                        int ind = keys.Count;

                        printParameterLines.Add(((status) =>
                        {
                            if (status[parameterName] == ControlStatus.Enabled)
                            {
                                if ((bool)tbr[parameterName])
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan("√ ", ConsoleColor.Green), new ConsoleTextSpan(parameterName) };
                                }
                                else
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan("□ ", ConsoleColor.Red), new ConsoleTextSpan(parameterName) };
                                }
                            }
                            else if (status[parameterName] == ControlStatus.Disabled)
                            {
                                if ((bool)tbr[parameterName])
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan("√ ", ConsoleColor.DarkGray), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan("□ ", ConsoleColor.DarkGray), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray) };
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                            , -1));

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });

                    }
                    else if (controlType == "Formatter")
                    {
                        string parameterName = parameters[i].Item1;

                        string[] parsedParameters = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                        object[] formatterParams = new object[parsedParameters.Length - 1];

                        string attrType = (string)tbr[parsedParameters[0]];

                        if (attrType == "String")
                        {
                            formatterParams[0] = parsedParameters[1];
                            formatterParams[1] = Convert.ToBoolean(parsedParameters[2]);
                        }
                        else if (attrType == "Number")
                        {
                            formatterParams[0] = int.Parse(parsedParameters[1], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[1] = double.Parse(parsedParameters[2], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[2] = double.Parse(parsedParameters[3], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[3] = double.Parse(parsedParameters[4], System.Globalization.CultureInfo.InvariantCulture);
                            formatterParams[4] = Convert.ToBoolean(parsedParameters[5]);
                            formatterParams[5] = Convert.ToBoolean(parsedParameters[6]);
                            formatterParams[6] = parsedParameters[7];
                            formatterParams[7] = Convert.ToBoolean(parsedParameters[8]);
                        }

                        keys.Add(parameterName);
                        tbr.Add(parameterName, new FormatterOptions(parsedParameters[parsedParameters.Length - 2]) { Parameters = formatterParams });
                        possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Formatter" });
                        controlTypes.Add(parameterName, "Formatter");

                        int ind = keys.Count;

                        printParameterLines.Add(((status) =>
                        {
                            if (status[parameterName] == ControlStatus.Enabled)
                            {
                                if ((bool)((FormatterOptions)tbr[parameterName]).Parameters[^1])
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    [Default " + (string)tbr[parsedParameters[0]] + "]", ConsoleColor.Blue) };
                                }
                                else
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    [Custom " + (string)tbr[parsedParameters[0]] + "]", ConsoleColor.Blue) };
                                }
                            }
                            else if (status[parameterName] == ControlStatus.Disabled)
                            {
                                if ((bool)((FormatterOptions)tbr[parameterName]).Parameters[^1])
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    [Default " + (string)tbr[parsedParameters[0]] + "]", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    [Custom " + (string)tbr[parsedParameters[0]] + "]", ConsoleColor.DarkGray) };
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                            , -1));

                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });


                    }
                    else if (controlType == "Label")
                    {
                        string parameterName = parameters[i].Item1;

                        tbr.Add(parameterName, null);

                        int controlCode = -1;

                        if (controlParameters.StartsWith("["))
                        {
                            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            if (items.Length > 0)
                            {
                                switch (items[0])
                                {
                                    case "Left":
                                        controlCode = -1;
                                        break;
                                    case "Right":
                                        controlCode = -2;
                                        break;
                                    case "Center":
                                        controlCode = -3;
                                        break;
                                }
                            }
                        }

                        printParameterLines.Add(((status) =>
                        {
                            if (status[parameterName] == ControlStatus.Enabled)
                            {
                                return new ConsoleTextSpan[] { new ConsoleTextSpan(parameterName) };
                            }
                            else if (status[parameterName] == ControlStatus.Disabled)
                            {
                                return new ConsoleTextSpan[] { new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray) };
                            }
                            else
                            {
                                return null;
                            }

                        }, controlCode));
                    }
                    else
                    {
                        string parameterName = parameters[i].Item1;

                        if (controlType == "ComboBox")
                        {
                            int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);



                            tbr.Add(parameterName, defaultIndex);


                            parameterUpdaters.Add(parameterName, value =>
                            {
                                ////programmaticUpdate = true;
                                tbr[parameterName] = value;
                                ////programmaticUpdate = false;

                            });



                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, items);
                            controlTypes.Add(parameterName, "ComboBox");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan(items[(int)tbr[parameterName]] + " ▼", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan(items[(int)tbr[parameterName]] + " ▼", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));


                        }
                        else if (controlType == "TextBox")
                        {
                            tbr.Add(parameterName, controlParameters);

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "TextBox" });
                            controlTypes.Add(parameterName, "TextBox");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan((string)tbr[parameterName], ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan((string)tbr[parameterName], ConsoleColor.Blue) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });
                        }
                        else if (controlType == "AttributeSelector")
                        {
                            int defaultIndex = Math.Max(0, Program.AttributeList.IndexOf(controlParameters));

                            tbr.Add(parameterName, controlParameters);

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, Program.AttributeList);
                            controlTypes.Add(parameterName, "AttributeSelector");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan((string)tbr[parameterName] + " ▼", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan((string)tbr[parameterName] + " ▼", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });
                        }
                        else if (controlType == "Attachment")
                        {
                            tbr.Add(parameterName, null);

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, Program.AttachmentList);
                            controlTypes.Add(parameterName, "Attachment");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan(tbr[parameterName] != null ? ((Attachment)tbr[parameterName]).Name + " ▼" : "(None) ▼", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan(tbr[parameterName] != null ? ((Attachment)tbr[parameterName]).Name + " ▼" : "(None) ▼", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;
                            });
                        }
                        else if (controlType == "Node")
                        {
                            string[] defaultValue = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);



                            tbr.Add(parameterName, defaultValue);

                            parameterUpdaters.Add(parameterName, (value) =>
                            {
                                tbr[parameterName] = value;
                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Node" });
                            controlTypes.Add(parameterName, "Node");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan("LCA of " + ((string[])tbr[parameterName]).Length.ToString() + " nodes", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan("LCA of " + ((string[])tbr[parameterName]).Length.ToString() + " nodes", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            , -1));

                        }
                        else if (controlType == "NumericUpDown")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (range.Length > 2)
                            {
                                increment = double.Parse(range[2], System.Globalization.CultureInfo.InvariantCulture);
                            }

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = TreeViewer.Extensions.GetFormatString(increment);

                            if (range.Length > 3)
                            {
                                formatString = range[3];
                            }



                            tbr.Add(parameters[i].Item1, defaultValue);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "NumericUpDown", range[0], range[1], formatString });
                            controlTypes.Add(parameterName, "NumericUpDown");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan(((double)tbr[parameterName]).ToString(formatString), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan(((double)tbr[parameterName]).ToString(formatString), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }

                            }
                            , -1));
                        }
                        else if (controlType == "NumericUpDownByNode")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = TreeViewer.Extensions.GetFormatString(increment);

                            object[] formatterParams = new object[4];

                            string attrType = range[4];

                            if (attrType == "String")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = range[2];
                                formatterParams[1] = minRange;
                                formatterParams[2] = maxRange;
                                formatterParams[3] = Convert.ToBoolean(range[5]);
                            }

                            tbr.Add(parameters[i].Item1, new NumberFormatterOptions(range[2]) { AttributeName = range[3], AttributeType = attrType, DefaultValue = defaultValue, Parameters = formatterParams });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "NumericUpDownByNode", range[0], range[1], formatString });
                            controlTypes.Add(parameterName, "NumericUpDownByNode");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + ((NumberFormatterOptions)tbr[parameterName]).DefaultValue.ToString(formatString) + " ", ConsoleColor.Blue), new ConsoleTextSpan(" … ", ConsoleColor.Blue, ConsoleColor.White) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + ((NumberFormatterOptions)tbr[parameterName]).DefaultValue.ToString(formatString) + " ", ConsoleColor.DarkGray), new ConsoleTextSpan(" … ", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "Slider")
                        {
                            double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = TreeViewer.Extensions.GetFormatString(increment);


                            tbr.Add(parameters[i].Item1, defaultValue);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "NumericUpDown", range[0], range[1], formatString });
                            controlTypes.Add(parameterName, "NumericUpDown");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan(((double)tbr[parameterName]).ToString(formatString), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan(((double)tbr[parameterName]).ToString(formatString), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));
                        }
                        else if (controlType == "Font")
                        {
                            string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            VectSharp.Font fnt = new VectSharp.Font(new VectSharp.FontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                            tbr.Add(parameters[i].Item1, fnt);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Font" });
                            controlTypes.Add(parameterName, "Font");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + ((VectSharp.Font)tbr[parameterName]).ToFontString(), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + ((VectSharp.Font)tbr[parameterName]).ToFontString(), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "Point")
                        {
                            double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters);

                            string formatStringX = TreeViewer.Extensions.GetFormatString(point[0]);
                            string formatStringY = TreeViewer.Extensions.GetFormatString(point[1]);

                            tbr.Add(parameters[i].Item1, new VectSharp.Point(point[0], point[1]));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Point" });
                            controlTypes.Add(parameterName, "Point");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + ((VectSharp.Point)tbr[parameterName]).X.ToString(formatStringX) + ", " + ((VectSharp.Point)tbr[parameterName]).Y.ToString(formatStringY), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + ((VectSharp.Point)tbr[parameterName]).X.ToString(formatStringX) + ", " + ((VectSharp.Point)tbr[parameterName]).Y.ToString(formatStringY), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "Colour")
                        {
                            int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                            tbr.Add(parameters[i].Item1, col);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Colour" });
                            controlTypes.Add(parameterName, "Colour");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + ((VectSharp.Colour)tbr[parameterName]).ToHexString(), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + ((VectSharp.Colour)tbr[parameterName]).ToHexString(), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));

                        }
                        else if (controlType == "SourceCode")
                        {
                            string defaultSource = controlParameters;

                            tbr.Add(parameters[i].Item1, new CompiledCode(defaultSource));

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "SourceCode" });
                            controlTypes.Add(parameterName, "SourceCode");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    [Source code]", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    [Source code]", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "Markdown")
                        {
                            string defaultSource = controlParameters;

                            tbr.Add(parameters[i].Item1, defaultSource);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                tbr[parameterName] = value;
                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Markdown" });
                            controlTypes.Add(parameterName, "Markdown");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    [Markdown]", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    [Markdown]", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "Dash")
                        {
                            double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters);

                            VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);



                            tbr.Add(parameters[i].Item1, lineDash);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "Dash" });
                            controlTypes.Add(parameterName, "Dash");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                VectSharp.LineDash currentDash = (VectSharp.LineDash)tbr[parameterName];

                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + currentDash.UnitsOn.ToString() + " " + currentDash.UnitsOff.ToString() + " " + currentDash.Phase.ToString(), ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + currentDash.UnitsOn.ToString() + " " + currentDash.UnitsOff.ToString() + " " + currentDash.Phase.ToString(), ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "ColourByNode")
                        {
                            string[] colour = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba(int.Parse(colour[3]), int.Parse(colour[4]), int.Parse(colour[5]), int.Parse(colour[6]));

                            object[] formatterParams = new object[colour.Length - 5];

                            string attrType = colour[2];

                            if (attrType == "String")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = Convert.ToBoolean(colour[7]);
                            }
                            else if (attrType == "Number")
                            {
                                formatterParams[0] = colour[0];
                                formatterParams[1] = double.Parse(colour[7], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[2] = double.Parse(colour[8], System.Globalization.CultureInfo.InvariantCulture);
                                formatterParams[3] = Modules.DefaultGradients[colour[9]];
                                formatterParams[4] = Convert.ToBoolean(colour[10]);
                            }

                            tbr.Add(parameters[i].Item1, new ColourFormatterOptions(colour[0]) { AttributeName = colour[1], AttributeType = attrType, DefaultColour = col, Parameters = formatterParams });

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });

                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, new string[] { ConsoleWrapper.Whitespace + "ColourByNode" });
                            controlTypes.Add(parameterName, "ColourByNode");

                            int ind = keys.Count;

                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName), new ConsoleTextSpan("    " + ((ColourFormatterOptions)tbr[parameterName]).DefaultColour.ToHexString() + " ", ConsoleColor.Blue), new ConsoleTextSpan(" … ", ConsoleColor.Blue, ConsoleColor.White) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName, ConsoleColor.DarkGray), new ConsoleTextSpan("    " + ((ColourFormatterOptions)tbr[parameterName]).DefaultColour.ToHexString() + " ", ConsoleColor.DarkGray), new ConsoleTextSpan(" … ", ConsoleColor.Blue, ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                                , -1));
                        }
                        else if (controlType == "AttributeType")
                        {
                            List<string> attributeTypes = new List<string>(Modules.AttributeTypes);

                            int defaultIndex = attributeTypes.IndexOf(controlParameters);

                            tbr.Add(parameterName, attributeTypes[defaultIndex]);

                            parameterUpdaters.Add(parameterName, value =>
                            {
                                //programmaticUpdate = true;
                                tbr[parameterName] = value;
                                //programmaticUpdate = false;

                            });


                            keys.Add(parameterName);
                            possibleValues.Add(parameterName, attributeTypes);
                            controlTypes.Add(parameterName, "AttributeType");

                            int ind = keys.Count;
                            printParameterLines.Add(((status) =>
                            {
                                if (status[parameterName] == ControlStatus.Enabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan("#" + ind.ToString() + new string(ConsoleWrapper.Whitespace, 4 - ind.ToString().Length), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    "), new ConsoleTextSpan((string)tbr[parameterName] + " ▼", ConsoleColor.Blue) };
                                }
                                else if (status[parameterName] == ControlStatus.Disabled)
                                {
                                    return new ConsoleTextSpan[] { new ConsoleTextSpan(new string(ConsoleWrapper.Whitespace, 5), ConsoleColor.Yellow), new ConsoleTextSpan(parameterName + "    ", ConsoleColor.DarkGray), new ConsoleTextSpan((string)tbr[parameterName] + " ▼", ConsoleColor.DarkGray) };
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            , -1));


                        }

                    }
                }

                parameterChangeDelegate(tbr, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);
                UpdateParameters(parametersToChange, parameterUpdaters);

                UpdateParameterAction = (parametersToChange) =>
                {
                    foreach (KeyValuePair<string, object> kvp in parametersToChange)
                    {
                        Dictionary<string, object> previousParameters = tbr.ShallowClone();
                        UpdateParameters(new Dictionary<string, object>() { { kvp.Key, kvp.Value } }, parameterUpdaters);
                        parameterChangeDelegate(previousParameters, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange2);
                        UpdateParameters(parametersToChange2, parameterUpdaters);
                    }
                };
            }
            else
            {
                Dictionary<string, Action<object>> parameterUpdaters = new Dictionary<string, Action<object>>();

                for (int i = 0; i < parameters.Count; i++)
                {
                    string controlType = parameters[i].Item2.Substring(0, parameters[i].Item2.IndexOf(":"));
                    string controlParameters = parameters[i].Item2.Substring(parameters[i].Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        string parameterName = parameters[i].Item1;
                        tbr.Add(parameterName, controlParameters);
                        parameterUpdaters.Add(parameterName, (value) =>
                        {
                            tbr[parameterName] = value;
                        });
                    }
                }

                printParameterLines.Add(((status) => new ConsoleTextSpan[] { new ConsoleTextSpan("No options available", ConsoleColor.DarkGray) }, -1));
                UpdateParameterAction = (parametersToChange) =>
                {
                    UpdateParameters(parametersToChange, parameterUpdaters);
                };
            }

            Action PrintParameters = () =>
            {
                parameterChangeDelegate(tbr, tbr, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);

                foreach (KeyValuePair<string, object> kvp in tbr)
                {
                    if (!controlStatus.ContainsKey(kvp.Key))
                    {
                        controlStatus.Add(kvp.Key, ControlStatus.Enabled);
                    }
                }

                List<(ConsoleTextSpan[], int)> lines = (from el in printParameterLines let line = el.Item1(controlStatus) select (line, el.Item2)).ToList();


                Stack<int> childrenTillPop = new Stack<int>();
                childrenTillPop.Push(-1);

                ControlItem rootParent = new ControlItem() { Children = new List<ControlItem>(), ControlCode = 0 };

                Stack<ControlItem> parents = new Stack<ControlItem>();
                parents.Push(rootParent);

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i].Item2 < 0)
                    {
                        ControlItem line = new ControlItem(parents.Peek()) { Text = lines[i].Item1, ControlCode = lines[i].Item2 };
                        parents.Peek().Children.Add(line);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }
                    }
                    else
                    {
                        ControlItem group = new ControlItem(parents.Peek()) { Children = new List<ControlItem>(), Text = lines[i].Item1, ControlCode = lines[i].Item2 };
                        parents.Peek().Children.Add(group);

                        int popping = childrenTillPop.Pop();
                        if (popping > 0)
                        {
                            popping--;
                            if (popping == 0)
                            {
                                parents.Pop();
                            }
                            else
                            {
                                childrenTillPop.Push(popping);
                            }
                        }
                        else
                        {
                            childrenTillPop.Push(popping);
                        }


                        parents.Push(group);
                        childrenTillPop.Push(lines[i].Item2);
                    }
                }

                ConsoleWrapper.WriteLine();

                ConsoleWrapper.WriteLine(new ConsoleTextSpan[] { new ConsoleTextSpan("  Options for module ", 2),
                                                                 new ConsoleTextSpan(moduleName, 2, ConsoleColor.Cyan),
                                                                 new ConsoleTextSpan(" (" + moduleId + "):", 2)
                });

                ConsoleWrapper.WriteLine();

                rootParent.Print("    ");

                ConsoleWrapper.WriteLine();
            };


            return new ModuleParametersContainer() { ParameterKeys = keys, Parameters = tbr, PrintParameters = PrintParameters, UpdateParameterAction = UpdateParameterAction, PossibleValues = possibleValues, ControlTypes = controlTypes };
        }

        private class ControlItem
        {
            public List<ControlItem> Children { get; set; } = null;

            public ConsoleTextSpan[] Text { get; set; } = null;

            public int ControlCode { get; set; } = -1;

            public ControlItem Parent { get; set; } = null;

            public ControlItem() { }

            public ControlItem(ControlItem parent)
            {
                this.Parent = parent;
            }

            public ControlItem Root()
            {
                ControlItem currentParent = this;

                while (currentParent.Parent != null)
                {
                    currentParent = currentParent.Parent;
                }

                return currentParent;
            }

            public int Measure()
            {
                if (this.ControlCode < 0)
                {
                    if (this.Text == null)
                    {
                        return 0;
                    }
                    else
                    {
                        int parentCount = 0;

                        ControlItem currentParent = this.Parent;

                        while (currentParent != null)
                        {
                            parentCount++;
                            currentParent = currentParent.Parent;
                        }

                        return this.Text.Unformat().Length + (parentCount - 1) * 4;
                    }
                }
                else if (this.ControlCode > 0)
                {
                    int parentCount = 0;

                    ControlItem currentParent = this.Parent;

                    while (currentParent != null)
                    {
                        parentCount++;
                        currentParent = currentParent.Parent;
                    }

                    return Math.Max((from el in this.Children select el.Measure()).Max(), this.Text.Unformat().Length + parentCount * 4);
                }
                else
                {
                    return (from el in this.Children select el.Measure()).Max();
                }
            }

            public void Print(string baseIndent)
            {
                if (this.ControlCode < 0)
                {
                    if (this.Text != null)
                    {
                        int parentCount = 0;

                        ControlItem currentParent = this.Parent;

                        while (currentParent != null)
                        {
                            parentCount++;
                            currentParent = currentParent.Parent;
                        }

                        ConsoleWrapper.Write(baseIndent);

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write("│ ");
                        }

                        if (this.ControlCode == -2)
                        {
                            int deltaLeft = this.Root().Measure() - this.Text.Unformat().Length - (parentCount - 1) * 4;
                            ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, deltaLeft));
                        }
                        else if (this.ControlCode == -3)
                        {
                            int deltaLeft = (this.Root().Measure() - this.Text.Unformat().Length - (parentCount - 1) * 4) / 2;
                            ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, deltaLeft));
                        }

                        ConsoleWrapper.Write(this.Text);

                        if (parentCount > 1 && this.ControlCode == -1)
                        {
                            int deltaRight = this.Root().Measure() - this.Text.Unformat().Length - (parentCount - 1) * 4;
                            ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, deltaRight));
                        }
                        else if (this.ControlCode == -3)
                        {
                            int delta = this.Root().Measure() - this.Text.Unformat().Length - (parentCount - 1) * 4;
                            ConsoleWrapper.Write(new string(ConsoleWrapper.Whitespace, delta - delta / 2));
                        }

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write(" │");
                        }

                        ConsoleWrapper.WriteLine();
                    }
                }
                else if (this.ControlCode > 0)
                {
                    if (this.Text != null)
                    {
                        int parentCount = 0;

                        ControlItem currentParent = this.Parent;

                        while (currentParent != null)
                        {
                            parentCount++;
                            currentParent = currentParent.Parent;
                        }

                        ConsoleWrapper.Write(baseIndent);

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write("│ ");
                        }

                        ConsoleWrapper.Write("┌");

                        int measure = this.Root().Measure();

                        int deltaLeft = (measure - this.Text.Unformat().Length - parentCount * 4) / 2;
                        int deltaRight = measure - this.Text.Unformat().Length - parentCount * 4 - deltaLeft;

                        ConsoleWrapper.Write(new string('─', deltaLeft) + " ");
                        ConsoleWrapper.Write(this.Text);
                        ConsoleWrapper.Write(" " + new string('─', deltaRight));
                        ConsoleWrapper.Write("┐");

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write(" │");
                        }

                        ConsoleWrapper.WriteLine();

                        for (int i = 0; i < this.Children.Count; i++)
                        {
                            this.Children[i].Print(baseIndent);
                        }

                        ConsoleWrapper.Write(baseIndent);

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write("│ ");
                        }

                        ConsoleWrapper.Write("└" + new string('─', measure - 2 - (parentCount - 1) * 4) + "┘");

                        for (int i = 1; i < parentCount; i++)
                        {
                            ConsoleWrapper.Write(" │");
                        }

                        ConsoleWrapper.WriteLine();
                    }
                }
                else
                {
                    for (int i = 0; i < this.Children.Count; i++)
                    {
                        this.Children[i].Print(baseIndent);
                    }
                }

            }
        }
    }
}
