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
using System.Collections.Generic;
using System;

namespace TreeViewer
{
    public partial class RibbonTabContent : UserControl
    {
        public List<List<RibbonButton>> RibbonButtons { get; } = new List<List<RibbonButton>>();
        public List<RibbonGroup> RibbonGroups { get; } = new List<RibbonGroup>();

        public RibbonTabContent()
        {
            InitializeComponent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
               
            });
        }

        public RibbonTabContent(List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> structure)
        {
            InitializeComponent(structure);
        }

        private void InitializeComponent(List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)> structure)
        {
            AvaloniaXamlLoader.Load(this);

            for (int i = 0; i < structure.Count; i++)
            {
                RibbonGroup group = new RibbonGroup(structure[i].Item2) { GroupName = structure[i].Item1 };

                RibbonButtons.Add(group.RibbonButtons);
                RibbonGroups.Add(group);

                this.FindControl<StackPanel>("MainContainer").Children.Add(group);
            }
        }
    }
}
