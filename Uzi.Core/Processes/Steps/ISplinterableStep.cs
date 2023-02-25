namespace Uzi.Core
{
    public interface ISplinterableStep
    {
        CoreProcess Process { get; }
        CoreStep MasterStep { get; set; }
    }
}
