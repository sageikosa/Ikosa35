using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Ikosa.Guildsmanship.Overland;
using Uzi.Packaging;

namespace Uzi.Ikosa.Guildsmanship
{
    public class ModuleResources : BasePart, ICorePartNameManager, IHideCorePackagePartsFolder
    {
        protected readonly ModuleImports _Imports;

        // storage state
        private readonly CorePackagePartsFolder _Components;
        // presentation state
        private readonly PartsFolder _ComponentsFolder;

        /// <summary>Relationship type to identify ModuleResources (http://pack.guildsmanship.com/moduleresources)</summary>
        public const string ModuleResourcesRelation = @"http://pack.guildsmanship.com/moduleresources";

        #region ctor()
        public ModuleResources(ICorePartNameManager manager, PackagePart part, string name)
            : base(manager, part, name)
        {
            // Load module-references
            using (var _rStream = Part.GetStream(FileMode.Open, FileAccess.Read))
            {
                IFormatter _fmt = new BinaryFormatter();
                _Imports = (ModuleImports)_fmt.Deserialize(_rStream);
            }

            CorePackagePartsFolder _getFolder(string relID)
            {
                if (part.RelationshipExists(relID))
                {
                    var _folderRel = part.GetRelationship(relID);
                    return this.GetBasePart(_folderRel) as CorePackagePartsFolder
                        ?? new CorePackagePartsFolder(this, relID, true);
                }
                return new CorePackagePartsFolder(this, relID, true);
            }

            _Components = _getFolder(@"Components");

            // connect folders
            _ComponentsFolder = new PartsFolder(this, @"Components", _Components.Relationships, typeof(ModuleNode))
            { HideTree = true };
        }

        public ModuleResources(ICorePartNameManager manager, string name)
            : base(manager, name)
        {
            _Imports = new ModuleImports();
            _Components = new CorePackagePartsFolder(this, @"Components", true);

            // connect folders
            _ComponentsFolder = new PartsFolder(this, @"Components", _Components.Relationships, typeof(ModuleNode))
            { HideTree = true };
        }
        #endregion

        public override string TypeName => typeof(ModuleResources).FullName;

        public Module Module => NameManager as Module;
        public ModuleImports Imports => _Imports;

        #region public override IEnumerable<ICorePart> Relationships
        public override IEnumerable<ICorePart> Relationships
        {
            get
            {
                yield return _ComponentsFolder;
                yield return _Imports;
                yield break;
            }
        }
        #endregion

        #region public override void Save(Package parent)
        public override void Save(Package parent)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), $@"{Name}.moduleresources");
            var _content = UriHelper.ConcatRelative(_folder, @"external.moduleimports");
            _Part = parent.CreatePart(_content, @"ikosa/moduleimports", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, ModuleResourcesRelation, Name);

            DoSave(_folder);
        }
        #endregion

        #region public override void Save(PackagePart parent, Uri baseUri)
        public override void Save(PackagePart parent, Uri baseUri)
        {
            // create new part
            var _folder = UriHelper.ConcatRelative(baseUri, $@"{Name}.moduleresources");
            var _content = UriHelper.ConcatRelative(_folder, @"external.moduleimports");
            _Part = parent.Package.CreatePart(_content, @"ikosa/moduleimports", CompressionOption.Normal);
            parent.CreateRelationship(_content, TargetMode.Internal, ModuleResourcesRelation, Name);

            DoSave(_folder);
        }
        #endregion

        #region private void DoSave(Uri baseUri)
        private void DoSave(Uri baseUri)
        {
            // references
            using (var _refStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                IFormatter _fmt = new BinaryFormatter();
                _fmt.Serialize(_refStream, _Imports);
            }

            _Components.Save(_Part, baseUri);
        }
        #endregion

        #region protected override void OnRefreshPart()
        protected override void OnRefreshPart()
        {
            if (Part != null)
            {
                // sites
                var _siteRel = Part.GetRelationship(@"Components");
                _Components.RefreshPart(Part.Package.GetPart(_siteRel.TargetUri));

            }
        }
        #endregion

        public override void Close()
        {
            _Imports?.Close();
            _Components?.Close();
        }

        // IHideCorePackagePartsFolder
        public bool ShouldHide(string id)
            => true;

        #region ICorePartNameManager Members

        public bool CanUseName(string name, Type partType)
        {
            if (typeof(ModuleNode).IsAssignableFrom(partType))
            {
                return _Components.CanUseName(name, partType);
            }

            // unknown type
            return false;
        }

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: not indexed by name
            //if (typeof(BitmapImagePart).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(BrushCollectionPart).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(MetaModelFragment).IsAssignableFrom(partType))
            //{
            //}
            //else if (typeof(Model3DPart).IsAssignableFrom(partType))
            //{
            //}
        }

        #endregion

        public void AddPart(ModuleNode moduleNode)
        {
            _Components.Add(moduleNode);
            _ComponentsFolder.ContentsChanged();
        }

        // TODO: module-node resolution and part control

        public INode GetNode<INode>(Guid id)
            where INode : class, IModuleNode
            => _Components.Relationships.OfType<INode>().FirstOrDefault(_s => _s.ID == id);
    }
}
