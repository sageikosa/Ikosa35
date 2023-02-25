using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Magic
{
    public interface IMagicPowerSource : IPowerSource, IMagicAura
    {
        IMagicPowerDef MagicPowerDef { get; }
    }
}
