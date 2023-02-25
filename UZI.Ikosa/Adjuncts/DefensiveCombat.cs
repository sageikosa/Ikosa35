using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Interactions;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>Only exists for a short time (max)</summary>
    [Serializable]
    public class DefensiveCombat : Adjunct, ITrackTime, IQualifyDelta
    {
        #region construction
        public DefensiveCombat(object source, double expiration, int attack, int dodge, string name) :
            base(source)
        {
            _Expires = expiration;
            _Attack = new Delta(attack, typeof(DefensiveCombat), name);
            _Dodge = new QualifyingDelta(dodge, typeof(DefensiveCombat), name);
            _Term = new TerminateController(this);
        }
        #endregion

        #region data
        private TerminateController _Term;
        private double _Expires;
        private Delta _Attack;
        private IDelta _Dodge;
        #endregion

        public Creature Creature => Anchor as Creature;
        public Delta Attack => _Attack;
        public IDelta Dodge => _Dodge;
        public double Expiration => _Expires;
        public override bool IsProtected => true;

        public override object Clone()
            => new DefensiveCombat(Source, Expiration, _Attack.Value, _Dodge.Value, _Attack.Name);

        protected override void OnActivate(object source)
        {
            // attack delta
            Creature.MeleeDeltable.Deltas.Add(_Attack);
            Creature.RangedDeltable.Deltas.Add(_Attack);

            // qualified dodge delta
            Creature.NormalArmorRating.Deltas.Add(this);
            Creature.IncorporealArmorRating.Deltas.Add(this);
            Creature.TouchArmorRating.Deltas.Add(this);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            DoTerminate();
            base.OnDeactivate(source);
        }

        #region ITrackTime Members

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if ((timeVal >= Expiration) && (direction == TimeValTransition.Entering))
            {
                IsActive = false;
            }
        }

        public double Resolution
            => Round.UnitFactor;

        #endregion

        #region IQualifyDelta Members

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            var _iAct = qualify as Interaction;
            if (_iAct?.InteractData is AttackData)
            {
                if (Creature.CanDodge(_iAct))
                {
                    yield return Dodge;
                }
            }
            yield break;
        }

        #endregion

        #region IControlTerminate Members

        public void DoTerminate()
        {
            // terminate attack
            if (_Attack != null)
                _Attack.DoTerminate();

            // terminate defense
            _Term.DoTerminate();
        }

        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Term.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Term.TerminateSubscriberCount;

        #endregion
    }
}