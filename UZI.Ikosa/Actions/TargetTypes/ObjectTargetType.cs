using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Object only
    /// </summary>
    [Serializable]
    public class ObjectTargetType : TargetType
    {
        public override bool ValidTarget(ICore iCore)
        {
            // TODO: check if the iCore is an item or object
            return true;
        }

        public override TargetTypeInfo ToTargetTypeInfo()
            => new ObjectTargetTypeInfo();
    }
}
