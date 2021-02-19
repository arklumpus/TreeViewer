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
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;

namespace TreeViewer
{
    public class AddRemoveButton : UserControl
    {
        public enum ButtonTypes { Add, Remove, Up, Down, Edit, OK, Cancel, Download }

        public static readonly StyledProperty<ButtonTypes> ButtonTypeProperty = AvaloniaProperty.Register<AddRemoveButton, ButtonTypes>(nameof(ButtonType), ButtonTypes.Add);

        public ButtonTypes ButtonType
        {
            get { return GetValue(ButtonTypeProperty); }
            set
            {
                SetValue(ButtonTypeProperty, value);
                switch (value)
                {
                    case ButtonTypes.Add:
                        this.FindControl<Path>("Add").IsVisible = true;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Remove:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = true;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Down:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = true;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Up:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = true;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Edit:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = true;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Cancel:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = true;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.OK:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = true;
                        this.FindControl<Path>("Download").IsVisible = false;
                        break;
                    case ButtonTypes.Download:
                        this.FindControl<Path>("Add").IsVisible = false;
                        this.FindControl<Path>("Remove").IsVisible = false;
                        this.FindControl<Path>("Down").IsVisible = false;
                        this.FindControl<Path>("Up").IsVisible = false;
                        this.FindControl<Canvas>("Edit").IsVisible = false;
                        this.FindControl<Path>("Cancel").IsVisible = false;
                        this.FindControl<Path>("OK").IsVisible = false;
                        this.FindControl<Path>("Download").IsVisible = true;
                        break;
                }
            }
        }

        public AddRemoveButton()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }


    }
}
