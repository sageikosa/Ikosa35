using System;
using System.Collections.Generic;
using Uzi.Core.Contracts;

namespace Uzi.Core
{
    public interface IActionProvider : ICore
    {
        /// <summary>Provider ID with which to present the actions.  Usually equals ID.</summary>
        Guid PresenterID { get; }

        IEnumerable<CoreAction> GetActions(CoreActionBudget budget);

        /// <summary>Provides information about the IActionProvider</summary>
        Info GetProviderInfo(CoreActionBudget budget);
    }
}
