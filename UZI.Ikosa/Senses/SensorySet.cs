using System;
using System.Collections.Generic;
using Uzi.Core;
using System.Collections.ObjectModel;
using System.Linq;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Senses
{
    // TODO: controllable set
    [Serializable]
    public class SensorySet : ICreatureBind
    {
        public SensorySet()
        {
            _Senses = [];
        }

        #region state
        private Collection<SensoryBase> _Senses;
        private Delta _BlindPenalty;
        private Delta _DeafPenalty;
        #endregion

        public SensoryBase BestAvailable(Type sType)
            => AllSenses
            .Where(_s => _s.IsActive && _s.GetType().Equals(sType))
            .OrderByDescending(_s => _s.Range)
            .FirstOrDefault();

        public IEnumerable<Type> SenseTypes
            => AllSenses.Select(_s => _s.GetType()).Distinct();

        /// <summary>Returns the best active senses versions of each sense in descending precedence order</summary>
        public IEnumerable<SensoryBase> BestSenses
            => (from _sType in SenseTypes
                let _sense = BestAvailable(_sType)
                where _sense != null
                orderby _sense.Precedence descending
                select _sense);

        /// <summary>Returns the best active senses versions of each terrain sense in descending precedence order</summary>
        public IEnumerable<SensoryBase> BestTerrainSenses
            => BestSenses.Where(_s => _s.ForTerrain);

        #region public PlanarPresence PlanarPresence { get; }
        /// <summary>Aggregate PlanarPresence for all senses</summary>
        public PlanarPresence PlanarPresence
        {
            get
            {
                // NOTE: ethereal vision is granted upon going ethereal
                if (AllSenses.Any(_s => _s.IsActive && _s.PlanarPresence == PlanarPresence.Both))
                {
                    return PlanarPresence.Both;
                }

                var _material = AllSenses.Any(_s => _s.IsActive && _s.PlanarPresence.HasMaterialPresence());
                var _ethereal = AllSenses.Any(_s => _s.IsActive && _s.PlanarPresence.HasEtherealPresence());
                if (_material && _ethereal)
                {
                    return PlanarPresence.Both;
                }

                if (_material)
                {
                    return PlanarPresence.Material;
                }

                if (_ethereal)
                {
                    return PlanarPresence.Ethereal;
                }

                return PlanarPresence.None;
            }
        }
        #endregion

        #region public Collection<SensoryBase> CollectBestSenses()
        public Collection<SensoryBase> CollectBestSenses()
        {
            return new Collection<SensoryBase>(BestSenses.ToList());
        }
        #endregion

        public IEnumerable<SensoryBase> AllSenses
            => _Senses.Select(_s => _s);

        public Creature Creature { get; private set; }

        #region ICreatureBind
        /// <summary>Bind set to a creature, binding all sense as well</summary>
        public bool BindTo(Creature creature)
        {
            if (Creature == null)
            {
                // bind
                Creature = creature;
                foreach (var _sense in _Senses)
                {
                    // bind senses
                    _sense.BindTo(Creature);
                }

                // apply
                ApplySenses();
                return true;
            }
            return false;
        }

        /// <summary>Bind set from creature, unbinding all sense as well</summary>
        public void UnbindFromCreature()
        {
            foreach (var _sense in _Senses)
            {
                // unbind senses
                _sense.UnbindFromCreature();
            }

            // apply
            ApplySenses();

            // unbind
            Creature = null;
        }
        #endregion

        #region Add(SensoryBase sense)
        public void Add(SensoryBase sense)
        {
            // only bind unbound senses
            if (sense.Creature == null)
            {
                if (!_Senses.Contains(sense))
                {
                    // track it, even if can't be bound at this time
                    _Senses.Add(sense);

                    // only bind if set is connected
                    if (Creature != null)
                    {
                        // make sure bind succeeds
                        if (sense.BindTo(Creature))
                        {
                            ApplySenses();
                        }
                    }
                }
            }
        }
        #endregion

        #region Remove(SensoryBase sense)
        public void Remove(SensoryBase sense)
        {
            if (_Senses.Remove(sense))
            {
                sense.UnbindFromCreature();
                if (Creature != null)
                {
                    ApplySenses();
                }
            }
        }
        #endregion

        /// <summary>True if any permanent sense uses sight</summary>
        public bool ReliesOnSight
            => _Senses.Any(_s => _s.UsesSight && !(_s.Source is Adjunct));

        public bool CanViewTransientVisualizations
            => ReliesOnSight && _Senses.Any(_s => _s.UsesSight && _s.IsActive);

        /// <summary>True if any permanent sense uses hearing</summary>
        public bool ReliesOnHearing
            => _Senses.Any(_s => _s.UsesHearing && !(_s.Source is Adjunct));

        #region public void ApplySenses() -- blind/deaf
        /// <summary>Called to apply blind/deaf effect from currently valid set of active senses</summary>
        public void ApplySenses()
        {
            if (ReliesOnSight)
            {
                if (_Senses.Any(_s => _s.UsesSight && _s.IsActive))
                {
                    // not-blind
                    if (_BlindPenalty != null)
                    {
                        _BlindPenalty.DoTerminate();
                        _BlindPenalty = null;
                    }
                }
                else
                {
                    // blind
                    if (_BlindPenalty == null)
                    {
                        _BlindPenalty = new Delta(-4, typeof(SensorySet));
                        Creature.Skills.Skill<SearchSkill>().Deltas.Add(_BlindPenalty);
                        foreach (var _skill in from _s in Creature.Skills
                                               where _s.KeyAbilityMnemonic.Equals(MnemonicCode.Str) || _s.KeyAbilityMnemonic.Equals(MnemonicCode.Dex)
                                               select _s)
                        {
                            _skill.Deltas.Add(_BlindPenalty);
                        }
                        // TODO: fail spot checks! fail decipher script!
                    }
                }
            }
            else
            {
                // cannot be blind
                if (_BlindPenalty != null)
                {
                    _BlindPenalty.DoTerminate();
                    _BlindPenalty = null;
                }
            }

            if (ReliesOnHearing)
            {
                if (_Senses.Any(_s => _s.UsesHearing && _s.IsActive))
                {
                    // not-deaf
                    if (_DeafPenalty != null)
                    {
                        _DeafPenalty.DoTerminate();
                        _DeafPenalty = null;
                    }
                }
                else
                {
                    // deaf
                    if (_DeafPenalty == null)
                    {
                        _DeafPenalty = new Delta(-4, typeof(SensorySet));
                        Creature.Initiative.Deltas.Add(_DeafPenalty);
                        // TODO: add other penalties... (fail listen checks, 20% magic failure with chanting components)
                    }
                }
            }
            else
            {
                // cannot be deaf
                if (_DeafPenalty != null)
                {
                    _DeafPenalty.DoTerminate();
                    _DeafPenalty = null;
                }
            }
        }
        #endregion
    }
}
