using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Actions.Steps
{
    [Serializable]
    public class SetLockStep : CoreStep
    {
        public SetLockStep(CoreProcess process, LockGroup lockGroup, bool setLock)
            : base(process)
        {
            _Lock = lockGroup;
            _SetLock = setLock;
        }

        #region state
        private LockGroup _Lock;
        private bool _SetLock;
        #endregion

        public LockGroup LockGroup => _Lock;
        public bool SetLock => _SetLock;

        public override bool IsDispensingPrerequisites => false;
        protected override StepPrerequisite OnNextPrerequisite() => null;

        protected override bool OnDoStep()
        {
            LockGroup.IsLocked = SetLock;
            return true;
        }
    }
}
