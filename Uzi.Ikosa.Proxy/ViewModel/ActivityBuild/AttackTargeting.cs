using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AttackTargeting : AimTargeting<AttackAimInfo, AttackTargetInfo>
    {
        public AttackTargeting(ActivityInfoBuilder builder, AttackAimInfo aimMode)
            : base(builder, aimMode)
        {
            _Choices = new ObservableCollection<AttackChoice>();
            _TargetBuilders = new ObservableCollection<AttackTargetInfoBuilder>();
            SyncSelectableAttacks();
        }

        #region data
        private ObservableCollection<AttackChoice> _Choices;
        private ObservableCollection<AttackTargetInfoBuilder> _TargetBuilders;
        #endregion

        #region public void SyncSelectableAttacks()
        /// <summary>Ensure that up to the maximum number of attacks are presented to be selected (one at a time)</summary>
        public void SyncSelectableAttacks()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (Targets.All(_t => _t.IsAttackReady))
                {
                    // TODO: more stuff to seed? ¿cell?
                    var _newTarget = new AttackTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null,
                        TargetCell = null,
                        AttackScore = null,
                        CriticalConfirm = null,
                        Impact = AimingMode.Impact
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new AttackTargetInfoBuilder(this, _newTarget));
                    DoPropertyChanged(nameof(TargetBuilders));
                }
            }
            else
                while ((Targets.Count > AimingMode.MaximumAimingModes) && _TargetBuilders.Any())
                {
                    Targets.Remove(Targets.Last());
                    _TargetBuilders.Remove(_TargetBuilders.Last());
                }
        }
        #endregion

        public ObservableCollection<AttackChoice> AttackChoices
            => _Choices;

        #region public void SetAttackChoices(List<AttackChoice> choices)
        public void SetAttackChoices(List<AttackChoice> choices)
        {
            var _choiceCompare = new AttackChoiceComparer();

            // same action, synchronize targets...
            foreach (var _rmv in (from _ch in _Choices
                                  where !choices.Any(_c => _choiceCompare.Equals(_ch, _c))
                                  select _ch).ToList())
            {
                // remove target builders that are no longer available
                _Choices.Remove(_rmv);
            }

            foreach (var _add in (from _c in choices
                                  where !_Choices.Any(_ch => _choiceCompare.Equals(_ch, _c))
                                  select _c).ToList())
            {
                // add target builders that are now available
                _Choices.Add(_add);
            }

            FillEmptyChoices();
        }

        private void FillEmptyChoices()
        {
            if (_Choices.Any() && _TargetBuilders.All(_tb => _tb.AttackChoice == null))
            {
                // assign indexers
                var _tx = 0;
                var _cx = 0;
                do
                {
                    // reset choices indexer
                    if (_cx >= _Choices.Count)
                        _cx = 0;

                    // set and sync available
                    _TargetBuilders[_tx].AttackChoice = _Choices[_cx];
                    SyncSelectableAttacks();

                    // step forward in both lists
                    _tx++;
                    _cx++;

                } while ((_tx < _TargetBuilders.Count) && (_cx < _Choices.Count));
            }
        }
        #endregion

        /// <summary>Targets wrapped in builders</summary>
        public ObservableCollection<AttackTargetInfoBuilder> TargetBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => _t.IsAttackReady) >= AimingMode.MinimumAimingModes;

        public Visibility LethalityVisibility
            => (AimingMode.LethalOption == Lethality.AlwaysLethal) || (AimingMode.LethalOption == Lethality.AlwaysNonLethal)
                ? Visibility.Collapsed
                : Visibility.Visible;

        public Visibility RollVisibility
            => AimingMode.UseHiddenRolls
                ? Visibility.Collapsed
                : Visibility.Visible;

        protected override void SyncAimMode(AttackAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableAttacks();
            FillEmptyChoices();
        }

        protected override void SetAimTargets(List<AttackTargetInfo> targets)
        {
            // NOP
            if (TargetBuilders.Any() && targets.Any())
            {
                var _builder = TargetBuilders[0];
                var _target = targets.FirstOrDefault();
                _builder.AttackChoice = AttackChoices.FirstOrDefault(_ac => _ac.Awareness.ID == _target.TargetID);
                _builder.AttackScore = _target.AttackScore?.ToString() ?? @"-";
                _builder.CriticalConfirm = _target.CriticalConfirm?.ToString() ?? @"-";
                _builder.IsNonLethal = _target.IsNonLethal;
            }
        }
    }
}
