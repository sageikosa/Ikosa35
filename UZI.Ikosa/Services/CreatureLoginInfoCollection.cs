using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Services
{
    public class CreatureLoginInfoCollection
    {
        public CreatureLoginInfoCollection()
        {
            _Lock = new ReaderWriterLockSlim();
            _Creatures = [];
        }

        #region data
        private ReaderWriterLockSlim _Lock = null;
        private Dictionary<Guid, CreatureLoginInfo> _Creatures;
        #endregion

        #region public void Add(CreatureLoginInfo critterLoginInfo)
        public void Add(CreatureLoginInfo critterLoginInfo)
        {
            try
            {
                if (critterLoginInfo != null)
                {
                    _Lock.EnterWriteLock();
                    if (!_Creatures.ContainsKey(critterLoginInfo.ID))
                    {
                        _Creatures.Add(critterLoginInfo.ID, critterLoginInfo);
                    }
                }
            }
            finally
            {
                if (_Lock.IsWriteLockHeld)
                {
                    _Lock.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public bool Remove(Guid id)
        public bool Remove(Guid id)
        {
            try
            {
                _Lock.EnterWriteLock();
                if (_Creatures.ContainsKey(id))
                {
                    return _Creatures.Remove(id);
                }

                return false;
            }
            finally
            {
                if (_Lock.IsWriteLockHeld)
                {
                    _Lock.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public void Clear()
        public void Clear()
        {
            try
            {
                _Lock.EnterWriteLock();
                _Creatures.Clear();
            }
            finally
            {
                if (_Lock.IsWriteLockHeld)
                {
                    _Lock.ExitWriteLock();
                }
            }
        }
        #endregion

        #region public List<CreatureLoginInfo> GetList()
        public List<CreatureLoginInfo> GetList()
        {
            try
            {
                _Lock.EnterReadLock();
                return _Creatures.Select(_kcp => _kcp.Value).ToList();
            }
            finally
            {
                if (_Lock.IsReadLockHeld)
                {
                    _Lock.ExitReadLock();
                }
            }
        }
        #endregion

        #region public CreatureLoginInfo GetCreatureLoginInfo(Guid id)
        public CreatureLoginInfo GetCreatureLoginInfo(Guid id)
        {
            try
            {
                _Lock.EnterReadLock();
                if (_Creatures.ContainsKey(id))
                {
                    return _Creatures[id];
                }

                return null;
            }
            finally
            {
                if (_Lock.IsReadLockHeld)
                {
                    _Lock.ExitReadLock();
                }
            }
        }
        #endregion
    }
}
