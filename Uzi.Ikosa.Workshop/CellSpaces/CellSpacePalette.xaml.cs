using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CellSpacePalette.xaml
    /// </summary>
    public partial class CellSpacePalette : UserControl
    {
        public static RoutedCommand SelectCell = new RoutedCommand();
        public static RoutedCommand CaptureCell = new RoutedCommand();

        #region construction
        public CellSpacePalette(LocalMap map, IInputElement drawTarget, ICellStructureProvider provider)
        {
            InitializeComponent();
            LoadMap(map);
            _Provider = provider;

            // draw targets
            btnCell1.CommandTarget = drawTarget;
            btnCell2.CommandTarget = drawTarget;
            btnCell3.CommandTarget = drawTarget;
            btnCell4.CommandTarget = drawTarget;
            btnCell5.CommandTarget = drawTarget;
            btnCell6.CommandTarget = drawTarget;
            btnCell7.CommandTarget = drawTarget;
            btnCell8.CommandTarget = drawTarget;
            btnCell9.CommandTarget = drawTarget;
            btnCell0.CommandTarget = drawTarget;

            // command parameters
            btnCell1.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace1, ccParams1);
            btnCell2.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace2, ccParams2);
            btnCell3.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace3, ccParams3);
            btnCell4.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace4, ccParams4);
            btnCell5.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace5, ccParams5);
            btnCell6.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace6, ccParams6);
            btnCell7.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace7, ccParams7);
            btnCell8.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace8, ccParams8);
            btnCell9.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace9, ccParams9);
            btnCell0.CommandParameter = new Tuple<ContentControl, ContentControl>(ccCellSpace0, ccParams0);
        }
        #endregion

        ICellStructureProvider _Provider;

        #region public object CommandParameter(int slot, bool keepData)
        public object CommandParameter(int slot, bool keepData)
        {
            switch (slot)
            {
                case 1: return new Tuple<ContentControl, ContentControl>(ccCellSpace1, keepData ? null : ccParams1);
                case 2: return new Tuple<ContentControl, ContentControl>(ccCellSpace2, keepData ? null : ccParams2);
                case 3: return new Tuple<ContentControl, ContentControl>(ccCellSpace3, keepData ? null : ccParams3);
                case 4: return new Tuple<ContentControl, ContentControl>(ccCellSpace4, keepData ? null : ccParams4);
                case 5: return new Tuple<ContentControl, ContentControl>(ccCellSpace5, keepData ? null : ccParams5);
                case 6: return new Tuple<ContentControl, ContentControl>(ccCellSpace6, keepData ? null : ccParams6);
                case 7: return new Tuple<ContentControl, ContentControl>(ccCellSpace7, keepData ? null : ccParams7);
                case 8: return new Tuple<ContentControl, ContentControl>(ccCellSpace8, keepData ? null : ccParams8);
                case 9: return new Tuple<ContentControl, ContentControl>(ccCellSpace9, keepData ? null : ccParams9);
                default: return new Tuple<ContentControl, ContentControl>(ccCellSpace0, keepData ? null : ccParams0);
            }
        }
        #endregion

        #region public void LoadMap(LocalMap map)
        public void LoadMap(LocalMap map)
        {
            // menu items
            foreach (var _mnu in new[]
            {
                mnuCell1, mnuCell2, mnuCell3, mnuCell4, mnuCell5,
                mnuCell6, mnuCell7, mnuCell8, mnuCell9, mnuCell0
            })
            {
                _mnu.Items.Clear();
                _mnu.Items.Add(new MenuItem
                {
                    Header = @"Capture",
                    Command = CaptureCell,
                    CommandParameter = _mnu.Tag.ToString()
                });
                _mnu.Items.Add(new Separator());
                foreach (var _cSpace in map.CellSpaces)
                {
                    _mnu.Items.Add(new MenuItem
                    {
                        Header = _cSpace,
                        Command = SelectCell,
                        CommandParameter = new Tuple<string, CellSpace>(_mnu.Tag.ToString(), _cSpace)
                    });
                }
            }
        }
        #endregion

        #region select cell
        private void cbSelectCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbSelectCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _param = e.Parameter as Tuple<string, CellSpace>;
            ContentControl _ccCellSpace = null;
            ContentControl _ccParams = null;
            switch (_param.Item1)
            {
                case @"1":
                    _ccCellSpace = ccCellSpace1;
                    _ccParams = ccParams1;
                    break;
                case @"2":
                    _ccCellSpace = ccCellSpace2;
                    _ccParams = ccParams2;
                    break;
                case @"3":
                    _ccCellSpace = ccCellSpace3;
                    _ccParams = ccParams3;
                    break;
                case @"4":
                    _ccCellSpace = ccCellSpace4;
                    _ccParams = ccParams4;
                    break;
                case @"5":
                    _ccCellSpace = ccCellSpace5;
                    _ccParams = ccParams5;
                    break;
                case @"6":
                    _ccCellSpace = ccCellSpace6;
                    _ccParams = ccParams6;
                    break;
                case @"7":
                    _ccCellSpace = ccCellSpace7;
                    _ccParams = ccParams7;
                    break;
                case @"8":
                    _ccCellSpace = ccCellSpace8;
                    _ccParams = ccParams8;
                    break;
                case @"9":
                    _ccCellSpace = ccCellSpace9;
                    _ccParams = ccParams9;
                    break;
                default:
                    _ccCellSpace = ccCellSpace0;
                    _ccParams = ccParams0;
                    break;
            }
            _ccCellSpace.Content = _param.Item2;
            _ccParams.Content = ParamPicker.GetParamControl(_param.Item2 as CellSpace);
            e.Handled = true;
        }
        #endregion

        #region capture cell
        private void cbCaptureCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = _Provider.CanCaptureCellStructure();
            e.Handled = true;
        }

        private void cbCaptureCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ContentControl _ccCellSpace = null;
            ContentControl _ccParams = null;
            switch (e.Parameter.ToString())
            {
                case @"1":
                    _ccCellSpace = ccCellSpace1;
                    _ccParams = ccParams1;
                    break;
                case @"2":
                    _ccCellSpace = ccCellSpace2;
                    _ccParams = ccParams2;
                    break;
                case @"3":
                    _ccCellSpace = ccCellSpace3;
                    _ccParams = ccParams3;
                    break;
                case @"4":
                    _ccCellSpace = ccCellSpace4;
                    _ccParams = ccParams4;
                    break;
                case @"5":
                    _ccCellSpace = ccCellSpace5;
                    _ccParams = ccParams5;
                    break;
                case @"6":
                    _ccCellSpace = ccCellSpace6;
                    _ccParams = ccParams6;
                    break;
                case @"7":
                    _ccCellSpace = ccCellSpace7;
                    _ccParams = ccParams7;
                    break;
                case @"8":
                    _ccCellSpace = ccCellSpace8;
                    _ccParams = ccParams8;
                    break;
                case @"9":
                    _ccCellSpace = ccCellSpace9;
                    _ccParams = ccParams9;
                    break;
                default:
                    _ccCellSpace = ccCellSpace0;
                    _ccParams = ccParams0;
                    break;
            }
            ref readonly var _struc = ref _Provider.GetCellStructure();
            _ccCellSpace.Content = _struc.CellSpace;
            _ccParams.Content = ParamPicker.GetParamControl(_struc.CellSpace, _struc.ParamData);
            e.Handled = true;
        }
        #endregion
    }
}
