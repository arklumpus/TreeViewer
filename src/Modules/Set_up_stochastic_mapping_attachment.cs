using System;
using System.Collections.Generic;
using PhyloTree;
using PhyloTree.Extensions;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;
using System.IO;
using System.Text;
using PhyloTree.Formats;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace a32858c9d0247497faeee03f7bfe24158
{
    //Do not change class name
    public static class MyModule
    {
        public const string Name = "Set up stochastic map (attachment)";
        public const string HelpText = "Parses the information from a stochastic mapping analysis contained in an attachment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "0e2f5255-2d34-474b-955d-b531ee5ba605";

        public static bool Repeatable { get; } = false;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADfSURBVDhPvZExDsIwDEVN4U6stCOqxD0YuyOI2DtyD6SKtawcCoG/E0duk1RMPCmyE+e78e+KCuxP909IF0kadF0nwqZpZH97vSWWqEKc0Pd9bHzcrmX9jL7gb8SnDsOw4zCamd3jeriEvMgGbvNFNBr9UeTMa9KAx8Me54rLmpjDiOtgco19NXc45/hM/MSZxsQDvyNq21ZqOTHgc7kfG+SwYl4QyPwYgWv43S5pgEK4kBMjH1HnKCyZaJ+t4oTFv6Azmy/CI+dTT9LAPi8YpYiY6xgtUjTReKAkYiKiL2r5XaqT8YTjAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE6SURBVEhL1ZSxjsIwDIZDeShW2hFV4j1u7I4AsXfkPZAq1rIeusc4HuMA/65dVWmiJqlu4JMix6n6O3EcL4yHze7ykuksRgGqqmLhoijYB/ffJ9ufR3xMZ4C6rhdN00yqJQXWE3wMfYooJWsy7fn7r1swJr+etjeZJ5MNqqUVq9h+EpnYfyP7Wi1l2mH7c8nKstR7yMVqkN6fg/clx0LlfSCz7zzmSO/pMLqDlHcwEM/xSGHhY937kqd6EZUw/2uJ92VN61z2zgCww15ko/fmE1egNQqg+HpRhLj7BCGEitM4RgewxQc+4MqhNZye591yIBDDzzSwQ5c/Smtwq4AYGdfO1ecgNjG9yM65Lc45x4ckXMcHEMc3Gs58R3VT2WmP+Lxz34XGBMDxWw0SIg6iylTSoCUJJkrRmDerf7I6L4dJ8wAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIeSURBVFhHxZY9TsNAEIXtcAF6aDhBMELQxinBgiMALaV7BBG96WhJjgAytKQFIQwnoIFjQHhvvXb8s453nUV80mg8a3vfeHZ2E9cxZO/s9gLuPI2Wpye9CdbESWsFwjAsffFwOHSeP3+cl6+ZHFkOnQrUvnhnveec7q4422vGK1hDewmiKHJpMhTYSKRLD4ykz7FZkRrogRlNhtbpUgGr/HsCf7Boc7B0A7gI5omBlAQWoqGnDBoTiOOYLz/y+vrpm474D5eH4sU2IH4Ddwyj4B2M73HOAxgTGiGJC2UCEI/h9tOolAC5RxKBvFZSEB9D5EQMSnBvFe4Kxvu+i7M973BM7Ba/PKOSAGmsxCLxInjulV7VhCXxBpTP6IpLuCyetV1QFUe8CRvQiwfqiApaSUAhzpjNx0olMq7CpXZ6PEIza2LRvQbxLGaTjxljPK8ErrPdkKgq4EtfopgEm5W+RVz0QOYBuz/jCMatGGptwyJBEGiLk8K4h/G36nPKHoAI97mqEmIMk7CEJuIcr4nDdz+KmQQmmWqKNybZmgBeFAcVXqo9u6w46bwNbYiTTgnYEifGCSwQ12rMKkYJVEXEoAQxj1bfRJyYVoAHSNI0KZMwESc6CRT/BW/A3tPLOqbixOgckAJ9TL6VjszpIk5Ml2AC86RYTldxYnwSFsT4c8vl6MP4w2IsTjodxUiCu4ENyZ74gE0grvVntYzj/ALyo00WSGSzcQAAAABJRU5ErkJggg==";

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
                ( "StateData", "InstanceStateData:" ),
                
                /// <param name="Attachment:">
                /// This parameter specifies the attachment from which the stochastic mapping data will be read.
                /// </param>
                ( "Attachment:", "Attachment:" ),

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
            Attachment attachment = (Attachment)parameterValues["Attachment:"];

            if (attachment == null)
            {
                return;
            }

            attachment.Stream.Seek(attachment.StreamStart, SeekOrigin.Begin);

            TreeCollection trees = new TreeCollection(OpenFile(attachment.Stream, progressAction));

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
				
				progressAction(0.5 + (double)sampleIndex / trees.Count * 0.25);
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
				progressAction(0.75 + (double)nodeIndex / nodeCorresps.Count * 0.25);
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


        private static List<TreeNode> OpenFile(Stream fileStream, Action<double> progressAction)
        {
			Action<double> nwkaProgressAction = (prog) => { progressAction(prog * 0.25); };
			
            List<TreeNode> trees = NWKA.ParseTrees(fileStream, progressAction: nwkaProgressAction, keepOpen: true).ToList();

            HashSet<string>[] characters = null;
	
			int treeIndex = 0;

            foreach (TreeNode tree in trees)
            {
                foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                {
					string attributeToRemove = null;
					
                    foreach (KeyValuePair<string, object> attribute in node.Attributes)
                    {
                        if (attribute.Key.StartsWith("Unknown") && attribute.Value is string attributeValue && attributeValue.StartsWith("{") && attributeValue.EndsWith("}"))
                        {
                            SimmapBranchState[] states = SimmapBranchState.Parse(attributeValue).ToArray();

                            if (states.Length > 0)
                            {
                                if (characters == null)
                                {
                                    characters = new HashSet<string>[states[0].States.Length];

                                    for (int i = 0; i < characters.Length; i++)
                                    {
                                        characters[i] = new HashSet<string>();
                                    }
                                }

                                node.Attributes.Add("States", System.Text.Json.JsonSerializer.Serialize(states, Modules.DefaultSerializationOptions));
                                node.Length = (from el in states select el.Length).Sum();

                                foreach (SimmapBranchState state in states)
                                {
                                    for (int i = 0; i < state.States.Length; i++)
                                    {
                                        characters[i].Add(state.States[i]);
                                    }
                                }
								
								attributeToRemove = attribute.Key;

                                break;
                            }
                        }
                    }
					
					if (!string.IsNullOrEmpty(attributeToRemove))
					{
						node.Attributes.Remove(attributeToRemove);
					}
                }

                tree.Attributes.Add("Characters", System.Text.Json.JsonSerializer.Serialize(characters, Modules.DefaultSerializationOptions));
				
				treeIndex++;
				progressAction(0.25 + (double)treeIndex / trees.Count * 0.25);
            }

            return trees;
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
