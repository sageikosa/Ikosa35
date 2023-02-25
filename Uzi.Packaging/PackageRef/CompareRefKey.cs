using System;
using System.Collections.Generic;
using System.Linq;

namespace Uzi.Packaging
{
    public class CompareRefKey : IEqualityComparer<(string packageSet, string packageID)>
    {
        public bool Equals((string packageSet, string packageID) x, (string packageSet, string packageID) y)
            => string.Equals(x.packageSet, y.packageSet, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(x.packageID, y.packageID, StringComparison.InvariantCultureIgnoreCase);

        public int GetHashCode((string packageSet, string packageID) obj)
            => obj.GetHashCode();
    }
}
