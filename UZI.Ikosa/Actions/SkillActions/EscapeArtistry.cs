using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions.Steps;

namespace Uzi.Ikosa.Actions
{
    [Serializable]
    public class EscapeArtistry : ActionBase
    {
        public EscapeArtistry(ICanEscape escape, bool provokesMelee, string orderKey)
            : base(escape, escape.EscapeTime, provokesMelee, false, orderKey)
        {
        }

        public ICanEscape EscapeSource => Source as ICanEscape;
        public override string Key => @"Skill.Escape";
        public override string DisplayName(CoreActor actor) => $@"Escape from {EscapeSource.EscapeName(actor)}";

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield break;
        }

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => ObservedActivityInfoFactory.CreateInfo(@"Escape", activity.Actor, observer);

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            activity.EnqueueRegisterPreEmptively(Budget);
            if (activity.Actor is Creature _critter)
            {
                return new EscapeStep(_critter, this);
            }
            return null;
        }
    }

    public interface ICanEscape : IActionSource
    {
        ActionTime EscapeTime { get; }
        string EscapeName(CoreActor actor);
        Deltable EscapeDifficulty { get; }
        void DoEscape();
        IInteract EscapeFrom { get; }
    }
}
