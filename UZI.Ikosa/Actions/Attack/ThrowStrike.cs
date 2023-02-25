using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class ThrowStrike : AttackActionBase, IRangedSourceProvider
    {
        #region construction
        public ThrowStrike(IWeaponHead head, IThrowableWeapon throwable, AttackImpact impact, string orderKey)
            : this(head, throwable, impact, true, orderKey)
        {
        }

        public ThrowStrike(IWeaponHead head, IThrowableWeapon throwable, AttackImpact impact, bool provokesMelee, string orderKey)
            : base(head, provokesMelee, false, orderKey)
        {
            _Throwable = throwable;
            _Impact = impact;
        }
        #endregion

        #region private data
        private AttackImpact _Impact;

        /// <summary>this may be provided by an adjunct, so it might not be the weapon itself</summary>
        private IThrowableWeapon _Throwable;
        #endregion

        public AttackImpact Impact => _Impact;
        public IThrowableWeapon ThrowableWeapon => _Throwable;

        public override int StandardAttackBonus
            => WeaponHead.AttackBonus.EffectiveValue + Weapon.CreaturePossessor.RangedDeltable.QualifiedValue(
                new Interaction(Weapon.CreaturePossessor, WeaponHead, null,
                    new RangedAttackData(null, ThrowableWeapon, this, Weapon.GetLocated()?.Locator, Impact, null, false, new Point3D(), null, null, 1, 1)));

        public override string Key => @"Attack.Thrown";
        public override string DisplayName(CoreActor actor)
            => $@"Thrown Attack with {Weapon.GetKnownName(actor)}";

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
        {
            var _obs = ObservedActivityInfoFactory.CreateInfo(@"Throw Strike", activity.Actor, observer, activity.Targets[0].Target as CoreObject);
            _obs.Implement = GetInfoData.GetInfoFeedback(WeaponHead, observer);
            return _obs;
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new AttackAim(@"Thrown.Target", @"Throw Attack Target", Impact,
                WeaponHead.LethalitySelection, true, WeaponHead.CriticalLow, this, FixedRange.One, FixedRange.One,
                new FixedRange((double)ThrowableWeapon.MaxRange),
                new ITargetType[] { new ObjectTargetType(), new CreatureTargetType() });
            yield break;
        }

        public IRangedSource GetRangedSource(CoreActor actor, ActionBase action, RangedAim aim, IInteract target)
            => ThrowableWeapon;

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => IsAttackProvocableTarget(activity, potentialTarget, @"Ranged.Target");

        public override void AttackResultEffects(AttackResultStep result, Interaction workSet)
        {
            // unslot weapon if still slotted (weapon may have been handled elsehow)
            if (Weapon.MainSlot != null)
            {
                // need to figure out if we hit or missed
                var _atkFB = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                var _target = result.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
                var _critter = result.AttackData.Attacker as Creature;
                if (_target != null)
                {
                    // assume weapon makes it to nearest target cell
                    var _tCell = _target.TargetCell;
                    if (!(_atkFB?.Hit ?? false))
                    {
                        var _planar = _critter.GetPlanarPresence();

                        // weapon will be somewhere along line (first blocked cell?)
                        // either to the target, or the finalized cell location...
                        var _line = GetSingleLine(_target.SourcePoint, _target.TargetPoint, _target.Target,
                            _target.Attack.SourceCell, _tCell, _planar);
                        if (_line.BlockedCell.IsActual)
                            _tCell = _line.BlockedCell.ToCellPosition();
                        else if (_line.UnblockedCell.IsActual)
                            _tCell = _line.UnblockedCell.ToCellPosition();
                        else
                            _tCell = _target.Attack.SourceCell.ToCellPosition();
                    }

                    Weapon.ClearSlots();

                    if (Weapon.MainSlot == null)
                    {
                        // drop at target cell
                        var _dropData = new Drop(_critter, _critter.Setting as LocalMap, _tCell, false);
                        var _interact = new Interaction(_critter, this, Weapon, _dropData);
                        Weapon.HandleInteraction(_interact);
                    }
                }
            }
        }

        protected SegmentSet GetSingleLine(Point3D sourcePt, Point3D targetPt, IInteract target,
            ICellLocation sourceCell, ICellLocation targetCell, PlanarPresence planar
            )
        {
            var _map = Weapon.Setting as LocalMap;
            // TODO: exclusions? actor at least...?
            var _factory = new SegmentSetFactory(_map, sourceCell.ToCellPosition(), targetCell.ToCellPosition(),
                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect);
            return _map.SegmentCells(sourcePt, targetPt, _factory, planar);
        }
    }
}
