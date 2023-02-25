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
using System.Windows.Shapes;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewLine.xaml
    /// </summary>
    public partial class NewLine : Window
    {
        public NewLine(Room room, CellSpace cellSpace)
        {
            InitializeComponent();
            _Room = room;
            txtSteps.Tag = @"Invalid";
            _Space = cellSpace;
            ccParams.Content = ParamPicker.GetParamControl(cellSpace);
        }

        #region private data
        private Room _Room;
        private CellSpace _Space;
        private int _Z = 0;
        private int _Y = 0;
        private int _X = 0;
        private int _ZStep = 0;
        private int _YStep = 0;
        private int _XStep = 0;
        private int _Steps = 0;
        #endregion

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!int.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_txt.Name.Equals(@"txtZ", StringComparison.OrdinalIgnoreCase))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Negatives not allowed for room positions";
                    return;
                }

                if (_out >= _Room.ZHeight)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Z cannot be outside room";
                    return;
                }
                _Z = _out;
            }

            if (_txt.Name.Equals(@"txtY", StringComparison.OrdinalIgnoreCase))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Negatives not allowed for room positions";
                    return;
                }

                if (_out >= _Room.YLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Y cannot be outside room";
                    return;
                }
                _Y = _out;
            }

            if (_txt.Name.Equals(@"txtX", StringComparison.OrdinalIgnoreCase))
            {
                if (_out < 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Negatives not allowed for room positions";
                    return;
                }

                if (_out >= _Room.XLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"X cannot be outside room";
                    return;
                }
                _X = _out;
            }

            if (_txt.Name.Equals(@"txtSteps", StringComparison.OrdinalIgnoreCase))
            {
                if (_out < 1)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Only positives allowed for step count";
                    return;
                }
                _Steps = _out;
            }

            if (_txt.Name.Equals(@"txtZStep", StringComparison.OrdinalIgnoreCase))
            {
                _ZStep = _out;
            }

            if (_txt.Name.Equals(@"txtYStep", StringComparison.OrdinalIgnoreCase))
            {
                _YStep = _out;
            }

            if (_txt.Name.Equals(@"txtXStep", StringComparison.OrdinalIgnoreCase))
            {
                _XStep = _out;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        public uint ParamData { get { return ParamPicker.ParamData(ccParams); } }

        public void DrawLine()
        {
            var _param = ParamData;
            var _z = _Z;
            var _y = _Y;
            var _x = _X;
            for (var _ax = 0; _ax < _Steps; _ax++)
            {
                _Room[_z, _y, _x] = new CellStructure(_Space, _param);
                _z += _ZStep;
                _y += _YStep;
                _x += _XStep;
            }
        }

        private void cmdbndOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // parse values and keep them at the ready...
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #region private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            if (((txtZ.Tag != null) && txtZ.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtY.Tag != null) && txtY.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtX.Tag != null) && txtX.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtZStep.Tag != null) && txtZStep.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtYStep.Tag != null) && txtYStep.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtXStep.Tag != null) && txtXStep.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtSteps.Tag != null) && txtSteps.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)))
            {
                txtMessage.Text = @"One or more invalid values";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            if (_Room != null)
            {
                var _endZ = _Z + (_ZStep * (_Steps - 1));
                if ((_endZ < 0) || (_endZ >= _Room.ZHeight))
                {
                    txtMessage.Text = @"Z steps outside of room";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                var _endY = _Y + (_YStep * (_Steps - 1));
                if ((_endY < 0) || (_endY >= _Room.YLength))
                {
                    txtMessage.Text = @"Y steps outside of room";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }

                var _endX = _X + (_XStep * (_Steps - 1));
                if ((_endX < 0) || (_endX >= _Room.XLength))
                {
                    txtMessage.Text = @"X steps outside of room";
                    e.CanExecute = false;
                    e.Handled = true;
                    return;
                }
            }
            else
            {
                txtMessage.Text = @"Unloaded";
                e.CanExecute = false;
                e.Handled = true;
            }

            txtMessage.Text = string.Empty;
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
    }
}
