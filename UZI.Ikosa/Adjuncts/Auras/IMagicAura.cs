using Uzi.Ikosa.Magic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IMagicAura: IAura
    {
        AuraStrength MagicStrength { get; }
        MagicStyle MagicStyle { get; }

        /// <summary>This is not intended for divination result</summary>
        int PowerLevel { get; }
        /// <summary>This is not intended for divination result</summary>
        int CasterLevel { get; }
    }
}
