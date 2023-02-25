using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    public interface IMagicPowerDef : IPowerDef
    {
        /// <summary>Evocation, Conjuration, Enchantment, General, Illusion, Necromancy, Divination, Transformation or Abjuration</summary>
        MagicStyle MagicStyle { get; }
        MagicPowerDefInfo ToMagicPowerDefInfo();
    }
}
