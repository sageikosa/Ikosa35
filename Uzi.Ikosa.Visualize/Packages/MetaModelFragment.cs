using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Packaging;
using System.Xml;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    public class MetaModelFragment : StorablePart, ICacheMeshes
    {
        #region ctor()
        public MetaModelFragment(IRetrievablePartNameManager manager, FileInfo fragFile)
            : base(manager, fragFile.Name.Replace(@" ", @"_"))
        {
            _FragFile = fragFile;
            _Entry = null;
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }

        public MetaModelFragment(IRetrievablePartNameManager manager, MetaModelFragment source, string id)
            : base(manager, id)
        {
            _Source = source;
            _Entry = null;
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }

        public MetaModelFragment(IRetrievablePartNameManager manager, string id)
            : base(manager, id.Replace(@" ", @"_"))
        {
            _Entry = null;
            _Meshes = new Dictionary<int, MeshGeometry3D>();
        }
        #endregion

        #region state
        private ZipArchiveEntry _Entry;
        private readonly FileInfo _FragFile;
        private readonly MetaModelFragment _Source;
        private Model3D _Model;
        private readonly Dictionary<int, MeshGeometry3D> _Meshes;
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

        private Stream GetArchiveStream()
            => _Entry.Open();

        #region public virtual Model3D ResolveModel()
        public virtual Model3D ResolveModel(ICacheMeshes meshCache)
        {
            Model3D _mdl = null;
            ICacheMeshes _preCache = MeshCache.Current;
            try
            {
                MeshCache.Current = meshCache;
                if (_Entry != null)
                {
                    // load model
                    using var _mStream = GetArchiveStream();
                    _mdl = XamlReader.Load(_mStream) as Model3D;
                }
                else if (_FragFile != null)
                {
                    using var _fStream = _FragFile.OpenRead();
                    // load model
                    _mdl = XamlReader.Load(_fStream) as Model3D;
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
                            if (_Entry != null)
                            {
                                using var _mStream = _Entry.Open();
                                _Bytes = new byte[_mStream.Length];
                                _mStream.Read(_Bytes, 0, (int)_mStream.Length);
                            }
                            else if (_FragFile != null)
                            {
                                using var _fStream = _FragFile.OpenRead();
                                _Bytes = new byte[_fStream.Length];
                                _fStream.Read(_Bytes, 0, (int)_fStream.Length);
                            }
                        }
                    }
                }
                return _Bytes;
            }
        }
        #endregion

        public override IEnumerable<IRetrievablePart> Parts => Enumerable.Empty<IRetrievablePart>();
        public override string PartType => GetType().FullName;

        public override void ClosePart()
        {
            // NOTE: no open resources
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            using var _srcStream = _Entry?.Open()
                ?? (_FragFile?.OpenRead() as Stream)
                ?? ((_Source != null) ? new MemoryStream(_Source.StreamBytes) : null);
            if (_srcStream != null)
            {
                var _entry = archive.CreateEntry($@"{parentPath}/{PartName}");
                using var _saveStream = _entry.Open();
                StreamHelper.CopyStream(_srcStream, _saveStream);
                _Entry = _entry;
            }
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            _Entry = archive.GetEntry($@"{parentPath}/{PartName}");
            FlushCache();
            _Bytes = null;
        }

        #region ICacheMeshes Members

        public bool HasKey(int key)
            => _Meshes.ContainsKey(key);

        public MeshGeometry3D GetMesh(int key)
            => _Meshes[key];

        public void SetMesh(int key, MeshGeometry3D mesh)
            => _Meshes[key] = mesh;

        public void FlushCache()
            => _Meshes.Clear();

        #endregion
    }

    public class MetaModelFragmentPartListItem
    {
        public MetaModelFragment MetaModelFragment { get; set; }
        public bool IsLocal { get; set; }
    }
}
