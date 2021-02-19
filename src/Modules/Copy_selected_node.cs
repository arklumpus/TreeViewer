using PhyloTree;
using TreeViewer;
using VectSharp;
using System;
using PhyloTree.Formats;
using System.Runtime.InteropServices;

namespace Copy_selected_node
{
    /// <summary>
    /// This module copies the currently selected node to the clipboard in Newick-with-attributes format. The node can then be pasted
    /// e.g. in a text editor or in another tree viewer progam.
    /// </summary>

    class MyModule
    {
        public const string Name = "Copy selected node";
        public const string HelpText = "Copies the selected node to the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "debd9130-8451-4413-88f0-6357ec817021";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Copy\nnode";

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
            return (selection != null);
        }

        public static void PerformAction(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            string text = NWKA.WriteTree(selection, true);
            if (!text.EndsWith(";"))
            {
                text += ";";
            }
            _ = Avalonia.Application.Current.Clipboard.SetTextAsync(text);
        }
    }
}
