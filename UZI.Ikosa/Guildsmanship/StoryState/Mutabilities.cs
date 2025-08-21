using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship
{
    /// <summary>
    /// Mutabilities defines variables that can be exposed for control with this context.
    /// </summary>
    [Serializable]
    public class Mutabilities
    {
        private HashSet<Guid> _Mutabilities;

        /// <summary>
        /// Mutabilities defines variables that can be exposed for control with this context.
        /// </summary>
        public Mutabilities()
        {
            _Mutabilities = [];
        }

        /// <summary>Add variable to local exposure list</summary>
        public void Add(Guid variableID)
        {
            if (!_Mutabilities.Contains(variableID))
            {
                _Mutabilities.Add(variableID);
            }
        }

        /// <summary>Remove variable from local exposure list</summary>
        public bool Remove(Guid variableID)
            => _Mutabilities.Remove(variableID);

        /// <summary>As defined by the local mutability list</summary>
        public bool ShowVariable(Guid variableID)
            => _Mutabilities.Contains(variableID);

        public void CleanUp(Module module)
        {
            foreach (var _m in _Mutabilities.ToList())
            {
                if (!module.CanResolveVariable(_m))
                {
                    _Mutabilities.Remove(_m);
                }
            }
        }

        /// <summary>Mutable variables defined by the local mutability list</summary>
        public IEnumerable<Variable> GetMutableVariables(Module module)
            => from _m in _Mutabilities
               let _state = module.GetVariable(_m)
               where _state.variable != null
               select _state.variable;
    }
}
