using System;
using System.Security.Cryptography;
using System.Linq;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core.Dice
{
    /// <summary>Handles expressions like 2d6+5+1d6 and 2d8*10</summary>
    [Serializable]
    public class ComplexDiceRoller : Roller
    {
        /// <summary>Construction with an empty set of rollers (use Add to build out step by step)</summary>
        public ComplexDiceRoller()
        {
            _Expression = string.Empty;
            _ComplexRollers = new List<Roller>();
        }

        #region Constructor(string expression)
        public ComplexDiceRoller(string expression)
        {
            //-- Remove Spaces and Store Value --
            expression = expression.Replace(@" ", @"");
            _Expression = expression;

            _ComplexRollers = new List<Roller>();
            var _rollers = expression.Split('+', '-');
            foreach (var _roller in _rollers)
            {
                #region Get Sign (+/-) and Step past it
                //-- Assume Positive (at beginning of current Expression) --
                var _signFactor = 1;
                if (expression[0] == '-')
                {
                    //-- Change Sign and Step past negative --
                    _signFactor = -1;
                    expression = expression.Substring(1);
                }
                else if (expression[0] == '+')
                {
                    //-- Only step past positive if explicit --
                    expression = expression.Substring(1);
                }
                #endregion

                //-- Ensure an initial "blank" is skipped if negative --
                if (_roller.Length > 0)
                {
                    var _diceParts = _roller.Trim().Split('d', 'D');
                    if (_diceParts.Length == 1)
                    {
                        _ComplexRollers.Add(new ConstantRoller(Convert.ToInt32(_diceParts[0]) * _signFactor));
                    }
                    else
                    {
                        // look for multiplication
                        var _scaling = _diceParts[1].Split('*', 'x', 'X');
                        if (_scaling.Length == 2)
                        {
                            // allows 2d8*10 or 2d6x10
                            _ComplexRollers.Add(new DiceRoller(Convert.ToInt32(_diceParts[0]) * _signFactor, Convert.ToByte(_scaling[0]), Convert.ToDecimal(_scaling[1])));
                        }
                        else
                        {
                            _ComplexRollers.Add(new DiceRoller(Convert.ToInt32(_diceParts[0]) * _signFactor, Convert.ToByte(_scaling[0])));
                        }
                    }
                }

                //-- Step past the Roller Expression --
                expression = expression.Substring(_roller.Length);
            }
        }
        #endregion

        #region data
        private string _Expression;
        private readonly List<Roller> _ComplexRollers;
        #endregion

        public string Expression => _Expression;

        public void Add(Roller roller) { _ComplexRollers.Add(roller); }

        /// <summary>To be used to sync the expression string back to the collection built using Add()</summary>
        public void SetExpression(string expression) { _Expression = expression; }

        public override int RollValue(Guid? principalID, string title, string description, params Guid[] targets)
            => Roller.LogRollValue(title, description, principalID ?? Guid.Empty, GetRollerLog(), targets);

        #region public override RollerLog GetRollerLog()
        public override RollerLog GetRollerLog()
        {
            var _log = new RollerLog { Expression = Expression };
            _log.Parts = (from _cr in _ComplexRollers
                          select _cr.GetRollerLog()).ToList();
            _log.Total = _log.Parts.Sum(_l => _l.Total);
            return _log;
        }
        #endregion

        public override string ToString() => Expression;

        public override int MaxRoll
            => _ComplexRollers.Sum(_cr => _cr.MaxRoll);
    }
}
