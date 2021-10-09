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
using Avalonia.Controls.Presenters;
using Avalonia.Markup.Xaml;

namespace TreeViewer
{
    public partial class RibbonFilePageContentTemplate : UserControl
    {
        public static readonly StyledProperty<Control> PageContentProperty = AvaloniaProperty.Register<RibbonFilePageContentTemplate, Control>(nameof(PageContent), null);

        public Control PageContent
        {
            get { return GetValue(PageContentProperty); }
            set { SetValue(PageContentProperty, value); }
        }


        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RibbonFilePageContentTemplate.PageContentProperty)
            {
                this.FindControl<ContentPresenter>("ContentContainer").Content = change.NewValue.GetValueOrDefault<Control>();
            }
        }

        public RibbonFilePageContentTemplate()
        {
            InitializeComponent(null);
        }

        public RibbonFilePageContentTemplate(string title)
        {
            InitializeComponent(title);
        }

        private void InitializeComponent(string title)
        {
            AvaloniaXamlLoader.Load(this);

            this.FindControl<TextBlock>("TitleContainer").Text = title;
        }
    }
}
