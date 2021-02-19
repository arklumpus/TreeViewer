using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Threading;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace PasteTreeAction
{
    /// <summary>
    /// This module can be used to paste a tree from the clipboard.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// When this module is invoked (by clicking on the button, or by using the keyboard shortcut), the text contents of the clipboard
    /// are saved to a temporary file, which is then opened using the installed File type and Load file modules. If the tree is in any
    /// known format (e.g. Newick or NEXUS format), these modules are able to load it. If the text in the clipboard does not represent
    /// a valid tree that can be read from any File type module, an error message is displayed.
    /// </description>

    public static class MyModule
    {
        public const string Name = "Paste tree";
        public const string HelpText = "Loads a tree from the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "a916ad8e-2f22-439f-b764-8beb54673f7d";
        public const ModuleTypes ModuleType = ModuleTypes.Action;


        public static bool IsAvailableInCommandLine { get; } = false;
        public static string ButtonText { get; } = "Paste\ntree";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.V;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
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

        public static void PerformAction(MainWindow window, InstanceStateData stateData)
        {
            Avalonia.Application.Current.Clipboard.GetTextAsync().ContinueWith(async textTask =>
            {
                string text = textTask.Result;

                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString().Replace("-", ""));

                System.IO.File.WriteAllText(tempFile, text);

                await Dispatcher.UIThread.InvokeAsync(async () => { await window.LoadFile(tempFile, true); });
            });
        }
    }
}
