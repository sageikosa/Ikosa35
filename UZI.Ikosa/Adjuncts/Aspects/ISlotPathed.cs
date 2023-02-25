using System.Collections.Generic;
using Uzi.Ikosa.Items;

namespace Uzi.Ikosa.Adjuncts
{
    /// <summary>
    /// Any Pathed adjunct that works through slots (directly or one-step indirectly) defines this
    /// </summary>
    public interface ISlotPathed
    {
        IEnumerable<ItemSlot> PathedSlots { get; }

        /// <summary>
        /// Indicates the slotted item binding the source of the pathed adjunct to a creature
        /// </summary>
        ISlottedItem SlottedConnector { get; }
    }
}
