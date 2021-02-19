using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;

namespace UnrootTree
{
    /// <summary>
    /// This module transforms a rooted tree into an unrooted tree.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "Unroot tree";
        public const string HelpText = "Unroots the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const string Id = "f06dce2a-794b-4897-a154-82f7f44c125d";
        public const ModuleTypes ModuleType = ModuleTypes.FurtherTransformation;

        public static bool Repeatable { get; } = false;

        public static List<(string, string)> GetParameters(TreeNode tree)
        {
            return new List<(string, string)>()
            {

            };
        }

        public static bool OnParameterChange(object tree, Dictionary<string, object> previousParameterValues, Dictionary<string, object> currentParameterValues, out Dictionary<string, ControlStatus> controlStatus, out Dictionary<string, object> parametersToChange)
        {
            controlStatus = new Dictionary<string, ControlStatus>();
            parametersToChange = new Dictionary<string, object>();
            return true;
        }

        public static void Transform(ref TreeNode tree, Dictionary<string, object> parameterValues)
        {
            tree = tree.GetUnrootedTree();
        }
    }
}
