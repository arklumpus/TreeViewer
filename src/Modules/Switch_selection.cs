using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace SwitchSelectionAction
{
    /// <summary>
    /// This module is used to apply the _Switch chidren_ Further transformation to the selected node.
    /// 
    /// **Note**: using this module without installing also the _Switch children_ module (id `c4c71099-c7dc-44b3-93be-25a79afb1102`) may lead
    /// to program crashes or unexpected results.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Switch selection";
        public const string HelpText = "Switches the order of the children of the selection.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.2");
        public const string Id = "0b27abb2-0d48-40c0-9d1e-cfc7ffb7284c";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = true;
        public static string ButtonText { get; } = "Switch children";
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Arrange";
        public static double GroupIndex { get; } = 5;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>()
        {
            ( "", null ),
            ( "Switch children at selected node", scaling => GetIcon(scaling, Icon_0_16, Icon_0_24,Icon_0_32)),
            ( "Switch children at node and all descendants", scaling => GetIcon(scaling, Icon_3_16, Icon_3_24,Icon_3_32) )
        };

        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACKSURBVDhPY2TAAoqKiv5DmeQBUgxggtJkA6IN8Krd+B+EgYABhGGAKANAGqFMBu+6TSjeI2gAsmYYQDcEA2ALRGQvIAMWYkIcpAloK5jNyIg15lEBPhdAuXDAAqVxAnRNMP62Zn+wUwgGIkwhMkAWIyoakTVgMxAD0DUpY3UOKS7AagB6YsENGBgAnVxGI+3nlw0AAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC6SURBVEhLzZUxDsIwDEXjHq8Ld0lmVBB77sLC9VI+OFKJHLeyieBJlhsP/9exq1IpJfRIKV2eaXmfbExEFKRgXOIq6CzGWBBcMjFxHsbvDDZzUJnPd/UKXR1Ucc3EbNCK9kxMBj0xqW4yeNxOhODjC6kGXDM4Ah35kHLO4kptr0R6ezC+A84itbu/7sC8pu1KSjVgXlN+/OCra9qKDZlBFe2JA/eQNXHgNthjuIHanvd/DPY6uHI2EsIKyvRMkS58JvYAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEaSURBVFhH7ZbPDoMgDMbBJ5vX7bB30fP+ZWd9l+2wq3szRrEsxlFokxoTs19CGgTtx5cCWuec4dC27dWHy9jTo7LWGk7zqCcHwpc5NE0TrOq6jv0Ohwojm5Q7qcadKxagzSIC9qfH7nB+Ooj4iERdACYdxp4ZSiJUBcySR7IiWAI4ZwWRPEKK0HSASh5JjrP2NDjgT8JgQ9/3xXf8ar+Wve7H7PxFdoGE7QmYFyz0c0WsKiCXiBqz8ZLhsrkiDD8kcCuViE6pO8BJviT/c2D9IsRYpFSE08KjSBWkpgM1RorkuJoAv7q3D5SIGsd/UK0BQgSZHFAvwpmIbHIRUISSeyN3MU1ZfRuKBcDKOI07V3wOaCNx4IZREWM+C0+rf7XsM3gAAAAASUVORK5CYII=";

        private static string Icon_0_16 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC3SURBVDhPnZJhDsIgDIXB7HYmHgb+O+d/OIyJ50Me4xFgZaJf0rSD9tEytJoghJCjHWttuyCBopEZY6LbNYYdMAFIJzrnjrU8oTeeKH1fcm2D1t+vBl3BmkwogumWe9jWWcu9iSP8wgKV6/31iPEa/fZ+3hAfGP37Jfu18kkAFwlx4L0X569H2DpfigFiycom4Ub90nqYQ9NIZossGs0rAoEcFvpTKAzqGEy/xFqIMSyNkFb/QqkPWqrxaDSOh+cAAAAASUVORK5CYII=";
        private static string Icon_3_16 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADGSURBVDhPnZJdDgIhDITBeDpNPAz77M/6DnfRxPMhQxjsQtkQv6RpF2jplLVmghhjiX4sy5IXhwXapPPt/Ujunmz9PC+IjbXWHBBMgmTp8yWbAligKayNz2wkMBGtna6vrmWJOgN5c9JcP1KB4az2ZqC23DLsYBaLpPaJoM97X4tTr8axePlE9Y3ZUQhBnQH2OYNOr5SDWLO6SbjhnEtO75pnaBaH2SKT9jR3oEAJK+0tLAxkDNT/AANskYUYw7KEvPoXxnwBP7WnNz1vvD4AAAAASUVORK5CYII=";

        private static string Icon_0_24 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEKSURBVEhLtZVLDsIgEIah8WB2beJh2r2vPT2MiWu9WeXHGaV0qEDgSwiUknn8M7RaVWIYhoudzp+nHx3NNVgZB9kZzPNMqyXjOLoX0zQtbGY5gHFrSJSC8R3gfIlEUeMhWuuiDEQpbJFF7aIZwFg4/nCleYGYAYzlaB0DdrZqkKx1DNRg5SCUApFiGGPcoO1ktjIQNc1Fc8SH031vp6d7UKp/3I4vdAxrzee4i1LpqJiAjQN/7YCeRFZmOztQTHayCTnB2aTzYRf1NAN/XYwLCDeQPrVf4Bl7sduZip+B1PNuj4tbiuYo/ZsJo7ndEqP5PeAaVIk2JOyiJjR14H44reRhkEGdYooo9QbItYM5KaQiCgAAAABJRU5ErkJggg==";
        private static string Icon_3_24 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEZSURBVEhLtZRBDsIgEEXBeDC71cTDtGu1eoDeRRPXejPk0wEpZVog9CXNUBwH+H+oFJVo27bX4Ta+/dlR3IzsEyilTDxdXwcdPuZFiOb9OH+7rlPDMExqZp0AxXURSAFsceCPHcgvkWimM4eUUuxpnMOdYqMfJxFFmD1qSLAerGjda62tVAaui6KgOP6AeLw8lf/Qb5NdciB3yYNkrTngwWwBrBrg9NX44zKwAMnQk66GYD5JIiDtjtcujs3DnBkksku9ONCTsG2aBO4BzJy0HActgtykfJzaN7mumRqzIRjmmwlKzIzhnyDW82bOmluKtLv0P7MomtstHEs3OatbWMiDKrsNCbtoEzZdAG3qTN4KnKCOmVGE+AH6MZ3XRLSL2wAAAABJRU5ErkJggg==";

        private static string Icon_0_32 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAE8SURBVFhH7ZVdDoIwEIQpx9NE7wLPKugz3EUTvV7t1K2B0nZLSsVEv2TDT8Hdnc6iKDIipSzqum7U6el1Z0pJx8VAUhOENzlIVmCQyIlSQD/Q9/0kF95NUoBLHkOyAlVVBffYkEUBgk2eFaWARNDlCHTIrS8+BT6QzA7AesA86INzuVn3wRaQYjLAvR8sIKYDg6+AEPj9GAV0AV3XOZ8NbQEHCog2oRDCGal8bAp8jArYHK6NCkkB84CWjkEgp4k5vDWkhLZb2/t512CfAy6fl9GixJhQ1a5RifnMRinkQ6ADOHx7vDk7UQqIkAIp2FPg6iSpuxh0AarD5nHZwwPDhC3uYY2u84AtCBmJW09htAVzx2cpVv8Q6SnACSbB9WnNJb8hRoG8k2BM9rMe+BfwPWO4FlAg+x+On6J4AnMSxsdaSfTCAAAAAElFTkSuQmCC";
        private static string Icon_3_32 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFISURBVFhH7ZVhjoIwEIVbj7eb6F3g9y6gv+Eua+Jer/bhDCnSdqil0ez6JQ1QkGnfvIdaFcQYo+q6bu1pc5tZsqPjZqAoDyJYHGQr4BRSn99nd7fd5bhvWYFhGBa18NssBSLFQYO5vu8xHyRrAVpr7jHwSR2VH2zhAbFIjC1N2NHRxTc3I2kBH18/rR2GBkvfoQ2/pwOu3YKuCadkuAOIKeAHPSYDXMSEXI57dOlFVCDHZDCpJdqGqAK8AxslbRXw7sQqoEMKSOD9UQVoB8xDJpNYZUK0wWcyzDktKkNVVQaDLhdI92MsWhCKGR2j4GU8UpiaTAWTYgYe3T2zsy9oadXJMSOyjKixg1Ixk7j3QJGYSYwL+LMxk5i1IDU+W7HqS1iSMQU4QRLuvv0jpeRn1ihQNglssn/rgfcCXieGzwIKFP/DCaPUFcCT56f94F9BAAAAAElFTkSuQmCC";

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, Icon16Base64, Icon24Base64, Icon32Base64);
        }

        private static Page GetIcon(double scaling, string low, string medium, string high)
        {
            if (scaling <= 1)
            {
                return GetIcon(low);
            }
            else if (scaling <= 1.5)
            {
                return GetIcon(medium);
            }
            else
            {
                return GetIcon(high);
            }
        }

        private static Page GetIcon(string byteImage)
        {
            byte[] bytes;

            bytes = Convert.FromBase64String(byteImage);

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

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return new List<bool>() { selection != null && selection.Children.Count > 1, selection != null && selection.Children.Count > 1, selection != null && selection.GetChildrenRecursive().Count > selection.Children.Count + 1 };
        }

        public static void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            if (actionIndex == -1)
            {
                actionIndex = 0;
            }

            List<string> nodeNames = selection.GetNodeNames();

            if (nodeNames.Count == 0 || !selection.IsLastCommonAncestor(nodeNames))
            {
                if (InstanceStateData.IsUIAvailable)
                {
                    MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    box.ShowDialog2(window);
                    return;
                }
                else if (InstanceStateData.IsInteractive)
                {
                    Console.WriteLine();
                    Console.WriteLine("Attention! The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    Console.WriteLine();
                    return;
                }
                else
                {
                    return;
                }
            }
			
			if (InstanceStateData.IsUIAvailable)
			{
				window.PushUndoFrame(UndoFrameLevel.FurtherTransformationModule, window.FurtherTransformations.Count);
			}

            FurtherTransformationModule module = Modules.GetModule(Modules.FurtherTransformationModules, "c4c71099-c7dc-44b3-93be-25a79afb1102");
            Action<Dictionary<string, object>> changeParameter = stateData.AddFurtherTransformationModule(module);

            if (actionIndex == 0)
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() } });
            }
            else
            {
                changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Recursive", true } });
            }

            if (InstanceStateData.IsUIAvailable)
            {
                _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    await window.UpdateFurtherTransformations(window.FurtherTransformations.Count - 1);
                    window.SetSelection(window.TransformedTree.GetLastCommonAncestor(selection.GetNodeNames()));
                });
            }
        }
    }
}