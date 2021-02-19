using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace NodeAgeDistributions
{
    /// <summary>
    /// This module can be used to draw age distributions on the tree. The age distributions must have been previously set up using the
    /// _Set up age distributions_ module (id `a1ccf05a-cf3c-4ca4-83be-af56f501c2a6`). This module can only be used if the tree is being
    /// drawn using _Rectangular_ coordinates.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Age distributions";
        public const string HelpText = "Plots node age distributions.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "5dbe1f3c-dbea-49b3-8f04-f319aefca534";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "StateData", "InstanceStateData:"),
                
                /// <param name="Plot type:">
                /// This parameter determines the kind of plot used to draw the age distributions. If the value is `Histogram`, a histogram
                /// is drawn at each node, displaying the age distributions. The bars of the histogram are centered so that the plot looks
                /// similar to a violin plot. The width of the bars of the histogram is chosen automatically.
                /// 
                /// If the value is `Envelope`, a violin plot is drawn, using the same samples that would be used to draw the histogram;
                /// instead of drawing rectangular bars, a smooth spline is interpolated between the sample points to produce a smooth-looking
                /// plot. Please note that this is not a kernel density estimation of the age distribution (as that would be too expensive to
                /// draw in real time).
                /// </param>
                ( "Plot type:", "ComboBox:0[\"Histogram\",\"Envelope\"]" ),
                
                /// <param name="Show on:">
                /// This parameter determines on which nodes the age distributions are shown. If the value is `Leaves`, the
                /// age distributions are only shown for terminal nodes (nodes with no child nodes). If the value is `Internal
                /// nodes` they are shown only for internal nodes (nodes which have at least one child).
                /// If the value is `All nodes`, age distributions are shown for both leaves and internal nodes.
                /// </param>
                ( "Show on:", "ComboBox:1[\"Leaves\",\"Internal nodes\",\"All nodes\"]"),
                
                /// <param name="Height:">
                /// This parameter determines the height of the age distribution plot for each node.
                /// </param>
                ( "Height:", "NumericUpDown:10[\"0\",\"Infinity\"]"),
                
                /// <param name="Auto colour by node">
                /// If this check box is checked, the colour of each age distribution is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible
                /// if the same tree is rendered multiple times.
                /// </param>
                ( "Auto colour by node", "CheckBox:true"),
                
                /// <param name="Opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto colour by node](#auto-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Opacity:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Colour:">
                /// This parameter determines the colour used to draw each age distribution (if the [Auto colour by node](#auto-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>() { };

            if ((bool)currentParameterValues["Auto colour by node"])
            {
                controlStatus.Add("Opacity:", ControlStatus.Enabled);
                controlStatus.Add("Colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Opacity:", ControlStatus.Hidden);
                controlStatus.Add("Colour:", ControlStatus.Enabled);
            }

            parametersToChange = new Dictionary<string, object>() { };

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            bool autoColourByNode = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];
            ColourFormatterOptions customColour = (ColourFormatterOptions)parameterValues["Colour:"];
            Colour defaultFill = customColour.DefaultColour;
            Func<object, Colour?> fillFormatter = customColour.Formatter;

            double maxHeight = (double)parameterValues["Height:"];
            int plotType = (int)parameterValues["Plot type:"];
            int showWhere = (int)parameterValues["Show on:"];

            Point scalePoint;

            if (!coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out scalePoint))
            {
                throw new Exception("The coordinates module is not supported!");
            }

            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            if (!stateData.Tags.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6") || !tree.Attributes.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"))
            {
                throw new Exception("The node ages have not been correctly set up!\nPlease use the \"Set up age distributions\" module.");
            }

            Dictionary<string, List<double>> ageDistributions = (Dictionary<string, List<double>>)stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"];

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            void updateMaxMin(Point pt)
            {
                minX = Math.Min(minX, pt.X);
                maxX = Math.Max(maxX, pt.X);
                minY = Math.Min(minY, pt.Y);
                maxY = Math.Max(maxY, pt.Y);
            }

            static Point sumPoint(Point pt1, Point pt2)
            {
                return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
            }

            static Point rotatePoint(Point pt, double angle)
            {
                return new Point(pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle), pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle));
            }

            static Point normalizePoint(Point pt)
            {
                double modulus = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                return new Point(pt.X / modulus, pt.Y / modulus);
            }

            static (int[], double[]) histogram(List<double> samples)
            {
                samples.Sort();
                double iqr = BayesStats.Quantile(samples, 0.75) - BayesStats.Quantile(samples, 0.25);

                double binWidth = 2 * iqr / Math.Cbrt(samples.Count);

                double min = samples.Min();
                double max = samples.Max();
                int binCount = (int)Math.Ceiling((max - min) / binWidth);

                int[] bins = new int[binCount];

                for (int i = 0; i < samples.Count; i++)
                {
                    int bin = (int)Math.Floor((samples[i] - min) / binWidth);
                    bins[Math.Min(bin, binCount - 1)]++;
                }

                return (bins, new double[] { min, max, binWidth });
            }


            static (int[], double[], double[]) histogramWithBinMeans(List<double> samples)
            {
                samples.Sort();
                double iqr = BayesStats.Quantile(samples, 0.75) - BayesStats.Quantile(samples, 0.25);

                double binWidth = 2 * iqr / Math.Cbrt(samples.Count);

                double min = samples.Min();
                double max = samples.Max();
                int binCount = (int)Math.Ceiling((max - min) / binWidth);

                int[] bins = new int[binCount];

                double[] binValues = new double[binCount];

                for (int i = 0; i < samples.Count; i++)
                {
                    int bin = (int)Math.Floor((samples[i] - min) / binWidth);
                    bins[Math.Min(bin, binCount - 1)]++;
                    binValues[Math.Min(bin, binCount - 1)] += samples[i];
                }

                for (int i = 0; i < bins.Length; i++)
                {
                    if (bins[i] > 0)
                    {
                        binValues[i] /= bins[i];
                    }
                    else
                    {
                        binValues[i] = min + binWidth * (i + 0.5);
                    }
                }

                return (bins, new double[] { min, max, binWidth }, binValues);
            }

            Point perpScale = normalizePoint(rotatePoint(scalePoint, Math.PI / 2));
            Point parallScale = normalizePoint(scalePoint);

            for (int i = 0; i < nodes.Count; i++)
            {
                if (showWhere == 2 || (showWhere == 1 && nodes[i].Children.Count > 0) || (showWhere == 0 && nodes[i].Children.Count == 0))
                {
                    List<double> distrib = ageDistributions[nodes[i].Id];

                    if (distrib.Count > 0)
                    {
                        Colour colour = defaultFill;

                        if (autoColourByNode)
                        {
                            colour = Modules.DefaultColours[Math.Abs(nodes[i].GetLeafNames().Aggregate((a, b) => a + "," + b).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                        }
                        else if (nodes[i].Attributes.TryGetValue(customColour.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                        {
                            colour = fillFormatter(fillAttributeObject) ?? defaultFill;
                        }

                        if (colour.A > 0)
                        {

                            if (plotType == 0)
                            {
                                (int[], double[]) hist = histogram(distrib);

                                int[] bins = hist.Item1;
                                double[] range = hist.Item2;

                                double maxBin = bins.Max();

                                double age = nodes[i].LongestDownstreamLength();
                                double deltaLeft = age - range[1];
                                double deltaRight = range[0] - age;

                                Point rightPoint = sumPoint(new Point(-scalePoint.X * deltaRight, -scalePoint.Y * deltaRight), coordinates[nodes[i].Id]);
                                Point leftPoint = sumPoint(new Point(scalePoint.X * deltaLeft, scalePoint.Y * deltaLeft), coordinates[nodes[i].Id]);

                                updateMaxMin(leftPoint);
                                updateMaxMin(rightPoint);

                                GraphicsPath histPth = new GraphicsPath();

                                for (int j = 0; j < bins.Length; j++)
                                {
                                    Point vertDist = new Point(perpScale.X * bins[bins.Length - j - 1] / maxBin * maxHeight, perpScale.Y * bins[bins.Length - j - 1] / maxBin * maxHeight);
                                    Point binStart = sumPoint(leftPoint, new Point(scalePoint.X * range[2] * j, scalePoint.Y * range[2] * j));
                                    Point binEnd = sumPoint(leftPoint, new Point(scalePoint.X * Math.Min(range[2] * (j + 1), range[1] - range[0]), scalePoint.Y * Math.Min(range[2] * (j + 1), range[1] - range[0])));

                                    histPth.MoveTo(sumPoint(binStart, vertDist)).LineTo(sumPoint(binStart, new Point(-vertDist.X, -vertDist.Y))).LineTo(sumPoint(binEnd, new Point(-vertDist.X, -vertDist.Y))).LineTo(sumPoint(binEnd, vertDist)).Close();
                                }

                                graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                            }
                            else if (plotType == 1)
                            {
                                (int[], double[], double[]) hist = histogramWithBinMeans(distrib);

                                int[] bins = hist.Item1;
                                double[] range = hist.Item2;
                                double[] binMeans = hist.Item3;

                                double maxBin = bins.Max();

                                double age = nodes[i].LongestDownstreamLength();
                                double deltaLeft = age - range[1];
                                double deltaRight = range[0] - age;

                                Point rightPoint = sumPoint(new Point(-scalePoint.X * deltaRight, -scalePoint.Y * deltaRight), coordinates[nodes[i].Id]);
                                Point leftPoint = sumPoint(new Point(scalePoint.X * deltaLeft, scalePoint.Y * deltaLeft), coordinates[nodes[i].Id]);

                                updateMaxMin(leftPoint);
                                updateMaxMin(rightPoint);

                                List<Point> pointsUp = new List<Point>();

                                GraphicsPath histPth = new GraphicsPath();

                                pointsUp.Add(sumPoint(leftPoint, new Point(perpScale.X / maxBin * maxHeight, perpScale.Y / maxBin * maxHeight)));

                                for (int j = 0; j < bins.Length; j++)
                                {
                                    Point vertDist = new Point(perpScale.X * bins[bins.Length - j - 1] / maxBin * maxHeight, perpScale.Y * bins[bins.Length - j - 1] / maxBin * maxHeight);

                                    Point horiz = sumPoint(leftPoint, new Point(scalePoint.X * Math.Min(binMeans[j] - range[0], range[1] - range[0]), scalePoint.Y * Math.Min(binMeans[j] - range[0], range[1] - range[0])));

                                    pointsUp.Add(sumPoint(horiz, vertDist));
                                }

                                histPth.AddSmoothSpline(pointsUp.ToArray());

                                List<Point> pointsDown = new List<Point>();

                                for (int j = bins.Length - 1; j >= 0; j--)
                                {
                                    Point vertDist = new Point(-perpScale.X * bins[bins.Length - j - 1] / maxBin * maxHeight, -perpScale.Y * bins[bins.Length - j - 1] / maxBin * maxHeight);
                                    Point horiz = sumPoint(leftPoint, new Point(scalePoint.X * Math.Min(binMeans[j] - range[0], range[1] - range[0]), scalePoint.Y * Math.Min(binMeans[j] - range[0], range[1] - range[0])));

                                    pointsDown.Add(sumPoint(horiz, vertDist));
                                }

                                pointsDown.Add(sumPoint(leftPoint, new Point(perpScale.X / maxBin * maxHeight, perpScale.Y / maxBin * maxHeight)));

                                histPth.AddSmoothSpline(pointsDown.ToArray());


                                histPth.Close();

                                graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                            }
                        }
                    }
                }
            }



            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }
    }
}
