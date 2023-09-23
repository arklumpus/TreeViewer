/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
    public partial class SpreadsheetTransformTemplateDialog : ChildWindow
    {
        public int Result { get; private set; } = -1;

        public SpreadsheetTransformTemplateDialog()
        {
            InitializeComponent();

            this.FindControl<Grid>("HeaderGrid").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.PasteTransformed")) { Width = 32, Height = 32 });

            this.FindControl<Canvas>("StringToStringIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringToString")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("StringToNumberIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringToNumber")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("NumberToNumberIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumberToNumber")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("NumberToStringIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumberToString")) { Width = 32, Height = 32 });

            this.FindControl<Canvas>("StringsToStringsIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringsToStrings")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("StringsToNumbersIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.StringsToNumbers")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("NumbersToNumbersIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumbersToNumbers")) { Width = 32, Height = 32 });
            this.FindControl<Canvas>("NumbersToStringsIcon").Children.Add(new DPIAwareBox(Icons.GetIcon32("TreeViewer.Assets.NumbersToStrings")) { Width = 32, Height = 32 });

            this.FindControl<Button>("CancelButton").Click += (s, e) =>
            {
                this.Result = -1;
                this.Close();
            };

            this.FindControl<Button>("StringToStringButton").Click += (s, e) =>
            {
                this.Result = 0;
                this.Close();
            };

            this.FindControl<Button>("StringToNumberButton").Click += (s, e) =>
            {
                this.Result = 1;
                this.Close();
            };

            this.FindControl<Button>("NumberToNumberButton").Click += (s, e) =>
            {
                this.Result = 2;
                this.Close();
            };

            this.FindControl<Button>("NumberToStringButton").Click += (s, e) =>
            {
                this.Result = 3;
                this.Close();
            };

            this.FindControl<Button>("StringsToStringsButton").Click += (s, e) =>
            {
                this.Result = 4;
                this.Close();
            };

            this.FindControl<Button>("StringsToNumbersButton").Click += (s, e) =>
            {
                this.Result = 5;
                this.Close();
            };

            this.FindControl<Button>("NumbersToNumbersButton").Click += (s, e) =>
            {
                this.Result = 6;
                this.Close();
            };

            this.FindControl<Button>("NumbersToStringsButton").Click += (s, e) =>
            {
                this.Result = 7;
                this.Close();
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
