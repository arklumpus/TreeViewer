using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace UnrootTree
{
    /// <summary>
    /// This module transforms a rooted tree into an unrooted tree.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Unroot tree";
        public const string HelpText = "Unroots the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f06dce2a-794b-4897-a154-82f7f44c125d";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACvSURBVDhPYxhwwAilSQZFRUUJQGo+M5Tz39LS8uHx48cvgPiEAEwzEDsygUWADCCeD5XAC5A19/X1HQC7AGjzA6ALDgKZ6/G5BF0zSAxsAAgQMgSbZhDACESgQgcgtf8Gtz1EAAo0voLMRtUMAigGwDQDcSJQ4QKwIBTgcgHcC/g0gwDISyCvAZkgLx4EeRkkDotGvJphAJshTMRqhgGomkQgBukB2/4f6r+RCRgYABT8bhcGAgz9AAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACzSURBVEhL7ZRBEoMgDEXRrRfpLVquxEG4kvUWvUjXLWE+DmhEg4y68G3yZ0z+Z4yibk6nQa2KMeblSk+6ekBsTowB7sEPUltr39AipuYO3ULE9GgUwZnTQeMAjUqIQpbMSSQ7yDUusTYzW7IkZEsv+xVNBz/dEyrl8R2gPOxBuCVX5dhXJDEPrM2MASXmgdys38EecwK9yX+Eqtq95gEmxNO4gHAHEUXmOZKrorb5zRVQ6g8vCW0CIPHKEgAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADbSURBVFhH7ZVNEoMgDIWxWy/SW7RciYNwJdtb9CJdK2GCI8ivBnTBt0mcMe+9UaKs07maAWtThBBvVSbomwfYmgNNA7jmCm4FUDfM2HIp5Qd7Enzm4PHAC5cJB0gImUPjBuBYAZIQMXNgdwZSAyXkaHkPIUWIXI3gFpwJUTIbXcMjIUpnogEAV/A3vrDz8/x/sdMkA4fWsBn3fQVHzA0ls94AZ8wNuRq7ABTmhhwtKwCluSGluW5BDXMANax/DFaNDlDL3OAJsTLUNk8BT+Ayc2D7JWxu3uncAMYW5QKdc01O3iIAAAAASUVORK5CYII=";

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
            return new List<(string, string)>()
            {

            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();
            return true;
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            tree = tree.GetUnrootedTree();
        }
    }
}
