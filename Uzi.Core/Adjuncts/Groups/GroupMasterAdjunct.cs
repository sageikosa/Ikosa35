using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core
{
    /// <summary>Adjunct that is capable of reconstituting an AdjunctGroup on deserialization and setting binding</summary>
    [Serializable]
    public abstract class GroupMasterAdjunct : GroupMemberAdjunct, IDeserializationCallback, IBindToSetting
    {
        /// <summary>Adjunct that is capable of reconstituting an AdjunctGroup on deserialization and setting binding</summary>
        protected GroupMasterAdjunct(object source, AdjunctGroup group)
            : base(source, group)
        {
            _MGroup = group;
        }

        private AdjunctGroup _MGroup;

        /// <summary>
        /// MasterGroup is used to re-establish AdjunctGroup relationships amongst a set of CoreObjects
        /// that serialize as a logical set, independent of context.  
        /// This is a serializing copy of the reference availble in Group.
        /// </summary>
        protected AdjunctGroup MasterGroup { get { return _MGroup; } }

        /// <summary>Ejects the group, and all members</summary>
        public override bool Eject()
        {
            // try to eject parts
            var _eject = true;
            foreach (var _member in Group?.Members.Where(_m => _m != this).ToList()
                ?? new List<GroupMemberAdjunct>())
                _eject &= _member.Eject();

            // then self
            if (_eject)
                _eject &= base.Eject();

            // and report it
            return _eject;
        }

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // ensures group is established in context
            oldSetting?.ContextSet.AdjunctGroups.Remove(MasterGroup);

            // new setting 
            Anchor?.Setting?.ContextSet.AdjunctGroups.Add(MasterGroup);
        }
        #endregion

        #region IDeserializationCallback Members

        public virtual void OnDeserialization(object sender)
        {
            // ensures group is established in context
            if (Anchor?.Setting != null)
            {
                Anchor.Setting.ContextSet.AdjunctGroups.Add(MasterGroup);
            }
        }

        #endregion

        #region IBindToSetting Members

        public virtual void BindToSetting()
        {
            // ensures group is defined
            if (Anchor?.Setting != null)
            {
                Anchor.Setting.ContextSet.AdjunctGroups.Add(MasterGroup);
            }
        }

        public virtual void UnbindFromSetting()
        {
            // ensures group is removed
            if (Anchor.Setting != null)
                Anchor.Setting.ContextSet.AdjunctGroups.Remove(MasterGroup);
        }

        #endregion
    }
}
