using Newtonsoft.Json;
using System;
using System.Linq;
using Uzi.Visualize;

namespace Uzi.Core
{
    /// <summary>Defines geometry based on a geometry builder anchored at an adjustable location</summary>
    [Serializable]
    public class Geometry : IMonitorChange<ICellLocation>, IControlChange<IGeometricRegion>
    {
        #region construction
        /// <summary>Defines geometry based on a geometry builder anchored at an adjustable location</summary>
        public Geometry(IGeometryBuilder builder, IGeometryAnchorSupplier supplier, bool snapshot)
        {
            // invariants
            _Builder = builder;
            _Supplier = supplier;
            if (!snapshot)
            {
                _Supplier.AddChangeMonitor(this);
            }

            // variants (but not under our control
            _Region = _Builder.BuildGeometry(supplier.LocationAimMode, supplier.Location);
            if (!snapshot)
            {
                _RCtrl = new ChangeController<IGeometricRegion>(this, _Region);
            }
        }
        #endregion

        #region data
        private IGeometryBuilder _Builder;
        private IGeometryAnchorSupplier _Supplier;
        private ICellLocation _Location;
        private IGeometricRegion _Region;
        private ChangeController<IGeometricRegion> _RCtrl;
        #endregion

        public IGeometryBuilder Builder => _Builder;
        public IGeometryAnchorSupplier Supplier => _Supplier;
        public ICellLocation Location => _Location;
        public IGeometricRegion Region => _Region;

        public void RecalcGeometry()
        {
            _Region = Builder.BuildGeometry(_Supplier.LocationAimMode, _Supplier.Location);
            _RCtrl?.DoValueChanged(_Region);
        }

        #region IMonitorChange<ICellLocation> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<ICellLocation> args)
        {
            if (_RCtrl?.AllMonitors().Any() ?? false)
            {
                // prevent calculating geometry if nothing's listening
                var _abort = _RCtrl?.WillAbortChange(Builder.BuildGeometry(_Supplier.LocationAimMode, args.NewValue)) ?? false;
                if (_abort)
                {
                    args.DoAbort(@"Geometric Abort", this);
                }
            }
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<ICellLocation> args)
        {
            if (_RCtrl?.AllMonitors().Any() ?? false)
            {
                // prevent calculating geometry if nothing's listening
                _RCtrl?.DoPreValueChanged(Builder.BuildGeometry(_Supplier.LocationAimMode, args.NewValue));
            }
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<ICellLocation> args)
        {
            // definitely changing, so lock-in new values
            _Region = Builder.BuildGeometry(_Supplier.LocationAimMode, _Supplier.Location);
            _RCtrl?.DoValueChanged(_Region);
        }

        #endregion

        #region IControlChange<IGeometricRegion> Members

        public void AddChangeMonitor(IMonitorChange<IGeometricRegion> monitor)
        {
            _RCtrl?.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<IGeometricRegion> monitor)
        {
            _RCtrl?.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
