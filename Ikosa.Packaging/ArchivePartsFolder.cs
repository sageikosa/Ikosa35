using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ikosa.Packaging
{
    public class ArchivePartsFolder : StorablePart, IRetrievablePartNameManager
    {
        public ArchivePartsFolder(IRetrievablePartNameManager manager, string name, bool invisible,
            Func<IRetrievablePartNameManager, string, ZipArchive, string, IStorablePart> partGenerator)
            : base(manager, name)
        {
            _Invisible = invisible;
            _Parts = new List<IStorablePart>();
            _Generator = partGenerator;
        }

        #region state
        private readonly List<IStorablePart> _Parts;
        private readonly bool _Invisible;
        private readonly Func<IRetrievablePartNameManager, string, ZipArchive, string, IStorablePart> _Generator;
        #endregion

        private IRetrievablePartNameManager ChildNameManager => _Invisible ? PartNameManager : this;

        #region public void Add(IStorablePart part)
        public void Add(IStorablePart part)
        {
            if ((part != null)
                && !_Parts.Contains(part)
                && !(part is IkosaPackage)
                && CanUseName(part.PartName))
            {
                _Parts.Add(part);
                part.PartNameManager = ChildNameManager;
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        #region public void Remove(IStorablePart part)
        public void Remove(IStorablePart part)
        {
            if ((part != null) && _Parts.Contains(part))
            {
                _Parts.Remove(part);
                DoPropertyChanged(nameof(Parts));
            }
        }
        #endregion

        // IRetrievablePart Members
        public override IEnumerable<IRetrievablePart> Parts => _Parts.Select(_bp => _bp as IRetrievablePart);
        public override string PartType => typeof(ArchivePartsFolder).FullName;
        public int Count => _Parts?.Count ?? 0;

        // IRetrievablePartNameManager Members
        public bool CanUseName(string name)
        {
            var _name = name.ToSafeString();
            return !_Parts.Any(_i => _i.PartName.Equals(_name, StringComparison.OrdinalIgnoreCase));
        }

        public void Rename(string oldName, string newName)
        {
            // NOTE: parts not indexed by name
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            var _folderPath = $@"{parentPath}/{PartName}";
            foreach (var _child in _Parts)
            {
                _child.StorePart(archive, _folderPath);
            }
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            var _folderPath = $@"{parentPath}/{PartName}";
            var _childPartCount = _folderPath.Split('/').Length;
            foreach (var _childName in from _e in archive.Entries
                                       where _e.FullName.StartsWith(_folderPath)
                                       let _split = _e.FullName.Split('/')
                                       where _split.Length == _childPartCount
                                       select _split.Last())
            {
                var _child = _Generator?.Invoke(this, _childName, archive, _folderPath);
                _child?.ReloadPart(archive, _folderPath);
            }
        }

        public override void ClosePart()
        {
            foreach (var _part in _Parts)
                _part.ClosePart();
        }
    }
}
