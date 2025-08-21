using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Items;
using Uzi.Packaging;
using Uzi.Visualize.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    [Serializable]
    public class Module : IBasePart, ICorePartNameManager, INotifyPropertyChanged
    {
        #region package serialized state
        private string _ModuleID;
        private readonly Description _Description;

        /// <summary>Registered Variables that can be inspected across the module-node complex</summary>
        private readonly Dictionary<Guid, Variable> _Variables;

        /// <summary>Defined info keys</summary>
        private readonly Dictionary<Guid, InfoKey> _InfoKeys;

        private readonly Dictionary<Guid, string> _NamedKeys = [];

        private Dictionary<Guid, ItemElement> _ItemElements = [];

        private Dictionary<Guid, TeamGroupSummary> _Teams;
        #endregion

        #region non-serialized: package-relative state
        [NonSerialized, JsonIgnore]
        private ICorePartNameManager _NameManager = null;

        [NonSerialized, JsonIgnore]
        private PackagePart _ModulePart;

        [NonSerialized, JsonIgnore]
        private ModuleResources _Resources = null;

        [NonSerialized, JsonIgnore]
        private VisualResources _Visuals = null;

        [NonSerialized, JsonIgnore]
        private PartsFolder _VariableFolder = null;

        [NonSerialized, JsonIgnore]
        private PartsFolder _InfoKeyFolder = null;

        [NonSerialized, JsonIgnore]
        private NamedKeysPart _NamedKeysPart = null;

        [NonSerialized, JsonIgnore]
        private PartsFolder _ItemsFolder = null;

        [NonSerialized, JsonIgnore]
        private PartsFolder _TeamGroupFolder = null;
        #endregion

        /// <summary>Relationship type to identify a module object part (http://pack.guildsmanship.com/ikosa/module)</summary>
        public const string ModulePartRelation = @"http://pack.guildsmanship.com/ikosa/module";

        #region ctor()

        /// <summary>Create a New module</summary>
        public Module(string moduleID, Description description)
        {
            // CurrentUse will be New
            _ModuleID = moduleID;
            _Description = description;

            _Variables = [];
            _InfoKeys = [];
            _ItemElements = [];
            _NamedKeys = [];
            _Teams = [];

            _VariableFolder = new PartsFolder(this, @"Variables", _Variables.Values.OfType<Variable>(), typeof(Variable)) { HideTree = true };
            _InfoKeyFolder = new PartsFolder(this, @"Info Keys", _InfoKeys.Values.OfType<InfoKey>(), typeof(InfoKey)) { HideTree = true };
            _ItemsFolder = new PartsFolder(this, @"Items", _ItemElements.Values, typeof(ItemElement)) { HideTree = true };
            _NamedKeysPart = new NamedKeysPart(this);

            _TeamGroupFolder = new PartsFolder(this, @"Team Groups", Teams.Values.OfType<TeamGroupSummary>(), typeof(TeamGroupSummary)) { HideTree = true };

            _Resources = new ModuleResources(this, @"ModuleResources");
            _Visuals = new VisualResources(this, @"VisualResources");
        }

        /// <summary>Load a module from a package part, only making the importable infomation accessible.</summary>
        public static Module GetModule(PackagePart part)
        {
            if (part != null)
            {
                // CurrentUse will be Imported
                using var _ctxStream = part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                var _mod = (Module)_fmt.Deserialize(_ctxStream);
                _mod.SetPackagePart(part);
                return _mod;
            }
            return null;
        }

        protected void SetPackagePart(PackagePart part)
        {
            // CurrentUse will be Imported
            _ModulePart = part;
            _Resources = null;
            _Visuals = null;

            // fixup late-add to schema
            _ItemElements ??= [];

            // establish
            _VariableFolder = new PartsFolder(this, @"Variables", _Variables.Values.OfType<Variable>(), typeof(Variable)) { HideTree = true };
            _InfoKeyFolder = new PartsFolder(this, @"Info Keys", _InfoKeys.Values.OfType<InfoKey>(), typeof(InfoKey)) { HideTree = true };
            _ItemsFolder = new PartsFolder(this, @"Items", _ItemElements.Values.OfType<ItemElement>(), typeof(ItemElement)) { HideTree = true };
            _NamedKeysPart = new NamedKeysPart(this);
            _Teams ??= [];
            _TeamGroupFolder = new PartsFolder(this, @"Team Groups", Teams.Values.OfType<TeamGroupSummary>(), typeof(TeamGroupSummary)) { HideTree = true };
        }

        #endregion

        #region ModuleUse control

        public ModuleUse CurrentUse
            => _ModulePart == null ? ModuleUse.New
            : _Resources == null ? ModuleUse.Referenced
            : ModuleUse.Open;

        public void Open()
        {
            if (CurrentUse == ModuleUse.Referenced)
            {
                // parts
                var _parts = _ModulePart?.GetRelationships().RelatedBaseParts(this).ToList() ?? [];

                // after this, CurrentUse will be Open
                _Resources
                    = _parts.OfType<ModuleResources>().FirstOrDefault() ?? new ModuleResources(this, @"ModuleResources");

                _Visuals
                    = _parts.OfType<VisualResources>().FirstOrDefault() ?? new VisualResources(this, @"VisualResources");
            }
        }

        public void Close()
        {
            if (CurrentUse == ModuleUse.Open)
            {
                // after this, CurrentUse will be Referenced
                _Resources?.Close();
                _Resources = null;
                _Visuals?.Close();
                _Visuals = null;
            }
        }

        #endregion

        public Description Description => _Description;

        // INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));

        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        // ICorePart

        #region public string BindableName { get; set; }
        public string BindableName
        {
            get => Name;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (!(_NameManager?.CanUseName(value, typeof(Module)) ?? false))
                {
                    return;
                }

                // TODO: confirm that moduleID is unique in set of registered modules
                _ModuleID = value.ToSafeString();
                DoPropertyChanged(nameof(Name));
                DoPropertyChanged(nameof(BindableName));
            }
        }
        #endregion

        public ICorePartNameManager NameManager
        {
            get => _NameManager;
            set => _NameManager ??= value;
        }

        public PackagePart Part => _ModulePart;
        public string Name => _ModuleID;
        public string TypeName => GetType().FullName;

        public ModuleResources Resources => _Resources;
        public VisualResources Visuals => _Visuals;

        public IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return _InfoKeyFolder;
                yield return _VariableFolder;
                yield return _TeamGroupFolder;
                yield return _ItemsFolder;
                yield return _NamedKeysPart;
                if (Resources != null)
                {
                    yield return Resources;
                }
                if (Visuals != null)
                {
                    yield return Visuals;
                }
                yield break;
            }
        }

        // TODO: state saving only...

        #region saving

        public void Save(Package parent)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), $@"{Name}.module");
            var _content = UriHelper.ConcatRelative(_folder, @"ikosa.module");
            _ModulePart = parent.CreatePart(_content, @"ikosa/module", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, ModulePartRelation);

            DoSave(_folder);

            //JsonSave();
        }

        public void Save(PackagePart parent, Uri baseUri)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(baseUri, $@"{Name}.module");
            var _content = UriHelper.ConcatRelative(_folder, @"ikosa.module");
            _ModulePart = parent.Package.CreatePart(_content, @"ikosa/module", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, ModulePartRelation);

            DoSave(_folder);

            //JsonSave();
        }

        #region private void DoSave(Uri _base)
        private void DoSave(Uri _base)
        {
            // conditions, affinities, info keys
            using (var _modStream = _ModulePart.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                IFormatter _fmt = new BinaryFormatter();
                _fmt.Serialize(_modStream, this);
            }

            // save resources
            _Resources?.Save(_ModulePart, _base);
            _Visuals?.Save(_ModulePart, _base);
        }
        #endregion

        #endregion

        #region public void RefreshPart(PackagePart part)
        public void RefreshPart(PackagePart part)
        {
            _ModulePart = part;

            // if this wasn't cleared, update and add
            if ((_ModulePart != null) && (CurrentUse == ModuleUse.Open))
            {
                var _parts = _ModulePart.GetRelationships().RelatedPackageParts().ToList();

                // module resources
                var _modResources = _parts.FirstOrDefault(_p => _p.RelationshipType == ModuleResources.ModuleResourcesRelation);
                if (_modResources != null)
                {
                    _Resources.RefreshPart(_modResources.Part);
                }

                // visual resources
                var _visResources = _parts.FirstOrDefault(_p => _p.RelationshipType == VisualResources.VisualResourcesRelation);
                if (_visResources != null)
                {
                    _Visuals.RefreshPart(_visResources.Part);
                }
            }
        }
        #endregion

        // ICorePartNameManager
        public bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            if (partType.Equals(typeof(Variable)))
            {
                return !_Variables.Values.Any(_v => _v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else if (partType.Equals(typeof(InfoKey)))
            {
                return !_InfoKeys.Values.Any(_v => _v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else if (partType.Equals(typeof(ItemElement)))
            {
                return !_ItemElements.Values.Any(_v => _v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            else if (partType.Equals(typeof(TeamGroupSummary)))
            {
                return !_Teams.Values.Any(_v => _v.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: nothing
        }

        #region Named Key Resolution and Access

        /// <summary>Names for in-game security keys for this module</summary>
        public Dictionary<Guid, string> NamedKeyGuids => _NamedKeys;

        public bool CanResolveNamedKey(Guid namedKey)
            // local resolution
            => NamedKeyGuids.TryGetValue(namedKey, out var _nKey)
            // module imports
            || ((CurrentUse == ModuleUse.Open)
            && Resources.Imports.Modules.Any(_imp => _imp.Module.CanResolveNamedKey(namedKey)));

        public (Module module, string name) GetNamedKey(Guid namedKey)
            // local resolution
            => NamedKeyGuids.TryGetValue(namedKey, out var _nKey)
            ? (this, _nKey)
            // module imports
            : CurrentUse == ModuleUse.Open
            ? (from _imp in Resources.Imports.Modules
               let _iName = _imp.Module.GetNamedKey(namedKey)
               where _iName.module != null
               select _iName).FirstOrDefault()
            // nothing
            : default;

        public IEnumerable<(Module module, Guid id, string name)> GetResolvableNamedKeys()
            // list direct module teams
            => NamedKeyGuids
            .Select(_ik => (this, _ik.Key, _ik.Value))
            // if open, include imported
            .Concat((CurrentUse == ModuleUse.Open)
                ? (from _ref in Resources.Imports.Modules
                   from _t in _ref.Module.GetResolvableNamedKeys()
                   select _t)
                : (new (Module, Guid, string)[] { }));

        #endregion

        #region InfoKey Resolution Checks and Access

        /// <summary>Information Keys for this module</summary>
        public Dictionary<Guid, InfoKey> InfoKeys => _InfoKeys;

        public bool CanResolveInfoKey(Guid infoKeyID)
            // local resolution
            => InfoKeys.TryGetValue(infoKeyID, out var _iKey)
            // module imports
            || ((CurrentUse == ModuleUse.Open)
                && Resources.Imports.Modules.Any(_imp => _imp.Module.CanResolveInfoKey(infoKeyID)));

        public (Module module, InfoKey infoKey) GetInfoKey(Guid infoKeyID)
            // local resolution
            => InfoKeys.TryGetValue(infoKeyID, out var _iKey)
            ? (this, _iKey)
            // module imports
            : CurrentUse == ModuleUse.Open
            ? (from _imp in Resources.Imports.Modules
               let _iInfo = _imp.Module.GetInfoKey(infoKeyID)
               where _iInfo.module != null
               select _iInfo).FirstOrDefault()
            // nothing
            : default;

        public IEnumerable<(Module module, InfoKey infoKey)> GetResolvableInfoKeys()
            // list direct module info keys
            => InfoKeys.Values.OfType<InfoKey>()
            .Select(_ik => (this, _ik))
            // if open, include imported
            .Concat((CurrentUse == ModuleUse.Open)
                ? (from _ref in Resources.Imports.Modules
                   from _t in _ref.Module.GetResolvableInfoKeys()
                   select _t)
                : (new (Module, InfoKey)[] { }));

        #endregion

        #region ItemElement Resolution and Access

        /// <summary>Items for this module</summary>
        public Dictionary<Guid, ItemElement> ItemElements => _ItemElements;

        public bool CanResolveItemElement(Guid itemID)
            // local resolution
            => ItemElements.TryGetValue(itemID, out var _item)
            // module imports
            || ((CurrentUse == ModuleUse.Open)
                && Resources.Imports.Modules.Any(_imp => _imp.Module.CanResolveItemElement(itemID)));

        public (Module module, ItemElement item) GetItemElement(Guid itemID)
            // local resolution
            => ItemElements.TryGetValue(itemID, out var _item)
            ? (this, _item)
            // module imports
            : CurrentUse == ModuleUse.Open
            ? (from _imp in Resources.Imports.Modules
               let _iItem = _imp.Module.GetItemElement(itemID)
               where _iItem.module != null
               select _iItem).FirstOrDefault()
            // nothing
            : default;

        public IEnumerable<(Module module, ItemElement item)> GetResolvableItemElements()
            // list direct module items
            => ItemElements.Values.OfType<ItemElement>()
            .Select(_ie => (this, _ie))
            // if open, include imported
            .Concat((CurrentUse == ModuleUse.Open)
                ? (from _ref in Resources.Imports.Modules
                   from _t in _ref.Module.GetResolvableItemElements()
                   select _t)
                : (new (Module, ItemElement)[] { }));

        #endregion

        #region Variable Resolution Checks and Access

        /// <summary>Variables for this module</summary>
        public Dictionary<Guid, Variable> Variables => _Variables;

        public bool CanResolveVariable(Guid variableID)
            // local resolution
            => Variables.TryGetValue(variableID, out var _state)
            // module imports
            || ((CurrentUse == ModuleUse.Open)
                && Resources.Imports.Modules.Any(_imp => _imp.Module.CanResolveVariable(variableID)));

        public (Module module, Variable variable) GetVariable(Guid variableID)
            // local resolution
            => Variables.TryGetValue(variableID, out var _state)
            ? (this, _state)
            // module imports
            : CurrentUse == ModuleUse.Open
            ? (from _imp in Resources.Imports.Modules
               let _mss = _imp.Module.GetVariable(variableID)
               where _mss.module != null
               select _mss).FirstOrDefault()
            // nothing
            : (null, null);

        public IEnumerable<(Module module, Variable variable)> GetResolvableVariables()
            // list direct module Variables
            => Variables.Values.OfType<Variable>().Select(_ss => (this, _ss))
            // if open, include imported
            .Union(CurrentUse == ModuleUse.Open
                ? from _ref in Resources.Imports.Modules
                  from _t in _ref.Module.GetResolvableVariables()
                  select _t
                : new (Module module, Variable variable)[] { });

        #endregion

        #region TeamGroupSummary Resolution

        /// <summary>Summary information for teams available in this module, and some disposition information to other teams</summary>
        public Dictionary<Guid, TeamGroupSummary> Teams => _Teams;

        public TeamGroupSummary GetTeamGroupSummary(string teamName)
            // look at direct module teams
            => Teams.Values
            .OfType<TeamGroupSummary>()
            .Where(_tsg => teamName.Equals(_tsg.TeamName, StringComparison.OrdinalIgnoreCase))
            .Select(_tsg => _tsg)
            .FirstOrDefault()
            // fallback to first import module that has the team (when open)
            ?? (CurrentUse == ModuleUse.Open
                ? (from _ref in Resources.Imports.Modules
                   select _ref.Module.GetTeamGroupSummary(teamName)).FirstOrDefault()
                : null);

        public TeamGroupSummary GetTeamGroupSummary(Guid teamID)
            // look at direct module teams
            => Teams.TryGetValue(teamID, out var _team)
            ? _team
            // fallback to first improt module that has the team (when open)
            : CurrentUse == ModuleUse.Open
            ? (from _imp in Resources.Imports.Modules
               select _imp.Module.GetTeamGroupSummary(teamID)).FirstOrDefault()
            : null;

        public IEnumerable<TeamGroupSummary> GetResolvableTeamGroups()
            // list direct module teams
            => Teams.Values.OfType<TeamGroupSummary>()
            // if open, include imported
            .Union(CurrentUse == ModuleUse.Open
                ? from _ref in Resources.Imports.Modules
                  from _t in _ref.Module.GetResolvableTeamGroups()
                  select _t
                : new TeamGroupSummary[] { });

        #endregion
    }
}
