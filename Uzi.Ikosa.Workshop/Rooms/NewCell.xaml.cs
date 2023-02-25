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
    /// Interaction logic for NewCell.xaml
    /// </summary>
    public partial class NewCell : Window
    {
        public NewCell(Room room, CellSpace cellSpace)
        {
            InitializeComponent();
            _Room = room;
            _Space = cellSpace;
            ccParams.Content = ParamPicker.GetParamControl(cellSpace);
        }

        #region private data
        private Room _Room;
        private CellSpace _Space;
        private int _Z = 0;
        private int _Y = 0;
        private int _X = 0;
        #endregion

        public uint ParamData { get { return ParamPicker.ParamData(ccParams); } }

        public void DrawCell()
        {
            var _param = ParamData;
            _Room[_Z, _Y, _X] = new CellStructure(_Space, _param);
        }

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

            if (_out < 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed for room positions";
                return;
            }

            if (_txt.Name.Equals(@"txtZ", StringComparison.OrdinalIgnoreCase))
            {
                if (_out >= _Room.ZHeight)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Z cannot be larger than room's height";
                    return;
                }
                _Z = _out;
            }

            if (_txt.Name.Equals(@"txtY", StringComparison.OrdinalIgnoreCase))
            {
                if (_out >= _Room.YLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Y cannot be larger than room's Y length";
                    return;
                }
                _Y = _out;
            }

            if (_txt.Name.Equals(@"txtX", StringComparison.OrdinalIgnoreCase))
            {
                if (_out >= _Room.XLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"X cannot be larger than room's X length";
                    return;
                }
                _X = _out;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

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
                ((txtX.Tag != null) && txtX.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)))
            {
                txtMessage.Text = @"One or more invalid values";
                e.CanExecute = false;
                e.Handled = true;
                return;
            }

            txtMessage.Text = string.Empty;
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
    }
}
