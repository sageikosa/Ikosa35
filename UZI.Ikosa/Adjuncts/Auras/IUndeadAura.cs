using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IUndeadAura : IAura
    {
        AuraStrength Strength { get; }
        int PowerLevel { get; }
    }
}
