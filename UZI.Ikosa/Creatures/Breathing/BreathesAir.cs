using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Creatures
{
    [Serializable]
    public class BreathesAir : Breathes
    {
        public BreathesAir(object source)
            : base(source)
        {
        }

        public override object Clone()
        {
            return new BreathesAir(Source);
        }

        #region public override bool CanBreathe()
        public override bool CanBreathe()
        {
            if (Anchor is Creature _critter)
            {
                var _located = _critter.GetLocated();
                if ((_critter.Setting is LocalMap _map) && (_located != null))
                {
                    // test the region for the locator
                    var _locator = _located.Locator;
                    if (_locator.PlanarPresence == PlanarPresence.Ethereal)
                    {
                        // pure ethereal doesn't need to worry about breathing
                        return true;
                    }

                    var _region = _locator.GeometricRegion;

                    // extend region if successfully swimming
                    if (_critter.HasActiveAdjunct<Swimming>())
                    {
                        // all cells in region are testable
                        var _cells = _region.AllCellLocations().ToList();

                        // include any cell adjacent to a surface cell in the up direction
                        var _up = _locator.GetGravityFace().ReverseFace();
                        _cells.AddRange(from _cl in _region.AllCellLocations()
                                        where _region.IsCellAtSurface(_cl, _up)
                                        select CellLocation.GetAdjacentCellLocation(_cl, _up));

                        // make a cell list region out of this
                        _region = new CellList(_cells, _locator.ZFit, _locator.YFit, _locator.XFit);
                    }

                    // then check if the region has breathable air...
                    var _check = (from _cell in _region.AllCellLocations()
                                  let _space = _map[_cell]
                                  where _space != null
                                  select _space).Any(_s => _s.SuppliesBreathableAir);
                    return _check;
                }
            }
            return false;
        }
        #endregion

        public static TraitBase GetTrait(Species species)
            => new ExtraordinaryTrait(species, @"Breathes Air", @"Can drown without air",
                TraitCategory.Quality, new AdjunctTrait(species, new BreathesAir(species)));
    }
}
