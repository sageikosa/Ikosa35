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

namespace Uzi.Ikosa.Client.UI
{
    /// <summary>Interaction logic for Headings.xaml</summary>
    public partial class Headings : UserControl
    {
        public Headings()
        {
            try { InitializeComponent(); } catch { }
        }

        public int FreeYaw
        {
            get { return (int)GetValue(FreeYawProperty); }
            set { SetValue(FreeYawProperty, value); }
        }

        public int MaxYaw
        {
            get { return (int)GetValue(MaxYawProperty); }
            set { SetValue(MaxYawProperty, value); }
        }

        public double YawGap
        {
            get { return (double)GetValue(YawGapProperty); }
            set { SetValue(YawGapProperty, value); }
        }

        public double Incline
        {
            get { return (double)GetValue(InclineProperty); }
            set { SetValue(InclineProperty, value); }
        }

        public double DistanceSinceTurn
        {
            get { return (double)GetValue(DistanceSinceTurnProperty); }
            set { SetValue(DistanceSinceTurnProperty, value); }
        }

        public int? MoveHeading
        {
            get { return (int?)GetValue(MoveHeadingProperty); }
            set { SetValue(MoveHeadingProperty, value); }
        }

        public int LookHeading
        {
            get { return (int)GetValue(LookHeadingProperty); }
            set { SetValue(LookHeadingProperty, value); }
        }

        #region public void RefreshGrid()
        public void RefreshGrid()
        {
            // build brush wheel
            Brush _ok = Brushes.Green;
            Brush _costly = Brushes.Yellow;
            Brush _bad = Brushes.LightGray;
            var _colorList = new List<Brush>();
            _colorList.Add(_ok); // 0
            if ((DistanceSinceTurn >= YawGap) && (MaxYaw >= 1))
            {
                _colorList.Add(_ok); // 1
                _colorList.Add(MaxYaw >= 2 ? (FreeYaw > 1 ? _ok : _costly) : _bad); // 2
                _colorList.Add(MaxYaw >= 3 ? (FreeYaw > 2 ? _ok : _costly) : _bad); // 3
                _colorList.Add(MaxYaw >= 4 ? (FreeYaw > 3 ? _ok : _costly) : _bad); // 4
                _colorList.Add(MaxYaw >= 3 ? (FreeYaw > 2 ? _ok : _costly) : _bad); // 5
                _colorList.Add(MaxYaw >= 2 ? (FreeYaw > 1 ? _ok : _costly) : _bad); // 6
                _colorList.Add(_ok); // 7
            }
            else
                // everything else is not-usable
                for (int _b = 1; _b <= 7; _b++)
                    _colorList.Add(_bad);

            // apply brush wheel
            var _index = (8 + (LookHeading - (MoveHeading ?? LookHeading))) % 8;
            Func<int> _colorIndex = () =>
            {
                // each call steps the color index forward (and rolls over if necessary)
                var _ret = _index;
                _index = (_index + 1) % 8;
                return _ret;
            };

            rect0.Fill = _colorList[_colorIndex()];
            rect1.Fill = _colorList[_colorIndex()];
            rect2.Fill = _colorList[_colorIndex()];
            rect3.Fill = _colorList[_colorIndex()];
            rect4.Fill = _colorList[_colorIndex()];
            rect5.Fill = _colorList[_colorIndex()];
            rect6.Fill = _colorList[_colorIndex()];
            rect7.Fill = _colorList[_colorIndex()];

            // apply incline
            int _incline = (int)Incline;
            int _span = (_incline > -3) && (_incline < 3) ? _incline + 3 : 3;
            Grid.SetRowSpan(rectDip, _span);
        }
        #endregion

        private static void OnHeadingsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            var _headings = dependencyObject as Headings;
            _headings.RefreshGrid();
        }

        public static readonly DependencyProperty FreeYawProperty =
            DependencyProperty.Register(@"FreeYaw", typeof(int), typeof(Headings),
            new UIPropertyMetadata(1, OnHeadingsChanged));

        public static readonly DependencyProperty MaxYawProperty =
            DependencyProperty.Register(@"MaxYaw", typeof(int), typeof(Headings),
            new UIPropertyMetadata(1, OnHeadingsChanged));

        public static readonly DependencyProperty InclineProperty =
            DependencyProperty.Register(@"Incline", typeof(double), typeof(Headings),
            new UIPropertyMetadata(0d, OnHeadingsChanged));

        public static readonly DependencyProperty YawGapProperty =
            DependencyProperty.Register(@"YawGap", typeof(double), typeof(Headings),
            new UIPropertyMetadata(0d, OnHeadingsChanged));

        public static readonly DependencyProperty DistanceSinceTurnProperty =
            DependencyProperty.Register(@"DistanceSinceTurn", typeof(double),
            typeof(Headings), new UIPropertyMetadata(0d, OnHeadingsChanged));

        public static readonly DependencyProperty MoveHeadingProperty =
            DependencyProperty.Register(@"MoveHeading", typeof(int?),
            typeof(Headings), new UIPropertyMetadata(0, OnHeadingsChanged));

        public static readonly DependencyProperty LookHeadingProperty =
            DependencyProperty.Register(@"LookHeading", typeof(int),
            typeof(Headings), new UIPropertyMetadata(0, OnHeadingsChanged));

    }
}
