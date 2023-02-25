using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.TypeListers
{
    public static class LanguageLister
    {
        public static IEnumerable<TypeListItem> AvailableLanguages(Creature creature)
        {
            foreach (var _tli in Campaign.SystemCampaign.Languages)
            {
                if (!creature.Languages.Any(_l => _l.GetType().FullName.Equals(_tli.Key)))
                {
                    yield return _tli.Value;
                }
            }
            yield break;
        }
    }
}
