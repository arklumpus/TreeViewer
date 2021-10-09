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
using Avalonia.Markup.Xaml;
using Markdig;
using System.Collections.Generic;
using VectSharp.Markdown;
using VectSharp.MarkdownCanvas;

namespace TreeViewer
{
    public class HelpWindow : ChildWindow
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        public HelpWindow(string markdownSource, string moduleId)
        {
            InitializeComponent();

            Markdig.Syntax.MarkdownDocument markdownDocument = Markdig.Markdown.Parse(markdownSource, new Markdig.MarkdownPipelineBuilder().UseGridTables().UsePipeTables().UseEmphasisExtras().UseGenericAttributes().UseAutoIdentifiers().UseAutoLinks().UseTaskLists().UseListExtras().UseCitations().UseMathematics().Build());

            MarkdownRenderer renderer = this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").Renderer;
            renderer.ImageUnitMultiplier /= 1.4;
            renderer.ImageMultiplier *= 1.4;
            renderer.HeaderFontSizeMultipliers[0] *= 1.3;
            this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").FontSize = 14;

            renderer.RasterImageLoader = image => new VectSharp.MuPDFUtils.RasterImageFile(image);

            int imageIndex = 0;

            Dictionary<string, string> imageLookup = new Dictionary<string, string>();

            renderer.ImageUriResolver = (imageUri, baseUri) =>
            {
                string[] compatibleFiles = System.IO.Directory.GetFiles(System.IO.Path.Combine(Modules.ModulePath, "assets"), moduleId + "_" + "image" + imageIndex + ".*");

                if (compatibleFiles.Length > 0)
                {
                    string imagePath = compatibleFiles[0];

                    imageLookup[baseUri + "|||" + imageUri] = imagePath;
                    imageIndex++;
                    return (imagePath, false);
                }
                else
                {
                    return (null, false);
                }
            };

            renderer.RenderSinglePage(markdownDocument, 800, out _);

            renderer.ImageUriResolver = (imageUri, baseUri) =>
            {
                return (imageLookup.TryGetValue(baseUri + "|||" + imageUri, out string imagePath) ? imagePath : null, false);
            };

            this.FindControl<MarkdownCanvasControl>("MarkdownCanvas").Document = markdownDocument;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
