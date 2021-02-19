using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using VectSharp;

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
