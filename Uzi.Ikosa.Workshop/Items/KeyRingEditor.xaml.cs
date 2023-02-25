using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.UI;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for KeyRingEditor.xaml
    /// </summary>
    public partial class KeyRingEditor : UserControl
    {
        public KeyRingEditor()
        {
            InitializeComponent();
            DataContextChanged += new DependencyPropertyChangedEventHandler(_DataContextChanged);
        }

        private KeyRing _KeyRing => (DataContext as KeyRingVM)?.Thing;

        void _DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _kr = _KeyRing;
            if (_kr != null)
            {
                cboSize.SelectedIndex = _kr.ItemSizer.ExpectedCreatureSize.Order + 4;
            }
        }

        #region double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed";
                _txt.Text = 0.ToString();
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_KeyRing != null)
            {
                var _cbItem = cboSize.SelectedItem as ComboBoxItem;
                _KeyRing.ItemSizer.ExpectedCreatureSize = _cbItem.Tag as Size;
            }
        }
    }
}
