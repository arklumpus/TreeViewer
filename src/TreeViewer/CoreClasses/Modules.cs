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

using Markdig;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml;
using VectSharp;
using VectSharp.SVG;

namespace TreeViewer
{
    public class SimpleCommand : ICommand
    {
        private EventHandler _canExecuteChanged;

        event EventHandler ICommand.CanExecuteChanged
        {
            add
            {
                _canExecuteChanged += value;
            }

            remove
            {
                _canExecuteChanged -= value;
            }
        }

        private Func<object, bool> _canExecute;

        private bool cachedCanExecute;

        private Action<object> _execute;

        bool ICommand.CanExecute(object parameter)
        {
            return cachedCanExecute;
        }

        void ICommand.Execute(object parameter)
        {
            _execute(parameter);
        }

        public SimpleCommand(Func<object, bool> canExecute, Action<object> execute, Avalonia.Controls.Window parent) : this(canExecute, execute, parent, (Avalonia.AvaloniaProperty)null) { }

        public SimpleCommand(Func<object, bool> canExecute, Action<object> execute, Avalonia.Controls.Window parent, Avalonia.AvaloniaProperty property)
        {
            this._canExecute = canExecute;
            this._execute = execute;
            this.cachedCanExecute = canExecute(parent);

            if (parent != null && property != null)
            {
                parent.PropertyChanged += (s, e) =>
                {
                    if (e.Property == property)
                    {
                        bool newCanExecute = _canExecute(parent);
                        if (newCanExecute != cachedCanExecute)
                        {
                            cachedCanExecute = newCanExecute;
                            _canExecuteChanged?.Invoke(this, new EventArgs());
                        }
                    }
                };
            }
        }

        public SimpleCommand(Func<object, bool> canExecute, Action<object> execute, Avalonia.Controls.Window parent, List<Avalonia.AvaloniaProperty> properties)
        {
            this._canExecute = canExecute;
            this._execute = execute;
            this.cachedCanExecute = canExecute(parent);

            if (parent != null && properties != null && properties.Count > 0)
            {
                parent.PropertyChanged += (s, e) =>
                {
                    if (properties.Contains(e.Property))
                    {
                        bool newCanExecute = _canExecute(parent);
                        if (newCanExecute != cachedCanExecute)
                        {
                            cachedCanExecute = newCanExecute;
                            _canExecuteChanged?.Invoke(this, new EventArgs());
                        }
                    }
                };
            }
        }
    }

    public enum ControlStatus { Enabled, Disabled, Hidden };

    public static class Modules
    {
        public static Avalonia.Input.KeyModifiers ControlModifier => System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX) ? Avalonia.Input.KeyModifiers.Meta : Avalonia.Input.KeyModifiers.Control;

        public static Avalonia.Input.KeyModifiers GetModifier(Avalonia.Input.KeyModifiers modifier)
        {
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
            {
                if (modifier.HasFlag(Avalonia.Input.KeyModifiers.Control))
                {
                    return (modifier & ~Avalonia.Input.KeyModifiers.Control) | Avalonia.Input.KeyModifiers.Meta;
                }
                else
                {
                    return modifier;
                }
            }
            else
            {
                return modifier;
            }
        }

        public static string GetShortcutString((Avalonia.Input.Key key, Avalonia.Input.KeyModifiers modifier) shortcut)
        {
            if (shortcut.key == Avalonia.Input.Key.None)
            {
                return null;
            }

            return GetModifier(shortcut.modifier).ToString().Replace(", ", " + ") + " + " + shortcut.key.ToString();
        }

        internal static Dictionary<string, Assembly> ExternalAssemblies = new Dictionary<string, Assembly>();

        public static SimpleFontLibrary FontLibrary = new SimpleFontLibrary();

        public static System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();

        public static async Task DownloadFileTaskAsync(this System.Net.Http.HttpClient client, Uri address, string fileName)
        {
            if (address.Scheme != "file")
            {
                using (Stream remoteStream = await client.GetStreamAsync(address))
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create))
                    {
                        await remoteStream.CopyToAsync(fs);
                    }
                }
            }
            else
            {
                File.Copy(address.AbsolutePath, fileName, true);
            }
        }

        public static async Task DownloadFileTaskAsync(this System.Net.Http.HttpClient client, Uri address, string fileName, IProgress<double> progress)
        {
            using System.Net.Http.HttpResponseMessage response = await client.GetAsync(address, System.Net.Http.HttpCompletionOption.ResponseHeadersRead);
            long totalLength = response.Content.Headers.ContentLength ?? 0;

            using Stream remoteStream = await response.Content.ReadAsStreamAsync();
            using FileStream fs = new FileStream(fileName, FileMode.Create);

            await remoteStream.CopyToAsync(fs, progress: new Progress<long>((p) =>
            {
                progress.Report((double)p / totalLength);
            }));

            progress.Report(1);
        }

        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize = 81920, IProgress<long> progress = null)
        {
            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }



        public static void DownloadFile(this System.Net.Http.HttpClient client, string address, string fileName)
        {
            using System.Net.Http.HttpRequestMessage message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, address);

            using System.Net.Http.HttpResponseMessage response = client.Send(message);

            using (FileStream fs = new FileStream(fileName, FileMode.Create))
            {
                response.Content.ReadAsStream().CopyTo(fs);
            }
        }

        public static Task<string> DownloadStringTaskAsync(this System.Net.Http.HttpClient client, string address)
        {
            using System.Net.Http.HttpRequestMessage message = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, address);
            message.Headers.Add("User-Agent", "arklumpus/TreeViewer");

            using System.Net.Http.HttpResponseMessage response = client.Send(message);

            return response.Content.ReadAsStringAsync();
        }

        static Modules()
        {
            if (!Directory.Exists(ModulePath))
            {
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux) || System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    string path = Path.Combine(Environment.GetEnvironmentVariable("HOME"), ".local", "share");
                    Directory.CreateDirectory(path);
                }

                Directory.CreateDirectory(ModulePath);
            }

            if (!Directory.Exists(Path.Combine(ModulePath, "assets")))
            {
                Directory.CreateDirectory(Path.Combine(ModulePath, "assets"));
            }

            if (!Directory.Exists(Path.Combine(ModulePath, "libraries")))
            {
                Directory.CreateDirectory(Path.Combine(ModulePath, "libraries"));
            }

            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                if (ExternalAssemblies.TryGetValue(e.Name, out Assembly ass))
                {
                    return ass;
                }
                else
                {
                    return null;
                }
            };

            VectSharp.FontFamily.DefaultFontLibrary = FontLibrary;
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.RobotoMono-Regular.ttf")));
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.RobotoMono-Italic.ttf")));
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.OpenSans-Regular.ttf")));
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.OpenSans-Bold.ttf")));
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.OpenSans-Italic.ttf")));
            FontLibrary.Add(new FontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.OpenSans-BoldItalic.ttf")));
        }

        public static CSharpEditor.InterprocessDebuggerServer GetNewDebuggerServer()
        {
            string exeName = null;

            if (IsWindows)
            {
                exeName = "DebuggerClient.exe";
            }
            else if (IsMac || IsLinux)
            {
                exeName = "DebuggerClient";
            }

            string myPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

#if DEBUG
            return new CSharpEditor.InterprocessDebuggerServer(Path.Combine(myPath, "..", "..", "..", "..", "DebuggerClient", "bin", "Debug", "net6.0", exeName));
#else
            if (!IsMac)
            {
                return new CSharpEditor.InterprocessDebuggerServer(Path.Combine(myPath, exeName));
            }
            else
            {
                return new CSharpEditor.InterprocessDebuggerServer("open", new List<string>() { Path.Combine(myPath, "..", "Resources", "DebuggerClient.app"), "-n", "--args" }, (id) =>
                {
                    System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("pgrep", "-n DebuggerClient") { RedirectStandardOutput = true };
                    psi.UseShellExecute = false;

                    System.Diagnostics.Process proc = System.Diagnostics.Process.Start(psi);
                    proc.WaitForExit();

                    string output = proc.StandardOutput.ReadToEnd();

                    return int.Parse(output);
                });
            }
#endif
        }

        public static VectSharp.FontFamily CodeFontFamily = new VectSharp.ResourceFontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.RobotoMono-Regular.ttf"), "resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono");

        public static bool IsMac
        {
            get
            {
                return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX);
            }
        }

        public static bool IsWindows
        {
            get
            {
                return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            }
        }


        public static bool IsLinux
        {
            get
            {
                return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            }
        }



        public const string RootNodeId = "650a0ef5-5322-4511-ae86-68bd87b47ecd";
        public const string ModuleIDKey = "eee4a077-63b2-4126-9f35-5f66d40aa2cf";
        public const string FileMenuFirstAreaId = "638a680b-fbc4-4ed5-adca-b2819b71b986";
        public const string FileMenuThirdAreaId = "c46dd264-4831-4a8a-a979-0884e293c6c8";
        public static readonly string[] AttributeTypes = { "String", "Number" };

        public static readonly string[] DefaultAttributeConverters = { @"public static string Format(object attribute)
{
    return attribute as string;
}",
            @"public static string Format(object attribute)
{
    if (attribute is double attributeValue &&
        !double.IsNaN(attributeValue))
    {
        if (attributeValue >= 0)
        {
            return attributeValue.ToString(2, false);
        }
        else
        {
            return null;
        }
    }
    else
    {
        return null;
    }
}"
    };

        public static readonly string[] SafeAttributeConverters = { @"public static string Format(object attribute)
{
    return attribute as string ?? """";
}",
            @"public static string Format(object attribute)
{
    if (attribute is double attributeValue)
    {
        return attributeValue.ToString();
    }
    else
    {
        return ""NaN"";
    }
}"
    };

        public static readonly string[] DefaultAttributeConvertersToColour = { @"public static Colour? Format(object attribute)
{
    if (attribute is string colour)
    {
        return Colour.FromCSSString(colour);
    }
    else
    {
        return null;
    }
}",
            @"static Gradient gradient = new Gradient(
new List<TreeViewer.GradientStop>()
{
    new TreeViewer.GradientStop(0, Colour.FromRgba(0d, 0d, 0d, 0d)),
    new TreeViewer.GradientStop(1, Colour.FromRgba(0d, 0d, 0d, 1d))
});

public static Colour? Format(object attribute)
{
    if (attribute is double attributeValue &&
        !double.IsNaN(attributeValue))
    {
        double position = (attributeValue - 0) / 1;
        position = Math.Max(Math.Min(position, 1), 0);
        return gradient.GetColour(position);
    }
    else
    {
        return null;
    }
}"
    };

        public static readonly ColourFormatterOptions[] DefaultAttributeConvertersToColourCompiled =
        {
            new ColourFormatterOptions((object attribute) =>
            {
                if (attribute is string colour)
                {
                    return Colour.FromCSSString(colour);
                }
                else
                {
                    return null;
                }
            }),
            new ColourFormatterOptions((object attribute) =>
            {
                if (attribute is double attributeValue && !double.IsNaN(attributeValue))
                {
                    double position = (attributeValue - 0) / 1;
                    position = Math.Max(Math.Min(position, 1), 0);
                    return DefaultGradients["TransparentToBlack"].GetColour(position);
                }
                else
                {
                    return null;
                }
            })
        };

        public static readonly string[] DefaultAttributeConvertersToDouble = { @"public static double? Format(object attribute)
{
    if (attribute is string number)
    {
        if (double.TryParse(number, out double parsed))
        {
            return parsed;
        }
        else
        {
            return null;
        }
    }
    else
    {
        return null;
    }
}",
            @"public static double? Format(object attribute)
{
    if (attribute is double attributeValue &&
        !double.IsNaN(attributeValue))
    {
        return attributeValue;
    }
    else
    {
        return null;
    }
}"
    };

        public static readonly Colour[] DefaultColours = { Colour.FromRgb(237, 28, 36), Colour.FromRgb(34, 177, 76), Colour.FromRgb(255, 127, 39), Colour.FromRgb(0, 162, 232), Colour.FromRgb(255, 242, 0), Colour.FromRgb(63, 72, 204), Colour.FromRgb(255, 201, 14), Colour.FromRgb(163, 73, 164), Colour.FromRgb(181, 230, 29) };

        public static readonly Dictionary<string, Gradient> DefaultGradients = new Dictionary<string, Gradient>()
        {
            { "TransparentToBlack", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgba(0, 0, 0, 0)), new GradientStop(1, Colour.FromRgb(0, 0, 0)) }) },
            { "WhiteToBlack", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(255, 255, 255)), new GradientStop(1, Colour.FromRgb(0, 0, 0)) }) },
            { "RedToGreen", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(237, 28, 36)), new GradientStop(1, Colour.FromRgb(34, 177, 76)) }) },
            { "Rainbow", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(237, 28, 36)), new GradientStop(1.0 / 6, Colour.FromRgb(255, 127, 39)), new GradientStop(1.0 / 3, Colour.FromRgb(255, 242, 0)),
                                                                 new GradientStop(0.5, Colour.FromRgb(34, 177, 76)), new GradientStop(2.0 / 3, Colour.FromRgb(0, 162, 232)), new GradientStop(5.0 / 6, Colour.FromRgb(63, 72, 204)), new GradientStop(1, Colour.FromRgb(163, 73, 164)) }) },
            { "Viridis", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(0.267004, 0.004874, 0.329415)), new GradientStop(0.09803921568627451, Colour.FromRgb(0.282623, 0.140926, 0.457517)), new GradientStop(0.2, Colour.FromRgb(0.253935, 0.265254, 0.529983)), new GradientStop(0.2980392156862745, Colour.FromRgb(0.206756, 0.371758, 0.553117)), new GradientStop(0.4, Colour.FromRgb(0.163625, 0.471133, 0.558148)), new GradientStop(0.4980392156862745, Colour.FromRgb(0.128729, 0.563265, 0.551229)), new GradientStop(0.6, Colour.FromRgb(0.134692, 0.658636, 0.517649)), new GradientStop(0.6980392156862745, Colour.FromRgb(0.259857, 0.745492, 0.444467)), new GradientStop(0.796078431372549, Colour.FromRgb(0.468053, 0.818921, 0.323998)), new GradientStop(0.8980392156862745, Colour.FromRgb(0.730889, 0.871916, 0.156029)), new GradientStop(1, Colour.FromRgb(0.993248, 0.906157, 0.143936)) }) },


            { "RedYellowGreen", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(237, 28, 36)), new GradientStop(0.5, Colour.FromRgb(255, 242, 0)), new GradientStop(1, Colour.FromRgb(34, 177, 76)) }) },

            { "WongRainbow", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(204, 121, 167)), new GradientStop(1.0 / 6, Colour.FromRgb(213, 94, 0)), new GradientStop(1.0 / 3, Colour.FromRgb(230, 159, 0)),
                                                                 new GradientStop(0.5, Colour.FromRgb(240, 228, 66)), new GradientStop(2.0 / 3, Colour.FromRgb(0, 158, 115)), new GradientStop(5.0 / 6, Colour.FromRgb(0, 114, 178)), new GradientStop(1, Colour.FromRgb(86, 180, 233)) }) },

            { "WongDiscrete", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(204, 121, 167)), new GradientStop(0.2, Colour.FromRgb(204, 121, 167)), new GradientStop(0.201, Colour.FromRgb(230, 159, 0)), new GradientStop(0.4, Colour.FromRgb(230, 159, 0)),
                                                                 new GradientStop(0.401, Colour.FromRgb(0, 158, 115)), new GradientStop(0.6, Colour.FromRgb(0, 158, 115)), new GradientStop(0.601, Colour.FromRgb(0, 114, 178)), new GradientStop(0.8, Colour.FromRgb(0, 114, 178)), new GradientStop(0.801, Colour.FromRgb(86, 180, 233)), new GradientStop(1, Colour.FromRgb(86, 180, 233)) }) },

            { "Muted", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(221, 204, 119)), new GradientStop(0.125, Colour.FromRgb(153, 153, 51)), new GradientStop(0.25, Colour.FromRgb(17, 119, 51)),
                                                               new GradientStop(0.375, Colour.FromRgb(68, 170, 153)), new GradientStop(0.5, Colour.FromRgb(136, 204, 238)), new GradientStop(0.625, Colour.FromRgb(51, 34, 136)),
                                                               new GradientStop(0.75, Colour.FromRgb(170, 68, 153)), new GradientStop(0.875, Colour.FromRgb(136, 34, 85)), new GradientStop(1, Colour.FromRgb(204, 102, 119)) }) },

            { "MutedDiscrete", new Gradient(new List<GradientStop>() { new GradientStop(0.101, Colour.FromRgb(187, 187, 187)), new GradientStop(0.1, Colour.FromRgb(187, 187, 187)), new GradientStop(0.101, Colour.FromRgb(221, 204, 119)), new GradientStop(0.2, Colour.FromRgb(221, 204, 119)), new GradientStop(0.201, Colour.FromRgb(153, 153, 51)), new GradientStop(0.3, Colour.FromRgb(153, 153, 51)),
                                                               new GradientStop(0.301, Colour.FromRgb(17, 119, 51)), new GradientStop(0.4, Colour.FromRgb(17, 119, 51)), new GradientStop(0.401, Colour.FromRgb(68, 170, 153)), new GradientStop(0.5, Colour.FromRgb(68, 170, 153)),
                                                                new GradientStop(0.501, Colour.FromRgb(136, 204, 238)), new GradientStop(0.6, Colour.FromRgb(136, 204, 238)), new GradientStop(0.601, Colour.FromRgb(51, 34, 136)), new GradientStop(0.7, Colour.FromRgb(51, 34, 136)),
                                                               new GradientStop(0.701, Colour.FromRgb(170, 68, 153)), new GradientStop(0.8, Colour.FromRgb(170, 68, 153)), new GradientStop(0.801, Colour.FromRgb(136, 34, 85)), new GradientStop(0.9, Colour.FromRgb(136, 34, 85)), new GradientStop(0.901, Colour.FromRgb(204, 102, 119)), new GradientStop(1, Colour.FromRgb(204, 102, 119))}) },
        };

        public static string ModulePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "modules");
        public static string ModuleListPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "modules.json");

        public static Avalonia.Media.FontFamily UIFontFamily = Avalonia.Media.FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans");

        public static List<FileTypeModule> FileTypeModules { get; } = new List<FileTypeModule>();
        public static List<LoadFileModule> LoadFileModules { get; } = new List<LoadFileModule>();
        public static List<TransformerModule> TransformerModules { get; } = new List<TransformerModule>();
        public static List<FurtherTransformationModule> FurtherTransformationModules { get; } = new List<FurtherTransformationModule>();
        public static List<CoordinateModule> CoordinateModules { get; } = new List<CoordinateModule>();
        public static List<PlottingModule> PlottingModules { get; } = new List<PlottingModule>();
        public static List<SelectionActionModule> SelectionActionModules { get; } = new List<SelectionActionModule>();
        public static List<ActionModule> ActionModules { get; } = new List<ActionModule>();

        public static List<MenuActionModule> MenuActionModules = new List<MenuActionModule>();

        public static event EventHandler<ModuleLoadedEventArgs> ModuleLoaded;

        public class ModuleLoadedEventArgs : EventArgs
        {
            public ModuleMetadata LoadedModuleMetadata { get; }
            public Module LoadedModule { get; }

            public ModuleLoadedEventArgs(ModuleMetadata loadedModuleMetadata, Module loadedModule) : base()
            {
                this.LoadedModuleMetadata = loadedModuleMetadata;
                this.LoadedModule = loadedModule;
            }
        }

        public static T GetModule<T>(List<T> moduleCollection, string id) where T : Module
        {
            for (int i = 0; i < moduleCollection.Count; i++)
            {
                if (moduleCollection[i].Id == id)
                {
                    return moduleCollection[i];
                }
            }

            return null;
        }

        public static bool HasModule<T>(List<T> moduleCollection, string id) where T : Module
        {
            for (int i = 0; i < moduleCollection.Count; i++)
            {
                if (moduleCollection[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static Dictionary<string, ModuleMetadata> LoadedModulesMetadata { get; } = new Dictionary<string, ModuleMetadata>();

        public static Module LoadModule(ModuleMetadata metaData)
        {
            Assembly assembly;
            using (MemoryStream ms = new MemoryStream())
            {
                metaData.Build(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                assembly = System.Reflection.Assembly.Load(ms.ToArray());
            }

            Type type = ModuleMetadata.GetTypeFromAssembly(assembly, "MyModule");
            Module loadedModule = metaData.Load(type, true);
            switch (metaData.ModuleType)
            {
                case ModuleTypes.Action:
                    Modules.ActionModules.Add((ActionModule)loadedModule);
                    break;
                case ModuleTypes.Coordinate:
                    Modules.CoordinateModules.Add((CoordinateModule)loadedModule);
                    break;
                case ModuleTypes.FileType:
                    Modules.FileTypeModules.Add((FileTypeModule)loadedModule);
                    break;
                case ModuleTypes.FurtherTransformation:
                    Modules.FurtherTransformationModules.Add((FurtherTransformationModule)loadedModule);
                    break;
                case ModuleTypes.LoadFile:
                    Modules.LoadFileModules.Add((LoadFileModule)loadedModule);
                    break;
                case ModuleTypes.MenuAction:
                    Modules.MenuActionModules.Add((MenuActionModule)loadedModule);
                    break;
                case ModuleTypes.Plotting:
                    Modules.PlottingModules.Add((PlottingModule)loadedModule);
                    break;
                case ModuleTypes.SelectionAction:
                    Modules.SelectionActionModules.Add((SelectionActionModule)loadedModule);
                    break;
                case ModuleTypes.Transformer:
                    Modules.TransformerModules.Add((TransformerModule)loadedModule);
                    break;
            }

            Modules.LoadedModulesMetadata.Add(metaData.Id, metaData);
            ModuleLoaded?.Invoke(null, new ModuleLoadedEventArgs(metaData, loadedModule));
            return loadedModule;
        }

        public static Module LoadPreCompiledModule(ModuleMetadata metaData, string fileName)
        {
            for (int i = 0; i < metaData.AdditionalReferences.Length; i++)
            {
                string actualPath = ModuleMetadata.LocateReference(metaData.AdditionalReferences[i]);

                if (Path.GetDirectoryName(actualPath) != Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                {
                    Assembly refAss = Assembly.LoadFile(actualPath);

                    try
                    {
                        AppDomain.CurrentDomain.Load(refAss.GetName());
                    }
                    catch (FileNotFoundException)
                    {
                        ExternalAssemblies.Add(refAss.FullName, refAss);
                    }
                }
            }

            Assembly ass = Assembly.LoadFrom(fileName);
            Type compiledModule = ModuleMetadata.GetTypeFromAssembly(ass, "MyModule");
            Module loadedModule = metaData.Load(compiledModule, true);

            LoadModule(metaData, loadedModule);

            return loadedModule;
        }

        public static void LoadModule(ModuleMetadata metaData, Module loadedModule)
        {
            switch (metaData.ModuleType)
            {
                case ModuleTypes.Action:
                    Modules.ActionModules.Add((ActionModule)loadedModule);
                    break;
                case ModuleTypes.Coordinate:
                    Modules.CoordinateModules.Add((CoordinateModule)loadedModule);
                    break;
                case ModuleTypes.FileType:
                    Modules.FileTypeModules.Add((FileTypeModule)loadedModule);
                    break;
                case ModuleTypes.FurtherTransformation:
                    Modules.FurtherTransformationModules.Add((FurtherTransformationModule)loadedModule);
                    break;
                case ModuleTypes.LoadFile:
                    Modules.LoadFileModules.Add((LoadFileModule)loadedModule);
                    break;
                case ModuleTypes.MenuAction:
                    Modules.MenuActionModules.Add((MenuActionModule)loadedModule);
                    break;
                case ModuleTypes.Plotting:
                    Modules.PlottingModules.Add((PlottingModule)loadedModule);
                    break;
                case ModuleTypes.SelectionAction:
                    Modules.SelectionActionModules.Add((SelectionActionModule)loadedModule);
                    break;
                case ModuleTypes.Transformer:
                    Modules.TransformerModules.Add((TransformerModule)loadedModule);
                    break;
            }

            Modules.LoadedModulesMetadata.Add(metaData.Id, metaData);
            ModuleLoaded?.Invoke(null, new ModuleLoadedEventArgs(metaData, loadedModule));
        }

        public static List<ModuleMetadata> MissingModules;

        public static async Task LoadInstalledModules(bool updateGlobalSettings, SplashScreen splash)
        {
            try
            {
                List<ModuleMetadata> installedModules = JsonSerializer.Deserialize<List<ModuleMetadata>>(File.ReadAllText(ModuleListPath), Modules.DefaultSerializationOptions);
                MissingModules = new List<ModuleMetadata>();

                if (splash != null)
                {
                    for (int i = 0; i < installedModules.Count; i++)
                    {
                        splash.SetProgress((double)(i) / installedModules.Count, installedModules[i].Name);
                        await Task.Run(() =>
                        {
                            string modulePath = Path.Combine(ModulePath, installedModules[i].Id + ".dll");
                            if (File.Exists(modulePath))
                            {
                                installedModules[i].IsInstalled = true;
                                LoadPreCompiledModule(installedModules[i], modulePath);
                            }
                            else
                            {
                                MissingModules.Add(installedModules[i]);
                            }
                        });
                        splash.SetProgress((double)(i + 1) / installedModules.Count, installedModules[i].Name);
                    }

                    splash.SetProgress(1, "");
                    await Task.Delay(100);
                }
                else
                {
                    for (int i = 0; i < installedModules.Count; i++)
                    {
                        string modulePath = Path.Combine(ModulePath, installedModules[i].Id + ".dll");
                        if (File.Exists(modulePath))
                        {
                            installedModules[i].IsInstalled = true;
                            LoadPreCompiledModule(installedModules[i], modulePath);
                        }
                        else
                        {
                            MissingModules.Add(installedModules[i]);
                        }
                    }
                }

                if (updateGlobalSettings)
                {
                    if (!Avalonia.Threading.Dispatcher.UIThread.CheckAccess())
                    {
                        await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            TreeViewer.GlobalSettings.Settings.UpdateAdditionalSettings();
                        });
                    }
                    else
                    {
                        TreeViewer.GlobalSettings.Settings.UpdateAdditionalSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        try
                        {
                            new MessageBox("Attention", "An error occurred while loading the installed modules!\nPlease run the program with the --rebuild-all-modules switch to fix this.\n" + ex.Message) { Topmost = true }.Show();
                        }
                        catch
                        {

                        }
                    });
                }
                catch
                {

                }
            }
        }

        public static void RebuildAllModules()
        {
            List<ModuleMetadata> installedModules = JsonSerializer.Deserialize<List<ModuleMetadata>>(File.ReadAllText(ModuleListPath), Modules.DefaultSerializationOptions);

            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            List<string> filesToCopy = new List<string>();

            for (int i = 0; i < installedModules.Count; i++)
            {
                string modulePath = Path.Combine(ModulePath, installedModules[i].Id + ".dll");

                string newModuleDLL = Path.Combine(tempDir, Guid.NewGuid().ToString());

                using (FileStream fs = new FileStream(newModuleDLL, FileMode.Create))
                {
                    installedModules[i].Build(fs);
                }

                filesToCopy.Add(Path.GetFullPath(newModuleDLL));
                filesToCopy.Add(Path.GetFullPath(modulePath));
            }

            try
            {
                for (int j = 0; j < filesToCopy.Count; j += 2)
                {
                    File.Copy(filesToCopy[j], filesToCopy[j + 1], true);
                }
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        private static List<CSharpEditor.CachedMetadataReference> cachedReferences = null;

        internal static List<CSharpEditor.CachedMetadataReference> CachedReferences
        {
            get
            {
                if (cachedReferences == null)
                {
                    RebuildCachedReferences();
                }
                return cachedReferences;
            }
        }

        internal static void RebuildCachedReferences()
        {
            cachedReferences = new List<CSharpEditor.CachedMetadataReference>();

            foreach (Assembly ass in AppDomain.CurrentDomain.GetAssemblies())
            {
                string location = null;

                try
                {
                    location = ass.Location;
                }
                catch (NotSupportedException) { };

                if (!string.IsNullOrEmpty(location))
                {
                    cachedReferences.Add(CSharpEditor.CachedMetadataReference.CreateFromFile(location, Path.Combine(Path.GetDirectoryName(location), Path.GetFileNameWithoutExtension(location) + ".xml")));
                }
            }
        }

        public static JsonSerializerOptions DefaultSerializationOptions = new JsonSerializerOptions()
        {
            Converters =
            {
                new VersionConverter()
            }
        };

        public async static Task<TreeCollection> OpenTreeFile(string fileName, Avalonia.Controls.Window parentWindow)
        {
            double maxResult = 0;
            int maxIndex = -1;

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                    if (priority > maxResult)
                    {
                        maxResult = priority;
                        maxIndex = i;
                    }
                }
                catch { }
            }

            if (maxIndex >= 0)
            {
                double maxLoadResult = 0;
                int maxLoadIndex = -1;

                IEnumerable<TreeNode> loader;

                try
                {
                    List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };

                    EventWaitHandle progressWindowHandle = new EventWaitHandle(false, EventResetMode.ManualReset);
                    ProgressWindow progressWin = new ProgressWindow(progressWindowHandle) { ProgressText = "Opening and loading file..." };
                    SemaphoreSlim progressSemaphore = new SemaphoreSlim(0, 1);
                    Action<double> progressAction = (progress) =>
                    {
                        if (progress >= 0)
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progressWin.IsIndeterminate = false;
                                progressWin.Progress = progress;
                            });
                        }
                        else
                        {
                            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                progressWin.IsIndeterminate = true;
                            });
                        }
                    };

                    TreeCollection coll = null;

                    Thread thr = new Thread(async () =>
                    {
                        progressWindowHandle.WaitOne();

                        Action<double> openerProgressAction = (_) => { };

                        bool? codePermissionGranted = null;

                        bool askForCodePermission(RSAParameters? publicKey)
                        {
                            if (codePermissionGranted.HasValue)
                            {
                                return codePermissionGranted.Value;
                            }
                            else
                            {
                                MessageBox box = null;

                                EventWaitHandle handle = new EventWaitHandle(false, EventResetMode.ManualReset);

                                async void showDialog()
                                {
                                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                                    {
                                        box = new MessageBox("Attention!", "The selected file contains source code and its signature does not match any known keys. Do you want to load and compile it? You should only do this if you trust the source of the file and/or you have accurately reviewed the code.", MessageBox.MessageBoxButtonTypes.YesNo);
                                        await box.ShowDialog2(parentWindow);

                                        if (box.Result == MessageBox.Results.Yes && publicKey.HasValue)
                                        {
                                            MessageBox box2 = new MessageBox("Question", "Would you like to add the file's public key to the local storage? This will allow you to open other files produced by the same author without seeing this dialog. You should only do this if you trust the source of the file.", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                                            await box2.ShowDialog2(parentWindow);
                                            if (box2.Result == MessageBox.Results.Yes)
                                            {
                                                CryptoUtils.AddPublicKey(publicKey.Value);
                                            }
                                        }
                                    });

                                    handle.Set();
                                }

                                showDialog();

                                handle.WaitOne();

                                if (box.Result == MessageBox.Results.Yes)
                                {
                                    codePermissionGranted = true;
                                    return true;
                                }
                                else
                                {
                                    codePermissionGranted = false;
                                    return false;
                                }
                            }
                        };

                        loader = Modules.FileTypeModules[maxIndex].OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

                        FileInfo finfo = new FileInfo(fileName);

                        for (int i = 0; i < Modules.LoadFileModules.Count; i++)
                        {
                            try
                            {
                                double priority = Modules.LoadFileModules[i].IsSupported(finfo, Modules.FileTypeModules[maxIndex].Id, loader);
                                if (priority > maxLoadResult)
                                {
                                    maxLoadResult = priority;
                                    maxLoadIndex = i;
                                }
                            }
                            catch { }
                        }

                        if (maxLoadIndex >= 0)
                        {
                            try
                            {
                                coll = Modules.LoadFileModules[maxLoadIndex].Load(parentWindow, finfo, Modules.FileTypeModules[maxIndex].Id, loader, moduleSuggestions, ref openerProgressAction, progressAction);
                            }
                            catch (Exception ex)
                            {
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                                await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Error!", "An error has occurred while loading the file!\n" + ex.Message).ShowDialog2(parentWindow); return Task.CompletedTask; });
                            }

                            progressSemaphore.Release();
                        }
                        else
                        {
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await new MessageBox("Attention!", "The file cannot be loaded by any of the currently installed modules!").ShowDialog2(parentWindow); return Task.CompletedTask; });
                            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                        }
                    });

                    thr.Start();

                    _ = progressWin.ShowDialog2(parentWindow);

                    await progressSemaphore.WaitAsync();
                    progressSemaphore.Release();
                    progressSemaphore.Dispose();

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { progressWin.Close(); });
                    return coll;
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "An error has occurred while opening the file!\n" + ex.Message).ShowDialog2(parentWindow);
                    return null;
                }
            }
            else
            {
                await new MessageBox("Attention!", "The file type is not supported by any of the currently installed modules!").ShowDialog2(parentWindow);
                return null;
            }
        }
    }


    public abstract class Module
    {
        public string Name { get; set; }

        public string Id { get; set; }

        public string HelpText { get; set; }
    }

    public delegate IEnumerable<TreeNode> OpenFileDelegate(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission);

    public class FileTypeModule : Module
    {
        public string[] Extensions { get; set; }
        public Func<string, double> IsSupported { get; set; }
        public OpenFileDelegate OpenFile { get; set; }

        public FileTypeModule()
        {

        }
    }

    public delegate TreeCollection LoadFileDelegate(Avalonia.Controls.Window parentWindow, FileInfo fileInfo, string filetypeModuleId, IEnumerable<TreeNode> loader, List<(string, Dictionary<string, object>)> moduleSuggestions, ref Action<double> openerProgressAction, Action<double> progressAction);

    public class LoadFileModule : Module
    {
        public LoadFileDelegate Load { get; set; }
        public Func<FileInfo, string, IEnumerable<TreeNode>, double> IsSupported { get; set; }

        public LoadFileModule()
        {

        }
    }

    public delegate bool ParameterChangeDelegate(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);

    public delegate bool GenericParameterChangeDelegate(Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange);

    public class TransformerModule : Module
    {
        public Func<TreeCollection, List<(string, string)>> GetParameters { get; set; }
        public ParameterChangeDelegate OnParameterChange { get; set; }
        public Func<TreeCollection, Dictionary<string, object>, Action<double>, TreeNode> Transform { get; set; }

        public TransformerModule()
        {

        }

    }

    public class CoordinateModule : Module
    {
        public Func<TreeNode, List<(string, string)>> GetParameters { get; set; }
        public ParameterChangeDelegate OnParameterChange { get; set; }
        public Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>> GetCoordinates { get; set; }

        public CoordinateModule()
        {

        }
    }


    public class PlottingModule : Module
    {
        public Func<TreeNode, List<(string, string)>> GetParameters { get; set; }
        public ParameterChangeDelegate OnParameterChange { get; set; }
        public Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>, Graphics, Point[]> PlotAction { get; set; }
        public Func<double, Page> GetIcon { get; set; }

        public PlottingModule()
        {

        }
    }


    public class FurtherTransformationModule : Module
    {
        public delegate void TreeTransformDelegate(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction);

        public Func<TreeNode, List<(string, string)>> GetParameters { get; set; }
        public ParameterChangeDelegate OnParameterChange { get; set; }
        public TreeTransformDelegate Transform { get; set; }
        public Func<double, Page> GetIcon { get; set; }

        public bool Repeatable { get; set; }

        public FurtherTransformationModule()
        {

        }
    }


    public class SelectionActionModule : Module
    {
        public string ButtonText { get; set; }
        public Action<int, TreeNode, MainWindow, InstanceStateData> PerformAction { get; set; }
        public Func<TreeNode, MainWindow, InstanceStateData, List<bool>> IsAvailable { get; set; }
        public Avalonia.Input.Key ShortcutKey { get; set; }
        public Avalonia.Input.KeyModifiers ShortcutModifier { get; set; }
        public bool TriggerInTextBox { get; set; }
        public bool IsAvailableInCommandLine { get; set; }

        public string GroupName { get; set; }
        public double GroupIndex { get; set; }
        public Func<double, Page> GetIcon { get; set; }
        public bool IsLargeButton { get; set; }
        public List<(string, Func<double, Page>)> SubItems { get; set; }

        public SelectionActionModule()
        {

        }
    }

    public class ActionModule : Module
    {
        public string ButtonText { get; set; }
        public Func<double, Page> GetIcon { get; set; }
        public Action<int, MainWindow, InstanceStateData> PerformAction { get; set; }
        public List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; set; }
        public bool TriggerInTextBox { get; set; }
        public bool IsAvailableInCommandLine { get; set; }
        public string GroupName { get; set; }
        public bool IsLargeButton { get; set; }
        public double GroupIndex { get; set; }
        public List<(string, Func<double, Page>)> SubItems { get; set; }
        public ActionModule()
        {

        }
    }


    public class MenuActionModule : Module
    {
        public string ItemText { get; set; }
        public string ParentMenu { get; set; }
        public string GroupName { get; set; }
        public Func<int, MainWindow, Task> PerformAction { get; set; }
        public Func<MainWindow, List<bool>> IsEnabled { get; set; }
        public List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; set; }
        public bool TriggerInTextBox { get; set; }
        public List<Avalonia.AvaloniaProperty> PropertiesAffectingEnabled { get; set; }

        public double GroupIndex { get; set; }
        public Func<double, Page> GetIcon { get; set; }
        public bool IsLargeButton { get; set; }
        public List<(string, Func<double, Page>)> SubItems { get; set; }
        public Func<Avalonia.Controls.Control> GetFileMenuPage { get; set; }

        public MenuActionModule()
        {

        }
    }

    public class ModuleHeader
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        public ModuleTypes ModuleType { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }

        public ModuleHeader()
        {

        }

        public ModuleHeader(ModuleMetadata metadata)
        {
            this.Id = metadata.Id;
            this.Name = metadata.Name;
            this.HelpText = metadata.HelpText;
            this.ModuleType = metadata.ModuleType;
            this.Author = metadata.Author;
            this.Version = metadata.Version;
        }
    }

    public enum ModuleTypes { FileType, LoadFile, Transformer, FurtherTransformation, Coordinate, Plotting, SelectionAction, Action, MenuAction }

    public class ModuleMetadata
    {
        public bool IsInstalled = false;
        public string Id { get; set; }
        public string Name { get; set; }
        public string HelpText { get; set; }
        public ModuleTypes ModuleType { get; set; }
        public string SourceCode { get; set; }
        public string[] AdditionalReferences { get; set; }
        public string SourceSignature { get; set; }
        public string[] ReferenceSignatures { get; set; }
        public string Author { get; set; }
        public Version Version { get; set; }

        public static ModuleMetadata CreateFromSource(string sourceCode, string[] additionalReferences)
        {
            Type compiledModule = Compile(sourceCode, "MyModule", additionalReferences);

            ModuleMetadata meta = new ModuleMetadata()
            {
                SourceCode = sourceCode,
                AdditionalReferences = additionalReferences,
                Id = (string)GetField(compiledModule, "Id"),
                Name = (string)GetField(compiledModule, "Name"),
                Author = (string)GetField(compiledModule, "Author"),
                HelpText = (string)GetField(compiledModule, "HelpText"),
                ModuleType = (ModuleTypes)GetField(compiledModule, "ModuleType"),
                Version = (Version)GetField(compiledModule, "Version")
            };

            return meta;
        }

        public void Build(Stream outputStream)
        {
            CompileToStream(this.SourceCode, this.AdditionalReferences, outputStream);
        }

        public Module Load(Type compiledModule, bool loadGlobalSettings)
        {
            if (loadGlobalSettings && compiledModule.GetMethod("GetGlobalSettings") != null)
            {
                List<(string, string)> globalSettings = ((Func<List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<List<(string, string)>>), compiledModule, "GetGlobalSettings"))();
                foreach ((string, string) element in globalSettings)
                {
                    TreeViewer.GlobalSettings.Settings.AdditionalSettingsList[element.Item1] = element.Item2;
                }
            }

            Version = (Version)GetField(compiledModule, "Version");

            if (this.ModuleType == ModuleTypes.Action)
            {
                ActionModule mod = new ActionModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    ButtonText = (string)GetProperty(compiledModule, "ButtonText"),
                    IsAvailableInCommandLine = (bool)GetProperty(compiledModule, "IsAvailableInCommandLine"),
                    ShortcutKeys = (List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>)GetProperty(compiledModule, "ShortcutKeys"),
                    TriggerInTextBox = (bool)GetProperty(compiledModule, "TriggerInTextBox"),
                    GetIcon = (Func<double, Page>)Delegate.CreateDelegate(typeof(Func<double, Page>), compiledModule, "GetIcon"),
                    PerformAction = (Action<int, MainWindow, InstanceStateData>)Delegate.CreateDelegate(typeof(Action<int, MainWindow, InstanceStateData>), compiledModule, "PerformAction"),
                    SubItems = (List<(string, Func<double, Page>)>)GetProperty(compiledModule, "SubItems"),
                    GroupIndex = (double)GetProperty(compiledModule, "GroupIndex"),
                    GroupName = (string)GetProperty(compiledModule, "GroupName"),
                    IsLargeButton = (bool)GetProperty(compiledModule, "IsLargeButton")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.Coordinate)
            {
                CoordinateModule mod = new CoordinateModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    GetParameters = (Func<TreeNode, List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<TreeNode, List<(string, string)>>), compiledModule, "GetParameters"),
                    OnParameterChange = (ParameterChangeDelegate)Delegate.CreateDelegate(typeof(ParameterChangeDelegate), compiledModule, "OnParameterChange"),
                    GetCoordinates = (Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>>)Delegate.CreateDelegate(typeof(Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>>), compiledModule, "GetCoordinates")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.FileType)
            {
                FileTypeModule mod = new FileTypeModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    Extensions = (string[])GetProperty(compiledModule, "Extensions"),
                    IsSupported = (Func<string, double>)Delegate.CreateDelegate(typeof(Func<string, double>), compiledModule, "IsSupported"),
                    OpenFile = (OpenFileDelegate)Delegate.CreateDelegate(typeof(OpenFileDelegate), compiledModule, "OpenFile")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.FurtherTransformation)
            {
                FurtherTransformationModule mod = new FurtherTransformationModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    Repeatable = (bool)GetProperty(compiledModule, "Repeatable"),
                    GetParameters = (Func<TreeNode, List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<TreeNode, List<(string, string)>>), compiledModule, "GetParameters"),
                    OnParameterChange = (ParameterChangeDelegate)Delegate.CreateDelegate(typeof(ParameterChangeDelegate), compiledModule, "OnParameterChange"),
                    Transform = (FurtherTransformationModule.TreeTransformDelegate)Delegate.CreateDelegate(typeof(FurtherTransformationModule.TreeTransformDelegate), compiledModule, "Transform"),
                    GetIcon = (Func<double, Page>)Delegate.CreateDelegate(typeof(Func<double, Page>), compiledModule, "GetIcon"),
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.LoadFile)
            {
                LoadFileModule mod = new LoadFileModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    IsSupported = (Func<FileInfo, string, IEnumerable<TreeNode>, double>)Delegate.CreateDelegate(typeof(Func<FileInfo, string, IEnumerable<TreeNode>, double>), compiledModule, "IsSupported"),
                    Load = (LoadFileDelegate)Delegate.CreateDelegate(typeof(LoadFileDelegate), compiledModule, "Load")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.MenuAction)
            {
                MenuActionModule mod = new MenuActionModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    ItemText = (string)GetProperty(compiledModule, "ItemText"),
                    ParentMenu = (string)GetProperty(compiledModule, "ParentMenu"),
                    GroupName = (string)GetProperty(compiledModule, "GroupName"),
                    ShortcutKeys = (List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>)GetProperty(compiledModule, "ShortcutKeys"),
                    TriggerInTextBox = (bool)GetProperty(compiledModule, "TriggerInTextBox"),
                    IsEnabled = (Func<MainWindow, List<bool>>)Delegate.CreateDelegate(typeof(Func<MainWindow, List<bool>>), compiledModule, "IsEnabled"),
                    PerformAction = (Func<int, MainWindow, Task>)Delegate.CreateDelegate(typeof(Func<int, MainWindow, Task>), compiledModule, "PerformAction"),
                    GetIcon = (Func<double, Page>)Delegate.CreateDelegate(typeof(Func<double, Page>), compiledModule, "GetIcon"),
                    GroupIndex = (double)GetProperty(compiledModule, "GroupIndex"),
                    IsLargeButton = (bool)GetProperty(compiledModule, "IsLargeButton"),
                    SubItems = (List<(string, Func<double, Page>)>)GetProperty(compiledModule, "SubItems")
                };

                if (compiledModule.GetMethod("GetFileMenuPage") != null)
                {
                    mod.GetFileMenuPage = (Func<Avalonia.Controls.Control>)Delegate.CreateDelegate(typeof(Func<Avalonia.Controls.Control>), compiledModule, "GetFileMenuPage");
                }

                if (HasProperty(compiledModule, "PropertiesAffectingEnabled"))
                {
                    mod.PropertiesAffectingEnabled = (List<Avalonia.AvaloniaProperty>)GetProperty(compiledModule, "PropertiesAffectingEnabled");
                }
                else if (GetProperty(compiledModule, "PropertyAffectingEnabled") != null)
                {
                    mod.PropertiesAffectingEnabled = new List<Avalonia.AvaloniaProperty>() { (Avalonia.AvaloniaProperty)GetProperty(compiledModule, "PropertyAffectingEnabled") };
                }
                else
                {
                    mod.PropertiesAffectingEnabled = null;
                }

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.Plotting)
            {
                PlottingModule mod = new PlottingModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    GetParameters = (Func<TreeNode, List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<TreeNode, List<(string, string)>>), compiledModule, "GetParameters"),
                    OnParameterChange = (ParameterChangeDelegate)Delegate.CreateDelegate(typeof(ParameterChangeDelegate), compiledModule, "OnParameterChange"),
                    PlotAction = (Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>, Graphics, Point[]>)Delegate.CreateDelegate(typeof(Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>, Graphics, Point[]>), compiledModule, "PlotAction"),
                    GetIcon = (Func<double, Page>)Delegate.CreateDelegate(typeof(Func<double, Page>), compiledModule, "GetIcon")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.SelectionAction)
            {
                SelectionActionModule mod = new SelectionActionModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    ButtonText = (string)GetProperty(compiledModule, "ButtonText"),
                    ShortcutKey = (Avalonia.Input.Key)GetProperty(compiledModule, "ShortcutKey"),
                    ShortcutModifier = (Avalonia.Input.KeyModifiers)GetProperty(compiledModule, "ShortcutModifier"),
                    TriggerInTextBox = (bool)GetProperty(compiledModule, "TriggerInTextBox"),
                    IsAvailableInCommandLine = (bool)GetProperty(compiledModule, "IsAvailableInCommandLine"),
                    GetIcon = (Func<double, Page>)Delegate.CreateDelegate(typeof(Func<double, Page>), compiledModule, "GetIcon"),
                    IsAvailable = (Func<TreeNode, MainWindow, InstanceStateData, List<bool>>)Delegate.CreateDelegate(typeof(Func<TreeNode, MainWindow, InstanceStateData, List<bool>>), compiledModule, "IsAvailable"),
                    PerformAction = (Action<int, TreeNode, MainWindow, InstanceStateData>)Delegate.CreateDelegate(typeof(Action<int, TreeNode, MainWindow, InstanceStateData>), compiledModule, "PerformAction"),
                    GroupName = (string)GetProperty(compiledModule, "GroupName"),
                    GroupIndex = (double)GetProperty(compiledModule, "GroupIndex"),
                    IsLargeButton = (bool)GetProperty(compiledModule, "IsLargeButton"),
                    SubItems = (List<(string, Func<double, Page>)>)GetProperty(compiledModule, "SubItems")
                };

                return mod;
            }
            else if (this.ModuleType == ModuleTypes.Transformer)
            {
                TransformerModule mod = new TransformerModule()
                {
                    Name = this.Name,
                    Id = this.Id,
                    HelpText = this.HelpText,
                    GetParameters = (Func<TreeCollection, List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<TreeCollection, List<(string, string)>>), compiledModule, "GetParameters"),
                    OnParameterChange = (ParameterChangeDelegate)Delegate.CreateDelegate(typeof(ParameterChangeDelegate), compiledModule, "OnParameterChange"),
                    Transform = (Func<TreeCollection, Dictionary<string, object>, Action<double>, TreeNode>)Delegate.CreateDelegate(typeof(Func<TreeCollection, Dictionary<string, object>, Action<double>, TreeNode>), compiledModule, "Transform")
                };

                return mod;
            }
            else
            {
                return null;
            }
        }

        public static Type Compile(string source, string requestedType, string[] additionalReferences)
        {
            using var ms = new MemoryStream();

            CompileToStream(source, additionalReferences, ms);

            ms.Seek(0, SeekOrigin.Begin);
            Assembly assembly = Assembly.Load(ms.ToArray());

            return GetTypeFromAssembly(assembly, requestedType);
        }

        public static Type GetTypeFromAssembly(Assembly assembly, string requestedType)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.Name == requestedType || type.FullName == requestedType)
                {
                    return type;
                }
            }

            throw new Exception("The requested type was not found!");
        }

        public static void CompileToStream(string source, string[] additionalReferences, Stream outputStream)
        {
            SyntaxTree syntaxTree = SyntaxFactory.ParseSyntaxTree(SourceText.From(source));

            string assemblyName = Guid.NewGuid().ToString().Replace("-", "");

            List<MetadataReference> references = new List<MetadataReference>();

            foreach (string sr in additionalReferences)
            {
                references.Add(CSharpEditor.CachedMetadataReference.CreateFromFile(LocateReference(sr)));
            }

            CSharpCompilation compilation = CSharpCompilation.Create(assemblyName, new[] { syntaxTree }, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            EmitResult result = compilation.Emit(outputStream);

            if (!result.Success)
            {
                IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                    diagnostic.IsWarningAsError ||
                    diagnostic.Severity == DiagnosticSeverity.Error);

                StringBuilder message = new StringBuilder();

                foreach (Diagnostic diagnostic in failures)
                {
                    message.AppendLine(diagnostic.Id + ": " + diagnostic.GetMessage());
                }
                throw new Exception(message.ToString());
            }
        }

        public static Func<object[], object> GetMethod(Type type, string methodName)
        {
            return (object[] args) =>
            {
                return type.InvokeMember(methodName,
                    BindingFlags.Default | BindingFlags.InvokeMethod,
                    null,
                    null,
                    args);
            };
        }

        public static object GetField(Type type, string fieldName)
        {
            return type.InvokeMember(fieldName,
                BindingFlags.Default | BindingFlags.GetField,
                null,
                null,
                null);
        }

        public static bool HasProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName) != null;
        }

        public static object GetProperty(Type type, string propertyName)
        {
            return type.InvokeMember(propertyName,
                BindingFlags.Default | BindingFlags.GetProperty,
                null,
                null,
                null);
        }

        public bool VerifySignature()
        {
            if (string.IsNullOrEmpty(this.SourceSignature))
            {
                return false;
            }

            bool sourceVerified = false;

            using (MemoryStream codeStream = new MemoryStream())
            using (StreamWriter memoryWriter = new StreamWriter(codeStream, Encoding.UTF8))
            {
                memoryWriter.Write(this.SourceCode);
                byte[] signature = Convert.FromBase64String(this.SourceSignature);

                for (int i = 0; i < CryptoUtils.ModuleRSADecrypters.Length; i++)
                {
                    codeStream.Seek(0, SeekOrigin.Begin);
                    if (CryptoUtils.ModuleRSADecrypters[i].VerifyData(codeStream, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
                    {
                        sourceVerified = true;
                        break;
                    }
                }
            }

            if (!sourceVerified)
            {
                return false;
            }

            if (AdditionalReferences != null && AdditionalReferences.Length > 0)
            {
                if (ReferenceSignatures == null || ReferenceSignatures.Length != AdditionalReferences.Length)
                {
                    return false;
                }

                for (int i = 0; i < AdditionalReferences.Length; i++)
                {
                    bool referenceVerified = false;

                    string actualPath = LocateReference(AdditionalReferences[i]);

                    if (!File.Exists(actualPath))
                    {
                        return false;
                    }

                    if (Path.GetDirectoryName(actualPath) != Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                    {
                        byte[] signature = Convert.FromBase64String(this.ReferenceSignatures[i]);

                        using (FileStream fs = new FileStream(actualPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            for (int j = 0; j < CryptoUtils.ModuleRSADecrypters.Length; j++)
                            {
                                fs.Seek(0, SeekOrigin.Begin);
                                if (CryptoUtils.ModuleRSADecrypters[j].VerifyData(fs, signature, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1))
                                {
                                    referenceVerified = true;
                                    break;
                                }
                            }
                        }

                        if (!referenceVerified)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public void Sign(string privateKeyFileName)
        {
            using RSACryptoServiceProvider crypto = new RSACryptoServiceProvider(4096);
            crypto.ImportParameters(CryptoUtils.LoadPrivateKey(privateKeyFileName));

            using (MemoryStream codeStream = new MemoryStream())
            using (StreamWriter memoryWriter = new StreamWriter(codeStream, Encoding.UTF8))
            {
                memoryWriter.Write(this.SourceCode);
                codeStream.Seek(0, SeekOrigin.Begin);

                this.SourceSignature = Convert.ToBase64String(crypto.SignData(codeStream, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1)); ;
            }


            if (AdditionalReferences != null && AdditionalReferences.Length > 0)
            {
                ReferenceSignatures = new string[AdditionalReferences.Length];

                for (int i = 0; i < AdditionalReferences.Length; i++)
                {
                    string actualPath = LocateReference(AdditionalReferences[i]);

                    if (!File.Exists(actualPath))
                    {
                        throw new FileNotFoundException("The requested reference " + AdditionalReferences[i] + " could not be located!");
                    }

                    using FileStream fs = new FileStream(actualPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    ReferenceSignatures[i] = Convert.ToBase64String(crypto.SignData(fs, HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1));
                }
            }
        }

        public static string LocateReference(string referenceFile)
        {
            string actualPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), Path.GetFileName(referenceFile));

            if (!File.Exists(actualPath))
            {
                actualPath = Path.Combine(Modules.ModulePath, "libraries", Path.GetFileName(referenceFile));
            }

            if (!File.Exists(actualPath))
            {
                actualPath = Path.Combine(Modules.ModulePath, Path.GetFileName(referenceFile));
            }

            if (!File.Exists(actualPath))
            {
                return referenceFile;
            }

            return actualPath;
        }

        public void Export(string outputFile, bool includeReferences, bool includeAssets, bool fetchAssets)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                if (includeReferences)
                {
                    for (int i = 0; i < this.AdditionalReferences.Length; i++)
                    {
                        string actualPath = ModuleMetadata.LocateReference(this.AdditionalReferences[i]);

                        if (Path.GetDirectoryName(actualPath) != Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                        {
                            File.Copy(actualPath, Path.Combine(tempDir, Path.GetFileName(actualPath)));
                        }
                    }
                }

                if (includeAssets)
                {
                    Directory.CreateDirectory(Path.Combine(tempDir, "assets"));

                    string markdownSource = this.BuildReadmeMarkdown();

                    VectSharp.Markdown.MarkdownRenderer renderer = new VectSharp.Markdown.MarkdownRenderer();

                    Func<string, string, (string, bool)> imageUriResolver = renderer.ImageUriResolver;

                    int imageIndex = 0;

                    if (fetchAssets)
                    {
                        renderer.ImageUriResolver = (imageUri, baseUri) =>
                        {
                            if (!imageUri.StartsWith("data:"))
                            {
                                (string imageFile, bool wasDownloaded) = imageUriResolver(imageUri, baseUri);

                                string assetLocation = Path.Combine(tempDir, "assets", "image" + imageIndex.ToString() + Path.GetExtension(imageFile));

                                if (imageFile != null && File.Exists(imageFile))
                                {
                                    File.Copy(imageFile, assetLocation);
                                    imageIndex++;
                                }

                                if (wasDownloaded)
                                {
                                    System.IO.File.Delete(imageFile);
                                    System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(imageFile));
                                }

                                return (assetLocation, false);
                            }
                            else
                            {
                                VectSharp.Page pag = VectSharp.SVG.Parser.ParseImageURI(imageUri, true);

                                string assetLocation = Path.Combine(tempDir, "assets", "image" + imageIndex.ToString() + ".svg");
                                imageIndex++;

                                VectSharp.SVG.SVGContextInterpreter.SaveAsSVG(pag, assetLocation);

                                return (assetLocation, false);
                            }
                        };
                    }
                    else
                    {
                        renderer.ImageUriResolver = (imageUri, baseUri) =>
                        {
                            string[] compatibleFiles = System.IO.Directory.GetFiles(System.IO.Path.Combine(Modules.ModulePath, "assets"), this.Id + "_" + "image" + imageIndex + ".*");

                            if (compatibleFiles.Length > 0)
                            {
                                string imagePath = compatibleFiles[0];
                                string assetLocation = Path.Combine(tempDir, "assets", "image" + imageIndex.ToString() + Path.GetExtension(imagePath));

                                File.Copy(imagePath, assetLocation);
                                imageIndex++;

                                return (assetLocation, false);
                            }
                            else
                            {
                                return (null, false);
                            }
                        };
                    }

                    Markdig.Syntax.MarkdownDocument markdownDocument = Markdig.Markdown.Parse(markdownSource, new Markdig.MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().Build());

                    renderer.Render(markdownDocument, out Dictionary<string, string> linkDestinations);
                }

                for (int i = 0; i < this.AdditionalReferences.Length; i++)
                {
                    this.AdditionalReferences[i] = Path.GetFileName(this.AdditionalReferences[i]);
                }

                using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Create))
                {
                    System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), this, typeof(ModuleMetadata), Modules.DefaultSerializationOptions);
                }

                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }

                ZipFile.CreateFromDirectory(tempDir, outputFile, CompressionLevel.Optimal, false);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        public static void Install(string modulePath, bool ignoreInvalidSignature, bool installReferences)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            try
            {
                ZipFile.ExtractToDirectory(modulePath, tempDir);

                ModuleMetadata metaData;

                string jsonText = File.ReadAllText(Path.Combine(tempDir, "Module.json"));

                metaData = System.Text.Json.JsonSerializer.Deserialize<ModuleMetadata>(jsonText, Modules.DefaultSerializationOptions);

                if (Modules.LoadedModulesMetadata.ContainsKey(metaData.Id))
                {
                    throw new InvalidOperationException("A module with the same Id has already been loaded!");
                }

                if (!ignoreInvalidSignature && !metaData.VerifySignature())
                {
                    throw new InvalidOperationException("The source code of the module could not be verified.");
                }

                metaData.IsInstalled = true;

                Modules.LoadModule(metaData);

                string newModuleDLL = Path.Combine(tempDir, Guid.NewGuid().ToString());

                using (FileStream fs = new FileStream(newModuleDLL, FileMode.Create))
                {
                    metaData.Build(fs);
                }

                string newModuleList = Path.Combine(tempDir, Guid.NewGuid().ToString());

                using (FileStream fs = new FileStream(newModuleList, FileMode.Create))
                {
                    System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), from el in Modules.LoadedModulesMetadata where el.Value.IsInstalled == true select el.Value, typeof(IEnumerable<ModuleMetadata>), Modules.DefaultSerializationOptions);
                }

                List<string> filesToCopy = new List<string>();
                filesToCopy.Add(Path.GetFullPath(newModuleList));
                filesToCopy.Add(Path.GetFullPath(Modules.ModuleListPath));
                filesToCopy.Add(Path.GetFullPath(newModuleDLL));
                filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, metaData.Id + ".dll")));

                if (installReferences)
                {
                    for (int i = 0; i < metaData.AdditionalReferences.Length; i++)
                    {
                        string actualPath = Path.Combine(tempDir, Path.GetFileName(metaData.AdditionalReferences[i]));

                        if (File.Exists(actualPath))
                        {
                            filesToCopy.Add(Path.GetFullPath(actualPath));
                            filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, "libraries", Path.GetFileName(actualPath))));
                        }
                    }
                }

                if (Directory.Exists(Path.Combine(tempDir, "assets")))
                {
                    foreach (string file in Directory.GetFiles(Path.Combine(tempDir, "assets"), "*.*"))
                    {
                        filesToCopy.Add(Path.GetFullPath(file));
                        filesToCopy.Add(Path.GetFullPath(Path.Combine(Modules.ModulePath, "assets", metaData.Id + "_" + Path.GetFileName(file))));
                    }
                }

                for (int i = 0; i < filesToCopy.Count; i += 2)
                {
                    File.Copy(filesToCopy[i], filesToCopy[i + 1], true);
                }

            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch { }
            }
        }

        public static void Uninstall(string moduleId)
        {
            ModuleMetadata selectedMetadata = Modules.LoadedModulesMetadata[moduleId];

            Modules.LoadedModulesMetadata.Remove(selectedMetadata.Id);

            string newModuleList = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            string fileList = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());

            try
            {
                using (FileStream fs = new FileStream(newModuleList, FileMode.Create))
                {
                    System.Text.Json.JsonSerializer.Serialize(new System.Text.Json.Utf8JsonWriter(fs), from el in Modules.LoadedModulesMetadata where el.Value.IsInstalled == true select el.Value, typeof(IEnumerable<ModuleMetadata>), Modules.DefaultSerializationOptions);
                }

                List<string> filesToCopy = new List<string>();
                filesToCopy.Add(newModuleList);
                filesToCopy.Add(Path.GetFullPath(Modules.ModuleListPath));

                for (int i = 0; i < filesToCopy.Count; i += 2)
                {
                    if (filesToCopy[i + 1] != "-")
                    {
                        File.Copy(filesToCopy[i], filesToCopy[i + 1], true);
                    }
                    else
                    {
                        if (File.Exists(filesToCopy[i]))
                        {
                            File.Delete(filesToCopy[i]);
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    File.Delete(newModuleList);
                }
                catch { }

                try
                {
                    if (File.Exists(fileList))
                    {
                        File.Delete(fileList);
                    }
                }
                catch { }
            }
        }

        public string BuildReadmeMarkdown()
        {
            Assembly assembly;
            using (MemoryStream ms = new MemoryStream())
            {
                this.Build(ms);
                ms.Seek(0, System.IO.SeekOrigin.Begin);
                assembly = System.Reflection.Assembly.Load(ms.ToArray());
            }

            Type type = ModuleMetadata.GetTypeFromAssembly(assembly, "MyModule");
            Module loadedModule = this.Load(type, false);

            TreeNode testTree = PhyloTree.Formats.NWKA.ParseTree("(A:2,(B:1,C:1):1);");
            testTree.Attributes.Add("aa323df2-20c0-4fec-9c58-1094ea4b0122", "Test");

            TreeCollection testCollection = new TreeCollection(new List<TreeNode>() { testTree, testTree, testTree });

            List<(string, string, bool)> parameters = new List<(string, string, bool)>();

            if (type.GetMethod("GetGlobalSettings") != null)
            {
                List<(string, string)> globalSettings = ((Func<List<(string, string)>>)Delegate.CreateDelegate(typeof(Func<List<(string, string)>>), type, "GetGlobalSettings"))();
                parameters.AddRange(from el in globalSettings select (el.Item1, el.Item2, true));
            }

            string customIconSource = null;
            double customIconHeight = double.NaN;

            switch (this.ModuleType)
            {
                case ModuleTypes.Action:
                    {
                        Page icon;

                        Page pag;

                        if (((ActionModule)loadedModule).IsLargeButton)
                        {
                            icon = ((ActionModule)loadedModule).GetIcon(1);
                        }
                        else
                        {
                            icon = ((ActionModule)loadedModule).GetIcon(2);
                        }

                        pag = new Page(32, 32);
                        customIconHeight = 52.706;

                        pag.Graphics.Scale(pag.Width / icon.Width, pag.Height / icon.Height);
                        pag.Graphics.DrawGraphics(0, 0, icon.Graphics);

                        byte[] bytes;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            pag.SaveAsSVG(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                            }
                        }

                        customIconSource = "data:image/svg+xml;base64," + System.Convert.ToBase64String(bytes);

                    }
                    break;
                case ModuleTypes.Coordinate:
                    parameters.AddRange(from el in ((CoordinateModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.FileType:
                    break;
                case ModuleTypes.FurtherTransformation:
                    {
                        Page icon = ((FurtherTransformationModule)loadedModule).GetIcon(2);

                        Page pag = new Page(32, 32);
                        customIconHeight = 52.706;

                        pag.Graphics.Scale(pag.Width / icon.Width, pag.Height / icon.Height);
                        pag.Graphics.DrawGraphics(0, 0, icon.Graphics);

                        byte[] bytes;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            pag.SaveAsSVG(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                            }
                        }

                        customIconSource = "data:image/svg+xml;base64," + System.Convert.ToBase64String(bytes);
                    }
                    parameters.AddRange(from el in ((FurtherTransformationModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.LoadFile:
                    break;
                case ModuleTypes.MenuAction:
                    {
                        Page icon;

                        Page pag = new Page(32, 32);


                        if (((MenuActionModule)loadedModule).ParentMenu == "File")
                        {
                            pag.Background = Colour.FromRgb(0, 114, 178);
                            icon = ((MenuActionModule)loadedModule).GetIcon(1.5);
                        }
                        else if (((MenuActionModule)loadedModule).IsLargeButton)
                        {
                            icon = ((MenuActionModule)loadedModule).GetIcon(1);
                        }
                        else
                        {
                            icon = ((MenuActionModule)loadedModule).GetIcon(2);
                        }

                        customIconHeight = 52.706;



                        if (((MenuActionModule)loadedModule).ParentMenu == "File")
                        {
                            pag.Graphics.Translate(4, 4);
                            pag.Graphics.Scale(24 / icon.Width, 24 / icon.Height);
                            pag.Graphics.DrawGraphics(0, 0, icon.Graphics);
                        }
                        else
                        {
                            pag.Graphics.Scale(pag.Width / icon.Width, pag.Height / icon.Height);
                            pag.Graphics.DrawGraphics(0, 0, icon.Graphics);
                        }

                        byte[] bytes;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            pag.SaveAsSVG(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                            }
                        }

                        customIconSource = "data:image/svg+xml;base64," + System.Convert.ToBase64String(bytes);

                    }
                    break;
                case ModuleTypes.Plotting:
                    {
                        Page icon = ((PlottingModule)loadedModule).GetIcon(2);

                        Page pag = new Page(32, 32);
                        customIconHeight = 52.706;

                        pag.Graphics.Scale(pag.Width / icon.Width, pag.Height / icon.Height);
                        pag.Graphics.DrawGraphics(0, 0, icon.Graphics);

                        byte[] bytes;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            pag.SaveAsSVG(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                            }
                        }

                        customIconSource = "data:image/svg+xml;base64," + System.Convert.ToBase64String(bytes);
                    }
                    parameters.AddRange(from el in ((PlottingModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.SelectionAction:
                    {
                        Page icon;

                        Page pag;

                        if (((SelectionActionModule)loadedModule).IsLargeButton)
                        {
                            icon = ((SelectionActionModule)loadedModule).GetIcon(1);
                        }
                        else
                        {
                            icon = ((SelectionActionModule)loadedModule).GetIcon(2);
                        }

                        pag = new Page(32, 32);
                        customIconHeight = 52.706;

                        pag.Graphics.Scale(pag.Width / icon.Width, pag.Height / icon.Height);
                        pag.Graphics.DrawGraphics(0, 0, icon.Graphics);

                        byte[] bytes;

                        using (MemoryStream stream = new MemoryStream())
                        {
                            pag.SaveAsSVG(stream);
                            stream.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(stream))
                            {
                                bytes = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                            }
                        }

                        customIconSource = "data:image/svg+xml;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.Transformer:
                    parameters.AddRange(from el in ((TransformerModule)loadedModule).GetParameters(testCollection) select (el.Item1, el.Item2, false));
                    break;
            }

            return BuildReadmeMarkdown(parameters, customIconSource, customIconHeight);
        }

        public string BuildReadmeMarkdown(List<(string, string, bool)> parameters, string customIconSource, double customIconHeight)
        {
            StringBuilder xml = new StringBuilder();

            xml.AppendLine("<xml>");

            using (StringReader reader = new StringReader(this.SourceCode))
            {
                string line = reader.ReadLine();

                while (line != null)
                {
                    if (line.TrimStart().StartsWith("///"))
                    {
                        xml.AppendLine(line.TrimStart().Substring(3));
                    }

                    line = reader.ReadLine();
                }
            }
            xml.AppendLine("</xml>");

            XmlDocument documentation = new XmlDocument();
            documentation.LoadXml(xml.ToString());

            StringBuilder builder = new StringBuilder();

            string iconSource = "";

            double iconHeight = 52.706;

            switch (this.ModuleType)
            {
                case ModuleTypes.FileType:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FileType-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.LoadFile:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.LoadFile-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.Transformer:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.Transformer-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.FurtherTransformation:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FurtherTransformations-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.Coordinate:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.Coordinates-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.Plotting:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.PlotActions-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.SelectionAction:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.SelectionAction-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.Action:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.Action-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
                case ModuleTypes.MenuAction:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.MenuAction-32.png").CopyTo(stream);
                            bytes = stream.ToArray();
                        }

                        iconSource = "data:image/png;base64," + System.Convert.ToBase64String(bytes);
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(customIconSource))
            {
                iconSource = customIconSource;
            }

            if (!double.IsNaN(customIconHeight))
            {
                iconHeight = customIconHeight;
            }

            if (!string.IsNullOrEmpty(iconSource))
            {
                builder.Append("# ");
                builder.Append("<img src=\"" + iconSource + "\" height=\"" + iconHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\" /> ");

                builder.AppendLine(this.Name);
                builder.AppendLine();
            }
            else
            {
                builder.AppendLine("# " + this.Name);
                builder.AppendLine();
            }

            builder.AppendLine("### _Version " + this.Version.ToString() + ", by " + this.Author + "_");
            builder.AppendLine();

            builder.AppendLine("**Description**: " + this.HelpText);
            builder.AppendLine();

            builder.AppendLine("**Module type**: " + this.ModuleType.ToString());
            builder.AppendLine();

            builder.AppendLine("**Module ID**: `" + this.Id + "`");
            builder.AppendLine();

            XmlNode summary = documentation.DocumentElement.SelectSingleNode("/xml/summary");

            if (summary != null)
            {
                builder.AppendLine(summary.InnerText);
                builder.AppendLine();
            }

            if (parameters.Count > 0)
            {
                Dictionary<string, string> displayNames = new Dictionary<string, string>();

                Dictionary<string, string> parameterDescriptions = new Dictionary<string, string>();

                Dictionary<string, string> parameterDefaults = new Dictionary<string, string>();

                Dictionary<string, string[]> parameterPossibleValues = new Dictionary<string, string[]>();

                Dictionary<string, string> parameterPossibleValuesHeader = new Dictionary<string, string>();

                XmlNodeList nodes = documentation.DocumentElement.SelectNodes("/xml/param");

                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        if (node is XmlElement element)
                        {
                            if (element.HasAttribute("name"))
                            {
                                string name = element.GetAttribute("name");

                                if (element.HasAttribute("display"))
                                {
                                    displayNames[name] = element.GetAttribute("display");
                                }

                                if (element.HasAttribute("default"))
                                {
                                    parameterDefaults[name] = element.GetAttribute("default");
                                }

                                if (element.HasAttribute("value-header"))
                                {
                                    parameterPossibleValuesHeader[name] = element.GetAttribute("value-header");
                                }

                                if (element.HasAttribute("values"))
                                {
                                    parameterPossibleValues[name] = JsonSerializer.Deserialize<string[]>(element.GetAttribute("values"), Modules.DefaultSerializationOptions);
                                }

                                if (!string.IsNullOrEmpty(node.InnerText))
                                {
                                    parameterDescriptions[name] = node.InnerText;
                                }
                            }
                        }
                    }
                }

                bool isFirst = true;

                foreach ((string, string, bool) parameter in parameters)
                {
                    string displayName = parameter.Item1.TrimEnd(' ', '\t', ':').Trim();
                    if (displayName.EndsWith("..."))
                    {
                        displayName = displayName.TrimEnd('.').Trim();
                    }

                    if (displayNames.TryGetValue(parameter.Item1, out string newDisplayName))
                    {
                        displayName = newDisplayName;
                    }

                    string parameterType = null;
                    string defaultValue = null;
                    string[] possibleValues = null;
                    string possibleValuesHeader = null;

                    bool displayParameter = false;


                    string controlType = parameter.Item2.Substring(0, parameter.Item2.IndexOf(":"));
                    string controlParameters = parameter.Item2.Substring(parameter.Item2.IndexOf(":") + 1);

                    if (controlType == "Id")
                    {
                        displayParameter = false;
                    }
                    else if (controlType == "TreeCollection")
                    {
                        displayParameter = false;
                    }
                    else if (controlType == "Window")
                    {
                        displayParameter = false;
                    }
                    else if (controlType == "InstanceStateData")
                    {
                        displayParameter = false;
                    }
                    else if (controlType == "Group")
                    {
                        displayParameter = false;
                    }
                    else if (controlType == "Button")
                    {
                        parameterType = "Button";
                        defaultValue = null;
                        possibleValues = null;
                        possibleValuesHeader = null;
                        displayParameter = true;
                    }
                    else if (controlType == "Buttons")
                    {
                        parameterType = "Buttons";
                        possibleValues = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);
                        defaultValue = null;
                        possibleValuesHeader = "**Buttons**:";
                        displayParameter = true;
                    }
                    else if (controlType == "CheckBox")
                    {
                        parameterType = "Check box";
                        possibleValues = null;
                        defaultValue = Convert.ToBoolean(controlParameters) ? "Checked" : "Unchecked";
                        possibleValuesHeader = null;
                        displayParameter = true;

                    }
                    else if (controlType == "Formatter")
                    {
                        parameterType = "Attribute formatter";
                        possibleValues = null;
                        defaultValue = null;
                        possibleValuesHeader = null;
                        displayParameter = true;
                    }
                    else if (controlType == "Label")
                    {
                        displayParameter = false;
                    }
                    else
                    {
                        if (controlType == "ComboBox")
                        {
                            int defaultIndex = int.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));
                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            parameterType = "Drop-down list";
                            possibleValues = items;
                            defaultValue = items[defaultIndex];
                            possibleValuesHeader = "**Possible values**:";
                            displayParameter = true;
                        }
                        else if (controlType == "TextBox")
                        {
                            parameterType = "Text box";
                            possibleValues = null;
                            defaultValue = controlParameters;
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "AttributeSelector")
                        {
                            parameterType = "Attribute selector";
                            possibleValues = null;
                            defaultValue = controlParameters;
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Attachment")
                        {
                            parameterType = "Attachment";
                            possibleValues = null;
                            defaultValue = null;
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Node")
                        {
                            parameterType = "Node";
                            possibleValues = null;
                            defaultValue = null;
                            possibleValuesHeader = null;
                            displayParameter = true;

                        }
                        else if (controlType == "NumericUpDown")
                        {
                            parameterType = "Number spin box";
                            possibleValues = null;
                            double defaultValueDouble = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));

                            displayParameter = true;

                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

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

                            if (!formatString.StartsWith("{"))
                            {
                                formatString = "{0:" + formatString + "}";
                            }

                            defaultValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, defaultValueDouble, System.Globalization.CultureInfo.InvariantCulture);

                            string leftParen = "\\[";
                            string rightParen = "\\]";
                            string min = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, minRange);
                            string max = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, maxRange);

                            if (double.IsPositiveInfinity(minRange))
                            {
                                min = "+$\\infty$";
                                leftParen = "(";
                            }
                            else if (double.IsNegativeInfinity(minRange))
                            {
                                min = "-$\\infty$";
                                leftParen = "(";
                            }

                            if (double.IsPositiveInfinity(maxRange))
                            {
                                max = "+$\\infty$";
                                rightParen = ")";
                            }
                            else if (double.IsNegativeInfinity(maxRange))
                            {
                                max = "-$\\infty$";
                                rightParen = ")";
                            }

                            possibleValuesHeader = "**Range**: " + leftParen + " " + min + ", " + max + " " + rightParen;
                        }
                        else if (controlType == "NumericUpDownByNode")
                        {
                            parameterType = "Number spin box (by node)";
                            possibleValues = null;
                            double defaultValueDouble = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));

                            displayParameter = true;

                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = TreeViewer.Extensions.GetFormatString(increment);

                            if (!formatString.StartsWith("{"))
                            {
                                formatString = "{0:" + formatString + "}";
                            }

                            defaultValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, defaultValueDouble, System.Globalization.CultureInfo.InvariantCulture);

                            string leftParen = "\\[";
                            string rightParen = "\\]";
                            string min = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, minRange);
                            string max = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, maxRange);

                            if (double.IsPositiveInfinity(minRange))
                            {
                                min = "+$\\infty$";
                                leftParen = "(";
                            }
                            else if (double.IsNegativeInfinity(minRange))
                            {
                                min = "-$\\infty$";
                                leftParen = "(";
                            }

                            if (double.IsPositiveInfinity(maxRange))
                            {
                                max = "+$\\infty$";
                                rightParen = ")";
                            }
                            else if (double.IsNegativeInfinity(maxRange))
                            {
                                max = "-$\\infty$";
                                rightParen = ")";
                            }

                            possibleValuesHeader = "**Range**: " + leftParen + " " + min + ", " + max + " " + rightParen + Environment.NewLine + Environment.NewLine + "**Default attribute**: `" + range[3] + "`";
                        }
                        else if (controlType == "Slider")
                        {
                            parameterType = "Slider";
                            possibleValues = null;
                            double defaultValueDouble = double.Parse(controlParameters.Substring(0, controlParameters.IndexOf("[")));

                            displayParameter = true;

                            controlParameters = controlParameters.Substring(controlParameters.IndexOf("["));

                            string[] range = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            double minRange = double.Parse(range[0], System.Globalization.CultureInfo.InvariantCulture);
                            double maxRange = double.Parse(range[1], System.Globalization.CultureInfo.InvariantCulture);

                            double increment = (maxRange - minRange) * 0.01;

                            if (double.IsNaN(increment) || double.IsInfinity(increment))
                            {
                                increment = 1;
                            }

                            string formatString = TreeViewer.Extensions.GetFormatString(increment);

                            if (range.Length > 2)
                            {
                                formatString = range[2];
                            }

                            if (!formatString.StartsWith("{"))
                            {
                                formatString = "{0:" + formatString + "}";
                            }

                            defaultValue = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, defaultValueDouble, System.Globalization.CultureInfo.InvariantCulture);

                            string leftParen = "\\[";
                            string rightParen = "\\]";
                            string min = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, minRange);
                            string max = string.Format(System.Globalization.CultureInfo.InvariantCulture, formatString, maxRange);

                            if (double.IsPositiveInfinity(minRange))
                            {
                                min = "+$\\infty$";
                                leftParen = "(";
                            }
                            else if (double.IsNegativeInfinity(minRange))
                            {
                                min = "-$\\infty$";
                                leftParen = "(";
                            }

                            if (double.IsPositiveInfinity(maxRange))
                            {
                                max = "+$\\infty$";
                                rightParen = ")";
                            }
                            else if (double.IsNegativeInfinity(maxRange))
                            {
                                max = "-$\\infty$";
                                rightParen = ")";
                            }

                            possibleValuesHeader = "**Range**: " + leftParen + " " + min + ", " + max + " " + rightParen;
                        }
                        else if (controlType == "Font")
                        {
                            string[] font = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            parameterType = "Font";
                            possibleValues = null;
                            defaultValue = font[0] + " " + font[1] + "pt";
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Point")
                        {
                            double[] point = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            string formatStringX = TreeViewer.Extensions.GetFormatString(point[0]);
                            string formatStringY = TreeViewer.Extensions.GetFormatString(point[1]);

                            parameterType = "Point";
                            possibleValues = null;
                            defaultValue = "( " + point[0].ToString(formatStringX, System.Globalization.CultureInfo.InvariantCulture) + ", " + point[1].ToString(formatStringY, System.Globalization.CultureInfo.InvariantCulture) + " )";
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Colour")
                        {
                            int[] colour = System.Text.Json.JsonSerializer.Deserialize<int[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba((byte)colour[0], (byte)colour[1], (byte)colour[2], (byte)colour[3]);

                            VectSharp.Page colourPreview = new Page(24, 16);

                            if (col.A < 1)
                            {
                                colourPreview.Graphics.FillRectangle(0, 0, 8, 8, Colour.FromRgb(220, 220, 220));
                                colourPreview.Graphics.FillRectangle(16, 0, 8, 8, Colour.FromRgb(220, 220, 220));
                                colourPreview.Graphics.FillRectangle(8, 8, 8, 8, Colour.FromRgb(220, 220, 220));
                            }

                            colourPreview.Graphics.FillRectangle(0, 0, 24, 16, col);
                            colourPreview.Graphics.StrokeRectangle(0, 0, 24, 16, Colours.Black);

                            string svg;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                colourPreview.SaveAsSVG(ms);

                                using (StreamReader reader = new StreamReader(ms))
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    string rawSvg = reader.ReadToEnd();
                                    svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawSvg));
                                }
                            }

                            defaultValue = "<img src=\"data:image/svg+xml;base64," + svg + "\" width=\"24\" height=\"16\"/>";

                            parameterType = "Colour";
                            possibleValues = null;
                            defaultValue += " #" + colour[0].ToString("X2") + colour[1].ToString("X2") + colour[1].ToString("X2") + " (opacity: " + ((double)colour[3] / 255.0).ToString("0%", System.Globalization.CultureInfo.InvariantCulture) + ")";
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "SourceCode")
                        {
                            parameterType = "Source code";
                            possibleValues = null;
                            defaultValue = Environment.NewLine + Environment.NewLine + "```CSharp" + Environment.NewLine + controlParameters + (!controlParameters.EndsWith("\n") ? Environment.NewLine : "") + "```";
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Markdown")
                        {
                            parameterType = "Markdown";
                            possibleValues = null;
                            defaultValue = Environment.NewLine + Environment.NewLine + "```Markdown" + Environment.NewLine + controlParameters + (!controlParameters.EndsWith("\n") ? Environment.NewLine : "") + "```";
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "Dash")
                        {
                            double[] dash = System.Text.Json.JsonSerializer.Deserialize<double[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.LineDash lineDash = new VectSharp.LineDash(dash[0], dash[1], dash[2]);

                            VectSharp.Page dashPreview = new Page(128, 16);

                            dashPreview.Graphics.StrokePath(new GraphicsPath().MoveTo(0, 8).LineTo(128, 8), Colours.Black, 2, lineDash: lineDash);

                            string svg;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                dashPreview.SaveAsSVG(ms);

                                using (StreamReader reader = new StreamReader(ms))
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    string rawSvg = reader.ReadToEnd();
                                    svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawSvg));
                                }
                            }

                            defaultValue = "<img src=\"data:image/svg+xml;base64," + svg + "\" width=\"128\" height=\"16\"/>";

                            parameterType = "Line dash";
                            possibleValues = null;
                            defaultValue += Environment.NewLine + Environment.NewLine + "* *Units on*: " + dash[0].ToString(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine + "* *Units off*: " + dash[1].ToString(System.Globalization.CultureInfo.InvariantCulture) + Environment.NewLine + "* *Phase*: " + dash[2].ToString(System.Globalization.CultureInfo.InvariantCulture);
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                        else if (controlType == "ColourByNode")
                        {
                            string[] colour = System.Text.Json.JsonSerializer.Deserialize<string[]>(controlParameters, Modules.DefaultSerializationOptions);

                            VectSharp.Colour col = VectSharp.Colour.FromRgba(int.Parse(colour[3]), int.Parse(colour[4]), int.Parse(colour[5]), int.Parse(colour[6]));

                            VectSharp.Page colourPreview = new Page(24, 16);

                            if (col.A < 1)
                            {
                                colourPreview.Graphics.FillRectangle(0, 0, 8, 8, Colour.FromRgb(220, 220, 220));
                                colourPreview.Graphics.FillRectangle(16, 0, 8, 8, Colour.FromRgb(220, 220, 220));
                                colourPreview.Graphics.FillRectangle(8, 8, 8, 8, Colour.FromRgb(220, 220, 220));
                            }

                            colourPreview.Graphics.FillRectangle(0, 0, 24, 16, col);
                            colourPreview.Graphics.StrokeRectangle(0, 0, 24, 16, Colours.Black);

                            string svg;

                            using (MemoryStream ms = new MemoryStream())
                            {
                                colourPreview.SaveAsSVG(ms);

                                using (StreamReader reader = new StreamReader(ms))
                                {
                                    ms.Seek(0, SeekOrigin.Begin);
                                    string rawSvg = reader.ReadToEnd();
                                    svg = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(rawSvg));
                                }
                            }

                            defaultValue = "<img src=\"data:image/svg+xml;base64," + svg + "\" width=\"24\" height=\"16\"/>";

                            parameterType = "Colour (by node)";
                            possibleValues = null;
                            defaultValue += " #" + int.Parse(colour[3]).ToString("X2") + int.Parse(colour[4]).ToString("X2") + int.Parse(colour[5]).ToString("X2") + " (opacity: " + ((double)int.Parse(colour[6]) / 255.0).ToString("0%", System.Globalization.CultureInfo.InvariantCulture) + ")";
                            possibleValuesHeader = "**Default attribute:** `" + colour[1] + "`";
                            displayParameter = true;
                        }
                        else if (controlType == "AttributeType")
                        {
                            parameterType = "Attribute type";
                            possibleValues = Modules.AttributeTypes;
                            defaultValue = controlParameters;
                            possibleValuesHeader = "**Possible values:**";
                            displayParameter = true;
                        }
                        else if (controlType == "FileSize")
                        {
                            long defaultValueLong = long.Parse(controlParameters);

                            double value;

                            string unit;

                            if (defaultValueLong < 1024)
                            {
                                unit = "B";
                                value = defaultValueLong;
                            }
                            else
                            {
                                double longSize = defaultValueLong / 1024.0;

                                if (longSize < 1024)
                                {
                                    unit = "kiB";
                                }
                                else
                                {
                                    longSize /= 1024.0;

                                    if (longSize < 1024)
                                    {
                                        unit = "MiB";
                                    }
                                    else
                                    {
                                        longSize /= 1024.0;
                                        unit = "GiB";
                                    }
                                }

                                value = longSize;
                            }

                            parameterType = "File size";
                            possibleValues = null;
                            defaultValue = value.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + unit;
                            possibleValuesHeader = null;
                            displayParameter = true;
                        }
                    }

                    if (parameterPossibleValues.TryGetValue(parameter.Item1, out string[] newPossibleValues))
                    {
                        possibleValues = newPossibleValues;
                    }

                    if (parameterPossibleValuesHeader.TryGetValue(parameter.Item1, out string newPossibleValuesHeader))
                    {
                        possibleValuesHeader = newPossibleValuesHeader;
                    }

                    if (parameterDefaults.TryGetValue(parameter.Item1, out string newDefault))
                    {
                        defaultValue = newDefault;
                    }

                    if (!string.IsNullOrEmpty(displayName) && displayParameter)
                    {
                        if (!isFirst)
                        {
                            builder.AppendLine("<br/>");
                            builder.AppendLine();
                        }
                        else
                        {
                            builder.AppendLine("## Parameters");
                            builder.AppendLine();

                            isFirst = false;
                        }

                        builder.AppendLine("### " + displayName);
                        builder.AppendLine();

                        if (parameter.Item3)
                        {
                            builder.AppendLine("*Global setting*");
                            builder.AppendLine();
                        }

                        builder.AppendLine("**Control type**: " + parameterType);
                        builder.AppendLine();

                        if (!string.IsNullOrEmpty(defaultValue))
                        {
                            builder.AppendLine("**Default value**: " + defaultValue);
                            builder.AppendLine();
                        }

                        if (!string.IsNullOrEmpty(possibleValuesHeader))
                        {
                            builder.AppendLine(possibleValuesHeader);
                            builder.AppendLine();
                        }

                        if (possibleValues != null)
                        {
                            builder.AppendLine();

                            for (int i = 0; i < possibleValues.Length; i++)
                            {
                                builder.AppendLine("* " + possibleValues[i]);
                            }

                            builder.AppendLine();
                        }

                        if (parameterDescriptions.TryGetValue(parameter.Item1, out string description))
                        {
                            builder.AppendLine(description);
                            builder.AppendLine();
                        }
                    }
                }
            }

            XmlNode moduleDescription = documentation.DocumentElement.SelectSingleNode("/xml/description");

            if (moduleDescription != null)
            {
                builder.AppendLine(moduleDescription.InnerText);
                builder.AppendLine();
            }

            return builder.ToString();
        }
    }

    public class InstanceStateData
    {
        public static bool IsUIAvailable = true;
        public static bool IsInteractive = true;

        public Dictionary<string, Attachment> Attachments { get; } = new Dictionary<string, Attachment>();

        public TreeCollection Trees { get; set; }
        public Dictionary<string, object> Tags { get; } = new Dictionary<string, object>();
        public TreeNode TransformedTree;

        public Func<PlottingModule, Action<Dictionary<string, object>>> AddPlottingModule { get; set; }
        public Action<int> RemovePlottingModule { get; set; }

        public Func<FurtherTransformationModule, Action<Dictionary<string, object>>> AddFurtherTransformationModule { get; set; }
        public Action<int> RemoveFurtherTransformationModule { get; set; }

        public Func<CoordinateModule, Action<Dictionary<string, object>>> SetCoordinatesModule { get; set; }
        public Func<TransformerModule, Action<Dictionary<string, object>>> SetTransformerModule { get; set; }

        public Func<TransformerModule> TransformerModule { get; set; }
        public Func<List<FurtherTransformationModule>> FurtherTransformationModules { get; set; }
        public Func<CoordinateModule> CoordinateModule { get; set; }
        public Func<List<PlottingModule>> PlottingModules { get; set; }


        public Func<Action<Dictionary<string, object>>> TransformerModuleParameterUpdater { get; set; }
        public Func<Action<Dictionary<string, object>>> CoordinatesModuleParameterUpdater { get; set; }

        public Func<int, Action<Dictionary<string, object>>> FurtherTransformationModulesParameterUpdater { get; set; }
        public Func<int, Action<Dictionary<string, object>>> PlottingModulesParameterUpdater { get; set; }


        public Func<Dictionary<string, object>> GetTransformerModuleParameters { get; set; }
        public Func<Dictionary<string, object>> GetCoordinatesModuleParameters { get; set; }

        public Func<int, Dictionary<string, object>> GetFurtherTransformationModulesParamters { get; set; }
        public Func<int, Dictionary<string, object>> GetPlottingModulesParameters { get; set; }

        public Func<TreeNode> GetSelectedNode { get; set; }
        public Action<TreeNode> SetSelectedNode { get; set; }

        public Colour GraphBackgroundColour { get; set; } = Colour.FromRgb(255, 255, 255);

        public Action<string, bool> OpenFile { get; set; }

        public Func<MainWindow.ModuleTarget, bool, string> SerializeAllModules { get; set; }

        public InstanceStateData()
        {

        }
    }
}
