using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using System.Collections.ObjectModel;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    /// <summary>
    /// Allows target to fulfill a choice.
    /// </summary>
    [Serializable]
    public class ChoicePrerequisite : StepPrerequisite, ISourcedObject
    {
        #region construction
        /// <summary>Used by game master</summary>
        public ChoicePrerequisite(object source, string key, string name, IEnumerable<OptionAimOption> choices, bool serial)
            : base(source, key, name)
        {
            _Choices = new Collection<OptionAimOption>(choices.ToList());
            _IsSerial = serial;
        }

        /// <summary>Used by players</summary>
        public ChoicePrerequisite(object source, CoreActor actor, object iSource, IInteract target, string key, string name, IEnumerable<OptionAimOption> choices, bool serial)
            : base(source, actor, iSource, target, key, name)
        {
            _Choices = new Collection<OptionAimOption>(choices.ToList());
            _IsSerial = serial;
        }
        #endregion

        #region private data
        private Collection<OptionAimOption> _Choices;
        private bool _IsSerial;
        private OptionAimOption _Selected;
        #endregion

        public override bool IsSerial => _IsSerial;

        public IEnumerable<OptionAimOption> Choices
            => _Choices.AsEnumerable();

        public override bool IsReady
            => (_Selected != null);

        public OptionAimOption Selected
        {
            get { return _Selected; }
            set
            {
                if (value != null)
                    _Selected = _Choices.FirstOrDefault(_c => _c.Key.Equals(value.Key));
            }
        }

        public override bool FailsProcess
            => false;

        public override CoreActor Fulfiller
            => (Qualification?.Target as CoreActor);

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<ChoicePrerequisiteInfo>(step);
            _info.Choices = Choices.Select(_c => _c.Contracted).ToArray();
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            var _choicePre = info as ChoicePrerequisiteInfo;
            if (_choicePre?.Selected != null)
            {
                // match selected key with native choices
                Selected = Choices.FirstOrDefault(_c => _c.Key == _choicePre.Selected.Key);
            }
        }
    }
}