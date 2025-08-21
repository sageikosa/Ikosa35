using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Visualize
{
    [Serializable]
    public class ImagesResourceReference : ResourceReference, ICorePart
    {
        public ImagesResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath) 
            : base(parent, name, packageSet, packageID, internalPath)
        {
        }

        #region private data
        [field: NonSerialized, JsonIgnore]
        private IResolveBitmapImage _Resolver = null;
        #endregion

        public IResolveBitmapImage Resolver { get { return _Resolver; } set { _Resolver = value; } }

        protected override void RefreshResolver()
        {
            Parent.RefreshImageResolver();
        }

        public override IBasePart Part
            => Resolver as IBasePart
            ?? (Resolver as ICorePartImageResolver)?.Part as IBasePart;

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName => GetType().FullName;

        #endregion
    }
}
