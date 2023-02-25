using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;
using Uzi.Visualize;

namespace Uzi.Ikosa.Senses
{
    [Serializable]
    public class SoundImpact
    {
        public SoundImpact(IGeometricRegion region, double magnitude, DeltaCalcInfo difficulty, DeltaCalcInfo checkValue)
        {
            _Region = region;
            _Mag = magnitude;
            _Difficulty = difficulty;
            _CheckValue = checkValue;
        }

        #region state
        private readonly IGeometricRegion _Region;
        private readonly double _Mag;
        private readonly DeltaCalcInfo _Difficulty;
        private readonly DeltaCalcInfo _CheckValue;
        #endregion

        public IGeometricRegion Region => _Region;
        public double RelativeMagnitude => _Mag;
        public DeltaCalcInfo Difficulty => _Difficulty;
        public DeltaCalcInfo Check => _CheckValue;
    }
}
