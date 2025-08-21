using System;
using System.Collections.Generic;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class HeldItemsGroupSet : ICreatureBound
    {
        #region Construction
        internal HeldItemsGroupSet(Creature creature)
        {
            Creature = creature;
            _Groups = [];
        }
        #endregion

        private Dictionary<string, HeldItemsGroup> _Groups;
        public Creature Creature { get; private set; }

        #region public HeldItemsGroup Snapshot(string index)
        /// <summary>
        /// Create a new named group with current items
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public HeldItemsGroup Snapshot(string index)
        {
            HeldItemsGroup _group = new HeldItemsGroup(Creature);
            _Groups.Add(index, _group);
            return _group;
        }
        #endregion

        #region public void Remove(string index)
        /// <summary>
        /// Removes a named group
        /// </summary>
        /// <param name="index"></param>
        public void Remove(string index)
        {
            if (_Groups.ContainsKey(index))
            {
                _Groups.Remove(index);
            }
        }
        #endregion

        public IEnumerable<KeyValuePair<string, HeldItemsGroup>> GetGroups()
        {
            foreach (var _kvp in _Groups)
            {
                yield return _kvp;
            }

            yield break;
        }
    }
}
