using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Objects;

namespace Uzi.Ikosa.Interactions
{
    [Serializable]
    public class ForceOpenData : InteractData
    {
        public ForceOpenData(CoreActivity activity)
            : base(activity?.Actor)
        {
            _Activity = activity;
        }

        #region data
        private CoreActivity _Activity;
        #endregion

        public CoreActivity Activity => _Activity;

        public int GetStrengthRoll()
            => Activity.GetFirstTarget<ValueTarget<int>>(@"Strength")?.Value ?? 10;

        public override IEnumerable<IInteractHandler> GetDefaultHandlers(IInteract target)
        {
            switch (target)
            {
                case SlidingPortal _slider:
                    {
                        yield return new SlidingPortalForceOpenHandler();
                    }
                    break;

                case CornerPivotPortal _corner:
                    {
                        yield return new CornerPivotPortalForceOpenHandler();
                    }
                    break;

                default:
                    break;
            }
            yield break;
        }
    }
}
