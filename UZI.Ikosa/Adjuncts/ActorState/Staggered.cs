using System;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class Staggered : ActorStateBase, IHealthActivityLimiter
    {
        public Staggered(object source)
            : base(source)
        {
            _SingleAct = new SingleActionsOnly(this);
        }

        private SingleActionsOnly _SingleAct;

        #region Activate
        protected override void OnActivate(object source)
        {
            if (Anchor is Creature _critter)
            {
                // add condition
                _critter.Conditions.Add(new Condition(Condition.Staggered, this));

                // add action filter
                _critter.AddAdjunct(_SingleAct);

                // notify
                NotifyStateChange();
            }
            base.OnActivate(source);
        }
        #endregion

        #region DeActivate
        protected override void OnDeactivate(object source)
        {
            base.OnDeactivate(source);
            if (Anchor is Creature _critter)
            {
                // remove action filter
                _SingleAct.Eject();

                // remove condition
                _critter.Conditions.Remove(_critter.Conditions[Condition.Staggered, this]);
            }
        }
        #endregion

        public override object Clone()
        {
            return new Staggered(Source);
        }
    }
}
