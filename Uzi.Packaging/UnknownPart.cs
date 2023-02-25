using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.IO;

namespace Uzi.Packaging
{
    /// <summary>
    /// Used when a BasePart doesn't have a registered factory.  
    /// Attempts to preserve stream and related items when saving the package.
    /// </summary>
    public class UnknownPart : BasePart, ICorePartNameManager
    {
        public UnknownPart(ICorePartNameManager manager, PackagePart part, string name, string relationType)
            : base(manager, part, name)
        {
            _RelationType = relationType;
        }

        private readonly string _RelationType;

        /// <summary>UnknownPart is opaque, even if the related parts are known, because the management of the parts is unknown</summary>
        public override IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();

        /// <summary>Rather than a CLR type (which wasn't resolvable), provide the relation type.</summary>
        public override string TypeName => _RelationType;

        protected override void OnRefreshPart() { }

        #region public override void Save(Package parent)
        public override void Save(Package parent)
        {
            // only need to copy streams and relations if saving somewhere else
            using var _oldStream = Part.GetStream(FileMode.Open, FileAccess.Read);
            if (_oldStream != null)
            {
                // new part should be pretty much like the last part
                var _newPart = parent.CreatePart(Part.Uri, Part.ContentType, Part.CompressionOption);
                parent.CreateRelationship(Part.Uri, TargetMode.Internal, _RelationType, Name);
                using (Stream _newStream = _newPart.GetStream(FileMode.Create, FileAccess.ReadWrite))
                {
                    // copy stream
                    StreamHelper.CopyStream(_oldStream, _newStream);
                }

                // copy relationships (iteratively)
                foreach (var _relation in Part.GetRelationships())
                {
                    // NOTE: raw stream and relationship copy...some containers add extra URI parts we wouldn't catch otherwise
                    var _relPart = _relation.Package.GetPart(_relation.TargetUri);
                    Save(_newPart, _relPart, _relation.TargetUri);
                }

                // done
                _Part = _newPart;
            }
        }
        #endregion

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // use iterative copy method
            _Part = Save(parent, _Part, baseUri);
        }

        public override void Close()
        {
            // NOTE: no resources that need to be closed
        }

        #region private PackagePart Save(PackagePart newParent, PackagePart origPart, Uri baseUri)
        private PackagePart Save(PackagePart newParent, PackagePart origPart, Uri baseUri)
        {
            using (var _oldStream = origPart.GetStream(FileMode.Open, FileAccess.Read))
            {
                if (_oldStream != null)
                {
                    // new part should be pretty much like the last part
                    var _newPart = newParent.Package.CreatePart(origPart.Uri, origPart.ContentType, origPart.CompressionOption);
                    newParent.CreateRelationship(origPart.Uri, TargetMode.Internal, _RelationType, Name);
                    using (Stream _newStream = _newPart.GetStream(FileMode.Create, FileAccess.ReadWrite))
                    {
                        // copy stream
                        StreamHelper.CopyStream(_oldStream, _newStream);
                    }

                    // copy relationships (iteratively)
                    foreach (var _relation in origPart.GetRelationships())
                    {
                        // NOTE: raw stream and relationship copy...some containers add extra URI parts we wouldn't catch otherwise
                        var _relPart = _relation.Package.GetPart(_relation.TargetUri);
                        Save(_newPart, _relPart, _relation.TargetUri);
                    }

                    // done
                    return _newPart;
                }
            }
            return null;
        }
        #endregion

        #region ICorePartNameManager Members

        public bool CanUseName(string name, Type partType)
        {
            return false;
        }

        public void Rename(string oldName, string newName, Type partType)
        {
        }

        #endregion
    }
}