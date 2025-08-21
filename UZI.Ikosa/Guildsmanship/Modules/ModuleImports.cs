using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class ModuleImports : INotifyPropertyChanged, ICorePart, IListPackagePartReferences, IDeserializationCallback
    {
        private readonly List<ModuleImportReference> _Modules;
        private readonly VariableAssociationSet _Preferences;

        public ModuleImports()
        {
            _Modules = [];
            _Preferences = new VariableAssociationSet();
            PackageManager.Manager.AddPackagePartReferenceLister(this);
        }

        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        public IEnumerable<IPackagePartReference> AllReferences
            => _Modules.AsEnumerable<IPackagePartReference>();

        /// <summary>
        /// <para>Module Elements associated with specific variable values.</para>
        /// <para>The intent is this are shown as preferences in lists and links.</para>
        /// </summary>
        public VariableAssociationSet VariableAssociationSet => _Preferences;

        #region private Module GetModule(ModuleImportReference reference)
        private Module GetModule(ModuleImportReference reference)
        {
            var (_package, _parts) = PackageManager.Manager.GetResolutionContext(reference);
            if (_parts != null)
            {
                if (_parts.ContainsKey(reference.InternalPath))
                {
                    // already tracking this part, so supply it
                    var _part = _parts[reference.InternalPath] as Module;
                    reference.Module = _part;
                    return reference.Module;
                }
                else
                {
                    // not tracking the part, so find in the package
                    if (_package.FindBasePart(reference.InternalPath) is Module _mod)
                    {
                        // found, so add ...
                        _parts.Add(reference.InternalPath, _mod);
                        reference.Module = _mod;

                        // ... and supply
                        return reference.Module;
                    }
                }
            }

            // no good
            reference.Module = null;
            return null;
        }
        #endregion

        #region internal void RefreshModules()
        internal void RefreshModules()
        {
            var _modules = from _mRef in _Modules
                           let _mod = GetModule(_mRef)
                           where (_mRef != null)
                           select _mod;
            PackageManager.Manager.CleanupCache();
            DoPropertyChanged(nameof(Modules));
            DoPropertyChanged(nameof(Relationships));
        }
        #endregion

        public void AddModule(ModuleImportReference moduleReference)
        {
            if (!_Modules.Any(_mr => _mr.Name.Equals(moduleReference.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                _Modules.Add(moduleReference);
                RefreshModules();
            }
        }

        public void RemoveModule(ModuleImportReference moduleReference)
        {
            var _modRef = _Modules.FirstOrDefault(_ir => _ir.Name.Equals(moduleReference.Name, StringComparison.InvariantCultureIgnoreCase));
            if (_modRef != null)
            {
                // remove referebce
                _Modules.Remove(_modRef);
                RefreshModules();
            }
        }

        public IEnumerable<ModuleImportReference> Modules
            => _Modules.Select(_mi => _mi);

        // ICorePart
        public string Name => @"Modules References";

        public IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return new PartsFolder(this, @"Imported Modules", Modules, typeof(ModuleImportReference));
                yield break;
            }
        }

        public string TypeName => typeof(ModuleImports).FullName;

        public void Close()
        {
            // release all references
            _Modules.Clear();

            // cleanup cache
            PackageManager.Manager?.CleanupCache();
            PackageManager.Manager?.RemovePackagePartReferenceLister(this);
        }

        public void OnDeserialization(object sender)
        {
            PackageManager.Manager.AddPackagePartReferenceLister(this);
        }
    }
}
