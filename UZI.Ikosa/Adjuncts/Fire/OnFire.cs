using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Core.Dice;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Creature on-fire due to non-magical fire exposure.
    /// </summary>
    [Serializable]
    public class OnFire : Adjunct, ITrackTime, IActionProvider
    {
        public OnFire(object source, int difficulty, Roller damageRoller)
            : base(source)
        {
            _Difficulty = difficulty;
            _DamageRoller = damageRoller;
        }

        #region state
        private int _Difficulty;
        private Roller _DamageRoller;
        private ContinuousDamageSource _DamageSource;
        #endregion

        public int Difficulty => _Difficulty;
        public Roller DamageRoller => _DamageRoller;

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
        }

        public double Resolution => Round.UnitFactor;

        public bool IsContextMenuOnly => true;

        public override object Clone()
            => new OnFire(Source, Difficulty, DamageRoller);

        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
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
            // TODO: must make reflex save: more damage (continuous) or eject; saves per item...
        }
    }
}
