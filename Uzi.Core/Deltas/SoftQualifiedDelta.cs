using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    /// <summary>Best used to wrap qualified deltas for transient deltable properties, does not monitor base IQualifyDelta</summary>
    [Serializable]
    public class SoftQualifiedDelta : IQualifyDelta
    {
        /// <summary>Best used to wrap qualified deltas for transient deltable properties, does not monitor base IQualifyDelta</summary>
        public SoftQualifiedDelta(ISupplyQualifyDelta delta)
        {
            _Delta = delta;
            _Term = new TerminateController(this);
        }

        #region data
        private readonly ISupplyQualifyDelta _Delta;
        private readonly TerminateController _Term;
        #endregion

        public ISupplyQualifyDelta SupplyQualifyDelta => _Delta; 

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _Delta.QualifiedDeltas(qualify);

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
    }
}
