using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Adjuncts
{
    // NOTE: workflow: 
    // 1 - interaction (secret FORT roll)
    // 2 - leads to adjunct binding
    //   a - NOTE! diseased must be bound by either supernatural or extraordinary binders
    // 3 - then when time-track sweep
    // 4 - disease-damage process with either
    //   a - immediate damage after initial incubation
    //   b - ¿¿¿ public ??? FORT save (since damage is now apparent)

    /// <summary>indicates a creature has been diseased</summary>
    [Serializable]
    public class Diseased : Adjunct, ITrackTime
    {
        /// <summary>indicates a creature has been diseased</summary>
        public Diseased(Disease source, double nextTime)
            : base(source)
        {
            _Incubating = true;
            _NextTime = nextTime;
            _SaveSuccess = 0;
        }

        #region data
        private bool _Incubating;
        private double _NextTime;
        private int _SaveSuccess;
        #endregion

        public Disease Disease => Source as Disease;
        public bool Incubating => _Incubating;
        public double NextTime => _NextTime;

        public int SaveSuccess
        {
            get { return _SaveSuccess; }
            set
            {
                // TODO: subsequent successes *may* eject the disease (usually 2, sometimes 3, sometimes never)
                _SaveSuccess = value;
                if (Disease.SaveSuccessToHeal.HasValue && (_SaveSuccess >= Disease.SaveSuccessToHeal.Value))
                {
                    Eject();
                }
            }
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= _NextTime) && (direction == TimeValTransition.Leaving))
            {
                // calculate next time
                _NextTime += Day.UnitFactor;

                if (Incubating)
                {
                    // take damage when incubation period is done
                    _Incubating = false;
                    Anchor?.StartNewProcess(new DiseasedDamageStep(this), @"Disease Damage");
                }
                else
                {
                    // subsequent days, save for damage or build-up resistance
                    Anchor?.StartNewProcess(new DiseasedSaveStep(this), @"Disease Save");
                }
            }
        }

        public double Resolution
        {
            get
            {
                if (Incubating)
                {
                    return Disease.IncubationUnitFactor.BaseUnitFactor;
                }
                else
                {
                    return Day.UnitFactor;
                }
            }
        }

        #endregion

        public override object Clone()
        {
            return new Diseased(Disease, NextTime);
        }
    }

    [Serializable]
    public class DiseasedSaveStep : PreReqListStepBase
    {
        #region ctor()
        public DiseasedSaveStep(Diseased diseased)
            : base((CoreProcess)null)
        {
            _Critter = null;
            _Disease = null;
            _Diseased = diseased;
            var _qualifier = new Qualifier(null, Diseased.Disease, Diseased.Anchor as IInteract);
            _PendingPreRequisites.Enqueue(new SavePrerequisite(Diseased.Disease, _qualifier,
                @"Disease", @"Save versus Disease", new SaveMode(SaveType.Fortitude, SaveEffect.Negates,
                    Diseased.Disease.Difficulty.GetDeltaCalcInfo(_qualifier, @"Disease Save Difficulty"))));
        }

        public DiseasedSaveStep(StepInteraction stepInteract, Disease disease, Creature critter)
            : base(stepInteract.Step)
        {
            _Critter = critter;
            _Disease = disease;
            _Diseased = null;
            var _qualifier = new Qualifier(null, _Disease, _Critter);
            _PendingPreRequisites.Enqueue(new SavePrerequisite(_Disease, _qualifier,
                @"Disease", @"Save versus Disease", new SaveMode(SaveType.Fortitude, SaveEffect.Negates,
                 Disease.Difficulty.GetDeltaCalcInfo(_qualifier, @"Disease Save Difficulty")), true));
        }
        #endregion

        #region data
        private Creature _Critter;
        private Disease _Disease;

        private Diseased _Diseased;
        #endregion

        public SavePrerequisite Save
            => GetPrerequisite<SavePrerequisite>();

        /// <summary>If not initialized with a creature, this represents a full-on infection</summary>
        public bool IsInfected => _Critter == null;

        public Creature Creature => _Critter;
        public Disease Disease => _Disease;

        public Diseased Diseased => _Diseased;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (Save.Success)
            {
                if (IsInfected)
                {
                    Diseased.SaveSuccess += 1;
                }

                // success means disease doesn't take hold
            }
            else
            {
                if (!IsInfected)
                {
                    // save failure means diease takes hold
                    AppendFollowing(new DiseaseContractStep(Disease, Creature));
                }
                else
                {
                    // save failure means taking damage
                    Diseased.SaveSuccess = 0;
                    AppendFollowing(new DiseasedDamageStep(Diseased));
                }
            }
            return true;
        }
        #endregion
    }

    [Serializable]
    public class DiseaseContractStep : PreReqListStepBase
    {
        #region ctor()
        public DiseaseContractStep(Disease disease, Creature creature)
            : base((CoreProcess)null)
        {
            _Disease = disease;
            _Critter = creature;
            var _units = disease.IncubationRoller;
            if (!((_units is ConstantRoller) || (_units == null)))
            {
                _PendingPreRequisites.Enqueue(new RollPrerequisite(disease, @"Duration", @"Duration", _units, true));
            }
        }
        #endregion

        #region data
        private Disease _Disease;
        private Creature _Critter;
        #endregion

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (IsComplete)
            {
                return true;
            }

            var _roll = GetPrerequisite<RollPrerequisite>();
            if (_roll != null)
            {
                if (_roll.IsReady)
                {
                    var _timeFactor = _Disease.IncubationUnitFactor.BaseUnitFactor;
                    var _span = _roll.RollValue * _timeFactor;
                    var _curr = _Critter?.GetCurrentTime() ?? 0;
                    _Critter.AddAdjunct(new Diseased(_Disease, _curr + _span));
                }
                else
                {
                    return false;
                }
            }
            else
            {
                // check for constant roller...
                var _units = _Disease.IncubationRoller;
                if (_units is ConstantRoller)
                {
                    var _timeFactor = _Disease.IncubationUnitFactor.BaseUnitFactor;
                    var _span = _units.RollValue(Guid.Empty, @"Diseased", _Disease.IncubationUnitFactor.PluralName) * _timeFactor;
                    var _curr = _Critter?.GetCurrentTime() ?? 0;
                    _Critter.AddAdjunct(new Diseased(_Disease, _curr + _span));
                }
                else
                {
                    _Critter.AddAdjunct(new Diseased(_Disease, double.MaxValue));
                }
            }
            return true;
        }
        #endregion
    }

    [Serializable]
    public class DiseasedDamageStep : PreReqListStepBase
    {
        public DiseasedDamageStep(Diseased diseased)
            : base((CoreProcess)null)
        {
            _Diseased = diseased;
            foreach (var _rPrereq in (from _roll in Diseased.Disease.GetDamageRollers()
                                      select new RollPrerequisite(Diseased.Disease,
                                            new Interaction(null, Diseased.Anchor, null, null),
                                            null, _roll.Key, _roll.Name, _roll.Roller, false)))
            {
                _PendingPreRequisites.Enqueue(_rPrereq);
            }
        }

        #region data
        private Diseased _Diseased;
        #endregion

        public Diseased Diseased => _Diseased;

        #region protected override bool OnDoStep()
        protected override bool OnDoStep()
        {
            if (IsComplete)
            {
                return true;
            }

            var _rollers = (from _roll in Diseased.Disease.GetDamageRollers()
                            let _pre = AllPrerequisites<RollPrerequisite>(_roll.Key).FirstOrDefault()
                            where _pre != null
                            select _pre.RollValue).ToArray();

            // perform primary adjunct
            var _dmg = Diseased.Disease.ApplyDamage(this, Diseased.Anchor as Creature, _rollers);

            // and done
            EnqueueNotify(new RefreshNotify(true, true, true, false, false), Diseased.Anchor.ID);

            // clear prereqs
            return true;
        }
        #endregion
    }
}
