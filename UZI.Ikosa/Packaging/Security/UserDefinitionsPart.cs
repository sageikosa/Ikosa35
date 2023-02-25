using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Windows.Markup;
using System.IO;
using Uzi.Packaging;

namespace Uzi.Ikosa.Services
{
    public class UserDefinitionsPart : BasePart
    {
        /// <summary>Relationship type to identify user definitions (http://pack.guildsmanship.com/ikosa/services/userdefinitions)</summary>
        public const string UserDefinitionsRelation = @"http://pack.guildsmanship.com/ikosa/services/userdefinitions";

        #region construction
        public UserDefinitionsPart(ICorePartNameManager nameManager, string name)
            : base(nameManager, name)
        {
            _Users = new UserDefinitionCollection();
            UserValidator.UserDefinitions ??= _Users;
        }

        public UserDefinitionsPart(ICorePartNameManager nameManager, PackagePart part, string name) :
            base(nameManager, part, name)
        {
            _Users = null;
            ResolveCollection();
            UserValidator.UserDefinitions ??= _Users;
        }
        #endregion

        #region private data
        private UserDefinitionCollection _Users;
        #endregion

        public override IEnumerable<ICorePart> Relationships { get { yield break; } }
        public override string TypeName => GetType().FullName;

        #region public UserDefinitionCollection UserDefinitions { get; }
        public UserDefinitionCollection UserDefinitions
        {
            get
            {
                if (_Users == null)
                {
                    ResolveCollection();
                    UserValidator.UserDefinitions ??= _Users;
                }
                return _Users;
            }
        }

        private void ResolveCollection()
        {
            if (Part != null)
            {
                try
                {
                    // Load material collection
                    using var _mStream = Part.GetStream(FileMode.Open, FileAccess.Read);
                    _Users = XamlReader.Load(_mStream) as UserDefinitionCollection;
                }
                finally
                {
                }
            }
            else
            {
                _Users = new UserDefinitionCollection();
            }
        }
        #endregion

        #region Saving
        public override void Save(Package parent)
        {
            // resolve materials before changing part
            var _users = UserDefinitions;

            var _base = UriHelper.ConcatRelative(new Uri(@"/", UriKind.Relative), Name);
            var _target = UriHelper.ConcatRelative(_base, @"users.xaml");
            _Part = parent.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, UserDefinitionsRelation, Name);

            DoSave(_users, _base);
        }

        public override void Save(PackagePart parent, Uri baseUri)
        {
            // resolve materials before changing part
            var _users = UserDefinitions;

            var _base = UriHelper.ConcatRelative(baseUri, Name);
            var _target = UriHelper.ConcatRelative(_base, Name);
            _Part = parent.Package.CreatePart(_target, @"text/xaml+xml", CompressionOption.Normal);
            parent.CreateRelationship(_target, TargetMode.Internal, UserDefinitionsRelation, Name);

            DoSave(_users, _base);
        }

        private void DoSave(UserDefinitionCollection users, Uri baseUri)
        {
            // save xaml
            using var _userStream = _Part.GetStream(FileMode.Create, FileAccess.ReadWrite);
            using var _writer = new StreamWriter(_userStream);
            XamlWriter.Save(users, _writer);
        }
        #endregion

        protected override void OnRefreshPart() { }

        public override void Close()
        {
            if (UserValidator.UserDefinitions == _Users)
                UserValidator.UserDefinitions = null;
        }
    }
}
