using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

namespace ad46f971574534d3aa498caf69e7808bc
{
    /// <summary>
    /// This module assigns to each leaf a number going from 1 to $n$ or from 0 to $n - 1$ (where $n$ is the number of leaves in the tree)
    /// and stores it on the tree as an attribute with the specified name.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Add index";
        public const string HelpText = "Adds the index of each leaf as an attribute on the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public const string Id = "d46f9715-7453-4d3a-a498-caf69e7808bc";

        public static bool Repeatable { get; } = false;
		
		private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACdSURBVDhPvZLhEYAgCEata7BcyfZwhFapzQxIOUT0vH707sow+ABlcYIQwg7LFWM091+L8OBz48dGJmA4SSpR8E2wkL3iK9NkLqhgTMSwQC9YIqr0tAHICobI4NI/MiXQC0b0wVSUtkb/fuY8Ej2C0Rw0/VqwANAdlhFdBxQgQVWyprlGDMzZeVg+kUVqjEPkCsyACeQheiUy0YJzDyNPUJ1kouRxAAAAAElFTkSuQmCC";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJnSURBVEhLvZXPa1NBEMfzo4n1kEslwX/AVrQgqIec1Hu9iQFBjO1ZMT80Bw+GIAqSvCQQCIIQPEgLVVEQvCgI9eChqCDYogdBRMGICjaHoPnhZ95u22yfr5pE/cLwnZndnXm7M7vP63FBKpU67PV6v1mWtaBdDiSTya3QHuZth5eKxeJre6AHPs1rIPAxZAn1XqfTmVFeJxKJxBT0juBPut3uNfRXJLTswR4YCZjwBpplwbLyuIPAh5BaNBodKZVKYdZcxubbUuN6io2NO7iB7GXBEWW6gznnOJJMLBZri02C25p3C6/CSMCiCyx6rs2+4PP5wsIkMOrgqMEQOIE8KpfLL5Wp8FcSpNPpS9BRdpFQnnUMnUCC021nUacKhcIL5V3HUAkkOGd+nu45Tu0eaLcB46LJHdCqQNp1gcVXxSDAnO3VoKWvMJZhznX4vnbbYO5NrTp2IBdrVov0+gGtl8TuBWOnNZ+E5jfI/4PjLeIJ2M9XjSMf/X7/cj6f/6CH3FFLd22esRzxjCOiBmdotUWC5zAfttvt93LWanQwGAn44looFNpCkXY0m80Qrlsky2Sz2RE1o38YCTiOlVwu9130arXaoEMei95oNMaEB8Gm94Cv30WSRf4Jde3qG46iUIeL0DSBt5HgKzzJI/hFjYLVgv4hHDsg6F2CytU/BT+F33Jj99mD/wJ0kfypqtr8NWRXLjvbtAYaHWRFqf3DaD/OfyfHUm+1Wv5gMBjmlZzmyCaQga+/sQOC3yHY50AgUEd/hj5JkoN0kdRiIBhdFI/HRyORyBiX7EelUvmk3b+H61Ph8fwE0ATsYzdO6vQAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAANVSURBVFhH3ZdNSFRRFMedSUYrDYSgcKFFgkFFLgwXRa0saBGFQdCHi2xTNuM4o6hRiiZF4Hw4MUhEQQVhBS5aBNGidRDRIvqgKKOyJIaiWZSOjv3Oe8905t2ZeY7PTX843HPPu/ee/z3nvDNvHAVZ0N7eXjo9Pd0/A0KhkNcwZwV7yqempvY7HI4apk7kJduH2P9bW5AGWaBEa2vrUQ56jepBDmjGHPD5fCfZ8xnnUZwextSEBJBXnLdF1qTDRMDtdhdx0F0OucX0k9jQl8mYCzitkH1ILTcuicViyzGfZV6JXNFXpcKUgpaWlhqn0xllQ18wGHwImRnMX9HL9RULB2f8YCjijBW6ZQ6mCAwODj6H/XZxbpjkZpYikAnsFwJ/9FkqMtbAfBCNvAm0tbWtZf96SDwzTCmwRADkTYC36JSMkHigGdJgiUC+KfD7/XU4Pof6mJQGdWsqrEbA6rp/wHklxIcl/5CQV1mJJamBzs7OsmQyeQd1HXsPBQKBF/oTM2yvgZ6enpKJiYn7OJbwNxD5R8YjJWytAWli8XhcnO8gAl3cfMR4lBHK3wIah5thJSKO+8UGvBBJ0KTiHCxd0gT23WM4iLxj7RPGn5D5Jc8EkHoTDodvGFMNpgh4PJ4KhghyEZl1LghzmPT43cZchTXGWMXaI0gzetesMK+Th/OhjAB5zJia3t7epKH+H8j6PbAoXPfLj9gcjgeUvpRGaSIUjHwDbCJvLvTRwsLCoYGBgW/6CguwSMCUaz4cmii0URyHmO5Eb6TyuyHxwev11uqr7IOq2LYil1wu12p+lqsRYS5vRDFEzssCO2GpBuQ7j1+1L6jv6WwbdGsO5JsCFRKJRJmMpCOmGWyEpQhQFzepiWOop4lAVLcaSL/pAqEkQEtt5rbStUpxvJmxCunGubkGFklAmQKcl+G4GlUci4htF8Q2im4nLKUAxycYrkJibHJycls0Gh3Tn4ClSIEKkJBPqz7EQyoua8ZssPMtEHD7j4YqfzZsg4kAN6031BRQE3tkpCM+1Qw2wRQWCMS47VvUcZzeZpQPk71IA/YROmMDem7kmwKcnMFxMbKP6TByDanHfoFW3Chr7ETGIuzo6Kjgn+4quuD3SCQybpitw1IECgr+AnD1UA9AFvbWAAAAAElFTkSuQmCC";

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
                /// <param name="Attribute name:">
                /// This parameter determines the name of the attribute where the index value is stored. If the attribute already exists,
                /// it is overwritten.
                /// </param>
                ("Attribute name:", "TextBox:Index"),
                
                /// <param name="Order:">
                /// This parameter determines the order in which the leaves of the tree are numbered.
                /// </param>
                ("Order:","ComboBox:0[\"Ascending\",\"Descending\"]"),
                
                /// <param name="Start at:">
                /// This parameter determines whether the leaf indices go from 1 to $n$ or from 0 to $n - 1$. 
                /// </param>
                ("Start at:","ComboBox:1[\"0\",\"1\"]"),
                
                /// <param name="Apply">
                /// This button applies the changes to the other parameters and triggers a redraw of the tree.
                /// </param>
                ("Apply","Button:")
            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>() { { "Apply", false } };

            return (bool)currentParameterValues["Apply"] || ((int)previousParameterValues["Order:"] != (int)currentParameterValues["Order:"]) || ((int)previousParameterValues["Start at:"] != (int)currentParameterValues["Start at:"]);
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            int order = (int)parameterValues["Order:"];
            string attributeName = (string)parameterValues["Attribute name:"];
            int startAt = (int)parameterValues["Start at:"];

            List<TreeNode> leaves = tree.GetLeaves();

            for (int i = 0; i < leaves.Count; i++)
            {
                int index = -1;

                if (order == 0)
                {
                    index = i + startAt;
                }
                else if (order == 1)
                {
                    index = leaves.Count - 1 - i + startAt;
                }

                leaves[i].Attributes[attributeName] = (double)index;
            }
        }
    }
}
