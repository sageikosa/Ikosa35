using System;
using System.Text;
using Uzi.Core.Dice;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public class DiceRange : Range
    {
        #region ctor()
        public DiceRange(string key, string description, Roller value)
        {
            OffSet = value;
            PerStep = new ConstantRoller(0);
            StepSize = 1;
            GroundStep = 0;
            _MaxStepDice = 0;
            Key = key;
            Description = description;
        }

        public DiceRange(string key, string description, int maxStepDice, Roller perStep, int stepSize)
        {
            OffSet = new ConstantRoller(0);
            PerStep = perStep;
            StepSize = stepSize;
            GroundStep = 0;
            _MaxStepDice = maxStepDice;
            Key = key;
            Description = description;
        }

        public DiceRange(string key, string description, Roller offSet, int maxStepDice, Roller perStep, int stepSize)
            : base()
        {
            OffSet = offSet;
            PerStep = perStep;
            StepSize = stepSize;
            GroundStep = 0;
            _MaxStepDice = maxStepDice;
            Key = key;
            Description = description;
        }

        public DiceRange(string key, string description, Roller offSet, int maxStepDice, Roller perStep, int stepSize, int groundStep)
            : base()
        {
            OffSet = offSet;
            PerStep = perStep;
            StepSize = stepSize;
            GroundStep = groundStep;
            _MaxStepDice = maxStepDice;
            Key = key;
            Description = description;
        }
        #endregion

        public string Key { get; set; }
        public string Description { get; set; }

        /// <summary>Non-level dependent rolling</summary>
        public Roller OffSet { get; set; }

        /// <summary>How much rolling per step</summary>
        public Roller PerStep { get; set; }

        /// <summary>How many power levels count as a step</summary>
        public int StepSize { get; private set; }

        /// <summary>Base power level from which steps start</summary>
        public int GroundStep { get; private set; }

        private readonly int _MaxStepDice;
        public int MaxStepDice => _MaxStepDice;

        public override double EffectiveRange(CoreActor actor, int powerLevel)
        {
            Guid[] _actor = actor != null ? new[] { actor.ID } : null;

            var _result = OffSet.RollValue(actor?.ID ?? Guid.Empty, Key, Description, _actor);
            var _steps = (powerLevel - GroundStep) / StepSize;
            for (var _sx = 0; _sx < _steps; _sx++)
            {
                _result += PerStep.RollValue(actor?.ID ?? Guid.Empty, Key, Description, _actor);
            }
            return Convert.ToDouble(_result);
        }

        #region public Roller EffectiveRoller(int powerLevel)
        /// <summary>Build a complex roller for the dice range</summary>
        public Roller EffectiveRoller(Guid principalID, int powerLevel, params Guid[] targets)
        {
            var _expr = new StringBuilder();
            var _steps = (powerLevel - GroundStep) / StepSize;
            var _complex = new ComplexDiceRoller();

            // offset first
            if ((!(OffSet is ConstantRoller _const)) || (_const.RollValue(principalID, Key, Description, targets) != 0))
            {
                // only if it isn't a constant roller, or the constant is not 0
                _complex.Add(OffSet);
                _expr.AppendFormat(@"{0}", OffSet.ToString());
            }

            // then the per Step parts
            if (PerStep is DieRoller _die)
            {
                // single die (per step) becomes multiple dice
                _steps = Math.Min(MaxStepDice, _steps);
                _complex.Add(new DiceRoller(_steps, _die.Sides));
                _expr.AppendFormat(@"{0}{1}d{2}", (_expr.Length > 0 ? @"+" : string.Empty), _steps, _die.Sides);
            }
            else
            {
                if (PerStep is DiceRoller _dice)
                {
                    // multiple dice per step becomes even more multiple steps per dice
                    _steps = _dice.Number * _steps;
                    _steps = Math.Min(MaxStepDice, _steps);
                    _complex.Add(new DiceRoller(_steps, _dice.Sides));
                    _expr.AppendFormat(@"{0}{1}d{2}", (_expr.Length > 0 ? @"+" : string.Empty), _steps, _dice.Sides);
                }
                else
                {
                    _const = PerStep as ConstantRoller;
                    if ((_const != null) && (_const.RollValue(principalID, Key, Description, targets) != 0))
                    {
                        // constant per step is a multiplicative constant
                        _steps = _const.RollValue(principalID, Key, Description, targets) * _steps;
                        _steps = Math.Min(MaxStepDice, _steps);
                        _complex.Add(new ConstantRoller(_steps));
                        _expr.AppendFormat(@"{0}{1}", (_expr.Length > 0 ? @"+" : string.Empty), _steps);
                    }
                }
            }

            // set the expression also
            _complex.SetExpression(_expr.ToString());
            return _complex;
        }
        #endregion

        public override RangeInfo ToRangeInfo(CoreAction action, CoreActor actor)
        {
            var _info = ToInfo<DiceRangeInfo>(action, actor);
            _info.RollerString = EffectiveRoller(actor?.ID ?? Guid.Empty, _info.ClassLevel).ToString();
            _info.OffsetRoll = OffSet.ToString();
            _info.PerStep = PerStep.ToString();
            _info.StepSize = StepSize;
            _info.GroundStep = GroundStep;
            _info.MaxStepDice = MaxStepDice;
            return _info;
        }
    }
}
