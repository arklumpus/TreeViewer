using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeViewer
{
    public class PreviewComboBox : UserControl
    {
        public static readonly DirectProperty<PreviewComboBox, int> SelectedIndexProperty = ComboBox.SelectedIndexProperty.AddOwner<PreviewComboBox>(x => x.SelectedIndex, (o, x) => o.SelectedIndex = x);

        private int selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }

            set
            {
                selectedIndex = value;
                if (ContainedBox != null)
                {
                    ContainedBox.SelectedIndex = value;
                }
            }
        }

        private ComboBox ContainedBox;

        public System.Collections.IEnumerable Items
        {
            get
            {
                return ContainedBox.Items;
            }

            set
            {
                ContainedBox.Items = value;
            }
        }

        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
        public event EventHandler<SelectionChangedEventArgs> PreviewSelectionChanged;

        public PreviewComboBox()
        {
            ContainedBox = new ComboBox() { HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };

            this.Content = ContainedBox;

            ContainedBox.SelectionChanged += (s, e) =>
            {
                this.PreviewSelectionChanged?.Invoke(this, e);
                this.SelectedIndex = ContainedBox.SelectedIndex;
                this.SelectionChanged?.Invoke(this, e);
            };
        }



    }
}
