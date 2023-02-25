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
    public class FragmentsResourceReference : ResourceReference, ICorePart
    {
        public FragmentsResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath)
            : base(parent, name, packageSet, packageID, internalPath)
        {
        }

        #region private data
        [field: NonSerialized, JsonIgnore]
        private IResolveFragment _Resolver = null;
        #endregion

        public IResolveFragment Resolver { get { return _Resolver; } set { _Resolver = value; } }

        protected override void RefreshResolver()
        {
            Parent.RefreshFragmentResolver();
        }

        public override IBasePart Part
            => Resolver as IBasePart
            ?? (Resolver as ICorePartMetaModelFragmentResolver)?.Part as IBasePart;

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName
        {
            get { return this.GetType().FullName; }
        }

        #endregion
    }
}
