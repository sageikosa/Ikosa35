using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa
{
    public class NaturalWeaponLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(
            ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (var _natWpnTrait in from _trait in creature.Traits
                                         let _nwt = _trait.Trait as NaturalWeaponTrait
                                         where _nwt != null
                                         select _nwt)
            {
                yield return new FeatParameter(target, _natWpnTrait.Weapon.GetType(),
                    _natWpnTrait.Weapon.Name, _natWpnTrait.Weapon.Name, powerDie);
            }
            yield break;
        }
    }
}
