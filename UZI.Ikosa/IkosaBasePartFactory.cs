using System.Collections.Generic;
using Uzi.Packaging;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Services;
using System;
using Uzi.Ikosa.Guildsmanship;
using Uzi.Ikosa.Guildsmanship.Overland;

namespace Uzi.Ikosa
{
    public class IkosaBasePartFactory : IBasePartFactory
    {
        public static void RegisterFactory()
        {
            BasePartHelper.RegisterFactory(new IkosaBasePartFactory());
        }


        public IEnumerable<string> Relationships
        {
            get
            {
                yield return LocalMap.IkosaMapRelation;
                yield return UserDefinitionsPart.UserDefinitionsRelation;
                yield return Module.ModulePartRelation;
                yield return ModuleResources.ModuleResourcesRelation;
                yield return Region.RegionRelation;
                yield return Settlement.SettlementRelation;
                yield return LocalMapSite.LocalMapSiteRelation;
                yield return EncounterTable.EncounterTableRelation;
                yield return CreatureNode.CreatureRelation;
                yield return SitePathGraph.SitePathGraphRelation;
                yield break;
            }
        }

        public IBasePart GetPart(string relationshipType, ICorePartNameManager manager, System.IO.Packaging.PackagePart part, string id)
        {
            switch (relationshipType)
            {
                case LocalMap.IkosaMapRelation:
                    var _map = LocalMap.GetLocalMap(part);
                    _map.NameManager = manager;
                    return _map;

                case UserDefinitionsPart.UserDefinitionsRelation:
                    return new UserDefinitionsPart(manager, part, id);

                case Module.ModulePartRelation:
                    var _module = Module.GetModule(part);
                    _module.NameManager = manager;
                    return _module;

                case ModuleResources.ModuleResourcesRelation:
                    return new ModuleResources(manager, part, id); ;

                case Region.RegionRelation:
                    var _region = Region.GetRegion(part);
                    _region.NameManager = manager;
                    return _region;

                case Settlement.SettlementRelation:
                    var _settlement = Settlement.GetSettlement(part);
                    _settlement.NameManager = manager;
                    return _settlement;

                case LocalMapSite.LocalMapSiteRelation:
                    var _lmSite = LocalMapSite.GetLocalMapSite(part);
                    _lmSite.NameManager = manager;
                    return _lmSite;

                case EncounterTable.EncounterTableRelation:
                    var _eTbl = EncounterTable.GetEncounterTable(part);
                    _eTbl.NameManager = manager;
                    return _eTbl;

                case CreatureNode.CreatureRelation:
                    var _crtr = CreatureNode.GetCreatureNode(part);
                    _crtr.NameManager = manager;
                    return _crtr;

                case SitePathGraph.SitePathGraphRelation:
                    var _spGraph = SitePathGraph.GetSitePathGraph(part);
                    _spGraph.NameManager = manager;
                    return _spGraph;
            }
            return null;
        }

        public Type GetPartType(string relationshipType)
            => relationshipType switch
            {
                LocalMap.IkosaMapRelation => typeof(LocalMap),
                UserDefinitionsPart.UserDefinitionsRelation => typeof(UserDefinitionsPart),
                Module.ModulePartRelation => typeof(Module),
                ModuleResources.ModuleResourcesRelation=> typeof(ModuleResources),
                Region.RegionRelation => typeof(Region),
                Settlement.SettlementRelation => typeof(Settlement),
                LocalMapSite.LocalMapSiteRelation => typeof(LocalMapSite),
                EncounterTable.EncounterTableRelation => typeof(EncounterTable),
                CreatureNode.CreatureRelation => typeof(CreatureNode),
                SitePathGraph.SitePathGraphRelation => typeof(SitePathGraph),
                _ => null,
            };
    }
}
