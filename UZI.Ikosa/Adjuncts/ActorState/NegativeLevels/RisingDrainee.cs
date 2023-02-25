using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Templates;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Fidelity;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{

    [Serializable]
    public class RisingDrainee : Adjunct, ITrackTime
    {
        public RisingDrainee(Type riserSpecies, double time)
            : base(riserSpecies)
        {
            _Time = time;
        }

        #region data
        private double _Time;
        #endregion

        public Type RiserSpecies => Source as Type;
        public double Time => _Time;
        public double Resolution => Hour.UnitFactor;

        public override object Clone()
            => new RisingDrainee(RiserSpecies, Time);

        #region public void TrackTime(double timeVal, TimeValTransition direction)
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= Time)
            {
                // if time expired and still dead, rise as species...
                if ((Anchor as Creature)?.HasActiveAdjunct<DeadEffect>() ?? false)
                {
                    // make a new species
                    var _critter = Anchor as Creature;
                    Species _riser = null;
                    try { _riser = Activator.CreateInstance(RiserSpecies, _critter) as Species; }
                    catch { }

                    // successful
                    if ((_riser as IReplaceCreature)?.CanGenerate ?? true)
                    {
                        // start the binding process
                        var _replace = new Creature(_riser.Name, _riser.DefaultAbilities());
                        _riser.BindTo(_replace);
                        _replace.Devotion = new Devotion(@"Death Magic");

                        // place riser in environment, remove old creature...
                        if (Anchor?.GetLocated()?.Locator is ObjectPresenter _presenter)
                        {
                            var _newPresenter = new ObjectPresenter(_replace, _presenter.MapContext, _presenter.ModelKey,
                                _presenter.NormalSize, _presenter.GeometricRegion);
                            _presenter.MapContext.Remove(_presenter);
                        }
                    }
                }

                // eject when time elapsed regardless of rising condition
                Eject();
            }
        }
        #endregion
    }
}
