using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    /// <summary>Triggers responses based on the changing Openable state of an IOpenable</summary>
    [Serializable]
    public class OpenableTriggerMechanism : TriggerMechanism
    {
        #region ctor()
        /// <summary>Triggers responses based on the changing Openable state of an IOpenable</summary>
        public OpenableTriggerMechanism(string name, Material material, int disableDifficulty,
            PostTriggerState postState, bool whenOpen, bool whenClosed, IOpenable openable)
            : base(name, material, disableDifficulty, postState)
        {
            _OnOpen = whenOpen;
            _OnClose = whenClosed;

            // this mechanism is the masterof the group
            var _otGroup = new OpenableTriggerGroup();
            var _otMaster = new OpenableTriggerMaster(_otGroup);
            AddAdjunct(_otMaster);

            // connect to an openable
            var _otTarget = new OpenableTriggerTarget(_otGroup);
            openable?.AddAdjunct(_otTarget);
        }
        #endregion

        public OpenableTriggerMaster OpenableTriggerMaster
            => Adjuncts.OfType<OpenableTriggerMaster>().FirstOrDefault();

        #region public IOpenable Openable { get; set; }
        public IOpenable Openable
        {
            get => OpenableTriggerMaster?.OpenableTriggerGroup.OpenableTriggerTarget?.Openable;
            set
            {
                var _target = OpenableTriggerMaster?.OpenableTriggerGroup.OpenableTriggerTarget;
                if (_target?.Openable != null)
                {
                    // stop monitoring the old one, and de-link it
                    _target.Eject();
                }

                // link the new one
                value?.AddAdjunct(_target);
            }
        }
        #endregion

        #region data
        private bool _OnOpen;
        private bool _OnClose;
        #endregion

        public bool WhenOpened
        {
            get => _OnOpen;
            set
            {
                _OnOpen = value;
                DoPropertyChanged(nameof(WhenOpened));
            }
        }

        public bool WhenClosed
        {
            get => _OnClose;
            set
            {
                _OnClose = value;
                DoPropertyChanged(nameof(WhenClosed));
            }
        }

        protected override string ClassIconKey => nameof(OpenableTriggerMechanism);
    }

    [Serializable]
    public class OpenableTriggerGroup : TriggerMountGroup<OpenableTriggerMechanism>
    {
        public OpenableTriggerGroup()
            : base(typeof(OpenableTriggerGroup))
        {
        }
        public OpenableTriggerMaster OpenableTriggerMaster => Master as OpenableTriggerMaster;
        public OpenableTriggerTarget OpenableTriggerTarget => Target as OpenableTriggerTarget;

        public override void ValidateGroup()
            => this.ValidateMasteredPlanarLink();
    }

    [Serializable]
    public class OpenableTriggerMaster : TriggerMountMaster<OpenableTriggerMechanism>
    {
        public OpenableTriggerMaster(OpenableTriggerGroup group)
            : base(group)
        {
        }

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is OpenableTriggerMechanism) && base.CanAnchor(newAnchor);

        public OpenableTriggerGroup OpenableTriggerGroup
            => TriggerGroup as OpenableTriggerGroup;

        public override object Clone()
            => new OpenableTriggerMaster(OpenableTriggerGroup);
    }

    [Serializable]
    public class OpenableTriggerTarget : TriggerMountTarget<OpenableTriggerMechanism>, IMonitorChange<OpenStatus>
    {
        public OpenableTriggerTarget(OpenableTriggerGroup group)
            : base(group)
        {
        }

        #region data
        private OpenStatus _Last;
        #endregion

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is IOpenable) && base.CanAnchor(newAnchor);

        protected override void OnActivate(object source)
        {
            _Last = (Anchor as IOpenable)?.OpenState ?? default;
            (Anchor as IOpenable)?.AddChangeMonitor(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            (Anchor as IOpenable)?.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }

        public IOpenable Openable
            => Anchor as IOpenable;

        public OpenableTriggerGroup OpenableTriggerGroup
            => TriggerGroup as OpenableTriggerGroup;

        public override object Clone()
            => new OpenableTriggerTarget(OpenableTriggerGroup);

        public override void CheckTriggering()
        {
            var _openable = Openable;
            if (_openable != null)
            {
                // state change?
                var _state = _openable.OpenState;
                if (_state.Value != _Last.Value)
                {
                    var _mech = OpenableTriggerGroup.Master.TriggerMechanism;
                    if ((_state.IsClosed && (_mech?.WhenClosed ?? false))
                        || (!_state.IsClosed && (_mech?.WhenOpened ?? false)))
                    {
                        _mech.DoTrigger(new Locator[] { });
                    }

                    // update last state...
                    _Last = _state;
                }
            }
        }

        // IMonitorChange<OpenStatus>
        public void PreTestChange(object sender, AbortableChangeEventArgs<OpenStatus> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<OpenStatus> args)
        {
            CheckTriggering();
        }
    }
}
