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
        public static Version Version = new Version("1.0.0");
        public const string Id = "6c340923-e3d1-4646-a673-6b542a05275b";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Toggle\ncartoon";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        private const string IconBase64 = "iVBORw0KGgoAAAANSUhEUgAAAEIAAAAqCAYAAAAH843fAAAACXBIWXMAAAr/AAAK/wE0YpqCAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAADAZJREFUaIHdWmlwVFUW/u57rzvdne4snY0QliSyBsIiYiAiEVFGNh3HAomyCnF0KGtGLZcqHXVGRaVYVIoqFZARUFDHZURlZFhEAgjIJgQIAUKICensa6e73733zI/XySCkm5DQpc5XdSvp9+67Z+n7vnPuOQ38BkBENwpJFyorK/uFSoYSqoWvFTjn4znhO8aQ8Omnnw5CiHTWLr0w7MG3Tc0sfDRJwUIhsAUK1Lq8FdP3B5vDOZ8BRX13fQm0e7pI7vF4TABCotdli6bNW3szGH1JPm9eKAS2QAqfK3/d/JkAGtq6rwvxFEF56a0i0vIagIX9IawKBBgTgdbkhAYiOmUivqakpGRtSkqKp736XOaI9Jw1WZx7l5xYnTMBQHN7F+oACEATAPmzi0TMK+SbAsof3yyEqbCJQACiNUK4ZjxFRJAEEAhEAIggAVgUQnK4glvjmC/azGqkxz3V4XB81x5lLns1LkITgMaO2dhxcCmfUZjyp5cLSCnzACQNR1R4COUMIClBAIQE4HeElARpeAQ/1gh8UUzmiUla/P09wrdWVVVlx8TEfALD8QHxqyPL/BMnNnp1vWFCPARIQhJBSgKBIEkaRkvjfyEJQhLIf73lMxeEz4t8bMlJj2aPjPqgoKBg6JXkaukPftBPSvEiiBgACKK40JsbGAMHDjz66KOPTn727y9+9EiKNXb5Wal5BOGBZIXHmUn41QTAAAYiSYxBIyLggpdMX5cK9UyDgCDCTpdA3whFzYxNeA/AUAA8kFxN9XnLGmuLdkjuNQOAyea8zmSLzAz0QPLs1ZZwk1YJILwzBpMUp46vmj0IgPeSW3Lp0qV7Kioq7lj0+rKPn+jtSFl8SmrDohiOHz78ucvlcgVas0fPlORXh/SfuPQEqd+5dBAR1p3xKBNvdqRt2rRpxPjx43fhCq9IK9Jz1mT1n7PiAAB7W/d7PbIuYkDOWjeA3gCSOjksQVRRR40alXqupHT/hSaue7kUr7322hwAXQDEBxjdNmzY8IRbF/zeHbV0x+ZqumNzNe0rb/YdPHhwEQA1kLDOcEQdgJJOjmDhTeTm5p7Lyhx5Z23pua1mlSkWi8ULoBJARYBRMm3atDcbmzylg6JUI7pIieImaTKFWVIRJAf51ZHlJZBFRUVl/Xv3um/Dhg3PLV++vADG1g42fGBwOTQGIoNIvRJgqmJDEEdcFj6lwmoVzTJkQM7aNhMdeAhE1O5E5RqAANRkZ2cvhmGIbGuSV4hnfYJNJEgWprD+RG5IkiAigIDknskj6rx6LmOMLApWmjVt5cXPX+aIvLdnHAEQC8DWlkBHn5GO7qMfPhRM84Hz1iwiouuDW0dFx1fNfgDtIy8C4IbhiLbms5qqqrqEuLgRG8/rONPAsdvl8e8IwqZiN0rd5sihMaYbb0m00EcffvgBABMAvWWBQAlVjX9choTfzY9gHgqqvCS6x1t9/mPBPaWB5nB3XSUAB4D6YGtdgkByqUt8/Ds/HDp0fVb/9Bn/LGxUqzwCIECC8FMjh1UhzE9ziC82blyWnZ39LS7ZWcEyy8DaELHYwROiEjPv19u6L3SueFxn/12ye/W2jqzfQfhuGDr0kWMnC6IXDk+e+PDuaq2m2TiWdAlXsTAjWny/K3fd3Xfd9T6AkwB+dmbpAFlWexmoPmH4tH1SF4VtDcaUbtfAsKsFAWiaMG7snObq8v2vDY/iFhWIMAGLMqJ50ZnTW8ZkjX4HwDFc9Ep0FuEw4nmbI23uP4qSMufceq2EXSXYpEmTkkorqk7sd7l5Qa1Xzz97bo/FYsny691m5AjJ2T5t7nuFvLn2U5PNmR9YXVTlrZjxSSjkA1BycnJ6LVi0ZHuz21N3w+ABfy4vL/8exiGyTZ4JyhEej6evxWIpQICQFQjS17SaAYN4c02PgHO4XgVgC4zE7FpDrlix4kx+fv4teXl53auqqg4hiBOCQhfiCSKi4ydPPosgqWkAMABhVxhmhGhHXqKH1mE5nPPpPkH6R6VEPiH5nj17HsbVO+O3DV3Xx+qC9GWFRA/9SLS8kMjHBd+xY8d9+D92xs+2jM/nG85ULXd9CTPvrjGSESJgdAyjqV2kvm3L5gnjx4/fjqvkjI5gQM7alQS6YkGlM2AEzqU+P//duT+0kqXH4+kF1bRlo4tMu2r8rCIBSYTt5cQcKpnGjhn7+fr160dnZ2cfRkeI52pA8lZ3WcEavbn6QqhEMIIsP/J5I+CPGkTk9ErKbRYUYVMZxsQQtlShNVcfl8CggTFSNPv4yb//5sknn7x+4cKFJQixM/S6CztLdq7aGkoZLdAAoKysTFUUZXNNTU1Mf6vVkZDUPfM/5aRKf6F0apJKhWfP7iw9qzdyzvXc3NxUABdwSZr6W4YGAImJiRUA5gKw7N27d2BCUvedkozKcAsZvPLKK2+tXLlyEwzjdQRwAuc8u76+/pjT6TyGq9gxA+d9kMAktZb/JHiHzkEdxcXCdAA8JSWlwqQwhUi29gzqvIIPGjQoDkZSIhDEQLdgf7XaI1J27949JTMz82u0k1iJ9PVSQWrLZ8aUxA5Z1E70fWB9V03jEzRFbjry1qySS71OeXl5P43OyqK4MMZKm4zCxs5Kqd47fdZfysrKPlywYEF5kPUZkVSO1ZPlhhszPjtw4MCDw4YNW4N2vEJ5q2ZNhlEHAQCkzV29o2MmBsbAeWv6Q1HvVsymGZLzVIWpSqPr/GMAll22/caMGeOtbvLuHRGljvikiZggwofFXEntZ+v+1DPPH3/y6affDbdaqwMJ40x1bi5pxKZiqT09eOiKY8ePxw9MS1uMIKV0P5r8AwDAmNL5EP3CC0r6T72HMpPyBzA2i4BYa4xDsTptpvCkBFQcPttcX+y2Am2fNUjlnr9N6GL/aqsLaoVHwiOB5496tJtiNeegqLDHLM3SeDX87TcwoKVU4xU6O1XnQ3EjR61XqC/d0PflU6fPdu3TK/Xxdjij00ievdpiNSmjTGbLvVQupjKrarbHRZissXbV2jUBZmskFFuE0RlTilqfa5OQIiMjN7uqazc+0dc+8fmjwtQojU7St2U+bCslRZK/8yQlpJ9HjD/ScA6MbtTBCi8e2VWtLhrRY/6ZouKu1/Xsfh+uYS2gBekPvx8thLjNHGadznV9nNlmJlucw2KPj2LW2FhojigolnAIkhAS4CQgpARdRHWBmJm++fKL6eMm37l9wWD7kKUnPaaCOtnaY5RSGgzoL4ySv+/YkncYXjHItrjRh9P1HKnR0Tc5HI7rGhoaTqGdBBoW06NP+rz3a9tUEMwJJtI0i/l+ofMh9pgoHh4TbrUnOhEW7YTmiALTwlp11jkHEcCFbG0dEv0vsw4YombOnNk0ZcqUrBcXLFj2ypDrZh+t0eUPVdxU6RGQZBw5SLZU0P22k9F73FfuRTMnODSGVzOcIobc5yffMenRhoaGcrQzpEqh7wuLTprPyfdQW/eZqsQzUhKELhDXpxtz9ulpsjjjQIrm75YTuBAgInBhfClCSBBJcAlI+vl30Z7jqfbGG28Mzhg58sH4pJ4jbDZbDMBaS3xExKDA+I4VIMZui335UK12rNqHJSNjuKyvzLv9ltFPnT59ei+M2kN7c4swXBRF2vJFROrwxOh+Y24J79pnsqJaM8x2i4xM6REWkdKdaQ670RgmASEu2slERoedCNW79ntrftzz3LkvX17Y3nM6g3HytMEod5kDTSxvaN72VYkvdVIPG68oPb9n9IiM5yorKw+go4WR9sFmSeibEJs+7rbwbmkTTWGO25jZpNm7Jmi25CTVFO2EIOHvfBEkCJAStXsOeWuPGo5ob/ZGMBi/AQF+4eIHUzVVzO4bgb0HD/1rVMaNiznnR2D84CSU5xK3x5Vf+JMrfwWANbDZnEnXZ4/yVKRNMBcVT2ZMjTDHOZmlW1dNi4u+KMrRlTkiAK5ojEZi/5ZtO7fcPnbsKgB5CN7fDAW8cLsvlOSu+hjAZwAiu9w0I8OelDbRW+6azKAlqtER0ty1i5k4b3VEKEplKoxqdjlCECo7ARVAhDN9Ur/o1GGTLAmpdwNK36bio4+f27Tw9V9auV8KDEBEWGxqH8CS8ksr86vCfwEU4B9YZII03AAAAABJRU5ErkJggg==";

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
            return selection != null && selection.Children.Count > 0;
        }

        public static void PerformAction(TreeNode selection, MainWindow window, InstanceStateData stateData)
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
                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "0c3400fd-8872-4395-83bc-a5dc5f4967fe");
                Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });
                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1).ContinueWith(task =>
                    {
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
                            stateData.RemoveFurtherTransformationModule(i);
                            minIndex = Math.Min(minIndex, i);
                        }
                    }
                }

                if (InstanceStateData.IsUIAvailable)
                {
                    _ = window.UpdateFurtherTransformations(minIndex).ContinueWith(task =>
                    {
                        window.SetSelection(window.TransformedTree.GetLastCommonAncestor(nodeNames));
                    });
                }
            }
        }
    }
}