using System;
using Uzi.Core;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Universal;

namespace Uzi.Ikosa.Actions
{
    // NOTE: if roller is a constant roller, it can be generally be ignored, if it is constant 0, it should be ignored
    [Serializable]
    public class RollPrerequisite : StepPrerequisite
    {
        #region ctor(...)
        /// <summary>System Master RollPrerequisite</summary>
        public RollPrerequisite(object source, string key, string name, Roller roller, bool serial) :
            base(source, key, name)
        {
            _Roller = roller;
            _Serial = serial;
            if (roller is ConstantRoller)
            {
                RollValue = Roller.RollValue(Guid.Empty, BindKey, Name);
            }
            _Fulfiller = null;
        }

        /// <summary>General actor RollPrerequisite</summary>
        public RollPrerequisite(object source, Qualifier workSet, CoreActor fulfiller, string key, string name, Roller roller, bool serial) :
            base(source, workSet, key, name)
        {
            _Roller = roller;
            _Serial = serial;
            _Fulfiller = fulfiller;
            if (roller is ConstantRoller)
            {
                RollValue = Roller.RollValue(Fulfiller.ID, BindKey, Name, Fulfiller.ID);
            }
        }
        #endregion

        #region data
        private CoreActor _Fulfiller;
        private Roller _Roller;
        private int? _RollValue = null;
        private bool _Serial;
        #endregion

        public Roller Roller { get => _Roller; set => _Roller = value; }
        public override CoreActor Fulfiller => _Fulfiller;

        public override bool IsReady
            => _RollValue.HasValue;

        /// <summary>Controllable RollPrerequisite member (IStep member is not publicly settable)</summary>
        public bool IsSerializing => _Serial;

        /// <summary>IStep member</summary>
        public override bool IsSerial
            => IsSerializing;

        /// <summary>IStep member</summary>
        public override bool FailsProcess
            => false;

        public int RollValue
        {
            get => _RollValue ?? 0;
            set => _RollValue = value;
        }

        public override string ToString()
            => Roller.ToString();

        public override PrerequisiteInfo ToPrerequisiteInfo(CoreStep step)
        {
            var _info = ToInfo<RollPrerequisiteInfo>(step);
            _info.Expression = Roller.ToString();

            if (Roller is DiceRoller)
            {
                // dice, but only one
                var _dice = Roller as DiceRoller;
                if (_dice.Number == 1)
                    _info.SingletonSides = _dice.Sides;
            }
            else if (Roller is DieRoller)
            {
                // explicit single die
                _info.SingletonSides = (Roller as DieRoller)?.Sides;
            }

            if (IsReady)
            {
                _info.Value = RollValue;
            }
            return _info;
        }

        public override void MergeFrom(PrerequisiteInfo info)
        {
            if (info is RollPrerequisiteInfo _rollInfo)
            {
                if (_rollInfo.Value.HasValue)
                    RollValue = _rollInfo.Value.Value;
            }
        }
    }
}
