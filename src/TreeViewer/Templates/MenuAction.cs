using System.Threading.Tasks;
using TreeViewer;
using System;
using System.Collections.Generic;
using VectSharp;
using System.Runtime.InteropServices;

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
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // The text of the menu item that triggers this action.
        public static string ItemText { get; } = "Item text";

        // The name of the parent menu item in which the item should be placed (e.g. "File", "Edit", or "Help"). If
        // the parent menu does not exist, it will be created.
        public static string ParentMenu { get; } = "Edit";

        // The name of the group of buttons in which the button for this module will appear. If multiple modules specify
        // the same group name, they will be placed together. The group name is also shown on the ribbon interface.
        // In the native menu (displayed on macOS and some Linux environments), modules with the same group name will be
        // separated by other modules with different group names using separators.
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

        // This property determines the shortcut keys that can be used to perform the action without having to click
        // the button or on the menu item. This should be a list containing 1 (if the menu item has no sub-items), n (if the
        // menu item has sub-items and it does not have a default action), or n+1 (if the menu item has sub-items and a default
        // action) tuples, where n is the number of sub-items for this module. The first element of each tuple represents the
        // key, while the second element is the key modifier (e.g. CTRL or SHIFT). The first element of the list is the shortcut
        // for the default action of the module (if any), while each subsequence element is associated to the corresponding sub-item.
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };

        // This property determines whether the shortcut keys should trigger this module's action even if a text box
        // is focused when they are pressed (e.g. you would not want to hook to the CTRL+A combination if a text box
        // is focused, because that is used to select all text in the text box).
        public static bool TriggerInTextBox { get; } = false;

        // These variables hold a PNG icon at three resolutions (32x32px, 48x48px and 64x64px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGLSURBVFhHYywqKvrPQALo6+tjhDKpApig9ICBUQcMuAMYt2zZQlIiJBX4+PjgTbSjUTDqgIHPBV61G2maC7Y1+w/zXLCnP4fhw7N7UB7pgCohcHPfKrIdQRUHvLl3jGxHUC0NkOsIiuuCmNhMhvfvHkF5DAwGhtYMcXHhDOpqymA+3euCC+ePMixatJLh5q27UBH8gOoOAAFSHEETB4AAzBFMrGLmUCGsgGYOAIGTJw8y/P/7qw2fI2jqAHY2FmACfexEyBGUAUa+5/8JgBcvXv1nZBLYSxtHEOEAEHj48DHYEVBdcEB5JwPkgH8fJaA8hq9ffzBwc3OA2UzMgvvADCTw/98HZyiTSgApBL58+Q4s1Pi+bN6yB8wH+ngPE4uoL1QljQDUATDLQfHMyMh/GSQGDnYgG6qSRgDoAGTLwUJMglsOHToJcgMwFAS30DQUIAkLYTkIgNmM/NdBDrh27QYoFC5CpegHQD4HWQ4OBUaBM1Bh+gFmVgkpUCgwMgnNY2IVN4QKowEGBgB9yvgho1DEXgAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAI4SURBVGhD7ZrNSxtBFMBn1kYF2QhaUHottMeIBz96tAehpPRcCLTgpdBb7j0VeszfYE/iUVg1FhtoQIKJEIIHP1JbNKkpSYpliaQxsuu87IshqJhNd3cmML/Lzrxkw/vNm9lh2NBwOGwSB4hEIhSbnqLgtWeRAryRAryRAryhmqY5sg84RTAYtLWfyCnEGynAGynAGynAG/riw4pQG9nax1dyI+sppMB9bEbek7+/fmDPeTypwEFs2TUJTwTKPxOuSXi2BtyS8HQRuyHh+oksFHpHzs5y2LMIBGbJm7evydMnjzHSoidOZJlMgnxeXCIHh0cY6R4uAoBTEtwEACckuAoA/yvBXQBIp7uXEEJAYVmkdrYaEsqDh9MY7gghBIB+n0KSqTgxLi8+2ZEQRgAY6PexPSM/Z1fCZdSCaZNi8Q/bXP1fBZGwLwDk879FkehOwDAM8/g4J4KEfQFIvl6vm7VazcxmjxoS+GM38ODVKAjo49gBzPPzf3RoaBC7FpQOx7B5B/pzbHhNewUqlSobUbWyuh7DiAUb5U3aN/oSbxKJlkAzeTanp9h1F8MNTnKn8Nku3iQSlkB78gzq17aT6UbyMOcBiAlYBbVQrdaayV8/Tay2utdMHrAWrHBVgCeIeuobGJ/BQAs24vH4NqZvIWgVbkfxjT2CKmDupq7rZjQagypk8Cv3wuUfJu34o+xMML9/+J1sfPlGyqUy0bT1CeOy1LEEV9haeMYkVqkyssDakxjuEEKuAJhGDazvo6EMAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAL4SURBVHhe7ZvNaxNBFMBnFQQh30HwVJDU1j+h3ryl6IJ/RaWQD7HeCkpA6ak0h+SkXnLzJCkJ7R9gr6Eo5iSIJhdNm3bbgBASWN+beWlrtV/JbjKzsz94vOmbWTrv103YYamxtLRkMxdZW1szaCgl1yhriy+Asrb4Aihriy+Asrb4Aihri/YCjGq16uqjsNuYpjnSo7b/EaCsLb4AytriC6CsLb4AytriC6CsLdoLMB6+WFf6LLDx6rF/FhgFXwDliQG3MDv8+Y1+Gj9S3AFb755PTIIUAg4PLCHh13eqjA8pBASDQXZg7bOtt8/GLkGaL8FQKDQRCdIIQCYhQSoBCEqw9vfGJkE6AUg4HD6S0Gk1qOoOUgpAUML+Xpt9fPPUVQkTfy9gmiaz7bO3YFkWi8fjrFgssqmpKaoe4/n3ApFIhLXbbZZOp1mj4fydIL0AxE0JSghABhKy2ayjEpQRgKCEnZ0dLqHZbFJ1NJQSgKCEVqvFMpmMIxKUE4BEo9EjCcA9XhwSJQUgAwnAJ4hZHAyDsgIQlABfjDdg+BniLi9eEaUFILFYjO3u7qKELxBXlqC8AASfFIeV4AkByCkJ07yoCHAUcA6QgAeLLoQyEmjrzkESfkMoIYG27SwnJDzgv0ViaMuXZ3FxEZu7bFgQZyLDv7NgTzT8m263y3q9HgsEAlQRGAbftgx7dwT+Vz0NnPjQyg+IDoigqmBubg7n7uPFXoDaOuZE83cgCqurqzQjKJVKOP8ewhNQW4JTzSPTiUSCZgX9fh/X9CFu8xWKQ239t/kBm+VymVYJcrkcrn0tptWGN3RO88h8Mpnk6wbASRDX43HwJl+hMBc1j1yH+Fqv16l9wcLCAl73hK9QmIuaH5BOpVLUumB7exuvrYtpdfkAcUsMzyUM0QGofQF+NKA+z1doQCGfz1PrglqthgLWxbT3mZ2ZmaHWbRvvhpWVFRSAp7/L3EWeYLNSqdjLy8vYeAeiAPHvOzMP8wiiDfESIoaF4WHsD9dR/mVCxBd3AAAAAElFTkSuQmCC";

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

        // A list of AvaloniaProperty objects on the MainWindow to which the menu item belongs, which affect whether the
        // menu item action is available or not. Set to null or to an empty list if not applicable. This property is optional,
        // but either this or PropertyAffectingEnabled must be defined (even though they may be null). This property should be
        // used when multiple properties on the parent window can cause the menu item to become enabled or disabled.
        // E.g.
        //      new List<Avalonia.AvaloniaProperty>() { MainWindow.IsTreeOpened, MainWindow.IsSelectionAvailableProperty }
        public static List<Avalonia.AvaloniaProperty> PropertiesAffectingEnabled { get; } = new List<Avalonia.AvaloniaProperty>() { MainWindow.IsTreeOpened, MainWindow.IsSelectionAvailableProperty };

        // An AvaloniaProperty on the MainWindow to which the menu item belongs, which affects whether the menu item
        // action is available or not. Set to null if not applicable. This property is optional, but either this
        // or PropertiesAffectingEnabled must be defined (even though they may be null). This property should be used when only
        // one property on the parent window can cause the menu item to become enabled or disabled.
        // E.g.
        //      MainWindow.IsSelectionAvailableProperty
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsSelectionAvailableProperty;

        // This method returns true if the action is available, or false if it is not available. This method is called
        // every time the property of the MainWindow specified by the PropertyAffectingEnabled changes.
        //
        // window: the MainWindow that contains the plot.
        //
        // This method returns a list containing n+1 elements, where n is the number of sub-items for this module.
        // The first element of the list determines whether the button corresponding to this module is enabled or
        // not, while each subsequence element is associated to the corresponding sub-item.
        public static List<bool> IsEnabled(MainWindow window)
        {
            // TODO: check whether this module's action is available or not.
            return new List<bool>() { window.IsSelectionAvailable };
        }

        // If the ParentMenu is "File", this optional method returns a control that is used to populate the page of the
        // File menu corresponding to this module. If this method is omitted or returns null, the menu item will not be
        // associated to a page in the file menu (e.g. like the "Open" or "Close" menu items). When this method is defined 
        // and the user clicks on the menu item in the File page, the page is shown and then the the PerformAction method is
        // invoked.
        //
        // However, if the user instead uses the keyboard shortcut for the module, the File menu is not opened and the page
        // is not selected. To ensure that the File menu is opened at the right page when the user invokes the module through
        // the keyboard shortcut, you should use the PerformAction method (see e.g. the Export module for an example of how
        // to do this).
        //
        // If the ParentMenu is not "File", this method is never invoked.
        public static Avalonia.Controls.Control GetFileMenuPage()
        {
            // The RibbonFilePageContentTemplate contains a basic template for a page of the File menu. Set the title in the
            // constructor and the contents of the page through the PageContent property (this could be e.g. a Grid with
            // multiple controls inside or a RibbonFilePageContentTabbedWithButtons like this example).
            return new RibbonFilePageContentTemplate("Title of the file page")
            {
                PageContent = new RibbonFilePageContentTabbedWithButtons(new List<(string, string, Avalonia.Controls.Control, Avalonia.Controls.Control)>()
                {
                    (
                        "First tab button text",
                        "First tab button description", 
                        // Icon
                        new Avalonia.Controls.Canvas() { Width = 32, Height = 32, Background = Avalonia.Media.Brushes.Blue },
                        // Tab contents
                        new Avalonia.Controls.TextBlock() { Text = "First tab content" }
                    ),
                    (
                        "Second tab button text",
                        null, 
                        // Icon
                        new Avalonia.Controls.Canvas() { Width = 32, Height = 32, Background = Avalonia.Media.Brushes.Red },
                        // Tab contents
                        new Avalonia.Controls.TextBlock() { Text = "Second tab content" }
                    )
                })
            };

        }

        // This method performs the actual action. This method returns a Task, so that it can be marked as async and
        // awaited. Return Task.CompletedTask if you do not need to run any async code.
        //
        // actionIndex: the index of the sub-item that has been clicked by the user. If the module button has a
        //              "default action" (i.e. the first element of the list returned by SubItems is empty), this
        //              parameter will be -1 if the user clicks on the button, and values >= 0 if the user clicks
        //              on a sub-item. If the module button does not have a default action, this parameter will
        //              always be >= 0.
        //
        // window: the MainWindow that contains the plot.
        //
        public static Task PerformAction(int actionIndex, MainWindow window)
        {
            // TODO: do something.

            return Task.CompletedTask;
        }
    }
}
