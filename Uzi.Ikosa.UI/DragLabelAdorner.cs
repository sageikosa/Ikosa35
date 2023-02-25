using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;

namespace Uzi.Ikosa.UI
{
    public class DragLabelAdorner : Adorner
    {
        // track what and where we show
        private Label _Label;
        private double _Left;
        private double _Top;

        public DragLabelAdorner(UIElement target, string msg)
            : base(target)
        {
            _Label = new Label();
            _Label.Content = msg;
            _Label.Background = Brushes.White;
            _Label.BorderBrush = Brushes.Black;
            _Label.BorderThickness = new Thickness(1);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size constraint)
        {
            _Label.Measure(constraint);
            return _Label.DesiredSize; ;
        }

        protected override System.Windows.Size ArrangeOverride(System.Windows.Size finalSize)
        {
            _Label.Arrange(new Rect(finalSize));
            return finalSize; ;
        }

        protected override Visual GetVisualChild(int index) { return _Label; }
        protected override int VisualChildrenCount { get { return 1; } }

        public double Left
        {
            get { return _Left; }
            set
            {
                _Left = value;
                UpdatePosition();
            }
        }

        public double Top
        {
            get { return _Top; }
            set
            {
                _Top = value;
                UpdatePosition();
            }
        }

        private void UpdatePosition()
        {
            AdornerLayer _aLayer = this.Parent as AdornerLayer;
            if (_aLayer != null)
                _aLayer.Update(AdornedElement);
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            GeneralTransformGroup _result = new GeneralTransformGroup();
            _result.Children.Add(base.GetDesiredTransform(transform));
            _result.Children.Add(new TranslateTransform(Left, Top));
            return _result;
        }
    }
}