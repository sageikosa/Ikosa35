using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Core
{
    [Serializable]
    public class ColorMapAdjunct : Adjunct
    {
        #region state
        protected IDictionary<string, string> _CMap;
        #endregion

        public ColorMapAdjunct()
            : base(typeof(ColorMapAdjunct))
        {
            _CMap = new Dictionary<string, string>();
        }

        public ColorMapAdjunct(IDictionary<string,string> colorMap)
            : base(typeof(ColorMapAdjunct))
        {
            _CMap = colorMap;
        }

        public override object Clone()
            => new ColorMapAdjunct(_CMap.ToDictionary(_cm => _cm.Key, _cm => _cm.Value));

        public IDictionary<string, string> ColorMap => _CMap;
    }
}
