using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp;
using PhyloTree.Formats;
using Avalonia.Media.Imaging;
using Avalonia;
using Avalonia.Styling;
using Avalonia.Media;
using System.Runtime.InteropServices;
using Avalonia.Layout;
using Avalonia.VisualTree;
using System.IO;


namespace SaveTree
{
    /// <summary>
    /// This module is used to save the currently opened tree to a file on disk. The tree can be saved in Newick, NEXUS Binary or NCBI ASN.1 format.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// When saving the tree, the first choice that needs to be done is _which_ tree to save. There are three possible options:
    /// 
    /// 1. The original tree(s) that were loaded from a file that was opened in TreeViewer.
    /// 2. The transformed tree that was produced by the Transformer module (e.g. a consensus tree).
    /// 3. The final transformed tree that was produced after all the Further transformation modules acted on the transformed tree.
    /// 
    /// If the tree(s) is/are saved in Newick, Newick-with-attributes, or NCBI ASN.1 format, only the tree itself is saved, without including any
    /// information about the modules that are currently active in the plot. This means that if the file is later opened again in
    /// TreeViewer, all information about the active modules will be lost.
    /// 
    /// Instead, if the file is saved in NEXUS or Binary format, all the information about the modules can be kept, if desired. This means
    /// that the tree can be opened again in TreeViewer to obtain exactly the same plot. Other software opening the file should ignore
    /// the information about TreeViewer modules; thus, including information about the active modules should not cause compatibility
    /// issues with other programs. The attachments that have been added to the tree can be included in the file as well; this makes
    /// it possible to obtain a portable file that contains all the information required to reliably reproduce the plot. Note however that
    /// users opening the file need to have the relevant modules installed.
    /// 
    /// If the tree being exported is the original tree, the state of all the modules that are currently enabled is saved. If the transformed
    /// tree is being exported, the state of the Transformer module is not exported. If the final transformed tree is saved, only the state of
    /// the Coordinates and Plot action modules is saved.
    /// 
    /// Furthermore, if the file includes information about the modules or attachments, it can be signed. This adds a layer
    /// of security, by ensuring that the source code contained in the module information (e.g. in attribute formatters) has not been
    /// tampered with. The files are signed with the user's unique private key.
    /// 
    /// When a user opens a file created by someone else that contains source code, they will be asked if they trust the origin
    /// of the file. The source code is only loaded and compiled if they respond affirmatively. In addition, if the file has been signed,
    /// the public key of the signer can be added to the users key store; this causes subsequent files that have been signed with the
    /// same private key (i.e. by the same user) to be opened automatically, without asking for confirmation (and thus providing a more
    /// streamlined interface when repeatedly opening files coming from the same collaborators).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Save tree";
        public const string HelpText = "Saves the tree file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.2.0");
        public const string Id = "a8f25c08-4935-4fd5-80ea-1d29ada66f1e";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Save";
        public static string ParentMenu { get; } = "File";
        public static string GroupName { get; } = "Save";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { ( Avalonia.Input.Key.S, Avalonia.Input.KeyModifiers.Control ), ( Avalonia.Input.Key.S, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Shift ), ( Avalonia.Input.Key.S, Avalonia.Input.KeyModifiers.Control | Avalonia.Input.KeyModifiers.Alt ) };
        public static bool TriggerInTextBox { get; } = false;

        public static double GroupIndex { get; } = 0;
        public static bool IsLargeButton { get; } = false;
        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>() { ( "Save loaded tree(s)", null ), ( "Save transformed tree", null ), ( "Save final tree", null )  };


        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAB0SURBVDhPzZJRDsAgCEPZTubR9ORIZ5eg0c3ws73EpEBFooqqZlsRkgCoSxhee5AHt3a5FG4AoE/qMFsNDsKwY2xQMNYT8DQrYTIE9nYTWLzzpJn2BjKUW9OM/uklojCD5R5fWJoco/9/E3zzD15PXSEipQKq4n6ztzlLxQAAAABJRU5ErkJggg==";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC1SURBVEhL7ZQBDoMwCEVxJ9vR9OTVTz+KHc3WzW5m8SW28AmQolZSSuPyHM1dDHg0VyLNg7hBaZejAWsCSw1HpHkQNyg95GgQTbBSW4k0D+IGpWqdG+1unLPB4KBUpdZgwvxaQE5OLWCwC6j9hy95OdUnV8fIMhtQaSql30JUq/uIBnTx37P52Cm9hOVgp5RPpIuj9FuIal0j2hHV+smf3HzRGcjNJRwMdAG1vzKit0fyDBGZZkhIGZfw5E3tAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAD9SURBVFhH7ZcLDoMgEES1J/No9uR0dp21BQr4QWwTXqJ1PywT3AgdBOfcjKsVk05qwNFycuMtgg5H04Ohr7EQpip0eePxMy3WyiKCxuUCBDzGIvjQRIAA0xPRXIAA1ypilJs4R6DRD3KxEMsVLD833mIPtSogkxh0baIk4Ck3UXsErcAaSZi7Ll8IQme+EzPLRDCe74ErsXmr9cBRflsAVqnGPpHsA4VJURPCVXOTikTQn27ClH8vpfq398DmFTB7K+E4sw3z/88KHKVUv3+IehP2JswJOHUaMrRS7lTEPEv0gPuy3ZCxfiJSAVXe9V5kTrD0Buw7/iGjN4bhBQrx2vu1/S1QAAAAAElFTkSuQmCC";

        public static VectSharp.Page GetIcon(double scaling)
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

            VectSharp.RasterImage icon;

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

            VectSharp.Page pag = new VectSharp.Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static Avalonia.Controls.Control GetFileMenuPage()
        {
            return new SavePage();
        }

        public static List<bool> IsEnabled(MainWindow window)
        {
            return new List<bool>() { window.IsTreeOpened, window.IsTreeOpened, window.IsTreeOpened, window.IsTreeOpened };
        }

        public static Task PerformAction(int index, MainWindow window)
        {
			window.RibbonBar.SelectedIndex = 0;
			SavePage pag = window.GetFilePage<SavePage>(out int ind);
			window.RibbonFilePage.SelectedIndex = ind;
			((RibbonFilePageContentTabbedWithButtons)pag.PageContent).SelectedIndex = Math.Max(0, index);

			
            return Task.CompletedTask;
        }
    }




    class SavePage : RibbonFilePageContentTemplate
    {
        public static readonly StyledProperty<int> SelectedFileTypeProperty = AvaloniaProperty.Register<SavePage, int>(nameof(SelectedFileType), -1);

        public int SelectedFileType
        {
            get { return GetValue(SelectedFileTypeProperty); }
            set { SetValue(SelectedFileTypeProperty, value); }
        }

        public SavePage() : base("Save")
        {
            RibbonFilePageContentTabbedWithButtons content = new RibbonFilePageContentTabbedWithButtons(new List<(string, string, Control, Control)>()
            {
                ("Save loaded tree(s)", "Trees that were loaded from the file", new DPIAwareBox(GetStep1Icon), GetLoadedTreesPage()),
                ("Save transformed tree", "Tree created by the Transformer module", new DPIAwareBox(GetStep2Icon), GetTransformedTreePage()),
                ("Save final tree", "Final tree after Further transformations", new DPIAwareBox(GetStep3Icon), GetFinalTreePage()),
            });

            this.SelectedFileType = 0;

            this.PageContent = content;

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == SavePage.IsVisibleProperty)
                {
                    content.SelectedIndex = 0;
                    this.SelectedFileType = 0;
                }
            };
        }

        private List<(string, string, string)> FileFormats = new List<(string, string, string)>()
        {
            ( "Binary format (*.tbi)", "TreeViewer.Assets.FileTypeIcons.tbi", "Preserves all attributes, attachments and modules"),
            ( "NEXUS format (*.nex)", "TreeViewer.Assets.FileTypeIcons.nex", "Preserves all attributes, attachments and modules"),
            ( "Newick format (*.tre, *.nwk)", "TreeViewer.Assets.FileTypeIcons.tre", "Preserves only basic attributes"),
            ( "Newick with attributes format (*.nwka)", "TreeViewer.Assets.FileTypeIcons.nwka", "Preserves all attributes"),
            ( "NCBI ASN.1 text format (*.asn)", "TreeViewer.Assets.FileTypeIcons.asnb", "Preserves all attributes, but supports only one tree"),
            ( "NCBI ASN.1 binary format (*.asnb)", "TreeViewer.Assets.FileTypeIcons.asn", "Preserves all attributes, but supports only one tree")
        };

        private static string Icon32Step1Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFWSURBVFhH5ZWxEsIgDIZbrw+mq4vv0t2zPfe+i4trfTNN2sSjkEAAdfG766XQGMKfgM3f05J90/f9AOayjmSmaQp+l0KLuyPrEl28AltcyPSJDw0/hhZXKsHiVCIzAzHUMvpxO8tutYCRJM1llHpAoqgvMEF+aCqg8z/GFNECWVRk2JdjqQqgIz80VctIdkOwI1jQ3EAa/i6ryVUixz9rRy6x3eUkaz0FYv0i5Pr/MdVdejzfNqfmfj0FMcFn6Qn8xu/AAcYPUw9AUw3c2UKDFd2SwAzJ7L96FRuYrQks4NEruVxQeqk0SDCJcoMx34ROTZmltvS+ASUHM6+jFSkB9RIxJpCFWgKWu0TyHFptx+7CMZ9aBTqyKfBqTZ4ErdF83KTVHrBK7wYrScCqgJlUSfwkVQU0fGVqFZBOwU//Sk0Zx/AVsJTA9fl4AvQaJVWCXCpK1owvTvvDjeK++0UAAAAASUVORK5CYII=";
        private static string Icon48Step1Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIVSURBVGhD7ZjPbsIwDMbpnmxcd9mzwH3a0O7wLFx2ZW/WxVuCLGM7X90EOqk/CbUKxPEX/0npZmVlHkO+3jCO42a/33+k2/e/EYzT6WTa7IG52G63m+w80VIAsolP+aox2fnWIBngRWDMt3dPiwL3wWKgMA3DrX9LE3A8HlUfBkRlDwG0cQTaKCwfwgKQAqsJT2vDjcKy5RWxS6TFKsDzS8QkvzWgkRy8fqGpnxo5vk6pOW5DrpEj7PpAqIOEZ5yofc9BNomKVDYTb5PKmpCAGjUBiC1DgJmmZU2vBg75+jCSKBLg+mHuXM5BqFB7RUBCPsnf+DMcuFNSgLaQhmdDQ7MbbqMccoR/8nBTrEYwJwJmeiHpQLQQG4oA7QZSYACzG0U4AlZIC0gEyEajE31lJUq4BlBe3s5qjn99vpprpznXAuO/Y+PbNP5NN03OgQo9CvSSxDzTTSgCXveQJyrfTc7MCBS2oQgsqPVdoim0mL4dSiH+CFB7CIukkIVmyzSCnpJRAYlDEkH2IZId1Rdzce9hjTNDQBO8GlhMnntARUy7zD95eBGEXqsgz/FlTu8UCr2ZQ+pj0QKQDuUImNR9OFon6v5yVwqI9H+OtHc1JmvBq4Ep3E2AhEemhwA5XsOaBwlAsEQ+UgB0EhemCogi7ZkHWaPXJt2ZtRsIvSPwMAFyvIY1L/qHZjH8+xS6RwQ6NoLN4QfO341yUUk61QAAAABJRU5ErkJggg==";
        private static string Icon64Step1Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAKJSURBVHhe7ZpNbsMgEIXjnqzddtOzJPuqjbpPztJNt+nNXIZiyZ0C85PBQMInRViJA8PjwYCT3WAwuGumUCaZ53l3OBze3eXb7zt6zucz2d7WZAOy7DywhQDSmB9CGWWafLwmnd8K6YBlBegU0YCRltzv93O49LQ4j9fgeCluXoDT6eTjDdP5Hw+waORet84ktUwNB8BAaLMROCA1+oCZAItbJIFyxXQxqlMx1YZpFrDcMyBK1OkxE6DHPQNAWlCSBUqtJ9fEQLVRVABuBylwvbmFDd+bYolNLIAEjgCcVOvWltsVAJC2QQjAWoiX2DiL4DGURdButmLfg/ecOCAAO2ZyhKBSbXrjTgFscQpwAJQpF0i4voYV2MolBbDoPNCEAECuQ5p6uRQVIIYm+CEAo14t1k+EimaMEpgKwElBy5wXUkxY0ylAAZ3XrN4hU5Q6aQ4Ggztm00UwxvPrZ3Zx+/p4YcXo6vmTXtbfQ589uc++w7X5PkDD1iv7xQnyGK5tHMBNU7FDDB45TAEHLHgnmDiAm6OtTnBGeCdYTYFeNyiXFtaAqph4Ep/Wlic2mNgUsFoDcuTaKOIA6Gjs1SJkVJqDiOTMTznAcXQugPZVuPqzsZOBOnuLOg8YC1AUzhTodYVnMbJAKJNwHkhy7knR7BRQPrrywHdzr5ZIjhQECqkrluNxSsP3UKwdUtsB4r/IDAHiAqj3CRkBrsr/a1wbyfjEWQB3HnCiQAOmj66tOg/k6qr+N7mUAyzOAGtS7ZCNYIGWg07MCRq6E6CGA1L3UHDqGDvBUCaRrhEYyjG1p8DdO4AjQHc/eUsgBSiR41vCdJ5pGFmgMl04wILhgARDgFDWpGKG2R1/AOQKw3riJ/oyAAAAAElFTkSuQmCC";

        private static string Icon32Step2Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGTSURBVFhHzZbBTsMwDIZbtAeDK5e9S3tGbOLcvgsXrtubgf9gV47jtmkSMT4patxssfPbztYPw3Dpuu6dRhPmee55msUTjWbOiyAFvjGIYOMpc4usra2XAAUW9MbamQyN966EKIBH8P8C6Ps+jFxqU4E2DN+epil4Fed6UxuQdTiO42Yrb7VmqxQUtTIOUqQA0OukQDC8kwYnGyldAmjB0VsQPLwLoptQy2rn2rbIHmweYrcGYHtzjdTAGn/RBVd+HqdGvhac+FnM69tndAl9fZwTuekz4YBYkznxQvZ9SYEtNLH1WKH0/8SNgnnOUkCuWkrV7wumpO8Nt9wirPrXBOm91IDdAPQ1ilaVsXZ6yMrTBG/NvQfonfvrJp8BEpgqqiISBbjYqiQ/QhKAJzmbERtdcYikC7AxVT1bMc6VGwW3VmgWnbZFAWwug19pyq/aHaAANt/MOaXhQqlBYe6yV5RWpUQyTkHYxKt6i3ZYlYJcWhWfkP1jpB3L3FOlaQostiURQNMUwDmfKqvqWYmKDumuPykCJGQRG2aMAAAAAElFTkSuQmCC";
        private static string Icon48Step2Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAHMSURBVGhD7ZhBboUgEIb1tQdrt930LrpuWtO13qWbbtudB/IAdv4GDfoAYYRBE7+EiOShM87PDLxyHMeiruuPoijeqYnRdV2purt4GIZB3HjQ932juru4URM3PiZlVVWj6kcLqySIwKm5HMjN5UBuTu+ASBoNrfYhdohEIOVWRUpCyaq9iIT0d7RtWxLqbj/iizim8WARAQliR/mqA7kRX8Sx3yHugA8hdkhJKMrx0YSIA5T7UYmTOCEiIYD9UOwaAMSyUArjQfIIvLx9GTdy35+v1nfRnNkm/Xfa+DON/6IjEYEUG7kfcuYJnTMXsn8nWBLyPaDgebocdHZKaIYVgRz/pdrgSiip8fjqU1NDVlgSCpljkxDRkIGIpBf0HGPUnQ74aH3rhOVwIApOCR1J6za21sChjQfeixjympoaOgTcLDSTao/jy6O6stGTwBpLtIKyj44pEy0igKyjN53pXl3Ze3uu8cA0d+EAZZ1Rb2p4BsZDMikPKKGgDtyFxQTkMDkQwroOTNU1tD7Y5t1CvmbuBWvCaRFnm7HGFgEudxFQ19NyegeySSjWIvZ2wIXLudwOeKVY15ba5gCX9fOca8A3xW6k14QFr2j+ABV6FubSDELBAAAAAElFTkSuQmCC";
        private static string Icon64Step2Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIoSURBVHhe7ZtLcoMwDIahvVi77aZ3gXWnZbqGu3TTbbvjQByAWozCECcY4QcSlb8ZD56QGPtHkmVDynEci7quP4qieDdFHF3XlVhNwuMwDGIHD/R932A1CQ+miB38EYAAqimrqhqxPpHa56Sh3gKyAHhUSxYAj2rJAuBRLeoFEJEIhS7IQvoswgJCV6MgoC9SXIBtQZZjgIQYQOmDy8zL0r/Lp7EAGORaCSG7gG1+Emjb1tzYYzxRvQVkAfAoiqPMH7g7DV6mnKM6wjkV37UAGPiRd4ETkYnQXkL6nIMgHrlJ+vjLhQgBTOIDy2EWEUTEAE7UxwB2C3h5+3LuBn1/vpL6Y9q5Gsfyd9a5Z3PuF+siLODo3aAfI8gT1tW6wCyC5hgwiRAlBvhua8O1bN+1SRADrohiAZJfstoilguwDx7u+LLgx5tojgETUWKA3Qbs6WF1Zm15vRUDDI25o+BiXpj2ne6ZRIA9bRAESApJgL1R/kwCkGLAmaP8FtQg+C8HD6ifBbwEAB9fFvz4lOQ8gDILbH3HPk8F2nHMAkHz/xJXLiDWAmINHnC1FUsAtl3dUG4EAHO2C56asd/W4NzVDeUmBlBYe36/FIb6aG0tBixXdL7ZIqUNsIBodw4GfSlnwetfYzHf4KBYQAzWrkO6iO0mMZMfbgHUJ0LqBTiFC6x9ZwvqLKAaLwugApYCs4xrxshBkBmqAF7Jkp0yS4QkQOJcn3ENUTR/384rX2bGKBkAAAAASUVORK5CYII=";

        private static string Icon32Step3Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGuSURBVFhHvZVBcoMwDEVDJwdrt93kLrDutJmu4S7ddJvejOo7FiNkyTaG5M0w2IFY0te36U4HM89zHN0ZhuGLbp/3WcpLvK/Qi+zEDQ4SBTh417WLIwsgBcJkmiYzlpsAqEmi7/usxIyVAEha0FB5MfhmtniAFJhxxWlC6bnbAigRelRQhBeHxFbi7AGPpQX4s1zAWkxT8w5xjXeTpTxerLRvPTyT5UBM6xw43lQOaK+lQBiM4+hWJH0hPRB+2EhQoLKXDyFkLRPQCshq+T1LgVbMbwGjg/NcKZZ1eYlk70oFnIALMsEWsG5WAS/wUaCAzuuh9AAnIscA8/ePn9W58ft9SWShd8Kf8IzHxBvN/7IKAK1CyDpekdZz40bJvJ55/3Kg0tlNii0Vt+59wa2ogMHmiiG91RpQTEBIvWqHVz1kjcME69myiG4Bm5Dm5seJExCmaqKmBa0mq6LaA6j4ANMlnON9Mzg/dEKe0TSyba4C8IKzJXed/RpLAQRw+05Vw5S4TEqm1Cq5kuld4fVfBjy0Bc+i2oT6mwDkIcU8rAUWaMszWlBy/Y5dcbr+AwtN/vR7Cx4dAAAAAElFTkSuQmCC";
        private static string Icon48Step3Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIvSURBVGhD1ZexUsMwDIbbHhNPBSsL79LMHPSY03dhYYWnYg36g91TXCuWHNu4353PaWonkvVLdva7jpimae6HYXij7nX+keDg+m6wGA+6c4BQGw+6kxBF4E9HxDiOSfuqOnA8Hu/JiB+rLDxwgHC/roHDVR1wK5plPEg5AFrkQJbxWlpEwKTpcHwPEVgAg2JNAouw1ppHQDIWYwEfr6HHfeDkehVRB3iItjQLPjIUJVQttRPRePqXbymBMVJJaXUaiBIqbbwGOGdpYC0HmhqfiyihsHq4SzPaKpQD7FRXIR46basN3tG8jGLVSja1hHJWNHxODVpEwLQxWanugHVjslJdQrWJRiA0Nsd4gHm1W/MqVJo9fbcm43w+n/NCQDy9fESPJJ/vz+Izac7FJj6O3X+k+9+4aBGBGkeSL3LmARe3LKHZCTGMXFoaCbnKtZAL5nE5cDZK6EKxCITGt6KkhIoZj1X3zd0SqZIDkE5KdiQHREyNNF58iZQD0u7Kd24+XsqBUswRsG750DsM5s391RyzhGA8dc2TVeKA1ceZwkA3xoOD0fgFPllTCVuTO9cXxRcAwbETlUdTBfK4SrRQwCIHICffONL9ANVHS67xIDZ34QA/Z3M01ab2l5eEqF2SwVW4Qta0j2hhIcJ9wO+u1v1Bmhcto3j51hXdUhwsmN9iPaVKEchFFYFb4uYd+DcJlUriTQ6kgIM9OpAsr541B3IJn2fOgYzyWnFz251+AXl70fqTvZAzAAAAAElFTkSuQmCC";
        private static string Icon64Step3Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAJGSURBVHhe7ZnNUoQwDMdBX0yvXnyX3bOjjGd4Fy9e9bYPtA+ADQan1G1Dm9APym+GKVMWSkLyb9ptm8IZx7E5n89v6vT1t8ePoh3ANR64w7ZI2nb6fsHGA0U7QILiNeB0Oo14OjEMg5dN2UQA5DMcsUkeARJCpuMbAffYJuN6vYoZD1wulw5PV5FDCogZH8IxC2CbDK6K72YWSMXuIsCX6iNgDw7wmvZMindA3/dQRwQ7gcw36UqNwlcDuDgjILbxKXA6QGK9nTvVzwJkvnHnaYqtn09RfQQcDsC2WrLTgNgcKYDtP2JsUOIYrFqeS7IImB3MreW5WPN5fkFVCi9CQVIDYAysNpNhjYAYL5baeOAQQWyrpfWdh6XrgKeXD+dy+/P9edV46jkLO/T7jGuP6to3nmcRAbGX21/KIQ94Xm0K/DmhZg2YnEDmF3ctAHO9bVsNnmXmrskGGrBg8wjIfU8xRgpEMR6+uH5gN0nNGjAR3QGQ9/OBXU5U/kIKBUPdT74EVwSp+ykR3Bp2BMyrxlu4ruUCKwK0JfNqpd9dBJT+15mECBZrPHBMg9iKATmuH9idLWwRpPYMzft14LcOEexURceqAWawFriZqsERsPUUJ2U84HpWsANgQ3PlpmbSfX8KbwdASOsHdltJve9P4a0BFL7CZ9MAfUUXWiytecaaCMg6hLmQXwuV3qqiJltEgAS2cdiDmClSmgPEC6HSqN4BRaSA7TcUUrPArhGPAB9ca4FDBCMh4YCiCyW2AwRq/YQObLofyDU8WGwVCmIAAAAASUVORK5CYII=";


        private static string Icon16SaveBase64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAB1SURBVDhPYywsLGxgYGCoB2Iw6O/vZ4Qy4QCo5j+IBsnB2EDgCOQfYAIy4JpJBPuBhjmADKAE7CfKAJDTsXkNBDAMADkLysQA2OSQA4UsQGkYUG4AihdwBRQ6QNZDsQtYoDQcEApUdFeOegHihUYIkxzA0AgAg2E1fE0CEN8AAAAASUVORK5CYII=";
        private static string Icon24SaveBase64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACmSURBVEhL7ZQNCoAgDIVXdLA6kicoT+CR6ma1lYU214+0IOgDacjcY71pYYzpAKDFFeGcK3zIwDOjD6O8YL/B/YGCEhcr/gA9itUUkIAWs4imANFrCwCZvBkWcmSyRKqW2AEm03RdRsoXO3gKdQ9+gVNSJlsc0VsTtOInKXp6WAe5xYnUWfVfVPkv4+79kG7++ybnvEEh+3rfvwe/yYyUyXYJNQA7AbQ0QK1sxcbyAAAAAElFTkSuQmCC";
        private static string Icon32SaveBase64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADBSURBVFhH7ZVtCoMwDIbjTqa/t8v0BK4n6GX2X2/mkhqGX41FLOkgDxReKW1e0iQ2zrk3APS4DgkhNCxF8J6JZWR5brPX4d7IGh64ksELMaChlnU0oMHPhJYBIprQNEAM2gaAumBVvVtyu0BCiqGegVMD6P7D8hJ4nuZMktMnKE39T1AaM2AGpDb0OAXFHs6FZ8lz/lqTzMBdwQm868Vyh9VAsgaWf8Gr4zrnjv/IwB1UmwEzYF1gXVCFAT9LDcB/AdJlT6191EDNAAAAAElFTkSuQmCC";

        private static VectSharp.Page GetSaveIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon16SaveBase64, ref Icon24SaveBase64, ref Icon32SaveBase64, 16);
        }

        private static VectSharp.Page GetStep1Icon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Step1Base64, ref Icon48Step1Base64, ref Icon64Step1Base64, 32);
        }

        private static VectSharp.Page GetStep2Icon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Step2Base64, ref Icon48Step2Base64, ref Icon64Step2Base64, 32);
        }

        private static VectSharp.Page GetStep3Icon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Step3Base64, ref Icon48Step3Base64, ref Icon64Step3Base64, 32);
        }

        private static VectSharp.Page GetIcon(double scaling, ref string icon32, ref string icon48, ref string icon64, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon32);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon48);
            }
            else
            {
                bytes = Convert.FromBase64String(icon64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            VectSharp.RasterImage icon;

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

            VectSharp.Page pag = new VectSharp.Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        private Control GetLoadedTreesPage()
        {
            Grid mainContainer = new Grid() { Margin = new Avalonia.Thickness(25, 0, 0, 0) };
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            mainContainer.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Save loaded tree(s)" });

            StackPanel descriptionPanel = new StackPanel();

            descriptionPanel.Children.Add(new TextBlock()
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Text = "Saves all the trees that have been loaded, possibly including all the active Transformer, Further transformations, Coordinates and Plot action modules.",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            });

            Grid.SetRow(descriptionPanel, 1);

            mainContainer.Children.Add(descriptionPanel);

            ScrollViewer contentScroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 17, 0), AllowAutoHide = false };
            Grid.SetRow(contentScroller, 2);
            mainContainer.Children.Add(contentScroller);

            StackPanel contentContainer = new StackPanel();
            contentScroller.Content = contentContainer;

            contentContainer.Children.Add(new TextBlock() { Text = "File format", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 5) });

            WrapPanel fileFormatContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFormatContainer);

            List<Button> FileTypeButtons = new List<Button>();

            for (int i = 0; i < FileFormats.Count; i++)
            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Width = 340, Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");
                FileTypeButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5) };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = FileFormats[i].Item1, Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = FileFormats[i].Item3, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(TreeViewer.Icons.GetIcon32(FileFormats[i].Item2)) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                fileFormatContainer.Children.Add(brd);

                int index = i;

                brd.Click += (s, e) =>
                {
                    this.SelectedFileType = index;
                };
            }

            contentContainer.Children.Add(new TextBlock() { Text = "File features", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 5, 0, 0) });

            WrapPanel fileFeaturesContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFeaturesContainer);

            CheckBox includeAttachmentsBox = new CheckBox() { Content = "Include attachments", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeAttachmentsBox);

            CheckBox includeModulesBox = new CheckBox() { Content = "Include all active modules", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeModulesBox);

            CheckBox signFileBox = new CheckBox() { Content = "Sign file", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(signFileBox);

            CheckBox taxaBlockBox = new CheckBox() { Content = "Include a Taxa block and a Translate statement", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxaBlockBox);

            CheckBox taxonQuotesBox = new CheckBox() { Content = "Enclose taxon names within single quotes", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxonQuotesBox);

            StackPanel saveButtonContent = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            saveButtonContent.Children.Add(new DPIAwareBox(GetSaveIcon) { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center });
            saveButtonContent.Children.Add(new TextBlock() { Text = "Save", Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Black });

            Button saveButton = new Button() { Content = saveButtonContent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 5, 0, 0), Width = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
            saveButton.Classes.Add("SideBarButton");
            contentContainer.Children.Add(saveButton);

            taxaBlockBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    if (this.SelectedFileType == 1)
                    {
                        taxonQuotesBox.IsEnabled = taxaBlockBox.IsChecked == true;
                        taxonQuotesBox.IsChecked = taxaBlockBox.IsChecked == true;
                    }
                }
            };

            this.PropertyChanged += (s, change) =>
            {
                if (change.Property == SavePage.SelectedFileTypeProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        FileTypeButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        FileTypeButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = false;
                            taxonQuotesBox.IsChecked = false;
                            break;

                        case 1:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = true;
                            taxaBlockBox.IsChecked = true;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 2:
                            includeAttachmentsBox.IsEnabled = false;
                            includeAttachmentsBox.IsChecked = false;

                            includeModulesBox.IsEnabled = false;
                            includeModulesBox.IsChecked = false;

                            signFileBox.IsEnabled = false;
                            signFileBox.IsChecked = false;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 3:
                        case 4:
                        case 5:
                            includeAttachmentsBox.IsEnabled = false;
                            includeModulesBox.IsEnabled = false;
                            signFileBox.IsEnabled = false;
                            taxaBlockBox.IsEnabled = false;
                            taxonQuotesBox.IsEnabled = false;

                            includeAttachmentsBox.IsChecked = false;
                            includeModulesBox.IsChecked = false;
                            signFileBox.IsChecked = false;
                            taxaBlockBox.IsChecked = false;
                            taxonQuotesBox.IsChecked = false;
                            break;
                    }
                }
            };

            saveButton.Click += async (s, e) =>
            {
                List<FileDialogFilter> filters = new List<FileDialogFilter>() { };

                string extension = ".tbi";

                switch (this.SelectedFileType)
                {
                    case 0:
                        filters.Add(new FileDialogFilter() { Name = "Binary tree format", Extensions = new List<string>() { "tbi" } });
                        extension = ".tbi";
                        break;

                    case 1:
                        filters.Add(new FileDialogFilter() { Name = "NEXUS format", Extensions = new List<string>() { "nex" } });
                        extension = ".nex";
                        break;

                    case 2:
                        filters.Add(new FileDialogFilter() { Name = "Newick format", Extensions = new List<string>() { "tre", "nwk" } });
                        extension = ".tre";
                        break;

                    case 3:
                        filters.Add(new FileDialogFilter() { Name = "Newick with attributes format", Extensions = new List<string>() { "nwka" } });
                        extension = ".nwka";
                        break;

                    case 4:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 text", Extensions = new List<string>() { "asn" } });
                        extension = ".asn";
                        break;

                    case 5:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 binary", Extensions = new List<string>() { "asnb" } });
                        extension = ".asnb";
                        break;
                }

                filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

                SaveFileDialog dialog = new SaveFileDialog() { Filters = filters, Title = "Save tree(s)" };

                string result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                if (!string.IsNullOrEmpty(result))
                {
                    bool saveResult = await SaveFile(0, result, extension, includeModulesBox.IsChecked == true, signFileBox.IsChecked == true, includeAttachmentsBox.IsChecked == true, taxaBlockBox.IsChecked == true, taxonQuotesBox.IsChecked == true);

                    if (saveResult)
                    {
                        this.FindAncestorOfType<RibbonFilePage>().Close();
                    }
                }
            };

            this.PropertyChanged += (s, e) =>
            {
                if (e.Property == SavePage.IsVisibleProperty && (bool)e.NewValue)
                {
                    MainWindow parent = this.FindAncestorOfType<MainWindow>();

                    if (parent != null && parent.Trees != null)
                    {
                        bool oneTree = parent.Trees.Count == 1;

                        if (oneTree)
                        {
                            FileTypeButtons[4].IsEnabled = true;
                            FileTypeButtons[5].IsEnabled = true;
                        }
                        else
                        {
                            FileTypeButtons[4].IsEnabled = false;
                            FileTypeButtons[5].IsEnabled = false;
                        }
                    }
                }
            };

            return mainContainer;
        }

        private Control GetTransformedTreePage()
        {
            Grid mainContainer = new Grid() { Margin = new Avalonia.Thickness(25, 0, 0, 0) };
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            mainContainer.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Save transformed tree" });

            StackPanel descriptionPanel = new StackPanel();

            descriptionPanel.Children.Add(new TextBlock()
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Text = "Saves the first transformed tree, possibly including all the active Further transformations, Coordinates and Plot action modules.",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            });

            Grid.SetRow(descriptionPanel, 1);

            mainContainer.Children.Add(descriptionPanel);

            ScrollViewer contentScroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 17, 0), AllowAutoHide = false };
            Grid.SetRow(contentScroller, 2);
            mainContainer.Children.Add(contentScroller);

            StackPanel contentContainer = new StackPanel();
            contentScroller.Content = contentContainer;

            contentContainer.Children.Add(new TextBlock() { Text = "File format", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 5) });

            WrapPanel fileFormatContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFormatContainer);

            List<Button> FileTypeButtons = new List<Button>();

            for (int i = 0; i < FileFormats.Count; i++)
            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Width = 340, Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");
                FileTypeButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5) };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = FileFormats[i].Item1, Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = FileFormats[i].Item3, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(TreeViewer.Icons.GetIcon32(FileFormats[i].Item2)) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                fileFormatContainer.Children.Add(brd);

                int index = i;

                brd.Click += (s, e) =>
                {
                    this.SelectedFileType = index;
                };
            }

            contentContainer.Children.Add(new TextBlock() { Text = "File features", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 5, 0, 0) });

            WrapPanel fileFeaturesContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFeaturesContainer);

            CheckBox includeAttachmentsBox = new CheckBox() { Content = "Include attachments", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeAttachmentsBox);

            CheckBox includeModulesBox = new CheckBox() { Content = "Include all active modules", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeModulesBox);

            CheckBox signFileBox = new CheckBox() { Content = "Sign file", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(signFileBox);

            CheckBox taxaBlockBox = new CheckBox() { Content = "Include a Taxa block and a Translate statement", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxaBlockBox);

            CheckBox taxonQuotesBox = new CheckBox() { Content = "Enclose taxon names within single quotes", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxonQuotesBox);

            StackPanel saveButtonContent = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            saveButtonContent.Children.Add(new DPIAwareBox(GetSaveIcon) { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center });
            saveButtonContent.Children.Add(new TextBlock() { Text = "Save", Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Black });

            Button saveButton = new Button() { Content = saveButtonContent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 5, 0, 0), Width = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
            saveButton.Classes.Add("SideBarButton");
            contentContainer.Children.Add(saveButton);

            taxaBlockBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    if (this.SelectedFileType == 1)
                    {
                        taxonQuotesBox.IsEnabled = taxaBlockBox.IsChecked == true;
                        taxonQuotesBox.IsChecked = taxaBlockBox.IsChecked == true;
                    }
                }
            };

            this.PropertyChanged += (s, change) =>
            {
                if (change.Property == SavePage.SelectedFileTypeProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        FileTypeButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        FileTypeButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = false;
                            taxonQuotesBox.IsChecked = false;
                            break;

                        case 1:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = true;
                            taxaBlockBox.IsChecked = true;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 2:
                            includeAttachmentsBox.IsEnabled = false;
                            includeAttachmentsBox.IsChecked = false;

                            includeModulesBox.IsEnabled = false;
                            includeModulesBox.IsChecked = false;

                            signFileBox.IsEnabled = false;
                            signFileBox.IsChecked = false;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 3:
                        case 4:
                        case 5:
                            includeAttachmentsBox.IsEnabled = false;
                            includeModulesBox.IsEnabled = false;
                            signFileBox.IsEnabled = false;
                            taxaBlockBox.IsEnabled = false;
                            taxonQuotesBox.IsEnabled = false;

                            includeAttachmentsBox.IsChecked = false;
                            includeModulesBox.IsChecked = false;
                            signFileBox.IsChecked = false;
                            taxaBlockBox.IsChecked = false;
                            taxonQuotesBox.IsChecked = false;
                            break;
                    }
                }
            };

            saveButton.Click += async (s, e) =>
            {
                List<FileDialogFilter> filters = new List<FileDialogFilter>() { };

                string extension = ".tbi";

                switch (this.SelectedFileType)
                {
                    case 0:
                        filters.Add(new FileDialogFilter() { Name = "Binary tree format", Extensions = new List<string>() { "tbi" } });
                        extension = ".tbi";
                        break;

                    case 1:
                        filters.Add(new FileDialogFilter() { Name = "NEXUS format", Extensions = new List<string>() { "nex" } });
                        extension = ".nex";
                        break;

                    case 2:
                        filters.Add(new FileDialogFilter() { Name = "Newick format", Extensions = new List<string>() { "tre", "nwk" } });
                        extension = ".tre";
                        break;

                    case 3:
                        filters.Add(new FileDialogFilter() { Name = "Newick with attributes format", Extensions = new List<string>() { "nwka" } });
                        extension = ".nwka";
                        break;

                    case 4:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 text", Extensions = new List<string>() { "asn" } });
                        extension = ".asn";
                        break;

                    case 5:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 binary", Extensions = new List<string>() { "asnb" } });
                        extension = ".asnb";
                        break;
                }

                filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

                SaveFileDialog dialog = new SaveFileDialog() { Filters = filters, Title = "Save tree(s)" };

                string result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                if (!string.IsNullOrEmpty(result))
                {
                    bool saveResult = await SaveFile(1, result, extension, includeModulesBox.IsChecked == true, signFileBox.IsChecked == true, includeAttachmentsBox.IsChecked == true, taxaBlockBox.IsChecked == true, taxonQuotesBox.IsChecked == true);

                    if (saveResult)
                    {
                        this.FindAncestorOfType<RibbonFilePage>().Close();
                    }
                }
            };

            return mainContainer;
        }

        private Control GetFinalTreePage()
        {
            Grid mainContainer = new Grid() { Margin = new Avalonia.Thickness(25, 0, 0, 0) };
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            mainContainer.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            mainContainer.Children.Add(new TextBlock() { FontSize = 20, Foreground = new SolidColorBrush(Color.FromRgb(0, 114, 178)), Text = "Save final tree" });

            StackPanel descriptionPanel = new StackPanel();

            descriptionPanel.Children.Add(new TextBlock()
            {
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                TextWrapping = TextWrapping.Wrap,
                Text = "Saves the final transformed tree, possibly including all the active Coordinates and Plot action modules.",
                Margin = new Avalonia.Thickness(0, 0, 0, 5)
            });

            Grid.SetRow(descriptionPanel, 1);

            mainContainer.Children.Add(descriptionPanel);

            ScrollViewer contentScroller = new ScrollViewer() { HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, Padding = new Thickness(0, 0, 17, 0), AllowAutoHide = false };
            Grid.SetRow(contentScroller, 2);
            mainContainer.Children.Add(contentScroller);

            StackPanel contentContainer = new StackPanel();
            contentScroller.Content = contentContainer;

            contentContainer.Children.Add(new TextBlock() { Text = "File format", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 0, 0, 5) });

            WrapPanel fileFormatContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFormatContainer);

            List<Button> FileTypeButtons = new List<Button>();

            for (int i = 0; i < FileFormats.Count; i++)
            {
                Button brd = new Button() { Margin = new Thickness(0, 5, 5, 0), Width = 340, Height = 50, Padding = new Thickness(0), Background = Brushes.Transparent };
                brd.Classes.Add("SideBarButtonNoForeground");
                brd.Classes.Add("SideBarButtonNoForegroundHighlight");
                FileTypeButtons.Add(brd);

                Grid grd = new Grid() { Margin = new Thickness(5) };
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(40, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                TextBlock titleBlock = new TextBlock() { Text = FileFormats[i].Item1, Foreground = Brushes.Black, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(titleBlock, 1);

                TextBlock subTitleBlock = new TextBlock() { Text = FileFormats[i].Item3, Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, FontSize = 12, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetRow(subTitleBlock, 1);
                Grid.SetColumn(subTitleBlock, 1);

                grd.Children.Add(titleBlock);
                grd.Children.Add(subTitleBlock);

                DPIAwareBox filetypeIcon = new DPIAwareBox(TreeViewer.Icons.GetIcon32(FileFormats[i].Item2)) { Width = 32, Height = 32, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRowSpan(filetypeIcon, 2);
                grd.Children.Add(filetypeIcon);

                brd.Content = grd;

                fileFormatContainer.Children.Add(brd);

                int index = i;

                brd.Click += (s, e) =>
                {
                    this.SelectedFileType = index;
                };
            }

            contentContainer.Children.Add(new TextBlock() { Text = "File features", FontSize = 16, FontWeight = FontWeight.Bold, Margin = new Thickness(0, 5, 0, 0) });

            WrapPanel fileFeaturesContainer = new WrapPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal };
            contentContainer.Children.Add(fileFeaturesContainer);

            CheckBox includeAttachmentsBox = new CheckBox() { Content = "Include attachments", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeAttachmentsBox);

            CheckBox includeModulesBox = new CheckBox() { Content = "Include all active modules", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(includeModulesBox);

            CheckBox signFileBox = new CheckBox() { Content = "Sign file", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(signFileBox);

            CheckBox taxaBlockBox = new CheckBox() { Content = "Include a Taxa block and a Translate statement", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxaBlockBox);

            CheckBox taxonQuotesBox = new CheckBox() { Content = "Enclose taxon names within single quotes", IsChecked = true, FontSize = 13, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Width = 340, Margin = new Thickness(0, 5, 5, 0) };
            fileFeaturesContainer.Children.Add(taxonQuotesBox);

            StackPanel saveButtonContent = new StackPanel() { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            saveButtonContent.Children.Add(new DPIAwareBox(GetSaveIcon) { Width = 16, Height = 16, VerticalAlignment = VerticalAlignment.Center });
            saveButtonContent.Children.Add(new TextBlock() { Text = "Save", Margin = new Thickness(5, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.Black });

            Button saveButton = new Button() { Content = saveButtonContent, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 5, 0, 0), Width = 100, HorizontalContentAlignment = HorizontalAlignment.Center };
            saveButton.Classes.Add("SideBarButton");
            contentContainer.Children.Add(saveButton);

            taxaBlockBox.PropertyChanged += (s, e) =>
            {
                if (e.Property == CheckBox.IsCheckedProperty)
                {
                    if (this.SelectedFileType == 1)
                    {
                        taxonQuotesBox.IsEnabled = taxaBlockBox.IsChecked == true;
                        taxonQuotesBox.IsChecked = taxaBlockBox.IsChecked == true;
                    }
                }
            };

            this.PropertyChanged += (s, change) =>
            {
                if (change.Property == SavePage.SelectedFileTypeProperty)
                {
                    int oldValue = (int)change.OldValue;
                    int newValue = (int)change.NewValue;

                    if (oldValue >= 0)
                    {
                        FileTypeButtons[oldValue].Classes.Remove("active");
                    }

                    if (newValue >= 0)
                    {
                        FileTypeButtons[newValue].Classes.Add("active");
                    }

                    switch (newValue)
                    {
                        case 0:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = false;
                            taxonQuotesBox.IsChecked = false;
                            break;

                        case 1:
                            includeAttachmentsBox.IsEnabled = true;
                            includeAttachmentsBox.IsChecked = true;

                            includeModulesBox.IsEnabled = true;
                            includeModulesBox.IsChecked = true;

                            signFileBox.IsEnabled = true;
                            signFileBox.IsChecked = true;

                            taxaBlockBox.IsEnabled = true;
                            taxaBlockBox.IsChecked = true;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 2:
                            includeAttachmentsBox.IsEnabled = false;
                            includeAttachmentsBox.IsChecked = false;

                            includeModulesBox.IsEnabled = false;
                            includeModulesBox.IsChecked = false;

                            signFileBox.IsEnabled = false;
                            signFileBox.IsChecked = false;

                            taxaBlockBox.IsEnabled = false;
                            taxaBlockBox.IsChecked = false;

                            taxonQuotesBox.IsEnabled = true;
                            taxonQuotesBox.IsChecked = true;
                            break;

                        case 3:
                        case 4:
                        case 5:
                            includeAttachmentsBox.IsEnabled = false;
                            includeModulesBox.IsEnabled = false;
                            signFileBox.IsEnabled = false;
                            taxaBlockBox.IsEnabled = false;
                            taxonQuotesBox.IsEnabled = false;

                            includeAttachmentsBox.IsChecked = false;
                            includeModulesBox.IsChecked = false;
                            signFileBox.IsChecked = false;
                            taxaBlockBox.IsChecked = false;
                            taxonQuotesBox.IsChecked = false;
                            break;
                    }
                }
            };

            saveButton.Click += async (s, e) =>
            {
                List<FileDialogFilter> filters = new List<FileDialogFilter>() { };

                string extension = ".tbi";

                switch (this.SelectedFileType)
                {
                    case 0:
                        filters.Add(new FileDialogFilter() { Name = "Binary tree format", Extensions = new List<string>() { "tbi" } });
                        extension = ".tbi";
                        break;

                    case 1:
                        filters.Add(new FileDialogFilter() { Name = "NEXUS format", Extensions = new List<string>() { "nex" } });
                        extension = ".nex";
                        break;

                    case 2:
                        filters.Add(new FileDialogFilter() { Name = "Newick format", Extensions = new List<string>() { "tre", "nwk" } });
                        extension = ".tre";
                        break;

                    case 3:
                        filters.Add(new FileDialogFilter() { Name = "Newick with attributes format", Extensions = new List<string>() { "nwka" } });
                        extension = ".nwka";
                        break;

                    case 4:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 text", Extensions = new List<string>() { "asn" } });
                        extension = ".asn";
                        break;

                    case 5:
                        filters.Add(new FileDialogFilter() { Name = "NCBI ASN.1 binary", Extensions = new List<string>() { "asnb" } });
                        extension = ".asnb";
                        break;
                }

                filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

                SaveFileDialog dialog = new SaveFileDialog() { Filters = filters, Title = "Save tree(s)" };

                string result = await dialog.ShowAsync(this.FindAncestorOfType<Window>());

                if (!string.IsNullOrEmpty(result))
                {
                    bool saveResult = await SaveFile(2, result, extension, includeModulesBox.IsChecked == true, signFileBox.IsChecked == true, includeAttachmentsBox.IsChecked == true, taxaBlockBox.IsChecked == true, taxonQuotesBox.IsChecked == true);

                    if (saveResult)
                    {
                        this.FindAncestorOfType<RibbonFilePage>().Close();
                    }
                }
            };

            return mainContainer;
        }


        private async Task<bool> SaveFile(int targetChoice, string result, string extension, bool includeModules, bool signModules, bool includeAttachments, bool translateBlock, bool quotes)
        {
            MainWindow window = this.FindAncestorOfType<MainWindow>();

            try
            {
                if (System.IO.File.Exists(result))
                {
                    System.IO.File.Delete(result);
                }

                if (targetChoice == 0)
                {
                    if (extension == ".nex")
                    {
                        using (FileStream fs = File.Create(result))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, leaveOpen: true))
                            {
                                if (includeModules)
                                {
                                    sw.WriteLine();

                                    sw.WriteLine("Begin TreeViewer;");
                                    string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.AllModules, signModules);
                                    sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                    sw.WriteLine(serializedModules);
                                    sw.WriteLine("End;");
                                }

                                if (includeAttachments)
                                {
                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin Attachment;");

                                        sw.WriteLine("\tName: " + kvp.Key + ";");
                                        sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                        sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                        kvp.Value.WriteBase64Encoded(sw);
                                        sw.WriteLine();
                                        sw.WriteLine("End;");
                                    }
                                }
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(ms))
                            {
                                NEXUS.WriteAllTrees(window.Trees, fs, true, null, translateBlock, quotes, reader);
                            }
                        }
                    }
                    else if (extension == ".tbi")
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                        {
                            if (!includeModules)
                            {
                                if (!includeAttachments)
                                {
                                    BinaryTree.WriteAllTrees(window.Trees, fs);
                                }
                                else
                                {

                                    string tempFile = System.IO.Path.GetTempFileName();
                                    using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                    {
                                        using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                        {
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }

                                            bw.Flush();
                                            bw.Write(ms.Position - 3);
                                        }

                                        ms.Seek(0, System.IO.SeekOrigin.Begin);

                                        BinaryTree.WriteAllTrees(window.Trees, fs, additionalDataToCopy: ms);
                                    }

                                    System.IO.File.Delete(tempFile);
                                }
                            }
                            else
                            {
                                string tempFile = System.IO.Path.GetTempFileName();
                                using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                {
                                    using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                    {
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write("#TreeViewer");
                                        bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.AllModules, signModules));

                                        if (includeAttachments)
                                        {
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }
                                        }

                                        bw.Flush();
                                        bw.Write(ms.Position - 3);
                                    }

                                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                                    BinaryTree.WriteAllTrees(window.Trees, fs, additionalDataToCopy: ms);
                                }

                                System.IO.File.Delete(tempFile);
                            }
                        }
                    }
                    else if (extension == ".tre" || extension == ".nwk")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            foreach (TreeNode tree in window.Trees)
                            {
                                sw.WriteLine(NWKA.WriteTree(tree, false, quotes));
                            }
                        }
                    }
                    else if (extension == ".nwka")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            foreach (TreeNode tree in window.Trees)
                            {
                                sw.WriteLine(NWKA.WriteTree(tree, true));
                            }
                        }
                    }
                    else if (extension == ".asn")
                    {
                        if (window.Trees.Count > 1)
                        {
                            await new MessageBox("Attention", "NCBI ASN.1 files can only contain one tree!").ShowDialog2(window);
                            return false;
                        }
                        else
                        {
                            NcbiAsnText.WriteTree(window.Trees[0], result, null, null);
                        }
                    }
                    else if (extension == ".asnb")
                    {
                        if (window.Trees.Count > 1)
                        {
                            await new MessageBox("Attention", "NCBI ASN.1 files can only contain one tree!").ShowDialog2(window);
                            return false;
                        }
                        else
                        {
                            NcbiAsnBer.WriteTree(window.Trees[0], result, null, null);
                        }
                    }
                }
                else if (targetChoice == 1)
                {
                    if (extension == ".nex")
                    {
                        using (FileStream fs = File.Create(result))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, leaveOpen: true))
                            {
                                if (includeModules)
                                {
                                    sw.WriteLine();

                                    sw.WriteLine("Begin TreeViewer;");
                                    string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, signModules);
                                    sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                    sw.WriteLine(serializedModules);
                                    sw.WriteLine("End;");
                                }

                                if (includeAttachments)
                                {
                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin Attachment;");

                                        sw.WriteLine("\tName: " + kvp.Key + ";");
                                        sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                        sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                        kvp.Value.WriteBase64Encoded(sw);
                                        sw.WriteLine();
                                        sw.WriteLine("End;");
                                    }
                                }
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(ms))
                            {
                                NEXUS.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, true, null, translateBlock, quotes, reader);
                            }
                        }
                    }
                    else if (extension == ".tbi")
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                        {
                            if (!includeModules)
                            {
                                if (!includeAttachments)
                                {
                                    BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs);
                                }
                                else
                                {

                                    string tempFile = System.IO.Path.GetTempFileName();
                                    using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                    {
                                        using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                        {
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }

                                            bw.Flush();
                                            bw.Write(ms.Position - 3);
                                        }

                                        ms.Seek(0, System.IO.SeekOrigin.Begin);

                                        BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, additionalDataToCopy: ms);
                                    }

                                    System.IO.File.Delete(tempFile);
                                }
                            }
                            else
                            {
                                string tempFile = System.IO.Path.GetTempFileName();
                                using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                {
                                    using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                    {
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write("#TreeViewer");
                                        bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, signModules));

                                        if (includeAttachments)
                                        {
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }
                                        }

                                        bw.Flush();
                                        bw.Write(ms.Position - 3);
                                    }

                                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                                    BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, additionalDataToCopy: ms);
                                }
                                System.IO.File.Delete(tempFile);
                            }
                        }
                    }
                    else if (extension == ".tre" || extension == ".nwk")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            sw.WriteLine(NWKA.WriteTree(window.FirstTransformedTree, false, quotes));
                        }
                    }
                    else if (extension == ".nwka")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            sw.WriteLine(NWKA.WriteTree(window.FirstTransformedTree, true));
                        }
                    }
                    else if (extension == ".asn")
                    {
                        NcbiAsnText.WriteTree(window.FirstTransformedTree, result, null, null);
                    }
                    else if (extension == ".asnb")
                    {
                        NcbiAsnBer.WriteTree(window.FirstTransformedTree, result, null, null);
                    }
                }
                else if (targetChoice == 2)
                {
                    if (extension == ".nex")
                    {
                        using (FileStream fs = File.Create(result))
                        using (MemoryStream ms = new MemoryStream())
                        {
                            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(ms, leaveOpen: true))
                            {
                                if (includeModules)
                                {
                                    sw.WriteLine();

                                    sw.WriteLine("Begin TreeViewer;");
                                    string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, signModules);
                                    sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                    sw.WriteLine(serializedModules);
                                    sw.WriteLine("End;");
                                }

                                if (includeAttachments)
                                {
                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin Attachment;");

                                        sw.WriteLine("\tName: " + kvp.Key + ";");
                                        sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                        sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                        kvp.Value.WriteBase64Encoded(sw);
                                        sw.WriteLine();
                                        sw.WriteLine("End;");
                                    }
                                }
                            }

                            ms.Seek(0, SeekOrigin.Begin);

                            using (StreamReader reader = new StreamReader(ms))
                            {
                                NEXUS.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, true, null, translateBlock, quotes, reader);
                            }
                        }
                    }
                    else if (extension == ".tbi")
                    {
                        using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                        {
                            if (!includeModules)
                            {
                                if (!includeAttachments)
                                {
                                    BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs);
                                }
                                else
                                {

                                    string tempFile = System.IO.Path.GetTempFileName();
                                    using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                    {
                                        using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                        {
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write((byte)0);
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }

                                            bw.Flush();
                                            bw.Write(ms.Position - 3);
                                        }

                                        ms.Seek(0, System.IO.SeekOrigin.Begin);

                                        BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, additionalDataToCopy: ms);
                                    }

                                    System.IO.File.Delete(tempFile);
                                }
                            }
                            else
                            {
                                string tempFile = System.IO.Path.GetTempFileName();
                                using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                {
                                    using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                    {
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write((byte)0);
                                        bw.Write("#TreeViewer");
                                        bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, signModules));

                                        if (includeAttachments)
                                        {
                                            bw.Write("#Attachments");
                                            bw.Write(window.StateData.Attachments.Count);

                                            foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                            {
                                                bw.Write(kvp.Key);
                                                bw.Write(2);
                                                bw.Write(kvp.Value.StoreInMemory);
                                                bw.Write(kvp.Value.CacheResults);
                                                bw.Write(kvp.Value.StreamLength);
                                                bw.Flush();

                                                kvp.Value.WriteToStream(ms);
                                            }
                                        }

                                        bw.Flush();
                                        bw.Write(ms.Position - 3);
                                    }

                                    ms.Seek(0, System.IO.SeekOrigin.Begin);

                                    BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, additionalDataToCopy: ms);
                                }

                                System.IO.File.Delete(tempFile);
                            }
                        }
                    }
                    else if (extension == ".tre" || extension == ".nwk")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            sw.WriteLine(NWKA.WriteTree(window.TransformedTree, false, quotes));
                        }
                    }
                    else if (extension == ".nwka")
                    {
                        using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                        {
                            sw.WriteLine(NWKA.WriteTree(window.TransformedTree, true));
                        }
                    }
                    else if (extension == ".asn")
                    {
                        NcbiAsnText.WriteTree(window.TransformedTree, result, null, null);
                    }
                    else if (extension == ".asnb")
                    {
                        NcbiAsnBer.WriteTree(window.TransformedTree, result, null, null);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                await new MessageBox("Error!", "Error while saving the tree(s):\n" + ex.Message).ShowDialog2(window);
                return false;
            }
        }

    }
}
