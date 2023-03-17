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
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace RerootTree
{
    /// <summary>
    /// This module is used to re-root the tree using a specific node as the outgroup.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Reroot tree";
        public const string HelpText = "Re-roots the tree using the specified outgroup.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.2");
        public const string Id = "c6f96861-11c0-4853-9738-6a90cc81d660";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAB5SURBVDhPY8AHvGo3/ocycQImKE02oNgARijNUFRUhOHcG9z2DBpfD0J5CNDX1wfXBwfYDMAG0NVR7gVsNmNzIk4XokvgUohNHCTGAmXjBMgaYWxkFxI0AASQNaC7hP4JCT2AKXIByDAMA7BFIT5AcRhgjV/iAQMDAIR3L5AFry/3AAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC9SURBVEhLYyAWeNVu/A/CUC7RgAlK0wyMWkAQMEJpMCgqKmoAUvUQHiq4wW0PpjW+HgTTOEBjX18fyAw4QPcBVsNJAPj1A33wH4ShXJIALr20jwN84Q4EGGGKDAjoBYFGkA/wKSAUJwTlWaAMBqBL0VMU0XGBrhcEYPppHgdwHxALsIU71LVY44scH+AKd6ziZAcRKNxhGCqEFdA8DqhiATafwMSGhg/wAZwWYPM2OWDoBxE4F5JS7pAGGBgAH+1FP/vxtzgAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFNSURBVFhH7ZZRDoIwDIaZ8c2LeAvfPQ48G/UAcBeP4C28iM/Ijy0pyzq2uYXE8CXNxlbKv7YzVqmcL48eRo/J7GhcjU3AJmB1AYbGibqub8Nw/T7pvA6ncTy+n+MYwL3rOsSeMctA34/XevHjiSzHhYAhAz2MlrLgi/lfTUgljMIENp2zgYD90aZpgpqYuCMDIc4xjRnlu6dJNZxwdiX5ZMOJkhrSjidBbI6r9oAx6vtOYv2Zn5sQNcdpcM1ktvjqUY+p5LgFSzX37me7hm3bGhhqz0ZbXrIJACl9kFWARP4+YC5NMgmwnWzHEOz3uCyYIztskkmAdHA5SmHSXLjWpT9ic58El8AWxybR1oG2XqQHtAy4yCbAdboQvAIQNOZOA+2kmsDsJdA+VLwEPiCqSAZi0+0C/4jGKDF1ZqSApdQXyQAH9Z0Ye/p+VX0A++O2Bt8aiN0AAAAASUVORK5CYII=";

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
                /// <param name="Rooting mode:">
                /// This parameter determines the algorithm used to re-root the tree. If the selected value is
                /// `Mid-point`, the root of the tree is determined automatically as the point halfway between
                /// the two most distant tips. If the selected value is `Outgroup`, the root of the tree is
                /// determined by the user-selected outgroup.
                /// </param>
                ("Rooting mode:", "ComboBox:1[\"Mid-point\",\"Outgroup\"]"),
                
                /// <param name="Outgroup:">
                /// This parameter determines the node used as the outgroup. If only a
                /// single node is selected, the outgroup corresponds to that node. If more than one node is
                /// selected, the outgroup corresponds to the last common ancestor (LCA) of all of them. Nodes
                /// are selected based on their `Name`.
                /// </param>
                ( "Outgroup:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),
                
                /// <param name="Position:">
                /// This parameter determines which proportion of the branch on which the root is placed is
                /// ascribed to the outgroup and which proportion to the ingroup. If the value is `0%`, the
                /// outgroup has branch length 0 and the ingroup's branch length corresponds to the original
                /// length of the branch on which the root is placed; if the value is `100%`, the ingroup has
                /// branch length 0 and the outgroup's branch length corresponds to the original branch length.
                /// </param>
                ( "Position:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw
                /// of the tree.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            if ((int)currentParameterValues["Rooting mode:"] == 0)
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Outgroup:", ControlStatus.Hidden },
                    { "Position:", ControlStatus.Hidden }
                };
            }
            else
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Outgroup:", ControlStatus.Enabled },
                    { "Position:", ControlStatus.Enabled }
                };
            }

            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };


            return (bool)currentParameterValues["Apply"] || previousParameterValues["Outgroup:"] != currentParameterValues["Outgroup:"] || (int)previousParameterValues["Rooting mode:"] != (int)currentParameterValues["Rooting mode:"];
        }

        private static (double, TreeNode) GetDeepestTip(TreeNode node)
        {
            if (node.Children.Count == 0)
            {
                return (0, node);
            }
            else
            {
                double maxLen = double.MinValue;
                TreeNode maxDeepestTip = null;
                for (int i = 0; i < node.Children.Count; i++)
                {
                    (double chLen, TreeNode deepestTip) = GetDeepestTip(node.Children[i]);
                    chLen += node.Children[i].Length;

                    if (chLen > maxLen)
                    {
                        maxLen = chLen;
                        maxDeepestTip = deepestTip;
                    }
                }

                return (maxLen, maxDeepestTip);
            }
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            int rootingMode = (int)parameterValues["Rooting mode:"];

            if (rootingMode == 0)
            {
                double maxDistance = double.MinValue;
                bool found = false;

                TreeNode subject = tree;

                if (subject.Children.Count < 3)
                {
                    subject = subject.GetUnrootedTree();
                }

                (double _, TreeNode leaf1) = GetDeepestTip(subject);

                TreeNode rerooted = subject.GetRootedTree(leaf1);
                rerooted.Children.Remove(leaf1);

                (double _, TreeNode leaf2) = GetDeepestTip(rerooted);

                if (leaf1 != null && leaf2 != null)
                {
                    leaf2 = subject.GetNodeFromId(leaf2.Id);

                    found = true;
                    maxDistance = leaf1.PathLengthTo(leaf2);
                }

                if (found)
                {
                    TreeNode lca = TreeNode.GetLastCommonAncestor(new TreeNode[] { leaf1, leaf2 });

                    List<TreeNode> path = new List<TreeNode>();
                    List<TreeNode> path2 = new List<TreeNode>();

                    TreeNode anc1 = leaf1;
                    TreeNode anc2 = leaf2;

                    while (anc1 != lca)
                    {
                        path.Add(anc1);
                        anc1 = anc1.Parent;
                    }

                    while (anc2 != lca)
                    {
                        path2.Add(anc2);
                        anc2 = anc2.Parent;
                    }

                    for (int i = path2.Count - 1; i >= 0; i--)
                    {
                        path.Add(path2[i]);
                    }

                    int index = 0;
                    double currDistance = 0;

                    while (currDistance + (!double.IsNaN(path[index].Length) ? path[index].Length : 0) <= maxDistance * 0.5)
                    {
                        currDistance += (!double.IsNaN(path[index].Length) ? path[index].Length : 0);
                        index++;
                    }

                    double proportion = (maxDistance * 0.5 - currDistance) / path[index].Length;

                    if (index >= path.Count - path2.Count)
                    {
                        proportion = 1 - proportion;
                    }

                    tree = subject.GetRootedTree(path[index], proportion);
                }
                else
                {
                    throw new Exception("Could not find a suitable branch for rooting. Does the tree have branch lengths?");
                }
            }
            else if (rootingMode == 1)
            {
                string[] outgroupElements = (string[])parameterValues["Outgroup:"];

                TreeNode outgroup = tree.GetLastCommonAncestor(outgroupElements);

                if (outgroup == null)
                {
                    throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
                }

                double position = (double)parameterValues["Position:"];

                tree = tree.GetRootedTree(outgroup, position);
            }
        }
    }
}
