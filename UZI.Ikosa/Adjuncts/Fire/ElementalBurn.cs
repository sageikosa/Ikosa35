using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Magic;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Hit by a creature (such as an elemental) that may activate ongoing burn damage.</summary>
    [Serializable]
    public class ElementalBurn : Adjunct, ITrackTime, IActionProvider
    {
        /// <summary>Hit by a creature (such as an elemental) that may activate ongoing burn damage.</summary>
        public ElementalBurn(object source, int difficulty, Roller damageRoller, double endTime, int powerLevel)
            : base(source)
        {
            _Difficulty = difficulty;
            _DamageRoller = damageRoller;
            _EndTime = endTime;
            _PowerLevel = powerLevel;
        }

        #region state
        private int _Difficulty;
        private Roller _DamageRoller;
        private double _EndTime;
        private int _PowerLevel;
        private ContinuousDamageSource _DamageSource;
        #endregion

        public int Difficulty => _Difficulty;
        public Roller DamageRoller => _DamageRoller;
        public double EndTime => _EndTime;
        public int PowerLevel => _PowerLevel;

        public override object Clone()
            => new ElementalBurn(Source, Difficulty, DamageRoller, EndTime, PowerLevel);

        protected override void OnActivate(object source)
        {
            // setup continuous damage
            var _nextTime = ((Anchor as ICoreObject)?.GetLocated()?.Locator.Map?.CurrentTime ?? 0d) + Round.UnitFactor;
            _DamageSource = new ContinuousDamageSource(Source,
                [
                    new EnergyDamageRule(@"Fire.Damage", new DiceRange(@"Fire", @"Burn", DamageRoller), @"Burn Damage", EnergyType.Fire)
                ],
                _nextTime, EndTime, Resolution, PowerLevel);
            Anchor.AddAdjunct(_DamageSource);

            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            _DamageSource?.Eject();
            base.OnDeactivate(source);
        }

        public double Resolution => Round.UnitFactor;

        public bool IsContextMenuOnly => true;

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            // TODO: save-to-eject as full-round action
            // TODO: drop-prone (and roll) to save-to-eject as full-round action (+4)
            yield break;
        }

        public Info GetProviderInfo(CoreActionBudget budget)
        {
            // TODO: fire
            return null;
        }

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (direction == TimeValTransition.Leaving)
            {
                if (timeVal >= EndTime)
                {
                    Eject();
                }
            }
        }
    }
}
