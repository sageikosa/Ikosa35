using System.Collections.Generic;
using Uzi.Core;
using Uzi.Visualize;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using System;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class MeleeAttackData : AttackData
    {
        #region construction
        /// <summary>Non critical threat constructor</summary>
        public MeleeAttackData(CoreActor actor, ActionBase action, Locator locator,
            AttackImpact impact, Deltable score, bool harmless, ICellLocation source, ICellLocation target,
            int targetIndex, int targetCount)
            : this(actor, action, locator, impact, score, null, harmless, source, target, targetIndex, targetCount)
        {
        }

        /// <summary>Critical threat constructor</summary>
        public MeleeAttackData(CoreActor actor, ActionBase action, Locator locator,
            AttackImpact impact, Deltable score, Deltable criticalConfirm, bool harmless,
            ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : base(actor, action, locator, impact, score, criticalConfirm, harmless, source, target, targetIndex, targetCount)
        {
        }
        #endregion

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (Attacker is Creature _critter)
            {
                foreach (var _delta in _critter.MeleeDeltable.QualifiedDeltas(qualify, this, @"Melee"))
                {
                    yield return _delta;
                }
            }
            else
            {
                yield return new QualifyingDelta(0, GetType(), @"Melee");
            }
            yield break;
        }

        /// <summary>Clones the attack data, but uses fresh deltables</summary>
        public OpposedAttackData ToOpposedAttackData()
        {
            return new OpposedAttackData(Attacker, Action, AttackLocator,
                Impact, new Deltable(AttackScore.BaseValue), Harmless, SourceCell, TargetCell,
                TargetIndex, TargetCount);
        }
    }
}
