using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Objects;
using Uzi.Core.Dice;
using Uzi.Visualize;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class Potion : SlottedItemBase, IActionProvider, IConsumableItem, IAppliableItem, IFeedableItem
    {
        private enum PotionColor : byte
        {
            Black, Red, Green, Blue, Cyan, Magenta, Yellow, White, LimeGreen, Violet, Orange, Silver
        }

        #region ctor()
        public Potion(string name, SpellSource source, string extra, ISpellMode mode, Dictionary<string, string> options)
            : base(@"vial of liquid", ItemSlot.HoldingSlot)
        {
            Name = name;
            _Spell = source;
            _Extra = extra;
            _Mode = mode;
            AddAdjunct(new MagicSourceAuraAdjunct(source));
            _Fails = [];
            _OptionsSet = options;
            var _timeFactor = (source.SpellDef.ActionTime > new ActionTime(TimeType.Regular))
                ? 5m
                : 1m;
            if (source.SlotLevel == 0)
            {
                Price.BaseItemExtraPrice = 25 * source.CasterLevel * _timeFactor;
            }
            else
            {
                Price.BaseItemExtraPrice = 50 * source.SlotLevel * source.CasterLevel * _timeFactor;
            }

            // NOTE: these stats are for the vial holding the potion
            ItemMaterial = GlassMaterial.Static;
            Sizer.NaturalSize = Size.Fine;
            BaseWeight = 0.1d;
            // TODO: AR = 13 ?? Fine ==> AR = 8+3 = 11
            // TODO: break DC=12

            // color for potion
            AddAdjunct(new ColorMapAdjunct { ColorMap = { { @"Substance", ((PotionColor)(DieRoller.RollDie(Guid.Empty, 8, @"Potion", @"Icon color") - 1)).ToString() } } });
        }
        #endregion

        #region state
        private SpellSource _Spell;
        private string _Extra;
        private ISpellMode _Mode;
        private Dictionary<string, string> _OptionsSet;
        #endregion

        public SpellSource SpellSource => _Spell;
        public ISpellMode SpellMode => _Mode;
        public IDictionary<string, string> OptionsSet => _OptionsSet;
        public string Extra => _Extra;

        #region public override IEnumerable<string> IconKeys { get; }
        public override IEnumerable<string> IconKeys
        {
            get
            {
                // all adjunct overrides
                foreach (var _ik in base.IconKeys.Where(_ik => _ik != ClassIconKey))
                {
                    yield return _ik;
                }

                // standard class Icon
                yield return ClassIconKey;
                yield break;
            }
        }
        #endregion

        #region public bool IsDrinkable { get; }
        /// <summary>
        /// IsDrinkable if the SpellMode has CreatureTargets for target types
        /// </summary>
        public bool IsDrinkable
        {
            get
            {
                // must have a creature target type 
                var _aimMode = (from _mode in SpellMode.AimingMode(null, SpellMode)
                                where typeof(IEnumerable<ITargetType>).IsAssignableFrom(_mode.GetType())
                                let _targMode = _mode as IEnumerable<ITargetType>
                                from _tType in _targMode
                                where typeof(CreatureTargetType).IsAssignableFrom(_tType.GetType())
                                select _tType).FirstOrDefault();
                return (_aimMode != null);
            }
        }
        #endregion

        #region public bool IsOily { get; }
        /// <summary>
        /// IsOily if the SpellMode has ObjectTargets for the target types
        /// </summary>
        public bool IsOily
        {
            get
            {
                // must have an object target type 
                var _aimMode = (from _mode in SpellMode.AimingMode(null, SpellMode)
                                where typeof(IEnumerable<ITargetType>).IsAssignableFrom(_mode.GetType())
                                let _targMode = _mode as IEnumerable<ITargetType>
                                from _tType in _targMode
                                where typeof(ObjectTargetType).IsAssignableFrom(_tType.GetType())
                                select _tType).FirstOrDefault();
                return (_aimMode != null);
            }
        }
        #endregion

        #region Check Failure
        private Collection<Guid> _Fails;
        public void FailCheck(Guid guid)
        {
            if (!_Fails.Contains(guid))
            {
                _Fails.Add(guid);
            }
        }

        public void ClearFailCheck(Guid guid)
        {
            if (_Fails.Contains(guid))
            {
                _Fails.Remove(guid);
            }
        }

        public bool HasFailedCheck(Guid guid)
            => _Fails.Contains(guid);
        #endregion

        /// <summary>removes the potion from the creature (permanently)</summary>
        public void DoneUsePotion()
        {
            ClearSlots();
            Possessor = null;
        }

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;
            var _info = GetInfoData.GetInfoFeedback(this, budget.Actor);
            if (_budget.CanPerformRegular)
            {
                yield return new ApplySpellItem(this, SpellSource, SpellMode, @"102");
                yield return new ConsumeSpellItem(this, SpellSource, SpellMode, @"101");
            }
            if (_budget.CanPerformTotal)
            {
                yield return new FeedSpellItem(this, SpellSource, SpellMode, @"201");
            }

            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => GetInfoData.GetInfoFeedback(this, budget.Actor);
        #endregion

        #region public override Info GetInfos(CoreActor actor, bool baseValues)
        public override Info GetInfo(CoreActor actor, bool baseValues)
        {
            var _objInfo = ObjectInfoFactory.CreateInfo<ObjectInfo>(actor, this, baseValues);
            if (!baseValues)
            {
                var _sourceInfo = SpellSource.ToSpellSourceInfo();
                if (!string.IsNullOrWhiteSpace(_Extra))
                {
                    _sourceInfo.Message = $@"{_sourceInfo.Message} ({Extra})";
                }

                _objInfo.AdjunctInfos =
                    _objInfo.AdjunctInfos.Union(_sourceInfo.ToEnumerable()).ToArray();
            }
            return _objInfo;
        }
        #endregion

        #region IConsumableItem Members

        public CoreStep DoConsume(CoreActivity activity)
        {
            DoneUsePotion();
            if (IsDrinkable)
            {
                return new PowerActivationStep<SpellSource>(activity, activity.Action as ConsumeSpellItem, activity.Actor);
            }
            return activity.GetActivityResultNotifyStep(@"Nothing happens");
        }

        public IEnumerable<AimingMode> ConsumptionAimingMode(CoreActivity activity)
        {
            var _actor = activity.Actor;
            if (_actor != null)
            {
                var _atkLoc = _actor.GetLocated().Locator;
                var _sensors = _actor as ISensorHost;
                var _srcCell = _sensors.GetAimCell(_atkLoc.GeometricRegion);
                var _map = _atkLoc.Map;
                var _targetLocator = Locator.FindFirstLocator(_actor);
                foreach (var _aimMode in SpellMode.AimingMode(null, SpellMode))
                {
                    if (_aimMode is IEnumerable<ITargetType> _targs)
                    {
                        if ((from _targType in _targs
                             where _targType is CreatureTargetType
                             select _targType).Any())
                        {
                            // fixed aim to self...
                            if (_aimMode is AwarenessAim)
                            {
                                var _planLine = _targetLocator.EffectLinesToTarget(_targetLocator.GeometricRegion,
                                    ITacticalInquiryHelper.GetITacticals(_actor).ToArray(), _targetLocator.PlanarPresence)
                                    .FirstOrDefault();
                                yield return new FixedAim(new AwarenessTarget(_aimMode.Key, _actor, _planLine),
                                    _aimMode.Key, _aimMode.DisplayName);
                            }
                            else if (_aimMode is AttackAim)
                            {
                                var _cell = _targetLocator.GeometricRegion.AllCellLocations().FirstOrDefault();
                                var _atkAim = _aimMode as AttackAim;
                                if ((_atkAim.Range is MeleeRange) || (_atkAim.Range is StrikeZoneRange))
                                {
                                    yield return new FixedAim(new AttackTarget(_aimMode.Key, _actor,
                                        new MeleeAttackData(_actor, activity.Action as AttackActionBase, _targetLocator,
                                        _atkAim.Impact, new Deltable(20), true, _srcCell, _cell, 1, 1)), _aimMode.Key, _aimMode.DisplayName);
                                }
                                else
                                {
                                    yield return new FixedAim(
                                        new AttackTarget
                                        (
                                            _aimMode.Key,
                                            _actor,
                                            new RangedAttackData
                                            (
                                                _actor,
                                                _atkAim.RangedSourceProvider.GetRangedSource(_actor, activity.Action as ActionBase, _atkAim, _actor),
                                                activity.Action as AttackActionBase, _targetLocator, _atkAim.Impact,
                                                new Deltable(20), true, _targetLocator.GeometricRegion.GetPoint3D(),
                                                _srcCell, null, 1, 1
                                            )
                                        ),
                                        _aimMode.Key,
                                        _aimMode.DisplayName);
                                }
                            }
                        }
                        else
                        {
                            yield return _aimMode;
                        }
                    }
                    else if (_aimMode is OptionAim _optAim)
                    {
                        if (_OptionsSet.TryGetValue(_optAim.Key, out var _optKey))
                        {
                            // lock in option
                            var _replaceMode = new OptionAim(_optAim.Key, _optAim.DisplayName, _optAim.NoDuplicates,
                                _optAim.MinimumAimingModes, _optAim.MaximumAimingModes,
                                _optAim.ListOptions.Where(_lo => _lo.Key == _optKey).ToList(),
                                _aimMode.PrepositionOption);
                            yield return _replaceMode;
                        }
                        else
                        {
                            // not set...
                            yield return _aimMode;
                        }
                    }
                    else
                    {
                        // non creature targetting aiming mode
                        // TODO: double check
                        yield return _aimMode;
                    }
                }
            }
            yield break;
        }

        #endregion

        #region IAppliableItem Members

        public CoreStep DoApply(CoreActivity activity)
        {
            // destroy the potion (unslot it, take it out of the object load, and depossess it)
            DoneUsePotion();

            // activate the spell
            if (IsOily)
            {
                return new PowerActivationStep<SpellSource>(activity, activity.Action as ApplySpellItem, activity.Actor);
            }
            return activity.GetActivityResultNotifyStep(@"Nothing happens");
        }

        public IEnumerable<AimingMode> ApplicationAimingMode(CoreActivity activity)
        {
            foreach (var _aimMode in SpellMode.AimingMode(activity.Actor, SpellMode))
            {
                if (_aimMode is OptionAim _optAim)
                {
                    if (_OptionsSet.TryGetValue(_optAim.Key, out var _optKey))
                    {
                        // lock in option
                        var _replaceMode = new OptionAim(_optAim.Key, _optAim.DisplayName, _optAim.NoDuplicates,
                            _optAim.MinimumAimingModes, _optAim.MaximumAimingModes,
                            _optAim.ListOptions.Where(_lo => _lo.Key == _optKey).ToList(),
                            _aimMode.PrepositionOption);
                        yield return _replaceMode;
                    }
                    else
                    {
                        // not set...
                        yield return _aimMode;
                    }
                }
                else
                {
                    if (_aimMode is IEnumerable<ITargetType> _targs)
                    {
                        if ((from _targType in _targs
                             where typeof(ObjectTargetType).IsAssignableFrom(_targType.GetType())
                             select _targType).Count() > 0)
                        {
                            // replace range with melee range
                            if (_aimMode is RangedAim _range)
                            {
                                _range.Range = new MeleeRange();
                            }
                        }
                    }

                    // yield the aiming mode
                    // TODO: double check
                    yield return _aimMode;
                }
            }
            yield break;
        }
        #endregion

        #region IFeedableItem Members

        public CoreStep DoFeed(CoreActivity activity)
            => DoConsume(activity);

        public IEnumerable<AimingMode> FeedableAimingMode(CoreActivity activity)
        {
            foreach (var _aimMode in SpellMode.AimingMode(activity.Actor, SpellMode))
            {
                if (_aimMode is OptionAim _optAim)
                {
                    if (_OptionsSet.TryGetValue(_optAim.Key, out var _optKey))
                    {
                        // lock in option
                        var _replaceMode = new OptionAim(_optAim.Key, _optAim.DisplayName, _optAim.NoDuplicates,
                            _optAim.MinimumAimingModes, _optAim.MaximumAimingModes,
                            _optAim.ListOptions.Where(_lo => _lo.Key == _optKey).ToList(),
                            _aimMode.PrepositionOption);
                        yield return _replaceMode;
                    }
                    else
                    {
                        // not set...
                        yield return _aimMode;
                    }
                }
                else
                {
                    if (_aimMode is IEnumerable<ITargetType> _targs)
                    {
                        if ((from _targType in _targs.OfType<CreatureTargetType>()
                             select _targType).Count() > 0)
                        {
                            // replace range with melee range
                            if (_aimMode is RangedAim _range)
                            {
                                _range.Range = new MeleeRange();
                            }
                        }

                    }

                    // yield the aiming mode
                    yield return _aimMode;
                }

            }
            yield break;
        }

        #endregion

        public override IVolatileValue ActionClassLevel
            => new Deltable(1);

        public int ActionLevel
            => 1;

        protected override string ClassIconKey => @"potion";

        public override bool IsTransferrable
            => true;

        public override ActionTime SlottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool SlottingProvokes => false;
        public override ActionTime UnslottingTime => new ActionTime(Contracts.TimeType.Brief);
        public override bool UnslottingProvokes => false;
    }
}
