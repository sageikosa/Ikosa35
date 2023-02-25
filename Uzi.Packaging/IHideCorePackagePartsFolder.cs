using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Packaging
{
    public interface IHideCorePackagePartsFolder
    {
        bool ShouldHide(string id);
    }
}
