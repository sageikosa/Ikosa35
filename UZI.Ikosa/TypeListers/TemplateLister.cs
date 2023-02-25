using System.Collections.Generic;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.TypeListers
{
    public static class TemplateLister
    {
        public static IEnumerable<TypeListItem> InheritedTemplates()
        {
            if (Campaign.SystemCampaign.TemplateLists.ContainsKey(@"Inherited"))
            {
                foreach (KeyValuePair<string, TypeListItem> _tli in Campaign.SystemCampaign.TemplateLists[@"Inherited"])
                {
                    yield return _tli.Value;
                }
            }
            else
            {
                foreach (KeyValuePair<string, TypeListItem> _tli in Campaign.SystemCampaign.TemplateLists[@"All"])
                {
                    yield return _tli.Value;
                }
            }
            yield break;
        }

    }
}
