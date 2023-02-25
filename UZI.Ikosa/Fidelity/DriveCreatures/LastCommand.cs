using System;
using System.Linq;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Predisposition for last command provided to creature</summary>
    [Serializable]
    public class LastCommand : PredispositionBase
    {
        #region construction
        /// <summary>Predisposition for last command provided to creature</summary>
        public LastCommand(object source, string command, int checkValue)
            : base(source)
        {
            _Cmd = command;
            _Check = checkValue;
        }
        #endregion

        #region private data
        private string _Cmd;
        private int _Check;
        #endregion

        protected override void OnActivate(object source)
        {
            #region purge other last commands
            foreach (var _lastCmd in Anchor.Adjuncts.OfType<LastCommand>().ToList().Where(_lc => _lc != this))
            {
                _lastCmd.Eject();
            }
            #endregion

            base.OnActivate(source);
        }

        /// <summary>Necessary to compare conflicting commands.</summary>
        public int CheckValue { get { return _Check; } set { _Check = value; } }

        public string Command { get { return _Cmd; } set { _Cmd = value; } }
        public override string Description { get { return string.Format(@"Commanded({0}): {1}", CheckValue, Command); } }
        public override object Clone() { return new LastCommand(Source, Command, CheckValue); }
    }
}
