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
using System.Windows.Shapes;
using Uzi.Visualize;
using Uzi.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for NewLinearGradient.xaml
    /// </summary>
    public partial class LinearGradient : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();
        public static RoutedCommand AngleCommand = new RoutedCommand();
        public static RoutedCommand InsertCommand = new RoutedCommand();
        public static RoutedCommand StepsCommand = new RoutedCommand();

        #region construction
        public LinearGradient(BrushCollection manager, string name, GradientStopCollection stops, double angle, bool exists,
            ColorInterpolationMode mode, GradientSpreadMethod spread, double size)
        {
            InitializeComponent();
            _Manager = manager;
            _Exists = exists;
            if (_Manager == null)
            {
                lblBrushKey.Visibility = System.Windows.Visibility.Collapsed;
                txtBrushKey.Visibility = System.Windows.Visibility.Collapsed;
                txtBrushKey.IsEnabled = false;
                cpStart.UsingAlphaChannel = true;
            }
            else
                cpStart.UsingAlphaChannel = false;
            txtBrushKey.Text = name;
            lstStops.ItemsSource = new GradientStopCollection(stops.OrderBy(_s => _s.Offset));
            cboInterpolation.SelectedItem = mode;
            cboSpread.SelectedItem = spread;
            sldrSize.Value = size;
            _Angle = angle;
            RefreshSample();
        }
        #endregion

        #region private data
        private BrushCollection _Manager;
        private bool _Exists;
        private double _Angle;
        #endregion

        #region private void RefreshSample()
        private void RefreshSample()
        {
            var _spread = cboSpread.SelectedItem != null
                ? (GradientSpreadMethod)cboSpread.SelectedItem
                : GradientSpreadMethod.Pad;
            var _inter = cboInterpolation.SelectedItem != null
                ? (ColorInterpolationMode)cboInterpolation.SelectedItem
                : ColorInterpolationMode.SRgbLinearInterpolation;
            if (lstStops.ItemsSource is GradientStopCollection _stops)
            {
                var _size = sldrSize.Value / 2;
                rectSample.Fill = new LinearGradientBrush(_stops,
                    new Point(0.5 - _size, 0.5),
                    new Point(0.5 + _size, 0.5))
                {
                    RelativeTransform = new RotateTransform(_Angle, 0.5d, 0.5d),
                    SpreadMethod = _spread,
                    ColorInterpolationMode = _inter
                };
            }
        }
        #endregion

        #region public BrushDefinition GetBrushDefinition()
        public BrushDefinition GetBrushDefinition()
        {
            return new LinearGradientBrushDefinition
            {
                BrushKey = txtBrushKey.Text,
                GradientStops = lstStops.ItemsSource as GradientStopCollection,
                ColorInterpolationMode = (ColorInterpolationMode)cboInterpolation.SelectedItem,
                SpreadMethod = (GradientSpreadMethod)cboSpread.SelectedItem,
                Angle = _Angle,
                Size = sldrSize.Value
            };
        }
        #endregion

        #region private void cbAngle_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbAngle_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
        #endregion

        #region private void cbAngle_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbAngle_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _Angle = Convert.ToDouble(e.Parameter.ToString());
            RefreshSample();
        }
        #endregion

        #region private void cbOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_Manager == null)
            {
                e.CanExecute = true;
            }
            else if (txtBrushKey != null)
            {
                var _key = txtBrushKey.Text;
                if (!string.IsNullOrEmpty(_key))
                    e.CanExecute = _Exists || !_Manager.Any(_b => _b.BrushKey.Equals(_key));
            }
            e.Handled = true;
        }
        #endregion

        #region private void cbOK_Executed(object sender, ExecutedRoutedEventArgs e)
        private void cbOK_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DialogResult = true;
            Close();
            e.Handled = true;
        }
        #endregion

        #region private void btnCancel_Click(object sender, RoutedEventArgs e)
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        #endregion

        #region private void lstStops_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void lstStops_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var _stop = (lstStops.SelectedItem as GradientStop);
            if (_stop != null)
            {
                cpStart.SelectedColor = _stop.Color;
                sldrOffset.Value = _stop.Offset;
                cpStart.IsEnabled = true;
            }
        }
        #endregion

        #region private void Reorder()
        private void Reorder()
        {
            var _stops = lstStops.ItemsSource as GradientStopCollection;
            var _newStops = _stops.OrderBy(_s => _s.Offset).ToList();
            if (!_stops.SequenceEqual(_newStops))
            {
                var _selected = lstStops.SelectedItem;
                lstStops.ItemsSource = new GradientStopCollection(_newStops);
                lstStops.SelectedItem = _selected;
            }
        }
        #endregion

        #region private void sldrOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        private void sldrOffset_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var _stop = (lstStops.SelectedItem as GradientStop);
            if (_stop != null)
            {
                _stop.Offset = e.NewValue;
                Reorder();
                RefreshSample();
            }
            e.Handled = true;
        }
        #endregion

        #region private void cboInterpolation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboInterpolation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSample();
            e.Handled = true;
        }
        #endregion

        #region private void cboSpread_SelectionChanged(object sender, SelectionChangedEventArgs e)
        private void cboSpread_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshSample();
            e.Handled = true;
        }
        #endregion

        #region private void sldrSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        private void sldrSize_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshSample();
            e.Handled = true;
        }
        #endregion

        #region private void cpStart_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        private void cpStart_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {
            var _stop = (lstStops.SelectedItem as GradientStop);
            if (_stop != null)
            {
                _stop.Color = e.NewValue ?? Colors.Black;
                RefreshSample();
            }
            e.Handled = true;
        }
        #endregion

        #region Add, Remove, and Insert
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            GradientStop _new = null;
            if (lstStops.ItemsSource is GradientStopCollection _stops)
            {
                _new = new GradientStop(Colors.Black, 0);
                _stops.Add(_new);
            }

            // refresh controls and select new item
            lstStops.Items.Refresh();
            if (_new != null)
                lstStops.SelectedItem = _new;
            RefreshSample();
            e.Handled = true;
        }

        private void cbSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstStops != null) && (lstStops.SelectedItem != null);
            e.Handled = true;
        }

        private void cbDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _s = lstStops.SelectedItem as GradientStop;
            if ((lstStops.ItemsSource is GradientStopCollection _stops) && (_s != null))
            {
                _stops.Remove(_s);
            }

            // refresh controls
            lstStops.Items.Refresh();
            RefreshSample();
            e.Handled = true;
        }

        private void cbInsert_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // existing stops
            var _stops = lstStops.ItemsSource as GradientStopCollection;
            var _lowStop = (lstStops.SelectedIndex > 0) ? _stops[lstStops.SelectedIndex - 1] as GradientStop : null;
            var _hiStop = lstStops.SelectedItem as GradientStop;
            GradientStop _new = null;

            if ((_stops != null) && (_hiStop != null))
            {
                var _mid = (_lowStop != null) ? (_hiStop.Offset + _lowStop.Offset) / 2 : 0;
                _new = new GradientStop(Colors.Black, _mid);
                _stops.Insert(lstStops.SelectedIndex, _new);
            }

            // refresh controls and select new item
            lstStops.Items.Refresh();
            if (_new != null)
                lstStops.SelectedItem = _new;
            RefreshSample();
            e.Handled = true;
        }
        #endregion

        private void cbSteps_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            sldrSize.Value = (1d / Convert.ToDouble(e.Parameter));
            e.Handled = true;
        }

        private void cbSteps_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }
    }
}
