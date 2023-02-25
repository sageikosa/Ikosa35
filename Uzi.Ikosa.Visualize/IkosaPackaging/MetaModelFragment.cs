using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ikosa.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Windows.Media.Media3D;
using System.Windows.Markup;

namespace Uzi.Visualize.IkosaPackaging
{
    public class MetaModelFragment : BasePart, ICacheMeshes
    {
        /// <summary>Relationship to metamap models (http://pack.guildsmanship.com/visualize/metamodal/fragment)</summary>
        public const string MetaModelFragmentRelation = @"http://pack.guildsmanship.com/visualize/metamodal/fragment";

        #region construction
        public MetaModelFragment(ICorePartNameManager manager, FileInfo fragFile)
            : base(manager, fragFile.Name.Replace(@" ", @"_"))
        {
            _FragFile = fragFile;
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }

        public MetaModelFragment(ICorePartNameManager manager, MetaModelFragment source, string id)
            : base(manager, id)
        {
            _Source = source;
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }

        public MetaModelFragment(ICorePartNameManager manager, PackagePart part, string id)
            : base(manager, part, id.Replace(@" ", @"_"))
        {
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }
        #endregion

        #region data
        private readonly FileInfo _FragFile;
        private readonly MetaModelFragment _Source;
        private Model3D _Model;
        private readonly Dictionary<int, MeshGeometry3D> _Meshes;
        #endregion

        #region public static IEnumerable<MetaModelFragment> GetMetaModelFragmentResources(ICorePartNameManager manager, PackagePart part)
        /// <summary>Pre-loads meta-models from related package parts for IResolveModel3D</summary>
        /// <param name="part">part with (possible) MetaModelFragment relations</param>
        public static IEnumerable<MetaModelFragment> GetMetaModelFragmentResources(ICorePartNameManager manager, PackagePart part)
        {
            foreach (var _imgRel in part.GetRelationshipsByType(MetaModelFragment.MetaModelFragmentRelation))
            {
                var _imgPart = part.Package.GetPart(_imgRel.TargetUri);
                yield return new MetaModelFragment(manager, _imgPart, _imgRel.Id);
            }
        }
        #endregion

        #region public Model3D Model { get; }
        public virtual Model3D Model
        {
            get
            {
                if (_Model == null)
                {
                    _Model = ResolveModel(this);
                }
                return _Model;
            }
        }
        #endregion

        #region public virtual Model3D ResolveModel()
        public virtual Model3D ResolveModel(ICacheMeshes meshCache)
        {
            Model3D _mdl = null;
            ICacheMeshes _preCache = MeshCache.Current;
            try
            {
                MeshCache.Current = meshCache;
                if (Part != null)
                {
                    // load model
                    using (var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read))
                    {
                        _mdl = XamlReader.Load(_mStream) as Model3D;
                    }
                }
                else if (_FragFile != null)
                {
                    using (var _fStream = _FragFile.OpenRead())
                    {
                        // load model
                        _mdl = XamlReader.Load(_fStream) as Model3D;
                    }
                }
                else if (_Source != null)
                {
                    _mdl = _Source.ResolveModel(meshCache);
                }
            }
            catch
            {
            }
            finally
            {
                MeshCache.Current = _preCache;
            }
            return _mdl;
        }
        #endregion

        #region public byte[] StreamBytes { get; }
        private byte[] _Bytes;
        public byte[] StreamBytes
        {
            get
            {
                if (_Bytes == null)
                {
                    lock (_Meshes)
                    {
                        if (_Bytes == null)
                        {
                            if (Part != null)
                            {
                                using (var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read))
                                {
                                    _Bytes = new byte[_mStream.Length];
                                    _mStream.Read(_Bytes, 0, (int)_mStream.Length);
                                }
                            }
                            else if (_FragFile != null)
                            {
                                using (var _fStream = _FragFile.OpenRead())
                                {
                                    _Bytes = new byte[_fStream.Length];
                                    _fStream.Read(_Bytes, 0, (int)_fStream.Length);
                                }
                            }
                        }
                    }
                }
                return _Bytes;
            }
        }
        #endregion

        public override IEnumerable<ICorePart> Relationships { get { yield break; } }
        public override string TypeName => GetType().FullName;

        #region public override void Save(Package parent)
        public override void Save(Package parent)
        {
            // re-Resolve Model before changing parts
            using (var _fStream =
                (Part != null ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FragFile != null) ? (Stream)_FragFile.OpenRead()
                : (_Source != null) ? new MemoryStream(_Source.StreamBytes)
                : null))
            {
                if (_fStream != null)
                {
                    Uri _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
                    _Part = parent.CreatePart(_base, @"text/xaml+xml", CompressionOption.Normal);
                    parent.CreateRelationship(_base, TargetMode.Internal, MetaModelFragmentRelation, Name);

                    DoSave(_fStream);
                }
            }
        }
        #endregion

        #region public override void Save(PackagePart parent, Uri baseUri)
        public override void Save(PackagePart parent, Uri baseUri)
        {
            // re-Resolve Model before changing parts
            using (var _fStream =
                (Part != null ? Part.GetStream(FileMode.Open, FileAccess.Read)
                : (_FragFile != null) ? (Stream)_FragFile.OpenRead()
                : (_Source != null) ? new MemoryStream(_Source.StreamBytes)
                : null))
            {
                if (_fStream != null)
                {
                    Uri _base = UriHelper.ConcatRelative(baseUri, Name);
                    _Part = parent.Package.CreatePart(_base, @"text/xaml+xml", CompressionOption.Normal);
                    parent.CreateRelationship(_base, TargetMode.Internal, MetaModelFragmentRelation, Name);

                    DoSave(_fStream);
                }
            }
        }
        #endregion

        #region private void DoSave(Stream fragStream)
        private void DoSave(Stream fragStream)
        {
            // save xaml
            using (var _saveStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                StreamHelper.CopyStream(fragStream, _saveStream);
            }
        }
        #endregion

        public override void Close()
        {
            // NOTE: no open resources
        }

        protected override void OnRefreshPart()
        {
            FlushCache();
        }

        #region ICacheMeshes Members

        public bool HasKey(int key)
        {
            return _Meshes.ContainsKey(key);
        }

        public MeshGeometry3D GetMesh(int key)
        {
            return _Meshes[key];
        }

        public void SetMesh(int key, MeshGeometry3D mesh)
        {
            _Meshes[key] = mesh;
        }

        public void FlushCache()
        {
            _Meshes.Clear();
        }

        #endregion
    }
}