using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Uzi.Core;
using Uzi.Ikosa.Interactions;

namespace Uzi.Ikosa.Fidelity
{
    [Serializable]
    public class DriveResistance<Source> : Delta, IQualifyDelta
    {
        public DriveResistance(object source, int modifier, string name)
            : base(modifier, source, name)
        {
        }

        public IEnumerable<IDelta> QualifiedDeltas(Qualifier qualify)
        {
            // get ability controlling this attempt
            var _ability = ((qualify as Interaction)?.InteractData as DriveCreatureData)?.DriveCreatureAbility;
            if (_ability != null)
            {
                // see if battery is source by the right type of driving
                if ((_ability.MagicBattery as ISourcedObject)?.Source is Source)
                {
                    yield return this;
                }
            }
            yield break;
        }
    }
}
