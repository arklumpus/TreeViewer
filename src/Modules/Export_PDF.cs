using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.PDF;
using System.Text.Json;

namespace ExportPDF
{
    /// <summary>
    /// This module is used to export the tree plot in PDF format.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Export PDF";
        public const string HelpText = "Exports the tree plot as a PDF document.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f26d503e-52aa-46fe-afb7-68d662b4de2e";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Export PDF...";
        public static string ParentMenu { get; } = "File";
        public static string GroupId { get; } = "da6493c8-45e1-4286-bf71-265546c5c412";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = null;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.P;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
        public static bool TriggerInTextBox { get; } = false;


        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Maximum PDF page size:">
                /// This global settings determined the maximum page width and height for exported PDF document. If the tree plot
                /// is larger than this, it is scaled so that it fits in a square of this size. This is necessary because the
                /// standard PDF format imposes a 200x200 inch limit (14'400 units) on the page size, and pages larger than
                /// this may not be visualised correctly in some viewers. Except in rare instances when some numerical underflow
                /// may occur, using a smaller page size should not cause any discernible difference in the actual drawing. The
                /// value can be changed from the global settings window accessible from Edit > Preferences...
                /// </param>
                ("Maximum PDF page size:", "NumericUpDown:10000[\"1\",\"Infinity\"]")
            };
        }


        public static bool IsEnabled(MainWindow window)
        {
            return true;
        }

        public static async Task PerformAction(MainWindow window)
        {
            double maximumPageSize = 10000;

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Maximum PDF page size:", out object maximumPageSizeObject))
            {
                if (maximumPageSizeObject is double maxValue)
				{
					maximumPageSize = maxValue;
				}
				else if (maximumPageSizeObject is JsonElement element)
				{
					maximumPageSize = element.GetDouble();
				}
            }

            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "PDF document", Extensions = new List<string>() { "pdf" } } }, Title = "Save tree plot" };

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

                    if (pag.Width > maximumPageSize || pag.Height > maximumPageSize)
                    {
                        double scale = Math.Min(maximumPageSize / pag.Width, maximumPageSize / pag.Height);

                        Page newPag = new Page(scale * pag.Width, scale * pag.Height);
                        newPag.Graphics.Scale(scale, scale);
                        newPag.Graphics.DrawGraphics(0, 0, pag.Graphics);

                        pag = newPag;
                    }

                    Document doc = new Document() { Pages = new List<Page>() { pag } };
                    doc.SaveAsPDF(result);
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "Error while saving the PDF document:\n" + ex.Message).ShowDialog2(window);
                }
            }
        }
    }
}
