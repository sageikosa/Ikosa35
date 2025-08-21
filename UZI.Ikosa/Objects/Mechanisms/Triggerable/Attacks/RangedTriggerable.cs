using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Items;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Ranged;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class RangedTriggerable : AttackTriggerable, IRangedSource
    {
        public RangedTriggerable(string name, Material material, int seedDifficulty)
            : base(name, material, seedDifficulty)
        {
            AttackCount = RangedTriggerableAttackCount.One;
            _Template = RangedTriggerableTemplate.Arrow;
            DamageType = Contracts.DamageType.Piercing;
            PostTriggerState = PostTriggerState.DeActivate;
        }

        #region data
        private int _Loaded;
        private RangedTriggerableAttackCount _Count;
        private RangedTriggerableTemplate _Template;
        #endregion

        #region public int Loaded { get; set; }
        public int Loaded
        {
            get => _Loaded;
            set
            {
                _Loaded = Math.Max(Math.Min(value, MaxLoadable), 0);
                DoPropertyChanged(nameof(Loaded));
            }
        }
        #endregion

        #region public int MaxLoadable { get; }
        public int MaxLoadable
        {
            get
            {
                switch (AttackCount)
                {
                    case RangedTriggerableAttackCount.One:
                    case RangedTriggerableAttackCount.D2:
                    case RangedTriggerableAttackCount.D3:
                    case RangedTriggerableAttackCount.D4:
                        return (int)AttackCount + 1;

                    case RangedTriggerableAttackCount.D6:
                        return 6;

                    case RangedTriggerableAttackCount.D8:
                        return 8;

                    default:
                        return 1;
                }
            }
        }
        #endregion

        #region public RangedTriggerableAttackCount AttackCount { get; set; }
        public RangedTriggerableAttackCount AttackCount
        {
            get => _Count;
            set
            {
                _Count = value;
                DoPropertyChanged(nameof(AttackCount));
                Loaded = MaxLoadable;
            }
        }
        #endregion

        #region public RangedTriggerableTemplate RangedTemplate { get; set; }
        public RangedTriggerableTemplate RangedTemplate
        {
            get => _Template;
            set
            {
                _Template = value;
                if (value == RangedTriggerableTemplate.SlingBullet)
                {
                    DamageType = Contracts.DamageType.Bludgeoning;
                }
                else
                {
                    DamageType = Contracts.DamageType.Piercing;
                }
                DoPropertyChanged(nameof(RangedTemplate));
            }
        }
        #endregion

        protected override string ClassIconKey => nameof(RangedTriggerable);

        #region public int RangeIncrement { get; }
        public int RangeIncrement
        {
            get
            {
                switch (RangedTemplate)
                {
                    case RangedTriggerableTemplate.CrossbowBolt:
                        return 80;

                    case RangedTriggerableTemplate.Arrow:
                        return 60;

                    case RangedTriggerableTemplate.SlingBullet:
                        return 50;

                    case RangedTriggerableTemplate.Javelin:
                        return 30;

                    case RangedTriggerableTemplate.Dart:
                    case RangedTriggerableTemplate.Spear:
                        return 20;

                    case RangedTriggerableTemplate.Shuriken:
                    case RangedTriggerableTemplate.Needle:
                    default:
                        return 10;
                }
            }
        }
        #endregion

        public int MaxRange => RangeIncrement * 10;

        #region public override void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators)
        public override void DoTrigger(IActivatableObject mechanism, IEnumerable<Locator> locators)
        {
            if (Activation.IsActive && !IsDisabled)
            {
                if (this.GetLocated()?.Locator is Locator _loc)
                {
                    ConfusedDisablers.Clear();
                    switch (AttackCount)
                    {
                        case RangedTriggerableAttackCount.D2:
                        case RangedTriggerableAttackCount.D3:
                        case RangedTriggerableAttackCount.D4:
                        case RangedTriggerableAttackCount.D6:
                        case RangedTriggerableAttackCount.D8:
                            {
                                var _proc = new CoreTargetingProcess(new TriggeredMultiRangedAttackStep(this), this, @"Ranged Trap Multi",
                                        IsDirect
                                        ? [new ValueTarget<List<Locator>>(@"Direct", locators.ToList())]
                                        : null);
                                _proc.AppendCompletion(new AttackTriggerablePostTriggerStep(_proc, this));
                                _loc.Map.ContextSet.ProcessManager.StartProcess(_proc);
                            }
                            break;

                        case RangedTriggerableAttackCount.One:
                        default:
                            {
                                var _proc = new CoreTargetingProcess(new TriggeredRangedAttackStep(this, 1, 1), this, @"Ranged Trap",
                                        IsDirect
                                        ? [new ValueTarget<List<Locator>>(@"Direct", locators.ToList())]
                                        : null);
                                _proc.AppendCompletion(new AttackTriggerablePostTriggerStep(_proc, this));
                                _loc.Map.ContextSet.ProcessManager.StartProcess(_proc);
                            }
                            break;
                    }
                }
            }
        }
        #endregion

        #region protected override void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        protected override void DamageAttackResult(AttackResultStep result, Interaction workSet, List<ISecondaryAttackResult> secondaries)
        {
            base.DamageAttackResult(result, workSet, secondaries);
            var _atkFB = workSet.Feedback.OfType<AttackFeedback>().FirstOrDefault();
            var _target = result.TargetingProcess.Targets.OfType<AttackTarget>().FirstOrDefault();
            if (_target != null)
            {
                var _aLoc = _target.Attack.AttackLocator;

                // assume weapon makes it to nearest target cell
                var _tCell = _target.TargetCell;
                var _hit = (_atkFB?.Hit ?? false);
                if (!_hit)
                {
                    // weapon will be somewhere along line (first blocked cell?)
                    // either to the target, or the finalized cell location...
                    var _line = GetSingleLine(_target.SourcePoint, _target.TargetPoint, _target.Target,
                        _target.Attack.SourceCell, _tCell, _aLoc.PlanarPresence);
                    if (_line.BlockedCell.IsActual)
                    {
                        _tCell = _line.BlockedCell.ToCellPosition();
                    }
                    else if (_line.UnblockedCell.IsActual)
                    {
                        _tCell = _line.UnblockedCell.ToCellPosition();
                    }
                    else
                    {
                        _tCell = _target.Attack.SourceCell.ToCellPosition();
                    }
                }

                void _dropItem(IItemBase item)
                {
                    var _dropData = new Drop(null, Setting as LocalMap, _tCell, false);
                    var _interact = new Interaction(null, this, item, _dropData);
                    item.HandleInteraction(_interact);
                }

                void _tryDropItem(IItemBase item)
                {
                    // 50% recovery on a miss
                    if (!_hit && (DieRoller.RollDie(Guid.Empty, 100, @"Ammo", @"Recovery %") <= 50))
                    {
                        _dropItem(item);
                    }
                }

                switch (RangedTemplate)
                {
                    case RangedTriggerableTemplate.Arrow:
                        // 50% on miss
                        _tryDropItem(new Arrow());
                        break;

                    case RangedTriggerableTemplate.CrossbowBolt:
                        // 50% on miss
                        _tryDropItem(new CrossbowBolt());
                        break;

                    case RangedTriggerableTemplate.SlingBullet:
                        // 50% on miss
                        _tryDropItem(new SlingBullet());
                        break;

                    case RangedTriggerableTemplate.Shuriken:
                        // 50% on miss
                        _tryDropItem(new Shuriken());
                        break;

                    case RangedTriggerableTemplate.Dart:
                        // always drop
                        _dropItem(new Dart());
                        break;

                    case RangedTriggerableTemplate.Spear:
                        // always drop
                        _dropItem(new Spear());
                        break;

                    case RangedTriggerableTemplate.Javelin:
                        // always drop
                        _dropItem(new Javelin());
                        break;

                    case RangedTriggerableTemplate.Needle:
                    default:
                        // never drop
                        break;
                }
            }
        }
        #endregion

        protected SegmentSet GetSingleLine(
            System.Windows.Media.Media3D.Point3D sourcePt, System.Windows.Media.Media3D.Point3D targetPt,
            IInteract target, ICellLocation sourceCell, ICellLocation targetCell, PlanarPresence planar)
        {
            var _map = Setting as LocalMap;
            // TODO: exclusions? actor at least...?
            var _factory = new SegmentSetFactory(_map, sourceCell.ToCellPosition(), targetCell.ToCellPosition(),
                ITacticalInquiryHelper.EmptyArray, SegmentSetProcess.Effect);
            return _map.SegmentCells(sourcePt, targetPt, _factory, planar);
        }

        public override IEnumerable<CoreAction> GetTacticalActions(CoreActionBudget budget)
        {
            // TODO: reload and reset RangedTriggerable

            // base tactical actions
            foreach (var _act in BaseMechanismActions(budget))
            {
                yield return _act;
            }
        }
    }

    [Serializable]
    public enum RangedTriggerableTemplate
    {
        Arrow,
        CrossbowBolt,
        SlingBullet,
        Needle,
        Shuriken,
        Dart,
        Spear,
        Javelin
    }

    [Serializable]
    public enum RangedTriggerableAttackCount
    {
        One,
        D2,
        D3,
        D4,
        D6,
        D8
    }
}
