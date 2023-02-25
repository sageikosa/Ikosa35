using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    /// <summary>Derived from ConstDeltable, and implementing IQualifyDelta.  Thus it modifies and can be modified.</summary>
    [Serializable]
    public class DeltableQualifiedDelta : ConstDeltable, IQualifyDelta
    {
        #region construction
        public DeltableQualifiedDelta(int seedValue, string name, object source)
            : base(seedValue)
        {
            _Source = source;
            _TCtrl = new TerminateController(this);
            _Name = name;
            _Active = true;
        }
        #endregion

        #region data
        private readonly object _Source;
        private readonly string _Name;
        private bool _Active;
        private readonly TerminateController _TCtrl;
        #endregion

        public string Name => _Name;

        /// <summary>If false, no QualifiedDelta is provided and QualifiedValue returns 0</summary>
        public bool IsActive
        {
            get => _Active;
            set
            {
                _Active = value;
                DoPropertyChanged(nameof(IsActive));
            }
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _TCtrl.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _TCtrl.RemoveTerminateDependent(subscriber);
        }
        public int TerminateSubscriberCount => _TCtrl?.TerminateSubscriberCount ?? 0;
        #endregion
        #endregion

        // ISourcedObject
        public object Source => _Source;

        // IQualifyDelta 
        public override IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify, object baseSource, string baseName)
        {
            if (IsActive)
            {
                if (BaseValue != 0)
                {
                    yield return new QualifyingDelta(BaseValue, _Source, _Name);
                }
                foreach (var _del in Deltas.GetQualifiedDeltas(qualify))
                {
                    yield return _del;
                }
            }
            yield break;
        }

        public override int QualifiedValue(Qualifier qualification, DeltaCalcInfo calcInfo = null)
            => IsActive
            ? base.QualifiedValue(qualification, calcInfo)
            : 0;
    }
}
