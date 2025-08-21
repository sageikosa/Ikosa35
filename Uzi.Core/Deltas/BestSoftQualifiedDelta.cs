using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Core
{
    /// <summary>
    /// Only supply bonuses, penalties will be ignored
    /// </summary>
    [Serializable]
    public class BestSoftQualifiedDelta : IQualifyDelta
    {
        /// <summary>Best used to wrap qualified deltas for transient deltable properties, does not monitor base IQualifyDelta</summary>
        public BestSoftQualifiedDelta(params ConstDeltable[] deltas)
        {
            if (deltas?.Any() ?? false)
            {
                _Deltas = deltas.ToList();
            }
            else
            {
                _Deltas = [];
            }

            _Term = new TerminateController(this);
        }

        private readonly List<ConstDeltable> _Deltas;
        private readonly TerminateController _Term;

        public IEnumerable<ISupplyQualifyDelta> Deltas
            => _Deltas.Select(_d => _d).ToList();

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // get const deltable that supplies the best value
            var _supply = (from _d in _Deltas
                           let _q = _d.QualifiedValue(qualify)
                           orderby _q descending
                           select _d).FirstOrDefault();
            if (_supply != null)
            {
                // all deltas for this deltable
                foreach (var _q in _supply.QualifiedDeltas(qualify, this, @"base"))
                {
                    yield return _q;
                }
            }
            yield break;
        }

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
