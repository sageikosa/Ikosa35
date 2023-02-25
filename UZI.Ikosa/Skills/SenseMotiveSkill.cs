using System;
using Uzi.Core;
using Uzi.Ikosa.Abilities;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Sense Motive", MnemonicCode.Wis)]
    public class SenseMotiveSkill : SkillBase, IModifier
    {
        public SenseMotiveSkill(Creature skillUser)
            : base(skillUser)
        {
            _Terminator = new TerminateController(this);
        }

        #region IDelta Members
        public int Value
        {
            get
            {
                if (BaseValue >= 5)
                {
                    return 2;
                }
                else
                {
                    return 0;
                }
            }
        }

        public object Source { get { return GetType(); } }
        public string Name { get { return @">=5 ranks in Sense Motive"; } }

        public bool Enabled
        {
            get
            {
                return true;
            }
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
        private readonly TerminateController _Terminator;
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
    }
}
