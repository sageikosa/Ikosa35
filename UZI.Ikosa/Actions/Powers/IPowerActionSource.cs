using Uzi.Core;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerActionSource : IPowerSource, INamedActionSource
    {
        IPowerActionDef PowerActionDef { get; }
        void UsePower();
    }
}
