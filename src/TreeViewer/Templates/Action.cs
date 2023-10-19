using System;
using TreeViewer;
using VectSharp;
using System.Collections.Generic;
using VectSharp.MuPDFUtils;
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
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // This property determines whether the action defined by this module can be performed when the program is run
        // in command-line mode or not.
        public static bool IsAvailableInCommandLine { get; } = false;

        // The name of the group of buttons in which the button for this module will appear. If multiple modules specify
        // the same group name, they will be placed together. The group name is also shown on the ribbon interface.
        public static string GroupName { get; } = "Group name";

        // The index of the module corresponding to this button. Within a group, the buttons are sorted based on their
        // GroupIndex; the various groups are sorted based on the elements with the lowest GroupIndex that they contain.
        public static double GroupIndex { get; } = 0;

        // This property should return a list of tuples whose first element is the text of a "sub-item", while the second
        // element is a method that returns the icon for that sub-item. If this list is not empty, the button will have a
        // little "arrow" icon that can be used to open a menu containing all the sub-items. If the first sub-item's text
        // is empty, the button will also be clickable on its own (i.e., users will be able to click on the button, or to
        // expand the button and click on a sub-item in the menu); otherwise clicking on the button will only open the
        // sub-item menu.
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        // This property determines the shortcut keys that can be used to perform the action without having to click the
        // button. This should be a list containing 1 (if the action has no sub-items), n (if the action has sub-items
        // and it does not have a default action), or n+1 (if the action has sub-items and a default action) tuples, where
        // n is the number of sub-items for this module. The first element of each tuple represents the key, while the
        // second element is the key modifier (e.g. CTRL or SHIFT). The first element of the list is the shortcut for the
        // default action of the module (if any), while each subsequence element is associated to the corresponding sub-item.
        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };

        // This property determines whether the shortcut keys should trigger this module's action even if a text box
        // is focused when they are pressed (e.g. you would not want to hook to the CTRL+A combination if a text box
        // is focused, because that is used to select all text in the text box.
        public static bool TriggerInTextBox { get; } = false;

        // A short label that will appear on the button for the action. The label text should not be too long, otherwise
        // it will overflow the button's client area.
        public static string ButtonText { get; } = "Short text";

        // If this is true, the button corresponding to this module will be a "large" button with a 32x32 device-independent
        // pixels icon. If this is false, the button will be "small", with an icon size of 16x16 device-independent pixels.
        public static bool IsLargeButton { get; } = true;


        // If this is true, the button corresponding to this module will be enabled even when no tree has been loaded in
        // the window yet. This property is optional; omitting it will have the same effect as it being set to false.
        public static bool EnabledWithoutTree { get; } = false;

        // These variables hold a PNG icon at three resolutions (32x32px, 48x48px and 64x64px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFuSURBVFhHYxjxgBFEFBUV/Qfz6Az6+voY4Q5wcnICC9IL7Nu3D+wAJih/wMCoA0YdMOqA4eGAmNhMhpu37kJ5pAGqhcCiRSvJcgTVHHDh/FGyHEHVNECOI6ieCEl1BNUdAAKkOIImDgABYh1BMweAADGOoEqLaMbMpQxfv7yA8lDBz19/GNRU9RisbUwZJCVEoKIQAGqQQJkUAka+5//xgA8fPv9nZBLYy8QqZg7VQWVAwAEg8OLFKxo6gggHgMDDh4/BjoDqAgPqxAHIAf8+SkB5DF+//mDg5uYAs5mYBfeBGUjg/78PzlAmlQBSCHz58h2YoPm+bN6yB8wH+ngPE4uoL1QljQDUATDLQfHMyMh/GSQGDnYgG6qSRgDoAGTLwUJMglsOHToJcgMwFAS30DQUIAkLYTkIgNmM/NdBDrh27QYoFC5CpegHQD4HWQ4OBUaBM1Bh+gFmVgkpUCgwMgnNY2IVN4QKDybAwAAAEKcVl+whzcsAAAAASUVORK5CYII=";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIOSURBVGhD7ZhNSwJBGIBnt6wg1qAPss51DKKDRdTFDp36B926dPaHeO5YEPQHFC1qoS+iDMQ69OnBlKysy6LYZrjNu/taSFltbjtTzHNwZl7d5X3mnRlwiEDQGBJ8BINBwxz9MUKhkCRj/89SU4FAIAAN96iqarb/ogJCgDVCgDVCgDVCgDVCgDVC4LvMzs6Ts/MUjpzD1QosLa44LuGqQDK557iE63vAaQkmm9hJCSYCgFMSzAQAJySYCgCNSjAXABqR4EIA+KkENwJAImFfgisBmWYTP9y1JeHa1eLCwjIpFm9x9Dn6U5kMDgyRiUk/6fP1YPQ93F5stbZ4yNFRnOxsH5DcTR6jzFFyhk3u7h7oyvBuyM3do/gSltgXALLZG14kfiZQqVSMdDrDg4R9AUi+XC4buq4bFxcpUwJf9op5CrkDCGg+HABGsfgotbe34dBCkjqsu/O6aFPYcZvaChQKJTqjSiESVTFiQWd5XWrqmsGHeOJNoJo8XdN+2h5j2OQqcw3fHeNDPGEJ1CZPkbzh/YOEmTyseQBiHFZByZVKejX519PE6isn1eQBa8NyVwU4QZRrT6tvDANv0Bnf2trH9C04rcLHyJ7efqgC5m5ommbEYipUIYk/qYuLx+hXeGP0P8H06fklWV3bJPf5exIOR4crz/kvJbiA7oVxKhGR5M452h/BsEDwuxDyAjImEB5YfmgYAAAAAElFTkSuQmCC";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAALSSURBVHhe7Zo9ixpBGMfHpAr4DkKqQPBivoLpgo0H8TtYXRB8xdIiWAQr0cJPYJcqGCyu8xuIlVXA98p3EAKisJln5vGiRziN7pmZnfnB46wzs7r/n3Pr7iHRaDRKY4OHbDZrsGeKUSqVbC9wW1kOVkAoFILG8jQaDdbqFUDRArBVFi0AW2XRArBVFi0AW2XRArBVFi0AW2XRArBVFi0AW2URSkAkEiGdTgefXQfhVkAmk7mqBOEELJdLJqHb7WLP8yKcAIfDQRaLBUmn01eRIORJ0Ol0Xk2CkAKAfQm9Xg97zUdYAQBImM/nJJVKPZsEoQUALpfrQUK/38de8xBeAAASZrMZSSaTpkuQQgDgdrsfJAwGA+y9HGkEADsJiUTCNAlSCQDMliCdAGAnAU6Ml0qQUgAAEiaTCZMwHA6x99+RVgAAEsbjMTsxnitBagGAx+O5SIL0AoB9CaPRCHtPQ6gfSZVKJWIY5x8KXDH6fD4SjUaZlGNY7icyXq+XrYRqtcpupGSELoDLmU6nsIzWtN6xV5UIjHA5exJu2CtLAh6+OcgoAQ/dPFDCL1pSSMDDNpc9CR/ZuwgMHvLpxGIxCHdqLWkdwK4DBAIy4eYh6/WabDYbYrfbsYdjs7EIouU4G/apPobe8YEV+FfQiorAXk4wGISxD7CzFcBYf9gL/5ZWpVgs4giHXvTA+DdalgBjcR6FB278fj+OcrbbLczZ0nrNZkgOxvpr+B33tVoNZ3Hy+TzM/cqH5YYFeiI8cBsOh9m8HfT6H+aPab1iMyTmWHjgJa2f7XYb43Pu7u5gv89shsQcC78jEY/HMTqn1WrBvm0+LC/fafn45pO4aK0oGJ8Dfxq0/5bNUIBKuVzG6JxmswkCfvBh6/M+EAhgdMOA1VAoFEAA3P2dsooswX29XjdyuRwEX9Gq0HoDA6rwidaM1hdaXuhQDfhKlP67/z9CyG9Ka5Lk9+k8EQAAAABJRU5ErkJggg==";

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

        // This method performs the actual action.
        //
        // actionIndex: the index of the sub-item that has been clicked by the user. If the module button has a
        //              "default action" (i.e. the first element of the list returned by SubItems is empty), this
        //              parameter will be -1 if the user clicks on the button, and values >= 0 if the user clicks
        //              on a sub-item. If the module button does not have a default action, this parameter will
        //              always be >= 0.
        //
        // window: the MainWindow that contains the plot. If the program is running in command-line mode and this
        //         module has signalled that it is available in command-line mode, this may be null.
        //
        // stateData: an InstanceStateData object that can be used to access features in way that does not depend
        //            on the program running in command-line or GUI mode.
        public static void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            // TODO: do something.
        }
    }
}
