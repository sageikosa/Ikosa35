using System;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Added to a creature that is commanded (through a DriveCreature derived effect)</summary>
    [Serializable]
    public class CommandedCreature : GroupMemberAdjunct
    {
        #region construction
        /// <summary>Added to a creature that is commanded (through a DriveCreature derived effect)</summary>
        public CommandedCreature(IPowerSource powerSource, CommandGroup group, int checkValue, string initialCommand)
            : base(powerSource, group)
        {
            _Check = checkValue;
            _Cmd = initialCommand;
        }
        #endregion

        #region state
        private string _Cmd;
        private int _Check;
        #endregion

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (DirectCommand == null)
            {
                Anchor.AddAdjunct(new LastCommand(this, Command, CheckValue));
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            Anchor.AddAdjunct(new LastCommand(typeof(OverwhelmCreatures), Command, CheckValue));
            base.OnDeactivate(source);
        }
        #endregion

        /// <summary>DirectCommand (if defined) for this group adjunct</summary>
        public LastCommand DirectCommand
            => Anchor.Adjuncts.OfType<LastCommand>().FirstOrDefault(_icc => _icc.Source.Equals(this));

        public IPowerSource PowerSource => Source as IPowerSource;
        public CommandGroup CommandGroup => Group as CommandGroup;

        /// <summary>Commanded Creature</summary>
        public Creature Creature => Anchor as Creature;

        /// <summary>Necessary to compare conflicting commands, and see if a cleric can capture undead already commanded.</summary>
        public int CheckValue { get => _Check; set => _Check = value; }

        #region public string Command { get; set; }
        /// <summary>Gets or sets the value of the last command used in the group</summary>
        public string Command
        {
            get => _Cmd;
            set
            {
                _Cmd = value;
                var _direct = DirectCommand;
                if (_direct == null)
                {
                    Anchor.AddAdjunct(new LastCommand(this, Command, CheckValue));
                }
                else
                {
                    _direct.Command = _Cmd;
                    _direct.CheckValue = CheckValue;
                }
            }
        }
        #endregion

        public override object Clone() 
            => new CommandedCreature(PowerSource, CommandGroup, CheckValue, Command);
    }
}