using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize
{
    [Serializable]
    public class ModelsResourceReference : ResourceReference, ICorePart
    {
        public ModelsResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath)
            : base(parent, name, packageSet, packageID, internalPath)
        {
        }

        #region private data
        [field: NonSerialized, JsonIgnore]
        private IResolveModel3D _Resolver = null;
        #endregion

        public override IBasePart Part
            => Resolver as IBasePart;

        public IResolveModel3D Resolver { get { return _Resolver; } set { _Resolver = value; } }

        protected override void RefreshResolver()
        {
            Parent.RefreshModelResolver();
        }

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName
        {
            get { return this.GetType().FullName; }
        }

        #endregion
    }
}