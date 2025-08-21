using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize
{
    public class BuildableContext
    {
        public BuildableContext()
        {
            _Buildables = new Dictionary<BuildableMeshKey, BuildableMesh>(new BuildableMeshKeyComparer());
        }

        private Dictionary<BuildableMeshKey, BuildableMesh> _Buildables;

        public BuildableMesh GetBuildableMesh(BuildableMeshKey key, Func<BuildableMaterial> materialProvider)
        {
            if (!_Buildables.ContainsKey(key))
            {
                var _material = materialProvider();
                _Buildables.Add(key, new BuildableMesh(_material));
            }
            return _Buildables[key];
        }

        private Model3D FreezeModel(Model3D model)
        {
            model.Freeze();
            return model;
        }

        public IEnumerable<Model3D> GetModel3D(bool isAlpha)
            => from _bm in _Buildables
               where _bm.Value.IsAlpha == isAlpha
               from _m in _bm.Value.GetModels()
               select FreezeModel(_m);
    }
}
