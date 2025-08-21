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
using System.Windows.Controls.Primitives;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for StackedBrush.xaml
    /// </summary>
    public partial class StackedBrush : Window
    {
        public static RoutedCommand OKCommand = new RoutedCommand();
        public static RoutedCommand UpCommand = new RoutedCommand();
        public static RoutedCommand DownCommand = new RoutedCommand();

        #region construction
        public StackedBrush(BrushCollection manager, string name, bool exists, BrushCollection components)
        {
            InitializeComponent();
            _Manager = manager;
            _Exists = exists;
            txtBrushKey.Text = name;
            _Components = components;
            RefreshSample();
        }
        #endregion

        #region private data
        private BrushCollection _Components;
        private BrushCollection _Manager;
        private bool _Exists;
        #endregion

        private BrushCollection Components
        {
            get { return _Components; }
        }

        #region private void RefreshSample()
        private void RefreshSample()
        {
            lstComponents.ItemsSource = null;
            lstComponents.ItemsSource = Components;

            // initialize
            var _brush = new DrawingBrush { Stretch = Stretch.Uniform };
            var _group = new DrawingGroup();

            // draw all brushes on overlapping rectangles (assumes alpha will take care of blending)
            // NOTE: first brush is on the top, others are underneath ".Reverse()"
            // NOTE: ... also, had to use ".Select(...), "
            // NOTE: ... since built-in "List.Reverse()" re-orders in place and returns void
            foreach (var _def in _Components.Select(_d => _d).Reverse())
            {
                _group.Children.Add(
                    new GeometryDrawing(_def.GetBrush(Uzi.Visualize.VisualEffect.Normal), null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
            }

            // finalize
            _brush.Drawing = _group;
            _brush.Freeze();
            rectSample.Fill = _brush;
        }
        #endregion

        public BrushDefinition GetBrushDefinition()
        {
            return new StackedBrushDefinition
            {
                BrushKey = txtBrushKey.Text,
                Components = Components
            };
        }

        #region private void cbOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        private void cbOK_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (txtBrushKey != null)
            {
                var _key = txtBrushKey.Text;
                if (!string.IsNullOrEmpty(_key))
                {
                    e.CanExecute = _Exists || !_Manager.Any(_b => _b.BrushKey.Equals(_key));
                }
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

        private void cbDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Components.Remove(lstComponents.SelectedItem as BrushDefinition);
            RefreshSample();
            e.Handled = true;
        }

        private void cbSelected(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstComponents != null) && (lstComponents.SelectedItem != null);
            e.Handled = true;
        }

        private void lstComponents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO: components changed
        }

        private void cbUp_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstComponents != null) && (lstComponents.SelectedIndex > 0);
            e.Handled = true;
        }

        private void cbUp_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _idx = lstComponents.SelectedIndex;
            var _item = Components[_idx];
            Components.Remove(_item);
            Components.Insert(_idx - 1, _item);
            RefreshSample();
            e.Handled = true;
        }

        private void cbDown_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (lstComponents != null) 
                && (lstComponents.SelectedItem!=null)
                && (lstComponents.SelectedIndex < lstComponents.Items.Count-1);
            e.Handled = true;
        }

        private void cbDown_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var _idx = lstComponents.SelectedIndex + 1;
            var _item = Components[_idx];
            Components.Remove(_item);
            Components.Insert(_idx - 1, _item);
            RefreshSample();
            e.Handled = true;
        }

        private void btnAddColor_Click(object sender, RoutedEventArgs e)
        {
            var _dlg = new SolidColor(null, string.Empty, Colors.Magenta, false)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                Components.Insert(0, _dlg.GetBrushDefinition());
                RefreshSample();
            }
        }

        private void btnAddGradient_Click(object sender, RoutedEventArgs e)
        {
            var _stops = new GradientStopCollection
            {
                new GradientStop(Colors.Black, 0),
                new GradientStop(Colors.White, 1)
            };
            var _dlg = new LinearGradient(null, string.Empty, _stops, 0d, false,
                ColorInterpolationMode.SRgbLinearInterpolation, GradientSpreadMethod.Pad, 1)
            {
                Owner = Window.GetWindow(this)
            };
            if (_dlg.ShowDialog() ?? false)
            {
                Components.Insert(0, _dlg.GetBrushDefinition());
                RefreshSample();
            }
        }

        private void lstComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstComponents.SelectedItem != null)
            {
                var _original = (BrushDefinition)lstComponents.SelectedItem;
                if (_original is SolidBrushDefinition _solid)
                {
                    var _opacity = _solid.Opacity;
                    var _dlg = new SolidColor(null, string.Empty, _solid.Color, true)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _brushDef = _dlg.GetBrushDefinition();
                        _brushDef.Opacity = _opacity; // TODO: unnecessary with Alpha-capable colors?
                        var _index = Components.IndexOf(_solid);
                        Components.Remove(_solid);
                        Components.Insert(_index, _brushDef);
                    }
                }
                else if (_original is LinearGradientBrushDefinition _linear)
                {
                    var _opacity = _linear.Opacity; // TODO: unnecessary with Alpha-capable colors?
                    var _dlg = new LinearGradient(null, string.Empty, _linear.GradientStops.Clone(), _linear.Angle, true,
                        _linear.ColorInterpolationMode, _linear.SpreadMethod, _linear.Size)
                    {
                        Owner = Window.GetWindow(this)
                    };
                    if (_dlg.ShowDialog() ?? false)
                    {
                        var _brushDef = _dlg.GetBrushDefinition();
                        _brushDef.Opacity = _opacity; // TODO: necessary with Alpha-capable colors?
                        var _index = Components.IndexOf(_linear);
                        Components.Remove(_linear);
                        Components.Insert(_index, _brushDef);
                    }
                }
                RefreshSample();
            }
        }
    }
}
