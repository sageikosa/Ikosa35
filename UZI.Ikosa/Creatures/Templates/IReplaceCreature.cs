namespace Uzi.Ikosa.Creatures.Templates
{
    /// <summary>
    /// To be implemented on a species that has a single parameter Creature constructor
    /// </summary>
    public interface IReplaceCreature : ICreatureTemplate
    {
        /// <summary>
        /// Indicates true if this species can process it's source creature
        /// </summary>
        bool CanGenerate { get; }
    }
}
