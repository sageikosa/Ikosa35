using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using Newtonsoft.Json;

namespace Uzi.Packaging
{
    public class CorePackage : ICorePart, INotifyPropertyChanged, ICorePartNameManager
    {
        #region construction
        public CorePackage(FileInfo fileInfo, Package package)
        {
            _FileInfo = fileInfo;
            _Package = package;
            _AllParts = new List<IBasePart>(_Package.GetRelationships().RelatedBaseParts(this).ToList());
        }

        public CorePackage()
        {
            _FileInfo = null;
            _Package = null;
            _AllParts = [];
        }
        #endregion

        #region data
        private List<IBasePart> _AllParts;
        private FileInfo _FileInfo;
        private Package _Package;
        #endregion

        // ICorePart Members
        public string Name => _FileInfo?.Name ?? @"-- No Name (yet) --";
        public IEnumerable<ICorePart> Relationships => _AllParts.Select(_bp => _bp as ICorePart);
        public string TypeName => GetType().FullName;

        // ICorePartNameManager Members
        public bool CanUseName(string name, Type partType)
            => !_AllParts.Any(_i => _i.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && _i.GetType().Equals(partType));

        public void Rename(string oldName, string newName, Type partType)
        {
            // NOTE: parts not indexed by name
        }

        // INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Call to inform listeners that the relationships may have changed</summary>
        protected void ContentsChanged()
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Relationships)));

        #region public void Close()
        public void Close()
        {
            if (_Package != null)
            {
                _Package.Close();
                _Package = null;
                if ((_AllParts != null) && _AllParts.Any())
                {
                    foreach (var _part in _AllParts)
                    {
                        _part.Close();
                    }
                }
            }
            _AllParts = null;
        }
        #endregion

        #region public void Save()
        public void Save()
        {
            if (_Package != null)
            {
                // create "temp" file
                var _id = Guid.NewGuid().ToString().Replace(@"{", string.Empty).Replace(@"}", string.Empty);
                var _fileName = string.Concat(FileName, @".", _id);
                var _savePack = DoSaveAs(_fileName);
                _savePack.Close();

                // close our file
                _Package.Close();

                // remove old backups
                var _bakFileName = string.Concat(FileName, @".bak");
                File.Delete(_bakFileName);

                // shuffle files
                try
                {
                    File.Move(FileName, _bakFileName);
                    File.Move(_fileName, FileName);
                }
                catch (IOException)
                {
                    // open new package
                    _FileInfo = new FileInfo(_fileName);
                    if (PropertyChanged != null)
                    {
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileName)));
                    }
                    throw;
                }
                finally
                {
                    // open new package
                    _Package = Package.Open(FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                    _Package.GetRelationships().RefreshParts(this);
                    ContentsChanged();
                }
            }
        }
        #endregion

        #region public void SaveAs(string fileName)
        public void SaveAs(string fileName)
        {
            // create new package
            var _savePack = DoSaveAs(fileName);

            // close old package
            if (_Package != null)
            {
                _Package.Close();
            }

            // and swap...
            _Package = _savePack;
            _Package.GetRelationships().RefreshParts(this);

            _FileInfo = new FileInfo(fileName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileName)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Relationships)));
            }
        }
        #endregion

        private Package DoSaveAs(string fileName)
        {
            var _newPackage = Package.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            foreach (var _part in _AllParts)
            {
                _part.Save(_newPackage);
            }
            _newPackage.Close();
            return Package.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public Package Package => _Package;

        #region public void Add(IBasePart part)
        public void Add(IBasePart part)
        {
            if ((part != null) && !_AllParts.Contains(part) && !(part is CorePackage) && CanUseName(part.Name, part.GetType()))
            {
                _AllParts.Add(part);
                part.NameManager = this;
                ContentsChanged();
            }
        }
        #endregion

        #region public void Remove(IBasePart part)
        public void Remove(IBasePart part)
        {
            if ((part != null) && _AllParts.Contains(part))
            {
                _AllParts.Remove(part);
                ContentsChanged();
            }
        }
        #endregion

        public FileInfo FileInfo => _FileInfo;

        public string FileName => _FileInfo?.FullName ?? @"-- No File (yet) --";
    }
}
