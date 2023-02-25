using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Actions;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Contracts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;
using Uzi.Visualize.Contracts.Tactical;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class MagicEffectHolder : LocatableObject
    {
        #region ctor()
        protected MagicEffectHolder(string name, Size size, IGeometricSize geomSize, string iconKey, bool isVisible)
            : base(name, isVisible)
        {
            _Sizer = new ObjectSizer(size, this);
            _GeomSize = geomSize;
            _IconKey = iconKey;
            switch (_Sizer.Size.Order)
            {
                case -4:
                    Width = 0.5d;
                    Length = 0.5d;
                    Height = 0.5d;
                    break;
                case -3:
                    Width = 0.75d;
                    Length = 0.75d;
                    Height = 0.75d;
                    break;
                case -2:
                    Width = 1.5d;
                    Length = 1.5d;
                    Height = 1.5d;
                    break;
                case -1:
                    Width = 3d;
                    Length = 3d;
                    Height = 3d;
                    break;
                case 1:
                    Width = 8d;
                    Length = 8d;
                    Height = 8d;
                    break;
                case 2:
                    Width = 12.5d;
                    Length = 12.5d;
                    Height = 12.5d;
                    break;
                case 3:
                    Width = 18d;
                    Length = 18d;
                    Height = 18d;
                    break;
                case 4:
                    Width = 23d;
                    Length = 23d;
                    Height = 23d;
                    break;
                default:
                    Width = 4.5d;
                    Length = 4.5d;
                    Height = 4.5d;
                    break;
            }
        }
        #endregion

        #region data
        private ObjectSizer _Sizer;
        private IGeometricSize _GeomSize;
        private string _IconKey;
        #endregion

        public override Sizer Sizer => _Sizer;
        public override IGeometricSize GeometricSize => _GeomSize;
        public override bool IsTargetable => false;
        protected override string ClassIconKey => _IconKey;

        public override Info GetInfo(CoreActor actor, bool baseValues)
            => new ObjectInfo
            {
                ID = ID,
                Message = Name,
                Size = Sizer.Size.ToSizeInfo(),
                Hardness = new DeltableInfo(0),
                Material = VoidMaterial.Static.ToMaterialInfo(),
                CreatureSize = Size.Medium.ToSizeInfo(),
                AdjunctInfos = new Info[] { },
                Weight = Weight,
                StructurePercent = 1d,
                Icon = new ImageryInfo
                {
                    Keys = IconKeys.ToArray(),
                    IconRef = new IconReferenceInfo
                    {
                        IconAngle = IconAngle,
                        IconScale = IconScale,
                        IconColorMap = IconColorMap
                    }
                }
            };

        public override Info MergeConnectedInfos(Info fetchedInfo, CoreActor actor)
            => fetchedInfo;

        /// <summary>UnPath() and UnGroup().  Effectively removing this from gameplay.</summary>
        public void Abandon()
        {
            this.UnPath();
            this.UnGroup();
        }

        /// <summary>
        /// Create a magic effect holder in context with planar presence.  Call in ApplySpell (generally).
        /// SpellMode should implement IDurableAnchorMode and Destroy the MagicEffectHolder during OnEndAnchor.
        /// </summary>
        public static MagicEffectHolder CreateMagicEffectHolder(string name, Size size, IGeometricSize geometricSize, string iconKey,
            string modelKey, bool ethereal, ICellLocation location, MapContext mapContext, bool isVisible)
        {
            var _magicHolder = new MagicEffectHolder(name, size, geometricSize, iconKey, isVisible);
            if (ethereal)
            {
                // if actor was ethereal, the effect will be ethereal (even if the actor ceases to be)
                _magicHolder.AddAdjunct(new EtherealEffect(_magicHolder, null));
            }
            new ObjectPresenter(_magicHolder, mapContext, modelKey, _magicHolder.GeometricSize,
                new Cubic(location, _magicHolder.GeometricSize));
            return _magicHolder;
        }

        /// <summary>UnGroup and UnPath, removing MagicEffectHolder from the context</summary>
        public void Destroy()
        {
            this.UnPath();
            this.UnGroup();
        }
    }
}
