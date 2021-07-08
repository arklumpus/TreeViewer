
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;

namespace SetUpAgeDistributions
{
    /// <summary>
    /// This module is used to set up the age distributions for the nodes, that can then be plotted using the _Plot age distributions_ (id ``) Plot
    /// Action module.
    /// 
    /// To use this module, you should open a tree file containing e.g. a sample from the posterior distribution of dated trees. This module will use
    /// all the trees in the file to compute the age distributions.
    /// </summary>


    public static class MyModule
    {
        public const string Name = "Set up age distributions";
        public const string HelpText = "Computes node age distributions.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "a1ccf05a-cf3c-4ca4-83be-af56f501c2a6";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Trees", "TreeCollection:" ),
                ( "StateData", "InstanceStateData:" ),
                
                /// <param name="Age type:">
                /// This parameter determines the kind of age that is computed.
                /// 
                /// If the value is `Since root`, the age of each node corresponds to the distance $d$ (as in, the
                /// sum of branch lengths) from the node to the root of the tree; in this case, the root node would
                /// have an age of `0`.
                /// 
                /// If the value is `Until tips`, first the total length $l$ of the tree from the root node to the
                /// most distant tip is computed; then, the age of each node is $d - l$. In this case, if all the
                /// tips of the tree are contemporaneous, they will have an age of `0`.
                /// </param>
                ( "Age type:", "ComboBox:0[\"Until tips\", \"Since root\"]"),
                
                /// <param name="Compute mean">
                /// If this check box is checked, in addition to the age distribution, the mean age for each node.
                /// </param>
                ( "Compute mean", "CheckBox:true" ),
                
                /// <param name="Credible interval:">
                /// This parameter determines what kind of credible interval for the age is computed. If the value is
                /// `None`, no credible interval is computed. If the value is `Highest-density`, the interval that contains
                /// the proportion of samples specified by the [Threshold](#parameter) with the highest probability
                /// density is computed. If the value is `Equal-tailed`, the interval corresponds to the symmetrical
                /// interval around the average that contains the specified proportion of samples.
                /// 
                /// The functions for computing credible intervals are based on code from the R package `bayestestR`,
                /// available under a GPLv3 licence [here](https://github.com/easystats/bayestestR/blob/e26dc16f3df711afbc2e79851442cfb5ea3b6f26/R/hdi.R).
                /// </param>
                ( "Credible interval:", "ComboBox:1[\"None\",\"Highest-density\",\"Equal-tailed\"]" ),
                
                /// <param name="Threshold">
                /// This parameter determines the threshold for the credible interval.
                /// </param>
                ( "Threshold:", "Slider:0.89[\"0\",\"1\",\"0.00\"]"),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();

            if ((int)currentParameterValues["Credible interval:"] != 0)
            {
                controlStatus.Add("Threshold:", ControlStatus.Enabled);
            }
            else
            {
                controlStatus.Add("Threshold:", ControlStatus.Hidden);
            }

            parametersToChange = new Dictionary<string, object>()
            {
                { "Apply", false }
            };

            return (bool)currentParameterValues["Apply"];
        }

        private static bool Compare(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
            {
                return false;
            }

            return !list1.Except(list2).Any();
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            bool fromLeft = (int)parameterValues["Age type:"] == 1;
            double threshold = (double)parameterValues["Threshold:"];
            int ciType = (int)parameterValues["Credible interval:"];
            bool computeMean = (bool)parameterValues["Compute mean"];

            TreeCollection treeCollection = (TreeCollection)parameterValues["Trees"];
            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            Dictionary<string, List<double>> ageSamples = new Dictionary<string, List<double>>();
            for (int i = 0; i < nodes.Count; i++)
            {
                ageSamples.Add(nodes[i].Id, new List<double>());
            }

            foreach (TreeNode sampledTree in treeCollection)
            {
                double treeHeight = -1;

                if (!fromLeft)
                {
                    treeHeight = sampledTree.LongestDownstreamLength();
                }

                foreach (TreeNode node in sampledTree.GetChildrenRecursiveLazy())
                {
                    TreeNode LCA = tree.GetLastCommonAncestor(node.GetLeafNames());

                    if (Compare(LCA.GetLeafNames(), node.GetLeafNames()))
                    {
                        double age = node.UpstreamLength();

                        if (!fromLeft)
                        {
                            age = treeHeight - age;
                        }

                        ageSamples[LCA.Id].Add(age);
                    }
                }
            }

            if (ciType > 0 || computeMean)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (ageSamples[nodes[i].Id].Count > 0)
                    {
                        if (computeMean)
                        {
                            nodes[i].Attributes["Mean age"] = ageSamples[nodes[i].Id].Average();
                        }

                        if (ciType == 1)
                        {
                            double[] hdi = BayesStats.HighestDensityInterval(ageSamples[nodes[i].Id], threshold);
                            nodes[i].Attributes[threshold.ToString("0%") + "_HDI"] = "[ " + hdi[0].ToString() + ", " + hdi[1].ToString() + "]";
                        }
                        else if (ciType == 2)
                        {
                            double[] eti = BayesStats.EqualTailedInterval(ageSamples[nodes[i].Id], threshold);
                            nodes[i].Attributes[threshold.ToString("0%") + "_ETI"] = "[ " + eti[0].ToString() + ", " + eti[1].ToString() + "]";
                        }
                    }
                }
            }

            stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = ageSamples;
            tree.Attributes["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = "Age distributions";
        }
    }
}

