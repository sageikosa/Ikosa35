using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Uzi.Packaging
{
    public class CorePackagePartsFolder : BasePart, ICorePartNameManager
    {
        /// <summary>Relationship type to identify a CorePackagePartsFolder (http://pack.guildsmanship.com/packageparts)</summary>
        public const string CorePackagePartsFolderRelation = @"http://pack.guildsmanship.com/packageparts";

        #region construction
        public CorePackagePartsFolder(ICorePartNameManager manager, PackagePart part, string name, bool invisible)
            : base(manager, part, name)
        {
            _Invisible = invisible;
            _IBaseParts = part.GetRelationships().RelatedBaseParts(ChildNameManager).ToList();
        }

        public CorePackagePartsFolder(ICorePartNameManager manager, string name, bool invisible)
            : base(manager, name)
        {
            _Invisible = invisible;
            _IBaseParts = [];
        }
        #endregion

        #region data
        private readonly List<IBasePart> _IBaseParts;
        private readonly bool _Invisible;
        #endregion

        // TODO: detect naming collisions?

        private ICorePartNameManager ChildNameManager => _Invisible ? NameManager : this;

        #region public void Add(IBasePart part)
        public void Add(IBasePart part)
        {
            if ((part != null) && !_IBaseParts.Contains(part) && !(part is CorePackage) && CanUseName(part.Name, part.GetType()))
            {
                _IBaseParts.Add(part);
                part.NameManager = ChildNameManager;
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        #region public void Remove(IBasePart part)
        public void Remove(IBasePart part)
        {
            if ((part != null) && _IBaseParts.Contains(part))
            {
                _IBaseParts.Remove(part);
                DoPropertyChanged(@"Relationships");
            }
        }
        #endregion

        // ICorePart Members
        public override IEnumerable<ICorePart> Relationships => _IBaseParts.Select(_bp => _bp as ICorePart);
        public override string TypeName => GetType().FullName;

        // ICorePartNameManager Members
        public bool CanUseName(string name, Type partType)
        {
            var _name = name.ToSafeString();
            return !_IBaseParts.Any(_i => _i.Name.Equals(_name, StringComparison.OrdinalIgnoreCase));
        }

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: parts not indexed by name
        }

        #region Saving
        public override void Save(Package parent)
        {
            var _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            var _target = UriHelper.ConcatRelative(_base, @"part.folder");
            _Part = parent.CreatePart(_target, @"text/plain", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, CorePackagePartsFolderRelation, Name);

            // save contents to part
            foreach (var _item in _IBaseParts)
            {
                _item.Save(_Part, _base);
            }
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            var _base = UriHelper.ConcatRelative(baseUri, Name);
            var _target = UriHelper.ConcatRelative(_base, @"part.folder");
            _Part = parent.Package.CreatePart(_target, @"text/plain", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, CorePackagePartsFolderRelation, Name);

            // save contents to part
            foreach (var _item in _IBaseParts)
            {
                _item.Save(_Part, _base);
            }
        }
        #endregion

        protected override void OnRefreshPart()
        {
            if (Part != null)
            {
                foreach (var _part in _Part.GetRelationships().RelatedBaseParts(ChildNameManager))
                {
                    var _exist = _IBaseParts
                        .FirstOrDefault(_p => _p.Name.Equals(_part.Name, StringComparison.OrdinalIgnoreCase)
                        && _p.GetType().Equals(_part.GetType()));
                    if (_exist != null)
                    {
                        _exist.RefreshPart(_part.Part);
                    }
                    else
                    {
                        _IBaseParts.Add(_part);
                    }
                }
            }
        }

        public override void Close()
        {
            foreach (var _part in _IBaseParts)
            {
                _part.Close();
            }
        }
    }
}
