using System.Collections.Generic;
using PhyloTree;
using TreeViewer;
using System;

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
        public const ModuleTypes ModuleType = ModuleTypes.Transformer;

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
        // trees: the trees that have been loaded from the file.
        public static List<(string, string)> GetParameters(TreeCollection trees)
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
        // than one parameter changing. Consider that if this method returns true, all Further transformations will have
        // to be computed again, then the Coordinates module will be invoked, and finally all Plot actions will be redrawn;
        // therefore, you should return true sparingly; a good idea would be to use an "Apply" Button and only return true
        // when that is clicked.
        //
        // tree: the trees that have been loaded from the file. Despite the method signature, this is guaranteed to
        //       be a TreeCollection instance (i.e. you can cast it without worry).
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

        // This method performs the actual transformation, turning a TreeCollection object into a single TreeNode object.
        //
        // trees: the trees that have been loaded from the file.
        //
        // parameterValues: the current value for the parameters of the module. The keys in the dictionary correspond to
        //                  the names of the parameters as returned by the GetParameters method.
        //
        // progressAction: as your module transforms the trees, you can invoke this Action to provide feedback to the user
        //                  as to the status of the operation. The argument to this method should be a number between 0 and 1.
        public static TreeNode Transform(TreeCollection trees, Dictionary<string, object> parameterValues, Action<double> progressAction)
        {
            // TODO: use the trees to compute the tree that will be drawn.
            // E.g.
            //      return trees.GetConsensus(true, false, 0.5, true);

            return trees[0];
        }
    }
}
