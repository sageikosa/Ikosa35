using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Core;

namespace Uzi.Ikosa.Tactical
{
    public interface ITacticalInquiry : ICore
    {
        CoverLevel SuppliesCover(in TacticalInfo tacticalInfo);
        bool SuppliesConcealment(in TacticalInfo tacticalInfo);
        bool SuppliesTotalConcealment(in TacticalInfo tacticalInfo);
        bool BlocksLineOfEffect(in TacticalInfo tacticalInfo);
        bool BlocksLineOfDetect(in TacticalInfo tacticalInfo);

        /// <summary>Indicates whether the object blocks a spread effect</summary>
        bool BlocksSpread(AnchorFace transitFace, ICellLocation sourceCell, ICellLocation targetCell);

        /// <summary>True if the object might be able to block spread</summary>
        bool CanBlockSpread { get; }
    }

    public static class ITacticalInquiryHelper
    {
        public static IEnumerable<ITacticalInquiry> GetITacticals(params ICore[] self)
        {
            foreach (var _core in self.Where(_s => _s != null).OfType<ITacticalInquiry>())
                yield return _core;
            yield break;
        }

        public static ITacticalInquiry[] EmptyArray = new ITacticalInquiry[] { };
    }
}
