using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.TypeListers
{
    // TODO: handle class specific listers separately...
    public static class FeatLister
    {
        /// <summary>
        /// Lists feats available to the creature at the specified power die level
        /// </summary>
        public static IEnumerable<FeatListItem> AvailableFeats(Creature creature, object source, int powerDie)
        {
            foreach (var _tli in Campaign.SystemCampaign.FeatLists[@"General"])
            {
                // check attributes (at least)
                var _req = _tli.Value.ListedType.GetCustomAttributes(typeof(RequirementAttribute), true)
                    .OfType<RequirementAttribute>().ToList();
                if (!_req.Any() || _req.All(_r => _r?.MeetsRequirement(creature, powerDie) ?? true))
                {
                    if (!_tli.Value.ListedType.IsGenericType)
                    {
                        // only show non-generic feats we can add
                        FeatBase _feat = null;
                        try
                        {
                            _feat = (FeatBase)Activator.CreateInstance(_tli.Value.ListedType, source, powerDie);
                        }
                        catch (Exception _ex)
                        {
                            Debug.WriteLine($@"Uzi.Ikosa.TypeListers.{nameof(AvailableFeats)}: {_tli.Value.ListedType} --> {_ex.Message}");
                        }
                        if (_feat?.CanAdd(creature) ?? false)
                        {
                            yield return new FeatListItem(_feat);
                        }
                    }
                    else
                    {
                        // generic feat type
                        var _pflItem = new ParameterizedFeatListItem(_tli.Value.ListedType, creature, powerDie);
                        if ((_pflItem.Info != null) && _pflItem.AvailableParameters.Any())
                        {
                            yield return _pflItem;
                        }
                    }
                }
            }
            yield break;
        }
    }
}
