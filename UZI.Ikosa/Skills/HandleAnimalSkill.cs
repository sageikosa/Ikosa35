using System;
using Uzi.Core;

namespace Uzi.Ikosa.Skills
{
    [Serializable, SkillInfo("Handle Animal", "CHA", true, 0d)]
    public class HandleAnimalSkill : SkillBase, IModifier
    {
        public HandleAnimalSkill(Creature skillUser)
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

        public object Source => GetType(); 
        public string Name   => @">=5 ranks in Handle Animal"; 

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
