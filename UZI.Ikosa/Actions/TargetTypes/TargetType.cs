using System;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public abstract class TargetType : ITargetType
    {
        // TODO: define filter to check object awarenesses
        // TODO: define expander to see if an object might have parts to use
        public abstract bool ValidTarget(ICore iCore);

        public abstract TargetTypeInfo ToTargetTypeInfo();
    }
}
