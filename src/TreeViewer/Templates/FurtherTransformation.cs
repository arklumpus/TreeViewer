using System;
using System.Collections.Generic;
using PhyloTree;
using TreeViewer;

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
        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            // TODO: do something with the tree.
        }
    }
}
