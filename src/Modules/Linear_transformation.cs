using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace a50a98b436e924499a81e4f9609b47e7b
{
    /// <summary>
    /// This module applies a linear transformation $f(x) = ax + b$ to the value of an attribute on the tree.
    /// $x$ is the value of the [Attribute](#attribute), $a$ is the [Scaling factor](#scaling-factor), and $b$
    /// is the [Translation factor](#translation-factor). The resulting value can be used to replace the
    /// current attribute value, or saved to a new attribute.
    /// 
    /// For obvious reasons, this module can only be applied to attributes with numerical values.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Linear transformation";
        public const string HelpText = "Applies a linear transformation to a numeric attribute.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "50a98b43-6e92-4499-a81e-4f9609b47e7b";

        public static bool Repeatable { get; } = true;

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC5SURBVDhPpZNBEsIgDEWhJ5OtG6+gV2DflXuuoFdw47berM2PJIMtDWV8M2kK0/8DoXhnEGM8UZooQkrpw5PEeXxdKT3e94sfvlNbCjGQrGKKG8ZVg5UYBDxKMVV/Yq4Jmc3IEFPM2UQxeyBYlZsGrWWbBi0x2D0FS5ybzFQNDlSexGRjsBbLKVRgEy8f0J/mO8SKNnFv2TCBeR4yhXHgLRzYcw2+H8M/YrxgBb1i9EtvJgy6xL84twCdXnQImp7gkgAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADzSURBVEhLtZYxGoIwDEYDJ5PVxSvoFbo7uXMFvQKLK95M88eWr/QrkED7Bls7vL9Ji9iQAefciYfx/426vu8/fi6c78OVhyfm78dF3C0+NCRyMPo1IZYzNz/qAjLygKylct79y89ps0UrcuIWNWtyYDoDwIFfjBo5UJ9BjFYOzBUArRyYAyxyYAqwyoE6YI8cqAL2ysHmLcrJw1XNET/dYDXAKvfMfkIWA460hZlCsgEH5QEJaeKSl55QRVsWmSqoIQfTNdW2JQRiQ7KQkGyokwoK9TxF3nhtTTkmqKCaHIRDLiXH2cz+CCCgmDxHW1NORPQD7V2qX06izIAAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEgSURBVFhHxZYxFsIgEAWJJzOtjVfQK6S3ss8V9Ao2tvFmyifAEwLJwhKYhqRxJhvkpRMMhmE4ymWa70Q/juNHXwc53V4XuTzmO8U1O8CTG6IRIfn7fn5mBUTkhkVETI6L5IANucFGrMnBQa8kiHLLlhywNiGQUV+s8omd36LIQdIEqFDloHhAihwUDUiVgyIBeP85csDehCBXDtgBHDlgBXDlIDughBxkBZSSg+R/AUeuj3KHpICY3BzHBCY/ghxQQG5wIkgBJd+5xkZsBuwgN6iILjbCteM1Y+xRghOoJQfBcyBl7CbI/yAJEYjvFxPY8Z37qO9GJ6C2HBc2oIUcqIBWcnCoKMdGdeQAE6gij/G/CavLgQloIgcIaCYXQogfFPHeZZQaC5oAAAAASUVORK5CYII=";

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
            string attributeName = null;

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (node.Parent != null)
                {
                    foreach (KeyValuePair<string, object> kvp in node.Attributes)
                    {
                        if (!kvp.Key.Equals("Length", StringComparison.OrdinalIgnoreCase) && !kvp.Key.Equals("Support", StringComparison.OrdinalIgnoreCase) && kvp.Value is double d && !double.IsNaN(d))
                        {
                            attributeName = kvp.Key;
                            break;
                        }
                    }
                }

                if (!string.IsNullOrEmpty(attributeName))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(attributeName))
            {
                attributeName = "Length";
            }

            return new List<(string, string)>()
            {
                /// <param name="Attribute:">
                /// This parameter determines the attribute whose value is modified by this module. This must be a numeric attribute.
                /// </param>
                ( "Attribute:", "AttributeSelector:" + attributeName ),

                ( "Linear transformation", "Group:2"),

                /// <param name="Scaling factor:">
                /// This parameter controls the scaling of the attribute, i.e. the value of $a$ in the equation $f(x) = a x + b$.
                /// </param>
                ( "Scaling factor:", "NumericUpDown:1[\"-Infinity\",\"Infinity\",\"1\",\"0.####\"]"),

                /// <param name="Translation factor:">
                /// This parameter controls the translation of the attribute, i.e. the value of $b$ in the equation $f(x) = a x + b$.
                /// </param>
                ( "Translation factor:", "NumericUpDown:0[\"-Infinity\",\"Infinity\",\"1\",\"0.####\"]"),

                /// <param name="Replacement attribute:">
                /// This parameter determines to which attribute the modified value is assigned. If this is the same as the original
                /// [Attribute](#attribute), the value of the attribute is replaced; otherwise, a new attribute is created.
                /// </param>
                ( "Replacement attribute:", "TextBox:" + attributeName ),

                /// <param name="Apply">
                /// This button applies the changes to the other parameter values and signals that the tree needs to be redrawn.
                /// </param>
                ("Apply", "Button:")
             };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((string)currentParameterValues["Attribute:"] != (string)previousParameterValues["Attribute:"] && (string.IsNullOrEmpty((string)currentParameterValues["Replacement attribute:"]) || ((string)currentParameterValues["Replacement attribute:"]).Equals((string)previousParameterValues["Attribute:"], StringComparison.OrdinalIgnoreCase)))
            {
                parametersToChange["Replacement attribute:"] = (string)currentParameterValues["Attribute:"];
            }

            return (bool)currentParameterValues["Apply"];
        }

        const string AllWrongType = "5cb75b86-104a-4092-a4e7-e5572a2faecf";
        const string SomeWrongType = "18d514cf-2b95-408f-9893-86976a40f46e";

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string message = "";
            string messageId = Id;

            string attributeName = (string)parameterValues["Attribute:"];

            string replacementAttribute = (string)parameterValues["Replacement attribute:"];

            if (string.IsNullOrEmpty(replacementAttribute))
            {
                throw new Exception("Empty replacement attribute name!");
            }

            double scalingFactor = (double)parameterValues["Scaling factor:"];
            double translationFactor = (double)parameterValues["Translation factor:"];

            bool anyString = false;
            bool anyNumber = false;

            foreach (TreeNode node in tree.GetChildrenRecursiveLazy())
            {
                if (node.Attributes.TryGetValue(attributeName, out object attrValue))
                {
                    if (attrValue is string)
                    {
                        anyString = true;
                    }
                    else if (attrValue is double x)
                    {
                        anyNumber = true;

                        node.Attributes[replacementAttribute] = translationFactor + scalingFactor * x;
                    }
                }
            }

            if (anyString && !anyNumber)
            {
                message = "The selected attribute appears to be of the wrong type (String instead of Number)!";
                messageId = AllWrongType;
            }
            else if (anyString && anyNumber)
            {
                message = "For some of the nodes in the tree, the attribute has a String value, instead of a Number!";
                messageId = SomeWrongType;
            }

            if (parameterValues.TryGetValue(Modules.WarningMessageControlID, out object action) && action is Action<string, string> setWarning)
            {
                setWarning(message, messageId);
            }
        }
    }
}
