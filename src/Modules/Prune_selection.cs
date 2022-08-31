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
        public static Version Version = new Version("1.0.4");
        public const string Id = "f8abf1cd-d79f-403b-b0f2-4bb1412839ff";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;
        public static bool IsAvailableInCommandLine { get; } = true;

        public static string ButtonText { get; } = "Prune node";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Simplify";
        public static double GroupIndex { get; } = 3;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEtSURBVDhPrVGhUsRADM2WBQECc2j4jSro4DiB5jeKQgC3g0C134BDwwzYO1R/AVkU5gwCBDBT8tKks73juBngzWyzyUtesin9K/I839PrHBZxTq0ljPlkZVlOJKiY5Q5ObxohGB6fphEfRSJYFAU512pHxUCtlu4vDiXBQ214divBGBy3JBs9sHD9uLHbdQe8KekUPRy/Svfz1qMrncrFT2jnjDAzsqG3lx8FABYZsbHOgYvhf4tE7a+xorZD1D3wecA9TVNXVVXv1xrkCUeXd6OXt08ZefBe0+DjiXhs4Viwma5u03RtBy5trvtwfTLsnpRYMRMZDhKft/bRXYA7YsYjFzVKU2LFrDrBsSTlaRn/5yV6VgysOOaxMgRwR0xYxjJ+bokg4yUBi3miL6eLpTCC5hLNAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGXSURBVEhLtZSxTsQwDIZbrsCKxAPAgHiHbtcVkHgFFnYoiA2Vig3dFR6BV0DiWI+tr8EM4gUQ6LDT31GaJrlWHJ+Uc+y6ju06F/03MeRKOLh+WmCrWYPU5Hl+g+1S+vh2DiAKevEZey/wKRqtzcvtcSyrc0BVVTGtI6hOkPlho4VJXH3zwRlR8DFtJfMZ/4RiuFrkxAh+2VgUE0gvg6YIfZfWlNTK1keWSjgZZSASyA5WsAxS9Jkd3EeoRWb5cywh2BquRJb3AMrwlYRkbpLhWS+WfgOrVdya4AjbjCCdUHDu82mjKfbTNI3ruv57BRhJs+8mvdvkPMAOTsGUH9nNC9XrkM5H5uAf6ztm5iUko/fsg0SC6ApopLjf+s9r++stGi2+zx/vLh5gUpxcTc9+4uT+c2MXFkVJl8t5L1QFRvAMtzDjAO+be1v83IRtCK59aRWI0UFaJMFVTyHVi6xbDPFV2Y/5xkFtwXZ+DnWQrxD6q1gdrtNZZztUzRBfRirg8ZvLi5A8quaICkN8/WNKeEevv28U/QJdKMMEMy5iTgAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAH6SURBVFhH5ZY7TgMxEIadBzQpoyA6qKihIxVJC0jcJT0KES2KuAFnQCK0hIZcIRJN0tAgCqADKTC/M2t5d/3cTRr4JOPXjv/xeOwg/j0VrtfK8fntDzdzVLn+2yACtigYI9Dr9awh8xFrm3OAFrjT6xiK2K4sCVn0ZNkTg+FweMFtlYT3l2c5vborQ0PAohlxMMafkLVL3QKLeJd2Pw7dWKkjMIinQp/gPAKujbgEaA61PjcyifvwRsDkBNd9rgHET7mdwxUBbw7wwqNlTwLhYHEfwTlgiAQIEnclZMwtuOJaxzQWRUwEkGB66IEx62Ooce3EIg467Xa7MplM5MNTBK8DBnEk5DOVPdkr6YTTAYs4zv2JSotKaSesDpB4h6qbZU8ixenMxyQ0I8EX6s+p4DsAJx4xx/0gjA5A/LPWvP6ob203Fu8YUuLoAHYCTenE68aOWFTqB53D/WmME6lbQPc1l2zNr5lofc/lDwwPpYCzJP7wtrnLI4oBvXzeG6LeAU0cT22XywALTxtHSZhzYI7FU3ZU+rymExkBXTzr9TrmdJIIWD/kMbkjOZCmqJ2iSp4m4XVdITmnfau3o+yylPqPaBVUKVTJDqxeEnJO+1ZvR9llSSJgzVoek2ctB9IUtVOodyBjoO/OmmigqF2C9yEivIsUtRNCiF+43xNi/2MuCQAAAABJRU5ErkJggg==";

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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData data)
        {
            return new List<bool>(){ selection != null && selection.Parent != null };
        }

        public static void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData data)
        {
            List<string> nodeNames = selection.GetNodeNames();

            if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
            {
				if (InstanceStateData.IsUIAvailable)
				{
					MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
					box.ShowDialog2(window);
				}
				else if (InstanceStateData.IsInteractive)
                {
                    Console.WriteLine();
                    Console.WriteLine("Attention! The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    Console.WriteLine();
                }
				
				return;
            }
			
			if (InstanceStateData.IsUIAvailable)
			{
				window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, window.FurtherTransformations.Count);
			}

            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "ffc97742-4cf5-44ef-81aa-d5b51708a003");
            Action<Dictionary<string, object>> changeParameter = data.AddFurtherTransformationModule(module);
            if (selection.Children.Count == 0)
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Position:", 0.0 } });

				if (InstanceStateData.IsUIAvailable)
				{
					_ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
						{
							await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
							window.SetSelection(null);
						});
				}
            }
            else
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

				if (InstanceStateData.IsUIAvailable)
				{
					_ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
						{
							await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
							TreeNode selectedNode = window.TransformedTree.GetNodeFromId(selection.Id);
							window.SetSelection(selectedNode);
						});
				}
            }
        }
    }
}