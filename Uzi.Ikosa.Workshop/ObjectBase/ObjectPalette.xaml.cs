using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Uzi.Core;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for ObjectPalette.xaml
    /// </summary>
    public partial class ObjectPalette : UserControl
    {
        public static RoutedCommand SelectObject = new RoutedCommand();
        public static RoutedCommand CaptureObject = new RoutedCommand();

        #region ctor()
        public ObjectPalette(LocalMap map, IInputElement drawTarget)
        {
            InitializeComponent();
            LoadMap(map);

            // targets
            btnObject1.CommandTarget = drawTarget;
            btnObject2.CommandTarget = drawTarget;
            btnObject3.CommandTarget = drawTarget;
            btnObject4.CommandTarget = drawTarget;
            btnObject5.CommandTarget = drawTarget;
            btnObject6.CommandTarget = drawTarget;
            btnObject7.CommandTarget = drawTarget;
            btnObject8.CommandTarget = drawTarget;
            btnObject9.CommandTarget = drawTarget;
            btnObject0.CommandTarget = drawTarget;

            // parameters
            btnObject1.CommandParameter = ccObject1;
            btnObject2.CommandParameter = ccObject2;
            btnObject3.CommandParameter = ccObject3;
            btnObject4.CommandParameter = ccObject4;
            btnObject5.CommandParameter = ccObject5;
            btnObject6.CommandParameter = ccObject6;
            btnObject7.CommandParameter = ccObject7;
            btnObject8.CommandParameter = ccObject8;
            btnObject9.CommandParameter = ccObject9;
            btnObject0.CommandParameter = ccObject0;
        }
        #endregion

        #region public void LoadMap(LocalMap map)
        public void LoadMap(LocalMap map)
        {
            // menu items
            foreach (var _mnu in new[]
            {
                mnuObject1, mnuObject2, mnuObject3, mnuObject4, mnuObject5,
                mnuObject6, mnuObject7, mnuObject8, mnuObject9, mnuObject0
            })
            {
                _mnu.Items.Clear();
                _mnu.Items.Add(new MenuItem
                {
                    Header = @"Capture",
                    Command = CaptureObject,
                    CommandParameter = _mnu.Tag.ToString()
                });
                _mnu.Items.Add(new Separator());
                foreach (var _objBase in map.MapContext.AllOf<IObjectBase>())
                {
                    _mnu.Items.Add(new MenuItem
                    {
                        Header = _objBase,
                        Command = SelectObject,
                        CommandParameter = new Tuple<string, IObjectBase>(_mnu.Tag.ToString(), _objBase)
                    });
                }
            }
        }
        #endregion

        #region cpCaptureObject
        private void cbCaptureObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

        }

        private void cbCaptureObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
        #endregion

        #region cbSelectObject
        private void cbSelectObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

        private void cbSelectObject_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _param = e.Parameter as Tuple<string, IObjectBase>;
            ContentControl _ccObject = null;
            switch (_param.Item1)
            {
                case @"1": _ccObject = ccObject1; break;
                case @"2": _ccObject = ccObject2; break;
                case @"3": _ccObject = ccObject3; break;
                case @"4": _ccObject = ccObject4; break;
                case @"5": _ccObject = ccObject5; break;
                case @"6": _ccObject = ccObject6; break;
                case @"7": _ccObject = ccObject7; break;
                case @"8": _ccObject = ccObject8; break;
                case @"9": _ccObject = ccObject9; break;
                default: _ccObject = ccObject0; break;
            }
            _ccObject.Content = _param.Item2;
            e.Handled = true;
        }
        #endregion

        #region public object CommandParameter(int slot)
        public object CommandParameter(int slot)
        {
            switch (slot)
            {
                case 1: return ccObject1;
                case 2: return ccObject2;
                case 3: return ccObject3;
                case 4: return ccObject4;
                case 5: return ccObject5;
                case 6: return ccObject6;
                case 7: return ccObject7;
                case 8: return ccObject8;
                case 9: return ccObject9;
                default: return ccObject0;
            }
        }
        #endregion
    }
}
