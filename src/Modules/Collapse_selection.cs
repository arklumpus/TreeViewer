using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using PhyloTree.Extensions;
using TreeViewer;
using VectSharp;

namespace CollapseSelection
{
    /// <summary>
    /// This module is used to apply the _Collapse node_ Further transformation to the selected node. If this transformation has already been
    /// applied to the selected node, it is removed. This action is only available if the selected node is not a terminal node.
    /// 
    /// **Note**: using this module without installing also the _Collapse node_ module (id `3812314b-e821-4399-abfd-2a929a7a7d80`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Collapse selection";
        public const string HelpText = "Collapses the selected node.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.2");
        public const string Id = "e149aeb6-a019-41e2-8830-e4dc3e0eee43";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Toggle collapse";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Simplify";
        public static double GroupIndex { get; } = 1;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAGQSURBVFhH7ZQ9SwNBEIZn9q4QLKws/ChEu1T2UTwhKCqkFKzsLYOlYmws8wP8DYKI/pwQzoAgWAiiYJMd390787G54s7cJYX3wLK3M3uZd9/ZC5XMGo7nKSC8f/lY8UQHzBwI0Y6QtAsUMF4QwcUoNyBHAWkK8oeIhIopRL5uIzb+R46aT+vU0zURvUXMuwitRpkIWPyJqcvCHWLpkshblAGsruxkFymZqKBLGgG5FnRJEtBoNJqYbCKc26Rvb8E8DjHUQy0hir7HiezEApRdDLBBg8L5xhGPmXwt4mODP/56dhJ/odVq8d3NKfseraDgMaTcIhxizGNUcMsPcYQz+HeO1pwwqSr2LTF6Yd7PgtsCe2wjwAYc6s2H5Z6mqhaqYcMeQmtRps8XXHqBji6x7uD5VYSTrOy3IJMAl4kE5SHA5eDifkMpL8BFDbA0Y+SrARBEz+YiR23MWYBLCoeKFeAy6pBso0ibf4sOU5SAJCb/kPPEuJHkSJHM3IFSQCmgFJD4VzxNXAeu47nkv0D0A4bH0ZO2zTH8AAAAAElFTkSuQmCC";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAImSURBVGhD7ZZPSxtBGIffdxdj1JZS8GaPnqqXFktIQ6tC6EXshwkq3vTYUz5CT54LofTQQ2899huUSKwIevIWSGPn9TdxsmQnrtF2s7NL5wnDzO67gd8zfzYhj+c/h01fGLYPPy+qK/VWMS2LUCv3AsPAwrxOJJu4tYo2zN3JncD7vdbjPyWpqIDqLEEdoV/gdnBTHce5wIQZHgMFJSNCmQs8dIYRVvA5J5ZjFvlFwicU8L4pT18glcAsPVO+gYMDM0pfYCqBbdIUyCSwzb8IOAlsM0mg0WgcoltD2xrcABelZeqGT6gXPEKmZHEUlCI6C4RO8GBHEJpZ+qacDiMCSTMXC9+eq9DlzJIOry9j4XVgBD1loe+Y6SOl5AOL+iikvqHUTj28RZJAFF6DEGY0DioMiVlhmsFlCRe6z4zQ9DGq1areQhFP+2fYunBF0isu6xkd/Z5ekQW0ZxitQOc12nO0RZS0TBct5S3EG2YU3w5DcAaiKW82m7Fn8Ms53+/Ty5ClJiw4xPQGbXZQTELocnCIiY9xJjq4oaX+nnsc4kQBGydCaQrYZCI0TQGbqQhlKWCTihDzrqlkL2DzbufrQlj+XeNA6TfLOsK+Qn/Xq1hnizI5F7B56ArlTsBmwgr9zL2ATSTEalWF6hOPhr2NvAnYJP4NLgqFFyjcGbDxW8g1XsA1XsA1XsA1XsA1E/9K5J2kFfhi+tyTJPADrTASHo+nsBBdAzZFYueRbvPNAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAB2HAAAdhwGP5fFlAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAo5JREFUeJztmL1uE0EURs+d/BmJIkWK0FBYAaWgoKCLpbiJhASRaPIGSDRU9gv4CVIiJDpK2kDNawQhRf6RiJFSEYJCjPejSCKcSezYxtnZdeaUs7uz9zs7szuzEIlEIpHbi4UuIA3Ktc+zdzqHT8wlZbB1g5Lg2MSrqRTQJ/DdK05tT4WAra0PM79W5x5LroSxBmwAi8Ncm0sBIzzha8mFgEkEFvppoo7Zo9722YlWOiEuDenujw0ci+fPS8N0IjuSJQ2TNc2SJnL7kAAZFOAHPjqfw6OMTy+wcPsmAUKC0/CXCSIgZGCfVARkKbDPjQjIcmCfiQjIU2CfsQTkObBPXwHVavWFpHfAUm/74cwS7QWHcCOtIv59h2kg6pAcmM6OpBjYp68ASW/wwgN8X3hA1+au7dgPbOjg7ECmcAOO3buq0YZM4LB5OSsYFJAVhBt0r2CM/A5Y/v2F9vxDOq4w8DzBvIkVYSs4cHCSmGs6qaHE6jK+GUmYcd/D0AK2t7cvzPhntY/FpPunBLZm8BS4P+h6X4ihDuZaEi1L1MSpgeiOmWNsxv4Mfqo93wP2gPcwuhCwOUTRoIgzgA7OUhcysYVQXoXc2FI4L0JS2wxlVUiw7XBWhGTifwCEE5IZAT6+kM3azmq3m5QF6wZlYHlwDxeFGHaSOJrmreMyK8Bnp7a5C+wCb2H8dYjfnhsBPv8/ZU7JrQCfMadMa2oE+PhTxhNSMjjG9NoqlcpQ2zt/LzAtZHKLmiZRQOgCQtN3XvvvhvgOmFKigNAFhCYKCF1AaKKA0AWEJgoIXUBoooDQBYQmCghdQGiigNAFhGbo/wHTyq0fAYMENFKrIiB9BUh6KelrmsVEIpFIJJIufwG8Me76d6duKwAAAABJRU5ErkJggg==";

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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return new List<bool>() { selection != null && selection.Children.Count > 0 };
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

            if (!selection.Attributes.ContainsKey("3812314b-e821-4399-abfd-2a929a7a7d80"))
            {
				if (InstanceStateData.IsUIAvailable)
                {
					window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, window.FurtherTransformations.Count);
				}
				
                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "3812314b-e821-4399-abfd-2a929a7a7d80");
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });

                if (InstanceStateData.IsUIAvailable)
                {
					_ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => 
					{
						await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
						window.SetSelection(window.TransformedTree.GetLastCommonAncestor(new string[] { nodeNames[0], nodeNames[^1] }));
					});
                }
            }
            else
            {
                List<FurtherTransformationModule> furtherTransformations = stateData.FurtherTransformationModules();

                int minIndex = furtherTransformations.Count - 1;

                for (int i = 0; i < furtherTransformations.Count; i++)
                {
                    if (furtherTransformations[i].Id == "3812314b-e821-4399-abfd-2a929a7a7d80")
                    {
                        string[] node = (string[])stateData.GetFurtherTransformationModulesParamters(i)["Node:"];

                        if (node.ContainsAll(nodeNames))
                        {
                            minIndex = Math.Min(minIndex, i);
                        }
                    }
                }
				
				if (InstanceStateData.IsUIAvailable)
                {
					window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, minIndex);
				}
				
				for (int i = 0; i < furtherTransformations.Count; i++)
                {
                    if (furtherTransformations[i].Id == "3812314b-e821-4399-abfd-2a929a7a7d80")
                    {
                        string[] node = (string[])stateData.GetFurtherTransformationModulesParamters(i)["Node:"];

                        if (node.ContainsAll(nodeNames))
                        {
                            stateData.RemoveFurtherTransformationModule(i);
                        }
                    }
                }

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => 
					{
						await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
						window.SetSelection(window.TransformedTree.GetLastCommonAncestor(selection.GetNodeNames()));
					});
                }
            }
        }
    }
}