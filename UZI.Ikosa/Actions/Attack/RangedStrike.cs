using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Interactions;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Ikosa.Items.Weapons.Ranged;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Base for ranged weapon attacks.</summary>
    [Serializable]
    public abstract class RangedStrike : AttackActionBase, IRangedSourceProvider
    {
        protected RangedStrike(RangedAmmunition source, AttackImpact impact, string orderKey)
            : this(source, impact, source.ContainingWeapon.ProvokesMelee, orderKey)
        {
        }

        protected RangedStrike(RangedAmmunition source, AttackImpact impact, bool provokesMelee, string orderKey)
            : base(source, provokesMelee, source.ContainingWeapon.ProvokesTarget, orderKey)
        {
            _Impact = impact;
        }

        private AttackImpact _Impact;

        public AttackImpact Impact => _Impact;
        public IRangedSource RangedSource => Weapon as IRangedSource;
        public RangedAmmunition RangedAmmunition => WeaponHead as RangedAmmunition;

        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.RangedDeltable.QualifiedValue(
                new Interaction(Weapon.CreaturePossessor, WeaponHead, null,
                    new RangedAttackData(null, RangedSource, this, Weapon.GetLocated()?.Locator, Impact, null, false, new Point3D(), null, null, 1, 1)));

        public override string Key => @"Attack.Ranged";
        public override string DisplayName(CoreActor actor)
            => $@"Ranged Attack with {Weapon.GetKnownName(actor)} using {RangedAmmunition.Ammunition.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Ranged Strike", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(WeaponHead, observer);
            return _obs;
        }

        protected IEnumerable<AimingMode> BaseAimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Ranged.Target", @"Ranged Target", Impact,
                WeaponHead.LethalitySelection, false, WeaponHead.CriticalLow, this, FixedRange.One, FixedRange.One,
                new FixedRange(RangedSource.MaxRange),
                new ObjectTargetType(), new CreatureTargetType());
            yield break;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
            => BaseAimingMode(activity);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Ranged.Target");

        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => RangedSource;

        protected SegmentSet GetSingleLine(Point3D sourcePt, Point3D targetPt, IInteract target,
            ICellLocation sourceCell, ICellLocation targetCell, PlanarPresence planar)
        {
            var _map = Weapon.Setting as LocalMap;
            // TODO: exclusions? actor at least...?
            var _factory = new SegmentSetFactory(_map, sourceCell.ToCellPosition(), targetCell.ToCellPosition(),
                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect);
            return _map.SegmentCells(sourcePt, targetPt, _factory, planar);
        }
    }
}
