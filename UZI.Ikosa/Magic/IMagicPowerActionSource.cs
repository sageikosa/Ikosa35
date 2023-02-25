using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Magic
{
    public interface IMagicPowerActionSource : IMagicPowerSource, IPowerActionSource
    {
        IMagicPowerActionDef MagicPowerActionDef { get; }
    }
}
