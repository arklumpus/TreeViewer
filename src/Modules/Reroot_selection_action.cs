using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace RerootSelectionAction
{
    /// <summary>
    /// This module is used to apply the _Reroot tree_ Further transformation using the selected node as an outgroup.
    /// 
    /// **Note**: using this module without installing also the _Reroot tree_ module (id `c6f96861-11c0-4853-9738-6a90cc81d660`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Root tree on selection";
        public const string HelpText = "Re-roots the tree using the selection as outgroup.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.1");
        public const string Id = "77f387fb-c843-4164-aed2-bd5b8f325809";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Re-root tree";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Arrange";
        public static double GroupIndex { get; } = 4;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFNSURBVFhH7ZZRDoIwDIaZ8c2LeAvfPQ48G/UAcBeP4C28iM/Ijy0pyzq2uYXE8CXNxlbKv7YzVqmcL48eRo/J7GhcjU3AJmB1AYbGibqub8Nw/T7pvA6ncTy+n+MYwL3rOsSeMctA34/XevHjiSzHhYAhAz2MlrLgi/lfTUgljMIENp2zgYD90aZpgpqYuCMDIc4xjRnlu6dJNZxwdiX5ZMOJkhrSjidBbI6r9oAx6vtOYv2Zn5sQNcdpcM1ktvjqUY+p5LgFSzX37me7hm3bGhhqz0ZbXrIJACl9kFWARP4+YC5NMgmwnWzHEOz3uCyYIztskkmAdHA5SmHSXLjWpT9ic58El8AWxybR1oG2XqQHtAy4yCbAdboQvAIQNOZOA+2kmsDsJdA+VLwEPiCqSAZi0+0C/4jGKDF1ZqSApdQXyQAH9Z0Ye/p+VX0A++O2Bt8aiN0AAAAASUVORK5CYII=";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAFgSURBVGhD7Zi/boMwEMZNBhjyGhn6Ft275FmAqa06VSVLgGfJkr1v0Rfp0oXeoaOyKAZ8/DmQ7idZdpzYvk8+fzEEZgGe3m4VNc39/bzIGg0HqneLCpBGBUijAqRRAdI4BcRxfKqqynCKTdf3vqWPzr95HJQkyUcQBC/U5cXX8ZFaxjx8f1KLB8SSlWX5Sh//0bkDEDgWVvBzMxTH7s+A86YIKfSXfEVRLHqjbIMpnKbpqPXVRqXZ/xmAXM+gTqGEdc84fiBPr332NgRzXZs6BtwBziQh2BuOm8KU4JE6BhTAnSSimsuU4BsiTCEvu5zLXqfMY49VF5Jm8RSC33Pc5gJzO+9Aa6cQx22eqR5kDQFzuI2TVc8Appyr5HnOcjR1IWk2IQDv/1x0B+bEPtTUNchmBOCLhLHYQjchwCf4NnoGpFEB0ngLsB2AukTRFJJGBUizewHeD/VbQ1NIFmN+AfLL0EpMdigmAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAB2HAAAdhwGP5fFlAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAYpJREFUeJztmTFOwzAYRmOLjSPQ7tyCHQZuwQGirEzMyUyPwVJmbsEBuAJjYgaUqk2H2PHfvEr+3hbJTV4//Y2/pK5amcfXj3B8vH97dms7HOPJi18DCoAWoFEAtACNAqAFaBQALUCjAGgBGgVAC9DMPok1TfM0DMOuqqo7iwt+3z6cHN//flmc9kAI4cd7/9K27WfM+tkJ6Pv+vTL68mvgnNuEEHax62cDcM5t8pQQtrELi78H3KR+oOu6zDc43cnRPu9kB+q6DvOrzil+AhQALUCjAGgBGmfV9FIbWCzWTXRk9PVWTS+1gcVyqSY6+nrjphfdwGK5cBPdFn8POGuCqU1vaQNbSm4TnfoWPwEKgBagSX4atCZ1n5/+hnP7Bz4Buft8bv/AAzDa5xf3DzwAGvweMCV2n7fqH8VPgAKgBWgUAC1Ac3W7QCz5/0/8owmgBdZmOjnFT4ACoAVoFAAtQKMAaAGa7B5g1cgoip8ABUAL0CgAWoBGAdACNPj7ALpHFD8BCoAWoPkDLlpt4WVFCQ0AAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon32Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon48Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon64Base64);
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

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return new List<bool>() { (selection != null && selection.Parent != null && (selection.Parent.Parent != null || selection.Parent.Children.Count > 2)) };
        }

        public static void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "c6f96861-11c0-4853-9738-6a90cc81d660");
            int index = stateData.FurtherTransformationModules().IndexOf(module);

            string[] nodeNames = selection.GetNodeNames().ToArray();

            if (nodeNames.Length == 0 || !selection.IsLastCommonAncestor(nodeNames))
            {
                if (InstanceStateData.IsUIAvailable)
                {
                    MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    box.ShowDialog2(window);
                    return;
                }
                else if (InstanceStateData.IsInteractive)
                {
                    Console.WriteLine();
                    Console.WriteLine("Attention! The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    return;
                }
            }

            if (InstanceStateData.IsUIAvailable && index >= 0)
            {
                TreeNode tree = index == 0 ? window.FirstTransformedTree : window.AllTransformedTrees[index - 1];

                List<TreeNode> originalNodes = new List<TreeNode>();

                foreach (TreeNode node in selection.GetChildrenRecursiveLazy())
                {
                    if (!string.IsNullOrEmpty(node.Name))
                    {
                        TreeNode correspNode = tree.GetNodeFromId(node.Id);

                        if (correspNode != null)
                        {
                            originalNodes.Add(correspNode);
                        }
                    }
                }

                string[] origNodeNames = (from el in originalNodes where !string.IsNullOrEmpty(el.Name) orderby el.Name select el.Name).ToArray();

                TreeNode lca = tree.GetLastCommonAncestor(origNodeNames);

                List<string> newNodeNames = lca.GetNodeNames();
                newNodeNames.Sort();

                if (newNodeNames.SequenceEqual(origNodeNames))
                {
                    nodeNames = origNodeNames;
                }
                else
                {
                    originalNodes.Clear();
                    List<TreeNode> selectionNodes = selection.GetChildrenRecursive();

                    foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                    {
                        if (!selectionNodes.Contains(node))
                        {
                            if (!string.IsNullOrEmpty(node.Name))
                            {
                                TreeNode correspNode = tree.GetNodeFromId(node.Id);

                                if (correspNode != null)
                                {
                                    originalNodes.Add(correspNode);
                                }
                            }
                        }
                    }

                    origNodeNames = (from el in originalNodes where !string.IsNullOrEmpty(el.Name) orderby el.Name select el.Name).ToArray();

                    lca = tree.GetLastCommonAncestor(origNodeNames);

                    newNodeNames = lca.GetNodeNames();
                    newNodeNames.Sort();

                    if (newNodeNames.SequenceEqual(origNodeNames))
                    {
                        nodeNames = origNodeNames;
                    }
                }
            }

            if (index < 0)
            {
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Outgroup:",  nodeNames }, { "Rooting mode:", 1 } });
                if (InstanceStateData.IsUIAvailable)
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await window.UpdateFurtherTransformations(index);
                        window.SetSelection(window.TransformedTree.GetLastCommonAncestor(selection.GetNodeNames()));
                    });
                }
            }
            else
            {
                stateData.FurtherTransformationModulesParameterUpdater(index)(new Dictionary<string, object>() { { "Outgroup:", nodeNames }, { "Rooting mode:", 1 } });

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        await window.UpdateFurtherTransformations(index);
                        window.SetSelection(window.TransformedTree.GetLastCommonAncestor(selection.GetNodeNames()));
                    });
                }
            }
        }
    }
}
