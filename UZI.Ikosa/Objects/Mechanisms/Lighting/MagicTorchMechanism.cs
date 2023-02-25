using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement.CharacterClasses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Magic.Spells;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MagicTorchMechanism : Mechanism
    {
        public MagicTorchMechanism(string name, Material material, int disableDifficulty)
            : base(name, material, disableDifficulty)
        {
            var _caster = new ItemCaster(MagicType.Arcane, 3, Alignment.TrueNeutral, 13, Guid.Empty, typeof(Wizard));

            // create a magicSource for a permanent torch (using specified casting parameters)
            var _permaTorch = new PermanentTorch();
            var _magicSource = new SpellSource(_caster, 2, 2, false, _permaTorch);

            // create a magicPowerEffect from the magicSource
            var _mode = _permaTorch.SpellModes.FirstOrDefault();
            var _magicPower = new MagicPowerEffect(_magicSource, _mode, new PowerAffectTracker(), 0);

            // add the magicPowerEffect to the torch
            AddAdjunct(_magicPower);
        }

        public override IEnumerable<IActivatable> Dependents { get { yield break; } }

        protected override string ClassIconKey
            => @"torch";

        protected override void OnActivate()
        {
            var _power = Adjuncts.OfType<MagicPowerEffect>().FirstOrDefault();
            if (_power != null)
            {
                _power.Activation = new Activation(this, true);
            }
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            var _power = Adjuncts.OfType<MagicPowerEffect>().FirstOrDefault();
            if (_power != null)
            {
                _power.Activation = new Activation(this, false);
            }
            base.OnDeactivate();
        }
    }
}
