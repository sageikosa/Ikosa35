using System.Collections.Generic;
using System.Runtime.Serialization;
using Uzi.Visualize.Packaging;
using System.Windows.Media.Media3D;
using System.Windows.Markup;
using System.IO;

namespace Uzi.Visualize.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class MetaModelFragmentInfo : ICacheMeshes
    {
        public MetaModelFragmentInfo()
        {
        }

        public MetaModelFragmentInfo(MetaModelFragment fragment)
        {
            Bytes = fragment.StreamBytes;
        }

        [DataMember]
        public byte[] Bytes { get; set; }

        private Dictionary<int, MeshGeometry3D> _Meshes;

        public virtual Model3D ResolveModel(ICacheMeshes meshCache)
        {
            ICacheMeshes _preCache = MeshCache.Current;
            try
            {
                MeshCache.Current = meshCache;
                using (var _memStream = new MemoryStream(this.Bytes))
                {
                    return XamlReader.Load(_memStream) as Model3D;
                }
            }
            finally
            {
                MeshCache.Current = _preCache;
            }
        }

        #region ICacheMeshes Members

        private Dictionary<int, MeshGeometry3D> _MeshDictionary
        {
            get
            {
                _Meshes ??= [];
                return _Meshes;
            }
        }

        public bool HasKey(int key)
        {
            return _MeshDictionary.ContainsKey(key);
        }

        public MeshGeometry3D GetMesh(int key)
        {
            return _MeshDictionary[key];
        }

        public void SetMesh(int key, MeshGeometry3D mesh)
        {
            _MeshDictionary[key] = mesh;
        }

        public void FlushCache()
        {
            _Meshes.Clear();
        }

        #endregion
    }
}
