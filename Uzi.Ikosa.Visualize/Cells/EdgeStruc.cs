namespace Uzi.Visualize
{
    public struct EdgeStruc
    {
        public EdgeStruc(ICellEdge edge, AnchorFaceList faces, double pOff, double sOff)
        {
            _Edge = edge;
            _Faces = faces;
            _PrimeOff = pOff;
            _SecondOff = sOff;
        }

        #region data
        private ICellEdge _Edge;
        private AnchorFaceList _Faces;
        private double _PrimeOff;
        private double _SecondOff;
        #endregion

        public Axis Axis
        {
            get
            {
                if (!_Faces.Intersects(AnchorFaceList.ZMask))
                {
                    return Axis.Z;
                }
                else if (!_Faces.Intersects(AnchorFaceList.YMask))
                {
                    return Axis.Y;
                }

                return Axis.X;
            }
        }

        public double PrimeOff
        {
            get
            {
                switch (Axis)
                {
                    case Axis.Z:
                        return (_Faces.Intersects(AnchorFaceList.XHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _PrimeOff);
                    case Axis.Y:
                        return (_Faces.Intersects(AnchorFaceList.ZHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _PrimeOff);
                    case Axis.X:
                    default:
                        return (_Faces.Intersects(AnchorFaceList.YHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _PrimeOff);
                }
            }
        }

        public double SecondOff
        {
            get
            {
                switch (Axis)
                {
                    case Axis.Z:
                        return (_Faces.Intersects(AnchorFaceList.YHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _SecondOff);
                    case Axis.Y:
                        return (_Faces.Intersects(AnchorFaceList.XHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _SecondOff);
                    case Axis.X:
                    default:
                        return (_Faces.Intersects(AnchorFaceList.ZHigh) ? -1d : 1d) * ((_Edge?.Width ?? 0) + _SecondOff);
                }
            }
        }

        public BuildableMaterial GetOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect)
            => _Edge?.GetOrthoFaceMaterial(axis, isPlusFace, effect) ?? new BuildableMaterial();

        public BuildableMaterial GetOtherFaceMaterial(int index, VisualEffect effect)
            => _Edge?.GetOtherFaceMaterial(index, effect) ?? new BuildableMaterial();

        public BuildableMaterial GetPlusOrthoFaceMaterial(Axis axis, bool isPlusFace, VisualEffect effect)
            => new BuildableMaterial();

        public BuildableMaterial GetPlusOtherFaceMaterial(int index, VisualEffect effect)
            => new BuildableMaterial();
    }
}
