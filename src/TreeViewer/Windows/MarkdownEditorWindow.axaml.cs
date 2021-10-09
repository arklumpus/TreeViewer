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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class MarkdownEditorWindow : ChildWindow
    {
        public MarkdownEditorWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private MDEdit.Editor Editor;

        public string Result { get; private set; } = null;

        public async Task FinishInitialization(string sourceCode, string editorId, InstanceStateData stateData)
        {
            Editor = await MDEdit.Editor.Create(sourceCode.Replace("\t", "    "), guid: editorId);
            Editor.Background = this.Background;
            Editor.MarkdownRenderer.ImageUriResolver = (a, b) => MarkdownUtils.ImageUriResolverAsynchronous(a, b, stateData);
            Editor.MarkdownRenderer.RasterImageLoader = imageFile => new VectSharp.MuPDFUtils.RasterImageFile(imageFile);

            this.FindControl<Grid>("MainGrid").Children.Add(Editor);
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            Result = Editor.Text;
            this.Close();
        }

        private void CancelClicked(object sender, RoutedEventArgs e)
        {
            Result = null;
            this.Close();
        }
    }
}
