using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

namespace CopySelectedNodeMenuAction
{
    /// <summary>
    /// This module copies the currently selected node to the clipboard in Newick-with-attributes format. The node can then be pasted
    /// e.g. in a text editor or in another tree viewer progam. It can also be used to copy to the clipboard the value of an attribute
    /// from the selected tips or from all the selected nodes.
    /// 
    /// **Note**: this module is a shortcut for the _Copy selected node_ Selection action module (id `debd9130-8451-4413-88f0-6357ec817021`)
    /// and requires it to be installed to work.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Copy selected node";
        public const string HelpText = "Copies the selected node to the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "650a0ef5-5322-4511-ae86-68bd87b47ecd";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Copy";
        public static string ParentMenu { get; } = "Edit";
        public static string GroupName { get; } = "Clipboard";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsSelectionAvailableProperty;
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None ), ( Avalonia.Input.Key.C, Avalonia.Input.KeyModifiers.Control ), ( Avalonia.Input.Key.C, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt ), ( Avalonia.Input.Key.C, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt | Avalonia.Input.KeyModifiers.Shift ) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = 5;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>() { ("", null), ("Copy subtree", GetIcon1), ("Copy attribute value at tips", GetIcon2), ("Copy attribute value at all nodes", GetIcon3) };

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC0SURBVDhPlZNNDgIhDIXReDpNPAUngLWzcA8n4BKaeD6lnXaGn1LGLyGPxRQer9OTyTjnvqAaMUb8VgQO0NAuOJNW3Jb3tmZcSJGy4PO80+4A/ITr44WaMdKSqByUeO+7inwR7VYg2OEBQAiBdj35AtQuxFFwo2ArB1Jws2DFNrZwoeqA3wRIb/+rrYDQ2gr+O9UuMGAbHJQumekBrXVrLWpKCfVQiC1QzNM5HFFtAvfRNuYHBK2WCHb+5NcAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVEhLrZZdDsIgDIDReDpNPMVOMJ71wfftBFxCE6/CdZQuHYFCKSX7EkIT1vWf7WQC8zz/YNewruum2wUY0KBx6Ix7ldvzU6whuAiujzdKOcMRHOItoUjR93WP6wiaNQAwEuiYuJZl6e6gC+5V0iistVneQx1Q4omtvBeZK2pguJXFFEnQxsCURtgUpQ9KBW+dd9dgh3ooUaSoFiYFDO9LIosgVdB4Sp9N39NMEShKXkrnbBeBoqTcQ4wgDBJKJWFyUdKzGZA+HuG8OrlcnYYir017Y/LjJDeLzFHznEux2kAaPjU0TRNKxjjntv2Qu4gCL1f9FADSbQrn3vvic9pthSpy5J4b8wde12Qk6SWxrAAAAABJRU5ErkJggg==";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFaSURBVFhHxZZNasMwEIXlkrOEHqNk2UAO0G59AnudLrq3yQG8TRfdteBVoWSRIwQTeohcIZHMyCjWjzWSxv1gkBbGeRq9eXHGOEVRXMUaS13X/fvQCAGxhB7iAdZ/w0vA/vfM1m/fxorFS8DH4Q92REx54Hn71ZeLZB4wtZsSTQB5u0dYPdC+b4aixMuEErgOETZaVVUlVjQLWJ28PC3vrqYsS6PhuBFh58eQnOoUeDg+aXKiroACEgGY5CQRgBll0itolVGWNSZawO7zONlmF1YB45faXvxzusAuDE2AmPkQXG12oQXR6+qxrxSYujYWSGJCTBeHDvB4hZ0bnvmws5Oyi4ynplbI6DbiHcVZlmmVEhIPYAg6jmif9IJ0um38XN4Sf8ezdCDPc60kyQSoiSnLRtM0w8dItABscqo/Hgzmi0g823VdX3L0VGabAtvJg6cAtt6Y287YDaOfAyHHYnfQAAAAAElFTkSuQmCC";

		private static string Icon1_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABiSURBVDhPY6AUMEJprMCrduN/KBMDbGv2x6sXDPAZAAMYpqBrIsomZIBsADEuYIHSOAHJLiLGVmTAiE0Dyf5GBqS6gOQwAAG8LkTXQMhFBNMBCNA0TAiajMsAilxFRcDAAACgFi/GiyJfpQAAAABJRU5ErkJggg==";
        private static string Icon1_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABzSURBVEhLYxjygBFKEwW8ajf+hzLxgm3N/iSZCwfEWoAMmKA0zQBOr+ByLdneRwfYLBh6QYQeHPh8QHLQkRMc2ADtg4iq3iYFDJ0ggtIYAFsqAgGqBSkug0gNusFXFoHAoEpdAxdEuAAxPqBpEA43wMAAAK1kN8yp/tZ5AAAAAElFTkSuQmCC";
        private static string Icon1_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACsSURBVFhH7ZZBCoAgEEWt47XpRFHRidp0vbKaFgn2dZrStAcyiAuZPw9LZU9B1ZuqGTtd2n3nzjTUpztLqhy8LxdFJzCvi7Zs7iQgAnQAzdqcqS8uCcQ566wc2Lo0Z+3aPXLkTgI91WeRmrWN8A5IzZJL8AQg6TtA1crRvc0BfQ7/C678kUjg2W8FcgCdI77jAIL7Trgk8M6bzyV9BxC+3ZuuSCQQtyM/kaPUAsDKTpICQiT1AAAAAElFTkSuQmCC";

        private static string Icon2_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACXSURBVDhPrZNRDkAwEERbcTo/7tJ/Qfz3Ln5cj53Nkk3VqtRLmh0SY2eCdx/phnWiMdKZt6WfTIMQwi6SiTF6MrjukYFvRT+Ch0SezHR4A1zcNsi9VWQZ2kBrZMf60sFFI/MRZYK19WRMA6yvInBmNRmfZgafc2tyhie5Hoo6sHp4NQBWD6/fAfi1k7SHoghWD9X/QiXOHaKXUaEMtC8DAAAAAElFTkSuQmCC";
        private static string Icon2_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADKSURBVEhL1ZVBDsIgEEXBeDo33oW90cY9d3Hj9ZSffJq2zgBDMdGXkAGazOcPk+K+jWccwunyuKVwTWN63s+Y2wRCCC9OP4gx+iQwf08C9sOXBAAcQIRO7NQEJFQbWjKUgtN9SAI9Dg6M3dTqbhaAizy4hbZcxjZay1FzoNJTbwlfSjSsYyRGOdjdRaB0D0fGZrbOWMZlJ61EuhwgaR7cmjaxjnYH1rsx/4vAT3XXkC7KSN1ktqo5QNmQnMv5RRtaS5589Sb/O869AdWXcOk0wEu7AAAAAElFTkSuQmCC";
        private static string Icon2_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAEVSURBVFhH7ZZBDgIhDEXBeDo33mX2RifuuYsbrzfysSZEKVTaxo0vIcgshvb3t2P4NZF2dw6n2yVv5+cprPfrEef5AJZlqV/YJaUUcwAbHQs5gHL3rpzmEF0+QqNAyQjZlQcDuBJMgwBeQWgYRj+qtVQBDokHTGo9DSe1VQk0XdAFpkPr0WINJ/FA0+297Ht9/45GgZV2FdMKjCDZ9X1vZTaOKH25tt853LrADO8SuCoAI9IcYGfBnvZpsjrNbwV5pn6O3x9BWCig+laYlQAZ14se18OqObhcPYDhgxFMq+kB8SQcUWX9FRIFTGa+G9o54eqBGm4mqOe7JHv4AxfTsQBjYrdQQOURtQJSSHb9/4M/toTwAPYQkgshHGVKAAAAAElFTkSuQmCC";

        private static string Icon3_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACzSURBVDhPpZNLDsMgDEShyum6yV3YV02UPXfpptdLPJaNTA0JUZ9k8ZPxMHFiuMnz9VloeFOs321eHrx7QUppR8gSyWWceNrAVsrbbJWuFLzPq1+0El2wa/BBh7MnaAUeoUguhLLCsIlWCZlX8ioFvSpCpUiJ+uacc+xVOWNCoswBbu86LMr4HD2AvWEPQEuh+wqmYVqeOB+cgrueuE688kTUVD4M0evQoZ8JGGXOhz8I4QCVRWeniwbFsAAAAABJRU5ErkJggg==";
        private static string Icon3_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADLSURBVEhL1ZPBDsIgEETB+HVe/BfuRhvv/IsXf0+ZZtKQsgtLWxJ9CVlIU4aZBTcaz3oIl9vrkco9jen9vGLeJxBC+HBaEGP0SWD5ngTmvX/HgfSzBVUgj0Ozb+HEKoKNMbicVnUftYbmIDq4Y4QFZ1YTa1G6Q18AaiFizlKj1XxRoPVTD16yjUy57LoxEuMdsO6idiBVANFlb2DhsEit7wDAAUToxIYksGWjakSczmyNwpYX4cm7bleXQAvpAN0CWvO1CIc7+Hec+wKbIIohffvOSQAAAABJRU5ErkJggg==";
        private static string Icon3_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADuSURBVFhH7ZZBDsIwDAQTnseFv/SOoOKev3Dhe5CEFFkFJ3a8oUJipCp1K9Xb7Naq2xpf1uHsj9dzXE7Pys23yyHV/QKmaaIPrBJC8FHAvZSZKCD33uWqD1HzFr9pAfewHpoC1l7X/OxBkgGI1xziEKY3T0cp57Im6LkaiQV5u0lzEdKcdAtYrn9CkxPLHDBt/YJqWzVALEB+7xye81LjowVLBiBsb0FZhyB5gaaA1iCK92mTF9IMITLw1lyDeQdqDLcAEVKxgDWoOSHJAGTmc6gVU75iAQpOrFkAlxFKLS+IOTD2lwwFIi9/BuDcA32+lH1pIG+BAAAAAElFTkSuQmCC";


		public static Page GetIcon1(double scaling)
        {
            return GetIcon(scaling, ref Icon1_16Base64, ref Icon1_24Base64, ref Icon1_32Base64, 32);
        }

        public static Page GetIcon2(double scaling)
        {
            return GetIcon(scaling, ref Icon2_16Base64, ref Icon2_24Base64, ref Icon2_32Base64, 32);
        }

        public static Page GetIcon3(double scaling)
        {
            return GetIcon(scaling, ref Icon3_16Base64, ref Icon3_24Base64, ref Icon3_32Base64, 32);
        }

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon16Base64, ref Icon24Base64, ref Icon32Base64, 32);
        }

        public static Page GetIcon(double scaling, ref string icon1, ref string icon15, ref string icon2, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon1);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon15);
            }
            else
            {
                bytes = Convert.FromBase64String(icon2);
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

            Page pag = new Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { window.IsSelectionAvailable, window.IsSelectionAvailable, window.IsSelectionAvailable, window.IsSelectionAvailable };
        }

        public static Task PerformAction(int index, MainWindow window)
        {
			if (window.IsSelectionAvailable)
			{
				Modules.GetModule(Modules.SelectionActionModules, "debd9130-8451-4413-88f0-6357ec817021").PerformAction(index, window.SelectedNode, window, window.StateData);
			}
            return Task.CompletedTask;
        }
    }
}