using System;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Magic.Spells;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Items
{
    [
    Serializable,
    ItemInfo(@"Magic torch", @"Magical item that sheds light", @"magic_torch")
    ]
    public class MagicTorch : ItemBase
    {
        public MagicTorch()
            : this(new ItemCaster(MagicType.Arcane, 3, Alignment.TrueNeutral, 13, Guid.Empty, typeof(Wizard)), 2, 2)
        {
        }

        public MagicTorch(ItemCaster caster, int spellLevel, int slotLevel)
            : base(@"Torch", Size.Tiny)
        {
            Price.CorePrice = 110;
            BaseWeight = 1;
            Name = @"Magic Torch";
            MaxStructurePoints.BaseValue = WoodMaterial.Static.StructurePerInch * 2;

            // create a magicSource for a permanent torch (using specified casting parameters)
            var _permaTorch = new PermanentTorch();
            var _magicSource = new SpellSource(caster, spellLevel, slotLevel, false, _permaTorch);

            // create a magicPowerEffect from the magicSource
            var _mode = _permaTorch.SpellModes.FirstOrDefault();
            var _magicPower = new MagicPowerEffect(_magicSource, _mode, new PowerAffectTracker(), 0);

            // add the magicPowerEffect to the torch
            AddAdjunct(_magicPower);
        }

        protected override string ClassIconKey
            => @"torch";
    }
}
