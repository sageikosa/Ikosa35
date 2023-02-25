using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class SuccessCheckMarker : SuccessCheck
    {
        #region Construction
        public SuccessCheckMarker(ISupplyQualifyDelta checkQualify, int difficulty, ICore source, ActionTime reqAction, bool retry) :
            this(checkQualify, difficulty, source, 0, reqAction, retry)
        {
        }

        public SuccessCheckMarker(ISupplyQualifyDelta checkQualify, int difficulty, ICore source, int penaltyCost, ActionTime reqAction, bool retry)
            : base(checkQualify, difficulty, source, penaltyCost)
        {
            _ReqTime = reqAction;
            _Retry = retry;
        }
        #endregion

        #region Private Data
        private ActionTime _ReqTime;
        private bool _Retry;
        #endregion

        public ActionTime RequiredTime { get { return _ReqTime; } }
        public bool Retry { get { return _Retry; } }
    }
}
