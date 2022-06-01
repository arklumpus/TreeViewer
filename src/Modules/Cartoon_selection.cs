using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using PhyloTree.Extensions;
using TreeViewer;
using VectSharp;

namespace CartoonSelection
{
    /// <summary>
    /// This module is used to apply the _Cartoon node_ Further transformation to the selected node. If this transformation has already been
    /// applied to the selected node, it is removed. This action is only available if the selected node is not a terminal node.
    /// 
    /// **Note**: using this module without installing also the _Cartoon node_ module (id `0c3400fd-8872-4395-83bc-a5dc5f4967fe`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Cartoon selection";
        public const string HelpText = "Marks the selected node to be displayed as a \"cartoon\".";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.2");
        public const string Id = "6c340923-e3d1-4646-a673-6b542a05275b";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Toggle cartoon";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Simplify";
        public static double GroupIndex { get; } = 0;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFVSURBVFhHvZTNccMgEEZFrikqPsdFuAfp7B8VoCJcglNNjulE2Q/p86wYg5BZ9GZ2AFv2vl1ArlG0bXuT4Tqt9uFjHsmuyUHYgRHjMAyLz2viOzCOo48cnHOm4QW4eIV+OPbMVlgwIjwDVXnV6dUzYFk1OF5+9E3rq3aAFSOQWAIW+qZdqwgwKYgkvs9j49h2TckW6MQyhO8VJP6bptN3Jh1gxYhExb0Ekz9xNAZd1/lFbgf0bzMqDpk6wPtd0uotFYcktyAlVZqYJAV0iwHW3+fHTaI4MVk9hEjKxBYVh6wKWLU6xlMgbLckPSC5TKskJotTpl9Kv59f88yDxGZJZ3xh4Ragul2J3jM5dAcZEOEWWHXC/+/q2wenXwYtASxE8gRIBZFtAsRQ5D0BYiBSJkAKRGwEyBsitgJkg0gdAZIhUleAJEROmFQXIBGR/QTIUqTp/wGoudwIQ454rgAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAIcSURBVGhD7ZkxS8NAFMfvElpcnQTBRdwEJ7+DUh1ExUFw9QO0g4vVfoL2A7i5COJQBXfdRXQT3RURcVfa53vxXpompKTN5e4C/iBc7jWF/y/3mgtUihgAIBqNRgtPl/FYC4oO46kxTinCE2kCpQhPpLUQqKlot9uJa1wibQVKw7+Abf4FbFO6p9Bq63LJ74kmBlzH9J3SrAAFrzW7F15PPGD4bSxNCbzXzgtEg2PDbGEp2hFVZwVGBA/bm3BOYKV5tVg77J5i8PtkcHiWon+i5gHOCHBwX8CjkHIPS/7fJ5HgAGf4jHlT9QBZr9eHliRO0U8hCu5D/wBD7+KUQxMU/EUKuImHFtI7Vmf2VmDSOx7HuICu4EyiPYrayCZqlTRMtpDuOx6nMIGigzPaBUwFZ7QJmA7O5BawFZwZEqAnUFZsB2fGXgFXgjOZBVwLzgxtUtxC0Y2M+KjOi8/KHNWi1wN+4QmkuJUA76pmhgwb2bUaA74qszSE4emW487ZxeHcePgYCQF8O13A4Q6PUGL655WGcFXQRIKQGzjsgJQzqmyFzO859BvQ9i6Tl0gLZRZgnBDJI8BYFdEhwFgR0SnAGBUpQoAxIlKkAFOoiAkBphARkwKMVhEbAowWEZsCTC4RFwSYiURcEmDGEnFRgMkkIrx9VXNPgKH/B7weHGHETZxGc6LIYO6sADNCJMB5ASZF5Ls0AgyJDP6lFJ1frhD0CoqV+VMAAAAASUVORK5CYII=";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYYAAB2GAV2iE4EAAAKISURBVHhe7ZtPSxtBGIfnTSnRWjzYS7Xn0i/guehR/SBChYo52EtKA730lNKe/Ay9KehJKBQLLfUb9FA8KH4AQdP8eTu7mV+WxM12SXZ235nNA8vMzubye+bdzc6QkBpDrVZr6GZVH1vhgKdUTBuH9+EDkgR4Hz4gSUApSHoGsOmGNJvNsZ91mdJXwEyAaUvLTIBpS8tMgGlLS6neAzYaJ4vUbb/W3Vc6HBPTdikqIAi++fawTt3OH336Xh/LejZXFPGB1wLWGl8eb9WP3kTBeal/ZcAzLwVgxh915y50nX+ICT7AKwEpZvweXghIM+PE6laPfzWnA5wWkG7G+S8r/q5U97Ni/mYGBzgpAMGT7/F+cOLeR2I+ZUV35sIQNPp9Pw4J7wFB8IXO3A6T2o8PHRAEV78q3DuLDU2Vd6YX4kQFZDnjo4gWYDM4ECkgj+BA1Fogk3v8f0h8BuQ546MUKqDI4KAQARKCg1wFSAoOchEgMTiwKkBycGBFgAvBQaYCXAoOMhHgYnAwlQCXg4OJBPgQHKReC4CbB0/UVfWF6tFDMzJMsPWkpfzQ3Z+KudUfFcS0a4Hr6vPY8Nhz0+E/hVtPEsPHkCTg2LRekyTgXB/3JDxt/VYVbpuzCL2Endd1sE5Mu4ropT6q5pJoJlrjBw9B6+t2W4w8A6ba5HBSRJYCgFMibAgAToiwKQCIFpGHACBSRJ4CgCgRRQgAIkQUKQAUKkKCAFCICEkCQK4iJAoAuYiQLABYFeGCAGBFhEsCQKYiXBQAMhHhsgAwlQgfBICJRPgkAES/Aqe9cSKi3WpaN0MhXggA6SpiGK8EgDQVAbwUANJUhNcCQEJFXJZCAIhE4C8ztP0Pe+oELTPnsZ8AAAAASUVORK5CYII=";

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

            if (!selection.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe"))
            {
				if (InstanceStateData.IsUIAvailable)
                {
					window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, window.FurtherTransformations.Count);
				}
				
                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "0c3400fd-8872-4395-83bc-a5dc5f4967fe");
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });
				
				if (InstanceStateData.IsUIAvailable)
                {
					_ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => 
					{
						await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
						window.SetSelection(window.TransformedTree.GetLastCommonAncestor(nodeNames));
					});
                }
            }
            else
            {
                List<FurtherTransformationModule> furtherTransformationModules = stateData.FurtherTransformationModules();

                int minIndex = furtherTransformationModules.Count - 1;

                for (int i = 0; i < furtherTransformationModules.Count; i++)
                {
                    if (furtherTransformationModules[i].Id == "0c3400fd-8872-4395-83bc-a5dc5f4967fe")
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
				
				for (int i = 0; i < furtherTransformationModules.Count; i++)
                {
                    if (furtherTransformationModules[i].Id == "0c3400fd-8872-4395-83bc-a5dc5f4967fe")
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