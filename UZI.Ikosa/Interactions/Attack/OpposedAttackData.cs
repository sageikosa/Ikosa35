using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class OpposedAttackData : MeleeAttackData
    {
        #region construction
        /// <summary>Non critical threat constructor</summary>
        public OpposedAttackData(CoreActor actor, ActionBase action, Locator locator,
            AttackImpact impact, Deltable score, bool harmless,
            ICellLocation source, ICellLocation target, int targetIndex, int targetCount)
            : base(actor, action, locator, impact, score, null, harmless, source, target, targetIndex, targetCount)
        {
        }
        #endregion

        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (Attacker is Creature _critter)
            {
                foreach (var _delta in _critter.OpposedDeltable.QualifiedDeltas(qualify, this, @"Opposed"))
                    yield return _delta;
                if (Action is AttackActionBase _attack
                    // TODO: if grappling, no wield template deltas
                    && _attack.Weapon is IWeapon _weapon)
                {
                    var _wieldTemplate = _weapon.GetWieldTemplate();
                    var _wieldDelta = _wieldTemplate.OpposedDelta();
                    if (_wieldDelta != 0)
                    {
                        yield return new QualifyingDelta(_wieldDelta, typeof(WieldTemplate), $@"{_wieldTemplate} Wielding");
                    }
                }
            }
            else
            {
                yield return new QualifyingDelta(0, GetType(), @"Opposed");
            }
            yield break;
        }
    }
}
