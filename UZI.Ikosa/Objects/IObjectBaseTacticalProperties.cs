using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Objects
{
    public interface IObjectBaseTacticalProperties : IObjectBase
    {
        CoverLevel DoesSupplyCover { get; set; }
        bool DoesSupplyConcealment { get; set; }
        bool DoesSupplyTotalConcealment { get; set; }
        bool DoesBlocksLineOfEffect { get; set; }
        bool DoesBlocksLineOfDetect { get; set; }
        bool BlocksMove { get; set; }
        bool DoesHindersMove { get; set; }
        bool DoesBlocksSpread { get; set; }
        double Opacity { get; set; }
        void SetMaxStructurePoints(int maxPts);
    }
}
