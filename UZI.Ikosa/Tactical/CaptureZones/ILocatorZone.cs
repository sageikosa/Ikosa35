using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public interface ILocatorZone : IControlChange<Activation>
    {
        /// <summary>
        /// When the zone is starting, every locator within is passed through this method
        /// </summary>
        /// <param name="locator"></param>
        void Start(Locator locator);
        /// <summary>
        /// When the zone is shutting down, every locator is passed through this method
        /// </summary>
        /// <param name="locator"></param>
        void End(Locator locator);
        /// <summary>
        /// Any locator that enters the zone after it is setup passes through this method
        /// </summary>
        /// <param name="locator"></param>
        void Enter(Locator locator);
        /// <summary>
        /// Any locator that exits the zone after it is setup passed through this method
        /// </summary>
        /// <param name="locator"></param>
        void Exit(Locator locator);
        /// <summary>
        /// If the zone geometry changes and a locator would end up in the zone, this method is called
        /// </summary>
        /// <param name="locator"></param>
        void Capture(Locator locator);
        /// <summary>
        /// If the zone geometry changes and a locator would no longer be in the zone, this method is called
        /// </summary>
        /// <param name="locator"></param>
        void Release(Locator locator);
        /// <summary>
        /// Any locator moving within the area (or changing size) passes through this method.
        /// </summary>
        void MoveInArea(Locator locator, bool followOn);
        bool IsActive { get; }
    }
}
