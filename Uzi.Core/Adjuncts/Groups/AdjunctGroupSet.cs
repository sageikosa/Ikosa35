using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>Used by settings to keep track of adjunct groups (across objects)</summary>
    [Serializable]
    public class AdjunctGroupSet : ICanReactBySideEffect, ICanReactBySuppress, ICanReactToStepComplete
    {
        public AdjunctGroupSet()
        {
            _AdjunctGroups = new Dictionary<Guid, AdjunctGroup>();
        }

        private Dictionary<Guid, AdjunctGroup> _AdjunctGroups;

        #region public void Add(AdjunctGroup group)
        /// <summary>Safely add a group (if ID is already present, it is not re-added)</summary>
        internal void Add(AdjunctGroup group)
        {
            if (!_AdjunctGroups.ContainsKey(group.ID))
            {
                _AdjunctGroups.Add(group.ID, group);
                group.OnAdded();
            }
        }
        #endregion

        #region internal void Remove(AdjunctGroup group)
        /// <summary>Removes by the group's ID</summary>
        internal void Remove(AdjunctGroup group)
        {
            if (_AdjunctGroups.ContainsKey(group?.ID ?? Guid.Empty))
            {
                group.OnRemoved();
                _AdjunctGroups.Remove(group?.ID ?? Guid.Empty);
            }
        }
        #endregion

        #region public bool Contains(Guid id)
        public bool Contains(Guid id)
        {
            return _AdjunctGroups.ContainsKey(id);
        }
        #endregion

        #region public AdjunctGroup this[Guid id] { get; }
        public AdjunctGroup this[Guid id]
        {
            get
            {
                if (_AdjunctGroups.TryGetValue(id, out var _group))
                    return _group;
                return null;
            }
        }
        #endregion

        #region public IEnumerable<AdjunctGroup> All()
        public IEnumerable<AdjunctGroup> All()
        {
            foreach (var _kvp in _AdjunctGroups)
                yield return _kvp.Value;
            yield break;
        }
        #endregion

        public GroupType Singleton<GroupType>(Func<GroupType> generator)
            where GroupType : AdjunctGroup
            => All().OfType<GroupType>().FirstOrDefault() ?? generator?.Invoke();

        public bool IsFunctional => true;

        public void ReactToProcessBySideEffect(CoreProcess process)
        {
            foreach (var _g in _AdjunctGroups.OfType<ICanReactBySideEffect>())
                _g.ReactToProcessBySideEffect(process);
        }

        public void ReactToProcessBySuppress(CoreProcess process)
        {
            foreach (var _g in _AdjunctGroups.OfType<ICanReactBySuppress>())
                _g.ReactToProcessBySuppress(process);
        }

        public void ReactToStepComplete(CoreStep step)
        {
            foreach (var _g in _AdjunctGroups.OfType<ICanReactToStepComplete>())
                _g.ReactToStepComplete(step);
        }

        public bool CanReactToStepComplete(CoreStep step)
            => _AdjunctGroups.OfType<ICanReactToStepComplete>().Any(_g => _g.CanReactToStepComplete(step));
    }
}
