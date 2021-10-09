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
using System;
using System.Collections.Generic;

namespace TreeViewer
{
    public partial class RibbonGroup : UserControl
    {
        public static readonly StyledProperty<string> GroupNameProperty = AvaloniaProperty.Register<RibbonGroup, string>(nameof(GroupName), "");

        public List<RibbonButton> RibbonButtons { get; } = new List<RibbonButton>();

        public string GroupName
        {
            get { return GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == RibbonGroup.GroupNameProperty)
            {
                this.FindControl<TextBlock>("TextBlock").Text = change.NewValue.GetValueOrDefault<string>();
            }
        }

        public RibbonGroup()
        {
            InitializeComponent(new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
            {

            });
        }

        public RibbonGroup(List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)> buttons)
        {
            InitializeComponent(buttons);
        }

        private void InitializeComponent(List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)> buttons)
        {
            AvaloniaXamlLoader.Load(this);

            Grid currentSmallButtonGrid = GetSmallButtonGrid();

            for (int i = 0; i < buttons.Count; i++)
            {
                int index = i;

                if (buttons[i].Item5)
                {
                    if (currentSmallButtonGrid.Children.Count > 0)
                    {
                        this.FindControl<StackPanel>("ItemsContainer").Children.Add(currentSmallButtonGrid);
                        currentSmallButtonGrid = GetSmallButtonGrid();
                    }

                    LargeRibbonButton ribbonButton = new LargeRibbonButton(buttons[i].Item4) { ButtonText = buttons[i].Item1, Icon = buttons[i].Item2, ShortcutText = buttons[i].Item3 };
                    ribbonButton.ButtonPressed += (s, e) => buttons[index].Item7(e.Index);
                    this.FindControl<StackPanel>("ItemsContainer").Children.Add(ribbonButton);

                    if (!string.IsNullOrEmpty(buttons[i].Item8))
                    {
                        AvaloniaBugFixes.SetToolTip(ribbonButton, buttons[i].Item8);
                    }

                    RibbonButtons.Add(ribbonButton);
                }
                else
                {
                    SmallRibbonButton ribbonButton = new SmallRibbonButton(buttons[i].Item4) { ButtonText = buttons[i].Item1, Icon = buttons[i].Item2, ShortcutText = buttons[i].Item3 };
                    ribbonButton.ButtonPressed += (s, e) => buttons[index].Item7(e.Index);
                    Grid.SetRow(ribbonButton, currentSmallButtonGrid.Children.Count);
                    currentSmallButtonGrid.Children.Add(ribbonButton);

                    if (!string.IsNullOrEmpty(buttons[i].Item8))
                    {
                        AvaloniaBugFixes.SetToolTip(ribbonButton, buttons[i].Item8);
                    }

                    RibbonButtons.Add(ribbonButton);

                    if (currentSmallButtonGrid.Children.Count == 3)
                    {
                        this.FindControl<StackPanel>("ItemsContainer").Children.Add(currentSmallButtonGrid);
                        currentSmallButtonGrid = GetSmallButtonGrid();
                    }
                }
            }

            if (currentSmallButtonGrid.Children.Count > 0)
            {
                this.FindControl<StackPanel>("ItemsContainer").Children.Add(currentSmallButtonGrid);
            }
        }

        private static Grid GetSmallButtonGrid()
        {
            Grid grd = new Grid() { Margin = new Thickness(4, 4, 4, 0) };

            grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
            grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            return grd;
        }
    }
}
