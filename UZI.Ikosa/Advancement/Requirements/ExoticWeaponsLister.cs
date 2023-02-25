using System.Collections.Generic;
using Uzi.Ikosa.Universal;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa
{
    public class ExoticWeaponsLister : IFeatParameterProvider
    {
        #region IFeatParameterProvider Members

        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, 
            Creature creature, int powerDie)
        {
            foreach (var _item in Campaign.SystemCampaign.ExoticWeapons)
            {
                // must be treated as an exotic weapon
                if (creature.Proficiencies.WeaponTreatment(_item.Value.ItemType, powerDie) == WeaponProficiencyType.Exotic)
                {
                    // creature must not have proficiency already
                    if (!creature.Proficiencies.IsProficientWithWeapon(_item.Value.ItemType, powerDie))
                        yield return new FeatParameter(target, _item.Value.ItemType, _item.Value.Info.Name, _item.Value.Info.Description, powerDie);
                }
            }
            yield break;
        }

        #endregion
    }
}
