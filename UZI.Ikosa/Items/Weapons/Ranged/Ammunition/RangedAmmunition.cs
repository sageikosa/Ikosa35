using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items.Weapons.Ranged
{
    /// <summary>Instance of ranged ammunition for use in attack flight and attack damage.  </summary>
    [Serializable]
    public class RangedAmmunition : WeaponHead
    {
        #region construction
        /// <summary>Instance of ranged ammunition for use in attack flight and attack damage</summary>
        public RangedAmmunition(WeaponBase weapon, IAmmunitionBase ammo)
            : base(weapon, ammo.MediumDamageRollString, ammo.DamageTypes, ammo.CriticalLow,
            new DeltableQualifiedDelta(0, ammo.CriticalDamageFactor.Name, ammo.CriticalDamageFactor.Source),
            ammo.HeadMaterial, ammo.LethalitySelection)
        {
            // NOTE: need same ID as ammo in order to sync client and server attack actions
            ID = ammo.ID;
            _DamageTypes = ammo.DamageTypes.Clone() as DamageType[]; // array of value types, so clone is as deep a copy as it needs to be
            _Ammo = ammo;

            // merge ammo qualified deltas
            AttackBonus.Deltas.Add(new SoftQualifiedDelta(ammo.AttackBonus));
            DamageBonus.Deltas.Add(new SoftQualifiedDelta(ammo.DamageBonus));
            CriticalDamageFactor.Deltas.Add(new SoftQualifiedDelta(ammo.CriticalDamageFactor));
            CriticalRangeFactor.Deltas.Add(new SoftQualifiedDelta(ammo.CriticalRangeFactor));

            // merge weapon qualified deltas
            AttackBonus.Deltas.Add(new SoftQualifiedDelta(Projector.AttackBonus));
            DamageBonus.Deltas.Add(new SoftQualifiedDelta(Projector.DamageBonus));
            CriticalDamageFactor.Deltas.Add(new SoftQualifiedDelta(Projector.CriticalDamageFactor));
            CriticalRangeFactor.Deltas.Add(new SoftQualifiedDelta(Projector.CriticalRangeFactor));
        }
        #endregion

        private IAmmunitionBase _Ammo;

        public IProjectileWeapon Projector => ContainingWeapon as IProjectileWeapon;
        public IAmmunitionBase Ammunition => _Ammo;

        #region public override void AttackResult(AttackResultStep result, Interaction workSet)
        public override void AttackResult(AttackResultStep result, Interaction workSet)
        {
            #region ISecondaryAttackResult
            // extra damage effects due to the weapon...
            var _sources = new List<object>(); ;
            var _secondaries = new List<ISecondaryAttackResult>();
            foreach (var _processor in from _proc in Ammunition.Adjuncts.OfType<ISecondaryAttackResult>()
                                       where _proc.PoweredUp
                                       select _proc)
            {
                if (!_sources.Contains(_processor.AttackResultSource))
                {
                    _sources.Add(_processor.AttackResultSource);
                    _secondaries.Add(_processor);
                }
            }
            foreach (var _processor in from _proc in Projector.Adjuncts.OfType<ISecondaryAttackResult>()
                                       where _proc.PoweredUp
                                       select _proc)
            {
                if (!_sources.Contains(_processor.AttackResultSource))
                {
                    _sources.Add(_processor.AttackResultSource);
                    _secondaries.Add(_processor);
                }
            }
            #endregion

            DamageAttackResult(result, workSet, _secondaries);
            result.AttackAction?.Attack?.AttackResultEffects(result, workSet);
        }
        #endregion

        #region public override IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction workSet)
        public override IEnumerable<DamageRollPrerequisite> ExtraDamageRollers(Interaction workSet)
        {
            // merge from projectile and ammo (only one of each sourced roller)
            var _sources = new Collection<object>();
            foreach (var _roller in Ammunition.ExtraDamageRollers(workSet))
            {
                if (!_sources.Contains(_roller.Source))
                {
                    _sources.Add(_roller.Source);
                    yield return _roller;
                }
            }
            foreach (var _roller in Projector.ExtraDamageRollers(workSet))
            {
                if (!_sources.Contains(_roller.Source))
                {
                    _sources.Add(_roller.Source);
                    yield return _roller;
                }
            }
            yield break;
        }
        #endregion

        #region public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        public override IEnumerable<DamageRollPrerequisite> BaseDamageRollers(Interaction workSet, string keyFix, int minGroup)
        {
            // merge from projectile and ammo (only one of each sourced roller)
            var _sources = new Collection<object>();

            var _atk = (workSet.InteractData as AttackData);
            var _nonLethal = (_atk == null ? false : _atk.IsNonLethal);

            // weapon damage bonuses
            yield return new DamageRollPrerequisite(typeof(DeltableQualifiedDelta), workSet, $@"{keyFix}Enhancement", @"Enhancement",
                new ConstantRoller(DamageBonus.QualifiedValue(workSet)), false, _nonLethal, @"Ammo", minGroup);

            foreach (var _roller in Ammunition.BaseDamageRollers(workSet, keyFix, minGroup))
            {
                _sources.Add(_roller.Source);
                yield return _roller;
            }
            foreach (var _roller in Projector.BaseDamageRollers(workSet, keyFix, minGroup).Where(_r => !_sources.Contains(_r.Source)))
            {
                yield return _roller;
            }
            yield break;
        }
        #endregion

        public override double Weight { get { return _Ammo.Weight; } }
        public override bool IsMagicalDamage { get { return Projector.IsMagicalDamage || _Ammo.IsMagicalDamage; } }

        public override Alignment Alignment
        {
            // NOTE: should there be some penalty for mixing alignments...?
            get
            {
                var _ammoAlign = Ammunition.Alignment;
                return (_ammoAlign.IsNeutral ? Projector.Alignment : _ammoAlign);
            }
        }

        public override int CriticalRange { get { return Math.Max(Projector.CriticalRange, Ammunition.CriticalRange); } }

        protected override string ClassIconKey
            => string.Empty;

        public override IEnumerable<string> IconKeys
            => Ammunition?.IconKeys ?? new string[] { };
    }
}
