using System;
using System.Collections.Generic;
using Uzi.Core;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public abstract class SpellComponent
    {
        public virtual string TargetKey => $@"Component.{ComponentName}";
        public abstract string ComponentName { get; }

        /// <summary>Returns true if the caster can attempt to use the component</summary>
        public abstract bool CanStartActivity(Creature caster);

        /// <summary>Returns true if the use will succeed</summary>
        public abstract bool WillUseSucceed(CoreActivity activity);

        /// <summary>Start using component</summary>
        public abstract void StartUse(CoreActivity activity);

        /// <summary>Finishes using component</summary>
        public abstract void StopUse(CoreActivity activity);

        /// <summary>Starts as false, but conditions can set to true</summary>
        public bool HasFailed { get; set; }
    }

    [Serializable]
    public class LifeForceComponent : SpellComponent
    {
        public override string ComponentName => @"Life-Force";

        // TODO: life force value
        public override bool CanStartActivity(Creature caster)
        {
            // TODO: only if this won't bump creature down a level
            return true;
        }

        public override void StartUse(CoreActivity activity)
        {
            // TODO: subtract life-force points
        }

        public override void StopUse(CoreActivity activity)
        {
            // TODO: subtract life-force points
        }

        public override bool WillUseSucceed(CoreActivity activity)
        {
            return CanStartActivity(activity.Actor as Creature);
        }
    }
}
