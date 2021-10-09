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

using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using System;
using System.Collections.Generic;
using System.IO;
using VectSharp.Canvas;

namespace TreeViewer
{
    public partial class RibbonFilePage : UserControl
    {
        public static readonly StyledProperty<int> SelectedIndexProperty = AvaloniaProperty.Register<RibbonBar, int>(nameof(SelectedIndex), 0);

        public int SelectedIndex
        {
            get { return GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        public event EventHandler BackButtonPressed;


        public RibbonFilePage()
        {
            InitializeComponent(new List<(double, string, Control, Control)>()
            {

            }, new List<(double, string, Control, Control)>()
            {

            }, new List<(double, string, Control, Control)>()
            {

            });
        }

        public RibbonFilePage(List<(double, string, Control, Control)> itemsFirstArea, List<(double, string, Control, Control)> itemsSecondArea, List<(double, string, Control, Control)> itemsThirdArea)
        {
            InitializeComponent(itemsFirstArea, itemsSecondArea, itemsThirdArea);
        }

        List<Border> BorderItems = new List<Border>();
        public List<Control> ClientItems = new List<Control>();

        public void SetEnabled(string label, bool value)
        {
            for (int i = 0; i < BorderItems.Count; i++)
            {
                if (((TextBlock)((Grid)BorderItems[i].Child).Children[1]).Text == label)
                {
                    BorderItems[i].IsEnabled = value;
                }
            }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RibbonFilePage.SelectedIndexProperty)
            {
                int oldValue = change.OldValue.GetValueOrDefault<int>();
                int newValue = change.NewValue.GetValueOrDefault<int>();
                this.BorderItems[oldValue].Classes.Remove("active");
                this.BorderItems[newValue].Classes.Add("active");

                if (this.ClientItems[newValue] != null)
                {
                    if (this.ClientItems[oldValue] != null)
                    {
                        this.ClientItems[oldValue].Opacity = 0;
                        this.ClientItems[oldValue].RenderTransform = OffScreen;

                        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                        {
                            await System.Threading.Tasks.Task.Delay(200);
                            this.ClientItems[oldValue].IsVisible = false;
                        });
                    }

                    this.ClientItems[newValue].IsVisible = true;
                    this.ClientItems[newValue].Opacity = 1;
                    this.ClientItems[newValue].RenderTransform = TransformOperations.Identity;
                }
                else
                {
                    this.SelectedIndex = oldValue;
                }
            }
        }

        private void BackButtonPressedEvent(object sender, PointerPressedEventArgs e)
        {
            e.GetCurrentPoint(this).Pointer.Capture(this);

            BackButtonPressed?.Invoke(this, new EventArgs());
        }

        public void Close()
        {
            BackButtonPressed?.Invoke(this, new EventArgs());
        }

        private void BuildPlatform()
        {
            Control windowIcon;
            Control windowTree;

            Image themeLeft1x = null;
            Image themeLeft15x = null;
            Image themeLeft2x = null;

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-1x.png")))
            {
                themeLeft1x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-1x.png")) };
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-1.5x.png")))
            {
                themeLeft15x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-1.5x.png")) };
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-2x.png")))
            {
                themeLeft2x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-home-2x.png")) };
            }

            if (themeLeft1x != null || themeLeft15x != null || themeLeft2x != null)
            {
                if (themeLeft2x != null)
                {
                    if (themeLeft15x == null)
                    {
                        themeLeft15x = themeLeft2x;
                    }

                    if (themeLeft1x == null)
                    {
                        themeLeft1x = themeLeft15x;
                    }
                }
                else if (themeLeft15x != null)
                {
                    themeLeft2x = themeLeft15x;

                    if (themeLeft1x == null)
                    {
                        themeLeft1x = themeLeft15x;
                    }
                }
                else
                {
                    themeLeft15x = themeLeft1x;
                    themeLeft2x = themeLeft15x;
                }

                themeLeft1x.Height = 66;
                themeLeft15x.Height = 66;
                themeLeft2x.Height = 66;

                windowIcon = new DPIAwareBox((scaling) => scaling <= 1 ? themeLeft1x : scaling <= 1.5 ? themeLeft15x : themeLeft2x);
            }
            else
            {
                windowIcon = new Canvas();
            }



            Image themeRight1x = null;
            Image themeRight15x = null;
            Image themeRight2x = null;

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-1x.png")))
            {
                themeRight1x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-1x.png")) };
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-1.5x.png")))
            {
                themeRight15x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-1.5x.png")) };
            }

            if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-2x.png")))
            {
                themeRight2x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-home-2x.png")) };
            }

            if (themeRight1x != null || themeRight15x != null || themeRight2x != null)
            {
                if (themeRight2x != null)
                {
                    if (themeRight15x == null)
                    {
                        themeRight15x = themeRight2x;
                    }

                    if (themeRight1x == null)
                    {
                        themeRight1x = themeRight15x;
                    }
                }
                else if (themeRight15x != null)
                {
                    themeRight2x = themeRight15x;

                    if (themeRight1x == null)
                    {
                        themeRight1x = themeRight15x;
                    }
                }
                else
                {
                    themeRight15x = themeRight1x;
                    themeRight2x = themeRight15x;
                }

                themeRight1x.Height = 66;
                themeRight15x.Height = 66;
                themeRight2x.Height = 66;

                windowTree = new DPIAwareBox((scaling) => scaling <= 1 ? themeRight1x : scaling <= 1.5 ? themeRight15x : themeRight2x);
            }
            else
            {
                windowTree = VectSharp.SVG.Parser.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.WindowTreeHome.svg")).PaintToCanvas(false);
            }

            this.FindControl<Viewbox>("WindowIconBox").Child = windowIcon;
            this.FindControl<Viewbox>("WindowTreeBox").Child = windowTree;
        }

        TransformOperations OffScreen;


        private void InitializeComponent(List<(double, string, Control, Control)> itemsFirstArea, List<(double, string, Control, Control)> itemsSecondArea, List<(double, string, Control, Control)> itemsThirdArea)
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Canvas>("BackArrowContainer").Children.Add(new DPIAwareBox(Icons.GetIcon16("TreeViewer.Assets.BackArrow")) { Width = 21, Height = 21 });

            BuildPlatform();

            TransformOperations.Builder builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(0, -16);
            OffScreen = builder.Build();

            for (int i = 0; i < itemsFirstArea.Count; i++)
            {
                Grid grd = new Grid();

                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(16, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));

                Control icon = itemsFirstArea[i].Item3;
                Grid.SetColumn(icon, 1);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(icon);

                TextBlock blk = new TextBlock() { Text = itemsFirstArea[i].Item2, Foreground = Brushes.White, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(blk);

                Border brd = new Border() { Child = grd, Height = 40, Margin = new Thickness(0, -1, 1, -1) };
                brd.Classes.Add("ItemBorder");

                if (i == 0)
                {
                    brd.Classes.Add("active");
                }

                this.FindControl<StackPanel>("FirstArea").Children.Add(brd);

                if (itemsFirstArea[i].Item4 != null)
                {
                    this.FindControl<Grid>("ClientArea").Children.Add(itemsFirstArea[i].Item4);

                    if (i != 0)
                    {
                        itemsFirstArea[i].Item4.Opacity = 0;
                        itemsFirstArea[i].Item4.RenderTransform = OffScreen;
                        itemsFirstArea[i].Item4.IsVisible = false;
                    }

                    itemsFirstArea[i].Item4.Transitions = new Transitions() { new DoubleTransition() { Property = Control.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new TransformOperationsTransition() { Property = Control.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
                }
                ClientItems.Add(itemsFirstArea[i].Item4);
                BorderItems.Add(brd);

                int index = i;

                brd.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(grd).Pointer.Capture(grd);

                    this.SelectedIndex = index;
                };
            }


            for (int i = 0; i < itemsSecondArea.Count; i++)
            {
                Grid grd = new Grid();

                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(16, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));

                Control icon = itemsSecondArea[i].Item3;
                Grid.SetColumn(icon, 1);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(icon);

                TextBlock blk = new TextBlock() { Text = itemsSecondArea[i].Item2, Foreground = Brushes.White, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(blk);

                Border brd = new Border() { Child = grd, Height = 40, Margin = new Thickness(0, -1, 1, -1) };
                brd.Classes.Add("ItemBorder");

                this.FindControl<StackPanel>("SecondArea").Children.Add(brd);
                if (itemsSecondArea[i].Item4 != null)
                {
                    this.FindControl<Grid>("ClientArea").Children.Add(itemsSecondArea[i].Item4);
                    itemsSecondArea[i].Item4.Opacity = 0;
                    itemsSecondArea[i].Item4.RenderTransform = OffScreen;
                    itemsSecondArea[i].Item4.IsVisible = false;

                    itemsSecondArea[i].Item4.Transitions = new Transitions() { new DoubleTransition() { Property = Control.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new TransformOperationsTransition() { Property = Control.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
                }
                ClientItems.Add(itemsSecondArea[i].Item4);
                BorderItems.Add(brd);

                int index = i + itemsFirstArea.Count;

                brd.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(grd).Pointer.Capture(grd);

                    this.SelectedIndex = index;
                };
            }

            for (int i = 0; i < itemsThirdArea.Count; i++)
            {
                Grid grd = new Grid();

                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(16, GridUnitType.Pixel));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(20, GridUnitType.Pixel));

                Control icon = itemsThirdArea[i].Item3;
                Grid.SetColumn(icon, 1);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(icon);

                TextBlock blk = new TextBlock() { Text = itemsThirdArea[i].Item2, Foreground = Brushes.White, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                Grid.SetColumn(blk, 2);
                icon.VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center;
                grd.Children.Add(blk);

                Border brd = new Border() { Child = grd, Height = 40, Margin = new Thickness(0, -1, 1, -1) };
                brd.Classes.Add("ItemBorder");

                this.FindControl<StackPanel>("ThirdArea").Children.Add(brd);
                if (itemsThirdArea[i].Item4 != null)
                {
                    this.FindControl<Grid>("ClientArea").Children.Add(itemsThirdArea[i].Item4);

                    itemsThirdArea[i].Item4.Opacity = 0;
                    itemsThirdArea[i].Item4.RenderTransform = OffScreen;
                    itemsThirdArea[i].Item4.IsVisible = false;

                    itemsThirdArea[i].Item4.Transitions = new Transitions() { new DoubleTransition() { Property = Control.OpacityProperty, Duration = TimeSpan.FromMilliseconds(100) }, new TransformOperationsTransition() { Property = Control.RenderTransformProperty, Duration = TimeSpan.FromMilliseconds(100) } };
                }
                ClientItems.Add(itemsThirdArea[i].Item4);
                BorderItems.Add(brd);

                int index = i + itemsFirstArea.Count + itemsSecondArea.Count;

                brd.PointerPressed += (s, e) =>
                {
                    e.GetCurrentPoint(grd).Pointer.Capture(grd);

                    this.SelectedIndex = index;
                };
            }
        }
    }
}
