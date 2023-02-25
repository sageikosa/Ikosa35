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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ContainerCreate.xaml
    /// </summary>
    public partial class ContainerCreate : UserControl
    {
        public ContainerCreate()
        {
            InitializeComponent();
        }

        public bool CanCreate
        {
            get
            {
                if (cboMaterial.SelectedItem != null)
                {
                    return (new TextBox[]
                    {
                        txtStructure, txtWeight
                    }).All(_txt => _txt.Tag == null);
                }
                return false;
            }
        }

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

            // exclude txtZ, txtY and txtX
            if (_txt.Name.Length > 4)
            {
                if (_out < 1)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Cannot be zero or negative";
                    return;
                }
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region Double text field validation
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

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Cannot be zero or negative";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        public ObjectBase GetContainer(string name)
        {
            var _weight = Convert.ToDouble(txtWeight.Text);
            var _struct = Convert.ToInt32(txtStructure.Text);

            var _container = new ContainerObject(name, cboMaterial.SelectedItem as Material, true, true)
            {
                MaxStructurePoints = _struct,
                MaximumLoadWeight = 100,
                TareWeight = _weight
            };

            if (chkCloseableContainer.IsChecked ?? false)
            {
                _container.TareWeight = 0;

                // inside a closeable container
                var _closeable = new CloseableContainerObject(name, cboMaterial.SelectedItem as Material,
                    _container, true, 1)
                {
                    MaxStructurePoints = _struct,
                    TareWeight = _weight,
                    OpenWeight = _weight / 5
                };
                return _closeable;
            }
            else
            {
                return _container;
            }
        }
    }
}
