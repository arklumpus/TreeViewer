using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace a2b9a55634acf49df9cbc46623975081e
{
    public static class MyModule
    {
        public const string Name = "Subsample tree";
        public const string HelpText = "Removes taxa from the tree until only the requested number of taxa remain.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "2b9a5563-4acf-49df-9cbc-46623975081e";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACASURBVDhPY2TAA4qKiv5DmQx9fX1Y1cIFkRUjA1waYQDFAHTF6IZiM4yRGGfiA3hdgA6weRPFACgTL0C3hCQnU+pd6gGv2o1wlzBBabIBSf5AthkGSIqFG9z2jkBqP4QHBiA+BBAbjUBXOIBcAqJBfIpTIhwQ6wJ0QFIYYAIGBgAFaDzvLFasFgAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADjSURBVEhLY2QgAIqKihqAVD2Ehwr6+voI6meC0vgAVsOJBSguoNS12AC6DyhyLTaA7oP/IBrZtTAxfACf74iJg0YoTRZgxOZCcsMbGyDGBxQBgnFACOBLeSBADR/gTXlYfYAM0H2DK1Xh8jW6D7CmGK/ajf9BGMolCRAV1jDDtzX7Y6gn5GtqxAFF+QQMaBJEQAMdgNR+CA8DOAKD6wCUjRfgjQMclhBtOAgQjGQ0S0gyHARQLMCRKxtvcNuDDdX4ehBkGUbGwpfz0S0gKyLxWkBM7qUEDExpSk1AVFlEPmBgAAAEiFoomgXcawAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADrSURBVFhH7ZcxDsIwDEVTTlZWFu6SvQLEnruwsMLNIFYDQqpxvo3TMvQtblSptn+/07QLCmKMxxwO44onpaR65qZEFDG5hUm1LbqU4BRw71KCU+BBkevydQ8FUUrrgVOJbnTfuvJ8zxJaBdxReUADMk1ESwWgafrZ6TVqSnIKuDtdopnT0elq6QFISbMCu+Hy7vB63pufs/g+sBaweAGwebLp+hxu40pkm015L9dVVO4FilAlJ9TjIxShTk6Y5pcpwpScMG8gH0WYk68Qk1eAnmQQrKfief8L0O92K/5vK/Y+E9bgFJjxTBjCE/R4TddyWnv1AAAAAElFTkSuQmCC";

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
                ("Target taxa", "Group:3"),
                
                /// <param name="Type:">
                /// This parameter determines whether the number of taxa to leave in the tree should be specified as a relative value
                /// (e.g., 50% of the total number of taxa), or as an absolute value (e.g., 100 taxa).
                /// </param>
                ("Type:", "ComboBox:0[\"Relative\",\"Absolute\"]"),
                
                /// <param name="Threshold:">
                /// This slider is used to specify the number of taxa to leave in the tree when the [Type](#type) combo box is set to
                /// `Relative`. At least three taxa are always left in the tree.
                /// </param>
                ("Threshold:","Slider:0.5[\"0\",\"1\",\"{0:P1}\"]"),
                
                /// <param name="Threshold: ">
                /// This number box is used to specify the number of taxa to leave in the tree when the [Type](#type) combo box is set to
                /// `Absolute`. At least three taxa are always left in the tree.
                /// </param>
                ("Threshold: ","NumericUpDown:" + (tree.GetLeaves().Count / 2).ToString() + "[\"3\",\"Infinity\"]"),

                /// <param name="Criterion:">
                /// For each pair of leaves one of which must be removed for the tree, this parameter determines whether the one with the
                /// shortest or longest branch is removed.
                /// </param>
                ("Criterion:", "ComboBox:0[\"Shortest branch\",\"Longest branch\"]"),
                
                /// <param name="Average lengths">
                /// If this check box is unchecked, branch lengths are not changed. Otherwise, for each pair of leaves one of which must
                /// be removed for the tree, the length of the surviving leaf is set to the average of the lengths of the two leaves.
                /// </param>
                ("Average lengths", "CheckBox:true"),

                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw of the tree.
                /// </param>
                ("Apply", "Button:")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((int)currentParameterValues["Type:"] == 0)
            {
                controlStatus["Threshold:"] = ControlStatus.Enabled;
                controlStatus["Threshold: "] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Threshold:"] = ControlStatus.Hidden;
                controlStatus["Threshold: "] = ControlStatus.Enabled;
            }



            return (bool)currentParameterValues["Apply"] || ((bool)currentParameterValues["Average lengths"] != (bool)previousParameterValues["Average lengths"]) || ((int)currentParameterValues["Type:"] != (int)previousParameterValues["Type:"]) || ((int)currentParameterValues["Criterion:"] != (int)previousParameterValues["Criterion:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            int criterionType = (int)parameterValues["Type:"];
            int targetTaxa = (int)Math.Round((double)parameterValues["Threshold: "]);

            List<TreeNode> leaves = tree.GetLeaves();

            if (criterionType == 0)
            {
                targetTaxa = (int)Math.Round((double)parameterValues["Threshold:"] * leaves.Count);
            }

            targetTaxa = Math.Max(targetTaxa, 3);

            int criterion = (int)parameterValues["Criterion:"];
            bool averageLength = (bool)parameterValues["Average lengths"];

            HashSet<int> removedIndices = new HashSet<int>(leaves.Count - targetTaxa);

            double[][] distanceMatrix = tree.CreateDistanceMatrixDouble(progressCallback: x => progressAction(0.5 * x));


            while (leaves.Count - removedIndices.Count > targetTaxa)
            {
                double minDist = double.MaxValue;
                int minI = -1;
                int minJ = -1;

                for (int i = 0; i < leaves.Count; i++)
                {
                    if (!removedIndices.Contains(i))
                    {
                        for (int j = 0; j < i; j++)
                        {
                            if (!removedIndices.Contains(j))
                            {
                                if (distanceMatrix[i][j] < minDist)
                                {
                                    minDist = distanceMatrix[i][j];
                                    minI = i;
                                    minJ = j;
                                }
                            }
                        }
                    }
                }

                int indexToRemove = -1;
                int otherIndex = -1;

                if (criterion == 0)
                {
                    if (leaves[minI].Length < leaves[minJ].Length)
                    {
                        indexToRemove = minI;
                        otherIndex = minJ;
                    }
                    else
                    {
                        indexToRemove = minJ;
                        otherIndex = minI;
                    }
                }
                else
                {
                    if (leaves[minI].Length >= leaves[minJ].Length)
                    {
                        indexToRemove = minI;
                        otherIndex = minJ;
                    }
                    else
                    {
                        indexToRemove = minJ;
                        otherIndex = minI;
                    }
                }

                removedIndices.Add(indexToRemove);

                if (averageLength)
                {
                    double delta = (leaves[indexToRemove].Length - leaves[otherIndex].Length) * 0.5;
                    leaves[otherIndex].Length += delta;

                    for (int j = 0; j < leaves.Count; j++)
                    {
                        if (j != otherIndex)
                        {
                            distanceMatrix[Math.Max(otherIndex, j)][Math.Min(otherIndex, j)] += delta;
                        }
                    }
                }

                progressAction(0.5 + 0.5 * removedIndices.Count / (leaves.Count - targetTaxa));
            }

            foreach (int index in removedIndices)
            {
                tree = tree.Prune(leaves[index], false);
            }

        }
    }
}
