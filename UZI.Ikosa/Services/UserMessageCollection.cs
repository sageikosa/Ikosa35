using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading;
using Uzi.Ikosa.Contracts.Host;

namespace Uzi.Ikosa.Services
{
    public class UserMessageCollection
    {
        public UserMessageCollection()
        {
            _Lock = new ReaderWriterLockSlim();
            _Messages = [];
        }

        #region private data
        private ReaderWriterLockSlim _Lock;
        private Collection<UserMessage> _Messages;
        #endregion

        #region public void Add(UserMessage message)
        public void Add(UserMessage message)
        {
            try
            {
                _Lock.EnterWriteLock();
                _Messages.Add(message);
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

        #region public void Add(IList<UserMessage> messages)
        public void Add(IList<UserMessage> messages)
        {
            try
            {
                _Lock.EnterWriteLock();
                foreach (var _msg in messages)
                {
                    _Messages.Add(_msg);
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

        #region public bool Remove(UserMessage message)
        public bool Remove(UserMessage message)
        {
            try
            {
                _Lock.EnterWriteLock();
                return _Messages.Remove(message);
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

        #region public void Remove(IList<UserMessage> messages)
        public void Remove(IList<UserMessage> messages)
        {
            try
            {
                _Lock.EnterWriteLock();
                foreach (var _msg in messages)
                {
                    _Messages.Remove(_msg);
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

        #region public List<UserMessage> GetToUser(string user)
        public List<UserMessage> GetToUser(string user)
        {
            try
            {
                _Lock.EnterReadLock();
                return _Messages.Where(_m => _m.ToUser.Equals(user)).ToList();
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

        #region public IList<UserMessage> GetFromUser(string user)
        public IList<UserMessage> GetFromUser(string user)
        {
            try
            {
                _Lock.EnterReadLock();
                return _Messages.Where(_m => _m.FromUser.Equals(user)).ToList();
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
