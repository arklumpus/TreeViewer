using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
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
        public const ModuleTypes ModuleType = ModuleTypes.Plotting;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // These variables hold a PNG icon at three resolutions (16x16px, 24x24px and 32x32px). The GetIcon method below
        // uses these to return the appropriate image based on the scaling value. You can replace these with your icon
        // or delete them and produce a vector icon.
        private static string Icon16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAB2AAAAdgAUeEWlIAAAFHSURBVDhPY2RAA0VFRf+hTKygr68PQw8KIGQAOmAhRgNJhmJTjM8AosOAoN/xAZJcAAJuDevEdjUEvYJysRqA00XuVWslvWo3TIVyCQImKA0HzEzMEUCHhRmnzWSFCqGAzsitDVAmGICd4V230ZvhH6MOiP2fkSEeSGr+Z/w/mekf09N/DP//cPz9u2B9e9BbqOZ6IG4sX+4NNgjsgq9MfHv/Mf0TBGpqA2kGiTH+Z8z9z/gvk4nx/ymQZpAYENQDNYIsBRkCBmADDjQ4/tjeFFABNADZ73//MP8z29occBjEQXc6jI8SBoz/GG2B1Eegxw4BaWbmP6xWYAkIgNnOgOwKuAG+DZs1gBp//mVmMNnW5OfAyMCYzsT4zxcqDQZAW//DMFQIAYAGyDk07GeBcsHAu2GLEpQJ1gxlggFWQ/AB7AYwMAAAcQ2Fj94R5YcAAAAASUVORK5CYII=";
        private static string Icon24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAACxEAAAsRAX9kX5EAAAHUSURBVEhL1ZM9SAJRHMDfO7XagqAvqiXaipaaCqIgkLogh7CMGqvdRUOFowySwjWKppbAFpO0paFJaGhtCtoq3YKCyPT1f/ee1314d3rV0A+O/wfc//thZEIwGCRctSWZTJrGMaWRBFYIXP4Z2KpSR63rsNyBVYIfj9AuQL0JrCr8BOFiVm1+Y4Sm/LgDO+wS1NXdyPqhh6uOsXwHHZ1dq17pfICbjrBMAD0uCWXs56YjTBPMbObaQUxhQhaZxxmmCbDrg1buhjaGvbHMIPM2jrxpuVpXaQ9j1CN7GTRoN1PRHXyPTEWIIJwvFp7it0cbJe5CiUB2MnQqXnNTQTml6VCqtcnTfACeAHfVokww2S8+F2K64BKIMHy7kITqCsqIrhL+l1x8fhnqWwPzjXk1POAKmbjc8oXVwTlhCNxCJTO/Mewgt+07BkFHoucsu+PLc12BV6+gtw0JROmiH8Qo1WF+ryAqVIfO4JqI5nUSZlerR9UuuF9G8wNFjKYjBOM4qDdlF1pxV1AfIegE7F4Y0bi6C9Xs9Si7cMumCriQBVhkQrXIeziAIXoARBDom1CPSaleDSR+B6EZlYxXSrWJkfQYNw3MRjNzXJXhgQyY+f8jCH0Bcv6odgXoqPQAAAAASUVORK5CYII=";
        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAHySURBVFhH5Za/SsRAEMbvxMLCwkawsFDwAbze4uxECwst9AEsBAuNiljIeVwlarDwHawFFQtLS0HfwMZCsLGwsBDO70smR/7sbubCHgj+4GOyu0lmZ3Z2k3qthCAIjmFacas/wjAsff+QWBeVnHsDGehS0vSOJgMDpa6NTrOeVfA2garFqtkF0QQVExhMnfDFmpdr78ujzoCWfmtFswvaYv82A1sCLf04Ty+T+iBaPLpekEsjVc8J1UNwPgLzCk3fdZa/o05PaDPA6CfEekU7gZWc9UbpEiD9wzAf0Bj0BY37XAZNBpoQnZNRyOsy9DKASLdgduNWBjpPJkA+RXnOkZlLuVbTy4A8vAEx5VMppZ0TttPj3CGbNucn67fO8yGzBHjJA0wDuok6yrmHGniOtgCc8xPdsyasRShLcgoxwjw/0CEcn8VNMxI9vyWtg6sloy9rEUpK3+JWgXeF8yhqOHZmwToBZGAWZiZuFZjE+Jxc2+DfUfIljbIQX2ZxbcP0ocP9fyE2wXoo5aN3ZcE1gTWxLxALbYcWemInWBVrIh19gjELxsKQ9D9DjHofzll0ERjjNu1Ae9A8xh7ZnyBRun5O20lGrMDJNuQ88TDehDiJDJhA11Zw7Oe4NP2jccBx3idNZw1UwbT2eaw74j9Sq/0C/Em1T70ILNwAAAAASUVORK5CYII=";

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
        // tree: the final transformed tree that is being plotted.
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
        // true if the parameter change should trigger a redraw, or false if the changed parameter does not affect the
        // rendering of the module. Note that one call to this method may correspond to more than one parameter changing.
        //
        // tree: the final transformed tree that is being plotted. Despite the method signature, this is guaranteed to
        //       be a TreeNode instance (i.e. you can cast it without worry).
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
            // Return true if the plot action needs to be redrawn, false if it does not.

            return false;
        }

        // This method actually performs the actual plot action (e.g. drawing on the tree). The method should return two
        // points, representing respectively the top-left and bottom-right corner of the area in which the plot action
        // has been drawn. These will be used to determine the boundary of the plot.
        //
        // tree: the final transformed tree that is being plotted.
        //
        // parameterValues: the current value for the parameters of the module. The keys in the dictionary correspond to
        //                  the names of the parameters as returned by the GetParameters method.
        //
        // coordinates: the coordinates of the nodes of the tree, as computed by the Coordinates module. The keys in this
        //              dictionary are the Ids of the nodes of the tree. You can update these in the plot action, but the
        //              other plot modules will not be updated until a full refresh is requested, and this may confuse
        //              users.
        //
        // graphics: the graphics surface on which the plot action should be drawn.
        public static Point[] PlotAction(TreeNode tree, Dictionary<string, object> parameterValues, Dictionary<string, Point> coordinates, Graphics graphics)
        {
            Point topLeft = new Point();
            Point bottomRight = new Point();

            // TODO: draw something and update the topLeft and bottomRight points.
            // E.g.:
            //      graphics.FillRectangle(10, 10, 30, 30, Colours.Red);
            //      topLeft = new Point(10, 10);
            //      bottomRight = new Point(40, 40);

            return new Point[] { topLeft, bottomRight };
        }
    }
}
