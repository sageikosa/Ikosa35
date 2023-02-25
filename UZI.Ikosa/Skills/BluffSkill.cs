using System;
using Uzi.Core;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Bluff", "CHA")]
    public class BluffSkill : SkillBase, IModifier
    {
        public BluffSkill(Creature skillUser)
            : base(skillUser)
        {
            _Terminator = new TerminateController(this);
        }

        private readonly TerminateController _Terminator;

        #region IDelta Members
        // synergy bonus
        public int Value
            => (BaseValue >= 5) ? 2 : 0;

        public object Source
            => GetType();

        public string Name
            => @">=5 ranks in Bluff";

        public bool Enabled
        {
            get { return true; }
            set
            {
                // ignore
            }
        }
        #endregion

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion

        // TODO: bluff to feint
    }
}
