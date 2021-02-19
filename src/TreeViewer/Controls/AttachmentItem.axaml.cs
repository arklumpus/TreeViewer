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
using System;
using System.Collections.Generic;
using Avalonia.VisualTree;
using System.IO;

namespace TreeViewer
{
    public class AttachmentItem : UserControl
    {
        public event EventHandler ItemDeleted;

        private string attachmentName;

        public AttachmentItem()
        {
            this.InitializeComponent();
        }

        public AttachmentItem(string attachmentName)
        {
            this.InitializeComponent();
            this.attachmentName = attachmentName;
            this.FindControl<TrimmedTextBox>("AttachmentNameBox").Text = attachmentName;
            ToolTip.SetTip(this, attachmentName);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            this.PointerPressed += (s, e) =>
            {
                ((ContextMenu)this.Resources["ContextMenu"]).Open(this);
            };
        }

        private void DeleteClicked(object sender, RoutedEventArgs e)
        {
            MainWindow window = this.FindAncestorOfType<MainWindow>();
            Attachment att = window.StateData.Attachments[attachmentName];
            window.StateData.Attachments.Remove(this.attachmentName);
            att.Dispose();

            ((Panel)this.Parent).Children.Remove(this);

            ItemDeleted?.Invoke(this, new EventArgs());
        }

        private async void ExportClicked(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Extensions = new List<string>() { "*" }, Name = "All files" } }, Title = "Export attachment" };

            MainWindow window = this.FindAncestorOfType<MainWindow>();

            string result = await dialog.ShowAsync(window);

            if (!string.IsNullOrEmpty(result))
            {
                using (FileStream fs = new FileStream(result, FileMode.Create))
                {
                    window.StateData.Attachments[this.attachmentName].WriteToStream(fs);
                }
            }
        }
    }
}
