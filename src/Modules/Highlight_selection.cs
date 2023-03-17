/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using PhyloTree;
using TreeViewer;
using VectSharp;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace a5bf2292fcff34aa89c15e881d426cba6
{
    /// <summary>
    /// This module is used to apply the _Highlight node_ Plot action to the selected node.
    /// 
    /// **Note**: using this module without installing also the _Highlight node_ module (id `64769664-d163-4fce-b7ba-18fd9445fcfb`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Highlight selection";
        public const string HelpText = "Highlights the selected node in the plot.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public const string Id = "5bf2292f-cff3-4aa8-9c15-e881d426cba6";

        public static bool IsAvailableInCommandLine { get; } = true;

        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.I;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;

        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Highlight";
        public static string GroupName { get; } = "Simplify";
        public static double GroupIndex { get; } = 1.5;
        public static bool IsLargeButton { get; } = false;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACRSURBVDhPvZIxEoAgDATR8XU2/oXeQceev9j4PTWKGOCIqdwmMISbI0djMvpxnc7i7t0ncxsWHO1lwiEHO9VtGYozztPXXbsK1tqrKcd7H8VFAYI3I8RDckACj11EFJDsSgLJE3K7NVFOMQOUAkpElQKBXCQpSDa/EiBig2biCPSV51A1vL2aiSOSJ4TlnxhzAMdtPUxJycfxAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADWSURBVEhLtZTBDsMgCIZ16dP1snfxvrRmd99ll71eJw4XElGBrl/SiAcEfqDeCVgfrz0f2/emIt7QmGF5HNikFRxoxvfzDtUMoRWrAuTHfQhhKldKyVcfqUQUlVwLnmogSzQbUKKCRaJiUxlGWCRSIQoAmcOHVxWNk2RKgDMSWZeKpTtFvSmpTZYi6gFIIZGD4/IpMi8a0JusXO1vUBatppWRX8qBc5ASwCJRxFMEtwclMzpFpMHnf9eTAGqkEqlkIbR+UIG18RyX70G3B/+Cq8CqN4NzH3m5XpX1MoTyAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADhSURBVFhH7ZY9DsMgDIWhyumy9C7sVYK6c5cuvV6LU4gYSvAfYuGTIkik4Bfbz4o1TNbHa4/L9rtj429pw0EaHNgkGfikrX8/75ANNGX2xAJicOucI5UjhGDz+5ISlLDLoZWBYw9fBmuLHiU4BeTnWLRKwGYKYAuA2sOVbtlUD6B6G9BuQra3KVxlAO3tITaEQNRg/5g2HC5gSasamNkQe+e0+JI7WAPsWSGKjCIOAVol8Gklw54DhQX7/JIRBIiQlICd9oL6GZABzQatMQfRFDBcQHMO9CLPl6sMaPi8gTFfHD5tlSodNYEAAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon16Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon24Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon32Base64);
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

            Page pag = new Page(32, 32);
            pag.Graphics.DrawRasterImage(0, 0, 32, 32, icon);

            return pag;
        }

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return new List<bool>() { selection != null };
        }

        public static void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData stateData)
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
			
			if (InstanceStateData.IsUIAvailable)
			{
				window.PushUndoFrame(UndoFrameLevel.PlotActionModule, window.PlottingActions.Count);
			}

            PlottingModule module = Modules.GetModule(Modules.PlottingModules, "64769664-d163-4fce-b7ba-18fd9445fcfb");
            Action<Dictionary<string, object>> changeParameter = stateData.AddPlottingModule(module);
            changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

            if (InstanceStateData.IsUIAvailable)
            {
                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.AddPlottingModuleAccessoriesAndUpdate();
                });
            }
        }
    }
}
