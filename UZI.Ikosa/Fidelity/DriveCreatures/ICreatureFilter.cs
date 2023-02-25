namespace Uzi.Ikosa.Fidelity
{
    public interface ICreatureFilter
    {
        bool DoesMatch(Creature critter);
        string Key { get; }
        string Description { get; }
    }
}
