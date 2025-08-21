using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Ikosa.Guildsmanship
{
    /// <summary>
    /// Registered module elements (outer dictionary) that are "associated" with Variables (nested dictionaries) when those states have specific values (nested-nested hashsets).
    /// </summary>
    [Serializable]
    public class VariableAssociationSet
    {
        // VariableAssociationSet associates a moduleElement to one or more Variables having one or more values
        //   StoryInformation members:  ModuleReference<EncounterTable>, PreferrableInfo
        //   ModuleNodes: Site, EncounterTable, Region, SitePathGraph, CreatureNode
        //   ModuleLinks: SiteLink, SubRegion, SettlementSubDivision

        // Guid key = moduleElementID
        // Dictionary<Guid key = storeStateID, HashSet<int> = acceptable values>
        private readonly Dictionary<Guid, Dictionary<Guid, HashSet<Guid>>> _Associations;

        /// <summary>
        /// Registered module elements (outer dictionary) that are "associated" with Variables (nested dictionaries) when those states have specific values (nested-nested hashsets).
        /// </summary>
        public VariableAssociationSet()
        {
            _Associations = [];
        }

        public bool? IsAssociated(TeamTracker tracker, Module module, Guid moduleElementID)
        {
            if (_Associations.TryGetValue(moduleElementID, out var _preferences))
            {
                // Variable preferences for the target
                foreach (var _kvp in _preferences)
                {
                    // TODO: check resource imported variables

                    if (module.Variables.TryGetValue(_kvp.Key, out var _state))
                    {
                        if (_kvp.Value.Any())
                        {
                            return _kvp.Value.Any(_i => tracker.GetVariable(_kvp.Key) == _i);
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<(Guid variableID, Variable variable, Guid intVal, VariableValue value)> AllAssociations(Module module, Guid moduleElementID)
        {
            if (_Associations.TryGetValue(moduleElementID, out var _preferences))
            {
                // Variable preferences for the target
                foreach (var _kvp in _preferences)
                {
                    // TODO: check resource imported variables

                    // only checking module Variables
                    if (module.Variables.TryGetValue(_kvp.Key, out var _state))
                    {
                        if (_kvp.Value.Any())
                        {
                            foreach (var _val in _kvp.Value)
                            {
                                if (_state.TryGetValue(_val, out var _sVal))
                                {
                                    yield return (_kvp.Key, _state, _val, _sVal);
                                }
                                else
                                {
                                    yield return (_kvp.Key, _state, _val, null);
                                }
                            }
                        }
                        else
                        {
                            yield return (_kvp.Key, _state, Guid.Empty, null);
                        }
                    }
                    else
                    {
                        foreach (var _val in _kvp.Value)
                        {
                            yield return (_kvp.Key, null, _val, null);
                        }
                    }
                }
            }
            yield break;
        }

        public void AddAssociation(Module module, Guid moduleElementID, Variable variable, VariableValue value)
        {
            if (!_Associations.TryGetValue(moduleElementID, out var _preferences))
            {
                _preferences = [];
                _Associations[moduleElementID] = _preferences;
            }

            // TODO: check resource imported variables

            if (module.Variables.ContainsKey(variable.ID))
            {
                if (!_preferences.TryGetValue(variable.ID, out var _list))
                {
                    // add preference condition
                    _list = [];
                    _preferences.Add(variable.ID, _list);
                }
                if (!_list.Contains(value.ID))
                {
                    // add condition value
                    _list.Add(value.ID);
                }
            }
        }

        public void RemoveAssociation(Guid moduleElementID, Variable variable, VariableValue value)
        {
            if (_Associations.TryGetValue(moduleElementID, out var _preferences))
            {
                if (_preferences.TryGetValue(variable.ID, out var _list))
                {
                    // add preference condition
                    _list.Remove(value.ID);
                    if (_list.Count == 0)
                    {
                        _preferences.Remove(variable.ID);
                        if (!_preferences.Any())
                        {
                            _Associations.Remove(moduleElementID);
                        }
                    }
                }
            }
        }

        public void RemoveAssociations(Guid moduleElementID, Variable variable)
        {
            if (_Associations.TryGetValue(moduleElementID, out var _preferences))
            {
                if (_preferences.Remove(variable.ID))
                {
                    // remove preference
                    if (!_preferences.Any())
                    {
                        _Associations.Remove(moduleElementID);
                    }
                }
            }
        }

        public void RemoveAssociations(ModuleElement target)
        {
            _Associations.Remove(target.ID);
        }

        // TODO: remove associations based on Variable or VariableValue by traversing target lists

    }
}
