using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Actions
{
    public interface IPowerSource : ICore
    {
        IPowerClass PowerClass { get; }
        IPowerDef PowerDef { get; }
        int PowerLevel { get; }
    }
}
