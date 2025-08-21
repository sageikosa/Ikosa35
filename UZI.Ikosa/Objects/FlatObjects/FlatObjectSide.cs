using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    [Serializable]
    public class FlatObjectSide : AnchorableObject, IFlatObjectSide
    {
        public FlatObjectSide(string name, Material material, double thickness)
            : base(name, material)
        {
            base.Width = 5;
            base.Height = 5;
            Thickness = (thickness > 0 && thickness <= 0.1) ? thickness : 0.05;
            MaxStructurePoints = Convert.ToInt32(Convert.ToDouble(material.StructurePerInch) * 12d / thickness);
        }

        public override double Width { get => base.Width; set { if (value > 0 && value <= 10) { base.Width = value; } } }
        public override double Height { get => base.Height; set { if (value > 0 && value <= 10) { base.Height = value; } } }
        public override double Length { get => base.Length; set { if (value > 0 && value <= 0.1) { base.Length = value; } } }
        public double Thickness { get => Length; set => Length = value; }

        public override bool IsTargetable => false;
        public bool IsContextMenuOnly => true;

        public void SetName(string name)
            => Name = name;

        public virtual string SoundDescription => GetType().Name.ToLower();

        public void SetMaxStructurePoints(int maxPts)
            => DoSetMaxStructurePoints(maxPts);

        protected override string ClassIconKey => string.Empty;

        public override IGeometricSize GeometricSize
            => new GeometricSize(1, 1, 1);

        // IActionProvider
        public IEnumerable<CoreAction> GetActions(CoreActionBudget budget)
        {
            var _budget = budget as LocalActionBudget;

            // mechanism actions...
            foreach (var _action in this.AccessibleActions(_budget))
            {
                yield return _action;
            }

            yield break;
        }

        public object Clone()
            => new FlatObjectSide(Name, ObjectMaterial, Thickness);
    }
}
