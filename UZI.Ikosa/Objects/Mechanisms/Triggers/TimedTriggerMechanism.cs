using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Uzi.Ikosa.Items.Materials;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class TimedTriggerMechanism : TriggerMechanism
    {
        public TimedTriggerMechanism(string name, Material material, int disableDifficulty,
            PostTriggerState postState)
            : base(name, material, disableDifficulty, postState)
        {
            // TODO: frequency
            // TODO: activation?
        }

        protected override string ClassIconKey => nameof(TimedTriggerMechanism);
    }
}
