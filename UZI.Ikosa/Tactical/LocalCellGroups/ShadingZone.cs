using System;
using System.Collections.Generic;
using Uzi.Visualize;
using Uzi.Packaging;
using System.Runtime.Serialization;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ShadingZone : ICorePart, IDeserializationCallback
    {
        #region construction
        public ShadingZone(Guid id, Cubic cubic, AnchorFace face)
        {
            _ID = id;
            _Cubic = cubic;
            _Face = face;
            var _zh = Convert.ToInt32(_Cubic.ZHeight);
            var _yl = Convert.ToInt32(_Cubic.YLength);
            var _xl = Convert.ToInt32(_Cubic.XLength);
            _Shadings = GetLightShadeLevelArray(_zh, _yl, _xl);
        }
        #endregion

        #region state
        private Guid _ID;
        private Cubic _Cubic;
        private AnchorFace _Face;
        private LightShadeLevel[,,] _Levels;    // TODO: obsolete

        private LightShadeLevel[][][] _Shadings;
        #endregion

        #region private LightShadeLevel[][][] GetLightShadeLevelArray(int z, int y, int x)
        private LightShadeLevel[][][] GetLightShadeLevelArray(int z, int y, int x)
        {
            var _shades = new LightShadeLevel[z][][];
            for (var _cz = 0; _cz < z; _cz++)
            {
                _shades[_cz] = new LightShadeLevel[y][];
                for (var _cy = 0; _cy < y; _cy++)
                {
                    _shades[_cz][_cy] = new LightShadeLevel[x];
                    for (var _cx = 0; _cx < x; _cx++)
                    {
                        _shades[_cz][_cy][_cx] = LightShadeLevel.None;
                    }
                }
            }
            return _shades;
        }
        #endregion

        public void Wipe()
        {
            _Shadings = GetLightShadeLevelArray(
                Convert.ToInt32(Cube.ZHeight), Convert.ToInt32(Cube.YLength), Convert.ToInt32(Cube.XLength));
        }

        public LightShadeLevel this[ICellLocation location]
        {
            get => _Shadings[location.Z - _Cubic.Z][location.Y - _Cubic.Y][location.X - _Cubic.X];
            set => _Shadings[location.Z - _Cubic.Z][location.Y - _Cubic.Y][location.X - _Cubic.X] = value;
        }

        public Guid ID => _ID;
        public Cubic Cube => _Cubic;
        public AnchorFace Face => _Face;

        #region ICorePart Members

        public string Name
            => $@"[{Cube.Z},{Cube.Y},{Cube.X}] [{Cube.UpperZ},{Cube.UpperY},{Cube.UpperX}]";

        public IEnumerable<ICorePart> Relationships { get { yield break; } }

        public string TypeName
            => GetType().FullName;

        #endregion

        #region public void OnDeserialization(object sender)
        public void OnDeserialization(object sender)
        {
            // migrate _Levels to _Shadings
            if (_Shadings != null)
            {
                var _mz = _Levels.GetLength(0);
                var _my = _Levels.GetLength(1);
                var _mx = _Levels.GetLength(2);

                // copy from old rectangular arrays
                _Shadings = new LightShadeLevel[_mz][][];
                for (var _cz = 0; _cz < _mz; _cz++)
                {
                    _Shadings[_cz] = new LightShadeLevel[_my][];
                    for (var _cy = 0; _cy < _my; _cy++)
                    {
                        _Shadings[_cz][_cy] = new LightShadeLevel[_mx];
                        for (var _cx = 0; _cx < _mx; _cx++)
                        {
                            _Shadings[_cz][_cy][_cx] = _Levels[_cz, _cy, _cx];
                        }
                    }
                }
                _Levels = null;
            }
        }
        #endregion
    }
}
