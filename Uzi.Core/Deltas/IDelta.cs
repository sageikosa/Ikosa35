namespace Uzi.Core
{
    public interface IDelta : ISourcedObject
    {
        int Value { get; }
        string Name { get; }
        bool Enabled { get; set; }
    }
}
