/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TreeViewer;
using Markdig;
using System.IO.Compression;
using Mono.Options;

namespace BuildRepositoryModuleDatabase
{
    class Program
    {
        static int Main(string[] args)
        {
            bool showHelp = false;
            bool showUsage = false;

            string rootPath = null;
            string privateKeyFile = null;

            OptionSet argParser = new OptionSet()
            {
                { "h|help", "Print this message and exit.", v => { showHelp = v != null; } },
                { "r|root=", "The path to the root of the repository. Required.", v => { rootPath = v; } },
                { "k|key=", "Provide the path to the private key file that will be used to sign the modules. Required.", v => { privateKeyFile = v; } },
            };

            List<string> unrecognised = argParser.Parse(args);

            if (unrecognised.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Unrecognised argument" + (unrecognised.Count > 1 ? "s" : "") + ": " + unrecognised.Aggregate((a, b) => a + " " + b));
                showUsage = true;
            }

            if (string.IsNullOrEmpty(rootPath) && !showHelp)
            {
                Console.WriteLine();
                Console.WriteLine("The root path is required!");
                showUsage = true;
            }

            if (string.IsNullOrEmpty(privateKeyFile) && !showHelp)
            {
                Console.WriteLine();
                Console.WriteLine("The private key file is required!");
                showUsage = true;
            }

            if (showUsage || showHelp)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("BuildRepositoryModuleDatabase");
                Console.WriteLine();
                Console.WriteLine("Usage:");
                Console.WriteLine();
                Console.WriteLine("  BuildRepositoryModuleDatabase {-h|--help}");
                Console.WriteLine("  BuildRepositoryModuleDatabase --root <root_path> --key <key_file>");                
            }

            if (showHelp)
            {
                Console.WriteLine();
                Console.WriteLine("Options:");
                Console.WriteLine();
                argParser.WriteOptionDescriptions(Console.Out);
                return 0;
            }

            if (showUsage)
            {
                return 64;
            }

            System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

            Directory.SetCurrentDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            string[] files = Directory.GetFiles(Path.Combine(rootPath, "src", "Modules"), "*.cs");

            VectSharp.SVG.Parser.ParseImageURI = VectSharp.MuPDFUtils.ImageURIParser.Parser(VectSharp.SVG.Parser.ParseSVGURI);

            File.Delete(Modules.ModuleListPath);

            TreeViewer.ModuleMetadata[] modules = new TreeViewer.ModuleMetadata[files.Length];

            if (!Directory.Exists("references"))
            {
                Directory.CreateDirectory("references");
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("Compiling modules...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(Path.GetFileName(files[i]));
                if (!File.Exists(Path.Combine(rootPath, "src", "Modules", "references", Path.GetFileName(files[i]) + ".references")))
                {
                    References[Path.GetFileName(files[i])] = new List<string>(baseReferences);
                }
                else
                {
                    References[Path.GetFileName(files[i])] = new List<string>(File.ReadAllLines(Path.Combine(rootPath, "src", "Modules", "references", Path.GetFileName(files[i]) + ".references")));
                }

                bool compiledWithoutErrors = false;

                while (!compiledWithoutErrors)
                {
                    try
                    {
                        modules[i] = TreeViewer.ModuleMetadata.CreateFromSource(File.ReadAllText(files[i]), References[Path.GetFileName(files[i])].ToArray());
                        compiledWithoutErrors = true;
                    }
                    catch (Exception e)
                    {
                        HashSet<string> newReferences = new HashSet<string>();

                        string msg = e.Message;

                        while (msg.Contains("You must add a reference to assembly '"))
                        {
                            msg = msg.Substring(msg.IndexOf("You must add a reference to assembly '") + "You must add a reference to assembly '".Length);
                            newReferences.Add(msg.Substring(0, msg.IndexOf(",")));
                        }

                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine();
                        Console.WriteLine(e.Message);
                        Console.WriteLine();
                        Console.ForegroundColor = ConsoleColor.Gray;

                        if (newReferences.Count == 0)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write("Which references should I add? ");
                            Console.ForegroundColor = ConsoleColor.Gray;
                            string[] newRefs = Console.ReadLine().Split(',');

                            foreach (string sr in newRefs)
                            {
                                string refName = sr.Trim();
                                if (refName.EndsWith(".dll"))
                                {
                                    refName = refName.Substring(0, refName.Length - 4);
                                }

                                newReferences.Add(refName);
                            }
                            Console.WriteLine();
                        }

                        Console.WriteLine("Adding references: ");

                        foreach (string sr in newReferences)
                        {
                            References[Path.GetFileName(files[i])].Add(sr + ".dll");
                            Console.WriteLine("\t" + sr + ".dll");
                        }

                    }
                }
                File.WriteAllLines(Path.Combine(rootPath, "src", "Modules", "references", Path.GetFileName(files[i]) + ".references"), References[Path.GetFileName(files[i])]);
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("Checking for unnecessary references...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;

            for (int i = 0; i < files.Length; i++)
            {
                Console.WriteLine(Path.GetFileName(files[i]));

                List<string> toBeRemoved = new List<string>();
                List<string> originalReferences = References[Path.GetFileName(files[i])];

                for (int j = 0; j < originalReferences.Count; j++)
                {
                    List<string> currentReferences = new List<string>(originalReferences);
                    currentReferences.Remove(originalReferences[j]);

                    try
                    {
                        TreeViewer.ModuleMetadata.CreateFromSource(File.ReadAllText(files[i]), currentReferences.ToArray());
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("\tRemoving " + originalReferences[j]);
                        Console.ForegroundColor = ConsoleColor.Gray;
                        toBeRemoved.Add(originalReferences[j]);
                    }
                    catch
                    {

                    }
                }

                for (int j = 0; j < toBeRemoved.Count; j++)
                {
                    References[Path.GetFileName(files[i])].Remove(toBeRemoved[j]);
                }

                File.WriteAllLines(Path.Combine(rootPath, "src", "Modules", "references", Path.GetFileName(files[i]) + ".references"), References[Path.GetFileName(files[i])]);
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine();
            Console.WriteLine("Exporting modules and rendering manuals...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Gray;


            Directory.CreateDirectory(Path.Combine(rootPath, "Modules"));

            VectSharp.Markdown.MarkdownRenderer renderer = new VectSharp.Markdown.MarkdownRenderer() { BaseFontSize = 12 };

            Func<string, string, (string, bool)> imageUriResolver = renderer.ImageUriResolver;

            List<string> imagesToDelete = new List<string>();

            Dictionary<string, string> imageCache = new Dictionary<string, string>();

            renderer.ImageUriResolver = (imageUri, baseUri) =>
            {
                if (!imageCache.TryGetValue(baseUri + "|||" + imageUri, out string cachedImage))
                {
                    if (!imageUri.StartsWith("data:"))
                    {
                        bool wasDownloaded;

                        (cachedImage, wasDownloaded) = imageUriResolver(imageUri, baseUri);

                        if (wasDownloaded)
                        {
                            imagesToDelete.Add(cachedImage);
                        }

                        imageCache.Add(baseUri + "|||" + imageUri, cachedImage);
                    }
                    else
                    {
                        string tempFile = Path.GetTempFileName();
                        if (File.Exists(tempFile))
                        {
                            File.Delete(tempFile);
                        }

                        Directory.CreateDirectory(tempFile);

                        string uri = imageUri;

                        string mimeType = uri.Substring(uri.IndexOf(":") + 1, uri.IndexOf(";") - uri.IndexOf(":") - 1);

                        string type = uri.Substring(uri.IndexOf(";") + 1, uri.IndexOf(",") - uri.IndexOf(";") - 1);

                        if (mimeType == "image/svg+xml")
                        {
                            int offset = uri.IndexOf(",") + 1;

                            string data;

                            switch (type)
                            {
                                case "base64":
                                    data = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(uri.Substring(offset)));
                                    break;
                            }
                        }

                        VectSharp.Page pag = VectSharp.SVG.Parser.ParseImageURI(imageUri, true);
                        VectSharp.SVG.SVGContextInterpreter.SaveAsSVG(pag, Path.Combine(tempFile, "temp.svg"));

                        imagesToDelete.Add(Path.Combine(tempFile, "temp.svg"));

                        cachedImage = Path.Combine(tempFile, "temp.svg");

                        imageCache.Add(baseUri + "|||" + imageUri, cachedImage);
                    }
                }
                else
                {
                    Console.WriteLine("Fetching {0} from cache.", imageUri);
                }

                return (cachedImage, false);
            };


            List<ModuleHeader> moduleHeaders = new List<ModuleHeader>();

            foreach (TreeViewer.ModuleMetadata module in modules)
            {
                Directory.CreateDirectory(Path.Combine(rootPath, "Modules", module.Id));
                string modulePath = Path.Combine(rootPath, "Modules", module.Id, module.Id + ".v" + module.Version.ToString() + ".json.zip");
                module.Sign(privateKeyFile);
                module.Export(modulePath, true, true, true);

                string markdownSource = module.BuildReadmeMarkdown();

                Console.WriteLine("Rendering {0} - {1}", module.Name, module.Id);

                Markdig.Syntax.MarkdownDocument markdownDocument = Markdig.Markdown.Parse(markdownSource, new Markdig.MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().Build());

                VectSharp.Document doc = renderer.Render(markdownDocument, out Dictionary<string, string> linkDestinations);
                VectSharp.PDF.PDFContextInterpreter.SaveAsPDF(doc, Path.Combine(rootPath, "Modules", module.Id, "Readme.pdf"), linkDestinations: linkDestinations);

                TreeViewer.ModuleMetadata.Install(modulePath, true, false);

                ModuleHeader header = new ModuleHeader(module);
                moduleHeaders.Add(header);
            }

            foreach (string imageFile in imagesToDelete)
            {
                System.IO.File.Delete(imageFile);
                System.IO.Directory.Delete(System.IO.Path.GetDirectoryName(imageFile));
            }

            string serializedModuleHeaders = System.Text.Json.JsonSerializer.Serialize(moduleHeaders, Modules.DefaultSerializationOptions);
            using (FileStream fs = new FileStream(Path.Combine(rootPath, "Modules", "modules.json.gz"), FileMode.Create))
            {
                using (GZipStream compressionStream = new GZipStream(fs, CompressionLevel.Optimal))
                {
                    using (StreamWriter sw = new StreamWriter(compressionStream))
                    {
                        sw.Write(serializedModuleHeaders);
                    }
                }
            }

            using (StreamWriter sw = new StreamWriter(Path.Combine(rootPath, "Modules", "Readme.md")))
            {
                sw.WriteLine("# Module repository");
                sw.WriteLine();
                sw.WriteLine("This folder contains a collection of modules maintained by the developer(s) of TreeViewer. All the modules are signed and tested before being included in this repository.");
                sw.WriteLine();
                sw.WriteLine("The modules can be loaded or installed by using the `Load from repository...` or `Install from repository...` options in the module manager window.");
                sw.WriteLine("Alternatively, the module `json.zip` files can be downloaded and loaded or installed manually using the `Load...` or `Install...` options.");
                sw.WriteLine();
                sw.WriteLine("Click on the name of any module to open the folder containing the module's manual and `json.zip` file.");
                sw.WriteLine();
                sw.WriteLine("## List of currently available modules");
                sw.WriteLine();

                foreach (ModuleHeader header in moduleHeaders)
                {
                    sw.WriteLine("<br />");
                    sw.WriteLine();
                    sw.WriteLine("### [" + header.Name + "](" + header.Id + ")");
                    sw.WriteLine();
                    sw.WriteLine("_Version " + header.Version.ToString() + ", by " + header.Author + "_");
                    sw.WriteLine();
                    sw.WriteLine("**Description**: " + header.HelpText);
                    sw.WriteLine();
                    sw.WriteLine("**Module type**: " + header.ModuleType.ToString());
                    sw.WriteLine();
                    sw.WriteLine("**Module ID**: `" + header.Id + "`");
                    sw.WriteLine();
                }
            }

            return 0;
        }

        private static void CopyDirectory(string sourceDirectory, string targetDirectoryParent)
        {
            Directory.CreateDirectory(Path.Combine(targetDirectoryParent, Path.GetFileName(sourceDirectory)));

            foreach (string file in Directory.GetFiles(sourceDirectory, "*"))
            {
                File.Copy(file, Path.Combine(targetDirectoryParent, Path.GetFileName(sourceDirectory), Path.GetFileName(file)), true);
            }

            foreach (string directory in Directory.GetDirectories(sourceDirectory, "*"))
            {
                CopyDirectory(directory, Path.Combine(targetDirectoryParent, Path.GetFileName(sourceDirectory)));
            }
        }

        static List<string> baseReferences = new List<string>() { "System.Runtime.dll", "System.Private.CoreLib.dll", "netstandard.dll", "System.Collections.dll", "TreeViewer.dll" };

        static Dictionary<string, List<string>> References = new Dictionary<string, List<string>>()
        {

        };
    }
}
