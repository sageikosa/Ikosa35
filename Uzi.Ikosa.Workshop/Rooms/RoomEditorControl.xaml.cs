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
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using System.Windows.Media.Media3D;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for RoomEditorControl.xaml
    /// </summary>
    public partial class RoomEditorControl : UserControl
    {
        public static readonly RoutedCommand Clone = new RoutedCommand();
        public static readonly RoutedCommand NewCell = new RoutedCommand();
        public static readonly RoutedCommand NewBlock = new RoutedCommand();
        public static readonly RoutedCommand NewLine = new RoutedCommand();
        public static readonly RoutedCommand FillRoom = new RoutedCommand();
        public static readonly RoutedCommand IncCoordinate = new RoutedCommand();
        public static readonly RoutedCommand DecCoordinate = new RoutedCommand();

        #region construction
        public RoomEditorControl(Room room)
        {
            InitializeComponent();
            _Room = room;
            tvwCellSpaces.ItemContainerStyleSelector = new RoomEditorContextMenuSelector(this);
            DataContext = _Room;
            Resources.Add(@"roomMaterials", Room.Map.AllCellMaterials);

            // ambient lights
            cboAmbient.Items.Clear();
            cboAmbient.Items.Add(new ComboBoxItem { Content = @"-None-", Tag = null });
            if (_Room.Light == null)
            {
                cboAmbient.SelectedIndex = 0;
            }
            foreach (var _ambient in room.Map.AmbientLights.Select(_kvp => _kvp.Value))
            {
                var _cboItem = new ComboBoxItem
                {
                    Content = _ambient.Name,
                    Tag = _ambient,
                    ToolTip = string.Format(@"({0},{1},{2}) {3}", _ambient.ZOffset, _ambient.YOffset, _ambient.XOffset, _ambient.AmbientLevel)
                };

                cboAmbient.Items.Add(_cboItem);
                if (_Room.Light == _cboItem.Tag)
                {
                    cboAmbient.SelectedItem = _cboItem;
                }
            }
        }
        #endregion

        private Room _Room;
        public Room Room { get { return _Room; } }

        private void cboAmbient_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _light = (cboAmbient.SelectedItem as ComboBoxItem).Tag as Tactical.AmbientLight;
            if (_light != _Room.Light)
            {
                _Room.Light = _light;
            }
        }

        #region drawing
        private void cbNewCell_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbNewCell_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dlg = new NewCell(Room, e.Parameter as CellSpace);
            if (Parent is Window)
            {
                _dlg.Owner = Parent as Window;
                _dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            if (_dlg.ShowDialog() ?? false)
            {
                _dlg.DrawCell();
            }
            e.Handled = true;
        }

        private void cbNewBlock_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dlg = new NewBlock(Room, e.Parameter as CellSpace);
            if (Parent is Window)
            {
                _dlg.Owner = Parent as Window;
                _dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            if (_dlg.ShowDialog() ?? false)
            {
                _dlg.DrawBlock();
            }
            e.Handled = true;
        }

        private void cbNewLine_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dlg = new NewLine(Room, e.Parameter as CellSpace);
            if (Parent is Window)
            {
                _dlg.Owner = Parent as Window;
                _dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            if (_dlg.ShowDialog() ?? false)
            {
                _dlg.DrawLine();
            }
            e.Handled = true;
        }

        private void cbFillRoom_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _dlg = new FillRoom(Room, e.Parameter as CellSpace);
            if (Parent is Window)
            {
                _dlg.Owner = Parent as Window;
                _dlg.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
            if (_dlg.ShowDialog() ?? false)
            {
                _dlg.DrawFill();
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbIncDimension_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbIncDimension_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_Room != null)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z":
                    case @"ZH":
                        e.CanExecute = _Room.UpperZ < int.MaxValue;
                        break;
                    case @"Y":
                    case @"YL":
                        e.CanExecute = _Room.UpperY < int.MaxValue;
                        break;
                    case @"X":
                    case @"XL":
                        e.CanExecute = _Room.UpperX < int.MaxValue;
                        break;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbIncDimension_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbIncDimension_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_Room != null)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z":
                        _Room.BindableZ++;
                        break;
                    case @"Y":
                        _Room.BindableY++;
                        break;
                    case @"X":
                        _Room.BindableX++;
                        break;
                    case @"ZH":
                        _Room.BindableZHeight++;
                        break;
                    case @"YL":
                        _Room.BindableYLength++;
                        break;
                    case @"XL":
                        _Room.BindableXLength++;
                        break;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbDecDimension_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbDecDimension_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_Room != null)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z":
                        e.CanExecute = _Room.BindableZ > int.MinValue;
                        break;
                    case @"Y":
                        e.CanExecute = _Room.BindableY > int.MinValue;
                        break;
                    case @"X":
                        e.CanExecute = _Room.BindableX > int.MinValue;
                        break;
                    case @"ZH":
                        e.CanExecute = _Room.BindableZHeight > 1;
                        break;
                    case @"YL":
                        e.CanExecute = _Room.BindableYLength > 1;
                        break;
                    case @"XL":
                        e.CanExecute = _Room.BindableXLength > 1;
                        break;
                }
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbDecDimension_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbDecDimension_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _txt = e.Parameter as TextBox;
            if (_Room != null)
            {
                switch (e.Parameter.ToString())
                {
                    case @"Z":
                        _Room.BindableZ--;
                        break;
                    case @"Y":
                        _Room.BindableY--;
                        break;
                    case @"X":
                        _Room.BindableX--;
                        break;
                    case @"ZH":
                        _Room.BindableZHeight--;
                        break;
                    case @"YL":
                        _Room.BindableYLength--;
                        break;
                    case @"XL":
                        _Room.BindableXLength--;
                        break;
                }
            }
            e.Handled = true;
        }
        #endregion
    }

    public class RoomEditorContextMenuSelector : StyleSelector
    {
        public RoomEditorContextMenuSelector(Control roomEditor)
        {
            _RoomEditor = roomEditor;
        }

        public Control _RoomEditor;

        public override Style SelectStyle(object item, DependencyObject container)
        {
            var itemsControl = ItemsControl.ItemsControlFromItemContainer(container);
            var _item = container as TreeViewItem;
            if (item is CellSpace)
            {
                return _RoomEditor.Resources[@"styleSpace"] as Style;
            }

            // TODO: and the rest...!!!
            return Application.Current.Resources[@"styleTreeViewItem"] as Style;
        }
    }
}
