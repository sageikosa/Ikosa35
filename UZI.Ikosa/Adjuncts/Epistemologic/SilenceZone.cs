using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class SilenceZone : Adjunct, ILocatorZone, IAlterInteraction, IPathDependent
    {
        public SilenceZone(object source, IGeometryBuilder geometryBuilder)
            : base(source)
        {
            _Builder = geometryBuilder;
            _ICapture = null;
            _LocCapture = null;
        }

        #region state
        protected readonly IGeometryBuilder _Builder;
        protected InteractCapture _ICapture;
        protected LocatorCapture _LocCapture;
        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            RefreshZone();
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            RemoveZone();
        }

        public IGeometryBuilder GeometryBuilder => _Builder;
        public InteractCapture InteractCapture => _ICapture;
        public LocatorCapture LocatorCapture => _LocCapture;

        public override object Clone()
            => new SilenceZone(Source, _Builder);

        #region ILocatorZone Members

        public void Start(Locator locator) { Capture(locator); }
        public void End(Locator locator) { Release(locator); }
        public void Enter(Locator locator) { Capture(locator); }
        public void Exit(Locator locator) { Release(locator); }

        public void Capture(Locator locator)
        {
            if (locator.ICore is ICoreObject _obj)
            {
                if (!_obj.Adjuncts.OfType<Silenced>().Any(_s => _s.Source == Source))
                {
                    _obj.AddAdjunct(new Silenced(Source));
                }
            }
        }

        public void Release(Locator locator)
        {
            if (locator.ICore is ICoreObject _obj)
            {
                foreach (var _adj in (from _a in _obj.Adjuncts.OfType<Silenced>()
                                      where _a.Source == Source
                                      select _a).ToList())
                {
                    _adj.Eject();
                }
            }
        }

        public void MoveInArea(Locator locator, bool followOn) { }

        #endregion

        // IPathDependent
        public void PathChanged(Pathed source)
        {
            // see if we are still locatable
            if (source is Located)
            {
                RefreshZone();
            }
        }

        private void RefreshZone()
        {
            var _loc = Anchor.GetLocated()?.Locator;
            if (_loc != null)
            {
                // remove old captures
                _ICapture?.RemoveZone();
                _LocCapture?.RemoveZone();

                // define new captures
                var _builder = new Geometry(GeometryBuilder, _loc, false);
                _ICapture = new InteractCapture(_loc.MapContext, this,
                    _builder, this, true, _loc.PlanarPresence);
                _LocCapture = new LocatorCapture(_loc.MapContext, this,
                    _builder, _loc, this, true, _loc.PlanarPresence);
            }
            else
            {
                RemoveZone();
            }
        }

        private void RemoveZone()
        {
            // remove captures
            _ICapture?.RemoveZone();
            _ICapture = null;
            _LocCapture?.RemoveZone();
            _LocCapture = null;
        }

        // IAlterInteraction
        public bool CanAlterInteraction(Interaction workSet)
            => workSet?.InteractData is SoundTransit;

        public bool WillDestroyInteraction(Interaction workSet, ITacticalContext context)
            => workSet?.InteractData is SoundTransit;
    }
}
