using System;
using System.Collections.Generic;
using System.IO.Compression;
using Ikosa.Packaging;

namespace Uzi.Visualize.IkosaPackaging
{
    public class VisualizeBasePartFactory : IBasePartFactory
    {
        public static void RegisterFactory()
        {
            BasePartArchiving.RegisterFactory(new VisualizeBasePartFactory());
        }

        #region IBasePartFactory Members

        public IEnumerable<string> PartTypes
        {
            get
            {
                yield return Model3DPart.ModelRelation;
                yield return BitmapImagePart.ImageRelation;
                yield return BrushCollectionPart.BrushSetRelation;
                yield return MetaModel.MetaModelRelation;
                yield return MetaModelFragment.MetaModelFragmentRelation;
                yield return ResourceManager.ResourceManagerRelation;
                yield return IconPart.IconRelation;
                yield break;
            }
        }

        public IBasePart GetPart(ICorePartNameManager manager, ZipArchiveEntry entry, PartRelation relation)
        {
            switch (relation.PartType)
            {
                case Model3DPart.ModelRelation:
                    return new Model3DPart(manager, entry);
                case BitmapImagePart.ImageRelation:
                    return new BitmapImagePart(manager, entry, relation);
                case BrushCollectionPart.BrushSetRelation:
                    return new BrushCollectionPart(manager, entry, relation);
                case MetaModel.MetaModelRelation:
                    return new MetaModel(manager, entry, relation);
                case MetaModelFragment.MetaModelFragmentRelation:
                    return new MetaModelFragment(manager, entry, relation);
                case ResourceManager.ResourceManagerRelation:
                    return new ResourceManager(manager, entry, relation);
                case IconPart.IconRelation:
                    return new IconPart(manager, entry, relation);
            }
            return null;
        }

        public Type GetPartType(string relationshipType)
        {
            switch (relationshipType)
            {
                case Model3DPart.ModelRelation:
                    return typeof(Model3DPart);
                case BitmapImagePart.ImageRelation:
                    return typeof(BitmapImagePart);
                case BrushCollectionPart.BrushSetRelation:
                    return typeof(BrushCollectionPart);
                case MetaModel.MetaModelRelation:
                    return typeof(MetaModel);
                case MetaModelFragment.MetaModelFragmentRelation:
                    return typeof(MetaModelFragment);
                case ResourceManager.ResourceManagerRelation:
                    return typeof(ResourceManager);
                case IconPart.IconRelation:
                    return typeof(IconPart);
            }
            return null;
        }

        #endregion
    }
}
