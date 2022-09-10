using System;
using TreeViewer;
using VectSharp;
using System.IO;
using PhyloTree;
using PhyloTree.Formats;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace a7476c9e405b14a928d36aa878d22cceb
{
	/// <summary>
    /// This module applies all the modules that are currently active to another tree file. This module can only be
	/// invoked from TreeViewerCommandLine. The new tree is opened in the current session.
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Apply modules to other tree (command-line)";
        public const string HelpText = "Applies the modules that are currently enabled to another tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public const string Id = "7476c9e4-05b1-4a92-8d36-aa878d22cceb";

        public static bool IsAvailableInCommandLine { get; } = true;

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };

        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = null;
		
		public static string GroupName { get; } = null;
		
		public static double GroupIndex { get; } = double.NaN;
		
		public static bool IsLargeButton { get; } = true;

		public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAGaSURBVFhH7ZZbTsMwEEWTqqujEmwiK0i+gQLfyQqyiSLBUrKd4HF9k8lk4oxbix84kutXa1/Pw27x5ylDXdR1PYbmRNd103wOHp4v4/fH02LNQ6g9VVVNJRe0aWguwPhhHNX5bNCJsRlOzy3hLbAlAuP3ipRm5/1S872kbdvSEXo2SHTTNGfXfL2OzNB6p5dPv++RPrjP+75f9e9A3Zzqr/fH2QWppLqENqWMQlbh9IS3QA6cKxfmduYPrRkKvuDL8lRcAzFbDMTWwe959IP4qglAAHxMOCusxkgIuRAHWgmQOWsFAvjtabGuOQghLJG3UG9itoAcl5kAc3MLWLJlU0AMymFsKEkV4N8CXizsZQSg72mFs2mBHC7Yg9ZIuoi4KHkSF/G+5sKcKPUtAG7engXSIpyI6zY3B2ZzxSABcMHeRSQxXcUWv8bWiQnwLpB/xWTfyO6lo3HTc6zhTnmmk/ISpqJke46RFVpAIhY0ssUASEnDBSRkGIapWISloFmGxrLFwB7y4gK/JkDDvw2hreZxiu//uY2i+AGWIw2QmqbdmAAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAKiSURBVGhD7Zi9choxEMc57BR5nIxfAJdJkzTwBIxTQ52M7yapgdYZngCaJEVSmhdIyYO4xo6iv7LLCJ1O0uk4Hczwm9nRx8m6Xe3HCfcuXGhGRq1iMpkI6paYzWaZhEbpEEL03n3+IX5//WB9eZ/aPePxuCQpgcIsVejPSgZ0i8hw2hCaOIANw/O3n76rNSdmQCZ+fXmvQgVK3tw9vELY6uHDxllDCjmw3W5LgnmXS48Jn7LtfTDInD8xD0gfyBNnMfnz7ePOnM9clacKVCS0tpfEIvXIZXP/f1TNfD7POP6BMsBWaZbLpbUCYb4lA7wHyaVcD6PoEDqm8iGw8oP88RotS9IcsCWmDSiLUIFwH8ri718/P+30EEpqwHQ6zREqNqElVnSFucx29R3wJqkLeAHACDYkugrxZnUIfZe+P8IGH6/DD5fI5LTAGtmprT8UGSwWiw0Ng9ENgJLUVcjwOnimGwB4bNJXVtSUGOVNzD2r8D6n9gAzaao2GA5XV+v16IWGXnQPoLJQVxETyqBREq9WwxfUZRp2QiMDkFyP94PnmDyyUFBbi0YhxOv2Jc0Rq8AVQrEEeQAnbBOG7+h5nqv9bGshbeD1QB3gidtic/3m6eeOpipJ6oFQOCdomISjGuDKl7bwhhAnqA2OfYB1RVH0ZR78pakSnSUxTtUmTIjybRHkAV1ZHaxjD1Wt0fF8iYN+Upo0ygEoj6oTonwAUVftfpN6PRqtrzb5bdKqYxJ1nQYxp+4Jof0zvmqHvKOzHzS+3wNogw2I+bdKUwNc1Nn/qB+yLkhtQNSV2UVSA2Ti5khem9CS2iTNARcxxQQkrUIuog2gVuHyRhtK68h3R10lTsaAWC5ltGvO3oBSDlC3xKnmQDCxt9a2OfsQOnN6vX/ZFcvv5/PDOgAAAABJRU5ErkJggg==";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAB2HAAAdhwGP5fFlAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAA3dJREFUeJztm89P1EAUx7+vXRZvnDip/4AJnvDHUcAYFkhYSPRmjEg4SYQjAcyoi8SDAYMHTRD8ceMg4I8tWSX6D3hD/wJMFE8kkojSPg+kZLeytF1mulO3n9OmfZ3OvPn2vTczWSAhIaGWIe+F4eFhDvLg1NTUP8/qRGZsyQZgWLnsgf00IuqPYjjEZJTapsqZ9ff373t9dnY2+LsioP3mYpvhLN9le6HLmrz04yDbzMhCI8zl12wsjq7c7lkFYq4AIYRBDt1j4DTM9GpmZKGxnO3u4NPvAZwhB5OuEuLuAKfOoU4QvgBogplePSc+HAEAK5c13e8/M5ivJ7P+HYCTAD7bKe4CiIGYOwAAXt3t/m6bTguANTDPfRQtv7w21kzHNoB5AGt2ymktiN4N917sHQAABdG7gc10szXRM13OJp/rfoDNdHPx4IH/xAHA3iyHtqGged+PqOuCoaGhTiJ6DOBoEPuGhoY6IcROZmypZLyxVQARPULAwQP4KoSw97uxVwf45X0N64JjIWwfAmAA8FaGsVVACPJbW1v3AeCiWEh7b5atBOOGXwzKjC8O/LTpRod425YXnd/c67WgAGQG8/Vgug7GCd75UyiuGLVTQNjoXo7i1aA107F9Qbw8b+4YqwCayEy/AfgsQKydAkJG98AURO8G7N+tAD45wC23FN5TgF80jzDah4nuodhdLfIpd/BAjcSAUqikEErpvrNTrn9+FayVy5pB2q9BBZTimwW8tbPfHlu1qHRNI10BHaOLl2W3qRLpDmCip+2jy1dkt7sP6zIaUREDDCKeU+0EZh6ABCeoqgQNIn6SGV9i6072uYoXTE9PWwCOH7YdlVnABGM+os+hYkIrwJsVfHA/B6xMdD8DKo/WqoiiDogkJlRKVIUQk8FazbxLFA5wQOhTFQwPS+gY4FcJemKEw0x9K7nuvcH7rT2ijhEqFWCDcNUNfrqiqg5wmOla8czLRtbOkQoF2MzUp3rmZe0cSXcAMUcleyk7R9IdkJ/oeSG7TZX4xgBd1/9eKt3ZSnaEdD8dVl0XJApwf2h0OrwOhWcDXrRTgKydnqBodzYYdKdHVmzQTgFRo50CKkWbc4G4oePpcFCkZIvYKkBWtij7fwG/vK/7qXJQYqsAWSQOqHYHqk3igGp3oNqUrQQ1zPtKqHkFJCQk1DZ/AQubQWW2Bn8TAAAAAElFTkSuQmCC";

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

            Page pag = new Page(16, 16);
            pag.Graphics.DrawRasterImage(0, 0, 16, 16, icon);

            return pag;
        }

        public static async void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            ConsoleWrapper.WriteLine();
            ConsoleWrapper.Write("Enter path to the other tree file: ");
            string fileName = ConsoleWrapper.ReadLine();

            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                {
                    
                    TreeCollection coll = await LoadFile(fileName);

                    if (coll != null)
                    {
                        string tempFileName = Path.GetTempFileName();

                        using (System.IO.FileStream fs = new System.IO.FileStream(tempFileName, System.IO.FileMode.Create))
                        {
                            string tempFile = System.IO.Path.GetTempFileName();
                            using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                            {
                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                {
                                    bw.Write((byte)0);
                                    bw.Write((byte)0);
                                    bw.Write((byte)0);
                                    bw.Write("#TreeViewer");
                                    
                                    bw.Write(stateData.SerializeAllModules(MainWindow.ModuleTarget.AllModules, true));

                                    if (stateData.Attachments.Count > 0)
                                    {
                                        bw.Write("#Attachments");
                                        bw.Write(stateData.Attachments.Count);

                                        foreach (KeyValuePair<string, Attachment> kvp in stateData.Attachments)
                                        {
                                            bw.Write(kvp.Key);
                                            bw.Write(2);
                                            bw.Write(kvp.Value.StoreInMemory);
                                            bw.Write(kvp.Value.CacheResults);
                                            bw.Write(kvp.Value.StreamLength);
                                            bw.Flush();

                                            kvp.Value.WriteToStream(ms);
                                        }
                                    }

                                    bw.Flush();
                                    bw.Write(ms.Position - 3);
                                }

                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                BinaryTree.WriteAllTrees(coll, fs, additionalDataToCopy: ms);
                            }

                            System.IO.File.Delete(tempFile);
                        }

                        stateData.OpenFile(tempFileName, true);
                    }
                }
                else
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("The selected file does not exist!", ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                }
            }
        }

        private static async Task<TreeCollection> LoadFile(string fileName)
        {
            double maxResult = 0;
            int maxIndex = -1;

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                try
                {
                    double priority = Modules.FileTypeModules[i].IsSupported(fileName);
                    if (priority > maxResult)
                    {
                        maxResult = priority;
                        maxIndex = i;
                    }
                }
                catch { }
            }

            if (maxIndex >= 0)
            {
                double maxLoadResult = 0;
                int maxLoadIndex = -1;

                IEnumerable<TreeNode> loader;

                try
                {
                    List<(string, Dictionary<string, object>)> moduleSuggestions = new List<(string, Dictionary<string, object>)>()
                    {
                        ("32914d41-b182-461e-b7c6-5f0263cc1ccd", new Dictionary<string, object>()),
                        ("68e25ec6-5911-4741-8547-317597e1b792", new Dictionary<string, object>()),
                    };

                    TreeCollection coll = null;

                    Action<double> openerProgressAction = (_) => { };

                    bool askForCodePermission(RSAParameters? publicKey)
                    {
                        return false;
                    };

                    loader = Modules.FileTypeModules[maxIndex].OpenFile(fileName, moduleSuggestions, (val) => { openerProgressAction(val); }, askForCodePermission);

                    FileInfo finfo = new FileInfo(fileName);

                    for (int i = 0; i < Modules.LoadFileModules.Count; i++)
                    {
                        try
                        {
                            double priority = Modules.LoadFileModules[i].IsSupported(finfo, Modules.FileTypeModules[maxIndex].Id, loader);
                            if (priority > maxLoadResult)
                            {
                                maxLoadResult = priority;
                                maxLoadIndex = i;
                            }
                        }
                        catch { }
                    }

                    if (maxLoadIndex >= 0)
                    {
                        try
                        {
                            (coll, openerProgressAction) = await Modules.LoadFileModules[maxLoadIndex].Load(null, finfo, Modules.FileTypeModules[maxIndex].Id, loader, moduleSuggestions, openerProgressAction, a => { });
                        }
                        catch (Exception ex)
                        {
                            ConsoleWrapper.WriteLine();
                            ConsoleWrapper.WriteLine(new ConsoleTextSpan("An error has occurred while loading the file!\n" + ex.Message, ConsoleColor.Red));
                            ConsoleWrapper.WriteLine();
                        }
                    }
                    else
                    {
                        ConsoleWrapper.WriteLine();
                        ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file cannot be loaded by any of the currently installed modules!", ConsoleColor.Red));
                        ConsoleWrapper.WriteLine();
                    }

                    if (coll != null)
                    {
                        return coll;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    ConsoleWrapper.WriteLine();
                    ConsoleWrapper.WriteLine(new ConsoleTextSpan("An error has occurred while opening the file!\n" + ex.Message, ConsoleColor.Red));
                    ConsoleWrapper.WriteLine();
                    return null;
                }
            }
            else
            {
                ConsoleWrapper.WriteLine();
                ConsoleWrapper.WriteLine(new ConsoleTextSpan("The file type is not supported by any of the currently installed modules!", ConsoleColor.Red));
                ConsoleWrapper.WriteLine();
                return null;
            }
        }
    }
}
