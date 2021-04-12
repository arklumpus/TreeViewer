using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace CustomScriptPlot
{
    /// <summary>
    /// This module makes it possible to execute custom C# code to plot the tree. This can be useful either to draw one-off
    /// complicated graphics, or as a first step in developing a new module for TreeViewer.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the other module with the same name is that this module is a Plot action,
    /// while the other _Custom script_ module (id `a76d00d2-95e0-4274-a77d-1439a013e3d9`) is instead a Further transformation.
    /// 
    /// The code in the module can do anything, including loading additional data from a file on disk. However, this is discouraged,
    /// becaus it ties the tree file on the computer it was created on. A better approach to load additional data would be to import
    /// the data file as an attachment and read the data from the attachment. Attachments can be accessed using the `Attachments`
    /// property of the `stateData` object that is passed as a method parameter.
    /// 
    /// Furthermore, since the code in the module can do anything, it may also be a security risk to open files originating from
    /// unknown sources; thus, you should either make sure that any file you open comes from a reputable source, or avoid loading
    /// source code from tree files at all.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Custom script";
        public const string HelpText = "Executes custom code.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "cdb74bfb-8a90-48b3-815a-8f908d2a1ff5";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            StringBuilder defaultSourceCode = new StringBuilder();

            defaultSourceCode.AppendLine("using PhyloTree;");
            defaultSourceCode.AppendLine("using System.Collections.Generic;");
            defaultSourceCode.AppendLine("using VectSharp;");
            defaultSourceCode.AppendLine("using TreeViewer;");
            defaultSourceCode.AppendLine();
            defaultSourceCode.AppendLine("namespace a" + Guid.NewGuid().ToString().Replace("-", ""));
            defaultSourceCode.AppendLine("{");
            defaultSourceCode.AppendLine("\t//Do not change class name");
            defaultSourceCode.AppendLine("\tpublic static class CustomCode");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\t//Do not change method signature");
            defaultSourceCode.AppendLine("\t\tpublic static Point[] PerformPlotAction(TreeNode tree, Dictionary<string, Point> coordinates, Graphics graphics, InstanceStateData stateData)");
            defaultSourceCode.AppendLine("\t\t{");
            defaultSourceCode.AppendLine("\t\t\tPoint topLeft = new Point();");
            defaultSourceCode.AppendLine("\t\t\tPoint bottomRight = new Point();");
            defaultSourceCode.AppendLine("\t\t\treturn new Point[] { topLeft, bottomRight };");
            defaultSourceCode.AppendLine("\t\t}");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("}");

            return new List<(string, string)>()
            {
                /// <param name="Description:">
                /// This parameter can be used to provide a short description to quickly identify what the module does without
                /// having to look at the source code. It is ignored by the module.
                /// </param>
                ( "Description:", "TextBox:Describe the script" ),
                
                /// <param name="Source code:">
                /// This parameter contains the source code of the script. The arguments to the `PerformAction` method are as follows:
                /// 
                /// * `tree`: the final transformed tree that has gone through all the further transformation modules.
                /// 
                /// * `coordinates`: a dictionary associating the Id of each node in the tree to its coordinates. You can use the
                /// script to change the coordinates of some nodes; however, other Plot action modules will not be automatically
                /// notified of this and you will have to invalidate either the Coordinates module or all of the other Plot action
                /// modules for the changes to be applied.
                /// 
                /// * `graphics`: the graphics surface on which the plot should be drawn.
                /// 
                /// * `stateData`: an `InstanceStateData` object that can be used to access features in way that does not depend
                /// on the program running in command-line or GUI mode.
                /// </param>
                ( "Source code:", "SourceCode:" + defaultSourceCode.ToString() ),
                ( "StateData", "InstanceStateData:" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();
            return previousParameterValues["Source code:"] != currentParameterValues["Source code:"];
        }


        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            Assembly assembly = ((CompiledCode)parameterValues["Source code:"]).CompiledAssembly;

            object[] args = new object[] { tree, coordinates, graphics, stateData };

            return (Point[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("PerformPlotAction", BindingFlags.Default | BindingFlags.InvokeMethod,
                        null,
                        null,
                        args);
        }

    }
}
