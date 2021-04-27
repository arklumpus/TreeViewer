using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class MarkdownEditorWindow : Window
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
