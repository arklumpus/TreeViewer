using PhyloTree;
using TreeViewer;
using VectSharp;
using System;
using PhyloTree.Formats;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace Copy_selected_node
{
    /// <summary>
    /// This module copies the currently selected node to the clipboard in Newick-with-attributes format. The node can then be pasted
    /// e.g. in a text editor or in another tree viewer progam. It can also be used to copy to the clipboard the value of an attribute
    /// from the selected tips or from all the selected nodes.
    /// </summary>

    class MyModule
    {
        public const string Name = "Copy selected node";
        public const string HelpText = "Copies the selected node to the clipboard.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.1");
        public const string Id = "debd9130-8451-4413-88f0-6357ec817021";
        public const ModuleTypes ModuleType = ModuleTypes.SelectionAction;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static Avalonia.Input.Key ShortcutKey { get; } = Avalonia.Input.Key.None;
        public static Avalonia.Input.KeyModifiers ShortcutModifier { get; } = Avalonia.Input.KeyModifiers.None;
        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Copy";

        public static string GroupName { get; } = "Clipboard";
        public static double GroupIndex { get; } = 6;
        public static bool IsLargeButton { get; } = true;
        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>() { ("", null), ("Copy subtree", GetIcon1), ("Copy attribute value at tips", GetIcon2), ("Copy attribute value at all nodes", GetIcon3) };

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFaSURBVFhHxZZNasMwEIXlkrOEHqNk2UAO0G59AnudLrq3yQG8TRfdteBVoWSRIwQTeohcIZHMyCjWjzWSxv1gkBbGeRq9eXHGOEVRXMUaS13X/fvQCAGxhB7iAdZ/w0vA/vfM1m/fxorFS8DH4Q92REx54Hn71ZeLZB4wtZsSTQB5u0dYPdC+b4aixMuEErgOETZaVVUlVjQLWJ28PC3vrqYsS6PhuBFh58eQnOoUeDg+aXKiroACEgGY5CQRgBll0itolVGWNSZawO7zONlmF1YB45faXvxzusAuDE2AmPkQXG12oQXR6+qxrxSYujYWSGJCTBeHDvB4hZ0bnvmws5Oyi4ynplbI6DbiHcVZlmmVEhIPYAg6jmif9IJ0um38XN4Sf8ezdCDPc60kyQSoiSnLRtM0w8dItABscqo/Hgzmi0g823VdX3L0VGabAtvJg6cAtt6Y287YDaOfAyHHYnfQAAAAAElFTkSuQmCC";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAHOSURBVGhD7Zg7TsQwEIYdRJ1bUHAJKiqQ2I6WJidIalCy1MkJtqGlQAIBFRUSVwgF4ia8MpFjeY3txI7t2OBPGq290mpnPPNPJk4QJs/zb7y0StM05D+NAgHYxsYh7eDPYAk+gC0N1HWNd3Kunl7R9fMb3sl5WJ/gFUJFURjXgFYGpjrvguBLiKDShY7Ob4mpELsQB2EAINTjizuu+YQwAJ+EKuPvlhAN9HLafOJ/ZEAGLe5kBBuTqHCUoLsNWzaiTvR4uUps9Hoew2Hs9jtFTg/2pF1q6kylC8xUA1oldHa4/0vYbJZcEUU8B9nTnjURiwZg4mkfS8gUvKZAm4iYgTFAqKvqXkmYKlgP4OblHX18fuGdeSYFwDs92mTYdB5wqoGpwlRBGADMOyEgHOZg3gFzxVgpijLmtIRssGgAJsp064UGL7WBlwyVFyNdbNyxEiAAGt1bPBn0YUcNLE0MYGmMK3luF6JvHGQMXcjLDGRZJjWa4Epos9nAR9VvOpyW0BhQYlBC7CkPDM535VP2X3QEkwGe84D1AEzMOyLnrcCOEqrA79u2JQb7zoSOe11C1MkT0bJ4GwDjvHBSttKF8HIuVZqm67IsJbcCCP0ADHhS2AQiCN0AAAAASUVORK5CYII=";
        private static string Icon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYYAAB2GAV2iE4EAAAIKSURBVHhe7ZihUgMxEIZTHAbFe/AgzFCPAFOLaT0CbFsDsh7fGRDwCCiQvAEzYGpAAd3rXqeT3jUbLnvJXvabyWyuor3d/vmTTc8gw+HwF6dJMJ1O1+/GyR7GbNlSwGQyKZ5jMRqNiqgKaAktAMZsCeYBz2/v5mb+Yj4X3/gJjfurE5ytEOsBt/NX7+RTIFgBPhZfOJOFegDGxh5wfDnH2Qp7bVPRc0DLOBUQyt2pJKcAqe5OxVkAqe5OJXsP8C4ArO2qIRVVAMbgwLkARs8TcP+2dgAgewU4zwHUE975+Klyx3i47he/UX5/KpQqC6aAi/6ROTzYxyc5BFNAHbCuITbtNUJhnzR1F8CYLboLYEzeA7juHMUogKsrFVMArq5UTRCjOGBtU4YLVQDGaIC7n40f191j3eAiegFi3zl6F6Dq36kaVGLfOaoHYKyl7Ra3yslhcOEsgNQ+n4qzF2iKqxew/aLu3/bxlU3s79P7AAsxBeBahmIKwOVFWx4QmnKtcd03+KIeYLFWADeqgETRAmDMFi0AxmwRvwuUru6L7gJIZxQwGAyKuIvZbIazDBWwmfyyQz/FaXoKoFIqhaIAO/nlO9zhY/cVsCt5IHoBOK/bXMkD0QvA1edTkgeie0BTqjyAmjzQOQ/wSR7oVAF8kwc6U4D/JA+07gHc+CQPdEYBy8R/fJM3xpg/2jBVAuytoXYAAAAASUVORK5CYII=";

        private static string Icon1_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABiSURBVDhPY6AUMEJprMCrduN/KBMDbGv2x6sXDPAZAAMYpqBrIsomZIBsADEuYIHSOAHJLiLGVmTAiE0Dyf5GBqS6gOQwAAG8LkTXQMhFBNMBCNA0TAiajMsAilxFRcDAAACgFi/GiyJfpQAAAABJRU5ErkJggg==";
        private static string Icon1_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABzSURBVEhLYxjygBFKEwW8ajf+hzLxgm3N/iSZCwfEWoAMmKA0zQBOr+ByLdneRwfYLBh6QYQeHPh8QHLQkRMc2ADtg4iq3iYFDJ0ggtIYAFsqAgGqBSkug0gNusFXFoHAoEpdAxdEuAAxPqBpEA43wMAAAK1kN8yp/tZ5AAAAAElFTkSuQmCC";
        private static string Icon1_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAACsSURBVFhH7ZZBCoAgEEWt47XpRFHRidp0vbKaFgn2dZrStAcyiAuZPw9LZU9B1ZuqGTtd2n3nzjTUpztLqhy8LxdFJzCvi7Zs7iQgAnQAzdqcqS8uCcQ566wc2Lo0Z+3aPXLkTgI91WeRmrWN8A5IzZJL8AQg6TtA1crRvc0BfQ7/C678kUjg2W8FcgCdI77jAIL7Trgk8M6bzyV9BxC+3ZuuSCQQtyM/kaPUAsDKTpICQiT1AAAAAElFTkSuQmCC";

        private static string Icon2_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACXSURBVDhPrZNRDkAwEERbcTo/7tJ/Qfz3Ln5cj53Nkk3VqtRLmh0SY2eCdx/phnWiMdKZt6WfTIMQwi6SiTF6MrjukYFvRT+Ch0SezHR4A1zcNsi9VWQZ2kBrZMf60sFFI/MRZYK19WRMA6yvInBmNRmfZgafc2tyhie5Hoo6sHp4NQBWD6/fAfi1k7SHoghWD9X/QiXOHaKXUaEMtC8DAAAAAElFTkSuQmCC";
        private static string Icon2_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADKSURBVEhL1ZVBDsIgEEXBeDo33oW90cY9d3Hj9ZSffJq2zgBDMdGXkAGazOcPk+K+jWccwunyuKVwTWN63s+Y2wRCCC9OP4gx+iQwf08C9sOXBAAcQIRO7NQEJFQbWjKUgtN9SAI9Dg6M3dTqbhaAizy4hbZcxjZay1FzoNJTbwlfSjSsYyRGOdjdRaB0D0fGZrbOWMZlJ61EuhwgaR7cmjaxjnYH1rsx/4vAT3XXkC7KSN1ktqo5QNmQnMv5RRtaS5589Sb/O869AdWXcOk0wEu7AAAAAElFTkSuQmCC";
        private static string Icon2_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAAEVSURBVFhH7ZZBDgIhDEXBeDo33mX2RifuuYsbrzfysSZEKVTaxo0vIcgshvb3t2P4NZF2dw6n2yVv5+cprPfrEef5AJZlqV/YJaUUcwAbHQs5gHL3rpzmEF0+QqNAyQjZlQcDuBJMgwBeQWgYRj+qtVQBDokHTGo9DSe1VQk0XdAFpkPr0WINJ/FA0+297Ht9/45GgZV2FdMKjCDZ9X1vZTaOKH25tt853LrADO8SuCoAI9IcYGfBnvZpsjrNbwV5pn6O3x9BWCig+laYlQAZ14se18OqObhcPYDhgxFMq+kB8SQcUWX9FRIFTGa+G9o54eqBGm4mqOe7JHv4AxfTsQBjYrdQQOURtQJSSHb9/4M/toTwAPYQkgshHGVKAAAAAElFTkSuQmCC";

        private static string Icon3_16Base64 = "iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAACzSURBVDhPpZNLDsMgDEShyum6yV3YV02UPXfpptdLPJaNTA0JUZ9k8ZPxMHFiuMnz9VloeFOs321eHrx7QUppR8gSyWWceNrAVsrbbJWuFLzPq1+0El2wa/BBh7MnaAUeoUguhLLCsIlWCZlX8ioFvSpCpUiJ+uacc+xVOWNCoswBbu86LMr4HD2AvWEPQEuh+wqmYVqeOB+cgrueuE688kTUVD4M0evQoZ8JGGXOhz8I4QCVRWeniwbFsAAAAABJRU5ErkJggg==";
        private static string Icon3_24Base64 = "iVBORw0KGgoAAAANSUhEUgAAABgAAAAYCAYAAADgdz34AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADLSURBVEhL1ZPBDsIgEETB+HVe/BfuRhvv/IsXf0+ZZtKQsgtLWxJ9CVlIU4aZBTcaz3oIl9vrkco9jen9vGLeJxBC+HBaEGP0SWD5ngTmvX/HgfSzBVUgj0Ozb+HEKoKNMbicVnUftYbmIDq4Y4QFZ1YTa1G6Q18AaiFizlKj1XxRoPVTD16yjUy57LoxEuMdsO6idiBVANFlb2DhsEit7wDAAUToxIYksGWjakSczmyNwpYX4cm7bleXQAvpAN0CWvO1CIc7+Hec+wKbIIohffvOSQAAAABJRU5ErkJggg==";
        private static string Icon3_32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsIAAA7CARUoSoAAAADuSURBVFhH7ZZBDsIwDAQTnseFv/SOoOKev3Dhe5CEFFkFJ3a8oUJipCp1K9Xb7Naq2xpf1uHsj9dzXE7Pys23yyHV/QKmaaIPrBJC8FHAvZSZKCD33uWqD1HzFr9pAfewHpoC1l7X/OxBkgGI1xziEKY3T0cp57Im6LkaiQV5u0lzEdKcdAtYrn9CkxPLHDBt/YJqWzVALEB+7xye81LjowVLBiBsb0FZhyB5gaaA1iCK92mTF9IMITLw1lyDeQdqDLcAEVKxgDWoOSHJAGTmc6gVU75iAQpOrFkAlxFKLS+IOTD2lwwFIi9/BuDcA32+lH1pIG+BAAAAAElFTkSuQmCC";

        public static Page GetIcon1(double scaling)
        {
            return GetIcon(scaling, ref Icon1_16Base64, ref Icon1_24Base64, ref Icon1_32Base64, 32);
        }

        public static Page GetIcon2(double scaling)
        {
            return GetIcon(scaling, ref Icon2_16Base64, ref Icon2_24Base64, ref Icon2_32Base64, 32);
        }

        public static Page GetIcon3(double scaling)
        {
            return GetIcon(scaling, ref Icon3_16Base64, ref Icon3_24Base64, ref Icon3_32Base64, 32);
        }

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Base64, ref Icon48Base64, ref Icon64Base64, 32);
        }

        public static Page GetIcon(double scaling, ref string icon1, ref string icon15, ref string icon2, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon1);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon15);
            }
            else
            {
                bytes = Convert.FromBase64String(icon2);
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

            Page pag = new Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        public static List<bool> IsAvailable(TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            return new List<bool>() { selection != null, selection != null, selection != null, selection != null };
        }

        public static async void PerformAction(int actionIndex, TreeNode selection, MainWindow window, InstanceStateData stateData)
        {
            if (actionIndex <= 0)
            {
                string text = NWKA.WriteTree(selection, true);
                if (!text.EndsWith(";"))
                {
                    text += ";";
                }
                _ = Avalonia.Application.Current.Clipboard.SetTextAsync(text);
            }
            else if (actionIndex == 1)
            {
                List<TreeNode> selectedTips = window.SelectedNode.GetLeaves();

                ChildWindow attributeSelectionWindow = new ChildWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Icon = window.Icon, Width = 350, Height = 190, Title = "Select attribute...", WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(231, 231, 231)), CanMaximizeMinimize = false };

                Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
                attributeSelectionWindow.Content = grd;
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                {
					Grid header = new Grid();
					header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
					header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
					
					header.Children.Add(new DPIAwareBox(GetIcon) { Width = 32, Height = 32, Margin = new Avalonia.Thickness(0, 0, 10, 0) });
					
                    TextBlock blk = new TextBlock() { Text = "Copy attribute at tips", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 0, 10), FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)) };
                    Grid.SetColumn(blk, 1);
					header.Children.Add(blk);
					
					Grid.SetColumnSpan(header, 2);
                    grd.Children.Add(header);
                }

                {
                    TextBlock blk = new TextBlock() { Text = selectedTips.Count + " tips selected.", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 5, 0, 10), FontSize = 13, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(102, 102, 102)) };
                    Grid.SetColumnSpan(blk, 2);
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { Text = "Select attribute to copy:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 2);
                    grd.Children.Add(blk);
                }

                Grid buttonGrid = new Grid();
                Grid.SetColumnSpan(buttonGrid, 2);

                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Button okButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "OK", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black } };
                okButton.Classes.Add("SideBarButton");
                Grid.SetColumn(okButton, 1);
                buttonGrid.Children.Add(okButton);

                Button cancelButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "Cancel", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black }, Foreground = Avalonia.Media.Brushes.Black };
                cancelButton.Classes.Add("SideBarButton");
                Grid.SetColumn(cancelButton, 3);
                buttonGrid.Children.Add(cancelButton);

                Grid.SetRow(buttonGrid, 4);
                grd.Children.Add(buttonGrid);

                bool result = false;

                okButton.Click += (s, e) =>
                {
                    result = true;
                    attributeSelectionWindow.Close();
                };

                cancelButton.Click += (s, e) =>
                {
                    attributeSelectionWindow.Close();
                };

                HashSet<string> attributes = new HashSet<string>();

                foreach (TreeNode node in selectedTips)
                {
                    foreach (KeyValuePair<string, object> attribute in node.Attributes)
                    {
                        attributes.Add(attribute.Key);
                    }
                }

                List<string> attributesList = attributes.ToList();

                ComboBox attributeBox = new ComboBox() { Items = attributesList, SelectedIndex = Math.Max(attributesList.IndexOf("Name"), 0), Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                Grid.SetRow(attributeBox, 2);
                Grid.SetColumn(attributeBox, 1);
                grd.Children.Add(attributeBox);


                await attributeSelectionWindow.ShowDialog2(window);

                if (result)
                {
                    string attributeName = attributesList[attributeBox.SelectedIndex];

                    List<string> attributeValues = new List<string>();

                    if (attributeName != null)
                    {
                        foreach (TreeNode node in selectedTips)
                        {
                            if (node.Attributes.TryGetValue(attributeName, out object attributeValue))
                            {
                                if (attributeValue is string attributeString)
                                {
                                    attributeValues.Add(attributeString);
                                }
                                else if (attributeValue is double attributeDouble)
                                {
                                    attributeValues.Add(attributeDouble.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }

                    if (attributeValues.Count > 0)
                    {
                        _ = Avalonia.Application.Current.Clipboard.SetTextAsync(attributeValues.Aggregate((a, b) => a + "\n" + b));
                    }
                }
            }
            else if (actionIndex == 2)
            {
                List<TreeNode> selectedTips = window.SelectedNode.GetChildrenRecursive();

                ChildWindow attributeSelectionWindow = new ChildWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Icon = window.Icon, Width = 350, Height = 190, Title = "Select attribute...", WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(231, 231, 231)), CanMaximizeMinimize = false };

                Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
                attributeSelectionWindow.Content = grd;
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                {
					Grid header = new Grid();
					header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
					header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
					
					header.Children.Add(new DPIAwareBox(GetIcon) { Width = 32, Height = 32, Margin = new Avalonia.Thickness(0, 0, 10, 0) });
					
                    TextBlock blk = new TextBlock() { Text = "Copy attribute at nodes", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 0, 10), FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)) };
                    Grid.SetColumn(blk, 1);
					header.Children.Add(blk);
					
					Grid.SetColumnSpan(header, 2);
                    grd.Children.Add(header);
                }

                {
                    TextBlock blk = new TextBlock() { Text = selectedTips.Count + " nodes selected.", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 5, 0, 10), FontSize = 13, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(102, 102, 102)) };
                    Grid.SetColumnSpan(blk, 2);
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { Text = "Select attribute to copy:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 2);
                    grd.Children.Add(blk);
                }

                Grid buttonGrid = new Grid();
                Grid.SetColumnSpan(buttonGrid, 2);

                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Button okButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "OK", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black } };
                okButton.Classes.Add("SideBarButton");
                Grid.SetColumn(okButton, 1);
                buttonGrid.Children.Add(okButton);

                Button cancelButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "Cancel", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black }, Foreground = Avalonia.Media.Brushes.Black };
                cancelButton.Classes.Add("SideBarButton");
                Grid.SetColumn(cancelButton, 3);
                buttonGrid.Children.Add(cancelButton);

                Grid.SetRow(buttonGrid, 4);
                grd.Children.Add(buttonGrid);

                bool result = false;

                okButton.Click += (s, e) =>
                {
                    result = true;
                    attributeSelectionWindow.Close();
                };

                cancelButton.Click += (s, e) =>
                {
                    attributeSelectionWindow.Close();
                };

                HashSet<string> attributes = new HashSet<string>();

                foreach (TreeNode node in selectedTips)
                {
                    foreach (KeyValuePair<string, object> attribute in node.Attributes)
                    {
                        attributes.Add(attribute.Key);
                    }
                }

                List<string> attributesList = attributes.ToList();

                ComboBox attributeBox = new ComboBox() { Items = attributesList, SelectedIndex = Math.Max(attributesList.IndexOf("Name"), 0), Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                Grid.SetRow(attributeBox, 2);
                Grid.SetColumn(attributeBox, 1);
                grd.Children.Add(attributeBox);


                await attributeSelectionWindow.ShowDialog2(window);

                if (result)
                {
                    string attributeName = attributesList[attributeBox.SelectedIndex];

                    List<string> attributeValues = new List<string>();

                    if (attributeName != null)
                    {
                        foreach (TreeNode node in selectedTips)
                        {
                            if (node.Attributes.TryGetValue(attributeName, out object attributeValue))
                            {
                                if (attributeValue is string attributeString)
                                {
                                    attributeValues.Add(attributeString);
                                }
                                else if (attributeValue is double attributeDouble)
                                {
                                    attributeValues.Add(attributeDouble.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                }
                            }
                        }
                    }

                    if (attributeValues.Count > 0)
                    {
                        _ = Avalonia.Application.Current.Clipboard.SetTextAsync(attributeValues.Aggregate((a, b) => a + "\n" + b));
                    }
                }
            }
        }
    }
}
