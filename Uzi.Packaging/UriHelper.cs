using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Uzi.Packaging
{
    public static class UriHelper
    {
        public static Uri ConcatRelative(Uri baseUri, string relative)
        {
            string _rel = HttpUtility.UrlEncode(relative);
            string _base = baseUri.ToString();
            return new Uri(string.Format(@"{0}{1}{2}", _base, !_base.EndsWith(@"/") ? @"/" : string.Empty, _rel), UriKind.Relative);
        }

        public const string SafeCharacters = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._-";

        /// <summary>
        /// String safe for OPC URI parts and XSD IDs
        /// </summary>
        public static string ToSafeString(this string source)
        {
            StringBuilder _build = new StringBuilder();
            _build.Append((from _c in source
                           where SafeCharacters.Contains(_c)
                           select _c).ToArray());
            return _build.ToString();
        }
    }
}
