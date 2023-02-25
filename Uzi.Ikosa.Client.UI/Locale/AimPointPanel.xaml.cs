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
using System.Diagnostics;
using Uzi.Ikosa.Proxy.VisualizationSvc;
using Uzi.Ikosa.Proxy.ViewModel;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>
    /// Interaction logic for AimPointPanel.xaml
    /// </summary>
    public partial class AimPointPanel : UserControl
    {
        public AimPointPanel()
        {
            try { InitializeComponent(); } catch { }
        }

        #region public AimPointActivation AimPointActivation { get; set; } (DEPENDENCY)
        public AimPointActivation AimPointActivation
        {
            get { return (AimPointActivation)GetValue(AimPointActivationProperty); }
            set { SetValue(AimPointActivationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AimPointActivation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AimPointActivationProperty =
            DependencyProperty.Register(@"AimPointActivation", typeof(AimPointActivation), typeof(AimPointPanel),
            new UIPropertyMetadata(AimPointActivation.Off, OnAimPointActivationChanged));

        private static void OnAimPointActivationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is AimPointPanel _app)
            {
                var _activation = (AimPointActivation)args.NewValue;
                switch (_activation)
                {
                    case AimPointActivation.Off:
                        if (_app.LocaleViewModel != null)
                            _app.LocaleViewModel.ShowOverlay = false;
                        _app.IsEnabled = false;
                        _app.Background = null;
                        _app.ToolTip = null;
                        break;

                    case AimPointActivation.TargetCell:
                        if (_app.LocaleViewModel != null)
                            _app.LocaleViewModel.ShowOverlay = true;
                        _app.IsEnabled = true;
                        _app.Background = Brushes.Red;
                        _app.ToolTip = @"Target Cell";
                        break;

                    case AimPointActivation.TargetIntersection:
                        if (_app.LocaleViewModel != null)
                            _app.LocaleViewModel.ShowOverlay = true;
                        _app.IsEnabled = true;
                        _app.Background = Brushes.Orange;
                        _app.ToolTip = @"Target Intersection";
                        break;

                    case AimPointActivation.SetExtent:
                        if (_app.LocaleViewModel != null)
                            _app.LocaleViewModel.ShowOverlay = false;
                        _app.IsEnabled = true;
                        _app.Background = Brushes.LimeGreen;
                        _app.ToolTip = @"Aim from Cardinal Point";
                        break;

                    case AimPointActivation.AdjustPoint:
                        if (_app.LocaleViewModel != null)
                            _app.LocaleViewModel.ShowOverlay = false;
                        _app.IsEnabled = true;
                        _app.Background = Brushes.DarkGreen;
                        _app.ToolTip = @"Adjust Aim Point";
                        break;
                }
            }
        }
        #endregion

        #region public LocaleViewModel LocaleViewModel { get; set; } DEPENDENCY
        public LocaleViewModel LocaleViewModel
        {
            get { return (LocaleViewModel)GetValue(LocaleViewModelProperty); }
            set { SetValue(LocaleViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Shadings.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LocaleViewModelProperty =
            DependencyProperty.Register(@"LocaleViewModel", typeof(LocaleViewModel), typeof(AimPointPanel),
            new UIPropertyMetadata(null));
        #endregion

        #region public int ForwardHeading { get; set; } (DEPENDENCY)
        public int ForwardHeading
        {
            get { return (int)GetValue(ForwardHeadingProperty); }
            set { SetValue(ForwardHeadingProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ForwardHeading.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ForwardHeadingProperty =
            DependencyProperty.Register(nameof(ForwardHeading), typeof(int), typeof(AimPointPanel), new UIPropertyMetadata(0,
                OnSensorHostChanged));
        #endregion

        #region public SensorHostInfo SensorHost { get; set; } (DEPENDENCY)
        public SensorHostInfo SensorHost
        {
            get { return (SensorHostInfo)GetValue(SensorHostProperty); }
            set { SetValue(SensorHostProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SensorHost.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SensorHostProperty =
            DependencyProperty.Register(@"SensorHost", typeof(SensorHostInfo), typeof(AimPointPanel),
            new UIPropertyMetadata(null, OnSensorHostChanged));

        private static void OnSensorHostChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var _app = obj as AimPointPanel;
            if (_app != null)
            {
                var _sensors = _app.SensorHost as SensorHostInfo;
                if (_sensors != null)
                {

                    // heading
                    _app.txHeading.Angle = (_app.ForwardHeading % 8) * 45d;

                    // relative distance
                    _app.txDistance.ScaleY = _sensors.AimPointRelDistance;
                    _app.txDistance2.ScaleX = _sensors.AimPointRelDistance;

                    // relative longitude
                    if (double.IsNaN(_sensors.AimPointRelLongitude))
                        _app.txLongitude.Angle = 0d;
                    else
                        _app.txLongitude.Angle = -1d * _sensors.AimPointRelLongitude;

                    // latitude
                    if (double.IsNaN(_sensors.AimPointRelLatitude))
                        _app.txLatitude.Angle = 0;
                    else
                        _app.txLatitude.Angle = -1d * _sensors.AimPointRelLatitude;

                    // offset
                    double _bottom = 0;
                    double _offset = 0;
                    double _top = 0;
                    long _cross = 1;
                    long _depth = 1;
                    long _height = 1;
                    switch (_sensors.GravityAnchorFace)
                    {
                        case AnchorFace.ZLow:
                        case AnchorFace.ZHigh:
                            _bottom = _sensors.Z * 5d;
                            _top = (_sensors.ZTop + 1) * 5d;
                            _offset = _sensors.AimPoint.Z;
                            _height = _sensors.ZHeight;
                            switch (_app.ForwardHeading)
                            {
                                case 7:
                                case 0:
                                case 3:
                                case 4:
                                    _cross = _sensors.YLength;
                                    _depth = _sensors.XLength;
                                    break;

                                default:
                                    _depth = _sensors.YLength;
                                    _cross = _sensors.XLength;
                                    break;
                            }
                            break;

                        case AnchorFace.YLow:
                        case AnchorFace.YHigh:
                            _bottom = _sensors.Y * 5d;
                            _top = (_sensors.YTop + 1) * 5d;
                            _offset = _sensors.AimPoint.Y;
                            _height = _sensors.YLength;
                            switch (_app.ForwardHeading)
                            {
                                case 7:
                                case 0:
                                case 3:
                                case 4:
                                    _cross = _sensors.XLength;
                                    _depth = _sensors.ZHeight;
                                    break;

                                default:
                                    _depth = _sensors.XLength;
                                    _cross = _sensors.ZHeight;
                                    break;
                            }
                            break;

                        case AnchorFace.XLow:
                        case AnchorFace.XHigh:
                            _bottom = _sensors.X * 5d;
                            _top = (_sensors.XTop + 1) * 5d;
                            _offset = _sensors.AimPoint.X;
                            _height = _sensors.XLength;
                            switch (_app.ForwardHeading)
                            {
                                case 7:
                                case 0:
                                case 3:
                                case 4:
                                    _cross = _sensors.ZHeight;
                                    _depth = _sensors.YLength;
                                    break;

                                default:
                                    _depth = _sensors.ZHeight;
                                    _cross = _sensors.YLength;
                                    break;
                            }
                            break;
                    }

                    // TODO: height index of aim cell (diff color)
                    if (_app.ugHeight.Rows != (int)_height)
                    {
                        _app.ugHeight.Rows = (int)_height;
                        _app.ugHeight.Children.Clear();
                        for (int _hx = 0; _hx < _height; _hx++)
                        {
                            _app.ugHeight.Children.Add(new Rectangle { Margin = new Thickness(0.5), Fill = Brushes.DarkGray });
                        }
                    }

                    // TODO: cross,deep indices of aim cell (diff color)
                    if ((_app.ugFootprint.Columns != (int)_cross) || (_app.ugFootprint.Rows != (int)_depth))
                    {
                        _app.ugFootprint.Columns = (int)_cross;
                        _app.ugFootprint.Rows = (int)_depth;
                        _app.ugFootprint.Children.Clear();
                        for (int _cx = 0; _cx < _cross; _cx++)
                            for (int _rx = 0; _rx < _depth; _rx++)
                            {
                                _app.ugHeight.Children.Add(new Rectangle { Margin = new Thickness(0.5), Fill = Brushes.DarkGray });
                            }
                    }

                    // scale line to size
                    _offset = (_offset - _bottom) / (_top - _bottom);
                    if (_sensors.GravityAnchorFace.IsLowFace())
                    {
                        _offset = 1 - _offset;
                    }

                    // scale line to rectangle
                    _offset = (28 * _offset) + 2;

                    _app.lnRectangle.Y1 = _offset;
                    _app.lnRectangle.Y2 = _offset;
                }
            }
        }
        #endregion

        public void ResetTargetCell()
        {
            LocaleViewModel.ResetTargetCell();
        }
    }
}
