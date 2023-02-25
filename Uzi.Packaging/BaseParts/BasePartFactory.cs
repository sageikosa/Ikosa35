using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Uzi.Packaging
{
    public class BasePartFactory : IBasePartFactory
    {
        public static void RegisterFactory()
        {
            BasePartHelper.RegisterFactory(new BasePartFactory());
        }

        #region IBasePartFactory Members

        public IEnumerable<string> Relationships
        {
            get
            {
                yield return CorePackagePartsFolder.CorePackagePartsFolderRelation;
                yield break;
            }
        }

        public IBasePart GetPart(string relationshipType, ICorePartNameManager manager, System.IO.Packaging.PackagePart part, string id)
        {
            switch (relationshipType)
            {
                case CorePackagePartsFolder.CorePackagePartsFolderRelation:
                    {
                        var _hider = manager as IHideCorePackagePartsFolder;
                        return new CorePackagePartsFolder(manager, part, id, ((_hider != null) && _hider.ShouldHide(id)));
                    }
            }
            return null;
        }

        public Type GetPartType(string relationshipType)
            => relationshipType switch
            {
                CorePackagePartsFolder.CorePackagePartsFolderRelation => typeof(CorePackagePartsFolder),
                _ => null,
            };

        #endregion
    }
}
