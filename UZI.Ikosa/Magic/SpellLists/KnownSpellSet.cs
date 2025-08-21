using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Magic
{
    [Serializable]
    public class KnownSpellSet
    {
        #region construction
        public KnownSpellSet()
        {
            _Spells = [];
        }
        #endregion

        private Collection<KnownSpell> _Spells;

        #region public void Add(KnownSpell spell)
        /// <summary>Replaces by learned level and learned index if necessary</summary>
        public void Add(KnownSpell spell)
        {
            var _exist = _Spells.FirstOrDefault(_ks => (_ks.LearnedLevel == spell.LearnedLevel)
                && (_ks.LearnedIndex == spell.LearnedIndex));
            if (_exist != null)
            {
                _Spells.Remove(_exist);
            }

            _Spells.Add(spell);
        }
        #endregion

        #region public void Remove(KnownSpell spell)
        public void Remove(KnownSpell spell)
        {
            if (Contains(spell))
            {
                _Spells.Remove(this[spell]);
            }
        }
        #endregion

        #region public IEnumerable<KnownSpell> AllKnown { get; }
        public IEnumerable<KnownSpell> AllKnown
        {
            get
            {
                foreach (var _spell in _Spells)
                {
                    yield return _spell;
                }

                yield break;
            }
        }
        #endregion

        #region public IEnumerable<KnownSpell> KnownForSpellLevel(int level)
        /// <summary>Enumerates the known spells for a specific spell level</summary>
        public IEnumerable<KnownSpell> KnownForSpellLevel(int slotLevel)
        {
            foreach (var _known in _Spells.Where(_ks => _ks.SlotLevel == slotLevel))
            {
                yield return _known;
            }

            yield break;
        }
        #endregion

        #region public IEnumerable<KnownSpell> LearnedAtPowerLevel(int classPowerLevel)
        /// <summary>Enumerates knowns spells learned as a specific power level</summary>
        public IEnumerable<KnownSpell> LearnedAtPowerLevel(int classPowerLevel)
        {
            foreach (var _known in _Spells.Where(_ks => _ks.LearnedLevel == classPowerLevel))
            {
                yield return _known;
            }

            yield break;
        }
        #endregion

        #region public KnownSpell LearnedSpell(int classPowerLevel, int index)
        public KnownSpell LearnedSpell(int classPowerLevel, int index)
        {
            return _Spells.FirstOrDefault(_ks => (_ks.LearnedLevel == classPowerLevel) && (_ks.LearnedIndex == index));
        }
        #endregion

        #region public bool Contains(KnownSpell tester)
        /// <summary>Tests to see if the spell is known</summary>
        public bool Contains(KnownSpell tester)
        {
            return _Spells.Any(_ks => _ks.Equals(tester));
        }
        #endregion

        /// <summary>Returns equivalent known spell in the set</summary>
        public KnownSpell this[KnownSpell index]
        {
            get { return _Spells.FirstOrDefault(_ks => _ks.Equals(index)); }
        }
    }
}
