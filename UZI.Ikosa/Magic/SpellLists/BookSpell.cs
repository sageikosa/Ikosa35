using System;
using Uzi.Ikosa.Magic.SpellLists;
using System.Collections.ObjectModel;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Magic
{
    /// <summary>Book spells are used by certain preparational casters</summary>
    [Serializable]
    public class BookSpell : ClassSpell
    {
        #region construction
        public BookSpell(int level, SpellDef spellDef, Guid bookOwner)
            : base(level, spellDef)
        {
            _Deciphered = new Collection<Guid>();
            _Owner = bookOwner;
            _Deciphered.Add(bookOwner);
        }
        #endregion

        #region data
        private Guid _Owner;
        private Collection<Guid> _Deciphered;
        #endregion

        public Guid Owner => _Owner;

        #region public void Decipher (...)
        public void Decipher(ICasterClass caster)
        {
            if (!HasDecipheredOrScribed(caster))
                _Deciphered.Add(caster.OwnerID);
        }

        public void Decipher(Guid guid)
        {
            if (!HasDecipheredOrScribed(guid))
                _Deciphered.Add(guid);
        }
        #endregion

        public bool HasDecipheredOrScribed(ICasterClass caster)
            => _Deciphered.Contains(caster.OwnerID);

        public bool HasDecipheredOrScribed(Guid guid)
            => _Deciphered.Contains(guid);

        public override ClassSpellInfo ToClassSpellInfo()
            => new BookSpellInfo
            {
                Level = Level,
                SpellDef = SpellDef.GetSpellDefInfo(),
                Message = SpellDef.DisplayName,
                Owner = Owner
            };
    }
}
