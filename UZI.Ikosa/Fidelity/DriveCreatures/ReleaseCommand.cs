using Uzi.Core.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Actions;
using Uzi.Core;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class ReleaseCommand : ActionBase
    {
        #region construction
        public ReleaseCommand(CommandMaster master, string orderKey)
            : base(master, new ActionTime(TimeType.Free), false, false, orderKey)
        {
        }
        #endregion

        public override string Key => @"Command.Release";
        public override string DisplayName(CoreActor actor) => @"Release creature(s) from command";
        public CommandMaster CommandMaster => Source as CommandMaster;
        public override bool IsMental => true;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            foreach (var _target in activity.Targets.OfType<OptionAimValue<Creature>>())
            {
                // release
                var _cmd = CommandMaster.CommandGroup[_target.Value];
                if (_cmd != null)
                {
                    _cmd.Eject();
                }
            }
            return null;
        }
        #endregion

        #region public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Creature", @"Creatures to release", true, FixedRange.One,
                new FixedRange(Convert.ToDouble(CommandMaster.CommandGroup.Count)), GroupMembers());
            yield break;
        }
        #endregion

        #region private IEnumerable<OptionAimOption> GroupMembers()
        private IEnumerable<OptionAimOption> GroupMembers()
        {
            foreach (var _creature in CommandMaster.CommandGroup.CommandedCreatures)
            {
                yield return new OptionAimValue<Creature>
                {
                    Key = _creature.ID.ToString(),
                    Description = _creature.Name,
                    Name = _creature.Name,
                    Value = _creature
                };
            }
            yield break;
        }
        #endregion

        public override bool IsProvocableTarget(CoreActivity activity, CoreObject potentialTarget)
            => false;
    }
}