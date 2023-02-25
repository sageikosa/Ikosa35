using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class CoreSelectPrerequisite : StepPrerequisite
    {
        #region ctor()
        /// <summary>Used by game master</summary>
        public CoreSelectPrerequisite(object source, string key, string name, IEnumerable<Guid> ids, bool serial)
            : base(source, key, name)
        {
            _IDs = ids.ToList();
            _IsSerial = serial;
            _Selected = null;
        }

        /// <summary>Used by players</summary>
        public CoreSelectPrerequisite(object source, CoreActor actor, object iSource, IInteract target, string key, string name, IEnumerable<Guid> ids, bool serial)
           : base(source, actor, iSource, target, key, name)
        {
            _IDs = ids.ToList();
            _IsSerial = serial;
            _Selected = null;
        }
        #endregion

        #region data
        private List<Guid> _IDs;
        private bool _IsSerial;
        private Guid? _Selected;
        #endregion

        public override bool IsSerial => _IsSerial;

        public IEnumerable<Guid> IDs => _IDs.Select(_i => _i);

        public override bool IsReady
            => (_Selected != null);

        public Guid? Selected
        {
            get { return _Selected; }
            set
            {
                if (value != null)
                    _Selected = _IDs.FirstOrDefault(_c => _c == value);
            }
        }

        public override bool FailsProcess
            => false;

        public override CoreActor Fulfiller
            => Qualification?.Actor;

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<CoreSelectPrerequisiteInfo>(step);
            _info.IDs = IDs.Select(_i => _i).ToArray();
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            var _selectPre = info as CoreSelectPrerequisiteInfo;
            if (_selectPre?.Selected != null)
            {
                // match selected key with native choices
                Selected = IDs.FirstOrDefault(_c => _c == _selectPre.Selected);
            }
        }
    }
}
