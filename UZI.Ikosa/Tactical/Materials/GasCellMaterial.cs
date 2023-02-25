using System;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class GasCellMaterial : CellMaterial
    {
        public GasCellMaterial(string name, LocalMap localMap)
            : base(name, localMap)
        {
            IsInvisible = true;
            DetectBlockingThickness = 10;
        }

        private bool _AirBreathe = true;

        public bool AirBreathe
        {
            get { return _AirBreathe; }
            set { _AirBreathe = value; }
        }

        public bool IsInvisible { get; set; }

        public override bool BlocksEffect { get { return false; } }
    }

    public static class GasHelper
    {
        /// <summary>
        /// CellSpace is a uniformly filled invisible gas...
        /// </summary>
        public static bool IsInvisibleGasCell(this CellStructure self)
        {
            // uniformly filled with an invisible gas
            var _template = self.Template;
            return ((_template != null) && (_template.GetType() == typeof(CellSpace))
                && (_template.CellMaterial is GasCellMaterial)
                && (_template.CellMaterial as GasCellMaterial).IsInvisible);
        }
    }
}