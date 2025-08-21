using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Media.Media3D;
using Uzi.Visualize;

namespace Uzi.Visualize.Contracts.Tactical
{
    [DataContract(Namespace = Statics.TacticalNamespace)]
    public class ModelPresentationInfo : PresentationInfo, IModelTransformer
    {
        #region construction
        public ModelPresentationInfo()
            : base()
        {
        }

        public ModelPresentationInfo(ModelPresentation presentation, IEnumerable<Guid> presentedIDs) :
            base(presentation, presentedIDs)
        {
            ModelKey = presentation.ModelKey;
            Twist = presentation.Twist;
            Pivot = presentation.Pivot;
            Tilt = presentation.Tilt;
            TiltAxis = presentation.TiltAxis;
            TiltElevation = presentation.TiltElevation;
            CustomTransformInfos = presentation.CustomTransformInfos.ToList();

            CubeFitScale = presentation.CubeFitScale;
            IntraModelOffset = presentation.IntraModelOffset;
            ApparentScale = presentation.ApparentScale;
            IsAdjustingPosition = presentation.IsAdjustingPosition;
            IsFullOrigin = presentation.IsFullOrigin;
            Adornments = presentation.Adornments.Select(_a => _a).ToList();
        }
        #endregion

        [DataMember]
        public double ApparentScale { get; set; }
        [DataMember]
        public double Twist { get; set; }
        [DataMember]
        public double Pivot { get; set; }
        [DataMember]
        public double Tilt { get; set; }
        [DataMember]
        public Vector3D TiltAxis { get; set; }
        [DataMember]
        public double TiltElevation { get; set; }
        [DataMember]
        public string ModelKey { get; set; }
        [DataMember]
        public bool IsAdjustingPosition { get; set; }
        [DataMember]
        public bool IsFullOrigin { get; set; }
        [DataMember]
        public List<Transform3DInfo> CustomTransformInfos { get; set; }
        [DataMember]
        public Vector3D CubeFitScale { get; set; }
        [DataMember]
        public Vector3D IntraModelOffset { get; set; }
        [DataMember]
        public List<ModelAdornment> Adornments { get; set; }

        // TODO: previous values for animation

        private Transform3DGroup _Transforms = null;

        #region public Transform3DGroup CustomTransforms { get; }
        public Transform3DGroup CustomTransforms
        {
            get
            {
                if (_Transforms == null)
                {
                    _Transforms = new Transform3DGroup();
                    if (CustomTransformInfos?.Count>0)
                    {
                        foreach (var _t in CustomTransformInfos)
                        {
                            _Transforms.Children.Add(_t.ToTransform3D());
                        }
                    }
                    _Transforms.Freeze();
                }
                return _Transforms;
            }
        }
        #endregion

        #region public override Presentable GetPresentable(...)
        public override Presentable GetPresentable(IResolveIcon iconResolver, IResolveModel3D modelResolver, IEnumerable<PresentationInfo> selected,
            Point3D sourcePosition, int sensors, IZoomIcons zoomIcons)
        {
            var _effects = selected.Contains(this)
                ? VisualEffects.Select(_ve => new VisualEffectValue(_ve.Type, VisualEffect.Highlighted)).ToList()
                : VisualEffects;

            var _model = ModelGenerator.GenerateModel(modelResolver, _effects, ModelKey, ExternalValues);
            if (_model != null)
            {
                // wrap model in a transformable wrapper
                var _group = new Model3DGroup();
                _group.Children.Add(_model);
                ModelGenerator.TransformModel(_group, this, GetPoint3D(),
                    out ScaleTransform3D _scale,
                    out AxisAngleRotation3D _twist,
                    out AxisAngleRotation3D _tilt,
                    out AxisAngleRotation3D _pivot,
                    out TranslateTransform3D _loc);
                _model = _group;

                // adornments
                foreach (var _adorn in Adornments)
                {
                    _adorn.DrawAdornment(_group);
                }
            }

            return new Presentable
            {
                Model3D = _model,
                Presentations = new[] { this },
                Presenter = this,
                MoveFrom = MoveFrom,
                SerialState = SerialState
            };
        }
        #endregion
    }
}
