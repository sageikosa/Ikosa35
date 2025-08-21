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
    /// Interaction logic for LFrameParams.xaml
    /// </summary>
    public partial class LFrameParams : UserControl, IParamControl
    {
        public LFrameParams()
        {
            InitializeComponent();
        }

        public LFrameParams(uint paramData)
        {
            InitializeComponent();
            cboThickWall.SelectedItem = LFrameSpaceFaces.GetThickFace(paramData);
            cboFrame1.SelectedItem = LFrameSpaceFaces.GetFrame1Face(paramData);
            cboFrame2.SelectedItem = LFrameSpaceFaces.GetFrame2Face(paramData);
        }

        public AnchorFace ThickFace
        {
            get
            {
                if (cboThickWall.SelectedIndex > -1)
                {
                    return (AnchorFace)cboThickWall.SelectedItem;
                }

                return AnchorFace.XLow;
            }
        }

        public AnchorFace Frame1Face { get { return (AnchorFace)cboFrame1.SelectedItem; } }

        public AnchorFace Frame2Face { get { return (AnchorFace)cboFrame2.SelectedItem; } }

        public uint ParamData { get { return LFrameSpaceFaces.GetParam(ThickFace, Frame1Face, Frame2Face); } }

        private void cboThickWall_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((AnchorFace)cboThickWall.SelectedItem)
            {
                case AnchorFace.ZHigh:
                case AnchorFace.ZLow:
                    cboFrame1.ItemsSource =
                        new[] { AnchorFace.YHigh, AnchorFace.YLow, AnchorFace.XHigh, AnchorFace.XLow };
                    cboFrame1.SelectedIndex = 0;
                    break;

                case AnchorFace.YHigh:
                case AnchorFace.YLow:
                    cboFrame1.ItemsSource =
                        new[] { AnchorFace.ZHigh, AnchorFace.ZLow, AnchorFace.XHigh, AnchorFace.XLow };
                    cboFrame1.SelectedIndex = 0;
                    break;

                default:
                    cboFrame1.ItemsSource =
                        new[] { AnchorFace.ZHigh, AnchorFace.ZLow, AnchorFace.YHigh, AnchorFace.YLow };
                    cboFrame1.SelectedIndex = 0;
                    break;
            }
        }

        private void cboFrame1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch ((AnchorFace)cboThickWall.SelectedItem)
            {
                case AnchorFace.ZHigh:
                case AnchorFace.ZLow:
                    if (cboFrame1.SelectedItem != null)
                    {
                        switch ((AnchorFace)cboFrame1.SelectedItem)
                        {
                            case AnchorFace.YHigh:
                            case AnchorFace.YLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.XHigh, AnchorFace.XLow };
                                cboFrame2.SelectedIndex = 0;
                                break;

                            case AnchorFace.XHigh:
                            case AnchorFace.XLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.YHigh, AnchorFace.YLow };
                                cboFrame2.SelectedIndex = 0;
                                break;
                        }
                    }

                    break;

                case AnchorFace.YHigh:
                case AnchorFace.YLow:
                    if (cboFrame1.SelectedItem != null)
                    {
                        switch ((AnchorFace)cboFrame1.SelectedItem)
                        {
                            case AnchorFace.ZHigh:
                            case AnchorFace.ZLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.XHigh, AnchorFace.XLow };
                                cboFrame2.SelectedIndex = 0;
                                break;

                            case AnchorFace.XHigh:
                            case AnchorFace.XLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.ZHigh, AnchorFace.ZLow };
                                cboFrame2.SelectedIndex = 0;
                                break;
                        }
                    }

                    break;

                default:
                    if (cboFrame1.SelectedItem != null)
                    {
                        switch ((AnchorFace)cboFrame1.SelectedItem)
                        {
                            case AnchorFace.ZHigh:
                            case AnchorFace.ZLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.YHigh, AnchorFace.YLow };
                                cboFrame2.SelectedIndex = 0;
                                break;

                            case AnchorFace.YHigh:
                            case AnchorFace.YLow:
                                cboFrame2.ItemsSource =
                                    new[] { AnchorFace.ZHigh, AnchorFace.ZLow };
                                cboFrame2.SelectedIndex = 0;
                                break;
                        }
                    }

                    break;
            }
        }
    }
}
