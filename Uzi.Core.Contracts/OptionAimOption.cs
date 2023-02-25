using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Windows;

namespace Uzi.Core.Contracts
{
    [Serializable]
    [DataContract(Namespace = Statics.Namespace)]
    public class OptionAimOption
    {
        [DataMember]
        public string Key { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
        [DataMember]
        public bool IsCurrent { get; set; }

        public Visibility DescriptionVisibility
            => !string.IsNullOrWhiteSpace(this.Description) 
            ? Visibility.Visible 
            : Visibility.Collapsed;

        /// <summary>
        /// Presents the option suitable for delivery in a data-contract
        /// </summary>
        public OptionAimOption Contracted
            => new OptionAimOption
            {
                Key = Key,
                Name = Name,
                Description = Description,
                IsCurrent = IsCurrent
            };
    }
}
