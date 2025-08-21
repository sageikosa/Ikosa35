using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class TempHPChunk : IDependOnTerminate, ISourcedObject
    {
        #region Construction
        public TempHPChunk(TempHPSet set, Delta delta)
        {
            _Set = set;
            _Value = delta.Value;
            _Delta = delta;
            delta.AddTerminateDependent(this);
        }
        #endregion

        #region Private Data
        private TempHPSet _Set;
        private int _Value;
        private Delta _Delta;
        #endregion

        public Delta Delta { get { return _Delta; } }
        public object Source { get { return _Delta.Source; } }
        public int Value { get { return _Value; } set { _Value = value; } }

        public void DoTerminate()
        {
            _Delta.RemoveTerminateDependent(this);
            if (_Set != null)
            {
                _Set.PruneChunks();
            }
        }

        #region IDependOnTerminate Members
        public void Terminate(object sender)
        {
            _Value = 0;
            DoTerminate();
        }
        #endregion
    }
}
