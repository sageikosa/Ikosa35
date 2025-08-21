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
    /// Interaction logic for WedgeParams.xaml
    /// </summary>
    public partial class WedgeParams : UserControl, IParamControl
    {
        public WedgeParams()
        {
            InitializeComponent();
        }

        public WedgeParams(uint paramData)
        {
            InitializeComponent();
            var _param = new Visualize.WedgeParams(paramData);
            cboAxis.SelectedItem = _param.Axis;
            chkFlip.IsChecked = _param.FlipOffsets;
            chkInvertPrime.IsChecked = _param.InvertPrimary;
            chkInvertSecond.IsChecked = _param.InvertSecondary;
        }

        public Axis ParallelAxis
        {
            get
            {
                if (cboAxis.SelectedIndex > -1)
                {
                    return (Axis)cboAxis.SelectedItem;
                }

                return Axis.Z;
            }
        }

        public bool FlipOffsets { get { return chkFlip.IsChecked ?? false; } }
        public bool InvertPrimary { get { return chkInvertPrime.IsChecked ?? false; } }
        public bool InvertSecondary { get { return chkInvertSecond.IsChecked ?? false; } }

        public uint ParamData
            => (new Visualize.WedgeParams
            {
                Axis = ParallelAxis,
                FlipOffsets = FlipOffsets,
                InvertPrimary = InvertPrimary,
                InvertSecondary = InvertSecondary
            }).Value;
    }
}
