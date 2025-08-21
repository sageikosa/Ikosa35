using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;
using System.ComponentModel;
using System.Windows;
using System.Runtime.Serialization;
using Uzi.Visualize.Contracts;

namespace Uzi.Visualize
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaModelFragmentNode : IMetaModelFragmentTransform, ICacheMeshes
    {
        #region ctor()
        public MetaModelFragmentNode()
        {
            Components = [];
            Brushes = [];
            IntRefs = [];
            DoubleRefs = [];
        }

        public MetaModelFragmentNode(MetaModelFragmentNode source)
        {
            // clone all references
            Brushes = new BrushCrossRefNodeCollection(
                source.Brushes.Select(_b => new BrushCrossRefNode(_b)).ToList());
            IntRefs = new IntReferenceCollection(
                source.IntRefs.Select(_ir => new IntReference(_ir)).ToList());
            DoubleRefs = new DoubleReferenceCollection(
                source.DoubleRefs.Select(_dr => new DoubleReference(_dr)).ToList());

            // clone fragments
            Components = [];
            foreach (var _c in source.Components.ToList())
            {
                Components.Add(new MetaModelFragmentNode(_c));
            }

            // clone parameters
            ReferenceKey = source.ReferenceKey;
            FragmentKey = source.FragmentKey;
            NoseUp = source.NoseUp;
            Roll = source.Roll;
            Pitch = source.Pitch;
            Yaw = source.Yaw;
            Scale = source.Scale;
            Offset = source.Offset;
            OriginOffset = source.OriginOffset;
        }
        #endregion

        #region Attached Dependency Property
        public static MetaModelFragmentNode GetMetaModelFragmentNode(DependencyObject obj)
        {
            return (MetaModelFragmentNode)obj.GetValue(MetaModelFragmentReferenceProperty);
        }

        public static void SetMetaModelFragmentNode(DependencyObject obj, MetaModelFragmentNode value)
        {
            obj.SetValue(MetaModelFragmentReferenceProperty, value);
        }

        // Using a DependencyProperty as the backing store for MetaModelFragmentNode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MetaModelFragmentReferenceProperty =
            DependencyProperty.RegisterAttached(@"MetaModelFragmentNode", typeof(MetaModelFragmentNode), typeof(MetaModelFragmentNode), new UIPropertyMetadata(null));
        #endregion

        [DataMember]
        /// <summary>Key referenced in XAML</summary>
        public string ReferenceKey { get; set; }

        [DataMember]
        /// <summary>Key defined for fragment as a package part</summary>
        public string FragmentKey { get; set; }

        [DataMember]
        public bool NoseUp { get; set; }
        [DataMember]
        public double? Roll { get; set; }
        [DataMember]
        public double? Pitch { get; set; }
        [DataMember]
        public double? Yaw { get; set; }

        [DataMember]
        public Vector3D? Scale { get; set; }
        [DataMember]
        public Vector3D? Offset { get; set; }
        [DataMember]
        public Vector3D? OriginOffset { get; set; }

        #region public void PrepareGather()
        /// <summary>Sets all components and brush XRef to InActive</summary>
        public void PrepareGather()
        {
            IsActive = false;

            // prepare components
            foreach (var _node in Components)
            {
                _node.PrepareGather();
            }

            // prepare brushes
            foreach (var _xRef in Brushes)
            {
                _xRef.IsActive = false;
            }

            // prepare intVals
            foreach (var _iVal in IntRefs)
            {
                _iVal.IsActive = false;
            }

            // prepare doubleVals
            foreach (var _dVal in DoubleRefs)
            {
                _dVal.IsActive = false;
            }
        }
        #endregion

        #region public void PruneAfterGather()
        /// <summary>Removes all inactive components and brush XRefs</summary>
        public void PruneAfterGather()
        {
            // remove dead components
            var _pruneNodes = Components.Where(_c => !_c.IsActive).ToList();
            foreach (var _node in _pruneNodes)
            {
                Components.Remove(_node);
            }

            // tell live components to prune
            foreach (var _node in Components)
            {
                _node.PruneAfterGather();
            }

            // remove dead brushes
            var _pruneXRefs = Brushes.Where(_b => !_b.IsActive).ToList();
            foreach (var _xRef in _pruneXRefs)
            {
                Brushes.Remove(_xRef);
            }

            // remove dead intVals
            var _pruneIVals = IntRefs.Where(_i => !_i.IsActive).ToList();
            foreach (var _val in _pruneIVals)
            {
                IntRefs.Remove(_val);
            }

            // remove dead doubleVals
            var _pruneDVals = DoubleRefs.Where(_d => !_d.IsActive).ToList();
            foreach (var _val in _pruneDVals)
            {
                DoubleRefs.Remove(_val);
            }

            FlushCache();
        }
        #endregion

        #region public void Clear()
        public void Clear()
        {
            FragmentKey = null;
            Components.Clear();
            Brushes.Clear();
            IntRefs.Clear();
            DoubleRefs.Clear();
            FlushCache();
        }
        #endregion

        public void UnSelect()
        {
            IsSelected = false;
            foreach (var _c in Components)
            {
                _c.UnSelect();
            }
        }

        [DataMember]
        public MetaModelFragmentNodeCollection Components { get; set; }
        [DataMember]
        public BrushCrossRefNodeCollection Brushes { get; set; }
        [DataMember]
        public IntReferenceCollection IntRefs { get; set; }
        [DataMember]
        public DoubleReferenceCollection DoubleRefs { get; set; }

        public IEnumerable<object> Children
            => Brushes
            .OrderBy(_b => _b.ReferenceKey).Select(_b => (object)_b)
            .Union(Components.OrderBy(_c => _c.ReferenceKey));

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsExpanded { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive { get; set; }

        public void AttachReferenceTrackers()
        {
            #region track parameter extensions
            IntVal.ReferencedKey = delegate (IntVal intVal)
            {
                var _iVal = IntRefs[intVal.Key];
                if (_iVal != null)
                {
                    _iVal.IsActive = true;
                }
                else
                {
                    IntRefs.Add(new IntReference
                    {
                        Key = intVal.Key,
                        Value = intVal.Value,
                        MinValue = intVal.MinValue,
                        MaxValue = intVal.MaxValue,
                        IsActive = true,
                        UseMaster = true
                    });
                }
            };
            DoubleVal.ReferencedKey = delegate (DoubleVal doubleVal)
            {
                var _dVal = DoubleRefs[doubleVal.Key];
                if (_dVal != null)
                {
                    _dVal.IsActive = true;
                }
                else
                {
                    DoubleRefs.Add(new DoubleReference
                    {
                        Key = doubleVal.Key,
                        Value = doubleVal.Value,
                        MinValue = doubleVal.MinValue,
                        MaxValue = doubleVal.MaxValue,
                        IsActive = true,
                        UseMaster = true
                    });
                }
            };
            #endregion

            #region track visual effect material references
            VisualEffectMaterial.ReferencedKey = delegate (string materialKey)
            {
                var _brush = Brushes[materialKey];
                if (_brush != null)
                {
                    _brush.IsActive = true;
                }
                else
                {
                    Brushes.Add(new BrushCrossRefNode
                    {
                        ReferenceKey = materialKey,
                        IsActive = true
                    });
                }
            };
            #endregion

            #region track fragment references
            FragmentReference.ReferencedKey = delegate (string fragKey)
            {
                var _nodes = Components[fragKey];
                if (_nodes.Any<MetaModelFragmentNode>())
                {
                    foreach (var _n in _nodes)
                    {
                        _n.IsActive = true;
                    }
                }
                else
                {
                    Components.Add(new MetaModelFragmentNode
                    {
                        ReferenceKey = fragKey,
                        IsExpanded = true,
                        IsActive = true
                    });
                }
            };
            #endregion
        }

        /// <summary>Climbs the node tree gathering all brush names found (without distinction nor order)</summary>
        public IEnumerable<string> GatherDefaultBrushNames()
            => Brushes.Select(_b => _b.ReferenceKey)
            .Union(Components.SelectMany(_c => _c.GatherDefaultBrushNames()));

        /// <summary>Climbs the node tree gathering all IntReferences found (without distinction nor order)</summary>
        public IEnumerable<IntReference> GatherMasterIntReferences()
            => IntRefs.Union(Components.SelectMany(_c => _c.GatherMasterIntReferences()));

        /// <summary>Climbs the node tree gathering all DoubleReferences found (without distinction nor order)</summary>
        public IEnumerable<DoubleReference> GatherMasterDoubleReferences()
            => DoubleRefs.Union(Components.SelectMany(_c => _c.GatherMasterDoubleReferences()));

        #region ICacheMeshes Members
        private Dictionary<int, MeshGeometry3D> _Meshes;
        private Dictionary<int, MeshGeometry3D> Meshes
        {
            get
            {
                _Meshes ??= [];
                return _Meshes;
            }
        }

        public bool HasKey(int key)
            => Meshes.ContainsKey(key);

        public MeshGeometry3D GetMesh(int key)
            => Meshes[key];

        public void SetMesh(int key, MeshGeometry3D mesh)
        {
            Meshes[key] = mesh;
        }

        public void FlushCache()
        {
            Meshes.Clear();
        }

        #endregion
    }
}
