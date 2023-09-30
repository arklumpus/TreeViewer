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
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Linq;
using System.Runtime.InteropServices;

namespace aea7e246be93f4d0da67a88af05479b48
{
    /// <summary>
    /// This module reads an alignment file in FASTA format from an attachment and adds the alignment to the tree plot.
    /// Clicking on a sequence in the alignment selects the corresponding tip in the tree and vice versa.
    /// 
    /// The module can be used in two ways, depending on the value of the [Mode](#mode) parameter: if this is `Alignment block`,
    /// the alignment is drawn as a single block that can be positioned on the tree plot; if this is `Sequences at nodes`, each
    /// sequence is drawn individually at a position corresponding to the node it refers to. In this case, the sequences may not
    /// appear "aligned" in the plot, unless the [Anchor type](#anchor-type) is set to `Origin`.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Plot alignment";
        public const string HelpText = "Adds the plot of an alignment to the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.2");
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        public const string Id = "ea7e246b-e93f-4d0d-a67a-88af05479b48";

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACuSURBVDhPY2YAgqKiogZLS8v9QMx4/PjxAyAxYgELlK7v6+tjBBr0H8huAAl41W4EsRkWuF4EUQxidnWMIPrVoSawuNidjyCKgQlkO5gFBeh8QoAJiMG2gzhQuh7EJhaAvQB1OlkAbADMBSBAsmHoGihxzcAAWNwjA0dgmBCdmHAFIiMsIW1r9gfLdUZuBfMTM8+CKERCApMUAIx0gOwaYgBWL5BiCMVeoDAWGBgAmytK8c+kTR8AAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAItSURBVEhLzVU9aFNRFL4vvnQIZGwQXeogdNI6RHDpIi6x0MVgOxSpYkBsMH+DDjGkDvoazE8hpRRaOwhSqoNDMjmI2EkLdbFUO4R2KR2CkujySOJ37jt55gUdfDeDH5yc75z7OOedn/si+pFIJEaZDgQe1hIIfg9qNxaLrVkedTgSdDqdMOSLpmnXI5GIl91K0Fh3W7PbbreDHo/nAxLNFAqF59apEKH06w5TsX7lEzMhAuMPZYzjd/P2eWD/OzNnBWH6KRaLHxH8AHSKbFX0JriGt39KBC1agFyNx+On5YkCZIJkMhmEOoegy2Tn8/kyaZoFaRXIBDRcUpAdvHUT8gO+n7AHkwCgBJt441ssN5GAqriYSqXG5BMuoWF7LkO/abVaZ0qlUs1yW0AldSRbQcvus+ufQRXQ7r/tD86gNVVuk8hkMr3b5EA0Gh1m+n9C3kKqoF6vnyLu8/kahmH8voqKkK1pNBovvF7vIYlpmt8w+CMMeF4+oQi79xj0V2yL5vf7T4C/xPaksaKX+Ng1dNY2stlsG2oOFdzG6p60vM6PXfXRpGytMV2xfbN3tpn9/WNnA8FDqGBI1/XP7HINOwECjqD37xF8C7wC1+NcLrdnnbpH7wxqkGegZegn0A+Q0PUN7sIxA/zBrDKlNlFVd0EpmWv8cQYEBG+ikiE2XUNuA1qxgWAXEHRCeoU4D3sJdhWre4N9riArQDATwc6C0lD3YC/CfqUaXAghfgFrwM+xmnG/pQAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAKXSURBVFhH7ZZLaFNREIbvTQ3ZFYxacCEuFF+4ExUUurKbgHZTqW6UbgoKIU81oLbYUowQEiSmIm4qPhAEUaGiIAqiC0EXcVGLunCjgiLBRcSASfzm3NOQG21XHQTpD8M/Z+ZyZ87MeTn/Gq7lFpLJ5HSz2VyPer5QKEx6Vj0ELBtkMpnlUMR13Q3woDEqw1eBVCo1zOwvkcA9eB+mzfl8ftbzeoicvtu0qsFUX9lqHnp6R3z//PJ0zPd9z/vvVvPgqwBB9yO/kGN2fMA4FNFKgNmvhfYgZ5j1W/gnot6GVgIye+FQKFQSbjQaOVqxicR6ZayF9hYMIB+y2WxFBpVKZUKYRFTbYBJglluhnQSbZRueTCQSp8LhcBrbR6qg2gaTwFz5CbYb/QTqcRH0bjhMglIdFcy1QBJ4wsHT3S7VanWFOElErQouJd8FP0eGWP1TxtoG/LeggVqttrJUKn3zrIuHALO7ANcovwT6A/hvCLM7DhuDBqLR6Cqr/hXxeHy1VZewhP8P5u7mpNvLMTzKVjR3N1vvM/SO8eXO98Biw5yEBF9HsG2oMwS/Db9hPIheJrkt8o0Wllk2IOgkM34hejqdvk5i5Xq9Li+jGbEJOl9E98f7fS+gcwenff6hI6+s5mHBF1E7crnca6geCATCnkUH8ybAlXwI6qINjz2LDnwJUPIcl88d5BntuILkuRUfWLcKfAlQ7ioz/oQqPX+JTi7JjHEqobMFo8z4KAtxGNnO+BpyNhaLbTReBcy7BgRU4KFwMBhcYwwKWDABYF5CbMWvZqQAs4dZ8XEWXAF1DJHDqAvegS2G/oi29Ml3GjAVYPH9MCPHGUFuIlcJ3k/wi7wLI8ajhNYpxqsoZFWnWCzWrKoMx/kN3uAG5DuQ66gAAAAASUVORK5CYII=";

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

                ("Position","Group:6"),

                /// <param name="Mode:">
                /// This parameter determines whether the alignment is drawn as a single "block" positioned somewhere on
                /// the tree plot, or as individual sequences positioned at the tips they refer to. In the second case, the
                /// lines showing the % identity and the % of gaps are not shown.
                /// </param>
                ("Mode:", "ComboBox:0[\"Alignment block\",\"Sequences at nodes\"]"),
                
                /// <param name="Anchor:">
                /// If the [Mode](#mode) is `Alignment block`, this parameter is used to select the anchor used to determine
                /// the position of the alignment plot. If the selected value is `Node`, the specified node is used as an anchor.
                /// Otherwise, the selected point on the tree plot is used. Note that these positions refer to the _tree_ plot and
                /// do not take into account the presence of labels and other elements.
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
                
                /// <param name="Anchor type:">
                /// If the [Mode](#mode) is `Sequences at nodes`, this parameter is used to determine how the position of each
                /// sequence is computed. If the selected value is `Node`, each sequence is positioned at the node it corresponds to;
                /// if the selected value is `Origin`, the position of each sequence depends on the current Coordinates module:
                /// 
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | Coordinates module                 | Origin                                                                 |
                /// +====================================+========================================================================+
                /// | _Rectangular_                      | A point corresponding to the projection of the node on a line          |
                /// |                                    | perpedicular to the direction in which the tree expands and passing    |
                /// |                                    | through the root node. Usually (i.e. if the tree is horizontal), this  |
                /// |                                    | means a point with the same horizontal coordinate as the root node and |
                /// |                                    | the same vertical coordinate as the current node.                      |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | _Radial_                           | The root node.                                                         |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// | _Circular_                         | The root node.                                                         |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// |                                    |                                                                        |
                /// +------------------------------------+------------------------------------------------------------------------+
                /// </param>
                ("Anchor type:", "ComboBox:0[\"Node\",\"Origin\"]"),
                
                /// <param name="Position:">
                /// If the [Mode](#mode) is `Alignment block`, this parameter determines how much the alignment plot is shifted with respect to the position determined
                /// by the [Anchor](#anchor) and the [Alignment](#alignment). If the [Mode](#mode) is `Sequences at nodes`, instead, each sequence is
                /// shifted by the specified amount with respect to the [Reference](#reference).
                /// </param>
                ("Position:","Point:[0,0]"),

                ("Orientation", "Group:3"),

                /// <param name="Orientation:">
                /// This parameter determines the orientation of the sequence with respect to the [Reference](#reference), in degrees. If this is `0°`, the sequence is parallel to the reference (e.g. the branch), if it is `90°` it is perpendicular to the branch and so on.
                /// </param>
                ( "Orientation:", "Slider:0[\"0\",\"360\",\"0°\"]" ),
                
                /// <param name="Reference:">
                /// This parameter (along with the [Orientation](#orientation)) determines the reference for the direction along which the sequence flows.
                /// If this is `Horizontal`, the sequences are all drawn in the same direction, regardless of the orientation of the branch to which they refer.
                /// If it is `Branch`, each sequence is drawn along the direction of the branch connecting the node to its parent.
                /// </param>
                ( "Reference:", "ComboBox:1[\"Horizontal\",\"Branch\"]" ),

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



            if ((int)currentParameterValues["Colour mode:"] == 0)
            {
                controlStatus["Auto colour by node"] = ControlStatus.Enabled;

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

                controlStatus["Residue colours:"] = ControlStatus.Hidden;
                controlStatus["ResidueStyleButtons"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Auto colour by node"] = ControlStatus.Hidden;
                controlStatus["Opacity:"] = ControlStatus.Hidden;
                controlStatus["Colour:"] = ControlStatus.Hidden;

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



            if ((int)currentParameterValues["Mode:"] == 0)
            {
                controlStatus["Anchor:"] = ControlStatus.Enabled;
                controlStatus["Node:"] = ControlStatus.Enabled;
                controlStatus["Alignment:"] = ControlStatus.Enabled;

                controlStatus["Anchor type:"] = ControlStatus.Hidden;
                controlStatus["Orientation:"] = ControlStatus.Hidden;
                controlStatus["Reference:"] = ControlStatus.Hidden;
                controlStatus["Orientation"] = ControlStatus.Hidden;

                controlStatus["Label position:"] = ControlStatus.Enabled;
                controlStatus["Labels"] = ControlStatus.Enabled;
                controlStatus["Margin:"] = ControlStatus.Enabled;
                controlStatus["Identity colour:"] = ControlStatus.Enabled;
                controlStatus["Gaps colour:"] = ControlStatus.Enabled;

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

            }
            else
            {
                controlStatus["Anchor:"] = ControlStatus.Hidden;
                controlStatus["Node:"] = ControlStatus.Hidden;
                controlStatus["Alignment:"] = ControlStatus.Hidden;

                controlStatus["Anchor type:"] = ControlStatus.Enabled;
                controlStatus["Orientation:"] = ControlStatus.Enabled;
                controlStatus["Reference:"] = ControlStatus.Enabled;
                controlStatus["Orientation"] = ControlStatus.Enabled;

                controlStatus["Label position:"] = ControlStatus.Hidden;
                controlStatus["Labels"] = ControlStatus.Hidden;
                controlStatus["Margin:"] = ControlStatus.Hidden;
                controlStatus["Identity colour:"] = ControlStatus.Hidden;
                controlStatus["Gaps colour:"] = ControlStatus.Hidden;


                controlStatus["Label font:"] = ControlStatus.Hidden;
                controlStatus["Attribute"] = ControlStatus.Hidden;
                controlStatus["Label attribute:"] = ControlStatus.Hidden;
                controlStatus["Attribute type:"] = ControlStatus.Hidden;
                controlStatus["Attribute format..."] = ControlStatus.Hidden;
                controlStatus["Identity label:"] = ControlStatus.Hidden;
                controlStatus["Gaps label:"] = ControlStatus.Hidden;
            }


            if ((string)previousParameterValues["Label attribute:"] != (string)currentParameterValues["Label attribute:"])
            {
                string attributeName = (string)currentParameterValues["Label attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType) && (string)previousParameterValues["Attribute type:"] == (string)currentParameterValues["Attribute type:"] && (string)currentParameterValues["Attribute type:"] != attrType)
                {
                    parametersToChange.Add("Attribute type:", attrType);

                    if (previousParameterValues["Attribute format..."] == currentParameterValues["Attribute format..."])
                    {
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
            }

            if ((string)previousParameterValues["Attribute type:"] != (string)currentParameterValues["Attribute type:"])
            {
                string attrType = (string)currentParameterValues["Attribute type:"];

                if (previousParameterValues["Attribute format..."] == currentParameterValues["Attribute format..."])
                {
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

            if ((int)currentParameterValues["ResidueStyleButtons"] == 0)
            {
                object[] formatterParams = new object[] { DefaultNucleotideColourSource, false };

                ColourFormatterOptions formatterOptions = new ColourFormatterOptions(DefaultNucleotideColourSource, formatterParams);

                formatterOptions.AttributeName = "(N/A)";
                formatterOptions.AttributeType = "String";
                formatterOptions.DefaultColour = ((ColourFormatterOptions)currentParameterValues["Residue colours:"]).DefaultColour;

                if (previousParameterValues["Residue colours:"] == currentParameterValues["Residue colours:"])
                {
                    parametersToChange["Residue colours:"] = formatterOptions;
                }
            }
            else if ((int)currentParameterValues["ResidueStyleButtons"] == 1)
            {
                object[] formatterParams = new object[] { DefaultAminoAcidColourSource, false };

                ColourFormatterOptions formatterOptions = new ColourFormatterOptions(DefaultAminoAcidColourSource, formatterParams);

                formatterOptions.AttributeName = "(N/A)";
                formatterOptions.AttributeType = "String";
                formatterOptions.DefaultColour = ((ColourFormatterOptions)currentParameterValues["Residue colours:"]).DefaultColour;

                if (previousParameterValues["Residue colours:"] == currentParameterValues["Residue colours:"])
                {
                    parametersToChange["Residue colours:"] = formatterOptions;
                }
            }

            if ((Attachment)currentParameterValues["FASTA alignment:"] != (Attachment)previousParameterValues["FASTA alignment:"])
            {
                Attachment attachment = (Attachment)currentParameterValues["FASTA alignment:"];

                Dictionary<string, string> fasta = ReadFasta(attachment);

                if (fasta.Count > 0)
                {
                    int maxLen = (from el in fasta select el.Value.Length).Max();

                    if (previousParameterValues["Start:"] == currentParameterValues["Start:"])
                    {
                        parametersToChange["Start:"] = 1.0;
                    }

                    if (previousParameterValues["End:"] == currentParameterValues["End:"])
                    {
                        parametersToChange["End:"] = (double)maxLen;
                    }
                }
            }

            if ((int)currentParameterValues["Mode:"] != (int)previousParameterValues["Mode:"] && ((Point)currentParameterValues["Position:"]).Equals((Point)previousParameterValues["Position:"]))
            {
                parametersToChange["Position:"] = new Point();
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

            int mode = (int)parameterValues["Mode:"];

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

            double orientation = (double)parameterValues["Orientation:"] * Math.PI / 180;
            int orientationReference = (int)parameterValues["Reference:"];


            int branchReference;

            if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out _))
            {
                // Rectangular coordinates
                branchReference = 0;
            }
            else if (coordinates.TryGetValue("d0ab64ba-3bcd-443f-9150-48f6e85e97f3", out _))
            {
                // Circular coordinates
                branchReference = 2;
            }
            else
            {
                // Radial coordinates
                branchReference = 1;
            }

            if (orientationReference == 1)
            {
                orientationReference += branchReference;
            }

            int anchorKind = (int)parameterValues["Anchor type:"];
            Point delta = (Point)parameterValues["Position:"];


            if (mode == 0)
            {
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
                                sequenceColour = Modules.AutoColour(leaf).WithAlpha(opacity);
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
                                    IEnumerable<FormattedText> formattedText;

                                    if (labelFont.FontFamily.IsStandardFamily)
                                    {
                                        formattedText = FormattedText.Format(labelValue, (FontFamily.StandardFontFamilies)Array.IndexOf(FontFamily.StandardFamilies, labelFont.FontFamily.FileName), labelFont.FontSize);
                                    }
                                    else
                                    {
                                        formattedText = FormattedText.Format(labelValue, labelFont, labelFont, labelFont, labelFont);
                                    }

                                    double labelWidth = formattedText.Measure().Width;

                                    maxLabelWidth = Math.Max(labelWidth, maxLabelWidth);

                                    if (labelPosition == 1 || labelPosition == 3)
                                    {
                                        newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, height * 0.5, formattedText, sequenceColour, TextBaselines.Middle, tag: leaf.Id);
                                    }

                                    if (labelPosition == 2 || labelPosition == 3)
                                    {
                                        newGpr.FillText(kvp.Value.Length * width + labelFont.FontSize * 0.625, height * 0.5, formattedText, sequenceColour, TextBaselines.Middle, tag: leaf.Id);
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

                if (identityColour.A > 0 && !string.IsNullOrEmpty(identityLabel))
                {
                    IEnumerable<FormattedText> formattedText;

                    if (labelFont.FontFamily.IsStandardFamily)
                    {
                        formattedText = FormattedText.Format(identityLabel, (FontFamily.StandardFontFamilies)Array.IndexOf(FontFamily.StandardFamilies, labelFont.FontFamily.FileName), labelFont.FontSize);
                    }
                    else
                    {
                        formattedText = FormattedText.Format(identityLabel, labelFont, labelFont, labelFont, labelFont);
                    }

                    double labelWidth = formattedText.Measure().Width;
                    maxLabelWidth = Math.Max(maxLabelWidth, labelWidth);

                    if (labelPosition == 1 || labelPosition == 3)
                    {
                        newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, defaultHeight * 0.5, formattedText, identityColour, TextBaselines.Middle);
                    }

                    if (labelPosition == 2 || labelPosition == 3)
                    {
                        newGpr.FillText(maxSequenceLength * width + labelFont.FontSize * 0.625, defaultHeight * 0.5, formattedText, identityColour, TextBaselines.Middle);
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

                if (gapColour.A > 0 && !string.IsNullOrEmpty(gapLabel))
                {
                    IEnumerable<FormattedText> formattedText;

                    if (labelFont.FontFamily.IsStandardFamily)
                    {
                        formattedText = FormattedText.Format(gapLabel, (FontFamily.StandardFontFamilies)Array.IndexOf(FontFamily.StandardFamilies, labelFont.FontFamily.FileName), labelFont.FontSize);
                    }
                    else
                    {
                        formattedText = FormattedText.Format(gapLabel, labelFont, labelFont, labelFont, labelFont);
                    }

                    double labelWidth = formattedText.Measure().Width;
                    maxLabelWidth = Math.Max(maxLabelWidth, labelWidth);

                    if (labelPosition == 1 || labelPosition == 3)
                    {
                        newGpr.FillText(-labelFont.FontSize * 0.625 - labelWidth, defaultHeight * 0.5, formattedText, gapColour, TextBaselines.Middle);
                    }

                    if (labelPosition == 2 || labelPosition == 3)
                    {
                        newGpr.FillText(maxSequenceLength * width + labelFont.FontSize * 0.625, defaultHeight * 0.5, formattedText, gapColour, TextBaselines.Middle);
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
            else
            {
                Point rootPoint = coordinates[Modules.RootNodeId];
                coordinates.TryGetValue("92aac276-3af7-4506-a263-7220e0df5797", out Point circularCenter);

                double minX = double.MaxValue;
                double maxX = double.MinValue;
                double minY = double.MaxValue;
                double maxY = double.MinValue;

                bool anyMaxMin = false;

                void updateMaxMin(Point pt)
                {
                    anyMaxMin = true;
                    minX = Math.Min(minX, pt.X);
                    maxX = Math.Max(maxX, pt.X);
                    minY = Math.Min(minY, pt.Y);
                    maxY = Math.Max(maxY, pt.Y);
                }

                static Point rotatePoint(Point pt, double angle)
                {
                    return new Point(pt.X * Math.Cos(angle) - pt.Y * Math.Sin(angle), pt.X * Math.Sin(angle) + pt.Y * Math.Cos(angle));
                }

                static Point sumPoint(Point pt1, Point pt2)
                {
                    return new Point(pt1.X + pt2.X, pt1.Y + pt2.Y);
                }

                foreach (TreeNode node in tree.GetLeaves())
                {
                    Point point = coordinates[node.Id];

                    Point anglePoint = point;

                    if (orientationReference == 1 && node.Parent != null)
                    {
                        Point parentPoint = coordinates[node.Parent.Id];

                        TreeNode parent = node.Parent;

                        while (point.Y - parentPoint.Y == 0 && point.X - parentPoint.X == 0 && parent.Parent != null)
                        {
                            parent = parent.Parent;
                            parentPoint = coordinates[parent.Id];
                        }

                        Point pA = coordinates[parent.Children[0].Id];
                        Point pB = coordinates[parent.Children[^1].Id];

                        double numerator = pA.Y + pB.Y - 2 * parentPoint.Y;
                        double denominator = pA.X + pB.X - 2 * parentPoint.X;

                        if (Math.Abs(numerator) > 1e-5 && Math.Abs(denominator) > 1e-5)
                        {
                            double m = numerator / denominator;

                            double x = (m * (parentPoint.Y - point.Y + m * point.X) + parentPoint.X) / (m * m + 1);
                            double y = parentPoint.Y - (x - parentPoint.X) / m;

                            anglePoint = new Point(x, y);
                        }
                        else if (Math.Abs(numerator) > 1e-5)
                        {
                            anglePoint = new Point(point.X, parentPoint.Y);
                        }
                        else if (Math.Abs(denominator) > 1e-5)
                        {
                            anglePoint = new Point(parentPoint.X, point.Y);
                        }
                        else
                        {
                            anglePoint = point;
                        }
                    }

                    double rotationAngle = 0;

                    if (orientationReference > 0 && node.Parent != null)
                    {
                        if (orientationReference == 1)
                        {
                            double angle = Math.Atan2(point.Y - anglePoint.Y, point.X - anglePoint.X);
                            rotationAngle += angle;
                        }
                        else if (orientationReference == 2)
                        {
                            Point parentPoint = coordinates[node.Parent.Id];

                            TreeNode parent = node.Parent;

                            while (point.Y - parentPoint.Y == 0 && point.X - parentPoint.X == 0 && parent.Parent != null)
                            {
                                parent = parent.Parent;
                                parentPoint = coordinates[parent.Id];
                            }

                            double angle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                            rotationAngle += angle;
                        }
                        else if (orientationReference == 3)
                        {
                            Point pt = coordinates[node.Id];

                            rotationAngle += Math.Atan2(pt.Y, pt.X);
                        }
                    }
                    else if (orientationReference > 0 && node.Parent == null)
                    {
                        if (orientationReference != 3)
                        {
                            Point parentPoint = coordinates[Modules.RootNodeId];
                            double angle = Math.Atan2(point.Y - parentPoint.Y, point.X - parentPoint.X);
                            rotationAngle += angle;
                        }
                        else
                        {
                            Point pt = coordinates[node.Id];

                            rotationAngle += Math.Atan2(pt.Y, pt.X);
                        }
                    }

                    while (Math.Abs(rotationAngle) > Math.PI)
                    {
                        rotationAngle -= 2 * Math.PI * Math.Sign(rotationAngle);
                    }

                    if (anchorKind == 1)
                    {
                        double referenceAngle = rotationAngle;

                        if (coordinates.TryGetValue("68e25ec6-5911-4741-8547-317597e1b792", out Point coordinateReference))
                        {
                            referenceAngle = Math.Atan2(coordinateReference.Y, coordinateReference.X);
                        }

                        if (branchReference == 0)
                        {
                            point = coordinates[node.Id];

                            Point branchVector = new Point(Math.Cos(referenceAngle), Math.Sin(referenceAngle));

                            double d = (rootPoint.X - point.X) * branchVector.X + (rootPoint.Y - point.Y) * branchVector.Y;

                            Point proj = new Point(point.X + d * branchVector.X, point.Y + d * branchVector.Y);

                            point = proj;
                        }
                        else if (branchReference == 1)
                        {
                            point = coordinates[Modules.RootNodeId];
                        }
                        else if (branchReference == 2)
                        {
                            point = circularCenter;
                        }
                    }

                    graphics.Save();

                    graphics.Translate(point.X, point.Y);
                    graphics.Rotate(rotationAngle);

                    graphics.Translate(delta.X, delta.Y);
                    graphics.Rotate(orientation);

                    double totalAngle = rotationAngle + orientation;

                    while (totalAngle > Math.PI)
                    {
                        totalAngle -= 2 * Math.PI;
                    }

                    while (totalAngle < -Math.PI)
                    {
                        totalAngle += 2 * Math.PI;
                    }

                    foreach (KeyValuePair<string, string> kvp in fastaAlignment)
                    {
                        if (kvp.Key.Contains(node.Name))
                        {
                            double height = defaultHeight;

                            if (node.Attributes.TryGetValue(heightFO.AttributeName, out object heightAttributeObject) && heightAttributeObject != null)
                            {
                                height = heightFormatter(heightAttributeObject) ?? defaultHeight;
                            }

                            Colour sequenceColour = defaultColour;


                            if (!autoColour)
                            {
                                if (node.Attributes.TryGetValue(colourFO.AttributeName, out object colourAttributeObject) && colourAttributeObject != null)
                                {
                                    sequenceColour = colourFormatter(colourAttributeObject) ?? defaultColour;
                                }
                            }
                            else
                            {
                                sequenceColour = Modules.AutoColour(node).WithAlpha(opacity);
                            }

                            if (height > 0 && (sequenceColour.A > 0 || colourMode == 1))
                            {
                                if (colourMode == 0)
                                {
                                    List<int> sequence = new List<int>();
                                    int currentPiece = 0;

                                    bool inGap = false;

                                    graphics.Save();

                                    graphics.Translate(0, -height * 0.5);

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
                                            graphics.FillRectangle(0, 0, sequence[i] * width, height, sequenceColour, tag: node.Id);
                                        }
                                        graphics.Translate(sequence[i] * width, 0);
                                    }
                                    graphics.Restore();

                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(0, -height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(kvp.Value.Length * width, -height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(kvp.Value.Length * width, height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(0, height * 0.5), orientation)), rotationAngle)));
                                }
                                else if (colourMode == 1)
                                {
                                    List<(int, Colour)> sequence = new List<(int, Colour)>();
                                    int currentPiece = 0;
                                    Colour currColour = defaultResidueColour;

                                    graphics.Save();

                                    graphics.Translate(0, -height * 0.5);

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
                                        graphics.FillRectangle(-Math.Min(width * 0.2, 0.05), 0, sequence[i].Item1 * width + Math.Min(width * 0.4, 0.1), height, sequence[i].Item2, tag: node.Id);
                                        graphics.Translate(sequence[i].Item1 * width, 0);
                                    }
                                    graphics.Restore();

                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(0, -height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(kvp.Value.Length * width, -height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(kvp.Value.Length * width, height * 0.5), orientation)), rotationAngle)));
                                    updateMaxMin(sumPoint(point, rotatePoint(sumPoint(delta, rotatePoint(new Point(0, height * 0.5), orientation)), rotationAngle)));
                                }

                                if (drawLetters)
                                {
                                    graphics.Save();
                                    graphics.Translate(0, -height * 0.5);
                                    graphics.Translate(width * 0.5, height * 0.5);
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
                                            graphics.Save();
                                            graphics.Scale(scaleX, scaleY);
                                        }

                                        graphics.FillText(-letterSize.Width * 0.5, 0, kvp.Value[i].ToString(), residueFont, residueLetterColour, TextBaselines.Middle);

                                        if (scaleX != 1 || scaleY != 1)
                                        {
                                            graphics.Restore();
                                        }

                                        graphics.Translate(width, 0);
                                    }
                                    graphics.Restore();
                                }
                            }

                            break;
                        }
                    }

                    graphics.Restore();

                }

                if (anyMaxMin)
                {
                    return new Point[] { new Point(minX, minY), new Point(maxX, maxY) };
                }
                else
                {
                    return new Point[] { new Point(), new Point() };
                }
            }
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
