using System.Collections.Generic;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.TypeListers
{
    public class WeaponTypeLister
    {
        public static IEnumerable<KeyValuePair<string, ItemTypeListItem>> ProficientWeapons(Creature creature, int powerLevel)
        {
            foreach (KeyValuePair<string, ItemTypeListItem> _skvp in Campaign.SystemCampaign.SimpleWeapons)
            {
                if (creature.Proficiencies.IsProficientWithWeapon(_skvp.Value.ItemType, powerLevel))
                {
                    yield return _skvp;
                }
            }
            foreach (KeyValuePair<string, ItemTypeListItem> _mkvp in Campaign.SystemCampaign.MartialWeapons)
            {
                if (creature.Proficiencies.IsProficientWithWeapon(_mkvp.Value.ItemType, powerLevel))
                {
                    yield return _mkvp;
                }
            }
            foreach (KeyValuePair<string, ItemTypeListItem> _xkvp in Campaign.SystemCampaign.ExoticWeapons)
            {
                if (creature.Proficiencies.IsProficientWithWeapon(_xkvp.Value.ItemType, powerLevel))
                {
                    yield return _xkvp;
                }
            }
            yield break;
        }
    }
}
