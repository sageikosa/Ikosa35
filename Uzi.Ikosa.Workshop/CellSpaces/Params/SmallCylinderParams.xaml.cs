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
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SmallCylinderParams.xaml
    /// </summary>
    public partial class SmallCylinderParams : UserControl, IParamControl
    {
        public SmallCylinderParams()
        {
            InitializeComponent();
        }

        public SmallCylinderParams(uint paramData)
        {
            InitializeComponent();
            var _param = new Uzi.Visualize.WedgeParams(paramData);
            cboAxis.SelectedItem = _param.Axis;
            chkPri.IsChecked = _param.InvertPrimary;
            chkSec.IsChecked = _param.InvertSecondary;
            cboStyle.SelectedItem = _param.Style;
            sldrSegment.Value = _param.SegmentCount;
        }

        public Visualize.Axis SelectedAxis
            => (cboAxis.SelectedIndex > -1)
            ? (Visualize.Axis)cboAxis.SelectedItem
            : Visualize.Axis.Z;

        public Visualize.CylinderStyle CylinderStyle
            => (cboStyle.SelectedIndex > -1)
            ? (Visualize.CylinderStyle)cboStyle.SelectedItem
            : Visualize.CylinderStyle.Smooth;

        public uint ParamData
            => (new Uzi.Visualize.WedgeParams
            {
                Axis = SelectedAxis,
                InvertPrimary = chkPri.IsChecked ?? false,
                InvertSecondary = chkSec.IsChecked ?? false,
                SegmentCount = (byte)sldrSegment.Value,
                Style = CylinderStyle
            }).Value;

        private void cboAxis_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _param = new Uzi.Visualize.WedgeParams
            {
                Axis = SelectedAxis,
                InvertPrimary = chkPri?.IsChecked ?? false,
                InvertSecondary = chkSec?.IsChecked ?? false,
                SegmentCount = (byte)(sldrSegment?.Value ?? 2),
                Style = CylinderStyle
            };
            chkPri.Content = $@"{_param.PrimarySnap.GetAxis()} Hi";
            chkSec.Content = $@"{_param.SecondarySnap.GetAxis()} Hi";
        }
    }
}
