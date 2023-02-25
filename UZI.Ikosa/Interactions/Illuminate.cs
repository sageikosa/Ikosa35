using Uzi.Core;
using Uzi.Ikosa.Tactical;
using Uzi.Ikosa.Senses;

namespace Uzi.Ikosa.Interactions
{
    /// <summary>
    /// Used by an object to request illumination from an illumination handler.  
    /// </summary>
    public class Illuminate : InteractData
    {
        public Illuminate(CoreActor actor, ILightTarget lightTarget, IIllumination illuminator)
            : base(actor)
        {
            LightTarget = lightTarget;
            Illuminator = illuminator;
        }

        public readonly ILightTarget LightTarget;
        public readonly IIllumination Illuminator;
    }

    /// <summary>
    /// Returns the illumination effect (light-level) supplied by the light source handler
    /// </summary>
    public class IlluminateResult : InteractionFeedback
    {
        public IlluminateResult(object source, LightRange level, IIllumination lightSource)
            : base(source)
        {
            Level = level;
            LightSource = lightSource;
        }

        public readonly LightRange Level;
        public readonly IIllumination LightSource;
    }
}
