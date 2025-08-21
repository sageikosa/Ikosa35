using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class WillingPrerequisite : StepPrerequisite
    {
        public WillingPrerequisite(object source, Qualifier workSet, string key, string name, bool isSerial)
            : base(source, workSet, key, name)
        {
            _IsSerial = isSerial;
            _Choices =
            [
                new OptionAimValue<bool>
                {
                    Key = @"True",
                    Description = @"Allow",
                    Name = @"Yes",
                    Value = true
                },
                new OptionAimValue<bool>
                {
                    Key = @"False",
                    Description = @"Do not allow",
                    Name = @"No",
                    Value = false
                }
            ];
        }

        #region private data
        private List<OptionAimOption> _Choices;
        private bool _IsSerial;
        private OptionAimOption _Selected;
        #endregion


        public IEnumerable<OptionAimOption> Choices
            => _Choices.Select(_c => _c);

        public override bool IsSerial => _IsSerial;

        public override bool IsReady
            => (_Selected != null);

        public override bool FailsProcess 
            => Selected?.Key.Equals(@"False", StringComparison.OrdinalIgnoreCase) ?? false;

        public OptionAimOption Selected
        {
            get => _Selected;
            set
            {
                if (value != null)
                {
                    _Selected = _Choices.FirstOrDefault(_c => _c.Key.Equals(value.Key));
                }
            }
        }

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
