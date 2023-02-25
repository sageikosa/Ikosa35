using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Actions
{

    /// <summary>GroupMemberAdjunct representing any actor grabbing the object</summary>
    [Serializable]
    public class ObjectGrabber : GroupMemberAdjunct, IProcessFeedback
    {
        /// <summary>GroupMemberAdjunct representing any actor grabbing the object</summary>
        public ObjectGrabber(ObjectGrabGroup group)
            : base(typeof(ObjectGrabGroup), group)
        {
            _Unprepared = new Condition(Condition.UnpreparedToDodge, this, @"Grabbing");
        }

        private Condition _Unprepared;

        public ObjectGrabGroup ObjectGrabGroup
            => Group as ObjectGrabGroup;

        public Creature Creature
            => Anchor as Creature;

        public override bool CanAnchor(IAdjunctable newAnchor)
            => (newAnchor is Creature) && base.CanAnchor(newAnchor);

        public override object Clone()
            => new ObjectGrabber(ObjectGrabGroup);

        protected override void OnActivate(object source)
        {
            Creature?.AddIInteractHandler(this);
            Creature?.Conditions.Add(_Unprepared);
            base.OnActivate(source);
        }

        protected override void OnDeactivate(object source)
        {
            Creature?.RemoveIInteractHandler(this);
            Creature?.Conditions.Remove(_Unprepared);
            base.OnDeactivate(source);
        }

        #region IProcessFeedback
        public void ProcessFeedback(Interaction workSet)
        {
            // successfully added this locator to the gathering set?
            if (workSet.InteractData is GetLocatorsData _gld
                && workSet.Feedback.OfType<ValueFeedback<bool>>().Any(_vfb => _vfb.Value))
            {
                // if any were retrieved, then add the object also...
                var _grabbed = ObjectGrabGroup?.ObjectGrabbed?.CoreObject;
                if (_grabbed != null)
                {
                    _gld.AddObject(_grabbed, workSet.Source as MovementBase);
                }
            }
        }

        public void HandleInteraction(Interaction workSet)
        {
            // FEEDBACK ONLY!
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(GetLocatorsData);
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
            => (interactType == typeof(GetLocatorsData))
            && (existingHandler is GetLocatorsHandler);
        #endregion

        public static double GetTotalMoveLoad(Creature critter, MovementBase movement)
            => GetLocatorsData.GetLocators(critter, movement, null, 0)
            .Where(_glr => _glr.IsExtraWeight)
            .Select(_glr => _glr.Locator.ICore)
            .OfType<ICoreObject>().Sum(_ico => _ico.Weight)
            + (critter?.ObjectLoad.Weight ?? 0d);
    }
}
