using System;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Note, this should not block targetting, just prevent delivery
    /// </summary>
    [Serializable]
    public class LivingCreatureTargetType : TargetType
    {
        public override bool ValidTarget(Uzi.Core.ICore iCore)
        {
            // TODO: validate
            return true;
        }

        public override TargetTypeInfo ToTargetTypeInfo()
            => new LivingCreatureTargetTypeInfo();
    }
}
