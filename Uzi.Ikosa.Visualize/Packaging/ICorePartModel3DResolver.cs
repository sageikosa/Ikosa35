using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;
using System.Windows.Media.Media3D;

namespace Uzi.Visualize.Packaging
{
    public class ICorePartModel3DResolver : IResolveModel3D
    {
        public ICorePartModel3DResolver(ICorePart part)
        {
            _Part = part;
        }

        private ICorePart _Part;

        public ICorePart Part { get { return _Part; } }

        #region IResolveModel3D Members

        public Model3D GetPrivateModel3D(object key)
        {
            if (_Part != null)
            {
                string _key = key.ToString();
                var _mdl = _Part.FindBasePart(_key) as Model3DPart;
                if (_mdl != null)
                {
                    return _mdl.ResolveModel();
                }
            }
            return null;
        }

        public bool CanResolveModel3D(object key)
        {
            if (_Part != null)
            {
                string _key = key.ToString();
                if (_Part.FindBasePart(_key) is Model3DPart)
                {
                    return true;
                }
            }
            return false;
        }

        public IResolveModel3D IResolveModel3DParent { get { return null; } }

        public IEnumerable<Model3DPartListItem> ResolvableModels
        {
            get
            {
                if (_Part != null)
                {
                    return _Part.Relationships.OfType<Model3DPart>()
                        .Union(_Part.Relationships.OfType<MetaModel>())
                        .OrderBy(_mdl => _mdl.Name)
                        .Select(_mdl => new Model3DPartListItem
                        {
                            Model3DPart = _mdl,
                            IsLocal = true
                        });
                }

                // empty list
                return new Model3DPartListItem[] { };
            }
        }

        #endregion
    }
}
