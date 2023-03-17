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

using Markdig;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VectSharp;
using static TreeViewer.Stats.TreeReport;

namespace TreeViewer.Stats
{
    internal static class TwoTreesReport
    {
        public static (Markdig.Syntax.MarkdownDocument report, string reportSource) CreateReport(TreeNode tree1, TreeNode tree2, Action<string, double> progressAction, Dictionary<string, GetPlot> Plots, Dictionary<string, Func<(string header, IEnumerable<double[]>)>> Data)
        {
            List<string> leafNames1 = tree1.GetLeafNames();
            List<TreeNode> nodes1 = tree1.GetChildrenRecursive();
            bool rooted1 = tree1.IsRooted();

            List<string> intersection = new List<string>(leafNames1);
            HashSet<string> union = new HashSet<string>(intersection);

            List<string> leafNames2 = tree2.GetLeafNames();
            List<TreeNode> nodes2 = tree2.GetChildrenRecursive();
            bool rooted2 = tree2.IsRooted();

            bool binary1 = true;
            bool binary2 = true;

            foreach (TreeNode node in nodes1)
            {
                if (node.Parent != null && node.Children.Count > 2)
                {
                    binary1 = false;
                    break;
                }
            }

            foreach (TreeNode node in nodes2)
            {
                if (node.Parent != null && node.Children.Count > 2)
                {
                    binary2 = false;
                    break;
                }
            }

            foreach (string leafName in leafNames2)
            {
                union.Add(leafName);
            }

            {
                List<int> toBeRemoved = new List<int>();

                for (int j = 0; j < intersection.Count; j++)
                {
                    if (!leafNames2.Contains(intersection[j]))
                    {
                        toBeRemoved.Add(j);
                    }
                }

                for (int j = toBeRemoved.Count - 1; j >= 0; j--)
                {
                    intersection.RemoveAt(toBeRemoved[j]);
                }
            }

            bool sameLeaves = union.Count == intersection.Count;

            List<string> diff1 = new List<string>(leafNames1.Count - intersection.Count);
            List<string> diff2 = new List<string>(leafNames2.Count - intersection.Count);

            for (int i = 0; i < leafNames1.Count; i++)
            {
                if (!intersection.Contains(leafNames1[i]))
                {
                    diff1.Add(leafNames1[i]);
                }
            }

            for (int i = 0; i < leafNames2.Count; i++)
            {
                if (!intersection.Contains(leafNames2[i]))
                {
                    diff2.Add(leafNames2[i]);
                }
            }

            diff1.Sort(StringComparer.OrdinalIgnoreCase);
            diff2.Sort(StringComparer.OrdinalIgnoreCase);
            intersection.Sort(StringComparer.OrdinalIgnoreCase);

            TreeNode currTree1;
            TreeNode currTree2;

            if (sameLeaves)
            {
                currTree1 = tree1;
                currTree2 = tree2;
            }
            else
            {
                currTree1 = tree1.Clone();

                List<TreeNode> leaves1 = currTree1.GetLeaves();

                for (int j = 0; j < leaves1.Count; j++)
                {
                    if (!intersection.Contains(leaves1[j].Name))
                    {
                        currTree1 = currTree1.Prune(leaves1[j], false);
                    }
                }

                if (!rooted1)
                {
                    currTree1 = currTree1.GetUnrootedTree();
                }
                
                currTree2 = tree2.Clone();

                List<TreeNode> leaves2 = currTree2.GetLeaves();

                for (int j = 0; j < leaves2.Count; j++)
                {
                    if (!intersection.Contains(leaves2[j].Name))
                    {
                        currTree2 = currTree2.Prune(leaves2[j], false);
                    }
                }

                if (!rooted2)
                {
                    currTree2 = currTree2.GetUnrootedTree();
                }
            }

            bool sameTopology = false;


            List<(string[], string[], double)> splits1 = (from el in currTree1.GetSplits()
                                                          select (
                                                          (from el1 in el.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToArray(),
                                                          (from el2 in el.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToArray(),
                                                          el.branchLength
                                                          )).ToList();

            List<(string[], string[], double)> splits2 = (from el in currTree2.GetSplits()
                                                          select (
                                                          (from el1 in el.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToArray(),
                                                          (from el2 in el.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToArray(),
                                                          el.branchLength
                                                          )).ToList();

            (int RFdistance, double wRFDistance, double elDistance, int in1NotIn2, int in2NotIn1, int common, List<double> splitLengthDiffs) = Comparisons.RobinsonFouldsDistanceWithSplitData(splits1, splits2);

            sameTopology = RFdistance == 0;

            bool hasLengths1 = !((from el in nodes1 where el.Parent != null select el.Length).Any(x => double.IsNaN(x)));
            bool hasLengths2 = !((from el in nodes2 where el.Parent != null select el.Length).Any(x => double.IsNaN(x)));

            double averageSplitLengthDiff = double.NaN;
            double[] splitLengthDiffHDI = null;
            string splitLengthDiffPlotGuid = null;
            Page splitLengthDiffDistribution = null;
            (double splitLengthDiffUnderflow, int splitLengthDiffUnderflowCount, double splitLengthDiffOverflow, int splitLengthDiffOverflowCount) = (0, 0, 0, 0);


            if (hasLengths1 && hasLengths2 && splitLengthDiffs.Count > 1)
            {
                splitLengthDiffPlotGuid = Guid.NewGuid().ToString();
                splitLengthDiffDistribution = Stats.SplitLengthDifferences.GetPlot(splitLengthDiffs, splitLengthDiffPlotGuid);
                (splitLengthDiffUnderflow, splitLengthDiffUnderflowCount, splitLengthDiffOverflow, splitLengthDiffOverflowCount) = Stats.Histogram.GetOverUnderflow(splitLengthDiffs);

                Plots.Add(splitLengthDiffPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                {
                    clickActions = null;
                    return TreeViewer.Stats.Histogram.GetPlot(splitLengthDiffs, "Split length difference", "Distribution of split length differences", null, out descriptions, interactive);
                });

                {
                    IEnumerable<double[]> getData()
                    {
                        foreach (double d in splitLengthDiffs)
                        {
                            yield return new double[] { d };
                        }
                    }

                    Data.Add(splitLengthDiffPlotGuid, () => ("Split length difference", getData()));
                }

                averageSplitLengthDiff = splitLengthDiffs.Average();
                splitLengthDiffHDI = BayesStats.HighestDensityInterval(splitLengthDiffs, 0.89);
            }

            int sackinIndex1 = -1;
            int sackinIndex2 = -1;

            double sackinYule1 = double.NaN;
            double sackinPDA1 = double.NaN;

            double sackinYule2 = double.NaN;
            double sackinPDA2 = double.NaN;

            int collessIndex1 = -1;
            int collessIndex2 = -1;

            double collessYule1 = double.NaN;
            double collessPDA1 = double.NaN;

            double collessYule2 = double.NaN;
            double collessPDA2 = double.NaN;

            double sackinYuleP = double.NaN;
            double sackinPDAP = double.NaN;

            double collessYuleP = double.NaN;
            double collessPDAP = double.NaN;

            int numOfCherries1 = -1;
            int numOfCherries2 = -1;

            double[] cherryYuleDistribution = null;
            double[] cherryPdaDistribution = null;


            double[] sackinYuleDistribution = null;
            double[] collessYuleDistribution = null;

            double[] sackinPdaDistribution = null;
            double[] collessPdaDistribution = null;

            double sackinMinValue = double.NaN;
            double sackinMaxValue = double.NaN;

            double collessMinValue = double.NaN;
            double collessMaxValue = double.NaN;

            double cherryMinValue = double.NaN;
            double cherryMaxValue = double.NaN;

            string sackinDistributionPlotGuid = null;
            Page sackinDistribution = null;

            string collessDistributionPlotGuid = null;
            Page collessDistribution = null;


            numOfCherries1 = (int)currTree1.NumberOfCherries(TreeNode.NullHypothesis.None);
            numOfCherries2 = (int)currTree2.NumberOfCherries(TreeNode.NullHypothesis.None);

            double numberOfCherriesYule1 = (numOfCherries1 - leafNames1.Count / 3.0) / Math.Sqrt(2.0 * leafNames1.Count / 45.0);
            double numberOfCherriesPDA1;

            {
                double mu = (double)leafNames1.Count * (leafNames1.Count - 1) / (2.0 * (2 * leafNames1.Count - 5));
                double sigmaSq = (double)leafNames1.Count * (leafNames1.Count - 1) * (leafNames1.Count - 4) * (leafNames1.Count - 5) / (2.0 * (2 * leafNames1.Count - 5) * (2 * leafNames1.Count - 5) * (2 * leafNames1.Count - 7));
                numberOfCherriesPDA1 = (numOfCherries1 - mu) / Math.Sqrt(sigmaSq);
            }

            double numberOfCherriesYule2 = (numOfCherries2 - leafNames2.Count / 3.0) / Math.Sqrt(2.0 * leafNames2.Count / 45.0);
            double numberOfCherriesPDA2;

            {
                double mu = (double)leafNames2.Count * (leafNames2.Count - 1) / (2.0 * (2 * leafNames2.Count - 5));
                double sigmaSq = (double)leafNames2.Count * (leafNames2.Count - 1) * (leafNames2.Count - 4) * (leafNames2.Count - 5) / (2.0 * (2 * leafNames2.Count - 5) * (2 * leafNames2.Count - 5) * (2 * leafNames2.Count - 7));
                numberOfCherriesPDA2 = (numOfCherries2 - mu) / Math.Sqrt(sigmaSq);
            }



            if (intersection.Count > 3)
            {
                if (binary1 && binary2)
                {
                    progressAction("Sampling trees under the YHK model", 0);

                    (sackinYuleDistribution, collessYuleDistribution, cherryYuleDistribution) = ShapeIndices.GetDistribution(intersection.Count, TreeNode.NullHypothesis.YHK, rooted1 && rooted2, rooted1 && rooted2, rooted1 && rooted2 && binary1 && binary2, progress =>
                    {
                        progressAction(null, 0.5 * progress);
                    });

                    progressAction("Sampling trees under the PDA model", 0.5);

                    (sackinPdaDistribution, collessPdaDistribution, cherryPdaDistribution) = ShapeIndices.GetDistribution(intersection.Count, TreeNode.NullHypothesis.PDA, rooted1 && rooted2, rooted1 && rooted2, rooted1 && rooted2 && binary1 && binary2, progress =>
                    {
                        progressAction(null, 0.5 * progress + 0.5);
                    });

                    progressAction(null, 1);
                }
            }

            if (rooted1 && rooted2)
            {
                sackinIndex1 = (int)currTree1.SackinIndex(TreeNode.NullHypothesis.None);
                sackinIndex2 = (int)currTree2.SackinIndex(TreeNode.NullHypothesis.None);

                if (binary1 && binary2)
                {
                    sackinYule1 = (sackinIndex1 - 2 * leafNames1.Count * (from el in Enumerable.Range(2, leafNames1.Count - 1) select 1.0 / el).Sum()) / leafNames1.Count;
                    sackinPDA1 = sackinIndex1 / Math.Pow(leafNames1.Count, 1.5);

                    sackinYule2 = (sackinIndex2 - 2 * leafNames2.Count * (from el in Enumerable.Range(2, leafNames2.Count - 1) select 1.0 / el).Sum()) / leafNames2.Count;
                    sackinPDA2 = sackinIndex2 / Math.Pow(leafNames2.Count, 1.5);

                    collessIndex1 = (int)currTree1.CollessIndex(TreeNode.NullHypothesis.None);
                    collessIndex2 = (int)currTree2.CollessIndex(TreeNode.NullHypothesis.None);

                    collessYule1 = (collessIndex1 - TreeNode.GetCollessExpectationYHK(leafNames1.Count)) / leafNames1.Count;
                    collessPDA1 = collessIndex1 / Math.Pow(leafNames1.Count, 1.5);

                    collessYule2 = (collessIndex2 - TreeNode.GetCollessExpectationYHK(leafNames2.Count)) / leafNames2.Count;
                    collessPDA2 = collessIndex2 / Math.Pow(leafNames2.Count, 1.5);


                    if (sameTopology)
                    {
                        sackinYuleP = (double)sackinYuleDistribution.Count(x => x >= sackinYule1) / sackinYuleDistribution.Length;
                        sackinPDAP = (double)sackinPdaDistribution.Count(x => x >= sackinPDA1) / sackinPdaDistribution.Length;

                        sackinDistributionPlotGuid = Guid.NewGuid().ToString();
                        sackinDistribution = Stats.SackinDistribution.GetPlot(sackinYuleDistribution, sackinPdaDistribution, sackinYule1, sackinPDA1, sackinYuleP, sackinPDAP, sackinDistributionPlotGuid);
                        Plots.Add(sackinDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            clickActions = null;
                            return TreeViewer.Stats.Distribution.GetPlot(sackinYuleDistribution, sackinPdaDistribution, sackinYule1, sackinPDA1, sackinYuleP, sackinPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Sackin index", "Sackin index distribution", "S", null, out descriptions, interactive);
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int i = 0; i < sackinYuleDistribution.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        yield return new double[] { sackinYuleDistribution[i], sackinPdaDistribution[i], sackinYule1, sackinPDA1 };
                                    }
                                    else
                                    {
                                        yield return new double[] { sackinYuleDistribution[i], sackinPdaDistribution[i] };
                                    }
                                }
                            }

                            Data.Add(sackinDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                        }

                        collessYuleP = (double)collessYuleDistribution.Count(x => x >= collessYule1) / collessYuleDistribution.Length;
                        collessPDAP = (double)collessPdaDistribution.Count(x => x >= collessPDA1) / collessPdaDistribution.Length;


                        collessDistributionPlotGuid = Guid.NewGuid().ToString();
                        collessDistribution = Stats.CollessDistribution.GetPlot(collessYuleDistribution, collessPdaDistribution, collessYule1, collessPDA1, collessYuleP, collessPDAP, collessDistributionPlotGuid);
                        Plots.Add(collessDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            clickActions = null;
                            return TreeViewer.Stats.Distribution.GetPlot(collessYuleDistribution, collessPdaDistribution, collessYule1, collessPDA1, collessYuleP, collessPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Colless index", "Colless index distribution", "C", null, out descriptions, interactive);
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int i = 0; i < collessYuleDistribution.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        yield return new double[] { collessYuleDistribution[i], collessPdaDistribution[i], collessYule1, collessPDA1 };
                                    }
                                    else
                                    {
                                        yield return new double[] { collessYuleDistribution[i], collessPdaDistribution[i] };
                                    }
                                }
                            }

                            Data.Add(collessDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                        }
                    }
                    else
                    {
                        double sackinDiffYule = Math.Abs(sackinYule1 - sackinYule2);
                        double sackinDiffPDA = Math.Abs(sackinPDA1 - sackinPDA2);
                        double collessDiffYule = Math.Abs(collessYule1 - collessYule2);
                        double collessDiffPDA = Math.Abs(collessPDA1 - collessPDA2);

                        List<double> sackinYuleDiffDistribution = new List<double>(sackinYuleDistribution.Length * (sackinYuleDistribution.Length - 1) / 2);
                        List<double> sackinPDADiffDistribution = new List<double>(sackinYuleDistribution.Length * (sackinYuleDistribution.Length - 1) / 2);
                        List<double> collessYuleDiffDistribution = new List<double>(sackinYuleDistribution.Length * (sackinYuleDistribution.Length - 1) / 2);
                        List<double> collessPDADiffDistribution = new List<double>(sackinYuleDistribution.Length * (sackinYuleDistribution.Length - 1) / 2);

                        for (int i = 0; i < sackinYuleDistribution.Length; i++)
                        {
                            for (int j = 0; j < i; j++)
                            {
                                sackinYuleDiffDistribution.Add(sackinYuleDistribution[i] - sackinYuleDistribution[j]);
                                sackinPDADiffDistribution.Add(sackinPdaDistribution[i] - sackinPdaDistribution[j]);
                                collessYuleDiffDistribution.Add(collessYuleDistribution[i] - collessYuleDistribution[j]);
                                collessPDADiffDistribution.Add(collessPdaDistribution[i] - collessPdaDistribution[j]);
                            }
                        }

                        {
                            double minValue = Math.Min(Math.Min(sackinYuleDiffDistribution.Min(), sackinPDADiffDistribution.Min()), Math.Min(sackinYule1 - sackinYule2, sackinPDA1 - sackinPDA2));
                            double maxValue = Math.Max(Math.Max(sackinYuleDiffDistribution.Max(), sackinPDADiffDistribution.Max()), Math.Max(sackinYule1 - sackinYule2, sackinPDA1 - sackinPDA2));

                            int resolutionSteps = 100;

                            double d = (maxValue - minValue) / (resolutionSteps - 1);

                            int[] cdf1Count = new int[resolutionSteps];
                            int[] cdf2Count = new int[resolutionSteps];

                            for (int i = 0; i < sackinYuleDiffDistribution.Count; i++)
                            {
                                int index1 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((sackinYuleDiffDistribution[i] - minValue) / d));
                                cdf1Count[index1]++;

                                int index2 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((sackinPDADiffDistribution[i] - minValue) / d));
                                cdf2Count[index2]++;
                            }

                            double[] cdf1 = new double[resolutionSteps];
                            double[] cdf2 = new double[resolutionSteps];

                            for (int i = 0; i < resolutionSteps; i++)
                            {
                                if (i == 0)
                                {
                                    cdf1[i] = cdf1Count[i] / (double)sackinYuleDiffDistribution.Count;
                                }
                                else
                                {
                                    cdf1[i] = cdf1Count[i] / (double)sackinYuleDiffDistribution.Count + cdf1[i - 1];
                                }


                                if (i == 0)
                                {
                                    cdf2[i] = cdf2Count[i] / (double)sackinPDADiffDistribution.Count;
                                }
                                else
                                {
                                    cdf2[i] = cdf2Count[i] / (double)sackinPDADiffDistribution.Count + cdf2[i - 1];
                                }
                            }

                            double[] pdf1 = new double[resolutionSteps];
                            double[] pdf2 = new double[resolutionSteps];

                            pdf1[0] = 0.5 * cdf1[1] / d;
                            pdf1[resolutionSteps - 1] = 0.5 * (cdf1[resolutionSteps - 1] - cdf1[resolutionSteps - 2]) / d;

                            pdf2[0] = 0.5 * cdf2[1] / d;
                            pdf2[resolutionSteps - 1] = 0.5 * (cdf2[resolutionSteps - 1] - cdf2[resolutionSteps - 2]) / d;

                            for (int i = 1; i < resolutionSteps - 1; i++)
                            {
                                pdf1[i] = 0.5 * (cdf1[i + 1] - cdf1[i - 1]) / d;
                                pdf2[i] = 0.5 * (cdf2[i + 1] - cdf2[i - 1]) / d;
                            }

                            sackinYuleDistribution = pdf1;
                            sackinPdaDistribution = pdf2;
                            sackinMinValue = minValue;
                            sackinMaxValue = maxValue;
                        }

                        {
                            double minValue = Math.Min(Math.Min(collessYuleDiffDistribution.Min(), collessPDADiffDistribution.Min()), Math.Min(collessYule1 - collessYule2, collessPDA1 - collessPDA2));
                            double maxValue = Math.Max(Math.Max(collessYuleDiffDistribution.Max(), collessPDADiffDistribution.Max()), Math.Max(collessYule1 - collessYule2, collessPDA1 - collessPDA2));

                            int resolutionSteps = 100;

                            double d = (maxValue - minValue) / (resolutionSteps - 1);

                            int[] cdf1Count = new int[resolutionSteps];
                            int[] cdf2Count = new int[resolutionSteps];

                            for (int i = 0; i < collessYuleDiffDistribution.Count; i++)
                            {
                                int index1 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((collessYuleDiffDistribution[i] - minValue) / d));
                                cdf1Count[index1]++;

                                int index2 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((collessPDADiffDistribution[i] - minValue) / d));
                                cdf2Count[index2]++;
                            }

                            double[] cdf1 = new double[resolutionSteps];
                            double[] cdf2 = new double[resolutionSteps];

                            for (int i = 0; i < resolutionSteps; i++)
                            {
                                if (i == 0)
                                {
                                    cdf1[i] = cdf1Count[i] / (double)collessYuleDiffDistribution.Count;
                                }
                                else
                                {
                                    cdf1[i] = cdf1Count[i] / (double)collessYuleDiffDistribution.Count + cdf1[i - 1];
                                }


                                if (i == 0)
                                {
                                    cdf2[i] = cdf2Count[i] / (double)collessPDADiffDistribution.Count;
                                }
                                else
                                {
                                    cdf2[i] = cdf2Count[i] / (double)collessPDADiffDistribution.Count + cdf2[i - 1];
                                }
                            }

                            double[] pdf1 = new double[resolutionSteps];
                            double[] pdf2 = new double[resolutionSteps];

                            pdf1[0] = 0.5 * cdf1[1] / d;
                            pdf1[resolutionSteps - 1] = 0.5 * (cdf1[resolutionSteps - 1] - cdf1[resolutionSteps - 2]) / d;

                            pdf2[0] = 0.5 * cdf2[1] / d;
                            pdf2[resolutionSteps - 1] = 0.5 * (cdf2[resolutionSteps - 1] - cdf2[resolutionSteps - 2]) / d;

                            for (int i = 1; i < resolutionSteps - 1; i++)
                            {
                                pdf1[i] = 0.5 * (cdf1[i + 1] - cdf1[i - 1]) / d;
                                pdf2[i] = 0.5 * (cdf2[i + 1] - cdf2[i - 1]) / d;
                            }


                            collessYuleDistribution = pdf1;
                            collessPdaDistribution = pdf2;

                            collessMinValue = minValue;
                            collessMaxValue = maxValue;
                        }

                        sackinYuleP = (double)sackinYuleDiffDistribution.Count(x => Math.Abs(x) >= sackinDiffYule) / sackinYuleDiffDistribution.Count;
                        sackinPDAP = (double)sackinPDADiffDistribution.Count(x => Math.Abs(x) >= sackinDiffPDA) / sackinPDADiffDistribution.Count;

                        collessYuleP = (double)collessYuleDiffDistribution.Count(x => Math.Abs(x) >= collessDiffYule) / collessYuleDiffDistribution.Count;
                        collessPDAP = (double)collessPDADiffDistribution.Count(x => Math.Abs(x) >= collessDiffPDA) / collessPDADiffDistribution.Count;

                        sackinDistributionPlotGuid = Guid.NewGuid().ToString();
                        sackinDistribution = Stats.SackinDistribution.GetPlotTwoTailed(sackinYuleDistribution, sackinPdaDistribution, sackinMinValue, sackinMaxValue, sackinYule1 - sackinYule2, sackinPDA1 - sackinPDA2, sackinYuleP, sackinPDAP, sackinDistributionPlotGuid);
                        Plots.Add(sackinDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            clickActions = null;
                            return TreeViewer.Stats.Distribution.GetPlotTwoTailed(sackinYuleDistribution, sackinPdaDistribution, sackinMinValue, sackinMaxValue, sackinYule1 - sackinYule2, sackinPDA1 - sackinPDA2, sackinYuleP, sackinPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Sackin index difference", "Sackin index difference distribution", "ΔS", null, out descriptions, interactive);
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int i = 0; i < sackinYuleDistribution.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        yield return new double[] { sackinMinValue + (sackinMaxValue - sackinMinValue) / 99 * i, sackinYuleDistribution[i], sackinPdaDistribution[i], sackinYule1 - sackinYule2, sackinPDA1 - sackinPDA2 };
                                    }
                                    else
                                    {
                                        yield return new double[] { sackinMinValue + (sackinMaxValue - sackinMinValue) / 99 * i, sackinYuleDistribution[i], sackinPdaDistribution[i] };
                                    }
                                }
                            }

                            Data.Add(sackinDistributionPlotGuid, () => ("x\tDensity (YHK model)\tDensity (PDA model)\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                        }

                        collessDistributionPlotGuid = Guid.NewGuid().ToString();
                        collessDistribution = Stats.CollessDistribution.GetPlotTwoTailed(collessYuleDistribution, collessPdaDistribution, sackinMinValue, sackinMaxValue, collessYule1 - collessYule2, collessPDA1 - collessPDA2, collessYuleP, collessPDAP, collessDistributionPlotGuid);
                        Plots.Add(collessDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            clickActions = null;
                            return TreeViewer.Stats.Distribution.GetPlotTwoTailed(collessYuleDistribution, collessPdaDistribution, sackinMinValue, sackinMaxValue, collessYule1 - collessYule2, collessPDA1 - collessPDA2, collessYuleP, collessPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Colless index difference", "Colless index difference distribution", "ΔC", null, out descriptions, interactive);
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int i = 0; i < collessYuleDistribution.Length; i++)
                                {
                                    if (i == 0)
                                    {
                                        yield return new double[] { collessMinValue + (collessMaxValue - collessMinValue) / 99 * i, collessYuleDistribution[i], collessPdaDistribution[i], collessYule1 - collessYule2, collessPDA1 - collessPDA2 };
                                    }
                                    else
                                    {
                                        yield return new double[] { collessMinValue + (collessMaxValue - collessMinValue) / 99 * i, collessYuleDistribution[i], collessPdaDistribution[i] };
                                    }
                                }
                            }

                            Data.Add(collessDistributionPlotGuid, () => ("x\tDensity (YHK model)\tDensity (PDA model)\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                        }

                    }
                }
            }

            string numberOfCherriesDistributionPlotGuid = null;
            Page numberOfCherriesDistribution = null;

            double numberOfCherriesYuleP = double.NaN;
            double numberOfCherriesPDAP = double.NaN;

            double numberOfCherriesYuleNormalP = double.NaN;
            double numberOfCherriesPDANormalP = double.NaN;

            if (binary1 && binary2 && sameTopology && intersection.Count >= 6)
            {
                numberOfCherriesYuleP = (double)cherryYuleDistribution.Count(x => x >= numberOfCherriesYule1) / cherryYuleDistribution.Length;
                numberOfCherriesPDAP = (double)cherryPdaDistribution.Count(x => x >= numberOfCherriesPDA1) / cherryPdaDistribution.Length;

                numberOfCherriesYuleNormalP = 1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, numberOfCherriesYule1);
                numberOfCherriesPDANormalP = 1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, numberOfCherriesPDA1);

                numberOfCherriesDistributionPlotGuid = Guid.NewGuid().ToString();
                numberOfCherriesDistribution = Stats.NumberOfCherriesDistribution.GetPlot(cherryYuleDistribution, cherryPdaDistribution, numberOfCherriesYule1, numberOfCherriesPDA1, numberOfCherriesYuleP, numberOfCherriesPDAP, numberOfCherriesDistributionPlotGuid);
                Plots.Add(numberOfCherriesDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                {
                    clickActions = null;
                    return TreeViewer.Stats.Distribution.GetPlotWithStandardNormal(cherryYuleDistribution, cherryPdaDistribution, numberOfCherriesYule1, numberOfCherriesPDA1, numberOfCherriesYuleP, numberOfCherriesPDAP, "Density (YHK model)", "Density (PDA model)", "Density (limit distribution)", "Normalised number of cherries", "Distribution of the number of cherries", "K", null, out descriptions, interactive);
                });

                {
                    IEnumerable<double[]> getData()
                    {
                        for (int i = 0; i < cherryYuleDistribution.Length; i++)
                        {
                            if (i == 0)
                            {
                                yield return new double[] { cherryYuleDistribution[i], cherryPdaDistribution[i], numberOfCherriesYule1, numberOfCherriesPDA1 };
                            }
                            else
                            {
                                yield return new double[] { cherryYuleDistribution[i], cherryPdaDistribution[i] };
                            }
                        }
                    }

                    Data.Add(numberOfCherriesDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                }
            }
            else if (binary1 && binary2 && intersection.Count >= 6)
            {
                double cherryDiffYule = Math.Abs(numberOfCherriesYule1 - numberOfCherriesYule2);
                double cherryDiffPDA = Math.Abs(numberOfCherriesPDA1 - numberOfCherriesPDA2);

                List<double> cherryYuleDiffDistribution = new List<double>(cherryYuleDistribution.Length * (cherryYuleDistribution.Length - 1) / 2);
                List<double> cherryPDADiffDistribution = new List<double>(cherryYuleDistribution.Length * (cherryYuleDistribution.Length - 1) / 2);

                for (int i = 0; i < cherryYuleDistribution.Length; i++)
                {
                    for (int j = 0; j < i; j++)
                    {
                        cherryYuleDiffDistribution.Add(cherryYuleDistribution[i] - cherryYuleDistribution[j]);
                        cherryPDADiffDistribution.Add(cherryPdaDistribution[i] - cherryPdaDistribution[j]);
                    }
                }

                {
                    double minValue = Math.Min(Math.Min(cherryYuleDiffDistribution.Min(), cherryPDADiffDistribution.Min()), Math.Min(numberOfCherriesYule1 - numberOfCherriesYule2, numberOfCherriesPDA1 - numberOfCherriesPDA2));
                    double maxValue = Math.Max(Math.Max(cherryYuleDiffDistribution.Max(), cherryPDADiffDistribution.Max()), Math.Max(numberOfCherriesYule1 - numberOfCherriesYule2, numberOfCherriesPDA1 - numberOfCherriesPDA2));

                    int resolutionSteps = 100;

                    double d = (maxValue - minValue) / (resolutionSteps - 1);

                    int[] cdf1Count = new int[resolutionSteps];
                    int[] cdf2Count = new int[resolutionSteps];

                    for (int i = 0; i < cherryYuleDiffDistribution.Count; i++)
                    {
                        int index1 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((cherryYuleDiffDistribution[i] - minValue) / d));
                        cdf1Count[index1]++;

                        int index2 = Math.Min(resolutionSteps - 1, (int)Math.Ceiling((cherryPDADiffDistribution[i] - minValue) / d));
                        cdf2Count[index2]++;
                    }

                    double[] cdf1 = new double[resolutionSteps];
                    double[] cdf2 = new double[resolutionSteps];

                    for (int i = 0; i < resolutionSteps; i++)
                    {
                        if (i == 0)
                        {
                            cdf1[i] = cdf1Count[i] / (double)cherryYuleDiffDistribution.Count;
                        }
                        else
                        {
                            cdf1[i] = cdf1Count[i] / (double)cherryYuleDiffDistribution.Count + cdf1[i - 1];
                        }


                        if (i == 0)
                        {
                            cdf2[i] = cdf2Count[i] / (double)cherryPDADiffDistribution.Count;
                        }
                        else
                        {
                            cdf2[i] = cdf2Count[i] / (double)cherryPDADiffDistribution.Count + cdf2[i - 1];
                        }
                    }

                    double[] pdf1 = new double[resolutionSteps];
                    double[] pdf2 = new double[resolutionSteps];

                    pdf1[0] = 0.5 * cdf1[1] / d;
                    pdf1[resolutionSteps - 1] = 0.5 * (cdf1[resolutionSteps - 1] - cdf1[resolutionSteps - 2]) / d;

                    pdf2[0] = 0.5 * cdf2[1] / d;
                    pdf2[resolutionSteps - 1] = 0.5 * (cdf2[resolutionSteps - 1] - cdf2[resolutionSteps - 2]) / d;

                    for (int i = 1; i < resolutionSteps - 1; i++)
                    {
                        pdf1[i] = 0.5 * (cdf1[i + 1] - cdf1[i - 1]) / d;
                        pdf2[i] = 0.5 * (cdf2[i + 1] - cdf2[i - 1]) / d;
                    }

                    cherryYuleDistribution = pdf1;
                    cherryPdaDistribution = pdf2;
                    cherryMinValue = minValue;
                    cherryMaxValue = maxValue;
                }

                numberOfCherriesYuleP = (double)cherryYuleDiffDistribution.Count(x => Math.Abs(x) >= cherryDiffYule) / cherryYuleDiffDistribution.Count;
                numberOfCherriesPDAP = (double)cherryPDADiffDistribution.Count(x => Math.Abs(x) >= cherryDiffPDA) / cherryPDADiffDistribution.Count;

                numberOfCherriesYuleNormalP = (1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, cherryDiffYule)) * 2;
                numberOfCherriesPDANormalP = (1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, cherryDiffPDA)) * 2;

                numberOfCherriesDistributionPlotGuid = Guid.NewGuid().ToString();
                numberOfCherriesDistribution = Stats.NumberOfCherriesDistribution.GetPlotTwoTailed(cherryYuleDistribution, cherryPdaDistribution, cherryMinValue, cherryMaxValue, numberOfCherriesYule1 - numberOfCherriesYule2, numberOfCherriesPDA1 - numberOfCherriesPDA2, numberOfCherriesYuleP, numberOfCherriesPDAP, numberOfCherriesDistributionPlotGuid);
                Plots.Add(numberOfCherriesDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                {
                    clickActions = null;
                    return TreeViewer.Stats.Distribution.GetPlotWithStandardNormalTwoTailed(cherryYuleDistribution, cherryPdaDistribution, cherryMinValue, cherryMaxValue, numberOfCherriesYule1 - numberOfCherriesYule2, numberOfCherriesPDA1 - numberOfCherriesPDA2, numberOfCherriesYuleP, numberOfCherriesPDAP, "Density (YHK model)", "Density (PDA model)", "Density (limit distribution)", "Normalised cherry index difference", "Cherry index difference distribution", "ΔK", null, out descriptions, interactive);
                });

                {
                    IEnumerable<double[]> getData()
                    {
                        for (int i = 0; i < cherryYuleDistribution.Length; i++)
                        {
                            if (i == 0)
                            {
                                yield return new double[] { cherryMinValue + (cherryMaxValue - cherryMinValue) / 99 * i, cherryYuleDistribution[i], cherryPdaDistribution[i], numberOfCherriesYule1 - numberOfCherriesYule2, numberOfCherriesPDA1 - numberOfCherriesPDA2 };
                            }
                            else
                            {
                                yield return new double[] { cherryMinValue + (cherryMaxValue - cherryMinValue) / 99 * i, cherryYuleDistribution[i], cherryPdaDistribution[i] };
                            }
                        }
                    }

                    Data.Add(numberOfCherriesDistributionPlotGuid, () => ("x\tDensity (YHK model)\tDensity (PDA model)\tObserved value (YHK model)\tObserved value (PDA model)", getData()));
                }
            }


            int figNum = 1;

            StringBuilder markdownSourceBuilder = new StringBuilder();

            markdownSourceBuilder.AppendLine("# Comparison of two trees");
            markdownSourceBuilder.AppendLine();

            markdownSourceBuilder.AppendLine("## General information");
            markdownSourceBuilder.AppendLine();

            if (rooted1 == rooted2)
            {
                markdownSourceBuilder.Append("Both trees are **");
                markdownSourceBuilder.Append(rooted1 ? "rooted" : "unrooted");
                markdownSourceBuilder.Append("**. ");
            }
            else
            {
                markdownSourceBuilder.Append("The first tree is **");
                markdownSourceBuilder.Append(rooted1 ? "rooted" : "unrooted");
                markdownSourceBuilder.Append("**, while the second tree is **");
                markdownSourceBuilder.Append(rooted2 ? "rooted" : "unrooted");
                markdownSourceBuilder.Append("**. ");
            }

            if (nodes1.Count == nodes1.Count)
            {
                markdownSourceBuilder.Append("Both trees contain **");
                markdownSourceBuilder.Append(nodes1.Count.ToString());
                markdownSourceBuilder.Append(" nodes**. ");
            }
            else
            {
                markdownSourceBuilder.Append("The first tree contains **");
                markdownSourceBuilder.Append(nodes1.Count.ToString());
                markdownSourceBuilder.Append(" nodes**, while the second tree contains **");
                markdownSourceBuilder.Append(nodes2.Count.ToString());
                markdownSourceBuilder.Append(" nodes**. ");
            }

            if (leafNames1.Count == leafNames2.Count)
            {
                markdownSourceBuilder.Append("Both trees contain **");
                markdownSourceBuilder.Append(leafNames1.Count.ToString());
                markdownSourceBuilder.Append(" leaves**.");
            }
            else
            {
                markdownSourceBuilder.Append("The first tree contains **");
                markdownSourceBuilder.Append(leafNames1.Count.ToString());
                markdownSourceBuilder.Append(" leaves**, while the second tree contains **");
                markdownSourceBuilder.Append(leafNames2.Count.ToString());
                markdownSourceBuilder.Append(" leaves**.");
            }

            if (hasLengths1 && hasLengths2)
            {
                markdownSourceBuilder.Append(" Both trees have branch lengths.");
            }
            else if (!hasLengths1 && !hasLengths2)
            {
                markdownSourceBuilder.Append(" Neither tree has branch lengths.");
            }
            else if (hasLengths1 && !hasLengths2)
            {
                markdownSourceBuilder.Append(" The first tree has branch lengths, while the second tree does not.");
            }
            else if (!hasLengths1 && hasLengths2)
            {
                markdownSourceBuilder.Append(" The first tree does not have branch lengths, while the second tree has them.");
            }

            if (sameLeaves)
            {
                markdownSourceBuilder.AppendLine(" The trees contain the same leaves.");
                markdownSourceBuilder.AppendLine();
            }
            else
            {
                markdownSourceBuilder.Append(" ");
                markdownSourceBuilder.Append(intersection.Count.ToString());
                markdownSourceBuilder.Append(" leaves are shared between the two trees; **");
                markdownSourceBuilder.Append((leafNames1.Count - intersection.Count).ToString());
                markdownSourceBuilder.Append(" leaves** are present only in the first tree, while **");
                markdownSourceBuilder.Append((leafNames2.Count - intersection.Count).ToString());
                markdownSourceBuilder.AppendLine("** are present only in the second tree. These are summarised in the following table:");
                markdownSourceBuilder.AppendLine();

                int maxLength = (from el in union select el.Length + 3).Max();
                maxLength = Math.Max(maxLength, 19);

                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.AppendLine("+");


                markdownSourceBuilder.Append("| In both trees");
                markdownSourceBuilder.Append(new String(' ', maxLength - 13));
                markdownSourceBuilder.Append(" | Only in first tree");
                markdownSourceBuilder.Append(new String(' ', maxLength - 18));
                markdownSourceBuilder.Append(" | Only in second tree");
                markdownSourceBuilder.Append(new String(' ', maxLength - 19));
                markdownSourceBuilder.AppendLine(" |");

                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('=', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('=', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('=', maxLength + 2));
                markdownSourceBuilder.AppendLine("+");

                for (int i = 0; i < Math.Max(Math.Max(diff1.Count, diff2.Count), intersection.Count); i++)
                {
                    markdownSourceBuilder.Append("| ");

                    if (i < intersection.Count)
                    {
                        markdownSourceBuilder.Append("`");
                        markdownSourceBuilder.Append(intersection[i]);
                        if (i < intersection.Count - 1)
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        markdownSourceBuilder.Append(' ', maxLength - 3 - intersection[i].Length);
                    }
                    else
                    {
                        markdownSourceBuilder.Append(' ', maxLength);
                    }

                    markdownSourceBuilder.Append(" | ");

                    if (i < diff1.Count)
                    {
                        markdownSourceBuilder.Append("`");
                        markdownSourceBuilder.Append(diff1[i]);
                        if (i < diff1.Count - 1)
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        markdownSourceBuilder.Append(' ', maxLength - 3 - diff1[i].Length);
                    }
                    else
                    {
                        markdownSourceBuilder.Append(' ', maxLength);
                    }

                    markdownSourceBuilder.Append(" | ");

                    if (i < diff2.Count)
                    {
                        markdownSourceBuilder.Append("`");
                        markdownSourceBuilder.Append(diff2[i]);
                        if (i < diff2.Count - 1)
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("` ");
                        }
                        markdownSourceBuilder.Append(' ', maxLength - 3 - diff2[i].Length);
                    }
                    else
                    {
                        markdownSourceBuilder.Append(' ', maxLength);
                    }

                    markdownSourceBuilder.AppendLine(" |");
                }



                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.Append("+");
                markdownSourceBuilder.Append(new String('-', maxLength + 2));
                markdownSourceBuilder.AppendLine("+");

                markdownSourceBuilder.AppendLine();

                if (intersection.Count > 3)
                {
                    markdownSourceBuilder.AppendLine("The following analyses are based on the subset of leaves shared between the two trees.");
                    markdownSourceBuilder.AppendLine();
                }
            }


            if (sameTopology)
            {
                if (hasLengths1 && hasLengths2)
                {
                    if (elDistance == 0)
                    {
                        markdownSourceBuilder.AppendLine("The trees have the same topology and branch lengths.");
                        markdownSourceBuilder.AppendLine();
                    }
                    else
                    {
                        markdownSourceBuilder.Append("The trees have the same topology. The edge-length distance between them is **");
                        markdownSourceBuilder.Append(elDistance.ToString(elDistance.GetDigits()));
                        markdownSourceBuilder.AppendLine("**.");
                        markdownSourceBuilder.AppendLine();
                    }
                }
                else
                {
                    markdownSourceBuilder.AppendLine("The trees have the same topology.");
                    markdownSourceBuilder.AppendLine();
                }
            }
            else
            {
                markdownSourceBuilder.Append("The trees have different topologies. The Robinson-Foulds distance between them is **");
                markdownSourceBuilder.Append(RFdistance.ToString());
                markdownSourceBuilder.Append("**. ");

                if (hasLengths1 && hasLengths2)
                {
                    markdownSourceBuilder.Append("The weighted Robinson-Foulds distance between them is **");
                    markdownSourceBuilder.Append(wRFDistance.ToString(wRFDistance.GetDigits()));
                    markdownSourceBuilder.Append("**. ");
                }

                markdownSourceBuilder.Append("There are **");
                markdownSourceBuilder.Append(common.ToString());
                markdownSourceBuilder.Append(" splits in common** between the two trees. There are **");
                markdownSourceBuilder.Append(in1NotIn2.ToString());
                markdownSourceBuilder.Append(" splits** that only appear in the first tree, and **");
                markdownSourceBuilder.Append(in2NotIn1.ToString());
                markdownSourceBuilder.AppendLine(" splits** that only appear in the second tree.");
                markdownSourceBuilder.AppendLine();
            }

            if (hasLengths1 && hasLengths2 && splitLengthDiffs.Count > 1 && splitLengthDiffs.Any(x => x != 0))
            {
                markdownSourceBuilder.Append("Considering the ");
                markdownSourceBuilder.Append(common.ToString());
                markdownSourceBuilder.Append(" splits in common between the two trees, the average difference in length between a split from the first tree and the corresponding split from the second tree is **");
                markdownSourceBuilder.Append(averageSplitLengthDiff.ToString(averageSplitLengthDiff.GetDigits()));
                markdownSourceBuilder.Append("** (89% highest-density interval: ");
                markdownSourceBuilder.Append(splitLengthDiffHDI[0].ToString(splitLengthDiffHDI[0].GetDigits()));
                markdownSourceBuilder.Append(" &#8212; ");
                markdownSourceBuilder.Append(splitLengthDiffHDI[1].ToString(splitLengthDiffHDI[1].GetDigits()));
                markdownSourceBuilder.Append("). ");


                markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of the difference in length between splits from the first tree and the corresponding splits from the second tree.");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                markdownSourceBuilder.Append(GetSVGBase64(splitLengthDiffDistribution));

                markdownSourceBuilder.AppendLine("\"></p>");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of split length differences.** The histogram shows the distribution of the difference in length between splits from the first tree and the corresponding splits from the second tree.");

                if (splitLengthDiffOverflowCount > 0 && splitLengthDiffUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(splitLengthDiffUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(splitLengthDiffUnderflow.ToString(splitLengthDiffUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                    markdownSourceBuilder.Append(splitLengthDiffOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(splitLengthDiffOverflow.ToString(splitLengthDiffOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (splitLengthDiffOverflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(splitLengthDiffOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(splitLengthDiffOverflow.ToString(splitLengthDiffOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (splitLengthDiffUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(splitLengthDiffUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(splitLengthDiffUnderflow.ToString(splitLengthDiffUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin.");
                }

                markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median branch length, the interquartile range, and the 89% HDI.");

                markdownSourceBuilder.AppendLine();

                figNum++;
            }

            if ((hasLengths1 && hasLengths2 && splitLengthDiffs.Count > 1 && splitLengthDiffs.Any(x => x != 0)) || !sameLeaves)
            {
                markdownSourceBuilder.AppendLine("<br type=\"page\"/>");
                markdownSourceBuilder.AppendLine();
            }

            markdownSourceBuilder.AppendLine("## Tree shape statistics");
            markdownSourceBuilder.AppendLine();

            if (sameTopology)
            {
                if (rooted1 && rooted2)
                {
                    markdownSourceBuilder.Append("The **Sackin index** of the trees is **");
                    markdownSourceBuilder.Append(sackinIndex1.ToString());

                    if (!binary1 || !binary2)
                    {
                        markdownSourceBuilder.AppendLine("**.");
                        markdownSourceBuilder.AppendLine("");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(sackinYule1.ToString(sackinYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(sackinPDA1.ToString(sackinPDA1.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (sackinYuleP > 0.95)
                        {
                            markdownSourceBuilder.Append("According to the Sackin index, the trees are significantly more balanced than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - sackinYuleP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (sackinYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("According to the Sackin index, the trees are significantly less balanced than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append(sackinYuleP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("According to the Sackin index, the balancing of the trees is not significantly different than expected under the YHK model. ");
                        }

                        if (sackinPDAP > 0.95)
                        {
                            markdownSourceBuilder.Append("The trees are significantly more balanced than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - sackinPDAP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (sackinPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The trees are significantly less balanced than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append(sackinPDAP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The balancing of the trees is not significantly different than expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Sackin index values under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(sackinDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Sackin index values.** The curve shows the expected distribution of (normalised) Sackin index values under the null hypothesis for a tree containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                            " The dashed line represents the observed value of the Sackin index, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;

                        markdownSourceBuilder.Append("The **Colless index** of the trees is **");
                        markdownSourceBuilder.Append(collessIndex1.ToString());
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(collessYule1.ToString(collessYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(collessPDA1.ToString(collessPDA1.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (collessYuleP > 0.95)
                        {
                            markdownSourceBuilder.Append("According to the Colless index, the trees are significantly more balanced than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - collessYuleP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (collessYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("According to the Colless index, the trees are significantly less balanced than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append(collessYuleP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("According to the Colless index, the balancing of the trees is not significantly different than expected under the YHK model. ");
                        }

                        if (collessPDAP > 0.95)
                        {
                            markdownSourceBuilder.Append("The trees are significantly more balanced than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - collessPDAP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (collessPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The trees are significantly less balanced than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append(collessPDAP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The balancing of the trees is not significantly different than expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Colless index values under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(collessDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Colless index values.** The curve shows the expected distribution of (normalised) Colless index values under the null hypothesis for a tree containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                            " The dashed line represents the observed value of the Colless index, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;
                    }
                }


                if (intersection.Count >= 6)
                {
                    markdownSourceBuilder.Append("The trees have **");
                    markdownSourceBuilder.Append(numOfCherries1.ToString());

                    if (!binary1 || !binary2)
                    {
                        markdownSourceBuilder.AppendLine("** cherries.");
                        markdownSourceBuilder.AppendLine();
                    }
                    else
                    {
                        markdownSourceBuilder.Append("** cherries (YHK model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesYule1.ToString(numberOfCherriesYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesPDA1.ToString(numberOfCherriesPDA1.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (numberOfCherriesYuleP > 0.95 || numberOfCherriesYuleNormalP > 0.95)
                        {
                            markdownSourceBuilder.Append("The number of cherries is significantly lower than expected under the YHK model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesYuleP).ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesYuleNormalP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (numberOfCherriesYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("The number of cherries is significantly higher than expected under the YHK model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesYuleP.ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesYuleNormalP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The number of cherries does not differ significantly from the number expected under the YHK model. ");
                        }

                        if (numberOfCherriesPDAP > 0.95 || numberOfCherriesPDANormalP > 0.95)
                        {
                            markdownSourceBuilder.Append("The number of cherries is significantly lower than expected under the PDA model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesPDAP).ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesPDANormalP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (numberOfCherriesPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The number of cherries is significantly higher than expected under the PDA model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesPDAP.ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesPDANormalP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The number of cherries does not differ significantly from the number expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of the number of cherries under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(numberOfCherriesDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of the number of cherries.** The curve shows the expected distribution of the (normalised) number of cherries under the null hypothesis for a tree containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                            " The dashed line represents the observed value of the number of cherries, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model. The green curve represents the limit distribution for the (normalised) number of cherries as the number of tips grows to infinity.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;
                    }
                }

            }
            else
            {
                if (rooted1 && rooted2)
                {
                    if (!binary1 || !binary2)
                    {
                        markdownSourceBuilder.Append("The **Sackin indices** of the trees are **");
                        markdownSourceBuilder.Append(sackinIndex1.ToString());
                        markdownSourceBuilder.Append("** and **");
                        markdownSourceBuilder.Append(sackinIndex2.ToString());
                        markdownSourceBuilder.AppendLine("**.");
                        markdownSourceBuilder.AppendLine("");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("The **Sackin indices** of the trees are **");
                        markdownSourceBuilder.Append(sackinIndex1.ToString());
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(sackinYule1.ToString(sackinYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(sackinPDA1.ToString(sackinPDA1.GetDigits()));
                        markdownSourceBuilder.Append(") and **");
                        markdownSourceBuilder.Append(sackinIndex2.ToString());
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(sackinYule2.ToString(sackinYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(sackinPDA2.ToString(sackinPDA1.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (sackinYuleP > 0.95)
                        {
                            markdownSourceBuilder.Append("The Sackin index difference of the trees is significantly smaller than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - sackinYuleP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (sackinYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("The Sackin index difference of the trees is significantly greater than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append(sackinYuleP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the Sackin indices of the trees is not significantly different than expected under the YHK model. ");
                        }

                        if (sackinPDAP > 0.95)
                        {
                            markdownSourceBuilder.Append("The Sackin index difference of the trees is significantly smaller than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - sackinPDAP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (sackinPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The Sackin index difference of the trees is significantly greater than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append(sackinPDAP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the Sackin indices of the trees is not significantly different than expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Sackin index differences under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(sackinDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Sackin index differences.** The curve shows the expected distribution of (normalised) Sackin index differences under the null hypothesis for two trees containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model, and all possible pairings between them were considered)." +
                            " The dashed line represents the observed value of the Sackin index difference, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;

                        markdownSourceBuilder.Append("The **Colless indices** of the trees are **");
                        markdownSourceBuilder.Append(collessIndex1.ToString());
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(collessYule1.ToString(collessYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(collessPDA1.ToString(collessPDA1.GetDigits()));
                        markdownSourceBuilder.Append(") and **");
                        markdownSourceBuilder.Append(collessIndex2.ToString());
                        markdownSourceBuilder.Append("** (YHK model normalisation: ");
                        markdownSourceBuilder.Append(collessYule2.ToString(collessYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(collessPDA2.ToString(collessPDA1.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (collessYuleP > 0.95)
                        {
                            markdownSourceBuilder.Append("The Colless index difference of the trees is significantly smaller than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - collessYuleP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (collessYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("The Colless index difference of the trees is significantly greater than expected under the YHK model (p &asymp; ");
                            markdownSourceBuilder.Append(collessYuleP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the Colless indices of the trees is not significantly different than expected under the YHK model. ");
                        }

                        if (collessPDAP > 0.95)
                        {
                            markdownSourceBuilder.Append("The Colless index difference of the trees is significantly smaller than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append((1 - collessPDAP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (collessPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The Colless index difference of the trees is significantly greater than expected under the PDA model (p &asymp; ");
                            markdownSourceBuilder.Append(collessPDAP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the Colless indices of the trees is not significantly different than expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Colless index differences under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(collessDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Colless index differences.** The curve shows the expected distribution of (normalised) Colless index differences under the null hypothesis for two trees containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model, and all possible pairings between them were considered)." +
                            " The dashed line represents the observed value of the Colless index difference, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;
                    }
                }

                if (intersection.Count >= 6)
                {
                    if (!binary1 || !binary2)
                    {
                        markdownSourceBuilder.Append("The trees have **");
                        markdownSourceBuilder.Append(numOfCherries1.ToString());
                        markdownSourceBuilder.AppendLine("** and **");
                        markdownSourceBuilder.Append(numOfCherries2.ToString());
                        markdownSourceBuilder.AppendLine("** cherries.");
                        markdownSourceBuilder.AppendLine();
                    }
                    else
                    {
                        markdownSourceBuilder.Append("The first tree has **");
                        markdownSourceBuilder.Append(numOfCherries1.ToString());
                        markdownSourceBuilder.Append("** cherries (YHK model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesYule1.ToString(numberOfCherriesYule1.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesPDA1.ToString(numberOfCherriesPDA1.GetDigits()));
                        markdownSourceBuilder.Append("), while the second tree has **");
                        markdownSourceBuilder.Append(numOfCherries2.ToString());
                        markdownSourceBuilder.Append("** cherries (YHK model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesYule2.ToString(numberOfCherriesYule2.GetDigits()));
                        markdownSourceBuilder.Append(", PDA model normalisation: ");
                        markdownSourceBuilder.Append(numberOfCherriesPDA2.ToString(numberOfCherriesPDA2.GetDigits()));
                        markdownSourceBuilder.Append("). ");

                        if (numberOfCherriesYuleP > 0.95 || numberOfCherriesYuleNormalP > 0.95)
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries is significantly lower than expected under the YHK model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesYuleP).ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesYuleNormalP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (numberOfCherriesYuleP < 0.05)
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries is significantly higher than expected under the YHK model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesYuleP.ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesYuleNormalP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries does not differ significantly from the value expected under the YHK model. ");
                        }

                        if (numberOfCherriesPDAP > 0.95 || numberOfCherriesPDANormalP > 0.95)
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries is significantly lower than expected under the PDA model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesPDAP).ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append((1 - numberOfCherriesPDANormalP).ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else if (numberOfCherriesPDAP < 0.05)
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries is significantly higher than expected under the PDA model (MC approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesPDAP.ToString(1, false));
                            markdownSourceBuilder.Append(", Gaussian approximation: p &asymp; ");
                            markdownSourceBuilder.Append(numberOfCherriesPDANormalP.ToString(1, false));
                            markdownSourceBuilder.Append("). ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("The difference between the numbers of cherries does not differ significantly from the number expected under the PDA model. ");
                        }

                        markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of the difference between the numbers of cherries under the YHK and PDA models.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" ></a><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(numberOfCherriesDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of the difference between the numbers of cherries.** The curve shows the expected distribution of the (normalised) difference between the numbers of cherries under the null hypothesis for two trees containing " + intersection.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model, and all possible pairings between them were considered)." +
                            " The dashed line represents the observed value of the difference between the numbers of cherries, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model. The green curve represents the limit distribution for the (normalised) difference between the numbers of cherries as the number of tips grows to infinity.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;
                    }
                }
            }


            string markdownSource = markdownSourceBuilder.ToString();

            Markdig.Syntax.MarkdownDocument markdownDocument = Markdig.Markdown.Parse(markdownSource, new Markdig.MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().Build());

            return (markdownDocument, markdownSource);
        }
    }
}
