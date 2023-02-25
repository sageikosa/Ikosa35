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
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using System.ComponentModel;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Workshop
{
    /// <summary>
    /// Interaction logic for LocatorEditorControl.xaml
    /// </summary>
    public partial class LocatorEditorControl : UserControl, INotifyPropertyChanged, IHostedTabItem
    {
        #region construction
        public LocatorEditorControl(Locator locator)
        {
            InitializeComponent();
            _Locator = locator;
            DataContext = this;
            _Locator.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_Locator_PropertyChanged);

            if (_Locator is ObjectPresenter)
            {
                brdPresenter.Visibility = Visibility.Visible;
            }
        }
        #endregion

        public void CloseTabItem()
        {
            _Locator.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_Locator_PropertyChanged);
        }

        private Locator _Locator;
        public Locator Locator { get { return _Locator; } }

        public ObjectPresenter ObjectPresenter { get { return _Locator as ObjectPresenter; } }

        private CellLocation _Relative = new CellLocation(0, 0, 0);

        #region void _Locator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        void _Locator_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(@"CurrentCube") && (PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(@"ZHeight"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"YLength"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"XLength"));
            }
            else if (e.PropertyName.Equals(@"ModelScaleSize") && (PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(@"ModelZHeight"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"ModelYLength"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"ModelXLength"));
            }
            else if (e.PropertyName.Equals(@"NormalSize") && (PropertyChanged != null))
            {
                PropertyChanged(this, new PropertyChangedEventArgs(@"NormalZHeight"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"NormalYLength"));
                PropertyChanged(this, new PropertyChangedEventArgs(@"NormalXLength"));
            }
        }
        #endregion

        #region Int32 text field validation
        private void txtInt_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!int.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region Size double text field validation
        private void txtDblSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            var _txt = sender as TextBox;
            if (!double.TryParse(_txt.Text, out var _out))
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Not a number";
                return;
            }

            if (_out <= 0)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Negatives not allowed for extents";
                return;
            }

            if (_out > 10)
            {
                _txt.Tag = @"Invalid";
                _txt.ToolTip = @"Extent cannot exceed 10 for locators";
                return;
            }

            _txt.Tag = null;
            _txt.ToolTip = null;
        }
        #endregion

        #region locator position
        public int Z
        {
            get { return _Locator.GeometricRegion.LowerZ - _Relative.Z; }
            set
            {
                _Locator.Relocate(new Cubic(
                    new CellLocation(value + _Relative.Z, Y + _Relative.Y, X + _Relative.X),
                    _Locator.NormalSize), _Locator.PlanarPresence);
            }
        }

        public int Y
        {
            get { return _Locator.GeometricRegion.LowerY - _Relative.Y; }
            set
            {
                _Locator.Relocate(new Cubic(
                    new CellLocation(Z + _Relative.Z, value + _Relative.Y, X + _Relative.X),
                    _Locator.NormalSize), _Locator.PlanarPresence);
            }
        }

        public int X
        {
            get { return _Locator.GeometricRegion.LowerX - _Relative.X; }
            set
            {
                _Locator.Relocate(new Cubic(
                    new CellLocation(Z + _Relative.Z, Y + _Relative.Y, value + _Relative.X),
                    _Locator.NormalSize), _Locator.PlanarPresence);
            }
        }
        #endregion

        #region current cubic extents
        public double ZHeight
        {
            get { return _Locator.GeometricRegion.UpperZ - _Locator.GeometricRegion.LowerZ + 1; }
            set
            {
                if ((value > 0) && (value <= 10))
                {
                    _Locator.Relocate(new Cubic(new CellPosition(Z + _Relative.Z, Y + _Relative.Y, X + _Relative.X),
                        new GeometricSize(value, YLength, XLength)), _Locator.PlanarPresence);
                }
            }
        }

        public double YLength
        {
            get { return _Locator.GeometricRegion.UpperY - _Locator.GeometricRegion.LowerY + 1; }
            set
            {
                if ((value > 0) && (value <= 10))
                {
                    _Locator.Relocate(new Cubic(new CellPosition(Z + _Relative.Z, Y + _Relative.Y, X + _Relative.X),
                        new GeometricSize(ZHeight, value, YLength)), _Locator.PlanarPresence);
                }
            }
        }

        public double XLength
        {
            get { return _Locator.GeometricRegion.UpperX - _Locator.GeometricRegion.LowerX + 1; }
            set
            {
                if ((value > 0) && (value <= 10))
                {
                    _Locator.Relocate(new Cubic(new CellPosition(Z + _Relative.Z, Y + _Relative.Y, X + _Relative.X),
                        new GeometricSize(ZHeight, YLength, value)), _Locator.PlanarPresence);
                }
            }
        }
        #endregion

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            Locator.Relocate
                (new Cubic(new CellPosition(Z + _Relative.Z, Y + _Relative.Y, X + _Relative.X), Locator.NormalSize), 
                _Locator.PlanarPresence);
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region IAwarenessLevels Members
        public AwarenessLevel GetAwarenessLevel(Guid guid) { return AwarenessLevel.Aware; }
        public bool ShouldDraw(Guid guid) { return true; }
        #endregion

        #region IZoomIcons Members

        public double ZoomLevel
        {
            get
            {
                return 1d;
            }
            set
            {
            }
        }

        public double UnZoomLevel
        {
            get
            {
                return 0.3d;
            }
            set
            {
            }
        }

        public Guid ZoomedIcon
        {
            get
            {
                return Guid.Empty;
            }
            set
            {
            }
        }

        #endregion
    }
}
