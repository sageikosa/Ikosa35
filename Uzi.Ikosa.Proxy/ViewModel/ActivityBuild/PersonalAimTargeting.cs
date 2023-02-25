using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class PersonalAimTargeting : AimTargeting<PersonalAimInfo, AimTargetInfo>
    {
        #region construction
        public PersonalAimTargeting(ActivityInfoBuilder builder, PersonalAimInfo aimMode)
            : base(builder, aimMode)
        {
            _TargetBuilders = new ObservableCollection<PersonalAimInfoBuilder>();
            for (var _tx = 0; _tx < AimingMode.MinimumAimingModes; _tx++)
            {
                var _newTarget = new AimTargetInfo
                {
                    Key = AimingMode.Key,
                    TargetID = null
                };
                Targets.Add(_newTarget);
                _TargetBuilders.Add(new PersonalAimInfoBuilder(this, _newTarget));
            }
        }
        #endregion

        private ObservableCollection<PersonalAimInfoBuilder> _TargetBuilders;

        public IEnumerable<AwarenessInfo> Persons
            => AimingMode.ValidTargets;

        #region public void SyncSelectablePersons()
        /// <summary>Ensure that up to the maximum number of objects are presented to be selected (on at a time)</summary>
        public void SyncSelectablePersons()
        {
            // currently have none unselected, but have more capacity
            if (Targets.Count < AimingMode.MaximumAimingModes)
            {
                if (!Targets.Any(_t => _t.TargetID == null))
                {
                    var _newTarget = new AimTargetInfo
                    {
                        Key = AimingMode.Key,
                        TargetID = null
                    };
                    Targets.Add(_newTarget);
                    _TargetBuilders.Add(new PersonalAimInfoBuilder(this, _newTarget));
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
        public ObservableCollection<PersonalAimInfoBuilder> PersonalBuilders
            => _TargetBuilders;

        public override bool IsReady
            => Targets.Count(_t => (_t.TargetID != null) && (_t.TargetID != Guid.Empty)) >= AimingMode.MinimumAimingModes;

        protected override void SyncAimMode(PersonalAimInfo aimMode)
        {
            _AimingMode = aimMode;
            SyncSelectablePersons();
            DoPropertyChanged(nameof(Persons));
        }

        protected override void SetAimTargets(List<AimTargetInfo> targets)
        {
            if (PersonalBuilders.Any() && targets.Any())
            {
                foreach (var _target in targets)
                {
                    var _idx = targets.IndexOf(_target);
                    if (PersonalBuilders.Count > _idx)
                    {
                        // only do stuff if we have capacity
                        var _builder = PersonalBuilders[_idx];
                        _builder.SelectedPerson = Persons.FirstOrDefault(_pers => _pers.ID == _target.TargetID);

                        // try to expand capacity if necessary (more available and unselected)
                        SyncSelectablePersons();
                    }
                }
            }
        }
    }
}
