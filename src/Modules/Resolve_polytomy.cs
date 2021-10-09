using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace a45e08b7008524141ac678c8cd574926d
{
    /// <summary>
    /// This module is used to resolve a polytomy in the tree. The polytomy is resolved by specifying two nodes that
    /// will be joined together. If the two nodes are not siblings or are not part of a polytomy, nothing is done.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Resolve polytomy";
        public const string HelpText = "Resolves a polytomy in the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "45e08b70-0852-4141-ac67-8c8cd574926d";

        public static bool Repeatable { get; } = true;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABhSURBVDhPY6AUMEJpOCgqKvoPZZIHSDWACUoPHMAIA6/ajf+3NfujiMPEiPIeSDGUCQfYxGAAbGpfXx8jPkUggO4qggDZQHyGs0BpogDFaYQkQKxtFCcknCFLX/+SDxgYAJ5gJ+cAGuNtAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACaSURBVEhLYxjygBFKY4CioqIGIFUP4ZEPmKA0NkCx4XgB0Af/QRjKJRvg8wFVAM0toDnAmYq8ajeCw39bsz9WNUB5cCoDyeOLK0qCiLJUBvIBzBfYACF5GIB7r6+vj5EYDegAVxDCwOBIRdiCg+gggtJ4Acwg5OBAFqNVKmqE0kMAUFLwDf3CjqhURG7wgACxPhhCKYa6gIEBAPQUQHgCpn8dAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAEgSURBVFhH7ZVNDoMgEIXR43XR3sV907+13qWL9nrWR51kJMo8KsQu/BKiwASeb2Cs3Ax9349v5ZkVoCktxhTQNM11eFy+vfzU4zNGsc0B44DPQdu21YAfY2HSxziwCoiOtaICsIFFcQcsdgFJx7pEUTIFHM5Pv+v7cWJifdFCrFxfi9wpyF+04IC4YMHGIpWSzlo6YbOw4sO5sA/wPslV13UV+7UxcAb0RjH+7xqK8qUyKg697sdJAOJlbpUD8pOwYOMsklfQX+kHFHpuqzpwG580tAPh2VjKsTUfkiQAi4u1uLIyrmEFCqtTgAV1E8JxaSGb14FdAC1gLn8Ahy3WLOZXjcAWGJZfUpBcbGLQDsBOpIGxNQXTAb1h7s2dc+4DOsTBOz+rFWoAAAAASUVORK5CYII=";

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
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Node 1:">
                /// This parameter determines the first node that will be joined. The two nodes must be siblings and part of a polytomy.
                /// </param>
                ("Node 1:", "Node:[" + System.Text.Json.JsonSerializer.Serialize(leafNames[0]) +"," + System.Text.Json.JsonSerializer.Serialize(leafNames[^1]) + "]"),
                
                /// <param name="Node 2:">
                /// This parameter determines the second node that will be joined. The two nodes must be siblings and part of a polytomy.
                /// </param>
                ("Node 2:", "Node:[" + System.Text.Json.JsonSerializer.Serialize(leafNames[0]) +"," + System.Text.Json.JsonSerializer.Serialize(leafNames[^1]) + "]"),

                /// <param name="Position:">
                /// This parameter determines the length of the branch that resolves the polytomy.
                /// </param>
                ("Position:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Apply">
                /// Applies the changes to the other parameter values and triggers an update of the tree.
                /// </param>
                ("Apply", "Button:")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return previousParameterValues["Node 1:"] != currentParameterValues["Node 1:"] || previousParameterValues["Node 2:"] != currentParameterValues["Node 2:"] || (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string[] node1Elements = (string[])parameterValues["Node 1:"];
            TreeNode node1 = tree.GetLastCommonAncestor(node1Elements);

            string[] node2Elements = (string[])parameterValues["Node 2:"];
            TreeNode node2 = tree.GetLastCommonAncestor(node2Elements);

            if (node1 == null || node2 == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            if (node1.Parent != node2.Parent)
            {
                throw new Exception("The two selected nodes are not siblings!");
            }

            if (node1 == node2)
            {
                throw new Exception("The two nodes are the same!");
            }

            if (node1.Parent.Children.Count <= 2)
            {
                throw new Exception("The two selected nodes are not part of a polytomy!");
            }

            double position = (double)parameterValues["Position:"];

            double branchLength = Math.Min(node1.Length, node2.Length) * position;

            TreeNode newParent = new TreeNode(node1.Parent);
            newParent.Length = branchLength;

            node1.Parent.Children.Remove(node1);
            node1.Parent.Children.Remove(node2);

            newParent.Children.Add(node1);
            newParent.Children.Add(node2);

            TreeNode oldParent = node1.Parent;

            node1.Parent = newParent;
            node2.Parent = newParent;

            node1.Length -= branchLength;
            node2.Length -= branchLength;

            oldParent.Children.Add(newParent);
        }
    }
}
