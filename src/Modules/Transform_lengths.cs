using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace TransformLengths
{
    /// <summary>
    /// This module is used to transform the branch lengths of the tree, either by making them all equal, or by turning the tree into a cladogram.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Transform lengths";
        public const string HelpText = "Transforms all the branch lengths in the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f9241a42-c0cb-41a5-a1ee-a68fc339dee8";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;
        public static bool Repeatable { get; } = false;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACHSURBVDhPY6AUMEJpMCgqKvoPZeIEfX19KHpQADEGoAMWUjR51W78v63Zn3wXgAyAMuGABUrDAbIheP0LBQQVgADUZkcg3g+jYV5hAhFEAJhmEIAZAgZEuQAEgK5wAFJgzUDbD4AFqQFQXEBOOkAB5BjAiK6JmKjDCchyAZQGA4rDgHTAwAAAcB8v28nDln4AAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADKSURBVEhL1ZTBDoMgDIZxTzavu+xduC9qdudddtl1vtnWanWIRSkgiV9CWkJo6V+tOpqK7AKtdQumGXdhGGPYWBeyLqLgYqCCLy7aJlFtBfKVbXN7vIb77+ddJNGxSCTCCqYqOHxfEXuBkyxWoo5sMrtN5LAkqWF9Rvfv29WkNnkKjtj+TGwCfK2PxVmURAjIdAXjvroGeXryB6ITIE6SVfAirCqAf0A8SbfgmnyeSYp4p2nIJA2h/DTNLhHZmZzBEU6ibJO0AEr9ADDQSFp89flJAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD3SURBVFhH5ZbNDsIgDMc3n8xdvfgu3I0a77yLF6/zzSbdSrIJLV91OP0lpCsj6Z9SSJvatGi9KKUuxpwnLw+tNRtjh5aiKHgxJgMDDHQ/QigD2RxO9wEGuiRtzA5D5+jDBn/cjkU1UJeSGog9guoZCL0D7A642pCqgSva7bFqDdhgZuxxygH+2XU4NSJdhL1PBM71k7dESkCHFliI8ASfr+VvQQrcLmd05lY88XtETAAQEOEEB0QFAIQIb3BAXADwJoIM/hWQGTDPcHE7FgN3DX+/HQOkX8Jkgi1ZTjuWQvUMkPxNDdQvQrQOa6Qf4DKw3X4wnqZ5AUcPa+0GK3ETAAAAAElFTkSuQmCC";

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

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                /// <param name="Transform:">
                /// This parameter determines what kind of transformation is performed. If the value is `All equal`, all the branch lengths are set to a
                /// value of `1`. If the value is `Cladogram`, the branch lengths are adjusted so that the tips are all contemporaneous and the tree
                /// looks like a cladogram (the length of the shortest branch in this case will be `1`).
                /// </param>
                ( "Transform:", "ComboBox:0[\"All equal\",\"Cladogram\"]" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            return (int)currentParameterValues["Transform:"] != (int)previousParameterValues["Transform:"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            List<TreeNode> nodes = tree.GetChildrenRecursive();

            switch ((int)parameterValues["Transform:"])
            {
                case 0:
                    foreach (TreeNode node in nodes)
                    {
                        node.Length = 1;
                    }
                    break;
                case 1:
                    foreach (TreeNode node in nodes)
                    {
                        node.Length = 1;
                    }
                    double longestPath = tree.LongestDownstreamLength();

                    for (int i = 1; i < nodes.Count; i++)
                    {
                        double upstreamLength = nodes[i].UpstreamLength();
                        double downstreamLength = nodes[i].LongestDownstreamLength();

                        if (Math.Abs((upstreamLength + downstreamLength - longestPath) / longestPath) > 0.001)
                        {
                            double factor = (longestPath - upstreamLength + nodes[i].Length) / (downstreamLength + nodes[i].Length);
                            foreach (TreeNode node in nodes[i].GetChildrenRecursiveLazy())
                            {
                                node.Length *= factor;
                            }
                        }
                    }

                    break;
            }
        }

    }
}
