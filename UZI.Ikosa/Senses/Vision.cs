using System;

namespace Uzi.Ikosa.Senses
{
    /// <summary>
    /// Vision could be sourced from the creature, or it could be spell-granted low-light vision
    /// </summary>
    [Serializable]
    public class Vision : SensoryBase
    {
        public Vision(bool lowLight, object source)
            : base(source)
        {
            LowLight = lowLight;
        }

        public override int Precedence 
            => LowLight ? 85 : 80;

        public override bool UsesLineOfEffect => true;
        public override bool UsesLight => true;
        public override bool UsesSight => true;

        public override string Name
            => LowLight ? @"Low-Light Vision" : @"Vision";
    }
}
