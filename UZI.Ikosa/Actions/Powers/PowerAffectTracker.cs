using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PowerAffectTracker
    {
        #region state
        private HashSet<Guid> _Resisted;
        private HashSet<Guid> _Affected;
        #endregion

        public void SetAffect(Guid targetID, bool result)
        {
            if (result)
            {
                _Affected ??= [];
                if (!_Affected.Contains(targetID))
                {
                    _Affected.Add(targetID);
                }
            }
            else
            {
                _Resisted ??= [];
                if (!_Resisted.Contains(targetID))
                {
                    _Resisted.Add(targetID);
                }
            }
        }

        public bool? DoesAffect(Guid targetID)
            => (_Affected?.Contains(targetID) ?? false) ? true
            : ((_Resisted?.Contains(targetID) ?? false) ? false
            : (bool?)null);
    
    }
}
