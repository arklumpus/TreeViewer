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
using CSharpEditor;
using System;
using System.IO;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class CodeViewerWindow : Window
    {
        public CodeViewerWindow()
        {
            this.InitializeComponent();
        }

        public async Task Initialize(string type, string autosavePath)
        {
            string guid = Path.GetFileName(autosavePath);

            string oldestFile = null;
            DateTime oldestTime = DateTime.UnixEpoch;

            foreach (string sr in Directory.GetFiles(autosavePath))
            {
                FileInfo fi = new FileInfo(sr);

                if (fi.LastWriteTime.CompareTo(oldestTime) == 1)
                {
                    oldestTime = fi.LastWriteTime;
                    oldestFile = sr;
                }
            }

            string source = File.ReadAllText(oldestFile);

            string preSource = "";
            string postSource = "";

            if (type == "StringFormatter")
            {
                preSource = "using TreeViewer;\npublic static class FormatterModule { ";
                postSource = "}";
            }
            else if (type == "NumberFormatter")
            {
                preSource = "using TreeViewer;\npublic static class FormatterModule {";
                postSource = "}";
            }
            else if (type == "ColourFormatter")
            {
                preSource = "using TreeViewer;\nusing VectSharp;\nusing System;\nusing System.Collections.Generic;\npublic static class FormatterModule {";
                postSource = "}";
            }

            Editor editor = await Editor.Create(source, preSource, postSource, guid: guid);
            editor.AccessType = Editor.AccessTypes.ReadOnlyWithHistoryAndErrors;

            this.Content = editor;
        }

        public async Task Initialize(string source)
        {
            Editor editor = await Editor.Create(source);
            editor.AccessType = Editor.AccessTypes.ReadOnly;

            this.Content = editor;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
