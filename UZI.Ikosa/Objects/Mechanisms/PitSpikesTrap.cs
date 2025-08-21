using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Core.Dice;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Actions;
using Uzi.Visualize;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class PitSpikesTrap : LocatableObject, ILocatorZone
    {
        #region Construction
        public PitSpikesTrap(string name, MapContext mapContext, Geometry spikeSpot)
            : base(name, true)
        {
            // NOTE: forcing to material, no gravity (generally) on ethereal plane, so falling seems pointless...
            _Capture = new LocatorCapture(mapContext, this, spikeSpot, this, true, PlanarPresence.Material);
            _BAtk = new BaseAttackValue();
            _BAtk.Deltas.Add(new Delta(10, this));
            _Spike = new Dagger();
            _MaxSpikes = new ComplexDiceRoller(@"1d4");
            AddAdjunct(new TrapPart(true));
        }
        #endregion

        #region private data
        private BaseAttackValue _BAtk;
        private Roller _MaxSpikes;
        private Dagger _Spike;
        private LocatorCapture _Capture;
        #endregion

        /// <summary>Represents the attack bonus of the pit spikes</summary>
        public BaseAttackValue BaseAttack => _BAtk;

        /// <summary>Maximum spikes attacking any falling creature entering the capture area</summary>
        public Roller MaxSpikes => _MaxSpikes;

        /// <summary>The dagger template is made public so it can be replaced and/or adjusted</summary>
        public Dagger Spike { get { return _Spike; } set { _Spike = value; } }

        #region ILocatorZone Members
        public void Start(Locator locator) { }
        public void End(Locator locator) { }

        public void Enter(Locator locator)
        {
            if (locator.ActiveMovement is FallMovement)
            {
                var _process = new CoreProcess(new PitSpikeCountStep(this, locator), @"Pit Spikes");
                locator.MapContext.ContextSet.ProcessManager.StartProcess(_process);
            }
        }

        public void Exit(Locator locator) { }
        public void Capture(Locator locator) { }
        public void Release(Locator locator) { }
        public void MoveInArea(Locator locator, bool followOn) { }
        public bool IsActive => true;
        #endregion

        #region IControlChange<Activation> Members
        public void AddChangeMonitor(IMonitorChange<Activation> monitor)
        {
        }

        public void RemoveChangeMonitor(IMonitorChange<Activation> monitor)
        {
        }
        #endregion

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => null;

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        public override Sizer Sizer
            => new ObjectSizer(Size.Large, this);

        public override IGeometricSize GeometricSize
            => new GeometricSize(_Capture.Geometry.Region);

        protected override string ClassIconKey
            => nameof(PitSpikesTrap);

        public override bool IsTargetable => false;
    }

    [Serializable]
    /// <summary>Determines how many spikes hit the target during the fall into the pit</summary>
    public class PitSpikeCountStep : PreReqListStepBase
    {
        #region construction
        public PitSpikeCountStep(PitSpikesTrap pitSpikes, Locator target)
            : base((CoreProcess)null)
        {
            Spikes = pitSpikes;
            _Roll = new RollPrerequisite(this, new Interaction(null, Spikes, null, null), null,
                @"Spike#", @"Spike Count", Spikes.MaxSpikes, true);
            _PendingPreRequisites.Enqueue(_Roll);
            SpikesRemaining = 0;
            _Target = target;
        }
        #endregion

        #region private data
        private RollPrerequisite _Roll;
        private Locator _Target;
        #endregion

        public PitSpikesTrap Spikes { get; private set; }
        public Locator Target => _Target;

        /// <summary>Number of spikes left to attack from this counter</summary>
        public int SpikesRemaining { get; set; }

        protected override bool OnDoStep()
        {
            // however many rolled is how many we need to step through
            var _total = _Roll.RollValue;
            SpikesRemaining = _total;
            while (SpikesRemaining > 0)
            {
                AppendFollowing(new PitSpikeAttackStep(this, (_total - SpikesRemaining) + 1, _total));
                SpikesRemaining--;
            }
            return true;
        }
    }

    [Serializable]
    public class PitSpikeAttackStep : PreReqListStepBase
    {
        #region construction
        public PitSpikeAttackStep(PitSpikeCountStep counter, int spikeIndex, int spikeCount) :
            base(counter)
        {
            Counter = counter;
            _CustomDispense = true;
            _Hit = false;
            _Critical = false;
            _SpikeIndex = spikeIndex;
            _SpikeCount = spikeCount;
        }
        #endregion

        #region private data
        private bool _CustomDispense;
        private bool _Hit;
        private bool _Critical;
        private int _SpikeIndex;
        private int _SpikeCount;
        #endregion

        public RollPrerequisite AttackRoll
            => AllPrerequisites<RollPrerequisite>(@"PitSpike.Attack").FirstOrDefault();

        public RollPrerequisite CriticalRoll
            => AllPrerequisites<RollPrerequisite>(@"PitSpike.Critical").FirstOrDefault();

        public PitSpikeCountStep Counter { get; private set; }

        public override bool IsDispensingPrerequisites
            => _CustomDispense || base.IsDispensingPrerequisites;

        #region public override StepPrerequisite NextPrerequisite()
        protected override StepPrerequisite OnNextPrerequisite()
        {
            if (_CustomDispense)
            {
                #region Attack Roll
                var _attackRoll = AttackRoll;
                if (_attackRoll == null)
                {
                    // dispense attack roll
                    _attackRoll = new RollPrerequisite(this, new Interaction(null, Counter.Spikes, null, null), null,
                        @"PitSpike.Attack", @"Pit Spike Attack", new DieRoller(20), true);
                    _PendingPreRequisites.Enqueue(_attackRoll);
                    return base.OnNextPrerequisite();
                }

                if (!_attackRoll.IsReady)
                {
                    return null;
                }

                if (_attackRoll.RollValue == 1)
                {
                    // done with prerequisite, since it missed
                    _CustomDispense = false;
                    return null;
                }
                #endregion

                if (_attackRoll.RollValue != 1)
                {
                    #region Critical Roll
                    var _criticalRoll = CriticalRoll;
                    if (_attackRoll.RollValue == 20)
                    {
                        // NOTE: critical is serial...and conditional on attack roll value of 20
                        if (_criticalRoll == null)
                        {
                            // if a critical threat, dispense critical confirmation
                            _criticalRoll = new RollPrerequisite(this, new Interaction(null, Counter.Spikes, null, null), null,
                                @"PitSpike.Critical", @"Pit Spike Critical", new DieRoller(20), true);
                            _PendingPreRequisites.Enqueue(_criticalRoll);
                            return base.OnNextPrerequisite();
                        }
                        else if (!_criticalRoll.IsReady)
                        {
                            return null;
                        }
                    }
                    #endregion

                    #region Spikes Attacks
                    // build interaction
                    var _score = new Deltable(_attackRoll.RollValue);
                    _score.Deltas.Add(Counter.Spikes.BaseAttack);
                    MeleeAttackData _mAtk;
                    var _loc = Counter.Spikes.GetLocated().Locator;
                    var _cell = _loc.GeometricRegion.AllCellLocations().FirstOrDefault();
                    if (_criticalRoll != null)
                    {
                        var _crit = new Deltable(_criticalRoll.RollValue);
                        _crit.Deltas.Add(Counter.Spikes.BaseAttack);
                        _mAtk = new MeleeAttackData(null, null, _loc, AttackImpact.Penetrating, _score, _crit, false, _cell, _cell, _SpikeIndex, _SpikeCount);
                    }
                    else
                    {
                        _mAtk = new MeleeAttackData(null, null, _loc, AttackImpact.Penetrating, _score, false, _cell, _cell, _SpikeIndex, _SpikeCount);
                    }

                    // handle attack interaction
                    var _meleeInteract = new Interaction(null, Counter.Spikes.Spike, Counter.Target.Chief, _mAtk);
                    Counter.Target.Chief.HandleInteraction(_meleeInteract);
                    #endregion

                    // process feedback: roll for damage if successful
                    var _feedback = _meleeInteract.Feedback.OfType<AttackFeedback>().FirstOrDefault();
                    if (_feedback?.Hit ?? false)
                    {
                        _Hit = true;
                        _Critical = _feedback.CriticalHit;
                        foreach (var _dRoll in Counter.Spikes.Spike.MainHead.DamageRollers(_meleeInteract))
                        {
                            // add a prerequisite for each spike
                            _PendingPreRequisites.Enqueue(_dRoll);
                        }
                    }

                }

                // All done custom dispensing
                _CustomDispense = false;
            }

            return base.OnNextPrerequisite();
        }
        #endregion

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            // NOTE: this is based on WeaponHead processing (should probably generalize this for other traps)

            var _target = Counter.Target.Chief;
            if (_Hit && (_target != null))
            {
                // collect damage
                var _damages = (from _dmgRoll in AllPrerequisites<DamageRollPrerequisite>()
                                from _dmg in _dmgRoll.GetDamageData()
                                select _dmg).ToList();

                // extra damage effects due to the weapon...
                var _sources = new List<object>();
                var _secondaries = new List<ISecondaryAttackResult>();
                foreach (var _processor in from _proc in Counter.Spikes.Adjuncts.OfType<ISecondaryAttackResult>()
                                           where _proc.PoweredUp
                                           select _proc)
                {
                    if (!_sources.Contains(_processor.AttackResultSource))
                    {
                        _secondaries.Add(_processor);
                        _sources.Add(_processor.AttackResultSource);
                    }
                }

                // build and notify
                var _deliverDamage = new DeliverDamageData(null, _damages, false, _Critical);
                if (_secondaries.Any())
                {
                    _deliverDamage.Secondaries.AddRange(_secondaries);
                }

                // interact
                var _dmgInteract = new StepInteraction(this, null, this, _target, _deliverDamage);
                _target.HandleInteraction(_dmgInteract);
                if (_dmgInteract.Feedback.OfType<PrerequisiteFeedback>().Any())
                {
                    new RetryInteractionStep(this, @"Retry", _dmgInteract);
                }
            }
            else
            {
                // ATTACK missed
            }
            return true;
        }
        #endregion

    }
}
