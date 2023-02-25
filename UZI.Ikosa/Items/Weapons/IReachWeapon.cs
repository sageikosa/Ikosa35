namespace Uzi.Ikosa.Items.Weapons
{
    public interface IReachWeapon : IMeleeWeapon
    {
        /// <summary>Weapon can target within creatures natural range also</summary>
        bool TargetAdjacent { get; }
        /// <summary>Extra squares for reach distance (usually 0)</summary>
        int ExtraReach { get; }
    }
}
