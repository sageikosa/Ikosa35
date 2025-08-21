using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Abilities;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Deltas;
using Uzi.Ikosa.Skills;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Raging : Adjunct, IActionFilter, ITrackTime
    {
        public Raging(object source, int physical, int save, int recoveryRounds, bool isExternal, params Adjunct[] extras)
            : base(source)
        {
            _Phys = new Delta(physical, this);
            _Will = new Delta(save, typeof(Morale));
            _AR = new Delta(-2, this);
            _Recovery = recoveryRounds;
            _External = isExternal;
            _Extras = extras;
        }

        #region data
        private Delta _Phys;
        private Delta _Will;
        private Delta _AR;
        private Adjunct[] _Extras;
        private int _Recovery;
        private bool _External;
        private double _Expiration = 0d;
        #endregion

        public Delta PhysicalDelta => _Phys;
        public Delta WillDelta => _Will;
        public Delta ArmorDelta => _AR;
        public bool IsExternal => _External;
        public IEnumerable<Adjunct> Extras => _Extras.Select(_e => _e);
        public int RecoveryRounds => _Recovery;
        public double Expiration => _Expiration;

        public double Resolution
            => Round.UnitFactor;

        public override object Clone()
            => new Raging(Source, PhysicalDelta.Value, WillDelta.Value, RecoveryRounds, IsExternal);

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor is Creature _critter)
            {
                // abilities
                _critter.Abilities.Strength.Deltas.Add(PhysicalDelta);
                _critter.Abilities.Constitution.Deltas.Add(PhysicalDelta);

                // will save
                _critter.WillSave.Deltas.Add(WillDelta);

                // armor rating
                _critter.NormalArmorRating.Deltas.Add(ArmorDelta);
                _critter.TouchArmorRating.Deltas.Add(ArmorDelta);
                _critter.IncorporealArmorRating.Deltas.Add(ArmorDelta);

                // action filtering
                _critter.Actions.Filters.Add(this, this);

                // other stuff (e.g., indomitable will while raging)
                if (_Extras?.Any() ?? false)
                {
                    foreach (var _ex in Extras)
                    {
                        _critter.AddAdjunct(_ex);
                    }
                }

                // calculate expiration based on constitution (3 + Delta)
                if (!IsExternal)
                {
                    _Expiration = ((_critter.Setting as ITacticalMap)?.CurrentTime ?? 0)
                        + _critter.Abilities.Constitution.DeltaValue + 3;
                }

                var _budget = _critter.GetLocalActionBudget();
                if (_budget != null)
                {
                    // strip away any defensive combat budget (from improved source only?)
                    foreach (var _dcb in _budget.BudgetItems
                        .Where(_bi => _bi.Value is DefensiveCombatBudget)
                        .ToList())
                    {
                        _budget.BudgetItems.Remove(_dcb.Key);
                    }
                }
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            // end all deltas
            PhysicalDelta.DoTerminate();
            WillDelta.DoTerminate();
            ArmorDelta.DoTerminate();

            // action filtering
            var _critter = Anchor as Creature;
            _critter?.Actions.Filters.Remove(this);

            // other stuff (e.g., indomitable will while raging)
            if (_Extras?.Any() ?? false)
            {
                foreach (var _ex in Extras)
                {
                    _ex.Eject();
                }
            }

            if (RecoveryRounds > 0)
            {
                // setup recovery fatigued
                var _endTime = ((_critter.Setting as ITacticalMap)?.CurrentTime ?? 0) + RecoveryRounds;
                var _fatigued = new Fatigued(typeof(Fatigued));
                var _expiry = new Expiry(_fatigued, _endTime, TimeValTransition.Entering, Round.UnitFactor);
                _critter.AddAdjunct(_expiry);
            }
            base.OnDeactivate(source);
        }
        #endregion

        // IActionFilter members
        public bool SuppressAction(object source, CoreActionBudget budget, CoreAction action)
        {
            // no spell-cast, no command-word, no spell-trigger, no spell-completion
            if (action is CastSpell)
            {
                return true;
            }

            // Balance, Escape Artist, Intimidate, and Ride can be used
            if ((source is BalanceSkill)
                || (source is EscapeArtistSkill)
                || (source is IntimidateSkill)
                || (source is RideSkill))
            {
                return false;
            }

            // cannot use Charisma-, Dexterity-, or Intelligence-based skills 
            // cannot use concentration
            if (source is SkillBase _skill
                && ((_skill.KeyAbilityMnemonic == MnemonicCode.Cha)
                || (_skill.KeyAbilityMnemonic == MnemonicCode.Dex)
                || (_skill.KeyAbilityMnemonic == MnemonicCode.Int)
                || (_skill is ConcentrationSkill)))
            {
                return true;
            }

            // suppress (improved) defensive combat choice
            if ((action is ImprovedDefensiveCombatChoice)
                || (action is DefensiveCombatChoice))
            {
                return true;
            }

            // TODO: concentration (checks and actions) blocked
            // TODO: no item creation...
            return false;
        }

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            // time's up
            if (!IsExternal && (timeVal >= _Expiration))
            {
                Eject();
            }
        }
    }
}
