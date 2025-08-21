using System.Collections.Generic;
using Uzi.Ikosa.TypeListers;

namespace Uzi.Ikosa
{
    /// <summary>Lists proficient weapon types derived from a specific base weapon</summary>
    public class ProficientDerivedWeaponsLister<BaseWeapon> : IFeatParameterProvider
    {
        #region IFeatParameterProvider Members

        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (KeyValuePair<string, ItemTypeListItem> _kvp in WeaponTypeLister.ProficientWeapons(creature, powerDie))
            {
                if (typeof(BaseWeapon).IsAssignableFrom(_kvp.Value.ItemType))
                {
                    yield return new FeatParameter(target, _kvp.Value.ItemType, _kvp.Value.Info.Name, _kvp.Value.Info.Description, powerDie);
                }
            }
            yield break;
        }

        #endregion
    }
}
