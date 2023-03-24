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

using Avalonia.Controls;
using Avalonia.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using PhyloTree;
using VectSharp.Canvas;
using Avalonia.Styling;

namespace TreeViewer
{
    public partial class MainWindow
    {
        public static readonly Avalonia.StyledProperty<bool> IsSelectionAvailableProperty = Avalonia.AvaloniaProperty.Register<MainWindow, bool>(nameof(IsSelectionAvailable), false);
        public bool IsSelectionAvailable
        {
            get { return GetValue(IsSelectionAvailableProperty); }
            set { SetValue(IsSelectionAvailableProperty, value); }
        }

        public static readonly Avalonia.StyledProperty<TreeNode> SelectedNodeProperty = Avalonia.AvaloniaProperty.Register<MainWindow, TreeNode>(nameof(SelectedNode), null);
        public TreeNode SelectedNode
        {
            get { return GetValue(SelectedNodeProperty); }
            set { SetValue(SelectedNodeProperty, value); }
        }

        static Func<object, string> StringAttributeFormatter = new FormatterOptions(Modules.SafeAttributeConverters[0]).Formatter;
        static Func<object, string> NumberAttributeFormatter = new FormatterOptions(Modules.SafeAttributeConverters[1]).Formatter;

        public HashSet<SKRenderAction> ActionsWhoseColourHasBeenChanged = new HashSet<SKRenderAction>();

        public void ChangeActionColour(SKRenderAction action, SkiaSharp.SKColor newColour)
        {
            if (!action.Disposed)
            {
                action.Paint.Color = newColour;
                ActionsWhoseColourHasBeenChanged.Add(action);
            }
        }

        Avalonia.Media.ISolidColorBrush transparentBrush = new Avalonia.Media.SolidColorBrush(0x00000000);

        enum HighlightModes
        {
            OnlyParent,
            ParentAndChildren,
            Auto
        }

        long MaxHighlightTime = 500;

        HighlightModes HighlightMode = HighlightModes.Auto;

        public void ResetActionColours(bool invalidateVisuals = true)
        {
            foreach (SKRenderAction pth in ActionsWhoseColourHasBeenChanged)
            {
                if (!pth.Disposed)
                {
                    pth.Paint.Color = TransparentSKColor;
                }
            }

            if (invalidateVisuals)
            {
                FullSelectionCanvas.InvalidateDirty();
            }

            ActionsWhoseColourHasBeenChanged.Clear();
        }

        public void SetSelection(TreeNode node)
        {
            for (int i = 0; i < SelectionCanvas.Children.Count; i++)
            {
                if (SelectionCanvas.Children[i] is Canvas can)
                {
                    _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        can.ZIndex = 0;
                    });
                }
            }

            ResetActionColours();

            if (node != null)
            {
                SkiaSharp.SKColor selectionColor = SelectionSKColor;
                SkiaSharp.SKColor selectionChildColor = SelectionChildSKColor;

                if (HighlightMode == HighlightModes.ParentAndChildren || HighlightMode == HighlightModes.Auto)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    List<string> childIds = new List<string>((from el in node.GetChildrenRecursiveLazy() select el.Id).Skip(1));

                    foreach (string childId in childIds)
                    {
                        foreach ((double, SKRenderAction) pth in FindPaths(FullSelectionCanvas, childId))
                        {
                            pth.Item2.ZIndex = 1;
                            ChangeActionColour(pth.Item2, selectionChildColor);
                        }

                        if (sw.ElapsedMilliseconds > MaxHighlightTime)
                        {
                            ResetActionColours();
                            break;
                        }
                    }

                    sw.Stop();
                }

                var nodePaths = FindPaths(FullSelectionCanvas, node.Id);

                foreach ((double, SKRenderAction) pth in nodePaths)
                {
                    pth.Item2.ZIndex = 2;
                    ChangeActionColour(pth.Item2, selectionColor);
                }

                FullSelectionCanvas.InvalidateZIndex();

                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Clear();
                this.FindControl<TextBlock>("SelectedNodeTextBlock").Text = node.ToString();

                int nodeCount = 0;
                int tipCount = 0;
                List<string> names = new List<string>();

                foreach (TreeNode child in node.GetChildrenRecursiveLazy())
                {
                    nodeCount++;

                    if (child.Children.Count == 0)
                    {
                        tipCount++;
                        if (!string.IsNullOrEmpty(child.Name))
                        {
                            names.Add(child.Name);
                        }
                    }
                }

                this.FindControl<TextBlock>("SelectedNodeNodeCountBlock").Text = nodeCount.ToString() + " node" + (nodeCount > 1 ? "s" : "");
                this.FindControl<TextBlock>("SelectedNodeLeafCountBlock").Text = tipCount.ToString() + " tip" + (tipCount > 1 ? "s" : "");

                this.FindControl<TextBlock>("SelectedNodeNamedLeafCountBlock").Text = names.Count.ToString() + (names.Count > 1 ? " named tips:" : " named tip:");

                StringBuilder text = new StringBuilder();
                for (int i = 0; i < names.Count; i++)
                {
                    if (i < names.Count - 1)
                    {
                        text.AppendLine(names[i]);
                    }
                    else
                    {
                        text.Append(names[i]);
                    }
                }

                this.FindControl<TextBox>("SelectedNodeLeavesBox").Text = text.ToString();

                int ind = 0;

                foreach (KeyValuePair<string, object> kvp in node.Attributes)
                {
                    Grid attributeString = new Grid() { Margin = new Avalonia.Thickness(5, 5, 0, 0), Height = 20 };
                    attributeString.Classes.Add("AttributeLine");


                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    TrimmedTextBox2 attributeNameBlock = new TrimmedTextBox2() { MaxWidth = 120, Margin = new Avalonia.Thickness(5, 0, 0, 0) };

                    attributeNameBlock.FontFamily = "resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans";
                    attributeNameBlock.FontSize = 13;
                    attributeNameBlock.FontWeight = Avalonia.Media.FontWeight.Bold;
                    attributeNameBlock.Text = kvp.Key + ":";
                    attributeNameBlock.EllipsisText = "... :";

                    AvaloniaBugFixes.SetToolTip(attributeNameBlock, new TextBlock() { Text = kvp.Key, FontWeight = Avalonia.Media.FontWeight.Regular });

                    attributeString.Children.Add(attributeNameBlock);
                    double nameWidth = AvaloniaBugFixes.MeasureTextWidth(attributeNameBlock.Text, attributeNameBlock.FontFamily, attributeNameBlock.FontStyle, attributeNameBlock.FontWeight, attributeNameBlock.FontSize);

                    TrimmedTextBox2 valueBlock = new TrimmedTextBox2() { FontSize = 13, Margin = new Avalonia.Thickness(0), MaxWidth = 0, Background = Avalonia.Media.Brushes.Transparent };

                    string attributeType = node.GetAttributeType(kvp.Key);

                    if (attributeType == "String")
                    {
                        string tipText = StringAttributeFormatter(kvp.Value);

                        if (!string.IsNullOrEmpty(tipText))
                        {
                            valueBlock.Text = tipText;
                        }
                    }
                    else if (attributeType == "Number")
                    {
                        string tipText = NumberAttributeFormatter(kvp.Value);

                        if (!string.IsNullOrEmpty(tipText))
                        {
                            valueBlock.Text = tipText;
                        }
                    }

                    AvaloniaBugFixes.SetToolTip(valueBlock, new TextBlock() { Text = valueBlock.Text, FontWeight = Avalonia.Media.FontWeight.Regular });


                    Grid valueBlockParent = new Grid() { Margin = new Avalonia.Thickness(6, 0, 5, 0) };
                    Grid.SetColumn(valueBlockParent, 1);
                    valueBlockParent.Children.Add(valueBlock);

                    valueBlockParent.PropertyChanged += (s, e) =>
                    {
                        if (e.Property == Grid.BoundsProperty)
                        {
                            valueBlock.MaxWidth = ((Avalonia.Rect)e.NewValue).Width - 10;
                        }
                    };


                    TextBox editValueBox = new TextBox() { Margin = new Avalonia.Thickness(0, 0, 0, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), IsVisible = false, Height = 20, MaxHeight = 20, MinHeight = 20, MaxWidth = 0, FontSize = 13, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    editValueBox.Classes.Add("EditValueBox");

                    Button editButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.Pencil")) { Width = 16, Height = 16 }, Padding = new Avalonia.Thickness(2) };
                    editButton.Classes.Add("SideBarButton");
                    editButton.Classes.Add("AttributeEditButton");
                    Grid.SetColumn(editButton, 2);

                    Button okButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.TickGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2), IsVisible = false };
                    okButton.Classes.Add("SideBarButton");
                    okButton.Classes.Add("AttributeEditButton");
                    Grid.SetColumn(okButton, 2);

                    Button cancelButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2), IsVisible = false };
                    cancelButton.Classes.Add("SideBarButton");
                    cancelButton.Classes.Add("AttributeEditButton");
                    Grid.SetColumn(cancelButton, 3);

                    attributeString.Children.Add(editButton);
                    attributeString.Children.Add(okButton);
                    attributeString.Children.Add(cancelButton);

                    attributeString.Children.Add(valueBlockParent);

                    Grid editValueBoxParent = new Grid();
                    Grid.SetColumn(editValueBoxParent, 1);
                    editValueBoxParent.Children.Add(editValueBox);

                    editValueBoxParent.PropertyChanged += (s, e) =>
                    {
                        if (e.Property == Grid.BoundsProperty)
                        {
                            editValueBox.MaxWidth = ((Avalonia.Rect)e.NewValue).Width - 10;
                        }
                    };

                    attributeString.Children.Add(editValueBoxParent);

                    editButton.Click += (s, e) =>
                    {
                        valueBlock.IsVisible = false;
                        editValueBox.Text = valueBlock.Text;
                        editValueBox.IsVisible = true;
                        editValueBox.SelectAll();
                        editValueBox.Focus();
                        editButton.Classes.Add("EditButtonHidden");
                        okButton.IsVisible = true;
                        cancelButton.IsVisible = true;
                    };

                    valueBlock.DoubleTapped += (s, e) =>
                    {
                        valueBlock.IsVisible = false;
                        editValueBox.Text = valueBlock.Text;
                        editValueBox.IsVisible = true;
                        editValueBox.SelectAll();
                        editValueBox.Focus();
                        editButton.Classes.Add("EditButtonHidden");
                        okButton.IsVisible = true;
                        cancelButton.IsVisible = true;
                    };

                    cancelButton.Click += (s, e) =>
                    {
                        valueBlock.IsVisible = true;
                        editValueBox.IsVisible = false;
                        editButton.Classes.Remove("EditButtonHidden");
                        okButton.IsVisible = false;
                        cancelButton.IsVisible = false;
                    };

                    async void commitAttributeChange()
                    {
                        valueBlock.IsVisible = !false;
                        editValueBox.IsVisible = !true;
                        editButton.Classes.Remove("EditButtonHidden");
                        okButton.IsVisible = !true;
                        cancelButton.IsVisible = !true;


                        string value = editValueBox.Text;

                        if (attributeType == "Number")
                        {
                            if (!double.TryParse(editValueBox.Text, out _))
                            {
                                await new MessageBox("Attention", "Could not interpret the attribute as a valid Number!").ShowDialog2(this);
                                return;
                            }
                        }

                        List<string> nodeNames = node.GetNodeNames();

                        if (nodeNames.Count == 0 || !node.IsLastCommonAncestor(nodeNames))
                        {
                            MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                            await box.ShowDialog2(this);
                            return;
                        }

                        string id = this.SelectedNode.Id;

                        Action<Dictionary<string, object>> changeParameter = AddFurtherTransformation((from el in Modules.FurtherTransformationModules where el.Id == "8de06406-68e4-4bd8-97eb-2185a0dd1127" select el).First());

                        changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Attribute:", kvp.Key }, { "Attribute type:", attributeType }, { "New value:", value } });

                        await UpdateFurtherTransformations(this.FurtherTransformations.Count - 1);

                        SetSelection(TransformedTree.GetNodeFromId(id));
                    };

                    editValueBox.KeyDown += (s, e) =>
                    {
                        if (e.Key == Key.Enter)
                        {
                            commitAttributeChange();
                        }
                        else if (e.Key == Key.Escape)
                        {
                            valueBlock.IsVisible = true;
                            editValueBox.IsVisible = false;
                            editButton.Classes.Remove("EditButtonHidden");
                            okButton.IsVisible = false;
                            cancelButton.IsVisible = false;
                        }
                    };

                    okButton.Click += (s, e) =>
                    {
                        commitAttributeChange();
                    };

                    this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(attributeString);

                    ind++;
                }

                IsSelectionAvailable = true;
                SelectedNode = node;

                if (!SelectionPanelManuallyClosed)
                {
                    IsSelectionPanelOpen = true;
                }
            }
            else
            {
                SelectionCanvas.InvalidateVisual();
                IsSelectionAvailable = false;
                SelectedNode = null;
                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Clear();

                if (IsSelectionPanelOpen)
                {
                    SelectionPanelManuallyClosed = false;
                }

                IsSelectionPanelOpen = false;
            }
        }

        public Canvas SelectionCanvas;

        private async void AddAttributeButtonClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            NewAttributeWindow win = new NewAttributeWindow();
            await win.ShowDialog2(this);

            if (win.Result)
            {

                string value = win.AttributeValue;

                if (win.AttributeType == "Number")
                {
                    if (!double.TryParse(win.AttributeValue, out _))
                    {
                        await new MessageBox("Attention", "Could not interpret the attribute as a valid Number!").ShowDialog2(this);
                        return;
                    }
                }

                List<string> nodeNames = this.SelectedNode.GetNodeNames();

                if (nodeNames.Count == 0 || !this.SelectedNode.IsLastCommonAncestor(nodeNames))
                {
                    MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                    await box.ShowDialog2(this);
                    return;
                }

                string id = this.SelectedNode.Id;

                Action<Dictionary<string, object>> addParameter = AddFurtherTransformation((from el in Modules.FurtherTransformationModules where el.Id == "afb64d72-971d-4780-8dbb-a7d9248da30b" select el).First());

                addParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Attribute:", win.AttributeName }, { "Attribute type:", win.AttributeType }, { "New value:", value } });

                await UpdateFurtherTransformations(this.FurtherTransformations.Count - 1);

                SetSelection(TransformedTree.GetNodeFromId(id));
            }
        }

        private void UpdateSelectionWidth()
        {
            if (SelectionCanvas != null)
            {
                Avalonia.Controls.PanAndZoom.ZoomBorder zom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");
                double zoom = zom.ZoomX;

                foreach ((double, SKRenderAction) pth in FindPaths(FullSelectionCanvas, null))
                {
                    double orig = Math.Max(0, pth.Item1);

                    if (pth.Item2.Paint.Style == SkiaSharp.SKPaintStyle.Stroke)
                    {
                        pth.Item2.Paint.StrokeWidth = (float)((orig * 5) * zoom >= 5 ? (orig * 5) : (5 / zoom));
                    }
                }
            }

            SelectionCanvas.InvalidateVisual();
        }

        private void UpdateSelectionWidth(SKMultiLayerRenderCanvas parent)
        {
            if (!Modules.IsLinux)
            {
                Avalonia.Controls.PanAndZoom.ZoomBorder zom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");
                double zoom = zom.ZoomX;

                foreach ((double, SKRenderAction) pth in FindPaths(parent, null))
                {
                    double orig = Math.Max(0, pth.Item1);

                    if (pth.Item2.Paint.Style == SkiaSharp.SKPaintStyle.Stroke || pth.Item2.Paint.Style == SkiaSharp.SKPaintStyle.StrokeAndFill)
                    {
                        float newStrokeWidth = (float)((orig * 5) * zoom >= 5 ? (orig * 5) : (5 / zoom));

                        if (newStrokeWidth != pth.Item2.Paint.StrokeWidth)
                        {
                            pth.Item2.Paint.StrokeWidth = newStrokeWidth;
                            pth.Item2.InvalidateHitTestPath();
                        }
                    }

                }
            }
            else
            {
                Avalonia.Controls.PanAndZoom.ZoomBorder zom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");
                double zoom = zom.ZoomX;

                foreach ((double, SKRenderAction) pth in FindPaths(parent, null))
                {
                    double orig = Math.Max(0, pth.Item1);

                    if (pth.Item2.Paint.Style == SkiaSharp.SKPaintStyle.Stroke || pth.Item2.Paint.Style == SkiaSharp.SKPaintStyle.StrokeAndFill)
                    {
                        float newStrokeWidth = (float)((orig * 5) * zoom >= 5 ? (orig * 5) : (5 / zoom));

                        if (newStrokeWidth != pth.Item2.Paint.StrokeWidth)
                        {
                            pth.Item2.Paint.StrokeWidth = newStrokeWidth;
                            pth.Item2.Paint.StrokeJoin = SkiaSharp.SKStrokeJoin.Round;
                            pth.Item2.InvalidateHitTestPath();
                        }
                    }
                }
            }

            parent.InvalidateDirty();
        }

        bool IsPointerPressed = false;
        public bool HasPointerDoneSomething = false;

        private void BackgroundCanvasPointerPressed(object sender, PointerPressedEventArgs e)
        {
            IsPointerPressed = true;
        }

        private void BackgroundCanvasPointerReleased(object sender, PointerReleasedEventArgs e)
        {
            if (!HasPointerDoneSomething)
            {
                SetSelection(null);
            }

            HasPointerDoneSomething = false;
            IsPointerPressed = false;
        }

        private void BackgroundCanvasPointerMoved(object sender, PointerEventArgs e)
        {
            if (IsPointerPressed)
            {
                HasPointerDoneSomething = true;
                WasAutoFitted = false;
            }
        }

        private async void CopySelectionButtonClicked(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (this.SelectedNode != null)
            {
                List<TreeNode> selectedTips = this.SelectedNode.GetLeaves();

                Window attributeSelectionWindow = new Window() { FontFamily = this.FontFamily, FontSize = this.FontSize, Icon = this.Icon, Width = 300, Height = 180, Title = "Select attribute...", WindowStartupLocation = WindowStartupLocation.CenterOwner }; ;

                Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
                attributeSelectionWindow.Content = grd;

                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

                {
                    TextBlock blk = new TextBlock() { Text = selectedTips.Count + " tips selected.", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { Text = "Select attribute to copy:", FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 15, Margin = new Avalonia.Thickness(0, 0, 0, 10) };
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                Grid buttonGrid = new Grid();

                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Button okButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = "OK" };
                Grid.SetColumn(okButton, 1);
                buttonGrid.Children.Add(okButton);

                Button cancelButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = "Cancel" };
                Grid.SetColumn(cancelButton, 3);
                buttonGrid.Children.Add(cancelButton);

                Grid.SetRow(buttonGrid, 5);
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

                ComboBox attributeBox = new ComboBox() { Items = attributesList, SelectedIndex = Math.Max(attributesList.IndexOf("Name"), 0), Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center };
                Grid.SetRow(attributeBox, 3);
                grd.Children.Add(attributeBox);


                await attributeSelectionWindow.ShowDialog2(this);

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
