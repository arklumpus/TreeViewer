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

using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TreeViewer.Stats
{
    internal struct DistanceMatrixDistance : Accord.Math.Distances.IDistance<int>, Accord.Math.Distances.IDistance<int[]>
    {
        private double[,] DistanceMatrix;

        public DistanceMatrixDistance(double[,] distanceMatrix)
        {
            this.DistanceMatrix = distanceMatrix;
        }

        public double Distance(int x, int y)
        {
            return DistanceMatrix[Math.Min(x, y), Math.Max(x, y)];
        }

        public double Distance(int[] x, int[] y)
        {
            return DistanceMatrix[Math.Min(x[0], y[0]), Math.Max(x[0], y[0])];
        }
    }

    internal class PointClustering
    {
        private static Dictionary<double[,], PointClustering> ComputedPointClusterings = new Dictionary<double[,], PointClustering>();
        private static Dictionary<double[,], PointClustering> ComputedPointDistanceClusterings = new Dictionary<double[,], PointClustering>();

        public static PointClustering GetClustering(double[,] points, Action<double> progress)
        {
            if (!ComputedPointClusterings.TryGetValue(points, out PointClustering pointClustering))
            {
                pointClustering = new PointClustering(points, progress);
                ComputedPointClusterings[points] = pointClustering;
            }

            return pointClustering;
        }

        public static PointClustering GetClustering(double[,] distMat, double[,] points, Action<double> progress)
        {
            if (!ComputedPointDistanceClusterings.TryGetValue(distMat, out PointClustering pointClustering))
            {
                pointClustering = new PointClustering(distMat, points, progress);
                ComputedPointDistanceClusterings[distMat] = pointClustering;
            }

            return pointClustering;
        }

        public int ClusterCount { get; }
        public double[,] Centroids { get; }
        public double[,] Ellipsoids { get; }
        public int[] CentroidAssignments { get; }
        public double[,] Points { get; }
        public double[] ClusterNumberScores { get; }
        public double[] ClusterNumberSizeVariances { get; }
        public double MultipleClustersPValue { get; }

        private static (double[,] centroids, double[,] ellipsoids, int[] centroidAssignments, double silhouetteScore, double clusterSizeVariance, double error) KMedoidsClustering(double[][] points, int k)
        {
            if (k > 1)
            {
                Accord.MachineLearning.KMedoids kMedoids = new Accord.MachineLearning.KMedoids(k);
                kMedoids.Learn(points);

                double[,] centroids = new double[k, 2];

                for (int i = 0; i < k; i++)
                {
                    centroids[i, 0] = kMedoids.Clusters[i].Centroid[0];
                    centroids[i, 1] = kMedoids.Clusters[i].Centroid[1];
                }

                int[] clusterAssignments = new int[points.Length];
                kMedoids.Clusters.Decide(points, clusterAssignments);

                double[,] ellipsoids = new double[kMedoids.K, 3];

                for (int i = 0; i < kMedoids.K; i++)
                {
                    double varianceX = 0;
                    double varianceY = 0;
                    double covariance = 0;
                    int count = 0;

                    for (int j = 0; j < points.Length; j++)
                    {
                        if (clusterAssignments[j] == i)
                        {
                            varianceX += (points[j][0] - centroids[i, 0]) * (points[j][0] - centroids[i, 0]);
                            varianceY += (points[j][1] - centroids[i, 1]) * (points[j][1] - centroids[i, 1]);
                            covariance += (points[j][0] - centroids[i, 0]) * (points[j][1] - centroids[i, 1]);
                            count++;
                        }
                    }

                    varianceX /= count;
                    varianceY /= count;
                    covariance /= count;


                    Matrix<double> covarianceMatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { varianceX, covariance }, { covariance, varianceY } });
                    Evd<double> evd = covarianceMatrix.Evd();

                    double r1 = Math.Sqrt(10.5797 * evd.EigenValues[0].Real);
                    double r2 = Math.Sqrt(10.5797 * evd.EigenValues[1].Real);

                    double theta = Math.Atan2(evd.EigenVectors[1, 0], evd.EigenVectors[0, 0]);

                    ellipsoids[i, 0] = r1;
                    ellipsoids[i, 1] = r2;
                    ellipsoids[i, 2] = theta;
                }

                double[] silhouetteScores = new double[points.Length];

                Parallel.For(0, points.Length, i =>
                {
                    double[] distances = new double[kMedoids.K];
                    int[] counts = new int[kMedoids.K];

                    for (int j = 0; j < points.Length; j++)
                    {
                        if (j != i)
                        {
                            distances[clusterAssignments[j]] += Math.Sqrt((points[i][0] - points[j][0]) * (points[i][0] - points[j][0]) + (points[i][1] - points[j][1]) * (points[i][1] - points[j][1]));
                            counts[clusterAssignments[j]]++;
                        }
                    }

                    for (int j = 0; j < kMedoids.K; j++)
                    {
                        distances[j] /= counts[j];
                    }

                    if (counts[clusterAssignments[i]] > 0 && points.Length - 1 - counts[clusterAssignments[i]] > 0)
                    {
                        double a = distances[clusterAssignments[i]];
                        double b = (from el in Enumerable.Range(0, kMedoids.K) where el != clusterAssignments[i] select distances[el]).Min();

                        silhouetteScores[i] = (b - a) / Math.Max(a, b);
                    }
                    else
                    {
                        silhouetteScores[i] = 0;
                    }
                });

                double clusterSizeVariance = (from el in Enumerable.Range(0, kMedoids.K) select (double)clusterAssignments.Count(x => x == el)).Variance();

                double squaredError = 0;

                for (int i = 0; i < points.Length; i++)
                {
                    squaredError += (points[i][0] - centroids[clusterAssignments[i], 0]) * (points[i][0] - centroids[clusterAssignments[i], 0]) + (points[i][1] - centroids[clusterAssignments[i], 1]) * (points[i][1] - centroids[clusterAssignments[i], 1]);
                }

                return (centroids, ellipsoids, clusterAssignments, silhouetteScores.Average(), clusterSizeVariance, squaredError);
            }
            else
            {
                double[] distances = new double[points.Length * (points.Length - 1) / 2];

                static int getIndex(int i, int j, int n)
                {
                    return n * (n - 1) / 2 - (n - i) * (n - i - 1) / 2 + j - i - 1;
                }

                static (int i, int j) getIndices(int index, int n)
                {
                    int i = n - 2 - (int)Math.Floor(Math.Sqrt(-8 * index + 4 * n * (n - 1) - 7) / 2 - 0.5);
                    int j = index + i + 1 - n * (n - 1) / 2 + (n - i) * ((n - i) - 1) / 2;
                    return (i, j);
                }

                Parallel.For(0, points.Length * (points.Length - 1) / 2, index =>
                {
                    (int i, int j) = getIndices(index, points.Length);

                    distances[index] = (points[i][0] - points[j][0]) * (points[i][0] - points[j][0]) + (points[i][1] - points[j][1]) * (points[i][1] - points[j][1]);
                });

                double minSum = double.MaxValue;
                int minIndex = -1;
                object minLock = new object();

                Parallel.For(0, points.Length, i =>
                {
                    double sum = 0;

                    for (int j = 0; j < points.Length; j++)
                    {
                        if (j != i)
                        {
                            sum += distances[getIndex(Math.Min(i, j), Math.Max(i, j), points.Length)];
                        }

                    }

                    lock (minLock)
                    {
                        if (sum < minSum)
                        {
                            minSum = sum;
                            minIndex = i;
                        }
                    }
                });

                double[,] centroids = new double[1, 2] { { points[minIndex][0], points[minIndex][1] } };

                int[] clusterAssignments = new int[points.Length];

                double[,] ellipsoids = new double[1, 3];

                {
                    double varianceX = 0;
                    double varianceY = 0;
                    double covariance = 0;
                    int count = 0;

                    for (int j = 0; j < points.Length; j++)
                    {
                        varianceX += (points[j][0] - centroids[0, 0]) * (points[j][0] - centroids[0, 0]);
                        varianceY += (points[j][1] - centroids[0, 1]) * (points[j][1] - centroids[0, 1]);
                        covariance += (points[j][0] - centroids[0, 0]) * (points[j][1] - centroids[0, 1]);
                        count++;
                    }

                    varianceX /= count;
                    varianceY /= count;
                    covariance /= count;


                    Matrix<double> covarianceMatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { varianceX, covariance }, { covariance, varianceY } });
                    Evd<double> evd = covarianceMatrix.Evd();

                    double r1 = Math.Sqrt(10.5797 * evd.EigenValues[0].Real);
                    double r2 = Math.Sqrt(10.5797 * evd.EigenValues[1].Real);

                    double theta = Math.Atan2(evd.EigenVectors[1, 0], evd.EigenVectors[0, 0]);

                    ellipsoids[0, 0] = r1;
                    ellipsoids[0, 1] = r2;
                    ellipsoids[0, 2] = theta;
                }

                double clusterSizeVariance = 0;

                double squaredError = minSum;

                return (centroids, ellipsoids, clusterAssignments, 0, clusterSizeVariance, squaredError);
            }
        }

        private static (double[,] centroids, double[,] ellipsoids, int[] centroidAssignments, double silhouetteScore, double clusterSizeVariance, double error) KMedoidsClustering(double[,] distMat, double[][] points, int k)
        {
            if (k > 1)
            {
                int[][] indices = (from el in Enumerable.Range(0, distMat.GetLength(0)) select new int[] { el }).ToArray();

                DistanceMatrixDistance distance = new DistanceMatrixDistance(distMat);

                Accord.MachineLearning.KMedoids<int> kMedoids = new Accord.MachineLearning.KMedoids<int>(k, distance);
                kMedoids.Learn(indices);

                double[,] centroids = new double[k, 2];

                for (int i = 0; i < k; i++)
                {
                    centroids[i, 0] = points[kMedoids.Clusters[i].Centroid[0]][0];
                    centroids[i, 1] = points[kMedoids.Clusters[i].Centroid[0]][1];
                }

                int[] clusterAssignments = new int[indices.Length];
                kMedoids.Clusters.Decide(indices, clusterAssignments);

                double[,] ellipsoids = new double[kMedoids.K, 3];

                for (int i = 0; i < kMedoids.K; i++)
                {
                    double varianceX = 0;
                    double varianceY = 0;
                    double covariance = 0;
                    int count = 0;

                    for (int j = 0; j < indices.Length; j++)
                    {
                        if (clusterAssignments[j] == i)
                        {
                            varianceX += (points[j][0] - centroids[i, 0]) * (points[j][0] - centroids[i, 0]);
                            varianceY += (points[j][1] - centroids[i, 1]) * (points[j][1] - centroids[i, 1]);
                            covariance += (points[j][0] - centroids[i, 0]) * (points[j][1] - centroids[i, 1]);
                            count++;
                        }
                    }

                    varianceX /= count;
                    varianceY /= count;
                    covariance /= count;


                    Matrix<double> covarianceMatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { varianceX, covariance }, { covariance, varianceY } });
                    Evd<double> evd = covarianceMatrix.Evd();

                    double r1 = Math.Sqrt(10.5797 * evd.EigenValues[0].Real);
                    double r2 = Math.Sqrt(10.5797 * evd.EigenValues[1].Real);

                    double theta = Math.Atan2(evd.EigenVectors[1, 0], evd.EigenVectors[0, 0]);

                    ellipsoids[i, 0] = r1;
                    ellipsoids[i, 1] = r2;
                    ellipsoids[i, 2] = theta;
                }

                double[] silhouetteScores = new double[indices.Length];

                Parallel.For(0, indices.Length, i =>
                {
                    double[] distances = new double[kMedoids.K];
                    int[] counts = new int[kMedoids.K];

                    for (int j = 0; j < indices.Length; j++)
                    {
                        if (j != i)
                        {
                            distances[clusterAssignments[j]] += distance.Distance(i, j);
                            counts[clusterAssignments[j]]++;
                        }
                    }

                    for (int j = 0; j < kMedoids.K; j++)
                    {
                        distances[j] /= counts[j];
                    }

                    if (counts[clusterAssignments[i]] > 0 && indices.Length - 1 - counts[clusterAssignments[i]] > 0)
                    {
                        double a = distances[clusterAssignments[i]];
                        double b = (from el in Enumerable.Range(0, kMedoids.K) where el != clusterAssignments[i] select distances[el]).Min();

                        silhouetteScores[i] = (b - a) / Math.Max(a, b);
                    }
                    else
                    {
                        silhouetteScores[i] = 0;
                    }
                });

                double clusterSizeVariance = (from el in Enumerable.Range(0, kMedoids.K) select (double)clusterAssignments.Count(x => x == el)).Variance();

                double squaredError = 0;

                for (int i = 0; i < points.Length; i++)
                {
                    double dist = distance.Distance(i, kMedoids.Clusters[clusterAssignments[i]].Centroid[0]);

                    squaredError += dist * dist;
                }

                return (centroids, ellipsoids, clusterAssignments, silhouetteScores.Average(), clusterSizeVariance, squaredError);
            }
            else
            {
                double minSum = double.MaxValue;
                int minIndex = -1;
                object minLock = new object();

                Parallel.For(0, points.Length, i =>
                {
                    double sum = 0;

                    for (int j = 0; j < points.Length; j++)
                    {
                        if (j != i)
                        {
                            sum += distMat[Math.Min(i, j), Math.Max(i, j)];
                        }
                    }

                    lock (minLock)
                    {
                        if (sum < minSum)
                        {
                            minSum = sum;
                            minIndex = i;
                        }
                    }
                });

                double[,] centroids = new double[1, 2] { { points[minIndex][0], points[minIndex][1] } };

                int[] clusterAssignments = new int[points.Length];

                double[,] ellipsoids = new double[1, 3];

                {
                    double varianceX = 0;
                    double varianceY = 0;
                    double covariance = 0;
                    int count = 0;

                    for (int j = 0; j < points.Length; j++)
                    {
                        varianceX += (points[j][0] - centroids[0, 0]) * (points[j][0] - centroids[0, 0]);
                        varianceY += (points[j][1] - centroids[0, 1]) * (points[j][1] - centroids[0, 1]);
                        covariance += (points[j][0] - centroids[0, 0]) * (points[j][1] - centroids[0, 1]);
                        count++;
                    }

                    varianceX /= count;
                    varianceY /= count;
                    covariance /= count;


                    Matrix<double> covarianceMatrix = Matrix<double>.Build.DenseOfArray(new double[,] { { varianceX, covariance }, { covariance, varianceY } });
                    Evd<double> evd = covarianceMatrix.Evd();

                    double r1 = Math.Sqrt(10.5797 * evd.EigenValues[0].Real);
                    double r2 = Math.Sqrt(10.5797 * evd.EigenValues[1].Real);

                    double theta = Math.Atan2(evd.EigenVectors[1, 0], evd.EigenVectors[0, 0]);

                    ellipsoids[0, 0] = r1;
                    ellipsoids[0, 1] = r2;
                    ellipsoids[0, 2] = theta;
                }

                double clusterSizeVariance = 0;

                double squaredError = minSum;

                return (centroids, ellipsoids, clusterAssignments, 0, clusterSizeVariance, squaredError);
            }
        }



        private PointClustering(double[,] points, Action<double> progress)
        {
            int pointCount = points.GetLength(0);

            double[][] newPoints = new double[pointCount][];

            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new double[] { points[i, 0], points[i, 1] };
            }

            int maxClusterCount = 12;

            double[][,] allCentroids = new double[maxClusterCount][,];
            int[][] allCentroidAssignments = new int[maxClusterCount][];
            double[][,] allEllipsoids = new double[maxClusterCount][,];
            double[] allScores = new double[maxClusterCount];
            double[] allVariances = new double[maxClusterCount];

            for (int i = 0; i < allScores.Length; i++)
            {
                allScores[i] = double.MinValue;
            }

            double je1 = 0;
            double je2 = 0;

            bool onlyOneCluster = false;

            progress(0);

            {
                int i = 0;
                (allCentroids[i], allEllipsoids[i], allCentroidAssignments[i], allScores[i], allVariances[i], double errorI) = KMedoidsClustering(newPoints, i + 1);
                allVariances[i] = 0;
                je1 = errorI;

                progress((double)(i + 1) / maxClusterCount);
            }

            {
                int i = 1;
                (allCentroids[i], allEllipsoids[i], allCentroidAssignments[i], allScores[i], allVariances[i], double errorI) = KMedoidsClustering(newPoints, i + 1);

                je2 = errorI;

                double alpha = 3.090232;
                double d = 2;

                double critValue = 1 - 2 / (Math.PI * d) - alpha * Math.Sqrt(2 * (1 - 8 / (Math.PI * Math.PI * d)) / (pointCount * d));

                double pValue = (1 - 2 / (Math.PI * d) - je2 / je1) / Math.Sqrt(2 * (1 - 8 / (Math.PI * Math.PI * d)) / (pointCount * d));

                pValue = 1 - MathNet.Numerics.Distributions.Normal.CDF(0, 1, pValue);
                this.MultipleClustersPValue = pValue;

                if (je2 / je1 >= critValue)
                {
                    onlyOneCluster = true;
                }

                progress((double)(i + 1) / maxClusterCount);
            }

            int progressCount = 2;
            object progressLock = new object();

            if (!onlyOneCluster)
            {
                Parallel.For(2, maxClusterCount, i =>
                {
                    (allCentroids[i], allEllipsoids[i], allCentroidAssignments[i], allScores[i], allVariances[i], double errorI) = KMedoidsClustering(newPoints, i + 1);

                    lock (progressLock)
                    {
                        progressCount++;
                        progress((double)progressCount / maxClusterCount);
                    }
                });
            }

            progress(1);

            if (!onlyOneCluster)
            {
                double maxScore = allScores.Max();

                int bestSizeIndex = (from el in Enumerable.Range(0, maxClusterCount) where allScores[el] >= maxScore * 0.95 orderby allVariances[el] ascending select el).First();

                if (maxScore <= 0.5)
                {
                    bestSizeIndex = 0;
                }

                this.ClusterCount = bestSizeIndex + 1;
                this.Centroids = allCentroids[bestSizeIndex];
                this.CentroidAssignments = allCentroidAssignments[bestSizeIndex];
                this.Ellipsoids = allEllipsoids[bestSizeIndex];
                this.Points = points;
                this.ClusterNumberScores = allScores;
                this.ClusterNumberSizeVariances = allVariances;
            }
            else
            {
                int bestSizeIndex = 0;

                this.ClusterCount = bestSizeIndex + 1;
                this.Centroids = allCentroids[bestSizeIndex];
                this.CentroidAssignments = allCentroidAssignments[bestSizeIndex];
                this.Ellipsoids = allEllipsoids[bestSizeIndex];
                this.Points = points;
                this.ClusterNumberScores = allScores;
                this.ClusterNumberSizeVariances = allVariances;
            }
        }

        private PointClustering(double[,] distanceMatrix, double[,] points, Action<double> progress)
        {
            int pointCount = points.GetLength(0);

            double[][] newPoints = new double[pointCount][];

            for (int i = 0; i < newPoints.Length; i++)
            {
                newPoints[i] = new double[] { points[i, 0], points[i, 1] };
            }

            int maxClusterCount = (int)Math.Round((Math.Sqrt(newPoints.Length)) / 5) * 5 + 2;

            double[][,] allCentroids = new double[maxClusterCount][,];
            int[][] allCentroidAssignments = new int[maxClusterCount][];
            double[][,] allEllipsoids = new double[maxClusterCount][,];
            double[] allScores = new double[maxClusterCount];
            double[] allVariances = new double[maxClusterCount];

            for (int i = 0; i < allScores.Length; i++)
            {
                allScores[i] = double.MinValue;
            }

            progress(0);

            int progressCount = 0;
            object progressLock = new object();

            Parallel.For(0, maxClusterCount, i =>
            {
                (allCentroids[i], allEllipsoids[i], allCentroidAssignments[i], allScores[i], allVariances[i], double errorI) = KMedoidsClustering(distanceMatrix, newPoints, i + 1);

                if (i == 0)
                {
                    allVariances[i] = 0;
                }

                lock (progressLock)
                {
                    progressCount++;
                    progress((double)progressCount / maxClusterCount);
                }
            });

            progress(1);

            double maxScore = allScores.Max();

            int bestSizeIndex = (from el in Enumerable.Range(0, maxClusterCount) where allScores[el] >= maxScore * 0.95 orderby allVariances[el] ascending select el).First();

            if (maxScore <= 0.5)
            {
                bestSizeIndex = 0;
            }

            this.ClusterCount = bestSizeIndex + 1;
            this.Centroids = allCentroids[bestSizeIndex];
            this.CentroidAssignments = allCentroidAssignments[bestSizeIndex];
            this.Ellipsoids = allEllipsoids[bestSizeIndex];
            this.Points = points;
            this.ClusterNumberScores = allScores;
            this.ClusterNumberSizeVariances = allVariances;
            this.MultipleClustersPValue = double.NaN;
        }
    }
}
