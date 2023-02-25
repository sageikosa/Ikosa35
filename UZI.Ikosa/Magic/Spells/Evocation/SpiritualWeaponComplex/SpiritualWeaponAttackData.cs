using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class SpiritualWeaponAttackData : MeleeAttackData
    {
        public SpiritualWeaponAttackData(CoreActor actor, Locator locator, AttackImpact impact, Deltable score, bool harmless, ICellLocation source, ICellLocation target, int stepIndex)
            : base(actor, null, locator, impact, score, harmless, source, target, 1, 1)
        {
            _index = stepIndex;
        }

        public SpiritualWeaponAttackData(CoreActor actor, Locator locator, AttackImpact impact, Deltable score, Deltable criticalConfirm, bool harmless, ICellLocation source, ICellLocation target, int stepIndex)
            : base(actor, null, locator, impact, score, criticalConfirm, harmless, source, target, 1, 1)
        {
            _index = stepIndex;
        }

        #region state
        private readonly int _index;
        #endregion

        public int StepIndex => _index;

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (Attacker is Creature _critter)
            {
                foreach (var _delta in _critter.BaseAttack.QualifiedDeltas(qualify, this, @"Spiritual Weapon"))
                {
                    // base attack
                    yield return _delta;
                }

                // wisdom
                yield return _critter.Abilities.Wisdom;
            }
            else
            {
                yield return new QualifyingDelta(0, GetType(), @"Spiritual Weapon");
            }

            // attack sequence decay...
            if (StepIndex > 0)
            {
                yield return new Delta(0 - (StepIndex * 5), typeof(SequencePotential));
            }
            yield break;
        }

        public override Type ProcessType => typeof(MeleeAttackData);
    }
}
