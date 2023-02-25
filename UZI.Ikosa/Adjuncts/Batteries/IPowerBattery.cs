using Uzi.Core;
using Uzi.Ikosa.Time;

namespace Uzi.Ikosa.Adjuncts
{
    public interface IPowerBattery : IPowerCapacity, IControlChange<int>
    {
        IDeltable MaximumCharges { get; }
        int UsedCharges { get; }
        int AvailableCharges { get; }
        void AddCharges(int number);
    }

    public interface IRegeneratingBattery : IPowerBattery, ITrackTime
    {
        double RechargeTime { get; }
    }

    public interface IFullResetBattery : IRegeneratingBattery
    {
        double NextRecharge { get; }
    }
}
