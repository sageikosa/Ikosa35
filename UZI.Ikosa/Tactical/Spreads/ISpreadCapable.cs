using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Visualize;

namespace Uzi.Ikosa.Tactical
{
    public interface ISpreadCapable : ICapability
    {
        void PostInitialize(SpreadEffect spread);

        /// <summary>True if the spread can be applied to blockers, potentially altering blocking</summary>
        bool CanAlterBlockers { get; }

        /// <summary>Test cells to exclude from the spread</summary>
        bool IsExcluded(SpreadEffect spread, ICellLocation location);

        /// <summary>Test whether the spread can leave the cell on the specified face</summary>
        bool CanSpreadLeave(SpreadEffect spread, ICellLocation toCell, AnchorFace exitingFace);

        /// <summary>Test whether the spread can enter the cell on the specified face</summary>
        bool? CanSpreadEnter(SpreadEffect spread, ICellLocation toCell, AnchorFace enteringFace);
        // NOTE: a null result means that an object is blocking the spread 
        // NOTE: the blocking object should have the spread applied to it, and then the cell be re-tested

        /// <summary>Apply spread to locators without any follow-on steps</summary>
        void ApplySpreadToBlocker(SpreadEffect spread, Locator locator);

        /// <summary>Generate steps for spread as it affects the locator</summary>
        IEnumerable<CoreStep> ApplySpread(SpreadEffect spread, Locator locator);

        // TODO: linear-[target]/spherical/cloud-[height]
        // effects on potential cells/objects/creatures
        // terrain/hindering when done...
    }
}
