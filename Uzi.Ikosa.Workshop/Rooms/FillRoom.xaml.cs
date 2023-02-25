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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for FillRoom.xaml
    /// </summary>
    public partial class FillRoom : Window
    {
        public FillRoom(Room room, CellSpace cellSpace)
        {
            InitializeComponent();
            _Room = room;
            _Space = cellSpace;
            ccParams.Content = ParamPicker.GetParamControl(cellSpace);
        }

        private Room _Room;
        private CellSpace _Space;

        public uint ParamData { get { return ParamPicker.ParamData(ccParams); } }

        public bool IsFillEntire { get { return chkEntireRoom.IsChecked ?? false; } }

        public void DrawFill()
        {
            var _param = ParamData;
            var _entire = IsFillEntire;
            for (var _zc = 0; _zc < _Room.ZHeight; _zc++)
                for (var _yc = 0; _yc < _Room.YLength; _yc++)
                    for (var _xc = 0; _xc < _Room.XLength; _xc++)
                        if (IsFillEntire || (_Room[_zc, _yc, _xc].CellSpace == null))
                            _Room.SetCellStructure(_zc, _yc, _xc, new CellStructure(_Space,_param));

            // one relink when done instead of inside the loop
            _Room.ReLink(true);
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
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion
    }
}
