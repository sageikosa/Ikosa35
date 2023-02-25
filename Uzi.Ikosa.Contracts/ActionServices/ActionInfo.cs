using System;
using System.Linq;
using System.Runtime.Serialization;
using Uzi.Core.Contracts;
using System.Windows;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Uzi.Ikosa.Contracts
{
    [DataContract(Namespace = Statics.Namespace)]
    public class ActionInfo : INotifyPropertyChanged
    {
        [DataMember]
        public Guid ID { get; set; }
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string OrderKey { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool ProvokesMelee { get; set; }
        [DataMember]
        public bool ProvokesTarget { get; set; }
        [DataMember]
        public bool IsExternalProvider { get; set; }
        [DataMember]
        public bool IsHarmless { get; set; }
        [DataMember]
        public bool IsDistractable { get; set; }
        [DataMember]
        public bool IsChoice { get; set; }
        [DataMember]
        public string TimeCost { get; set; }
        [DataMember]
        public byte TimeTypeVal { get; set; }
        [DataMember]
        public AimingModeInfo[] AimingModes { get; set; }
        [DataMember]
        public ActionProviderInfo Provider { get; set; }
        [DataMember]
        public string HeadsUpMode { get; set; }
        [DataMember]
        public bool IsContextMenuOnly { get; set; }

        public TimeType TimeType => (TimeType)TimeTypeVal;

        public OptionAimInfo FirstOptionAimInfo
            => AimingModes.FirstOrDefault() as OptionAimInfo;

        public Visibility DescriptionVisibility
            => !string.IsNullOrWhiteSpace(Description)
            ? Visibility.Visible
            : Visibility.Collapsed;

        public bool IsSameAction(ActionInfo test)
            => (Provider?.ID ?? Guid.Empty) == (test.Provider?.ID ?? Guid.Empty)
            && ID == test.ID
            && Key == test.Key;

        [field:NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;

        public void DoNotify(string propName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}