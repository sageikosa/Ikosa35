using System;
using System.Collections.Generic;
using System.IO.Packaging;
using Uzi.Packaging;

namespace Uzi.Visualize.Packaging
{
    public class VisualizeBasePartFactory : IBasePartFactory
    {
        public static void RegisterFactory()
        {
            BasePartHelper.RegisterFactory(new VisualizeBasePartFactory());
        }

        #region IBasePartFactory Members

        public IEnumerable<string> Relationships
        {
            get
            {
                yield return Model3DPart.ModelRelation;
                yield return BitmapImagePart.ImageRelation;
                yield return BrushCollectionPart.BrushSetRelation;
                yield return MetaModel.MetaModelRelation;
                yield return MetaModelFragment.MetaModelFragmentRelation;
                yield return VisualResources.VisualResourcesRelation;
                yield return IconPart.IconRelation;
                yield break;
            }
        }

        public IBasePart GetPart(string relationshipType,
            ICorePartNameManager manager, PackagePart part, string id)
            => relationshipType switch
            {
                Model3DPart.ModelRelation => new Model3DPart(manager, part, id),
                BitmapImagePart.ImageRelation => new BitmapImagePart(manager, part, id),
                BrushCollectionPart.BrushSetRelation => new BrushCollectionPart(manager, part, id),
                MetaModel.MetaModelRelation => new MetaModel(manager, part, id),
                MetaModelFragment.MetaModelFragmentRelation => new MetaModelFragment(manager, part, id),
                VisualResources.VisualResourcesRelation => new VisualResources(manager, part, id),
                IconPart.IconRelation => new IconPart(manager, part, id),
                _ => null,
            };

        public Type GetPartType(string relationshipType)
            => relationshipType switch
            {
                Model3DPart.ModelRelation => typeof(Model3DPart),
                BitmapImagePart.ImageRelation => typeof(BitmapImagePart),
                BrushCollectionPart.BrushSetRelation => typeof(BrushCollectionPart),
                MetaModel.MetaModelRelation => typeof(MetaModel),
                MetaModelFragment.MetaModelFragmentRelation => typeof(MetaModelFragment),
                VisualResources.VisualResourcesRelation => typeof(VisualResources),
                IconPart.IconRelation => typeof(IconPart),
                _ => null,
            };

        #endregion
    }
}
