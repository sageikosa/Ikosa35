using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    /// <summary>
    /// Typically used to trigger when locators move into, through, or out of the same space.
    /// Shouldn't provide feedback as this may prevent other handlers from being aware of movement.
    /// </summary>
    [Serializable]
    public class LocatorMove : InteractData
    {
        /// <summary>
        /// Typically used to trigger when locators move into, through, or out of the same space.
        /// Shouldn't provide feedback as this may prevent other handlers from being aware of movement.
        /// </summary>
        public LocatorMove(CoreActor actor, Locator locator, LocatorMoveState locatorMoveState)
            : base(actor)
        {
            _Locator = locator;
            _State = locatorMoveState;
        }

        #region data
        private Locator _Locator;
        private LocatorMoveState _State;
        #endregion

        public Locator Locator => _Locator;
        public LocatorMoveState LocatorMoveState => _State;
    }

    [Serializable]
    public enum LocatorMoveState
    {
        /// <summary>Object receiving notice is being removed from map contxt, and the specified locator is also in the region</summary>
        DepartingFrom,

        /// <summary>Object receiving notice is in a region when the specified locator is removed from map context</summary>
        TargetDeparture,

        /// <summary>Object receiving notice is static, and the specified locator is actively moving</summary>
        TargetPassedBy,

        /// <summary>Object receiving notice is actively moving, and the specified locator appears to move due to relative motion</summary>
        PassingBy,

        /// <summary>Object receiving notice in a region when the specified locator is added to map context</summary>
        TargetArrival,

        /// <summary>Object receiving notice is being added to map context, and the specified locator is already in the region</summary>
        ArrivingTo,
    }
}
