using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core.Contracts;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    /// <summary>Schema for Variable</summary>
    [Serializable]
    public class Variable : ModuleElement, ICorePart
    {
        // Variable is a module-level construct
        // Variable defines choices

        private readonly Dictionary<Guid, VariableValue> _Choices;

        /// <summary>Schema for Variable</summary>
        public Variable(Description description)
            : base(description)
        {
            _Choices = [];
        }

        public VariableValue Add(Description description)
            => Add(new VariableValue(this, description));

        public VariableValue Add(VariableValue variableValue)
        {
            _Choices.Add(variableValue.ID, variableValue);
            return variableValue;
        }

        public bool Remove(Guid key)
            => _Choices.Remove(key);

        public bool TryGetValue(Guid key, out VariableValue outVal)
            => _Choices.TryGetValue(key, out outVal);

        public bool ContainsKey(Guid key)
            => _Choices.ContainsKey(key);

        public IEnumerable<VariableValue> Values => _Choices.Values.Select(_v => _v);

        public string Name => Description.Message;

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName => typeof(Variable).FullName;
    }
}
