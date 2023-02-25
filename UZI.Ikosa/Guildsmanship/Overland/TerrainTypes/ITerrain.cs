namespace Uzi.Ikosa.Guildsmanship.Overland
{
    public interface ITerrain
    {
        string Name { get; }
        decimal MoveFactor { get; }
        decimal RoadMoveFactor { get; }
        decimal HighwayMoveFactor { get; }
        int AvoidLostDifficulty { get; }
        int MapAvoidLostDifficulty { get; }
        string MaxSpotDistanceString { get; }
        decimal MaxSpotDistance();
        TileRef GetTileRef(byte index);
        bool HasTileRef(byte index);
    }
}
