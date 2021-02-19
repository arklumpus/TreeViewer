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

        public HashSet<RenderAction> ActionsWhoseColourHasBeenChanged = new HashSet<RenderAction>();

        public void ChangeActionFill(RenderAction action, Avalonia.Media.IBrush newFill)
        {
            action.Fill = newFill;
            ActionsWhoseColourHasBeenChanged.Add(action);
        }

        public void ChangeActionStroke(RenderAction action, Avalonia.Media.IBrush newStroke)
        {
            action.Stroke.Brush = newStroke;
            ActionsWhoseColourHasBeenChanged.Add(action);
        }

        static Avalonia.Media.ISolidColorBrush transparentBrush = new Avalonia.Media.SolidColorBrush(0x00000000);

        enum HighlightModes
        {
            OnlyParent,
            ParentAndChildren,
            Auto
        }

        long MaxHighlightTime = 500;

        HighlightModes HighlightMode = HighlightModes.Auto;

        public void ResetActionColours(bool invalidateVisuals = false)
        {
            foreach (RenderAction pth in ActionsWhoseColourHasBeenChanged)
            {
                if (pth.Fill != null)
                {
                    pth.Fill = transparentBrush;
                }

                if (pth.Stroke != null)
                {
                    pth.Stroke.Brush = transparentBrush;
                }
                pth.Parent.InvalidateVisual();
            }
            ActionsWhoseColourHasBeenChanged.Clear();
        }

        public void SetSelection(TreeNode node)
        {


            /*foreach (Avalonia.Controls.Shapes.Path pth in FindPaths(SelectionCanvas))
            {

                if (pth.Tag as object[] != null)
                {
                    pth.ZIndex = 0;

                    Canvas can = pth.Parent as Canvas;

                    while (can != SelectionCanvas)
                    {
                        can.ZIndex = 0;
                        can = can.Parent as Canvas;
                    }

                    if (pth.Fill != null)
                    {
                        pth.Fill = transparentBrush;
                    }

                    pth.Stroke = transparentBrush;
                }
            }*/

            for (int i = 0; i < SelectionCanvas.Children.Count; i++)
            {
                if (SelectionCanvas.Children[i] is Canvas can)
                {
                    can.ZIndex = 0;
                }
            }

            ResetActionColours();

            if (node != null)
            {
                Avalonia.Media.IBrush selectionBrush = SelectionBrush;
                Avalonia.Media.IBrush selectionChildBrush = SelectionChildBrush;

                /*foreach (Avalonia.Controls.Shapes.Path pth in FindPaths(SelectionCanvas))
                {
                    if (pth.Tag is object[] tag)
                    {
                        string id = (string)tag[0];
                        if (id == node.Id)
                        {
                            pth.ZIndex = 100;

                            Canvas can = pth.Parent as Canvas;

                            while (can != SelectionCanvas)
                            {
                                can.ZIndex = 100;
                                can = can.Parent as Canvas;
                            }

                            if (pth.Fill != null)
                            {
                                pth.Fill = selectionBrush;
                            }

                            pth.Stroke = selectionBrush;
                        }
                        else if (childIds.Contains(id))
                        {
                            pth.ZIndex = 90;

                            Canvas can = pth.Parent as Canvas;

                            while (can != SelectionCanvas)
                            {
                                can.ZIndex = 90;
                                can = can.Parent as Canvas;
                            }

                            if (pth.Fill != null)
                            {
                                pth.Fill = selectionChildBrush;
                            }

                            pth.Stroke = selectionChildBrush;
                        }
                    }
                }*/

                /*List<RenderAction> toBeBroughtForward = new List<RenderAction>();
                HashSet<Canvas> toBeBroughtForwardCanvas = new HashSet<Canvas>();
                
                foreach ((double, RenderAction) pth in FindPaths(SelectionCanvas, null))
                {
                    if (pth.Item2.Tag == node.Id)
                    {
                        toBeBroughtForward.Add(pth.Item2);
                        Canvas can = pth.Item2.Parent as Canvas;

                        while (can != SelectionCanvas)
                        {
                            toBeBroughtForwardCanvas.Add(can);
                            can = can.Parent as Canvas;
                        }


                        if (pth.Item2.Fill != null)
                        {
                            ChangeActionFill(pth.Item2, selectionBrush);
                        }

                        if (pth.Item2.Stroke != null)
                        {
                            ChangeActionStroke(pth.Item2, selectionBrush);
                        }
                    }
                    else if (childIds.Contains(pth.Item2.Tag))
                    {
                        pth.Item2.BringToFront();
                        Canvas can = pth.Item2.Parent as Canvas;

                        while (can != SelectionCanvas)
                        {
                            can.ZIndex = 90;
                            can = can.Parent as Canvas;
                        }

                        if (pth.Item2.Fill != null)
                        {
                            ChangeActionFill(pth.Item2, selectionChildBrush);
                        }

                        if (pth.Item2.Stroke != null)
                        {
                            ChangeActionStroke(pth.Item2, selectionChildBrush);
                        }
                    }
                }*/



                /* for (int i = 0; i < toBeBroughtForward.Count; i++)
                 {
                     toBeBroughtForward[i].BringToFront();
                 }

                 foreach (Canvas can in toBeBroughtForwardCanvas)
                 {
                     can.ZIndex = 100;
                 }*/

                if (HighlightMode == HighlightModes.ParentAndChildren || HighlightMode == HighlightModes.Auto)
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();

                    List<string> childIds = new List<string>((from el in node.GetChildrenRecursiveLazy() select el.Id).Skip(1));

                    foreach (string childId in childIds)
                    {
                        foreach ((double, RenderAction) pth in FindPaths(SelectionCanvas, childId))
                        {
                            pth.Item2.BringToFront();
                            /*Canvas can = pth.Item2.Parent as Canvas;

                            while (can != SelectionCanvas)
                            {
                                can.ZIndex = 90;
                                can = can.Parent as Canvas;
                            }*/

                            pth.Item2.Parent.InvalidateVisual();

                            if (pth.Item2.Fill != null)
                            {
                                ChangeActionFill(pth.Item2, selectionChildBrush);
                            }

                            if (pth.Item2.Stroke != null)
                            {
                                ChangeActionStroke(pth.Item2, selectionChildBrush);
                            }
                        }

                        if (sw.ElapsedMilliseconds > MaxHighlightTime)
                        {
                            ResetActionColours();
                            break;
                        }
                    }

                    sw.Stop();
                }

                foreach ((double, RenderAction) pth in FindPaths(SelectionCanvas, node.Id))
                {
                    pth.Item2.BringToFront();
                    Canvas can = pth.Item2.Parent as Canvas;

                    can.InvalidateVisual();

                    while (can != SelectionCanvas)
                    {
                        can.ZIndex = 100;
                        can = can.Parent as Canvas;
                    }

                    if (pth.Item2.Fill != null)
                    {
                        ChangeActionFill(pth.Item2, selectionBrush);
                    }

                    if (pth.Item2.Stroke != null)
                    {
                        ChangeActionStroke(pth.Item2, selectionBrush);
                    }
                }
                
                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Clear();

                Grid nodeString = new Grid() { Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(240, 240, 240)) };
                nodeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                nodeString.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                nodeString.Children.Add(new TextBlock() { Text = "Node:", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, FontWeight = Avalonia.Media.FontWeight.Bold });
                ScrollViewer nodeStringSV = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto };

                nodeStringSV.Content = new TextBlock() { Text = node.ToString(), Margin = new Avalonia.Thickness(5, 0, 0, 0) };
                Grid.SetColumn(nodeStringSV, 1);
                nodeString.Children.Add(nodeStringSV);

                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(nodeString);


                Grid leavesContainer = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0) };
                leavesContainer.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                leavesContainer.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                List<string> names = node.GetLeafNames();

                leavesContainer.Children.Add(new TextBlock() { Text = names.Count.ToString() + (names.Count > 1 ? " leaves:" : " leaf:"), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, FontWeight = Avalonia.Media.FontWeight.Bold });
                /*ScrollViewer leavesSV = new ScrollViewer() { VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto, MaxHeight = 150 };

                StackPanel leavesItemsContainer = new StackPanel() { Margin = new Avalonia.Thickness(5, 0, 0, 0) };

                for (int i = 0; i < names.Count; i++)
                {
                    leavesItemsContainer.Children.Add(new TextBlock() { Text = names[i], Margin = new Avalonia.Thickness(0, 0, 0, i < names.Count - 1 ? 5 : 0) });
                }
                leavesSV.Content = leavesItemsContainer;*/


                TextBox leavesSV = new TextBox() { Margin = new Avalonia.Thickness(5, 0, 0, 0), AcceptsReturn = true, BorderBrush = null, BorderThickness = new Avalonia.Thickness(0), MaxHeight = 150, IsReadOnly = true };

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

                leavesSV.Text = text.ToString();

                Grid.SetColumn(leavesSV, 1);
                leavesContainer.Children.Add(leavesSV);

                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(leavesContainer);


                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(new TextBlock() { Text = "Attributes", FontWeight = Avalonia.Media.FontWeight.Bold, FontSize = 18, Margin = new Avalonia.Thickness(0, 10, 0, 5) });

                int ind = 0;

                foreach (KeyValuePair<string, object> kvp in node.Attributes)
                {
                    Grid attributeString = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0) };


                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    attributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    TrimmedTextBox attributeNameBlock = new TrimmedTextBox(150) { MaxWidth = 150 };

                    attributeNameBlock.TextContainer.FontFamily = "resm:TreeViewer.Fonts.?assembly=TreeViewer#Open Sans";
                    attributeNameBlock.TextContainer.FontSize = 15;
                    attributeNameBlock.TextContainer.FontWeight = Avalonia.Media.FontWeight.Bold;
                    attributeNameBlock.Text = kvp.Key + ":";
                    attributeNameBlock.Ellipsis.Text = "... :";
                    attributeNameBlock.Ellipsis.FontWeight = Avalonia.Media.FontWeight.Bold;

                    ToolTip.SetTip(attributeNameBlock, kvp.Key);

                    attributeString.Children.Add(attributeNameBlock);

                    //FormattedText fmtText = new FormattedText(attributeNameBlock.TextContainer.Text, new Typeface(attributeNameBlock.TextContainer.FontFamily, attributeNameBlock.TextContainer.FontStyle, attributeNameBlock.TextContainer.FontWeight), attributeNameBlock.FontSize, TextAlignment.Left, TextWrapping.NoWrap, Avalonia.Size.Infinity);
                    double nameWidth = AvaloniaBugFixes.MeasureTextWidth(attributeNameBlock.TextContainer.Text, attributeNameBlock.TextContainer.FontFamily, attributeNameBlock.TextContainer.FontStyle, attributeNameBlock.TextContainer.FontWeight, attributeNameBlock.TextContainer.FontSize);

                    TrimmedTextBox valueBlock = new TrimmedTextBox(261 - 45 - 11 - Math.Min(150, nameWidth)) { Margin = new Avalonia.Thickness(11, 1, 5, 0) };

                    string attributeType = node.GetAttributeType(kvp.Key);

                    if (attributeType == "String")
                    {
                        valueBlock.Text = StringAttributeFormatter(kvp.Value);
                    }
                    else if (attributeType == "Number")
                    {
                        valueBlock.Text = NumberAttributeFormatter(kvp.Value);
                    }

                    ToolTip.SetTip(valueBlock, valueBlock.Text);

                    Grid.SetColumn(valueBlock, 1);
                    

                    TextBox editValueBox = new TextBox() { Margin = new Avalonia.Thickness(5, 0, 5, 0), Padding = new Avalonia.Thickness(5, 0, 5, 0), IsVisible = false, Height = 23, MaxHeight = 23, MinHeight = 23 };
                    Grid.SetColumn(editValueBox, 1);
                    

                    if (ind % 2 == 0)
                    {
                        attributeString.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(240, 240, 240));
                        editValueBox.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(240, 240, 240));
                    }

                    AddRemoveButton editButton = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Edit };
                    Grid.SetColumn(editButton, 2);

                    AddRemoveButton okButton = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.OK, IsVisible = false };
                    Grid.SetColumn(okButton, 3);

                    AddRemoveButton cancelButton = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Cancel, IsVisible = false };
                    Grid.SetColumn(cancelButton, 4);

                    attributeString.Children.Add(editButton);
                    attributeString.Children.Add(okButton);
                    attributeString.Children.Add(cancelButton);

                    attributeString.Children.Add(valueBlock);
                    attributeString.Children.Add(editValueBox);

                    editButton.PointerReleased += (s, e) =>
                    {
                        valueBlock.IsVisible = false;
                        editValueBox.Text = valueBlock.Text;
                        editValueBox.IsVisible = true;
                        editButton.IsVisible = false;
                        okButton.IsVisible = true;
                        cancelButton.IsVisible = true;
                    };

                    cancelButton.PointerReleased += (s, e) =>
                    {
                        valueBlock.IsVisible = !false;
                        editValueBox.IsVisible = !true;
                        editButton.IsVisible = !false;
                        okButton.IsVisible = !true;
                        cancelButton.IsVisible = !true;
                    };

                    okButton.PointerReleased += async (s, e) =>
                    {
                        valueBlock.IsVisible = !false;
                        editValueBox.IsVisible = !true;
                        editButton.IsVisible = !false;
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

                        Action<Dictionary<string, object>> changeParameter = AddFurtherTransformation((from el in Modules.FurtherTransformationModules where el.Id == "8de06406-68e4-4bd8-97eb-2185a0dd1127" select el).First());

                        changeParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Attribute:", kvp.Key }, { "Attribute type:", attributeType }, { "New value:", value } });

                        await UpdateFurtherTransformations(this.FurtherTransformations.Count - 1);

                        SetSelection(TransformedTree.GetLastCommonAncestor(nodeNames));
                    };

                    this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(attributeString);

                    ind++;
                }

                {
                    Grid addAttributeString = new Grid() { Margin = new Avalonia.Thickness(0, 5, 0, 0) };

                    addAttributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    addAttributeString.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    addAttributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    addAttributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    addAttributeString.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    AddRemoveButton addButton = new AddRemoveButton() { ButtonType = AddRemoveButton.ButtonTypes.Add };
                    Grid.SetColumn(addButton, 2);

                    addAttributeString.Children.Add(addButton);

                    addButton.PointerReleased += async (s, e) =>
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

                            List<string> nodeNames = node.GetNodeNames();

                            if (nodeNames.Count == 0 || !node.IsLastCommonAncestor(nodeNames))
                            {
                                MessageBox box = new MessageBox("Attention!", "The requested node cannot be uniquely identified! Please, make sure that it either has a Name or enough of its children have Names.");
                                await box.ShowDialog2(this);
                                return;
                            }

                            Action<Dictionary<string, object>> addParameter = AddFurtherTransformation((from el in Modules.FurtherTransformationModules where el.Id == "afb64d72-971d-4780-8dbb-a7d9248da30b" select el).First());

                            addParameter(new Dictionary<string, object>() { { "Node:", nodeNames.ToArray() }, { "Attribute:", win.AttributeName }, { "Attribute type:", win.AttributeType }, { "New value:", value } });

                            await UpdateFurtherTransformations(this.FurtherTransformations.Count - 1);

                            SetSelection(TransformedTree.GetLastCommonAncestor(nodeNames));
                        }
                    };

                    this.FindControl<StackPanel>("SelectionContainerPanel").Children.Add(addAttributeString);

                    ind++;
                }

                for (int i = 0; i < Modules.SelectionActionModules.Count; i++)
                {
                    int index = i;
                    ((Button)this.FindControl<Grid>("SelectionActionsContainerGrid").Children[i]).IsEnabled = Modules.SelectionActionModules[i].IsAvailable(node, this, this.StateData);
                    SelectionActionActions[i] = () =>
                    {
                        Modules.SelectionActionModules[index].PerformAction(node, this, this.StateData);
                    };
                }

                IsSelectionAvailable = true;
                SelectedNode = node;

                ExpandSelection();
            }
            else
            {
                SelectionCanvas.InvalidateVisual();
                IsSelectionAvailable = false;
                SelectedNode = null;
                this.FindControl<StackPanel>("SelectionContainerPanel").Children.Clear();
                ReduceSelection();
            }
        }

        public Canvas SelectionCanvas => this.FindControl<Canvas>("SelectionCanvas");

        private void UpdateSelectionWidth()
        {
            if (SelectionCanvas != null)
            {
                Avalonia.Controls.PanAndZoom.ZoomBorder zom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");
                double zoom = zom.ZoomX;

                foreach ((double, RenderAction) pth in FindPaths(SelectionCanvas, null))
                {
                    double orig = Math.Max(0, pth.Item1);
                    if (pth.Item2.Stroke != null)
                    {
                        pth.Item2.Stroke.Thickness = (orig * 5) * zoom >= 5 ? (orig * 5) : (5 / zoom);
                    }
                }
            }

            SelectionCanvas.InvalidateVisual();
        }

        private void UpdateSelectionWidth(Canvas parent)
        {
            Avalonia.Controls.PanAndZoom.ZoomBorder zom = this.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");
            double zoom = zom.ZoomX;

            foreach ((double, RenderAction) pth in FindPaths(parent, null))
            {
                double orig = Math.Max(0, pth.Item1);
                if (pth.Item2.Stroke != null)
                {
                    pth.Item2.Stroke.Thickness = (orig * 5) * zoom >= 5 ? (orig * 5) : (5 / zoom);
                }

            }

            parent.InvalidateVisual();
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
            }
        }
    }
}
