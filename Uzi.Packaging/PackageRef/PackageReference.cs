using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Packaging
{
    [Serializable]
    public class PackageReference
    {
        private readonly string _Specifier;

        [NonSerialized]
        private string _Path;

        public PackageReference(string specifier)
        {
            _Specifier = specifier;
        }

        public PackageReference(string specifier, string path)
        {
            _Specifier = specifier;
            _Path = path;
        }

        public void SetContext()
        {
            // TODO: calculate path from specifier
            _Path = string.Empty;
        }

        public string Specifier => _Specifier;
        public string Path => _Path;
    }
}
