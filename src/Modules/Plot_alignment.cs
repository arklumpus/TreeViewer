using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Linq;

namespace aea7e246be93f4d0da67a88af05479b48
{
    /// <summary>
    /// This module reads an alignment file in FASTA format from an attachment and adds the alignment to the tree plot.
    /// Clicking on a sequence in the alignment selects the corresponding tip in the tree and vice versa.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Plot alignment";
        public const string HelpText = "Adds the plot of an alignment to the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "ea7e246b-e93f-4d0d-a67a-88af05479b48";

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            List<string> leafNames = tree.GetLeafNames();

            return new List<(string, string)>()
            {
                ("Sequence alignment", "Group:2"),
                
                /// <param name="FASTA alignment:">
                /// This parameter is used to select the attachment containing the alignment file. The alignment must be
                /// in FASTA format (see e.g. [FASTA format on Wikipedia](https://en.wikipedia.org/wiki/FASTA_format)).
                /// </param>
                ("FASTA alignment:", "Attachment:"),

                ("Sequence range","Group:2"),
                
                /// <param name="Start:">
                /// This parameter determines the first nucleotide of the alignment that is shown.
                /// </param>
                ("Start:", "NumericUpDown:1[\"1\",\"Infinity\",\"1\",\"0\"]"),
                
                /// <param name="End:">
                /// This parameter determines the last nucleotide of the alignment that is shown.
                /// </param>
                ("End:", "NumericUpDown:1[\"1\",\"Infinity\",\"1\",\"0\"]"),

                ("Position","Group:4"),
                
                /// <param name="Anchor:">
                /// This parameter is used to select the anchor used to determine the position of the alignment plot. If
                /// the selected value is `Node`, the specified node is used as an anchor. Otherwise, the selected point
                /// on the tree plot is used. Note that these positions refer to the _tree_ plot and do not take into
                /// account the presence of labels and other elements.
                /// </param>
                ("Anchor:","ComboBox:8[\"Node\",\"Top-left\",\"Top-center\",\"Top-right\",\"Middle-left\",\"Middle-center\",\"Middle-right\",\"Bottom-left\",\"Bottom-center\",\"Bottom-right\"]"),
                
                /// <param name="Node:">
                /// If the [Anchor](#anchor) was set to `Node`, this control is used to select the node that acts as an
                /// anchor.
                /// </param>
                ("Node:","Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]"),
                
                /// <param name="Alignment:">
                /// This parameter controls to which point on the alignment plot the selected [Anchor](#anchor) corresponds.
                /// </param>
                ("Alignment:","ComboBox:1[\"Top-left\",\"Top-center\",\"Top-right\",\"Middle-left\",\"Middle-center\",\"Middle-right\",\"Bottom-left\",\"Bottom-center\",\"Bottom-right\"]"),
                
                /// <param name="Position:">
                /// This parameter determines how much the alignment plot is shifted with respect to the position determined
                /// by the [Anchor](#anchor) and the [Alignment](#alignment).
                /// </param>
                ("Position:","Point:[0,0]"),

                ("Labels","Group:5"),
                
                /// <param name="Label position:">
                /// This parameter determines the position of the labels for the sequences in the alignment (if any).
                /// </param>
                ("Label position:", "ComboBox:1[\"Neither\",\"Left\",\"Right\",\"Both\"]"),
                
                /// <param name="Label font:">
                /// This parameter determines the font used to draw the labels for the sequences in the alignment.
                /// </param>
                ("Label font:", "Font:[\"Helvetica-Oblique\",\"6\"]"),

                ("Attribute","Group:3"),
                
                /// <param name="Label attribute:">
                /// This parameter specifies the attribute used to determine the text of the labels. By default the `Name`
                /// of each node is drawn.
                /// </param>
                ("Label attribute:", "AttributeSelector:Name"),
                
                /// <param name="Attribute type:">
                /// This parameter specifies the type of the attribute used to determine the text of the labels. By default
                /// this is `String`. If the type chosen here does not correspond to the actual type of the attribute (e.g. 
                /// `Number` is chosen for the `Name` attribute, or `String` is chosen for the `Length` attribute), no label
                /// is drawn. If the attribute has values with different types for different leaves, the label is only shown
                /// for leaves whose attribute type corresponds to the one chosen here.
                /// </param>
                ("Attribute type:", "AttributeType:String"),
                
                /// <param name="Attribute format...">
                /// This parameter determines how the value of the selected attribute is used to determine the text of the
                /// label. By default, if the [Attribute type](#attribute-type) is `String` the text of the label corresponds
                /// to the value of the attribute, while if the [Attribute type](#attribute-type) is `Number` the text of the
                /// label corresponds to the number rounded to 2 significant digits.
                /// </param>
                ("Attribute format...", "Formatter:[\"Attribute type:\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConverters[0]) + ",\"true\"]"),

                /// <param name="Identity label:">
                /// This parameter determines the label used for the line showing the % identity at each residue in the alignment.
                /// </param>
                ("Identity label:", "TextBox:Identity"),
                
                /// <param name="Gaps label:">
                /// This parameter determines the label used for the line showing the % of gaps at each residue in the alignment.
                /// </param>
                ("Gaps label:", "TextBox:Gaps"),

                ("Size","Group:3"),
                
                /// <param name="Residue width:">
                /// This parameter determines the width of each residue in the alignment.
                /// </param>
                ("Residue width:", "NumericUpDown:0.25[\"0\",\"Infinity\",\"0.05\",\"0.####\"]"),
                
                /// <param name="Sequence height:">
                /// This parameter determines the height of each sequence in the alignment. The value can be changed on a per-node
                /// basis, but the lines showing the % identity and % gaps will always use the default value defined here.
                /// </param>
                ("Sequence height:", "NumericUpDownByNode:5[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"SequenceHeight\",\"Number\",\"true\"]"),
                
                /// <param name="Margin:">
                /// This parameter determines the space between a sequence and the next in the alignment. The value can be changed
                /// on a per-node basis, but the lines showing the % identity and % gaps will always use the default value defined
                /// here.
                /// </param>
                ("Margin:", "NumericUpDownByNode:2[\"0\",\"Infinity\"," + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToDouble[1]) + ",\"SequenceMargin\",\"Number\",\"true\"]"),

                ("Colours","Group:8"),
                
                /// <param name="Colour mode:">
                /// This parameter determines how each sequence in the alignment is coloured. If the selected value is `By sequence`,
                /// each sequence is coloured using a single colour. If the selected value is `By residue`, each residue in the sequence
                /// is coloured with a different colour. Note that colouring each residue with a different colour will likely cause
                /// reduced performance and is not recommended for alignment files containing many sequences or long sequences.
                /// </param>
                ("Colour mode:","ComboBox:0[\"By sequence\",\"By residue\"]"),
                
                /// <param name="Auto colour by node">
                /// If this check box is checked, the colour of each sequence is determined algorithmically in a pseudo-random way designed
                /// to achieve an aestethically pleasing distribution of colours, while being reproducible if the same tree is rendered
                /// multiple times.
                /// </param>
                ( "Auto colour by node", "CheckBox:false" ),

                /// <param name="Opacity:">
                /// This parameter determines the opacity of the colour used if the [Auto colour by node](#auto-colour-by-node) option is
                /// enabled.
                /// </param>
                ( "Opacity:", "Slider:1[\"0\",\"1\",\"{0:P0}\"]"),
                
                /// <param name="Colour:">
                /// If the selected [Colour mode](#colour-mode) is `By sequence`, this parameter determines the colour of each sequence;
                /// otherwise, it only controls the colour of the label for each sequence.
                /// </param>
                ("Colour:","ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(Modules.DefaultAttributeConvertersToColour[0])+ ",\"SequenceColour\",\"String\",\"0\",\"114\",\"178\",\"255\",\"true\"]"),
                
                /// <param name="Residue colours:">
                /// If the selected [Colour mode](#colour-mode) is `By residue`, this parameter determines the colour used for each residue.
                /// While this uses a "Colour by node" control, the colours are actually determined based on the residues, rather than on
                /// attributes of the tree. The colours associated with each residue can be changed (or additional residues can be added)
                /// by modifying the formatter code for this parameter.
                /// </param>
                ("Residue colours:", "ColourByNode:[" + System.Text.Json.JsonSerializer.Serialize(DefaultNucleotideColourSource) + ",\"(N/A)\",\"String\",\"255\",\"255\",\"255\",\"0\",\"false\"]"),
                
                /// <param name="ResidueStyleButtons" display="Residue style buttons">
                /// These buttons can be used to reset the default [Residue colours](#residue-colours) for nucleotide sequences or protein sequences.
                /// </param>
                ("ResidueStyleButtons","Buttons:[\"DNA/RNA\",\"Protein\"]"),
                
                /// <param name="Identity colour:">
                /// This parameter is used to determine the colour to use when drawing the line with the % identity.
                /// </param>
                ("Identity colour:", "Colour:[0, 158, 115, 255]"),
                
                /// <param name="Gaps colour:">
                /// This parameter is used to determine the colour to use when drawing the line with the % of gaps.
                /// </param>
                ("Gaps colour:", "Colour:[204, 121, 167, 255]"),

                ("Residue letters", "Group:3"),
                
                /// <param name="Draw residue letters">
                /// If this check box is checked, the letters corresponding to the sequences are also drawn with the alignment. Note that
                /// this will likely cause reduced performance and is not recommended for alignment files containing many sequences or
                /// long sequences. If necessary, the letters will be compressed so that they do not overflow the alignment.
                /// </param>
                ("Draw residue letters","CheckBox:false"),
                
                /// <param name="Residue font:">
                /// The font used to draw the letters in the alignment.
                /// </param>
                ("Residue font:", "Font:[\"Courier-Bold\",\"6\"]"),
                
                /// <param name="Letter colour:">
                /// The colour used to draw the letters in the alignment.
                /// </param>
                ("Letter colour:", "Colour:[0, 0, 0, 255]")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "ResidueStyleButtons", -1 } };

            if ((int)currentParameterValues["Label position:"] == 0)
            {
                controlStatus["Label font:"] = ControlStatus.Hidden;
                controlStatus["Attribute"] = ControlStatus.Hidden;
                controlStatus["Label attribute:"] = ControlStatus.Hidden;
                controlStatus["Attribute type:"] = ControlStatus.Hidden;
                controlStatus["Attribute format..."] = ControlStatus.Hidden;
                controlStatus["Identity label:"] = ControlStatus.Hidden;
                controlStatus["Gaps label:"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Label font:"] = ControlStatus.Enabled;
                controlStatus["Attribute"] = ControlStatus.Enabled;
                controlStatus["Label attribute:"] = ControlStatus.Enabled;
                controlStatus["Attribute type:"] = ControlStatus.Enabled;
                controlStatus["Attribute format..."] = ControlStatus.Enabled;
                controlStatus["Identity label:"] = ControlStatus.Enabled;
                controlStatus["Gaps label:"] = ControlStatus.Enabled;
            }

            if ((int)currentParameterValues["Colour mode:"] == 0)
            {
                controlStatus["Residue colours:"] = ControlStatus.Hidden;
                controlStatus["ResidueStyleButtons"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Residue colours:"] = ControlStatus.Enabled;
                controlStatus["ResidueStyleButtons"] = ControlStatus.Enabled;
            }

            if ((int)currentParameterValues["Anchor:"] == 0)
            {
                controlStatus["Node:"] = ControlStatus.Enabled;
            }
            else
            {
                controlStatus["Node:"] = ControlStatus.Hidden;
            }

            if ((bool)currentParameterValues["Draw residue letters"])
            {
                controlStatus["Residue font:"] = ControlStatus.Enabled;
                controlStatus["Letter colour:"] = ControlStatus.Enabled;
            }
            else
            {
                controlStatus["Residue font:"] = ControlStatus.Hidden;
                controlStatus["Letter colour:"] = ControlStatus.Hidden;
            }

            if ((bool)currentParameterValues["Auto colour by node"])
            {
                controlStatus["Opacity:"] = ControlStatus.Enabled;
                controlStatus["Colour:"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Opacity:"] = ControlStatus.Hidden;
                controlStatus["Colour:"] = ControlStatus.Enabled;
            }


            if ((string)previousParameterValues["Label attribute:"] != (string)currentParameterValues["Label attribute:"])
            {
                string attributeName = (string)currentParameterValues["Label attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Attribute type:"] == (string)currentParameterValues["Attribute type:"])
                {
                    parametersToChange.Add("Attribute type:", attrType);

                    if (attrType == "String")
                    {
                        parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[0]) { Parameters = new object[] { Modules.DefaultAttributeConverters[0], true } });
                    }
                    else if (attrType == "Number")
                    {
                        parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } });
                    }
                }
            }

            if ((string)previousParameterValues["Attribute type:"] != (string)currentParameterValues["Attribute type:"])
            {
                string attrType = (string)currentParameterValues["Attribute type:"];
                if (attrType == "String")
                {
                    parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[0]) { Parameters = new object[] { Modules.DefaultAttributeConverters[0], true } });
                }
                else if (attrType == "Number")
                {
                    parametersToChange.Add("Attribute format...", new FormatterOptions(Modules.DefaultAttributeConverters[1]) { Parameters = new object[] { 0, 2.0, 0.0, 0.0, false, true, Modules.DefaultAttributeConverters[1], true } });
                }
            }

            if ((int)currentParameterValues["ResidueStyleButtons"] == 0)
            {
                ColourFormatterOptions formatterOptions = new ColourFormatterOptions(DefaultNucleotideColourSource);

                formatterOptions.AttributeName = "(N/A)";
                formatterOptions.AttributeType = "String";
                formatterOptions.DefaultColour = ((ColourFormatterOptions)currentParameterValues["Residue colours:"]).DefaultColour;
                formatterOptions.Parameters = new object[] { DefaultNucleotideColourSource, false };

                parametersToChange["Residue colours:"] = formatterOptions;
            }
            else if ((int)currentParameterValues["ResidueStyleButtons"] == 1)
            {
                ColourFormatterOptions formatterOptions = new ColourFormatterOptions(DefaultAminoAcidColourSource);

                formatterOptions.AttributeName = "(N/A)";
                formatterOptions.AttributeType = "String";
                formatterOptions.DefaultColour = ((ColourFormatterOptions)currentParameterValues["Residue colours:"]).DefaultColour;
                formatterOptions.Parameters = new object[] { DefaultAminoAcidColourSource, false };

                parametersToChange["Residue colours:"] = formatterOptions;
            }

            if ((Attachment)currentParameterValues["FASTA alignment:"] != (Attachment)previousParameterValues["FASTA alignment:"])
            {
                Attachment attachment = (Attachment)currentParameterValues["FASTA alignment:"];

                int maxLen = (from el in ReadFasta(attachment) select el.Value.Length).Max();

                parametersToChange["Start:"] = 1.0;
                parametersToChange["End:"] = (double)maxLen;
            }

            return true;
        }

        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            Attachment attachment = (Attachment)parameterValues["FASTA alignment:"];
            Font labelFont = (Font)parameterValues["Label font:"];
            string gapLabel = (string)parameterValues["Gaps label:"];
            string identityLabel = (string)parameterValues["Identity label:"];

            NumberFormatterOptions heightFO = (NumberFormatterOptions)parameterValues["Sequence height:"];
            NumberFormatterOptions marginFO = (NumberFormatterOptions)parameterValues["Margin:"];

            double defaultHeight = heightFO.DefaultValue;
            double defaultMargin = marginFO.DefaultValue;

            Func<object, double?> heightFormatter = heightFO.Formatter;
            Func<object, double?> marginFormatter = marginFO.Formatter;

            Func<object, string> labelFormatter = ((FormatterOptions)parameterValues["Attribute format..."]).Formatter;
            string labelAttributeName = (string)parameterValues["Label attribute:"];

            ColourFormatterOptions colourFO = (ColourFormatterOptions)parameterValues["Colour:"];
            Colour defaultColour = colourFO.DefaultColour;
            Func<object, Colour?> colourFormatter = colourFO.Formatter;

            bool autoColour = (bool)parameterValues["Auto colour by node"];
            double opacity = (double)parameterValues["Opacity:"];

            ColourFormatterOptions residueFO = (ColourFormatterOptions)parameterValues["Residue colours:"];
            Colour defaultResidueColour = residueFO.DefaultColour;
            Func<object, Colour?> residueFormatter = residueFO.Formatter;

            int colourMode = (int)parameterValues["Colour mode:"];

            double width = (double)parameterValues["Residue width:"];
            Colour identityColour = (Colour)parameterValues["Identity colour:"];
            Colour gapColour = (Colour)parameterValues["Gaps colour:"];

            int labelPosition = (int)parameterValues["Label position:"];

            bool drawLetters = (bool)parameterValues["Draw residue letters"];
            Font residueFont = (Font)parameterValues["Residue font:"];
            Colour residueLetterColour = (Colour)parameterValues["Letter colour:"];

            TreeNode anchorNode = (tree.GetLastCommonAncestor((string[])parameterValues["Node:"]) ?? tree);

            int anchorType = (int)parameterValues["Anchor:"];
            int alignment = (int)parameterValues["Alignment:"];

            double plotMaxX = double.MinValue;
            double plotMinX = double.MaxValue;
            double plotMaxY = double.MinValue;
            double plotMinY = double.MaxValue;

            foreach (KeyValuePair<string, Point> kvp in coordinates)
            {
                if (tree.GetNodeFromId(kvp.Key) != null)
                {
                    plotMaxX = Math.Max(plotMaxX, kvp.Value.X);
                    plotMaxY = Math.Max(plotMaxY, kvp.Value.Y);
                    plotMinX = Math.Min(plotMinX, kvp.Value.X);
                    plotMinY = Math.Min(plotMinY, kvp.Value.Y);
                }
            }

            double anchorX = coordinates[Modules.RootNodeId].X;
            double anchorY = coordinates[Modules.RootNodeId].Y;

            if (anchorType == 0)
            {
                Point anchor = coordinates[anchorNode.Id];
                anchorX = anchor.X;
                anchorY = anchor.Y;
            }
            else
            {
                if (anchorType == 1 || anchorType == 2 || anchorType == 3)
                {
                    anchorY = plotMinY;
                }
                else if (anchorType == 4 || anchorType == 5 || anchorType == 6)
                {
                    anchorY = (plotMinY + plotMaxY) * 0.5;
                }
                else if (anchorType == 7 || anchorType == 8 || anchorType == 9)
                {
                    anchorY = plotMaxY;
                }

                if (anchorType == 1 || anchorType == 4 || anchorType == 7)
                {
                    anchorX = plotMinX;
                }
                else if (anchorType == 2 || anchorType == 5 || anchorType == 8)
                {
                    anchorX = (plotMinX + plotMaxX) * 0.5;
                }
                else if (anchorType == 3 || anchorType == 6 || anchorType == 9)
                {
                    anchorX = plotMaxX;
                }
            }

            Point position = (Point)parameterValues["Position:"];

            if (attachment == null)
            {
                throw new Exception("No attachment selected!");
            }

            Dictionary<string, string> fastaAlignment = ReadFasta(attachment);

            int start = (int)Math.Round((double)parameterValues["Start:"]) - 1;
            int end = (int)Math.Round((double)parameterValues["End:"]) - 1;

            end = Math.Min(end, (from el in fastaAlignment select el.Value.Length).Max());
            start = Math.Min(start, end);

            if (end - start <= 0)
            {
                return new Point[] { new Point(), new Point() };
            }
            else
            {
                foreach (KeyValuePair<string, string> kvp in fastaAlignment)
                {
                    string newSeq = kvp.Value.Substring(Math.Min(start, kvp.Value.Length), Math.Min(end - start, kvp.Value.Length - Math.Min(start, kvp.Value.Length)));
                    fastaAlignment[kvp.Key] = newSeq;
                }
            }



            Graphics newGpr = new Graphics();

            double maxLabelWidth = 0;
            double maxY = 0;

            foreach (TreeNode leaf in tree.GetLeaves())
            {
                foreach (KeyValuePair<string, string> kvp in fastaAlignment)
                {
                    if (kvp.Key.Contains(leaf.Name))
                    {
                        double height = defaultHeight;
                        double margin = defaultMargin;

                        if (leaf.Attributes.TryGetValue(heightFO.AttributeName, out object heightAttributeObject) && heightAttributeObject != null)
                        {
                            height = heightFormatter(heightAttributeObject) ?? defaultHeight;
                        }

                        if (leaf.Attributes.TryGetValue(marginFO.AttributeName, out object marginAttributeObject) && marginAttributeObject != null)
                        {
                            margin = marginFormatter(marginAttributeObject) ?? defaultMargin;
                        }

                        Colour sequenceColour = defaultColour;


                        if (!autoColour)
                        {
                            if (leaf.Attributes.TryGetValue(colourFO.AttributeName, out object colourAttributeObject) && colourAttributeObject != null)
                            {
                                sequenceColour = colourFormatter(colourAttributeObject) ?? defaultColour;
                            }
                        }
                        else
                        {
                            sequenceColour = Modules.DefaultColours[Math.Abs(leaf.Name.GetHashCode()) % Modules.DefaultColours.Length].WithAlpha(opacity);
                        }

                        if (height > 0 && (sequenceColour.A > 0 || colourMode == 1))
                        {
                            string labelValue = "";

                            if (leaf.Attributes.TryGetValue(labelAttributeName, out object attributeObject) && attributeObject != null)
                            {
                                labelValue = labelFormatter(attributeObject);
                            }

                            if (!string.IsNullOrEmpty(labelValue))
                            {
                                double labelWidth = labelFont.MeasureText(labelValue).Width;

                                maxLabelWidth = Math.Max(labelWidth, maxLabelWidth);

                                if (labelPosition == 1 || labelPosition == 3)
                                {
                                    newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, height * 0.5, labelValue, labelFont, sequenceColour, TextBaselines.Middle, tag: leaf.Id);
                                }

                                if (labelPosition == 2 || labelPosition == 3)
                                {
                                    newGpr.FillText(kvp.Value.Length * width + labelFont.FontSize * 0.625, height * 0.5, labelValue, labelFont, sequenceColour, TextBaselines.Middle, tag: leaf.Id);
                                }
                            }


                            if (colourMode == 0)
                            {
                                List<int> sequence = new List<int>();
                                int currentPiece = 0;

                                bool inGap = false;

                                newGpr.Save();

                                int firstInGap = -1;

                                for (int i = 0; i < kvp.Value.Length; i++)
                                {
                                    if (kvp.Value[i] == '-')
                                    {
                                        if (i == 0)
                                        {
                                            firstInGap = 0;
                                        }

                                        if (inGap)
                                        {
                                            currentPiece++;
                                        }
                                        else
                                        {
                                            if (currentPiece > 0)
                                            {
                                                sequence.Add(currentPiece);
                                            }
                                            inGap = true;
                                            currentPiece = 1;
                                        }
                                    }
                                    else
                                    {
                                        if (i == 0)
                                        {
                                            firstInGap = 1;
                                        }

                                        if (!inGap)
                                        {
                                            currentPiece++;
                                        }
                                        else
                                        {
                                            if (currentPiece > 0)
                                            {
                                                sequence.Add(currentPiece);
                                            }
                                            inGap = false;
                                            currentPiece = 1;
                                        }
                                    }
                                }

                                if (currentPiece > 0)
                                {
                                    sequence.Add(currentPiece);
                                }

                                for (int i = 0; i < sequence.Count; i++)
                                {
                                    if ((i + firstInGap) % 2 == 1)
                                    {
                                        newGpr.FillRectangle(0, 0, sequence[i] * width, height, sequenceColour, tag: leaf.Id);
                                    }
                                    newGpr.Translate(sequence[i] * width, 0);
                                }
                                newGpr.Restore();
                            }
                            else if (colourMode == 1)
                            {
                                List<(int, Colour)> sequence = new List<(int, Colour)>();
                                int currentPiece = 0;
                                Colour currColour = defaultResidueColour;

                                newGpr.Save();

                                for (int i = 0; i < kvp.Value.Length; i++)
                                {
                                    Colour colour = residueFormatter(kvp.Value[i].ToString()) ?? defaultResidueColour;

                                    if (currColour == colour)
                                    {
                                        currentPiece++;
                                    }
                                    else
                                    {
                                        if (currentPiece > 0)
                                        {
                                            sequence.Add((currentPiece, currColour));
                                        }

                                        currentPiece = 1;
                                        currColour = colour;
                                    }
                                }

                                if (currentPiece > 0)
                                {
                                    sequence.Add((currentPiece, currColour));
                                }

                                for (int i = 0; i < sequence.Count; i++)
                                {
                                    newGpr.FillRectangle(-Math.Min(width * 0.2, 0.05), 0, sequence[i].Item1 * width + Math.Min(width * 0.4, 0.1), height, sequence[i].Item2, tag: leaf.Id);
                                    newGpr.Translate(sequence[i].Item1 * width, 0);
                                }
                                newGpr.Restore();
                            }

                            if (drawLetters)
                            {
                                newGpr.Save();
                                newGpr.Translate(width * 0.5, height * 0.5);
                                for (int i = 0; i < kvp.Value.Length; i++)
                                {
                                    Size letterSize = residueFont.MeasureText(kvp.Value[i].ToString());

                                    double scaleX = 1;
                                    double scaleY = 1;

                                    if (letterSize.Width > width)
                                    {
                                        scaleX = width / letterSize.Width;
                                    }

                                    if (letterSize.Height > height)
                                    {
                                        scaleY = height / letterSize.Height;
                                    }

                                    if (scaleX != 1 || scaleY != 1)
                                    {
                                        newGpr.Save();
                                        newGpr.Scale(scaleX, scaleY);
                                    }

                                    newGpr.FillText(-letterSize.Width * 0.5, 0, kvp.Value[i].ToString(), residueFont, residueLetterColour, TextBaselines.Middle);

                                    if (scaleX != 1 || scaleY != 1)
                                    {
                                        newGpr.Restore();
                                    }

                                    newGpr.Translate(width, 0);
                                }
                                newGpr.Restore();
                            }



                            newGpr.Translate(0, height + margin);
                            maxY += height + margin;
                        }

                        break;
                    }
                }
            }

            int maxSequenceLength = (from el in fastaAlignment select el.Value.Length).Max();

            if (identityColour.A > 0)
            {
                double labelWidth = labelFont.MeasureText(identityLabel).Width;
                maxLabelWidth = Math.Max(maxLabelWidth, labelWidth);

                if (labelPosition == 1 || labelPosition == 3)
                {
                    newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, defaultHeight * 0.5, identityLabel, labelFont, identityColour, TextBaselines.Middle);
                }

                if (labelPosition == 2 || labelPosition == 3)
                {
                    newGpr.FillText(maxSequenceLength * width + labelFont.FontSize * 0.625, defaultHeight * 0.5, identityLabel, labelFont, identityColour, TextBaselines.Middle);
                }

                GraphicsPath path = new GraphicsPath();
                path.MoveTo(0, defaultHeight);

                for (int i = 0; i < maxSequenceLength; i++)
                {
                    double percIdentity = getPercIdentity(i, fastaAlignment);
                    path.LineTo(i * width, defaultHeight - percIdentity * defaultHeight);
                    path.LineTo(i * width + width, defaultHeight - percIdentity * defaultHeight);
                }

                path.LineTo(maxSequenceLength * width, defaultHeight);
                path.Close();
                newGpr.FillPath(path, identityColour);

                newGpr.Translate(0, defaultHeight + defaultMargin);
                maxY += defaultHeight + defaultMargin;
            }

            if (gapColour.A > 0)
            {
                double labelWidth = labelFont.MeasureText(gapLabel).Width;
                maxLabelWidth = Math.Max(maxLabelWidth, labelWidth);

                if (labelPosition == 1 || labelPosition == 3)
                {
                    newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, defaultHeight * 0.5, gapLabel, labelFont, gapColour, TextBaselines.Middle);
                }

                if (labelPosition == 2 || labelPosition == 3)
                {
                    newGpr.FillText(maxSequenceLength * width + labelFont.FontSize * 0.625, defaultHeight * 0.5, gapLabel, labelFont, gapColour, TextBaselines.Middle);
                }

                GraphicsPath path = new GraphicsPath();
                path.MoveTo(0, defaultHeight);

                for (int i = 0; i < maxSequenceLength; i++)
                {
                    double percGaps = getPercGaps(i, fastaAlignment);
                    path.LineTo(i * width, defaultHeight - percGaps * defaultHeight);
                    path.LineTo(i * width + width, defaultHeight - percGaps * defaultHeight);
                }

                path.LineTo(maxSequenceLength * width, defaultHeight);
                path.Close();
                newGpr.FillPath(path, gapColour);
                maxY += defaultHeight;
            }

            double leftMargin = labelPosition == 1 || labelPosition == 3 ? (labelFont.FontSize * 0.625 + maxLabelWidth) : 0;
            double rightMargin = labelPosition == 2 || labelPosition == 3 ? (labelFont.FontSize * 0.625 + maxLabelWidth) : 0;
            double alnWidth = maxSequenceLength * width;

            double totalWidth = alnWidth + leftMargin + rightMargin;

            double plotPosX = 0;
            double plotPosY = 0;

            if (alignment == 0 || alignment == 1 || alignment == 2)
            {
                plotPosY = 0;
            }
            else if (alignment == 3 || alignment == 4 || alignment == 5)
            {
                plotPosY = -maxY * 0.5;
            }
            else if (alignment == 6 || alignment == 7 || alignment == 8)
            {
                plotPosY = -maxY;
            }

            if (alignment == 0 || alignment == 3 || alignment == 6)
            {
                plotPosX = leftMargin;
            }
            else if (alignment == 1 || alignment == 4 || alignment == 7)
            {
                plotPosX = leftMargin - totalWidth * 0.5;
            }
            else if (alignment == 2 || alignment == 5 || alignment == 8)
            {
                plotPosX = leftMargin - totalWidth;
            }

            graphics.DrawGraphics(position.X + anchorX + plotPosX, position.Y + anchorY + plotPosY, newGpr);

            Point topLeft = new Point(position.X + anchorX + plotPosX - leftMargin, position.Y + anchorY + plotPosY);
            Point bottomRight = new Point(topLeft.X + totalWidth, topLeft.Y + maxY);

            return new Point[] { topLeft, bottomRight };
        }

        private static double getPercIdentity(int position, Dictionary<string, string> alignment)
        {
            Dictionary<char, int> chars = new Dictionary<char, int>();
            foreach (KeyValuePair<string, string> kvp in alignment)
            {
                if (chars.ContainsKey(kvp.Value[position]) && kvp.Value[position] != '-')
                {
                    chars[kvp.Value[position]]++;
                }
                else
                {
                    chars[kvp.Value[position]] = 1;
                }
            }

            return (double)chars.Values.Max() / (alignment.Count * (1 - getPercGaps(position, alignment)));
        }


        private static double getPercGaps(int position, Dictionary<string, string> alignment)
        {
            int countGaps = 0;
            foreach (KeyValuePair<string, string> kvp in alignment)
            {
                if (kvp.Value[position] == '-')
                {
                    countGaps++;
                }
            }

            return (double)countGaps / alignment.Count;
        }

        private static Dictionary<string, string> ReadFasta(Attachment alignmentFile)
        {
            Dictionary<string, string> tbr = new Dictionary<string, string>();

            string currSeqName = "";
            string currSeq = "";


            foreach (string line in alignmentFile.GetLines())
            {
                if (line.StartsWith(">"))
                {
                    if (!string.IsNullOrEmpty(currSeqName))
                    {
                        tbr.Add(currSeqName, currSeq);
                    }
                    currSeqName = line.Substring(1);
                    currSeq = "";
                }
                else
                {
                    currSeq += line;
                }
            }

            if (!string.IsNullOrEmpty(currSeqName))
            {
                tbr.Add(currSeqName, currSeq);
            }

            return tbr;
        }

        private const string DefaultNucleotideColourSource = @"public static Colour? Format(object attribute)
{
    if (attribute is string state)
    {
        switch (state)
        {
            case ""A"":
                 return Colour.FromRgb(204, 121, 67);
            case ""C"":
                 return Colour.FromRgb(0, 158, 115);
            case ""T"":
                 return Colour.FromRgb(0, 114, 178);
            case ""U"":
                 return Colour.FromRgb(0, 114, 178);
            case ""G"":
                 return Colour.FromRgb(240, 228, 66);
            case ""-"":
                 return Colour.FromRgba(255, 255, 255, 0);
            default:
                return null;
        }
    }
    else
    {
        return null;
    }
}";

        private const string DefaultAminoAcidColourSource = @"public static Colour? Format(object attribute)
{
    if (attribute is string state)
    {
        switch (state)
        {
            case ""A"":
                 return Colour.FromRgb(25, 128, 230);
            case ""C"":
                 return Colour.FromRgb(230, 128, 128);
            case ""D"":
                 return Colour.FromRgb(204, 77, 204);
            case ""E"":
                 return Colour.FromRgb(204, 77, 204);
            case ""F"":
                 return Colour.FromRgb(25, 128, 230);
            case ""G"":
                 return Colour.FromRgb(230, 153, 77);
            case ""H"":
                 return Colour.FromRgb(25, 179, 179);
            case ""I"":
                 return Colour.FromRgb(25, 128, 230);
            case ""K"":
                 return Colour.FromRgb(230, 51, 25);
            case ""L"":
                 return Colour.FromRgb(25, 128, 230);
            case ""M"":
                 return Colour.FromRgb(25, 128, 230);
            case ""N"":
                 return Colour.FromRgb(25, 204, 25);
            case ""P"":
                 return Colour.FromRgb(204, 204, 0);
            case ""Q"":
                 return Colour.FromRgb(25, 204, 25);
            case ""R"":
                 return Colour.FromRgb(230, 51, 25);
            case ""S"":
                 return Colour.FromRgb(25, 204, 25);
            case ""T"":
                 return Colour.FromRgb(25, 204, 25);
            case ""V"":
                 return Colour.FromRgb(25, 128, 230);
            case ""W"":
                 return Colour.FromRgb(25, 128, 230);
            case ""Y"":
                 return Colour.FromRgb(25, 179, 179);
            case ""-"":
                 return Colour.FromRgba(255, 255, 255, 0);
            default:
                return null;
        }
    }
    else
    {
        return null;
    }
}";
    }
}
