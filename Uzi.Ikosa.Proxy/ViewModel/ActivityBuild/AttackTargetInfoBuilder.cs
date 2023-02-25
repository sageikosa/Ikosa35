using System;
using System.Collections.Generic;
using System.Windows;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AttackTargetInfoBuilder : ViewModelBase
    {
        #region construction
        public AttackTargetInfoBuilder(AttackTargeting targetBuilder, AttackTargetInfo target)
        {
            _Choice = null;
            _Target = target;
            _TargetBuilder = targetBuilder;
            var _actorID = targetBuilder.Builder.ActivityBuilderActor.ActorID;
            _AttackRollCmd = new RelayCommand(() =>
            {
                var _result = TargetBuilder.Builder.ActivityBuilderActor.Proxies.RollDice(TargetBuilder.AimingMode.DisplayName, @"Attack Roll", @"1d20", _actorID);
                AttackScore = _result.Total.ToString();
            });
            _CriticalRollCmd = new RelayCommand(() =>
            {
                var _result = TargetBuilder.Builder.ActivityBuilderActor.Proxies.RollDice(TargetBuilder.AimingMode.DisplayName, @"Critical Confirm", @"1d20", _actorID);
                CriticalConfirm = _result.Total.ToString();
            });

            // default lethal option
            switch (TargetBuilder.AimingMode.LethalOption)
            {
                case Lethality.NormallyNonLethal:
                    IsNonLethal = true;
                    break;
                default:
                    IsNonLethal = false;
                    break;
            }
        }
        #endregion

        #region data
        private AttackChoice _Choice;
        private readonly AttackTargetInfo _Target;
        private AttackTargeting _TargetBuilder;
        private readonly RelayCommand _AttackRollCmd;
        private readonly RelayCommand _CriticalRollCmd;
        #endregion

        public AttackTargetInfo Target => _Target;
        public AttackTargeting TargetBuilder => _TargetBuilder;

        private void SyncFromProperty(string propName)
        {
            DoPropertyChanged(propName);
            _TargetBuilder.SyncSelectableAttacks();
            _TargetBuilder.SetIsReady();
        }

        #region public AttackChoice AttackChoice { get; set; }
        public AttackChoice AttackChoice
        {
            get => _Choice;
            set
            {
                _Choice = value;
                if (_Choice != null)
                {
                    Target.TargetCell = value.TargetCell;
                    Target.TargetID = value.Awareness?.ID;
                    if (_Choice.Awareness?.AutoMeleeHit ?? false)
                    {
                        AttackScore = @"20";
                    }
                }
                else
                {
                    Target.TargetCell = null;
                    Target.TargetID = null;
                }
                SyncFromProperty(nameof(AttackChoice));
            }
        }
        #endregion

        #region public string AttackScore { get; set; }
        public string AttackScore
        {
            get => Target.AttackScore.HasValue ? Target.AttackScore.ToString() : @"-";
            set
            {
                if ((value ?? @"-") == @"-")
                {
                    Target.AttackScore = null;
                }
                else
                {
                    Target.AttackScore = Convert.ToInt32(value);
                }
                SyncFromProperty(nameof(AttackScore));
                DoPropertyChanged(nameof(CriticalConfirmVisibility));
            }
        }
        #endregion

        #region public string CriticalConfirm { get; set; }
        public string CriticalConfirm
        {
            get => Target.CriticalConfirm.HasValue ? Target.CriticalConfirm.ToString() : @"-";
            set
            {
                if ((value ?? @"-") == @"-")
                {
                    Target.CriticalConfirm = null;
                }
                else
                {
                    Target.CriticalConfirm = Convert.ToInt32(value);
                }
                SyncFromProperty(nameof(CriticalConfirm));
            }
        }
        #endregion

        #region public bool IsNonLethal { get; set; }
        public bool IsNonLethal
        {
            get { return Target.IsNonLethal; }
            set
            {
                Target.IsNonLethal = value;
                SyncFromProperty(nameof(IsNonLethal));
            }
        }
        #endregion

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

        public RelayCommand AttackRollCommand => _AttackRollCmd;
        public RelayCommand CriticalRollCommand => _CriticalRollCmd;

        public Visibility CriticalConfirmVisibility
        {
            get
            {
                // attack score must be in threat range, and must not be using hidden rolls
                return (((Target.AttackScore ?? -1) >= TargetBuilder.AimingMode.CriticalThreatStart) && !TargetBuilder.AimingMode.UseHiddenRolls)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }
    }
}
