using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Core;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Objects
{
    public static class ObjectInfoFactory
    {
        private static IEnumerable<Info> GetAdjunctInfo(IItemBase itemBase)
        {
            return (from _adj in itemBase.Adjuncts.OfType<IIdentification>()
                    from _info in _adj.IdentificationInfos
                    select _info);
        }

        public static OInfo CreateInfo<OInfo>(CoreActor actor, IObjectBase objBase)
            where OInfo : ObjectInfo, new()
        {
            var _critter = actor as Creature;
            var _contains = ((_critter == null) || _critter.ObjectLoad.Contains(objBase));
            var _info = new OInfo()
            {
                ID = objBase.ID,
                Message = objBase.Name,
                Size = objBase.Sizer.Size.ToSizeInfo(),
                Hardness = new DeltableInfo(objBase.Hardness),
                Material = objBase.ObjectMaterial.ToMaterialInfo(),
                CreatureSize = Size.Medium.ToSizeInfo(),
                AdjunctInfos = new Info[] { },
                Weight = _contains ? objBase.Weight : (double?)null,
                StructurePercent =
                    _contains
                    ? ((objBase.MaxStructurePoints == 0)
                        ? 1d
                        : ((double)objBase.StructurePoints) / objBase.MaxStructurePoints)
                    : (double?)null,
                Icon = new ImageryInfo
                {
                    Keys = objBase.IconKeys.ToArray(),
                    IconRef = new IconReferenceInfo
                    {
                        IconAngle = objBase.IconAngle,
                        IconScale = objBase.IconScale,
                        IconColorMap = objBase.IconColorMap
                    }
                }
            };
            return _info;
        }

        public static OInfo CreateInfo<OInfo>(CoreActor actor, IItemBase itemBase, bool baseValues)
            where OInfo : ObjectInfo, new()
        {
            var _critter = actor as Creature;
            var _contains = (_critter == null)
                || _critter.ObjectLoad.Contains(itemBase)
                || _critter.Possessions.Contains(itemBase);
            var _info = new OInfo()
            {
                ID = itemBase.ID,
                Size = itemBase.Sizer.Size.ToSizeInfo(),
                CreatureSize = itemBase.ItemSizer.EffectiveCreatureSize.ToSizeInfo(),
                Hardness = baseValues ? new DeltableInfo(itemBase.Hardness.BaseValue) : itemBase.Hardness.ToDeltableInfo(),
                Material = itemBase.ItemMaterial.ToMaterialInfo(),
                Icon = new ImageryInfo
                {
                    Keys = itemBase.IconKeys.ToArray(),
                    IconRef= new IconReferenceInfo
                    {
                        IconAngle = itemBase.IconAngle,
                        IconScale = itemBase.IconScale,
                        IconColorMap = itemBase.IconColorMap
                    }
                },
                AdjunctInfos =
                    !baseValues
                    ? GetAdjunctInfo(itemBase).ToArray()
                    : new Info[] { },
                Message =
                    !baseValues
                    ? itemBase.Name
                    : itemBase.OriginalName,
                Weight = _contains ? itemBase.Weight : (double?)null,
                StructurePercent =
                    _contains
                    ? ((itemBase.MaxStructurePoints.EffectiveValue == 0)
                        ? 1d
                        : ((double)itemBase.StructurePoints) / itemBase.MaxStructurePoints.EffectiveValue)
                    : (double?)null
            };
            return _info;
        }
    }
}
