using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Adjuncts;

namespace Uzi.Ikosa.Abilities
{
    [Serializable]
    public class AbilitySet
    {
        #region ctor()
        public AbilitySet(Strength strength, Dexterity dexterity, Constitution constitution,
            Intelligence intelligence, Wisdom wisdom, Charisma charisma)
        {
            _AbilitySet = new Dictionary<Type, AbilityBase>
            {
                { strength.GetType(), strength },
                { dexterity.GetType(), dexterity },
                { constitution.GetType(), constitution },
                { intelligence.GetType(), intelligence },
                { wisdom.GetType(), wisdom },
                { charisma.GetType(), charisma }
            };
            HookAbilities();
        }

        public AbilitySet(int strength, int dexterity, int constitution, int intelligence, int wisdom, int charisma)
        {
            _AbilitySet = new Dictionary<Type, AbilityBase>
            {
                { typeof(Strength), new Strength(strength) },
                { typeof(Dexterity), new Dexterity(dexterity) },
                { typeof(Constitution), new Constitution(constitution) },
                { typeof(Intelligence), new Intelligence(intelligence) },
                { typeof(Wisdom), new Wisdom(wisdom) },
                { typeof(Charisma), new Charisma(charisma) }
            };
            HookAbilities();
        }

        private void HookAbilities()
        {
            Strength.Abilities = this;
            Dexterity.Abilities = this;
            Constitution.Abilities = this;
            Intelligence.Abilities = this;
            Wisdom.Abilities = this;
            Charisma.Abilities = this;
        }
        #endregion

        #region data
        protected Dictionary<Type, AbilityBase> _AbilitySet;
        #endregion

        public Strength Strength => Ability<Strength>();
        public Dexterity Dexterity => Ability<Dexterity>();
        public Constitution Constitution => Ability<Constitution>();
        public Intelligence Intelligence => Ability<Intelligence>();
        public Wisdom Wisdom => Ability<Wisdom>();
        public Charisma Charisma => Ability<Charisma>();

        public A Ability<A>() where A : AbilityBase
            => (A)(_AbilitySet[typeof(A)]);

        public AbilityBase this[string mnemonic]
            => _AbilitySet.FirstOrDefault(_kvp => _kvp.Value.Mnemonic.Equals(mnemonic)).Value;

        #region public IEnumerable<AbilityBase> AllAbilities
        public IEnumerable<AbilityBase> AllAbilities
        {
            get
            {
                yield return Strength;
                yield return Dexterity;
                yield return Constitution;
                yield return Intelligence;
                yield return Wisdom;
                yield return Charisma;
                yield break;
            }
        }
        #endregion

        #region public AbilitySetInfo ToAbilitySetInfo(Creature critter)
        public AbilitySetInfo ToAbilitySetInfo(Creature critter)
        {
            var _expiry = critter.GetTake10Remaining(typeof(AbilityBase));
            var _info = new AbilitySetInfo
            {
                Strength = Strength.ToAbilityInfo(critter),
                Dexterity = Dexterity.ToAbilityInfo(critter),
                Constitution = Constitution.ToAbilityInfo(critter),
                Intelligence = Intelligence.ToAbilityInfo(critter),
                Wisdom = Wisdom.ToAbilityInfo(critter),
                Charisma = Charisma.ToAbilityInfo(critter),
                Take10 = _expiry != null
                    ? new Take10Info { RemainingRounds = _expiry.Value }
                    : null
            };
            return _info;
        }
        #endregion

        public AbilitySet Clone()
            => new AbilitySet(Strength.Unboosted(), Dexterity.Unboosted(), Constitution.Unboosted(), 
                Intelligence.Unboosted(), Wisdom.Unboosted(), Charisma.Unboosted());
    }

    #region MnemonicCode Constants
    public static class MnemonicCode
    {
        public const string Str = @"STR";
        public const string Dex = @"DEX";
        public const string Con = @"CON";
        public const string Int = @"INT";
        public const string Wis = @"WIS";
        public const string Cha = @"CHA";
        public static (string Mnemonic, string Name)[] AllAbilities
            => new (string, string)[]
            {
                (Str, @"Strength"),
                (Dex, @"Dexterity"),
                (Con, @"Constitution"),
                (Int, @"Intelligence"),
                (Wis, @"Wisdom"),
                (Cha, @"Charisma")
            };
    }
    #endregion
}
