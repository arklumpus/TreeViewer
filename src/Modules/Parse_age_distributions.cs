using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using MathNet.Numerics.Distributions;
using System.Reflection;
using System.Linq;

namespace a15c955cebd4c4a968cd3b48d37aafc4c
{
    public static class MyModule
    {
        public const string Name = "Parse age distributions";
        public const string HelpText = "Parses node age distributions from node attributes.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "15c955ce-bd4c-4a96-8cd3-b48d37aafc4c";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAACmSURBVDhPpZIxDsMgDEWdqgdr1i65C3uVRN25S5euOVrKJ98ScoKR6JPQxwR/7MAgBc/XZ0kyH5Gs3/eE2OVGVTQZzDR0sQaWpkluwZRe47KlISXvnHfRaqHJ/y1QM5V23Ov0DLrewUoVJIcQHmnsUC4LYk7b6OYyyRpUb6E8FdhYuVOv2GKM+R9BeXKOiypGz+BUrqLG+O4a6EZQM+t6iTA7DGX8AchDTc6RnhTKAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAHxSURBVEhLtVU9T8JQFG0pCZuDg5GYOGj8AWxu1MFBB00MRE1cdeVzFJHoCsjCQIyTg4sYozI5oJuJcXFwcDEumrg2hgXqOfBQKH2VEjjJzXnvtj333Y8HqiLBcupyH5SGZSoHq1wPBE1wFzrECX0uuKm+3p9Vxd4VPIKtaIu3kRZBXaMngIPQQEG6SmQpjR1cl6vZ5D6EZbiBPToNgQpxU6xHAlmTh4bRl0hwE30GcnXxuqaI08EpwVJveXrg+lbb/lRIGj/QT4asyRnBBIXVQcQJ2wwI9oOiiURiwjTNF7i8tVptqlgsGq03/hCLxT5UVT3O5XIp4fqFdEw7ThyCjcPGfD7fetPjAv/eg0ajEQY9t3bKcAMkk8lZpK7DmE0WvBiNRmdaT/uDYwBxeiWbzZYNw9jjGkGkWcTjcbNt6Ms2fY4B0Nww7JrrUqn0zWY7BNjFcx2NVsEneK9Ep3SKcIoA6AlZbMFu6fN4PBuwI3w8j6we6COsU4R9EPsqAu04ZdAsDwRPvV7vJ43i9OHDDbIM+Xz+jowg004BQkwVov5Og/8C5jhNzICM799tS9ROsV6vLxQKha5/r0gksqZp2jmer6BMV/SJEk1CUOfpsa/AHcDab5sBXublerOKE/CVQV8waxaHPBQnCLyEPcqoKD934dcvcMc1kQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAJtSURBVFhHxVY9SwNBEL2LlmnsYq8/wN4YW9OIoqKNQsRWSIKChajYKUnAXsFOVBRBY+tHK+QHaG86G+uc761zYc0ld3uXlTyYzOzs7c3b2dm5uE4E8rt3+1B7kIP64Sxtq0iJDgODKy1krCKUQJeA1klEZcDfvQ6rJHoSiAhijcSQ6D+Ql3fbvY7p8dyK+/5y8STjRAgQMAzuo28S7WsYM3AvPEDe4lxXRcBS8DD0JOYiuCf2QGDSiP4Vgz8C0YMtQh0JiPT1kQr0Ad5p3m2Y07+eUPT9hezaCQ1JWPk8B45AR8gVtfbfIOoaHojWYfWPSWgGiI4sGAcvl8sLnuddQc5qtdq6uAMwaUR+FtrBi8VirlQqeRTYG2q2A61Wa0TMUEQSYFCI27HzVdHEnOhESNqK85APpLfpuu4MsjD2646PYdHGYMoRNAPzBJquTeg16F0OugFrtqBKXEfSsOt+XSTJgEo5XnRDUR7HWRYdAIJOQY5gNliQ4iuA1CntWASYaizO4kUN7OCZAvcHZAxzOfVQEBmsWcSzedl1FvIN3xInYxHAIqY6Dc2Pjo8L0XphtgGyl5VK5VqGDkiwdl5hpnmcsQhgoWrN0OfKAWg2C9MUn6LNM8AUY+eTCMgi2uEZUmjTh7kMd6QejgFjAggwL5pnWugQ3grCtCeM8ieVSn0ZE8AuVdFAj1erVVcX+jDFwsoiC396AnxLbMsy9DPJQm6yNowI8AXcJRY9sojE3YZeWHiOhaqjibkrBK7zyDB/D18aUuWkEQH09Rkxb0UHgHSqO45gqlCZXhmzB5xAJhC8gPE3ZBukjx3HcX4AQMwsOLbulb0AAAAASUVORK5CYII=";

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
                
                /// <param name="Attribute:">
                /// This parameter determines the attribute that this module parses into an age distribution.
                /// </param>
                ( "Attribute:", "AttributeSelector:Name"),

                ( "Scaling", "Group:2" ),

                /// <param name="Scaling factor:">
                /// This parameter is used to scale the age distributions (and the tree, if the [Apply scaling to transformed tree](#apply-scaling-to-transformed-tree)
                /// check box is checked).
                /// </param>
                ( "Scaling factor:", "NumericUpDown:1[\"0\",\"Infinity\",\"1\",\"0.####\"]" ),
                
                /// <param name="Apply scaling to transformed tree">
                /// If this check box is checked, the [scaling factor](#scaling-factor) is applied to the transformed tree, as well as to the age distributions.
                /// </param>
                ( "Apply scaling to transformed tree", "CheckBox:true" ),
                
                /// <param name="Name:">
                /// This parameter specifies a name that can be used to identify the age distributions in cases where multiple age distributions have been computed
                /// for the same tree.
                /// </param>
                ( "Name:", "TextBox:AgeDistributions" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string attributeName = (string)parameterValues["Attribute:"];
            string distributionName = (string)parameterValues["Name:"];
            int sampleCount = 200;

            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            Dictionary<string, (double[], double[])> ageDistributions = new Dictionary<string, (double[], double[])>();

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (node.Attributes.TryGetValue(attributeName, out object attrValue) && attrValue is string distribString)
                {
                    if (ParseDistributionString(distribString, sampleCount, out double[] sampledPoints, out double[] sampledValues))
                    {
                        ageDistributions[node.Id] = (sampledPoints, sampledValues);
                    }
                }
            }

            if (ageDistributions.Count > 0)
            {
                double scalingFactor = (double)parameterValues["Scaling factor:"];
                bool applyScalingToTree = (bool)parameterValues["Apply scaling to transformed tree"];

                if (scalingFactor != 1)
                {
                    foreach (KeyValuePair<string, (double[], double[])> kvp in ageDistributions)
                    {
                        for (int i = 0; i < kvp.Value.Item1.Length; i++)
                        {
                            kvp.Value.Item1[i] *= scalingFactor;
                        }
                    }

                    if (applyScalingToTree)
                    {
                        foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                        {
                            node.Length *= scalingFactor;
                        }
                    }
                }

                stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = ageDistributions;
                tree.Attributes["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = "Age distributions";

                Dictionary<string, (string, object)> distributionCollection;

                if (stateData.Tags.TryGetValue("4e5d3934-44e6-4fe3-b11c-bd78e5b577d0", out object distribCollect))
                {
                    distributionCollection = distribCollect as Dictionary<string, (string, object)>;
                }
                else
                {
                    distributionCollection = new Dictionary<string, (string, object)>();
                    stateData.Tags["4e5d3934-44e6-4fe3-b11c-bd78e5b577d0"] = distributionCollection;
                }

                distributionCollection[(string)parameterValues[Modules.ModuleIDKey]] = (distributionName, ageDistributions);
            }
        }

        private static bool ParseDistributionString(string distribution, int sampleCount, out double[] sampledPoints, out double[] sampledValues)
        {
            sampledPoints = null;
            sampledValues = null;

            try
            {
                distribution = distribution.Replace(" ", "");

                if ((distribution.Contains("(") && distribution.Contains(")")) || distribution.Contains(">") || distribution.Contains("<"))
                {

                    string distribName;
                    double[] args;

                    if (distribution.Contains("(") && distribution.Contains(")"))
                    {
                        distribName = distribution.Substring(0, distribution.IndexOf("("));
                        args = (from el in distribution.Substring(distribName.Length).Replace("(", "").Replace(")", "").Split(',') select double.Parse(el)).ToArray();
                    }
                    else if (distribution.StartsWith(">") && !distribution.Contains("<"))
                    {
                        distribName = "L";
                        args = new double[] { double.Parse(distribution.Substring(1)) };
                    }
                    else if (distribution.StartsWith("<") && !distribution.Contains(">"))
                    {
                        distribName = "U";
                        args = new double[] { double.Parse(distribution.Substring(1)) };
                    }
                    else
                    {
                        distribName = "B";
                        distribution = distribution.Replace(">", ";").Replace("<", ";");
                        args = (from el in distribution.Split(';') where double.TryParse(el, System.Globalization.CultureInfo.InvariantCulture, out _) select double.Parse(el, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
                    }

                    if (distribName.Equals("L", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 1)
                        {
                            return false;
                        }

                        double a = args[0];

                        double P = 0.1;
                        double c = 1;
                        double tailL = 0.025;

                        if (args.Length >= 2)
                        {
                            P = args[1];
                        }

                        if (args.Length >= 3)
                        {
                            c = args[2];
                        }

                        if (args.Length >= 4)
                        {
                            tailL = args[3];
                        }

                        Func<double, double> distrib = x => LowerDistribution(x, a, P, c, tailL);

                        double minBound = a;

                        if (tailL >= 0.001)
                        {
                            Func<double, double> cdf1 = u => (MathNet.Numerics.Integrate.OnClosedInterval(distrib, u, a) - tailL * 0.99);
                            minBound = MathNet.Numerics.FindRoots.OfFunction(cdf1, 0, a);
                        }


                        Func<double, double> cdf2 = u => (MathNet.Numerics.Integrate.OnClosedInterval(distrib, a, u) - 0.9 * (1 - tailL));
                        double maxBound = MathNet.Numerics.FindRoots.OfFunction(cdf2, a, 1e10);

                        List<double> sampledPointsPList = new List<double>(sampleCount + 1);
                        List<double> sampledValuesPList = new List<double>(sampleCount + 1);

                        if (minBound < a)
                        {
                            (double[] sampledPointsP, double[] sampledValuesP) = SampleFunction(distrib, minBound, a, sampleCount / 4);
                            sampledPointsPList.AddRange(sampledPointsP);
                            sampledValuesPList.AddRange(sampledValuesP);
                        }
                        else
                        {
                            sampledPointsPList.Add(a);
                            sampledValuesPList.Add(distrib(a));
                        }

                        {
                            (double[] sampledPointsP, double[] sampledValuesP) = SampleFunction(distrib, a, maxBound, sampleCount - sampleCount / 4);
                            sampledPointsPList.AddRange(sampledPointsP);
                            sampledValuesPList.AddRange(sampledValuesP);
                        }

                        sampledPoints = sampledPointsPList.ToArray();
                        sampledValues = sampledValuesPList.ToArray();

                        return true;
                    }
                    else if (distribName.Equals("U", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 1)
                        {
                            return false;
                        }

                        double b = args[0];
                        double tailR = 0.025;

                        if (args.Length >= 2)
                        {
                            tailR = args[1];
                        }

                        Func<double, double> distrib = x => UpperDistribution(x, b, tailR);

                        double maxBound = b;

                        if (tailR >= 0.001)
                        {
                            Func<double, double> cdf2 = u => (MathNet.Numerics.Integrate.OnClosedInterval(distrib, b, u) - 0.99 * tailR);

                            maxBound = MathNet.Numerics.FindRoots.OfFunction(cdf2, b, 1e10);
                        }

                        List<double> sampledPointsPList = new List<double>(sampleCount + 1);
                        List<double> sampledValuesPList = new List<double>(sampleCount + 1);

                        sampledPointsPList.Add(0);
                        sampledValuesPList.Add(distrib(0));

                        if (maxBound > b)
                        {
                            (double[] sampledPointsP, double[] sampledValuesP) = SampleFunction(distrib, b, maxBound, sampleCount - 1);
                            sampledPointsPList.AddRange(sampledPointsP);
                            sampledValuesPList.AddRange(sampledValuesP);
                        }
                        else
                        {
                            sampledPointsPList.Add(b);
                            sampledValuesPList.Add(distrib(b));
                        }

                        sampledPoints = sampledPointsPList.ToArray();
                        sampledValues = sampledValuesPList.ToArray();

                        return true;
                    }
                    else if (distribName.Equals("B", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 2)
                        {
                            return false;
                        }

                        double a = args[0];
                        double b = args[1];
                        double tailL = 0.025;
                        double tailR = 0.025;

                        if (args.Length >= 3)
                        {
                            tailL = args[2];
                        }

                        if (args.Length >= 4)
                        {
                            tailR = args[3];
                        }


                        Func<double, double> distrib = x => BoundDistribution(x, a, b, tailL, tailR);

                        double minBound = a;
                        double maxBound = b;

                        if (tailL >= 0.001)
                        {
                            Func<double, double> cdf1 = u => (MathNet.Numerics.Integrate.OnClosedInterval(distrib, u, a) - tailL * 0.99);
                            minBound = MathNet.Numerics.FindRoots.OfFunction(cdf1, 0, a);
                        }

                        if (tailR >= 0.001)
                        {
                            Func<double, double> cdf2 = u => (MathNet.Numerics.Integrate.OnClosedInterval(distrib, b, u) - 0.99 * tailR);
                            maxBound = MathNet.Numerics.FindRoots.OfFunction(cdf2, b, 1e10);
                        }

                        List<double> sampledPointsPList = new List<double>(sampleCount + 1);
                        List<double> sampledValuesPList = new List<double>(sampleCount + 1);


                        if (minBound < a)
                        {
                            (double[] sampledPointsP, double[] sampledValuesP) = SampleFunction(distrib, minBound, a, sampleCount / 2);
                            sampledPointsPList.AddRange(sampledPointsP);
                            sampledValuesPList.AddRange(sampledValuesP);
                        }
                        else
                        {
                            sampledPointsPList.Add(a);
                            sampledValuesPList.Add(distrib(a));
                        }

                        sampledPointsPList.Add((a + b) * 0.5);
                        sampledValuesPList.Add(distrib((a + b) * 0.5));

                        if (maxBound > b)
                        {
                            (double[] sampledPointsP, double[] sampledValuesP) = SampleFunction(distrib, b, maxBound, sampleCount / 2);
                            sampledPointsPList.AddRange(sampledPointsP);
                            sampledValuesPList.AddRange(sampledValuesP);
                        }
                        else
                        {
                            sampledPointsPList.Add(b);
                            sampledValuesPList.Add(distrib(b));
                        }

                        sampledPoints = sampledPointsPList.ToArray();
                        sampledValues = sampledValuesPList.ToArray();

                        return true;
                    }
                    else if (distribName.Equals("G", StringComparison.OrdinalIgnoreCase) || distribName.Equals("gamma", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 2)
                        {
                            return false;
                        }

                        double a = args[0];
                        double b = args[1];

                        Gamma gamma = new Gamma(a, b);


                        Func<double, double> distrib = x => gamma.Density(x);

                        double minBound = gamma.InverseCumulativeDistribution(0.0005);
                        double maxBound = gamma.InverseCumulativeDistribution(0.9995);

                        double minCDF = gamma.CumulativeDistribution(minBound);

                        (sampledPoints, sampledValues) = SampleFunction(gamma.Density, x => gamma.CumulativeDistribution(x) - minCDF, minBound, maxBound, sampleCount);

                        return true;
                    }
                    else if (distribName.Equals("SN", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 3)
                        {
                            return false;
                        }

                        double loc = args[0];
                        double scale = args[1];
                        double shape = args[2];

                        Func<double, double> distrib = x => SkewNormalDistribution(x, loc, scale, shape);

                        Func<double, double> cdf = x => MathNet.Numerics.Integrate.OnClosedInterval(distrib, 0, x);

                        double delta = shape / Math.Sqrt(1 + shape * shape);

                        double mean = loc + scale * delta * Math.Sqrt(2 / Math.PI);
                        double stdDev = Math.Sqrt(scale * scale * (1 - 2 * delta * delta / Math.PI));

                        double minBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.0005, distrib, Math.Max(0, mean - 3 * stdDev), mean);
                        double maxBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.9995, distrib, mean, mean + 3 * stdDev);

                        minBound = Math.Max(0, minBound);

                        (sampledPoints, sampledValues) = SampleFunction(distrib, minBound, maxBound, sampleCount);

                        return true;
                    }
                    else if (distribName.Equals("ST", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 4)
                        {
                            return false;
                        }

                        double loc = args[0];
                        double scale = args[1];
                        double shape = args[2];
                        double df = args[3];

                        Func<double, double> distrib = x => SkewTDistribution(x, loc, scale, shape, df);

                        Func<double, double> cdf = x => MathNet.Numerics.Integrate.OnClosedInterval(distrib, 0, x);

                        double delta = shape / Math.Sqrt(1 + shape * shape);

                        double mean = loc + scale * delta * Math.Sqrt(2 / Math.PI);
                        double stdDev = Math.Sqrt(scale * scale * (1 - 2 * delta * delta / Math.PI));

                        double weight = Math.Exp(1 - df);

                        double minBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.0005, distrib, Math.Max(0, mean - 3 * stdDev), mean);
                        double maxBound;

                        try
                        {
                            maxBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.95, distrib, mean, mean + 3 * stdDev);
                        }
                        catch
                        {
                            maxBound = mean + 3 * stdDev;
                        }

                        minBound = Math.Max(0, minBound);

                        (sampledPoints, sampledValues) = SampleFunction(distrib, minBound, maxBound, sampleCount);

                        return true;
                    }
                    else if (distribName.Equals("S2N", StringComparison.OrdinalIgnoreCase))
                    {
                        if (args.Length < 7)
                        {
                            return false;
                        }

                        double p1 = args[0];
                        double loc1 = args[1];
                        double scale1 = args[2];
                        double shape1 = args[3];
                        double loc2 = args[4];
                        double scale2 = args[5];
                        double shape2 = args[6];

                        Func<double, double> distrib = x => Skew2NormalDistribution(x, p1, loc1, scale1, shape1, loc2, scale2, shape2);

                        Func<double, double> cdf = x => MathNet.Numerics.Integrate.OnClosedInterval(distrib, 0, x);

                        double delta1 = shape1 / Math.Sqrt(1 + shape1 * shape1);
                        double mean1 = loc1 + scale1 * delta1 * Math.Sqrt(2 / Math.PI);
                        double stdDev1 = Math.Sqrt(scale1 * scale1 * (1 - 2 * delta1 * delta1 / Math.PI));

                        double delta2 = shape2 / Math.Sqrt(1 + shape2 * shape2);
                        double mean2 = loc2 + scale2 * delta2 * Math.Sqrt(2 / Math.PI);
                        double stdDev2 = Math.Sqrt(scale2 * scale2 * (1 - 2 * delta2 * delta2 / Math.PI));

                        double minMean;
                        double minStdDev;
                        double maxMean;
                        double maxStdDev;

                        if (mean1 > mean2)
                        {
                            minMean = mean2;
                            maxMean = mean1;
                            minStdDev = stdDev2;
                            maxStdDev = stdDev1;
                        }
                        else
                        {
                            minMean = mean1;
                            maxMean = mean2;
                            minStdDev = stdDev1;
                            maxStdDev = stdDev2;
                        }

                        double minBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.0005, distrib, Math.Max(0, minMean - 3 * minStdDev), minMean);

                        double maxBound;

                        try
                        {
                            maxBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - 0.9995, distrib, maxMean, maxMean + 3 * maxStdDev);
                        }
                        catch
                        {
                            maxBound = maxMean + 3 * maxStdDev;
                        }

                        minBound = Math.Max(0, minBound);

                        (sampledPoints, sampledValues) = SampleFunction(distrib, minBound, maxBound, sampleCount);

                        return true;
                    }
                    else
                    {
                        Assembly mathNet = typeof(MathNet.Numerics.Distributions.Normal).Assembly;

                        Type distributionType = mathNet.GetTypes().Where(x => x.Name.Equals(distribName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                        if (distributionType != null)
                        {
                            object instantiatedDistrib = Activator.CreateInstance(distributionType, (from el in args select (object)el).ToArray());

                            if (instantiatedDistrib != null && instantiatedDistrib is IContinuousDistribution continuousDistribution)
                            {
                                Func<double, double> distrib = x => continuousDistribution.Density(x);

                                double minBound;

                                try
                                {
                                    minBound = continuousDistribution.Mean - 3 * continuousDistribution.StdDev;
                                }
                                catch
                                {
                                    minBound = 0;
                                }

                                if (!double.IsFinite(minBound))
                                {
                                    minBound = 0;
                                }

                                double maxBound;

                                try
                                {
                                    maxBound = continuousDistribution.Mean + 3 * continuousDistribution.StdDev;
                                }
                                catch
                                {
                                    maxBound = 1e10;
                                }

                                if (!double.IsFinite(maxBound))
                                {
                                    maxBound = 1e10;
                                }

                                try
                                {
                                    minBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => continuousDistribution.CumulativeDistribution(x) - 0.005, continuousDistribution.Density, minBound, maxBound);
                                }
                                catch { }

                                try
                                {
                                    maxBound = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => continuousDistribution.CumulativeDistribution(x) - 0.995, continuousDistribution.Density, minBound, maxBound);
                                }
                                catch { }

                                minBound = Math.Max(minBound, 0);

                                double minCDF = continuousDistribution.CumulativeDistribution(minBound);

                                (sampledPoints, sampledValues) = SampleFunction(continuousDistribution.Density, x => continuousDistribution.CumulativeDistribution(x) - minCDF, minBound, maxBound, sampleCount);

                                if (sampledPoints.Any(x => !double.IsFinite(x)) || sampledValues.Any(x => !double.IsFinite(x)))
                                {
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                        }

                    }
                }
            }
            catch { }

            return false;
        }

        private static (double[] sampledPoints, double[] sampledValues) SampleFunction(Func<double, double> function, double min, double max, int sampleCount)
        {
            double totalDensity = MathNet.Numerics.Integrate.OnClosedInterval(function, min, max);

            Func<double, double> cdf = x => MathNet.Numerics.Integrate.OnClosedInterval(function, min, x);

            double[] sampledPoints = new double[sampleCount + 1];
            double[] sampledValues = new double[sampleCount + 1];

            for (int i = 0; i <= sampleCount; i++)
            {
                double target = i * totalDensity / sampleCount;

                double sampledPoint = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - target, function, min, max);
                sampledPoints[i] = sampledPoint;
                sampledValues[i] = function(sampledPoint);
            }

            return (sampledPoints, sampledValues);
        }

        private static (double[] sampledPoints, double[] sampledValues) SampleFunction(Func<double, double> function, Func<double, double> cdf, double min, double max, int sampleCount)
        {
            double totalDensity = cdf(max) - cdf(min);

            double[] sampledPoints = new double[sampleCount + 1];
            double[] sampledValues = new double[sampleCount + 1];

            for (int i = 0; i <= sampleCount; i++)
            {
                double target = i * totalDensity / sampleCount;

                double sampledPoint = MathNet.Numerics.FindRoots.OfFunctionDerivative(x => cdf(x) - target, function, min, max);
                sampledPoints[i] = sampledPoint;
                sampledValues[i] = function(sampledPoint);
            }

            return (sampledPoints, sampledValues);
        }


        private static double LowerDistribution(double x, double a, double P = 0.1, double c = 1, double tailL = 0.025)
        {
            double t0 = a * (1 + P);
            double s = a * c;
            double A = 0.5 + 1 / Math.PI * Math.Atan(P / c);

            if (x >= a)
            {
                double z = (x - t0) / s;
                return (1 - tailL) / (Math.PI * A * s * (1 + z * z));
            }
            else
            {
                double z = P / c;
                double thetaL = (1 / tailL - 1) / (Math.PI * A * c * (1 + z * z));
                return (tailL * thetaL / a) * Math.Pow(x / a, thetaL - 1);
            }
        }

        private static double UpperDistribution(double x, double b, double tailR)
        {
            if (x <= b)
            {
                return ((1 - tailR) / b);
            }
            else
            {
                double thetaR = (1 - tailR) / (tailR * b);
                return (tailR * thetaR) * Math.Exp(-thetaR * (x - b));
            }
        }

        private static double BoundDistribution(double x, double a, double b, double tailL, double tailR)
        {
            if (x >= a && x <= b)
                return ((1 - tailL - tailR) / (b - a));
            else if (x < a)
            {
                double thetaL = (1 - tailL - tailR) * a / (tailL * (b - a));
                return (tailL * thetaL / a) * Math.Pow(x / a, (thetaL - 1));
            }
            else
            {
                double thetaR = (1 - tailL - tailR) / (tailR * (b - a));
                return (tailR * thetaR) * Math.Exp(-thetaR * (x - b));
            }
        }

        private static double SkewNormalDistribution(double x, double loc, double scale, double shape)
        {
            double z = (x - loc) / scale;
            double lnpdf = 2 / scale;

            return Math.Sqrt(2 / (Math.PI * scale * scale)) * Math.Exp(-z * z / 2) * Normal.CDF(0, 1, shape * z);
        }

        private static double SkewTDistribution(double x, double loc, double scale, double shape, double df)
        {
            double z = (x - loc) / scale;

            return 2 / scale * StudentT.PDF(0, 1, df, z) * StudentT.CDF(0, 1, df + 1, shape * z * Math.Sqrt((df + 1) / (df + z * z)));
        }

        private static double Skew2NormalDistribution(double x, double p1, double loc1, double scale1, double shape1, double loc2, double scale2, double shape2)
        {
            double a = SkewNormalDistribution(x, loc1, scale1, shape1);
            double b = SkewNormalDistribution(x, loc2, scale2, shape2);
            return p1 * a + (1 - p1) * b;
        }
    }
}