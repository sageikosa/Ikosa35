using System;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class SuccessCheckTarget : AimTarget
    {
        public SuccessCheckTarget(string key, SuccessCheck check)
            : base(key, null)
        {
            _Check = check;
            _Roll = null;
            _Result = 0;
        }

        #region private data
        private SuccessCheck _Check;
        private Deltable _Roll;
        private bool _Penalty;
        private int _Result;
        #endregion

        public SuccessCheck Check { get { return _Check; } }

        /// <summary>Indicates that the penalty has been applied, so the benefit can be used</summary>
        public bool IsUsingPenalty { get { return _Penalty; } set { _Penalty = value; } }

        #region public Deltable CheckRoll { get; set; }
        public Deltable CheckRoll
        {
            get { return _Roll; }
            set
            {
                // clear any attachments
                Check.CheckQualified.DoTerminate();

                // set
                _Roll = value;

                // attach
                if (_Roll != null)
                {
                    _Roll.Deltas.Add(Check.CheckQualified);
                    _Result = CheckRoll.QualifiedValue(new Interaction(null, Check.Source, null, null), 
                        Deltable.GetDeltaCalcNotify(Guid.Empty, @"Success Check").DeltaCalc);
                }
            }
        }
        #endregion

        public int Result { get { return _Result; } }

        /// <summary>Test for soft failure</summary>
        /// <param name="offset">number subtracted from difficulty to test for soft failure</param>
        /// <returns></returns>
        public bool SoftFail(int offset)
        {
            return (_Result < Check.Difficulty) && (_Result >= Check.Difficulty - offset);
        }

        public bool Success { get { return (_Result >= Check.Difficulty); } }

        public override AimTargetInfo GetTargetInfo()
        {
            return new SuccessCheckTargetInfo
            {
                IsUsingPenalty = IsUsingPenalty,
                CheckRoll = (CheckRoll != null) ? CheckRoll.EffectiveValue : (int?)null,
                Key = Key,
                TargetID = (Target != null) ? (Guid?)Target.ID : null
            };
        }
    }
}
