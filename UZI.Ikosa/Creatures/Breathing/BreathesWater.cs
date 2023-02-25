using System;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Creatures
{
    [Serializable]
    public class BreathesWater : Breathes
    {
        public BreathesWater(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new BreathesWater(Source);
        }

        #region public override bool CanBreathe()
        public override bool CanBreathe()
        {
            var _critter = Anchor as Creature;
            if (_critter != null)
            {
                var _map = _critter.Setting as LocalMap;
                var _located = _critter.GetLocated();
                if ((_map != null) && (_located != null))
                {
                    var _locator = _located.Locator;
                    if (_locator.PlanarPresence == PlanarPresence.Ethereal)
                    {
                        // pure ethereal doesn't need to worry about breathing
                        return true;
                    }
                    return (from _cell in _locator.GeometricRegion.AllCellLocations()
                            let _space = _map[_cell]
                            where _space != null
                            select _space).Any(_s => _s.SuppliesBreathableWater);
                }
            }
            return false;
        }
        #endregion
    }
}
