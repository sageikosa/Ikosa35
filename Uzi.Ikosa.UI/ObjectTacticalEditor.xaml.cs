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

namespace Uzi.Ikosa.UI
{
    /// <summary>
    /// Interaction logic for ObjectTacticalEditor.xaml
    /// </summary>
    public partial class ObjectTacticalEditor : UserControl
    {
        public ObjectTacticalEditor()
        {
            InitializeComponent();
            this.DataContextChanged += new DependencyPropertyChangedEventHandler(ObjectTacticalEditor_DataContextChanged);
        }

        void ObjectTacticalEditor_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var _objectBase = _ObjectBase;
            if (_objectBase != null)
            {
                // tactical tab
                this.txtSoundDifficulty.Text = _objectBase.ExtraSoundDifficulty.BaseValue.ToString();
                this.sldrOpacity.Value = _objectBase.Opacity;
                this.cboConcealment.SelectedIndex = _objectBase.DoesSupplyTotalConcealment ? 2 : (_objectBase.DoesSupplyConcealment ? 1 : 0);
                switch (_objectBase.DoesSupplyCover)
                {
                    case Uzi.Ikosa.Tactical.CoverLevel.Soft:
                    case Uzi.Ikosa.Tactical.CoverLevel.Hard:
                        cboCover.SelectedIndex = 1;
                        break;
                    case Uzi.Ikosa.Tactical.CoverLevel.Improved:
                        cboCover.SelectedIndex = 2;
                        break;
                    default:
                        cboCover.SelectedIndex = 0;
                        break;
                }
                this.cboDetectLines.SelectedIndex = _objectBase.DoesBlocksLineOfDetect ? 0 : 1;
                this.cboEffectLines.SelectedIndex = _objectBase.DoesBlocksLineOfEffect ? 0 : 1;
                this.cboMovement.SelectedIndex =
                    (_objectBase.BlocksMove ? 0 : (_objectBase.DoesHindersMove ? 1 : 2));
                this.cboSpread.SelectedIndex = _objectBase.DoesBlocksSpread ? 0 : 1;
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

        #region should probably find a binding/template solution for all this at some point (Uzi.Ikosa.Ui)
        private void cboCover_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cboCover.SelectedIndex > 1)
            {
                _ObjectBase.DoesSupplyCover = Uzi.Ikosa.Tactical.CoverLevel.Improved;
            }
            else if (cboCover.SelectedIndex > 0)
            {
                _ObjectBase.DoesSupplyCover = Uzi.Ikosa.Tactical.CoverLevel.Hard;
            }
            else
            {
                _ObjectBase.DoesSupplyCover = Uzi.Ikosa.Tactical.CoverLevel.None;
            }
        }

        private void cboConcealment_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.DoesSupplyConcealment = cboConcealment.SelectedIndex > 0;
            _ObjectBase.DoesSupplyTotalConcealment = cboConcealment.SelectedIndex > 1;
        }

        private void cboEffectLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.DoesBlocksLineOfEffect = (cboEffectLines.SelectedIndex == 0);
        }

        private void cboDetectLines_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.DoesBlocksLineOfDetect = (cboDetectLines.SelectedIndex == 0);
        }

        private void cboMovement_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.BlocksMove = (cboMovement.SelectedIndex == 0);
            _ObjectBase.DoesHindersMove = (cboMovement.SelectedIndex <= 1);
        }

        private void cboSpread_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ObjectBase.DoesBlocksSpread = (cboSpread.SelectedIndex == 0);
        }

        private void sldrOpacity_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _ObjectBase.Opacity = sldrOpacity.Value;
        }
        #endregion
    }
}
