using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Treasure trove</summary>
    [Serializable]
    public class Trove : ContainerObject, IMonitorChange<Size>, ICapturePassthrough
    {
        // TODO: ICoreIconic?

        /// <summary>Treasure trove</summary>
        public Trove(string name)
            : base(name, VoidMaterial.Static, true, true)
        {
            ObjectSizer.NaturalSize = Size.Small;
        }

        public override bool IsTargetable => false;

        public IEnumerable<ICore> Contents
            => Objects
            .OrderByDescending(_c => _c.Length * _c.Width * _c.Height)
            .ToList();

        #region public override IGeometricSize GeometricSize { get; }
        public override IGeometricSize GeometricSize
        {
            get
            {
                var _z = 0d;
                var _y = 0d;
                var _x = 0d;
                foreach (var _sz in Objects.OfType<ISizable>())
                {
                    var _gs = _sz.GeometricSize;
                    if (_gs.ZExtent > _z)
                        _z = _gs.ZExtent;
                    if (_gs.YExtent > _y)
                        _y = _gs.YExtent;
                    if (_gs.XExtent > _x)
                        _x = _gs.XExtent;
                }
                return new GeometricSize(_z, _y, _x);
            }
        }
        #endregion

        #region private void ResyncSize()
        private void ResyncSize()
        {
            var _geomSize = GeometricSize;

            // alter locator when size changes
            var _loc = this.GetLocated()?.Locator;
            if (_loc != null)
            {
                // relocate if size changed
                var _geom = _loc.GeometricRegion;
                var _region = new Cubic(new CellPosition(_geom.LowerZ, _geom.LowerY, _geom.LowerX), _geomSize);
                // TODO: adjust region if unable to occupy cells...
                _loc.Relocate(_region, _loc.PlanarPresence);
            }
        }
        #endregion

        #region Add
        public override void Add(Core.ICoreObject item)
        {
            base.Add(item);
            if (item is ISizable)
            {
                (item as ISizable).Sizer.AddChangeMonitor(this);
                ResyncSize();
                this.IncreaseSerialState();
            }
        }
        #endregion

        #region Remove
        public override bool Remove(Core.ICoreObject item)
        {
            var (_map, _notifiers) = this.GetLocated()?.Locator?.GetDeferredRefreshObservers() ?? (null, null);
            var _removed = base.Remove(item);
            if (_removed && (Count == 0))
            {
                // if trove is empty, evaporate it...
                this.UnPath();
                this.UnGroup();
            }
            else if (item is ISizable)
            {
                (item as ISizable).Sizer.RemoveChangeMonitor(this);
                ResyncSize();
            }
            if (_map != null)
            {
                AwarenessSet.RecalculateAllSensors(_map, _notifiers, false);
            }
            this.IncreaseSerialState();
            return _removed;
        }
        #endregion

        public override IEnumerable<string> IconKeys
        {
            get
            {
                var _obj = Contents.OfType<ICoreIconic>().FirstOrDefault();
                if (_obj != null)
                    return _obj.IconKeys;
                return new string[] { };
            }
        }

        public override IEnumerable<string> PresentationKeys { get { yield break; } }

        public override double MaximumLoadWeight
        {
            get { return double.MaxValue; }
            set { /* NO maximum for a loose trove */ }
        }

        public override double TareWeight
        {
            get { return 0d; }
            set { /* NO tare weight for a loose trove */ }
        }

        public override bool AddsToLoad
        {
            get { return true; }
            set { /* Always adds to Load */ }
        }

        protected override void InitInteractionHandlers()
        {
            AddIInteractHandler(new BlockInteraction<PickUp>());
            // TODO: block PushAround???
            base.InitInteractionHandlers();
        }

        public override double FallReduce
            => Contents.OfType<IStructureDamage>().FirstOrDefault()?.FallReduce ?? 0d;

        public override IEnumerable<Core.CoreAction> GetTacticalActions(Core.CoreActionBudget budget)
        {
            // TODO: action to cross-load into another container (transfer contents in bulk)
            foreach (var _act in base.GetTacticalActions(budget))
            {
                yield return _act;
            }
            yield break;
        }

        #region IMonitorChange<Size> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<Size> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<Size> args)
        {
            ResyncSize();
        }

        #endregion

        public static (Trove trove, Locator fallingLocator, bool ethereal) GetTrove(Drop dropData, LocalMap map, ICoreObject coreObj)
        {
            var _startFall = false;
            Trove _trove = null;
            Locator _locator = null;
            var _ethereal = coreObj.HasActiveAdjunct<EtherealState>();
            if (dropData?.Location is ICellLocation _location)
            {
                if (dropData?.Surface is IObjectBase _surface)
                {
                    #region Get trove on the surface of the object
                    // see if a trove is on the surface
                    _trove = (from _os in _surface.Adjuncts.OfType<OnSurface>()
                              where _os.IsActive
                              from _c in _os.Surface.Contained
                              where (_c.Anchor is Trove _t) && (_t.HasActiveAdjunct<EtherealState>() == _ethereal)
                              select _c.Anchor as Trove).FirstOrDefault();

                    // if not, create one and put on the surface
                    if (_trove == null)
                    {
                        var _container = SurfaceGroup.GetSurfaceContainer(_surface);
                        _trove = new Trove(@"Trove");
                        if (_ethereal)
                        {
                            // object was ethereal, so is trove
                            _trove.AddAdjunct(new EtherealEffect(typeof(Trove), null));
                        }

                        // locate the trove
                        var _size = _trove.Sizer.Size.CubeSize();
                        _locator = new ObjectPresenter(_trove, map.MapContext, _size,
                            new Cubic(_location, _size), map.Resources);

                        // and ensure it is one the surface
                        _trove.AddAdjunct(new OnSurface(_container.Surface));
                    }
                    #endregion
                }

                #region Get trove at location
                if (_trove == null)
                {
                    // see if a trove is at the location
                    _trove = (from _l in map.MapContext.LocatorsInCell(_location, _ethereal ? PlanarPresence.Ethereal : PlanarPresence.Material)
                              where _l.ICore is Trove
                              select _l.ICore as Trove).FirstOrDefault();
                }

                // if not create one
                if (_trove == null)
                {
                    _trove = new Trove(@"Trove");
                    _startFall = true;
                    if (_ethereal)
                    {
                        // drop target was ethereal, so is the trove
                        _trove.AddAdjunct(new EtherealEffect(typeof(Trove), null));
                    }

                    // locate the trove
                    var _size = _trove.Sizer.Size.CubeSize();
                    _locator = new ObjectPresenter(_trove, map.MapContext, _size,
                        new Cubic(_location, _size), map.Resources);
                }
                #endregion
            }

            return (_trove, (_startFall && !_ethereal ? _locator : null), _ethereal);
        }
    }
}