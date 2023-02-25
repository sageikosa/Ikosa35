using System;
using System.Collections.Generic;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Items.Materials;
using Uzi.Ikosa.Tactical;

namespace Uzi.Ikosa.Movement
{
    [Serializable]
    public class EtherealMovement : FlightSuMovement
    {
        public EtherealMovement(int speed, CoreObject coreObj, EtherealMoveManager moveManager)
            : base(speed, coreObj, moveManager, FlightManeuverability.Perfect, false, true)
        {
        }

        // TODO: if material projection of sensors is inside material, add error chance to direction
        // TODO: ¿ should penalty to movement for unable to visualize be altered (if in solid material) ?

        public EtherealMoveManager EtherealMoveManager => Source as EtherealMoveManager;

        /// <summary>Always double cost</summary>
        protected override double BaseMoveCost(CoreActivity activity, double expected)
        {
            activity.AppendCompletion(new FlightBudgetUpdateStep(activity));
            return expected * 2;
        }

        public override bool IsUsable
            => CoreObject?.GetLocated()?.Locator.PlanarPresence.HasEtherealPresence() ?? false;

        public override string FlightType => @"Eth";
        public override string Name => @"Ethereal";
        public override string Description => $@"Ethereal Flight-Like Movement";
        public override bool IsNativeMovement => false;
        protected override bool MustVisualizeMovement => false;

        public override bool CanMoveThrough(CellMaterial material)
            // NOTE: cellMaterial only applies to PlanarPresense.Material
            => true;

        // NOTE: won't really be used, as movement isn't based on species
        public override MovementBase Clone(Creature forCreature, object source)
            => new EtherealMovement(BaseValue, forCreature, EtherealMoveManager.Clone() as EtherealMoveManager);
    }
}
