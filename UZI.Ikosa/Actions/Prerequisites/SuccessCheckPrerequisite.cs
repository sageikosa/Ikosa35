using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Target fulfills when a success check must be made. [Serial]</summary>
    [Serializable]
    public class SuccessCheckPrerequisite : StepPrerequisite, ISuccessCheckPrerequisite
    {
        #region Construction
        /// <summary>When a success check must be made to perform an action. [Serial]</summary>
        public SuccessCheckPrerequisite(object source, Qualifier workSet, string key, string name,
            SuccessCheck check, bool canFail)
            : base(source, workSet, key, name)
        {
            _Check = check;
            _Roll = null;
            _Penalty = false;
            _CanFail = canFail;
            _CanTake10 = true;

            if ((_Check != null) && (workSet != null) && (workSet.Actor != null))
            {
                // automatic take 10 settings?
                var _critter = workSet.Actor as Creature;
                var _type = _Check.CheckQualified.SupplyQualifyDelta.GetType();
                if (_critter.IsTake10InEffect(_type))
                {
                    CheckRoll = new Deltable(10);
                }
            }
        }

        /// <summary>When a success check must be made to perform an action. [Serial]</summary>
        public SuccessCheckPrerequisite(object source, Qualifier workSet, string key, string name,
            SuccessCheck check, bool canFail, bool canTake10)
            : base(source, workSet, key, name)
        {
            _Check = check;
            _Roll = null;
            _Penalty = false;
            _CanFail = canFail;
            _CanTake10 = canTake10;

            if (CanTake10
                && (_Check != null) && (workSet != null) && (workSet.Actor != null))
            {
                // automatic take 10 settings?
                var _critter = workSet.Actor as Creature;
                var _type = _Check.CheckQualified.SupplyQualifyDelta.GetType();
                if (_critter.IsTake10InEffect(_type))
                {
                    CheckRoll = new Deltable(10);
                }
            }
        }
        #endregion

        #region data
        private SuccessCheck _Check;
        private Deltable _Roll;
        private int _Result;
        private bool _CanFail;
        private bool _CanTake10;
        private bool _Penalty;
        #endregion

        public SuccessCheck Check => _Check;
        public bool CanTake10 => _CanTake10;

        #region public Deltable CheckRoll { get; set; }
        public Deltable CheckRoll
        {
            get { return _Roll; }
            set
            {
                // clear attachments
                Check.CheckQualified.DoTerminate();

                // set
                _Roll = value;

                // attach
                if (_Roll != null)
                {
                    _Roll.Deltas.Add(Check.CheckQualified);
                    _Result = CheckRoll.QualifiedValue(Qualification, Deltable.GetDeltaCalcNotify(Fulfiller?.ID, $@"{Name} (Success Check)").DeltaCalc);
                }
            }
        }
        #endregion

        public int Result => _Result;

        public override bool IsReady
            => CheckRoll != null;

        public override bool IsSerial
            => true;

        /// <summary>IStep member</summary>
        public override bool FailsProcess
            => !Success && _CanFail;

        public override CoreActor Fulfiller
            => Qualification?.Target as CoreActor;

        /// <summary>Indicates that the penalty has been applied, so the benefit can be used</summary>
        public bool IsUsingPenalty { get { return _Penalty; } set { _Penalty = value; } }

        public bool Success
            => (CheckRoll != null) && (_Result >= Check.Difficulty);

        /// <summary>Use a positive number by which the check can fail for a softer failure mode</summary>
        public bool SoftFail(int offset)
            => (CheckRoll != null)
            ? (_Result < Check.Difficulty) && (_Result >= Check.Difficulty - offset)
            : false;

        public SuccessCheckTarget ToTarget()
            => new SuccessCheckTarget(BindKey, Check)
            {
                CheckRoll = CheckRoll,
                IsUsingPenalty = IsUsingPenalty
            };

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<CheckPrerequisiteInfo>(step);
            _info.VoluntaryPenalty = Check.VoluntaryPenalty;
            _info.IsUsingPenalty = false;
            if (IsReady)
            {
                _info.Value = CheckRoll.EffectiveValue;
            }
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            if ((info is CheckPrerequisiteInfo _checkInfo)
                && _checkInfo.Value.HasValue)
            {
                IsUsingPenalty = _checkInfo.IsUsingPenalty;
                CheckRoll = new Deltable(_checkInfo.Value.Value);
            }
        }
    }
}
