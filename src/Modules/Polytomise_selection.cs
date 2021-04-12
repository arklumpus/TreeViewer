using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace PruneSelection
{
    /// <summary>
    /// This module is used to apply the _Polytomise node_ Further transformation to the selected node.
    /// 
    /// **Note**: using this module without installing also the _Polytomise node_ module (id `19d9a555-07e6-4dac-afc1-d5ffcef35f76`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Polytomise selection";
        public const string HelpText = "Transforms the selected node into a polytomy.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8202afa4-c9a6-47ac-98d5-dd0190c23f63";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;
        public static bool IsAvailableInCommandLine { get; } = false;

        public static string ButtonText { get; } = "Create\npolytomy";
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
            return selection != null && selection.Children.Count > 1 && selection.Parent != null;
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

            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "19d9a555-07e6-4dac-afc1-d5ffcef35f76");
            Action<Dictionary<string, object>> changeParameter = window.AddFurtherTransformation(module);

            changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

            _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1).ContinueWith(task =>
            {
                window.SetSelection(window.TransformedTree.GetLastCommonAncestor(new string[] { nodeNames[0], nodeNames[^1] }));
            });

        }
    }
}