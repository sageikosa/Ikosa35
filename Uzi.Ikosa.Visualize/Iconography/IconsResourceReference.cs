using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uzi.Visualize.Packaging;
using Uzi.Packaging;
using Newtonsoft.Json;

namespace Uzi.Visualize
{
    [Serializable]
    public class IconsResourceReference : ResourceReference, ICorePart
    {
        public IconsResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath)
            : base(parent, name, packageSet, packageID, internalPath)
        {
        }

        #region private data
        [field: NonSerialized, JsonIgnore]
        private IResolveIcon _Resolver = null;
        #endregion

        public IResolveIcon Resolver { get { return _Resolver; } set { _Resolver = value; } }

        protected override void RefreshResolver()
        {
            Parent.RefreshIconResolver();
        }

        public override IBasePart Part
            => Resolver as IBasePart
            ?? (Resolver as ICorePartImageResolver)?.Part as IBasePart;

        #region ICorePart Members

        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public string TypeName
        {
            get { return this.GetType().FullName; }
        }

        #endregion
    }
}
