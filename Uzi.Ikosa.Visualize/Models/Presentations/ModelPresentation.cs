using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize;
using System.Windows;
using System.Windows.Media.Media3D;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Visualize
{
    public class ModelPresentation : Presentation, IModelTransformer
    {
        #region ctor()
        public ModelPresentation()
        {
            _TransformInfos = new List<Transform3DInfo>();
            _Twist = 0d;
            _Pivot = 0d;
            _Tilt = 0d;
            _TiltAxis = new Vector3D(1, 0, 0);
            _TiltElevation = 0d;
            _Key = null;
            _Origin = false;
            _Adornments = new List<ModelAdornment>();
        }
        #endregion

        #region data
        private List<Transform3DInfo> _TransformInfos;
        private string _Key;
        private double _Twist;
        private double _Pivot;
        private double _Tilt;
        private Vector3D _TiltAxis;
        private double _TiltElevation;
        private Vector3D _CubeFitScale;
        private Vector3D _IntraModelOffset;
        private double _ApparentScale;
        private bool _Origin;
        private List<ModelAdornment> _Adornments;
        #endregion

        public string ModelKey { get => _Key; set { _Key = value; } }
        public double Twist { get => _Twist; set { _Twist = value; } }
        public double Pivot { get => _Pivot; set { _Pivot = value; } }
        public double Tilt { get => _Tilt; set { _Tilt = value; } }
        public Vector3D TiltAxis { get => _TiltAxis; set { _TiltAxis = value; } }
        public double TiltElevation { get => _TiltElevation; set { _TiltElevation = value; } }

        public List<Transform3DInfo> CustomTransformInfos { get => _TransformInfos; set => _TransformInfos = value; }

        private Transform3DGroup _Group = null;

        #region public Transform3DGroup CustomTransforms { get; }
        public Transform3DGroup CustomTransforms
        {
            get
            {
                if (_Group == null)
                {
                    _Group = new Transform3DGroup();
                    if (CustomTransformInfos?.Count > 0)
                    {
                        foreach (var _t in CustomTransformInfos)
                        {
                            _Group.Children.Add(_t.ToTransform3D());
                        }
                    }
                    _Group.Freeze();
                }
                return _Group;
            }
        }
        #endregion

        public Vector3D CubeFitScale { get { return _CubeFitScale; } set { _CubeFitScale = value; } }
        public Vector3D IntraModelOffset { get { return _IntraModelOffset; } set { _IntraModelOffset = value; } }
        public double ApparentScale { get { return _ApparentScale; } set { _ApparentScale = value; } }
        public bool IsAdjustingPosition { get { return _TransformInfos.Any(); } }
        public bool IsFullOrigin { get { return _Origin; } set { _Origin = value; } }
        public List<ModelAdornment> Adornments => _Adornments;
    }
}