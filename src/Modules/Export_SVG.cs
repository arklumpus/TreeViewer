using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.SVG;

namespace ExportSVG
{
    //Do not change class name
    public static class MyModule
    {
        public const string Name = "Export SVG";
        public const string HelpText = "Exports the tree plot as an SVG file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "99720888-ca7b-43f2-87ec-e683cab859a2";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Export SVG...";
        public static string ParentMenu { get; } = "File";
        public static string GroupId { get; } = "da6493c8-45e1-4286-bf71-265546c5c412";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.P;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift;
        public static bool TriggerInTextBox { get; } = false;

        public static bool IsEnabled(MainWindow window)
        {
            return true;
        }

        public static async Task PerformAction(MainWindow window)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "SVG file", Extensions = new List<string>() { "svg" } } }, Title = "Save tree plot" };

            string result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                try
                {
                    if (System.IO.File.Exists(result))
                    {
                        System.IO.File.Delete(result);
                    }

                    Page pag = window.RenderPlotToPage();

                    pag.SaveAsSVG(result, SVGContextInterpreter.TextOptions.ConvertIntoPaths);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "Error while saving the SVG file:\n" + ex.Message).ShowDialog2(window);
                }
            }
        }
    }
}
