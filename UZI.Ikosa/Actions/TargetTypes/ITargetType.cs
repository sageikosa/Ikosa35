using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    public interface ITargetType
    {
        bool ValidTarget(ICore iCore);
        TargetTypeInfo ToTargetTypeInfo();
    }
}
