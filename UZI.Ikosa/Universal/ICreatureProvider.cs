using System;

namespace Uzi.Ikosa
{
    /// <summary>
    /// Define interface for providing creatures from some context
    /// </summary>
    public interface ICreatureProvider
    {
        Creature GetCreature(Guid creatureID);
    }
}
