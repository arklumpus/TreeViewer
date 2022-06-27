using MathNet.Numerics.Statistics;
using PhyloTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer.Stats
{
    internal static class ShapeIndices
    {
        public static (double[] sackin, double[] colless, double[] cherries) GetDistribution(int leafCount, TreeNode.NullHypothesis model, bool rooted, bool computeSackin, bool computeColless, Action<double> progress)
        {
            int repeatCount = 2000;

            double[] sackin = computeSackin ? new double[repeatCount] : null;
            double[] colless = computeColless ? new double[repeatCount] : null;
            double[] cherries = new double[repeatCount];

            double collessYuleExpectation = (computeColless && model == TreeNode.NullHypothesis.YHK) ? TreeNode.GetCollessExpectationYHK(leafCount) : double.NaN;

            int completed = 0;

            object progressLock = new object();

            Parallel.For(0, repeatCount / 100, i =>
            {
                int start = i * 100;
                int end = start + 100;

                for (int j = start; j < end; j++)
                {
                    TreeNode tree = SimulateTree.Simulate(leafCount, model, rooted);

                    if (computeSackin)
                    {
                        sackin[j] = tree.SackinIndex(model);
                    }

                    if (computeColless)
                    {
                        colless[j] = tree.CollessIndex(model, collessYuleExpectation);
                    }

                    cherries[j] = tree.NumberOfCherries(model);

                    lock (progressLock)
                    {
                        completed++;

                        if (completed % 20 == 0)
                        {
                            progress((double)completed / repeatCount);
                        }
                    }    
                }
            });

            return (sackin, colless, cherries);
        }
    }
}
