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

using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.Canvas;
using System.Runtime.InteropServices;
using Avalonia.Media.Transformation;
using System.Text.Json;
using Avalonia.Styling;

namespace Search
{
    /// <summary>
    /// This module is used to search and highlight nodes in the tree.
    /// 
    /// When this module's button is clicked, a search bar appears above the tree plot. This can be used to search for specific values in the attributes
    /// of the tree. The default is to search within the names of taxa, but it is possible to perform the search in any attribute. For numerical attributes,
    /// it is also possible to highlight nodes where the attribute is greater than or smaller than the specified value.
    /// 
    /// When nodes are matched by the search criterion, they are highlighted in the tree. It is then possible to copy their names to the clipboard (using the
    /// `Copy` button, or to use the `Replace` button to replace the value of the attribute. This enables the _Replace attribute_ module (id `f17160ad-0462-449a-8a57-e1af775c92ba`).
    /// The settings for the _Replace attribute_ module can then be changed in order e.g. to add a new attribute to the nodes that have been matched by the
    /// search criterion.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Search";
        public const string HelpText = "Searches nodes in the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.4");
        public const string Id = "5f3a7147-f706-43dc-9f57-18ade0c7b15d";
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static string ButtonText { get; } = "Search";
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.F, Avalonia.Input.KeyModifiers.Control) };
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Selection";

        public static double GroupIndex { get; } = 6;

        public static bool IsLargeButton { get; } = true;

        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon32Base64 = "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";
        private static string Icon48Base64 = "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";
        private static string Icon64Base64 = "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";

        private static string IconCopy16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC0SURBVDhPlZNNDgIhDIXReDpNPAUngLWzcA8n4BKaeD6lnXaGn1LGLyGPxRQer9OTyTjnvqAaMUb8VgQO0NAuOJNW3Jb3tmZcSJGy4PO80+4A/ITr44WaMdKSqByUeO+7inwR7VYg2OEBQAiBdj35AtQuxFFwo2ArB1Jws2DFNrZwoeqA3wRIb/+rrYDQ2gr+O9UuMGAbHJQumekBrXVrLWpKCfVQiC1QzNM5HFFtAvfRNuYHBK2WCHb+5NcAAAAASUVORK5CYII=";
        private static string IconCopy24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVEhLrZZdDsIgDIDReDpNPMVOMJ71wfftBFxCE6/CdZQuHYFCKSX7EkIT1vWf7WQC8zz/YNewruum2wUY0KBx6Ix7ldvzU6whuAiujzdKOcMRHOItoUjR93WP6wiaNQAwEuiYuJZl6e6gC+5V0iistVneQx1Q4omtvBeZK2pguJXFFEnQxsCURtgUpQ9KBW+dd9dgh3ooUaSoFiYFDO9LIosgVdB4Sp9N39NMEShKXkrnbBeBoqTcQ4wgDBJKJWFyUdKzGZA+HuG8OrlcnYYir017Y/LjJDeLzFHznEux2kAaPjU0TRNKxjjntv2Qu4gCL1f9FADSbQrn3vvic9pthSpy5J4b8wde12Qk6SWxrAAAAABJRU5ErkJggg==";
        private static string IconCopy32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFaSURBVFhHxZZNasMwEIXlkrOEHqNk2UAO0G59AnudLrq3yQG8TRfdteBVoWSRIwQTeohcIZHMyCjWjzWSxv1gkBbGeRq9eXHGOEVRXMUaS13X/fvQCAGxhB7iAdZ/w0vA/vfM1m/fxorFS8DH4Q92REx54Hn71ZeLZB4wtZsSTQB5u0dYPdC+b4aixMuEErgOETZaVVUlVjQLWJ28PC3vrqYsS6PhuBFh58eQnOoUeDg+aXKiroACEgGY5CQRgBll0itolVGWNSZawO7zONlmF1YB45faXvxzusAuDE2AmPkQXG12oQXR6+qxrxSYujYWSGJCTBeHDvB4hZ0bnvmws5Oyi4ynplbI6DbiHcVZlmmVEhIPYAg6jmif9IJ0um38XN4Sf8ezdCDPc60kyQSoiSnLRtM0w8dItABscqo/Hgzmi0g823VdX3L0VGabAtvJg6cAtt6Y287YDaOfAyHHYnfQAAAAAElFTkSuQmCC";
        private static string IconCopy48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAHOSURBVGhD7Zg7TsQwEIYdRJ1bUHAJKiqQ2I6WJidIalCy1MkJtqGlQAIBFRUSVwgF4ia8MpFjeY3txI7t2OBPGq290mpnPPNPJk4QJs/zb7y0StM05D+NAgHYxsYh7eDPYAk+gC0N1HWNd3Kunl7R9fMb3sl5WJ/gFUJFURjXgFYGpjrvguBLiKDShY7Ob4mpELsQB2EAINTjizuu+YQwAJ+EKuPvlhAN9HLafOJ/ZEAGLe5kBBuTqHCUoLsNWzaiTvR4uUps9Hoew2Hs9jtFTg/2pF1q6kylC8xUA1oldHa4/0vYbJZcEUU8B9nTnjURiwZg4mkfS8gUvKZAm4iYgTFAqKvqXkmYKlgP4OblHX18fuGdeSYFwDs92mTYdB5wqoGpwlRBGADMOyEgHOZg3gFzxVgpijLmtIRssGgAJsp064UGL7WBlwyVFyNdbNyxEiAAGt1bPBn0YUcNLE0MYGmMK3luF6JvHGQMXcjLDGRZJjWa4Epos9nAR9VvOpyW0BhQYlBC7CkPDM535VP2X3QEkwGe84D1AEzMOyLnrcCOEqrA79u2JQb7zoSOe11C1MkT0bJ4GwDjvHBSttKF8HIuVZqm67IsJbcCCP0ADHhS2AQiCN0AAAAASUVORK5CYII=";
        private static string IconCopy64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYYAAB2GAV2iE4EAAAIKSURBVHhe7ZihUgMxEIZTHAbFe/AgzFCPAFOLaT0CbFsDsh7fGRDwCCiQvAEzYGpAAd3rXqeT3jUbLnvJXvabyWyuor3d/vmTTc8gw+HwF6dJMJ1O1+/GyR7GbNlSwGQyKZ5jMRqNiqgKaAktAMZsCeYBz2/v5mb+Yj4X3/gJjfurE5ytEOsBt/NX7+RTIFgBPhZfOJOFegDGxh5wfDnH2Qp7bVPRc0DLOBUQyt2pJKcAqe5OxVkAqe5OJXsP8C4ArO2qIRVVAMbgwLkARs8TcP+2dgAgewU4zwHUE975+Klyx3i47he/UX5/KpQqC6aAi/6ROTzYxyc5BFNAHbCuITbtNUJhnzR1F8CYLboLYEzeA7juHMUogKsrFVMArq5UTRCjOGBtU4YLVQDGaIC7n40f191j3eAiegFi3zl6F6Dq36kaVGLfOaoHYKyl7Ra3yslhcOEsgNQ+n4qzF2iKqxew/aLu3/bxlU3s79P7AAsxBeBahmIKwOVFWx4QmnKtcd03+KIeYLFWADeqgETRAmDMFi0AxmwRvwuUru6L7gJIZxQwGAyKuIvZbIazDBWwmfyyQz/FaXoKoFIqhaIAO/nlO9zhY/cVsCt5IHoBOK/bXMkD0QvA1edTkgeie0BTqjyAmjzQOQ/wSR7oVAF8kwc6U4D/JA+07gHc+CQPdEYBy8R/fJM3xpg/2jBVAuytoXYAAAAASUVORK5CYII=";

        private static string IconFind16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADRSURBVDhPYxhwwAilsQKv2o0pQCoAiL2BeCsQb9jW7D8HSMMBTgOAmrcAKe8oBzUGEX5OhjcfvzMsO3ALJLUVaIgPiIETgGwG4v87zjz4jwxAfJA41GVgwASl0UEAyGZ3Y3koFwJAfJA4EIC8BQa4DPAGORsbgIqDwgQMcBmwFeRnbAAqDgpQMMAwAOg/BxANCrCdZx+CxWAAxIcG5AawABCgxAJU834IDwIIxQLcACyawfEOxITTATbNBOMaCmBhQJZmEIAZ0AijSdE8GAADAwCtYGGYF7sCvwAAAABJRU5ErkJggg==";
        private static string IconFind24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHPSURBVEhL1ZTBSkJBFIaPmqQYFOKiKIKgIlq0qXZRRLsQXFRUkA8QPUCrVq3qAeoBWmTkJpA2IZTrRHAVtZBCKsoCC1EwqPMfx7iGtw6liz74Z7wzZ85/nXtmqNk4TK9mduNohbsp1hhriHXBOmfFjzdDEe5rUBtw4h7udllBGahPjLXKRtnKo9LAJE+xAj5PCy1ODtJIX4C6/D66ey5QOpOjg8QlFUpvCL9njVdNnGgU4M0Dw71+2lmbprmJfhro7qA2r1t6PGMc80yniRd+NDB7HsSbry+MUqDdW5n4AsYxjzgmyOuW8EPzD/BBZVvsklfBPOIMM2g0BqgW2XMNljhZpzFAKcoH1WCJk3UaA9S5VIsGS5ys0xjgEEkparDEyTqNwRka1HkuX5QBOzCPOEMcjcagxCrjEG0dJm1NMI55c9hi1WvDhcYOruV57vZZbtb7Y77oOEndkNPhILfLSa1uF10/vNJpOkvb0STdPsn+4ySHrhKRFzzYXhWW5HJyDGUWzOzQ3UU2yfHfl1ke1u9v0++Sc4Jo5VFPjUGjk4NPg2YkB2LAyXHFZljY3yp/Tg7kHHASlFaYhaSgIclBvW+wxwo3InldzHb9F4g+AM4TrApskYuKAAAAAElFTkSuQmCC";
        private static string IconFind32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAALDSURBVFhH5ZY9aBRBFMff7eWKgBdEY2KRaJAUQRBSaCEEgnZ+YdBCRAIWWlhbiKggaNDGysIiEAQLFRUVgsZGQUQFP0gjBIxijIXfJJdI0Mvd+v/Pzk729vZu55YcAf3BYz52Zt7/zc6+WfnvSekyMdtP3e1GQVutOkQ+w0bvndk96jWrk1gAHB9CcQzWqTrKGYcNQMhlrxlNzQLguBnFIKxPdcQzDDsMIdyZMmoSAOc9KG7CWlUHyDQ4srYlK13tK1R7bPKnTHydkfx8UbU132H9EDHiNRewFsDIUyl547rSwjYd92/tkr095W+gUCjKnWfv5crDsaCQKdgGiPjkNT0cXcaSdlJDvnNGfPFIb6Rzkk476hnHtDUv072yHHbJqy5gJYAHrlB0d7HOyI/v2yjtq7LqWTU45uT+TWqOZifWOqjrCisBiP6Erqptt3Huw7GcE8CsRWIFQHE3ou9gnZFU2vZq9G1eF9yFTq6p61Y7YAbz3SeBZyI0tyYBfoYzn1oSQnPNmlZnoJ7YCDAZjEkmKaG5Zk0bAeZSYYZLAhNTaK5ZM1YAb7VsY0bJZ1a79YR3TG0wKwYy4jjX1HW7M7CyqfGCrqr0OvnNfic4lnMCDOhSYSXgw5fcUCbt5FlnJOeuv7QSwTFnr74IRj+M6Euu57QuK4KkwU/mUdF1zQ04/euPPHj9USBK1q8p/zT5zm8/fSfnb7ySqdnfulemYTvePr6W85oeVW9D3zmsJJcGYYaLu45xi+ZwkR1A9Pw3KKGigGrOcTcgSDd29wjGjmAsf0hKrmGfSAExkfMb3tLR2rTnR27u6MxcPjI9Njipifmiezr8zsOUCbBxjkXNscb4xfsprdX5YmAELIVzogQslXPiJyL+cLR51RLq6pwoAXDwHMU22Czbmro7J+FDyP/++zAKqbvzSCgCFnUW/kVE/gKQphKx9drpvQAAAABJRU5ErkJggg==";

        private static string IconReplace16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEdSURBVDhPxZIxToVAEIbnqT0cgJJ4BRIaHgXNw8TXcANvgK1RY80NPIQm8hoK7OgsuYE9N9D/X2aRp7sxVn7J8O8szOzMsPLvbFSd1HV9BdnDSlgLe2qa5hG64E2A4BdIWRSFhGEo0zRJ13V81SLJBRfkVPUIPbmuqkqyLJMoiiSOYwmCQMZxPE/T9H0Yhjd+e8KHgz1PTpJE3Rn63Adsy+BLULJsy2aF7nMmBl+Clj0TBu1unrewD/q6z4EafgwR/W8h17ASwzLBWPeHh0vnwM9UDRrcz96C8W0F31kSOIJtmTnMW4GZgSvY/msEvkLydQVca2vzDJBgXd7RRfGhCXPbwj3slorgO7Oj+HpXeu9V/g1bwez9ka8ZiHwCi2lj6xV23vQAAAAASUVORK5CYII=";
        private static string IconReplace24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAISSURBVEhLtVS/a1NRGP2ifZCAkEA6ODgkEB5mk2CXLG3jJkIXF0H3dM7iIiqdM/cPMKUduhRKt6Y6ZYjg4mAhYAgOLoFsEQzoOV/uDfelSfpR2gPnfvfd+91z7s8nd42Ui2Y0Go3XCJvgU/Ax+AP8Cp43m80jxATMBhB+hLAPvtCGxTgFd2H0a/ppNHDi38D1TCYjtVpNSqWS5PN5GQ6H0uv1pN1uy3g8ZvpvcMOb3GdxHarV6gHCk0KhIPV6XcrlsmSzWYmiSGOxWJRKpSKDwUBGo9ED5MadTueQY681cHv+ljOneC6Xm3bMIZ1OSxzH0u12ZTKZxJjUJUy+33P9q8AD1W1ZJu7BfuY5PGNhMeBt0T23IMjTcRYDXkU9UAuCPB1nMeA919tiQZCn4ywGfER6FS0I8nScxeALC95zj9QcXDOvaJh3zsJi8Af8y0fk9FTw+buTLfAf62yjeKvV8o/t1P82VhrgDbxE4IOJQBUjKI5wAX7UBgCC0u/3WeVL3mWFWPqrCMTXtAGASCoUP9vb+aAdK7DQYJE4MIFB5LfFilBAsUwcfDWtyjZoXkHiDFaJY/bH/IDoZwSavMdqrhhwhW4bFTMDi7hHaKINSfDgL7yJngHEHyL8BNP8dlgoboFbGc23Z4c8t4Kl4twCVzUhcYucySfwzU1m7hGuQBtCuO26MSjOVYKzg75VJMVF/gP4882RY7JnqQAAAABJRU5ErkJggg==";
        private static string IconReplace32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAALBSURBVFhH5Za7qxNBGMUnD5uQwkLyACEWQSwUbh0kmCpoo/9AOiV1SEBQxCtyrRJSWgjiH2AjiJoqEoRAqtsIFreJCEkhYmGZh+fMzs6d7G52JxtT+YOPb2Z2Z8+ZZyL+exIqx6bdbh8hMQqyQYg54rTX65061XBiG4DwfaSHiLJs8HOGOIGRN041mJ0NQPgS0ivEPdkQzXvEAxjhzPjYyQDEbyK9ReRlA0in06JQKIhSqSTr0+lUzOdzsVgsZF3xE9GAiU9O9RxrAxx5IpH4ul6vc6xTuF6vi1qtJp+bLJdLMRqNxGAwMI38RtyAiR9O1SGpciTJZPK1K84Rt1qtQHGSSqXkM76Ty8ku5CLipVM8J6VyKNxwEO+wzJE3m02Rz+tV2Eo2mxXlcllMJhOxWq3YdLVSqUzH47E+IVYzgNE/VkU57TbiLnyXfQz0t0ikAYz+CO6vsMzRb5v2MKrVquyrKPObqmw1A/plrn0cuCc8fXcyoHu6Ry0Onr76m9anIAocUR/qUSg2BvQNxksmCFfszpN3txBrxLF8YODpq79pY0AfGd5wXkxxpCHL4KnKGk9f+2PIX7VMJvOLZd5qw6GrsVWc+I6KcSOe8ZuqbHcVd7vdR7PZ7IRlHifecDzfNBAk/uH53c+qHImVAZzbAoS/YxQXWOeRajQazDSwli/FJHIJKI40dMUJ17Pf76vafoTOgCuOuCYbPGAt916CrTMQJo7fhmWxWJR3uhIzN91QmbIi0EDEyOf4bbje6XReqHqgCZU1MHXM/eI15zMQJY6oYeq/OdVzPCaeqWzi3g0bM7SxB+KK26BEzZmRe0UbOKS4S5AJaWAfca6rKsbC3QP8w3HZKW7wT0YehrkE/Mv9EZGVDQcQ37oELoaJP4iDi3MTbhxDCH5Buo04xLT7xFnYmIFDghngnxTeBcZVLcRfqxUXRDlgaV8AAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Base64, ref Icon48Base64, ref Icon64Base64, 32);
        }

        public static Page GetIconCopy(double scaling)
        {
            return GetIcon(scaling, ref IconCopy32Base64, ref IconCopy48Base64, ref IconCopy64Base64, 32);
        }

        public static Page GetIconCopy16(double scaling)
        {
            return GetIcon(scaling, ref IconCopy16Base64, ref IconCopy24Base64, ref IconCopy32Base64, 16);
        }

        public static Page GetIconFind(double scaling)
        {
            return GetIcon(scaling, ref IconFind16Base64, ref IconFind24Base64, ref IconFind32Base64, 16);
        }

        public static Page GetIconReplace(double scaling)
        {
            return GetIcon(scaling, ref IconReplace16Base64, ref IconReplace24Base64, ref IconReplace32Base64, 16);
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

        public static List<(string, string)> GetGlobalSettings()
        {
            return new List<(string, string)>()
            {
                /// <param name="Search highlight colour:">
                /// This global setting determines the colour used to highlight search results.
                /// </param>
                ("Search highlight colour:", "Colour:[255, 255, 152,255]")
            };
        }

        public static void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            if (window.TransformedTree == null || window.PlottingActions.Count == 0 || (stateData.Tags.TryGetValue("5f3a7147-f706-43dc-9f57-18ade0c7b15d", out object searchTag) && searchTag != null))
            {
                if (window.TransformedTree != null && window.PlottingActions.Count != 0 && (stateData.Tags.TryGetValue("5f3a7147-f706-43dc-9f57-18ade0c7b15d", out searchTag) && searchTag is TextBox previousSearchBox))
                {
                    previousSearchBox.Focus();
                }

                return;
            }
            stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = true;

            Colour highlightColour = Colour.FromRgb(255, 255, 152);

            if (TreeViewer.GlobalSettings.Settings.AdditionalSettings.TryGetValue("Search highlight colour:", out object searchHighlightColourValue))
            {
                if (searchHighlightColourValue is Colour colourValue)
                {
                    highlightColour = colourValue;
                }
                else if (searchHighlightColourValue is JsonElement element)
                {
                    highlightColour = JsonSerializer.Deserialize<VectSharp.Colour>(element.GetRawText(), GlobalSettings.SerializationOptions);
                }
            }

            SkiaSharp.SKColor highlightSKColour = new SkiaSharp.SKColor((byte)(highlightColour.R * 255), (byte)(highlightColour.G * 255), (byte)(highlightColour.B * 255), (byte)(highlightColour.A * 255));

            Grid searchGrid = new Grid() { ClipToBounds = true };

            searchGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            searchGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            searchGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            Button closeButton = new Button() { Margin = new Avalonia.Thickness(5, 5, 10, 0), Width = 32, Height = 32, Background = Avalonia.Media.Brushes.Transparent, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Data = Avalonia.Media.Geometry.Parse("M0,0 L10,10 M10,0 L0,10"), StrokeThickness = 2 } };
            closeButton.Classes.Add("SideBarButton");
            Grid.SetColumn(closeButton, 1);
            searchGrid.Children.Add(closeButton);

            Grid row1 = new Grid() { Margin = new Avalonia.Thickness(10, 5, 0, 0) };
            row1.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            row1.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            row1.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            row1.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            row1.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchGrid.Children.Add(row1);

            row1.Children.Add(new TextBlock() { Text = "Search:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            TextBox searchBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 2, 5, 2), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
            Grid.SetColumn(searchBox, 1);
            row1.Children.Add(searchBox);
            stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = searchBox;

            DPIAwareBox warningIcon = MainWindow.GetAlertIcon();
            warningIcon.Width = 16;
            warningIcon.Height = 16;
            Grid.SetColumn(warningIcon, 1);
            warningIcon.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right;
            warningIcon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
            warningIcon.Margin = new Avalonia.Thickness(0, 0, 10, 0);
            AvaloniaBugFixes.SetToolTip(warningIcon, "Invalid regex specified!");
            warningIcon.IsVisible = false;
            row1.Children.Add(warningIcon);

            {
                TextBlock blk = new TextBlock() { Text = "Replace with:", Margin = new Avalonia.Thickness(5, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
                Grid.SetColumn(blk, 2);
                row1.Children.Add(blk);
            }

            TextBox replaceBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Padding = new Avalonia.Thickness(5, 2, 5, 2), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
            Grid.SetColumn(replaceBox, 3);
            row1.Children.Add(replaceBox);

            CheckBox autoSearchBox = new CheckBox() { Content = "Auto", Padding = new Avalonia.Thickness(5, 0, 5, 0), Margin = new Avalonia.Thickness(5, 0, 5, 0), IsChecked = true, FontSize = 13 };
            Grid.SetColumn(autoSearchBox, 4);
            row1.Children.Add(autoSearchBox);

            Grid searchResultsGrid = new Grid() { Margin = new Avalonia.Thickness(10, 5, 0, 5), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchResultsGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            Grid.SetColumnSpan(searchResultsGrid, 2);
            Grid.SetRow(searchResultsGrid, 1);
            searchGrid.Children.Add(searchResultsGrid);

            TextBlock searchResultsTipBlock = new TextBlock() { Text = "", FontSize = 13, Margin = new Avalonia.Thickness(0, 0, 20, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(searchResultsTipBlock, 1);
            searchResultsGrid.Children.Add(searchResultsTipBlock);

            TextBlock searchResultsNodesBlock = new TextBlock() { Text = "", FontSize = 13, Margin = new Avalonia.Thickness(0, 0, 20, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
            Grid.SetColumn(searchResultsNodesBlock, 2);
            searchResultsGrid.Children.Add(searchResultsNodesBlock);

            searchGrid.PropertyChanged += (s, e) =>
            {
                if (e.Property == Grid.BoundsProperty)
                {
                    if (searchGrid.Bounds.Width < 510)
                    {
                        searchResultsTipBlock.IsVisible = false;
                        searchResultsNodesBlock.IsVisible = false;
                    }
                    else
                    {
                        searchResultsTipBlock.IsVisible = true;
                        searchResultsNodesBlock.IsVisible = true;
                    }
                }
            };

            SmallRibbonButton findButton = new SmallRibbonButton(new List<(string, Control, string)>()
            {
                ("", null, ""),
                ("Find next", new DPIAwareBox(GetIconFind) { Width = 16, Height = 16 }, "Highlights the next matched node."),
                ("Find previous", new DPIAwareBox(GetIconFind) { Width = 16, Height = 16 }, "Highlights the previous matched node."),
                ("Find all", new DPIAwareBox(GetIconFind) { Width = 16, Height = 16 }, "Highlights all matched nodes."),
            })
            { Margin = new Avalonia.Thickness(5, 0, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            findButton.Icon = new DPIAwareBox(GetIconFind) { Width = 16, Height = 16 };
            findButton.ButtonText = "Find";
            AvaloniaBugFixes.SetToolTip(findButton, "Highlights the next matched node.");
            Grid.SetColumn(findButton, 3);
            searchResultsGrid.Children.Add(findButton);

            SmallRibbonButton replaceAllButton = new SmallRibbonButton(new List<(string, Control, string)>())
            { Margin = new Avalonia.Thickness(5, 0, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            replaceAllButton.Icon = new DPIAwareBox(GetIconReplace) { Width = 16, Height = 16 };
            replaceAllButton.ButtonText = "Replace all";
            AvaloniaBugFixes.SetToolTip(replaceAllButton, "Enables the Replace attribute module.");
            Grid.SetColumn(replaceAllButton, 4);
            searchResultsGrid.Children.Add(replaceAllButton);

            SmallRibbonButton copyButton = new SmallRibbonButton(new List<(string, Control, string)>())
            { Margin = new Avalonia.Thickness(5, 0, 5, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            copyButton.Icon = new DPIAwareBox(GetIconCopy16) { Width = 16, Height = 16 };
            copyButton.ButtonText = "Copy";
            AvaloniaBugFixes.SetToolTip(copyButton, "Copies the value of an attribute at the matched nodes to the clipboard.");
            Grid.SetColumn(copyButton, 5);
            searchResultsGrid.Children.Add(copyButton);

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(-16, 0);
            TransformOperations offScreen = builder.Build();

            Accordion advancedAccordion = new Accordion() { FontSize = 13, ArrowSize = 12, Margin = new Avalonia.Thickness(10, 7.5, 10, 0), AccordionHeader = new TextBlock() { Text = "Advanced" } };
            advancedAccordion.FindControl<Grid>("HeaderGrid").HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
            advancedAccordion.FindControl<ContentControl>("HeaderPresenter").Padding = new Avalonia.Thickness(0, 0, 10, 0);
            advancedAccordion.FindControl<Border>("ContentGrid").Margin = new Avalonia.Thickness(0, 0, 0, 0);
            Grid.SetRow(advancedAccordion, 1);
            Grid.SetColumnSpan(advancedAccordion, 2);
            searchGrid.Children.Add(advancedAccordion);

            Grid advancedContent = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            advancedContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            advancedContent.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            advancedAccordion.AccordionContent = advancedContent;


            RibbonBar attributeHeaderBar = new RibbonBar(new (string, bool)[] { ("Search attribute", false), ("Replacement attribute", false) }) { FontSize = 13 };
            attributeHeaderBar.Classes.Add("Grey");
            attributeHeaderBar.FindControl<Grid>("ContainerGrid").Background = Avalonia.Media.Brushes.Transparent;
            advancedContent.Children.Add(attributeHeaderBar);

            Grid searchAttributeGrid = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            searchAttributeGrid.Transitions = new Avalonia.Animation.Transitions()
            {
                new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
            };
            Grid.SetRow(searchAttributeGrid, 2);
            Grid.SetColumnSpan(searchAttributeGrid, 2);
            advancedContent.Children.Add(searchAttributeGrid);

            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            searchAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            searchAttributeGrid.Children.Add(new TextBlock() { Text = "Attribute:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            List<TreeNode> nodes = window.TransformedTree.GetChildrenRecursive();

            HashSet<string> attributes = new HashSet<string>();

            foreach (TreeNode node in nodes)
            {
                foreach (KeyValuePair<string, object> attribute in node.Attributes)
                {
                    attributes.Add(attribute.Key);
                }
            }

            List<string> sortedAttributes = new List<string>(attributes);
            sortedAttributes.Sort();

            ComboBox attributeBox = new ComboBox() { Padding = new Avalonia.Thickness(5, 2, 5, 2), Items = sortedAttributes, SelectedIndex = sortedAttributes.IndexOf("Name"), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            {
                FillingControl<ComboBox> boxContainer = new FillingControl<ComboBox>(attributeBox, 5);
                boxContainer.Margin = new Avalonia.Thickness(-5, 0, -5, 0);
                Grid.SetColumn(boxContainer, 1);
                searchAttributeGrid.Children.Add(boxContainer);
            }

            {
                TextBlock blk = new TextBlock() { Text = "Type:", Margin = new Avalonia.Thickness(10, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
                Grid.SetColumn(blk, 2);
                searchAttributeGrid.Children.Add(blk);
            }

            ComboBox attributeTypeBox = new ComboBox() { Padding = new Avalonia.Thickness(5, 2, 5, 2), Items = new List<string>() { "String", "Number" }, SelectedIndex = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            {
                FillingControl<ComboBox> boxContainer = new FillingControl<ComboBox>(attributeTypeBox, 5);
                boxContainer.Margin = new Avalonia.Thickness(-5, 0, -5, 0);
                Grid.SetColumn(boxContainer, 3);
                searchAttributeGrid.Children.Add(boxContainer);
            }

            {
                TextBlock blk = new TextBlock() { Text = "Comparison:", Margin = new Avalonia.Thickness(10, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
                Grid.SetColumn(blk, 4);
                searchAttributeGrid.Children.Add(blk);
            }

            List<string> stringComparisons = new List<string>() { "Normal", "Case-insensitive", "Culture-aware", "Culture-aware, case-insensitive" };
            List<string> numberComparisons = new List<string>() { "Equal", "Smaller than", "Greater than" };

            ComboBox comparisonTypeBox = new ComboBox() { Padding = new Avalonia.Thickness(5, 2, 5, 2), Items = stringComparisons, SelectedIndex = 1, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            {
                FillingControl<ComboBox> boxContainer = new FillingControl<ComboBox>(comparisonTypeBox, 5);
                boxContainer.Margin = new Avalonia.Thickness(-5, 0, -5, 0);
                Grid.SetColumn(boxContainer, 5);
                searchAttributeGrid.Children.Add(boxContainer);
            }

            CheckBox regexBox = new CheckBox() { Content = "Regex", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Avalonia.Thickness(10, 0, 10, 0) };
            Grid.SetColumn(regexBox, 6);
            searchAttributeGrid.Children.Add(regexBox);



            Grid replaceAttributeGrid = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0), Opacity = 0, IsHitTestVisible = false, RenderTransform = offScreen };
            replaceAttributeGrid.Transitions = new Avalonia.Animation.Transitions()
            {
                new Avalonia.Animation.DoubleTransition() { Property = Grid.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) },
                new Avalonia.Animation.TransformOperationsTransition() { Property = Grid.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) }
            };
            Grid.SetRow(replaceAttributeGrid, 2);
            Grid.SetColumnSpan(replaceAttributeGrid, 2);
            advancedContent.Children.Add(replaceAttributeGrid);

            replaceAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            replaceAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            replaceAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            replaceAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            replaceAttributeGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            replaceAttributeGrid.Children.Add(new TextBlock() { Text = "Attribute:", Margin = new Avalonia.Thickness(0, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });

            TextBox replacementAttributeBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 5, 0), Text = "Name", Padding = new Avalonia.Thickness(5, 2, 5, 2), VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
            Grid.SetColumn(replacementAttributeBox, 1);
            replaceAttributeGrid.Children.Add(replacementAttributeBox);

            {
                TextBlock blk = new TextBlock() { Text = "Type:", Margin = new Avalonia.Thickness(10, 0, 5, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 };
                Grid.SetColumn(blk, 2);
                replaceAttributeGrid.Children.Add(blk);
            }

            ComboBox replacementAttributeTypeBox = new ComboBox() { Padding = new Avalonia.Thickness(5, 2, 5, 2), Items = new List<string>() { "String", "Number" }, SelectedIndex = 0, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };

            {
                FillingControl<ComboBox> boxContainer = new FillingControl<ComboBox>(replacementAttributeTypeBox, 5);
                boxContainer.Margin = new Avalonia.Thickness(-5, 0, -5, 0);
                Grid.SetColumn(boxContainer, 3);
                replaceAttributeGrid.Children.Add(boxContainer);
            }

            CheckBox recursiveBox = new CheckBox() { Content = "Recursive", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13, Margin = new Avalonia.Thickness(10, 0, 10, 0) };
            Grid.SetColumn(recursiveBox, 4);
            replaceAttributeGrid.Children.Add(recursiveBox);


            attributeHeaderBar.PropertyChanged += (s, e) =>
            {
                if (e.Property == RibbonBar.SelectedIndexProperty)
                {
                    int newIndex = (int)e.NewValue;
                    if (newIndex == 0)
                    {
                        replaceAttributeGrid.ZIndex = 0;
                        replaceAttributeGrid.RenderTransform = offScreen;
                        replaceAttributeGrid.Opacity = 0;
                        replaceAttributeGrid.IsHitTestVisible = false;

                        searchAttributeGrid.ZIndex = 1;
                        searchAttributeGrid.RenderTransform = TransformOperations.Identity;
                        searchAttributeGrid.Opacity = 1;
                        searchAttributeGrid.IsHitTestVisible = true;
                    }
                    else
                    {
                        searchAttributeGrid.ZIndex = 0;
                        searchAttributeGrid.RenderTransform = offScreen;
                        searchAttributeGrid.Opacity = 0;
                        searchAttributeGrid.IsHitTestVisible = false;

                        replaceAttributeGrid.ZIndex = 1;
                        replaceAttributeGrid.RenderTransform = TransformOperations.Identity;
                        replaceAttributeGrid.Opacity = 1;
                        replaceAttributeGrid.IsHitTestVisible = true;
                    }
                }
            };

            Canvas separator = new Canvas() { Height = 1, Margin = new Avalonia.Thickness(5, 5, 5, 1), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
            separator.Classes.Add("RibbonSeparator");
            Grid.SetColumnSpan(separator, 2);
            Grid.SetRow(separator, 2);
            searchGrid.Children.Add(separator);

            searchGrid.MaxHeight = 0;

            Avalonia.Animation.Transitions openCloseTransitions = new Avalonia.Animation.Transitions();
            openCloseTransitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Avalonia.Controls.Shapes.Path.MaxHeightProperty, Duration = TimeSpan.FromMilliseconds(100) });

            searchGrid.Transitions = openCloseTransitions;
            window.FindControl<StackPanel>("UpperBarContainer").Children.Add(searchGrid);
            window.SetSelection(null);
            searchGrid.MaxHeight = 80;

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(150);
                searchGrid.Transitions = null;
                searchGrid.MaxHeight = double.PositiveInfinity;
            });

            attributeBox.SelectionChanged += (s, e) =>
            {
                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                replacementAttributeBox.Text = attribute;

                if (window.TransformedTree.GetAttributeType(attribute) == "String")
                {
                    attributeTypeBox.SelectedIndex = 0;
                }
                else if (window.TransformedTree.GetAttributeType(attribute) == "Number")
                {
                    attributeTypeBox.SelectedIndex = 1;
                }
            };

            attributeTypeBox.SelectionChanged += (s, e) =>
            {
                replacementAttributeTypeBox.SelectedIndex = attributeTypeBox.SelectedIndex;
                if (attributeTypeBox.SelectedIndex == 0)
                {
                    comparisonTypeBox.Items = stringComparisons;
                    comparisonTypeBox.SelectedIndex = 0;
                }
                else if (attributeTypeBox.SelectedIndex == 1)
                {
                    comparisonTypeBox.Items = numberComparisons;
                    comparisonTypeBox.SelectedIndex = 0;
                }
            };

            void findAll()
            {
                window.ResetActionColours(true);

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;

                double numberNeedle = -1;

                if (attributeTypeIndex == 1 && !double.TryParse(needle, out numberNeedle))
                {
                    return;
                }

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }


                StringComparison comparison = StringComparison.InvariantCulture;
                RegexOptions options = RegexOptions.CultureInvariant;
                switch (comparisonType)
                {
                    case 0:
                        comparison = StringComparison.InvariantCulture;
                        options = RegexOptions.CultureInvariant;
                        break;
                    case 1:
                        comparison = StringComparison.InvariantCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;
                    case 2:
                        comparison = StringComparison.CurrentCulture;
                        options = RegexOptions.None;
                        break;
                    case 3:
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase;
                        break;
                }


                Regex reg = null;

                if (regex)
                {
                    try
                    {
                        reg = new Regex(needle, options);
                        regexBox.Styles.Clear();
                        warningIcon.IsVisible = false;
                    }
                    catch (Exception ex)
                    {
                        reg = null;
                        regex = false;
                        AvaloniaBugFixes.SetToolTip(warningIcon, "Invalid regex specified!\n" + ex.Message);
                        warningIcon.IsVisible = true;
                    }
                }
                else
                {
                    regexBox.Styles.Clear();
                    warningIcon.IsVisible = false;
                }

                List<string> matchedIds = new List<string>();
                List<TreeNode> matchedNodes = new List<TreeNode>();
                int matchedTips = 0;

                foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                {
                    if (node.Attributes.TryGetValue(attribute, out object attributeValue))
                    {
                        if (attributeTypeIndex == 0 && attributeValue is string actualValue)
                        {
                            if (regex)
                            {
                                if (reg.IsMatch(actualValue))
                                {
                                    matchedNodes.Add(node);
                                    matchedIds.Add(node.Id);
                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                            else
                            {
                                if (actualValue.Contains(needle, comparison))
                                {
                                    matchedNodes.Add(node);
                                    matchedIds.Add(node.Id);

                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                        }
                        else if (attributeTypeIndex == 1 && attributeValue is double actualNumber)
                        {
                            switch (comparisonType)
                            {
                                case 0:
                                    if (actualNumber == numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 1:
                                    if (actualNumber < numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 2:
                                    if (actualNumber > numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

                foreach (string id in matchedIds)
                {
                    foreach ((double, SKRenderAction) pth in MainWindow.FindPaths(window.FullSelectionCanvas, id))
                    {
                        window.ChangeActionColour(pth.Item2, highlightSKColour);
                    }
                }

                window.SelectionCanvas.InvalidateVisual();

                searchResultsTipBlock.Text = matchedTips.ToString() + " tip" + (matchedTips != 1 ? "s" : "");
                searchResultsNodesBlock.Text = matchedNodes.Count.ToString() + " node" + (matchedNodes.Count != 1 ? "s" : "");
            };

            int foundIndex = -1;

            void findNext(int increment)
            {
                window.ResetActionColours(true);

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;

                double numberNeedle = -1;

                if (attributeTypeIndex == 1 && !double.TryParse(needle, out numberNeedle))
                {
                    return;
                }

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }


                StringComparison comparison = StringComparison.InvariantCulture;
                RegexOptions options = RegexOptions.CultureInvariant;
                switch (comparisonType)
                {
                    case 0:
                        comparison = StringComparison.InvariantCulture;
                        options = RegexOptions.CultureInvariant;
                        break;
                    case 1:
                        comparison = StringComparison.InvariantCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;
                    case 2:
                        comparison = StringComparison.CurrentCulture;
                        options = RegexOptions.None;
                        break;
                    case 3:
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase;
                        break;
                }


                Regex reg = null;

                if (regex)
                {
                    try
                    {
                        reg = new Regex(needle, options);
                        regexBox.Styles.Clear();
                        warningIcon.IsVisible = false;
                    }
                    catch (Exception ex)
                    {
                        reg = null;
                        regex = false;
                        AvaloniaBugFixes.SetToolTip(warningIcon, "Invalid regex specified!\n" + ex.Message);
                        warningIcon.IsVisible = true;
                    }
                }
                else
                {
                    regexBox.Styles.Clear();
                    warningIcon.IsVisible = false;
                }

                SkiaSharp.SKColor selectionColor = window.SelectionSKColor;
                List<string> matchedIds = new List<string>();
                List<TreeNode> matchedNodes = new List<TreeNode>();
                int matchedTips = 0;

                foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                {
                    if (node.Attributes.TryGetValue(attribute, out object attributeValue))
                    {
                        if (attributeTypeIndex == 0 && attributeValue is string actualValue)
                        {
                            if (regex)
                            {
                                if (reg.IsMatch(actualValue))
                                {
                                    matchedNodes.Add(node);
                                    matchedIds.Add(node.Id);
                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                            else
                            {
                                if (actualValue.Contains(needle, comparison))
                                {
                                    matchedNodes.Add(node);
                                    matchedIds.Add(node.Id);

                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                        }
                        else if (attributeTypeIndex == 1 && attributeValue is double actualNumber)
                        {
                            switch (comparisonType)
                            {
                                case 0:
                                    if (actualNumber == numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 1:
                                    if (actualNumber < numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 2:
                                    if (actualNumber > numberNeedle)
                                    {
                                        matchedNodes.Add(node);
                                        matchedIds.Add(node.Id);

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                            }
                        }
                    }
                }

                if (matchedIds.Count > 0)
                {
                    foundIndex = (foundIndex + increment) % matchedIds.Count;

                    if (foundIndex < 0)
                    {
                        foundIndex = matchedIds.Count - 1;
                    }

                    Point pt = window.Coordinates[matchedIds[foundIndex]];

                    window.SetSelection(matchedNodes[foundIndex]);

                    foreach ((double, SKRenderAction) pth in MainWindow.FindPaths(window.FullSelectionCanvas, matchedIds[foundIndex]))
                    {
                        window.ChangeActionColour(pth.Item2, selectionColor);
                    }

                    window.SelectionCanvas.InvalidateVisual();

                    window.CenterAt(pt.X, pt.Y);
                }
                else
                {
                    foundIndex = 0;
                }

                searchResultsTipBlock.Text = matchedTips.ToString() + " tip" + (matchedTips != 1 ? "s" : "");
                searchResultsNodesBlock.Text = matchedNodes.Count.ToString() + " node" + (matchedNodes.Count != 1 ? "s" : "");
            };

            findButton.ButtonPressed += (s, e) =>
            {
                if (e.Index <= 0)
                {
                    findNext(1);
                }
                else if (e.Index == 1)
                {
                    findNext(-1);
                }
                else if (e.Index == 2)
                {
                    foundIndex = -1;
                    findAll();
                }
            };

            searchBox.PropertyChanged += (s, e) =>
            {
                if (autoSearchBox.IsChecked == true && e.Property == TextBox.TextProperty)
                {
                    findAll();
                }

                if (e.Property == TextBox.TextProperty)
                {
                    foundIndex = -1;
                }
            };

            attributeBox.SelectionChanged += (s, e) =>
            {
                if (autoSearchBox.IsChecked == true)
                {
                    findAll();
                }

                foundIndex = -1;
            };

            attributeTypeBox.SelectionChanged += (s, e) =>
            {
                if (autoSearchBox.IsChecked == true)
                {
                    findAll();
                }

                foundIndex = -1;
            };

            comparisonTypeBox.SelectionChanged += (s, e) =>
            {
                if (autoSearchBox.IsChecked == true)
                {
                    findAll();
                }

                foundIndex = -1;
            };

            copyButton.ButtonPressed += async (s, e) =>
            {
                window.ResetActionColours(true);

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;

                double numberNeedle = -1;

                if (attributeTypeIndex == 1 && !double.TryParse(needle, out numberNeedle))
                {
                    return;
                }

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }


                StringComparison comparison = StringComparison.InvariantCulture;
                RegexOptions options = RegexOptions.CultureInvariant;
                switch (comparisonType)
                {
                    case 0:
                        comparison = StringComparison.InvariantCulture;
                        options = RegexOptions.CultureInvariant;
                        break;
                    case 1:
                        comparison = StringComparison.InvariantCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
                        break;
                    case 2:
                        comparison = StringComparison.CurrentCulture;
                        options = RegexOptions.None;
                        break;
                    case 3:
                        comparison = StringComparison.CurrentCultureIgnoreCase;
                        options = RegexOptions.IgnoreCase;
                        break;
                }


                Regex reg = null;

                if (regex)
                {
                    try
                    {
                        reg = new Regex(needle, options);
                        regexBox.Styles.Clear();
                        warningIcon.IsVisible = false;
                    }
                    catch (Exception ex)
                    {
                        reg = null;
                        regex = false;
                        AvaloniaBugFixes.SetToolTip(warningIcon, "Invalid regex specified!\n" + ex.Message);
                        warningIcon.IsVisible = true;
                    }
                }
                else
                {
                    regexBox.Styles.Clear();
                    warningIcon.IsVisible = false;
                }

                List<string> matchedIds = new List<string>();
                List<string> matchedNames = new List<string>();
                List<TreeNode> matchedNodes = new List<TreeNode>();
                int matchedTips = 0;

                foreach (TreeNode node in window.TransformedTree.GetChildrenRecursiveLazy())
                {
                    if (node.Attributes.TryGetValue(attribute, out object attributeValue))
                    {
                        if (attributeTypeIndex == 0 && attributeValue is string actualValue)
                        {
                            if (regex)
                            {
                                if (reg.IsMatch(actualValue))
                                {
                                    matchedNodes.Add(node);
                                    matchedIds.Add(node.Id);
                                    if (!string.IsNullOrEmpty(node.Name))
                                    {
                                        matchedNames.Add(node.Name);
                                    }

                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                            else
                            {
                                if (actualValue.Contains(needle, comparison))
                                {
                                    matchedIds.Add(node.Id);
                                    matchedNodes.Add(node);
                                    if (!string.IsNullOrEmpty(node.Name))
                                    {
                                        matchedNames.Add(node.Name);
                                    }

                                    if (node.Children.Count == 0)
                                    {
                                        matchedTips++;
                                    }
                                }
                            }
                        }
                        else if (attributeTypeIndex == 1 && attributeValue is double actualNumber)
                        {
                            switch (comparisonType)
                            {
                                case 0:
                                    if (actualNumber == numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        matchedNodes.Add(node);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 1:
                                    if (actualNumber < numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        matchedNodes.Add(node);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                                case 2:
                                    if (actualNumber > numberNeedle)
                                    {
                                        matchedIds.Add(node.Id);
                                        matchedNodes.Add(node);
                                        if (!string.IsNullOrEmpty(node.Name))
                                        {
                                            matchedNames.Add(node.Name);
                                        }

                                        if (node.Children.Count == 0)
                                        {
                                            matchedTips++;
                                        }
                                    }
                                    break;
                            }
                        }


                    }
                }

                foreach (string id in matchedIds)
                {
                    foreach ((double, SKRenderAction) pth in MainWindow.FindPaths(window.FullSelectionCanvas, id))
                    {
                        window.ChangeActionColour(pth.Item2, highlightSKColour);
                    }
                }

                window.SelectionCanvas.InvalidateVisual();

                searchResultsTipBlock.Text = matchedTips.ToString() + " tip" + (matchedTips != 1 ? "s" : "");
                searchResultsNodesBlock.Text = matchedNodes.Count.ToString() + " node" + (matchedNodes.Count != 1 ? "s" : "");

                if (matchedNames.Count > 0)
                {
                    ChildWindow attributeSelectionWindow = new ChildWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Icon = window.Icon, Width = 350, Height = 190, Title = "Select attribute...", WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(231, 231, 231)), CanMaximizeMinimize = false, SizeToContent = SizeToContent.Height };

                    Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
                    attributeSelectionWindow.Content = grd;
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                    grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                    grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    {
                        Grid header = new Grid();
                        header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                        header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                        header.Children.Add(new DPIAwareBox(GetIconCopy) { Width = 32, Height = 32, Margin = new Avalonia.Thickness(0, 0, 10, 0) });

                        TextBlock blk = new TextBlock() { Text = "Copy attribute", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 0, 10), FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)) };
                        Grid.SetColumn(blk, 1);
                        header.Children.Add(blk);

                        Grid.SetColumnSpan(header, 2);
                        grd.Children.Add(header);
                    }

                    {
                        TextBlock blk = new TextBlock() { Text = searchResultsTipBlock.Text + ", " + searchResultsNodesBlock.Text + " found", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 5, 0, 10), FontSize = 13, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(102, 102, 102)) };
                        Grid.SetColumnSpan(blk, 2);
                        Grid.SetRow(blk, 1);
                        grd.Children.Add(blk);
                    }

                    {
                        TextBlock blk = new TextBlock() { Text = "Select attribute to copy:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        Grid.SetRow(blk, 3);
                        grd.Children.Add(blk);
                    }

                    {
                        TextBlock blk = new TextBlock() { Text = "Copy attribute at:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                        Grid.SetRow(blk, 2);
                        grd.Children.Add(blk);
                    }

                    ComboBox nodeBox = new ComboBox() { Items = new List<string>() { "Internal nodes", "Tips", "All nodes" }, SelectedIndex = 2, Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    Grid.SetRow(nodeBox, 2);
                    Grid.SetColumn(nodeBox, 1);
                    grd.Children.Add(nodeBox);

                    Grid buttonGrid = new Grid();
                    Grid.SetColumnSpan(buttonGrid, 2);

                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    Button okButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "OK", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black } };
                    okButton.Classes.Add("SideBarButton");
                    Grid.SetColumn(okButton, 1);
                    buttonGrid.Children.Add(okButton);

                    Button cancelButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "Cancel", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black }, Foreground = Avalonia.Media.Brushes.Black };
                    cancelButton.Classes.Add("SideBarButton");
                    Grid.SetColumn(cancelButton, 3);
                    buttonGrid.Children.Add(cancelButton);

                    Grid.SetRow(buttonGrid, 5);
                    grd.Children.Add(buttonGrid);

                    bool result = false;

                    okButton.Click += (s, e) =>
                    {
                        result = true;
                        attributeSelectionWindow.Close();
                    };

                    cancelButton.Click += (s, e) =>
                    {
                        attributeSelectionWindow.Close();
                    };

                    HashSet<string> attributes = new HashSet<string>();

                    foreach (TreeNode node in matchedNodes)
                    {
                        foreach (KeyValuePair<string, object> nodeAttribute in node.Attributes)
                        {
                            attributes.Add(nodeAttribute.Key);
                        }
                    }

                    List<string> attributesList = attributes.ToList();

                    ComboBox attributeBox = new ComboBox() { Items = attributesList, SelectedIndex = Math.Max(attributesList.IndexOf("Name"), 0), Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                    Grid.SetRow(attributeBox, 3);
                    Grid.SetColumn(attributeBox, 1);
                    grd.Children.Add(attributeBox);


                    await attributeSelectionWindow.ShowDialog2(window);

                    if (result)
                    {
                        string attributeName = attributesList[attributeBox.SelectedIndex];

                        List<string> attributeValues = new List<string>();

                        if (attributeName != null)
                        {
                            foreach (TreeNode node in matchedNodes)
                            {
                                if (nodeBox.SelectedIndex == 2 || (nodeBox.SelectedIndex == 1 && node.Children.Count == 0) || (nodeBox.SelectedIndex == 0 && node.Children.Count > 0))
                                {
                                    if (node.Attributes.TryGetValue(attributeName, out object attributeValue))
                                    {
                                        if (attributeValue is string attributeString)
                                        {
                                            attributeValues.Add(attributeString);
                                        }
                                        else if (attributeValue is double attributeDouble)
                                        {
                                            attributeValues.Add(attributeDouble.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                        }
                                    }
                                }
                            }
                        }

                        if (attributeValues.Count > 0)
                        {
                            _ = Avalonia.Application.Current.Clipboard.SetTextAsync(string.Join("\n", attributeValues));
                        }
                    }
                }
            };

            replaceAllButton.ButtonPressed += (s, e) =>
            {
                findAll();

                string attribute = sortedAttributes[attributeBox.SelectedIndex];

                int attributeTypeIndex = attributeTypeBox.SelectedIndex;

                int comparisonType = comparisonTypeBox.SelectedIndex;

                bool regex = regexBox.IsChecked == true;

                string needle = searchBox.Text;
                string replacement = replaceBox.Text;

                if (string.IsNullOrEmpty(needle))
                {
                    return;
                }

                Dictionary<string, object> parametersToChange = new Dictionary<string, object>()
                {
                    { "Attribute:", attribute },
                    { "Attribute type:", attributeTypeIndex == 0 ? "String" : "Number" },
                    { "Value:", needle },
                    { "Attribute: ", replacementAttributeBox.Text },
                    { "Attribute type: ", replacementAttributeTypeBox.SelectedIndex == 0 ? "String" : "Number" },
                    { "Value: ", replacement },
                    { "Apply recursively to all children", recursiveBox.IsChecked == true }
                };

                if (attributeTypeIndex == 0)
                {
                    parametersToChange.Add("Comparison type:", comparisonType);
                    parametersToChange.Add("Regex", regex);
                }
                else
                {
                    parametersToChange.Add("Comparison type: ", comparisonType);
                }

                parametersToChange.Add("Apply", true);

                window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, window.FurtherTransformations.Count);

                FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "f17160ad-0462-449a-8a57-e1af775c92ba");
                Action<Dictionary<string, object>> changeParameter = window.AddFurtherTransformation(module);

                changeParameter(parametersToChange);
                _ = window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
            };

            void windowKeyDownHandler(object sender, Avalonia.Input.KeyEventArgs e)
            {
                if (e.Key == Avalonia.Input.Key.F3)
                {
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        findNext(-1);
                    }
                    else if (e.KeyModifiers == Avalonia.Input.KeyModifiers.None)
                    {
                        findNext(1);
                    }
                    else
                    {
                        findAll();
                    }
                }
            }

            window.KeyDown += windowKeyDownHandler;

            searchBox.Focus();

            searchBox.KeyDown += async (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.Enter)
                {
                    if (e.KeyModifiers == Avalonia.Input.KeyModifiers.Shift)
                    {
                        findNext(-1);
                    }
                    else if (e.KeyModifiers == Avalonia.Input.KeyModifiers.None)
                    {
                        findNext(1);
                    }
                    else
                    {
                        findAll();
                    }
                }
                else if (e.Key == Avalonia.Input.Key.Escape)
                {
                    searchGrid.MaxHeight = searchGrid.Bounds.Height;
                    searchGrid.Transitions = openCloseTransitions;
                    searchGrid.MaxHeight = 0;

                    await System.Threading.Tasks.Task.Delay(150);
                    window.FindControl<StackPanel>("UpperBarContainer").Children.Remove(searchGrid);

                    window.KeyDown -= windowKeyDownHandler;

                    stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = null;
                }
            };

            closeButton.Click += async (s, e) =>
            {
                searchGrid.MaxHeight = searchGrid.Bounds.Height;
                searchGrid.Transitions = openCloseTransitions;
                searchGrid.MaxHeight = 0;

                await System.Threading.Tasks.Task.Delay(150);
                window.FindControl<StackPanel>("UpperBarContainer").Children.Remove(searchGrid);

                window.KeyDown -= windowKeyDownHandler;

                stateData.Tags["5f3a7147-f706-43dc-9f57-18ade0c7b15d"] = null;
            };
        }
    }
}
