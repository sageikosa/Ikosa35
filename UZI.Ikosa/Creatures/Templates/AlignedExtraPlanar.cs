using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Creatures.SubTypes;
using Uzi.Ikosa.Creatures.Types;
using Uzi.Ikosa.Items.Weapons;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Creatures.Templates
{
    [Serializable]
    public abstract class AlignedExtraPlanar : Adjunct, ICreatureTemplate,
        IMonitorChange<PowerDiceCount>, IPowerClass, ITraitSource
    {
        protected AlignedExtraPlanar(object source)
            : base(source)
        {
            _Original = null;
            _IntActivated = false;
            _IntBoost = null;
            _PowerLevel = new DeltableQualifiedDelta(0, @"Power-Level", this);
            _SR = new Delta(0, typeof(Creature), TemplateName);
            _ER = new Delta(0, typeof(EnergyResistance), TemplateName);
        }

        #region data
        private CreatureType _Original;
        private bool _IntActivated;
        private Delta _IntBoost;
        private Delta _SR;
        private Delta _ER;
        private DeltableQualifiedDelta _PowerLevel;
        #endregion

        public string ClassName => TemplateName;
        public abstract string TemplateName { get; }
        public abstract string ClassIconKey { get; }
        protected abstract bool IsAlignable(Alignment alignment);
        protected abstract IEnumerable<Type> AllowedCreatureTypes();
        protected abstract IEnumerable<EnergyType> ResistedEnergies();
        protected abstract Alignment GetSmitingAlignment();
        protected abstract Alignment GetCreatureAlignment();

        public override bool CanAnchor(IAdjunctable newAnchor)
            => ((newAnchor is Creature _critter)
                && IsAlignable(_critter.Alignment)
                && !(_critter.Body is NoBody)
                && !_critter.HasAdjunct<Incorporeal>()
                && AllowedCreatureTypes().Contains(_critter.CreatureType.GetType())
                && !_critter.Adjuncts.Any(_a => (_a.GetType() == GetType()) && (_a != this))
                );

        public bool IsIntelligenceActivated => _IntActivated;
        public int IntelligenceBoost => _IntBoost?.Value ?? 0;
        public CreatureType OriginalCreatureType => _Original;

        public override bool CanUnAnchor()
        {
            return base.CanUnAnchor();
        }

        public bool IsAcquired => false;

        public Creature Creature => Anchor as Creature;

        #region OnActivate()
        protected override void OnActivate(object source)
        {
            Creature.AdvancementLog.AddChangeMonitor(this);

            // change type if needed
            if (Creature.CreatureType is AnimalType _animalType)
            {
                // track original
                _Original = _animalType;

                // now a magical beast
                var _magicalBeast = new MagicalBeastType();
                _Original.UnbindFromCreature();
                _magicalBeast.BindTo(Creature);
            }
            else if (Creature.CreatureType is VerminType _verminType)
            {
                // track original
                _Original = _verminType;

                // now a magical beast
                var _magicalBeast = new MagicalBeastType();
                _Original.UnbindFromCreature();
                _magicalBeast.BindTo(Creature);
            }

            // extraplanar subtype
            Creature.SubTypes.Add(new ExtraplanarSubType(this));

            // darkvision
            Creature.Senses.Add(new Darkvision(60, this));

            // damage reduction
            var _powerDice = Creature.AdvancementLog.NumberPowerDice;
            var _dr = DamageReductionAmount(_powerDice);
            if (_dr > 0)
            {
                Creature.AddAdjunct(DamageReductionTrait.GetDRMagicTrait(this, _dr, this));
                AugmentNaturalWeapons();
            }

            // resistance to energies
            if (ResistedEnergies().Any())
            {
                _ER.Value = _powerDice > 7 ? 10 : 5;
                AddEnergyResistanceTrait(_ER);
            }

            // spell resistance
            _SR.Value = Math.Min(5 + _powerDice, 25);
            AddSpellResistanceTrait(_SR);

            // alignment
            var _aligned = new AlignedCreature(GetCreatureAlignment());
            Creature.AddAdjunct(new ExtraordinaryTrait(this, @"Alignment", $@"Always {_aligned.Alignment.ToString()}",
                TraitCategory.Quality, new AdjunctTrait(this, _aligned)));

            // smiting
            AddSmiteTrait(GetSmitingAlignment());

            // boost Intelligence to 3 if needed
            var _intelligence = Creature.Abilities.Intelligence;
            if (_intelligence.IsNonAbility)
            {
                _IntActivated = true;
                _intelligence.IsNonAbility = false;
            }
            if (_intelligence.EffectiveValue < 3)
            {
                _IntBoost = new Delta(3 - _intelligence.EffectiveValue, this);
                _intelligence.Deltas.Add(_IntBoost);
            }
            base.OnActivate(source);
        }
        #endregion

        #region OnDeactivate()
        protected override void OnDeactivate(object source)
        {
            // cancel any intelligence boost
            if (_IntActivated)
            {
                Creature.Abilities.Intelligence.IsNonAbility = true;
                _IntActivated = false;
            }
            _IntBoost?.DoTerminate();
            DoTerminate();

            // spell resistance
            foreach (var _trait in MyTraits().ToList())
            {
                _trait.Eject();
            }

            // darkvision
            var _darkvision = Creature.Senses.AllSenses.OfType<Darkvision>().FirstOrDefault(_dv => _dv.Source == this);
            Creature.Senses.Remove(_darkvision);

            // extra planar
            Creature.SubTypes.Remove(
                Creature.SubTypes.OfType<ExtraplanarSubType>().FirstOrDefault(_epst => _epst.Source == this));

            if (_Original != null)
            {
                // drop replacement creature type
                Creature.CreatureType.UnbindFromCreature();

                // rebind original creature type
                _Original.BindTo(Creature);
            }
            Creature.AdvancementLog.RemoveChangeMonitor(this);
            base.OnDeactivate(source);
        }
        #endregion

        #region Trait Assistance

        private void AddSmiteTrait(Alignment alignment)
        {
            var _def = new SuperNaturalPowerDef(
                $@"Smite {alignment.NoNeutralString()}", $@"Extra damage to {alignment.NoNeutralString()} creatures", new Transformation());
            var _src = new SuperNaturalPowerSource(this, 1, _def);
            var _smite = new SmiteTrait(this, alignment);
            Creature.AddAdjunct(new SuperNaturalTrait(this, _src, TraitCategory.Quality, _smite));
        }

        private void AddSpellResistanceTrait(IModifier amount)
        {
            var _def = new SuperNaturalPowerDef(
                @"Spell Resistance", amount.Value.ToString(), new Abjuration());
            var _src = new SuperNaturalPowerSource(this, 1, _def);
            var _sr = new DeltaTrait(this, amount, Creature.SpellResistance);
            Creature.AddAdjunct(new SuperNaturalTrait(this, _src, TraitCategory.Quality, _sr));
        }

        private void AddEnergyResistanceTrait(IModifier amount)
        {
            var _energies = ResistedEnergies().ToList();
            var _def = new SuperNaturalPowerDef(
                @"Energy Resistances", string.Join(@",", _energies.Select(_e => _e.ToString())), new Abjuration());
            var _src = new SuperNaturalPowerSource(this, 1, _def);
            var _dr = new DeltaTrait(
                this, amount, _energies.Select(_re => Creature.EnergyResistances[_re]).ToArray());
            Creature.AddAdjunct(new SuperNaturalTrait(this, _src, TraitCategory.Quality, _dr));
        }

        private void AugmentNaturalWeapons()
            => Creature.AddAdjunct(
                MagicNaturalWeaponsTrait.GetMagicNaturalWeaponsTrait(this, 0, this));

        private IEnumerable<SuperNaturalTrait> MyTraits()
            => Creature.Traits
            .OfType<SuperNaturalTrait>().Where(_snt => _snt.TraitSource == this);

        private int DamageReductionAmount(int powerDice)
            => powerDice >= 12 ? 10
            : powerDice >= 4 ? 5
            : 0;

        #endregion

        #region IMonitorChange<PowerDiceCount>
        public void PreTestChange(object sender, AbortableChangeEventArgs<PowerDiceCount> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<PowerDiceCount> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<PowerDiceCount> args)
        {
            var _powerDice = Creature.AdvancementLog.NumberPowerDice;

            // update energy reistances
            _ER.Value = _powerDice > 7 ? 10 : 5;

            // update spell-resistance
            _SR.Value = Math.Min(5 + _powerDice, 25);

            // adjust damage-reduction
            var _oldDR = DamageReductionAmount(args.OldValue.Count);
            var _newDR = DamageReductionAmount(args.NewValue.Count);
            if (_oldDR != _newDR)
            {
                if (_oldDR > 0)
                {
                    // remove old
                    MyTraits().Where(_mt => _mt.Trait is DamageReductionTrait).FirstOrDefault()?.Eject();
                }
                if (_newDR > 0)
                {
                    // add new
                    Creature.AddAdjunct(DamageReductionTrait.GetDRMagicTrait(this, _newDR, this));
                }

                if ((_oldDR >= 0) && (_newDR == 0))
                {
                    // went from on to off
                    MyTraits().Where(_mt => _mt.Trait is MagicNaturalWeaponsTrait).FirstOrDefault()?.Eject();
                }
                else if ((_oldDR == 0) && (_newDR >= 0))
                {
                    // went from off to on
                    AugmentNaturalWeapons();
                }
            }
        }
        #endregion

        public IVolatileValue ClassPowerLevel => _PowerLevel;
        public Guid OwnerID => Creature?.ID ?? Guid.Empty;
        public string Key => GetType().FullName;
        public bool IsPowerClassActive { get => true; set { } }

        public PowerClassInfo ToPowerClassInfo()
            => new PowerClassInfo
            {
                OwnerID = OwnerID.ToString(),
                ID = ID,
                Key = Key,
                ClassPowerLevel = _PowerLevel.ToDeltableInfo(),
                IsPowerClassActive = IsPowerClassActive,
                Icon = new ImageryInfo { Keys = ClassIconKey.ToEnumerable().ToArray() }
            };

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
            => _PowerLevel.QualifiedDeltas(qualify);

        private TerminateController _TCtrl;
        private TerminateController _Term
            => _TCtrl ??= new TerminateController(this);

        public void DoTerminate()
            => _Term.DoTerminate();

        public void AddTerminateDependent(IDependOnTerminate subscriber)
            => _Term.AddTerminateDependent(subscriber);

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
            => _Term.RemoveTerminateDependent(subscriber);

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;
    }
}
