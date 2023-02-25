using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;

namespace Ikosa.Packaging
{
    public class IkosaPackageManager
    {
        private readonly Dictionary<string, IkosaPackage> _Packages = new Dictionary<string, IkosaPackage>();
        private readonly Dictionary<string, List<IStorablePart>> _Parts = new Dictionary<string, List<IStorablePart>>();

        //private readonly Dictionary<(string packageSet, string packageID), (IkosaPackage package, List<IStorablePart> parts)> _Cache;
        private readonly List<IListPartReferences> _Listers;
        private readonly List<string> _PackagePaths;
        private readonly List<string> _PackageSetPaths;

        [NonSerialized]
        private static readonly Dictionary<string, Func<FileInfo, ZipArchive, IkosaPackage>> _Generators
            = new Dictionary<string, Func<FileInfo, ZipArchive, IkosaPackage>>();

        [NonSerialized]
        private static readonly IkosaPackageManager _Manager
            = new IkosaPackageManager();

        private IkosaPackageManager()
        {
            _Listers = new List<IListPartReferences>();
            _PackagePaths = new List<string>();
            _PackageSetPaths = new List<string>();
        }

        public void SetPaths(List<string> packagePaths, List<string> packageSetPaths)
        {
            _PackagePaths.Clear();
            _PackagePaths.AddRange(packagePaths);

            _PackageSetPaths.Clear();
            _PackageSetPaths.AddRange(packageSetPaths);
        }

        public static JsonSerializer GetJsonSerializer()
            => new JsonSerializer
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };

        public IEnumerable<string> PackagePaths => _PackagePaths.Select(_p => _p);
        public IEnumerable<string> PackageSetPaths => _PackageSetPaths.Select(_p => _p);

        public static IkosaPackageManager Manager => _Manager;

        public void AddPackagePartReferenceLister(IListPartReferences partReferences)
        {
            if (!_Listers.Contains(partReferences))
            {
                _Listers.Add(partReferences);
            }
        }

        public void RemovePackagePartReferenceLister(IListPartReferences partReferences)
        {
            _Listers.Remove(partReferences);
        }

        public static void RegisterGenerator(string partType, Func<FileInfo, ZipArchive, IkosaPackage> generator)
            => _Generators[partType] = generator;

        public IkosaPackage GetPackage(PackageReference reference)
            => GetResolutionContext(reference).package;

        private FileInfo GetFileInfo(PackageReference reference)
        {
            var _probes = !reference.IsResource
                ? _PackageSetPaths
                : _PackagePaths;
            var _packagePath = reference.PackagePath;   // "contextual" set of packages
            foreach (var _probeDir in _probes)
            {
                try
                {
                    var _fInfo = new FileInfo($@"{_probeDir}{_packagePath}");    // actual package name
                    if (_fInfo.Exists)
                        return _fInfo;
                }
                catch
                {
                }
            }
            return null;
        }

        /// <summary>Gets top-level part of the specified type</summary>
        public Part GetStorablePart<Part>(PackageReference reference)
            where Part : class, IStorablePart
        {
            var (_package, _parts) = GetResolutionContext(reference);
            if (_package != null)
            {
                // get from top-level of package
                var _retrieve = _package.Parts.OfType<Part>().FirstOrDefault();
                if (_retrieve != null)
                {
                    // track it if not tracked for package
                    if (!_parts.Contains(_retrieve))
                    {
                        _parts.Add(_retrieve);
                    }
                    return _retrieve;
                }
            }
            return null;
        }

        #region private (IkosaPackage package, List<IStorablePart> parts) GetResolutionContext(PackageReference reference)
        private (IkosaPackage package, List<IStorablePart> parts) GetResolutionContext(PackageReference reference)
        {
            var _path = reference.PackagePath;
            if (_Packages.ContainsKey(_path))
            {
                // package already loaded and tracked by ID
                return (_Packages[_path], _Parts[_path]);
            }

            // use package name to find a package with the same name
            try
            {
                var _fInfo = GetFileInfo(reference);
                if (_Generators.TryGetValue(reference.PackageType, out var _generator))
                {
                    var _pck = _generator.Invoke(_fInfo, ZipFile.OpenRead(_fInfo.FullName));
                    var _parts = new List<IStorablePart>();

                    _Packages[_path] = _pck;
                    _Parts[_path] = _parts;
                    return (_pck, _parts);
                }
            }
            catch
            {
                // unable to get package
                return (null, null);
            }

            // nothing to work with
            return (null, null);
        }
        #endregion

        #region public void CleanupCache()
        public void CleanupCache()
        {
            // all parts in cache that are not referenced anymore
            var _clean = (from _c in _Parts
                          from _part in _c.Value
                          where !_Listers.SelectMany(_l => _l.AllReferences).Any(_ref => _ref == _part)
                          select new { Part = _part, List = _c.Value }).ToList();
            foreach (var _rmv in _clean)
            {
                _rmv.List.Remove(_rmv.Part);
            }

            // all cache entries that no longer have parts
            var _recover = (from _c in _Parts
                            where !_c.Value.Any()
                            select _c).ToList();
            foreach (var _del in _recover)
            {
                // remove empty parts entry
                _Parts.Remove(_del.Key);

                // package not tracking parts
                if (_Packages.TryGetValue(_del.Key, out var _closePackage))
                {
                    // close and stop tracking
                    _closePackage.Close();
                    _Packages.Remove(_del.Key);
                }
            }
        }
        #endregion
    }
}
