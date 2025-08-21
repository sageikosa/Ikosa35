using System;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Core;
using System.Collections.ObjectModel;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PowerBurstCapture<PowerActSrc> : BurstCapture
        where PowerActSrc : IPowerActionSource
    {
        public PowerBurstCapture(MapContext mapContext, PowerActivationStep<PowerActSrc> activation, Geometry geometry,
            Intersection origin, IBurstCaptureCapable burstBack, PlanarPresence planar)
            : base(mapContext, activation.PowerUse.PowerActionSource, geometry, origin, burstBack, planar)
        {
            _Activation = activation;
            _Context = [];
        }

        #region state
        private PowerActivationStep<PowerActSrc> _Activation;
        private Collection<AimTarget> _Context;
        #endregion

        public PowerActivationStep<PowerActSrc> Activation => _Activation;
        public Collection<AimTarget> Context => _Context;
        public PowerActSrc PowerActionSource => (PowerActSrc)Source;
    }
}