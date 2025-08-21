using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Visualize.Packaging;
using Uzi.Packaging;
using Newtonsoft.Json;

namespace Uzi.Visualize
{
    [Serializable]
    public class BrushesResourceReference : ResourceReference, ICorePart
    {
        public BrushesResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath)
            : base(parent, name, packageSet, packageID, internalPath)
        {
        }

        #region state
        [field: NonSerialized, JsonIgnore]
        private IResolveMaterial _Resolver = null;
        #endregion

        protected override void RefreshResolver() => Parent.RefreshBrushResolver();

        public IResolveMaterial Resolver { get => _Resolver; set => _Resolver = value; }
        public override IBasePart Part => Resolver as IBasePart;

        // ICorePart Members
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public string TypeName => GetType().FullName;
    }
}
