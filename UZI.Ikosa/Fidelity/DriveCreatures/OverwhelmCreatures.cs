using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;
using Uzi.Core;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class OverwhelmCreatures : DriveCreaturePowerDef, IDurableCapable,
        IApplyPowerCapable<SuperNaturalPowerActionSource>, IGeneralSubMode, IPowerDeliveryCapable
    {
        public OverwhelmCreatures(int powerLevel, PowerBattery battery, ICreatureFilter filter)
            : base(powerLevel, battery, filter)
        {
        }

        #region public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerSource> burst, int classPowerLevel, int critterPowerLevel, AimTarget target)
        public override CoreStep CreateStep(PowerBurstCapture<SuperNaturalPowerActionSource> burst, int classPowerLevel,
            int critterPowerLevel, AimTarget target)
        {
            if ((burst.PowerActionSource.SuperNaturalPowerActionDef is IDriveCreaturePowerDef _driveDef)
                && (target.Target is Creature _targetCreature))
            {
                // value qualifier
                var _drive = new Interaction(burst.Activation.Actor, burst.PowerActionSource.PowerClass, null,
                    new DriveCreatureData(burst.Activation.Actor as Creature, _driveDef));
                var _filterKey = _driveDef.CreatureFilter.Key;

                // by default, not assuming we are commanding this creature ...
                var _commanding = false;

                // ... but this creature is probably commandable
                var _commandable = true;

                #region actor's command master and max available to command
                // max available
                // NOTE: the interaction Source is the PowerClass here (not the burst), so the calc may be different
                // NOTE: this means, it may be possible to command more creatures than classPowerLevel
                var _commandCapacity = (decimal)(burst.PowerActionSource.PowerClass.ClassPowerLevel.QualifiedValue(_drive));
                var _cmdMaster = (from _cm in burst.Activation.Actor.Adjuncts.OfType<CommandMaster>()
                                  where _cm.PowerClass.Equals(burst.PowerActionSource.PowerClass)
                                     && _cm.CommandGroup.OverwhelmCreatures.CreatureFilter.Key.Equals(_filterKey)
                                  select _cm).FirstOrDefault();
                if (_cmdMaster != null)
                {
                    // minus any already under command
                    _commandCapacity = _commandCapacity - _cmdMaster.CommandGroup.CurrentPowerDice;

                    // check if already commanding
                    _commanding = _cmdMaster.CommandGroup.CommandedCreatures.Contains(_targetCreature);
                }
                #endregion

                #region target's existing group and whether the target may be commandable regardless
                // if target is already commanded by another with a more powerful check...
                var _existCmd = _targetCreature.Adjuncts.OfType<CommandedCreature>().OrderByDescending(_cc => _cc.CheckValue).FirstOrDefault();
                if (_existCmd != null)
                {
                    if (_commanding)
                    {
                        // if skipping our existing command creatures, then not commandable
                        _commandable = !DriveCreature.IsSkipping(burst.Activation);
                    }
                    else
                    {
                        // commanded by other source
                        var _checkRoll = burst.Context.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) as ValueTarget<int>;
                        if (_existCmd.CheckValue > _checkRoll.Value)
                        {
                            _commandable = false;
                        }
                    }
                }
                #endregion

                // command or cower
                if (_commandable && (classPowerLevel >= (critterPowerLevel * 2)))
                {
                    // command: ApplyPower will determine whether permanent control will be applied
                    var _step = DeliverNextStep(burst.Activation, 0, target, true, 0);

                    // add check value and command
                    AddCheckValue(_step, burst);
                    AddCommandValue(_step, burst);
                    return _step;
                }
                else if (!_commanding)
                {
                    #region apply cowering
                    if ((target.Target is Creature _critter)
                        && _critter.Conditions.Any(_c => _c.Name.Equals(Condition.Cowering)
                            && _c.Source is OverwhelmCreatures))
                    {
                        // skip over cowering creatures
                        if (DriveCreature.IsSkipping(burst.Activation))
                            return null;
                    }

                    // attempt to apply cowering
                    var _actor = burst.Activation.Actor;
                    var _aLoc = _actor.GetLocated()?.Locator;
                    var _step = DeliverDurableNextStep(burst.Activation, _actor, _aLoc, _aLoc.PlanarPresence,
                        burst.Activation.PowerUse, burst.Activation.TargetingProcess.Targets, target, true, 0);
                    AddCheckValue(_step, burst);
                    return _step;
                    #endregion
                }
            }

            // fall-through
            return null;
        }
        #endregion

        #region protected static void AddCommandValue(CoreStep target, PowerBurstCapture<SuperNaturalPowerSource> source)
        /// <summary>Adds command value to effects or powerTransit interaction data depending on power use results</summary>
        protected static void AddCommandValue(CoreStep target, PowerBurstCapture<SuperNaturalPowerActionSource> source)
        {
            if (target is PowerApplyStep<SuperNaturalPowerActionSource>)
            {
                var _step = target as PowerApplyStep<SuperNaturalPowerActionSource>;
                var _powerTransit = _step.DeliveryInteraction.InteractData as PowerActionTransit<SuperNaturalPowerActionSource>;
                var _cmdPre = source.Activation.AllPrerequisites<AimTargetPrerequisite<CharacterStringAim, CharacterStringTarget>>(@"Overwhelm.Command").FirstOrDefault();
                if ((_cmdPre != null) && (_powerTransit != null))
                {
                    var _cmdTarget = new ValueTarget<string>(@"Overwhelm.Command", _cmdPre.AimingTargets.FirstOrDefault().CharacterString);
                    if (_powerTransit != null)
                    {
                        // add command value to power transit (for ApplyPower)
                        _powerTransit.AllTargets.Add(_cmdTarget);
                    }
                }
            }
        }
        #endregion

        public override string DisplayName { get { return string.Format(@"Overwhelm {0}", CreatureFilter.Description); } }

        #region public override string Description { get; }
        public override string Description
        {
            get { return string.Format(@"Overwhelm or command {0}", CreatureFilter.Description); }
        }
        #endregion

        #region IDurableMode Members

        #region public IEnumerable<int> DurableSubModes { get; }
        public IEnumerable<int> DurableSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public object Activate(IAdjunctTracker source, IAdjunctable target, int subMode, object activateSource)
        {
            if ((target is Creature _critter) && (source is MagicPowerEffect _magicEffect))
            {
                if (_magicEffect.SubMode == 0)
                {
                    // cowering...
                    var _condition = new Condition(Condition.Cowering, this);
                    _critter.Conditions.Add(_condition);
                    return _condition;
                }
            }
            return null;
        }

        public void Deactivate(IAdjunctTracker source, IAdjunctable target, int subMode, object deactivateSource)
        {
            if ((target is Creature _critter) && (source is MagicPowerEffect _magicEffect))
            {
                if (_magicEffect.SubMode == 0)
                {
                    // remove cowering condition
                    var _condition = source.ActiveAdjunctObject as Condition;
                    _critter.Conditions.Remove(_condition);
                }
            }
        }

        public bool IsDismissable(int subMode) { return false; }
        public string DurableSaveKey(IEnumerable<AimTarget> targets, Interaction workSet, int subMode) { return string.Empty; }
        public IEnumerable<StepPrerequisite> GetDurableModePrerequisites(int subMode, Interaction interact) { yield break; }

        public DurationRule DurationRule(int subMode)
        {
            // always 10 rounds
            return new DurationRule(DurationType.Span, new SpanRulePart(10, new Round()));
        }

        #endregion

        #region IGeneralSubMode Members

        #region public IEnumerable<int> GeneralSubModes
        public IEnumerable<int> GeneralSubModes
        {
            get
            {
                yield return 0;
                yield break;
            }
        }
        #endregion

        public string GeneralSaveKey(CoreTargetingProcess targetProcess, Interaction workSet, int subMode) { return null; }
        public IEnumerable<StepPrerequisite> GetGeneralSubModePrerequisites(int subMode, Interaction interact)
        {
            yield break;
        }

        #endregion

        #region IApplyPowerMode<SuperNaturalPowerSource> Members

        public void ApplyPower(PowerApplyStep<SuperNaturalPowerActionSource> step)
        {
            var _powerSource = step.PowerUse.PowerActionSource;
            if ((step.DeliveryInteraction.Target is Creature _critter)
                && (step.DeliveryInteraction.InteractData is PowerActionTransit<SuperNaturalPowerActionSource> _transit)
                && (_powerSource.SuperNaturalPowerActionDef is IDriveCreaturePowerDef _driveDef))
            {
                // value qualifier
                var _actor = step.Actor;
                var _drive = new Interaction(_actor, _powerSource.PowerClass, null,
                    new DriveCreatureData(_actor as Creature, _driveDef));
                var _filterKey = _driveDef.CreatureFilter.Key;

                // get parameters calculated and gathered
                if ((_transit.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"Overwhelm.Command")) is ValueTarget<string> _issuedCommand)
                    && (_transit.AllTargets.FirstOrDefault(_t => _t.Key.Equals(@"Roll.CheckValue")) is ValueTarget<int> _check))
                {
                    #region actor's command master and max available to command
                    // max available
                    // NOTE: the interaction Source is the PowerClass here (not the burst), so the calc may be different
                    // NOTE: this means, it may be possible to command more creatures than classPowerLevel
                    var _commandCapacity = (decimal)(_powerSource.PowerClass.ClassPowerLevel.QualifiedValue(_drive));
                    var _cmdMaster = (from _cm in _actor.Adjuncts.OfType<CommandMaster>()
                                      where _cm.PowerClass.Equals(_powerSource.PowerClass)
                                         && _cm.CommandGroup.OverwhelmCreatures.CreatureFilter.Key.Equals(_filterKey)
                                      select _cm).FirstOrDefault();
                    if (_cmdMaster != null)
                    {
                        // minus any already under command
                        _commandCapacity = _commandCapacity - _cmdMaster.CommandGroup.CurrentPowerDice;
                    }
                    else
                    {
                        // create group, and master (in group)
                        var _cmdGroup = new CommandGroup(_driveDef.SeedPowerDef as OverwhelmCreatures);
                        _cmdMaster = new CommandMaster(_powerSource.PowerClass, _cmdGroup);

                        // add master to actor, and group to context
                        step.Actor.AddAdjunct(_cmdMaster);
                    }
                    #endregion

                    #region cleanup and update existing commands
                    var _ours = false;
                    foreach (var _existCmd in _critter.Adjuncts.OfType<CommandedCreature>().ToList())
                    {
                        if ((_cmdMaster == null) || (_existCmd.CommandGroup.Commander != _cmdMaster))
                        {
                            // remove existing command, since it cannot be ours
                            _existCmd.Eject();
                        }
                        else
                        {
                            // update existing if greater
                            if (_check.Value > _existCmd.CheckValue)
                                _existCmd.CheckValue = _check.Value;
                            _existCmd.Command = _issuedCommand.Value;
                            _ours = true;
                        }
                    }
                    #endregion

                    // one of them was ours
                    if (_ours)
                        return;

                    // need a new adjunct
                    var _critterLevel = _critter.AdvancementLog.PowerLevel.QualifiedValue(_drive);
                    if (_commandCapacity >= _critterLevel)
                    {
                        // associate with group
                        var _newCmd = new CommandedCreature(_powerSource, _cmdMaster.CommandGroup, _check.Value, _issuedCommand.Value);
                        _critter.AddAdjunct(_newCmd);
                    }
                    else
                    {
                        // new one-shot
                        var _oneShot = new LastCommand(typeof(OverwhelmCreatures), _issuedCommand.Value, _check.Value);
                        _critter.AddAdjunct(_oneShot);
                    }
                }
            }
        }

        #endregion

        #region IPowerDeliveryMode Members

        public IEnumerable<StepPrerequisite> PowerDeliveryPrerequisites(CoreTargetingProcess targetProcess, CoreActor actor)
        {
            // provide command
            yield return new AimTargetPrerequisite<CharacterStringAim, CharacterStringTarget>(
                new CharacterStringAim(@"Overwhelm.Command", @"Command to Issue", new FixedRange(1), new FixedRange(100)),
                    actor, this, null, @"Overwhelm.Command", @"Command to Issue", false);
            yield break;
        }

        #endregion
    }
}