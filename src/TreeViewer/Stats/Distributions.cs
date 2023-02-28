using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp;

namespace TreeViewer.Stats
{
    internal class BranchLengthDistribution
    {
        public static Page GetPlot(List<double> branchLengths, string tag)
        {
            return Histogram.GetPlot(branchLengths, "Branch length", "Branch length distribution", tag);
        }
    }

    internal class LeafDepthDistribution
    {
        public static Page GetPlot(List<double> leafDepths, string tag)
        {
            return Histogram.GetPlot(leafDepths, "Leaf depth", "Leaf depth distribution", tag);
        }
    }

    internal class LeafHeightDistribution
    {
        public static Page GetPlot(List<double> leafHeights, string tag)
        {
            return Histogram.GetPlot(leafHeights, "Leaf height", "Leaf height distribution", tag);
        }
    }

    internal static class SackinDistribution
    {
        public static Page GetPlot(double[] yuleSamples, double[] pdaSamples, double sackinYule, double sackinPDA, double sackinYuleP, double sackinPDAP, string tag)
        {
            return Distribution.GetPlot(yuleSamples, pdaSamples, sackinYule, sackinPDA, sackinYuleP, sackinPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Sackin index", "Sackin index distribution", "S", tag, out _);
        }

        public static Page GetPlot(List<double> sackinIndices, string tag)
        {
            return Histogram.GetPlot(sackinIndices, "Sackin index", "Sackin index distribution", tag);
        }

        public static Page GetPlotTwoTailed(double[] yuleSamples, double[] pdaSamples, double minValue, double maxValue, double sackinYule, double sackinPDA, double sackinYuleP, double sackinPDAP, string tag)
        {
            return Distribution.GetPlotTwoTailed(yuleSamples, pdaSamples, minValue, maxValue, sackinYule, sackinPDA, sackinYuleP, sackinPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Sackin index difference", "Sackin index difference distribution", "ΔS", tag, out _);
        }
    }

    internal static class CollessDistribution
    {
        public static Page GetPlot(double[] yuleSamples, double[] pdaSamples, double collessYule, double collessPDA, double collessYuleP, double collessPDAP, string tag)
        {
            return Distribution.GetPlot(yuleSamples, pdaSamples, collessYule, collessPDA, collessYuleP, collessPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Colless index", "Colless index distribution", "C", tag, out _);
        }

        public static Page GetPlot(List<double> collessIndices, string tag)
        {
            return Histogram.GetPlot(collessIndices, "Colless index", "Colless index distribution", tag);
        }

        public static Page GetPlotTwoTailed(double[] yuleSamples, double[] pdaSamples, double minValue, double maxValue, double collessYule, double collessPDA, double collessYuleP, double collessPDAP, string tag)
        {
            return Distribution.GetPlotTwoTailed(yuleSamples, pdaSamples, minValue, maxValue, collessYule, collessPDA, collessYuleP, collessPDAP, "Density (YHK model)", "Density (PDA model)", "Normalised Colless index difference", "Colless index difference distribution", "ΔC", tag, out _);
        }
    }

    internal static class NumberOfCherriesDistribution
    {
        public static Page GetPlot(double[] yuleSamples, double[] pdaSamples, double cherryYule, double cherryPDA, double cherryYuleP, double cherryPDAP, string tag)
        {
            return Distribution.GetPlotWithStandardNormal(yuleSamples, pdaSamples, cherryYule, cherryPDA, cherryYuleP, cherryPDAP, "Density (YHK model)", "Density (PDA model)", "Density (limit distribution)", "Normalised number of cherries", "Distribution of the number of cherries", "K", tag, out _);
        }

        public static Page GetPlot(List<double> numberOfCherries, string tag)
        {
            return Histogram.GetPlot(numberOfCherries, "Number of cherries", "Distribution of the number of cherries", tag);
        }

        public static Page GetPlotTwoTailed(double[] yuleSamples, double[] pdaSamples, double minValue, double maxValue, double cherryYule, double cherryPDA, double cherryYuleP, double cherryPDAP, string tag)
        {
            return Distribution.GetPlotWithStandardNormalTwoTailed(yuleSamples, pdaSamples, minValue, maxValue, cherryYule, cherryPDA, cherryYuleP, cherryPDAP, "Density (YHK model)", "Density (PDA model)", "Density (limit distribution)", "Normalised cherry index difference", "Cherry index difference distribution", "ΔK", tag, out _);
        }
    }

    internal static class RobinsonFouldsDistancePoints
    {
        public static Page GetPlot(double[,] distMat, double[,] points, bool weighted, string tag)
        {
            if (weighted)
            {
                return Points.GetPlot(distMat, points, "wRF distance component 1", "wRF distance component 2", "Tree space", tag, out _, out _);
            }
            else
            {
                return Points.GetPlot(distMat, points, "RF distance component 1", "RF distance component 2", "Tree space", tag, out _, out _);
            }
        }
    }

    internal static class SplitLengthDifferences
    {
        public static Page GetPlot(List<double> splitLengthDiffs, string tag)
        {
            return Histogram.GetPlot(splitLengthDiffs, "Split length difference", "Distribution of split length differences", tag);
        }
    }
}
