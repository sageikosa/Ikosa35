using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class OptionTargeting : AimTargeting<OptionAimInfo, OptionTargetInfo>
    {
        #region ctor()
        public OptionTargeting(ActivityInfoBuilder builder, OptionAimInfo optionAim)
            : base(builder, optionAim)
        {
            _TargetBuilders = new ObservableCollection<OptionAimOptionBuilder>();

            // ensure minimum targets are presented
            for (var _tx = 0; _tx < AimingMode.MinimumAimingModes; _tx++)
            {
                var _newTarget = new OptionTargetInfo
                {
                    Key = AimingMode.Key,
                    OptionKey = null,
                    OptionDescription = null,
                    OptionName = null
                };
                Targets.Add(_newTarget);
                _TargetBuilders.Add(new OptionAimOptionBuilder(this, _newTarget));
            }
        }

        static OptionTargeting()
        {
            Unselected = new OptionAimOption
            {
                Key = null,
                Description = @"Select Option",
                Name = @"-- Select --"
            };
        }
        #endregion

        private ObservableCollection<OptionAimOptionBuilder> _TargetBuilders;

        public static readonly OptionAimOption Unselected;

        public IEnumerable<OptionAimOption> Options
        {
            get
            {
                // add in a non-selected one
                yield return Unselected;
                foreach (var _opt in AimingMode.Options)
                {
                    yield return _opt;
                }
                yield break;
            }
        }

        #region public void SyncSelectableOptions()
        /// <summary>Ensure that up to the maximum number of options are presented to be selected (one at a time)</summary>
        public void SyncSelectableOptions()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (!Targets.Any(_t => _t.OptionKey == null))
                {
                    var _newTarget = new OptionTargetInfo
                    {
                        Key = AimingMode.Key,
                        OptionKey = null,
                        OptionDescription = null,
                        OptionName = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new OptionAimOptionBuilder(this, _newTarget));
                    DoPropertyChanged(nameof(OptionBuilders));
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

        /// <summary>Targets wrapped in builders</summary>
        public ObservableCollection<OptionAimOptionBuilder> OptionBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => _t.OptionKey != null) >= AimingMode.MinimumAimingModes;

        protected override void SyncAimMode(OptionAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectableOptions();
            DoPropertyChanged(nameof(Options));
        }

        protected override void SetAimTargets(List<OptionTargetInfo> targets)
        {
            if (OptionBuilders.Any() && targets.Any())
            {
                foreach (var _target in targets)
                {
                    var _idx = targets.IndexOf(_target);
                    if (OptionBuilders.Count > _idx)
                    {
                        // only do stuff if we have capacity
                        var _builder = OptionBuilders[_idx];
                        _builder.SelectedOption = Options.FirstOrDefault(_opt => _opt.Key == _target.OptionKey);

                        // try to expand capacity if necessary (more available and unselected)
                        SyncSelectableOptions();
                    }
                }
            }
        }
    }
}
