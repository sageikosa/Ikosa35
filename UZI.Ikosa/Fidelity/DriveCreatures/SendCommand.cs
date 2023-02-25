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
    public class SendCommand : ActionBase
    {
        #region construction
        public SendCommand(CommandMaster master, string orderKey)
            : base(master, new ActionTime(TimeType.Regular), false, false, orderKey)
        {
        }

        public SendCommand(CommandMaster master, ActionTime actionTime, string orderKey)
            : base(master, actionTime, false, false, orderKey)
        {
        }
        #endregion

        public override string Key => @"Command.Send";
        public override string DisplayName(CoreActor actor) => @"Command creatures to act";
        public CommandMaster CommandMaster => Source as CommandMaster;

        public override bool IsStackBase(CoreActivity activity)
            => false;

        public override ObservedActivityInfo GetActivityInfo(CoreActivity activity, CoreActor observer)
            => null;

        #region protected override CoreStep OnPerformActivity(CoreActivity activity)
        protected override CoreStep OnPerformActivity(CoreActivity activity)
        {
            var _command = activity.Targets.OfType<CharacterStringTarget>().FirstOrDefault(_cst => _cst.Key.Equals(@"Command"));
            if (_command != null)
            {
                foreach (var _target in activity.Targets.OfType<OptionAimValue<Creature>>())
                {
                    // release
                    var _cmd = CommandMaster.CommandGroup[_target.Value];
                    if (_cmd != null)
                    {
                        _cmd.Command = _command.CharacterString;
                    }
                }
            }
            return null;
        }
        #endregion

        public override IEnumerable<AimingMode> AimingMode(CoreActivity activity)
        {
            yield return new OptionAim(@"Creature", @"Creatures to command", true, FixedRange.One,
                new FixedRange(Convert.ToDouble(CommandMaster.CommandGroup.Count)), GroupMembers());
            yield return new CharacterStringAim(@"Command", @"Command to Issue", new FixedRange(1), new FixedRange(100));
            yield break;
        }

        #region private IEnumerable<OptionAimOption> GroupMembers()
        private IEnumerable<OptionAimOption> GroupMembers()
        {
            // only to creatures whose command power is active
            // NOTE: intention to block new commands to creatures in anti-magic fields
            foreach (var _creature in from _cmdCritter in CommandMaster.CommandGroup.Members.OfType<CommandedCreature>()
                                      where _cmdCritter.IsActive
                                      select _cmdCritter.Creature)
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
