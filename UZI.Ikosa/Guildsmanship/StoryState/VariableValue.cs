using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class VariableValue : ModuleElement
    {
        private Variable _State;
        private readonly List<ValueEnabler> _Enablers;
        private VariableValueEnablerValidation _Validation;

        public VariableValue(Variable state, Description description)
            : base(description)
        {
            _State = state;
            _Enablers = new List<ValueEnabler>();
            _Validation = VariableValueEnablerValidation.Any;
        }

        public Variable Variable => _State;

        /// <summary>If any of these conditions are met, the StoryConditions are met/</summary>
        public List<ValueEnabler> ValueEnablers => _Enablers;

        public VariableValueEnablerValidation Validation
        {
            get => _Validation;
            set => _Validation = value;
        }

        /// <summary>Check if any enabler is active</summary>
        public bool IsValueEnabled(TeamTracker tracker, IEnumerable<Creature> creatures)
            => _Validation == VariableValueEnablerValidation.Any
            ? _Enablers.Any(_c => _c.Enablesvalue(tracker, creatures))
            : _Enablers.All(_c => _c.Enablesvalue(tracker, creatures));
    }
}
