using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public abstract class CoreSetting
    {
        protected CoreSetting(string name)
        {
            _Name = name;
            _ID = Guid.NewGuid();
            _ContextSet = new CoreSettingContextSet(this);
        }

        #region data
        protected string _Name;
        protected CoreSettingContextSet _ContextSet;
        private Guid _ID;
        #endregion

        public abstract CoreProcessManager GenerateProcessManager();

        public virtual string Name => _Name;
        public CoreSettingContextSet ContextSet => _ContextSet;
        public Guid ID => _ID;

        #region public ObjType GetICore<ObjType>(Guid coreID) where ObjType : ICore
        public ObjType GetICore<ObjType>(Guid coreID) where ObjType : ICore
        {
            // all connected objects
            var _found = ContextSet.GetICore(coreID);
            if (_found is ObjType _oFound)
            {
                return _oFound;
            }

            return default(ObjType);
        }
        #endregion

        /// <summary>Gets all connected in all tokens</summary>
        public IEnumerable<ObjType> GetAllICore<ObjType>() where ObjType : ICore
            => (from _ctx in ContextSet.All()
                from _tok in _ctx.AllTokens
                from _found in _tok.AllConnectedOf<ObjType>()
                select _found);
    }
}
