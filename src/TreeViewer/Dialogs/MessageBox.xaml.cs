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
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System;
using System.IO;
using System.Reflection;

namespace TreeViewer
{
    public class MessageBox : ChildWindow
    {
        public MessageBox()
        {
            this.InitializeComponent();
        }

        public enum MessageBoxButtonTypes
        {
            OK,
            YesNo
        }

        public enum Results
        {
            Yes,
            No,
            OK
        }

        public enum MessageBoxIconTypes
        {
            Warning, Tick, QuestionMark
        }

        public Results Result = Results.No;


        public MessageBox(string title, string text, MessageBoxButtonTypes type = MessageBoxButtonTypes.OK, MessageBoxIconTypes iconType = MessageBoxIconTypes.Warning)
        {
            this.InitializeComponent();

            this.Title = title;

            using (StringReader reader = new StringReader(text))
            {
                string line = reader.ReadLine();

                bool isFirst = true;

                while (line != null)
                {
                    TextBlock message = new TextBlock() { TextWrapping = Avalonia.Media.TextWrapping.Wrap, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Text = line };

                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        message.Margin = new Thickness(0, 10, 0, 0);
                    }

                    this.FindControl<StackPanel>("MessageContainer").Children.Add(message);

                    line = reader.ReadLine();
                }
            }

            if (type == MessageBoxButtonTypes.OK)
            {
                this.FindControl<Button>("OKButton").IsVisible = true;
            }
            else
            {
                this.FindControl<Grid>("YesNoButtons").IsVisible = true;
            }

            if (iconType == MessageBoxIconTypes.Warning)
            {
                this.FindControl<Canvas>("WarningCanvas").IsVisible = true;
                this.Icon = new WindowIcon(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.Warning.png"));
            }
            else if (iconType == MessageBoxIconTypes.Tick)
            {
                this.FindControl<Canvas>("TickCanvas").IsVisible = true;
                this.Icon = new WindowIcon(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.Tick.png"));
            }
            else if (iconType == MessageBoxIconTypes.QuestionMark)
            {
                this.FindControl<Canvas>("QuestionMarkCanvas").IsVisible = true;
                this.Icon = new WindowIcon(Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.QuestionMark.png"));
            }
        }

        private void MessageBoxOpened(object sender, EventArgs e)
        {
            if (this.Bounds.Height > 500)
            {
                this.SizeToContent = SizeToContent.Manual;
                this.Height = 500;
                this.FindControl<ScrollViewer>("MessageScrollViewer").IsVisible = false;
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.FindControl<ScrollViewer>("MessageScrollViewer").IsVisible = true;
                }, Avalonia.Threading.DispatcherPriority.MinValue);
            }

            this.MaxHeight = 500;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OKClicked(object sender, RoutedEventArgs e)
        {
            this.Result = Results.OK;
            this.Close();
        }

        private void YesClicked(object sender, RoutedEventArgs e)
        {
            this.Result = Results.Yes;
            this.Close();
        }

        private void NoClicked(object sender, RoutedEventArgs e)
        {
            this.Result = Results.No;
            this.Close();
        }
    }
}
