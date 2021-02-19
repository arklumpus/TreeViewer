using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace SwitchSelectionAction
{
    /// <summary>
    /// This module is used to apply the _Switch chidren_ Further transformation to the selected node.
    /// 
    /// **Note**: using this module without installing also the _Switch children_ module (id `c4c71099-c7dc-44b3-93be-25a79afb1102`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Switch selection";
        public const string HelpText = "Switches the order of the children of the selection.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "0b27abb2-0d48-40c0-9d1e-cfc7ffb7284c";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Switch\nchildren";
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
            return selection != null;
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

            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "c4c71099-c7dc-44b3-93be-25a79afb1102");
            Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
            changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

            if (InstanceStateData.IsUIAvailable)
            {
                _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1).ContinueWith(task =>
                {
                    window.SetSelection(window.TransformedTree.GetLastCommonAncestor(selection.GetNodeNames()));
                });
            }
        }
    }
}