using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Ikosa.Packaging
{
    public abstract class IkosaPackage : IRetrievablePartNameManager, INotifyPropertyChanged
    {
        #region construction
        protected IkosaPackage(FileInfo fileInfo, ZipArchive zipArchive)
        {
            _FileInfo = fileInfo;
            _Archive = zipArchive;
            _Root = GetStorablePart();
        }

        protected IkosaPackage()
        {
            _FileInfo = null;
            _Archive = null;
            _Root = CreateStorablePart();
        }
        #endregion

        protected abstract IStorablePart GetStorablePart();
        protected abstract IStorablePart CreateStorablePart();

        #region state
        protected IStorablePart _Root;
        private FileInfo _FileInfo;
        private ZipArchive _Archive;
        #endregion

        public const string RegisteredPartType = @"Ikosa.Packaging.IkosaPackage";

        public FileInfo FileInfo => _FileInfo;
        public ZipArchive Archive => _Archive;

        public string FileName => _FileInfo?.FullName ?? @"-- No File (yet) --";

        #region INotifyPropertyChanged Members

        /// <summary>Call to inform listeners that the relationships may have changed</summary>
        protected void ContentsChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Parts)));
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        // IRetrievablePart Members
        public string PartName => _FileInfo?.Name ?? @"-- No Name (yet) --";
        public string PartType => RegisteredPartType;

        public IEnumerable<IRetrievablePart> Parts => new[] { _Root };

        // IRetrievablePartNameManager Members
        public bool CanUseName(string name) => false;

        public void Rename(string oldName, string newName)
        {
            // NOTE: parts not "indexed" by name
        }

        #region public void Close()
        public void Close()
        {
            _Archive?.Dispose();
            _Archive = null;
            _Root?.ClosePart();
            _Root = null;
        }
        #endregion

        #region public void Save()
        public void Save()
        {
            if (_Archive != null)
            {
                // create "temp" file
                var _id = Guid.NewGuid().ToString().Replace(@"{", string.Empty).Replace(@"}", string.Empty);
                var _fileName = string.Concat(FileName, @".", _id);
                using (var _savePack = DoSaveAs(_fileName))
                {
                    // immediately closing
                }

                // close current archive
                _Archive.Dispose();

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
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(PartName)));
                        PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileName)));
                    }
                    throw;
                }
                finally
                {
                    // open new package
                    _Archive = ZipFile.OpenRead(FileName);
                    _Root = GetStorablePart();
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
            _Archive?.Dispose();
            _Root?.ClosePart();

            // and swap...
            _Archive = _savePack;
            _Root = GetStorablePart();
            _FileInfo = new FileInfo(fileName);
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(PartName)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FileName)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Parts)));
            }
        }
        #endregion

        private ZipArchive DoSaveAs(string fileName)
        {
            using (var _newPackage = ZipFile.Open(fileName, ZipArchiveMode.Create))
            {
                _Root.StorePart(_newPackage, string.Empty);
            }
            return ZipFile.OpenRead(fileName);
        }
    }
}
