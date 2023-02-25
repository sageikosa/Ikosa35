using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Uzi.Packaging;
using System.Linq;

namespace Uzi.Core
{
    /// <summary>
    /// ObjectBase embedded directly in a package as a stream
    /// </summary>
    public class CoreObjectPart : BasePart
    {
        /// <summary>Relationship type to identify an object</summary>
        public const string CoreObjectRelation = @"http://pack.guildsmanship.com/coreobject";

        #region construction
        /// <summary>Create from PackagePart that represents the ObjectPart in serialized form</summary>
        public CoreObjectPart(ICorePartNameManager manager, PackagePart part, string id) :
            base(manager, part, id)
        {
            _CoreObject = null;
        }

        /// <summary>Creates a new object part</summary>
        public CoreObjectPart(ICorePartNameManager manager, CoreObject coreObject)
            : base(manager, coreObject.ID.ToString())
        {
            _CoreObject = coreObject;
        }
        #endregion

        #region state
        private CoreObject _CoreObject;
        #endregion

        #region public CoreObject CoreObject { get; }
        public CoreObject CoreObject
        {
            get
            {
                if (_CoreObject == null)
                {
                    ResolveObject();
                }
                return _CoreObject;
            }
        }

        private void ResolveObject()
        {
            if (Part != null)
            {
                // deserialize object from part
                using var _partStream = Part.GetStream(FileMode.Open, FileAccess.Read);
                IFormatter _fmt = new BinaryFormatter();
                _CoreObject = (CoreObject)_fmt.Deserialize(_partStream);
            }
        }
        #endregion

        #region ICorePart Members

        public override IEnumerable<ICorePart> Relationships => Enumerable.Empty<ICorePart>();
        public override string TypeName => GetType().FullName;

        #endregion

        public override void Save(Package parent)
        {
            if (_CoreObject == null)
                ResolveObject();

            var _target = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            _Part = parent.CreatePart(_target, @"ikosa/object", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, CoreObjectRelation, Name);

            DoSave();
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            if (_CoreObject == null)
                ResolveObject();

            var _target = UriHelper.ConcatRelative(baseUri, Name);
            _Part = parent.Package.CreatePart(_target, @"ikosa/object", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, CoreObjectRelation, Name);

            DoSave();
        }

        private void DoSave()
        {
            using var _objStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite);
            IFormatter _fmt = new BinaryFormatter();
            _fmt.Serialize(_objStream, _CoreObject);
        }

        protected override void OnRefreshPart() { }

        public override void Close()
        {
            // NOTE: no open resources
        }
    }
}
