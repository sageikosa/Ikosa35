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
using Uzi.Visualize;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for SlopeParams.xaml
    /// </summary>
    public partial class SlopeParams : UserControl, IParamControl
    {
        public SlopeParams()
        {
            InitializeComponent();
            sldrLoOffset.TickFrequency = 5d / 60d;
            sldrLoOffset.SmallChange = 5d / 60d;
            sldrHiOffset.TickFrequency = 5d / 60d;
            sldrHiOffset.SmallChange = 5d / 60d;
        }

        public SlopeParams(uint paramData)
        {
            InitializeComponent();
            sldrLoOffset.TickFrequency = 5d / 60d;
            sldrLoOffset.SmallChange = 5d / 60d;
            sldrHiOffset.TickFrequency = 5d / 60d;
            sldrHiOffset.SmallChange = 5d / 60d;
            var _param = new SliverSlopeParams(paramData);
            var _axis = _param.Axis;
            var _flip = _param.Flip;
            switch (_axis)
            {
                case Axis.Z:
                    cboBindFace.SelectedItem = _flip ? AnchorFace.ZHigh : AnchorFace.ZLow;
                    break;
                case Axis.Y:
                    cboBindFace.SelectedItem = _flip ? AnchorFace.YHigh : AnchorFace.YLow;
                    break;
                case Axis.X:
                    cboBindFace.SelectedItem = _flip ? AnchorFace.XHigh : AnchorFace.XLow;
                    break;
            }

            cboSlopeAxis.SelectedItem = _param.SlopeAxis;
            sldrLoOffset.Value = _param.Offset;
            sldrHiOffset.Value = _param.HiOffset;
            chkZLoYLo.IsChecked = _param.ZLoYLo;
            chkZLoYHi.IsChecked = _param.ZLoYHi;
            chkZLoXLo.IsChecked = _param.ZLoXLo;
            chkZLoXHi.IsChecked = _param.ZLoXHi;
            chkZHiYLo.IsChecked = _param.ZHiYLo;
            chkZHiYHi.IsChecked = _param.ZHiYHi;
            chkZHiXLo.IsChecked = _param.ZHiXLo;
            chkZHiXHi.IsChecked = _param.ZHiXHi;
            chkYLoXLo.IsChecked = _param.YLoXLo;
            chkYLoXHi.IsChecked = _param.YLoXHi;
            chkYHiXLo.IsChecked = _param.YHiXLo;
            chkYHiXHi.IsChecked = _param.YHiXHi;
        }

        public bool FlipAxis
            => !BindingFace.IsLowFace();

        public AnchorFace BindingFace
            => (cboBindFace.SelectedIndex > -1)
            ? (AnchorFace)(cboBindFace.SelectedItem ?? AnchorFace.ZLow)
            : AnchorFace.ZLow;

        public Axis SlopeAxis
            => (cboSlopeAxis.SelectedIndex > -1)
            ? (Axis)cboSlopeAxis.SelectedItem
            : Axis.Y;

        public Axis OrthoAxis
            => BindingFace.GetAxis();

        // IParamControl Members
        public uint ParamData
            => (new SliverSlopeParams
            {
                Axis = OrthoAxis,
                Flip = FlipAxis,
                SlopeAxis = SlopeAxis,
                Offset = sldrLoOffset.Value,
                HiOffset = sldrHiOffset.Value,
                ZLoYLo = chkZLoYLo.IsChecked ?? false,
                ZLoYHi = chkZLoYHi.IsChecked ?? false,
                ZLoXLo = chkZLoXLo.IsChecked ?? false,
                ZLoXHi = chkZLoXHi.IsChecked ?? false,
                ZHiYLo = chkZHiYLo.IsChecked ?? false,
                ZHiYHi = chkZHiYHi.IsChecked ?? false,
                ZHiXLo = chkZHiXLo.IsChecked ?? false,
                ZHiXHi = chkZHiXHi.IsChecked ?? false,
                YLoXLo = chkYLoXLo.IsChecked ?? false,
                YLoXHi = chkYLoXHi.IsChecked ?? false,
                YHiXLo = chkYHiXLo.IsChecked ?? false,
                YHiXHi = chkYHiXHi.IsChecked ?? false
            }).Value;
    }
}
