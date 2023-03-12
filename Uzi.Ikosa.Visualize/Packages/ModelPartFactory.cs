using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Ikosa.Packaging;

namespace Uzi.Visualize.Packages
{
    public static class ModelPartFactory
    {
        public static IStorablePart GetModelPart(IRetrievablePartNameManager manager, string id,
            ZipArchive zipArchive, string parentPath)
        {
            var _isModel = $@"{ parentPath}/{id}/model.xaml";
            if (zipArchive.Entries.Any(_e => _e.FullName.Equals(_isModel, StringComparison.OrdinalIgnoreCase)))
            {
                return new Model3DPart(manager, id);
            }
            else
            {
                return new MetaModel(manager, id);
            }
        }
    }
}
