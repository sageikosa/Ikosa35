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
    /// Interaction logic for NewBlock.xaml
    /// </summary>
    public partial class NewBlock : Window
    {
        public NewBlock(Room room, CellSpace cellSpace)
        {
            InitializeComponent();
            _Room = room;
            txtZH.Tag = @"Invalid";
            txtYL.Tag = @"Invalid";
            txtXL.Tag = @"Invalid";
            _Space = cellSpace;
            ccParams.Content = ParamPicker.GetParamControl(cellSpace);
        }

        #region private data
        private Room _Room;
        private CellSpace _Space;
        private int _Z = 0;
        private int _Y = 0;
        private int _X = 0;
        private int _ZH = 0;
        private int _YL = 0;
        private int _XL = 0;
        #endregion

        public uint ParamData { get { return ParamPicker.ParamData(ccParams); } }

        public void DrawBlock()
        {
            var _param = ParamData;
            for (var _zc = _Z; _zc < (_Z + _ZH); _zc++)
                for (var _yc = _Y; _yc < (_Y + _YL); _yc++)
                    for (var _xc = _X; _xc < (_X + _XL); _xc++)
                        _Room.SetCellStructure(_zc, _yc, _xc,
                            new CellStructure(_Space, _param));
            _Room.ReLink(true);
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
                _txt.ToolTip = @"Negatives not allowed for room positions or extents";
                return;
            }

            if (_txt.Name.Equals(@"txtZ", StringComparison.OrdinalIgnoreCase))
            {
                if (_out >= _Room.ZHeight)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Z must be inside room";
                    return;
                }
                if ((_out + _ZH) > _Room.ZHeight)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Z and ZHeight cannot extend outside room";
                    return;
                }
                _Z = _out;
            }

            if (_txt.Name.Equals(@"txtY", StringComparison.OrdinalIgnoreCase))
            {
                if (_out >= _Room.YLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Y must be inside room";
                    return;
                }
                if ((_out + _YL) > _Room.YLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Y and YLength cannot extend outside room";
                    return;
                }
                _Y = _out;
            }

            if (_txt.Name.Equals(@"txtX", StringComparison.OrdinalIgnoreCase))
            {
                if ((_out) >= _Room.XLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"X must be inside room";
                    return;
                }
                if ((_out + _XL) > _Room.XLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"X and XLength cannot extend outside room";
                    return;
                }
                _X = _out;
            }

            if (_txt.Name.Equals(@"txtZH", StringComparison.OrdinalIgnoreCase))
            {
                if (_out == 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Block cannot have a 0 dimension";
                    return;
                }
                if ((_out + _Z - 1) > _Room.ZHeight)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Z and ZHeight cannot extend outside room";
                    return;
                }
                _ZH = _out;
            }

            if (_txt.Name.Equals(@"txtYL", StringComparison.OrdinalIgnoreCase))
            {
                if (_out == 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Block cannot have a 0 dimension";
                    return;
                }
                if ((_out + _Y - 1) > _Room.YLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Y and YLength cannot extend outside room";
                    return;
                }
                _YL = _out;
            }

            if (_txt.Name.Equals(@"txtXL", StringComparison.OrdinalIgnoreCase))
            {
                if (_out == 0)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"Block cannot have a 0 dimension";
                    return;
                }
                if ((_out + _X - 1) > _Room.XLength)
                {
                    _txt.Tag = @"Invalid";
                    _txt.ToolTip = @"X and XLength cannot extend outside room";
                    return;
                }
                _XL = _out;
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

        private void cmdbndOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            // values
            if (((txtZ.Tag != null) && txtZ.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtY.Tag != null) && txtY.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtX.Tag != null) && txtX.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtZH.Tag != null) && txtZH.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtYL.Tag != null) && txtYL.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)) ||
                ((txtXL.Tag != null) && txtXL.Tag.ToString().Equals(@"Invalid", StringComparison.OrdinalIgnoreCase)))
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
    }
}
