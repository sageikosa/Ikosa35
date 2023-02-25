using System;
using System.Collections.Generic;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreTickTracker
    {
        public CoreTickTracker()
        {
            // build initial collections
            _Ticks = new LinkedList<CoreTurnTick>();
        }

        #region private data
        private LinkedList<CoreTurnTick> _Ticks;
        #endregion

        public LinkedList<CoreTurnTick> Ticks => _Ticks; 

        /// <summary>Returns the tick at the head of the list</summary>
        public CoreTurnTick LeadingTick => _Ticks.First?.Value; 

        // TODO: other re-usable operations
    }
}
