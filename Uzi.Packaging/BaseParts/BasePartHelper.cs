using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.Collections.Concurrent;

namespace Uzi.Packaging
{
    public static class BasePartHelper
    {
        private static readonly ConcurrentDictionary<string, IBasePartFactory> _Factories
            = new ConcurrentDictionary<string, IBasePartFactory>();

        public static Action<string> LoadMessage { get; set; }

        public static void RegisterFactory(IBasePartFactory factory)
        {
            foreach (var _rType in factory.Relationships)
            {
                _Factories.AddOrUpdate(_rType, factory, (name, f) =>
                {
                    return f;
                });
            }
        }

        /// <summary>Provides IBaseParts implied by this relationship collection</summary>
        public static IEnumerable<IBasePart> RelatedBaseParts(this PackageRelationshipCollection relations, ICorePartNameManager manager)
            => relations.Select(_relation => manager.GetBasePart( _relation));

        public static IBasePart GetBasePart(this ICorePartNameManager manager,  PackageRelationship relation)
        {
            LoadMessage?.Invoke($@"Load: {relation.TargetUri.OriginalString}");
            var _part = relation.Package.GetPart(relation.TargetUri);
            if (_Factories.TryGetValue(relation.RelationshipType, out IBasePartFactory _factory))
            {
                return _factory.GetPart( relation.RelationshipType, manager, _part, relation.Id);
            }
            else
            {
                return new UnknownPart( manager, _part, relation.Id, relation.RelationshipType);
            }
        }

        public static void RefreshParts(this PackageRelationshipCollection relations, ICorePartNameManager manager)
        {
            foreach (var _relation in relations)
            {
                manager.RefreshBasePart(_relation);
            }
        }

        public static void RefreshBasePart(this ICorePartNameManager manager, PackageRelationship relation)
        {
            LoadMessage?.Invoke($@"Refresh: {relation.TargetUri.OriginalString}");
            var _part = relation.Package.GetPart(relation.TargetUri);
            Type _type = null;
            if (_Factories.TryGetValue(relation.RelationshipType, out IBasePartFactory _factory))
            {
                _type = _factory.GetPartType(relation.RelationshipType);
            }
            else
            {
                _type = typeof(UnknownPart);
            }
            manager.Relationships.OfType<IBasePart>()
                .FirstOrDefault(_bp => _bp.GetType() == _type && _bp.Name == relation.Id)?
                .RefreshPart(_part);
        }

        #region public static IEnumerable<RelatedPackagePart> RelatedPackageParts(this PackageRelationshipCollection relations, ICorePartNameManager manager)
        /// <summary>Provides IBaseParts implied by this relationship collection</summary>
        public static IEnumerable<RelatedPackagePart> RelatedPackageParts(this PackageRelationshipCollection relations)
        {
            return from _rel in relations
                   let _part = _rel.Package.GetPart(_rel.TargetUri)
                   select new RelatedPackagePart
                   {
                       RelationshipType = _rel.RelationshipType,
                       ID = _rel.Id,
                       Part = _part
                   };
        }
        #endregion

        #region public static IBasePart FindBasePart(this ICorePart source, string internalPath)
        /// <summary>Finds a part given its internal path</summary>
        public static IBasePart FindBasePart(this ICorePart source, string internalPath)
        {
            var _current = source;
            var _pathParts = internalPath.Split('/').Where(_p => !string.IsNullOrEmpty(_p));
            foreach (var _part in _pathParts)
            {
                _current = _current.Relationships.FirstOrDefault(_p => _p.Name.Equals(_part, StringComparison.InvariantCultureIgnoreCase));
                if (_current == null)
                {
                    break;
                }
            }
            return _current as IBasePart;
        }
        #endregion

        #region public static string GetInternalPath(this IBasePart part)
        /// <summary>Climbs name manager tree to build an internal path for this part</summary>
        public static string GetInternalPath(this IBasePart part)
        {
            var _builder = new StringBuilder(string.Format(@"/{0}", part.Name));
            var _mgr = part.NameManager;
            while (_mgr != null)
            {
                if (_mgr is IBasePart _part)
                {
                    _builder.Insert(0, string.Format(@"/{0}", _part.Name));
                    _mgr = _part.NameManager;
                }
                else
                {
                    _mgr = null;
                }
            }
            return _builder.ToString();
        }
        #endregion
    }
}
