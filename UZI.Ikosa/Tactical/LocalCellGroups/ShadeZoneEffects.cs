using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize;
using Uzi.Visualize.Contracts;

namespace Uzi.Ikosa.Tactical
{
    public class ShadeZoneEffects
    {
        public ShadeZoneEffects(Guid id, Cubic cubic, OptionalAnchorFace face, IEnumerable<VisualEffect> effects,
            IEnumerable<PanelShadingInfo> panels)
        {
            _ID = id;
            _Cube = cubic;
            _Face = face;
            _Effects = effects;
            _Panels = panels;
        }

        #region data
        private Guid _ID;
        private Cubic _Cube;
        private OptionalAnchorFace _Face;
        private IEnumerable<VisualEffect> _Effects;
        private IEnumerable<PanelShadingInfo> _Panels;
        #endregion

        public Guid ID => _ID;
        public Cubic Cube => _Cube;
        public OptionalAnchorFace Face => _Face;
        public IEnumerable<VisualEffect> Effects => _Effects;
        public IEnumerable<PanelShadingInfo> Panels => _Panels;

        public ShadingInfo ToShadingInfo()
        {
            var _info = new ShadingInfo
            {
                EffectBytes = Effects.Select(_e => (byte)_e).ToArray(),
                Face = (int)Face,
                ID = _ID,
                PanelShadings = _Panels?.ToList() ?? new List<PanelShadingInfo>()
            };
            _info.SetCubicInfo(Cube);
            return _info;
        }
    }
}
