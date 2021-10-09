
using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace a32858c9d0247497faeee03f7bfe24158
{
    //Do not change class name
    public static class MyModule
    {
        public const string Name = "Set up stochastic map";
        public const string HelpText = "Parses the information from a stochastic mapping analysis.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "32858c9d-0247-497f-aeee-03f7bfe24158";

        public static bool Repeatable { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADbSURBVDhPvZIxDsIwDEVDOBQrdIRK3IOxO6IVe0fugZSyduZQCPI/TuQmLe3Ek1LbceJ+u12ZCfbn+1vcn2QFqqrixaIoGN+eL9oprNgBbdvGwqfNmmsxQcHfiFKdc1tveum5eVyPNZw5rJp2LxZcxEbQml9OwsjoEAP+Qp3M5ICHFKNCm044ialEF1E+c7YsyzCHnVjj++ee+pwdfIk7bIRcOJDh34R+KVn/F0Arymagkrw8A9UM0NUBYlFD4OszWQtIBsn64Bg4l7WQ9NvgofeUz9xiUulfjPkA1Xhe05963d8AAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEmSURBVEhL1ZTNDoIwDMcH+k5ehaOS+B4euRsk3jn6HiboFa8aH0Mfw492tGRsU9iIif6S0bXT/qHdFog3zFa7J00HYQikaSoTx3EsfeR8fUh7ublrWgWKogjKsuzM5iXMX/A3NCWCkkzBVNvTvQ4IER02iyPNvQmV3VKRZXTfi5Ds1wiXkxFNa3R/KGGSJNyHiCyLNP5/4XoO4Pdr/A9aCrUYkzWw3UWwbQNKlMHI4cTzHEGLYq31XncRg/3idSKHwQJIy8crx9imGMRBrg1MwqjJEV3MvOz6oJRhDy+TyCAA8RLMHAaXz01ASSyxfaleQteTrJeki8xVQK1/H/Lf6gGi9+EDUsTnNlWT6yUztrCPACdpysCQ36zjw6sHKkrJDEEhhHgBq81/e4N3lfMAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAH3SURBVFhHxZbBTsMwDIazwlvAY0xsR7ojq8R7cNwdsYn7jrwHUsd1Ow60x4DHgOE/c6osddI0y7RPspImaWM7ttOB6snD8/ucmpfD0+kU3PYh2+ag0wOz2ezI4slkor6+/9TuZ88jpxHjgZbFd7eFehpdqeFN7xNsEX0Ey+VyAOFHTQ5FUmJgwW1DTo+0oBjYQ/gxOykeyMrFFbg43qip6/qemjX6b5+/aED58fq44X4WxCOgzWtq9OYOayrFmMtGQR/cG8EAWz5F30NoTkNZs4XwYxDJA5Ll0dDG8NAIwv0g19wmQRvgngAbqpImNmwPNX1aC89CUFXNe+lpyJvjnoCsQ9byHDyr11uKqwIl1IiP0JzFlD7cqpg85o0byQMlt0cISpySjs27Yh3gNBS1ppqwolpQoc+uddetSHaHrhqStOYpBvT7IKoQWZRVVR1Z7rh9YQcY4PNu/incKz3p/vRZbltmE1rfmQWw0LaS+igw0vEYt0tIcwjabXIa5qK3AuS2MTUINBcEnA9pDkcw7owB4343eAz28RD5gzCkAM2dLw0NjoWNIjQupWksJX1Hp3NMDLT+ghl9sSTSvNupAGk6h9VGeFgCrm3N85gUtJrkNKQPI7jgHQhcKhYhwHO4Y/R6flfTGQN9cQLTWx3PCikR+Uum1D9s69NjHmXJAgAAAABJRU5ErkJggg==";

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

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {

            };
        }

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Trees", "TreeCollection:" ),
                ( "StateData", "InstanceStateData:" ),

                /// <param name="Treat trees as clock-like">
                /// If this check box is checked, it is assumed that the trees are clock-like trees, i.e. that the most recent tips in the tree represent
                /// taxa whose age is the same across all sampled trees. This results in all trees being aligned "to the right". Otherwise, it is assumed
                /// that the root node has the same age in all sampled trees, which results in the trees being aligned "to the left". This setting has no
                /// effect if all the sampled trees have the same branch lengths.
                /// </param>
                ( "Treat trees as clock-like", "CheckBox:"+tree.IsClockLike().ToString().ToLower() ),
                
                /// <param name="Resolution:">
                /// This parameter determines the resolution at which the character states are sampled. Increasing this parameter will yield a "smooother"
                /// plot, but it will increase the size of exported files and slow down the drawing process.
                /// </param>
                ( "Resolution:", "NumericUpDown:0.01[\"0\",\"Infinity\",\"0.01\",\"0.####\"]" ),

                /// <param name="Resolution unit:">
                /// This parameter determines the unit for the [Resolution](#resolution) parameter. If this is `Absolute`, the resolution is given in absolute
                /// tree units. If this is `Total tree length`, the resolution is intended as a fraction of the total tree length; e.g. if you set the resolution
                /// at `0.01`, the tree will be sampled at 100 intervals from the root to the tips. If the selected value is `Branch length`, the resolution is
                /// given as a fraction of the length of each branch; in this case a value of `0.01` will result in each branch being sampled 100
                /// times, regardless of its length.
                /// </param>
                ( "Resolution unit:", "ComboBox:1[\"Absolute\",\"Total tree length\",\"Branch length\"]" ),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ( "Apply", "Button:" )

            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>()
            {
                { "Apply", false }
            };

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            TreeCollection trees = (TreeCollection)parameterValues["Trees"];
            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            bool isClockLike = (bool)parameterValues["Treat trees as clock-like"];

            double resolution = (double)parameterValues["Resolution:"];
            int resolutionType = (int)parameterValues["Resolution unit:"];

            double actualResolution = resolution;

            switch (resolutionType)
            {
                case 0:
                    break;
                case 1:
                    actualResolution = resolution * tree.LongestDownstreamLength();
                    break;
                case 2:
                    break;
            }

            Dictionary<string, TreeNode> nodeCorresps = new Dictionary<string, TreeNode>();

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                nodeCorresps[node.GetLeafNames().OrderBy(a => a).Aggregate((a, b) => a + "," + b)] = node;
            }

            Dictionary<string, List<(double left, double right, SimmapBranchState[] states)>> branchStates = new Dictionary<string, List<(double, double, SimmapBranchState[])>>(nodeCorresps.Count);

            foreach (KeyValuePair<string, TreeNode> kvp in nodeCorresps)
            {
                branchStates.Add(kvp.Value.Id, new List<(double start, double end, SimmapBranchState[] states)>());
            }

            HashSet<string>[] states = null;

            int sampleIndex = 0;

            foreach (TreeNode sample in trees)
            {
                foreach (TreeNode node in sample.GetChildrenRecursiveLazy())
                {
                    SimmapBranchState[] nodeStates = null;

                    if (node.Attributes.TryGetValue("States", out object statesObj) && statesObj is string statesString)
                    {
                        try
                        {
                            nodeStates = System.Text.Json.JsonSerializer.Deserialize<SimmapBranchState[]>(statesString);
                        }
                        catch { }
                    }

                    if (nodeStates != null)
                    {
                        string leafString = node.GetLeafNames().OrderBy(a => a).Aggregate((a, b) => a + "," + b);

                        if (nodeCorresps.TryGetValue(leafString, out TreeNode corresp))
                        {
                            if (states == null)
                            {
                                states = new HashSet<string>[nodeStates[0].States.Length];
                                for (int i = 0; i < states.Length; i++)
                                {
                                    states[i] = new HashSet<string>();
                                }
                            }

                            for (int i = 0; i < nodeStates.Length; i++)
                            {
                                for (int j = 0; j < nodeStates[i].States.Length; j++)
                                {
                                    states[j].Add(nodeStates[i].States[j]);
                                }
                            }

                            double left;
                            double right;

                            if (!isClockLike)
                            {
                                right = node.UpstreamLength();
                                left = right - node.Length;
                            }
                            else
                            {
                                right = node.LongestDownstreamLength();
                                left = right + node.Length;
                            }

                            branchStates[corresp.Id].Add((left, right, nodeStates));
                        }
                    }
                }

                sampleIndex++;
                progressAction((double)sampleIndex / trees.Count * 0.5);
            }

            List<List<string>> allPossibleStates = GetAllPossibleStates(states);

            Dictionary<string, (double samplePosPerc, double[] stateProbs)[]> preparedStates = new Dictionary<string, (double, double[])[]>();

            int nodeIndex = 0;

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (!double.IsNaN(node.Length) && node.Length > 0)
                {
                    double left;
                    double right;

                    if (!isClockLike)
                    {
                        right = node.UpstreamLength();
                        left = right - node.Length;
                    }
                    else
                    {
                        right = node.LongestDownstreamLength();
                        left = right + node.Length;
                    }

                    if (resolutionType == 2)
                    {
                        actualResolution = resolution * node.Length;
                    }

                    List<(double left, double right, SimmapBranchState[] states)> observedStates = branchStates[node.Id];

                    double[] meanPosterior = new double[allPossibleStates.Count];

                    for (int i = 0; i < observedStates.Count; i++)
                    {
                        for (int j = 0; j < allPossibleStates.Count; j++)
                        {
                            if (allPossibleStates[j].SequenceEqual(observedStates[i].states[0].States))
                            {
                                meanPosterior[j]++;
                                break;
                            }
                        }
                    }

                    if (observedStates.Count > 0)
                    {
                        for (int i = 0; i < meanPosterior.Length; i++)
                        {
                            meanPosterior[i] /= observedStates.Count;
                        }
                    }

                    {
                        string state = "{";

                        for (int j = 0; j < allPossibleStates.Count; j++)
                        {
                            state += allPossibleStates[j].Aggregate((a, b) => a + "|" + b) + ":" + meanPosterior[j].ToString(System.Globalization.CultureInfo.InvariantCulture);

                            if (j < allPossibleStates.Count - 1)
                            {
                                state += ",";
                            }
                        }

                        state += "}";

                        node.Attributes["MeanPosteriors"] = state;
                    }


                    List<(double samplePosPerc, double[] stateProbs)> preparedBranchStates = new List<(double samplePosPerc, double[] stateProbs)>();

                    for (int i = 0; i < Math.Ceiling(Math.Abs(right - left) / actualResolution) + 1; i++)
                    {
                        double samplingTime;
                        if (!isClockLike)
                        {
                            samplingTime = Math.Min(left + actualResolution * i, right);
                        }
                        else
                        {
                            samplingTime = Math.Max(left - actualResolution * i, right);
                        }

                        double perc = (samplingTime - left) / (right - left);



                        int[] counts = new int[allPossibleStates.Count];

                        for (int j = 0; j < observedStates.Count; j++)
                        {
                            string[] sample = SampleBranch(samplingTime, observedStates[j].left, observedStates[j].right, observedStates[j].states);

                            if (sample != null)
                            {
                                for (int k = 0; k < allPossibleStates.Count; k++)
                                {
                                    if (sample.SequenceEqual(allPossibleStates[k]))
                                    {
                                        counts[k]++;
                                        break;
                                    }
                                }
                            }
                        }

                        double total = counts.Sum();

                        double[] probs = new double[counts.Length];

                        if (total > 0)
                        {
                            for (int j = 0; j < probs.Length; j++)
                            {
                                probs[j] = counts[j] / total;
                            }
                        }

                        preparedBranchStates.Add((perc, probs));

                        if (samplingTime == right)
                        {
                            if (total > 0)
                            {
                                string state = "{";

                                for (int j = 0; j < allPossibleStates.Count; j++)
                                {
                                    state += allPossibleStates[j].Aggregate((a, b) => a + "|" + b) + ":" + probs[j].ToString(System.Globalization.CultureInfo.InvariantCulture);

                                    if (j < allPossibleStates.Count - 1)
                                    {
                                        state += ",";
                                    }
                                }

                                state += "}";

                                node.Attributes["ConditionedPosteriors"] = state;
                            }
                            else
                            {
                                node.Attributes["ConditionedPosteriors"] = node.Attributes["MeanPosteriors"];
                            }

                            break;
                        }
                    }

                    preparedStates[node.Id] = preparedBranchStates.ToArray();
                }
                else if (node.Parent == null && node.Children.Count > 0)
                {
                    List<(double left, double right, SimmapBranchState[] states)> observedStates = branchStates[node.Children[0].Id];

                    double[] meanPosterior = new double[allPossibleStates.Count];

                    for (int i = 0; i < observedStates.Count; i++)
                    {
                        for (int j = 0; j < allPossibleStates.Count; j++)
                        {
                            if (allPossibleStates[j].SequenceEqual(observedStates[i].states[observedStates[i].states.Length - 1].States))
                            {
                                meanPosterior[j]++;
                                break;
                            }
                        }
                    }

                    if (observedStates.Count > 0)
                    {
                        for (int i = 0; i < meanPosterior.Length; i++)
                        {
                            meanPosterior[i] /= observedStates.Count;
                        }
                    }

                    {
                        string state = "{";

                        for (int j = 0; j < allPossibleStates.Count; j++)
                        {
                            state += allPossibleStates[j].Aggregate((a, b) => a + "|" + b) + ":" + meanPosterior[j].ToString(System.Globalization.CultureInfo.InvariantCulture);

                            if (j < allPossibleStates.Count - 1)
                            {
                                state += ",";
                            }
                        }

                        state += "}";

                        node.Attributes["MeanPosteriors"] = state;
                        node.Attributes["ConditionedPosteriors"] = state;
                    }
                }

                nodeIndex++;
                progressAction(0.5 + (double)nodeIndex / nodeCorresps.Count * 0.5);
            }

            stateData.Tags["32858c9d-0247-497f-aeee-03f7bfe24158"] = preparedStates;
            stateData.Tags["32858c9d-0247-497f-aeee-03f7bfe24158/states"] = (from el in states select el.ToArray()).ToArray();
            tree.Attributes["32858c9d-0247-497f-aeee-03f7bfe24158"] = "Stochastic map";
        }

        private static string[] SampleBranch(double samplingTime, double left, double right, SimmapBranchState[] branchStates)
        {
            double sign = Math.Sign(right - left);
            double currTime = right;
            SimmapBranchState currState = branchStates[0];
            int currIndex = 0;

            while (Math.Sign(currTime - samplingTime) == Math.Sign(right - samplingTime) && currIndex < branchStates.Length)
            {
                currTime -= branchStates[currIndex].Length * sign;
                currState = branchStates[currIndex];
                currIndex++;
            }

            if (Math.Sign(currTime - samplingTime) != Math.Sign(right - samplingTime))
            {
                return currState.States;
            }
            else
            {
                return null;
            }
        }

        private static List<List<string>> GetAllPossibleStates(IReadOnlyList<IEnumerable<string>> characterStates)
        {
            List<List<string>> tbr = new List<List<string>>();

            if (characterStates.Count <= 1)
            {
                foreach (string sr in characterStates[0])
                {
                    tbr.Add(new List<string> { sr });
                }
            }
            else
            {
                List<List<string>> otherStates = GetAllPossibleStates(characterStates.Skip(1).ToArray());

                foreach (string sr in characterStates[0])
                {
                    for (int i = 0; i < otherStates.Count; i++)
                    {
                        List<string> item = new List<string>() { sr };
                        item.AddRange(otherStates[i]);
                        tbr.Add(item);
                    }
                }
            }

            return tbr;
        }
    }

    internal class SimmapBranchState
    {
        public string[] States { get; set; }
        public double Length { get; set; }

        public SimmapBranchState()
        {

        }

        public static IEnumerable<SimmapBranchState> Parse(string value)
        {
            if (value.StartsWith("{") && value.EndsWith("}"))
            {
                value = value.Substring(1, value.Length - 2);

                string[] splitValue = value.Split(':');

                foreach (string sr in splitValue)
                {
                    string[] splitSr = sr.Split(',');
                    string[] states = splitSr[0].Split('|');
                    double length = double.Parse(splitSr[1], System.Globalization.CultureInfo.InvariantCulture);

                    yield return new SimmapBranchState() { Length = length, States = states };
                }
            }
        }
    }
}

