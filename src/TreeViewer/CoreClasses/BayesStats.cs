/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
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

namespace TreeViewer
{
    public static class BayesStats
    {
        //Adapted from https://github.com/easystats/bayestestR/blob/e26dc16f3df711afbc2e79851442cfb5ea3b6f26/R/hdi.R
        public static double[] HighestDensityInterval(IEnumerable<double> samples, double threshold)
        {
            if (threshold < 0 || threshold > 1)
            {
                throw new ArgumentOutOfRangeException("The threshold should be between 0 and 1 (inclusive).");
            }
            else if (threshold == 1)
            {
                return new double[] { samples.Min(), samples.Max() };
            }

            List<double> sorted = new List<double>(samples);
            sorted.Sort();

            int windowSize = (int)Math.Ceiling(threshold * sorted.Count);
            if (windowSize < 2)
            {
                throw new ArgumentException("The threshold is too low or the data does not contain enough samples!");
            }

            int nCIs = sorted.Count - windowSize;
            if (nCIs < 1)
            {
                throw new ArgumentException("The threshold is too high or the data does not contain enough samples!");
            }

            double[] ciWidths = new double[nCIs];

            double minWidth = double.MaxValue;
            List<int> minI = new List<int>();


            for (int i = 0; i < nCIs; i++)
            {
                ciWidths[i] = sorted[i + windowSize] - sorted[i];

                if (ciWidths[i] < minWidth)
                {
                    minWidth = ciWidths[i];
                    minI.Clear();
                    minI.Add(i);
                }
                else if (ciWidths[i] == minWidth)
                {
                    minI.Add(i);
                }
            }

            int realMinI = minI[0];

            if (minI.Count > 1)
            {
                bool foundDiff = false;
                for (int i = 1; i < minI.Count; i++)
                {
                    if (minI[i] - minI[i - 1] != 1)
                    {
                        foundDiff = true;
                        break;
                    }
                }

                if (foundDiff)
                {
                    realMinI = minI.Max();
                }
                else
                {
                    realMinI = (int)Math.Floor(minI.Average());
                }
            }

            return new double[] { sorted[realMinI], sorted[realMinI + windowSize] };
        }

        public static double Quantile(IEnumerable<double> samples, double quantile)
        {
            List<double> sortedSamples = new List<double>(samples);
            sortedSamples.Sort();
            int n = sortedSamples.Count;

            double index = (n - 1) * quantile;
            int lo = (int)Math.Floor(index);
            int hi = (int)Math.Ceiling(index);

            double qs = sortedSamples[lo];
            double h = index - lo;
            qs = (1 - h) * qs + h * sortedSamples[hi];

            return qs;
        }

        public static double[] EqualTailedInterval(IEnumerable<double> samples, double threshold)
        {
            if (threshold < 0 || threshold > 1)
            {
                throw new ArgumentOutOfRangeException("The threshold should be between 0 and 1 (inclusive).");
            }

            return new double[] { Quantile(samples, (1 - threshold) / 2), Quantile(samples, (1 + threshold) / 2) };
        }
    }
}
