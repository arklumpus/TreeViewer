using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using PhyloTree;
using TreeViewer;
using System.Linq;

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
        public const ModuleTypes ModuleType = ModuleTypes.LoadFile;

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
            //          ("Large file threshold:", "FileSize:26214400"),
            //      };

            return new List<(string, string)>()
            {
                
            };
        }

        // Given a FileInfo object referring to the file that the user has chosen, the Id of the File type module, and
        // the collection of trees returned by the File type module, this method should return a value indicating whether the
        // module can load or not the specified file. A return value of 0 means that the file cannot be loaded using this
        // module; a return value greater than 0 means that the file can be loaded with this module; a return value of 1 means
        // that the file must be loaded using this module. If multiple modules return a value between 0 and 1, the module that
        // returned the highest value is used to load the file.
        //
        // fileInfo: a FileInfo object pointing to the file that is being opened.
        // 
        // filetypeModuleId: the Id of the File type module used to open the file.
        //
        // treeLoader: the IEnumerable returned by the File type module. If you can, avoid enumerating this object, as it may
        //             have unintended consequences.
        public static double IsSupported(FileInfo fileInfo, string filetypeModuleId, IEnumerable<TreeNode> treeLoader)
        {
            // TODO: check the fileInfo, loaderModuleId and treeLoader to decide whether this module can be used to load
            // the tree file or not.

            return 0;
        }

        // This method actually loads the trees from the IEnumerable returned by the File type module into a TreeCollection
        // object.
        //
        // parentWindow: the window that originated the request to load the file. This could be e.g. a MainWindow, or an
        //               AdvancedOpenWindow. If the program is running in command-line mode, this will be null. This window
        //               can be used e.g. to show dialogs.
        //
        // fileInfo: a FileInfo object pointing to the file that is being opened.
        //
        // filetypeModuleId: the Id of the File type module used to open the file.
        //
        // treeLoader: the IEnumerable returned by the File type module. If you can, enumerate this object only once, as it
        //             may have unintended consequences.
        //
        // moduleSuggestions: a list of suggested modules that can be enabled after the file is opened. The first two elements
        //                    are, respectively, the suggested Transformer module and the suggested Coordinates module. The
        //                    The program intializes the list with default values, and File type and Load files modules can
        //                    modify the default suggestions or add additional modules (e.g. plot actions).
        //
        // openerProgressAction: a reference to the progress action of the File type module that was used to open the file.
        //                       The File type module may call this function as the treeLoader IEnumerable is enumerated.
        //                       This module can set the value of this function to receive a progress callback by the File
        //                       type module.
        //
        // progressAction: if you can, invoke this Action with an argument corresponding the the progress in loading the file
        //                 (0 = the file has just been opened; 1: the file has been completely loaded). Note that this may also
        //                 be null. This could be achieved e.g. by using the openerProgressAction to call the progressAction
        //                 passing through the progress parameter.
        public static TreeCollection Load(Window parentWindow, FileInfo fileInfo, string filetypeModuleId, IEnumerable<TreeNode> treeLoader, List<(string, Dictionary<string, object>)> moduleSuggestions, ref Action<double> openerProgressAction, Action<double> progressAction)
        {
            // TODO: enumerate the treeLoader and store the trees in a TreeCollection object.

            return new TreeCollection(treeLoader.ToList());
        }
    }
}
