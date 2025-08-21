using System;
using System.Linq;
using Uzi.Core;

namespace Uzi.Ikosa.Adjuncts
{
    [Serializable]
    public class PowerDescriptorsBlocker : MultiAdjunctBlocker
    {
        public PowerDescriptorsBlocker(object source, string reason, params Type[] descriptors)
            : base(source, reason, descriptors)
        {
        }

        public override object Clone()
        {
            return new PowerDescriptorsBlocker(Source, Reason, Blocked.ToArray());
        }

        public override void HandleInteraction(Interaction workSet)
        {
            if (workSet != null)
            {
                var _data = workSet.InteractData as AddAdjunctData;
                if (_data != null)
                {
                    var _magic = _data.Adjunct as MagicPowerEffect;
                    if (_magic != null)
                    {
                        var _source = _magic.MagicPowerActionSource;
                        if (_source != null)
                        {
                            var _def = _source.MagicPowerActionDef;
                            if (_def != null)
                            {
                                // compare with blocked descriptors
                                if (Blocked.Any(_b => _b.IsAssignableFrom(_def.Descriptors.GetType())))
                                {
                                    workSet.Feedback.Add(new ValueFeedback<bool>(this, false));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
