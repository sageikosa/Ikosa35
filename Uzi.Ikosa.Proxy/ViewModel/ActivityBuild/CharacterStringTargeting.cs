using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class CharacterStringTargeting : AimTargeting<CharacterStringAimInfo, CharacterStringTargetInfo>
    {
        #region ctor()
        public CharacterStringTargeting(ActivityInfoBuilder builder, CharacterStringAimInfo aimMode)
            : base(builder, aimMode)
        {
            Targets.Add(new CharacterStringTargetInfo
            {
                Key = AimingMode.Key,
                CharacterString = null
            });
        }
        #endregion

        public string CharacterString
        {
            get { return Targets[0].CharacterString; }
            set
            {
                Targets[0].CharacterString = value;
                DoPropertyChanged(nameof(CharacterString));
                SetIsReady();
            }
        }

        protected override void SyncAimMode(CharacterStringAimInfo aimMode)
        {
            _AimingMode = aimMode;
        }

        protected override void SetAimTargets(List<CharacterStringTargetInfo> targets)
        {
            CharacterString = targets?[0]?.CharacterString;
        }

        public override bool IsReady
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Targets[0].CharacterString)
                    && Targets[0].CharacterString.Length >= AimingMode.MinimumAimingModes
                    && Targets[0].CharacterString.Length <= AimingMode.MaximumAimingModes;
            }
        }
    }
}