using System;

namespace Uzi.Core
{
    [Serializable]
    public readonly struct Physical
    {
        public Physical(PhysicalType propertyType, double amount)
        {
            PropertyType = propertyType;
            Amount = amount;
        }

        public readonly PhysicalType PropertyType;
        public readonly double Amount;

        public enum PhysicalType : byte { Weight, Length, Width, Height };
    }
}
