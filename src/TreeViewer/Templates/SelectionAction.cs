using PhyloTree;
using TreeViewer;
using VectSharp;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace @NamespaceHere
{
    // Do not change class name.
    public static class MyModule
    {
        public const string Name = "A name for your module.";
        public const string HelpText = "A very short description for your module.";
        public const string Author = "Your name";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // This property determines whether the action defined by this module can be performed when the program is run
        // in command-line mode or not.
        public static bool IsAvailableInCommandLine { get; } = false;

        // The following two properties determine the shortcut keys that can be used to perform the action without
        // having to click on the button.
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;

        // This property determines whether the shortcut keys should trigger this module's action even if a text box
        // is focused when they are pressed (e.g. you would not want to hook to the CTRL+A combination if a text box
        // is focused, because that is used to select all text in the text box).
        public static bool TriggerInTextBox { get; } = false;

        // A short label that will appear on the button for the action.
        public static string ButtonText { get; } = "Short text";

        // The name of the group of buttons in which the button for this module will appear. If multiple modules specify
        // the same group name, they will be placed together. The group name is also shown on the ribbon interface.
        public static string GroupName { get; } = "Group name";

        // The index of the module corresponding to this button. Within a group, the buttons are sorted based on their
        // GroupIndex; the various groups are sorted based on the elements with the lowest GroupIndex that they contain.
        public static double GroupIndex { get; } = 0;

        // If this is true, the button corresponding to this module will be a "large" button with a 32x32 device-independent
        // pixels icon. If this is false, the button will be "small", with an icon size of 16x16 device-independent pixels.
        public static bool IsLargeButton { get; } = true;

        // This property should return a list of tuples whose first element is the text of a "sub-item", while the second
        // element is a method that returns the icon for that sub-item. If this list is not empty, the button will have a
        // little "arrow" icon that can be used to open a menu containing all the sub-items. If the first sub-item's text
        // is empty, the button will also be clickable on its own (i.e., users will be able to click on the button, or to
        // expand the button and click on a sub-item in the menu); otherwise clicking on the button will only open the
        // sub-item menu.
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        // These variables hold a PNG icon at three resolutions (32x32px, 48x48px and 64x64px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsAAAA7AAWrWiQkAAAH/SURBVFhH7ZXLLwNRFMbPHcqCmBK0iLS6FBYkYsGKnUesWJP4C7oXRGLZNRs7GxskYiHRRT0SYSOS8Vh4RKIWgkVLkRpzrnNlium8WpHwS27uvWemc76553xT+OfPw2h2RDgcntCm8fedPSKRCM8t8Z1zHCX/VdgqQc/YskpLQ1anBlyVNStmAqwI/IyrHtCaUMVBW0dYEuA2STZMBZDVMsjFmwusnEBWq83MzoPkqW6nrW0c9QB+RMSHBFHTz9NORbhqQsFr+q4LRdxfnVLEOq4E4Ck8PDxe4xpFHEcXbJfDtgD0un4Ulch+ugQ3p9uuyvEtph3PyuLqJ5jkXbcqgjeSFUvpm06Av0MXJBO8ChkEAo3Q0dkGNf5KimSSq3/DSZq/cHGhwNbmLsSvbyjiECclEKSeXkzLkRMbGlFcVAh3t5fcokYi8ioAkeVSiF+dGIrIuwDE56uC87MDLoJCH1gWoCgKE4NC35JMpmilPbygPCpGsKE5SuEMsj4MMbOo3oaY3Outg8WlBejr7YZAsBFaW5ogFKrn1/XYsaGh1fSI5INDfTAyPMpjG7E12NSsmF80GyYSj9oplSVEkzGpfCUW2+FWxLVUWNXP780H6HN9coSvmXyIAhTlSGVM3qdLPwe+OSbnp8C8exT+OQo8/lo8BSZVzEkeXwuFfxsAbyVC/ITkqdrKAAAAAElFTkSuQmCC";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiEAABYhAbavo+IAAAMgSURBVGhD7ZfPaxNBFMffbptGC0lL29RtsD0oBT0IWkFF9KAXpVC89SQiePSW3gTRUvCWnPsHePHgoRg1ii5Yf5TWgxZRa9PfRlPbQDU0/WWTdV7yNk3iNt3fyWE/MMzMmwm8b96bebPg4ODg4GAEjnpLCAQCd1l3Jzczj1AolPebp94qTHe+FEsjoAYWJYmGqimMgCkCum8P6UmV/icDV/B3hjArhfSkiuXppRoWAQkbTfdE6/5yWHqIMb/lRibTsfoWshxHQKXRJQAr7G65LduV1qxAbwRUX4GDg/chvrhMM/OxJYXevB4DvrblNE1NxbAASSrOlGAwyMmNTLCwMAGZ7a17KKJ0v1EMC+C44tcIzkttiCT9uYgialw+UyNhSwrJyCJ+/5ghi3FsFYCgiG/iAzBLhGUCMI3w2bu2tr5IpjyJ2RFAEWYcbF3P6cI7Hp0s9zB7EboJm6vfaVYMxzWIfG3drcx2YpRMmjErAv3Ua0I+E0YiURQBPdWz8OtIGU9ckpICTRTp6DgC586fgjbBR5by2PZNjH9Iff3+ss4jc3MT2WKnp2JbfQupSi2eeTE1/UWXCFWHuLR69vX15Q2FFVepgKlJIZnNrb+wz92i6WCrioBcXZUcLLemFXedC1ZWYpoOttUppJnGRg8sLc2qFlF1AhCfrwlisUlVIjQLMCNV1OD3t8L8/Oc9RVRlBPDSSKfTIAitEI1+yIqgpf+ohAApldqg4Q74rJAbzzeKLlez6Hb7xM7OEyJtUURXPmip2PhJmUr9ohmw8QY7qH4YevQQui9fICtW46PQdfIYHD7UTpbdsa0SlyI739vbA9euXidrjrfvRFbItL/p9ArQ/HgrdF4QWsDb4IGx9x+za5jz7QfbsrbpGeWXawXxxNfXN1nKeVYLb5Pc2PM1k8kw/3NEo9O47xNtqRa8L5lTP11u4QwZduC84eHhUXI/B9q4muYe2lHd8K4DfowC+S4lk0kpEhExCuO0ZU/sqUpl8UbGx0cuTUxOwbPnryCxnIBw+OnxzPayahEVhZ2Fs0zEY45vusHGXWR2cLAHgH+5olmDOf9TaQAAAABJRU5ErkJggg==";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYQAAB2EASJc0AgAAAPNSURBVHhe7ZrPSxtBFMcnDQg9qDGxFw/FQmuh0lKPHgq9Ke2hPXkRb1UQ2x48Cm0qBY9KyUELPSleS4O0/QN6rQqlKZScGm+LiYoKphC27zs7q3GbTTa7s7+y+4HHzL6drHnfvHkzs8hiYmJiYmJiIktCtJ4zPz//lJpnZI+5wyOWl5cvxXxFtH7gefCN8FMA34MHfgoQCPysAarocoxzM8YjXFP90av8G2qy2pVjFr+8fYLnScfNGiAreCDzWd5AGaDCxKVtZD3HDN9WARTBehNuz4n8MhgLINrIEnkBHO8DqID9d6rDrk6v3LR+J6wUObOdYP1zuEMyMjIgEKc6u8gQwFHw9MszRVHElfcEogZsbm6iuc8vPEa6AI3mMnxGE7c4R0dHaL6T3UPHSwKRAd3d3RAhSd0dMk9FCIQAoKenxxcRAiMAMIhwlztdJlACAIhweHgIEXbJXBfBNwEaFUOd3t7ecxGOlZLmdAnHuyvjLk8PyuoZnnZ4TFUbDyURWF86wx7MvGPf3r9w/F0b4WYGLIrWNqlUih1UyhT8Syh0R/PKxbUMaANKgObJwjOhr69GXawOv7hTEr7VAAhn5ZAEeCYcHKAm/CCTmgnnv5bVL9MKqxmg/z0ab1oDjCATMpkMm5qa4q0djN8vcMtgM5AJ5XKZbWxssEqlIrzO8FOAz6JtC4iwv7/P1tfXpYjgpwAfyGyJQAVRmgihWAXMoMLI0un0X+pidfjNnW0SqhpgBJlANaGLulgdbnFnm4RaAEAZgOkAEX6StS1C6AUAWBLtitARAgCDCDe50wIdIwCoE6FAZkmEjhIA1ImAwthShI4TAAgRrlIXIjzkThNCKcDs7CxLJBJNrb+/H0Mhwid0zAi0ANVqlZ2cnIirC9bW1tBgw2XFUmSmYIAjnJ4izU6De3t7bHh4mJ2dnXERurowrTVGR0fZ4OAgGxgYEB7rhOI0qAc/MTHB21wuJ+5oYArs7ODFsXNkCGDrQGNGffB4OToyMsJWV1fFXY3JyUlWLBbZ6emp8NhHhgC2T3VGjMED7PdrtRrL5/P8GiSTSZbNZtnuLt6chx+cBtVSqYRC8IfsBvdeZnxsbIyP01EUBeMVMlT6UNMqeID3gcVCoSDC15iensbnZviIENMqeJ3nc3NzInQNmgL4LLa9oeYj2TWt2xQUhWNChK+BqUH+cT4iAuRWVlZE6Brb29sQ4KJCdji3h4aGROiqimxYWlqCAFUyK1nUEXzd2tpSFxYWEPgxGXZJ13EjKuCfsspkr8nScEQNLImhX/sDAmP/AOROsOSzCFmAAAAAAElFTkSuQmCC";

        // This method returns the icon for the module. This is used in the button associated to the menu action.
        // If IsLargeButton is false, the image should be 16x16 device-independent pixels. Otherwise, its size should be
        // 32x32 device-independent pixels. The scaling parameter can be used to determine the actual resolution of the
        // image (e.g. if scaling is 1, the image will be 16x16px, while if scaling is 1.5 the image needs to be 24x24px).
        // This method can return a vector image or a raster image embedded in a Page. If you wish to return a raster image,
        // you can just embed it by replacing the Icon16Base64 (16x16px), Icon24Base64 (24x24px), and Icon32Base64 (32x32px)
        // variables with Base-64 encoded images. If you wish to return a vector image, you can delete those variables and
        // rewrite the body of the GetIcon method to produce the icon.
        // Note that even when scaling is greater than 1, the Page that is returned by this method should have size 16x16 or
        // 32x32 (based on the value of IsLargeButton).
        public static Page GetIcon(double scaling)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(Icon32Base64);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(Icon48Base64);
            }
            else
            {
                bytes = Convert.FromBase64String(Icon64Base64);
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

            Page pag = new Page(32, 32);
            pag.Graphics.DrawRasterImage(0, 0, 32, 32, icon);

            return pag;
        }

        // This method determines whether the action is available for a given node that has been selected. It is
        // invoked every time the selected node changes.
        //
        // selection: the node in the tree that has been selected. This may be null if no node has been selected.
        //
        // window: the MainWindow that contains the plot. If the program is running in command-line mode and this
        //         module has signalled that it is available in command-line mode, this may be null.
        //
        // stateData: an InstanceStateData object that can be used to access features in way that does not depend
        //            on the program running in command-line or GUI mode.
        //
        // This method returns a list containing n+1 elements, where n is the number of sub-items for this module.
        // The first element of the list determines whether the button corresponding to this module is enabled or
        // not, while each subsequence element is associated to the corresponding sub-item.
        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            // TODO: check whether this module's action is applicable to the selected node or not.
            return new List<bool>() { selection != null };
        }

        // This method performs the actual action.
        //
        // actionIndex: the index of the sub-item that has been clicked by the user. If the module button has a
        //              "default action" (i.e. the first element of the list returned by SubItems is empty), this
        //              parameter will be -1 if the user clicks on the button, and values >= 0 if the user clicks
        //              on a sub-item. If the module button does not have a default action, this parameter will
        //              always be >= 0.
        //
        // selection: the node in the tree that has been selected. This may be null if no node has been selected.
        //
        // window: the MainWindow that contains the plot. If the program is running in command-line mode and this
        //         module has signalled that it is available in command-line mode, this may be null.
        //
        // stateData: an InstanceStateData object that can be used to access features in way that does not depend
        //            on the program running in command-line or GUI mode.
        public static void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            // TODO: do something.
        }
    }
}
