using Markdig;
using MathNet.Numerics.Statistics;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VectSharp;
using VectSharp.SVG;
using static TreeViewer.Stats.TreeReport;

namespace TreeViewer.Stats
{
    internal static class TreeReport
    {
        public delegate Page GetPlot(bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions);
        public static string GetSVGBase64(Page pag)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                pag.SaveAsSVG(ms, SVGContextInterpreter.TextOptions.DoNotEmbed);

                ms.Position = 0;

                using (StreamReader sr = new StreamReader(ms))
                {
                    string svg = sr.ReadToEnd();

                    return "data:image/svg+xml;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(svg));
                }
            }
        }

        public static string GetPNGBase64(Stream pngStream)
        {
            byte[] data = new byte[pngStream.Length];

            byte[] buffer = new byte[8192];
            int read = 0;

            while (read < pngStream.Length)
            {
                int readNow = pngStream.Read(buffer, read, buffer.Length);

                for (int i = 0; i < readNow; i++)
                {
                    data[read + i] = buffer[i];
                }

                read += readNow;
            }

            return "data:image/png;base64," + Convert.ToBase64String(data);
        }

    }

    internal static class SingleTreeReport
    {

        public static (Markdig.Syntax.MarkdownDocument report, string reportSource) CreateReport(TreeNode tree, bool isFinalTransformed, Action<string, double> progressAction, Dictionary<string, GetPlot> Plots, Dictionary<string, Func<(string header, IEnumerable<double[]>)>> Data)
        {
            List<TreeNode> leaves = tree.GetLeaves();
            List<TreeNode> nodes = tree.GetChildrenRecursive();
            bool rooted = tree.Children.Count <= 2 && nodes.Count > 1;
            bool binary = leaves.Count > 1;

            List<double> branchLengths = new List<double>();

            double averageChildren = 0;
            int averageChildrenCount = 0;

            for (int i = 0; i < nodes.Count; i++)
            {
                if (!double.IsNaN(nodes[i].Length))
                {
                    branchLengths.Add(nodes[i].Length);
                }

                if (nodes[i].Children.Count > 0 && nodes[i].Parent != null)
                {
                    averageChildren += nodes[i].Children.Count;
                    averageChildrenCount++;
                }

                if (nodes[i].Children.Count > 2 && nodes[i].Parent != null)
                {
                    binary = false;
                }
            }

            averageChildren /= averageChildrenCount;

            bool hasLengths = branchLengths.Count >= nodes.Count - 1 && branchLengths.Count > 1;

            double totalLength = hasLengths ? branchLengths.Sum() : double.NaN;
            double minHeight = hasLengths ? tree.ShortestDownstreamLength() : double.NaN;
            double maxHeight = hasLengths ? tree.LongestDownstreamLength() : double.NaN;

            bool isClockLike = hasLengths && Math.Abs(minHeight - maxHeight) / (minHeight + maxHeight) < 0.02;

            double rootAge = (minHeight + maxHeight) * 0.5;
            double minBranchLength = hasLengths ? branchLengths.Min() : double.NaN;
            double maxBranchLength = hasLengths ? branchLengths.Max() : double.NaN;
            double avgBranchLength = hasLengths ? branchLengths.Average() : double.NaN;
            double medianBranchLength = hasLengths ? branchLengths.Median() : double.NaN;
            double[] branchLength89HDI = hasLengths ? BayesStats.HighestDensityInterval(branchLengths, 0.89) : null;
            double[] branchLength95HDI = hasLengths ? BayesStats.HighestDensityInterval(branchLengths, 0.95) : null;

            string branchLengthDistributionPlotGuid = Guid.NewGuid().ToString();
            Page branchLengthDistribution = hasLengths ? Stats.BranchLengthDistribution.GetPlot(branchLengths, branchLengthDistributionPlotGuid) : null;
            (double branchLengthUnderflow, int branchLengthUnderflowCount, double branchLengthOverflow, int branchLengthOverflowCount) = hasLengths ? Stats.Histogram.GetOverUnderflow(branchLengths) : (0, 0, 0, 0);

            Plots.Add(branchLengthDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
            {
                clickActions = null;
                return TreeViewer.Stats.Histogram.GetPlot(branchLengths, "Branch length", "Branch length distribution", null, out descriptions, interactive);
            });

            {
                IEnumerable<double[]> getData()
                {
                    foreach (double d in branchLengths)
                    {
                        yield return new double[] { d };
                    }
                }

                Data.Add(branchLengthDistributionPlotGuid, () => ("Branch length", getData()));
            }

            double averageLeafDepth = double.NaN;
            double leafDepthVariance = double.NaN;
            int sackinIndex = 0;
            double sackinYule = double.NaN;
            double sackinPDA = double.NaN;
            double sackinYuleP = double.NaN;
            double sackinPDAP = double.NaN;
            string leafDepthDistributionPlotGuid = null;
            Page leafDepthDistribution = null;
            (double leafDepthUnderflow, int leafDepthUnderflowCount, double leafDepthOverflow, int leafDepthOverflowCount) = (0, 0, 0, 0);

            string sackinDistributionPlotGuid = null;
            Page sackinDistribution = null;

            double averageLeafHeight = double.NaN;
            double leafHeightVariance = double.NaN;
            string leafHeightDistributionPlotGuid = null;
            Page leafHeightDistribution = null;
            (double leafHeightUnderflow, int leafHeightUnderflowCount, double leafHeightOverflow, int leafHeightOverflowCount) = (0, 0, 0, 0);


            int collessIndex = 0;
            double collessYule = double.NaN;
            double collessPDA = double.NaN;
            double collessYuleP = double.NaN;
            double collessPDAP = double.NaN;

            string collessDistributionPlotGuid = null;
            Page collessDistribution = null;

            double[] cherryYuleDistribution = null;
            double[] cherryPdaDistribution = null;


            double[] sackinYuleDistribution = null;
            double[] collessYuleDistribution = null;

            double[] sackinPdaDistribution = null;
            double[] collessPdaDistribution = null;

            if (binary)
            {
                (sackinYuleDistribution, collessYuleDistribution, cherryYuleDistribution) = ShapeIndices.GetDistribution(leaves.Count, TreeNode.NullHypothesis.YHK, rooted, rooted, rooted && binary, progress =>
                {
                    progressAction(null, 0.5 * progress);
                });

                progressAction("Sampling trees under the PDA model", 0.5);

                (sackinPdaDistribution, collessPdaDistribution, cherryPdaDistribution) = ShapeIndices.GetDistribution(leaves.Count, TreeNode.NullHypothesis.PDA, rooted, rooted, rooted && binary, progress =>
                {
                    progressAction(null, 0.5 * progress + 0.5);
                });

                progressAction(null, 1);
            }

            if (rooted)
            {
                List<double> leafDepths = new List<double>(leaves.Count);
                List<double> leafHeights = new List<double>(leaves.Count);

                for (int i = 0; i < leaves.Count; i++)
                {
                    leafDepths.Add(leaves[i].GetDepth());

                    if (hasLengths)
                    {
                        leafHeights.Add(leaves[i].UpstreamLength());
                    }
                }

                averageLeafDepth = leafDepths.Average();
                leafDepthVariance = leafDepths.Variance();

                leafDepthDistributionPlotGuid = Guid.NewGuid().ToString();
                leafDepthDistribution = Stats.LeafDepthDistribution.GetPlot(leafDepths, leafDepthDistributionPlotGuid);
                (leafDepthUnderflow, leafDepthUnderflowCount, leafDepthOverflow, leafDepthOverflowCount) = Stats.Histogram.GetOverUnderflow(leafDepths);

                Plots.Add(leafDepthDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                {
                    clickActions = null;
                    return TreeViewer.Stats.Histogram.GetPlot(leafDepths, "Leaf depth", "Leaf depth distribution", null, out descriptions, interactive);
                });


                {
                    IEnumerable<double[]> getData()
                    {
                        foreach (double d in leafDepths)
                        {
                            yield return new double[] { d };
                        }
                    }

                    Data.Add(leafDepthDistributionPlotGuid, () => ("Leaf depth", getData()));
                }

                sackinIndex = (int)leafDepths.Sum();

                if (binary)
                {
                    sackinYule = (sackinIndex - 2 * leaves.Count * (from el in Enumerable.Range(2, leaves.Count - 1) select 1.0 / el).Sum()) / leaves.Count;
                    sackinPDA = sackinIndex / Math.Pow(leaves.Count, 1.5);

                    sackinYuleP = (double)sackinYuleDistribution.Count(x => x >= sackinYule) / sackinYuleDistribution.Length;
                    sackinPDAP = (double)sackinPdaDistribution.Count(x => x >= sackinPDA) / sackinPdaDistribution.Length;

                    sackinDistributionPlotGuid = Guid.NewGuid().ToString();
                    sackinDistribution = Stats.SackinDistribution.GetPlot(sackinYuleDistribution, sackinPdaDistribution, sackinYule, sackinPDA, sackinYuleP, sackinPDAP, sackinDistributionPlotGuid);
                    Plots.Add(sackinDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                    {
                        clickActions = null;
                        return TreeViewer.Stats.Distribution.GetPlot(sackinYuleDistribution, sackinPdaDistribution, sackinYule, sackinPDA, sackinYuleP, sackinPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Sackin index", "Sackin index distribution", "S", null, out descriptions, interactive);
                    });

                    {
                        IEnumerable<double[]> getData()
                        {
                            for (int i = 0; i < sackinYuleDistribution.Length; i++)
                            {
                                if (i == 0)
                                {
                                    yield return new double[] { sackinYuleDistribution[i], sackinPdaDistribution[i], sackinYule, sackinPDA };
                                }
                                else
                                {
                                    yield return new double[] { sackinYuleDistribution[i], sackinPdaDistribution[i] };
                                }
                            }
                        }

                        Data.Add(sackinDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value(PDA model)", getData()));
                    }

                    collessIndex = (int)tree.CollessIndex(TreeNode.NullHypothesis.None);
                    collessYule = (collessIndex - TreeNode.GetCollessExpectationYHK(leaves.Count)) / leaves.Count;
                    collessPDA = collessIndex / Math.Pow(leaves.Count, 1.5);

                    collessYuleP = (double)collessYuleDistribution.Count(x => x >= collessYule) / collessYuleDistribution.Length;
                    collessPDAP = (double)collessPdaDistribution.Count(x => x >= collessPDA) / collessPdaDistribution.Length;


                    collessDistributionPlotGuid = Guid.NewGuid().ToString();
                    collessDistribution = Stats.CollessDistribution.GetPlot(collessYuleDistribution, collessPdaDistribution, collessYule, collessPDA, collessYuleP, collessPDAP, collessDistributionPlotGuid);
                    Plots.Add(collessDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                    {
                        clickActions = null;
                        return TreeViewer.Stats.Distribution.GetPlot(collessYuleDistribution, collessPdaDistribution, collessYule, collessPDA, collessYuleP, collessPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Colless index", "Colless index distribution", "C", null, out descriptions, interactive);
                    });

                    {
                        IEnumerable<double[]> getData()
                        {
                            for (int i = 0; i < collessYuleDistribution.Length; i++)
                            {
                                if (i == 0)
                                {
                                    yield return new double[] { collessYuleDistribution[i], collessPdaDistribution[i], collessYule, collessPDA };
                                }
                                else
                                {
                                    yield return new double[] { collessYuleDistribution[i], collessPdaDistribution[i] };
                                }
                            }
                        }

                        Data.Add(collessDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value(PDA model)", getData()));
                    }
                }

                if (hasLengths && !isClockLike)
                {
                    averageLeafHeight = leafHeights.Average();
                    leafHeightVariance = leafHeights.Variance();


                    leafHeightDistributionPlotGuid = Guid.NewGuid().ToString();
                    leafHeightDistribution = Stats.LeafHeightDistribution.GetPlot(leafHeights, leafHeightDistributionPlotGuid);
                    (leafHeightUnderflow, leafHeightUnderflowCount, leafHeightOverflow, leafHeightOverflowCount) = Stats.Histogram.GetOverUnderflow(leafHeights);

                    Plots.Add(leafHeightDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                    {
                        clickActions = null;
                        return TreeViewer.Stats.Histogram.GetPlot(leafHeights, "Leaf height", "Leaf height distribution", null, out descriptions, interactive);
                    });


                    {
                        IEnumerable<double[]> getData()
                        {
                            foreach (double d in leafHeights)
                            {
                                yield return new double[] { d };
                            }
                        }

                        Data.Add(leafHeightDistributionPlotGuid, () => ("Leaf height", getData()));
                    }

                }
            }

            int numberOfCherries = (int)tree.NumberOfCherries(TreeNode.NullHypothesis.None);
            double numberOfCherriesYule = (numberOfCherries - leaves.Count / 3.0) / Math.Sqrt(2.0 * leaves.Count / 45.0);

            double numberOfCherriesPDA;

            {
                double mu = (double)leaves.Count * (leaves.Count - 1) / (2.0 * (2 * leaves.Count - 5));
                double sigmaSq = (double)leaves.Count * (leaves.Count - 1) * (leaves.Count - 4) * (leaves.Count - 5) / (2.0 * (2 * leaves.Count - 5) * (2 * leaves.Count - 5) * (2 * leaves.Count - 7));
                numberOfCherriesPDA = (numberOfCherries - mu) / Math.Sqrt(sigmaSq);
            }

            string numberOfCherriesDistributionPlotGuid = null;
            Page numberOfCherriesDistribution = null;

            double numberOfCherriesYuleP = double.NaN;
            double numberOfCherriesPDAP = double.NaN;

            double numberOfCherriesYuleNormalP = 1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, numberOfCherriesYule);
            double numberOfCherriesPDANormalP = 1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, numberOfCherriesPDA);

            if (binary && leaves.Count >= 6)
            {
                numberOfCherriesYuleP = (double)cherryYuleDistribution.Count(x => x >= numberOfCherriesYule) / cherryYuleDistribution.Length;
                numberOfCherriesPDAP = (double)cherryPdaDistribution.Count(x => x >= numberOfCherriesPDA) / cherryPdaDistribution.Length;

                numberOfCherriesDistributionPlotGuid = Guid.NewGuid().ToString();
                numberOfCherriesDistribution = Stats.NumberOfCherriesDistribution.GetPlot(cherryYuleDistribution, cherryPdaDistribution, numberOfCherriesYule, numberOfCherriesPDA, numberOfCherriesYuleP, numberOfCherriesPDAP, numberOfCherriesDistributionPlotGuid);
                Plots.Add(numberOfCherriesDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                {
                    clickActions = null;
                    return TreeViewer.Stats.Distribution.GetPlotWithStandardNormal(cherryYuleDistribution, cherryPdaDistribution, numberOfCherriesYule, numberOfCherriesPDA, numberOfCherriesYuleP, numberOfCherriesPDAP, "Density (YHK model)", "Density (PDA model)", "Density (limit distribution)", "Normalised number of cherries", "Distribution of the number of cherries", "K", null, out descriptions, interactive);
                });

                {
                    IEnumerable<double[]> getData()
                    {
                        for (int i = 0; i < cherryYuleDistribution.Length; i++)
                        {
                            if (i == 0)
                            {
                                yield return new double[] { cherryYuleDistribution[i], cherryPdaDistribution[i], numberOfCherriesYule, numberOfCherriesPDA };
                            }
                            else
                            {
                                yield return new double[] { cherryYuleDistribution[i], cherryPdaDistribution[i] };
                            }
                        }
                    }

                    Data.Add(numberOfCherriesDistributionPlotGuid, () => ("Samples from YHK model\tSamples from PDA model\tObserved value (YHK model)\tObserved value(PDA model)", getData()));
                }
            }

            int figNum = 1;

            StringBuilder markdownSourceBuilder = new StringBuilder();

            markdownSourceBuilder.AppendLine("# Tree statistics");
            markdownSourceBuilder.AppendLine();

            markdownSourceBuilder.AppendLine("## General information");
            markdownSourceBuilder.AppendLine();

            if (isFinalTransformed)
            {
                markdownSourceBuilder.Append("The final transformed tree is **");
            }
            else
            {
                markdownSourceBuilder.Append("The tree is **");
            }

            if (rooted)
            {
                markdownSourceBuilder.Append("rooted");
            }
            else
            {
                markdownSourceBuilder.Append("unrooted");
            }

            markdownSourceBuilder.Append("** and has **");
            markdownSourceBuilder.Append(leaves.Count.ToString());
            markdownSourceBuilder.Append(" tips**, with **");
            markdownSourceBuilder.Append(nodes.Count.ToString());
            markdownSourceBuilder.Append(" total nodes**.");
            if (!binary)
            {
                markdownSourceBuilder.Append(" The tree is not fully bifurcating; each internal node has an average of ");
                markdownSourceBuilder.Append(averageChildren.ToString(averageChildren.GetDigits()));
                markdownSourceBuilder.AppendLine(" descendants.");
            }
            else
            {
                markdownSourceBuilder.AppendLine(" The tree is **fully bifurcating**.");
            }

            markdownSourceBuilder.AppendLine();

            if (hasLengths)
            {
                markdownSourceBuilder.Append("The total length of the tree (sum of all branch lengths) is **");

                markdownSourceBuilder.Append(totalLength.ToString(totalLength.GetDigits(), true));

                if (isClockLike)
                {
                    markdownSourceBuilder.Append("**; the tree is clock-like and the age of the root is **");

                    markdownSourceBuilder.Append(rootAge.ToString(rootAge.GetDigits(), true));

                    markdownSourceBuilder.AppendLine("**.");
                    markdownSourceBuilder.AppendLine();
                }
                else
                {
                    markdownSourceBuilder.Append("**; the tree is not clock-like: the minimum height of a leaf is **");

                    markdownSourceBuilder.Append(minHeight.ToString(minHeight.GetDigits(), true));

                    markdownSourceBuilder.Append("**, while the maximum height is **");

                    markdownSourceBuilder.Append(maxHeight.ToString(maxHeight.GetDigits(), true));

                    markdownSourceBuilder.AppendLine("**.");
                    markdownSourceBuilder.AppendLine();
                }

                markdownSourceBuilder.Append("Branch lengths range between ");
                markdownSourceBuilder.Append(minBranchLength.ToString(minBranchLength.GetDigits(), true));
                markdownSourceBuilder.Append(" and ");
                markdownSourceBuilder.Append(maxBranchLength.ToString(maxBranchLength.GetDigits(), true));
                markdownSourceBuilder.Append(", with mean **");
                markdownSourceBuilder.Append(avgBranchLength.ToString(avgBranchLength.GetDigits(), true));
                markdownSourceBuilder.Append("** and median ");
                markdownSourceBuilder.Append(medianBranchLength.ToString(avgBranchLength.GetDigits(), true));
                markdownSourceBuilder.Append(" (89% highest-density interval: **");
                markdownSourceBuilder.Append(branchLength89HDI[0].ToString(branchLength89HDI[0].GetDigits(), true));
                markdownSourceBuilder.Append(" &#8212; ");
                markdownSourceBuilder.Append(branchLength89HDI[1].ToString(branchLength89HDI[1].GetDigits(), true));
                markdownSourceBuilder.Append("**, 95% HDI: ");
                markdownSourceBuilder.Append(branchLength95HDI[0].ToString(branchLength95HDI[0].GetDigits(), true));
                markdownSourceBuilder.Append(" &#8212; ");
                markdownSourceBuilder.Append(branchLength95HDI[1].ToString(branchLength95HDI[1].GetDigits(), true));
                markdownSourceBuilder.AppendLine(").");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of branch lengths in the tree.");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                markdownSourceBuilder.Append(GetSVGBase64(branchLengthDistribution));

                markdownSourceBuilder.AppendLine("\"></p>");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of branch lengths.** The histogram shows the distribution of branch lengths in the tree.");

                if (branchLengthOverflowCount > 0 && branchLengthUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(branchLengthUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(branchLengthUnderflow.ToString(branchLengthUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                    markdownSourceBuilder.Append(branchLengthOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(branchLengthOverflow.ToString(branchLengthOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (branchLengthOverflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(branchLengthOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(branchLengthOverflow.ToString(branchLengthOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (branchLengthUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(branchLengthUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(branchLengthUnderflow.ToString(branchLengthUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin.");
                }

                markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median branch length, the interquartile range, and the 89% HDI.");

                markdownSourceBuilder.AppendLine();

                figNum++;

            }
            else
            {
                markdownSourceBuilder.AppendLine("The tree does not have length information associated to all the nodes.");
                markdownSourceBuilder.AppendLine();
            }

            markdownSourceBuilder.AppendLine();

            markdownSourceBuilder.AppendLine("<br type=\"page\"/>");
            markdownSourceBuilder.AppendLine();

            markdownSourceBuilder.AppendLine("## Tree shape statistics");
            markdownSourceBuilder.AppendLine();

            if (rooted)
            {
                markdownSourceBuilder.Append("The average leaf depth is ");
                markdownSourceBuilder.Append(averageLeafDepth.ToString(averageLeafDepth.GetDigits()));
                markdownSourceBuilder.Append(" and the leaf depth variance is ");
                markdownSourceBuilder.Append(leafDepthVariance.ToString(leafDepthVariance.GetDigits()));
                markdownSourceBuilder.Append(". ");

                markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of leaf depths.");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                markdownSourceBuilder.Append(GetSVGBase64(leafDepthDistribution));

                markdownSourceBuilder.AppendLine("\"></p>");
                markdownSourceBuilder.AppendLine();

                markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of leaf depths.** The histogram shows the distribution of leaf depths in the tree.");

                if (leafDepthOverflowCount > 0 && leafDepthUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(leafDepthUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(leafDepthUnderflow.ToString(leafDepthUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                    markdownSourceBuilder.Append(leafDepthOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(leafDepthOverflow.ToString(leafDepthOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (leafDepthOverflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(leafDepthOverflowCount.ToString());
                    markdownSourceBuilder.Append(" values greater than ");
                    markdownSourceBuilder.Append(leafDepthOverflow.ToString(leafDepthOverflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the overflow bin.");
                }
                else if (leafDepthUnderflowCount > 0)
                {
                    markdownSourceBuilder.Append(" ");
                    markdownSourceBuilder.Append(leafDepthUnderflowCount.ToString());
                    markdownSourceBuilder.Append(" values smaller than ");
                    markdownSourceBuilder.Append(leafDepthUnderflow.ToString(leafDepthUnderflow.GetDigits()));
                    markdownSourceBuilder.Append(" are shown in the underflow bin.");
                }

                markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median leaf depth, the interquartile range, and the 89% HDI.");

                markdownSourceBuilder.AppendLine();

                figNum++;

                if (hasLengths && !isClockLike)
                {
                    markdownSourceBuilder.Append("The average leaf height is ");
                    markdownSourceBuilder.Append(averageLeafHeight.ToString(averageLeafHeight.GetDigits()));
                    markdownSourceBuilder.Append(" and the leaf height variance is ");
                    markdownSourceBuilder.Append(leafHeightVariance.ToString(leafHeightVariance.GetDigits()));
                    markdownSourceBuilder.AppendLine(". [**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of leaf heights.");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(leafHeightDistribution));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of leaf heights.** The histogram shows the distribution of leaf heights in the tree.");

                    if (leafHeightOverflowCount > 0 && leafHeightUnderflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(leafHeightUnderflowCount.ToString());
                        markdownSourceBuilder.Append(" values smaller than ");
                        markdownSourceBuilder.Append(leafHeightUnderflow.ToString(leafHeightUnderflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                        markdownSourceBuilder.Append(leafHeightOverflowCount.ToString());
                        markdownSourceBuilder.Append(" values greater than ");
                        markdownSourceBuilder.Append(leafHeightOverflow.ToString(leafHeightOverflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the overflow bin.");
                    }
                    else if (leafHeightOverflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(leafHeightOverflowCount.ToString());
                        markdownSourceBuilder.Append(" values greater than ");
                        markdownSourceBuilder.Append(leafHeightOverflow.ToString(leafHeightOverflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the overflow bin.");
                    }
                    else if (leafHeightUnderflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(leafHeightUnderflowCount.ToString());
                        markdownSourceBuilder.Append(" values smaller than ");
                        markdownSourceBuilder.Append(leafHeightUnderflow.ToString(leafHeightUnderflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the underflow bin.");
                    }

                    markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median leaf height, the interquartile range, and the 89% HDI.");

                    markdownSourceBuilder.AppendLine();

                    figNum++;
                }

                markdownSourceBuilder.Append("The **Sackin index** of the tree is **");
                markdownSourceBuilder.Append(sackinIndex.ToString());

                if (!binary)
                {
                    markdownSourceBuilder.AppendLine("**.");
                    markdownSourceBuilder.AppendLine("");
                }
                else
                {
                    markdownSourceBuilder.Append("** (YHK model normalisation: ");
                    markdownSourceBuilder.Append(sackinYule.ToString(sackinYule.GetDigits()));
                    markdownSourceBuilder.Append(", PDA model normalisation: ");
                    markdownSourceBuilder.Append(sackinPDA.ToString(sackinPDA.GetDigits()));
                    markdownSourceBuilder.Append("). ");

                    if (sackinYuleP > 0.95)
                    {
                        markdownSourceBuilder.Append("According to the Sackin index, the tree is significantly more balanced than expected under the YHK model (p &asymp; ");
                        markdownSourceBuilder.Append((1 - sackinYuleP).ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else if (sackinYuleP < 0.05)
                    {
                        markdownSourceBuilder.Append("According to the Sackin index, the tree is significantly less balanced than expected under the YHK model (p &asymp; ");
                        markdownSourceBuilder.Append(sackinYuleP.ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("According to the Sackin index, the balancing of the tree is not significantly different than expected under the YHK model. ");
                    }

                    if (sackinPDAP > 0.95)
                    {
                        markdownSourceBuilder.Append("The tree is significantly more balanced than expected under the PDA model (p &asymp; ");
                        markdownSourceBuilder.Append((1 - sackinPDAP).ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else if (sackinPDAP < 0.05)
                    {
                        markdownSourceBuilder.Append("The tree is significantly less balanced than expected under the PDA model (p &asymp; ");
                        markdownSourceBuilder.Append(sackinPDAP.ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("The balancing of the tree is not significantly different than expected under the PDA model. ");
                    }

                    markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Sackin index values under the YHK and PDA models.");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(sackinDistribution));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Sackin index values.** The curve shows the expected distribution of (normalised) Sackin index values under the null hypothesis for a tree containing " + leaves.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                        " The dashed line represents the observed value of the Sackin index, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                    markdownSourceBuilder.AppendLine();

                    figNum++;

                    markdownSourceBuilder.Append("The **Colless index** of the tree is **");
                    markdownSourceBuilder.Append(collessIndex.ToString());
                    markdownSourceBuilder.Append("** (YHK model normalisation: ");
                    markdownSourceBuilder.Append(collessYule.ToString(collessYule.GetDigits()));
                    markdownSourceBuilder.Append(", PDA model normalisation: ");
                    markdownSourceBuilder.Append(collessPDA.ToString(collessPDA.GetDigits()));
                    markdownSourceBuilder.Append("). ");

                    if (collessYuleP > 0.95)
                    {
                        markdownSourceBuilder.Append("According to the Colless index, the tree is significantly more balanced than expected under the YHK model (p &asymp; ");
                        markdownSourceBuilder.Append((1 - collessYuleP).ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else if (collessYuleP < 0.05)
                    {
                        markdownSourceBuilder.Append("According to the Colless index, the tree is significantly less balanced than expected under the YHK model (p &asymp; ");
                        markdownSourceBuilder.Append(collessYuleP.ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("According to the Colless index, the balancing of the tree is not significantly different than expected under the YHK model. ");
                    }

                    if (collessPDAP > 0.95)
                    {
                        markdownSourceBuilder.Append("The tree is significantly more balanced than expected under the PDA model (p &asymp; ");
                        markdownSourceBuilder.Append((1 - collessPDAP).ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else if (collessPDAP < 0.05)
                    {
                        markdownSourceBuilder.Append("The tree is significantly less balanced than expected under the PDA model (p &asymp; ");
                        markdownSourceBuilder.Append(collessPDAP.ToString(1, false));
                        markdownSourceBuilder.Append("). ");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("The balancing of the tree is not significantly different than expected under the PDA model. ");
                    }

                    markdownSourceBuilder.AppendLine("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the expected distribution of Colless index values under the YHK and PDA models.");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(collessDistribution));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of Colless index values.** The curve shows the expected distribution of (normalised) Colless index values under the null hypothesis for a tree containing " + leaves.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                        " The dashed line represents the observed value of the Colless index, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model.");

                    markdownSourceBuilder.AppendLine();

                    figNum++;
                }
            }

            if (leaves.Count >= 6)
            {
                markdownSourceBuilder.Append("The tree has **");
                markdownSourceBuilder.Append(numberOfCherries.ToString());

                if (!binary)
                {
                    markdownSourceBuilder.AppendLine("** cherries.");
                    markdownSourceBuilder.AppendLine();
                }
                else
                {
                    markdownSourceBuilder.Append("** cherries (YHK model normalisation: ");
                    markdownSourceBuilder.Append(numberOfCherriesYule.ToString(numberOfCherriesYule.GetDigits()));
                    markdownSourceBuilder.Append(", PDA model normalisation: ");
                    markdownSourceBuilder.Append(numberOfCherriesPDA.ToString(numberOfCherriesPDA.GetDigits()));
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

                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(numberOfCherriesDistribution));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.AppendLine("**Figure " + figNum.ToString() + ". Distribution of the number of cherries.** The curve shows the expected distribution of the (normalised) number of cherries under the null hypothesis for a tree containing " + leaves.Count.ToString() + " tips, computed using a Monte Carlo approach (2000 random trees were sampled according to the YHK or PDA model)." +
                        " The dashed line represents the observed value of the number of cherries, normalised according to the YHK or PDA model. The blue plot refers to the YHK model, while the orange plot to the PDA model. The green curve represents the limit distribution for the (normalised) number of cherries as the number of tips grows to infinity.");

                    markdownSourceBuilder.AppendLine();

                    figNum++;
                }
            }

            string markdownSource = markdownSourceBuilder.ToString();

            Markdig.Syntax.MarkdownDocument markdownDocument = Markdig.Markdown.Parse(markdownSource, new Markdig.MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().Build());

            return (markdownDocument, markdownSource);
        }
    }
}
