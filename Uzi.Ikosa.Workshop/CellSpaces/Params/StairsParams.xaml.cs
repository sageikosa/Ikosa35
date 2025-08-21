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
    /// Interaction logic for StairsParams.xaml
    /// </summary>
    public partial class StairsParams : UserControl, IParamControl
    {
        public StairsParams()
        {
            InitializeComponent();
        }

        public StairsParams(uint paramData)
        {
            InitializeComponent();
            cboClimb.SelectedItem = StairSpaceFaces.GetClimbOpening(paramData);
            cboTravel.SelectedItem = StairSpaceFaces.GetTravelOpening(paramData);
        }

        public AnchorFace ClimbTowards
        {
            get
            {
                if (cboClimb.SelectedIndex > -1)
                {
                    return (AnchorFace)cboClimb.SelectedItem;
                }

                return AnchorFace.ZHigh;
            }
        }

        public AnchorFace TravelFrom
        {
            get
            {
                if (cboTravel.SelectedIndex > -1)
                {
                    return (AnchorFace)cboTravel.SelectedItem;
                }

                if (cboClimb.SelectedIndex == -1)
                {
                    return AnchorFace.YHigh;
                }

                switch ((AnchorFace)cboClimb.SelectedItem)
                {
                    case AnchorFace.ZHigh:
                    case AnchorFace.ZLow:
                        return AnchorFace.YHigh;

                    default:
                        return AnchorFace.ZHigh;
                }
            }
        }

        private void cboClimb_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((AnchorFace)cboClimb.SelectedItem)
            {
                case AnchorFace.ZHigh:
                case AnchorFace.ZLow:
                    cboTravel.ItemsSource =
                        new[] { AnchorFace.YHigh, AnchorFace.YLow, AnchorFace.XHigh, AnchorFace.XLow };
                    cboTravel.SelectedIndex = 0;
                    break;

                case AnchorFace.YHigh:
                case AnchorFace.YLow:
                    cboTravel.ItemsSource =
                        new[] { AnchorFace.ZHigh, AnchorFace.ZLow, AnchorFace.XHigh, AnchorFace.XLow };
                    cboTravel.SelectedIndex = 0;
                    break;

                default:
                    cboTravel.ItemsSource =
                        new[] { AnchorFace.ZHigh, AnchorFace.ZLow, AnchorFace.YHigh, AnchorFace.YLow };
                    cboTravel.SelectedIndex = 0;
                    break;
            }
        }

        public uint ParamData { get { return StairSpaceFaces.GetParam(ClimbTowards, TravelFrom); } }
    }
}
