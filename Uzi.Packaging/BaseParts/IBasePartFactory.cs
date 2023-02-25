using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Packaging;

namespace Uzi.Packaging
{
    public interface IBasePartFactory
    {
        /// <summary>Relationship to register</summary>
        IEnumerable<string> Relationships { get; }

        /// <summary>Returns a part from the package part</summary>
        IBasePart GetPart(string relationshipType, ICorePartNameManager manager, PackagePart part, string id);

        Type GetPartType(string relationshipType);
    }
}
