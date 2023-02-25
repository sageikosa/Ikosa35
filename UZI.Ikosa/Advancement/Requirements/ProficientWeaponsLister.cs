using System.Collections.Generic;
using Uzi.Ikosa.TypeListers;

namespace Uzi.Ikosa
{
    /// <summary>Lists proficient weapon types</summary>
    public class ProficientWeaponsLister : IFeatParameterProvider
    {
        public IEnumerable<FeatParameter> AvailableParameters(ParameterizedFeatListItem target, Creature creature, int powerDie)
        {
            foreach (KeyValuePair<string, ItemTypeListItem> _kvp in WeaponTypeLister.ProficientWeapons(creature, powerDie))
            {
                yield return new FeatParameter(target, _kvp.Value.ItemType, _kvp.Value.Info.Name, _kvp.Value.Info.Description, powerDie);
            }
            yield break;
        }
    }
}
