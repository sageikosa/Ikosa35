namespace Uzi.Core
{
    public interface ICoreItem : ICoreObject
    {
        string OriginalName { get; }
        CoreActor Possessor { get; set; }
    }
}
