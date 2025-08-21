using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Services
{
    public class UserDefinitionCollection : Collection<UserDefinition>
    {
        // NOTE: currently, only the host UI will be writing to this collection
        private ReaderWriterLockSlim _Lock = null;

        public ReaderWriterLockSlim Synchronizer { get => _Lock; set => _Lock = value; }

        #region public void AddLocked(UserDefinition userDef)
        public void AddLocked(UserDefinition userDef)
        {
            try
            {
                _Lock.EnterWriteLock();
                Add(userDef);
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

        #region public bool RemoveLocked(UserDefinition userDef)
        public bool RemoveLocked(UserDefinition userDef)
        {
            try
            {
                _Lock.EnterWriteLock();
                return Remove(userDef);
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

        #region public UserDefinition GetUser(string userName)
        public UserDefinition GetUser(string userName)
        {
            try
            {
                _Lock.EnterReadLock();
                return this.FirstOrDefault(_ud => _ud.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
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

        #region public IList<UserDefinition> GetList()
        public IList<UserDefinition> GetList()
        {
            try
            {
                _Lock.EnterReadLock();
                return this.ToList();
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

        #region public bool DoesUserExist(string userName)
        public bool DoesUserExist(string userName)
        {
            try
            {
                _Lock.EnterReadLock();
                return this.Any(_ud => _ud.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
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

        #region public bool DoesOtherUserExist(string userName, UserDefinition userDef)
        public bool DoesOtherUserExist(string userName, UserDefinition userDef)
        {
            try
            {
                _Lock.EnterReadLock();
                return this.Any(_ud => _ud.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) && (_ud != userDef));
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
