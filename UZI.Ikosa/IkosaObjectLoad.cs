using System;
using Uzi.Core;

namespace Uzi.Ikosa
{
    [Serializable]
    public class IkosaObjectLoad : ObjectLoad
    {
        public IkosaObjectLoad(Creature critter)
            : base(critter)
        {
        }

        public Creature CreatureOwner { get { return Owner as Creature; } }

        protected override double MaxLoad { get { return CreatureOwner.CarryingCapacity.LoadLiftOffGround; } }
    }
}
