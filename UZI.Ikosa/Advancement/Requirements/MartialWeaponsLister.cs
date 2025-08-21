using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public class MartialWeaponsLister : IFeatParameterProvider
    {
        #region IFeatParameterProvider Members

        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, 
            Creature creature, int powerDie)
        {
            // union martial and exotic, in case something treats certain exotic weapons as martial weapons
            foreach (var _item in Campaign.SystemCampaign.MartialWeapons.Union(Campaign.SystemCampaign.ExoticWeapons))
            {
                // must be treated as an exotic weapon
                if (creature.Proficiencies.WeaponTreatment(_item.Value.ItemType, powerDie) == WeaponProficiencyType.Martial)
                {
                    // creature must not have proficiency already
                    if (!creature.Proficiencies.IsProficientWithWeapon(_item.Value.ItemType, powerDie))
                    {
                        yield return new FeatParameter(target, _item.Value.ItemType, _item.Value.Info.Name, _item.Value.Info.Description, powerDie);
                    }
                }
            }
            yield break;
        }

        #endregion
    }
}
