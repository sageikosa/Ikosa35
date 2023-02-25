using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using Uzi.Core.Contracts.Faults;

namespace Uzi.Ikosa.Services
{
    public class SecureWriteLockHolder : IDisposable
    {
        public SecureWriteLockHolder(ReaderWriterLockSlim synchronizer, string role)
        {
            _Synchronizer = synchronizer;
            var _principal = Thread.CurrentPrincipal;
            if (_principal?.Identity != null)
            {
                if (_principal.IsInRole(role))
                {
                    Synchronizer.EnterWriteLock();
                }
                else
                {
                    throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authorized");
                }
            }
            else
            {
                throw new FaultException<SecurityFault>(new SecurityFault(), @"User not authenticated");
            }
        }

        private ReaderWriterLockSlim _Synchronizer;

        public ReaderWriterLockSlim Synchronizer => _Synchronizer;

        public void Dispose()
        {
            if (Synchronizer.IsWriteLockHeld)
                Synchronizer.ExitWriteLock();
        }
    }
}
