using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhyloTree;
using TreeViewer;
using VectSharp;
using PhyloTree.Formats;
using Avalonia.Media.Imaging;

namespace SaveTree
{
    /// <summary>
    /// This module is used to save the currently opened tree to a file on disk. The tree can be saved in Newick, NEXUS or Binary format.
    /// </summary>
    /// 
    /// <description>
    /// ## Further information
    /// 
    /// When saving the tree, the first choice that needs to be done is _which_ tree to save. There are three possible options:
    /// 
    /// 1. The original tree(s) that were loaded from a file that was opened in TreeViewer.
    /// 2. The transformed tree that was produced by the Transformer module (e.g. a consensus tree).
    /// 3. The final transformed tree that was produced after all the Further transformation modules acted on the transformed tree.
    /// 
    /// If the tree(s) is/are saved in Newick or Newick-with-attributes format, only the tree itself is saved, without including any
    /// information about the modules that are currently active in the plot. This means that if the file is later opened again in
    /// TreeViewer, all information about the active modules will be lost.
    /// 
    /// Instead, if the file is saved in NEXUS or Binary format, all the information about the modules can be kept, if desired. This means
    /// that the tree can be opened again in TreeViewer to obtain exactly the same plot. Other software opening the file should ignore
    /// the information about TreeViewer modules; thus, including information about the active modules should not cause compatibility
    /// issues with other programs. The attachments that have been added to the tree can be included in the file as well; this makes
    /// it possible to obtain a portable file that contains all the information required to reliably reproduce the plot. Note however that
    /// users opening the file need to have the relevant modules installed.
    /// 
    /// If the tree being exported is the original tree, the state of all the modules that are currently enabled is saved. If the transformed
    /// tree is being exported, the state of the Transformer module is not exported. If the final transformed tree is saved, only the state of
    /// the Coordinates and Plot action modules is saved.
    /// 
    /// Furthermore, if the file includes information about the modules or attachments, it can be signed. This adds a layer
    /// of security, by ensuring that the source code contained in the module information (e.g. in attribute formatters) has not been
    /// tampered with. The files are signed with the user's unique private key.
    /// 
    /// When a user opens a file created by someone else that contains source code, they will be asked if they trust the origin
    /// of the file. The source code is only loaded and compiled if they respond affirmatively. In addition, if the file has been signed,
    /// the public key of the signer can be added to the users key store; this causes subsequent files that have been signed with the
    /// same private key (i.e. by the same user) to be opened automatically, without asking for confirmation (and thus providing a more
    /// streamlined interface when repeatedly opening files coming from the same collaborators).
    /// </description>

    public static class MyModule
    {
        public const string Name = "Save tree";
        public const string HelpText = "Saves the tree file.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "a8f25c08-4935-4fd5-80ea-1d29ada66f1e";
        public const ModuleTypes ModuleType = ModuleTypes.MenuAction;

        public static string ItemText { get; } = "Save tree...";
        public static string ParentMenu { get; } = "File";
        public static string GroupId { get; } = "da6493c8-45e1-4286-bf71-265546c5c412";
        public static Avalonia.AvaloniaProperty PropertyAffectingEnabled { get; } = MainWindow.IsTreeOpenedProperty;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.S;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.Control;
        public static bool TriggerInTextBox { get; } = false;

        public static bool IsEnabled(MainWindow window)
        {
            return window.Trees != null;
        }

        public static async Task PerformAction(MainWindow window)
        {
            if (window.Trees == null)
            {
                return;
            }

            Bitmap lightArrowBitmap = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.LightArrow.png"));
            Bitmap darkArrowBitmap = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.DarkArrow.png"));

            Window targetChoiceWindow = new Window() { Width = 600, Height = 600, FontFamily = Avalonia.Media.FontFamily.Parse("resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans"), FontSize = 15, Title = "Save tree...", WindowStartupLocation = WindowStartupLocation.CenterOwner, Icon = window.Icon };

            Grid targetChoiceGrid = new Grid() { Margin = new Avalonia.Thickness(10) };
            targetChoiceWindow.Content = targetChoiceGrid;

            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            targetChoiceGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            {
                Grid headerGrid = new Grid();
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                headerGrid.Children.Add(new TextBlock() { Text = "Choose which tree(s) to save:", FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Margin = new Avalonia.Thickness(10, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center });

                HelpButton moduleHelpButton = new HelpButton() { Margin = new Avalonia.Thickness(10, 0, 10, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(moduleHelpButton, 1);
                headerGrid.Children.Add(moduleHelpButton);

                moduleHelpButton.Click += async (s, e) =>
                {
                    HelpWindow win = new HelpWindow(Modules.LoadedModulesMetadata[Id].BuildReadmeMarkdown(), Id);

                    await win.ShowDialog(targetChoiceWindow);
                };

                targetChoiceGrid.Children.Add(headerGrid);
            }

            CoolButton originalFileButton = new CoolButton() { CornerRadius = new Avalonia.CornerRadius(10), IsEnabled = false };
            originalFileButton.Opacity = 0.65;
            Grid.SetRow(originalFileButton, 1);

            {
                originalFileButton.Title = new TextBlock() { Text = "Original file", FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = new Avalonia.Thickness(10, 5), Foreground = Avalonia.Media.Brushes.White };
                originalFileButton.ButtonContent = new TextBlock() { Text = "The original tree file that has been opened.", Margin = new Avalonia.Thickness(10), TextWrapping = Avalonia.Media.TextWrapping.Wrap, FontSize = 13, TextAlignment = Avalonia.Media.TextAlignment.Center };
            }

            targetChoiceGrid.Children.Add(originalFileButton);

            {
                Grid arrowGrid = new Grid();
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Bitmap fileTypeIcon = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.FileTypeTemplate.png"));
                Bitmap openFileIcon = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.LoadFileTemplate.png"));

                StackPanel moduleIcons = new StackPanel() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Orientation = Avalonia.Layout.Orientation.Horizontal };
                moduleIcons.Children.Add(new Image() { Source = fileTypeIcon, Height = 32, Margin = new Avalonia.Thickness(0, 0, 5, 0) });
                moduleIcons.Children.Add(new Image() { Source = openFileIcon, Height = 32 });

                arrowGrid.Children.Add(moduleIcons);

                Image arrow = new Image() { Source = lightArrowBitmap, Width = 48, Height = 48 };

                Grid.SetColumn(arrow, 1);

                arrowGrid.Children.Add(arrow);


                TextBlock actionBlock = new TextBlock() { Text = "File type and Load file modules", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180)), FontStyle = Avalonia.Media.FontStyle.Italic, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(10, 0, 0, 0) };
                Grid.SetColumn(actionBlock, 2);
                arrowGrid.Children.Add(actionBlock);
                Grid.SetRow(arrowGrid, 2);
                targetChoiceGrid.Children.Add(arrowGrid);
            }

            CoolButton loadedTreesButton = new CoolButton() { CornerRadius = new Avalonia.CornerRadius(10) };
            Grid.SetRow(loadedTreesButton, 3);

            {
                loadedTreesButton.Title = new TextBlock() { Text = window.Trees.Count > 1 ? (window.Trees.Count.ToString() + " loaded trees") : "Loaded tree", FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = new Avalonia.Thickness(10, 5), Foreground = Avalonia.Media.Brushes.White };
                loadedTreesButton.ButtonContent = new TextBlock() { Text = "Saves all the trees that have been loaded, possibly including all the active Transformer, Further transformations, Coordinates and Plot action modules.", Margin = new Avalonia.Thickness(10), TextWrapping = Avalonia.Media.TextWrapping.Wrap, FontSize = 13, TextAlignment = Avalonia.Media.TextAlignment.Center };
            }

            targetChoiceGrid.Children.Add(loadedTreesButton);

            {
                Grid arrowGrid = new Grid();
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Bitmap transformerIcon = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.TransformerTemplate.png"));
                arrowGrid.Children.Add(new Image() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Height = 32, Source = transformerIcon });

                Image arrow = new Image() { Source = darkArrowBitmap, Width = 48, Height = 48 };

                Grid.SetColumn(arrow, 1);

                arrowGrid.Children.Add(arrow);


                TextBlock actionBlock = new TextBlock() { Text = "Transformer module", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180)), FontStyle = Avalonia.Media.FontStyle.Italic, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(10, 0, 0, 0) };
                Grid.SetColumn(actionBlock, 2);
                arrowGrid.Children.Add(actionBlock);
                Grid.SetRow(arrowGrid, 4);
                targetChoiceGrid.Children.Add(arrowGrid);
            }

            CoolButton transformedTreeButton = new CoolButton() { CornerRadius = new Avalonia.CornerRadius(10) };
            Grid.SetRow(transformedTreeButton, 5);

            {
                transformedTreeButton.Title = new TextBlock() { Text = "Transformed tree", FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = new Avalonia.Thickness(10, 5), Foreground = Avalonia.Media.Brushes.White };
                transformedTreeButton.ButtonContent = new TextBlock() { Text = "Saves the first transformed tree, possibly including all the active Further transformations, Coordinates and Plot action modules.", Margin = new Avalonia.Thickness(10), TextWrapping = Avalonia.Media.TextWrapping.Wrap, FontSize = 13, TextAlignment = Avalonia.Media.TextAlignment.Center };
            }

            targetChoiceGrid.Children.Add(transformedTreeButton);

            {
                Grid arrowGrid = new Grid();
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                arrowGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Bitmap transformerIcon = new Bitmap(typeof(Modules).Assembly.GetManifestResourceStream("TreeViewer.Assets.FurtherTransformationTemplate.png"));
                arrowGrid.Children.Add(new Image() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right, Height = 32, Source = transformerIcon });

                Image arrow = new Image() { Source = darkArrowBitmap, Width = 48, Height = 48 };

                Grid.SetColumn(arrow, 1);

                arrowGrid.Children.Add(arrow);


                TextBlock actionBlock = new TextBlock() { Text = "Further transformation modules", Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(180, 180, 180)), FontStyle = Avalonia.Media.FontStyle.Italic, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(10, 0, 0, 0) };
                Grid.SetColumn(actionBlock, 2);
                arrowGrid.Children.Add(actionBlock);
                Grid.SetRow(arrowGrid, 6);
                targetChoiceGrid.Children.Add(arrowGrid);
            }

            CoolButton finalTreeButton = new CoolButton() { CornerRadius = new Avalonia.CornerRadius(10) };
            Grid.SetRow(finalTreeButton, 7);

            {
                finalTreeButton.Title = new TextBlock() { Text = "Final tree", FontWeight = Avalonia.Media.FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Center, Margin = new Avalonia.Thickness(10, 5), Foreground = Avalonia.Media.Brushes.White };
                finalTreeButton.ButtonContent = new TextBlock() { Text = "Saves the final transformed tree, possibly including all the active Coordinates and Plot action modules.", Margin = new Avalonia.Thickness(10), TextWrapping = Avalonia.Media.TextWrapping.Wrap, FontSize = 13, TextAlignment = Avalonia.Media.TextAlignment.Center };
            }


            targetChoiceGrid.Children.Add(finalTreeButton);

            Button cancelButton = new Button() { Content = "Cancel", Width = 100, Margin = new Avalonia.Thickness(0, 10, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center };
            Grid.SetRow(cancelButton, 8);
            targetChoiceGrid.Children.Add(cancelButton);

            int targetChoice = -1;
            cancelButton.Click += (s, e) =>
            {
                targetChoice = -1;
                targetChoiceWindow.Close();
            };

            loadedTreesButton.Click += (s, e) =>
            {
                targetChoice = 0;
                targetChoiceWindow.Close();
            };

            transformedTreeButton.Click += (s, e) =>
            {
                targetChoice = 1;
                targetChoiceWindow.Close();
            };

            finalTreeButton.Click += (s, e) =>
            {
                targetChoice = 2;
                targetChoiceWindow.Close();
            };

            await targetChoiceWindow.ShowDialog2(window);

            if (targetChoice != -1)
            {
                bool includeAttachments = false;

                if (window.StateData.Attachments.Count > 0)
                {

                    MessageBox box3 = new MessageBox("Question", "Do you want to include the attachments?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box3.ShowDialog2(window);

                    includeAttachments = box3.Result == MessageBox.Results.Yes;
                }

                MessageBox box = new MessageBox("Question", "Do you want to include information about the active modules?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                await box.ShowDialog2(window);

                bool includeModules = box.Result == MessageBox.Results.Yes;
                bool signModules = false;

                if (includeModules)
                {
                    MessageBox box2 = new MessageBox("Question", "Do you want to sign the file?", MessageBox.MessageBoxButtonTypes.YesNo, MessageBox.MessageBoxIconTypes.QuestionMark);
                    await box2.ShowDialog2(window);
                    if (box2.Result == MessageBox.Results.Yes)
                    {
                        signModules = true;
                    }
                }

                List<FileDialogFilter> filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "Binary tree format", Extensions = new List<string>() { "tbi" } }, new FileDialogFilter() { Name = "NEXUS format", Extensions = new List<string>() { "nex" } } };

                if (!includeModules && !includeAttachments)
                {
                    filters.Add(new FileDialogFilter() { Name = "Newick format", Extensions = new List<string>() { "tre", "nwk" } });
                    filters.Add(new FileDialogFilter() { Name = "Newick-with-attributes format", Extensions = new List<string>() { "nwka" } });
                }

                SaveFileDialog dialog = new SaveFileDialog() { Filters = filters, Title = "Save tree(s)" };

                string result = await dialog.ShowAsync(window);



                if (!string.IsNullOrEmpty(result))
                {
                    string extension;

                    if (result.IndexOf(".") < 0)
                    {
                        extension = "";
                        await new MessageBox("Attention", "Please specify a file extension!").ShowDialog2(window);
                    }
                    else
                    {
                        extension = result.Substring(result.LastIndexOf("."));
                    }

                    try
                    {
                        if (System.IO.File.Exists(result))
                        {
                            System.IO.File.Delete(result);
                        }



                        if (targetChoice == 0)
                        {
                            if (extension == ".nex")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine("#NEXUS");
                                    sw.WriteLine();
                                    sw.WriteLine("Begin Trees;");
                                    int count = 0;
                                    foreach (TreeNode tree in window.Trees)
                                    {
                                        if (tree.Attributes.ContainsKey("TreeName"))
                                        {
                                            sw.Write("\tTree " + tree.Attributes["TreeName"].ToString() + " = ");
                                        }
                                        else
                                        {
                                            sw.Write("\tTree tree" + count.ToString() + " = ");
                                        }

                                        sw.Write(NWKA.WriteTree(tree, true));
                                        sw.WriteLine();
                                        count++;
                                    }
                                    sw.WriteLine("End;");

                                    if (includeModules)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin TreeViewer;");
                                        string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.AllModules, signModules);
                                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                        sw.WriteLine(serializedModules);
                                        sw.WriteLine("End;");
                                    }

                                    if (includeAttachments)
                                    {
                                        foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                        {
                                            sw.WriteLine();

                                            sw.WriteLine("Begin Attachment;");

                                            sw.WriteLine("\tName: " + kvp.Key + ";");
                                            sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                            sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                            kvp.Value.WriteBase64Encoded(sw);
                                            sw.WriteLine();
                                            sw.WriteLine("End;");
                                        }
                                    }
                                }
                            }
                            else if (extension == ".tbi")
                            {
                                using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                                {
                                    if (!includeModules)
                                    {
                                        if (!includeAttachments)
                                        {
                                            BinaryTree.WriteAllTrees(window.Trees, fs);
                                        }
                                        else
                                        {

                                            string tempFile = System.IO.Path.GetTempFileName();
                                            using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                                {
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                                    {
                                                        bw.Write(kvp.Key);
                                                        bw.Write(2);
                                                        bw.Write(kvp.Value.StoreInMemory);
                                                        bw.Write(kvp.Value.CacheResults);
                                                        bw.Write(kvp.Value.StreamLength);
                                                        bw.Flush();

                                                        kvp.Value.WriteToStream(ms);
                                                    }

                                                    bw.Flush();
                                                    bw.Write(ms.Position - 3);
                                                }

                                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                                BinaryTree.WriteAllTrees(window.Trees, fs, additionalDataToCopy: ms);
                                            }

                                            System.IO.File.Delete(tempFile);
                                        }
                                    }
                                    else
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
                                                bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.AllModules, signModules));

                                                if (includeAttachments)
                                                {
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
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

                                            BinaryTree.WriteAllTrees(window.Trees, fs, additionalDataToCopy: ms);
                                        }

                                        System.IO.File.Delete(tempFile);
                                    }
                                }
                            }
                            else if (extension == ".tre" || extension == ".nwk")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    foreach (TreeNode tree in window.Trees)
                                    {
                                        sw.WriteLine(NWKA.WriteTree(tree, false, true));
                                    }
                                }
                            }
                            else if (extension == ".nwka")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    foreach (TreeNode tree in window.Trees)
                                    {
                                        sw.WriteLine(NWKA.WriteTree(tree, true));
                                    }
                                }
                            }
                        }
                        else if (targetChoice == 1)
                        {
                            if (extension == ".nex")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine("#NEXUS");
                                    sw.WriteLine();
                                    sw.WriteLine("Begin Trees;");
                                    if (window.FirstTransformedTree.Attributes.ContainsKey("TreeName"))
                                    {
                                        sw.Write("\tTree " + window.FirstTransformedTree.Attributes["TreeName"].ToString() + " = ");
                                    }
                                    else
                                    {
                                        sw.Write("\tTree tree = ");
                                    }

                                    sw.Write(NWKA.WriteTree(window.FirstTransformedTree, true));
                                    sw.WriteLine();
                                    sw.WriteLine("End;");

                                    if (includeModules)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin TreeViewer;");
                                        string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, signModules);
                                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                        sw.WriteLine(serializedModules);
                                        sw.WriteLine("End;");
                                    }

                                    if (includeAttachments)
                                    {
                                        foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                        {
                                            sw.WriteLine();

                                            sw.WriteLine("Begin Attachment;");

                                            sw.WriteLine("\tName: " + kvp.Key + ";");
                                            sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                            sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                            kvp.Value.WriteBase64Encoded(sw);
                                            sw.WriteLine();
                                            sw.WriteLine("End;");
                                        }
                                    }
                                }
                            }
                            else if (extension == ".tbi")
                            {
                                using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                                {
                                    if (!includeModules)
                                    {
                                        if (!includeAttachments)
                                        {
                                            BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs);
                                        }
                                        else
                                        {

                                            string tempFile = System.IO.Path.GetTempFileName();
                                            using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                                {
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                                    {
                                                        bw.Write(kvp.Key);
                                                        bw.Write(2);
                                                        bw.Write(kvp.Value.StoreInMemory);
                                                        bw.Write(kvp.Value.CacheResults);
                                                        bw.Write(kvp.Value.StreamLength);
                                                        bw.Flush();

                                                        kvp.Value.WriteToStream(ms);
                                                    }

                                                    bw.Flush();
                                                    bw.Write(ms.Position - 3);
                                                }

                                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                                BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, additionalDataToCopy: ms);
                                            }

                                            System.IO.File.Delete(tempFile);
                                        }
                                    }
                                    else
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
                                                bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeTransform, signModules));

                                                if (includeAttachments)
                                                {
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
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

                                            BinaryTree.WriteAllTrees(new TreeNode[] { window.FirstTransformedTree }, fs, additionalDataToCopy: ms);
                                        }
                                        System.IO.File.Delete(tempFile);
                                    }
                                }
                            }
                            else if (extension == ".tre" || extension == ".nwk")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine(NWKA.WriteTree(window.FirstTransformedTree, false, true));
                                }
                            }
                            else if (extension == ".nwka")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine(NWKA.WriteTree(window.FirstTransformedTree, true));
                                }
                            }
                        }
                        else if (targetChoice == 2)
                        {
                            if (extension == ".nex")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine("#NEXUS");
                                    sw.WriteLine();
                                    sw.WriteLine("Begin Trees;");
                                    if (window.TransformedTree.Attributes.ContainsKey("TreeName"))
                                    {
                                        sw.Write("\tTree " + window.TransformedTree.Attributes["TreeName"].ToString() + " = ");
                                    }
                                    else
                                    {
                                        sw.Write("\tTree tree = ");
                                    }

                                    sw.Write(NWKA.WriteTree(window.TransformedTree, true));
                                    sw.WriteLine();
                                    sw.WriteLine("End;");

                                    if (includeModules)
                                    {
                                        sw.WriteLine();

                                        sw.WriteLine("Begin TreeViewer;");
                                        string serializedModules = window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, signModules);
                                        sw.WriteLine("\tLength: " + serializedModules.Length + ";");
                                        sw.WriteLine(serializedModules);
                                        sw.WriteLine("End;");
                                    }

                                    if (includeAttachments)
                                    {
                                        foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                        {
                                            sw.WriteLine();

                                            sw.WriteLine("Begin Attachment;");

                                            sw.WriteLine("\tName: " + kvp.Key + ";");
                                            sw.WriteLine("\tFlags: " + (kvp.Value.StoreInMemory ? "1" : "0") + (kvp.Value.CacheResults ? "1" : "0") + ";");
                                            sw.WriteLine("\tLength: " + kvp.Value.StreamLength + ";");
                                            kvp.Value.WriteBase64Encoded(sw);
                                            sw.WriteLine();
                                            sw.WriteLine("End;");
                                        }
                                    }
                                }
                            }
                            else if (extension == ".tbi")
                            {
                                using (System.IO.FileStream fs = new System.IO.FileStream(result, System.IO.FileMode.Create))
                                {
                                    if (!includeModules)
                                    {
                                        if (!includeAttachments)
                                        {
                                            BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs);
                                        }
                                        else
                                        {

                                            string tempFile = System.IO.Path.GetTempFileName();
                                            using (System.IO.FileStream ms = new System.IO.FileStream(tempFile, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                                            {
                                                using (System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms, System.Text.Encoding.UTF8, true))
                                                {
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write((byte)0);
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
                                                    {
                                                        bw.Write(kvp.Key);
                                                        bw.Write(2);
                                                        bw.Write(kvp.Value.StoreInMemory);
                                                        bw.Write(kvp.Value.CacheResults);
                                                        bw.Write(kvp.Value.StreamLength);
                                                        bw.Flush();

                                                        kvp.Value.WriteToStream(ms);
                                                    }

                                                    bw.Flush();
                                                    bw.Write(ms.Position - 3);
                                                }

                                                ms.Seek(0, System.IO.SeekOrigin.Begin);

                                                BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, additionalDataToCopy: ms);
                                            }

                                            System.IO.File.Delete(tempFile);
                                        }
                                    }
                                    else
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
                                                bw.Write(window.SerializeAllModules(MainWindow.ModuleTarget.ExcludeFurtherTransformation, signModules));

                                                if (includeAttachments)
                                                {
                                                    bw.Write("#Attachments");
                                                    bw.Write(window.StateData.Attachments.Count);

                                                    foreach (KeyValuePair<string, Attachment> kvp in window.StateData.Attachments)
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

                                            BinaryTree.WriteAllTrees(new TreeNode[] { window.TransformedTree }, fs, additionalDataToCopy: ms);
                                        }

                                        System.IO.File.Delete(tempFile);
                                    }
                                }
                            }
                            else if (extension == ".tre" || extension == ".nwk")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine(NWKA.WriteTree(window.TransformedTree, false, true));
                                }
                            }
                            else if (extension == ".nwka")
                            {
                                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(result))
                                {
                                    sw.WriteLine(NWKA.WriteTree(window.TransformedTree, true));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        await new MessageBox("Error!", "Error while saving the tree(s):\n" + ex.Message).ShowDialog2(window);
                    }
                }
            }
        }
    }
}