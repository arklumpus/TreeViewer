using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;

namespace abab85a0b90e04859b03140891293c7d7
{
    public static class MyModule
    {
        public const string Name = "Prune and regraft";
        public const string HelpText = "Prunes a subtree and grafts it in another position on the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "bab85a0b-90e0-4859-b031-40891293c7d7";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAClSURBVDhPY2TAAYqKiv5DmXgBigHomvr6+nBagBUgG0CsC1igNFYAMgTZFQQNxabAq3ajAxD/B9FQIRQANh1ZI7KNUE37ITwwcNzW7H8AygYDJiiNK8CQNYMAOh8CYC7A5UeQF6BMDAB3AbmABd1WdD5RsYBTAgrweQFvOoABYMjjdwU+F8Bsh9HoauEuwGXIDSgNAoS8ihUQcgG2xIMXoBrAwAAAeehW+Yx1StQAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD+SURBVEhLtZQxDsIwDEXTio2LcAtgZeEu2RFU7LkLC2vhFlyEGeI0loxlN05U3uImrRx/f6edM+C9v8RwnlZ19DmWaEoOiAq0ikMIJsUUTUFzxRxNwQciVozrFqweDDkCAxycD6f7IiYFlMPptothnFZuf78eH/lZxKogwZIDY95TwR4Xp0ZITlGVoALL1GjJAfXdKsfE3NTECqma9J7uaVR50MLfD/hpEUdqFR/d0iXslQ+KF8hKRw+QLpZElcnWpK00mQyVW6oHZk3m8NbQdWy1+DdYckzFv0GqpDRqyGu9TZEr2LyfEBLcU1Sw2FhyTEYhBQ/ELizpgdAF575beGw3Mj7ksQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE7SURBVFhH7ZY9DsIwDIXbio2LcAtgZeEu3RFU7L0LC2vhFlyEucStE0Vp4sRp+jPwSZVxKTh+eQ7kGZOyLG8iXPtsPAVGDsmKA04FfJ3Wdc1WzwalQNJO2QgFWrgwVbjuxxLjgaRQHui6NPc6ZfdAjAIVRp0KFoqLtb3vhK2AjdPlcRCh6bPs+LyfX/jay2gPGMWBBu8FoboTHVvnnlLAUlwnSAldAdbce4oDQUoMtkAzkw+quCTkmR4wnT5iZu5DdNvChWkwix9E/wVsMDoJ8YFpWo53CsJsrCM1ltwsHjiCA+QEiMOH9fmCMfeTsLgJVedyK1Jvgfhe8r9lMgWgsGP/yd8Y7xj6MDt3KeFSdn0e4PLZ7rtoKrD7viEoQhSY5eBJDnQsuwbMHJSl1F2PB6bC5605FCC8lWU/DtuWwKr9zZ8AAAAASUVORK5CYII=";

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

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {

            };
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                /// <param name="Subtree:">
                /// This parameter determines the subtree that is being pruned and regrafted.
                /// If only a single node is selected, then that node is pruned. If more than
                /// one node is selected, the last common ancestor (LCA) of all of them is
                /// pruned. Nodes are selected based on their `Name`.
                /// </param>
                ("Subtree:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]"),
                
                /// <param name="New sibling:">
                /// This parameter determines the position in the tree where the [Subtree](#subtree)
                /// will be regrafted.
                /// If only a single node is selected, then that node is pruned. If more than
                /// one node is selected, the last common ancestor (LCA) of all of them is
                /// pruned. Nodes are selected based on their `Name`.
                /// </param>
                ("New sibling:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]"),
                
                /// <param name="Position:">
                /// This parameter determines the relative position along the branch leading to the [New sibling](#new-sibling) at
                /// which the [Subtree](#subtree) is grafted. If the value is `0`, a polytomy is created. If the value is `1`,
                /// the [Subtree](#subtree) is grafted as a descendant of the [New sibling](#new-sibling).
                /// </param>
                ( "Position:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw of the tree.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string[] subtreeElements = (string[])parameterValues["Subtree:"];
            string[] newSiblingElements = (string[])parameterValues["New sibling:"];
            double position = (double)parameterValues["Position:"];

            TreeNode subtree = tree.GetLastCommonAncestor(subtreeElements);

            TreeNode newSibling = tree.GetLastCommonAncestor(newSiblingElements);

            if (subtree == null)
            {
                throw new Exception("Could not locate the subtree on the tree!");
            }

            if (newSibling == null)
            {
                throw new Exception("Could not locate the new position on the tree!");
            }

            if (subtree != null && newSibling != null)
            {
                if (!subtree.GetChildrenRecursiveLazy().Contains(newSibling) && subtree.Parent != newSibling.Parent)
                {
                    subtree.Parent.Children.Remove(subtree);

                    if (subtree.Parent.Children.Count == 1)
                    {
                        TreeNode parent = subtree.Parent;
                        TreeNode otherChild = subtree.Parent.Children[0];
                        if (parent.Parent != null)
                        {
                            int index = parent.Parent.Children.IndexOf(parent);

                            if (index >= 0)
                            {
                                parent.Parent.Children[index] = otherChild;
                                otherChild.Length += parent.Length;
                                otherChild.Parent = parent.Parent;
                            }
                        }
                        else
                        {
                            if (parent.Length > 0)
                            {
                                otherChild.Length += parent.Length;
                            }

                            foreach (KeyValuePair<string, object> kvp in parent.Attributes)
                            {
                                if (Guid.TryParse(kvp.Key, out _) && !otherChild.Attributes.ContainsKey(kvp.Key))
                                {
                                    otherChild.Attributes[kvp.Key] = kvp.Value;
                                }
                            }

                            otherChild.Parent = null;
                            tree = otherChild;
                        }
                    }

                    if (newSibling.Parent != null)
                    {
                        if (position > 0 && position < 1)
                        {
                            TreeNode newNode = new TreeNode(newSibling.Parent);
                            newNode.Length = newSibling.Length * position;
                            newSibling.Length *= 1 - position;

                            newSibling.Parent.Children.Remove(newSibling);
                            newSibling.Parent = newNode;
                            newNode.Children.Add(newSibling);
                            newNode.Parent.Children.Add(newNode);

                            newNode.Children.Add(subtree);
                            subtree.Parent = newNode;
                        }
                        else if (position == 0)
                        {
                            newSibling.Parent.Children.Add(subtree);
                            subtree.Parent = newSibling.Parent;
                        }
                        else if (position == 1)
                        {
                            newSibling.Children.Add(subtree);
                            subtree.Parent = newSibling;
                        }
                    }
                    else
                    {
                        if (position > 0 && position < 1)
                        {
                            TreeNode newNode = new TreeNode(null);
                            newNode.Length = newSibling.Length * position;
                            newSibling.Length *= 1 - position;

                            newSibling.Parent = newNode;
                            newNode.Children.Add(newSibling);

                            newNode.Children.Add(subtree);
                            subtree.Parent = newNode;

                            tree = newNode;
                        }
                        else if (position == 0)
                        {
                            TreeNode newRoot = new TreeNode(null);
                            newRoot.Children.Add(tree);
                            tree.Parent = newRoot;
                            newRoot.Children.Add(subtree);
                            subtree.Parent = newRoot;
                            tree = newRoot;
                        }
                        else if (position == 1)
                        {
                            newSibling.Children.Add(subtree);
                            subtree.Parent = newSibling;
                        }
                    }
                }
                else
                {
                    throw new Exception("Cannot regraft a node on one of its descendants!");
                }
            }
        }
    }
}

