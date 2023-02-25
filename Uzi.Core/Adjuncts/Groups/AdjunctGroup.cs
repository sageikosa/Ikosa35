using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace Uzi.Core
{
    /// <summary>Represents a grouping of GroupMemberAdjuncts.</summary>
    [Serializable]
    public abstract class AdjunctGroup : ISourcedObject
    {
        protected AdjunctGroup(object source)
        {
            _Src = source;
            _ID = Guid.NewGuid();
        }

        protected AdjunctGroup(object source, Guid id)
        {
            _Src = source;
            _ID = id;
        }

        protected AdjunctGroup(SerializationInfo info, StreamingContext context)
        {
            _Src = info.GetValue($@"{nameof(AdjunctGroup)}+{nameof(_Src)}", typeof(object));
            _ID = (Guid)info.GetValue($@"{nameof(AdjunctGroup)}+{nameof(_ID)}", typeof(Guid));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue($@"{nameof(AdjunctGroup)}+{nameof(_ID)}", _ID);
            info.AddValue($@"{nameof(AdjunctGroup)}+{nameof(_Src)}", _Src);
        }

        #region private data
        private Guid _ID;
        private object _Src;
        [NonSerialized, JsonIgnore]
        private Collection<GroupMemberAdjunct> _Members = new Collection<GroupMemberAdjunct>();
        #endregion

        /// <summary>Assigns a new ID to the group, propagating to all members</summary>
        protected void ResetID()
        {
            _ID = Guid.NewGuid();
            foreach (var _m in Members)
                _m.GroupID = _ID;
        }

        public int Count => _Members?.Count ?? 0;
        public object Source => _Src;
        public Guid ID => _ID;

        protected virtual void OnMemberAdded(GroupMemberAdjunct member)
        {
        }

        /// <summary>To be called from GroupMemberAdjunct or CoreSettingContextSet</summary>
        internal void AddMember(GroupMemberAdjunct member)
        {
            _Members ??= new Collection<GroupMemberAdjunct>();
            _Members.Add(member);
            OnMemberAdded(member);
        }

        protected virtual void OnMemberRemoved(GroupMemberAdjunct member)
        {
        }

        /// <summary>To be called from GroupMemberAdjunct</summary>
        internal bool RemoveMember(GroupMemberAdjunct member)
        {
            if (_Members?.Remove(member) ?? false)
            {
                OnMemberRemoved(member);
                return true;
            }
            return false;
        }

        public abstract void ValidateGroup();

        public void EjectMembers()
        {
            if (_Members != null)
                foreach (var _member in _Members.ToList())
                    _member.Eject();
        }

        public IEnumerable<GroupMemberAdjunct> Members
        {
            get
            {
                _Members ??= new Collection<GroupMemberAdjunct>();
                return _Members.Select(_m => _m);
            }
        }

        public virtual void OnAdded() { }
        public virtual void OnRemoved() { }
    }
}
