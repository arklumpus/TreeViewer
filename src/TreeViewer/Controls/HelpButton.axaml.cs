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
using Avalonia.Markup.Xaml;

namespace TreeViewer
{
    public class HelpButton : UserControl
    {
        public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<HelpButton, bool>(nameof(IsActive), true);

        public bool IsActive
        {
            get { return GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }


        public HelpButton()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == HelpButton.IsActiveProperty)
            {
                if (change.NewValue.GetValueOrDefault<bool>())
                {
                    this.FindControl<Canvas>("buttonCanvas").Classes.Add("Active");
                }
                else
                {
                    this.FindControl<Canvas>("buttonCanvas").Classes.Remove("Active");
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
