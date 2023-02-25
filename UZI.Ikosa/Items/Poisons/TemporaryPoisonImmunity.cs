using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Items
{
    [Serializable]
    public class TemporaryPoisonImmunity : Adjunct, IInteractHandler, ITrackTime, IQualifyDelta
    {
        public TemporaryPoisonImmunity(object source, Guid? sourceID, double endTime)
            : base(source)
        {
            _SourceID = sourceID;
            _EndTime = endTime;
            _Terminator = new TerminateController(this);
            _Delta = new QualifyingDelta(1000, typeof(TemporaryPoisonImmunity), @"Temporary Poison Immunity");
        }

        #region data
        private double _EndTime;
        private Guid? _SourceID;
        private IDelta _Delta;
        private readonly TerminateController _Terminator;
        #endregion

        public Guid? SourceID => _SourceID;
        public double EndTime => _EndTime;

        public double Resolution => Minute.UnitFactor;

        public override object Clone()
            => new TemporaryPoisonImmunity(Source, SourceID, EndTime);

        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            (Anchor as CoreObject)?.AddIInteractHandler(this);
            (Anchor as Creature)?.FortitudeSave.Deltas.Add(this);
        }

        protected override void OnDeactivate(object source)
        {
            DoTerminate();
            (Anchor as CoreObject)?.RemoveIInteractHandler(this);
            base.OnDeactivate(source);
        }

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
            // block attempts to add poison
            if ((workSet?.InteractData as AddAdjunctData)?.Adjunct is Poisoned _poisoned)
            {
                // exact match, or this immunity is against all poisons
                if ((SourceID ?? _poisoned.Poison.SourceID) == _poisoned.Poison.SourceID)
                {
                    // cannot add
                    workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
                }
            }
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => true;

        #endregion

        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (timeVal >= EndTime)
                Eject();
        }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // just in case a poison is in effect to which this immunity applied, boost saves
            if ((qualify?.Source is Poison _poison)
                && ((SourceID ?? _poison.SourceID) == _poison.SourceID))
                yield return _Delta;
            yield break;
        }

        #region ITerminating Members
        /// <summary>
        /// Tells all modifiable values using this modifier to release it.  Note: this does not destroy the modifier and it can be re-used.
        /// </summary>
        public void DoTerminate()
        {
            _Terminator.DoTerminate();
        }

        #region IControlTerminate Members
        public void AddTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.AddTerminateDependent(subscriber);
        }

        public void RemoveTerminateDependent(IDependOnTerminate subscriber)
        {
            _Terminator.RemoveTerminateDependent(subscriber);
        }

        public int TerminateSubscriberCount => _Terminator.TerminateSubscriberCount;
        #endregion
        #endregion
    }
}
