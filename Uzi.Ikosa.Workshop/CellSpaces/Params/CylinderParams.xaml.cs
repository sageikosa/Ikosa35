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

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for CylinderParams.xaml
    /// </summary>
    public partial class CylinderParams : UserControl, IParamControl
    {
        public CylinderParams()
        {
            InitializeComponent();
            sldrPediment.TickFrequency = 5d / 60d;
            sldrPediment.SmallChange = 5d / 60d;
        }

        public CylinderParams(uint paramData)
        {
            InitializeComponent();
            sldrPediment.TickFrequency = 5d / 60d;
            sldrPediment.SmallChange = 5d / 60d;
            var _param = new Uzi.Visualize.CylinderParams(paramData);
            cboBindFace.SelectedItem = _param.AnchorFace;
            cboStyle.SelectedItem = _param.Style;
            sldrSegment.Value = _param.SegmentCount;
            sldrPediment.Value = _param.Pediment;
        }

        public Visualize.AnchorFace BindingFace
            => (cboBindFace.SelectedIndex > -1)
            ? (Visualize.AnchorFace)cboBindFace.SelectedItem
            : Visualize.AnchorFace.ZLow;

        public Visualize.CylinderStyle CylinderStyle
            => (cboStyle.SelectedIndex > -1)
            ? (Visualize.CylinderStyle)cboStyle.SelectedItem
            : Visualize.CylinderStyle.Smooth;

        public uint ParamData
            => (new Uzi.Visualize.CylinderParams
            {
                AnchorFace = BindingFace,
                Pediment = sldrPediment.Value,
                SegmentCount = (byte)sldrSegment.Value,
                Style = CylinderStyle
            }).Value;
    }
}
