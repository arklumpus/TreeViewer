using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using PhyloTree;
using TreeViewer;

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
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        // Generated automatically, this is the unique identifier of your module. You should not need to change this.
        public const string Id = "@GuidHere";

        // A list of the file extensions supported by your module. The first element should be the name of the file type and each
        // subsequent element a file extension for that tile type.
        // E.g. { "Newick files", "tree", "tre", "nwk", "treefile" }
        public static string[] Extensions { get; } = { };

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
            //          ("Maximum file size:", "FileSize:26214400"),
            //      };

            return new List<(string, string)>()
            {

            };
        }

        // Given the full path to a file that the user has chosen, this method should return a double indicating whether the
        // module can open or not the specified file. A return value of 0 means that the file cannot be opened using this
        // module; a return value greater than 0 means that the file can be opened with this module; a return value of 1 means
        // that the file must be opened using this module. If multiple modules return a value between 0 and 1, the module that
        // returned the highest value is used to open the file.
        //
        // fileName: the full path of the file to be checked.
        public static double IsSupported(string fileName)
        {
            // TODO: Open file and check whether it can be read using this module.

            return 0;
        }

        // This method should return the tree(s) contained in the file as a IEnumerable. If possible, consider returning a "lazy"
        // IEnumerable that does not read from disk/parse the trees until required, as this will make it possible to deal with
        // fewer memory constraints.
        //
        // fileName: the full path to the file to open.
        //
        // moduleSuggestions: a list of suggested modules that can be enabled after the file is opened. The first two elements
        //                    are, respectively, the suggested Transformer module and the suggested Coordinates module. The
        //                    The program intializes the list with default values, and File type and Load files modules can
        //                    modify the default suggestions or add additional modules (e.g. plot actions).
        //
        // progressAction: if you can, invoke this Action with an argument corresponding the the progress in opening the file
        //                 (0 = the file has just been opened; 1: the file has been completely read). Note that this may also
        //                 be null.
        //
        // askForCodePermission: a function that asks the user's permission to compile and import code. Used as an argument to
        //                       ModuleUtils.DeserializeModules, if necessary. Otherwise, you can ignore it.
        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            progressAction(0);

            // TODO: read and parse trees from the file.
            //
            // Return individual trees like this:
            //       yield return tree;

            progressAction(1);

            yield break;
        }
    }
}
