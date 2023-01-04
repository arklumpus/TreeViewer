using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;
using System.Linq;
using PhyloTree.Extensions;

namespace a36ca9f8c25b24b2baa16308227788e5d
{
    /// <summary>
    /// This module compares the current tree with another tree. The other tree can be specified either as an attachment, or it can be one
    /// of the loaded trees.
    /// 
    /// For each branch in the current tree, the model will determine whether the split induced by the branch is present or compatible
    /// with the other tree. The results of these comparisons (`Yes` or `No`) are stored in the attributes `prefix_Present` and
    /// `prefix_Compatible` for each node (where `prefix_` is the value of the [Prefix](#prefix) attribute).
    /// 
    /// If the [Store attributes on equivalent splits](#store-attributes-on-equivalent-splits) check box is checked, for each split
    /// that is present in both trees, all the attributes from the other tree are copied on the current tree too, with the specified
    /// [Prefix](#prefix).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Compare trees";
        public const string HelpText = "Compares a tree with another tree loaded as an attachment or from the loaded trees.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "36ca9f8c-25b2-4b2b-aa16-308227788e5d";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAADhSURBVDhPxZIxEoIwEEUDrQdwPI1k7LTwCnoFGisre64AnbWNnaPHsLRzOAX+n9mFAEGw8s1ksmzy/oSA+TuRzD+TpukOUx7LQyWNQbAuVSNjWBfAAiP3Qyj4kuLLWZY96lfAQoLpjrHHQhGSQUtmo3UHfghGwd78vdKeLRc3SonKpHeJXkjkyYo9nDe1TAZPAPkldZdWyOAduCbgXSCsghSxVrDHzfoZgzLRjYT1c7asWItj4m/yCO71eIJRmceXsgb7Xa+3EGJ9vOgpu9hJASQQYq+nbfMnTsELcbIxxnwAm1Nqoy5dhSYAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAEmSURBVEhL7ZQxDsIwDEXTrhwAcRoascHAzDW6MDExsfQazAywofYY3ABxiuJvmShJkwBtJRaeVNkQ879jV6g/PyeTOCplWRYUauSjG9jiwBjQQSuprqqqkfwrfHFC55LY1FL4FSFxNGobaImgY0KfOR4254KeVh6uiYkjcXaQKoTB9L7wz4F+zK6oCY7YGZEc+DfhJCIOajlDs539Bd8i7yZZQtxGb48rRxx8ZCDRQIY8DhpN8PeAbsKxU5DawwssGFEMkDs6L3Hg7OAT8QC8s9tk3uLhbyyMQR9xdBqqES2GDXp2nsJo5WOI4yYxMjKw59a78+XuxDqX/dpZuPNXMXAsQaLv8TuoY3+0IXRvA/DGRNO4mkEGIGLC4kgGGwDPxIgrpdQTK6SZXYm0+0UAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsEAAA7BAbiRa+0AAAFsSURBVFhH7ZY9TsMwGIYd1h4AcQNu0VpsMDBzjSydOjF1yTWYGWBDKbfgBogDMLd+zfeZ2PFfhG2WPlJlR7L9Pv7stBVnzvw3HbVN6ft+o5oR/eYC03DQVMANV0hLQA04UlcOw3CgfhF84ci4oAeXkSYUIRSOjisgqQVFJGLhYHYHUhN87B9eeI7cPt2ZsTlreS/hEolJOKMlctcIvgU5C3jCGfl19YaxyUsdfQ1jEpFwhiU2oXAQFQCOhB5/+XmTCmesO+Ej9BoGWRAORqqURm2Ger8sPQI9XpVUPzAqhM9aqLJH13TnBisQOP/ZAgEw10jF8ArELl8ONFZ/qX2s1kf+4NllJlAgnNusOZbAX8NzoAyDEagRztVwmGb8CLTYeYiudvjt7tlcvuvv986tCirQbOe+I0EFkj8YuajdutVMYf8lK8ECCfn6eH8oLgAyJHQ4OlUEQETChINqAsAjYYWDqgJgIjELF0KIE2UF0pMbjH32AAAAAElFTkSuQmCC";

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
                ( "Tree", "Group:3" ),
                /// <param name="Tree source:">
                /// This parameter determines what tree is used for the comparison. If this is `Loaded tree`, the other tree is
                /// one of the loaded trees. If this is `Attachment`, the other tree is a tree that has been loaded as an attachment.
                /// </param>
                ( "Tree source:", "ComboBox:1[\"Loaded tree\",\"Attachment\"]" ),
                
                /// <param name="Tree:">
                /// If the value for [Tree source](#tree-source) is `Attachment`, this parameter determines the attachment from
                /// which the tree is loaded. The tree is loaded using the File type module that reports the highest compatibility.
                /// If the attachment contains multiple trees, only the first tree is used.
                /// </param>
                ( "Tree:", "Attachment:" ),
                
                /// <param name="Tree index:">
                /// If the value for [Tree source](#tree-source) is `Loaded tree`, this parameter determines which one of the loaded
                /// trees is used for the comparison.
                /// </param>
                ( "Tree index:", "NumericUpDown:1[\"1\",\"Infinity\",\"1\",\"0\"]" ),

                ( "Attributes", "Group:3" ),
                
                /// <param name="Use tree name as prefix">
                /// If this check box is checked, the prefix to use for attribute names is determined automatically from the name of
                /// the attachment or the loaded tree index. Otherwise, it can be specified manually.
                /// </param>
                ( "Use tree name as prefix", "CheckBox:true" ),
                
                /// <param name="Prefix:">
                /// If the [Use tree name as prefix](use-tree-name-as-prefix) check box is unchecked, this text box can be used to
                /// determine the prefix that will be used for attribute names.
                /// </param>
                ( "Prefix:", "TextBox:Other_" ),
                
                /// <param name="Store attributes on equivalent splits">
                /// If this check box is checked, for splits that are present in both trees, attributes are copied from the other
                /// tree on to the current tree. Each attribute is prefixed with the [Prefix](#prefix).
                /// </param>
                ( "Store attributes on equivalent splits", "CheckBox: true" ),

                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ("Apply", "Button:")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            int treeSource = (int)currentParameterValues["Tree source:"];

            if (treeSource == 0)
            {
                controlStatus["Tree:"] = ControlStatus.Hidden;
                controlStatus["Tree index:"] = ControlStatus.Enabled;
            }
            else if (treeSource == 1)
            {
                controlStatus["Tree:"] = ControlStatus.Enabled;
                controlStatus["Tree index:"] = ControlStatus.Hidden;
            }

            bool usePrefixFromTreeName = (bool)currentParameterValues["Use tree name as prefix"];

            if (usePrefixFromTreeName)
            {
                controlStatus["Prefix:"] = ControlStatus.Hidden;
            }
            else
            {
                controlStatus["Prefix:"] = ControlStatus.Enabled;
            }

            parametersToChange["Apply"] = false;

            return (bool)currentParameterValues["Apply"] || (treeSource == 1 && currentParameterValues["Tree:"] != previousParameterValues["Tree:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string message = "";
            string messageId = Id;

            int treeSource = (int)parameterValues["Tree source:"];
            bool usePrefixFromTreeName = (bool)parameterValues["Use tree name as prefix"];
            string prefix = (string)parameterValues["Prefix:"];
            bool storeAttributes = (bool)parameterValues["Store attributes on equivalent splits"];

            TreeNode otherTree = null;

            if (treeSource == 1)
            {
                Attachment otherTreeAtt = (Attachment)parameterValues["Tree:"];

                if (otherTreeAtt != null)
                {
                    otherTree = otherTreeAtt.GetTrees()[0];

                    if (usePrefixFromTreeName)
                    {
                        prefix = otherTreeAtt.Name + "_";
                    }
                }
                else
                {
                    message = "No attachment has been selected!";
                }
            }
            else if (treeSource == 0)
            {
                TreeCollection trees = (TreeCollection)parameterValues["Trees"];
                int treeIndex = (int)(Math.Round((double)parameterValues["Tree index:"]));
                otherTree = trees[treeIndex - 1];

                if (usePrefixFromTreeName)
                {
                    prefix = "Tree" + treeIndex.ToString() + "_";
                }
            }

            if (otherTree != null)
            {
                List<(string[], string[], TreeNode)> otherSplits = (from el in otherTree.GetChildrenRecursiveLazy()
                                                                    let split = el.GetSplit()
                                                                    select (
                                                                    (from el1 in split.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToArray(),
                                                                    (from el2 in split.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToArray(),
                                                                    el
                                                                    )).ToList();


                foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
                {
                    (List<TreeNode> side1, List<TreeNode> side2) currSplit = node.GetSplit();

                    (string[], string[], double) split = ((from el1 in currSplit.side1 where el1 == null || !string.IsNullOrEmpty(el1.Name) select el1 == null ? "@Root" : el1.Name).ToArray(), (from el2 in currSplit.side2 where el2 == null || !string.IsNullOrEmpty(el2.Name) select el2 == null ? "@Root" : el2.Name).ToArray(), node.Length);

                    bool isCompatible = true;
                    bool isPresent = false;
                    TreeNode correspondence = null;


                    foreach ((string[], string[], TreeNode) otherSplit in otherSplits)
                    {
                        if (AreSameSplit(split, otherSplit))
                        {
                            isPresent = true;
                            correspondence = otherSplit.Item3;
                        }

                        if (!AreCompatible(split, otherSplit))
                        {
                            isCompatible = false;
                        }

                        if (isPresent || !isCompatible)
                        {
                            break;
                        }
                    }

                    node.Attributes[prefix + "Present"] = isPresent ? "Yes" : "No";
                    node.Attributes[prefix + "Compatible"] = isCompatible ? "Yes" : "No";

                    if (storeAttributes && isPresent && correspondence != null)
                    {
                        foreach (KeyValuePair<string, object> attribute in correspondence.Attributes)
                        {
                            if (attribute.Value != null)
                            {
                                if (attribute.Value is string sr && !string.IsNullOrEmpty(sr))
                                {
                                    node.Attributes[prefix + attribute.Key] = attribute.Value;
                                }
                                else if (attribute.Value is double d && !double.IsNaN(d))
                                {
                                    node.Attributes[prefix + attribute.Key] = attribute.Value;
                                }
                            }
                        }
                    }
                }
            }

            if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
            {
                setWarning(message, messageId);
            }
        }

        static bool AreSameSplit((string[], string[], double) split1, (string[], string[], TreeNode) split2)
        {
            if (split1.Item1.Length == split1.Item2.Length || split2.Item1.Length == split2.Item2.Length)
            {
                if (split1.Item1.Length == split1.Item2.Length && split2.Item1.Length == split2.Item2.Length)
                {
                    return AreSameSplit2(split1.Item1, split1.Item2, split2.Item1, split2.Item2) || AreSameSplit2(split1.Item1, split1.Item2, split2.Item2, split2.Item1);
                }
                else
                {
                    return false;
                }
            }

            string[] split11, split12;

            if (split1.Item1.Length > split1.Item2.Length)
            {
                split11 = split1.Item1;
                split12 = split1.Item2;
            }
            else
            {
                split11 = split1.Item2;
                split12 = split1.Item1;
            }

            string[] split21, split22;

            if (split2.Item1.Length > split2.Item2.Length)
            {
                split21 = split2.Item1;
                split22 = split2.Item2;
            }
            else
            {
                split21 = split2.Item2;
                split22 = split2.Item1;
            }

            return AreSameSplit2(split11, split12, split21, split22);
        }


        static bool AreSameSplit2(string[] split11, string[] split12, string[] split21, string[] split22)
        {
            if (split11.Length != split21.Length || split12.Length != split22.Length)
            {
                return false;
            }

            HashSet<string> union2 = new HashSet<string>(split12.Length);

            for (int i = 0; i < split12.Length; i++)
            {
                union2.Add(split12[i]);
                union2.Add(split22[i]);
            }

            if (union2.Count != split12.Length)
            {
                return false;
            }

            HashSet<string> union1 = new HashSet<string>();

            for (int i = 0; i < split11.Length; i++)
            {
                union1.Add(split11[i]);
                union1.Add(split21[i]);
            }

            return union1.Count == split11.Length;
        }

        static bool AreCompatible((string[], string[], double) s1, (string[], string[], TreeNode) s2)
        {
            bool leaves1Two = s1.Item1.Length > 0 && s1.Item2.Length > 0;
            bool leaves2Two = s2.Item1.Length > 0 && s2.Item2.Length > 0;

            if (!leaves1Two && !leaves2Two)
            {
                string[] leaves1 = s1.Item1.Length > 0 ? s1.Item1 : s1.Item2;
                string[] leaves2 = s2.Item1.Length > 0 ? s2.Item1 : s2.Item2;

                return !leaves1.ContainsAny(leaves2) || leaves1.ContainsAll(leaves2) || leaves2.ContainsAll(leaves1);
            }
            else
            {
                string[][] leaves1;

                if (leaves1Two)
                {
                    leaves1 = new string[][] { s1.Item1, s1.Item2 };
                }
                else
                {
                    leaves1 = new string[][] { s1.Item1.Length > 0 ? s1.Item1 : s1.Item2 };
                }

                string[][] leaves2;

                if (leaves2Two)
                {
                    leaves2 = new string[][] { s2.Item1, s2.Item2 };
                }
                else
                {
                    leaves2 = new string[][] { s2.Item1.Length > 0 ? s2.Item1 : s2.Item2 };
                }

                if (leaves1Two && leaves2Two)
                {
                    return !leaves1[0].Intersect(leaves2[0]).Any() || !leaves1[0].Intersect(leaves2[1]).Any() || !leaves1[1].Intersect(leaves2[0]).Any() || !leaves1[1].Intersect(leaves2[1]).Any();
                }
                else if (!leaves1Two && leaves2Two)
                {
                    return (!leaves1[0].ContainsAny(leaves2[0]) || leaves1[0].ContainsAll(leaves2[0]) || leaves2[0].ContainsAll(leaves1[0])) && (!leaves1[0].ContainsAny(leaves2[1]) || leaves1[0].ContainsAll(leaves2[1]) || leaves2[1].ContainsAll(leaves1[0]));
                }
                else if (leaves1Two && !leaves2Two)
                {
                    return (!leaves2[0].ContainsAny(leaves1[0]) || leaves2[0].ContainsAll(leaves1[0]) || leaves1[0].ContainsAll(leaves2[0])) && (!leaves2[0].ContainsAny(leaves1[1]) || leaves2[0].ContainsAll(leaves1[1]) || leaves1[1].ContainsAll(leaves2[0]));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}
