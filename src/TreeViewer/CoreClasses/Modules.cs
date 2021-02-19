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

        public SimpleCommand(Func<object, bool> canExecute, Action<object> execute, Avalonia.Controls.Window parent, Avalonia.AvaloniaProperty property)
        {
            this._canExecute = canExecute;
            this._execute = execute;
            this.cachedCanExecute = canExecute(parent);

            if (parent != null)
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
            return new CSharpEditor.InterprocessDebuggerServer(Path.Combine(myPath, "..", "..", "..", "..", "DebuggerClient", "bin", "Debug", "net5.0", exeName));
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

        public static VectSharp.FontFamily CodeFontFamily = new VectSharp.Canvas.ResourceFontFamily(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Fonts.RobotoMono-Regular.ttf"), "resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono");

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
        public static readonly string[] AttributeTypes = { "String", "Number" };

        public static readonly string[] DefaultAttributeConverters = { @"public static string Format(object attribute)
{
    return attribute as string;
}",
            @"public static string Format(object attribute)
{
    if (attribute is double attributeValue && !double.IsNaN(attributeValue))
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
            @"static Gradient gradient = new Gradient(new List<GradientStop>()
{
    new GradientStop(0, Colour.FromRgba(0d, 0d, 0d, 0d)),
    new GradientStop(1, Colour.FromRgba(0d, 0d, 0d, 1d))
});

public static Colour? Format(object attribute)
{
    if (attribute is double attributeValue && !double.IsNaN(attributeValue))
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
    if (attribute is double attributeValue && !double.IsNaN(attributeValue))
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
            { "Viridis", new Gradient(new List<GradientStop>() { new GradientStop(0, Colour.FromRgb(0.267004, 0.004874, 0.329415)), new GradientStop(0.09803921568627451, Colour.FromRgb(0.282623, 0.140926, 0.457517)), new GradientStop(0.2, Colour.FromRgb(0.253935, 0.265254, 0.529983)), new GradientStop(0.2980392156862745, Colour.FromRgb(0.206756, 0.371758, 0.553117)), new GradientStop(0.4, Colour.FromRgb(0.163625, 0.471133, 0.558148)), new GradientStop(0.4980392156862745, Colour.FromRgb(0.128729, 0.563265, 0.551229)), new GradientStop(0.6, Colour.FromRgb(0.134692, 0.658636, 0.517649)), new GradientStop(0.6980392156862745, Colour.FromRgb(0.259857, 0.745492, 0.444467)), new GradientStop(0.796078431372549, Colour.FromRgb(0.468053, 0.818921, 0.323998)), new GradientStop(0.8980392156862745, Colour.FromRgb(0.730889, 0.871916, 0.156029)), new GradientStop(1, Colour.FromRgb(0.993248, 0.906157, 0.143936)) }) }
        };

        //public static string ModulePath { get; set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "modules");
        //public static string ModuleListPath { get; set; } = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(Modules)).Location), "modules.json");

        public static string ModulePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "modules");
        public static string ModuleListPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "modules.json");


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
            return (from el in moduleCollection where el.Id == id select el).FirstOrDefault();
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

        public static async Task LoadInstalledModules(bool updateGlobalSettings)
        {
            //FileTypeModules.Add(new FileTypeModule() { Id = "79dfb9b2-ff10-4ed9-aa74-f7b3ae93c3d2", Name = "Newick", HelpText = "Opens a file containing one or more trees in the (extendend) Newick format.\nSafe even when opening huge files.", Extensions = NewickOpener.Extensions, IsSupported = NewickOpener.IsSupported, OpenFile = NewickOpener.Open });
            //FileTypeModules.Add(new FileTypeModule() { Id = "31fdfc2f-1921-432e-bb47-51362dd4fabb", Name = "NEXUS", HelpText = "Opens a file in the NEXUS format, reading the trees in the \"Trees\" blocks.\nSafe even when opening huge files.", Extensions = OpenNexus.MyModule.Extensions, IsSupported = OpenNexus.MyModule.IsSupported, OpenFile = OpenNexus.MyModule.OpenFile });
            //FileTypeModules.Add(new FileTypeModule() { Id = "8ccec519-3d83-4617-824f-dd474c88bdea", Name = "Binary tree", HelpText = "Opens a file in the NEXUS format, reading the trees in the \"Trees\" blocks.\nSafe even when opening huge files.", Extensions = OpenBinaryTree.MyModule.Extensions, IsSupported = OpenBinaryTree.MyModule.IsSupported, OpenFile = OpenBinaryTree.MyModule.OpenFile });
            //FileTypeModules.Add(new FileTypeModule() { Id = OpenSimmap.MyModule.Id, Name = OpenSimmap.MyModule.Name, HelpText = OpenSimmap.MyModule.HelpText, Extensions = OpenSimmap.MyModule.Extensions, IsSupported = OpenSimmap.MyModule.IsSupported, OpenFile = OpenSimmap.MyModule.OpenFile });


            //LoadFileModules.Add(new LoadFileModule() { Id = "a22ff194-c486-4215-a4bf-7a006d6f88fa", Name = "Memory loader", HelpText = "Loads all the trees from the file into memory.\nHuge files may cause the program to run out of memory.", IsSupported = MemoryLoader.IsSupported, Load = MemoryLoader.Load });

            //TransformerModules.Add(new TransformerModule() { Id = "32914d41-b182-461e-b7c6-5f0263cc1ccd", Name = "Consensus", GetParameters = StandardTransformer.GetTransformerParameters, OnParameterChange = StandardTransformer.TransformerParametersChanged, Transform = StandardTransformer.Transform });

            //FurtherTransformationModules.Add(new FurtherTransformationModule() { Id = "c6f96861-11c0-4853-9738-6a90cc81d660", Name = "Root tree", HelpText = "Re-roots the tree using the specified outgroup.", Repeatable = false, GetParameters = RerootTree.GetParameters, OnParameterChange = RerootTree.TransformerParametersChanged, Transform = RerootTree.Transform }); ;
            //FurtherTransformationModules.Add(new FurtherTransformationModule() { Id = "f06dce2a-794b-4897-a154-82f7f44c125d", Name = "Unroot tree", HelpText = "Unroots the tree.", Repeatable = false, GetParameters = UnrootTree.GetParameters, OnParameterChange = UnrootTree.TransformerParametersChanged, Transform = UnrootTree.Transform });
            //FurtherTransformationModules.Add(new FurtherTransformationModule() { Id = "8de06406-68e4-4bd8-97eb-2185a0dd1127", Name = "Change attribute", HelpText = "Changes the value of an already existing attribute.", Repeatable = true, GetParameters = ChangeAttribute.GetParameters, OnParameterChange = ChangeAttribute.TransformerParametersChanged, Transform = ChangeAttribute.Transform });
            //FurtherTransformationModules.Add(new FurtherTransformationModule() { Id = CustomScript.MyModule.Id, Name = CustomScript.MyModule.Name, HelpText = CustomScript.MyModule.HelpText, Repeatable = true, GetParameters = CustomScript.MyModule.GetParameters, OnParameterChange = CustomScript.MyModule.OnParameterChange, Transform = CustomScript.MyModule.Transform });

            //CoordinateModules.Add(new CoordinateModule() { Id = "68e25ec6-5911-4741-8547-317597e1b792", Name = "Rectangular", GetParameters = RectangularCoordinates.MyModule.GetParameters, GetCoordinates = RectangularCoordinates.MyModule.GetCoordinates, OnParameterChange = RectangularCoordinates.MyModule.OnParameterChange });
            //CoordinateModules.Add(new CoordinateModule() { Id = CircularCoordinates.MyModule.Id, Name = "Circular", GetParameters = CircularCoordinates.MyModule.GetParameters, GetCoordinates = CircularCoordinates.MyModule.GetCoordinates, OnParameterChange = CircularCoordinates.MyModule.OnParameterChange });
            //CoordinateModules.Add(new CoordinateModule() { Id = "95b61284-b870-48b9-b51c-3276f7d89df1", Name = "Radial", GetParameters = RadialCoordinates.GetParameters, GetCoordinates = RadialCoordinates.GetCoordinates, OnParameterChange = RadialCoordinates.ParametersChanged });

            //PlottingModules.Add(new PlottingModule() { Id = "7c767b07-71be-48b2-8753-b27f3e973570", Name = "Simple branches", HelpText = "Plot tree branches as simple lines.", GetParameters = Branches.MyModule.GetParameters, OnParameterChange = Branches.MyModule.OnParameterChange, PlotAction = Branches.MyModule.PlotAction });
            //PlottingModules.Add(new PlottingModule() { Id = NodeShapes.MyModule.Id, Name = NodeShapes.MyModule.Name, HelpText = NodeShapes.MyModule.HelpText, GetParameters = NodeShapes.MyModule.GetParameters, OnParameterChange = NodeShapes.MyModule.OnParameterChange, PlotAction = NodeShapes.MyModule.PlotAction });

            //SelectionActionModules.Add(new SelectionActionModule() { Id = "77f387fb-c843-4164-aed2-bd5b8f325809", Name = "Root tree on selection", HelpText = "Re-roots the tree using the selection as outgroup.", GetIcon = RerootTreeSelectionAction.GetIcon, IsAvailable = RerootTreeSelectionAction.IsAvailable, PerformAction = RerootTreeSelectionAction.PerformAction, ButtonText = RerootTreeSelectionAction.ButtonText });
            //SelectionActionModules.Add(new SelectionActionModule() { Id = "debd9130-8451-4413-88f0-6357ec817021", Name = "Copy selected node", HelpText = "Copies the selected node to the clipboard.", GetIcon = CopyTreeAction.GetIcon, IsAvailable = CopyTreeAction.IsAvailable, PerformAction = CopyTreeAction.PerformAction, ButtonText = CopyTreeAction.ButtonText, ShortcutKey = Avalonia.Input.Key.C, ShortcutModifier = Avalonia.Input.KeyModifiers.Control });

            //ActionModules.Add(new ActionModule() { Id = "e56b8297-4417-4494-9369-cbe9e5d25397", Name = "Display tree as rooted", HelpText = "Sets the plot actions to display the tree as a rooted tree", GetIcon = DisplayRootedTreeAction.GetIcon, PerformAction = DisplayRootedTreeAction.PerformAction, ButtonText = DisplayRootedTreeAction.ButtonText });
            //ActionModules.Add(new ActionModule() { Id = PasteTreeAction.MyModule.Id, Name = PasteTreeAction.MyModule.Name, HelpText = PasteTreeAction.MyModule.HelpText, GetIcon = PasteTreeAction.MyModule.GetIcon, PerformAction = PasteTreeAction.MyModule.PerformAction, ButtonText = PasteTreeAction.MyModule.ButtonText, ShortcutKey = Avalonia.Input.Key.V, ShortcutModifier = Avalonia.Input.KeyModifiers.Control });
            //ActionModules.Add(new ActionModule() { Id = "f90b3794-ffe5-44ff-9f62-859c61438713", Name = "Search", HelpText = "Highlights nodes in the tree.", GetIcon = Search.MyModule.GetIcon, PerformAction = Search.MyModule.PerformAction, ButtonText = Search.MyModule.ButtonText });
            //ActionModules.Add(new ActionModule() { Id = "a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6", Name = "Lasso selection", HelpText = "Select tips from the tree.", GetIcon = LassoSelection.GetIcon, PerformAction = LassoSelection.PerformAction, ButtonText = LassoSelection.ButtonText });
            //ActionModules.Add(new ActionModule() { Id = CircularStyleAction.MyModule.Id, Name = CircularStyleAction.MyModule.Name, HelpText = CircularStyleAction.MyModule.HelpText, GetIcon = CircularStyleAction.MyModule.GetIcon, PerformAction = CircularStyleAction.MyModule.PerformAction, ButtonText = CircularStyleAction.MyModule.ButtonText });

            //MenuActionModules.Add(new MenuActionModule() { Id = "078318bc-907f-4ada-b1e5-171799957b2a", Name = "Open file", HelpText = "Opens a tree file.", ItemText = StandardOpenMenuItem.ItemText, ParentMenu = StandardOpenMenuItem.ParentMenu, GroupId = StandardOpenMenuItem.GroupId, ShortcutKey = Avalonia.Input.Key.O, ShortcutModifier = Avalonia.Input.KeyModifiers.Control, IsEnabled = StandardOpenMenuItem.IsEnabled, PerformAction = StandardOpenMenuItem.PerformAction, PropertyAffectingEnabled = StandardOpenMenuItem.AffectedProperty });
            //MenuActionModules.Add(new MenuActionModule() { Id = "98804064-922f-4395-8a96-216d4b3ff259", Name = "Open file (advanced)", HelpText = "Opens a tree file, specifying which modules should be used for the reading and loading of the file.", GroupId = StandardAdvancedOpenMenuItem.GroupId, ItemText = StandardAdvancedOpenMenuItem.ItemText, ParentMenu = StandardAdvancedOpenMenuItem.ParentMenu, ShortcutKey = Avalonia.Input.Key.O, ShortcutModifier = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift, IsEnabled = StandardAdvancedOpenMenuItem.IsEnabled, PerformAction = StandardAdvancedOpenMenuItem.PerformAction, PropertyAffectingEnabled = StandardAdvancedOpenMenuItem.AffectedProperty });
            //MenuActionModules.Add(new MenuActionModule() { Id = "d8d189b2-98ab-4630-ab25-4aad94bd10c3", Name = "Exit", HelpText = "Closes the current window.", ItemText = StandardExitMenuItem.ItemText, ParentMenu = StandardExitMenuItem.ParentMenu, GroupId = StandardExitMenuItem.GroupId, ShortcutKey = Avalonia.Input.Key.F4, ShortcutModifier = Avalonia.Input.KeyModifiers.Alt, IsEnabled = StandardExitMenuItem.IsEnabled, PerformAction = StandardExitMenuItem.PerformAction, PropertyAffectingEnabled = StandardExitMenuItem.AffectedProperty });
            //MenuActionModules.Add(new MenuActionModule() { Id = "650a0ef5-5322-4511-ae86-68bd87b47ecd", Name = "Copy selected node (menu item)", HelpText = "Copies the selected node to the clipboard.", ItemText = StandardCopyMenuItem.ItemText, ParentMenu = StandardCopyMenuItem.ParentMenu, GroupId = StandardCopyMenuItem.GroupId, IsEnabled = StandardCopyMenuItem.IsEnabled, PerformAction = StandardCopyMenuItem.PerformAction, PropertyAffectingEnabled = StandardCopyMenuItem.AffectedProperty });

            try
            {
                List<ModuleMetadata> installedModules = JsonSerializer.Deserialize<List<ModuleMetadata>>(File.ReadAllText(ModuleListPath), Modules.DefaultSerializationOptions);
                MissingModules = new List<ModuleMetadata>();

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
        public Func<TreeCollection, Dictionary<string, object>, TreeNode> Transform { get; set; }

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

        public PlottingModule()
        {

        }
    }


    public class FurtherTransformationModule : Module
    {
        public delegate void TreeTransformDelegate(ref TreeNode tree, Dictionary<string, object> parameterValues);

        public Func<TreeNode, List<(string, string)>> GetParameters { get; set; }
        public ParameterChangeDelegate OnParameterChange { get; set; }
        public TreeTransformDelegate Transform { get; set; }

        public bool Repeatable { get; set; }

        public FurtherTransformationModule()
        {

        }
    }


    public class SelectionActionModule : Module
    {
        public string ButtonText { get; set; }
        public Func<Page> GetIcon { get; set; }
        public Action<TreeNode, MainWindow, InstanceStateData> PerformAction { get; set; }
        public Func<TreeNode, MainWindow, InstanceStateData, bool> IsAvailable { get; set; }
        public Avalonia.Input.Key ShortcutKey { get; set; }
        public Avalonia.Input.KeyModifiers ShortcutModifier { get; set; }
        public bool TriggerInTextBox { get; set; }
        public bool IsAvailableInCommandLine { get; set; }
        public SelectionActionModule()
        {

        }
    }

    public class ActionModule : Module
    {
        public string ButtonText { get; set; }
        public Func<Page> GetIcon { get; set; }
        public Action<MainWindow, InstanceStateData> PerformAction { get; set; }
        public Avalonia.Input.Key ShortcutKey { get; set; }
        public Avalonia.Input.KeyModifiers ShortcutModifier { get; set; }
        public bool TriggerInTextBox { get; set; }
        public bool IsAvailableInCommandLine { get; set; }
        public ActionModule()
        {

        }
    }


    public class MenuActionModule : Module
    {
        public string ItemText { get; set; }
        public string ParentMenu { get; set; }
        public string GroupId { get; set; }
        public Func<MainWindow, Task> PerformAction { get; set; }
        public Func<MainWindow, bool> IsEnabled { get; set; }
        public Avalonia.Input.Key ShortcutKey { get; set; }
        public Avalonia.Input.KeyModifiers ShortcutModifier { get; set; }
        public bool TriggerInTextBox { get; set; }
        public Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; set; }

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
                    ShortcutKey = (Avalonia.Input.Key)GetProperty(compiledModule, "ShortcutKey"),
                    ShortcutModifier = (Avalonia.Input.KeyModifiers)GetProperty(compiledModule, "ShortcutModifier"),
                    TriggerInTextBox = (bool)GetProperty(compiledModule, "TriggerInTextBox"),
                    GetIcon = (Func<Page>)Delegate.CreateDelegate(typeof(Func<Page>), compiledModule, "GetIcon"),
                    PerformAction = (Action<MainWindow, InstanceStateData>)Delegate.CreateDelegate(typeof(Action<MainWindow, InstanceStateData>), compiledModule, "PerformAction"),
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
                    Transform = (FurtherTransformationModule.TreeTransformDelegate)Delegate.CreateDelegate(typeof(FurtherTransformationModule.TreeTransformDelegate), compiledModule, "Transform")
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
                    GroupId = (string)GetProperty(compiledModule, "GroupId"),
                    PropertyAffectingEnabled = (Avalonia.AvaloniaProperty)GetProperty(compiledModule, "PropertyAffectingEnabled"),
                    ShortcutKey = (Avalonia.Input.Key)GetProperty(compiledModule, "ShortcutKey"),
                    ShortcutModifier = (Avalonia.Input.KeyModifiers)GetProperty(compiledModule, "ShortcutModifier"),
                    TriggerInTextBox = (bool)GetProperty(compiledModule, "TriggerInTextBox"),
                    IsEnabled = (Func<MainWindow, bool>)Delegate.CreateDelegate(typeof(Func<MainWindow, bool>), compiledModule, "IsEnabled"),
                    PerformAction = (Func<MainWindow, Task>)Delegate.CreateDelegate(typeof(Func<MainWindow, Task>), compiledModule, "PerformAction")
                };

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
                    PlotAction = (Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>, Graphics, Point[]>)Delegate.CreateDelegate(typeof(Func<TreeNode, Dictionary<string, object>, Dictionary<string, Point>, Graphics, Point[]>), compiledModule, "PlotAction")
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
                    GetIcon = (Func<Page>)Delegate.CreateDelegate(typeof(Func<Page>), compiledModule, "GetIcon"),
                    IsAvailable = (Func<TreeNode, MainWindow, InstanceStateData, bool>)Delegate.CreateDelegate(typeof(Func<TreeNode, MainWindow, InstanceStateData, bool>), compiledModule, "IsAvailable"),
                    PerformAction = (Action<TreeNode, MainWindow, InstanceStateData>)Delegate.CreateDelegate(typeof(Action<TreeNode, MainWindow, InstanceStateData>), compiledModule, "PerformAction"),
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
                    Transform = (Func<TreeCollection, Dictionary<string, object>, TreeNode>)Delegate.CreateDelegate(typeof(Func<TreeCollection, Dictionary<string, object>, TreeNode>), compiledModule, "Transform")
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
                    /*fs.Seek(0, SeekOrigin.Begin);
                    bool verified = CryptoUtils.ModuleRSADecrypters[1].VerifyData(fs, Convert.FromBase64String(ReferenceSignatures[i]), HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1);*/
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

                /*using (FileStream fs = new FileStream(Path.Combine(tempDir, "Module.json"), FileMode.Open))
                {
                    metaData = await System.Text.Json.JsonSerializer.DeserializeAsync<ModuleMetadata>(fs, Modules.DefaultSerializationOptions);
                }*/

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
                        Page icon = ((ActionModule)loadedModule).GetIcon();

                        Page pag = new Page(42 / icon.Height * icon.Width + 20, 62);

                        GraphicsPath button = new GraphicsPath().MoveTo(5, 0).LineTo(pag.Width - 5, 0).Arc(pag.Width - 5, 5, 5, -Math.PI / 2, 0).LineTo(pag.Width, pag.Height - 5).Arc(pag.Width - 5, pag.Height - 5, 5, 0, Math.PI / 2);
                        button.LineTo(5, pag.Height).Arc(5, pag.Height - 5, 5, Math.PI / 2, Math.PI).LineTo(0, 5).Arc(5, 5, 5, Math.PI, Math.PI / 2 * 3).Close();

                        pag.Graphics.FillPath(button, Colour.FromRgb(217, 217, 217));

                        pag.Graphics.Translate(10, 10);
                        pag.Graphics.Scale(42 / icon.Height, 42 / icon.Height);
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
                        customIconHeight = 102;
                    }
                    break;
                case ModuleTypes.Coordinate:
                    parameters.AddRange(from el in ((CoordinateModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.FileType:
                    break;
                case ModuleTypes.FurtherTransformation:
                    parameters.AddRange(from el in ((FurtherTransformationModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.LoadFile:
                    break;
                case ModuleTypes.MenuAction:
                    break;
                case ModuleTypes.Plotting:
                    parameters.AddRange(from el in ((PlottingModule)loadedModule).GetParameters(testTree) select (el.Item1, el.Item2, false));
                    break;
                case ModuleTypes.SelectionAction:
                    {
                        Page icon = ((SelectionActionModule)loadedModule).GetIcon();

                        Page pag = new Page(42 / icon.Height * icon.Width + 20, 62);

                        GraphicsPath button = new GraphicsPath().MoveTo(5, 0).LineTo(pag.Width - 5, 0).Arc(pag.Width - 5, 5, 5, -Math.PI / 2, 0).LineTo(pag.Width, pag.Height - 5).Arc(pag.Width - 5, pag.Height - 5, 5, 0, Math.PI / 2);
                        button.LineTo(5, pag.Height).Arc(5, pag.Height - 5, 5, Math.PI / 2, Math.PI).LineTo(0, 5).Arc(5, 5, 5, Math.PI, Math.PI / 2 * 3).Close();

                        pag.Graphics.FillPath(button, Colour.FromRgb(217, 217, 217));

                        pag.Graphics.Translate(10, 10);
                        pag.Graphics.Scale(42 / icon.Height, 42 / icon.Height);
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
                        customIconHeight = 102;
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

            builder.AppendLine("# " + this.Name);
            builder.AppendLine();

            string iconSource = "";

            double iconHeight = 210;

            switch (this.ModuleType)
            {
                case ModuleTypes.FileType:
                    {
                        byte[] bytes;
                        using (MemoryStream stream = new MemoryStream())
                        {
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FileTypeTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.LoadFileTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.TransformerTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.FurtherTransformationTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.CoordinatesTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.PlotActionTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.SelectionActionTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.ActionTemplate.png").CopyTo(stream);
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
                            Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.MenuActionTemplate.png").CopyTo(stream);
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
                builder.AppendLine("<img src=\"" + iconSource + "\" height=\"" + iconHeight.ToString(System.Globalization.CultureInfo.InvariantCulture) + "\" align=\"right\"/>");
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
                            /* string min = minRange.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture);
                             string max = maxRange.ToString(formatString, System.Globalization.CultureInfo.InvariantCulture);*/
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

        public InstanceStateData()
        {

        }
    }
}
