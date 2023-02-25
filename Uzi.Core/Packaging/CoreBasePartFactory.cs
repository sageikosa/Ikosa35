using System;
using System.Collections.Generic;
using Uzi.Packaging;

namespace Uzi.Core.Packaging
{
    public class CoreBasePartFactory : IBasePartFactory
    {
        public static void RegisterFactory()
        {
            BasePartHelper.RegisterFactory(new CoreBasePartFactory());
        }

        public IEnumerable<string> Relationships
        {
            get
            {
                yield return CoreObjectPart.CoreObjectRelation;
                //yield return CoreProcessPart.CoreProcessRelation;
                yield break;
            }
        }

        public IBasePart GetPart(string relationshipType, ICorePartNameManager manager, System.IO.Packaging.PackagePart part, string id)
            => relationshipType switch
            {
                CoreObjectPart.CoreObjectRelation => new CoreObjectPart(manager, part, id),
                _ => null,
            };

        public Type GetPartType(string relationshipType)
            => relationshipType switch
            {
                CoreObjectPart.CoreObjectRelation => typeof(CoreObjectPart),
                _ => null,
            };
    }
}
