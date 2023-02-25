using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Creatures
{
    [Serializable]
    public abstract class Breathes : Adjunct, IProcessFeedback, IMonitorChange<IGeometricRegion>
    {
        protected Breathes(object source)
            : base(source)
        {
        }

        #region protected override void OnActivate(object source)
        protected override void OnActivate(object source)
        {
            base.OnActivate(source);
            if (Anchor != null)
            {
                var _loc = Anchor.GetLocated();
                if (_loc != null)
                {
                    _loc.Locator.AddChangeMonitor(this);
                    SyncBreath();
                }
                if (Anchor is CoreObject)
                {
                    (Anchor as CoreObject).AddIInteractHandler(this);
                }
            }
        }
        #endregion

        #region protected override void OnDeactivate(object source)
        protected override void OnDeactivate(object source)
        {
            if (Anchor != null)
            {
                var _loc = Anchor.GetLocated();
                if (_loc != null)
                {
                    _loc.Locator.RemoveChangeMonitor(this);
                }
                if (Anchor is CoreObject)
                {
                    (Anchor as CoreObject).RemoveIInteractHandler(this);
                }
            }
            base.OnDeactivate(source);
        }
        #endregion

        public abstract bool CanBreathe();

        #region private void SyncBreath()
        private void SyncBreath()
        {
            // when changed, check to see if need to hold breath, or recover...
            if (Anchor.Adjuncts.OfType<Breathes>().Any(_b => _b.CanBreathe()))
            {
                // if holding breathe or drowning, need to go into recovery
                if (Anchor.Adjuncts.Any(_a => _a is HoldingBreath || _a is Drowning))
                {
                    Anchor.AddAdjunct(new RecoveringBreath());
                }
            }
            else
            {
                // if not holding breath, drowning or dead, must start holding breath
                if (!Anchor.Adjuncts.Any(_a => _a is HoldingBreath || _a is Drowning || _a is DeadEffect))
                {
                    Anchor.AddAdjunct(new HoldingBreath());
                }
            }
        }
        #endregion

        #region IMonitorChange<IGeometricRegion> Members

        public void PreTestChange(object sender, AbortableChangeEventArgs<IGeometricRegion> args)
        {
        }

        public void PreValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
        }

        public void ValueChanged(object sender, ChangeValueEventArgs<IGeometricRegion> args)
        {
            SyncBreath();
        }

        #endregion

        #region IProcessFeedback Members

        public void ProcessFeedback(Interaction workSet)
        {
            if (workSet != null)
            {
                if (workSet.InteractData is AddAdjunctData)
                {
                    // hook into located.Locator
                    var _data = workSet.InteractData as AddAdjunctData;
                    if (_data.Adjunct is Located)
                    {
                        var _loc = _data.Adjunct as Located;
                        _loc.Locator.AddChangeMonitor(this);
                    }
                }
                else if (workSet.InteractData is RemoveAdjunctData)
                {
                    // de-hook from located.Locator
                    var _data = workSet.InteractData as RemoveAdjunctData;
                    if (_data.Adjunct is Located)
                    {
                        var _loc = _data.Adjunct as Located;
                        _loc.Locator.RemoveChangeMonitor(this);
                    }
                }
            }
        }

        #endregion

        #region IInteractHandler Members

        public void HandleInteraction(Interaction workSet)
        {
        }

        public IEnumerable<Type> GetInteractionTypes()
        {
            yield return typeof(AddAdjunctData);
            yield return typeof(RemoveAdjunctData);
            yield break;
        }

        public bool LinkBefore(Type interactType, IInteractHandler existingHandler)
        {
            // last feedback processor
            return true;
        }

        #endregion
    }
}
