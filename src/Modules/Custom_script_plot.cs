/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
using System.Reflection;
using System.Text;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

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
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACWSURBVDhP1ZJRDkAwDIaHU+0KjrNnCS6wsziCtx2L/tUmRcyCF1/StJO/Xf04JYQwUCxyvAQaaOXoasmgpxi3Mgs00O4puV2xWrvBI74ZYE0pRXsaKdjAlNKMh3d47ytKPTI24OYYI09su2lB5GrR8td47UFlX0G3uMP21HYdilLOF9LUP/9IipiT5aixA0qNZAO30rkVjzJIG2cH0LQAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD7SURBVEhL7ZPBDcIwDEVbmIUhcuLOBqyRO0q7QOfgygRUXDoEw4C/5VaJSaqkQCUknhQ1cR3/OHaqP8VYaxsaD1lmAX/sk2XARr6MODkaLRvygb+LiQQCBAfvui56mhTizyJsSFF6NZrYfp3Bx1lPINUFpeg4LCDGJd2jeemmrR987J7D6dLs9scrjfp+O/e5tmEYemNMTb8cvlgjA0eBa9WaY7v5bZdlQxzEoynbINBSFvoljlflX1mWDXEQj6Zsg1JQA5VJEbE4XGRZQHFKdSHzh5TUFhPbP72Db7G+gBSqmNQ+LcCFLhURfy4wGzwCgTe6ab57fpiqegJBnIHJmwfa/AAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAOZSURBVFhH7ZdbSBRhFMe/mVmQVNouUpSmEfrWBSqh20sRCoVlyeJbCJUPG7nqGj10YUF9qV1v0dKFinozpRISISsIEh8SpJAIeqk2XwzDjQradWf6n5mz084sPuzORj34g89zzjfznXPmmzNnP8Ui/xqJZRpNTU0VLpdrayQSGRwYGEjwdEZ4PB6lpKSkLpFITPb19b3naQtpCXi93sK8vLzbUD1kq6q6pbe39w3pmeLz+TbLsvwaqgbux2KxE+Fw+Ltx1UBmaYLgdyH04GASY4mhZgWtJR8SqIfvO/psCpYdaG5u3o0bX7I54Xa7dwYCgXm2swLrXdFodBzqdrKxE3t6enrGSCcsO4Dgh1klfdhpcIJ8wNcIm+S3jlUdewIVrFKmEVZzQaovMwZhSQBBS1mlZOKs5oJfLIm1LHXsRehi+TexxLAnoLCk3XD8/pPYfJkxCHsC61jS9/+ZVccggY+sEqtZ6pgJtLS01EAsNSzxBQ3kFeuOQQOiZjRrWKIIDaqadSG1trbWIcNK6KcwCjE0FGBDV1fXPbqBOHBuuEzI8RBud2tC6hhpP/SiOjBUriREELcvkYUSeNxeM37w4tBeTRPnsWR2XtX8TzprzerHA3ohrhqWmEOMK4g7JePPICbOYlDwT7hQnxpcR57vQHD6fvdLQtOvyQlxCQJ9Q6pShXaL5hCcru3D8LhkCWv+0N3dHYZvSmIOYxniXoDst9eAytKKZimcpJ4yp6XNSdY1Oqgr+h2IGZYBJVCPcRnjJ8Z6ZNaP19IA3URyKbStD/D0zzRNOqbPKYkzEEMYo6oqH9fnNI2uPUd/H0y4VFpjAp8nsQO0Q6swoojTCVlr/hbgHVEbfmRY4quiKKXBYPAH245obGzMLygooK9qOdko8KpQKDSq6/SHwDuip/lmWGJFPB7fxrpj8vPzd0DowcEMgj9lPa0PmFWL7TJ7Qg5Yw5J6wgwJw0pPwDz5YJvSiihbUn3hwSyFbk/AbJnIdMHjWhakxrG0+AVfARJIdkXHwJebVeIDSx17Au9Y0lZtZDUXbGJJvGWpY0kAQfshkgVytK2tjb5ZR+CYRwVYa1hCxQmZYphYEkALpgPkTcMSRbh5DA1kF9sZQ2vxUHTGXGnMiBs4nk+xrmN/BQIH0dMQ1zCoWsuRhOUYnQm8dgOGijq4Dt8+/UIKC1a63+8vg4PK6enph07+MSkuLj4CdQInYUvxLfKfIMRvVZY2LKUZwFcAAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

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
