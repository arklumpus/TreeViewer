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
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Reflection;
using VectSharp.Markdown;
using VectSharp.MarkdownCanvas;

namespace TreeViewer
{
    public class MacOSPermissionWindow : Window
    {
        public MacOSPermissionWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            string markdownSource;

            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.MacOSPermissionsInstructions.md")))
            {
                markdownSource = reader.ReadToEnd();
            }

            MarkdownRenderer renderer = this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").Renderer;
            renderer.ImageUnitMultiplier *= 3;
            this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").FontSize = 18;
            renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);


            this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").DocumentSource = markdownSource;

            this.FindControl<Button>("IgnoreButton").Click += (s, e) =>
            {
                MainWindow window = new MainWindow();
                window.Show();
                this.Close();
            };

            this.FindControl<Button>("RestartButton").Click += (s, e) =>
            {
                Program.Reboot(new string[0], false);
                ((IClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime).Shutdown(0);
            };

        }
    }
}
