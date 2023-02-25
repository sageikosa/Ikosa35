using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class SuccessCheckTargeting : AimTargeting<SuccessCheckAimInfo, SuccessCheckTargetInfo>
    {
        #region ctor()
        public SuccessCheckTargeting(ActivityInfoBuilder builder, SuccessCheckAimInfo aimMode)
            : base(builder, aimMode)
        {
            var _actorID = Builder.ActivityBuilderActor.ActorID;
            Targets.Add(new SuccessCheckTargetInfo
            {
                Key = AimingMode.Key,
                CheckRoll = null,
                IsUsingPenalty = false
            });
            _RollCmd = new RelayCommand(() =>
            {
                var _result = Builder.ActivityBuilderActor.Proxies.RollDice(AimingMode.DisplayName, @"Success Check", @"1d20", _actorID);
                CheckRoll = _result.Total.ToString();
            });
        }
        #endregion

        private readonly RelayCommand _RollCmd;

        #region public IEnumerable<string> RollValues { get; }
        public IEnumerable<string> RollValues
        {
            get
            {
                yield return @"20";
                yield return @"19";
                yield return @"18";
                yield return @"16";
                yield return @"17";
                yield return @"15";
                yield return @"14";
                yield return @"13";
                yield return @"12";
                yield return @"11";
                yield return @"10";
                yield return @"9";
                yield return @"8";
                yield return @"7";
                yield return @"6";
                yield return @"5";
                yield return @"4";
                yield return @"3";
                yield return @"2";
                yield return @"1";
                yield return @"-";
                yield break;
            }
        }
        #endregion

        #region public string CheckRoll { get; set; }
        public string CheckRoll
        {
            get => Targets[0].CheckRoll.HasValue ? Targets[0].CheckRoll.ToString() : @"-";
            set
            {
                if ((value ?? @"-") == @"-")
                {
                    Targets[0].CheckRoll = null;
                }
                else
                {
                    Targets[0].CheckRoll = Convert.ToInt32(value);
                }
                DoPropertyChanged(nameof(CheckRoll));
                SetIsReady();
            }
        }
        #endregion

        public bool IsUsingPenalty
        {
            get { return Targets[0].IsUsingPenalty; }
            set
            {
                Targets[0].IsUsingPenalty = value;
                DoPropertyChanged(nameof(IsUsingPenalty));
            }
        }

        public Visibility PenaltyVisibility
        {
            get
            {
                if (AimingMode.VoluntaryPenalty != 0)
                    return Visibility.Visible;
                return Visibility.Collapsed;
            }
        }

        public override bool IsReady => true;

        public RelayCommand RollCommand => _RollCmd;

        protected override void SyncAimMode(SuccessCheckAimInfo aimMode)
        {
            _AimingMode = aimMode;
        }

        protected override void SetAimTargets(List<SuccessCheckTargetInfo> targets)
        {
            CheckRoll = targets?.FirstOrDefault()?.CheckRoll?.ToString() ?? @"-";
        }
    }
}