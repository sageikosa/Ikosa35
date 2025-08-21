using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Magic.Spells
{
    [Serializable]
    public class RemoveParalysis : SpellDef, ISpellMode, ISaveCapable
    {
        public override MagicStyle MagicStyle => new Conjuration(Conjuration.SubConjure.Healing);
        public override string DisplayName => @"Remove Paralysis";
        public override string Description => @"Negate paralysis for 1 creature, or +4 Resistance re-save for 2 creatures, or +2 Resistance re-save for up to 4 creatures.";

        #region public override IEnumerable<SpellComponent> ArcaneComponents { get; }
        public override IEnumerable<SpellComponent> ArcaneComponents
        {
            get
            {
                yield return new VerbalComponent();
                yield return new SomaticComponent();
                yield break;
            }
        }
        #endregion

        #region public override IEnumerable<ISpellMode> SpellModes { get; }
        public override IEnumerable<ISpellMode> SpellModes
        {
            get
            {
                yield return this;
                yield break;
            }
        }
        #endregion

        #region ISpellMode
        public bool AllowsSpellResistance => true;
        public bool IsHarmless => true;

        public IEnumerable<AimingMode> AimingMode(CoreActor actor, ISpellMode mode)
        {
            yield return new AwarenessAim(@"Creature", @"Creature", new FixedRange(1), new FixedRange(4),
                new NearRange(), new CreatureTargetType());
            yield break;
        }

        public void ActivateSpell(PowerActivationStep<SpellSource> deliver)
        {
            var _targets = deliver.TargetingProcess.Targets.OfType<AwarenessTarget>().Cast<AimTarget>().ToList();
            if (_targets.Count == 1)
            {
                SpellDef.DeliverSpell(deliver, 0, _targets[0], 0);
            }
            else
            {
                SpellDef.DeliverSpellInCluster(deliver, _targets, true, true, 30, 0);
            }
        }

        public void ApplySpell(PowerApplyStep<SpellSource> apply)
        {
            var _critter = (apply.DeliveryInteraction.Target as Creature);
            if (_critter != null)
            {
                var _targets = apply.TargetingProcess.Targets.OfType<AwarenessTarget>().ToList();
                switch (_targets.Count)
                {
                    case 1:
                        foreach (var _paralyzed in _critter.Adjuncts.OfType<ParalyzedEffect>().ToList())
                        {
                            // single creature gets unparalyzed
                            _paralyzed.Eject();
                        }
                        foreach (var _slow in _critter.Adjuncts.OfType<SlowEffect>().ToList())
                        {
                            // single creature gets unslowed
                            if (_slow.Source is MagicPowerEffect _mpe)
                            {
                                _mpe.Eject();
                            }
                            else
                            {
                                _slow.Eject();
                            }
                        }
                        break;

                    case 2:
                        foreach (var _paralyzed in _critter.Adjuncts.OfType<ParalyzedEffect>()
                            .Where(_p => _p.SaveMode != null).ToList())
                        {
                            // two creatures get to make new saves at +4 resistance for each paralysis
                            new RemoveParalysisStep(apply, _critter, 4, _paralyzed);
                        }
                        break;

                    default:
                        foreach (var _paralyzed in _critter.Adjuncts.OfType<ParalyzedEffect>()
                            .Where(_p => _p.SaveMode != null).ToList())
                        {
                            // 3 or 4 creatures get to make new saves at +2 resistance for each paralysis
                            new RemoveParalysisStep(apply, _critter, 2, _paralyzed);
                        }
                        break;
                }
            }
        }
        #endregion

        #region ISaveMode
        public SaveMode GetSaveMode(CoreActor actor, IPowerActionSource powerSource, Interaction workSet, string saveKey)
        {
            return new SaveMode(SaveType.Will, SaveEffect.Negates,
                SpellDef.SpellDifficultyCalcInfo(actor, powerSource as SpellSource, workSet.Target));
        }
        #endregion
    }

    [Serializable]
    public class RemoveParalysisStep : PreReqListStepBase
    {
        public RemoveParalysisStep(CoreStep step, Creature creature, int resistance, ParalyzedEffect effect)
            : base(step)
        {
            _Bonus = resistance;
            _Paralyzed = effect;
            _PendingPreRequisites.Enqueue(new RollPrerequisite(this, null, creature, $@"Save.{effect.SaveMode.SaveType}", @"Save to break paralysis",
                new DieRoller(20), true));
        }

        #region data
        private ParalyzedEffect _Paralyzed;
        private int _Bonus;
        #endregion

        protected override bool OnDoStep()
        {
            if (IsComplete)
            {
                return true;
            }

            var _roll = GetPrerequisite<RollPrerequisite>();
            if (_roll != null)
            {
                if (_roll.IsReady
                    && _Paralyzed.Anchor is Creature _critter)
                {
                    // make a bonus against paralysis
                    var _resist = new RemoveParalysisSave(_Bonus);
                    var _save = new SavingThrowData(_critter, _Paralyzed.SaveMode, new Deltable(_roll.RollValue));

                    // let target's handlers alter the roll if possible
                    _critter.HandleInteraction(new Interaction(_critter, _Paralyzed, _critter, _save));

                    switch (_save.SaveMode.SaveType)
                    {
                        case SaveType.Fortitude:
                            _critter.FortitudeSave.Deltas.Add(_resist);
                            break;
                        case SaveType.Reflex:
                            _critter.ReflexSave.Deltas.Add(_resist);
                            break;
                        case SaveType.Will:
                            _critter.WillSave.Deltas.Add(_resist);
                            break;
                    }

                    if (_save.Success(new Interaction(_critter, _Paralyzed, _critter, _save)))
                    {
                        _Paralyzed.Eject();
                    }

                    // done with this bonus
                    _resist.DoTerminate();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }

    [Serializable]
    public class RemoveParalysisSave : IQualifyDelta
    {
        /// <summary>+bonus resistance against paralysis</summary>
        public RemoveParalysisSave(int bonus)
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(bonus, typeof(Resistance), @"Remove Paralysis");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify?.Source is ParalyzedEffect)
            {
                yield return _Delta;
            }
            yield break;
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
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
    }

    [Serializable]
    public class RemoveSlowStep : PreReqListStepBase
    {
        public RemoveSlowStep(CoreStep step, Creature creature, int resistance, SlowEffect effect)
            : base(step)
        {
            _Bonus = resistance;
            _Slow = effect;
            _PendingPreRequisites.Enqueue(
                new RollPrerequisite(
                    this, null, creature, $@"Save.{effect.SaveMode.SaveType}",
                    $@"{effect.SaveMode.SaveType} save to break slow", new DieRoller(20), true));
        }

        #region data
        private SlowEffect _Slow;
        private int _Bonus;
        #endregion

        protected override bool OnDoStep()
        {
            if (IsComplete)
            {
                return true;
            }

            var _roll = GetPrerequisite<RollPrerequisite>();
            if (_roll != null)
            {
                if (_roll.IsReady
                    && _Slow.Anchor is Creature _critter)
                {
                    // make a bonus against paralysis
                    var _resist = new RemoveSlowSave(_Bonus);
                    var _save = new SavingThrowData(_critter, _Slow.SaveMode, new Deltable(_roll.RollValue));

                    // let target's handlers alter the roll if possible
                    _critter.HandleInteraction(new Interaction(_critter, _Slow, _critter, _save));

                    switch (_save.SaveMode.SaveType)
                    {
                        case SaveType.Fortitude:
                            _critter.FortitudeSave.Deltas.Add(_resist);
                            break;
                        case SaveType.Reflex:
                            _critter.ReflexSave.Deltas.Add(_resist);
                            break;
                        case SaveType.Will:
                            _critter.WillSave.Deltas.Add(_resist);
                            break;
                    }

                    if (_save.Success(new Interaction(_critter, _Slow, _critter, _save)))
                    {
                        if (_Slow.Source is MagicPowerEffect _mpe)
                        {
                            _mpe.Eject();
                        }
                        else
                        {
                            _Slow.Eject();
                        }
                    }

                    // done with this bonus
                    _resist.DoTerminate();
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
    }

    /// <summary>+bonus resistance against slow</summary>
    [Serializable]
    public class RemoveSlowSave : IQualifyDelta
    {
        /// <summary>+bonus resistance against slow</summary>
        public RemoveSlowSave(int bonus)
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(bonus, typeof(Resistance), @"Remove Slow");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            if (qualify?.Source is SlowEffect)
            {
                yield return _Delta;
            }
            yield break;
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
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
    }
}
