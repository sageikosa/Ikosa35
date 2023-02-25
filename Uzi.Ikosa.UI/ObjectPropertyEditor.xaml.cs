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
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for ObjectPropertyEditor.xaml
    /// </summary>
    public partial class ObjectPropertyEditor : UserControl
    {
        public ObjectPropertyEditor()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ObjectPropertyEditor_DataContextChanged);
        }

        void ObjectPropertyEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _objectBase = _ObjectBase;
            if (_objectBase != null)
            {
                var _sizeOrder = _objectBase.Sizer.NaturalSize.Order + 4;
                try { this.cboSize.SelectedIndex = _sizeOrder; }
                catch { }

                var _omType = _objectBase.ObjectMaterial.GetType();
                foreach (var _item in this.cboMaterial.Items)
                {
                    if (_item.GetType() == _omType)
                    {
                        this.cboMaterial.SelectedItem = _item;
                        break;
                    }
                }
            }
        }

        private ObjectBase _ObjectBase { get { return DataContext as ObjectBase; } }

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            int _out = 0;
            if (!int.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }
            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"No negative points for objects";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region double text field validation
        private void txtDbl_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            double _out = 0;
            if (!double.TryParse(_txt.Text, out _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed for Weight";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        private void cboSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.Sizer.NaturalSize = Size.Medium.OffsetSize(cboSize.SelectedIndex - 4);
        }

        private void cboMaterial_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.ObjectMaterial = cboMaterial.SelectedItem as Material;
        }

    }
}
