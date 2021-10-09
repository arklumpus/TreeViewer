using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;
using System.Runtime.InteropServices;

// Name of the namespace. It does not really matter, but better if it is unique.
namespace @NamespaceHere
{
    //Do not change class name
    public static class MyModule
    {
        public const string Name = "A name for your module.";
        public const string HelpText = "A very short description for your module.";
        public const string Author = "Your name";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // The value of this property indicates whether multiple instances of this module can be added to the same plot
        // or not. Useful to prevent the same action (e.g. rooting the tree) being performed twice.
        public static bool Repeatable { get; } = true;

        // These variables hold a PNG icon at three resolutions (16x16px, 24x24px and 32x32px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACQSURBVDhPpZJNCoAgEEa14xXUXdzXotZ6l4K6njWDE2Xjbw8kRf3mOSTFA6WUdVMWrfXr/AcIuBAwUtA56VelKlLGiwGfQjGDdlwtDLdE0MDNEa4HZNRNG+7tc59W4+AMgMZ9q0nqcFWBYxnwbtZ7/BC6XESoB7+5VWK/sTGmXJmIBQPJ5FBAtlW2QV0PhDgBSulUC+7ANwAAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAADxSURBVEhLxZVbDgIhDEXB5Wmie4FvH6PfsBdNdHtIGWqA8CiQiSeZQCeh5ZYWOKsghLjZ4bpafWitne+ds8oMOQ+JFBhj/GxFSul+4G5G4OgUneWYCdBK0TyoIE0PYA/ZwOfNIrm1SKuKms4BpRS3eCumlaLFj8NUFdSkp5QUkKoDAx0uLzd5349uXclpCKmKwFHoLLVrbF6mpG3sz8/sYXwep+Z6mk5LGoTivBsIUlLzN34yqV0bsNhLEN6LKjNVRHorooOidm7POzFUCa10wuUHIzTjJo0WdnpWQZiq3JWACrpSNFBFpAAzKSK8FYx9AdSPV79zHIMfAAAAAElFTkSuQmCC";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEOSURBVFhH1ZfNDoMgDIBlj7fL3kXPizM7w7vsstdjVItxzcBSQNyXEO1B6D9VdQGstfi2MAzDwz3GRcrHGDOffZmlHyilvpaj2OEi+r63sFAsRtADR7HmANc6H7tSNPcAm6Y5QEuyBH5PdjxTrdda7+4N5Z2SAxM+i4C9he8BCbHQeQWyuN5fFhaKQbaKUKUOKcOttdTy5n3gPxWAONK40thyESkAcaRxpbHlkvzVXta/n7ekPUVqh5RIPRyQ+c1BlZAcng23EZ2a1W0V7vrJTU8wSUep2YhGaW/Ixk9PHAWat+IqpcPJJ5iYoHs290AVfA6gGOUcOQDZ6v5+i9eMn4xjN2VND8xTdPya7roPeHJqs36CU1MAAAAASUVORK5CYII=";

        // This method returns the icon for the module. This is shown next to instances of this module in the module panel,
        // as well as in the list of modules when the user wants to add a new module to the plot.
        // The image should be 16x16 device-independent pixels. The scaling parameter can be used to determine the actual
        // resolution of the image (e.g. if scaling is 1, the image will be 16x16px, while if scaling is 1.5 the image needs
        // to be 24x24px).
        // This method can return a vector image or a raster image embedded in a Page. If you wish to return a raster image,
        // you can just embed it by replacing the Icon16Base64 (16x16px), Icon24Base64 (24x24px), and Icon32Base64 (32x32px)
        // variables with Base-64 encoded images. If you wish to return a vector image, you can delete those variables and
        // rewrite the body of the GetIcon method to produce the icon.
        // Note that even when scaling is greater than 1, the Page that is returned by this method should have size 16x16.
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

        // This method should return a list of tuples representing the global parameters required by this module. The
        // first item of each tuple is the name of the parameter, the second element is the parameter type. These will
        // be presented to the user in the "Preferences" window. See the TreeViewer manual for details of the possible
        // parameter values and options. Note that this method is only called once, when the module is loaded.
        public static List<(string, string)> GetGlobalSettings()
        {
            // TODO: return the list of required parameters.
            // E.g.:
            //      return new List<(string, string)>()
            //      {
            //          ("Default colour:", "Colour:[0,162,232,255]"),
            //      };

            return new List<(string, string)>()
            {

            };
        }

        // This method should return a list of tuples representing the parameters required by this module. The first
        // item of each tuple is the name of the parameter, the second element is the parameter type. These will be
        // presented to the user in the interface. See the TreeViewer manual for details of the possible parameter
        // values and options. Note that this method is called once every time the module is added to the plot.
        //
        // tree: the tree that has been returned by the Transformer module and any Further transformation modules that
        //       have been run before this module.
        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            // TODO: return the list of required parameters.
            // E.g.:
            //      return new List<(string, string)>()
            //      {
            //          ("Size", "Group:2"),
            //          ("Width:", "NumericUpDown:100[\"0\",\"Infinity\"]"),
            //          ("Height:", "NumericUpDown:100[\"0\",\"Infinity\"]"),
            //      };

            return new List<(string, string)>()
            {

            };
        }

        // This method is called every time the value of a parameter of the module is changed. The method should return
        // true if the parameter change should trigger a recomputation of all downstream modules, or false if the changed
        // parameter does not affect the result of the module. Note that one call to this method may correspond to more
        // than one parameter changing. Consider that if this method returns true, Further transformations that act after
        // this one will have to be computed again, then the Coordinates module will be invoked, and finally all Plot
        // actions will be redrawn; therefore, you should return true sparingly; a good idea would be to use an "Apply"
        // Button and only return true when that is clicked.
        //
        // tree: the tree that has been returned by the Transformer module and any Further transformation modules that
        //       have been run before this module.
        //
        // previousParameterValues: the previous values for the parameters (i.e. before the change that triggered this
        //                          event happened).
        //
        // currentParameterValues: the new values for the parameters (i.e. after they have been changed).
        //
        // controlStatus: this method should create a dictionary containing the the status of controls used to change
        //                the parameters for this module. The keys of the dictionary should be parameter names, while
        //                the values determine whether each control is enabled, disabled or hidden. Parameters whose
        //                name is not included in this dictionary maintain the same state as before. Set to an empty
        //                dictionary if you do not need to change anything. Please do not set to null.
        //
        // parametersToChange: this method should also create a dictionary containing updates to other parameters
        //                     that may be triggered by the current parameter change. This makes it possible to update
        //                     multiple parameters after the user changes just one of them. Set to an empty dictionary
        //                     if no parameter needs to change. Note that this method will be called again with the new
        //                     changed parameters (even if the new parameter values are the same as the previous ones),
        //                     so make sure to check whether a parameter update is really necessary, otherwise you may
        //                     be stuck in an infinite loop!
        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();

            // TODO: update control status and parameters that need to change.
            // Return true if the everything needs to be updated, false if not.

            return false;
        }

        // This method performs the actual transformation. Note that it does not return anything.
        //
        // tree: the tree that has been returned by the Transformer module and any Further transformation modules that
        //       have been run before this module. If you need to, you can assign to this variable to change the tree,
        //       and everything should work correctly. Otherwise, just modify the tree in-place.
        // 
        // parameterValues: the current value for the parameters of the module. The keys in the dictionary correspond to
        //                  the names of the parameters as returned by the GetParameters method.
        //
        // progressAction: as your module does its thing, you can invoke this Action to provide feedback to the user about
        //                 the progress of the operation. The argument to the method should be a number between 0 and 1.
        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            // TODO: do something with the tree.
        }
    }
}
