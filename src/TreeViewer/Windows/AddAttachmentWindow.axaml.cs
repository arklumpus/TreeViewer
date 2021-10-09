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

namespace TreeViewer
{
    public class AddAttachmentWindow : ChildWindow
    {
        public bool Result { get; private set; } = false;

        public string AttachmentName { get; private set; } = null;
        public bool LoadInMemory { get; private set; } = false;
        public bool CacheResults { get; private set; } = true;

        public AddAttachmentWindow()
        {
            this.InitializeComponent();

            this.FindControl<Grid>("HeaderGrid").Children.Add(Icons.GetAddAttachmentIcon(1));

            this.FindControl<Button>("OKButton").Click += (s, e) =>
            {
                this.Result = true;
                this.AttachmentName = this.FindControl<TextBox>("NameBox").Text.Replace(";", "_");
                this.LoadInMemory = this.FindControl<CheckBox>("LoadInMemoryBox").IsChecked == true;
                this.CacheResults = this.FindControl<CheckBox>("CacheResultsBox").IsChecked == true;
                this.Close();
            };

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Result = false;
                this.Close();
            };
        }

        public AddAttachmentWindow(string defaultName, bool loadInMemory, bool cacheResults, bool canRename = true) : this()
        {
            this.FindControl<TextBox>("NameBox").Text = defaultName;
            if (!canRename)
            {
                this.FindControl<TextBox>("NameBox").IsEnabled = false;
            }
            this.FindControl<CheckBox>("LoadInMemoryBox").IsChecked = loadInMemory;
            this.FindControl<CheckBox>("CacheResultsBox").IsChecked = cacheResults;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
