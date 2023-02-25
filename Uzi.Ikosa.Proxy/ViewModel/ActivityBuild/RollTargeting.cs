using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class RollTargeting : AimTargeting<RollAimInfo, ValueIntTargetInfo>
    {
        #region ctor()
        public RollTargeting(ActivityInfoBuilder builder, RollAimInfo aimMode)
            : base(builder, aimMode)
        {
            var _actorID = Builder.ActivityBuilderActor.ActorID;
            Targets.Add(new ValueIntTargetInfo
            {
                Key = AimingMode.Key,
                Value = null
            });
            _RollCmd = new RelayCommand(() =>
            {
                var _result = Builder.ActivityBuilderActor.Proxies.RollDice(AimingMode.DisplayName, @"Dice Roll", AimingMode.RollerString, _actorID);
                Value = _result.Total;
            });
        }
        #endregion

        private readonly RelayCommand _RollCmd;

        public int? Value
        {
            get { return Targets[0].Value < 1 ? null : Targets[0].Value; }
            set
            {
                Targets[0].Value = (value ?? -1) < 1 ? null : value;
                DoPropertyChanged(nameof(Value));
                SetIsReady();
            }
        }

        public override bool IsReady => true;

        public RelayCommand RollCommand => _RollCmd;

        protected override void SyncAimMode(RollAimInfo aimMode)
        {
            _AimingMode = aimMode;
        }

        protected override void SetAimTargets(List<ValueIntTargetInfo> target)
        {
            Value = target?[0].Value ?? 1;
        }
    }
}
