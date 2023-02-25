using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class VariableValueEnabler : ValueEnabler
    {
        private Guid _State;
        private Guid _Value;

        public VariableValueEnabler(Description description)
            : base(description)
        {
            _State = Guid.Empty;
            _Value = Guid.Empty;
        }

        public Guid VariableID { get => _State; set => _State = value; }
        public Guid ValueID { get => _Value; set => _Value = value; }

        public (Module module, Guid id, Variable variable, Guid value, VariableValue choice) RequiredState(
            Module module)
        {
            if (module != null)
            {
                // look for state in module or imports
                var (_module, _state) = module.GetVariable(_State);
                if (_state != null)
                {
                    // found, now get expected choice
                    if (_state.TryGetValue(_Value, out var _choice))
                    {
                        // state, expected choice, and whether met in team tracker
                        return (_module, _state.ID, _state, _choice.ID, _choice);

                    }
                    else
                    {
                        // unable to find expected choice
                        return (_module, _state.ID, _state, _Value, null);
                    }
                }
                else
                {
                    // unable to find the state
                    return (null, _State, null, _Value, null);
                }
            }
            return (null, Guid.Empty, null, Guid.Empty, null);
        }

        public (Module module, Guid id, Variable variable, Guid value, VariableValue choice, bool isMet) RequiredState(
            TeamTracker tracker, Module module)
        {
            if (module != null)
            {
                // look for state in module or imports
                var (_module, _state) = module.GetVariable(_State);
                if (_state != null)
                {
                    // found, now get expected choice
                    if (_state.TryGetValue(_Value, out var _choice))
                    {
                        // state, expected choice, and whether met in team tracker
                        return (_module, _state.ID, _state, _choice.ID, _choice, _choice.ID == tracker.GetVariable(_Value));

                    }
                    else
                    {
                        // unable to find expected choice
                        return (_module, _state.ID, _state, _Value, null, false);
                    }
                }
                else
                {
                    // unable to find the state
                    return (null, _State, null, _Value, null, false);
                }
            }
            return (null, Guid.Empty, null, Guid.Empty, null, false);
        }

        /// <summary>Each required Variable must have its required value</summary>
        public override bool Enablesvalue(TeamTracker tracker, IEnumerable<Creature> creatures)
            => tracker?.GetVariable(_State) == _Value;
    }
}
