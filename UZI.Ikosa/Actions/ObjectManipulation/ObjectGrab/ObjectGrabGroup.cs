using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Uzi.Core;
using Uzi.Core.Contracts;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Movement;
using Uzi.Ikosa.Objects;
using Uzi.Ikosa.Tactical;
using Uzi.Visualize;

namespace Uzi.Ikosa.Actions
{
    /// <summary>Used to grab an object</summary>
    [Serializable]
    public class ObjectGrabGroup : AdjunctGroup, INotifyPropertyChanged
    {
        #region ctor()
        /// <summary>Used to grab an object</summary>
        public ObjectGrabGroup()
            : base(typeof(ObjectGrabGroup))
        {
        }
        #endregion

        protected void DoPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members
        [field: NonSerialized, JsonIgnore]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        /// <summary>GroupMasterAdjunct representing the grabbed object</summary>
        public ObjectGrabbed ObjectGrabbed
            => Members.OfType<ObjectGrabbed>().FirstOrDefault();

        /// <summary>GroupMemberAdjuncts representing all actors grabbing the object</summary>
        public IEnumerable<ObjectGrabber> ObjectGrabbers
            => Members.OfType<ObjectGrabber>().Select(_g => _g);

        public override void ValidateGroup()
            => this.ValidateOneToManyPlanarGroup();
    }
}
