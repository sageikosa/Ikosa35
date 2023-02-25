using System;
using Uzi.Core;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa
{
    [Serializable]
    public class CarryingCapacity : ICreatureBound, IMonitorChange<DeltaValue>, INotifyPropertyChanged
    {
        public CarryingCapacity(Creature creature)
        {
            Creature = creature;
            Creature.Abilities.Strength.AddChangeMonitor(this);
        }
        public Creature Creature { get; private set; }

        #region LightLoadLimit
        public double LightLoadLimit
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseLightLoadMax * Creature.Body.Sizer.Size.ExtraCarryingFactor);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseLightLoadMax * Creature.Body.Sizer.Size.BaseCarryingFactor);
                }
            }
        }
        #endregion

        #region MediumLoadLimit
        public double MediumLoadLimit
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMediumLoadMax * Creature.Body.Sizer.Size.ExtraCarryingFactor);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMediumLoadMax * Creature.Body.Sizer.Size.BaseCarryingFactor);
                }
            }
        }
        #endregion

        #region HeavyLoadLimit
        public double HeavyLoadLimit
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.ExtraCarryingFactor);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.BaseCarryingFactor);
                }
            }
        }
        #endregion

        #region LoadLiftOverHead
        public double LoadLiftOverHead
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.ExtraCarryingFactor);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.BaseCarryingFactor);
                }
            }
        }
        #endregion

        #region LoadLiftOffGround
        public double LoadLiftOffGround
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.ExtraCarryingFactor * 2d);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.BaseCarryingFactor * 2d);
                }
            }
        }
        #endregion

        #region LoadPushDrag
        public double LoadPushDrag
        {
            get
            {
                if (Creature.Body.UseExtraCarryingFactor)
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.ExtraCarryingFactor * 5d);
                }
                else
                {
                    return Math.Floor(Creature.Abilities.Strength.BaseMaxLoad * Creature.Body.Sizer.Size.BaseCarryingFactor * 5d);
                }
            }
        }
        #endregion

        #region IMonitorChange<DeltaValue> Members
        public void PreTestChange(object sender, AbortableChangeEventArgs<DeltaValue> args) { }
        public void PreValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args) { }

        public void ValueChanged(object sender, ChangeValueEventArgs<DeltaValue> args)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LightLoadLimit)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(MediumLoadLimit)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(HeavyLoadLimit)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LoadLiftOverHead)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LoadLiftOffGround)));
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(LoadPushDrag)));
            }
        }
        #endregion

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
