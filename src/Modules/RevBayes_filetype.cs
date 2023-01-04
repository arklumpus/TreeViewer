using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using PhyloTree;
using PhyloTree.Formats;
using TreeViewer;
using System.IO;
using System.Text;

namespace a89f05f57dc574cc4bace84e52c201769
{
    /// <summary>
    /// This module is used to read tree trace files produced by [RevBayes](https://revbayes.github.io/). These files consist of a single header
    /// line, containing tab-separated column headers, followed by multiple lines each containing a tree (in the last column) together with
    /// information about it (e.g., the iteration number, likelihood, prior and posterior). This information is preserved as attributes on the
    /// root node of each parsed tree.
    /// 
    /// To avoid memory overflow issues, the module does not read all the trees from the file into memory at once; instead, it only reads them
    /// one at a time when requested by the chosen Load file module.
    /// </summary>

    public static class MyModule
    {
        public const string Name = "RevBayes trace";
        public const string HelpText = "Opens a tree trace file produced by RevBayes.\nSafe even when opening huge files.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.FileType;

        public const string Id = "89f05f57-dc57-4cc4-bace-84e52c201769";

        public static string[] Extensions { get; } = { "RevBayes tree trace files", "trees" };

        public static double IsSupported(string fileName)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                StringBuilder sb = new StringBuilder();

                int c = sr.Read();

                while (c >= 0 && char.IsWhiteSpace((char)c))
                {
                    c = sr.Read();
                }

                while (c >= 0 && !char.IsWhiteSpace((char)c))
                {
                    sb.Append((char)c);
                    c = sr.Read();
                }

                if (sb.ToString() == "Iteration")
                {
                    sr.ReadLine();

                    string secondLine = sr.ReadLine();
                    string[] splitLine = secondLine.Split('\t');

                    if (splitLine[splitLine.Length - 1].StartsWith("(") && splitLine[splitLine.Length - 1].EndsWith(";"))
                    {
                        return 0.5;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        public static IEnumerable<TreeNode> OpenFile(string fileName, List<(string, Dictionary<string, object>)> moduleSuggestions, Action<double> progressAction, Func<RSAParameters?, bool> askForCodePermission)
        {
            using (StreamReader sr = new StreamReader(fileName))
            {
                double length = sr.BaseStream.Length;

                string headerLine = sr.ReadLine();

                string[] headers = headerLine.Split('\t');

                string line = sr.ReadLine();

                while (!string.IsNullOrEmpty(line))
                {
                    string[] splitLine = line.Split('\t');

                    TreeNode tree = NWKA.ParseTree(splitLine[splitLine.Length - 1]);

                    for (int i = 0; i < headers.Length - 1; i++)
                    {
                        if (double.TryParse(splitLine[i], System.Globalization.CultureInfo.InvariantCulture, out double doubleValue))
                        {
                            tree.Attributes[headers[i]] = doubleValue;
                        }
                        else
                        {
                            tree.Attributes[headers[i]] = splitLine[i];
                        }
                    }

                    yield return tree;

                    double progress = Math.Max(0, Math.Min(1, sr.BaseStream.Position / length));
                    progressAction?.Invoke(progress);
                    line = sr.ReadLine();
                }
            }
        }
    }
}
