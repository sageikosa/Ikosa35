using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// <para>Add to make an object ethereal.</para>
    /// <para>Multiple ethereal effects can be applied, object will remaing ethereal as long as it has an ethereal effect,</para>
    /// <para>Connected objects are also made ethereal.  However, removing an ethereal effect from an object may separate it from
    /// what it's connected to, or from conencted sub-obejcts if they are ethereal in their own right</para>
    /// </summary>
    [Serializable]
    public class EtherealEffect : Adjunct, ITrackTime
    {
        /// <summary>
        /// <para>Add to make an object ethereal.</para>
        /// <para>Multiple ethereal effects can be applied, object will remaing ethereal as long as it has an ethereal effect,</para>
        /// <para>Connected objects are also made ethereal.  However, removing an ethereal effect from an object may separate it from
        /// what it's connected to, or from conencted sub-obejcts if they are ethereal in their own right</para>
        /// </summary>
        public EtherealEffect(object source, double? endTime)
            : base(source)
        {
            _EndTime = endTime;
        }

        #region state
        private double? _EndTime;
        #endregion

        public DurableMagicEffect DurableSource => Source as DurableMagicEffect;

        public double? EndTime => _EndTime;

        /// <summary>
        /// Ensures that the object has an active EtherealState.
        /// </summary>
        protected override void OnActivate(object source)
        {
            var _state = Anchor.Adjuncts.OfType<EtherealState>().FirstOrDefault();
            if (_state == null)
            {
                _state = new EtherealState();
                Anchor.AddAdjunct(_state);
            }
            else if (!_state.IsActive)
            {
                // activate
                _state.Activation = new Activation(this, true);
            }
            base.OnActivate(source);
        }

        /// <summary>
        /// Informs the object's EtherealState to eject if not ethereal.
        /// </summary>
        protected override void OnDeactivate(object source)
        {
            // check for other ethereal effects, eject if none
            Anchor.Adjuncts.OfType<EtherealState>().FirstOrDefault()?.EjectIfNotEthereal();
            base.OnDeactivate(source);
        }

        public override object Clone()
            => DurableSource != null
            ? new EtherealEffect(typeof(DurableMagicEffect), DurableSource.ExpirationTime)
            : new EtherealEffect(Source, EndTime);

        public double Resolution => Round.UnitFactor;

        // ITrackTime
        public void TrackTime(double timeVal, TimeValTransition direction)
        {
            if (EndTime.HasValue && timeVal >= EndTime)
            {
                Eject();
            }
        }

        /// <summary>Make sure the object has ethereal continuity based upon its original source</summary>
        public static void EmbedEthereal(ICoreObject coreObj)
        {
            if (!coreObj.HasActiveAdjunct<EtherealEffect>())
            {
                // didn't have a direct ethereal adjunct, need to find parent holding the object in the ethereal
                var _parent = coreObj.GetPathed()?.GetPathParent();
                while (_parent != null)
                {
                    // is this parent explicitly tracking etherealness?
                    var _ethers = _parent.Adjuncts.OfType<EtherealEffect>().ToList();
                    if (_ethers.Any())
                    {
                        // found an explicitly ethereal parent, copy all it's ethereal effects
                        foreach (var _ether in _ethers)
                        {
                            if (_ether.DurableSource != null)
                            {
                                // go to the durable source
                                coreObj.AddAdjunct(_ether.DurableSource.Clone() as DurableMagicEffect);
                            }
                            else
                            {
                                // clone by whatever means necessary
                                coreObj.AddAdjunct(_ether.Clone() as EtherealEffect);
                            }
                        }

                        // first ancestor that succeeds ends the chain
                        break;
                    }

                    // look up
                    _parent = _parent.GetPathed()?.GetPathParent();
                }
            }
        }
    }
}
