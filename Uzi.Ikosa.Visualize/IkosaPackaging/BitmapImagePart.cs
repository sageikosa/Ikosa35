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
using Ikosa.Packaging;
using Newtonsoft.Json;

namespace Uzi.Visualize.IkosaPackaging
{
    /// <summary>
    /// Represents an image loaded from a related package part
    /// </summary>
    public class BitmapImagePart : BasePart, IGetImageByEffect
    {
        /// <summary>Relationship type to identify images (http://pack.guildsmanship.com/visualize/image)</summary>
        public const string ImageRelation = @"http://pack.guildsmanship.com/visualize/image";

        #region construction
        public BitmapImagePart(ICorePartNameManager manager, MemoryStream memStream, string id)
            : base(manager, id)
        {
            _Images = null;
            _MemStream = memStream;
            ResolveImages();
        }

        public BitmapImagePart(ICorePartNameManager manager, BitmapImagePart source, string id)
            : base(manager, id)
        {
            _Images = null;
            _MemStream = new MemoryStream((byte[])source.StreamBytes.Clone()); ;
            ResolveImages();
        }

        public BitmapImagePart(ICorePartNameManager manager, PackagePart part, string id)
            : base(manager, part, id)
        {
            _Images = null;
            _MemStream = null;
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

        #region private data
        private MemoryStream _MemStream;
        private Dictionary<VisualEffect, BitmapSource> _Images;
        [NonSerialized, JsonIgnore]
        private static BitmapImage _Black = null;
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

        #region private void ResolveStream()
        private void ResolveStream()
        {
            if ((Part != null) && (_MemStream == null))
            {
                lock (Part)
                {
                    if (_MemStream == null)
                    {
                        using (var _partStream = Part.GetStream(FileMode.Open, FileAccess.Read))
                        {
                            _MemStream = new MemoryStream((int)_partStream.Length);
                            StreamHelper.CopyStream(_partStream, _MemStream);
                        }
                    }
                }
            }

            if (_MemStream != null)
                _MemStream.Seek(0, SeekOrigin.Begin);
        }
        #endregion

        #region public static IEnumerable<BitmapImagePart> GetImageResources(ICorePartNameManager manager, PackagePart part)
        /// <summary>Pre-loads images from related package parts for IResolveImage</summary>
        /// <param name="part">part with (possible) image relations</param>
        public static IEnumerable<BitmapImagePart> GetImageResources(ICorePartNameManager manager, PackagePart part)
        {
            foreach (var _imgRel in part.GetRelationshipsByType(BitmapImagePart.ImageRelation))
            {
                var _imgPart = part.Package.GetPart(_imgRel.TargetUri);
                yield return new BitmapImagePart(manager, _imgPart, _imgRel.Id);
            }
        }
        #endregion

        public override IEnumerable<ICorePart> Parts { get { yield break; } }
        public override string PartType => GetType().FullName;

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region public override void Save(Package parent)
        public override void Save(Package parent)
        {
            if (_Images == null)
            {
                ResolveImages();
            }

            var _target = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            _Part = parent.CreatePart(_target, @"visualize/image", CompressionOption.NotCompressed);
            parent.CreateRelationship(_target, TargetMode.Internal, ImageRelation, Name);

            DoSave();
        }
        #endregion

        #region public override void Save(PackagePart parent, Uri baseUri)
        public override void Save(PackagePart parent, Uri baseUri)
        {
            if (_Images == null)
            {
                ResolveImages();
            }

            var _target = UriHelper.ConcatRelative(baseUri, Name);
            _Part = parent.Package.CreatePart(_target, @"visualize/image", CompressionOption.NotCompressed);
            parent.CreateRelationship(_target, TargetMode.Internal, ImageRelation, Name);

            DoSave();
        }
        #endregion

        #region private void DoSave()
        private void DoSave()
        {
            // save image
            using (var _imgStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite))
            {
                if (_MemStream != null)
                {
                    _MemStream.Seek(0, SeekOrigin.Begin);
                    StreamHelper.CopyStream(_MemStream, _imgStream);
                }
            }
        }
        #endregion

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

        protected override void OnRefreshPart() { }

        public override void Close()
        {
            // NOTE: no open resources
        }
    }

    public class BitmapImagePartListItem
    {
        public BitmapImagePart BitmapImagePart { get; set; }
        public bool IsLocal { get; set; }
    }
}
