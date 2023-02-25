using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for ItemPropertyEditor.xaml
    /// </summary>
    public partial class ItemPropertyEditor : UserControl
    {
        public ItemPropertyEditor()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ItemPropertyEditor_DataContextChanged);
        }

        private ItemBase _Item { get { return DataContext as ItemBase; } }

        void ItemPropertyEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _item = _Item;
            if (_item != null)
            {
                var _sizeOrder = _item.ItemSizer.ExpectedCreatureSize.Order + 4;
                try { this.cboSize.SelectedIndex = _sizeOrder; }
                catch { }
            }
        }

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_Item != null)
            {
                _Item.ItemSizer.ExpectedCreatureSize = Size.Medium.OffsetSize(cboSize.SelectedIndex - 4);
            }
        }

    }
}
