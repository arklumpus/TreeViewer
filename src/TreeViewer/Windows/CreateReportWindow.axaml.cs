using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using PhyloTree;
using System.Collections.Generic;
using System.Linq;

namespace TreeViewer
{
    public partial class CreateReportWindow : ChildWindow
    {
        public CreateReportWindow()
        {
            InitializeComponent();
        }

        private MainWindow ParentWindow;
        public List<IList<TreeNode>> Trees { get; } = new List<IList<TreeNode>>();
        public bool Result { get; private set; } = false;

        public CreateReportWindow(MainWindow parent) : this()
        {
            this.ParentWindow = parent;
            this.FindControl<NumericUpDown>("LoadedTreeIndexNUD").Maximum = parent.Trees.Count;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NewReport")) { Width = 32, Height = 32 });
        }

        private void AddFinalTreeClicked(object sender, RoutedEventArgs e)
        {
            Trees.Add(new List<TreeNode>() { ParentWindow.TransformedTree });

            Grid grd = new Grid();
            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            grd.Children.Add(new TextBlock() { Text = "Final transformed tree", Margin = new Thickness(5) });

            Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
            deleteButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteButton, 1);
            grd.Children.Add(deleteButton);

            this.FindControl<StackPanel>("TreeContainer").Children.Add(grd);

            int index = Trees.Count - 1;

            deleteButton.Click += (s, e) =>
            {
                Trees.RemoveAt(index);
                this.FindControl<StackPanel>("TreeContainer").Children.Remove(grd);
            };
        }

        private void AddAllLoadedTreesClicked(object sender, RoutedEventArgs e)
        {
            Trees.Add(ParentWindow.Trees);

            Grid grd = new Grid();
            grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

            grd.Children.Add(new TextBlock() { Text = ParentWindow.Trees.Count.ToString() + " loaded trees", Margin = new Thickness(5) });

            Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
            deleteButton.Classes.Add("SideBarButton");
            Grid.SetColumn(deleteButton, 1);
            grd.Children.Add(deleteButton);

            this.FindControl<StackPanel>("TreeContainer").Children.Add(grd);

            int index = Trees.Count - 1;

            deleteButton.Click += (s, e) =>
            {
                Trees.RemoveAt(index);
                this.FindControl<StackPanel>("TreeContainer").Children.Remove(grd);
            };
        }

        private void AddLoadedTreeClicked(object sender, RoutedEventArgs e)
        {
            if (e.Source == this.FindControl<Button>("AddLoadedTree"))
            {
                int treeIndex = (int)this.FindControl<NumericUpDown>("LoadedTreeIndexNUD").Value - 1;

                Trees.Add(new List<TreeNode>() { ParentWindow.Trees[treeIndex] });

                Grid grd = new Grid();
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                grd.Children.Add(new TextBlock() { Text = "Loaded tree #" + (treeIndex + 1).ToString(), Margin = new Thickness(5) });

                Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
                deleteButton.Classes.Add("SideBarButton");
                Grid.SetColumn(deleteButton, 1);
                grd.Children.Add(deleteButton);

                this.FindControl<StackPanel>("TreeContainer").Children.Add(grd);

                int index = Trees.Count - 1;

                deleteButton.Click += (s, e) =>
                {
                    Trees.RemoveAt(index);
                    this.FindControl<StackPanel>("TreeContainer").Children.Remove(grd);
                };
            }
        }

        private async void AddTreeFileClicked(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog;

            List<FileDialogFilter> filters = new List<FileDialogFilter>();

            List<string> allExtensions = new List<string>();

            for (int i = 0; i < Modules.FileTypeModules.Count; i++)
            {
                filters.Add(new FileDialogFilter() { Name = Modules.FileTypeModules[i].Extensions[0], Extensions = new List<string>(Modules.FileTypeModules[i].Extensions.Skip(1)) });
                allExtensions.AddRange(Modules.FileTypeModules[i].Extensions.Skip(1));
            }

            filters.Insert(0, new FileDialogFilter() { Name = "All tree files", Extensions = allExtensions });
            filters.Add(new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } });

            if (!Modules.IsMac)
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false,
                    Filters = filters
                };
            }
            else
            {
                dialog = new OpenFileDialog()
                {
                    Title = "Open tree file",
                    AllowMultiple = false
                };
            }

            string[] result = await dialog.ShowAsync(this);

            if (result != null && result.Length == 1)
            {
                TreeCollection loadedTrees = await Modules.OpenTreeFile(result[0], this);

                if (loadedTrees != null && loadedTrees.Count > 0)
                {
                    Trees.Add(loadedTrees);

                    Grid grd = new Grid();
                    grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                    grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));

                    grd.Children.Add(new TextBlock() { Text = System.IO.Path.GetFileName(result[0]) + " (" + loadedTrees.Count + " trees)", Margin = new Thickness(5) });

                    Button deleteButton = new Button() { Width = 20, Height = 20, Background = Avalonia.Media.Brushes.Transparent, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, Data = Icons.CrossGeometry, StrokeThickness = 2 }, Padding = new Avalonia.Thickness(2) };
                    deleteButton.Classes.Add("SideBarButton");
                    Grid.SetColumn(deleteButton, 1);
                    grd.Children.Add(deleteButton);

                    this.FindControl<StackPanel>("TreeContainer").Children.Add(grd);

                    int index = Trees.Count - 1;

                    deleteButton.Click += (s, e) =>
                    {
                        Trees.RemoveAt(index);
                        this.FindControl<StackPanel>("TreeContainer").Children.Remove(grd);
                    };
                }
            }
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void OKClicked(object sender, RoutedEventArgs e)
        {
            int totalCount = 0;

            if (Trees != null)
            {
                for (int i = 0; i < Trees.Count; i++)
                {
                    if (Trees[i] != null)
                    {
                        totalCount += Trees[i].Count;
                    }
                }
            }

            if (totalCount == 0)
            {
                await new MessageBox("Attention!", "You need to select at least one tree!").ShowDialog2(this);
            }
            else
            {
                this.Result = true;
                this.Close();
            }
        }
    }
}
