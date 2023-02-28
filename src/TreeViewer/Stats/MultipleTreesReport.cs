using Markdig;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Statistics;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;
using VectSharp.SVG;
using static TreeViewer.Stats.TreeReport;

namespace TreeViewer.Stats
{
    internal static class MultipleTreesReport
    {
        private static bool PairWise => GlobalSettings.Settings.PairwiseTreeComparisons;


        private static (int[], double[], double[]) GetRobinsonFouldsDistancesGlobal(IList<TreeNode> trees, Action<string, double> progressAction)
        {
            progressAction("Computing tree splits", 0);

            List<(string[], string[], double)>[] splits = new List<(string[], string[], double)>[trees.Count];

            object progressLock = new object();
            int completed = 0;
            Parallel.For(0, (int)Math.Ceiling((double)trees.Count / 20), j =>
            {
                for (int i = j * 20; i < (j + 1) * 20; i++)
                {
                    if (i < trees.Count)
                    {
                        splits[i] = (from el in trees[i].GetSplits()
                                     select (
                                     (from el1 in el.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToArray(),
                                     (from el2 in el.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToArray(),
                                     el.branchLength
                                     )).ToList();
                    }
                }

                lock (progressLock)
                {
                    completed += 20;
                    progressAction(null, (double)completed / trees.Count * 0.5);
                }
            });

            int[] robinsonFouldsDistances = new int[trees.Count * (trees.Count + 1) / 2];
            double[] weightedRobinsonFouldsDistances = new double[trees.Count * (trees.Count + 1) / 2];
            double[] edgeLengthDistances = new double[trees.Count * (trees.Count + 1) / 2];

            for (int i = 0; i < robinsonFouldsDistances.Length; i++)
            {
                robinsonFouldsDistances[i] = -1;
            }

            progressAction("Computing Robinson-Foulds distances", 0.5);

            completed = 0;

            const int itemsForStep = 1000;

            Parallel.For(0, (int)Math.Ceiling((double)robinsonFouldsDistances.Length / itemsForStep), j =>
            {
                int itemsDone = 0;

                for (int i = j * itemsForStep; i < (j + 1) * itemsForStep; i++)
                {
                    if (i < robinsonFouldsDistances.Length)
                    {
                        (int x, int y) = GetIndices(i);

                        if (x == y)
                        {
                            robinsonFouldsDistances[i] = 0;
                        }
                        else
                        {
                            (robinsonFouldsDistances[i], weightedRobinsonFouldsDistances[i], edgeLengthDistances[i]) = Comparisons.RobinsonFouldsDistance(splits[x], splits[y]);
                        }

                        itemsDone++;
                    }
                }

                lock (progressLock)
                {
                    completed += itemsDone;
                    progressAction(null, (double)completed / robinsonFouldsDistances.Length * 0.5 + 0.5);
                }
            });

            return (robinsonFouldsDistances, weightedRobinsonFouldsDistances, edgeLengthDistances);
        }


        private static (int[], double[], double[]) GetRobinsonFouldsDistancesPairwise(IList<TreeNode> trees, Action<string, double> progressAction)
        {
            progressAction("Computing tree splits", 0);

            List<(HashSet<string>, HashSet<string>, double)>[] splits = new List<(HashSet<string>, HashSet<string>, double)>[trees.Count];

            HashSet<string>[] leaves = new HashSet<string>[trees.Count];

            object progressLock = new object();
            int completed = 0;
            Parallel.For(0, (int)Math.Ceiling((double)trees.Count / 20), j =>
            {
                for (int i = j * 20; i < (j + 1) * 20; i++)
                {
                    if (i < trees.Count)
                    {
                        splits[i] = (from el in trees[i].GetSplits()
                                     select (
                                     (from el1 in el.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToHashSet(),
                                     (from el2 in el.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToHashSet(),
                                     el.branchLength
                                     )).ToList();

                        leaves[i] = trees[i].GetLeafNames().ToHashSet();

                        if (trees[i].IsRooted())
                        {
                            leaves[i].Add("@Root");
                        }
                    }
                }

                lock (progressLock)
                {
                    completed += 20;
                    progressAction(null, (double)completed / trees.Count * 0.5);
                }
            });

            int[] robinsonFouldsDistances = new int[trees.Count * (trees.Count + 1) / 2];
            double[] weightedRobinsonFouldsDistances = new double[trees.Count * (trees.Count + 1) / 2];
            double[] edgeLengthDistances = new double[trees.Count * (trees.Count + 1) / 2];

            for (int i = 0; i < robinsonFouldsDistances.Length; i++)
            {
                robinsonFouldsDistances[i] = -1;
            }

            progressAction("Computing Robinson-Foulds distances", 0.5);

            completed = 0;

            const int itemsForStep = 1000;

            Parallel.For(0, (int)Math.Ceiling((double)robinsonFouldsDistances.Length / itemsForStep), j =>
            {
                int itemsDone = 0;

                for (int i = j * itemsForStep; i < (j + 1) * itemsForStep; i++)
                {
                    if (i < robinsonFouldsDistances.Length)
                    {
                        (int x, int y) = GetIndices(i);

                        if (x == y)
                        {
                            robinsonFouldsDistances[i] = 0;
                        }
                        else
                        {
                            HashSet<string> commonLeaves = new HashSet<string>(leaves[x]);
                            commonLeaves.IntersectWith(leaves[y]);

                            List<(HashSet<string>, HashSet<string>, double)> splitsX = new List<(HashSet<string>, HashSet<string>, double)>(splits[x].Count);
                            for (int k = 0; k < splits[x].Count; k++)
                            {
                                HashSet<string> left = new HashSet<string>(splits[x][k].Item1);
                                left.IntersectWith(commonLeaves);

                                HashSet<string> right = new HashSet<string>(splits[x][k].Item2);
                                right.IntersectWith(commonLeaves);

                                if (left.Count >= 0 || right.Count >= 0)
                                {
                                    splitsX.Add((left, right, splits[x][k].Item3));
                                }
                            }

                            List<(HashSet<string>, HashSet<string>, double)> splitsY = new List<(HashSet<string>, HashSet<string>, double)>(splits[y].Count);
                            for (int k = 0; k < splits[y].Count; k++)
                            {
                                HashSet<string> left = new HashSet<string>(splits[y][k].Item1);
                                left.IntersectWith(commonLeaves);

                                HashSet<string> right = new HashSet<string>(splits[y][k].Item2);
                                right.IntersectWith(commonLeaves);

                                if (left.Count >= 0 || right.Count >= 0)
                                {
                                    splitsY.Add((left, right, splits[y][k].Item3));
                                }
                            }
                            (robinsonFouldsDistances[i], weightedRobinsonFouldsDistances[i], edgeLengthDistances[i]) = Comparisons.RobinsonFouldsDistance(splitsX, splitsY);
                        }

                        itemsDone++;
                    }
                }

                lock (progressLock)
                {
                    completed += itemsDone;
                    progressAction(null, (double)completed / robinsonFouldsDistances.Length * 0.5 + 0.5);
                }
            });

            return (robinsonFouldsDistances, weightedRobinsonFouldsDistances, edgeLengthDistances);
        }


        private static (double[,] distMat, double[,] points) PerformMDS(int treeCount, int[] distances)
        {
            Matrix<double> dSq = Matrix<double>.Build.Dense(treeCount, treeCount);
            double[,] distMat = new double[treeCount, treeCount];

            Matrix<double> centeringMatrix = Matrix<double>.Build.Dense(treeCount, treeCount, -1.0 / treeCount);

            for (int j = 0; j < treeCount; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    double val = distances[GetIndex(j, i)];

                    dSq[i, j] = val * val;
                    dSq[j, i] = val * val;
                    distMat[i, j] = val;
                    distMat[j, i] = val;
                }

                centeringMatrix[j, j] = 1 - 1.0 / treeCount;
            }

            Matrix<double> b = -0.5 * centeringMatrix * dSq * centeringMatrix;

            Evd<double> eigen = b.Evd();

            int[] sortedEigen = (from el in Enumerable.Range(0, treeCount) orderby eigen.EigenValues[el].Real descending select el).Take(2).ToArray();

            Matrix<double> eM = Matrix<double>.Build.DenseOfColumnVectors(eigen.EigenVectors.Column(sortedEigen[0]), eigen.EigenVectors.Column(sortedEigen[1]));

            Matrix<double> lamdbaMSqrt = Matrix<double>.Build.DiagonalOfDiagonalArray(new double[] { Math.Sqrt(eigen.EigenValues[sortedEigen[0]].Real), Math.Sqrt(eigen.EigenValues[sortedEigen[1]].Real) });

            Matrix<double> X = eM * lamdbaMSqrt;

            double[,] points = X.ToArray();

            return (distMat, points);
        }

        private static (double[,] distMat, double[,] points) PerformMDS(int treeCount, double[] distances)
        {
            Matrix<double> dSq = Matrix<double>.Build.Dense(treeCount, treeCount);
            double[,] distMat = new double[treeCount, treeCount];

            Matrix<double> centeringMatrix = Matrix<double>.Build.Dense(treeCount, treeCount, -1.0 / treeCount);

            for (int j = 0; j < treeCount; j++)
            {
                for (int i = 0; i < j; i++)
                {
                    double val = distances[GetIndex(j, i)];

                    dSq[i, j] = val * val;
                    dSq[j, i] = val * val;
                    distMat[i, j] = val;
                    distMat[j, i] = val;
                }

                centeringMatrix[j, j] = 1 - 1.0 / treeCount;
            }

            Matrix<double> b = -0.5 * centeringMatrix * dSq * centeringMatrix;

            Evd<double> eigen = b.Evd();

            int[] sortedEigen = (from el in Enumerable.Range(0, treeCount) orderby eigen.EigenValues[el].Real descending select el).Take(2).ToArray();

            Matrix<double> eM = Matrix<double>.Build.DenseOfColumnVectors(eigen.EigenVectors.Column(sortedEigen[0]), eigen.EigenVectors.Column(sortedEigen[1]));

            Matrix<double> lamdbaMSqrt = Matrix<double>.Build.DiagonalOfDiagonalArray(new double[] { Math.Sqrt(eigen.EigenValues[sortedEigen[0]].Real), Math.Sqrt(eigen.EigenValues[sortedEigen[1]].Real) });

            Matrix<double> X = eM * lamdbaMSqrt;

            double[,] points = X.ToArray();

            return (distMat, points);
        }



        private static int GetIndex(int x, int y)
        {
            if (x > y)
            {
                return GetIndex(y, x);
            }

            return y * (y + 1) / 2 + x;
        }

        private static (int x, int y) GetIndices(int index)
        {
            int y = (int)Math.Ceiling((Math.Sqrt(9 + 8 * index) - 3) / 2);
            int x = index - y * (y + 1) / 2;

            return (x, y);
        }


        public static (Markdig.Syntax.MarkdownDocument report, string reportSource) CreateReport(IList<TreeNode> trees, Action<string, double> progressAction, Dictionary<string, GetPlot> Plots, Dictionary<string, Func<(string header, IEnumerable<double[]>)>> Data, Avalonia.Controls.Window window)
        {
            HashSet<string> union = null;
            List<string> intersection = null;

            for (int i = 0; i < trees.Count; i++)
            {
                if (i == 0)
                {
                    intersection = trees[i].GetLeafNames();
                    union = new HashSet<string>(intersection);
                }
                else
                {
                    List<string> leafNames = trees[i].GetLeafNames();

                    foreach (string leafName in leafNames)
                    {
                        union.Add(leafName);
                    }

                    List<int> toBeRemoved = new List<int>();

                    for (int j = 0; j < intersection.Count; j++)
                    {
                        if (!leafNames.Contains(intersection[j]))
                        {
                            toBeRemoved.Add(j);
                        }
                    }

                    for (int j = toBeRemoved.Count - 1; j >= 0; j--)
                    {
                        intersection.RemoveAt(toBeRemoved[j]);
                    }
                }
            }

            bool sameLeaves = union.Count == intersection.Count;
            bool sameTopology = false;
            int maxDistance = -1;
            string treeSpacePlotGuid = null;
            Page treeSpacePlot = null;
            PointClustering unweightedRFClustering = null;

            bool haveLengths = false;
            double maxWeightedDistance = -1;
            string weightedTreeSpacePlotGuid = null;
            Page weightedTreeSpacePlot = null;
            PointClustering weightedRFClustering = null;
            bool areBinary = true;
            bool areRooted = true;

            double averageSackinIndex = double.NaN;
            double[] sackinIndexHDI = null;

            string sackinIndexDistributionPlotGuid = null;
            Page sackinIndexDistribution = null;
            (double sackinIndexUnderflow, int sackinIndexUnderflowCount, double sackinIndexOverflow, int sackinIndexOverflowCount) = (0, 0, 0, 0);

            double averageCollessIndex = double.NaN;
            double[] collessIndexHDI = null;

            string collessIndexDistributionPlotGuid = null;
            Page collessIndexDistribution = null;
            (double collessIndexUnderflow, int collessIndexUnderflowCount, double collessIndexOverflow, int collessIndexOverflowCount) = (0, 0, 0, 0);

            double averageNumberOfCherries = double.NaN;
            double[] numberOfCherriesHDI = null;

            string numberOfCherriesDistributionPlotGuid = null;
            Page numberOfCherriesDistribution = null;
            (double numberOfCherriesUnderflow, int numberOfCherriesUnderflowCount, double numberOfCherriesOverflow, int numberOfCherriesOverflowCount) = (0, 0, 0, 0);

            double maxEdgeLengthDistance = double.NaN;
            string edgeLengthTreeSpacePlotGuid = null;
            Page edgeLengthTreeSpacePlot = null;
            PointClustering edgeLengthClustering = null;

            if (sameLeaves || intersection.Count > 3 || PairWise)
            {
                IList<TreeNode> currentTrees;

                if (!sameLeaves && !PairWise)
                {
                    currentTrees = new List<TreeNode>(trees.Count);

                    for (int i = 0; i < trees.Count; i++)
                    {
                        TreeNode currTree = trees[i].Clone();

                        List<TreeNode> leaves = currTree.GetLeaves();

                        for (int j = 0; j < leaves.Count; j++)
                        {
                            if (!intersection.Contains(leaves[j].Name))
                            {
                                currTree = currTree.Prune(leaves[j], false);
                            }
                        }

                        currentTrees.Add(currTree);
                    }
                }
                else
                {
                    currentTrees = trees;
                }

                for (int i = 0; i < currentTrees.Count; i++)
                {
                    if (currentTrees[i].Children.Count > 2)
                    {
                        areRooted = false;
                    }

                    foreach (TreeNode node in currentTrees[i].GetChildrenRecursiveLazy())
                    {
                        if (node.Parent != null && node.Children.Count > 2)
                        {
                            areBinary = false;
                            break;
                        }
                    }

                    if (!areBinary && !areRooted)
                    {
                        break;
                    }
                }

                (int[] robinsonFouldsDistances, double[] weightedRobinsonFouldsDistances, double[] edgeLengthDistances) = (null, null, null);

                if (!PairWise)
                {
                    (robinsonFouldsDistances, weightedRobinsonFouldsDistances, edgeLengthDistances) = GetRobinsonFouldsDistancesGlobal(currentTrees, (p, x) => progressAction(p, x * 0.5));
                }
                else
                {
                    (robinsonFouldsDistances, weightedRobinsonFouldsDistances, edgeLengthDistances) = GetRobinsonFouldsDistancesPairwise(trees, (p, x) => progressAction(p, x * 0.5));
                }
                

                maxDistance = robinsonFouldsDistances.Max();
                maxWeightedDistance = weightedRobinsonFouldsDistances.Max();

                sameTopology = maxDistance == 0;
                haveLengths = !weightedRobinsonFouldsDistances.Any(double.IsNaN);

                if (!sameTopology)
                {
                    {
                        (double[,] distMat, double[,] points) = PerformMDS(currentTrees.Count, robinsonFouldsDistances);

                        if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            unweightedRFClustering = PointClustering.GetClustering(distMat, points, x => progressAction("Computing K-medoids (unweighted)", x / 4 + 0.5));
                        }
                        else
                        {
                            unweightedRFClustering = PointClustering.GetClustering(points, x => progressAction("Computing K-medoids (unweighted)", x / 4 + 0.5));
                        }

                        treeSpacePlotGuid = Guid.NewGuid().ToString();
                        treeSpacePlot = Stats.RobinsonFouldsDistancePoints.GetPlot(distMat, points, false, treeSpacePlotGuid);
                        Plots.Add(treeSpacePlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            Page pag = Points.GetPlot(distMat, points, "RF distance component 1", "RF distance component 2", "Tree space", null, out descriptions, out Dictionary<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> clickActions2, interactive);

                            clickActions = new Dictionary<string, Action<Avalonia.Controls.Window>>();

                            if (clickActions2 != null)
                            {
                                foreach (KeyValuePair<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> kvp in clickActions2)
                                {
                                    clickActions.Add(kvp.Key, win => kvp.Value(win, currentTrees));
                                }
                            }

                            return pag;
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int j = 0; j < currentTrees.Count; j++)
                                {
                                    double[] row = new double[j + 1];

                                    for (int i = 0; i < j; i++)
                                    {
                                        double val = robinsonFouldsDistances[GetIndex(j, i)];

                                        row[i] = val;
                                    }

                                    row[j] = 0;

                                    yield return row;
                                }
                            }

                            Data.Add(treeSpacePlotGuid, () => ("Robinson-Foulds distances (lower triangular matrix)", getData()));
                        }
                    }

                    if (haveLengths)
                    {
                        (double[,] distMat, double[,] points) = PerformMDS(currentTrees.Count, weightedRobinsonFouldsDistances);

                        if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            weightedRFClustering = PointClustering.GetClustering(distMat, points, x => progressAction("Computing K-medoids (weighted)", x / 4 + 0.75));
                        }
                        else
                        {
                            weightedRFClustering = PointClustering.GetClustering(points, x => progressAction("Computing K-medoids (weighted)", x / 4 + 0.75));
                        }

                        weightedTreeSpacePlotGuid = Guid.NewGuid().ToString();
                        weightedTreeSpacePlot = Stats.RobinsonFouldsDistancePoints.GetPlot(distMat, points, true, weightedTreeSpacePlotGuid);
                        Plots.Add(weightedTreeSpacePlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            Page pag = Points.GetPlot(distMat, points, "wRF distance component 1", "wRF distance component 2", "Tree space", null, out descriptions, out Dictionary<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> clickActions2, interactive);

                            clickActions = new Dictionary<string, Action<Avalonia.Controls.Window>>();

                            if (clickActions2 != null)
                            {
                                foreach (KeyValuePair<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> kvp in clickActions2)
                                {
                                    clickActions.Add(kvp.Key, win => kvp.Value(win, currentTrees));
                                }
                            }

                            return pag;
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                for (int j = 0; j < currentTrees.Count; j++)
                                {
                                    double[] row = new double[j + 1];

                                    for (int i = 0; i < j; i++)
                                    {
                                        double val = weightedRobinsonFouldsDistances[GetIndex(j, i)];

                                        row[i] = val;
                                    }

                                    row[j] = 0;

                                    yield return row;
                                }
                            }

                            Data.Add(weightedTreeSpacePlotGuid, () => ("Weighted Robinson-Foulds distances (lower triangular matrix)", getData()));
                        }
                    }

                    List<double> sackinIndices = new List<double>(currentTrees.Count);
                    List<double> collessIndices = new List<double>(currentTrees.Count);
                    List<double> numbersOfCherries = new List<double>(currentTrees.Count);

                    for (int i = 0; i < currentTrees.Count; i++)
                    {
                        if (areRooted)
                        {
                            sackinIndices.Add(currentTrees[i].SackinIndex());

                            if (areBinary)
                            {
                                collessIndices.Add(currentTrees[i].CollessIndex());
                            }
                        }

                        numbersOfCherries.Add(currentTrees[i].NumberOfCherries());
                    }

                    if (areRooted)
                    {
                        sackinIndexDistributionPlotGuid = Guid.NewGuid().ToString();
                        sackinIndexDistribution = Stats.SackinDistribution.GetPlot(sackinIndices, sackinIndexDistributionPlotGuid);
                        (sackinIndexUnderflow, sackinIndexUnderflowCount, sackinIndexOverflow, sackinIndexOverflowCount) = Stats.Histogram.GetOverUnderflow(sackinIndices);

                        Plots.Add(sackinIndexDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                        {
                            clickActions = null;
                            return TreeViewer.Stats.Histogram.GetPlot(sackinIndices, "Sackin index", "Sackin index distribution", null, out descriptions, interactive);
                        });

                        {
                            IEnumerable<double[]> getData()
                            {
                                foreach (double d in sackinIndices)
                                {
                                    yield return new double[] { d };
                                }
                            }

                            Data.Add(sackinIndexDistributionPlotGuid, () => ("Sackin index", getData()));
                        }

                        averageSackinIndex = sackinIndices.Average();
                        sackinIndexHDI = BayesStats.HighestDensityInterval(sackinIndices, 0.89);

                        if (areBinary)
                        {
                            collessIndexDistributionPlotGuid = Guid.NewGuid().ToString();
                            collessIndexDistribution = Stats.CollessDistribution.GetPlot(collessIndices, collessIndexDistributionPlotGuid);
                            (collessIndexUnderflow, collessIndexUnderflowCount, collessIndexOverflow, collessIndexOverflowCount) = Stats.Histogram.GetOverUnderflow(collessIndices);

                            Plots.Add(collessIndexDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                            {
                                clickActions = null;
                                return TreeViewer.Stats.Histogram.GetPlot(collessIndices, "Colless index", "Colless index distribution", null, out descriptions, interactive);
                            });

                            {
                                IEnumerable<double[]> getData()
                                {
                                    foreach (double d in collessIndices)
                                    {
                                        yield return new double[] { d };
                                    }
                                }

                                Data.Add(collessIndexDistributionPlotGuid, () => ("Colless index", getData()));
                            }

                            averageCollessIndex = collessIndices.Average();
                            collessIndexHDI = BayesStats.HighestDensityInterval(collessIndices, 0.89);
                        }
                    }

                    numberOfCherriesDistributionPlotGuid = Guid.NewGuid().ToString();
                    numberOfCherriesDistribution = Stats.NumberOfCherriesDistribution.GetPlot(numbersOfCherries, numberOfCherriesDistributionPlotGuid);
                    (numberOfCherriesUnderflow, numberOfCherriesUnderflowCount, numberOfCherriesOverflow, numberOfCherriesOverflowCount) = Stats.Histogram.GetOverUnderflow(numbersOfCherries);

                    Plots.Add(numberOfCherriesDistributionPlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                    {
                        clickActions = null;
                        return TreeViewer.Stats.Histogram.GetPlot(numbersOfCherries, "Number of cherries", "Distribution of the number of cherries", null, out descriptions, interactive);
                    });

                    {
                        IEnumerable<double[]> getData()
                        {
                            foreach (double d in numbersOfCherries)
                            {
                                yield return new double[] { d };
                            }
                        }

                        Data.Add(numberOfCherriesDistributionPlotGuid, () => ("Number of Cherries", getData()));
                    }

                    averageNumberOfCherries = numbersOfCherries.Average();
                    numberOfCherriesHDI = BayesStats.HighestDensityInterval(numbersOfCherries, 0.89);

                }
                else if (haveLengths)
                {
                    maxEdgeLengthDistance = edgeLengthDistances.Max();

                    (double[,] distMat, double[,] points) = PerformMDS(currentTrees.Count, edgeLengthDistances);

                    if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                    {
                        edgeLengthClustering = PointClustering.GetClustering(distMat, points, x => progressAction("Computing K-medoids (edge-length distances)", x / 4 + 0.5));
                    }
                    else
                    {
                        edgeLengthClustering = PointClustering.GetClustering(points, x => progressAction("Computing K-medoids (edge-length distances)", x / 4 + 0.5));
                    }

                    edgeLengthTreeSpacePlotGuid = Guid.NewGuid().ToString();
                    edgeLengthTreeSpacePlot = Stats.RobinsonFouldsDistancePoints.GetPlot(distMat, points, false, edgeLengthTreeSpacePlotGuid);
                    Plots.Add(edgeLengthTreeSpacePlotGuid, (bool interactive, out Dictionary<string, (Colour, Colour, string)> descriptions, out Dictionary<string, Action<Avalonia.Controls.Window>> clickActions) =>
                    {
                        Page pag = Points.GetPlot(distMat, points, "EL distance component 1", "EL distance component 2", "Tree space", null, out descriptions, out Dictionary<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> clickActions2, interactive);

                        clickActions = new Dictionary<string, Action<Avalonia.Controls.Window>>();

                        if (clickActions2 != null)
                        {
                            foreach (KeyValuePair<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> kvp in clickActions2)
                            {
                                clickActions.Add(kvp.Key, win => kvp.Value(win, currentTrees));
                            }
                        }

                        return pag;
                    });

                    {
                        IEnumerable<double[]> getData()
                        {
                            for (int j = 0; j < currentTrees.Count; j++)
                            {
                                double[] row = new double[j + 1];

                                for (int i = 0; i < j; i++)
                                {
                                    double val = edgeLengthDistances[GetIndex(j, i)];

                                    row[i] = val;
                                }

                                row[j] = 0;

                                yield return row;
                            }
                        }

                        Data.Add(edgeLengthTreeSpacePlotGuid, () => ("Edge-length distances (lower triangular matrix)", getData()));
                    }
                }
            }

            int figNum = 1;

            StringBuilder markdownSourceBuilder = new StringBuilder();

            markdownSourceBuilder.AppendLine("# Analysis of multiple trees");
            markdownSourceBuilder.AppendLine();

            markdownSourceBuilder.AppendLine("## General information");
            markdownSourceBuilder.AppendLine();

            if (sameLeaves)
            {
                markdownSourceBuilder.Append("The ");
                markdownSourceBuilder.Append(trees.Count.ToString());
                markdownSourceBuilder.AppendLine(" loaded trees all have the same tip labels.");
                markdownSourceBuilder.AppendLine();
            }
            else
            {
                markdownSourceBuilder.Append("The ");
                markdownSourceBuilder.Append(trees.Count.ToString());
                markdownSourceBuilder.Append(" loaded trees do not all have the same labels. There are **");
                markdownSourceBuilder.Append(intersection.Count.ToString());
                markdownSourceBuilder.Append("** tip labels shared between all the trees, and **");
                markdownSourceBuilder.Append(union.Count.ToString());
                markdownSourceBuilder.Append("** different tip labels in total.");

                if (intersection.Count > 3)
                {
                    markdownSourceBuilder.AppendLine(" The following analyses were performed on the subset of tips that are shared among all trees.");
                }
                else if (PairWise)
                {
                    markdownSourceBuilder.AppendLine(" The following analyses were performed using pairwise comparisons between trees.");
                }
                else
                {
                    markdownSourceBuilder.AppendLine();
                }

                markdownSourceBuilder.AppendLine();
            }


            if (sameLeaves || intersection.Count > 3 || PairWise)
            {
                if (sameTopology)
                {
                    markdownSourceBuilder.Append("The trees all have the same topology. The maximum edge-length distance between two trees is **");
                    markdownSourceBuilder.Append(maxEdgeLengthDistance.ToString());
                    markdownSourceBuilder.Append("**. ");
                    markdownSourceBuilder.Append("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") represents the trees in a 2-dimensional space, using the edge-length metric.");

                    markdownSourceBuilder.AppendLine();
                    markdownSourceBuilder.AppendLine();


                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(edgeLengthTreeSpacePlot));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    if (edgeLengthClustering.ClusterCount > 1)
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** _Left plot_: each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the edge-length distance between the corresponding trees. ");

                        if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the edge-length distances. ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                            markdownSourceBuilder.Append(edgeLengthClustering.MultipleClustersPValue.ToString(edgeLengthClustering.MultipleClustersPValue.GetDigits()));
                            markdownSourceBuilder.Append(", α = 0.001). ");
                        }

                        markdownSourceBuilder.Append(edgeLengthClustering.ClusterCount);
                        markdownSourceBuilder.Append(" clusters of trees were identified using the K-medoids (PAM) method; the medoid for each cluster is displayed as a star. _Right plot_: the optimal number of clusters for the K-medoids clustering was determined using the average silhouette score of all the points ");
                        markdownSourceBuilder.Append("when a certain number of clusters is used. The size of each point is proportional to the cluster size variance when the corresponding number of clusters is used. Clustering with up to ");
                        markdownSourceBuilder.Append(edgeLengthClustering.ClusterNumberScores.Length);
                        markdownSourceBuilder.Append(" clusters was attempted, and for each number of clusters the average silhouette score and the variance of the cluster size were computed; the best number of clusters was selected as the one with the smallest cluster size variance, amongst those with a silhouette ");
                        markdownSourceBuilder.Append("score greater than or equal to 95% of the highest observed silhouette score. This threshold is shown by the vertical dashed line. The number of clusters that was actually used in the plot is highlighted by an arrowhead.");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** Each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the edge-length distance between the corresponding trees. ");

                        if (!GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            if (edgeLengthClustering.MultipleClustersPValue > 0.001)
                            {
                                markdownSourceBuilder.Append("The Duda-Hart test was used to determine that the trees do not show significance evidence for clustering (p &asymp; ");
                                markdownSourceBuilder.Append(edgeLengthClustering.MultipleClustersPValue.ToString(edgeLengthClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001), based on the 2D metric obtained after the MDS analysis.");
                            }
                            else
                            {
                                markdownSourceBuilder.Append("Clustering was attempted based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                                markdownSourceBuilder.Append(edgeLengthClustering.MultipleClustersPValue.ToString(edgeLengthClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001); however, all clusterings with up to 12 clusters had an average silhouette score &leq; 0.5 and were therefore discarded.");
                            }
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was attempted based on the edge-length distances, with up to 12 clusters. However, since none of these clusterings had an average silhouette score &gt; 0.5, all clusters were discarded. ");
                        }
                    }

                    markdownSourceBuilder.AppendLine();
                    markdownSourceBuilder.AppendLine();

                    figNum++;
                }
                else
                {
                    markdownSourceBuilder.Append("The trees have different topologies. The maximum unweighted Robinson-Foulds distance between two trees is **");
                    markdownSourceBuilder.Append(maxDistance.ToString());
                    markdownSourceBuilder.Append("**. ");

                    if (haveLengths)
                    {
                        markdownSourceBuilder.Append("The maximum weighted Robinson-Foulds distance between two trees is **");
                        markdownSourceBuilder.Append(maxWeightedDistance.ToString(maxWeightedDistance.GetDigits()));
                        markdownSourceBuilder.Append("**. ");
                    }

                    markdownSourceBuilder.Append("[**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") represents the trees in a 2-dimensional space, using the Robinson-Foulds metric.");

                    if (haveLengths)
                    {
                        markdownSourceBuilder.Append(" [**Figure " + (figNum + 1).ToString() + "**](#fig" + (figNum + 1).ToString() + ") also represents the trees in a 2-dimensional space, using instead the weighted Robinson-Foulds metric.");
                    }

                    markdownSourceBuilder.AppendLine();
                    markdownSourceBuilder.AppendLine();


                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(treeSpacePlot));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    if (unweightedRFClustering.ClusterCount > 1)
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** _Left plot_: each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the unweighted Robinson-Foulds distance between the corresponding trees. ");

                        if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the Robinson-Foulds distances. ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                            markdownSourceBuilder.Append(unweightedRFClustering.MultipleClustersPValue.ToString(unweightedRFClustering.MultipleClustersPValue.GetDigits()));
                            markdownSourceBuilder.Append(", α = 0.001). ");
                        }

                        markdownSourceBuilder.Append(unweightedRFClustering.ClusterCount);
                        markdownSourceBuilder.Append(" clusters of trees were identified using the K-medoids (PAM) method; the medoid for each cluster is displayed as a star. _Right plot_: the optimal number of clusters for the K-medoids clustering was determined using the average silhouette score of all the points ");
                        markdownSourceBuilder.Append("when a certain number of clusters is used. The size of each point is proportional to the cluster size variance when the corresponding number of clusters is used. Clustering with up to ");
                        markdownSourceBuilder.Append(unweightedRFClustering.ClusterNumberScores.Length);
                        markdownSourceBuilder.Append(" clusters was attempted, and for each number of clusters the average silhouette score and the variance of the cluster size were computed; the best number of clusters was selected as the one with the smallest cluster size variance, amongst those with a silhouette ");
                        markdownSourceBuilder.Append("score greater than or equal to 95% of the highest observed silhouette score. This threshold is shown by the vertical dashed line. The number of clusters that was actually used in the plot is highlighted by an arrowhead.");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** Each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the unweighted Robinson-Foulds distance between the corresponding trees. ");

                        if (!GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            if (unweightedRFClustering.MultipleClustersPValue > 0.001)
                            {
                                markdownSourceBuilder.Append("The Duda-Hart test was used to determine that the trees do not show significance evidence for clustering (p &asymp; ");
                                markdownSourceBuilder.Append(unweightedRFClustering.MultipleClustersPValue.ToString(unweightedRFClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001), based on the 2D metric obtained after the MDS analysis.");
                            }
                            else
                            {
                                markdownSourceBuilder.Append("Clustering was attempted based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                                markdownSourceBuilder.Append(unweightedRFClustering.MultipleClustersPValue.ToString(unweightedRFClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001); however, all clusterings with up to 12 clusters had an average silhouette score &leq; 0.5 and were therefore discarded.");
                            }
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was attempted based on the Robinson-Foulds distances, with up to 12 clusters. However, since none of these clusterings had an average silhouette score &gt; 0.5, all clusters were discarded. ");
                        }
                    }

                    markdownSourceBuilder.AppendLine();
                    markdownSourceBuilder.AppendLine();

                    figNum++;



                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(weightedTreeSpacePlot));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    if (weightedRFClustering.ClusterCount > 1)
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** _Left plot_: each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the weighted Robinson-Foulds distance between the corresponding trees. ");

                        if (GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the Robinson-Foulds distances. ");
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was performed based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                            markdownSourceBuilder.Append(weightedRFClustering.MultipleClustersPValue.ToString(weightedRFClustering.MultipleClustersPValue.GetDigits()));
                            markdownSourceBuilder.Append(", α = 0.001). ");
                        }

                        markdownSourceBuilder.Append(weightedRFClustering.ClusterCount);
                        markdownSourceBuilder.Append(" clusters of trees were identified using the K-medoids (PAM) method; the medoid for each cluster is displayed as a star. _Right plot_: the optimal number of clusters for the K-medoids clustering was determined using the average silhouette score of all the points ");
                        markdownSourceBuilder.Append("when a certain number of clusters is used. The size of each point is proportional to the cluster size variance when the corresponding number of clusters is used. Clustering with up to ");
                        markdownSourceBuilder.Append(weightedRFClustering.ClusterNumberScores.Length);
                        markdownSourceBuilder.Append(" clusters was attempted, and for each number of clusters the average silhouette score and the variance of the cluster size were computed; the best number of clusters was selected as the one with the smallest cluster size variance, amongst those with a silhouette ");
                        markdownSourceBuilder.Append("score greater than or equal to 95% of the highest observed silhouette score. This threshold is shown by the vertical dashed line. The number of clusters that was actually used in the plot is highlighted by an arrowhead.");
                    }
                    else
                    {
                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". 2-dimensional representation of the trees.** Each tree is represented by a point, whose position was determined using multidimensional scaling (MDS); the distance between two points is approximately proportional to the weighted Robinson-Foulds distance between the corresponding trees. ");

                        if (!GlobalSettings.Settings.ClusterAccordingToRFDistances)
                        {
                            if (unweightedRFClustering.MultipleClustersPValue > 0.001)
                            {
                                markdownSourceBuilder.Append("The Duda-Hart test was used to determine that the trees do not show significance evidence for clustering (p &asymp; ");
                                markdownSourceBuilder.Append(unweightedRFClustering.MultipleClustersPValue.ToString(unweightedRFClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001), based on the 2D metric obtained after the MDS analysis.");
                            }
                            else
                            {
                                markdownSourceBuilder.Append("Clustering was attempted based on the 2D metric obtained after the MDS analysis. The presence of multiple clusters was determined using the Duda-Hart test (p &asymp; ");
                                markdownSourceBuilder.Append(unweightedRFClustering.MultipleClustersPValue.ToString(unweightedRFClustering.MultipleClustersPValue.GetDigits()));
                                markdownSourceBuilder.Append(", α = 0.001); however, all clusterings with up to 12 clusters had an average silhouette score &leq; 0.5 and were therefore discarded.");
                            }
                        }
                        else
                        {
                            markdownSourceBuilder.Append("Clustering was attempted based on the Robinson-Foulds distances, with up to 12 clusters. However, since none of these clusterings had an average silhouette score &gt; 0.5, all clusters were discarded. ");
                        }
                    }

                    markdownSourceBuilder.AppendLine();
                    markdownSourceBuilder.AppendLine();

                    figNum++;


                    markdownSourceBuilder.AppendLine("<br type=\"page\"/>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.AppendLine("## Tree shape statistics");
                    markdownSourceBuilder.AppendLine();

                    if (areRooted)
                    {
                        markdownSourceBuilder.Append("The trees are all rooted. The average **Sackin index** of the trees is **");
                        markdownSourceBuilder.Append(averageSackinIndex.ToString(averageSackinIndex.GetDigits()));
                        markdownSourceBuilder.Append("** (89% highest-density interval: ");
                        markdownSourceBuilder.Append(sackinIndexHDI[0].ToString(sackinIndexHDI[0].GetDigits()));
                        markdownSourceBuilder.Append(" &#8212; ");
                        markdownSourceBuilder.Append(sackinIndexHDI[1].ToString(sackinIndexHDI[1].GetDigits()));
                        markdownSourceBuilder.AppendLine("). [**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of Sackin index values among the trees.");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                        markdownSourceBuilder.Append(GetSVGBase64(sackinIndexDistribution));

                        markdownSourceBuilder.AppendLine("\"></p>");
                        markdownSourceBuilder.AppendLine();

                        markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of Sackin index values.** The histogram shows the distribution of Sackin index values among the trees.");

                        if (sackinIndexOverflowCount > 0 && sackinIndexUnderflowCount > 0)
                        {
                            markdownSourceBuilder.Append(" ");
                            markdownSourceBuilder.Append(sackinIndexUnderflowCount.ToString());
                            markdownSourceBuilder.Append(" values smaller than ");
                            markdownSourceBuilder.Append(sackinIndexUnderflow.ToString(sackinIndexUnderflow.GetDigits()));
                            markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                            markdownSourceBuilder.Append(sackinIndexOverflowCount.ToString());
                            markdownSourceBuilder.Append(" values greater than ");
                            markdownSourceBuilder.Append(sackinIndexOverflow.ToString(sackinIndexOverflow.GetDigits()));
                            markdownSourceBuilder.Append(" are shown in the overflow bin.");
                        }
                        else if (sackinIndexOverflowCount > 0)
                        {
                            markdownSourceBuilder.Append(" ");
                            markdownSourceBuilder.Append(sackinIndexOverflowCount.ToString());
                            markdownSourceBuilder.Append(" values greater than ");
                            markdownSourceBuilder.Append(sackinIndexOverflow.ToString(sackinIndexOverflow.GetDigits()));
                            markdownSourceBuilder.Append(" are shown in the overflow bin.");
                        }
                        else if (sackinIndexUnderflowCount > 0)
                        {
                            markdownSourceBuilder.Append(" ");
                            markdownSourceBuilder.Append(sackinIndexUnderflowCount.ToString());
                            markdownSourceBuilder.Append(" values smaller than ");
                            markdownSourceBuilder.Append(sackinIndexUnderflow.ToString(sackinIndexUnderflow.GetDigits()));
                            markdownSourceBuilder.Append(" are shown in the underflow bin.");
                        }

                        markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median branch length, the interquartile range, and the 89% HDI.");

                        markdownSourceBuilder.AppendLine();

                        figNum++;


                        if (areBinary)
                        {
                            markdownSourceBuilder.Append("The trees are all fully bifurcating. The average **Colless index** of the trees is **");
                            markdownSourceBuilder.Append(averageCollessIndex.ToString(averageCollessIndex.GetDigits()));
                            markdownSourceBuilder.Append("** (89% highest-density interval: ");
                            markdownSourceBuilder.Append(collessIndexHDI[0].ToString(collessIndexHDI[0].GetDigits()));
                            markdownSourceBuilder.Append(" &#8212; ");
                            markdownSourceBuilder.Append(collessIndexHDI[1].ToString(collessIndexHDI[1].GetDigits()));
                            markdownSourceBuilder.AppendLine("). [**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of Colless index values among the trees.");
                            markdownSourceBuilder.AppendLine();

                            markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                            markdownSourceBuilder.Append(GetSVGBase64(collessIndexDistribution));

                            markdownSourceBuilder.AppendLine("\"></p>");
                            markdownSourceBuilder.AppendLine();

                            markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of Colless index values.** The histogram shows the distribution of Colless index values among the trees.");

                            if (collessIndexOverflowCount > 0 && collessIndexUnderflowCount > 0)
                            {
                                markdownSourceBuilder.Append(" ");
                                markdownSourceBuilder.Append(collessIndexUnderflowCount.ToString());
                                markdownSourceBuilder.Append(" values smaller than ");
                                markdownSourceBuilder.Append(collessIndexUnderflow.ToString(collessIndexUnderflow.GetDigits()));
                                markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                                markdownSourceBuilder.Append(collessIndexOverflowCount.ToString());
                                markdownSourceBuilder.Append(" values greater than ");
                                markdownSourceBuilder.Append(collessIndexOverflow.ToString(collessIndexOverflow.GetDigits()));
                                markdownSourceBuilder.Append(" are shown in the overflow bin.");
                            }
                            else if (collessIndexOverflowCount > 0)
                            {
                                markdownSourceBuilder.Append(" ");
                                markdownSourceBuilder.Append(collessIndexOverflowCount.ToString());
                                markdownSourceBuilder.Append(" values greater than ");
                                markdownSourceBuilder.Append(collessIndexOverflow.ToString(collessIndexOverflow.GetDigits()));
                                markdownSourceBuilder.Append(" are shown in the overflow bin.");
                            }
                            else if (collessIndexUnderflowCount > 0)
                            {
                                markdownSourceBuilder.Append(" ");
                                markdownSourceBuilder.Append(collessIndexUnderflowCount.ToString());
                                markdownSourceBuilder.Append(" values smaller than ");
                                markdownSourceBuilder.Append(collessIndexUnderflow.ToString(collessIndexUnderflow.GetDigits()));
                                markdownSourceBuilder.Append(" are shown in the underflow bin.");
                            }

                            markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median branch length, the interquartile range, and the 89% HDI.");

                            markdownSourceBuilder.AppendLine();

                            figNum++;
                        }
                        else
                        {
                            markdownSourceBuilder.AppendLine("Not all the trees are fully bifurcating.");
                            markdownSourceBuilder.AppendLine();
                        }

                    }
                    else
                    {
                        markdownSourceBuilder.AppendLine("Not all the trees are rooted.");
                        markdownSourceBuilder.AppendLine();
                    }


                    markdownSourceBuilder.Append("The average **Number of cherries** of the trees is **");
                    markdownSourceBuilder.Append(averageNumberOfCherries.ToString(averageNumberOfCherries.GetDigits()));
                    markdownSourceBuilder.Append("** (89% highest-density interval: ");
                    markdownSourceBuilder.Append(numberOfCherriesHDI[0].ToString(numberOfCherriesHDI[0].GetDigits()));
                    markdownSourceBuilder.Append(" &#8212; ");
                    markdownSourceBuilder.Append(numberOfCherriesHDI[1].ToString(numberOfCherriesHDI[1].GetDigits()));
                    markdownSourceBuilder.AppendLine("). [**Figure " + figNum.ToString() + "**](#fig" + figNum.ToString() + ") shows the distribution of the number of cherries among the trees.");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("<p align=\"center\"><a name=\"fig" + figNum.ToString() + "\" /><img src=\"");

                    markdownSourceBuilder.Append(GetSVGBase64(numberOfCherriesDistribution));

                    markdownSourceBuilder.AppendLine("\"></p>");
                    markdownSourceBuilder.AppendLine();

                    markdownSourceBuilder.Append("**Figure " + figNum.ToString() + ". Distribution of the number of cherries.** The histogram shows the distribution of the number of cherries among the trees.");

                    if (numberOfCherriesOverflowCount > 0 && numberOfCherriesUnderflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(numberOfCherriesUnderflowCount.ToString());
                        markdownSourceBuilder.Append(" values smaller than ");
                        markdownSourceBuilder.Append(numberOfCherriesUnderflow.ToString(numberOfCherriesUnderflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the underflow bin; ");
                        markdownSourceBuilder.Append(numberOfCherriesOverflowCount.ToString());
                        markdownSourceBuilder.Append(" values greater than ");
                        markdownSourceBuilder.Append(numberOfCherriesOverflow.ToString(numberOfCherriesOverflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the overflow bin.");
                    }
                    else if (numberOfCherriesOverflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(numberOfCherriesOverflowCount.ToString());
                        markdownSourceBuilder.Append(" values greater than ");
                        markdownSourceBuilder.Append(numberOfCherriesOverflow.ToString(numberOfCherriesOverflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the overflow bin.");
                    }
                    else if (numberOfCherriesUnderflowCount > 0)
                    {
                        markdownSourceBuilder.Append(" ");
                        markdownSourceBuilder.Append(numberOfCherriesUnderflowCount.ToString());
                        markdownSourceBuilder.Append(" values smaller than ");
                        markdownSourceBuilder.Append(numberOfCherriesUnderflow.ToString(numberOfCherriesUnderflow.GetDigits()));
                        markdownSourceBuilder.Append(" are shown in the underflow bin.");
                    }

                    markdownSourceBuilder.AppendLine(" The box and whisker plot at the top represents the median branch length, the interquartile range, and the 89% HDI.");

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
