using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Any creature [TargetType]
    /// </summary>
    [Serializable]
    public class CreatureTargetType : TargetType
    {
        public override bool ValidTarget(ICore iCore)
        {
            // TODO: check if it is a creature
            return true;
        }

        public override TargetTypeInfo ToTargetTypeInfo()
            => new CreatureTargetTypeInfo();
    }
}
