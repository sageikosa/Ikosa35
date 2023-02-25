using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class WordTargeting : AimTargeting<WordAimInfo, CharacterStringTargetInfo>
    {
        #region ctor()
        public WordTargeting(ActivityInfoBuilder builder, WordAimInfo aimMode)
            : base(builder, aimMode)
        {
            Targets.Add(new CharacterStringTargetInfo
            {
                Key = AimingMode.Key,
                CharacterString = null
            });
        }
        #endregion

        public string WordData
        {
            get => Targets[0].CharacterString;
            set
            {
                Targets[0].CharacterString = value;
                DoPropertyChanged(@"WordData");
                SetIsReady();
            }
        }

        protected override void SyncAimMode(WordAimInfo aimMode)
        {
            _AimingMode = aimMode;
        }

        protected override void SetAimTargets(List<CharacterStringTargetInfo> targets)
        {
            WordData = targets?[0].CharacterString;
        }

        public override bool IsReady
        {
            get
            {
                // TODO: make a real "word count"
                var _string = Targets[0].CharacterString;
                return !string.IsNullOrWhiteSpace(_string)
                    && _string.Length >= AimingMode.MinimumAimingModes
                    && _string.Length <= AimingMode.MaximumAimingModes * 7
                    && _string.Count(_c => Char.IsWhiteSpace(_c)) < AimingMode.MaximumAimingModes;
            }
        }
    }
}
