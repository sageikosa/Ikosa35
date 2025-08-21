using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using Uzi.Ikosa.Adjuncts;
using Uzi.Core;
using Uzi.Ikosa.Senses;
using Uzi.Visualize;
using Uzi.Ikosa.Objects;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Tactical
{
    [Serializable]
    public class ObjectPresenter : Locator
    {
        #region Constructor
        public ObjectPresenter(ICoreObject entity, MapContext context, string modelKey, IGeometricSize normalSize, IGeometricRegion region) :
            base(entity, context, normalSize, region, false)
        {
            _Twist = 0d;
            _Pivot = 0d;
            _Tilt = 0d;
            _ModelKey = modelKey;
            context.Add(this);
        }

        public ObjectPresenter(ICoreObject entity, MapContext context, IGeometricSize normalSize, IGeometricRegion region,
            IResolveModel3D resolver) :
            base(entity, context, normalSize, region, false)
        {
            _Twist = 0d;
            _Pivot = 0d;
            _Tilt = 0d;
            _ModelKey = GetModelKey(entity, resolver);
            context.Add(this);
        }
        #endregion

        #region private data
        private double _Twist;
        private double _Pivot;
        private double _Tilt;
        private string _ModelKey;
        #endregion

        #region [NonSerialized, JsonIgnore] private data

        // TODO: perhaps this internal model manipulationness should be dropped, as it does not jibe with the client anyway
        // TODO: ... and conflicts with multiple presentations that have different "layers"

        /// <summary>This is the loaded model</summary>
        [NonSerialized, JsonIgnore]
        private Model3D _DefinedModel = null;

        /// <summary>
        /// This is the model presented to the environment rendering system.  
        /// It includes translation, scaling and rotation appropriate for the entity state.
        /// </summary>
        [NonSerialized, JsonIgnore]
        private Model3DGroup _ExpressedModel = null;

        #region Internal Model Manipulations
        // while included in the expressed model, these are tracked here for easy reference
        [NonSerialized, JsonIgnore]
        private TranslateTransform3D _ExpressedLocation;
        [NonSerialized, JsonIgnore]
        private AxisAngleRotation3D _TwistAxis;
        [NonSerialized, JsonIgnore]
        private AxisAngleRotation3D _PivotAxis;
        [NonSerialized, JsonIgnore]
        private AxisAngleRotation3D _TiltAxis;
        [NonSerialized, JsonIgnore]
        private ScaleTransform3D _ExpressedScale;
        #endregion

        #endregion

        #region public double Twist { get; set; }
        public double Twist
        {
            get { return _Twist; }
            set
            {
                _Twist = value;
                if (_TwistAxis != null)
                {
                    _TwistAxis.Angle = _Twist;
                }
            }
        }
        #endregion

        #region public double Pivot { get; set; }
        public double Pivot
        {
            get { return _Pivot; }
            set
            {
                _Pivot = value;
                if (_PivotAxis != null)
                {
                    _PivotAxis.Angle = _Pivot;
                }
            }
        }
        #endregion

        #region public double Tilt { get; set; }
        /// <summary>
        /// Tilt around +X axis vector (Size, Tilt, Pivot, Custom, Position)
        /// </summary>
        public double Tilt
        {
            get { return _Tilt; }
            set
            {
                _Tilt = value;
                if (_TiltAxis != null)
                {
                    _TiltAxis.Angle = _Tilt;
                }
            }
        }
        #endregion

        #region public string ModelKey { get; set; }
        public string ModelKey
        {
            get { return _ModelKey; }
            set
            {
                // set new value and merge into the expressed model
                _ModelKey = value;
            }
        }
        #endregion

        /// <summary>This should be set to point to the local-map location for model files</summary>
        public IResolveModel3D ModelResolver
            => Map?.Resources;

        /// <summary>This should be set to point to the local-map location for model files</summary>
        public IResolveIcon IconResolver
            => Map?.Resources;

        #region public Presentable GetPresentable(IGeometricRegion location, Creature creature, IZoomIcons zoomIcons, int heading, AnchorFace baseFace, IList<SensoryBase> filteredSenses, IAwarenessLevels awarenessLevels)
        /// <summary>
        /// This is the model presented to the environment rendering system.  
        /// It includes translation, scaling and rotation appropriate for the entity state.
        /// </summary>
        public virtual Presentable GetPresentable(IGeometricRegion observerLocation, Creature creature, ISensorHost sensors,
            IZoomIcons zoomIcons, int heading, AnchorFace baseFace, IList<SensoryBase> filteredSenses)
        {
            // wrap in a group (to preserve any transforms)
            if (_ExpressedModel == null)
            {
                _ExpressedModel = new Model3DGroup();
            }
            else
            {
                _ExpressedModel.Children.Clear();
            }

            var _pt = GeometricRegion.GetPoint3D();
            var _presentations = GetPresentations(observerLocation, creature, sensors, filteredSenses).ToList();
            foreach (var _present in _presentations)
            {
                if (_present is ModelPresentation)
                {
                    // model presentation
                    var _mdlPresent = _present as ModelPresentation;
                    _DefinedModel = ModelGenerator.GenerateModel(ModelResolver, _present.VisualEffects,
                        _mdlPresent.ModelKey, _mdlPresent.ExternalValues);
                    ModelGenerator.TransformModel(_ExpressedModel, _mdlPresent, _pt,
                        out _ExpressedScale, out _TwistAxis, out _TiltAxis, out _PivotAxis, out _ExpressedLocation);
                    _ExpressedModel.Children.Insert(0, _DefinedModel);
                }
                else if (_present is IconPresentation)
                {
                    // icon bill-board presentation
                    var _icoPresent = _present as IconPresentation;
                    _DefinedModel = ModelGenerator.GenerateMarker(IconResolver, _present.VisualEffects,
                        _icoPresent.IconRefs, observerLocation.GetPoint3D(), _pt, _icoPresent.BaseFace, heading);
                    _ExpressedModel.Children.Insert(0, _DefinedModel);
                }
            }
            return new Presentable { Presenter = this, Model3D = _ExpressedModel, Presentations = _presentations };
        }
        #endregion

        #region public override double ApparentScale { get; set; }
        /// <summary>Setting this may scale the visual representation of the model</summary>
        public override double ApparentScale
        {
            get { return base.ApparentScale; }
            set { base.ApparentScale = value; }
        }
        #endregion

        /// <summary>returns true if the model can be resolved</summary>
        private static bool CanResolve(string modelKey, IResolveModel3D resolver)
        {
            var _resolve = resolver;
            while (_resolve != null)
            {
                // resolve model
                if (_resolve.CanResolveModel3D(modelKey))
                {
                    return true;
                }
                _resolve = _resolve.IResolveModel3DParent;
            }
            return false;
        }

        public static string GetModelKey(ICoreObject entity, IResolveModel3D resolver)
        {
            // default
            var _default = entity.Adjuncts.OfType<DefaultModelKey>().FirstOrDefault();
            if ((_default != null) && !string.IsNullOrEmpty(_default.ModelKey))
            {
                var _modelKey = _default.ModelKey;
                if (CanResolve(_modelKey, resolver))
                {
                    return _modelKey;
                }
            }

            if (entity is CloseableContainerObject)
            {
                return @"WoodenChest";
            }
            return string.Empty;
        }
    }
}
