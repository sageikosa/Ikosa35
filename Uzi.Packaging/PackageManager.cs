using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Runtime.Serialization;

namespace Uzi.Packaging
{
    public class PackageManager
    {
        private readonly Dictionary<(string packageSet, string packageID), (CorePackage package, Dictionary<string, IBasePart> parts)> _Cache;
        private readonly List<IListPackagePartReferences> _Listers;
        private readonly List<string> _PackagePaths;
        private readonly List<string> _PackageSetPaths;

        private static readonly PackageManager _Manager = new PackageManager();

        private PackageManager()
        {
            _Cache = new Dictionary<(string packageSet, string packageID), (CorePackage package, Dictionary<string, IBasePart> parts)>(new CompareRefKey());
            _Listers = [];
            _PackagePaths = [];
            _PackageSetPaths = [];
        }

        public void SetPaths(List<string> packagePaths, List<string> packageSetPaths)
        {
            _PackagePaths.Clear();
            _PackagePaths.AddRange(packagePaths);

            _PackageSetPaths.Clear();
            _PackageSetPaths.AddRange(packageSetPaths);
        }

        public IEnumerable<string> PackagePaths => _PackagePaths.Select(_p => _p);
        public IEnumerable<string> PackageSetPaths => _PackageSetPaths.Select(_p => _p);

        /// <summary>IO package manager for process</summary>
        public static PackageManager Manager => _Manager;

        public void AddPackagePartReferenceLister(IListPackagePartReferences partReferences)
        {
            if (!_Listers.Contains(partReferences))
            {
                _Listers.Add(partReferences);
            }
        }

        public void RemovePackagePartReferenceLister(IListPackagePartReferences partReferences)
        {
            _Listers.Remove(partReferences);
        }

        public CorePackage GetPackage(IPackagePartReference reference)
            => GetResolutionContext(reference).package;

        private FileInfo GetFileInfo(string packageSet, string packageID)
        {
            var _probes = !string.IsNullOrWhiteSpace(packageSet)
                ? _PackageSetPaths
                : _PackagePaths;
            var _set = string.IsNullOrWhiteSpace(packageSet)
                ? @"/"
                : $@"/{packageSet}/";
            foreach (var _path in _probes)
            {
                try
                {
                    var _fInfo = new FileInfo($@"{_path}{_set}{packageID}");
                    if (_fInfo.Exists)
                    {
                        return _fInfo;
                    }
                }
                catch
                {
                }
            }
            return null;
        }

        #region public (CorePackage package, Dictionary<string, IBasePart> parts) GetResolutionContext(IPackagePartReference reference)
        public (CorePackage package, Dictionary<string, IBasePart> parts) GetResolutionContext(IPackagePartReference reference)
        {
            var _key = (PackageSet: reference?.PackageSet ?? string.Empty, reference?.PackageID);
            if (_Cache.ContainsKey(_key))
            {
                // package already tracked
                return _Cache[_key];
            }
            else if (!string.IsNullOrEmpty(_key.PackageID))
            {
                try
                {
                    var _fInfo = GetFileInfo(_key.PackageSet, _key.PackageID);
                    var _pck = new CorePackage(_fInfo, Package.Open(_fInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.Read));
                    var _tuple = (_pck, new Dictionary<string, IBasePart>());
                    _Cache.Add(_key, _tuple);
                    return _tuple;
                }
                catch
                {
                    // unable to get package
                    return (null, null);
                }
            }

            // nothing to work with
            return (null, null);
        }
        #endregion

        #region public void CleanupCache()
        public void CleanupCache()
        {
            // all parts in cache that are not referenced anymore
            var _clean = (from _c in _Cache
                          from _kvp in _c.Value.parts
                          let _part = _kvp.Value
                          where !_Listers.SelectMany(_l => _l.AllReferences).Any(_ref => _ref.Part == _part)
                          select new { _kvp.Key, Dictionary = _c.Value.parts }).ToList();
            foreach (var _rmv in _clean)
            {
                _rmv.Dictionary.Remove(_rmv.Key);
            }

            // all cache entries that no longer have parts
            var _recover = (from _c in _Cache
                            where !_c.Value.parts.Any()
                            select _c).ToList();
            foreach (var _del in _recover)
            {
                _del.Value.package.Close();
                _Cache.Remove(_del.Key);
            }
        }
        #endregion
    }
}
