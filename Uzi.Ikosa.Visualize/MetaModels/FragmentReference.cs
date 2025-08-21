using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    [MarkupExtensionReturnType(typeof(Model3D))]
    public class FragmentReference : MarkupExtension
    {
        [ThreadStatic]
        private static List<IResolveFragment> _Resolvers = [];

        public static List<IResolveFragment> Resolvers
        {
            get
            {
                _Resolvers ??= [];
                return _Resolvers;
            }
        }

        public static void PushResolver(IResolveFragment resolver)
        {
            if (!FragmentReference.Resolvers.Contains(resolver))
            {
                FragmentReference.Resolvers.Insert(0, resolver);
            }
        }

        public static void PullResolver(IResolveFragment resolver)
        {
            if (FragmentReference.Resolvers.Contains(resolver))
            {
                FragmentReference.Resolvers.Remove(resolver);
            }
        }

        [ThreadStatic]
        private static Action<string> _KeyReferenced = null;

        /// <summary>Thread static tracking callback</summary>
        public static Action<string> ReferencedKey { get { return _KeyReferenced; } set { _KeyReferenced = value; } }

        public string Key { get; set; }
        public double? OffsetX { get; set; }
        public double? OffsetY { get; set; }
        public double? OffsetZ { get; set; }

        #region public override object ProvideValue(IServiceProvider serviceProvider)
        /// <summary>Provide value as a MarkupExtension</summary>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var _group = new Model3DGroup();
            if (MetaModelResolutionStack.Any())
            {
                // able to track reference keys?
                if (_KeyReferenced != null)
                {
                    _KeyReferenced(this.Key);
                }

                var _thisNode = MetaModelResolutionStack.Peek();
                // TODO: handle programmatic nodes...(such as head model)
                foreach (var _fragNode in _thisNode.Components
                    .Where(_fn => (_fn.ReferenceKey == this.Key) && !string.IsNullOrEmpty(_fn.FragmentKey)))
                {
                    // store current values
                    var _prevIntFound = IntVal.ReferencedKey;
                    var _prevDoubleFound = DoubleVal.ReferencedKey;
                    var _prevMatFound = VisualEffectMaterial.ReferencedKey;
                    var _prevFragFound = FragmentReference.ReferencedKey;
                    var _track = _prevIntFound != null && _prevDoubleFound != null && _prevFragFound != null && _prevMatFound != null;
                    var _sEffect = SenseEffectExtension.EffectValue;
                    try
                    {
                        // push the fragment node
                        MetaModelResolutionStack.Push(_fragNode);

                        // highlight if selected
                        if (_fragNode.IsSelected)
                        {
                            SenseEffectExtension.EffectValue = VisualEffect.Highlighted;
                        }

                        if (_track)
                        {
                            _fragNode.AttachReferenceTrackers();
                        }

                        // tracking list
                        var _repeat = new List<IResolveFragment>();
                        foreach (var _res in FragmentReference.Resolvers)
                        {
                            Model3D _fragment = null;
                            var _rez = _res;

                            // repeat if we haven't found a fragment, still have a resolver, and are not repeating resolvers
                            while ((_fragment == null) && (_rez != null) && !_repeat.Contains(_rez))
                            {
                                // repeat tracking
                                _repeat.Add(_rez);

                                // attempt fragment resolution
                                _fragment = _rez.GetFragment(this, _fragNode);

                                // get next resolver
                                _rez = _rez.IResolveFragmentParent;
                            }

                            if (_fragment != null)
                            {
                                // if we found a fragment, then add it
                                if (MetaModelResolutionStack.IsHitTestable)
                                {
                                    // will need to hit test (such as an editor)
                                    MetaModelFragmentNode.SetMetaModelFragmentNode(_fragment, _fragNode);
                                }
                                _group.Children.Add(_fragment);
                                break;
                            }
                        }
                    }
                    finally
                    {
                        // put everything back, and pop the current model resolution node
                        if (_track)
                        {
                            IntVal.ReferencedKey = _prevIntFound;
                            DoubleVal.ReferencedKey = _prevDoubleFound;
                            VisualEffectMaterial.ReferencedKey = _prevMatFound;
                            FragmentReference.ReferencedKey = _prevFragFound;
                        }
                        SenseEffectExtension.EffectValue = _sEffect;
                        MetaModelResolutionStack.Pop();
                    }
                }
            }

            // freeze the group
            _group.Freeze();
            if (_group.Children.Count == 1)
            {
                // only had one, so ditch the group
                return _group.Children.First();
            }

            // return the group (even if empty to avoid NULL member adds in containing model)
            return _group;
        }
        #endregion

        #region public Model3D ApplyTransforms(IMetaModelFragmentTransform node, Model3D model)
        /// <summary>Applies the transforms to the model</summary>
        public Model3D ApplyTransforms(IMetaModelFragmentTransform node, Model3D model)
        {
            // transform as needed
            var _tx = new Transform3DGroup();

            // track base point for node
            var _origin = new Vector3D(this.OffsetX ?? 0, this.OffsetY ?? 0, this.OffsetZ ?? 0);

            // origin offset
            if (node.OriginOffset.HasValue)
            {
                _tx.Children.Add(new TranslateTransform3D(node.OriginOffset.Value));
            }

            // node scale
            if (node.Scale.HasValue)
            {
                _tx.Children.Add(new ScaleTransform3D(node.Scale ?? new Vector3D(1, 1, 1)));
            }

            // node orientation
            if (node.Roll.HasValue)
            {
                var _rollAxis = node.NoseUp ? new Vector3D(0, 0, 1) : new Vector3D(0, 1, 0);
                _tx.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(_rollAxis, node.Roll ?? 0d)));
            }
            if (node.Pitch.HasValue)
            {
                _tx.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(1, 0, 0), node.Pitch ?? 0d)));
            }

            if (node.Yaw.HasValue)
            {
                var _yawAxis = node.NoseUp ? new Vector3D(0, 1, 0) : new Vector3D(0, 0, 1);
                _tx.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(_yawAxis, node.Yaw ?? 0d)));
            }

            // fragment offset
            var _offset = (node.Offset ?? new Vector3D()) + _origin;
            if (_offset.LengthSquared > 0)
            {
                _tx.Children.Add(new TranslateTransform3D(_offset));
            }

            // apply transforms and capture inverse
            if (_tx.Children.Count == 1)
            {
                model.Transform = _tx.Children.First();
                //node.LocalTransform = model.Transform;
            }
            else if (_tx.Children.Any())
            {
                model.Transform = _tx;
                //node.LocalTransform = model.Transform;
            }
            else
            {
                //node.LocalTransform = new TranslateTransform3D();
            }

            // return
            return model;
        }
        #endregion
    }
}
