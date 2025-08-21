using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreTurnTick
    {
        protected CoreTurnTick(CoreTickTracker tracker, bool addToTail)
        {
            _Tracker = tracker;
            if (addToTail)
            {
                _Tracker.Ticks.AddLast(this);
            }
            else
            {
                _Tracker.Ticks.AddFirst(this);
            }
        }

        protected CoreTurnTick(CoreTickTracker tracker, LinkedListNode<CoreTurnTick> refNode)
        {
            _Tracker = tracker;
            if (refNode != null)
            {
                _Tracker.Ticks.AddBefore(refNode, this);
            }
            else
            {
                _Tracker.Ticks.AddLast(this);
            }
        }

        #region private data
        private CoreTickTracker _Tracker;
        #endregion

        public CoreTickTracker Tracker { get { return _Tracker; } }

        /// <summary>Moves the tick to the end of the list (virtual)</summary>
        public virtual void EndOfTick()
        {
            LinkedListNode<CoreTurnTick> _this = _Tracker.Ticks.Find(this);
            _Tracker.Ticks.Remove(_this);
            _Tracker.Ticks.AddLast(_this);
        }
    }
}
