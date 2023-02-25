using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace Ikosa.Packaging
{
    public static class StorablePartArchiving
    {
        public static Action<string> LoadMessage { get; set; }

        public const string SafeCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-";

        /// <summary>String safe part names</summary>
        public static string ToSafeString(this string source)
        {
            var _build = new StringBuilder();
            _build.Append((from _c in source
                           where SafeCharacters.Contains(_c)
                           select _c).ToArray());
            return _build.ToString();
        }
    }
}
