
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;

namespace ParseTipStates
{
    /// <summary>
    /// This module can be used to parse attributes for the tips of the tree from a separate file loaded as an attachment.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// This module can be used to read "complex" attributes from a text file. If you just wish to load "presence-absence"
    /// data, the _Add attribute_ module may also be suited to your needs.
    /// 
    /// This module reads each line of the text file and splits it using the selected separator. The first item of each line
    /// should be the taxon name on which the state is going to be applied. Other items represent the attributes that will
    /// be attached to that taxon.
    /// 
    /// More than one attribute can be parsed at once; the attributes that are loaded are specified by the [Attribute(s)](#attributes)
    /// parameter.
    /// 
    /// For example, if the [Separator](#separator) is `\s` (i.e. whitespace) and the [Attribute(s)](#attributes) are `State1 State2`,
    /// the following file would assign a `State1` of `A` and a `State2` of `5` to the taxon named `Nostoc`, and a `State1` of 
    /// `B` and a `State2` of `3` to the taxon named `Synechococcus`:
    /// 
    /// ```
    /// Nostoc          A   5
    /// Synechococcus   B   3
    /// ```
    /// </description>

    public static class MyModule
    {
        public const string Name = "Parse tip states";
        public const string HelpText = "Loads tip state data from an attachment.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "716b55a3-02d9-4007-a830-8326d407b24c";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {
                ( "Parsing", "Group:4" ),
                
                /// <param name="Attachment">
                /// This parameter is used to select the attachment that contains the data file to parse.
                /// </param>
                ( "Data file:", "Attachment:" ),
                
                /// <param name="Lines to skip:">
                /// This parameter determines the lines to skip at the start of the file (useful e.g. if the data file contains
                /// header lines).
                /// </param>
                ( "Lines to skip:", "NumericUpDown:0[\"0\",\"Infinity\",\"1\",\"0\"]" ),
                
                /// <param name="Separator:" default="`\s`">
                /// This parameter contains the separator used to split the lines of the data file. If the [Regex](#regex) checkbox
                /// is checked, regex escape characters can be used. These include:
                /// 
                /// * `\t` matches a tabulation
                /// * `\s` matches a whitespace character (e.g. a space or a tabulation)
                /// 
                /// Note that, since empty elements are discarded anyways, it is not a problem if multiple instances of the separator
                /// occur in sequence (e.g. `A     B` is parsed just as well as `A B`).
                /// </param>
                ( "Separator:", "TextBox:\\s" ),
                
                /// <param name="Regex">
                /// If this check box is checked, the separator is matched using a regular expression. This makes it possible e.g. to
                /// use escape characters or to perform advanced matching (for example, if this option is active, a separator of `[\s,;]`
                /// could be used to parse a file in which the states are separated by spaces, commas and/or semicolons).
                /// </param>
                ( "Regex", "CheckBox:true" ),

                ( "Attribute", "Group:3" ),
                
                /// <param name="Attribute(s):" default="`State`">
                /// This parameter determines the name of the attribute in which the parsed states are stored. If more than one attribute
                /// should be parsed from the file, the value of this parameter can be set to a string that will be split based on the same
                /// separator that is used for the data (e.g. if the separator is `;` and the attributes to be parsed are called `State1`
                /// and `State2`, a possible value for this parameter could be `State1;State2`).
                /// </param>
                ( "Attribute(s):", "TextBox:State" ),
                ( "Multiple attributes should use the same separator as the data", "Label:[\"Left\",\"Italic\",\"#808080\"]" ),
                
                /// <param name="Attribute type:">
                /// This parameter determines the type of the attribute that is parsed. If this is `String`, the attribute is stored as a
                /// string, even if the contents represent a number (e.g. the number `1` would be stored as the string `"1"`). Depending on
                /// how you intend to analyse the data, this may or may not be your intended behaviour - e.g. if the attribute represents
                /// a discrete character state, it is appropriate to parse it as a string; if it is instead a continuous character state,
                /// parsing it as a number may be more appropriate.
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameters and signals to the downstream modules that the tree should be
                /// redrawn.
                /// </param>
                ( "Apply", "Button:" )
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((string)previousParameterValues["Attribute(s):"] != (string)currentParameterValues["Attribute(s):"])
            {
                string attributeName = (string)currentParameterValues["Attribute(s):"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type:", attrType);
                }
            }

            Attachment previousAttachment = (Attachment)previousParameterValues["Data file:"];
            Attachment newAttachment = (Attachment)currentParameterValues["Data file:"];

            return (bool)currentParameterValues["Apply"] || (previousAttachment != newAttachment);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            int skipLines = (int)(double)parameterValues["Lines to skip:"];
            string separatorString = (string)parameterValues["Separator:"];
            bool separatorRegex = (bool)parameterValues["Regex"];
            string attributeType = (string)parameterValues["Attribute type:"];
            string attributeName = (string)parameterValues["Attribute(s):"];
            Attachment attachment = (Attachment)parameterValues["Data file:"];

            if (attachment != null)
            {
                Regex separator;

                if (separatorRegex)
                {
                    separator = new Regex(separatorString);
                }
                else
                {
                    separator = new Regex(Regex.Escape(separatorString));
                }

                string[] attributeNames = (from el in separator.Split(Regex.Unescape(attributeName)) where !string.IsNullOrEmpty(el) select el).ToArray();

                string[] lines = attachment.GetLines();

                Dictionary<string, object[]> attributes = new Dictionary<string, object[]>();

                for (int i = skipLines; i < lines.Length; i++)
                {
                    if (!string.IsNullOrEmpty(lines[i]))
                    {
                        string[] splitLine = (from el in separator.Split(lines[i]) where !string.IsNullOrEmpty(el) select el).ToArray();
                        if (splitLine.Length > 1)
                        {
                            object[] attribute = new object[splitLine.Length - 1];
                            for (int j = 1; j < splitLine.Length; j++)
                            {
                                if (attributeType == "String")
                                {
                                    attribute[j - 1] = splitLine[j];
                                }
                                else if (attributeType == "Number")
                                {
                                    if (double.TryParse(splitLine[j], out double parsed))
                                    {
                                        attribute[j - 1] = parsed;
                                    }
                                    else
                                    {
                                        attribute[j - 1] = double.NaN;
                                    }
                                }
                            }

                            attributes.Add(splitLine[0], attribute);
                        }
                    }
                }

                List<TreeNode> leaves = tree.GetLeaves();

                for (int i = 0; i < leaves.Count; i++)
                {
                    if (attributes.TryGetValue(leaves[i].Name, out object[] attribute))
                    {
                        for (int j = 0; j < Math.Min(attribute.Length, attributeNames.Length); j++)
                        {
                            leaves[i].Attributes[attributeNames[j]] = attribute[j];
                        }
                    }
                }
            }
        }

    }
}

