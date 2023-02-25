using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Magic.SpellLists;
using Uzi.Ikosa.Universal;
using Uzi.Core;
using Uzi.Ikosa.Advancement;

namespace Uzi.Ikosa.Fidelity
{
    /// <summary>Influences help determine a cleric's additional powers, and are provided by a devotion.</summary>
    [Serializable]
    public abstract class Influence : Adjunct, IActionSource
    {
        protected Influence(Devotion devotion, IPrimaryInfluenceClass influenceClass)
            : base(influenceClass)
        {
            _PowerLevel = 0;
            _Key = string.Empty;
            _Devotion = devotion;
        }

        // TODO: ANIMAL, DEATH, DESTRUCTION, TRAVEL: special powers
        // TODO: LUCK: meta-game re-roll
        // TODO: MAGIC: unadvanceable wizard class for spell-completion/triggers, or delta to wizard class levels

        #region data
        private int _PowerLevel;
        private string _Key;
        private Devotion _Devotion;
        #endregion

        /// <summary>PowerLevel when the influence was anchored must be set when anchoring</summary>
        public int PowerLevel { get => _PowerLevel; set { _PowerLevel = value; } }

        public IPrimaryInfluenceClass InfluenceClass => Source as IPrimaryInfluenceClass;

        /// <summary>Devotion supplying this influence</summary>
        public Devotion Devotion => _Devotion;

        /// <summary>NULL if Influence is not anchored as an adjunct</summary>
        public Creature Creature => Anchor as Creature;

        public abstract string Name { get; }
        public abstract string Description { get; }

        public string AdjunctKey { get { return _Key; } set { _Key = value; } }

        /// <summary>Influences cannot simply be removed, they must be ejected by the cleric character class.</summary>
        public override bool IsProtected => true;

        /// <summary>Pulled from Campaign SpellList for Type.Name of the Influence</summary>
        public IEnumerable<ClassSpell> Spells
        {
            get
            {
                var _key = GetType().FullName;
                if (Campaign.SystemCampaign.SpellLists.ContainsKey(_key))
                    foreach (var _spell in from _level in Campaign.SystemCampaign.SpellLists[_key]
                                           from _s in _level.Value
                                           orderby _s.Level
                                           select _s)
                        yield return _spell;
                yield break;
            }
        }

        // IActionSource
        public IVolatileValue ActionClassLevel => InfluenceClass.ClassPowerLevel;
    }
}
