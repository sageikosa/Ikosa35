using System;
using System.Collections.Generic;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Feats;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Items.Weapons
{
    // TODO: parameterized by creature type
    // TODO: species/creatureType?
    [Serializable]
    [MagicAugmentRequirement(8, typeof(CraftMagicArmsAndArmorFeat))] // TODO: summon monster 1
    public class Bane<CritterType> : WeaponExtraDamage, IQualifyDelta, IMonitorChange<DeltaValue>
        where CritterType : CreatureType
    {
        public Bane(object source)
            : base(source, 1, 0)
        {
            _BaneDelta = new Delta(1, GetType(), @"Bane");
            _Terminator = new TerminateController(this);
        }

        private Delta _BaneDelta;
        public Delta BaseDelta => _BaneDelta;

        public override bool CanUseOnRanged => true;

        public override IEnumerable<DamageRollPrerequisite> DamageRollers(Interaction workSet)
        {
            if (workSet.Target is Creature _critter)
            {
                if (typeof(CritterType).IsAssignableFrom(_critter.CreatureType.GetType()))
                {
                    yield return new DamageRollPrerequisite(
                        typeof(Bane<CritterType>), workSet, @"Bane", @"Bane",
                        new DiceRoller(2, 6), false, false, @"Bane", 0);
                }
            }
            yield break;
        }

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is IWeaponHead _head)
            {
                _head.AttackBonus.Deltas.Add(this);
                _head.DamageBonus.Deltas.Add(this);
                _head.TotalEnhancement.Deltas.Add(_BaneDelta);
            }
        }

        protected override void OnDeactivate(object source)
        {
            _BaneDelta.DoTerminate();
            DoTerminate();
            base.OnDeactivate(source);
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

        #region IMonitorChange<DeltaValue> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args)
        {
            throw new NotImplementedException();
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            throw new NotImplementedException();
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region IQualifyDelta Members
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify is Interaction _iAct)
            {
                if (_iAct.InteractData is AttackData)
                {
                    var _qd = QualifiedDelta(qualify.Target);
                    if (_qd != null)
                        yield return _qd;
                }
            }
            yield break;
        }

        public IDelta QualifiedDelta(ICore iCore)
        {
            var _critter = iCore as Creature;
            if (typeof(CritterType).IsAssignableFrom(_critter.CreatureType.GetType()))
                return new QualifyingDelta(2, GetType(), @"Bane");
            return null;
        }
        #endregion

        public override object Clone()
            => new Bane<CritterType>(Source);

        public override IEnumerable<Info> IdentificationInfos
        {
            get
            {
                // TODO: name the creature type
                yield return new Description(Name,
                    new string[]{
                    string.Format(@"Attack +2 Versus {0}", typeof(CritterType).SourceName()),
                    string.Format(@"Damage +2 Versus {0}", typeof(CritterType).SourceName()),
                    string.Format(@"Damage +2d6 Versus {0}", typeof(CritterType).SourceName())
                });
                yield break;
            }
        }

        public override bool Equals(Adjunct other)
            => other is Bane<CritterType>;
    }
}
