using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Actions.Steps;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class PowerSpreadEffect<PowerActSrc> : SpreadEffect
         where PowerActSrc : IPowerActionSource
    {
        public PowerSpreadEffect(MapContext mapContext, PowerActivationStep<PowerActSrc> delivery,
            Intersection origin, CellPosition center, int cellRadius, ISpreadCapable spread, PlanarPresence planar)
            : base(mapContext, delivery.PowerUse.PowerActionSource, origin, center, cellRadius, spread, planar)
        {
            _Delivery = delivery;
        }

        #region state
        private PowerActivationStep<PowerActSrc> _Delivery;
        #endregion

        public PowerActivationStep<PowerActSrc> Delivery => _Delivery;
        public PowerActSrc PowerActionSource => (PowerActSrc)Source;
    }
}
