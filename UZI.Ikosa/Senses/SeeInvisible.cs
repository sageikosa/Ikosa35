using System;

namespace Uzi.Ikosa.Senses
{
    public class SeeInvisible : SensoryBase
    {
        public SeeInvisible(object source, SensoryBase senseEnhancing)
            : base(source)
        {
            _SenseEnhancing = senseEnhancing;
            LowLight = _SenseEnhancing.LowLight;
        }

        private SensoryBase _SenseEnhancing;

        public override Type ExpressedType()
            => _SenseEnhancing.ExpressedType();

        public override bool IgnoresInvisibility => true;
        public override bool ForTerrain => _SenseEnhancing.ForTerrain;
        public override bool UsesLineOfEffect => _SenseEnhancing.UsesLineOfEffect;
        public override string Name => $@"See Invisible: {_SenseEnhancing.Name}";
        public override bool UsesLight => _SenseEnhancing.UsesLight;
        public override bool UsesSight => _SenseEnhancing.UsesSight;
        public override bool UsesHearing => _SenseEnhancing.UsesHearing;
        public override bool IgnoresVisualEffects => _SenseEnhancing.IgnoresVisualEffects;
        public override int Precedence => _SenseEnhancing.Precedence + 100;

        /// <summary>
        /// This must be active, and its base sense must be active
        /// </summary>
        public override bool IsActive
        {
            get => base.IsActive && _SenseEnhancing.IsActive;
            set => base.IsActive = value;
        }
    }
}