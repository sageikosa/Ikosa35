using System.Collections.Generic;
using Uzi.Ikosa.Contracts;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public class AwarenessChoice : ViewModelBase
    {
        public AwarenessChoice(AwarenessTargeting targetBuilder)
        {
            _Builder = targetBuilder;
        }

        #region data
        private AwarenessInfo _Awareness;
        private IGeometricRegion _Region;
        private LocaleViewModel _LocaleViewModel;
        private AwarenessTargeting _Builder;
        #endregion

        public AwarenessInfo Awareness { get { return _Awareness; } set { _Awareness = value; } }
        public IGeometricRegion TargetRegion { get { return _Region; } set { _Region = value; } }
        public AwarenessTargeting Targeting => _Builder;
    }

    public class AwarenessChoiceComparer : IEqualityComparer<AwarenessChoice>
    {
        #region IEqualityComparer<AttackChoice> Members
        private bool AwarenessEquals(AwarenessChoice x, AwarenessChoice y)
        {
            return ((x.Awareness == null) && (y.Awareness == null))
                || ((x.Awareness != null) && (y.Awareness != null) && (y.Awareness.ID == x.Awareness.ID));
        }

        public bool Equals(AwarenessChoice x, AwarenessChoice y)
        {
            return AwarenessEquals(x, y);
        }

        public int GetHashCode(AwarenessChoice obj)
        {
            if (obj.Awareness != null)
            {
                return obj.Awareness.ID.GetHashCode();
            }
            return 0;
        }

        #endregion
    }
}
