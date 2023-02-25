using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public abstract class ProficiencyDelta : IQualifyDelta
    {
        protected ProficiencyDelta(ProficiencySet profSet)
        {
            _Set = profSet;
            _Term = new TerminateController(this);
        }

        protected ProficiencySet _Set;
        private readonly TerminateController _Term;

        #region IQualifyDelta Members
        public abstract IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify);
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
    }
}
