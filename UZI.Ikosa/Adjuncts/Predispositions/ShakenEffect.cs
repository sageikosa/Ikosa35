using System;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>-2 on attack rolls, saves, skill checks and ability checks</summary>
    [Serializable]
    public class ShakenEffect : PredispositionBase
    {
        #region Construction
        public ShakenEffect(object source)
            : base(source)
        {
            _Fear = new Delta(-2, typeof(ShakenEffect));
        }
        #endregion

        private Delta _Fear;

        protected virtual string ConditionString => Condition.Shaken;

        protected override void OnActivate(object source)
        {
            var _critter = Anchor as Creature;

            // delta
            _critter?.MeleeDeltable.Deltas.Add(_Fear);
            _critter?.RangedDeltable.Deltas.Add(_Fear);
            _critter?.OpposedDeltable.Deltas.Add(_Fear);
            _critter?.ReflexSave.Deltas.Add(_Fear);
            _critter?.WillSave.Deltas.Add(_Fear);
            _critter?.FortitudeSave.Deltas.Add(_Fear);
            _critter?.ExtraSkillCheck.Deltas.Add(_Fear);
            _critter?.ExtraAbilityCheck.Deltas.Add(_Fear);

            // conditions
            _critter?.Conditions.Add(new Condition(ConditionString, this));
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            var _critter = Anchor as Creature;
            _critter?.Conditions.Remove(_critter.Conditions[ConditionString, this]);
            _Fear.DoTerminate();
            base.OnDeactivate(source);
        }

        public override string Description => @"Shaken";
        public override object Clone() => new ShakenEffect(Source);
    }
}