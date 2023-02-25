using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class LiquidCellMaterial: CellMaterial
    {
        public LiquidCellMaterial(string name, string maxVisibility, bool allowThrown, int rangedCellDelta, bool fireSuppression, bool aquaticBreathe, LocalMap localMap)
            : base(name, localMap)
        {
            _MaxVisibility = maxVisibility;
            _AllowThrownWeapons = allowThrown;
            _RangedCellDelta = rangedCellDelta;
            _FireSuppression = fireSuppression;
            _AquaticBreathe = aquaticBreathe;
            _Swim = 10;
            DetectBlockingThickness = 10;
        }

        private string _MaxVisibility;      // 4d8x10 down-to 1d8x10 for water to murky water
        private bool _AllowThrownWeapons;   // false for water (perhaps on the interaction?)
        private int _RangedCellDelta;       // -2 for water    (perhaps on the interaction?)
        private bool _FireSuppression;      // true for water  (perhaps on the interaction?)
        private bool _AquaticBreathe;       // aquatic creatures can breathe in this
        private int _Swim;                  // 10=calm; 15=rough; 20=stormy
        // -2 on attack, half damage unless Freedom of Movement
        // swim speed with a tail is normal attack and damage
        // movement...
        // off-balance (failed swim, not on firm-footing): no Dex to AC, opponents get +2 to hit

        public override bool BlocksEffect { get { return false; } }

        public bool AquaticBreathe
        {
            get { return _AquaticBreathe; }
            set { _AquaticBreathe = value; }
        }

        public int SwimDifficulty
        {
            get { return _Swim; }
            set { _Swim = value; }
        }
    }
}
