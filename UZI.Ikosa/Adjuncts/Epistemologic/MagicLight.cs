using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class MagicLight : Illumination, ILocatorZone
    {
        public MagicLight(object source, double brightRange, double shadowyRange, bool veryBright)
            : base(source, brightRange, shadowyRange, veryBright)
        {
            _Capture = null;
        }

        private LocatorCapture _Capture;

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

        // public override void PathChanged()
        public override void PathChanged(Pathed source)
        {
            if (source is Located)
            {
                RefreshZone();
            }
            base.PathChanged(source);
        }

        private void RefreshZone()
        {
            // see if we are still locatable
            var _loc = Anchor.GetLocated()?.Locator;
            if (_loc != null)
            {
                if (_Capture != null)
                {
                    // remove old capture
                    _Capture?.RemoveZone();
                }

                // define new capture
                _Capture = new LocatorCapture(_loc.MapContext, this,
                    new Geometry(new SphereBuilder(Convert.ToInt32(BrightRange / 5)), _loc, false), _loc,
                    this, true, _loc.PlanarPresence);
            }
            else
            {
                RemoveZone();
            }
        }

        private void RemoveZone()
        {
            // remove capture
            _Capture?.RemoveZone();
            _Capture = null;
        }

        public override object Clone()
            => new MagicLight(Source, BrightRange, ShadowyRange, VeryBrightRange > 0);

        #region ILocatorZone Members

        public void Start(Locator locator) => Capture(locator);
        public void End(Locator locator) => Release(locator);
        public void Enter(Locator locator) => Capture(locator);
        public void Exit(Locator locator) => Release(locator);

        public void Capture(Locator locator)
        {
            foreach (var _obj in locator.ICoreAs<ICoreObject>())
            {
                if (!_obj.Adjuncts.OfType<LightBathed>().Any(_a => _a.Source == Source))
                {
                    _obj.AddAdjunct(new LightBathed(Source));
                }
            }
        }

        public void Release(Locator locator)
        {
            foreach (var _adj in (from _o in locator.ICoreAs<ICoreObject>()
                                  from _a in _o.Adjuncts.OfType<LightBathed>()
                                  where _a.Source == Source
                                  select _a).ToList())
            {
                _adj.Eject();
            }
        }

        public void MoveInArea(Locator locator, bool followOn) { }

        #endregion
    }
}
