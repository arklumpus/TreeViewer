using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace SwitchChildren
{
    /// <summary>
    /// This module switches the children of a node, and, optionall, all their descendants.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Switch children";
        public const string HelpText = "Switches the order of the children of the node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "c4c71099-c7dc-44b3-93be-25a79afb1102";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = true;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVDhPY2TAAoqKiv5DmeQBUgxggtJkA6IN8Krd+B+EgYABhGGAKANAGqFMBu+6TSjeI2gAsmYYQDcEA2ALRGQvIAMWYkIcpAloK5jNyIg15lEBPhdAuXDAAqVxAnRNMP62Zn+wUwgGIkwhMkAWIyoakTVgMxAD0DUpY3UOKS7AagB6YsENGBgAnVxGI+3nlw0AAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC6SURBVEhLzZUxDsIwDEXjHq8Ld0lmVBB77sLC9VI+OFKJHLeyieBJlhsP/9exq1IpJfRIKV2eaXmfbExEFKRgXOIq6CzGWBBcMjFxHsbvDDZzUJnPd/UKXR1Ucc3EbNCK9kxMBj0xqW4yeNxOhODjC6kGXDM4Ah35kHLO4kptr0R6ezC+A84itbu/7sC8pu1KSjVgXlN+/OCra9qKDZlBFe2JA/eQNXHgNthjuIHanvd/DPY6uHI2EsIKyvRMkS58JvYAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEaSURBVFhH7ZbPDoMgDMbBJ5vX7bB30fP+ZWd9l+2wq3szRrEsxlFokxoTs19CGgTtx5cCWuec4dC27dWHy9jTo7LWGk7zqCcHwpc5NE0TrOq6jv0Ohwojm5Q7qcadKxagzSIC9qfH7nB+Ooj4iERdACYdxp4ZSiJUBcySR7IiWAI4ZwWRPEKK0HSASh5JjrP2NDjgT8JgQ9/3xXf8ar+Wve7H7PxFdoGE7QmYFyz0c0WsKiCXiBqz8ZLhsrkiDD8kcCuViE6pO8BJviT/c2D9IsRYpFSE08KjSBWkpgM1RorkuJoAv7q3D5SIGsd/UK0BQgSZHFAvwpmIbHIRUISSeyN3MU1ZfRuKBcDKOI07V3wOaCNx4IZREWM+C0+rf7XsM3gAAAAASUVORK5CYII=";

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
                /// <param name="Node:">
                /// This parameter determines the node whose children are switched. If only a
                /// single node is selected, that node's children are switched. If more than one node is
                /// selected, the node whose children are switched corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Recursive">
                /// If this check box is checked, the action is also applied recursively to all descendas of the
                /// selected node. Otherwise, only the direct children of the selected node are switched.
                /// </param>
                ( "Recursive", "CheckBox:false" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (bool)currentParameterValues["Recursive"] != (bool)previousParameterValues["Recursive"] || !((string[])currentParameterValues["Node:"]).SequenceEqual((string[])previousParameterValues["Node:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            bool recursive = (bool)parameterValues["Recursive"];

            if (!recursive)
            {
                List<TreeNode> newChildren = new List<TreeNode>(node.Children.Count);
                for (int i = node.Children.Count - 1; i >= 0; i--)
                {
                    newChildren.Add(node.Children[i]);
                }
                node.Children.Clear();
                node.Children.AddRange(newChildren);
            }
            else
            {
                List<TreeNode> nodes = node.GetChildrenRecursive();
                for (int j = 0; j < nodes.Count; j++)
                {
                    List<TreeNode> newChildren = new List<TreeNode>(nodes[j].Children.Count);
                    for (int i = nodes[j].Children.Count - 1; i >= 0; i--)
                    {
                        newChildren.Add(nodes[j].Children[i]);
                    }
                    nodes[j].Children.Clear();
                    nodes[j].Children.AddRange(newChildren);
                }
            }
        }

    }
}
