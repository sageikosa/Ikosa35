using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Core
{
    /// <summary>Adjunct that is capable of reconstituting an AdjunctGroup on deserialization and setting binding</summary>
    [Serializable]
    public abstract class GroupParticipantAdjunct : GroupMemberAdjunct, IDeserializationCallback, IBindToSetting
    {
        /// <summary>Adjunct that is capable of reconstituting an AdjunctGroup on deserialization and setting binding</summary>
        protected GroupParticipantAdjunct(object source, AdjunctGroup group)
            : base(source, group)
        {
            _PGroup = group;
        }

        private AdjunctGroup _PGroup;

        protected AdjunctGroup ParticipantGroup => _PGroup;

        #region protected override void OnAnchorSet(IAdjunctable oldAnchor)
        protected override void OnAnchorSet(IAdjunctable oldAnchor, CoreSetting oldSetting)
        {
            // last one out turns the lights off
            if (!ParticipantGroup.Members.Any(_m => _m != this))
            {
                oldSetting?.ContextSet.AdjunctGroups.Remove(ParticipantGroup);
            }

            // ensures group is established in context
            if (Anchor?.Setting != null)
            {
                // make sure we agree with setting on the final "real" group reference
                Anchor.Setting.ContextSet.AdjunctGroups.Add(ParticipantGroup);
                _PGroup = Anchor.Setting.ContextSet.AdjunctGroups[ParticipantGroup.ID];
            }
        }
        #endregion

        #region IDeserializationCallback Members

        public virtual void OnDeserialization(object sender)
        {
            // ensures group is established in context
            if (Anchor?.Setting != null)
            {
                // make sure we agree with setting on the final "real" group reference
                Anchor.Setting.ContextSet.AdjunctGroups.Add(ParticipantGroup);
                _PGroup = Anchor.Setting.ContextSet.AdjunctGroups[ParticipantGroup.ID];
            }
        }

        #endregion

        #region IBindToSetting Members

        public virtual void BindToSetting()
        {
            // ensures group is defined
            if (Anchor?.Setting != null)
            {
                // make sure we agree with setting on the final "real" group reference
                Anchor.Setting.ContextSet.AdjunctGroups.Add(ParticipantGroup);
                _PGroup = Anchor.Setting.ContextSet.AdjunctGroups[ParticipantGroup.ID];
            }
        }

        public virtual void UnbindFromSetting()
        {
            // last one out turns the lights off
            if (!ParticipantGroup.Members.Any())
            {
                Anchor?.Setting?.ContextSet.AdjunctGroups.Remove(ParticipantGroup);
            }
        }

        #endregion
    }
}
