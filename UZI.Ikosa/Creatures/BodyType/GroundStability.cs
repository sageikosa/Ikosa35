using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Creatures.BodyType
{
    /// <summary>+4 against trip or bull rush</summary>
    [Serializable]
    public class GroundStability : IQualifyDelta
    {
        /// <summary>+4 against trip or bull rush</summary>
        public GroundStability(Body body)
        {
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(4, body, @"Body Characterisic");
        }

        private IDelta _Delta;
        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // TODO: ensure grounded...
            if ((qualify.Source as Type) == typeof(Trip)) // || BullRush
            {
                yield return _Delta;
            }
            yield break;
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        private readonly TerminateController _Terminator;
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }
}
