using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class ModuleImportReference : IPackagePartReference, ICorePart
    {
        #region state
        private readonly string _Name;
        private string _PackageID;
        private string _PackageSet;
        private string _Internal;
        private ModuleImports _Parent;
        #endregion

        [field: NonSerialized, JsonIgnore]
        private Module _Module = null;

        public ModuleImportReference(ModuleImports parent, string name, string packageSet, string packageID, string internalPath)
        {
            _Name = name;
            _PackageSet = packageSet;
            _PackageID = packageID;
            _Internal = internalPath;
            _Parent = parent;
        }

        public string Name => _Name;
        public ModuleImports Parent => _Parent;
        public string TypeName => GetType().FullName;

        public string PackageSet { get => _PackageSet; set { _PackageSet = value; Parent.RefreshModules(); } }
        public string PackageID { get => _PackageID; set { _PackageID = value; Parent.RefreshModules(); } }
        public string InternalPath { get => _Internal; set { _Internal = value; Parent.RefreshModules(); } }

        public (string packageSet, string packageID) GetRefKey()
            => (_PackageSet ?? string.Empty, _PackageID);

        public Module Module { get => _Module; set => _Module = value; }
        public IBasePart Part => _Module;
        public IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        public CorePackage ResolvePackage()
            => PackageManager.Manager.GetPackage(this);
    }
}
