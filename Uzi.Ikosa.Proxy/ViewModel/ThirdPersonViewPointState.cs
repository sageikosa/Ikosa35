using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uzi.Ikosa.Proxy.ViewModel
{
    public struct ThirdPersonViewPointState
    {
        #region bit 32 vector management
        private static readonly int _Token;
        private static readonly BitVector32.Section _FOV;
        #endregion

        #region static setup
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2207:InitializeValueTypeStaticFieldsInline")]
        static ThirdPersonViewPointState()
        {
            _Token = BitVector32.CreateMask();
            _FOV = BitVector32.CreateSection(255, BitVector32.CreateSection(1));
        }
        #endregion

        public ThirdPersonViewPointState(uint value)
        {
            _Vector = new BitVector32((int)value);
            if (FieldOfView < 30)
                FieldOfView = 30;
            if (FieldOfView > 135)
                FieldOfView = 135;
        }

        private BitVector32 _Vector;

        public uint Value => (uint)_Vector.Data;

        public double FieldOfView
        {
            get => _Vector[_FOV];
            set => _Vector[_FOV] = (int)value;
        }

        public bool ShowToken
        {
            get => _Vector[_Token];
            set => _Vector[_Token] = value;
        }
    }
}
