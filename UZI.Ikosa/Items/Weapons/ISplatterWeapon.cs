using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Contracts;

namespace Uzi.Ikosa.Items
{
    public interface ISplatterWeapon : ICoreItem, IRangedSource, IActionSource
    {
        IEnumerable<StepPrerequisite> GetDirectPrerequisites(CoreActivity activity);
        void ApplyDirect(ApplySplatterStep step);

        IEnumerable<StepPrerequisite> GetSplatterPrerequisites(CoreActivity activity);
        void ApplySplatter(ApplySplatterStep step);

        /// <summary>Unslot, drop load and depossess</summary>
        void DoneUseItem();

        Lethality Lethality { get; }
    }
}
