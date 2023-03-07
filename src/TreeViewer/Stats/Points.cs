using Accord.MachineLearning;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Factorization;
using MathNet.Numerics.Statistics;
using PhyloTree;
using PhyloTree.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;


namespace TreeViewer.Stats
{
    internal static class Points
    {
        /*private static double Halton(int i, int b)
        {
            double f = 1;
            double r = 0;

            while (i > 0)
            {
                f = f / b;
                r = r + f * (i % b);
                i = i / b;
            }

            return r;
        }*/

        private static Point Rotate(Point x, double theta)
        {
            return new Point(x.X * Math.Cos(theta) - x.Y * Math.Sin(theta), x.X * Math.Sin(theta) + x.Y * Math.Cos(theta));
        }

        private static Point Scale(Point x, double scaleX, double scaleY)
        {
            return new Point(x.X * scaleX, x.Y * scaleY);
        }

        private static Point Translate(Point x, double deltaX, double deltaY)
        {
            return new Point(x.X + deltaX, x.Y + deltaY);
        }

        public static Page GetPlot(double[,] distMat, double[,] points, string xAxisName, string yAxisName, string title, string tag, out Dictionary<string, (Colour, Colour, string)> interactiveDescriptions, out Dictionary<string, Action<Avalonia.Controls.Window, IList<TreeNode>>> clickActions, bool interactive = false)
        {
            interactiveDescriptions = null;
            clickActions = null;

            if (interactive)
            {
                interactiveDescriptions = new Dictionary<string, (Colour, Colour, string)>();
                clickActions = new Dictionary<string, Action<Avalonia.Controls.Window, IList<TreeNode>>>();
            }

            int pointCount = points.GetLength(0);

            double minX = double.MaxValue;
            double maxX = double.MinValue;

            double minY = double.MaxValue;
            double maxY = double.MinValue;

            for (int i = 0; i < pointCount; i++)
            {
                minX = Math.Min(minX, points[i, 0]);
                maxX = Math.Max(maxX, points[i, 0]);

                minY = Math.Min(minY, points[i, 1]);
                maxY = Math.Max(maxY, points[i, 1]);
            }


            double plotWidth = 400;
            double plotHeight = 225;
            double axisMargin = 10;
            double axisLength = 10;
            Colour plotColour = Colour.FromRgb(0, 114, 178);

            Font axisFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 10);
            Font axisLegend = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.Helvetica), 12);
            Font titleFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.HelveticaBold), 14);

            Page pag = new Page(1, 1);
            Graphics gpr = pag.Graphics;

            gpr.Save();

            Colour[] clusterColours = new Colour[] {
                Colour.FromRgb(0, 114, 178),
                Colour.FromRgb(213, 94, 0),
                Colour.FromRgb(0, 158, 115),
                Colour.FromRgb(86, 180, 233),
                Colour.FromRgb(240, 228, 66),
                Colour.FromRgb(204, 121, 167),
                Colour.FromRgb(230, 159, 0)
            };

            PointClustering clustering;

            if (GlobalSettings.Settings.ClusterAccordingToRawDistances)
            {
                clustering = PointClustering.GetClustering(distMat, points, x => { });
            }
            else
            {
                clustering = PointClustering.GetClustering(points, x => { });
            }

            int clusterCount = clustering.ClusterCount;
            double[,] centroids = (double[,])clustering.Centroids.Clone();
            double[,] ellipsoids = (double[,])clustering.Ellipsoids.Clone();
            int[] centroidAssignments = (int[])clustering.CentroidAssignments.Clone();

            double[][] newCentroids = (from el in Enumerable.Range(0, clusterCount) orderby (centroids[el, 0] * centroids[el, 0] + centroids[el, 1] * centroids[el, 1]) select new double[] { centroids[el, 0], centroids[el, 1] }).ToArray();
            double[][] newEllipsoids = (from el in Enumerable.Range(0, clusterCount) orderby (centroids[el, 0] * centroids[el, 0] + centroids[el, 1] * centroids[el, 1]) select new double[] { ellipsoids[el, 0], ellipsoids[el, 1], ellipsoids[el, 2] }).ToArray();
            int[] newAssignments = (from el in Enumerable.Range(0, clusterCount) orderby (centroids[el, 0] * centroids[el, 0] + centroids[el, 1] * centroids[el, 1]) select el).ToArray();

            int[] newNewAssignments = new int[newAssignments.Length];

            for (int i = 0; i < newAssignments.Length; i++)
            {
                newNewAssignments[newAssignments[i]] = i;
            }

            newAssignments = newNewAssignments;

            for (int i = 0; i < clusterCount; i++)
            {
                centroids[i, 0] = newCentroids[i][0];
                centroids[i, 1] = newCentroids[i][1];

                ellipsoids[i, 0] = newEllipsoids[i][0];
                ellipsoids[i, 1] = newEllipsoids[i][1];
                ellipsoids[i, 2] = newEllipsoids[i][2];
            }

            gpr.SetClippingPath(-axisMargin, -axisMargin, plotWidth + axisMargin * 2, plotHeight + axisMargin * 2);

            if (clusterCount > 1)
            {
                for (int i = 0; i < clusterCount; i++)
                {
                    if (ellipsoids != null && ellipsoids[i, 0] > 1e-7 && ellipsoids[i, 1] > 1e-7)
                    {
                        Colour col = clusterColours[i % clusterColours.Length];
                        Colour bgCol = (!string.IsNullOrEmpty(tag) || interactive) ? Colour.FromRgb(243, 243, 243) : Colours.White;

                        gpr.Save();

                        gpr.Translate(0, plotHeight);
                        gpr.Scale(plotWidth / (maxX - minX), -plotHeight / (maxY - minY));
                        gpr.Translate(centroids[i, 0] - minX, centroids[i, 1] - minY);
                        gpr.Rotate(ellipsoids[i, 2]);
                        gpr.Scale(ellipsoids[i, 0], ellipsoids[i, 1]);

                        RadialGradientBrush brs = new RadialGradientBrush(new Point(0, 0), new Point(0, 0), 1,
                            new VectSharp.GradientStop(Colour.FromRgba(col.R, col.G, col.B, 0), 1),
                            new VectSharp.GradientStop(Colour.FromRgba(col.R, col.G, col.B, 0.5), 0));

                        gpr.FillPath(new GraphicsPath().Arc(0, 0, 1, 0, 2 * Math.PI), brs);

                        gpr.Restore();
                    }
                }

                if (interactive)
                {
                    int[] sortedClusters = (from el in Enumerable.Range(0, clusterCount) orderby ellipsoids[el, 0] * ellipsoids[el, 1] descending select el).ToArray();

                    for (int k = 0; k < clusterCount; k++)
                    {
                        int i = sortedClusters[k];

                        if (ellipsoids != null && ellipsoids[i, 0] > 1e-7 && ellipsoids[i, 1] > 1e-7)
                        {
                            Colour col = clusterColours[i % clusterColours.Length];
                            Colour bgCol = (!string.IsNullOrEmpty(tag) || interactive) ? Colour.FromRgb(243, 243, 243) : Colours.White;

                            if (interactive)
                            {
                                GraphicsPath ellipse1 = new GraphicsPath().Arc(0, 0, 1, 0, 2 * Math.PI);
                                ellipse1 = ellipse1.Linearise(0.01).Transform(x => Translate(Scale(Translate(Translate(Rotate(Scale(x, ellipsoids[i, 0], ellipsoids[i, 1]), ellipsoids[i, 2]), centroids[i, 0], centroids[i, 1]), -minX, -minY), plotWidth / (maxX - minX), -plotHeight / (maxY - minY)), 0, plotHeight));

                                string clusterTag = Guid.NewGuid().ToString();

                                List<int> clusterPoints = new List<int>();

                                for (int j = 0; j < centroidAssignments.Length; j++)
                                {
                                    if (newAssignments[centroidAssignments[j]] == i)
                                    {
                                        clusterPoints.Add(j);
                                    }
                                }

                                //int countPoints = centroidAssignments.Count(x => newAssignments[x] == i);
                                int countPoints = clusterPoints.Count;

                                interactiveDescriptions.Add(clusterTag, (Colour.FromRgba(0, 0, 0, 0), Colour.FromRgb(col.R * 0.5 + bgCol.R * 0.5, col.G * 0.5 + bgCol.G * 0.5, col.B * 0.5 + bgCol.B * 0.5), "Cluster " + (i + 1).ToString() + ": " + countPoints.ToString() + " points (" + ((double)countPoints / centroidAssignments.Length).ToString("0%") + ")"));

                                gpr.FillPath(ellipse1, Colour.FromRgba(0, 0, 0, 0), tag: clusterTag);

                                clickActions.Add(clusterTag, async (win, trees) => {
                                    
                                    string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

                                    BinaryTree.WriteAllTrees(from el in clusterPoints select trees[el], tempFile);

                                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () => { await GlobalSettings.Settings.MainWindows[0].LoadFile(tempFile, true); });
                                });
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < pointCount; i++)
            {
                if (!interactive)
                {
                    string cluster = clusterCount > 1 ? ("(cluster " + (newAssignments[centroidAssignments[i]] + 1).ToString() + ")") : "";
                    string pointTag = "Tree#" + (i + 1).ToString() + cluster;

                    gpr.FillPath(new GraphicsPath().Arc((points[i, 0] - minX) / (maxX - minX) * plotWidth, plotHeight - (points[i, 1] - minY) / (maxY - minY) * plotHeight, 2, 0, 2 * Math.PI), clusterColours[newAssignments[centroidAssignments[i]] % clusterColours.Length], tag: pointTag);
                }
                else
                {
                    Colour col = clusterColours[newAssignments[centroidAssignments[i]] % clusterColours.Length];

                    string pointTag = Guid.NewGuid().ToString();
                    string cluster = clusterCount > 1 ? (" (cluster " + (newAssignments[centroidAssignments[i]] + 1).ToString() + ")") : "";

                    interactiveDescriptions.Add(pointTag, (col, Colour.FromRgb(col.R * 0.66 + 0.33, col.G * 0.66 + 0.33, col.B * 0.66 + 0.33), "Tree #" + (i + 1).ToString() + cluster));

                    gpr.FillPath(new GraphicsPath().Arc((points[i, 0] - minX) / (maxX - minX) * plotWidth, plotHeight - (points[i, 1] - minY) / (maxY - minY) * plotHeight, 2, 0, 2 * Math.PI), col, tag: pointTag);
                }
            }

            if (clusterCount > 1)
            {
                for (int i = 0; i < clusterCount; i++)
                {
                    double cX = (centroids[i, 0] - minX) / (maxX - minX) * plotWidth;
                    double cY = plotHeight - (centroids[i, 1] - minY) / (maxY - minY) * plotHeight;

                    GraphicsPath star = new GraphicsPath();

                    for (int j = 0; j < 10; j++)
                    {
                        star.LineTo(cX + 6 * Math.Cos(Math.PI / 5 * j) * (j % 2 == 0 ? 1 : 0.33), cY + 6 * Math.Sin(Math.PI / 5 * j) * (j % 2 == 0 ? 1 : 0.33));
                    }

                    star.Close();

                    Colour col = clusterColours[i % clusterColours.Length];
                    Colour bgCol = (!string.IsNullOrEmpty(tag) || interactive) ? Colour.FromRgb(243, 243, 243) : Colours.White;

                    gpr.StrokePath(star, Colour.FromRgb(bgCol.R * 0.5 + col.R * 0.5, bgCol.G * 0.5 + col.G * 0.5, bgCol.B * 0.5 + col.B * 0.5), 2, lineJoin: LineJoins.Round);

                    if (!interactive)
                    {
                        gpr.FillPath(star, col);
                    }
                    else
                    {
                        double minDist = double.MaxValue;
                        int minIndex = -1;

                        for (int j = 0; j < pointCount; j++)
                        {
                            double dist = (points[j, 0] - centroids[i, 0]) * (points[j, 0] - centroids[i, 0]) + (points[j, 1] - centroids[i, 1]) * (points[j, 1] - centroids[i, 1]);

                            if (dist < minDist)
                            {
                                minDist = dist;
                                minIndex = j;
                            }
                        }

                        string medoidTag = Guid.NewGuid().ToString();
                        interactiveDescriptions.Add(medoidTag, (col, Colour.FromRgb(col.R * 0.66 + 0.33, col.G * 0.66 + 0.33, col.B * 0.66 + 0.33), "Cluster " + (i + 1).ToString() + " medoid: tree #" + (minIndex + 1).ToString()));

                        gpr.FillPath(star, col, tag: medoidTag);
                    }

                }
            }

            gpr.Restore();

            {

                GraphicsPath yAxis = new GraphicsPath().MoveTo(-axisMargin - axisLength, 0).LineTo(-axisMargin, 0).LineTo(-axisMargin, plotHeight).LineTo(-axisMargin - axisLength, plotHeight);

                for (int i = 1; i < 5; i++)
                {
                    yAxis.MoveTo(-axisMargin - axisLength, i * plotHeight / 5).LineTo(-axisMargin, i * plotHeight / 5);
                }

                double yWidth = 0;

                gpr.StrokePath(yAxis, Colours.Black);

                for (int i = 0; i <= 5; i++)
                {
                    double val = minY + (maxY - minY) / 5 * i;

                    string text = val.ToString(val.GetDigits());

                    double width = axisFont.MeasureText(text).Width;

                    yWidth = Math.Max(width, yWidth);

                    gpr.FillText(-axisMargin - axisLength - 3 - width, (5 - i) * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                }

                gpr.Save();
                gpr.Translate(-axisMargin - axisLength - 3 - yWidth - axisLegend.FontSize * 1.4, plotHeight * 0.5);
                gpr.Rotate(-Math.PI * 0.5);

                gpr.FillText(-axisLegend.MeasureText(yAxisName).Width * 0.5, 0, yAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

                gpr.Restore();

            }

            {

                GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin).LineTo(plotWidth, plotHeight + axisMargin + axisLength);

                for (int i = 1; i < 7; i++)
                {
                    xAxis.MoveTo(i * plotWidth / 7, plotHeight + axisMargin + axisLength).LineTo(i * plotWidth / 7, plotHeight + axisMargin);
                }

                gpr.StrokePath(xAxis, Colours.Black);

                for (int i = 0; i <= 7; i++)
                {
                    double val = minX + (maxX - minX) / 7 * i;

                    string text = val.ToString(val.GetDigits());

                    gpr.FillText(i * plotWidth / 7 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
                }

                gpr.FillText(plotWidth * 0.5 - axisLegend.MeasureText(xAxisName).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, xAxisName, axisLegend, Colours.Black, TextBaselines.Baseline);

            }

            if (clusterCount > 1)
            {
                gpr.Save();

                gpr.Translate(plotWidth + axisMargin * 4, 0);

                double subPlotWidth = 120;

                int minY2 = 2;
                int maxY2 = clustering.ClusterNumberScores.Length;

                double minX2 = clustering.ClusterNumberScores.Skip(1).Min();
                double maxX2 = clustering.ClusterNumberScores.Skip(1).Max();

                double minZ2 = clustering.ClusterNumberSizeVariances.Skip(1).Min();
                double maxZ2 = clustering.ClusterNumberSizeVariances.Skip(1).Max();

                {

                    GraphicsPath xAxis = new GraphicsPath().MoveTo(0, plotHeight + axisMargin + axisLength).LineTo(0, plotHeight + axisMargin).LineTo(subPlotWidth, plotHeight + axisMargin).LineTo(subPlotWidth, plotHeight + axisMargin + axisLength);

                    for (int i = 1; i < 2; i++)
                    {
                        xAxis.MoveTo(i * subPlotWidth / 2, plotHeight + axisMargin + axisLength).LineTo(i * subPlotWidth / 2, plotHeight + axisMargin);
                    }

                    gpr.StrokePath(xAxis, Colours.Black);

                    for (int i = 0; i <= 2; i++)
                    {
                        double val = minX2 + (maxX2 - minX2) / 2 * i;

                        string text = val.ToString(2);

                        gpr.FillText(i * subPlotWidth / 2 - axisFont.MeasureText(text).Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent, text, axisFont, Colours.Black, TextBaselines.Baseline);
                    }

                    gpr.FillText(subPlotWidth * 0.5 - axisLegend.MeasureText("Average silhouette score").Width * 0.5, plotHeight + axisMargin + axisLength + 3 + axisFont.Ascent + axisLegend.FontSize * 1.4, "Average silhouette score", axisLegend, Colours.Black, TextBaselines.Baseline);

                }

                {
                    GraphicsPath yAxis = new GraphicsPath().MoveTo(subPlotWidth + axisMargin + axisLength, 0).LineTo(subPlotWidth + axisMargin, 0).LineTo(subPlotWidth + axisMargin, plotHeight).LineTo(subPlotWidth + axisMargin + axisLength, plotHeight);

                    for (int i = 1; i < 5; i++)
                    {
                        yAxis.MoveTo(subPlotWidth + axisMargin + axisLength, i * plotHeight / 5).LineTo(subPlotWidth + axisMargin, i * plotHeight / 5);
                    }

                    double yWidth = 0;

                    gpr.StrokePath(yAxis, Colours.Black);

                    for (int i = 0; i <= 5; i++)
                    {
                        string text = (minY2 + (maxY2 - minY2) / 5 * i).ToString();

                        double width = axisFont.MeasureText(text).Width;

                        yWidth = Math.Max(width, yWidth);

                        gpr.FillText(subPlotWidth + axisMargin + axisLength + 3, i * plotHeight / 5, text, axisFont, Colours.Black, TextBaselines.Middle);
                    }

                    gpr.Save();
                    gpr.Translate(subPlotWidth + axisMargin + axisLength + 3 + yWidth + axisLegend.FontSize * 1.4, plotHeight * 0.5);
                    gpr.Rotate(-Math.PI * 0.5);

                    gpr.FillText(-(axisLegend.MeasureText("Number of clusters").Width) * 0.5, 0, "Number of clusters", axisLegend, Colours.Black, TextBaselines.Baseline);

                    gpr.Restore();
                }

                GraphicsPath scorePath = new GraphicsPath();

                for (int i = 1; i < clustering.ClusterNumberScores.Length; i++)
                {
                    double x = (clustering.ClusterNumberScores[i] - minX2) / (maxX2 - minX2) * subPlotWidth;
                    double y = (i + 1.0 - minY2) / (maxY2 - minY2) * plotHeight;

                    scorePath.LineTo(x, y);
                }

                gpr.StrokePath(scorePath, Colour.FromRgb(180, 180, 180), lineDash: new LineDash(5, 5, 0));

                for (int i = 1; i < clustering.ClusterNumberScores.Length; i++)
                {
                    double r = 2 + (clustering.ClusterNumberSizeVariances[i] - minZ2) / (maxZ2 - minZ2) * 6;
                    double x = (clustering.ClusterNumberScores[i] - minX2) / (maxX2 - minX2) * subPlotWidth;
                    double y = (i + 1.0 - minY2) / (maxY2 - minY2) * plotHeight;

                    if (i + 1 != clusterCount)
                    {
                        if (!interactive)
                        {
                            gpr.FillPath(new GraphicsPath().Arc(x, y, r, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128));
                        }
                        else
                        {
                            string pointTag = Guid.NewGuid().ToString();
                            interactiveDescriptions.Add(pointTag, (Colour.FromRgb(128, 128, 128), Colour.FromRgb(180, 180, 180), (i + 1).ToString() + " clusters\nAverage silhouette score: " + clustering.ClusterNumberScores[i].ToString(clustering.ClusterNumberScores[i].GetDigits()) + "\nCluster size variance: " + clustering.ClusterNumberSizeVariances[i].ToString(clustering.ClusterNumberSizeVariances[i].GetDigits())));

                            gpr.FillPath(new GraphicsPath().Arc(x, y, r, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128), tag: pointTag);
                        }
                    }
                    else
                    {
                        if (!interactive)
                        {
                            gpr.FillPath(new GraphicsPath().Arc(x, y, r, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128));
                        }
                        else
                        {
                            string pointTag = Guid.NewGuid().ToString();
                            interactiveDescriptions.Add(pointTag, (Colour.FromRgb(128, 128, 128), Colour.FromRgb(180, 180, 180), (i + 1).ToString() + " clusters\nAverage silhouette score: " + clustering.ClusterNumberScores[i].ToString(clustering.ClusterNumberScores[i].GetDigits()) + "\nCluster size variance: " + clustering.ClusterNumberSizeVariances[i].ToString(clustering.ClusterNumberSizeVariances[i].GetDigits())));

                            gpr.FillPath(new GraphicsPath().Arc(x, y, r, 0, 2 * Math.PI), Colour.FromRgb(128, 128, 128), tag: pointTag);
                        }

                        gpr.FillPath(new GraphicsPath().MoveTo(x, y - r - 2).LineTo(x - 3, y - r - 8).LineTo(x + 3, y - r - 8).Close(), Colour.FromRgb(128, 128, 128));
                    }
                }

                {
                    double xThreshold = (clustering.ClusterNumberScores.Max() * 0.95 - minX2) / (maxX2 - minX2) * subPlotWidth;

                    gpr.StrokePath(new GraphicsPath().MoveTo(xThreshold, -axisMargin).LineTo(xThreshold, plotHeight + axisMargin), Colours.Black, lineDash: new LineDash(5, 5, 0));
                }

                gpr.Restore();
            }

            Rectangle bounds = gpr.GetBounds();

            gpr.FillText(bounds.Location.X + bounds.Size.Width * 0.5 - titleFont.MeasureText(title).Width * 0.5, bounds.Location.Y - 5, title, titleFont, Colours.Black, TextBaselines.Bottom);

            if (!string.IsNullOrEmpty(tag))
            {
                bounds = gpr.GetBounds();

                gpr.FillRectangle(bounds.Location.X, bounds.Location.Y, bounds.Size.Width, bounds.Size.Height, Colour.FromRgba(0, 0, 0, 0), tag: "plotBounds/" + tag);

                Font buttonFont = new Font(FontFamily.ResolveFontFamily(FontFamily.StandardFontFamilies.CourierBold), 7.5);

                for (int i = 0; i < 4; i++)
                {
                    double x = bounds.Location.X + bounds.Size.Width - 24 - 29 * i;
                    gpr.FillRectangle(x, bounds.Location.Y, 24, 24, Colour.FromRgba(0, 0, 0, 0), tag: "buttonBg[" + i.ToString() + "]/" + tag);

                    switch (i)
                    {
                        case 0:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("PDF").Width * 0.5, 16, "PDF", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[0]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 1:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("SVG").Width * 0.5, 16, "SVG", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[1]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 2:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(6, 0).LineTo(10, 0).LineTo(10, 5).LineTo(13, 5).LineTo(8, 10).LineTo(3, 5).LineTo(6, 5).Close();
                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.FillPath(new GraphicsPath().AddText(8 - buttonFont.MeasureText("CSV").Width * 0.5, 16, "CSV", buttonFont, TextBaselines.Bottom), Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[2]/" + tag);

                                gpr.Restore();
                            }
                            break;

                        case 3:
                            {
                                gpr.Save();
                                gpr.Translate(x + 4, bounds.Location.Y + 4);

                                GraphicsPath arrowSymbol = new GraphicsPath().MoveTo(0, 0).LineTo(5, 0).LineTo(3.5, 1.5).LineTo(6, 4).LineTo(4, 6).LineTo(1.5, 3.5).LineTo(0, 5).Close();

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);
                                gpr.RotateAt(Math.PI / 2, new Point(8, 8));

                                gpr.FillPath(arrowSymbol, Colour.FromRgba(0, 0, 0, 0), tag: "buttonSymbol[3]/" + tag);

                                gpr.Restore();
                            }
                            break;

                    }
                }
            }

            pag.Crop();

            return pag;
        }
    }
}
