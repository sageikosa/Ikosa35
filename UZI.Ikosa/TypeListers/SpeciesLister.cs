using System.Collections.Generic;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.TypeListers
{
    public class SpeciesLister
    {
        public static IEnumerable<TypeListItem> PlayerCharacterSpecies()
        {
            if (Campaign.SystemCampaign.SpeciesLists.ContainsKey(@"Player"))
            {
                foreach (KeyValuePair<string, TypeListItem> _tli in Campaign.SystemCampaign.SpeciesLists[@"Player"])
                {
                    yield return _tli.Value;
                }
            }
            else
            {
                foreach (KeyValuePair<string, TypeListItem> _tli in Campaign.SystemCampaign.SpeciesLists[@"All"])
                {
                    yield return _tli.Value;
                }
            }
            yield break;
        }

        public static IEnumerable<TypeListItem> AllSpecies()
        {
            foreach (KeyValuePair<string, TypeListItem> _tli in Campaign.SystemCampaign.SpeciesLists[@"All"])
            {
                yield return _tli.Value;
            }
            yield break;
        }
    }
}
