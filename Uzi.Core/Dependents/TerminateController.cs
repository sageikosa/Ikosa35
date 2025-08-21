using System;
using System.Collections.ObjectModel;

namespace Uzi.Core
{
    [Serializable]
    public class TerminateController: IControlTerminate, ISourcedObject
    {
        public TerminateController(object source)
        {
            _Src = source;
            _Dependents = [];
        }

        private Collection<IDependOnTerminate> _Dependents;

        protected object _Src;
        public object Source { get { return _Src; } }
        public int TerminateSubscriberCount => _Dependents.Count;

        public void DoTerminate()
        {
            // note when terminating, typically the dependents will remove from the dependents list, so best to step through by index backwards
            for (var _tx = _Dependents.Count - 1; _tx >= 0; _tx--)
            {
                _Dependents[_tx].Terminate(Source);
            }
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            if (!_Dependents.Contains(subscriber))
            {
                _Dependents.Add(subscriber);
            }
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            if (_Dependents.Contains(subscriber))
            {
                _Dependents.Remove(subscriber);
            }
        }
        #endregion
    }
}
