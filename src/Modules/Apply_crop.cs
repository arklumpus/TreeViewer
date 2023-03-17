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

using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;
using PhyloTree;

namespace a6856e0463553465a8b35c351428120db
{
    /// <summary>
    /// This module is used to create a new crop region based on the area that is currently visible on the plot or on the
    /// currently selected node, by enabling the _Crop region_ Plot action module.
    /// 
    /// When the `Crop selection and current view` item is selected, the crop region contains the area that is currently
    /// visible in the plot and it is anchored on the currently selected node. When `Crop selection` is selected, the crop
    /// region contains the selected node and all of its descendants. When `Crop current view` is selected, the crop region
    /// contains the area that is currently visible in the plot and it is anchored on the root node of the tree.
    /// 
    /// If the button is clicked directly without choosing an item from the submenu, the action performed is either 
    /// `Crop selection and current view` (if a node is currently selected) or `Crop current view` (if no node is selected).
    /// 
    /// **Note**: using this module without installing also the _Crop region_ module (id `5a8eb0c8-7139-4583-9e9e-375749a98973`)
    /// may lead to program crashes or unexpected results.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply crop";
        public const string HelpText = "Adds a crop region including the current view or the selected node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public const string Id = "6856e046-3553-465a-8b35-c351428120db";

        public static string ItemText { get; } = "Apply crop";

        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "Plot region";

        public static double GroupIndex { get; } = 7.5;

        public static bool IsLargeButton { get; } = true;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>()
        {
            ("", null),
            ("Crop selection and current view", GetIcon16),
            ("Crop selection", GetIcon16),
            ("Crop current view", GetIcon16)
        };

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None), (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None), (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None), (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };
        public static bool TriggerInTextBox { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABVSURBVDhPY0AGXrUb/0OZOAG6GiYoTTag2ABGYpxNNKBaGBQVFf0HYSgXg48McIZBX18fI5SJwsYLBk80YvMvyWGAHohQJn5AThgM0oRECqDQAAYGAAuBM6ehKVKdAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADGSURBVEhL3ZRBEoMgDEWx09P1OOw71gtwl56wJfqjQUig1S7om2GACPkxibo9t/vzheXHlO5eMP+M/gWGIzn/irOLfMWc4b1PDocQBixnas+ZlhpMpcuwTctOpyZADsYY7WPZbsA2xmGKmAIxSnKSiUjnOKOiCnBapAjtQeKczzbxn78Kynkca3S0ljWxUAXYIRztuyUpPJ8tYb6BdC67RRaeRTRqKVJbUYrMBoWWGlCUWQpgM50XObtN+/9dN30HR+hdwLk3/FpsEYRclm8AAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEWSURBVFhH7ZbBDsIgDIY349N58V12N3Pxzrt48fWUskJQWtrOyQ7uSwiMQfvTlmXdJ6fL/QkNH1eDs3vAfjN2Af0v8m1h8wgUtL4FR+xJhmEoNjjnehySWPdYUzBhX0OzJmERMPmTXHHMgmvUIrQCwODowysKwDWjbyoRGgHx5KKI3Lk2EkVxxEp93M7FO8JBovYO4OyaipCLhOS8hvUWvIkIEzOLnJNwH4xv4eyaI7A2iwRAzn1Lp4FxXhMWzALyggsTM+IV5TAJyJ3nBcfdDg1VAd5YCi3nPEKJgB5swJhDE4FoULxqhAjYU0WbAtF5JBcRJgQsNaDKr/bkEVMRejSG1c5JWn8J97/igtYp2DwC/y6g617vZ7kcvSir0wAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAAJbSURBVGhD7ZlNTsJAFMdpuQAnIMYLuPAAhLDTRK/g0iUfC9yoqBtd8HEFly410R0hHMCFFyCEE3ABwP+btqS0M3a+0pHQXzKZN9MOfX/63nyAt9lsSjzO7z62Fz4fL7zQzBUZH/yw3lsKAa4pBLjGO7t9509De0IRQq45jIXM87zciyxSb+Dr6ZJ9Yrvd5t8c0Ov3+w+h/SedTqeL6jlopRkMBsyvPLcS0s4TuPcF1U3QMsOGACXnI2yJMBWwXK/Xr6GtDMa+oVoGLT1MBNCDK77vT1qt1lHQJQ+NobEwKyjaInQF9Far1QnqGQo5MkWCH7MrEjSbzSrGjGGS8AUS9hS1VjjpCGAxPxqNFhBRR5tEVDH1TWREkPPlcpm+ebqXnK9j1pnp5oSqgJ2EVRUhcp5dBDoilATEnY8gEUjGBsw5CokY83KC+uD8FCY5P8eYWtz5iFCENCZJvGU4HM7xJmowuTnBifkGjWEXDbEigEiEk4hU2JhiTQARhVMyPET9NlDaC4nutQ3yiD0rz72QM6wLoNlGNAvx+k2xKiCabUSzULLfBtYEJBYpEdIrtixWBGQtUrKLnQ5KAnCSSp2iZBeprMUuAs/ohaYUqm+gGxeRCJvMRSqx2KXCKXT+PmjJoRNCTISq8xEiETrOE7o50IXzP6i5MZ8FJye+USs7T5gkMTtJwZG6zsaMxtBYmOxkxzo1MBFA0JHyOrSVwdgrVNrOE6YCiJ3ElkU35pPYEEAoibDlPKG0G82b+E//RrtREumiyFD8weGavRdwGH9w/GcKAa4pBLilVPoFnnrnH7c79r0AAAAASUVORK5CYII=";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAAfPAAAHzwGGUVlPAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAidJREFUeJztms1OwzAMgF3EZa8GEi/jQ7nAMk7s4IcBiWeD2ziwSemU5sexY6D+Toh1if0taeOk0+l0ghz3z++LCz5eHqbsF4xpjfdGN5zfjwuwDsAaF2AdgDUuwDoAa6a7p7f8QuCfs/kR4AKsA7Bm8lpg49xyv4iILU+PQESHxvZnAHitvZ6IWCNzxAhoTh4AgIiOAPCoEM8CbQGs5C+MkKApoCv5C9oStASIJH9BU4KWgN0faVNNwIyI1XfwEogYAGAv1V6MpIAAy2EqIiGR/HU/XUgJCER0SMzVLgmp5Ff6YSMhYHHDk5KwlnymHxa9ApJ3+14JpeQz/TTTI+CQe9Sdg4s/n8+JZUkk39pPE2wBRBQqr4l/oX1uJKz88rX9sFCvBWqnQ+2wl2ZIOZyQ8Jm4LP7fkOQBBu4HRHM1OadLn2vB3g/gUJqrPXOZy+Z3hFyAdQDWDBWAiKG0DpCsImsYJiB6zpfWAaKldIkhAhKLnNI6YJgEdQEdhc0QCWwBNcG1Lm+5EmqKrDV6RkA2OO7avlVC73ZZ7xRQKWw6CqhmJO4Bi+CkqrqSBKmNUqlaYEbEr/Pf1ZsZJYjoiIi7qM21fthIPgX2wNjMKJHaVAHBLXI/GdJoFPxkaJsnQ9donQyJor0Ulj4ZEmdEMSR1MqQCex3AfSenof0APwehqmx+R8jfFbYOwBoXYB2ANf6usG44vx8XYB2ANS7AOgBrXIB1ANZ8A4hmSClgoScWAAAAAElFTkSuQmCC";

        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {
                bytes = Convert.FromBase64String(Icon32Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon48Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon64Base64);
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

        public static Page GetIcon16(double scaling)
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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<Avalonia.AvaloniaProperty> PropertiesAffectingEnabled { get; } = new List<Avalonia.AvaloniaProperty>() { MainWindow.IsSelectionAvailableProperty, MainWindow.IsTreeOpenedProperty };

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { window.IsTreeOpened, window.IsSelectionAvailable, window.IsSelectionAvailable, window.IsTreeOpened };
        }

        public static async Task PerformAction(int actionIndex, MainWindow window)
        {
            if (actionIndex == -1)
            {
                if (window.IsSelectionAvailable)
                {
                    actionIndex = 0;
                }
                else
                {
                    actionIndex = 2;
                }
            }

            if (actionIndex == 0 && window.IsSelectionAvailable)
            {
                TreeNode selection = window.SelectedNode;

                List<string> nodeNames = selection.GetNodeNames();

                if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
                {
                    MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    await box.ShowDialog2(window);
                    return;
                }

                Point anchor = window.Coordinates[selection.Id];

                Rectangle region = window.GetVisibleRegion();

                double left = region.Location.X - anchor.X;
                double right = region.Location.X + region.Size.Width - anchor.X;
                double top = region.Location.Y - anchor.Y;
                double bottom = region.Location.Y + region.Size.Height - anchor.Y;

                double guideThickness = Math.Sqrt((right - left) * (bottom - top)) * 0.01;

                window.PushUndoFrame(UndoFrameLevel.PlotActionModule, window.PlottingActions.Count);

                PlottingModule module = Modules.GetModule(Modules.PlottingModules, "5a8eb0c8-7139-4583-9e9e-375749a98973");
                Action<Dictionary<string, object>> changeParameter = window.StateData.AddPlottingModule(module);
                changeParameter(new Dictionary<string, object>() { { "Anchor:", nodeNames.ToArray() }, { "Top left:", new Point(left, top) }, { "Bottom right:", new Point(right, bottom) }, { "Guide thickness:", guideThickness } });

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.AddPlottingModuleAccessoriesAndUpdate();
                });
            }
            else if (actionIndex == 1 && window.IsSelectionAvailable)
            {
                TreeNode selection = window.SelectedNode;

                List<string> nodeNames = selection.GetNodeNames();

                if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
                {
                    MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    await box.ShowDialog2(window);
                    return;
                }

                Point anchor = window.Coordinates[selection.Id];

                double minX = anchor.X;
                double maxX = minX;
                double minY = anchor.Y;
                double maxY = minY;

                foreach (TreeNode node in selection.GetChildrenRecursiveLazy())
                {
                    Point pt = window.Coordinates[node.Id];

                    minX = Math.Min(minX, pt.X);
                    maxX = Math.Max(maxX, pt.X);

                    minY = Math.Min(minY, pt.Y);
                    maxY = Math.Max(maxY, pt.Y);
                }

                double left = minX - anchor.X - 20;
                double right = maxX - anchor.X + 20;
                double top = minY - anchor.Y - 20;
                double bottom = maxY - anchor.Y + 20;

                double guideThickness = Math.Sqrt((right - left) * (bottom - top)) * 0.01;

                window.PushUndoFrame(UndoFrameLevel.PlotActionModule, window.PlottingActions.Count);

                PlottingModule module = Modules.GetModule(Modules.PlottingModules, "5a8eb0c8-7139-4583-9e9e-375749a98973");
                Action<Dictionary<string, object>> changeParameter = window.StateData.AddPlottingModule(module);
                changeParameter(new Dictionary<string, object>() { { "Anchor:", nodeNames.ToArray() }, { "Top left:", new Point(left, top) }, { "Bottom right:", new Point(right, bottom) }, { "Guide thickness:", guideThickness } });

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.AddPlottingModuleAccessoriesAndUpdate();
                });
            }
            else if (actionIndex == 2 && window.IsTreeOpened)
            {
                TreeNode selection = window.TransformedTree;

                Point anchor = window.Coordinates[selection.Id];

                Rectangle region = window.GetVisibleRegion();

                double left = region.Location.X - anchor.X;
                double right = region.Location.X + region.Size.Width - anchor.X;
                double top = region.Location.Y - anchor.Y;
                double bottom = region.Location.Y + region.Size.Height - anchor.Y;

                double guideThickness = Math.Sqrt((right - left) * (bottom - top)) * 0.01;

                window.PushUndoFrame(UndoFrameLevel.PlotActionModule, window.PlottingActions.Count);

                PlottingModule module = Modules.GetModule(Modules.PlottingModules, "5a8eb0c8-7139-4583-9e9e-375749a98973");
                Action<Dictionary<string, object>> changeParameter = window.StateData.AddPlottingModule(module);
                changeParameter(new Dictionary<string, object>() { { "Top left:", new Point(left, top) }, { "Bottom right:", new Point(right, bottom) }, { "Guide thickness:", guideThickness } });

                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    window.AddPlottingModuleAccessoriesAndUpdate();
                });
            }
        }
    }
}
