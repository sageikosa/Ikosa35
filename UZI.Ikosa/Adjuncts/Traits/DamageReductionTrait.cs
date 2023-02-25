using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Uzi.Ikosa.Items.Weapons;
using System.Linq;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Creatures.Types;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class DamageReductionTrait : TraitEffect, IDamageReduction
    {
        public DamageReductionTrait(ITraitSource species, int amount, params DRException[] drTypes)
            : base(species)
        {
            Amount = amount;
            _Exceptions = new Collection<DRException>();
            if (drTypes?.Any() ?? false)
                foreach (var _drType in drTypes)
                {
                    _Exceptions.Add(_drType);
                }
        }

        protected Collection<DRException> _Exceptions;

        public override string ToString()
        {
            var _ret = new StringBuilder();
            _ret.Append($@"DR {Amount}/");
            if (_Exceptions.Any())
            {
                for (var _ex = 0; _ex < _Exceptions.Count; _ex++)
                {
                    _ret.AppendFormat("{0}{1}", (_ex > 0 ? " or " : string.Empty), _Exceptions[_ex].Name);
                }
            }
            else
            {
                _ret.Append(@"-");
            }
            return _ret.ToString();
        }

        #region IDamageReduction Members

        public string Name
            => ToString();

        public bool WeaponIgnoresReduction(IWeaponHead weaponHead)
        {
            foreach (var _drType in _Exceptions)
            {
                if (_drType.DoesWeaponIgnoreReduction(weaponHead))
                    return true;
            }
            return false;
        }

        public int Amount { get; protected set; }

        public IEnumerable<DRException> Exceptions
            => _Exceptions.Select(_e => _e);

        public void HasReduced(int amount) { }

        #endregion

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            Creature?.DamageReductions.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            Creature?.DamageReductions.Remove(this);
            base.OnDeactivate(source);
        }

        public override object Clone()
            => new DamageReductionTrait(TraitSource, Amount, _Exceptions.ToArray());

        public static SuperNaturalTrait GetDRMagicTrait(ITraitSource traitSource, int amount, IPowerClass powerClass)
        {
            // damage Reduction
            var _dr = new DamageReductionTrait(traitSource, amount, new DRMagicException());
            var _power = new SuperNaturalPowerSource(powerClass, 1, new SuperNaturalPowerDef(_dr.Name, @"Ignores some damage per weapon attack", new Abjuration()));
            return new SuperNaturalTrait(traitSource, _power, TraitCategory.Quality, _dr);
        }

        public static ExtraordinaryTrait GetDRDamageTypeTrait(ITraitSource traitSource, int amount, params Contracts.DamageType[] damageType)
        {
            // damage Reduction
            var _dr = new DamageReductionTrait(traitSource, amount, damageType.Select(_dt => new DRDamageTypeException(_dt)).ToArray());
            return new ExtraordinaryTrait(traitSource, _dr.Name, @"Ignores some damage from most weapons", TraitCategory.Quality, _dr);
        }

        public static ExtraordinaryTrait GetDRAllTrait(ITraitSource traitSource, int amount)
        {
            // damage Reduction
            var _dr = new DamageReductionTrait(traitSource, amount);
            return new ExtraordinaryTrait(traitSource, _dr.Name, @"Ignores some damage from all weapons", TraitCategory.Quality, _dr);
        }

        public override TraitEffect Clone(ITraitSource traitSource)
            => new DamageReductionTrait(traitSource, Amount, _Exceptions.ToArray());
    }
}
