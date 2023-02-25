using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Languages;
using Uzi.Ikosa.Advancement;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Creatures.BodyType;
using Uzi.Ikosa.Feats;
using Uzi.Ikosa.Senses;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Interactions.Action;
using Uzi.Ikosa.Skills;

namespace Uzi.Ikosa.Creatures.Types
{
    [Serializable]
    public abstract class Species : ICreatureBind, ILinkOwner<LinkableDock<Species>>, ITraitSource
    {
        protected Species()
        {
            _Dock = new BiDiPtr<SpeciesDock, Species>(this);
        }

        #region data
        protected BaseMonsterClass _MonsterClass = null;
        protected BiDiPtr<SpeciesDock, Species> _Dock;
        #endregion

        #region protected void UnslotAllItems(Creature source)
        protected void UnslotAllItems(Creature source)
        {
            if (source != null)
            {
                // equipment list to drop
                var _items = (from _slot in source.Body.ItemSlots.AllSlots
                              where (_slot.SlottedItem != null)
                              select _slot.SlottedItem).Distinct()
                              .Select(_si => _si)
                              .Distinct().ToList();
                foreach (var _item in _items)
                {
                    _item.ClearSlots();
                    if (_item.IsTransferrable)
                    {
                        Drop.DoDrop(source, _item, this, true);
                    }
                }
            }
        }
        #endregion

        #region protected void TransferItems(Creature source, Creature target)
        protected void TransferItems(Creature source, Creature target)
        {
            if (source != null)
            {
                // equipment list to transfer
                var _slotted = (from _slot in source.Body.ItemSlots.AllSlots
                                where (_slot.SlottedItem != null) && _slot.SlottedItem.IsTransferrable
                                select _slot.SlottedItem).Distinct()
                                .Select(_si => new
                                {
                                    Item = _si,
                                    Slots = _si.AllSlots.Select(_s => new
                                    {
                                        _s.SlotType,
                                        _s.SubType
                                    }).ToList()
                                })
                                .Distinct().ToList();

                // slot equipment into new creature
                foreach (var _slotInfo in _slotted)
                {
                    var _success = false;
                    if (target != null)
                    {
                        switch (_slotInfo.Slots.Count)
                        {
                            case 1:
                                {
                                    var _newSlot = target.Body.ItemSlots[_slotInfo.Slots[0].SlotType, _slotInfo.Slots[0].SubType];
                                    if (_newSlot != null)
                                    {
                                        // clear, swap, slot
                                        _slotInfo.Item.ClearSlots();
                                        if (_slotInfo.Item.MainSlot == null)
                                        {
                                            _slotInfo.Item.Possessor = target;
                                            _slotInfo.Item.SetItemSlot(_newSlot);
                                            _success = _newSlot.SlottedItem.BaseObject == _slotInfo.Item;
                                        }
                                    }
                                }
                                break;

                            case 2:
                                {
                                    var _newSlotA = target.Body.ItemSlots[_slotInfo.Slots[0].SlotType, _slotInfo.Slots[0].SubType];
                                    var _newSlotB = target.Body.ItemSlots[_slotInfo.Slots[1].SlotType, _slotInfo.Slots[1].SubType];
                                    if ((_newSlotA != null) && (_newSlotB != null))
                                    {
                                        // clear, swap, slot
                                        _slotInfo.Item.ClearSlots();
                                        if (_slotInfo.Item.MainSlot == null)
                                        {
                                            _slotInfo.Item.Possessor = target;
                                            _slotInfo.Item.SetItemSlot(_newSlotA, _newSlotB);
                                            _success = _newSlotA.SlottedItem.BaseObject == _slotInfo.Item;
                                        }
                                    }
                                }
                                break;
                        }
                    }

                    // if item cannot be slotted, drop where the original is located...
                    if (!_success)
                    {
                        Drop.DoDrop(source, _slotInfo.Item, this, true);
                    }
                }
            }
        }
        #endregion

        #region protected IEnumerable<Language> GenerateLanguageCopies(Creature source)
        protected IEnumerable<Language> GenerateLanguageCopies(Creature source)
        {
            if (source != null)
                foreach (var _lang in source.Languages)
                {
                    if (_lang.Source is Species)
                    {
                        yield return _lang.GetCopy(this);
                    }
                    else if (_lang.Source is Intelligence)
                    {
                        yield return _lang.GetCopy(Creature.Abilities.Intelligence);
                    }
                    else if (_lang.Source is LanguageSkill)
                    {
                        yield return _lang.GetCopy(Creature.Skills.Skill<LanguageSkill>());
                    }
                }
            yield break;
        }
        #endregion

        public virtual bool CanAdd(Creature testCreature)
            => true;

        public Creature Creature
            => _Dock.LinkDock.Creature;

        /// <summary>Default ability scores defined for the species</summary>
        public virtual AbilitySet DefaultAbilities()
            => new AbilitySet(10, 10, 10, 10, 10, 10);

        #region protected virtual void OnConnectSpecies()
        protected virtual void OnConnectSpecies()
        {
            // critter type
            var _critterType = GenerateCreatureType();
            _critterType.BindTo(Creature);
            foreach (var _sub in GenerateSubTypes())
                Creature.SubTypes.Add(_sub);

            // body
            var _body = GenerateBody();
            if (_body != null)
            {
                // movements
                foreach (var _move in GenerateMovements())
                    _body.Movements.Add(_move);

                // features
                foreach (var _feature in GenerateBodyFeatures())
                    _body.Features.Add(_feature);

                // armor and finishing
                _body.NaturalArmor.BaseValue = GenerateNaturalArmor();
                _body.BindTo(Creature);
            }

            // monster class bind before traits that may need base monster class...
            _MonsterClass = GenerateBaseMonsterClass();
            _MonsterClass?.BindTo(Creature);

            // traits
            var _hasNatural = false;
            foreach (var _trait in GenerateTraits())
            {
                Creature.AddAdjunct(_trait);
                _hasNatural |= (_trait.Trait is NaturalWeaponTrait);
            }

            if (_hasNatural)
            {
                // snapshot
                Creature.HeldItemsGroups.Snapshot(@"Natural");
            }

            // languages
            foreach (var _lang in GenerateAutomaticLanguages())
                Creature.Languages.Add(_lang);

            // senses
            foreach (var _sense in GenerateSenses())
                Creature.Senses.Add(_sense);

            // skill boosts
            foreach (var _skillBoost in GenerateSkillDeltas())
            {
                Creature.Skills[_skillBoost.Key].Deltas.Add(_skillBoost.Value);
            }

            // monster class power dice increases?
            if (_MonsterClass != null)
            {
                var _level = 0;
                foreach (var _incMethod in GeneratePowerDice())
                {
                    // increase level
                    if (_MonsterClass.CanIncreaseLevel())
                    {
                        _level++;
                        _MonsterClass.IncreaseLevel(_incMethod);

                        // configure power die
                        var _powerDie = Creature.AdvancementLog[_level];
                        if (_powerDie != null)
                        {
                            _powerDie.Feat = GenerateAdvancementFeat(_powerDie);
                            _powerDie.AbilityBoostMnemonic = GenerateAbilityBoostMnemonic(_level);
                        }
                    }
                }

                // fractional?
                if ((_level == 1) && GenerateSingleFractionalPowerDie())
                {
                    Creature.AdvancementLog[1].IsFractional = true;
                }

                // scatter skill points
                GenerateSkillPoints(1, _level);
                Creature.AdvancementLog.LockUpTo(_level);
            }
        }
        #endregion

        #region protected virtual void OnDisconnectSpecies()
        protected virtual void OnDisconnectSpecies()
        {
            // TODO: rollback monster class levels (if possible) (advancement classes derived from BaseMonsterClass...)

            // remove traits
            foreach (var _trait in (from _t in Creature.Traits
                                    where _t.Source == this
                                    select _t).ToList())
                _trait.Eject();

            // senses
            foreach (var _sense in Creature.Senses.AllSenses.Where(_s => _s.Source == this).ToList())
                Creature.Senses.Remove(_sense);

            // languages
            foreach (var _lang in Creature.Languages.Where(_l => _l.Source == this).ToList())
                Creature.Languages.Remove(_lang);

            // remove abilities
            foreach (var _delta in (from _ability in Creature.Abilities.AllAbilities
                                    from _d in _ability.Deltas
                                    where _d.Source == this
                                    select _d).ToList())
            {
                _delta.DoTerminate();
            }

            // remove skills
            foreach (var _delta in (from _skill in Creature.Skills
                                    from _d in _skill.Deltas
                                    where _d.Source == this
                                    select _d).ToList())
            {
                _delta.DoTerminate();
            }

            // remove body (will also take care of movements, natural weapons and features!)
            Body _noBody = new NoBody(Creature.Body.Sizer.Size, 1);
            _noBody.BindTo(Creature);
            Creature.HeldItemsGroups.Remove(@"Natural");

            // subtypes
            foreach (var _sub in Creature.SubTypes.Where(_s => _s.Source == this).ToList())
                Creature.SubTypes.Remove(_sub);

            // critter type
            Creature.CreatureType.UnbindFromCreature();
        }
        #endregion

        public virtual IEnumerable<AdvancementRequirement> Requirements(int powerDieLevel) { yield break; }
        public virtual IEnumerable<Feature> Features(int level) { yield break; }

        protected abstract CreatureType GenerateCreatureType();
        protected abstract IEnumerable<CreatureSubType> GenerateSubTypes();
        protected abstract Body GenerateBody();
        protected abstract IEnumerable<MovementBase> GenerateMovements();
        protected abstract IEnumerable<BodyFeature> GenerateBodyFeatures();
        protected abstract int GenerateNaturalArmor();
        protected abstract IEnumerable<TraitBase> GenerateTraits();
        protected abstract IEnumerable<Language> GenerateAutomaticLanguages();
        protected abstract IEnumerable<SensoryBase> GenerateSenses();
        protected abstract IEnumerable<KeyValuePair<Type, Delta>> GenerateSkillDeltas();
        protected abstract BaseMonsterClass GenerateBaseMonsterClass();

        /// <summary>
        /// Override to true if the creature is expected to have a single fractional power-die
        /// </summary>
        /// <returns></returns>
        protected virtual bool GenerateSingleFractionalPowerDie() => false;

        protected abstract IEnumerable<PowerDieCalcMethod> GeneratePowerDice();
        protected abstract FeatBase GenerateAdvancementFeat(PowerDie powerDie);
        protected abstract void GenerateSkillPoints(int minLevel, int maxLevel);
        protected abstract string GenerateAbilityBoostMnemonic(int powerDieLevel);

        public virtual string Name => GetType().Name;
        public virtual IEnumerable<Language> CommonLanguages() { yield break; }
        public virtual Type FavoredClass() => null;

        /// <summary>True if the species is suitable to advance by character classes</summary>
        public abstract bool IsCharacterCapable { get; }

        /// <summary>Some creatures do not truly advance, so do not get ability boosts.  Most do, however</summary>
        public virtual bool SupportsAbilityBoosts => true;

        /// <summary>A MonsterClass may support a fractional PowerDie.</summary>
        public virtual decimal FractionalPowerDie => 1m;

        public virtual decimal SmallestPowerDie => FractionalPowerDie;

        /// <summary>Undoes requirements</summary>
        public virtual void Rollback() { }

        #region ICreatureBind Members
        public bool BindTo(Creature creature)
        {
            if (!_Dock.WillAbortChange(creature.SpeciesDock))
            {
                _Dock.LinkDock = creature.SpeciesDock;
                return true;
            }
            return false;
        }

        public void UnbindFromCreature()
        {
            if (!_Dock.WillAbortChange(null))
            {
                _Dock.LinkDock = null;
            }
        }
        #endregion

        #region ILinkOwner<LinkableDock<Species>> Members

        public void LinkDropped(LinkableDock<Species> changer)
        {
            if (changer is SpeciesDock _dock)
            {
                // this is our dock, but it no longer points to us...
                if ((_Dock.LinkDock == _dock) && (_dock.Species != this))
                {
                    OnDisconnectSpecies();
                    _Dock.LinkDock = null;
                }
            }
        }

        public void LinkAdded(LinkableDock<Species> changer)
        {
            OnConnectSpecies();
        }

        #endregion

        public abstract Species TemplateClone(Creature creature);
    }
}
