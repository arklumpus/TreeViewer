using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Styling;

namespace af7a20f2f94b243318bbf4e0087da6fba
{
    public static class MyModule
    {
        public const string Name = "Stochastic mapping branches";
        public const string HelpText = "Plots branches with data from a stochastic mapping analysis.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "f7a20f2f-94b2-4331-8bbf-4e0087da6fba";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAB3SURBVDhPY2TAAbxqN/6HMvECDAOKiorAGp2cnMD8aSf/gmlcgAlKo4C+vj64wVnmzGBMNIC5gFiA1QWkAIoNYMQW2tua/XHGDjqAuwAWWCQFGBAwkaMJLxh6scACpTEAeuzgCieCeYEQwBnfW7ZsISkwyQQMDADr0yAozbzqWwAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADKSURBVEhLY2QgAnjVbvwPZZIMmKA0XpBlzsxgIk2UWzAATl1FRUVwVzs5OUFZDAynnvxjOPOUeA/h9UFfXx8jCEO5YGAmw0SSj4gKImyAVIswAHIQUQLI9gGxYOhbwIgvE21r9iczBhEAqw9AKQOUQqgBUCyAGQxKgtQCjFu2bIEH0d9//xmYmRCh4uPjQ90gQjac5mA0o8EAzS3AGaugOIAV1bgyIyhZE0rSRFmAnJRJBXgtgDJRajRSAU4LkAElPqB5JNMYMDAAACbxOEPrNHrzAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEMSURBVFhHYxhowAiliQZetRsbgFQ9hEc5YILSpACqWQ4CBEOgqKgIxcdOTk5g+tSTf2D6zNP/YJpcwAylcQJLS8v9UCYYKCoqgmlpPkYwNpVhAmOQM6SA/GefwdJEA2JCAOzFvr4+sNotW7YQ5WViQ4icNNAIpfECM2CogHCWOTMYm0gzgjHJABQCsFCgBSAnBKgKRh0w6gBGYNlOVArf1uxPRh4iDAZ3FIAKDlhBQiuA4QBkS0ElGa0BI7Flu4+Pz/BMAwTBaF1AazDqgAF3AMltQmL7BbDmF6HCjJwQwGs5rO0HaxMSAkSHAAzA+gXUAsSEAFGtYHIByeU7sXUHsYCcNEDTEKEzYGAAAOtJReHGRPMBAAAAAElFTkSuQmCC";

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

        // This method should return a list of tuples representing the global parameters required by this module. The
        // first item of each tuple is the name of the parameter, the second element is the parameter type. These will
        // be presented to the user in the "Preferences" window. See the TreeViewer manual for details of the possible
        // parameter values and options. Note that this method is only called once, when the module is loaded.
        public static List<(string, string)> GetGlobalSettings()
        {
            // TODO: return the list of required parameters.
            // E.g.:
            //      return new List<(string, string)>()
            //      {
            //          ("Default colour:", "Colour:[0,162,232,255]"),
            //      };

            return new List<(string, string)>()
            {

            };
        }

        // This method should return a list of tuples representing the parameters required by this module. The first
        // item of each tuple is the name of the parameter, the second element is the parameter type. These will be
        // presented to the user in the interface. See the TreeViewer manual for details of the possible parameter
        // values and options. Note that this method is called once every time the module is added to the plot.
        //
        // tree: the final transformed tree that is being plotted.
        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            // TODO: return the list of required parameters.
            // E.g.:
            //      return new List<(string, string)>()
            //      {
            //          ("Size", "Group:2"),
            //          ("Width:", "NumericUpDown:100[\"0\",\"Infinity\"]"),
            //          ("Height:", "NumericUpDown:100[\"0\",\"Infinity\"]"),
            //      };

            return new List<(string, string)>()
            {
                ( "Window", "Window:" ),
                ( "StateData", "InstanceStateData:" ),

                ( "Characters and states", "Group:5" ),
                
                /// <param name="Total characters:">
                /// This text box shows the total number of characters that have been set up and can be plotted.
                /// </param>
                ( "Total characters:", "TextBox:" ),

                /// <param name="Enabled characters:" default="">
                /// This parameter contains a script that is used to determine which characters are enabled in the plot. Each character is associated to a
                /// boolean value - `true` means that the character is enabled, while `false` means that the character is not enabled. This can be changed
                /// more easily using the [Wizard edit enabled characters](#wizard-edit-enabled-characters) button.
                /// </param>
                ( "Enabled characters:", "SourceCode:" + GetDefaultEnabledCharactersCode(new string[0][]) ),
                
                /// <param name="Wizard edit state colours">
                /// This button opens a window that can be used to specify which characters are enabled using a graphical interface.
                /// </param>
                ( "Wizard edit enabled characters", "Button:" ),

                /// <param name="State colours:">
                /// This parameter is used to determine the colour associated to each state. While this uses a "Colour by node" control, the colours are actually
                /// determined based on the names of the states, rather than on attributes of the tree. The colours associated with each state can be changed
                /// (or additional states can be added) by modifying the formatter code for this parameter.
                /// 
                /// The colour associated to each state can be changed by changing the RGB values in the `Colour.FromRgb` method calls. The possible states can
                /// be changed by modifying the `case` labels in the `switch` statement.
                /// 
                /// The colours can also be changed more easily by using the [Wizard edit state colours](#wizard-edit-state-colours) button.
                /// </param>
                ("State colours:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(GetDefaultStateColours(new string[0][])) + ",\"(N/A)\",\"String\",\"220\",\"220\",\"220\",\"255\",\"true\"]"),
                    
                /// <param name="Wizard edit state colours">
                /// This button opens a window that can be used to specify the state colours using a graphical interface.
                /// </param>
                ( "Wizard edit state colours", "Button:" ),
                
                /// <param name="Position shift:">
                /// The value of this parameter corresponds to a shift in the position of all the branches. Useful if you wish to plot multiple histories on the same
                /// tree.
                /// </param>
                ("Position shift:", "Point:[0,0]"),

                ( "Appearance", "Group:8" ),
                
                /// <param name="Shape:">
                /// This parameter determines the shape of the branches.
                /// </param>
                ("Shape:", "ComboBox:0[\"Rectangular\",\"Radial\",\"Circular\"]"),

                /// <param name="Branch thickness:">
                /// This parameter determines the thickness of the branches.
                /// </param>
                ("Branch thickness:", "NumericUpDown:5[\"0\",\"Infinity\"]"),

                /// <param name="Style:">
                /// This parameter determines the style of the branches.
                /// 
                /// If the selected value is `All states`, a "thick" branch is drawn using multiple colours,
                /// and the thickness of each colour is proportional to the probability of the state represented by that colour.
                /// 
                /// If the selected value is `Most probable states`, the branch is drawn using only one colour at a time, corresponding to the state(s) that pass the filtering process defined by the [Dominance threshold](#dominance-threshold)
                /// and the [Exclusion threshold](#exclusion-threshold).
                /// 
                /// The filtering consists in the following:
                /// * First, all states whose probability is lower than the exclusion threshold are excluded. However, if no states have a probability higher than the exclusion threshold, all states are retained.
                /// * Then, the remaining probabilities are scaled so that their sum is 1.
                /// * Finally, all states whose scaled probability is lower than the dominance threshold are excluded. However if no states have a probability higher than the dominance threshold, all states are retained.
                /// 
                /// If only one state passed this filtering, the branch is drawn in the colour corresponding to that state; otherwise, the branch is drawn using dashes of colours corresponding to the the filtered states.
                /// 
                /// If the selected value is `Maximum a posteriori`, the history with the highest posterior probability is shown, i.e. a history in which the state of each branch corresponds to the most probable state at each point in time.
                /// </param>
                ( "Style:", "ComboBox:0[\"All states\",\"Most probable states\",\"Maximum a posteriori\"]"),
                
                /// <param name="Dominance threshold:">
                /// If the [Style](#style) is set to `Most probable states`, this parameter determines the dominance threshold.
                /// </param>
                ("Dominance threshold:", "Slider:0.6[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Exclusion threshold:">
                /// If the [Style](#style) is set to `Most probable states`, this parameter determines the exclusion threshold.
                /// </param>
                ("Exclusion threshold:", "Slider:0.10[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Dash unit:">
                /// If the [Style](#style) is set to `Most probable states`, this parameter determines the spacing between dashes of different colours used to draw branches with a "mixed" state.
                /// </param>
                ("Dash unit:", "NumericUpDown:5[\"0\",\"Infinity\"]"),
                
                /// <param name="Gradually increase thickness">
                /// If this check box is checked, the branch starts off with thickness 0 and then grows bigger.
                /// </param>
                ("Gradually increase thickness", "CheckBox:true"),
                
                /// <param name="Thickness maximum:">
                /// If the [Gradually increase thickness](#gradually-increase-thickness) check box is checked, this parameter
                /// determines when the maximum thickness is reached.
                /// </param>
                ("Thickness maximum:", "Slider:0.2[\"0\",\"1\"]"),
                
                /// <param name="Add legend">
                /// This button adds an instance of the _Legend_ module containing a legend of the colours associated to each character state.
                /// </param>
                ( "Add legend", "Button:" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>()
            {
                { "Total characters:", ControlStatus.Disabled }
            };

            if ((int)currentParameterValues["Style:"] == 0)
            {
                controlStatus["Dominance threshold:"] = ControlStatus.Hidden;
                controlStatus["Exclusion threshold:"] = ControlStatus.Hidden;
                controlStatus["Dash unit:"] = ControlStatus.Hidden;
                controlStatus["Gradually increase thickness"] = ControlStatus.Enabled;
                controlStatus["Thickness maximum:"] = ControlStatus.Enabled;
            }
            else if ((int)currentParameterValues["Style:"] == 1)
            {
                controlStatus["Dominance threshold:"] = ControlStatus.Enabled;
                controlStatus["Exclusion threshold:"] = ControlStatus.Enabled;
                controlStatus["Dash unit:"] = ControlStatus.Enabled;
                controlStatus["Gradually increase thickness"] = ControlStatus.Hidden;
                controlStatus["Thickness maximum:"] = ControlStatus.Hidden;
            }

            parametersToChange = new Dictionary<string, object>() { { "Wizard edit state colours", false }, { "Wizard edit enabled characters", false }, { "Add legend", false } };

            if (((InstanceStateData)currentParameterValues["StateData"]).Tags.TryGetValue("32858c9d-0247-497f-aeee-03f7bfe24158/states", out object statesObj) && statesObj is string[][] states)
            {
                string statesString = states.Length.ToString();

                if (statesString != (string)currentParameterValues["Total characters:"])
                {
                    parametersToChange["Total characters:"] = statesString;

                    string code = GetDefaultStateColours(states);

                    object[] formatterParams = new object[2] { code, false };

                    ColourFormatterOptions cfo = new ColourFormatterOptions(code, formatterParams) { AttributeName = "(N/A)", AttributeType = "String", DefaultColour = Colour.FromRgb(220, 220, 220) };

                    if (currentParameterValues["State colours:"] == previousParameterValues["State colours:"])
                    {
                        parametersToChange["State colours:"] = cfo;
                    }

                    if (currentParameterValues["Enabled characters:"] == previousParameterValues["Enabled characters:"])
                    {
                        parametersToChange["Enabled characters:"] = new CompiledCode(GetDefaultEnabledCharactersCode(states));
                    }
                }
                else if (currentParameterValues["Enabled characters:"] != previousParameterValues["Enabled characters:"])
                {
                    Assembly assembly = ((CompiledCode)currentParameterValues["Enabled characters:"]).CompiledAssembly;

                    object[] args = new object[] { states };

                    bool[] enabledCharacters = (bool[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("GetEnabledCharacters", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);

                    string[][] enabledCharacterStates = GetEnabledStates(states, enabledCharacters).ToArray();

                    string code = GetDefaultStateColours(enabledCharacterStates);

                    object[] formatterParams = new object[2] { code, false };

                    ColourFormatterOptions cfo = new ColourFormatterOptions(code, formatterParams) { AttributeName = "(N/A)", AttributeType = "String", DefaultColour = Colour.FromRgb(220, 220, 220) };

                    if (currentParameterValues["State colours:"] == previousParameterValues["State colours:"])
                    {
                        parametersToChange["State colours:"] = cfo;
                    }
                }

                if ((bool)currentParameterValues["Wizard edit state colours"])
                {
                    InstanceStateData stateData = (InstanceStateData)currentParameterValues["StateData"];

                    Assembly assembly = ((CompiledCode)currentParameterValues["Enabled characters:"]).CompiledAssembly;

                    object[] args = new object[] { states };

                    bool[] enabledCharacters = (bool[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("GetEnabledCharacters", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);

                    string[][] enabledCharacterStates = GetEnabledStates(states, enabledCharacters).ToArray();

                    int moduleIndex = -1;

                    List<PlottingModule> plottingModules = stateData.PlottingModules();
                    for (int i = 0; i < plottingModules.Count; i++)
                    {
                        if (plottingModules[i].Id == Id)
                        {
                            if ((string)stateData.GetPlottingModulesParameters(i)[Modules.ModuleIDKey] == (string)currentParameterValues[Modules.ModuleIDKey])
                            {
                                moduleIndex = i;
                                break;
                            }
                        }
                    }

                    ColourFormatterOptions StateColours = (ColourFormatterOptions)currentParameterValues["State colours:"];
                    Colour defaultStateColour = StateColours.DefaultColour;
                    Func<object, Colour?> stateColourFormatter = StateColours.Formatter;

                    _ = ShowWizardEditWindow((Avalonia.Controls.Window)currentParameterValues["Window"], stateData, enabledCharacterStates, moduleIndex, defaultStateColour, stateColourFormatter);
                }

                if ((bool)currentParameterValues["Wizard edit enabled characters"])
                {
                    InstanceStateData stateData = (InstanceStateData)currentParameterValues["StateData"];

                    Assembly assembly = ((CompiledCode)currentParameterValues["Enabled characters:"]).CompiledAssembly;

                    object[] args = new object[] { states };

                    bool[] enabledCharacters = (bool[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("GetEnabledCharacters", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);

                    string[][] enabledCharacterStates = GetEnabledStates(states, enabledCharacters).ToArray();

                    int moduleIndex = -1;

                    List<PlottingModule> plottingModules = stateData.PlottingModules();
                    for (int i = 0; i < plottingModules.Count; i++)
                    {
                        if (plottingModules[i].Id == Id)
                        {
                            if ((string)stateData.GetPlottingModulesParameters(i)[Modules.ModuleIDKey] == (string)currentParameterValues[Modules.ModuleIDKey])
                            {
                                moduleIndex = i;
                                break;
                            }
                        }
                    }

                    _ = ShowWizardEditEnabledCharactersWindow((Avalonia.Controls.Window)currentParameterValues["Window"], stateData, states, enabledCharacters, moduleIndex);
                }

                if ((bool)currentParameterValues["Add legend"])
                {
                    try
                    {
                        InstanceStateData stateData = (InstanceStateData)currentParameterValues["StateData"];

                        Assembly assembly = ((CompiledCode)currentParameterValues["Enabled characters:"]).CompiledAssembly;

                        object[] args = new object[] { states };

                        bool[] enabledCharacters = (bool[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("GetEnabledCharacters", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);

                        string[][] enabledCharacterStates = GetEnabledStates(states, enabledCharacters).ToArray();


                        if (enabledCharacterStates.Length > 0)
                        {

                            ColourFormatterOptions stateColoursCFO = (ColourFormatterOptions)currentParameterValues["State colours:"];

                            List<List<string>> allPossibleStates = GetAllPossibleStates(states, enabledCharacters);

                            List<Colour> stateColours = new List<Colour>(allPossibleStates.Count);

                            for (int i = 0; i < allPossibleStates.Count; i++)
                            {
                                stateColours.Add(stateColoursCFO.Formatter(allPossibleStates[i].Aggregate((a, b) => a + "|" + b)) ?? stateColoursCFO.DefaultColour);
                            }


                            System.Text.StringBuilder legendBuilder = new System.Text.StringBuilder();

                            legendBuilder.AppendLine("### **Character states**");
                            legendBuilder.AppendLine();

                            if (enabledCharacterStates.Length == 1)
                            {
                                for (int i = 0; i < allPossibleStates.Count; i++)
                                {
                                    legendBuilder.AppendLine("![](circle://8," + stateColours[i].ToHexString() + ") `" + allPossibleStates[i][0] + "`");
                                    legendBuilder.AppendLine();
                                }
                            }
                            else
                            {
                                legendBuilder.AppendLine("+---------------------------+---------------------------+---------------------------+");
                                legendBuilder.Append("|                           |");
                                for (int i = 0; i < enabledCharacters.Length; i++)
                                {
                                    if (enabledCharacters[i])
                                    {
                                        string item = " _" + i.ToString() + "_";
                                        legendBuilder.Append(item);
                                        legendBuilder.Append(' ', 27 - item.Length);
                                        legendBuilder.Append("|");
                                    }
                                }
                                legendBuilder.AppendLine();
                                legendBuilder.AppendLine("+---------------------------+---------------------------+---------------------------+");

                                for (int i = 0; i < allPossibleStates.Count; i++)
                                {
                                    legendBuilder.Append("|");
                                    string item = " ![](circle://8," + stateColours[i].ToHexString() + ")";
                                    legendBuilder.Append(item);
                                    legendBuilder.Append(' ', 27 - item.Length);
                                    legendBuilder.Append("|");

                                    for (int j = 0; j < allPossibleStates[i].Count; j++)
                                    {
                                        item = " `" + allPossibleStates[i][j] + "`";
                                        legendBuilder.Append(item);
                                        legendBuilder.Append(' ', 27 - item.Length);
                                        legendBuilder.Append("|");
                                    }

                                    legendBuilder.AppendLine();
                                    legendBuilder.AppendLine("+---------------------------+---------------------------+---------------------------+");
                                }
                            }

                            Action<Dictionary<string, object>> updater = stateData.AddPlottingModule(Modules.GetModule(Modules.PlottingModules, "06888353-e930-4d08-ab24-5727bced8cd6"));
                            double width = Math.Max(157, 157 + 27 * (enabledCharacterStates.Length - 5));

                            updater(new Dictionary<string, object>() { { "Markdown source:", legendBuilder.ToString() }, { "Width:", width } });

                            if (InstanceStateData.IsUIAvailable)
                            {
                                MainWindow window = (MainWindow)currentParameterValues["Window"];
                                window.AddPlottingModuleAccessoriesAndUpdate();
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        _ = new MessageBox("Attention!", "An error occurred while generating the legend!\n" + ex.Message).ShowDialog2((Avalonia.Controls.Window)currentParameterValues["Window"]);
                    }
                }
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;

            double defaultThickness = (double)parameterValues["Branch thickness:"];
            bool graduallyIncreaseThickness = (bool)parameterValues["Gradually increase thickness"];
            double thicknessMaximum = (double)parameterValues["Thickness maximum:"] * tree.LongestDownstreamLength() * 0.1;

            int style = (int)parameterValues["Style:"];

            double dominanceThreshold = (double)parameterValues["Dominance threshold:"];
            double exclusionThreshold = (double)parameterValues["Exclusion threshold:"];
            double dashUnit = (double)parameterValues["Dash unit:"];

            int shape = (int)parameterValues["Shape:"];

            InstanceStateData stateData = (InstanceStateData)parameterValues["StateData"];

            if (!stateData.Tags.ContainsKey("32858c9d-0247-497f-aeee-03f7bfe24158") || !tree.Attributes.ContainsKey("32858c9d-0247-497f-aeee-03f7bfe24158"))
            {
                throw new Exception("The stochastic mapping samples have not been correctly set up!\nPlease use the \"Set up stochastic map\" module.");
            }

            Assembly assembly = ((CompiledCode)parameterValues["Enabled characters:"]).CompiledAssembly;

            string[][] states = (string[][])stateData.Tags["32858c9d-0247-497f-aeee-03f7bfe24158/states"];

            object[] args = new object[] { states };

            bool[] enabledCharacters = (bool[])ModuleMetadata.GetTypeFromAssembly(assembly, "CustomCode").InvokeMember("GetEnabledCharacters", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);


            ColourFormatterOptions stateColoursCFO = (ColourFormatterOptions)parameterValues["State colours:"];

            Dictionary<string, (double samplePosPerc, double[] stateProbs)[]> preparedStates = (Dictionary<string, (double samplePosPerc, double[] stateProbs)[]>)stateData.Tags["32858c9d-0247-497f-aeee-03f7bfe24158"];

            List<string> allPossibleStates = (from el in GetAllPossibleStates(states, enabledCharacters) select el.Aggregate((a, b) => a + "|" + b)).ToList();

            List<Colour> stateColours = new List<Colour>(allPossibleStates.Count);

            for (int i = 0; i < allPossibleStates.Count; i++)
            {
                stateColours.Add(stateColoursCFO.Formatter(allPossibleStates[i]) ?? stateColoursCFO.DefaultColour);
            }

            Point shift = (Point)parameterValues["Position shift:"];

            Point rootPoint;

            if (!coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out rootPoint))
            {
                rootPoint = coordinates[Modules.RootNodeId];
            }

            static double distance(Point p1, Point p2)
            {
                return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            };

            static Point sumPoint(Point p1, Point p2)
            {
                return new Point(p1.X + p2.X, p1.Y + p2.Y);
            }

            static Point subtractPoint(Point p1, Point p2)
            {
                return new Point(p1.X - p2.X, p1.Y - p2.Y);
            }

            static Point multiplyPoint(Point p1, double scale)
            {
                return new Point(p1.X * scale, p1.Y * scale);
            }

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                bool isCartooned = node.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe");
                bool isCartoonedParent = node.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe") && (node.Parent == null || !node.Parent.Attributes.ContainsKey("0c3400fd-8872-4395-83bc-a5dc5f4967fe"));

                if (node.Parent != null && (!isCartooned || isCartoonedParent))
                {
                    GraphicsPath branchPath = new GraphicsPath();

                    GraphicsPath preBranchPath = null;

                    Point childPoint = coordinates[node.Id];
                    Point parentPoint = coordinates[node.Parent.Id];

                    if (shape == 1)
                    {
                        branchPath = new GraphicsPath().MoveTo(parentPoint).LineTo(childPoint);
                        preBranchPath = null;
                    }
                    else if (shape == 0)
                    {
                        Point anglePoint;

                        Point pA = coordinates[node.Parent.Children[0].Id];
                        Point pB = coordinates[node.Parent.Children[^1].Id];

                        double numerator = pA.Y + pB.Y - 2 * parentPoint.Y;
                        double denominator = pA.X + pB.X - 2 * parentPoint.X;

                        if (Math.Abs(numerator) > 1e-5 && Math.Abs(denominator) > 1e-5)
                        {
                            double m = numerator / denominator;

                            double x = (m * (parentPoint.Y - childPoint.Y + m * childPoint.X) + parentPoint.X) / (m * m + 1);
                            double y = parentPoint.Y - (x - parentPoint.X) / m;

                            anglePoint = new Point(x, y);
                        }
                        else if (Math.Abs(numerator) > 1e-5)
                        {
                            anglePoint = new Point(childPoint.X, parentPoint.Y);
                        }
                        else if (Math.Abs(denominator) > 1e-5)
                        {
                            anglePoint = new Point(parentPoint.X, childPoint.Y);
                        }
                        else
                        {
                            anglePoint = childPoint;
                        }

                        Point intermediatePoint = new Point(anglePoint.X, anglePoint.Y);

                        branchPath = new GraphicsPath().LineTo(intermediatePoint).LineTo(childPoint);

                        preBranchPath = new GraphicsPath().MoveTo(parentPoint).LineTo(intermediatePoint);

                        minX = Math.Min(minX, anglePoint.X);
                        maxX = Math.Max(maxX, anglePoint.X);
                        minY = Math.Min(minY, anglePoint.Y);
                        maxY = Math.Max(maxY, anglePoint.Y);
                    }
                    else if (shape == 2)
                    {
                        double myRadius = distance(childPoint, rootPoint);
                        double parentRadius = distance(parentPoint, rootPoint);

                        Point realElbowPoint = sumPoint(childPoint, multiplyPoint(subtractPoint(rootPoint, childPoint), (myRadius - parentRadius) / myRadius));

                        Point elbowPoint = realElbowPoint;

                        double startAngle = Math.Atan2(parentPoint.Y - rootPoint.Y, parentPoint.X - rootPoint.X);
                        double endAngle = Math.Atan2(elbowPoint.Y - rootPoint.Y, elbowPoint.X - rootPoint.X);

                        if (Math.Abs(startAngle - endAngle) > Math.PI)
                        {
                            endAngle += 2 * Math.PI * Math.Sign(startAngle - endAngle);
                        }

                        branchPath = new GraphicsPath().MoveTo(elbowPoint).LineTo(childPoint);

                        preBranchPath = new GraphicsPath().MoveTo(parentPoint).Arc(rootPoint, parentRadius, startAngle, endAngle);
                    }

                    (double samplePosPerc, double[] stateProbs)[] samples = preparedStates[node.Id];

                    graphics.Save();
                    graphics.Translate(shift);


                    if (style == 0)
                    {
                        (Point pt, Point norm, double[] states, double thickness)[] points = new (Point, Point, double[], double)[samples.Length];

                        Point prevNorm = new Point(0, 1);

                        for (int i = 0; i < samples.Length; i++)
                        {
                            Point pt = branchPath.GetPointAtRelative(samples[i].samplePosPerc);
                            Point norm = branchPath.GetNormalAtRelative(samples[i].samplePosPerc);

                            if (prevNorm.X * norm.X + prevNorm.Y * norm.Y == 0)
                            {
                                prevNorm = norm;
                            }

                            if (prevNorm.X * norm.X + prevNorm.Y * norm.Y < 0)
                            {
                                norm = new Point(-norm.X, -norm.Y);
                            }

                            prevNorm = norm;

                            double thickness = defaultThickness;

                            if (graduallyIncreaseThickness)
                            {
                                thickness = GetThickness(defaultThickness, samples[i].samplePosPerc * node.Length, thicknessMaximum);
                            }

                            points[i] = (pt, norm, GetMarginalProbs(states, enabledCharacters, samples[i].stateProbs), thickness);
                        }

                        for (int i = 0; i < stateColours.Count; i++)
                        {
                            GraphicsPath statePath = new GraphicsPath();

                            for (int j = 0; j < points.Length; j++)
                            {
                                double prevProb = 0;

                                for (int k = 0; k < i; k++)
                                {
                                    prevProb += points[j].states[k];
                                }

                                prevProb -= 0.5;

                                statePath.LineTo(points[j].pt.X + points[j].norm.X * points[j].thickness * prevProb, points[j].pt.Y + points[j].norm.Y * points[j].thickness * prevProb);
                            }

                            for (int j = points.Length - 1; j >= 0; j--)
                            {
                                double prevProb = 0;

                                for (int k = 0; k <= i; k++)
                                {
                                    prevProb += points[j].states[k];
                                }

                                prevProb -= 0.5;

                                statePath.LineTo(points[j].pt.X + points[j].norm.X * points[j].thickness * prevProb, points[j].pt.Y + points[j].norm.Y * points[j].thickness * prevProb);
                            }

                            statePath.Close();

                            graphics.FillPath(statePath, stateColours[i], tag: node.Id);
                        }
                    }
                    else if (style == 1)
                    {
                        List<(double start, double end, int[] states)> segments = new List<(double, double, int[])>();

                        for (int i = 0; i < samples.Length; i++)
                        {
                            double[] probs = GetMarginalProbs(states, enabledCharacters, samples[i].stateProbs);

                            List<int> currStates = new List<int>();
                            for (int j = 0; j < probs.Length; j++)
                            {
                                if (probs[j] > exclusionThreshold)
                                {
                                    currStates.Add(j);
                                }
                            }

                            if (currStates.Count == 0)
                            {
                                for (int j = 0; j < probs.Length; j++)
                                {
                                    currStates.Add(j);
                                }
                            }

                            double currProb = (from el in currStates select probs[el]).Sum();

                            List<int> finalStates = new List<int>();

                            for (int j = 0; j < currStates.Count; j++)
                            {
                                if (probs[currStates[j]] / currProb > dominanceThreshold)
                                {
                                    finalStates.Add(currStates[j]);
                                }
                            }

                            if (finalStates.Count == 0)
                            {
                                finalStates = currStates;
                            }

                            if (i == 0)
                            {
                                segments.Add((samples[i].samplePosPerc, samples[i].samplePosPerc, finalStates.ToArray()));
                            }
                            else
                            {
                                if (segments[segments.Count - 1].states.SequenceEqual(finalStates))
                                {
                                    segments[segments.Count - 1] = (segments[segments.Count - 1].start, samples[i].samplePosPerc, segments[segments.Count - 1].states);
                                }
                                else
                                {
                                    segments.Add((segments[segments.Count - 1].end, samples[i].samplePosPerc, finalStates.ToArray()));
                                }
                            }
                        }

                        for (int i = 0; i < segments.Count; i++)
                        {
                            Point start = branchPath.GetPointAtRelative(segments[i].start);
                            Point end;

                            if (i < segments.Count - 1)
                            {
                                end = branchPath.GetPointAtRelative(segments[i].end + 0.01);
                            }
                            else
                            {
                                end = branchPath.GetPointAtRelative(segments[i].end);
                            }

                            if (segments[i].states.Length == 1)
                            {
                                Colour col = stateColours[segments[i].states[0]];

                                if (i == 0 && preBranchPath != null)
                                {
                                    graphics.StrokePath(preBranchPath.LineTo(end), col, defaultThickness, lineJoin: LineJoins.Round, tag: node.Id);
                                }
                                else
                                {
                                    graphics.StrokePath(new GraphicsPath().MoveTo(start).LineTo(end), col, defaultThickness, tag: node.Id);
                                }
                            }
                            else if (segments[i].states.Length > 1)
                            {
                                for (int j = 0; j < segments[i].states.Length; j++)
                                {
                                    LineDash dash = new LineDash((dashUnit / segments[i].states.Length) * (1 + 0.05), dashUnit / segments[i].states.Length * (segments[i].states.Length - 1 - 0.05), -j * dashUnit / segments[i].states.Length);

                                    Colour col = stateColours[segments[i].states[j]];

                                    if (i == 0 && preBranchPath != null)
                                    {
                                        graphics.StrokePath(preBranchPath.LineTo(end), col, defaultThickness, lineDash: dash, lineJoin: LineJoins.Round, tag: node.Id);
                                    }
                                    else
                                    {
                                        graphics.StrokePath(new GraphicsPath().MoveTo(start).LineTo(end), col, defaultThickness, lineDash: dash, tag: node.Id);
                                    }
                                }
                            }
                        }
                    }
                    else if (style == 2)
                    {
                        List<(double start, double end, int[] states)> segments = new List<(double, double, int[])>();

                        for (int i = 0; i < samples.Length; i++)
                        {
                            double[] probs = GetMarginalProbs(states, enabledCharacters, samples[i].stateProbs);

                            double maxProb = 0;
                            int currState = -1;

                            for (int j = 0; j < probs.Length; j++)
                            {
                                if (probs[j] > maxProb)
                                {
                                    maxProb = probs[j];
                                    currState = j;
                                }
                            }

                            List<int> finalStates = new List<int>();

                            if (currState >= 0)
                            {
                                finalStates.Add(currState);
                            }

                            if (finalStates.Count == 0)
                            {
                                for (int j = 0; j < probs.Length; j++)
                                {
                                    finalStates.Add(j);
                                }
                            }

                            if (i == 0)
                            {
                                segments.Add((samples[i].samplePosPerc, samples[i].samplePosPerc, finalStates.ToArray()));
                            }
                            else
                            {
                                if (segments[segments.Count - 1].states.SequenceEqual(finalStates))
                                {
                                    segments[segments.Count - 1] = (segments[segments.Count - 1].start, samples[i].samplePosPerc, segments[segments.Count - 1].states);
                                }
                                else
                                {
                                    segments.Add((segments[segments.Count - 1].end, samples[i].samplePosPerc, finalStates.ToArray()));
                                }
                            }
                        }

                        for (int i = 0; i < segments.Count; i++)
                        {
                            Point start = branchPath.GetPointAtRelative(segments[i].start);
                            Point end;

                            if (i < segments.Count - 1)
                            {
                                end = branchPath.GetPointAtRelative(segments[i].end + 0.01);
                            }
                            else
                            {
                                end = branchPath.GetPointAtRelative(segments[i].end);
                            }

                            if (segments[i].states.Length == 1)
                            {
                                Colour col = stateColours[segments[i].states[0]];

                                if (i == 0 && preBranchPath != null)
                                {
                                    graphics.StrokePath(preBranchPath.LineTo(end), col, defaultThickness, lineJoin: LineJoins.Round, tag: node.Id);
                                }
                                else
                                {
                                    graphics.StrokePath(new GraphicsPath().MoveTo(start).LineTo(end), col, defaultThickness, tag: node.Id);
                                }
                            }
                            else if (segments[i].states.Length > 1)
                            {
                                for (int j = 0; j < segments[i].states.Length; j++)
                                {
                                    LineDash dash = new LineDash((dashUnit / segments[i].states.Length) * (1 + 0.05), dashUnit / segments[i].states.Length * (segments[i].states.Length - 1 - 0.05), -j * dashUnit / segments[i].states.Length);

                                    Colour col = stateColours[segments[i].states[j]];

                                    if (i == 0 && preBranchPath != null)
                                    {
                                        graphics.StrokePath(preBranchPath.LineTo(end), col, defaultThickness, lineDash: dash, lineJoin: LineJoins.Round, tag: node.Id);
                                    }
                                    else
                                    {
                                        graphics.StrokePath(new GraphicsPath().MoveTo(start).LineTo(end), col, defaultThickness, lineDash: dash, tag: node.Id);
                                    }
                                }
                            }
                        }
                    }

                    graphics.Restore();

                    minX = Math.Min(minX, childPoint.X);
                    maxX = Math.Max(maxX, childPoint.X);
                    minY = Math.Min(minY, childPoint.Y);
                    maxY = Math.Max(maxY, childPoint.Y);

                    minX = Math.Min(minX, parentPoint.X);
                    maxX = Math.Max(maxX, parentPoint.X);
                    minY = Math.Min(minY, parentPoint.Y);
                    maxY = Math.Max(maxY, parentPoint.Y);

                }

            }
            
            minX += shift.X;
            maxX += shift.X;
            minY += shift.Y;
            maxY += shift.Y;
            return new Point[] { new Point(minX - defaultThickness * 2, minY - defaultThickness * 2), new Point(maxX + defaultThickness * 2, maxY + defaultThickness * 2) };

        }

        private static double GetThickness(double defaultThickness, double position, double maxPosition)
        {
            if (position >= maxPosition)
            {
                return defaultThickness;
            }

            double x = position / maxPosition;
            double val = (1 - 1 / (1 + Math.Exp(x * 6))) / 0.9975273768433652 * 2 - 1;

            if (double.IsNaN(val))
            {
                return defaultThickness;
            }

            return Math.Max(Math.Min(1, val), 0) * defaultThickness;
        }

        private static double[] GetMarginalProbs(string[][] states, bool[] enabledCharacters, double[] stateProbs)
        {
            List<List<string>> allStates = GetAllPossibleStates(states);
            List<List<string>> enabledStates = GetAllPossibleStates(states, enabledCharacters);

            double[] tbr = new double[enabledStates.Count];

            for (int i = 0; i < allStates.Count; i++)
            {
                List<string> correspState = GetEnabledStates(allStates[i], enabledCharacters);

                for (int j = 0; j < enabledStates.Count; j++)
                {
                    if (correspState.SequenceEqual(enabledStates[j]))
                    {
                        tbr[j] += stateProbs[i];
                        break;
                    }
                }
            }

            return tbr;
        }

        private static List<List<string>> GetAllPossibleStates(IReadOnlyList<IEnumerable<string>> characterStates)
        {
            List<List<string>> tbr = new List<List<string>>();

            if (characterStates.Count == 0)
            {

            }
            else if (characterStates.Count == 1)
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

        private static List<List<string>> GetAllPossibleStates(IReadOnlyList<IEnumerable<string>> characterStates, bool[] enabledCharacters)
        {
            List<IEnumerable<string>> enabledCharacterStates = new List<IEnumerable<string>>();

            for (int i = 0; i < characterStates.Count; i++)
            {
                if (enabledCharacters[i])
                {
                    enabledCharacterStates.Add(characterStates[i]);
                }
            }

            return GetAllPossibleStates(enabledCharacterStates);
        }

        private static List<T> GetEnabledStates<T>(IReadOnlyList<T> characterStates, bool[] enabledCharacters) where T : IEnumerable<string>
        {
            List<T> enabledCharacterStates = new List<T>();

            for (int i = 0; i < characterStates.Count; i++)
            {
                if (enabledCharacters[i])
                {
                    enabledCharacterStates.Add(characterStates[i]);
                }
            }

            return enabledCharacterStates;
        }

        private static List<string> GetEnabledStates(IReadOnlyList<string> characterStates, bool[] enabledCharacters)
        {
            List<string> enabledCharacterStates = new List<string>();

            for (int i = 0; i < characterStates.Count; i++)
            {
                if (enabledCharacters[i])
                {
                    enabledCharacterStates.Add(characterStates[i]);
                }
            }

            return enabledCharacterStates;
        }

        private static string GetEnabledState(IReadOnlyList<string> characterStates, bool[] enabledCharacters)
        {
            List<string> enabledCharacterStates = new List<string>();

            for (int i = 0; i < characterStates.Count; i++)
            {
                if (enabledCharacters[i])
                {
                    enabledCharacterStates.Add(characterStates[i]);
                }
            }

            return enabledCharacterStates.Aggregate((a, b) => a + "|" + b);
        }

        private static Colour GetColour(int index, int count)
        {
            TreeViewer.Gradient gradient;

            if (count <= 5)
            {
                gradient = Modules.DefaultGradients["WongDiscrete"];
            }
            else
            {
                gradient = Modules.DefaultGradients["WongRainbow"];
            }

            double pos;

            if (count <= 5)
            {
                pos = Math.Round((double)(index + 1) / count * 5) * 0.2;
            }
            else
            {
                pos = (double)(index + 1) / Math.Max(count, 5);
            }

            return gradient.GetColour(pos);
        }

        private static string GetDefaultStateColours(string[][] states)
        {
            List<string> allStates = (from el in GetAllPossibleStates(states) select el.Aggregate((a, b) => a + "|" + b)).ToList();

            System.Text.StringBuilder defaultSourceCode = new System.Text.StringBuilder();
            defaultSourceCode.AppendLine("public static Colour? Format(object attribute)");
            defaultSourceCode.AppendLine("{");
            defaultSourceCode.AppendLine("\tif (attribute is string state)");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\tswitch (state)");
            defaultSourceCode.AppendLine("\t\t{");


            for (int i = 0; i < allStates.Count; i++)
            {
                defaultSourceCode.Append("\t\t\tcase \"");
                defaultSourceCode.Append(allStates[i]);
                defaultSourceCode.Append("\": return Colour.FromRgb(");

                Colour col = GetColour(i, allStates.Count);

                defaultSourceCode.Append(Math.Round(col.R * 255));
                defaultSourceCode.Append(", ");
                defaultSourceCode.Append(Math.Round(col.G * 255));
                defaultSourceCode.Append(", ");
                defaultSourceCode.Append(Math.Round(col.B * 255));

                defaultSourceCode.AppendLine(");");
            }


            defaultSourceCode.AppendLine("\t\t\tdefault:");
            defaultSourceCode.AppendLine("\t\t\t\treturn null;");
            defaultSourceCode.AppendLine("\t\t}");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("\telse");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\treturn null;");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("}");

            return defaultSourceCode.ToString();
        }

        private static string GetDefaultEnabledCharactersCode(string[][] states)
        {
            System.Text.StringBuilder defaultSourceCode = new System.Text.StringBuilder();

            defaultSourceCode.AppendLine("using PhyloTree;");
            defaultSourceCode.AppendLine("using System.Collections.Generic;");
            defaultSourceCode.AppendLine("using VectSharp;");
            defaultSourceCode.AppendLine("using TreeViewer;");
            defaultSourceCode.AppendLine();
            defaultSourceCode.AppendLine("namespace a" + Guid.NewGuid().ToString().Replace("-", ""));
            defaultSourceCode.AppendLine("{");
            defaultSourceCode.AppendLine("\t//Do not change class name");
            defaultSourceCode.AppendLine("\tpublic static class CustomCode");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\t//Do not change method signature");
            defaultSourceCode.AppendLine("\t\tpublic static bool[] GetEnabledCharacters(string[][] allStates)");
            defaultSourceCode.AppendLine("\t\t{");

            defaultSourceCode.Append("\t\t\treturn new bool[] { ");
            for (int i = 0; i < states.Length; i++)
            {
                defaultSourceCode.Append("true");

                if (i < states.Length - 1)
                {
                    defaultSourceCode.Append(", ");
                }
            }
            defaultSourceCode.AppendLine(" };");
            defaultSourceCode.AppendLine("\t\t}");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("}");

            return defaultSourceCode.ToString();
        }

        private static string GetEnabledCharactersCode(bool[] enabledCharacters)
        {
            System.Text.StringBuilder defaultSourceCode = new System.Text.StringBuilder();

            defaultSourceCode.AppendLine("using PhyloTree;");
            defaultSourceCode.AppendLine("using System.Collections.Generic;");
            defaultSourceCode.AppendLine("using VectSharp;");
            defaultSourceCode.AppendLine("using TreeViewer;");
            defaultSourceCode.AppendLine();
            defaultSourceCode.AppendLine("namespace a" + Guid.NewGuid().ToString().Replace("-", ""));
            defaultSourceCode.AppendLine("{");
            defaultSourceCode.AppendLine("\t//Do not change class name");
            defaultSourceCode.AppendLine("\tpublic static class CustomCode");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\t//Do not change method signature");
            defaultSourceCode.AppendLine("\t\tpublic static bool[] GetEnabledCharacters(string[][] allStates)");
            defaultSourceCode.AppendLine("\t\t{");

            defaultSourceCode.Append("\t\t\treturn new bool[] { ");
            for (int i = 0; i < enabledCharacters.Length; i++)
            {
                if (enabledCharacters[i])
                {
                    defaultSourceCode.Append("true");
                }
                else
                {
                    defaultSourceCode.Append("false");
                }

                if (i < enabledCharacters.Length - 1)
                {
                    defaultSourceCode.Append(", ");
                }
            }
            defaultSourceCode.AppendLine(" };");
            defaultSourceCode.AppendLine("\t\t}");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("}");

            return defaultSourceCode.ToString();
        }


        private static async Task ShowWizardEditWindow(Avalonia.Controls.Window parent, InstanceStateData stateData, string[][] states, int moduleIndex, Colour defaultStateColour, Func<object, Colour?> stateColourFormatter)
        {
            ChildWindow window = new ChildWindow() { Icon = parent.Icon, Title = "Select state colours", Width = 300, WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner, FontFamily = parent.FontFamily, FontSize = 14, SizeToContent = Avalonia.Controls.SizeToContent.Height };

            window.Opened += (s, e) =>
            {
                if (window.Bounds.Height > 600)
                {
                    window.Height = 600;
                }
            };

            Avalonia.Controls.Grid mainContainer = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
            window.Content = mainContainer;

            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(1, Avalonia.Controls.GridUnitType.Star));
            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(0, Avalonia.Controls.GridUnitType.Auto));


            Avalonia.Controls.Grid titleGrid = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(10) };
            titleGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            titleGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));

            Avalonia.Controls.Canvas icon = new Avalonia.Controls.Canvas() { Width = 32, Height = 32 };
            icon.Children.Add(new Avalonia.Controls.Shapes.Ellipse() { Width = 32, Height = 32, Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180)) });

            icon.Children.Add(new Avalonia.Controls.Shapes.Path() { Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(74, 125, 177)), Data = Avalonia.Media.Geometry.Parse("M 16,16 L16,0 A16,16 0 0 1 27.3136,27.3136 Z") });
            icon.Children.Add(new Avalonia.Controls.Shapes.Ellipse() { Width = 32, Height = 32, Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(114, 114, 114)), StrokeThickness = 2 });


            titleGrid.Children.Add(icon);

            {
                Avalonia.Controls.TextBlock blk = new Avalonia.Controls.TextBlock() { FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)), Text = "Select state colours", Margin = new Avalonia.Thickness(10, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Avalonia.Controls.Grid.SetColumn(blk, 1);
                titleGrid.Children.Add(blk);
            }

            mainContainer.Children.Add(titleGrid);

            Avalonia.Controls.Grid buttonGrid = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            Avalonia.Controls.Grid.SetRow(buttonGrid, 2);
            mainContainer.Children.Add(buttonGrid);

            Avalonia.Controls.Button okButton = new Avalonia.Controls.Button() { Width = 100, Content = "OK", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
            okButton.Classes.Add("PlainButton");
            Avalonia.Controls.Grid.SetColumn(okButton, 1);
            buttonGrid.Children.Add(okButton);

            Avalonia.Controls.Button cancelButton = new Avalonia.Controls.Button() { Width = 100, Content = "Cancel", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
            cancelButton.Classes.Add("PlainButton");
            Avalonia.Controls.Grid.SetColumn(cancelButton, 3);
            buttonGrid.Children.Add(cancelButton);

            Dictionary<string, Colour> allStateColours = new Dictionary<string, Colour>();

            List<string> allStatesString = (from el in GetAllPossibleStates(states) select el.Aggregate((a, b) => a + "|" + b)).ToList();

            try
            {
                foreach (string sr in allStatesString)
                {
                    allStateColours[sr] = stateColourFormatter(sr) ?? defaultStateColour;
                }
            }
            catch { }

            Avalonia.Controls.ScrollViewer scroller = new Avalonia.Controls.ScrollViewer() { AllowAutoHide = false, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, Margin = new Avalonia.Thickness(10, 0, 10, 0), Padding = new Avalonia.Thickness(0, 0, 16, 0) };
            Avalonia.Controls.Grid.SetRow(scroller, 1);
            mainContainer.Children.Add(scroller);

            Avalonia.Controls.StackPanel itemsContainer = new Avalonia.Controls.StackPanel();
            scroller.Content = itemsContainer;

            Avalonia.Styling.Style normalStyle = new Style(x => x.OfType<Avalonia.Controls.Grid>().Class("ItemBackground"));
            normalStyle.Setters.Add(new Setter(Avalonia.Controls.Grid.BackgroundProperty, Avalonia.Media.Brushes.Transparent));
            itemsContainer.Styles.Add(normalStyle);

            Avalonia.Styling.Style hoverStyle = new Style(x => x.OfType<Avalonia.Controls.Grid>().Class("ItemBackground").Class(":pointerover"));
            hoverStyle.Setters.Add(new Setter(Avalonia.Controls.Grid.BackgroundProperty, new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(220, 220, 220))));
            itemsContainer.Styles.Add(hoverStyle);

            List<Avalonia.Controls.Grid> itemList = new List<Avalonia.Controls.Grid>();

            List<Avalonia.Controls.Control> stateLabels = GetStateLabels(states);

            foreach (KeyValuePair<string, Colour> stateColour in allStateColours)
            {
                Avalonia.Controls.Grid grd = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                grd.Classes.Add("ItemBackground");
                grd.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
                grd.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(100, Avalonia.Controls.GridUnitType.Pixel));

                string key = stateColour.Key;
                Colour col = stateColour.Value;

                int index = allStatesString.IndexOf(key);

                grd.Children.Add(/*new FillingControl<TrimmedTextBox2>(new TrimmedTextBox2()
                {
                    Text = key,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
                    FontSize = 14
                }, 5)*/stateLabels[index]);

                ColorButton colorButton = new ColorButton() { Color = col.ToAvalonia(), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, RenderTransform = null };
                Avalonia.Controls.Grid.SetColumn(colorButton, 1);
                grd.Children.Add(colorButton);

                colorButton.PropertyChanged += (s, e) =>
                {
                    if (e.Property == ColorButton.ColorProperty)
                    {
                        allStateColours[key] = colorButton.Color.ToVectSharp();
                    }
                };

                colorButton.Classes.Add("PlainButton");

                itemList.Add(grd);
                itemsContainer.Children.Add(grd);
            }

            bool result = false;

            okButton.Click += (s, e) =>
            {
                result = true;
                window.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                window.Close();
            };

            await window.ShowDialog2(parent);

            if (result)
            {
                try
                {
                    string code = @"public static Colour? Format(object attribute)
{
    if (attribute is string state)
    {
        switch (state)
        {
";
                    foreach (KeyValuePair<string, Colour> stateColour in allStateColours)
                    {
                        code += "            case " + System.Text.Json.JsonSerializer.Serialize(stateColour.Key) + ": return Colour.FromRgba(" + stateColour.Value.R.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " + stateColour.Value.G.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " + stateColour.Value.B.ToString(System.Globalization.CultureInfo.InvariantCulture) + ", " + stateColour.Value.A.ToString(System.Globalization.CultureInfo.InvariantCulture) + ");\n";
                    }

                    code += @"            default:
                return null;
        }
    }
    else
    {
        return null;
    }
}";

                    object[] formatterParams = new object[2] { code, false };

                    ColourFormatterOptions cfo = new ColourFormatterOptions(code, formatterParams) { AttributeName = "(N/A)", AttributeType = "String", DefaultColour = Colour.FromRgb(220, 220, 220) };

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { stateData.PlottingModulesParameterUpdater(moduleIndex)(new Dictionary<string, object>() { { "State colours:", cfo } }); });

                    typeof(MainWindow).InvokeMember("UpdatePlotLayer", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, parent, new object[] { moduleIndex, true });
                }
                catch (Exception ex)
                {
                    MessageBox box = new MessageBox("Attention!", "An error occurred while generating the code!\n" + ex.Message);
                    await box.ShowDialog2(parent);
                }
            }
        }

        private static async Task ShowWizardEditEnabledCharactersWindow(Avalonia.Controls.Window parent, InstanceStateData stateData, string[][] states, bool[] enabledCharacters, int moduleIndex)
        {
            ChildWindow window = new ChildWindow() { Icon = parent.Icon, Title = "Select enabled characters", Width = 320, WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner, FontFamily = parent.FontFamily, FontSize = 14, SizeToContent = Avalonia.Controls.SizeToContent.Height };

            window.Opened += (s, e) =>
            {
                if (window.Bounds.Height > 600)
                {
                    window.Height = 600;
                }
            };

            Avalonia.Controls.Grid mainContainer = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 10) };
            window.Content = mainContainer;

            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(1, Avalonia.Controls.GridUnitType.Star));
            mainContainer.RowDefinitions.Add(new Avalonia.Controls.RowDefinition(0, Avalonia.Controls.GridUnitType.Auto));


            Avalonia.Controls.Grid titleGrid = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(10) };
            titleGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            titleGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));

            Avalonia.Controls.Canvas icon = new Avalonia.Controls.Canvas() { Width = 32, Height = 32 };
            icon.Children.Add(new Avalonia.Controls.Shapes.Ellipse() { Width = 32, Height = 32, Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180)) });

            icon.Children.Add(new Avalonia.Controls.Shapes.Path() { Fill = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(74, 125, 177)), Data = Avalonia.Media.Geometry.Parse("M 16,16 L16,0 A16,16 0 0 1 27.3136,27.3136 Z") });
            icon.Children.Add(new Avalonia.Controls.Shapes.Ellipse() { Width = 32, Height = 32, Stroke = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(114, 114, 114)), StrokeThickness = 2 });


            titleGrid.Children.Add(icon);

            {
                Avalonia.Controls.TextBlock blk = new Avalonia.Controls.TextBlock() { FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)), Text = "Select enabled characters", Margin = new Avalonia.Thickness(10, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Avalonia.Controls.Grid.SetColumn(blk, 1);
                titleGrid.Children.Add(blk);
            }

            mainContainer.Children.Add(titleGrid);

            Avalonia.Controls.Grid buttonGrid = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 10, 0, 0) };
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
            buttonGrid.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));
            Avalonia.Controls.Grid.SetRow(buttonGrid, 2);
            mainContainer.Children.Add(buttonGrid);

            Avalonia.Controls.Button okButton = new Avalonia.Controls.Button() { Width = 100, Content = "OK", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
            okButton.Classes.Add("PlainButton");
            Avalonia.Controls.Grid.SetColumn(okButton, 1);
            buttonGrid.Children.Add(okButton);

            Avalonia.Controls.Button cancelButton = new Avalonia.Controls.Button() { Width = 100, Content = "Cancel", HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13 };
            cancelButton.Classes.Add("PlainButton");
            Avalonia.Controls.Grid.SetColumn(cancelButton, 3);
            buttonGrid.Children.Add(cancelButton);

            List<string> allStatesString = (from el in GetAllPossibleStates(states) select el.Aggregate((a, b) => a + "|" + b)).ToList();

            Avalonia.Controls.ScrollViewer scroller = new Avalonia.Controls.ScrollViewer() { AllowAutoHide = false, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, Margin = new Avalonia.Thickness(10, 0, 10, 0), Padding = new Avalonia.Thickness(0, 0, 16, 0) };
            Avalonia.Controls.Grid.SetRow(scroller, 1);
            mainContainer.Children.Add(scroller);

            Avalonia.Controls.StackPanel itemsContainer = new Avalonia.Controls.StackPanel();
            scroller.Content = itemsContainer;

            Avalonia.Styling.Style normalStyle = new Style(x => x.OfType<Avalonia.Controls.Grid>().Class("ItemBackground"));
            normalStyle.Setters.Add(new Setter(Avalonia.Controls.Grid.BackgroundProperty, Avalonia.Media.Brushes.Transparent));
            itemsContainer.Styles.Add(normalStyle);

            Avalonia.Styling.Style hoverStyle = new Style(x => x.OfType<Avalonia.Controls.Grid>().Class("ItemBackground").Class(":pointerover"));
            hoverStyle.Setters.Add(new Setter(Avalonia.Controls.Grid.BackgroundProperty, new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(220, 220, 220))));
            itemsContainer.Styles.Add(hoverStyle);

            List<Avalonia.Controls.Grid> itemList = new List<Avalonia.Controls.Grid>();

            List<Avalonia.Controls.Control> characterLabels = GetCharacterLabels(states);

            for (int i = 0; i < states.Length; i++)
            {
                Avalonia.Controls.Grid grd = new Avalonia.Controls.Grid() { Margin = new Avalonia.Thickness(0, 0, 0, 5) };
                grd.Classes.Add("ItemBackground");
                grd.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(0, Avalonia.Controls.GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new Avalonia.Controls.ColumnDefinition(1, Avalonia.Controls.GridUnitType.Star));

                int index = i;

                Avalonia.Controls.Grid.SetColumn(characterLabels[index], 1);
                grd.Children.Add(characterLabels[index]);

                Avalonia.Controls.CheckBox box = new Avalonia.Controls.CheckBox() { Content = i.ToString(), FontSize = 14, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, IsChecked = enabledCharacters[i], Margin = new Avalonia.Thickness(10, 0, 20, 0) };

                grd.Children.Add(box);

                box.PropertyChanged += (s, e) =>
                {
                    if (e.Property == Avalonia.Controls.CheckBox.IsCheckedProperty)
                    {
                        enabledCharacters[index] = box.IsChecked == true;
                    }
                };

                itemList.Add(grd);
                itemsContainer.Children.Add(grd);
            }

            bool result = false;

            okButton.Click += (s, e) =>
            {
                result = true;
                window.Close();
            };

            cancelButton.Click += (s, e) =>
            {
                window.Close();
            };

            await window.ShowDialog2(parent);

            if (result)
            {
                try
                {
                    string[][] enabledCharacterStates = GetEnabledStates(states, enabledCharacters).ToArray();

                    string code = GetDefaultStateColours(enabledCharacterStates);

                    object[] formatterParams = new object[2] { code, false };

                    ColourFormatterOptions cfo = new ColourFormatterOptions(code, formatterParams) { AttributeName = "(N/A)", AttributeType = "String", DefaultColour = Colour.FromRgb(220, 220, 220) };

                    await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => { stateData.PlottingModulesParameterUpdater(moduleIndex)(new Dictionary<string, object>() { { "Enabled characters:", new CompiledCode(GetEnabledCharactersCode(enabledCharacters)) }, { "State colours:", cfo } }); });

                    typeof(MainWindow).InvokeMember("UpdatePlotLayer", System.Reflection.BindingFlags.InvokeMethod | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, parent, new object[] { moduleIndex, true });
                }
                catch (Exception ex)
                {
                    MessageBox box = new MessageBox("Attention!", "An error occurred while generating the code!\n" + ex.Message);
                    await box.ShowDialog2(parent);
                }
            }
        }



        private static Colour BlendWithWhite(Colour col, double intensity)
        {
            return Colour.FromRgb(col.R * intensity + 1 - intensity, col.G * intensity + 1 - intensity, col.B * intensity + 1 - intensity);
        }

        private static List<Avalonia.Controls.Control> GetStateLabels(string[][] states)
        {
            List<List<string>> allPossibleStates = GetAllPossibleStates(states);

            List<Avalonia.Controls.Control> tbr = new List<Avalonia.Controls.Control>();

            for (int i = 0; i < allPossibleStates.Count; i++)
            {
                Avalonia.Controls.StackPanel pnl = new Avalonia.Controls.StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                for (int j = 0; j < allPossibleStates[i].Count; j++)
                {
                    Avalonia.Controls.Grid grd = new Avalonia.Controls.Grid() { Background = new Avalonia.Media.SolidColorBrush(BlendWithWhite(GetColour(Array.IndexOf(states[j], allPossibleStates[i][j]), states[j].Length), 0.25).ToAvalonia()) };

                    if (j > 0)
                    {
                        grd.Margin = new Avalonia.Thickness(2, 0, 0, 0);
                    }

                    grd.Children.Add(new Avalonia.Controls.TextBlock() { FontSize = 14, Text = allPossibleStates[i][j], Margin = new Avalonia.Thickness(5, 0), FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono") });

                    pnl.Children.Add(grd);
                }

                tbr.Add(pnl);
            }

            return tbr;

        }

        private static List<Avalonia.Controls.Control> GetCharacterLabels(string[][] states)
        {
            List<Avalonia.Controls.Control> tbr = new List<Avalonia.Controls.Control>();

            for (int i = 0; i < states.Length; i++)
            {
                Avalonia.Controls.StackPanel pnl = new Avalonia.Controls.StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

                for (int j = 0; j < states[i].Length; j++)
                {
                    Avalonia.Controls.Grid grd = new Avalonia.Controls.Grid() { Background = new Avalonia.Media.SolidColorBrush(BlendWithWhite(GetColour(Array.IndexOf(states[j], states[i][j]), states[j].Length), 0.25).ToAvalonia()) };

                    if (j > 0)
                    {
                        grd.Margin = new Avalonia.Thickness(2, 0, 0, 0);
                    }

                    grd.Children.Add(new Avalonia.Controls.TextBlock() { FontSize = 14, Text = states[i][j], Margin = new Avalonia.Thickness(5, 0), FontFamily = new Avalonia.Media.FontFamily("resm:TreeViewer.Fonts.?assembly=TreeViewer#Roboto Mono") });

                    pnl.Children.Add(grd);
                }

                tbr.Add(pnl);
            }

            return tbr;

        }
    }
}
