using System;
using Uzi.Core;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Note, this should not block targetting, just prevent delivery
    /// </summary>
    /// <typeparam name="CritterType"></typeparam>
    [Serializable]
    public class CreatureTypeTargetType<CritterType> : CreatureTargetType
        where CritterType : CreatureType
    {
        public override bool ValidTarget(ICore iCore)
        {
            // TODO: check that creature matches type
            return base.ValidTarget(iCore);
        }
    }
}
