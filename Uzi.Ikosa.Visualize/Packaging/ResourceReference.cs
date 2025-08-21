using System;
using Uzi.Packaging;

namespace Uzi.Visualize.Packaging
{
    [Serializable]
    public abstract class ResourceReference : IPackagePartReference
    {
        #region state
        private readonly string _Name;
        private string _PackageID;
        private string _PackageSet;
        private string _Internal;
        private ResourceReferenceManager _Parent;
        #endregion

        protected ResourceReference(ResourceReferenceManager parent, string name, string packageSet, string packageID, string internalPath)
        {
            _Name = name;
            _PackageSet = packageSet;
            _PackageID = packageID;
            _Internal = internalPath;
            _Parent = parent;
        }

        protected abstract void RefreshResolver();

        public string Name => _Name;

        public string PackageSet { get => _PackageSet; set { _PackageSet = value; RefreshResolver(); } }
        public string PackageID { get => _PackageID; set { _PackageID = value; RefreshResolver(); } }
        public string InternalPath { get => _Internal; set { _Internal = value; RefreshResolver(); } }

        public (string packageSet, string packageID) GetRefKey()
            => (_PackageSet ?? string.Empty, _PackageID);

        public ResourceReferenceManager Parent => _Parent;

        public abstract IBasePart Part { get; }

        public CorePackage ResolvePackage()
            => PackageManager.Manager.GetPackage(this);
    }
}