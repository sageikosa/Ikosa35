using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public abstract class Presentation : IPresentation
    {
        protected Presentation()
        {
            _ExternalValues = new Dictionary<string, int>();
            _VisualEffects = new List<VisualEffectValue>();
            _BaseFace = AnchorFace.ZLow;
        }

        #region data
        private readonly Dictionary<string, int> _ExternalValues;
        private readonly List<VisualEffectValue> _VisualEffects;
        private int _Z;
        private int _Y;
        private int _X;
        private long _ZH;
        private long _YL;
        private long _XL;
        private AnchorFace _BaseFace;
        private Vector3D _From;
        private ulong _Serial;
        #endregion

        public Dictionary<string, int> ExternalValues => _ExternalValues;
        public List<VisualEffectValue> VisualEffects => _VisualEffects;
        public int Z { get => _Z; set => _Z = value; }
        public int Y { get => _Y; set => _Y = value; }
        public int X { get => _X; set => _X = value; }
        public long ZHeight { get => _ZH; set => _ZH = value; }
        public long YLength { get => _YL; set => _YL = value; }
        public long XLength { get => _XL; set => _XL = value; }
        public AnchorFace BaseFace { get => _BaseFace; set => _BaseFace = value; }

        public long GetAxialLength(Axis axis) =>
            axis == Axis.Z ? ZHeight :
            axis == Axis.Y ? YLength :
            XLength;

        public double ZExtent => _ZH;
        public double YExtent => _YL;
        public double XExtent => _XL;

        public Vector3D MoveFrom { get => _From; set => _From = value; }
        public ulong SerialState { get => _Serial; set => _Serial = value; }
        public CellPosition ToCellPosition() => new CellPosition(this);
    }
}