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

using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using System;
using PhyloTree.Extensions;

namespace Consensus_tree
{
    /// <summary>
    /// This module computes a consensus tree out of all the trees contained in the selected file. Alternatively, it can return
    /// just one of the loaded trees.
    /// 
    /// The parameters for this module are only available if the selected tree file contains $n > 1$ trees. If $n = 1$, the
    /// tree is returned unchanged.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The consensus tree is computed as follows:
    /// 
    /// 1. Each tree is transformed into a set of splits. Each split corresponds to a branch of the tree, and separates the
    /// taxa on either side of the branch. If the tree is not a clock-like tree, each split has an associated length (corresponding
    /// to the branch length). If the tree is a clock-like tree, each split has an age (corresponding to the age of the end node
    /// of the branch).
    /// 
    /// 2. The splits from the various trees in the file are compared and sorted based on how many trees they appear in. Splits
    /// that appear in a proportion of trees lower than the [Threshold](#threshold) are discarded.
    /// 
    /// 3. Starting from the splits which appear in most trees, each split is tested to determine whether it is "compatible" with all
    /// preceding splits (i.e. whether there is a tree topology that can satisfy all the splits). If the split is compatible, it is
    /// kept; if it is incompatible, it is instead discarded.
    /// 
    /// 4. The length or age of each of these compatible splits is computed using either the mean or the median of the length/age of
    /// the splits in the original trees (based on the value for [Branch lengths](#branch-lengths).
    /// 
    /// 5. This final set of splits (which are all compatible with each other) is transformed back into a (possibly multifurcating)
    /// tree, which corresponds to the consensus tree.    
    /// </description>

    public static class MyModule
    {
        public const string Name = "Consensus";
        public const string HelpText = "Computes the consensus of multiple trees (or returns one of them).";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "32914d41-b182-461e-b7c6-5f0263cc1ccd";
        public const ModuleTypes ModuleType = ModuleTypes.Transformer;

        public static List<(string, string)> GetParameters(TreeCollection trees)
        {
            if (trees.Count > 1)
            {
                List<string> leaves0 = trees[0].GetLeafNames();
                List<string> leaves1 = trees[1].GetLeafNames();

                bool clockLike = trees[0].IsClockLike() && trees[1].IsClockLike();

                bool sameLeaves = leaves0.ContainsAll(leaves1) || leaves1.ContainsAll(leaves0);

                int every = System.Math.Max(1, trees.Count / 100);

                return new List<(string, string)>()
                {
                    ( "Total trees: " + trees.Count.ToString(), "Label:[\"Left\",\"Italic\",\"#808080\"]"),

                    /// <param name="Consensus tree" default="Checked if the trees all contain the same taxa; otherwise, unchecked">
                    /// If this check box is checked, a consensus tree is computed. Otherwise, one of the trees from the file is
                    /// returned.
                    /// </param>
                    ( "Consensus tree", "CheckBox:" + (sameLeaves ? "true" : "false") ),
                    
                    /// <param name="Tree #" value-header="**Range**: [ 1, $n$ ]">
                    /// If the [Consensus tree](#consensus-tree) check box is unchecked, this control is visible and determines which
                    /// one of the trees from the file is returned. Otherwise, this control is hidden.
                    /// </param>
                    ( "Tree #", "NumericUpDown:1[\"1\",\"" + trees.Count.ToString() + "\",\"1\",\"0\"]" ),

                    ( "Consensus options", "Group:7" ),
                    
                    /// <param name="Treat trees as clock-like">
                    /// If this check box is checked the trees will be treated as clock-like trees, and the consensus tree will also be
                    /// clock-like (i.e. the rightmost tips of all trees will be aligned with each other). If this check box is unchecked,
                    /// the tree is treated as non-clock-like, and the consensus tree will also be non-clock-like (i.e. the root of all trees
                    /// are aligned with each other).
                    /// </param>
                    ( "Treat trees as clock-like", "CheckBox:" + (clockLike ? "true" : "false") ),
                    
                    /// <param name="Branch lengths:">
                    /// This parameter determines the algorithm used to compute the branch lengths of the consensus tree. If the value
                    /// is `Mean`, the length of each branch in the consensus tree corresponds to the mean of the length of the
                    /// corresponding branch in each tree. If the value is `Median`, the median of is instead used.
                    /// </param>
                    ( "Branch lengths:", "ComboBox:1[\"Mean\",\"Median\"]" ),
                    
                    /// <param name="Threshold:">
                    /// This parameter determines the threshold for the proportion of trees in which a split needs to appear to be
                    /// included in the consensus tree. A value of `0.5` corresponds to a "majority-rule" consensus tree; a value of
                    /// 1 corresponds to a strict consensus tree (only splits that are present in all of the trees are included); a
                    /// value of 0 keeps all compatible splits in the tree.
                    /// </param>
                    ( "Threshold:", "NumericUpDown:0[\"0\",\"1\"]" ),
                    
                    /// <param name="Skip:" value-header="**Range**: [ 0, $n$ ]">
                    /// This parameter determines the number of trees to skip before starting to consider trees for the consensus.
                    /// This is useful e.g. to remove some trees as burn-in.
                    /// </param>
                    ( "Skip:", "NumericUpDown:0[\"0\",\"" + trees.Count.ToString() + "\",\"1\",\"0\"]"),
                    
                    /// <param name="Every:" value-header="**Range**: [ 1, $n$ ]">
                    /// This parameter determines how many trees are skipped between a sample and the following sample. If this is `1`,
                    /// then all the trees are considered; if this is `2`, then one tree every 2 is used, and so on.
                    /// </param>
                    ( "Every:", "NumericUpDown:" + every.ToString() + "[\"1\",\"" + trees.Count.ToString() + "\",\"1\",\"0\"]"),
                    
                    /// <param name="Until:" value-header="**Range**: [ 1, $n$ ]" default="$n$">
                    /// This parameter determines the last tree to be considered.
                    /// </param>
                    ( "Until:", "NumericUpDown:" + trees.Count.ToString() + "[\"1\",\"" + trees.Count.ToString() + "\",\"1\",\"0\"]"),
                    
                    /// <param name="Recompute consensus">
                    /// This buttons applies changes to the other parameters and signals to the downstream modules that the tree has
                    /// changed and everything needs to be computed again.
                    /// </param>
                    ( "Recompute consensus", "Button:" )
                };
            }
            else
            {
                return new List<(string, string)>();
            }
        }

        public static bool OnParameterChange(object trees, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            if (((TreeCollection)trees).Count > 1)
            {
                controlStatus = new Dictionary<string, ControlStatus>()
                {
                    { "Total trees: " + ((TreeCollection)trees).Count.ToString(), ControlStatus.Enabled },
                    { "Consensus tree", ControlStatus.Enabled },
                    { "Tree #", ControlStatus.Enabled },
                    { "Consensus options", ControlStatus.Enabled },
                    { "Branch lengths:", ControlStatus.Enabled },
                    { "Threshold:", ControlStatus.Enabled },
                    { "Skip:", ControlStatus.Enabled },
                    { "Every:", ControlStatus.Enabled },
                    { "Until:", ControlStatus.Enabled },
                    { "Recompute consensus", ControlStatus.Enabled }
                };

                if ((bool)currentParameterValues["Consensus tree"])
                {
                    controlStatus["Tree #"] = ControlStatus.Hidden;
                }
                else
                {
                    controlStatus["Consensus options"] = ControlStatus.Hidden;
                }

                parametersToChange = new Dictionary<string, object>() { { "Recompute consensus", false } };

                return (bool)currentParameterValues["Consensus tree"] != (bool)previousParameterValues["Consensus tree"] || (bool)currentParameterValues["Recompute consensus"] || (double)currentParameterValues["Tree #"] != (double)previousParameterValues["Tree #"];
            }
            else
            {
                controlStatus = new Dictionary<string, ControlStatus>();
                parametersToChange = new Dictionary<string, object>();
                return false;
            }
        }

        public static TreeNode Transform(TreeCollection trees, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            if (trees.Count == 1)
            {
                return trees[0];
            }
            else
            {
                if ((bool)parameterValues["Consensus tree"])
                {
                    int skip = (int)(double)parameterValues["Skip:"];
                    int every = (int)(double)parameterValues["Every:"];
                    int until = (int)(double)parameterValues["Until:"];

                    bool clocklike = (bool)parameterValues["Treat trees as clock-like"];

                    if (skip == 0 && every == 1 && until == trees.Count)
                    {
                        TreeNode consensus = ((IReadOnlyList<TreeNode>)trees).GetConsensus(trees[0].Children.Count < 3 && trees[1].Children.Count < 3, clocklike, (double)parameterValues["Threshold:"], (int)parameterValues["Branch lengths:"] == 1, progressAction, true);

                        if (consensus != null)
                        {
                            return consensus;
                        }
                        else
                        {
                            throw new Exception("An error occurred while computing the consensus tree! If you have highly discordant trees, the threshold may be too high.");
                        }
                    }
                    else
                    {
                        int totalTrees = (until - skip) / every;

                        return trees.Take(until).Skip(skip).Where((item, index) => index % every == 0).GetConsensus(trees[0].Children.Count < 3 && trees[1].Children.Count < 3, clocklike, (double)parameterValues["Threshold:"], (int)parameterValues["Branch lengths:"] == 1, x => progressAction(x / totalTrees));
                    }
                }
                else
                {
                    return trees[(int)(double)parameterValues["Tree #"] - 1];
                }
            }
        }
    }
}
