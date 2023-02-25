using Newtonsoft.Json;
using System;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public abstract class GroupMemberAdjunct : Adjunct, IPathDependent
    {
        #region state
        private Guid _GroupID;
        [NonSerialized, JsonIgnore]
        private AdjunctGroup _Group;
        #endregion

        /// <summary>Automatically adds the adjunct to the adjunct group as a member</summary>
        protected GroupMemberAdjunct(object source, AdjunctGroup group)
            : base(source)
        {
            _GroupID = group.ID;
            _Group = group;
            _Group.AddMember(this);
        }

        /// <summary>All group members adjuncts are protected</summary>
        public override bool IsProtected => true;

        public AdjunctGroup Group { get => _Group; internal set => _Group = value; }
        public Guid GroupID { get => _GroupID; internal set => _GroupID = value; }

        /// <summary>Ejects a member adjunct, and removes it from the members list for its group</summary>
        public override bool Eject()
        {
            if (Group?.RemoveMember(this) ?? false)
            {
                return base.Eject();
            }
            return true;
        }

        public virtual void PathChanged(Pathed source)
        {
            Group.ValidateGroup();
        }
    }

    public static class GroupMemberAdjunctHelper
    {
        /// <summary>Any group membership is removed.</summary>
        public static void UnGroup(this IAdjunctable self)
        {
            foreach (var _member in self.Adjuncts.OfType<GroupMemberAdjunct>().ToList())
            {
                _member.Eject();
            }
        }
    }
}
