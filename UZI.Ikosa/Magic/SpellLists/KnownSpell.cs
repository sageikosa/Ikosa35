using System;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Known spells are used by casters who learn or aquire spells over time or through study</summary>
    [Serializable]
    public class KnownSpell : IFeature
    {
        #region Construction
        public KnownSpell(SpellDef spellDef, int slotLevel, int learnedLevel, int learnedIndex)
        {
            _Def = spellDef;
            _SlotLvl = slotLevel;
            _LearnLvl = learnedLevel;
            _Idx = learnedIndex;
        }
        #endregion

        #region data
        private SpellDef _Def;
        private int _SlotLvl;
        private int _LearnLvl;
        private int _Idx;
        #endregion

        public SpellDef SpellDef => _Def;
        public int SlotLevel => _SlotLvl;
        public int LearnedLevel => _LearnLvl;
        public int LearnedIndex => _Idx;

        /// <summary>True if the SpellDefs are Equal</summary>
        public override bool Equals(object obj)
            => (obj is KnownSpell _ks)
            ? SpellDef.Equals(_ks.SpellDef)
            : false;

        public override int GetHashCode()
            => SpellDef.GetHashCode();

        #region IFeature Members

        public string Name => $@"{SpellDef.DisplayName} (Level:{SlotLevel})";
        public string Description => SpellDef.Description;

        public FeatureInfo ToFeatureInfo()
            => new FeatureInfo
            {
                Name = Name,
                Description = Description
            };

        #endregion
    }
}
