
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
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;


namespace SetUpAgeDistributions
{
    /// <summary>
    /// This module is used to set up the age distributions for the nodes, that can then be plotted using the _Plot age distributions_ (id `5dbe1f3c-dbea-49b3-8f04-f319aefca534`) Plot
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
        public static Version Version = new Version("1.0.3");
        public const string Id = "a1ccf05a-cf3c-4ca4-83be-af56f501c2a6";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADDSURBVDhPnZFhDsIgDIXB400T78L/ZS7+5y4mm9fTvkLNW7PB4pcMCl3b1xIDMYyvh2xTOYX5/bzj3ORSd8OCwVQTNvEJPN0k2oKTfsRuS1GCP9X+i1MJpLIf9i+m1cJGckoJQWvO+VZuCjpE/Oir4E6C8LHCKxbcwQe7+wpYOAnZ6vMJZiymRuSaKkiP9bziwnwb2YxUWmRTyZRIYUWtFjS4g6ppgmpVjQKbFRy2APjHPdBa7xV0qDwDstV3Gi+9EMIXKnxdPWrCJUAAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE8SURBVEhLzZQxrsIwDIZTOAuH6ERnQGLmGt1Rid7ec7AiASuIpQs34DA823WqNE5DE1GJT0qdOPR3YptmaoDV/nQAU8HQ178tzpOYs+1hiSPFYrnLXo/jnddRzNi6GHFDxUGjEQECQklBeilyUuMjOl1U5BHCQ1xgPENNkIH4m+eTMFTkrzF5gOlrwJYYGSjqn91rU2w/bEOYFq1HIMTLsjzkeX6DkTVNI9pX1CBwOhJHQRhvtOw3Nybr7nu/RZ5bdCfH05IH9vHUaNsl7eHaBCzgRtrbRSym25VIi/Ejbr3sNf2uV+Sx8PVR7FLX9YacAPjPYNYwNPjpUFEBLGECRMT7mH+eIv4UBXBT8okqNoCd/zHo36oB4tYhAAVJ+djZ4m7KRAunBDAiXRoMvO728ZFUAxsrZSKgUkr9AzIljLd4qVvLAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHSSURBVFhHxZZNTsMwEIUTOAs3YNEd7ZYiseYa2SOI2OccbJEo21asuugNOAy858xETjKO8+PSTxo5cRLP83g8Tp5FuH/+eEXzAiu/3h55nZQraYegc9eKmKQMCjAcJhcRi4DO3iepiKCAiJNkIq6lbSGDW7P3Wd/cPeU/3+8HuZ9FT8BI58piEc02nOg4xA52mrJdnYBEzocICsvh/FeuL8KYQnRWki8Bwtwq7xLh8BJIm0SEOi+K4si2qqoV2yFaaslYId2ZKnD+iWZb32U7iHiQaxNzkJgIb6Ya0gMcuVqAvlZSo1/fXaOhsa9ZCjMJZa3K+s5GnFMkbS8zN5Fne5h7X751BHeBlTARthi4t6WlT5ekR2wbWlHQviVnQPOtmQM+so2U1l+RhLY7O7fl6svsFtZ77ifmGAG61uYvWSfspZ9gRNa7SWhNSiUqwCI0c39mPkPvTy7FGIxFxkoqDbuF9YxJe7z4WTBZAMLG8spE68KEC2E94xKsZuWAD8L4/0mohJILlm4bhoBz1nWW1zlsIMIVoyVJ6A6WmTTfptwFDG0votJnJa1jtgAMzOTiuUBjSIPnvjzbwNz78q1jURJadBIzWB3PCkQcaXI7QJb9AbxO0TY21g8YAAAAAElFTkSuQmCC";

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

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            bool fromLeft = (int)parameterValues["Age type:"] == 1;
            double threshold = (double)parameterValues["Threshold:"];
            int ciType = (int)parameterValues["Credible interval:"];
            bool computeMean = (bool)parameterValues["Compute mean"];
            string name = (string)parameterValues["Name:"];

            TreeCollection treeCollection = (TreeCollection)parameterValues["Trees"];
            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            List<TreeNode> nodes = tree.GetChildrenRecursive();

            Dictionary<string, List<double>> ageSamples = new Dictionary<string, List<double>>();
            for (int i = 0; i < nodes.Count; i++)
            {
                ageSamples.Add(nodes[i].Id, new List<double>());
            }

            int sampleIndex = 0;

            TreeNode treeClone = tree;

            object progressLock = new object();
            object addLock = new object();
			object collectionLock = new object();

             System.Threading.Tasks.Parallel.For(0, treeCollection.Count, new ParallelOptions() { MaxDegreeOfParallelism = 6 }, i =>
              {
                  TreeNode sampledTree;

                  lock (collectionLock)
                  {
                      sampledTree = treeCollection[i];
                  }
				  
                  double treeHeight = -1;

                  if (!fromLeft)
                  {
                      treeHeight = sampledTree.LongestDownstreamLength();
                  }

                  foreach (TreeNode node in sampledTree.GetChildrenRecursiveLazy())
                  {
                      TreeNode LCA = treeClone.GetLastCommonAncestor(node.GetLeafNames());

                      if (Compare(LCA.GetLeafNames(), node.GetLeafNames()))
                      {
                          double age = node.UpstreamLength();

                          if (!fromLeft)
                          {
                              age = treeHeight - age;
                          }

                          lock (addLock)
                          {
                              ageSamples[LCA.Id].Add(age);
                          }
                      }
                  }

                  lock (progressLock)
                  {
                      sampleIndex++;
                      progressAction((double)sampleIndex / treeCollection.Count);
                  }
              });

            double scalingFactor = (double)parameterValues["Scaling factor:"];
            bool applyScalingToTree = (bool)parameterValues["Apply scaling to transformed tree"];

            if (scalingFactor != 1)
            {
                foreach (KeyValuePair<string, List<double>> kvp in ageSamples)
                {
                    for (int i = 0; i < kvp.Value.Count; i++)
                    {
                        kvp.Value[i] *= scalingFactor;
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

            if (ciType > 0 || computeMean)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    if (ageSamples[nodes[i].Id].Count > 0)
                    {
                        if (computeMean)
                        {
                            nodes[i].Attributes["Mean age"] = ageSamples[nodes[i].Id].Average();
                            nodes[i].Attributes[name + " mean age"] = ageSamples[nodes[i].Id].Average();
                        }

                        if (ciType == 1)
                        {
                            double[] hdi = BayesStats.HighestDensityInterval(ageSamples[nodes[i].Id], threshold);
                            nodes[i].Attributes[threshold.ToString("0%") + "_HDI"] = "[ " + hdi[0].ToString() + ", " + hdi[1].ToString() + "]";
                            nodes[i].Attributes[name + "_" + threshold.ToString("0%") + "_HDI"] = "[" + hdi[0].ToString() + ", " + hdi[1].ToString() + "]";
                        }
                        else if (ciType == 2)
                        {
                            double[] eti = BayesStats.EqualTailedInterval(ageSamples[nodes[i].Id], threshold);
                            nodes[i].Attributes[threshold.ToString("0%") + "_ETI"] = "[ " + eti[0].ToString() + ", " + eti[1].ToString() + "]";
                            nodes[i].Attributes[name + "_" + threshold.ToString("0%") + "_ETI"] = "[ " + eti[0].ToString() + ", " + eti[1].ToString() + "]";
                        }
                    }
                }
            }

            stateData.Tags["a1ccf05a-cf3c-4ca4-83be-af56f501c2a6"] = ageSamples;
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

            distributionCollection[(string)parameterValues[Modules.ModuleIDKey]] = (name, ageSamples);
        }
    }
}


