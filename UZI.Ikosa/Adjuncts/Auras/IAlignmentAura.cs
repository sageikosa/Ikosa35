using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IAlignmentAura : IAura
    {
        Alignment Alignment { get; }
        AuraStrength AlignmentStrength { get; }
        int PowerLevel { get; }
    }
}
