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

using Avalonia.Controls;
using Mono.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VectSharp;

namespace TreeViewer
{
    public class GlobalSettings
    {
        public enum UpdateCheckModes
        {
            DontCheck,
            ProgramOnly,
            ProgramAndInstalledModules,
            ProgramAndAllModules,
        }

        public enum InterfaceStyles
        {
            WindowsStyle,
            MacOSStyle
        }

        public enum RibbonStyles
        {
            Colourful,
            Grey
        }

        internal static readonly string ProgramRepository = "arklumpus/TreeViewer";

        internal static readonly string DefaultModuleRepository = @"https://raw.githubusercontent.com/arklumpus/TreeViewer/main/Modules/";

        public bool ShowLegacyUpDownArrows { get; set; } = false;
        public InterfaceStyles InterfaceStyle { get; set; } = Modules.IsMac ? InterfaceStyles.MacOSStyle : InterfaceStyles.WindowsStyle;
        public RibbonStyles RibbonStyle { get; set; } = Modules.IsWindows ? RibbonStyles.Colourful : RibbonStyles.Grey;
        public TimeSpan AutosaveInterval { get; set; } = new TimeSpan(0, 10, 0);
        public int DragInterval { get; set; } = 250;
        public int KeepRecentFilesFor { get; set; } = 30;
        public bool DrawTreeWhenOpened { get; set; } = true;
        public Colour SelectionColour { get; set; } = Colour.FromRgb(35, 127, 255);
        public Colour BackgroundColour { get; set; } = Colour.FromRgb(240, 244, 250);
        public Dictionary<string, object> AdditionalSettings { get; set; } = new Dictionary<string, object>();
        internal Dictionary<string, string> AdditionalSettingsList { get; } = new Dictionary<string, string>();
        public string ModuleRepositoryBaseUri { get; set; } = DefaultModuleRepository;
        public long UpdateCheckDate { get; set; } = 0;
        public UpdateCheckModes UpdateCheckMode { get; set; } = UpdateCheckModes.ProgramAndAllModules;
        public bool EnableUndoStack { get; set; } = true;
        public static GlobalSettings Settings { get; }
        internal List<MainWindow> MainWindows { get; } = new List<MainWindow>();

        public static JsonSerializerOptions SerializationOptions { get; } = new JsonSerializerOptions()
        {
            WriteIndented = true,
            Converters =
                {
                    new ColourConverter(),
                    new FontConverter(),
                    new PointConverter(),
                    new LineDashConverter(),
                    new TimeSpanConverter()
                }
        };

        static GlobalSettings()
        {
            string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetAssembly(typeof(Modules)).GetName().Name, "settings.json");

            try
            {
                if (File.Exists(settingsPath))
                {
                    GlobalSettings.Settings = System.Text.Json.JsonSerializer.Deserialize<GlobalSettings>(File.ReadAllText(settingsPath), SerializationOptions);
                }
                else
                {
                    GlobalSettings.Settings = new GlobalSettings();
                    SaveSettings();
                }
            }
            catch
            {
                GlobalSettings.Settings = new GlobalSettings();
                SaveSettings();
            }
        }

        internal static void SetSetting(string settingName, string settingValue)
        {
            switch (settingName)
            {
                case "AutosaveInterval":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.AutosaveInterval = new TimeSpan(0, 10, 0);
                    }
                    else
                    {
                        GlobalSettings.Settings.AutosaveInterval = TimeSpan.Parse(settingValue);
                    }
                    break;
                case "DragInterval":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.DragInterval = 250;
                    }
                    else
                    {
                        GlobalSettings.Settings.DragInterval = int.Parse(settingValue);
                    }
                    break;
                case "KeepRecentFilesFor":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.KeepRecentFilesFor = 30;
                    }
                    else
                    {
                        GlobalSettings.Settings.KeepRecentFilesFor = int.Parse(settingValue);
                    }
                    break;
                case "DrawTreeWhenOpened":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.DrawTreeWhenOpened = true;
                    }
                    else
                    {
                        GlobalSettings.Settings.DrawTreeWhenOpened = Convert.ToBoolean(settingValue);
                    }
                    break;
                case "ShowLegacyUpDownArrows":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.ShowLegacyUpDownArrows = false;
                    }
                    else
                    {
                        GlobalSettings.Settings.ShowLegacyUpDownArrows = Convert.ToBoolean(settingValue);
                    }
                    break;
                case "SelectionColour":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.SelectionColour = Colour.FromRgb(35, 127, 255);
                    }
                    else
                    {
                        GlobalSettings.Settings.SelectionColour = Colour.FromCSSString(settingValue) ?? Colour.FromRgb(35, 127, 255);
                    }
                    break;
                case "BackgroundColour":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.BackgroundColour = Colour.FromRgb(240, 244, 250);
                    }
                    else
                    {
                        GlobalSettings.Settings.BackgroundColour = Colour.FromCSSString(settingValue) ?? Colour.FromRgb(240, 244, 250);
                    }
                    break;
                case "ModuleRepositoryBaseUri":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.ModuleRepositoryBaseUri = GlobalSettings.DefaultModuleRepository;
                    }
                    else
                    {
                        GlobalSettings.Settings.ModuleRepositoryBaseUri = settingValue;
                    }
                    break;
                case "UpdateCheckMode":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.UpdateCheckMode = GlobalSettings.UpdateCheckModes.ProgramAndAllModules;
                    }
                    else
                    {
                        GlobalSettings.Settings.UpdateCheckMode = (GlobalSettings.UpdateCheckModes)Enum.Parse(typeof(GlobalSettings.UpdateCheckModes), settingValue);
                    }
                    break;
                case "EnableUndoStack":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.EnableUndoStack = false;
                    }
                    else
                    {
                        GlobalSettings.Settings.EnableUndoStack = Convert.ToBoolean(settingValue);
                    }
                    break;
                case "InterfaceStyle":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.InterfaceStyle = Modules.IsMac ? InterfaceStyles.MacOSStyle : InterfaceStyles.WindowsStyle;
                    }
                    else
                    {
                        GlobalSettings.Settings.InterfaceStyle = (GlobalSettings.InterfaceStyles)Enum.Parse(typeof(GlobalSettings.InterfaceStyles), settingValue);
                    }
                    break;
                case "RibbonStyle":
                    if (string.IsNullOrEmpty(settingValue))
                    {
                        GlobalSettings.Settings.RibbonStyle = Modules.IsWindows ? RibbonStyles.Colourful : RibbonStyles.Grey;
                    }
                    else
                    {
                        GlobalSettings.Settings.RibbonStyle = (GlobalSettings.RibbonStyles)Enum.Parse(typeof(GlobalSettings.RibbonStyles), settingValue);
                    }
                    break;
                default:
                    {
                        bool found = false;

                        foreach (KeyValuePair<string, string> kvp in GlobalSettings.Settings.AdditionalSettingsList)
                        {
                            string name = kvp.Key;
                            string data = kvp.Value;

                            if (name == settingName)
                            {
                                string controlType = data.Substring(0, data.IndexOf(":"));
                                string controlParameters = data.Substring(data.IndexOf(":") + 1);

                                if (controlType == "CheckBox")
                                {
                                    bool defaultValue = Convert.ToBoolean(controlParameters);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultValue;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = Convert.ToBoolean(settingValue);
                                    }
                                }
                                else if (controlType == "ComboBox")
                                {
                                    int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                                    controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                                    List<string> items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(controlParameters, Modules.DefaultSerializationOptions);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultIndex;
                                    }
                                    else
                                    {
                                        int index = items.IndexOf(settingValue);
                                        if (index < 0)
                                        {
                                            index = defaultIndex;
                                        }

                                        GlobalSettings.Settings.AdditionalSettings[name] = index;
                                    }
                                }
                                else if (controlType == "TextBox")
                                {
                                    string defaultValue = controlParameters;

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultValue;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = settingValue;
                                    }
                                }
                                else if (controlType == "NumericUpDown")
                                {
                                    double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultValue;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = double.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                                else if (controlType == "FileSize")
                                {
                                    long defaultValue = long.Parse(controlParameters);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultValue;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = long.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                                else if (controlType == "Slider")
                                {
                                    double defaultValue = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = defaultValue;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = double.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                                else if (controlType == "Font")
                                {
                                    string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                                    VectSharp.Font fnt = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = fnt;
                                    }
                                    else
                                    {
                                        string[] splitSettingValue = settingValue.Split(',');
                                        VectSharp.Font newFont = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(splitSettingValue[0]), double.Parse(splitSettingValue[1], System.Globalization.CultureInfo.InvariantCulture));

                                        GlobalSettings.Settings.AdditionalSettings[name] = double.Parse(settingValue, System.Globalization.CultureInfo.InvariantCulture);
                                    }
                                }
                                else if (controlType == "Point")
                                {
                                    double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);
                                    VectSharp.Point pt = new VectSharp.Point(point[0], point[1]);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = pt;
                                    }
                                    else
                                    {
                                        string[] splitSettingValue = settingValue.Split(',');
                                        VectSharp.Point newPoint = new VectSharp.Point(double.Parse(splitSettingValue[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(splitSettingValue[1], System.Globalization.CultureInfo.InvariantCulture));

                                        GlobalSettings.Settings.AdditionalSettings[name] = newPoint;
                                    }
                                }
                                else if (controlType == "Colour")
                                {
                                    int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters, Modules.DefaultSerializationOptions);

                                    VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = col;
                                    }
                                    else
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = Colour.FromCSSString(settingValue) ?? col;
                                    }
                                }
                                else if (controlType == "Dash")
                                {
                                    double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                                    VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);

                                    if (string.IsNullOrEmpty(settingValue))
                                    {
                                        GlobalSettings.Settings.AdditionalSettings[name] = lineDash;
                                    }
                                    else
                                    {
                                        string[] splitSettingValue = settingValue.Split(',');
                                        VectSharp.LineDash newDash = new VectSharp.LineDash(double.Parse(splitSettingValue[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(splitSettingValue[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(splitSettingValue[2], System.Globalization.CultureInfo.InvariantCulture));

                                        GlobalSettings.Settings.AdditionalSettings[name] = newDash;
                                    }
                                }

                                found = true;
                                break;
                            }
                        }

                        if (!found)
                        {
                            throw new ArgumentException("There is no global setting named \"" + settingName + "\"!");
                        }
                    }
                    break;
            }
        }

        internal static void SaveSettings()
        {
            try
            {
                string settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Assembly.GetAssembly(typeof(Modules)).GetName().Name, "settings.json");
                File.WriteAllText(settingsPath, System.Text.Json.JsonSerializer.Serialize(GlobalSettings.Settings, SerializationOptions));
            }
            catch
            {

            }
        }

        internal void UpdateAdditionalSettings()
        {
           
        }

        public GlobalSettings()
        {

        }
    }

    public class ColourConverter : JsonConverter<Colour>
    {
        public override Colour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Colour.FromCSSString(reader.GetString()) ?? Colour.FromRgba(0, 0, 0, 0);
        }

        public override void Write(Utf8JsonWriter writer, Colour value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToCSSString(true));
        }
    }

    public class FontConverter : JsonConverter<Font>
    {
        public override Font Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString();
            string[] font = str.Split(',');

            return new Font(FontFamily.ResolveFontFamily(font[0]), double.Parse(font[1], System.Globalization.CultureInfo.InvariantCulture));
        }

        public override void Write(Utf8JsonWriter writer, Font value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.FontFamily.FileName + "," + value.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }


    public class PointConverter : JsonConverter<Point>
    {
        public override Point Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString();
            string[] point = str.Split(',');

            return new Point(double.Parse(point[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(point[1], System.Globalization.CultureInfo.InvariantCulture));
        }

        public override void Write(Utf8JsonWriter writer, Point value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.X.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + value.Y.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    public class LineDashConverter : JsonConverter<LineDash>
    {
        public override LineDash Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            string str = reader.GetString();
            string[] dash = str.Split(',');

            return new LineDash(double.Parse(dash[0], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dash[1], System.Globalization.CultureInfo.InvariantCulture), double.Parse(dash[2], System.Globalization.CultureInfo.InvariantCulture));
        }

        public override void Write(Utf8JsonWriter writer, LineDash value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.UnitsOn.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + value.UnitsOff.ToString(System.Globalization.CultureInfo.InvariantCulture) + "," + value.Phase.ToString(System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.FromTicks(reader.GetInt64());
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Ticks);
        }
    }

    public class VersionConverter : JsonConverter<Version>
    {
        public override Version Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return new Version(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Version value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    internal class SettingOption<T1, T2> : Mono.Options.Option
    {
        Action<T1, T2> action;

        public SettingOption(string prototype, string description, Action<T1, T2> action) : base(prototype, description)
        {
            this.action = action;
        }

        protected override void OnParseComplete(OptionContext c)
        {
            action(Parse<T1>(c.OptionValues[0], c), Parse<T2>(c.OptionValues[1], c));
        }
    }
}
