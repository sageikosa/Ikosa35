using System;
using System.Collections.Generic;
using Uzi.Core;
using Uzi.Ikosa.Adjuncts;
using Uzi.Ikosa.Tactical;
using Uzi.Packaging;
using Uzi.Visualize;

namespace Uzi.Ikosa.Objects
{
    /// <summary>
    /// Typically used for abstractions that need to be located (such as portals), and do not need ObjectBase
    /// </summary>
    [Serializable]
    public abstract class LocatableObject : CoreObject, IVisible, ICorePart, ISizable
    {
        protected LocatableObject(string name, bool isVisible)
            : base(name)
        {
            IsVisible = isVisible;
        }

        public virtual bool IsVisible { get; set; }

        #region ICorePart Members

        public virtual IEnumerable<ICorePart> Relationships { get { yield break; } }

        public string TypeName
            => GetType().FullName;

        #endregion

        public override CoreSetting Setting
            => this.GetTokened()?.Token.Context.ContextSet.Setting;

        public abstract Sizer Sizer { get; }
        public abstract IGeometricSize GeometricSize { get; }
    }
}
