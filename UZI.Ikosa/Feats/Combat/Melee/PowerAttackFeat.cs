using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using Uzi.Ikosa.Advancement;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Items.Weapons.Natural;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Contracts;
using System.Linq;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Feats
{
    [
    Serializable,
    AbilityRequirement(Abilities.MnemonicCode.Str, 13),
    FighterBonusFeat,
    FeatInfo(@"Power Attack")
    ]
    public class PowerAttackFeat : FeatBase, IActionProvider, IModifier, IActionSource
    {
        public PowerAttackFeat(object source, int powerLevel)
            : base(source, powerLevel)
        {
            _OneHand = new PowerAttackFeat.OneHandDelta(this);
            _TwoHands = new PowerAttackFeat.TwoHandDelta(this);
            _Term = new TerminateController(this);
            _ValCtrl = new ChangeController<DeltaValue>(this, new DeltaValue(0));
        }

        public Guid ID => CoreID;

        public override string Benefit
            => @"Trade up to maximum base attack for damage until next turn.";

        #region data
        private IQualifyDelta _OneHand;
        private IQualifyDelta _TwoHands;
        private readonly TerminateController _Term;
        private ChangeController<DeltaValue> _ValCtrl;
        #endregion

        protected override void OnActivate()
        {
            base.OnActivate();
            Creature.Actions.Providers.Add(this, this);
            Creature.ExtraWeaponDamage.Deltas.Add(_OneHand);
            Creature.ExtraWeaponDamage.Deltas.Add(_TwoHands);
            Creature.MeleeDeltable.Deltas.Add(this);
        }

        protected override void OnDeactivate()
        {
            DoTerminate();
            _OneHand.DoTerminate();
            _TwoHands.DoTerminate();
            Creature.Actions.Providers.Remove(this);
            base.OnDeactivate();
        }

        public int Dial
        {
            get => 0 - _ValCtrl.LastValue.Value;
            set
            {
                var _new = new DeltaValue(0 - value);
                if (!_ValCtrl.WillAbortChange(_new))
                {
                    _ValCtrl.DoPreValueChanged(_new);
                    _ValCtrl.DoValueChanged(_new);
                    PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Value)));
                }
            }
        }

        public int Value
            => _ValCtrl.LastValue.Value;

        #region IActionProvider Members
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            if ((budget is LocalActionBudget _budget) && _budget.CanPerformRegular)
            {
                // an attack can only be made if there's a regular action left in the budget
                yield return new PowerAttackChoice(this);
            }
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
            => ToFeatInfo();
        #endregion

        [Serializable]
        public class TwoHandDelta : IQualifyDelta
        {
            internal TwoHandDelta(PowerAttackFeat pwrAtk)
            {
                _Terminator = new TerminateController(this);
                _PwrAtk = pwrAtk;
            }

            private PowerAttackFeat _PwrAtk;

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

            #region IQualifyDelta Members
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
                => QualifiedDelta(qualify.Source).ToEnumerable().Where(_d => _d!=null);

            public IDelta QualifiedDelta(object source)
            {
                // only weapon heads can meet qualification
                if (!(source is IWeaponHead _wpnHead))
                {
                    return null;
                }

                if (_wpnHead.ContainingWeapon is IMeleeWeapon _meleeWpn)
                {
                    // natural weapons do not get the two-hand delta
                    if (_meleeWpn is NaturalWeapon)
                    {
                        return null;
                    }

                    switch (_meleeWpn.GetWieldTemplate())
                    {
                        case WieldTemplate.Unarmed:
                        case WieldTemplate.Light:
                        case WieldTemplate.TooSmall:
                        case WieldTemplate.TooBig:
                            // light weapons get no power attack delta
                            return null;

                        case WieldTemplate.Double:
                            var _dblWpn = _meleeWpn as DoubleMeleeWeaponBase;
                            // double being treated as two-handed means this delta applies
                            if (!_dblWpn.UseAsTwoHanded)
                            {
                                return null;
                            }

                            break;

                        default:
                            // two-handed, or one handed wielding two handedly
                            if (_meleeWpn.SecondarySlot == null)
                            {
                                return null;
                            }

                            break;
                    }
                }

                return new QualifyingDelta(_PwrAtk.Dial * 2, typeof(PowerAttackFeat), @"Power Attack");
            }
            #endregion
        }

        [Serializable]
        public class OneHandDelta : IQualifyDelta
        {
            internal OneHandDelta(PowerAttackFeat pwrAtk)
            {
                _Terminator = new TerminateController(this);
                _PwrAtk = pwrAtk;
            }

            private PowerAttackFeat _PwrAtk;

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

            #region IQualifyDelta Members
            public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
                => QualifiedDelta(qualify.Source).ToEnumerable().Where(_d => _d!=null);

            public IDelta QualifiedDelta(object source)
            {
                // only weapon heads can meet qualifications
                if (!(source is IWeaponHead _wpnHead))
                {
                    return null;
                }

                if (_wpnHead.ContainingWeapon is IMeleeWeapon _meleeWpn)
                {
                    if (_meleeWpn is NaturalWeapon _natrl)
                    {
                        // secondary natural weapons get one-hand damage only if they are treated as sole
                        if (!_natrl.IsPrimary)
                        {
                            if (!_natrl.TreatAsSoleWeapon)
                            {
                                return null;
                            }
                        }
                    }
                    else
                    {
                        switch (_meleeWpn.GetWieldTemplate())
                        {
                            case WieldTemplate.Unarmed:
                            case WieldTemplate.Light:
                            case WieldTemplate.TooBig:
                            case WieldTemplate.TooSmall:
                                // these get nothing from power attack
                                return null;

                            case WieldTemplate.Double:
                                var _dblWpn = _meleeWpn as DoubleMeleeWeaponBase;
                                // the two-handed delta will pick this up
                                if (_dblWpn.UseAsTwoHanded)
                                {
                                    return null;
                                }

                                // only apply power attack to main head damage
                                if (_dblWpn.MainHead != _wpnHead)
                                {
                                    return null;
                                }

                                break;

                            default:
                                // one handed, or two-handed wielding one handed (when possible)
                                if (_meleeWpn.SecondarySlot != null)
                                {
                                    return null;
                                }

                                break;
                        }
                    }
                }

                return new QualifyingDelta(_PwrAtk.Dial, typeof(PowerAttackFeat), @"Power Attack");
            }
            #endregion
        }

        #region IDelta Members
        public bool Enabled { get { return MeetsRequirements; } set { } }
        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion

        #region IControlChange<DeltaValue> Members

        public void AddChangeMonitor(IMonitorChange<DeltaValue> monitor)
        {
            _ValCtrl.AddChangeMonitor(monitor);
        }

        public void RemoveChangeMonitor(IMonitorChange<DeltaValue> monitor)
        {
            _ValCtrl.RemoveChangeMonitor(monitor);
        }

        #endregion

        #region INotifyPropertyChanged Members
        [field:NonSerialized, JsonIgnore]
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        #endregion

        // IActionSource
        public IVolatileValue ActionClassLevel
            => Creature?.ActionClassLevel;
    }

    #region public class PowerAttackChoice : ActionBase
    [Serializable]
    public class PowerAttackChoice : ActionBase
    {
        internal PowerAttackChoice(PowerAttackFeat pwrAtk)
            : base(pwrAtk, new ActionTime(TimeType.Regular), new ActionTime(TimeType.FreeOnTurn), false, false, @"200")
        {
        }

        public override string Key => @"Attack.Power";
        public override string DisplayName(CoreActor actor) => @"Set Power Attack Level";
        public PowerAttackFeat PowerAttack => Source as PowerAttackFeat;
        public override bool IsMental => true;
        public override bool IsChoice => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            // read selected value
            if ((activity.Targets[0] as OptionTarget).Option is OptionAimValue<int> _switch)
            {
                var _newVal = _switch.Value;

                // set the dial
                if ((_newVal >= 0) && (_newVal <= PowerAttack.Creature.BaseAttack.EffectiveValue))
                {
                    PowerAttack.Dial = _newVal;
                }

                _Budget.Choices[Key] = new ChoiceBinder(this, _Budget.Actor, false, _switch);

                // status step
                return new NotifyStep(activity, new SysNotify(@"Choice",
                    new Description(@"Power Attack", $@"Set to {_newVal}")))
                {
                    InfoReceivers = new Guid[] { activity.Actor.ID }
                };
            }

            // status step
            return new NotifyStep(activity, new SysNotify(@"Choice",
                new Description(@"Power Attack", @"No switch setting")))
            {
                InfoReceivers = new Guid[] { activity.Actor.ID }
            };
        }

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Level", @"Power Attack Value", true, FixedRange.One, FixedRange.One, Options());
            yield break;
        }

        private IEnumerable<OptionAimOption> Options()
        {
            for (var _bav = 0; _bav <= PowerAttack.Creature.BaseAttack.EffectiveValue; _bav++)
            {
                yield return new OptionAimValue<int>()
                {
                    Key = _bav.ToString(),
                    Name = _bav.ToString(),
                    Description = _bav.ToString(),
                    Value = _bav,
                    IsCurrent = (PowerAttack.Dial == _bav)
                };
            }
            yield break;
        }

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
    #endregion
}
