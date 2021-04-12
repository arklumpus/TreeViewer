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
        public static Version Version = new Version("1.1.0");
        public const string Id = "77f387fb-c843-4164-aed2-bd5b8f325809";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Re-root\ntree";
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
            return (selection != null && selection.Parent != null && selection.Parent.Parent != null);
        }

        public static void PerformAction(TreeNode selection, MainWindow window, InstanceStateData stateData)
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

            if (index < 0)
            {
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Outgroup:", selection.GetNodeNames().ToArray() }, { "Rooting mode:", 1 } });
                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(index);
                }
            }
            else
            {
                stateData.FurtherTransformationModulesParameterUpdater(index)(new Dictionary<string, object>() { { "Outgroup:", selection.GetNodeNames().ToArray() }, { "Rooting mode:", 1 } });

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(index);
                }
            }
        }
    }
}
