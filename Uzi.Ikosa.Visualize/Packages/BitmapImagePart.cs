using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.IO.Packaging;
using System.IO;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Forms;
using Uzi.Visualize;
using Newtonsoft.Json;
using Ikosa.Packaging;
using System.IO.Compression;

namespace Uzi.Visualize.Packages
{
    /// <summary>
    /// Represents an image loaded from a related package part
    /// </summary>
    public class BitmapImagePart : StorablePart, IGetImageByEffect
    {
        #region construction
        public BitmapImagePart(IRetrievablePartNameManager manager, string id)
            : base(manager, id)
        {
            _Entry = null;
            _Images = null;
            _MemStream = null;
        }

        public BitmapImagePart(IRetrievablePartNameManager manager, MemoryStream memStream, string id)
            : base(manager, id)
        {
            _Entry = null;
            _Images = null;
            _MemStream = memStream;
            ResolveImages();
        }

        public BitmapImagePart(IRetrievablePartNameManager manager, BitmapImagePart source, string id)
            : base(manager, id)
        {
            _Entry = null;
            _Images = null;
            _MemStream = new MemoryStream((byte[])source.StreamBytes.Clone()); ;
            ResolveImages();
        }
        #endregion

        #region static construction
        static BitmapImagePart()
        {
            try
            {
                _Black = new BitmapImage(new Uri(@"pack://application:,,,/Uzi.Visualize;component/images/Black.bmp"));
                _Black.Freeze();
            }
            catch (Exception _ex)
            {
                Debug.WriteLine(_ex);
                MessageBox.Show(_ex.Message, @"Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region state
        private ZipArchiveEntry _Entry;
        private MemoryStream _MemStream;
        private Dictionary<VisualEffect, BitmapSource> _Images;
        [NonSerialized, JsonIgnore]
        private static readonly BitmapImage _Black = null;
        #endregion

        #region public BitmapSource GetImage(VisualEffect effect)
        public BitmapSource GetImage(VisualEffect effect)
        {
            if (_Images.ContainsKey(effect))
            {
                return _Images[effect];
            }
            return _Black;
        }
        #endregion

        #region public BitmapSource Image { get; }
        /// <summary>Gets referenced BitmapImage</summary>
        public BitmapSource Image
        {
            get
            {
                if (_Images == null)
                {
                    ResolveImages();
                }
                return _Images[VisualEffect.Normal];
            }
        }
        #endregion

        #region private void ResolveImages()
        private void ResolveImages()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                ResolveStream();

                // build image from the stream and keep it for XamlReader.Load
                _Images = new Dictionary<VisualEffect, BitmapSource>();
                var _normal = new BitmapImage();
                try
                {
                    _normal.BeginInit();
                    if (_MemStream != null)
                        _normal.StreamSource = _MemStream;
                }
                catch
                {
                    // NOTE: image may blow up
                }
                finally
                {
                    _normal.EndInit();
                }

                // freeze
                _normal.Freeze();
                _Images.Add(VisualEffect.Normal, _normal);
                foreach (var (_effect, _renderTarget) in _normal.RenderImages())
                {
                    _Images.Add(_effect, _renderTarget);
                }
            });
        }
        #endregion

        private Stream GetArchiveStream()
            => _Entry.Open();

        #region private void ResolveStream()
        private void ResolveStream()
        {
            if ((_Entry != null) && (_MemStream == null))
            {
                lock (_Entry)
                {
                    if (_MemStream == null)
                    {
                        using var _partStream = GetArchiveStream();
                        _MemStream = new MemoryStream((int)_partStream.Length);
                        StreamHelper.CopyStream(_partStream, _MemStream);
                    }
                }
            }

            if (_MemStream != null)
                _MemStream.Seek(0, SeekOrigin.Begin);
        }
        #endregion

        public override IEnumerable<IRetrievablePart> Parts => Enumerable.Empty<IRetrievablePart>();
        public override string PartType => GetType().FullName;

        [field: NonSerialized, JsonIgnore]
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #region public byte[] StreamBytes { get; }
        public byte[] StreamBytes
        {
            get
            {
                ResolveStream();
                if (_MemStream != null)
                {
                    return _MemStream.ToArray();
                }
                return new byte[] { };
            }
        }
        #endregion

        public override void ClosePart()
        {
            // NOTE: no open resources
        }

        public override void StorePart(ZipArchive archive, string parentPath)
        {
            _MemStream.Seek(0, SeekOrigin.Begin);
            var _entry = archive.CreateEntry($@"{parentPath}/{PartName}");
            using var _saveStream = _entry.Open();
            StreamHelper.CopyStream(_MemStream, _saveStream);
            _Entry = _entry;
        }

        public override void ReloadPart(ZipArchive archive, string parentPath)
        {
            _Entry = archive.GetEntry($@"{parentPath}/{PartName}");
            ResolveImages();
        }
    }

    public class BitmapImagePartListItem
    {
        public BitmapImagePart BitmapImagePart { get; set; }
        public bool IsLocal { get; set; }
    }
}
