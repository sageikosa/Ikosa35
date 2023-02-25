using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Guildsmanship
{
    /// <summary>
    /// Data saved and conformulated
    /// </summary>
    [Serializable]
    public class TeamTracker // IBasePart
    {
        #region state
        private readonly Guid _TeamID;
        private readonly string _Name;
        private readonly Dictionary<Guid, HashSet<Guid>> _InfoKeyMembers;
        private readonly Dictionary<Guid, Guid> _Variables;
        private readonly Dictionary<string, HashSet<Guid>> _ModuleElements;

        // record of current module
        private string _Module;

        // record of current node in the module
        private Guid _NodeID;

        // time and space
        private Vector3D _Position;
        private double _Time;
        #endregion

        // TODO: current set of Local map save-states relative to TeamTracker package, use these states if available

        [NonSerialized]
        private Module _CurrentModule = null;

        [NonSerialized]
        private ModuleNode _CurrentNode = null;

        public TeamTracker(TeamGroup teamGroup)
        {
            _NodeID = Guid.Empty;
            _TeamID = teamGroup.ID;
            _Name = teamGroup.Name;
            _ModuleElements = new Dictionary<string, HashSet<Guid>>();
            _InfoKeyMembers = new Dictionary<Guid, HashSet<Guid>>();
            _Variables = new Dictionary<Guid, Guid>();
        }

        public Guid TeamID => _TeamID;
        public string Name => _Name;

        public string ModuleName { get => _Module; set => _Module = value; }

        // TODO: get current module and current node...
        // TODO: use LocalMap saves for LocalMap state
        // TODO: set current module and current node

        public Module CurrentModule => _CurrentModule;

        public Guid NodeID { get => _NodeID; set => _NodeID = value; }

        public ModuleNode CurrentNode => _CurrentNode;

        public Vector3D Position { get => _Position; set => _Position = value; }
        public double Time { get => _Time; set => _Time = value; }

        #region modules

        // module tracking
        protected void AddModuleElement(string module, Guid guid)
        {
            if (!_ModuleElements.TryGetValue(module, out var _elements))
            {
                _elements = new HashSet<Guid>();
                _ModuleElements.Add(module, _elements);
            }
            if (!_elements.Contains(guid))
            {
                _elements.Add(guid);
            }
        }

        protected void RemoveModuleElement(string module, Guid guid)
        {
            if (_ModuleElements.TryGetValue(module, out var _elements)
                && _elements.Remove(guid)
                && (_elements.Count == 0))
            {
                _ModuleElements.Remove(module);
            }
        }

        // module clearing...
        public void ClearModuleTracking(string module)
        {
            if (_ModuleElements.TryGetValue(module, out var _elements))
            {
                // scrub elements
                foreach (var _elem in _elements)
                {
                    _InfoKeyMembers.Remove(_elem);
                    _Variables.Remove(_elem);
                }

                // scrub module
                _ModuleElements.Remove(module);
            }
        }

        #endregion

        #region info keys

        public bool HasInfoKey(Guid infoKey, Creature critter)
            => _InfoKeyMembers.TryGetValue(infoKey, out var _critterIDs)
            && _critterIDs.Contains(critter.ID);

        public bool HasInfoKey(Guid infoKey, IEnumerable<Creature> critters)
            => _InfoKeyMembers.TryGetValue(infoKey, out var _critterIDs)
            && critters.Any(_c => _critterIDs.Contains(_c.ID));

        public void AddInfoKey(string module, Guid infoKey, Creature critter)
        {
            if (!_InfoKeyMembers.TryGetValue(infoKey, out var _critterIDs))
            {
                _critterIDs = new HashSet<Guid>();
                _InfoKeyMembers.Add(infoKey, _critterIDs);
                AddModuleElement(module, infoKey);
            }

            if (!_critterIDs.Contains(critter.ID))
            {
                _critterIDs.Add(critter.ID);
            }
        }

        public void RemoveInfoKey(string module, Guid infoKey, Creature critter)
        {
            if (_InfoKeyMembers.TryGetValue(infoKey, out var _critterIDs)
                && _critterIDs.Remove(critter.ID)
                && (_critterIDs.Count == 0))
            {
                _InfoKeyMembers.Remove(infoKey);
                RemoveModuleElement(module, infoKey);
            }
        }

        public IEnumerable<Guid> InfoKeyMembers(Guid infoKey)
            => _InfoKeyMembers.TryGetValue(infoKey, out var _critterIDs)
            ? _critterIDs.Select(_i => _i).ToList()
            : new Guid[] { };

        #endregion

        #region variable values

        public Guid? GetVariable(Guid storyKey)
            => _Variables.TryGetValue(storyKey, out var _value)
            ? _value
            : null;

        public void SetVariable(string module, Guid storyKey, Guid value)
        {
            if (!_Variables.ContainsKey(storyKey))
            {
                AddModuleElement(module, storyKey);
            }
            _Variables.Add(storyKey, value);
        }

        public void ClearVariable(string module, Guid storyKey)
        {
            if (_Variables.Remove(storyKey))
            {
                RemoveModuleElement(module, storyKey);
            }
        }

        #endregion
    }
}
