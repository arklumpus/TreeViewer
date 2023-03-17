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

using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace NodeAgeDistributions
{
    /// <summary>
    /// This module can be used to draw age distributions on the tree. The age distributions must have been previously set up using the
    /// _Set up age distributions_ module (id `a1ccf05a-cf3c-4ca4-83be-af56f501c2a6`). This module can only be used if the tree is being
    /// drawn using _Rectangular_ or _Circular_ coordinates.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Age distributions";
        public const string HelpText = "Plots node age distributions.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.2.0");
        public const string Id = "5dbe1f3c-dbea-49b3-8f04-f319aefca534";
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAABlSURBVDhPYxj6gBFKg4FX7cYGIFUP4TE0bmv2B/HxAiYoDQMwzSBQDzUQL0A3AB0QNATsBTSn4wJYvcQI1PwfyiYLEPICQUC5F6A0GODwDt7oJOQFgmkB3YBGKA0CRCWkIQ8YGABWYSGHOxj3CQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACqSURBVEhLYxgFwx8wQmkM4FW7sQFI1QNx47ZmfxCbLMAMpVEAkuEg4KBqH8l4+9CKA1A+SYAJSqMDmOEwUA+1lGSAYQEeg8iyBCWI0IIGGyA5uMCRTITBuMBWID6DLxEwAg3/D2XTBOCKZKoB2gcRlAYDIi0iKeOhpCJQ6gClEiDTASKCAUjO1ViLChwRT1aRgSuSG6E0DJBdHmEti9CCiqLCbhQMe8DAAADv4UINVIWqQQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADjSURBVFhH7ZYxEsIgEEVBz+I5TG1j7WEczXgZWwutHWtv4GGUjUsGBTYkfIdC3gyzJEX+Z8kuqErl79Eco6y2p70JOzPay2FNcygzjhIk3kU2A0U0EBCEmxjKgF29C9RE1MCACMzEnOMH/PHQ6l2axXKjH7fjlZ8n4RlIFLdkm+jLcKRwjLMZ9zHl2hkAiUtEjWkj/uR5EVIa0U8pvwUcy/6ELhOMZB1SXh+gmqbaNtPm/UYk+4QMdsJEE5Dj2dsCF6FEYXeDoTJsObpALyZiBoivLEDFiZRGZLMAF69UKkop9QLwdF2F9rdo7gAAAABJRU5ErkJggg==";

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
                ( "StateData", "InstanceStateData:"),

                ( "Age distribution", "Group:2"),

                /// <param name="Age distribution:">
                /// This parameter determines whether the age distribution that is shown is the default one (i.e., the last one that has been
                /// set up), or whether a name should be entered to specify another age distribution.
                /// </param>
                ( "Age distribution:", "ComboBox:0[\"Default\",\"Custom\"]"),

                /// <param name="Distribution name:">
                /// If the value for the [Age distribution](#age-distribution) parameter is `Custom`, this text box can be used to enter the name
                /// of the age distribution to draw (as defined within the parameters of the _Set up age distributions_ module).
                /// </param>
                ( "Distribution name:", "TextBox:" ),
                
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
                ( "Plot type:", "ComboBox:1[\"Histogram\",\"Envelope\"]" ),
                
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

                ( "Fill colour", "Group:3" ),
                
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

                ( "Stroke style", "Group:6" ),
                
                /// <param name="Auto stroke colour by node">
                /// If this check box is checked, the colour of each age distribution is determined algorithmically in a pseudo-random
                /// way designed to achieve an aestethically pleasing distribution of colours, while being reproducible
                /// if the same tree is rendered multiple times.
                /// </param>
                ( "Auto stroke colour by node", "CheckBox:true"),
                
                /// <param name="Line opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto stroke colour by node](#auto-stroke-colour-by-node)
                /// option is enabled.
                /// </param>
                ( "Line opacity:", "Slider:0.5[\"0\",\"1\",\"{0:P0}\"]" ),
                
                /// <param name="Line colour:">
                /// This parameter determines the colour used to stroke each age distribution (if the [Auto stroke colour by node](#auto-stroke-colour-by-node)
                /// option is disabled). The colour can be determined based on the value of an attribute of the nodes in the tree.
                /// For nodes that do not possess the specified attribute (or that have the attribute with an invalid value), a
                /// default value is used. The default attribute used to determine the colour is `Color`.
                /// </param>
                ( "Line colour:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0]) + ",\"Color\",\"String\",\"0\",\"0\",\"0\",\"255\",\"true\"]" ),
                
                 /// <param name="Line weight:">
                /// The thickness of the outline for the branch distributions. This can be determined based on the value of an attribute of the nodes in the tree. For nodes that
                /// do not possess the specified attribute (or that have the attribute with an invalid value), a default value is used. The default attribute
                /// used to determine the thickness of the branches is `Thickness`.
                /// </param>
                ( "Line weight:", "NumericUpDownByNode:0[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"Thickness\",\"Number\",\"true\"]" ),
                
                /// <param name="Line cap:">
                /// The line cap to use when drawing the distributions.
                /// </param>
                ( "Line cap:", "ComboBox:1[\"Butt\",\"Round\",\"Square\"]" ),
                
                /// <param name="Line dash:">
                /// The line dash to use when drawing the distributions.
                /// </param>
                ( "Line dash:", "Dash:[0,0,0]"),
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

            if ((bool)currentParameterValues["Auto stroke colour by node"])
            {
                controlStatus.Add("Line opacity:", ControlStatus.Enabled);
                controlStatus.Add("Line colour:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Line opacity:", ControlStatus.Hidden);
                controlStatus.Add("Line colour:", ControlStatus.Enabled);
            }

            if ((int)currentParameterValues["Age distribution:"] == 0)
            {
                controlStatus.Add("Distribution name:", ControlStatus.Hidden);
            }
            else
            {
                controlStatus.Add("Distribution name:", ControlStatus.Enabled);
            }

            parametersToChange = new Dictionary<string, object>() { };

            if ((int)currentParameterValues["Age distribution:"] == 1 && (int)previousParameterValues["Age distribution:"] == 0 && string.IsNullOrEmpty((string)currentParameterValues["Distribution name:"]))
            {
                try
                {
                    InstanceStateData stateData = (InstanceStateData)currentParameterValues["StateData"];

                    if (stateData.Tags.TryGetValue("4e5d3934-44e6-4fe3-b11c-bd78e5b577d0", out object distribCollect) && distribCollect is Dictionary<string, (string, object)> distributionCollection && distributionCollection.Count > 0)
                    {
                        parametersToChange.Add("Distribution name:", distributionCollection.Where(x => checkIfFurtherTransformationModuleExists(x.Key, stateData)).Last().Value.Item1);
                    }
                }
                catch { }
            }

            return true;
        }

        public const string InvalidDistributionGuid = "527e3721-11cf-4e8c-aeec-18c93c7d8d7c";

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            bool autoColourByNode = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];
            ColourFormatterOptions customColour = (ColourFormatterOptions)parameterValues["Colour:"];
            Colour defaultFill = customColour.DefaultColour;
            Func<object, Colour?> fillFormatter = customColour.Formatter;

            bool defaultDistribution = (int)parameterValues["Age distribution:"] == 0;
            string distributionName = (string)parameterValues["Distribution name:"];

            double maxHeight = (double)parameterValues["Height:"];
            int plotType = (int)parameterValues["Plot type:"];
            int showWhere = (int)parameterValues["Show on:"];

            bool autoStroke = (bool)parameterValues["Auto stroke colour by node"];
            double strokeOpacity = (double)parameterValues["Line opacity:"];

            ColourFormatterOptions Stroke = (ColourFormatterOptions)parameterValues["Line colour:"];
            Colour defaultStroke = Stroke.DefaultColour;
            Func<object, Colour?> strokeFormatter = Stroke.Formatter;

            NumberFormatterOptions WeightFO = (NumberFormatterOptions)parameterValues["Line weight:"];
            double defaultWeight = WeightFO.DefaultValue;
            Func<object, double?> weightFormatter = WeightFO.Formatter;

            LineCaps cap = (LineCaps)(int)parameterValues["Line cap:"];
            LineJoins join = (LineJoins)(2 - (int)parameterValues["Line cap:"]);
            LineDash dash = (LineDash)parameterValues["Line dash:"];

            bool circular = false;

            Point scalePoint;
            if (coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out _))
            {
                scalePoint = coordinates["d0ab64ba-3bcd-443f-9150-48f6e85e97f3"];

                circular = true;
            }
            else if (!coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out scalePoint))
            {
                throw new Exception("The coordinates module is not supported!");
            }

            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            if (!stateData.Tags.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6") || !tree.Attributes.ContainsKey("a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"))
            {
                throw new Exception("The node ages have not been correctly set up!\nPlease use the \"Set up age distributions\" module.");
            }

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

                if (binCount < 0 || binCount > 100)
                {
                    binCount = 100;
                    binWidth = (max - min) / binCount;
                }

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

                if (binCount < 0 || binCount > 100)
                {
                    binCount = 100;
                    binWidth = (max - min) / binCount;
                }

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

            Point circularScalePoint = scalePoint;
            Point perpScale = normalizePoint(rotatePoint(scalePoint, Math.PI / 2));

            if (circular)
            {
                perpScale = new Point(0, 1);
                scalePoint = new Point(circularScalePoint.X, 0);
            }

            double totalTreeLength = tree.LongestDownstreamLength();

            Dictionary<string, List<double>> ageDistributionsBySamples = null;
            HashSet<string> availableNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (defaultDistribution)
            {
                if (stateData.Tags.TryGetValue("4e5d3934-44e6-4fe3-b11c-bd78e5b577d0", out object distribCollect) && distribCollect is Dictionary<string, (string, object)> distributionCollection && distributionCollection.Count > 0)
                {
                    foreach (KeyValuePair<string, (string, object)> kvp in distributionCollection)
                    {
                        if (checkIfFurtherTransformationModuleExists(kvp.Key, stateData))
                        {
                            if (kvp.Value.Item2 is Dictionary<string, List<double>> list)
                            {
                                ageDistributionsBySamples = list;
                            }
                        }
                    }
                }
                else
                {
                    ageDistributionsBySamples = (Dictionary<string, List<double>>)stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"];
                }
            }
            else
            {
                if (stateData.Tags.TryGetValue("4e5d3934-44e6-4fe3-b11c-bd78e5b577d0", out object distribCollect) && distribCollect is Dictionary<string, (string, object)> distributionCollection && distributionCollection.Count > 0)
                {
                    foreach (KeyValuePair<string, (string, object)> kvp in distributionCollection)
                    {
                        if (checkIfFurtherTransformationModuleExists(kvp.Key, stateData))
                        {
                            availableNames.Add(kvp.Value.Item1);

                            if (kvp.Value.Item1.Equals(distributionName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (kvp.Value.Item2 is Dictionary<string, List<double>> list)
                                {
                                    ageDistributionsBySamples = list;
                                }
                            }
                        }
                    }
                }
            }

            if (ageDistributionsBySamples == null)
            {
                if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
                {
                    setWarning(availableNames.Aggregate("Invalid distribution specified! The available distribution names are:", (a, b) => a + "\n\n • `" + b + "`"), InvalidDistributionGuid);
                }

                return new Point[] { coordinates[Modules.RootNodeId], coordinates[Modules.RootNodeId] };
            }
            else if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
            {
                setWarning("", InvalidDistributionGuid);
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                if (showWhere == 2 || (showWhere == 1 && nodes[i].Children.Count > 0) || (showWhere == 0 && nodes[i].Children.Count == 0))
                {
                    List<double> distrib = ageDistributionsBySamples[nodes[i].Id];

                    if (distrib.Count > 0)
                    {
                        Colour colour = defaultFill;

                        if (autoColourByNode)
                        {
                            colour = Modules.DefaultColours[Math.Abs(string.Join(",", nodes[i].GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                        }
                        else if (nodes[i].Attributes.TryGetValue(customColour.AttributeName, out object fillAttributeObject) && fillAttributeObject != null)
                        {
                            colour = fillFormatter(fillAttributeObject) ?? defaultFill;
                        }

                        Colour strokeColour = defaultStroke;

                        if (!autoStroke)
                        {
                            if (nodes[i].Attributes.TryGetValue(Stroke.AttributeName, out object strokeAttributeObject) && strokeAttributeObject != null)
                            {
                                strokeColour = strokeFormatter(strokeAttributeObject) ?? defaultStroke;
                            }
                        }
                        else
                        {
                            strokeColour = Modules.DefaultColours[Math.Abs(string.Join(",", nodes[i].GetLeafNames()).GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(strokeOpacity);
                        }

                        double weight = defaultWeight;

                        if (nodes[i].Attributes.TryGetValue(WeightFO.AttributeName, out object weightAttributeObject) && weightAttributeObject != null)
                        {
                            weight = weightFormatter(weightAttributeObject) ?? defaultWeight;
                        }


                        if (colour.A > 0 || (strokeColour.A > 0 && weight > 0))
                        {
                            if (!circular)
                            {
                                if (plotType == 0)
                                {
                                    (int[], double[]) hist = histogram(distrib);

                                    int[] bins = hist.Item1;
                                    double[] range = hist.Item2;

                                    double maxBin = bins.Max();

                                    double age = totalTreeLength - nodes[i].UpstreamLength();
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

                                    if (colour.A > 0)
                                    {
                                        graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                                    }

                                    if (strokeColour.A > 0 && weight > 0)
                                    {
                                        graphics.StrokePath(histPth, strokeColour, weight, cap, join, dash, tag: nodes[i].Id);
                                    }
                                }
                                else if (plotType == 1)
                                {
                                    (int[], double[], double[]) hist = histogramWithBinMeans(distrib);

                                    int[] bins = hist.Item1;
                                    double[] range = hist.Item2;
                                    double[] binMeans = hist.Item3;

                                    double maxBin = bins.Max();

                                    double age = totalTreeLength - nodes[i].UpstreamLength();
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


                                    if (colour.A > 0)
                                    {
                                        graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                                    }

                                    if (strokeColour.A > 0 && weight > 0)
                                    {
                                        graphics.StrokePath(histPth, strokeColour, weight, cap, join, dash, tag: nodes[i].Id);
                                    }
                                }
                            }
                            else
                            {
                                if (plotType == 0)
                                {
                                    (int[], double[]) hist = histogram(distrib);

                                    int[] bins = hist.Item1;
                                    double[] range = hist.Item2;

                                    double maxBin = bins.Max();

                                    double age = totalTreeLength - nodes[i].UpstreamLength();
                                    double deltaLeft = age - range[1];
                                    double deltaRight = range[0] - age;

                                    Point pt = coordinates[nodes[i].Id];

                                    double r = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                                    double theta = Math.Atan2(pt.Y, pt.X);

                                    double rightR = r - circularScalePoint.X * deltaRight;
                                    double leftR = r + circularScalePoint.X * deltaLeft;

                                    Point leftPoint = new Point(leftR * Math.Cos(theta), leftR * Math.Sin(theta));
                                    Point rightPoint = new Point(rightR * Math.Cos(theta), rightR * Math.Sin(theta));

                                    updateMaxMin(leftPoint);
                                    updateMaxMin(rightPoint);

                                    leftPoint = new Point(leftR - r, 0);
                                    rightPoint = new Point(rightR - r, 0);

                                    graphics.Save();
                                    graphics.Translate(pt);
                                    graphics.Rotate(theta);

                                    GraphicsPath histPth = new GraphicsPath();

                                    for (int j = 0; j < bins.Length; j++)
                                    {
                                        Point vertDist = new Point(perpScale.X * bins[bins.Length - j - 1] / maxBin * maxHeight, perpScale.Y * bins[bins.Length - j - 1] / maxBin * maxHeight);
                                        Point binStart = sumPoint(leftPoint, new Point(scalePoint.X * range[2] * j, scalePoint.Y * range[2] * j));
                                        Point binEnd = sumPoint(leftPoint, new Point(scalePoint.X * Math.Min(range[2] * (j + 1), range[1] - range[0]), scalePoint.Y * Math.Min(range[2] * (j + 1), range[1] - range[0])));

                                        histPth.MoveTo(sumPoint(binStart, vertDist)).LineTo(sumPoint(binStart, new Point(-vertDist.X, -vertDist.Y))).LineTo(sumPoint(binEnd, new Point(-vertDist.X, -vertDist.Y))).LineTo(sumPoint(binEnd, vertDist)).Close();
                                    }

                                    if (colour.A > 0)
                                    {
                                        graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                                    }

                                    if (strokeColour.A > 0 && weight > 0)
                                    {
                                        graphics.StrokePath(histPth, strokeColour, weight, cap, join, dash, tag: nodes[i].Id);
                                    }

                                    graphics.Restore();
                                }
                                else if (plotType == 1)
                                {
                                    (int[], double[], double[]) hist = histogramWithBinMeans(distrib);

                                    int[] bins = hist.Item1;
                                    double[] range = hist.Item2;
                                    double[] binMeans = hist.Item3;

                                    double maxBin = bins.Max();

                                    double age = totalTreeLength - nodes[i].UpstreamLength();
                                    double deltaLeft = age - range[1];
                                    double deltaRight = range[0] - age;

                                    Point pt = coordinates[nodes[i].Id];

                                    double r = Math.Sqrt(pt.X * pt.X + pt.Y * pt.Y);
                                    double theta = Math.Atan2(pt.Y, pt.X);

                                    double rightR = r - circularScalePoint.X * deltaRight;
                                    double leftR = r + circularScalePoint.X * deltaLeft;

                                    Point leftPoint = new Point(leftR * Math.Cos(theta), leftR * Math.Sin(theta));
                                    Point rightPoint = new Point(rightR * Math.Cos(theta), rightR * Math.Sin(theta));

                                    updateMaxMin(leftPoint);
                                    updateMaxMin(rightPoint);

                                    leftPoint = new Point(leftR - r, 0);
                                    rightPoint = new Point(rightR - r, 0);

                                    graphics.Save();
                                    graphics.Translate(pt);
                                    graphics.Rotate(theta);

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

                                    if (colour.A > 0)
                                    {
                                        graphics.FillPath(histPth, colour, tag: nodes[i].Id);
                                    }

                                    if (strokeColour.A > 0 && weight > 0)
                                    {
                                        graphics.StrokePath(histPth, strokeColour, weight, cap, join, dash, tag: nodes[i].Id);
                                    }

                                    graphics.Restore();
                                }
                            }
                        }
                    }

                }
            }



            return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
        }

        private static bool checkIfFurtherTransformationModuleExists(string id, InstanceStateData stateData)
        {
            List<FurtherTransformationModule> furtherTransformationModules = stateData.FurtherTransformationModules();

            for (int i = 0; i < furtherTransformationModules.Count; i++)
            {
                if (stateData.GetFurtherTransformationModulesParamters(i).TryGetValue(Modules.ModuleIDKey, out object key) && key is string str && str.Equals(id))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

