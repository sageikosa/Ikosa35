using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class PreviousSuccess : StepPrerequisite, ISuccessCheckPrerequisite
    {
        public PreviousSuccess(object source, string key, string name, bool success)
            : base(source, key, name)
        {
            _Success = success;
        }

        #region data
        private bool _Success;
        #endregion

        public override bool FailsProcess
            => false;

        public override CoreActor Fulfiller
            => null;

        public override bool IsReady
            => true;

        public bool Success
            => _Success;

        public override void MergeFrom(PrerequisiteInfo info)
        {
            var _vInfo = info as ValuePrerequisiteInfo;
            if (_vInfo != null)
            {
                _Success = (_vInfo.Value ?? 0) != 0;
            }
        }

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<ValuePrerequisiteInfo>(step);
            _info.Value = _Success ? -1 : 0;
            return _info;
        }
    }
}
