using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class LockGroup : AdjunctGroup, INotifyPropertyChanged, IForceOpenTarget
    {
        #region construction
        public LockGroup(string name, bool locked, bool allowClose)
            : base(typeof(LockGroup))
        {
            if (string.IsNullOrEmpty(name))
            {
                _Name = $@"Lock {ID}";
            }
            else
            {
                _Name = name;
            }
            _AllowClose = allowClose;
        }
        #endregion

        #region data
        private string _Name;
        private bool _AllowClose;
        #endregion

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                DoPropertyChanged(@"Name");
            }
        }

        public bool AllowClose => _AllowClose;

        #region public bool IsLocked { get; set; }
        public bool IsLocked
        {
            get => Blocker != null;
            set
            {
                if (Target != null)
                {
                    if (value)
                    {
                        // when locked and on a target, prevents open/close
                        if (Blocker == null)
                        {
                            Target.Openable.AddAdjunct(new OpenBlocked(this, this, AllowClose));
                        }
                    }
                    else
                    {
                        // when unlocked, no longer prevents open/close
                        var _block = Blocker;
                        if (_block != null)
                        {
                            _block.Eject();
                        }
                    }
                }
                DoPropertyChanged(@"IsLocked");
            }
        }
        #endregion

        public LockTarget Target
            => Members.OfType<LockTarget>().FirstOrDefault();

        public IEnumerable<LockMechanism> Mechanisms
            => Members.OfType<LockMechanism>();

        /// <summary>Find the blocker sourced from a lockgroup with our ID</summary>
        public OpenBlocked Blocker => Target?.Blocker;

        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        // IForceOpenTarget
        public void DoForcedOpen()
        {
            // destroy all mechanisms
            // TODO: damage and deactivate?
            foreach (var _mech in Mechanisms.ToList())
            {
                _mech.Mechanism.DoDestruction();
            }
        }

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }

    /// <summary>When anchored or re-bound to setting, automatically adds LockGroup</summary>
    [Serializable]
    public class LockTarget : GroupMasterAdjunct, IPathDependent
    {
        /// <summary>When anchored or re-bound to setting, automatically adds LockGroup</summary>
        public LockTarget(LockGroup group)
            : base(group, group)
        {
        }

        public LockGroup LockGroup
            => Group as LockGroup;

        public IOpenable Openable
            => Anchor as IOpenable;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is IOpenable) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new LockTarget(LockGroup);

        #region public OpenBlocked Blocker { get; }
        /// <summary>Find the blocker sourced from a lockgroup with our ID</summary>
        public OpenBlocked Blocker
        {
            get
            {
                // as long as the source is a lock group and the ID matches ours
                return (from _a in Openable.Adjuncts.OfType<OpenBlocked>()
                        where _a.Source is LockGroup
                        let _lg = _a.Source as LockGroup
                        where _lg.ID == LockGroup.ID
                        select _a).FirstOrDefault();
            }
        }
        #endregion

        protected override void OnDeactivate(object source)
        {
            Blocker?.Eject();
            base.OnDeactivate(source);
        }

        public override void PathChanged(Pathed source)
        {
            if ((source is ObjectBound) && (source.Anchor == null))
            {
                // no longer object bound...get rid of group
                // don't re-use this mechanism
                Eject();
            }
            else
            {
                base.PathChanged(source);
            }
        }
    }

    [Serializable]
    public class LockMechanism : GroupMemberAdjunct, IPathDependent
    {
        public LockMechanism(LockGroup group)
            : base(typeof(LockGroup), group)
        {
        }

        public LockGroup LockGroup
            => Group as LockGroup;

        public Mechanism Mechanism
            => Anchor as Mechanism;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is Mechanism) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new LockMechanism(LockGroup);

        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            if (Anchor == null)
            {
                // UnAnchored...(ie, this lock removed from a locking mechanism)
                if (!LockGroup.Mechanisms.Any())
                {
                    // eject the group
                    LockGroup.Target.Eject();
                }
            }
            base.OnAnchorSet(oldAnchor, oldSetting);
        }

        public override void PathChanged(Pathed source)
        {
            if ((source is ObjectBound) && (source.Anchor == null))
            {
                // no longer object bound...
                if (LockGroup.Mechanisms.Count() == 1)
                {
                    // eject the group
                    LockGroup.Target.Eject();
                    return;
                }
                else
                {
                    // just eject the mechanism
                    Eject();
                    return;
                }
            }
            base.PathChanged(source);
        }
    }
}
