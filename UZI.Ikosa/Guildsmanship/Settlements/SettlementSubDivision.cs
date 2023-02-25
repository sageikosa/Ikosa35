using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Core.Contracts;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class SettlementSubDivision : ModuleLink<Site>
    {
        private Vector3D _Position;
        private Vector3D _Size;

        public SettlementSubDivision(Description description)
            :base(description)
        {
        }

        public Vector3D Position { get => _Position; set => _Position = value; }
        public Vector3D Size { get => _Size; set => _Size = value; }
    }
}
