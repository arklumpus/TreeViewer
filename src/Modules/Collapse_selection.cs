using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using PhyloTree.Extensions;
using TreeViewer;
using VectSharp;

namespace CollapseSelection
{
    /// <summary>
    /// This module is used to apply the _Collapse node_ Further transformation to the selected node. If this transformation has already been
    /// applied to the selected node, it is removed. This action is only available if the selected node is not a terminal node.
    /// 
    /// **Note**: using this module without installing also the _Collapse node_ module (id `3812314b-e821-4399-abfd-2a929a7a7d80`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Collapse selection";
        public const string HelpText = "Collapses the selected node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "e149aeb6-a019-41e2-8830-e4dc3e0eee43";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Toggle\ncollapse";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        private const string IconBase64 = "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";

        public static Page GetIcon()
        {
            byte[] bytes = Convert.FromBase64String(IconBase64);

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

            Page pag = new Page(icon.Width, icon.Height);
            pag.Graphics.DrawRasterImage(0, 0, icon);

            return pag;
        }

        public static bool IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return selection != null && selection.Children.Count > 0;
        }

        public static void PerformAction(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            List<string> nodeNames = selection.GetNodeNames();

            if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
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

            if (!selection.Attributes.ContainsKey("3812314b-e821-4399-abfd-2a929a7a7d80"))
            {
                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "3812314b-e821-4399-abfd-2a929a7a7d80");
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1).ContinueWith(task =>
                    {
                        window.SetSelection(window.TransformedTree.GetLastCommonAncestor(new string[] { nodeNames[0], nodeNames[^1] }));
                    });
                }
            }
            else
            {
                List<FurtherTransformationModule> furtherTransformations = stateData.FurtherTransformationModules();

                int minIndex = furtherTransformations.Count - 1;

                for (int i = 0; i < furtherTransformations.Count; i++)
                {
                    if (furtherTransformations[i].Id == "3812314b-e821-4399-abfd-2a929a7a7d80")
                    {
                        string[] node = (string[])stateData.GetFurtherTransformationModulesParamters(i)["Node:"];

                        if (node.ContainsAll(nodeNames))
                        {
                            stateData.RemoveFurtherTransformationModule(i);
                            minIndex = Math.Min(minIndex, i);
                        }
                    }
                }

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(minIndex).ContinueWith(task =>
                    {
                        window.SetSelection(window.TransformedTree.GetLastCommonAncestor(nodeNames));
                    });
                }
            }
        }
    }
}