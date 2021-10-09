using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace ChangeAttribute
{
    /// <summary>
    /// This module can be used to change the value of an attribute of a node of the tree.
    ///
    /// The module can be added manually, or by selecting a node from the tree plot and using the "pencil" button
    /// to change the value of an attribute.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// The difference between this module and the _Add attribute_ module is that this module only lets you select
    /// an attribute that already exists in the tree (and, thus, only lets you change the value of existing
    /// attributes); the _Add attribute_ module, instead, lets you choose an arbitrary name for the new attribute
    /// (and, thus, lets you create a new attribute).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Change attribute";
        public const string HelpText = "Changes the value of an already existing attribute.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "8de06406-68e4-4bd8-97eb-2185a0dd1127";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = true;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFPSURBVDhPhZKxToRAEIZ3z9KOV4DCV8DGu4ZCC5/BkhYqCyPEwo6W0mew0ASaOxtpaLQygegDaGuN8w87mz3g4pdwMzs7/7+zcFoR5zeP2fPdZYZ8SpIkqN+OqzkaYorcQCZsKIg4jmMVBMFYdEjTVK3k5JPflycSDPSseXdkT1xVFYsQhRV+zMkXyImtmBRFod2T67rmGqLABoaNicCaaIIrRBRFuNaAKEzvDNF2XDEbOnGHZCC44sDvwOSMaV6c5P3rhwtC3/cc2YCaMoyGfGrycXzGBtcPr7wGEJdliTS34q7rMKUdEyfjE9PDNd4k0Id+6KTRioEIpmBvJiZwhRzjyJ3ur07xzxzePr/ZUHDHpmtaAwaO7iQQw0RYOnnG1ET4T3xkomqaZheGoW7bdu37vvI8b+9tz8Y+hDsJoikfxE4guJPQMsd63FlCqT9+QxHv4IDv1AAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAJOSURBVEhLlZa/a1NRFMfPq4voolHRUZugLtU5Lg0dEtJCOzi1Ti6WUKdk6KRGHASH4CJBZ7Ud2w6FhLaWDmbJoDhYTXAW/AsE9fn9nndvuHl9P/I+cHvP/fG+33vOvZB684+2PRGZRavuPltaR59IvV5vonsSjNKhQQn9h2AoD2HyysQnsOK1Wk0KhUIwmUCj0ZAp9NVgKC+u/P72BSJrZjxGVnELyyPIYo3i5/78fIDhXbSjVqtV4RpJE+90OtLtdjUul8tSqQSf2gyEZYH4DEKKn0YrQ3Sfa4bEk1McB/LYrJFFDQgWWfujYKTMwWSXAT/MUhaXkQExZTkIRkoVJhsMPKAzEbAs2OezMXaJ/Mic3F4+2YD5CgPcl79Sui735m7qQhKjOwgDsXl0m8FIWYbpNgPcl/f+8Lu8OzjWhTTGDCDSZJqMYbKMTstjWMTaHoPn9+/IpCYjA4qj09fiA86ZsuwwNnSxb+r29CUvLZPhcKi9Grji9rU4Jkvo+GTXj8/OfkT7i3u4wTWbSRiKt9tthk+9KHEX+3p4corrpMgqMnhjD+HiiuNwTWYQKb7weEfTpwjB5n+Y5tNZRXvNPWHC4jrJDND8wWBgpAI+//jl80m+3f+qY90cQhcM/J46piLjTGoSR5L4Kf7p9XqHxWLR6/f7pXw+L7lcThcvnz8jM1cvyMutTzyu3Lp2UeddIssSR1omYRLLEkecSZhJxbVELnHlcslSlhMGJMkkU81BpAGJMskqTmINSNgkqziJ/RFxMRfJf1UyiYuI/Acrj/J+e4Qz+AAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAJrSURBVFhHrZaxbhoxGMd94RX6AhEo6pSpCxtkANRUyt4ueQboWhWUNeIp2qRrpbYCpDYkC0v3Vq2Q+hKdyfcz9skY4/Pl7ieZz/7udP//ffbZZMrw8t3nLxKuv11dLLeZdIbD4VjC++2oHNqAET+nL3TLmKgiDpknPhFxHpiEK97r9VS/36ebxGg00vFI2rXuGXEeKm0jrWPyQaqIu9gp6FB296GG7nQ63ZuOFPH5fK4Wi4UZ7d9nK9Dg5+/Dp3/Edrt9R3S4lNz9arXS16Gk+Fdpt9L+r9frExKtVouQm2MKXLomutx501FYdisu1XslbUxk7FbEsmPAlDtqQu7JEuf8p4kWf6zxK5BkYjAY5PtHhBcmWvyx5uCDjJi/JiBfmBtBZzy8NcCbI37uVs79DIOkVCITPv74rZMuiCAmsL+wZnbEXQpLWVQJ+YQ3rzsn6s3Zc5NOI1oBEdWbEf0DlZjYaZD9I7tZ/lGhSqSwZwBxCfpTm81mIROIj3lzNjASVKCMCdaIZceAK27nzC40a8KKkxPe8kP5U014O+QkNxASt3gmgGqwwu0hlmTCF+dl9CKMifuw8k03xxoExDHhL8yQOJ2jmHjoTRD78P0X86+jKw62Eini0LAHUEicNxFFdXr8zGS36LHko9cNMXHAgE40m838pIIkkch1KBIHDDCnHTkudaIuEyni0JCzflmHidQ599F/SGoxYSgjDtoAVDUBZcUhNwBVTDxFHHYMQKqJp865z54BSDJhqCIOQQNQZAKqisNBAxAzUYc4RA1AyASxDnHgwUm4h5ZDJXEorIDFrcQ2U11cKaUeAUPOIVFB/1rUAAAAAElFTkSuQmCC";

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
                /// <param name="Node:">
                /// This parameter is used to select the node on which to change the attribute. If only a
                /// single node is selected, the attribute is changed to that node. If more than one node is
                /// selected, the attribute change is applied to the last common ancestor (LCA) of all of them.
                /// Nodes are selected based on their `Name`.
                /// </param>
                ( "Node:", "Node:[\"" + leafNames[0] +"\",\"" + leafNames[^1] + "\"]" ),

                ( "Attribute", "Group:3" ),
                
                /// <param name="Attribute:">
                /// The attribute to change. If the selected node does not have the chosen attribute (which
                /// must exist elsewhere in the tree), the attribute is added to the node.
                /// </param>
                ( "Attribute:", "AttributeSelector:Name" ),
                
                /// <param name="Attribute type:">
                /// The type of the attribute (this can be different than the current type of the attribute).
                /// </param>
                ( "Attribute type:", "AttributeType:String"),
                
                /// <param name="New value:">
                /// The value for the new attribute. This should be coherent with the [Attribute type](#attribute-type),
                /// i.e. a text string if the attribute type is `String` or a (decimal) number if the attribute
                /// type is `Number`.
                /// </param>
                ( "New value:", "TextBox:"),
                
                /// <param name="Apply">
                /// This button applies the changes to the values of the other parameters and triggers a redraw
                /// of the tree.
                /// </param>
                ( "Apply", "Button:" ),
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            if ((string)previousParameterValues["Attribute:"] != (string)currentParameterValues["Attribute:"])
            {
                string attributeName = (string)currentParameterValues["Attribute:"];

                string attrType = ((TreeNode)tree).GetAttributeType(attributeName);

                if (!string.IsNullOrEmpty(attrType))
                {
                    parametersToChange.Add("Attribute type:", attrType);
                }
            }

            return (bool)currentParameterValues["Apply"];
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            string[] nodeElements = (string[])parameterValues["Node:"];

            TreeNode node = tree.GetLastCommonAncestor(nodeElements);

            if (node == null)
            {
                throw new Exception("Could not find the requested node! If you have changed the Name of some nodes, please select the node again!");
            }

            string attributeName = (string)parameterValues["Attribute:"];

            string attrType = (string)parameterValues["Attribute type:"];

            string attrValue = (string)parameterValues["New value:"];

            if (attributeName == "Name" && attrType == "String")
            {
                node.Name = attrValue;
            }
            else if (attributeName == "Support" && attrType == "Number")
            {
                node.Support = double.Parse(attrValue);
            }
            else if (attributeName == "Length" && attrType == "Number")
            {
                node.Length = double.Parse(attrValue);
            }
            else if (!string.IsNullOrEmpty(attrType))
            {
                if (attrType == "String")
                {
                    node.Attributes[attributeName] = attrValue;
                }
                else if (attrType == "Number")
                {
                    node.Attributes[attributeName] = double.Parse(attrValue);
                }
            }
        }
    }
}
