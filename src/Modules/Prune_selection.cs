using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace PruneSelection
{
    /// <summary>
    /// This module is used to apply the _Prune node_ Further transformation to the selected node.
    /// 
    /// **Note**: using this module without installing also the _Prune node_ module (id `ffc97742-4cf5-44ef-81aa-d5b51708a003`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Prune selection";
        public const string HelpText = "Prunes the selected node off the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f8abf1cd-d79f-403b-b0f2-4bb1412839ff";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;
        public static bool IsAvailableInCommandLine { get; } = false;

        public static string ButtonText { get; } = "Prune\nnode";
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

        public static bool IsAvailable(TreeNode selection, MainWindow window, InstanceStateData data)
        {
            return selection != null;
        }

        public static void PerformAction(TreeNode selection, MainWindow window, InstanceStateData data)
        {
            List<string> nodeNames = selection.GetNodeNames();

            if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
            {
                MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                box.ShowDialog2(window);
                return;
            }

            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "ffc97742-4cf5-44ef-81aa-d5b51708a003");
            Action<Dictionary<string, object>> changeParameter = window.AddFurtherTransformation(module);
            if (selection.Children.Count == 0)
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Position:", 0.0 } });
            }
            else
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });
            }

            _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1).ContinueWith(task =>
            {
                window.SetSelection(window.TransformedTree.GetLastCommonAncestor(new string[] { nodeNames[0], nodeNames[^1] }));
            });

        }
    }
}