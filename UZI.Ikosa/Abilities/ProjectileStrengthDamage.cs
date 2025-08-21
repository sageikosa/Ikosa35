using System;
using Uzi.Core;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Interactions;
using System.Collections.Generic;

namespace Uzi.Ikosa.Abilities
{
    /// <summary>Qualified delta to apply strength damage on certain projectile weapon attacks</summary>
    [Serializable]
    public class ProjectileStrengthDamage : IQualifyDelta, ICreatureBound
    {
        public ProjectileStrengthDamage(Creature critter)
        {
            _Critter = critter;
            _Terminator = new TerminateController(this);
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  
        /// Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion

        private Creature _Critter;
        public Creature Creature { get { return _Critter; } }

        public virtual IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (!(qualify.Source is IWeaponHead _wpnHead))
            {
                yield break;
            }

            // ranged attack interactions only
            if (!(qualify is Interaction _iAct))
            {
                yield break;
            }

            if (!(_iAct.InteractData is RangedAttackData))
            {
                yield break;
            }

            // only projectile weapons
            if (!(_wpnHead.ContainingWeapon is IProjectileWeapon _projectile))
            {
                yield break;
            }

            // some projectile weapons do not deal with strength at all
            if (_projectile.UsesStrengthDamage || _projectile.TakesStrengthDamagePenalty)
            {
                yield break;
            }

            // NOTE: strict unqualified delta value
            var _delta = Creature.Abilities.Strength.DeltaValue;

            if (_projectile.UsesStrengthDamage)
            {
                // damage delta applies good or bad
                yield return new QualifyingDelta(_delta, typeof(Strength), @"Strength");
            }
            else if (_delta < 0)
            {
                // damage delta only applies if bad
                yield return new QualifyingDelta(_delta, typeof(Strength), @"Strength");
            }

            // nothing
            yield break;
        }
    }
}
